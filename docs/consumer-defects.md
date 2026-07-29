# Consumer & saga defects

Found while writing unit tests for all message consumers (branch `chore/204-commplete-unit-testing-for-consumers`).

**No production code was changed in that PR.** The new tests assert current behavior, so several of them
deliberately lock in the bugs below — each such test carries a comment pointing back to this document. When a
defect is fixed, the corresponding test must be updated in the same PR.

Severity legend: **P1** breaks a user-visible flow · **P2** degrades operability · **P3** cleanup.

---

## P1-1 · `GenerationFailedEvent` is published completely empty

**Where:** `Modules/QuizzArena.DocumentProcessing/Infrastructure/Adapters/In/Messaging/Consumers/Generation/GenerationProcessingConsumer.cs:262`

```csharp
await context.Publish(new GenerationFailedEvent
{

});
```

**What happens:** every `Guid` property defaults to `Guid.Empty` and `ErrorMessage` to `""`.

**Impact:** `GenerationSaga` correlates `FailedEvent` on `DocumentProcessingJobId`
(`Sagas/Generation/GenerationSaga.cs`), so no saga instance can ever match this message. The entire quiz
generation failure path is dead: when generation fails, the job is never marked `Failed`, the saga never
finalizes, and the user sees the request hang forever.

**Compounding issue:** the saga only handles `FailedEvent` inside `During(GenerationEnding, ...)`, but this
consumer runs while the saga is in `GenerationInProgress`. So even a fully populated event would be ignored.
Fixing the payload alone is not enough — `FailedEvent` also has to be handled during `GenerationInProgress`.

**Suggested fix:** populate the event from the command and add `When(FailedEvent)` to the
`GenerationInProgress` state (and to `GenerationStarting`, for symmetry).

**Tests affected:** `GenerationProcessingConsumerTests.Consume_*_PublishesGenerationFailedEvent` currently only
assert that *something* was published, not its contents.

---

## P1-2 · `GenerationSuccessEvent` is never published, so the generation saga never finalizes

**Where:** `Application/Messaging/Events/Generation/GenerationSuccessEvent.cs`, `Sagas/Generation/GenerationSaga.cs:18`

**What happens:** `GenerationSaga` transitions to `GenerationEnding` and waits for `GenerationSuccessEvent` to
reach `GenerationSuccess` and `Finalize()`. A repo-wide search shows the type is referenced in exactly two
places: its own declaration and the saga's event declaration. **Nothing ever publishes it.**
`GenerationEndingConsumer` — the natural place to publish it — publishes nothing at all.

**Impact:** the generation saga never completes on the happy path. Every successful quiz generation leaves a
saga instance stuck in `GenerationEnding`. With `InMemoryRepository()` that is a slow memory leak; once the
saga repository is moved to a database it becomes stuck rows.

**Suggested fix:** publish `GenerationSuccessEvent` at the end of `GenerationEndingConsumer.Consume`, carrying
at minimum `DocumentProcessingJobId` so the saga can correlate.

**Test locking this in:** `GenerationEndingConsumerTests.Consume_ValidCommand_NeverPublishesAnyEvent`.

---

## P1-3 · `TranscriptionRequestConsumer` only catches `HttpRequestException`

**Where:** `Consumers/Ingestion/TranscriptionRequestConsumer.cs:51`

```csharp
catch (HttpRequestException ex)
```

**What happens:** the `try` block also covers the class-source lookup (which throws
`InvalidOperationException` when the source is missing), the blob upload, and the repository update. None of
those failures are `HttpRequestException`, so they escape the consumer without publishing
`TranscriptionFailedEvent`.

**Impact:** the message goes to MassTransit retry and then the error queue, while the Ingestion saga sits in
`TranscriptionInProgress` forever. The class source keeps a stale status and the user gets no feedback.

**Suggested fix:** catch `Exception`, publish `TranscriptionFailedEvent`, and let retry be driven by the
transport policy rather than by which exception type happened to be thrown — matching what
`IndexingTranscriptConsumer` already does.

**Tests locking this in:**
`TranscriptionRequestConsumerTests.Consume_StorageThrowsNonHttpException_PropagatesWithoutPublishingFailedEvent`
and `Consume_ClassSourceNotFound_ThrowsInvalidOperationException`.

---

## P2-1 · `GenerationFailedConsumer` injects the wrong logger category

**Where:** `Consumers/Generation/GenerationFailedConsumer.cs:11`

```csharp
internal partial class GenerationFailedConsumer(
    ILogger<TranscriptionFailedConsumer> logger,   // <-- wrong generic argument
    IProcessingJobRepository processingJobRepository
)
```

**Impact:** every log line this consumer writes is attributed to `TranscriptionFailedConsumer`. Log filters and
per-category log levels targeting `GenerationFailedConsumer` silently match nothing, and generation failures
appear under the transcription category during incident triage.

**Suggested fix:** change to `ILogger<GenerationFailedConsumer>`.

**Test to update when fixed:** `GenerationFailedConsumerTests` constructs the consumer with
`NullLogger<TranscriptionFailedConsumer>.Instance` and will stop compiling — that is intentional.

---

## P2-2 · `GenerateQuizConsumer` reports false success outside Development and Production

**Where:** `Modules/QuizzArena.Quizzing/Infrastructure/Adapters/In/Messaging/Consumers/GenerateQuizConsumer.cs:19-40`

**What happens:** the method branches on `IsDevelopment()` / `IsProduction()`. In any other environment
(`Staging`, or an unset/typo'd `ASPNETCORE_ENVIRONMENT`) neither branch runs — but `QuizGenerationCompletedEvent`
is still published at the end of the `try`.

**Impact:** downstream consumers are told a quiz was generated when nothing happened. A misconfigured
environment name turns into a silent data-loss bug rather than a loud failure.

**Related:** the Production branch is still a `// TODO: Here use the IA` stub that only writes to the console,
so today Production also reports success without generating anything.

**Also:** the mock data path is hardcoded to `Path.Combine(AppContext.BaseDirectory, "MockData", "quiz.json")`.
It is not injectable, so testing the Development branch required copying a `MockData/quiz.json` fixture into
`QuizzArena.Quizzing.Tests` and adding a `CopyToOutputDirectory` entry to the test `.csproj`.

**Suggested fix:** throw (or publish `QuizGenerationFailedEvent`) on an unrecognised environment, and inject the
mock path via options.

**Test locking this in:** `GenerateQuizConsumerTests.Consume_StagingEnvironment_PublishesCompletedEventWithoutCallingUseCase`.

---

## P2-3 · `GenerateQuizConsumer` is dead code — it is never registered

**Where:** `Modules/QuizzArena.Quizzing/Infrastructure/Adapters/Out/Messaging/Configuration/QuizzingMassTransit.cs`
vs `Host/Program.cs:134-136`

```csharp
builder.Services.AddMassTransit(x =>
{
    DocumentProcessingMassTransit.AddConsumers(x);   // QuizzingMassTransit.AddConsumers(x) is never called
```

**Impact:** `QuizzingMassTransit.AddConsumers` has no call site anywhere in the solution, so
`GenerateQuizConsumer` is never subscribed and `TranscriptionCompletedEvent` is never handled by the Quizzing
module. It now has unit tests, but it does not run in the application.

**Suggested fix:** either register it in `Program.cs` or delete the consumer, its config class, and its tests.
Worth deciding explicitly — quiz generation currently lives entirely in the DocumentProcessing generation
pipeline, which suggests this is leftover scaffolding.

---

## P3-1 · `IndexingTranscriptConsumer` has two unreachable guards

**Where:** `Consumers/Indexing/IndexingTranscriptConsumer.cs:74` and `:81`

```csharp
if (sentences.Count == 0) { throw ... "has no valid sentences" }
if (fragments.Count == 0) { throw ... "has no valid fragments" }
```

**Why unreachable:** by the time control reaches them the transcript is already known to be non-empty after
trimming. `SentenceSplitter.SplitIntoSentences` always appends the trailing `currentWords` buffer, so it returns
at least one sentence for any non-whitespace input; `TextChunker.ChunkList` likewise always flushes its final
chunk. Neither can return an empty list here.

**Impact:** dead branches that permanently cap branch coverage and mislead readers into thinking the case is
handled.

**Suggested fix:** delete both guards, or fold them into the existing empty-transcript check.

---

## P3-2 · Stale test file name (fixed in this branch)

`QuizzArena.DocumentProcessing.Tests/Consumers/GenerationRequestConsumerTests.cs` contained
`public class GenerationProcessingConsumerTests`. There is no `GenerationRequestConsumer` in the codebase.
Renamed to `GenerationProcessingConsumerTests.cs`.

---

## P3-3 · Dead directory `QuizzArena.Backend/QuizzArena.Quizzing.test/`

Contains only stale `bin/` and `obj/` build artifacts from June 2025 — no `.cs` files, no `.csproj`, and it is
not referenced by `QuizzArena.Backend.slnx`. Leftover from the rename to `QuizzArena.Quizzing.Tests`.

**Suggested fix:** delete the directory.
