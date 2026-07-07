BEGIN;

-- ============================================================
-- 1. PROCESSING JOBS (all completed)
-- ============================================================

INSERT INTO document_processing.processing_job
(
    "Id",
    status,
    error_message,
    created_at,
    updated_at,
    finished_at
)
VALUES
(
    'aaaaaaaa-0000-0000-0000-000000000001'::uuid,
    'completed',
    '',
    NOW() - INTERVAL '7 days',
    NOW() - INTERVAL '6 days',
    NOW() - INTERVAL '6 days' + INTERVAL '2 hours'
),
(
    'aaaaaaaa-0000-0000-0000-000000000002'::uuid,
    'completed',
    '',
    NOW() - INTERVAL '5 days',
    NOW() - INTERVAL '4 days',
    NOW() - INTERVAL '4 days' + INTERVAL '1 hour'
),
(
    'aaaaaaaa-0000-0000-0000-000000000003'::uuid,
    'completed',
    '',
    NOW() - INTERVAL '3 days',
    NOW() - INTERVAL '2 days',
    NOW() - INTERVAL '2 days' + INTERVAL '3 hours'
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 2. CLASS SOURCES (one per processing job)
-- ============================================================

INSERT INTO document_processing.class_source
(
    "Id",
    "Type",
    "Status",
    "Name",
    "TranscriptUrl",
    "FileUrl",
    "Deleted",
    created_at,
    updated_at,
    deleted_at,
    course_id,
    user_d
)
VALUES
-- Class Source 1: Video (Course 1 - AI Fundamentals)
(
    'bbbbbbbb-0000-0000-0000-000000000001'::uuid,
    'video',
    'completed',
    'Introduction to Artificial Intelligence - Video 1',
    'https://storage.demo.ai/transcripts/intro-ai-video1.txt',
    'https://storage.demo.ai/videos/intro-ai-video1.mp4',
    FALSE,
    NOW() - INTERVAL '7 days',
    NOW() - INTERVAL '6 days',
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
),
-- Class Source 2: Audio (Course 2 - Python for Data Science)
(
    'bbbbbbbb-0000-0000-0000-000000000002'::uuid,
    'audio',
    'completed',
    'Python for Data Science - Audio 1',
    'https://storage.demo.ai/transcripts/python-ds-audio1.txt',
    'https://storage.demo.ai/audios/python-ds-audio1.mp3',
    FALSE,
    NOW() - INTERVAL '5 days',
    NOW() - INTERVAL '4 days',
    NULL,
    '10000000-0000-0000-0000-000000000002'::uuid,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
),
-- Class Source 3: Text (Course 3 - Deep Learning)
(
    'bbbbbbbb-0000-0000-0000-000000000003'::uuid,
    'text',
    'completed',
    'Deep Learning Fundamentals - Text 1',
    'https://storage.demo.ai/transcripts/dl-fundamentals-text1.txt',
    'https://storage.demo.ai/texts/dl-fundamentals-text1.pdf',
    FALSE,
    NOW() - INTERVAL '3 days',
    NOW() - INTERVAL '2 days',
    NULL,
    '10000000-0000-0000-0000-000000000003'::uuid,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 3. DOCUMENT PROCESSING JOBS (linking class_source with processing_job)
-- ============================================================

INSERT INTO document_processing.document_processing_job
(
    "Id",
    "DocumentId",
    "ProcessingJobId"
)
VALUES
(
    'cccccccc-0000-0000-0000-000000000001'::uuid,
    'bbbbbbbb-0000-0000-0000-000000000001'::uuid,
    'aaaaaaaa-0000-0000-0000-000000000001'::uuid
),
(
    'cccccccc-0000-0000-0000-000000000002'::uuid,
    'bbbbbbbb-0000-0000-0000-000000000002'::uuid,
    'aaaaaaaa-0000-0000-0000-000000000002'::uuid
),
(
    'cccccccc-0000-0000-0000-000000000003'::uuid,
    'bbbbbbbb-0000-0000-0000-000000000003'::uuid,
    'aaaaaaaa-0000-0000-0000-000000000003'::uuid
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;

-- ============================================================
-- SUMMARY
-- ============================================================
-- Total records inserted:
-- 3 processing_jobs (all completed)
-- 3 class_sources (video, audio, text)
-- 3 document_processing_jobs (linking each class_source with its processing_job)
-- 
-- All class_sources are associated with:
-- - Teacher ID: 959d0300-4473-4198-b551-6c1c6fb214dc
-- - Different courses (IDs: 10000000-0000-0000-0000-000000000001, 002, 003)
-- ============================================================