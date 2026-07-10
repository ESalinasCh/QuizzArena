BEGIN;

-- Extension to generate UUIDs if needed
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- 1. USERS (only if they don't exist)
-- ============================================================

-- Teacher
INSERT INTO users."user"
(
    "Id",
    "UserName",
    "FirstName",
    "LastName",
    "Email",
    "ExternalProvider",
    "Deleted",
    "Role",
    avatar_url,
    provider_id,
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid,
    'backend-teacher-test1-paigeblank',
    'Paige',
    'Blank',
    'backend.teacher.test1.paige@test.com',
    'keycloak',
    FALSE,
    'teacher',
    NULL,
    '959d0300-4473-4198-b551-6c1c6fb214dc',
    NOW() - INTERVAL '120 days',
    NOW(),
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM users."user"
    WHERE "Id" = '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
);

-- Student
INSERT INTO users."user"
(
    "Id",
    "UserName",
    "FirstName",
    "LastName",
    "Email",
    "ExternalProvider",
    "Deleted",
    "Role",
    avatar_url,
    provider_id,
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    'backend-student-test3-maxmaximus',
    'Max',
    'Maximus',
    'backend.student.test3.max@test.com',
    'keycloak',
    FALSE,
    'student',
    NULL,
    '37976960-c868-45d4-b3c2-4967cb46f4b0',
    NOW() - INTERVAL '90 days',
    NOW(),
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM users."user"
    WHERE "Id" = '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid
);

-- ============================================================
-- 2. COURSES (1 original + 3 new = 4 courses total)
-- ============================================================

INSERT INTO users.course
(
    "Id",
    "Name",
    "Description",
    "Deleted",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt",
    "TeacherId"
)
VALUES
-- Original course
(
    '10000000-0000-0000-0000-000000000001'::uuid,
    'Fundamentos de Inteligencia Artificial',
    'Introductory course on AI, Machine Learning, Generative AI and Prompt Engineering.',
    FALSE,
    NOW() - INTERVAL '85 days',
    NOW(),
    NULL,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
),
-- Course 2: Python for Data Science
(
    '10000000-0000-0000-0000-000000000002'::uuid,
    'Python Programming for Data Science',
    'Practical Python course focused on data analysis, visualization and Machine Learning.',
    FALSE,
    NOW() - INTERVAL '75 days',
    NOW(),
    NULL,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
),
-- Course 3: Deep Learning
(
    '10000000-0000-0000-0000-000000000003'::uuid,
    'Deep Learning Fundamentals',
    'Neural networks, CNN, RNN architectures and practical applications.',
    FALSE,
    NOW() - INTERVAL '65 days',
    NOW(),
    NULL,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
),
-- Course 4: AI Ethics
(
    '10000000-0000-0000-0000-000000000004'::uuid,
    'Ethics and Regulation in Artificial Intelligence',
    'Ethical, legal and regulatory aspects in AI development and implementation.',
    FALSE,
    NOW() - INTERVAL '55 days',
    NOW(),
    NULL,
    '959d0300-4473-4198-b551-6c1c6fb214dc'::uuid
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 3. STUDENT ENROLLMENTS (in all 4 courses)
-- ============================================================

INSERT INTO users.course_student
(
    "Id",
    "StudentId",
    "CourseId",
    "Deleted",
    "DeletedAt"
)
VALUES
-- Course 1
(
    '20000000-0000-0000-0000-000000000001'::uuid,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '10000000-0000-0000-0000-000000000001'::uuid,
    FALSE,
    NULL
),
-- Course 2
(
    '20000000-0000-0000-0000-000000000002'::uuid,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '10000000-0000-0000-0000-000000000002'::uuid,
    FALSE,
    NULL
),
-- Course 3
(
    '20000000-0000-0000-0000-000000000003'::uuid,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '10000000-0000-0000-0000-000000000003'::uuid,
    FALSE,
    NULL
),
-- Course 4
(
    '20000000-0000-0000-0000-000000000004'::uuid,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '10000000-0000-0000-0000-000000000004'::uuid,
    FALSE,
    NULL
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
