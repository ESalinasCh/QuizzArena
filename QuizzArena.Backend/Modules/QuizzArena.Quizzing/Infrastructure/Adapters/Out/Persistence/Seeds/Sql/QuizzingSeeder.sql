BEGIN;

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- 4. QUIZZES
-- ============================================================

INSERT INTO quizzing.quiz
(
    "Id",
    "Title",
    "Description",
    "Status",
    "Deleted",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt",
    "Origin"
)
VALUES
(
    '30000000-0000-0000-0000-000000000001'::uuid,
    'Introducción a la Inteligencia Artificial',
    'Quiz rápido sobre conceptos básicos de IA.',
    'published',
    FALSE,
    NOW() - INTERVAL '70 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000002'::uuid,
    'Machine Learning Básico',
    'Conceptos fundamentales de aprendizaje automático.',
    'published',
    FALSE,
    NOW() - INTERVAL '65 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000003'::uuid,
    'IA Generativa',
    'Modelos generativos, LLM y aplicaciones.',
    'published',
    FALSE,
    NOW() - INTERVAL '60 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000004'::uuid,
    'Ética en Inteligencia Artificial',
    'Privacidad, sesgos y uso responsable de la IA.',
    'published',
    FALSE,
    NOW() - INTERVAL '55 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000005'::uuid,
    'Examen Parcial I',
    'Evaluación del primer bloque: Fundamentos de IA.',
    'published',
    FALSE,
    NOW() - INTERVAL '50 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000006'::uuid,
    'Examen Parcial II',
    'Evaluación del segundo bloque: Machine Learning.',
    'published',
    FALSE,
    NOW() - INTERVAL '42 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000007'::uuid,
    'Examen de Redes Neuronales',
    'Arquitecturas y entrenamiento de redes neuronales.',
    'published',
    FALSE,
    NOW() - INTERVAL '34 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000008'::uuid,
    'Examen de Prompt Engineering',
    'Técnicas modernas de prompting para LLMs.',
    'published',
    FALSE,
    NOW() - INTERVAL '20 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000009'::uuid,
    'Examen Final',
    'Evaluación final del curso de IA.',
    'published',
    FALSE,
    NOW() - INTERVAL '10 days',
    NOW(),
    NULL,
    'manually_created'
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 5. QUESTIONS, OPTIONS Y QUIZ_QUESTIONS (VERSIÓN CORREGIDA)
-- ============================================================

DO $$
DECLARE
    -- IDs de los quizzes
    quiz_ids uuid[] := ARRAY[
        '30000000-0000-0000-0000-000000000001'::uuid,
        '30000000-0000-0000-0000-000000000002'::uuid,
        '30000000-0000-0000-0000-000000000003'::uuid,
        '30000000-0000-0000-0000-000000000004'::uuid,
        '30000000-0000-0000-0000-000000000005'::uuid,
        '30000000-0000-0000-0000-000000000006'::uuid,
        '30000000-0000-0000-0000-000000000007'::uuid,
        '30000000-0000-0000-0000-000000000008'::uuid,
        '30000000-0000-0000-0000-000000000009'::uuid
    ];
    
    -- Preguntas por quiz (8 preguntas cada uno)
    -- Cada pregunta tiene: [enunciado, opción1, opción2, opción3, opción4, posición_correcta (1-4)]
    q1 text[] := ARRAY[
        '¿Qué es la Inteligencia Artificial?', 'Sistemas que imitan la inteligencia humana', 'Solo robots físicos', 'Programas de cálculo matemático', 'Bases de datos avanzadas', '1',
        '¿Cuál es el objetivo principal de la IA?', 'Reemplazar a los humanos', 'Automatizar tareas y resolver problemas', 'Crear robots con emociones', 'Almacenar grandes cantidades de datos', '2',
        '¿Qué es un algoritmo en IA?', 'Un tipo de hardware', 'Una secuencia de instrucciones para resolver problemas', 'Un lenguaje de programación', 'Un tipo de dato', '2',
        '¿Cuándo comenzó el campo de la IA?', 'Década de 1950', 'Década de 1980', 'Década de 2000', 'Década de 2010', '1',
        '¿Qué es el aprendizaje automático?', 'Un tipo de hardware', 'Un subcampo de la IA que permite a las máquinas aprender', 'Un lenguaje de programación', 'Un tipo de base de datos', '2',
        '¿Qué es un modelo de IA?', 'Una representación matemática de un problema', 'Un tipo de hardware', 'Un lenguaje de programación', 'Una base de datos', '1',
        '¿Qué es el procesamiento del lenguaje natural?', 'Analizar datos numéricos', 'Interpretar y generar lenguaje humano', 'Procesar imágenes', 'Almacenar datos', '2',
        '¿Qué es la visión por computadora?', 'Analizar y entender imágenes y videos', 'Procesar texto', 'Analizar datos numéricos', 'Almacenar archivos', '1'
    ];
    
    q2 text[] := ARRAY[
        '¿Qué es el aprendizaje supervisado?', 'El modelo aprende sin etiquetas', 'El modelo aprende con datos etiquetados', 'El modelo aprende por refuerzo', 'El modelo no aprende nada', '2',
        '¿Qué es el aprendizaje no supervisado?', 'El modelo aprende sin etiquetas', 'El modelo aprende con datos etiquetados', 'El modelo aprende por refuerzo', 'El modelo memoriza datos', '1',
        '¿Qué es la regresión lineal?', 'Un modelo de clasificación', 'Un modelo para predecir valores continuos', 'Un modelo para agrupar datos', 'Un modelo de redes neuronales', '2',
        '¿Qué es la clasificación?', 'Predecir valores continuos', 'Predecir categorías o clases', 'Agrupar datos similares', 'Reducir dimensionalidad', '2',
        '¿Qué es el sobreajuste?', 'El modelo generaliza bien', 'El modelo memoriza los datos de entrenamiento', 'El modelo no aprende', 'El modelo es muy simple', '2',
        '¿Qué es la validación cruzada?', 'Una técnica para evaluar modelos', 'Un tipo de algoritmo', 'Un método de visualización', 'Una base de datos', '1',
        '¿Qué es la matriz de confusión?', 'Una tabla para evaluar clasificadores', 'Una base de datos', 'Un algoritmo de clustering', 'Un modelo de regresión', '1',
        '¿Qué es la precisión en ML?', 'Medida de falsos positivos', 'Medida de verdaderos positivos', 'Medida de falsos negativos', 'Medida de error general', '2'
    ];
    
    q3 text[] := ARRAY[
        '¿Qué es la IA generativa?', 'Modelos que generan nuevo contenido', 'Modelos que solo clasifican', 'Modelos que almacenan datos', 'Modelos que calculan', '1',
        '¿Qué es un modelo generativo?', 'Un modelo que predice etiquetas', 'Un modelo que aprende la distribución de los datos', 'Un modelo que solo memoriza', 'Un modelo que no aprende', '2',
        '¿Qué es un GAN?', 'Generative Adversarial Network', 'General Artificial Network', 'Genetic Algorithm Network', 'Graph Analysis Network', '1',
        '¿Qué es un LLM?', 'Large Language Model', 'Little Learning Module', 'Long Linear Matrix', 'Low Level Memory', '1',
        '¿Qué es ChatGPT?', 'Un modelo de IA conversacional', 'Un sistema operativo', 'Una base de datos', 'Un lenguaje de programación', '1',
        '¿Qué es la generación de texto?', 'Crear texto automáticamente con IA', 'Almacenar texto', 'Analizar texto', 'Traducir texto', '1',
        '¿Qué es la generación de imágenes?', 'Crear imágenes automáticamente con IA', 'Procesar imágenes', 'Almacenar imágenes', 'Analizar imágenes', '1',
        '¿Qué es el embedding en IA?', 'Una representación vectorial de datos', 'Un tipo de hardware', 'Un lenguaje de programación', 'Una base de datos', '1'
    ];
    
    q4 text[] := ARRAY[
        '¿Qué es el sesgo en IA?', 'Los modelos son siempre objetivos', 'La tendencia a producir resultados injustos', 'Los modelos nunca se equivocan', 'La IA es neutral', '2',
        '¿Qué es la privacidad en IA?', 'Proteger los datos de los usuarios', 'Compartir todos los datos', 'No usar datos', 'Almacenar datos sin control', '1',
        '¿Qué es la transparencia en IA?', 'Ocultar cómo funcionan los modelos', 'Explicar cómo funcionan los modelos', 'No explicar nada', 'Usar modelos complejos', '2',
        '¿Qué es la responsabilidad en IA?', 'No hacerse responsable de los errores', 'Asumir responsabilidad por las decisiones de la IA', 'La IA no tiene responsabilidad', 'Solo los humanos son responsables', '2',
        '¿Qué es el uso responsable de IA?', 'Usar IA sin control', 'Usar IA de manera ética y segura', 'No usar IA', 'Usar IA solo para negocios', '2',
        '¿Qué es la equidad en IA?', 'Tratar a todos los grupos de manera justa', 'Tratar a todos igual sin importar contexto', 'No considerar la equidad', 'Solo considerar algunos grupos', '1',
        '¿Qué son las normas éticas en IA?', 'Directrices para un desarrollo responsable', 'Requisitos técnicos', 'Leyes internacionales', 'Recomendaciones de marketing', '1',
        '¿Qué es la gobernanza de IA?', 'Marcos para gestionar riesgos de IA', 'No gestionar la IA', 'Solo aspectos técnicos', 'Ignorar los riesgos', '1'
    ];
    
    q5 text[] := ARRAY[
        '¿Qué es la IA fuerte?', 'IA que iguala o supera la inteligencia humana', 'IA básica', 'IA para juegos', 'IA para matemáticas', '1',
        '¿Qué es la IA débil?', 'IA diseñada para tareas específicas', 'IA que supera a los humanos', 'IA general', 'IA consciente', '1',
        '¿Qué es el test de Turing?', 'Prueba para evaluar inteligencia de máquinas', 'Un algoritmo', 'Un lenguaje de programación', 'Una base de datos', '1',
        '¿Qué es el razonamiento en IA?', 'Capacidad de inferir conclusiones', 'Almacenar datos', 'Procesar imágenes', 'Generar texto', '1',
        '¿Qué es la representación del conocimiento?', 'Cómo almacenar información útil en IA', 'Almacenar datos sin estructura', 'No almacenar información', 'Solo usar datos numéricos', '1',
        '¿Qué es la planificación en IA?', 'Secuenciar acciones para lograr objetivos', 'Reaccionar sin planificar', 'Solo memorizar', 'No tener objetivos', '1',
        '¿Qué es la robótica en IA?', 'Aplicación de IA a robots físicos', 'Solo software', 'No relacionado con IA', 'Juegos de computadora', '1',
        '¿Qué es el agente inteligente?', 'Entidad que percibe y actúa en un entorno', 'Un programa simple', 'Una base de datos', 'Un sistema operativo', '1'
    ];
    
    q6 text[] := ARRAY[
        '¿Qué es el deep learning?', 'Aprendizaje con redes neuronales profundas', 'Aprendizaje básico', 'Aprendizaje sin datos', 'Aprendizaje superficial', '1',
        '¿Qué es una red neuronal?', 'Inspirada en el cerebro humano', 'Un algoritmo simple', 'Una base de datos', 'Un lenguaje de programación', '1',
        '¿Qué es la capa oculta?', 'Capas internas de una red neuronal', 'Capa de entrada', 'Capa de salida', 'Sin capas', '1',
        '¿Qué es la función de activación?', 'Introduce no linealidad a la red', 'Una función lineal', 'Un tipo de dato', 'Un algoritmo de búsqueda', '1',
        '¿Qué es el backpropagation?', 'Algoritmo para entrenar redes neuronales', 'Un tipo de red', 'Un método de visualización', 'Una base de datos', '1',
        '¿Qué es el optimizador en deep learning?', 'Ajusta los pesos de la red para minimizar error', 'Un tipo de dato', 'Un algoritmo de búsqueda', 'Un método de visualización', '1',
        '¿Qué es el batch size?', 'Número de muestras procesadas a la vez', 'El tamaño total de la base de datos', 'El número de capas', 'El número de neuronas', '1',
        '¿Qué es el learning rate?', 'Controla qué tan rápido aprende la red', 'El número de épocas', 'El tamaño del batch', 'El número de capas', '1'
    ];
    
    q7 text[] := ARRAY[
        '¿Qué es una CNN?', 'Red neuronal convolucional para imágenes', 'Un tipo de base de datos', 'Un lenguaje de programación', 'Un sistema operativo', '1',
        '¿Qué es una RNN?', 'Red neuronal recurrente para secuencias', 'Una red simple', 'Un algoritmo básico', 'Un tipo de dato', '1',
        '¿Qué es el padding en CNN?', 'Agregar bordes a la imagen de entrada', 'Eliminar píxeles', 'Redimensionar imagen', 'Cambiar colores', '1',
        '¿Qué es el stride en CNN?', 'El paso de movimiento del filtro', 'El tamaño del filtro', 'El número de capas', 'El tipo de activación', '1',
        '¿Qué es el pooling en CNN?', 'Reducir dimensionalidad de la imagen', 'Aumentar resolución', 'Eliminar capas', 'Agregar filtros', '1',
        '¿Qué es el dropout?', 'Técnica de regularización para prevenir sobreajuste', 'Un tipo de red', 'Un algoritmo de optimización', 'Una función de activación', '1',
        '¿Qué es la transferencia de aprendizaje?', 'Reutilizar modelos pre-entrenados', 'Entrenar desde cero', 'Sin aprendizaje previo', 'Aprender sin datos', '1',
        '¿Qué es el fine-tuning?', 'Ajustar un modelo pre-entrenado para nuevas tareas', 'Entrenar desde cero', 'No ajustar nada', 'Usar el modelo sin cambios', '1'
    ];
    
    q8 text[] := ARRAY[
        '¿Qué es un prompt?', 'Una instrucción o pregunta para un LLM', 'Un tipo de base de datos', 'Un algoritmo', 'Un lenguaje de programación', '1',
        '¿Qué es el prompt engineering?', 'Diseñar prompts efectivos para LLMs', 'Programar en Python', 'Diseñar bases de datos', 'Optimizar algoritmos', '1',
        '¿Qué es few-shot prompting?', 'Incluir ejemplos en el prompt', 'No incluir ejemplos', 'Prompt muy corto', 'Prompt sin instrucciones', '1',
        '¿Qué es zero-shot prompting?', 'Prompt sin ejemplos previos', 'Con muchos ejemplos', 'Con pocos ejemplos', 'Sin instrucciones', '1',
        '¿Qué es chain-of-thought?', 'Guía al modelo a razonar paso a paso', 'Dar respuestas directas', 'Sin razonamiento', 'Respuestas cortas', '1',
        '¿Qué es el role prompting?', 'Asignar un rol o personalidad al modelo', 'Sin rol', 'Rol genérico', 'No especificar rol', '1',
        '¿Qué es la temperatura en LLM?', 'Controla la creatividad de las respuestas', 'Controla la velocidad', 'Controla la memoria', 'Controla el tamaño', '1',
        '¿Qué es el top-p sampling?', 'Selección de tokens con probabilidad acumulada', 'Selección aleatoria', 'Selección del primer token', 'Selección del último token', '1'
    ];
    
    q9 text[] := ARRAY[
        '¿Qué es la IA en el futuro?', 'Integración en todos los aspectos de la vida', 'Solo en robots', 'Solo en computadoras', 'No tendrá impacto', '1',
        '¿Cuál es un desafío de la IA?', 'Sesgo y equidad', 'No hay desafíos', 'Solo problemas técnicos', 'No tiene impacto social', '1',
        '¿Qué es el impacto social de IA?', 'Cambios en la sociedad y economía', 'No hay impacto', 'Solo impacto positivo', 'Solo impacto negativo', '1',
        '¿Qué es la regulación de IA?', 'Marcos legales para gobernar la IA', 'No regular IA', 'Solo autorregulación', 'Sin normas', '1',
        '¿Qué es la colaboración humano-IA?', 'Trabajar junto con sistemas de IA', 'IA sin humanos', 'Humanos sin IA', 'Competencia entre humanos e IA', '1',
        '¿Qué es la educación en IA?', 'Aprender sobre IA y su aplicación', 'No aprender sobre IA', 'Solo aprender a programar', 'No es importante', '1',
        '¿Qué son los agentes autónomos?', 'Sistemas que toman decisiones sin intervención humana', 'Sistemas que necesitan humanos', 'Sistemas sin decisiones', 'Sistemas básicos', '1',
        '¿Qué es la singularidad tecnológica?', 'Punto donde la IA supera a la inteligencia humana', 'Un tipo de algoritmo', 'Un hardware específico', 'Una base de datos', '1'
    ];
    
    questions text[][] := ARRAY[q1, q2, q3, q4, q5, q6, q7, q8, q9];
    
    -- Variables
    i int;
    j int;
    k int;
    question_id uuid;
    option_id uuid;
    correct_pos int;
    quiz_id uuid;
    base_index int;

BEGIN
    -- Recorrer cada quiz
    FOR i IN 1..array_length(quiz_ids, 1) LOOP
        quiz_id := quiz_ids[i];
        
        -- Recorrer cada pregunta del quiz (8 preguntas)
        FOR j IN 1..8 LOOP
            base_index := (j - 1) * 6 + 1;
            
            -- Generar UUID para la pregunta
            question_id := gen_random_uuid();
            correct_pos := questions[i][base_index + 5]::int;
            
            -- Insertar pregunta
            INSERT INTO quizzing.question (
                "Id",
                "Content",
                "Status",
                "Type",
                "Deleted",
                "CreatedAt",
                "UpdatedAt",
                "DeletedAt",
                "ProcessingJobId",
                "Origin",
                "Justification"
            ) VALUES (
                question_id,
                questions[i][base_index],
                'verified',
                'single_choice',
                FALSE,
                NOW() - INTERVAL '70 days' + (i * INTERVAL '5 days') + (j * INTERVAL '1 hour'),
                NOW(),
                NULL,
                'aaaaaaaa-0000-0000-0000-000000000001'::uuid,
                'manually_created',
                'Pregunta creada para demo'
            );
            
            -- Insertar opciones
            FOR k IN 1..4 LOOP
                option_id := gen_random_uuid();
                INSERT INTO quizzing."option" (
                    "Id",
                    "Description",
                    "IsCorrect",
                    "Position",
                    "Deleted",
                    "CreatedAt",
                    "UpdatedAt",
                    "DeletedAt",
                    "QuestionId"
                ) VALUES (
                    option_id,
                    questions[i][base_index + k],
                    (k = correct_pos),
                    k,
                    FALSE,
                    NOW() - INTERVAL '70 days' + (i * INTERVAL '5 days') + (j * INTERVAL '1 hour'),
                    NOW(),
                    NULL,
                    question_id
                );
            END LOOP;
            
            -- Insertar quiz_question
            INSERT INTO quizzing.quiz_question (
                "Id",
                "Position",
                "ValueScore",
                "Deleted",
                "CreatedAt",
                "UpdatedAt",
                "DeletedAt",
                "QuizId",
                "QuestionId"
            ) VALUES (
                gen_random_uuid(),
                j,
                12.50,
                FALSE,
                NOW() - INTERVAL '70 days' + (i * INTERVAL '5 days') + (j * INTERVAL '1 hour'),
                NOW(),
                NULL,
                quiz_id,
                question_id
            );
        END LOOP;
    END LOOP;
END $$;

-- ============================================================
-- 6. MATCHES
-- ============================================================

-- 5 matches de tipo EXAM:  5 activos
-- 4 matches de tipo SINGLE: todos activos

INSERT INTO quizzing."match"
(
    "Id",
    "Code",
    "Status",
    "StartedAt",
    "FinishedAt",
    "Mode",
    "TimeMinutes",
    "Deleted",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt",
    "CourseId",
    "QuizId",
    "AttemptsAmount",
    "QuestionsAmount",
    "ShuffleOptions",
    "ShuffleQuestion",
    "Title"
)
VALUES
-- EXAM 
(
    '40000000-0000-0000-0000-000000000001'::uuid,
    'EXAM-001',
    'active',
    NOW() - INTERVAL '45 days',
    NOW() - INTERVAL '45 days' + INTERVAL '60 minutes',
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '46 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000005'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Examen Parcial I'
),
(
    '40000000-0000-0000-0000-000000000002'::uuid,
    'EXAM-002',
    'active',
    NOW() - INTERVAL '35 days',
    NOW() - INTERVAL '35 days' + INTERVAL '60 minutes',
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '36 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000006'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Examen Parcial II'
),
(
    '40000000-0000-0000-0000-000000000003'::uuid,
    'EXAM-003',
    'active',
    NOW() - INTERVAL '25 days',
    NOW() - INTERVAL '25 days' + INTERVAL '60 minutes',
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '26 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000007'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Examen de Redes Neuronales'
),
-- EXAM activos
(
    '40000000-0000-0000-0000-000000000004'::uuid,
    'EXAM-004',
    'active',
    NOW() - INTERVAL '5 days',
    NULL,
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '6 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000008'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Examen de Prompt Engineering'
),
(
    '40000000-0000-0000-0000-000000000005'::uuid,
    'EXAM-005',
    'active',
    NOW() - INTERVAL '2 days',
    NULL,
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '3 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000009'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Examen Final'
),
-- SINGLE activos
(
    '40000000-0000-0000-0000-000000000006'::uuid,
    'SINGLE-001',
    'active',
    NOW() - INTERVAL '7 days',
    NULL,
    'single',
    15,
    FALSE,
    NOW() - INTERVAL '8 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000001'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Quiz: Introducción a la IA'
),
(
    '40000000-0000-0000-0000-000000000007'::uuid,
    'SINGLE-002',
    'active',
    NOW() - INTERVAL '6 days',
    NULL,
    'single',
    15,
    FALSE,
    NOW() - INTERVAL '7 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000002'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Quiz: Machine Learning Básico'
),
(
    '40000000-0000-0000-0000-000000000008'::uuid,
    'SINGLE-003',
    'active',
    NOW() - INTERVAL '4 days',
    NULL,
    'single',
    15,
    FALSE,
    NOW() - INTERVAL '5 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000003'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Quiz: IA Generativa'
),
(
    '40000000-0000-0000-0000-000000000009'::uuid,
    'SINGLE-004',
    'active',
    NOW() - INTERVAL '3 days',
    NULL,
    'single',
    15,
    FALSE,
    NOW() - INTERVAL '4 days',
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000004'::uuid,
    10,
    8,
    FALSE,
    FALSE,
    'Quiz: Ética en IA'
);

-- ============================================================
-- 7. MATCH_ATTEMPTS
-- ============================================================

INSERT INTO quizzing.match_attempt
(
    "Id",
    "StartDateTime",
    "EndDateTime",
    "JoinedAt",
    "Nickname",
    "Status",
    "Score",
    "UserId",
    "MatchId"
)
VALUES
-- Max: 3 exams completed
(
    '50000000-0000-0000-0000-000000000001'::uuid,
    NOW() - INTERVAL '45 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '55 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '3 minutes',
    'max.maximus',
    'completed',
    100.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000001'::uuid
),
(
    '50000000-0000-0000-0000-000000000002'::uuid,
    NOW() - INTERVAL '35 days' + INTERVAL '7 minutes',
    NOW() - INTERVAL '35 days' + INTERVAL '57 minutes',
    NOW() - INTERVAL '35 days' + INTERVAL '4 minutes',
    'max.maximus',
    'completed',
    87.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000002'::uuid
),
(
    '50000000-0000-0000-0000-000000000003'::uuid,
    NOW() - INTERVAL '25 days' + INTERVAL '3 minutes',
    NOW() - INTERVAL '25 days' + INTERVAL '52 minutes',
    NOW() - INTERVAL '25 days' + INTERVAL '2 minutes',
    'max.maximus',
    'completed',
    75.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000003'::uuid
),
-- Max: 4 single/quiz attempts (completed, completed, timeout, in_progress)
(
    '50000000-0000-0000-0000-000000000004'::uuid,
    NOW() - INTERVAL '7 days' + INTERVAL '10 minutes',
    NOW() - INTERVAL '7 days' + INTERVAL '23 minutes',
    NOW() - INTERVAL '7 days' + INTERVAL '8 minutes',
    'max.maximus',
    'completed',
    75.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000006'::uuid
),
(
    '50000000-0000-0000-0000-000000000005'::uuid,
    NOW() - INTERVAL '6 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '19 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '3 minutes',
    'max.maximus',
    'completed',
    62.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000007'::uuid
),
(
    '50000000-0000-0000-0000-000000000006'::uuid,
    NOW() - INTERVAL '4 days' + INTERVAL '2 minutes',
    NOW() - INTERVAL '4 days' + INTERVAL '17 minutes',
    NOW() - INTERVAL '4 days' + INTERVAL '1 minute',
    'max.maximus',
    'timeout',
    37.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000008'::uuid
),
(
    '50000000-0000-0000-0000-000000000007'::uuid,
    NOW() - INTERVAL '3 days' + INTERVAL '1 minute',
    NULL,
    NOW() - INTERVAL '3 days',
    'max.maximus',
    'in_progress',
    0.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000009'::uuid
);

-- student01: in_progress on an active exam
INSERT INTO quizzing.match_attempt
(
    "Id",
    "StartDateTime",
    "EndDateTime",
    "JoinedAt",
    "Nickname",
    "Status",
    "Score",
    "UserId",
    "MatchId"
)
VALUES
(
    '50000000-0000-0000-0000-000000000008'::uuid,
    NOW() - INTERVAL '5 days' + INTERVAL '2 minutes',
    NULL,
    NOW() - INTERVAL '5 days',
    'student01',
    'in_progress',
    0.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000004'::uuid
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 8. MATCH_ATTEMPT_QUESTIONS
-- ============================================================

-- Para MATCH_ATTEMPT 1 (EX-001)
INSERT INTO quizzing.match_attempt_question
("Id", "ValueScore", "MatchAttemptId", "QuestionId")
SELECT
    gen_random_uuid(),
    12.50,
    '50000000-0000-0000-0000-000000000001'::uuid,
        qq."QuestionId"
FROM quizzing.quiz_question qq
WHERE qq."QuizId" = '30000000-0000-0000-0000-000000000005'::uuid
ORDER BY qq."Position";

-- Para MATCH_ATTEMPT 2 (EX-002)
INSERT INTO quizzing.match_attempt_question
("Id", "ValueScore", "MatchAttemptId", "QuestionId")
SELECT
    gen_random_uuid(),
    12.50,
    '50000000-0000-0000-0000-000000000002'::uuid,
    qq."QuestionId"
FROM quizzing.quiz_question qq
WHERE qq."QuizId" = '30000000-0000-0000-0000-000000000006'::uuid
ORDER BY qq."Position";

-- Para MATCH_ATTEMPT 3 (EX-003)
INSERT INTO quizzing.match_attempt_question
("Id", "ValueScore", "MatchAttemptId", "QuestionId")
SELECT
    gen_random_uuid(),
    12.50,
    '50000000-0000-0000-0000-000000000003'::uuid,
    qq."QuestionId"
FROM quizzing.quiz_question qq
WHERE qq."QuizId" = '30000000-0000-0000-0000-000000000007'::uuid
ORDER BY qq."Position";

-- ============================================================
-- 9. ANSWERS
-- ============================================================

-- Función auxiliar para obtener el ID de la opción correcta de una pregunta
-- Primero, insertamos las respuestas para el primer intento (100% correcto)
DO $$
DECLARE
    attempt_id uuid := '50000000-0000-0000-0000-000000000001'::uuid;
    quiz_id uuid := '30000000-0000-0000-0000-000000000005'::uuid;
    rec RECORD;
    correct_option_id uuid;
    start_time timestamp;
    time_ms int;
    position int := 1;
BEGIN
    -- Obtener la hora de inicio del intento
    SELECT "StartDateTime" INTO start_time 
    FROM quizzing.match_attempt 
    WHERE "Id" = attempt_id;
    
    -- Para cada pregunta del quiz
    FOR rec IN 
        SELECT q."Id" as question_id
        FROM quizzing.quiz_question qq
        JOIN quizzing.question q ON q."Id" = qq."QuestionId"
        WHERE qq."QuizId" = quiz_id
        ORDER BY qq."Position"
    LOOP
        -- Obtener la opción correcta
        SELECT "Id" INTO correct_option_id
        FROM quizzing."option"
        WHERE "QuestionId" = rec.question_id AND "IsCorrect" = TRUE
        LIMIT 1;
        
        -- Tiempo de respuesta entre 2 y 15 segundos
        time_ms := (2000 + (random() * 13000))::int;
        
        -- Insertar respuesta
        INSERT INTO quizzing.answer
        ("Id", "IsCorrect", "AnsweredAt", "TimeMs", "OptionId", "QuestionId", "MatchAttemptId")
        VALUES (
            gen_random_uuid(),
            TRUE,
            start_time + (position * INTERVAL '30 seconds') + (time_ms * INTERVAL '1 millisecond'),
            time_ms,
            correct_option_id,
            rec.question_id,
            attempt_id
        );
        
        position := position + 1;
    END LOOP;
END $$;

-- Segundo intento (87.50% - 7 correctas, 1 incorrecta)
DO $$
DECLARE
    attempt_id uuid := '50000000-0000-0000-0000-000000000002'::uuid;
    quiz_id uuid := '30000000-0000-0000-0000-000000000006'::uuid;
    rec RECORD;
    correct_option_id uuid;
    incorrect_option_id uuid;
    start_time timestamp;
    time_ms int;
    position int := 1;
    is_correct boolean;
    question_counter int := 0;
BEGIN
    -- Obtener la hora de inicio del intento
    SELECT "StartDateTime" INTO start_time 
    FROM quizzing.match_attempt 
    WHERE "Id" = attempt_id;
    
    -- Para cada pregunta del quiz
    FOR rec IN 
        SELECT q."Id" as question_id
        FROM quizzing.quiz_question qq
        JOIN quizzing.question q ON q."Id" = qq."QuestionId"
        WHERE qq."QuizId" = quiz_id
        ORDER BY qq."Position"
    LOOP
        question_counter := question_counter + 1;
        
        -- Obtener la opción correcta
        SELECT "Id" INTO correct_option_id
        FROM quizzing."option"
        WHERE "QuestionId" = rec.question_id AND "IsCorrect" = TRUE
        LIMIT 1;
        
        -- La pregunta 3 será incorrecta (para obtener 87.50% = 7/8 correctas)
        IF question_counter = 3 THEN
            -- Obtener una opción incorrecta
            SELECT "Id" INTO incorrect_option_id
            FROM quizzing."option"
            WHERE "QuestionId" = rec.question_id AND "IsCorrect" = FALSE
            ORDER BY random()
            LIMIT 1;
            
            is_correct := FALSE;
        ELSE
            incorrect_option_id := NULL;
            is_correct := TRUE;
        END IF;
        
        -- Tiempo de respuesta entre 2 y 15 segundos
        time_ms := (2000 + (random() * 13000))::int;
        
        -- Insertar respuesta
        INSERT INTO quizzing.answer
        ("Id", "IsCorrect", "AnsweredAt", "TimeMs", "OptionId", "QuestionId", "MatchAttemptId")
        VALUES (
            gen_random_uuid(),
            is_correct,
            start_time + (position * INTERVAL '35 seconds') + (time_ms * INTERVAL '1 millisecond'),
            time_ms,
            CASE WHEN is_correct THEN correct_option_id ELSE incorrect_option_id END,
            rec.question_id,
            attempt_id
        );
        
        position := position + 1;
    END LOOP;
END $$;

-- Tercer intento (75.00% - 6 correctas, 2 incorrectas)
DO $$
DECLARE
    attempt_id uuid := '50000000-0000-0000-0000-000000000003'::uuid;
    quiz_id uuid := '30000000-0000-0000-0000-000000000007'::uuid;
    rec RECORD;
    correct_option_id uuid;
    incorrect_option_id uuid;
    start_time timestamp;
    time_ms int;
    position int := 1;
    is_correct boolean;
    question_counter int := 0;
BEGIN
    -- Obtener la hora de inicio del intento
    SELECT "StartDateTime" INTO start_time 
    FROM quizzing.match_attempt 
    WHERE "Id" = attempt_id;
    
    -- Para cada pregunta del quiz
    FOR rec IN 
        SELECT q."Id" as question_id
        FROM quizzing.quiz_question qq
        JOIN quizzing.question q ON q."Id" = qq."QuestionId"
        WHERE qq."QuizId" = quiz_id
        ORDER BY qq."Position"
    LOOP
        question_counter := question_counter + 1;
        
        -- Obtener la opción correcta
        SELECT "Id" INTO correct_option_id
        FROM quizzing."option"
        WHERE "QuestionId" = rec.question_id AND "IsCorrect" = TRUE
        LIMIT 1;
        
        -- Las preguntas 2 y 5 serán incorrectas (para obtener 75.00% = 6/8 correctas)
        IF question_counter = 2 OR question_counter = 5 THEN
            -- Obtener una opción incorrecta
            SELECT "Id" INTO incorrect_option_id
            FROM quizzing."option"
            WHERE "QuestionId" = rec.question_id AND "IsCorrect" = FALSE
            ORDER BY random()
            LIMIT 1;
            
            is_correct := FALSE;
        ELSE
            incorrect_option_id := NULL;
            is_correct := TRUE;
        END IF;
        
        -- Tiempo de respuesta entre 3 y 18 segundos (un poco más lentos)
        time_ms := (3000 + (random() * 15000))::int;
        
        -- Insertar respuesta
        INSERT INTO quizzing.answer
        ("Id", "IsCorrect", "AnsweredAt", "TimeMs", "OptionId", "QuestionId", "MatchAttemptId")
        VALUES (
            gen_random_uuid(),
            is_correct,
            start_time + (position * INTERVAL '40 seconds') + (time_ms * INTERVAL '1 millisecond'),
            time_ms,
            CASE WHEN is_correct THEN correct_option_id ELSE incorrect_option_id END,
            rec.question_id,
            attempt_id
        );
        
        position := position + 1;
    END LOOP;
END $$;

-- ============================================================
-- 10. ADDITIONAL QUIZZES (draft and archived for variety)
-- ============================================================

INSERT INTO quizzing.quiz
(
    "Id", "Title", "Description", "Status", "Deleted",
    "CreatedAt", "UpdatedAt", "DeletedAt", "Origin"
)
VALUES
(
    '30000000-0000-0000-0000-000000000010'::uuid,
    'Introducción a Python (Borrador)',
    'Quiz en preparación sobre fundamentos de Python.',
    'draft',
    FALSE,
    NOW() - INTERVAL '5 days',
    NOW(),
    NULL,
    'manually_created'
),
(
    '30000000-0000-0000-0000-000000000011'::uuid,
    'Historia de la Computación',
    'Quiz archivado sobre historia de la computación.',
    'archived',
    FALSE,
    NOW() - INTERVAL '180 days',
    NOW() - INTERVAL '90 days',
    NULL,
    'manually_created'
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 11. ADDITIONAL MATCHES (expired and pending for variety)
-- ============================================================

INSERT INTO quizzing."match"
(
    "Id", "Code", "Status", "StartedAt", "FinishedAt", "Mode",
    "TimeMinutes", "Deleted", "CreatedAt", "UpdatedAt", "DeletedAt",
    "CourseId", "QuizId", "AttemptsAmount", "QuestionsAmount",
    "ShuffleOptions", "ShuffleQuestion", "Title"
)
VALUES
(
    '40000000-0000-0000-0000-000000000010'::uuid,
    'EXAM-OLD-001',
    'expired',
    NOW() - INTERVAL '90 days',
    NOW() - INTERVAL '89 days' + INTERVAL '60 minutes',
    'exam',
    60,
    FALSE,
    NOW() - INTERVAL '91 days',
    NOW() - INTERVAL '89 days',
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000005'::uuid,
    10, 8, FALSE, FALSE,
    'Examen Parcial I (Pasado)'
),
(
    '40000000-0000-0000-0000-000000000011'::uuid,
    'SINGLE-OLD-001',
    'expired',
    NOW() - INTERVAL '60 days',
    NOW() - INTERVAL '59 days' + INTERVAL '15 minutes',
    'single',
    15,
    FALSE,
    NOW() - INTERVAL '61 days',
    NOW() - INTERVAL '59 days',
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000001'::uuid,
    10, 8, FALSE, FALSE,
    'Quiz: Intro IA (Pasado)'
),
(
    '40000000-0000-0000-0000-000000000012'::uuid,
    'EXAM-NEXT-001',
    'pending',
    NOW() + INTERVAL '5 days',
    NULL,
    'exam',
    60,
    FALSE,
    NOW(),
    NOW(),
    NULL,
    '10000000-0000-0000-0000-000000000001'::uuid,
    '30000000-0000-0000-0000-000000000009'::uuid,
    10, 8, FALSE, FALSE,
    'Examen Final (Próximo)'
)
ON CONFLICT ("Id") DO NOTHING;

-- student01 attempts on the expired matches
INSERT INTO quizzing.match_attempt
(
    "Id", "StartDateTime", "EndDateTime", "JoinedAt",
    "Nickname", "Status", "Score", "UserId", "MatchId"
)
VALUES
(
    '50000000-0000-0000-0000-000000000011'::uuid,
    NOW() - INTERVAL '90 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '90 days' + INTERVAL '58 minutes',
    NOW() - INTERVAL '90 days' + INTERVAL '3 minutes',
    'student01',
    'completed',
    75.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000010'::uuid
),
(
    '50000000-0000-0000-0000-000000000012'::uuid,
    NOW() - INTERVAL '60 days' + INTERVAL '2 minutes',
    NOW() - INTERVAL '60 days' + INTERVAL '16 minutes',
    NOW() - INTERVAL '60 days' + INTERVAL '1 minute',
    'student01',
    'timeout',
    25.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000011'::uuid
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 12. VERIFICACIÓN FINAL (opcional - descomentar para validar)
-- ============================================================

-- Verificar usuarios
-- SELECT "Id", "UserName", "FirstName", "LastName", "Role" FROM users."user" 
-- WHERE "Id" IN ('37976960-c868-45d4-b3c2-4967cb46f4b0', '959d0300-4473-4198-b551-6c1c6fb214dc');

-- Verificar curso y matrícula
-- SELECT c."Name", c."Description", cs."StudentId" 
-- FROM users.course c 
-- JOIN users.course_student cs ON cs."CourseId" = c."Id"
-- WHERE cs."StudentId" = '37976960-c868-45d4-b3c2-4967cb46f4b0';

-- Verificar quizzes
-- SELECT "Id", "Title", "Status", "Origin" FROM quizzing.quiz;

-- Verificar preguntas por quiz
-- SELECT q."Title", COUNT(qq."QuestionId") as question_count,
--        SUM(qq."ValueScore") as total_score
-- FROM quizzing.quiz q
-- JOIN quizzing.quiz_question qq ON qq."QuizId" = q."Id"
-- GROUP BY q."Id", q."Title"
-- ORDER BY q."Title";

-- Verificar matches
-- SELECT "Id", "Code", "Mode", "Status", "Title" 
-- FROM quizzing."match" 
-- ORDER BY "StartedAt";

-- Verificar intentos del estudiante
-- SELECT ma."Id", ma."Score", ma."Status", m."Title", m."Mode"
-- FROM quizzing.match_attempt ma
-- JOIN quizzing."match" m ON m."Id" = ma."MatchId"
-- WHERE ma."UserId" = '37976960-c868-45d4-b3c2-4967cb46f4b0'
-- ORDER BY ma."StartDateTime";

-- Verificar respuestas del estudiante
-- SELECT COUNT(*) as total_answers,
--        SUM(CASE WHEN "IsCorrect" THEN 1 ELSE 0 END) as correct_answers
-- FROM quizzing.answer
-- WHERE "MatchAttemptId" IN (
--     SELECT "Id" FROM quizzing.match_attempt 
--     WHERE "UserId" = '37976960-c868-45d4-b3c2-4967cb46f4b0'
-- );

-- Verificar consistencia de puntuaciones
-- SELECT ma."Id", ma."Score", SUM(maq."ValueScore") as calculated_score
-- FROM quizzing.match_attempt ma
-- JOIN quizzing.match_attempt_question maq ON maq."MatchAttemptId" = ma."Id"
-- WHERE ma."UserId" = '37976960-c868-45d4-b3c2-4967cb46f4b0'
-- GROUP BY ma."Id", ma."Score";

COMMIT;
