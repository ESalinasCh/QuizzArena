namespace QuizzArena.DocumentProcessing.Domain.Enums;

public enum ChunkCategory
{
    // The chunk could not be reliably classified (e.g. LLM error or unrecognized label).
    Unknown = 0,

    // Subject-matter content: explanations, definitions, academic examples. The only kind worth storing.
    Academic = 1,

    // Conversation unrelated to the topic: weather, anecdotes about logistics, greetings.
    OffTopic = 2,

    // An incorrect student answer that was not explicitly corrected.
    StudentError = 3,

    // An illustrative story; useful context but not key knowledge.
    Anecdote = 4,
}
