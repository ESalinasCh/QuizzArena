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
-- 5. QUESTIONS, OPTIONS Y QUIZ_QUESTIONS
-- ============================================================

DO $$
DECLARE
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

    -- 7 elementos por pregunta: [enunciado, opt1, opt2, opt3, opt4, tipo, mascara]
    -- tipo: 'single_choice' | 'multiple_choice' | 'true_false'
    -- mascara: 4 chars para single/multiple ('1000'=opt1, '0110'=opt2+opt3)
    --          2 chars para true_false ('10'=Verdadero, '01'=Falso)
    -- true_false: opt3 y opt4 son '' y no se insertan
    -- Estructura por quiz: Q1-Q4 single_choice, Q5-Q6 multiple_choice, Q7-Q8 true_false

    q1 text[] := ARRAY[
        '¿Qué es la Inteligencia Artificial?','Sistemas que imitan la inteligencia humana','Solo robots físicos','Programas de cálculo matemático','Bases de datos avanzadas','single_choice','1000',
        '¿Cuál es el objetivo principal de la IA?','Reemplazar a los humanos','Automatizar tareas y resolver problemas','Crear robots con emociones','Almacenar grandes cantidades de datos','single_choice','0100',
        '¿En qué década surgió el campo de la IA?','1930','1980','1950','2000','single_choice','0010',
        '¿Qué es el procesamiento del lenguaje natural?','Analizar datos numéricos','Interpretar y generar lenguaje humano','Procesar imágenes','Almacenar datos textuales','single_choice','0100',
        '¿Cuáles de los siguientes son subcampos de la IA?','Machine Learning','Visión por computadora','Contabilidad financiera','Procesamiento del lenguaje natural','multiple_choice','1101',
        '¿Qué aplicaciones actuales utilizan IA?','Reconocimiento de voz','Gestión manual de archivos','Traducción automática','Asistentes virtuales','multiple_choice','1011',
        '¿La IA puede aprender patrones a partir de datos?','Verdadero','Falso','','','true_false','10',
        '¿El campo de la IA surgió en la década de 2000?','Verdadero','Falso','','','true_false','01'
    ];

    q2 text[] := ARRAY[
        '¿Qué es el aprendizaje supervisado?','El modelo aprende con datos etiquetados','El modelo aprende sin etiquetas','El modelo aprende por refuerzo','El modelo memoriza datos','single_choice','1000',
        '¿Qué es la regresión lineal?','Un modelo de clasificación','Un modelo para predecir valores continuos','Un modelo de agrupamiento','Un modelo de redes neuronales','single_choice','0100',
        '¿Qué es el sobreajuste (overfitting)?','El modelo generaliza muy bien','El modelo memoriza los datos de entrenamiento','El modelo es demasiado simple','El modelo no converge','single_choice','0100',
        '¿Qué evalúa la matriz de confusión?','La velocidad del modelo','El rendimiento de clasificadores','El tamaño del dataset','La topología de la red','single_choice','0100',
        '¿Cuáles son técnicas de aprendizaje no supervisado?','Clustering K-Means','Árbol de decisión supervisado','Reducción de dimensionalidad PCA','Redes adversariales generativas','multiple_choice','1011',
        '¿Qué métricas evalúan clasificadores?','Precisión (Precision)','Tamaño del dataset','Exhaustividad (Recall)','F1-Score','multiple_choice','1011',
        '¿La validación cruzada ayuda a detectar sobreajuste?','Verdadero','Falso','','','true_false','10',
        '¿El aprendizaje no supervisado requiere datos etiquetados?','Verdadero','Falso','','','true_false','01'
    ];

    q3 text[] := ARRAY[
        '¿Qué son los modelos generativos?','Modelos que solo clasifican datos','Modelos que aprenden la distribución de datos para generar nuevo contenido','Modelos que almacenan datos','Modelos de búsqueda','single_choice','0100',
        '¿Qué significa GAN?','Generative Adversarial Network','General Artificial Network','Genetic Algorithm Node','Graph Analysis Network','single_choice','1000',
        '¿Qué es un LLM?','Large Language Model','Little Learning Module','Long Linear Matrix','Low Level Memory','single_choice','1000',
        '¿Qué controla la temperatura en un LLM?','La velocidad de respuesta','La creatividad y aleatoriedad de las respuestas','El tamaño del modelo','La memoria del contexto','single_choice','0100',
        '¿Cuáles son ejemplos de IA generativa?','GPT para generación de texto','DALL-E para generación de imágenes','Un sistema de detección de spam','Stable Diffusion para imágenes','multiple_choice','1101',
        '¿Qué técnicas usa la IA generativa?','Redes adversariales (GAN)','Modelos de difusión','Búsqueda binaria','Transformers','multiple_choice','1101',
        '¿ChatGPT es un ejemplo de IA generativa?','Verdadero','Falso','','','true_false','10',
        '¿Los modelos GAN usan solo una red neuronal?','Verdadero','Falso','','','true_false','01'
    ];

    q4 text[] := ARRAY[
        '¿Qué es el sesgo algorítmico?','La tendencia del modelo a producir resultados injustos','La velocidad del modelo','La cantidad de datos de entrenamiento','El número de capas del modelo','single_choice','1000',
        '¿Qué principio busca la transparencia en IA?','Ocultar cómo funcionan los modelos','Explicar cómo los modelos toman decisiones','No usar datos sensibles','Maximizar la velocidad','single_choice','0100',
        '¿Qué es la gobernanza de IA?','Un lenguaje de programación para IA','Marcos para gestionar riesgos y responsabilidades de la IA','Una técnica de entrenamiento','Un tipo de hardware','single_choice','0100',
        '¿Qué es la privacidad diferencial?','Una técnica para proteger datos individuales en análisis estadísticos','Un tipo de red neuronal','Un método de cifrado convencional','Una técnica de visualización','single_choice','1000',
        '¿Cuáles son principios del uso ético de la IA?','Transparencia','Equidad','Maximización del beneficio sin límites','Responsabilidad','multiple_choice','1101',
        '¿Qué riesgos presenta la IA sin supervisión ética?','Discriminación algorítmica','Mayor velocidad de procesamiento','Vulneración de privacidad','Propagación de desinformación','multiple_choice','1011',
        '¿El sesgo en IA puede provenir de datos de entrenamiento sesgados?','Verdadero','Falso','','','true_false','10',
        '¿La IA siempre toma decisiones objetivas e imparciales?','Verdadero','Falso','','','true_false','01'
    ];

    q5 text[] := ARRAY[
        '¿Qué distingue a la IA débil de la IA fuerte?','La IA débil está diseñada para tareas específicas','La IA fuerte solo funciona offline','La IA débil requiere más datos','La IA fuerte es más barata','single_choice','1000',
        '¿En qué consiste el Test de Turing?','Medir la velocidad de una computadora','Evaluar si una máquina exhibe comportamiento inteligente indistinguible del humano','Probar la capacidad de almacenamiento','Verificar la seguridad del sistema','single_choice','0100',
        '¿Qué es un agente inteligente?','Un programa simple sin percepción del entorno','Una entidad que percibe su entorno y actúa para lograr objetivos','Un tipo de base de datos','Un lenguaje de marcado','single_choice','0100',
        '¿Qué es la representación del conocimiento en IA?','Almacenar datos sin estructura','Codificar información para que la IA pueda razonar sobre ella','Solo usar datos numéricos','No almacenar información','single_choice','0100',
        '¿Qué características definen a la IA fuerte (AGI)?','Capacidad de razonar en dominios generales','Conciencia de sí misma','Especialización en una tarea única','Adaptación sin reentrenamiento explícito','multiple_choice','1101',
        '¿Qué áreas aplica la IA en robótica?','Percepción del entorno','Planificación de movimientos','Gestión de bases de datos relacionales','Toma de decisiones autónoma','multiple_choice','1101',
        '¿La IA débil puede superar a humanos en tareas específicas?','Verdadero','Falso','','','true_false','10',
        '¿Actualmente existe una IA fuerte (AGI) real?','Verdadero','Falso','','','true_false','01'
    ];

    q6 text[] := ARRAY[
        '¿Qué es el deep learning?','Aprendizaje con redes neuronales de pocas capas','Aprendizaje con redes neuronales profundas (muchas capas)','Aprendizaje sin datos','Aprendizaje superficial','single_choice','0100',
        '¿Qué hace la función de activación?','Suma todas las entradas linealmente','Introduce no-linealidad en la red neuronal','Almacena los pesos del modelo','Calcula el error de salida','single_choice','0100',
        '¿Qué algoritmo ajusta los pesos entrenando redes neuronales?','Forward propagation','Backpropagation','Clustering K-Means','Regresión lineal','single_choice','0100',
        '¿Qué controla el learning rate?','El número de capas de la red','Qué tan grandes son los ajustes de pesos en cada paso','El tamaño del batch','El número de neuronas','single_choice','0100',
        '¿Cuáles son técnicas de regularización en deep learning?','Dropout','L2 Regularization','Aumentar el learning rate','Batch Normalization','multiple_choice','1101',
        '¿Qué componentes forman una red neuronal?','Capas de entrada','Capas ocultas','Tablas de base de datos','Capas de salida','multiple_choice','1101',
        '¿El dropout se usa para prevenir el sobreajuste?','Verdadero','Falso','','','true_false','10',
        '¿Las redes neuronales requieren siempre GPU para entrenarse?','Verdadero','Falso','','','true_false','01'
    ];

    q7 text[] := ARRAY[
        '¿Para qué tipo de datos se usa principalmente una CNN?','Series temporales','Imágenes y video','Texto plano','Datos tabulares','single_choice','0100',
        '¿Qué hace la capa de pooling en una CNN?','Aumenta la resolución de la imagen','Reduce la dimensionalidad de los mapas de características','Añade más filtros','Normaliza los pesos','single_choice','0100',
        '¿Qué tipo de datos procesa mejor una RNN?','Imágenes estáticas','Datos tabulares','Secuencias y series temporales','Grafos','single_choice','0010',
        '¿En qué consiste la transferencia de aprendizaje?','Entrenar una red desde cero en cada tarea','Reutilizar pesos de un modelo pre-entrenado para tareas relacionadas','No usar datos previos','Solo usar modelos simples','single_choice','0100',
        '¿Cuáles son operaciones propias de una CNN?','Convolución','Pooling','Backpropagation en grafos','Detección de bordes con filtros','multiple_choice','1101',
        '¿Qué variantes de RNN existen?','LSTM','GRU','CNN residual','Redes de Elman','multiple_choice','1101',
        '¿Las CNN son especialmente efectivas para reconocimiento de imágenes?','Verdadero','Falso','','','true_false','10',
        '¿Una RNN tiene memoria de las entradas anteriores en la secuencia?','Verdadero','Falso','','','true_false','10'
    ];

    q8 text[] := ARRAY[
        '¿Qué es un prompt en el contexto de los LLM?','Una instrucción o entrada que se le da al modelo','Un tipo de red neuronal','Un algoritmo de búsqueda','Un lenguaje de programación','single_choice','1000',
        '¿Qué es el few-shot prompting?','Proporcionar ejemplos en el prompt para guiar la respuesta','Usar el modelo sin instrucciones','Hacer el prompt muy largo','No incluir ningún ejemplo','single_choice','1000',
        '¿Qué logra el chain-of-thought prompting?','Acortar las respuestas del modelo','Reducir el uso de memoria','Guiar al modelo a razonar paso a paso antes de responder','Aumentar la velocidad de inferencia','single_choice','0010',
        '¿Qué controla el parámetro top-p en un LLM?','El tamaño máximo del contexto','La selección de tokens por probabilidad acumulada','El número de capas del modelo','La temperatura del sistema','single_choice','0100',
        '¿Cuáles son técnicas de prompting?','Zero-shot prompting','Few-shot prompting','Kernel trick','Chain-of-thought','multiple_choice','1101',
        '¿Qué factores afectan la calidad de la respuesta de un LLM?','La claridad del prompt','La temperatura configurada','El color de la interfaz','El contexto incluido en el prompt','multiple_choice','1101',
        '¿El zero-shot prompting incluye ejemplos en el prompt?','Verdadero','Falso','','','true_false','01',
        '¿La temperatura 0 produce respuestas más deterministas en un LLM?','Verdadero','Falso','','','true_false','10'
    ];

    q9 text[] := ARRAY[
        '¿Cuál es un desafío social de la IA?','Mayor velocidad de procesamiento','Sesgo algorítmico y discriminación','Menor consumo energético','Más almacenamiento de datos','single_choice','0100',
        '¿Qué busca la regulación de la IA?','Eliminar toda IA','Establecer marcos legales para gobernar el desarrollo y uso de la IA','Prohibir el machine learning','Solo regular hardware','single_choice','0100',
        '¿Qué es la singularidad tecnológica?','Un tipo de hardware cuántico','El punto hipotético donde la IA supera la inteligencia humana general','Un algoritmo de búsqueda','Una técnica de optimización','single_choice','0100',
        '¿Qué describe la colaboración humano-IA?','Reemplazar humanos con IA','Trabajar junto con sistemas de IA para potenciar capacidades humanas','Competir contra la IA','No interactuar con IA','single_choice','0100',
        '¿Qué impactos genera la IA en la sociedad?','Automatización de empleos rutinarios','Mejora en diagnósticos médicos','Eliminación total de sesgos','Personalización de servicios digitales','multiple_choice','1101',
        '¿Cuáles son ejemplos de agentes autónomos de IA?','Vehículos autónomos','Robots quirúrgicos','Hojas de cálculo manuales','Drones inteligentes','multiple_choice','1101',
        '¿La IA puede generar desinformación (deepfakes)?','Verdadero','Falso','','','true_false','10',
        '¿La regulación de IA es igual en todos los países del mundo?','Verdadero','Falso','','','true_false','01'
    ];

    questions text[][] := ARRAY[q1, q2, q3, q4, q5, q6, q7, q8, q9];

    i int; j int; k int;
    question_id uuid;
    option_id uuid;
    q_type text;
    q_mask text;
    opt_count int;
    base_index int;
    quiz_id uuid;

BEGIN
    FOR i IN 1..array_length(quiz_ids, 1) LOOP
        quiz_id := quiz_ids[i];

        FOR j IN 1..8 LOOP
            base_index := (j - 1) * 7 + 1;

            q_type := questions[i][base_index + 5];
            q_mask := questions[i][base_index + 6];
            opt_count := CASE WHEN q_type = 'true_false' THEN 2 ELSE 4 END;

            question_id := gen_random_uuid();

            INSERT INTO quizzing.question (
                "Id", "Content", "Status", "Type", "Deleted",
                "CreatedAt", "UpdatedAt", "DeletedAt",
                "ProcessingJobId", "Origin", "Justification"
            ) VALUES (
                question_id,
                questions[i][base_index],
                'verified',
                q_type::quizzing.question_type,
                FALSE,
                NOW() - INTERVAL '70 days' + (i * INTERVAL '5 days') + (j * INTERVAL '1 hour'),
                NOW(),
                NULL,
                'aaaaaaaa-0000-0000-0000-000000000001'::uuid,
                'manually_created',
                'Pregunta creada para demo'
            );

            FOR k IN 1..opt_count LOOP
                option_id := gen_random_uuid();
                INSERT INTO quizzing."option" (
                    "Id", "Description", "IsCorrect", "Position",
                    "Deleted", "CreatedAt", "UpdatedAt", "DeletedAt", "QuestionId"
                ) VALUES (
                    option_id,
                    questions[i][base_index + k],
                    substring(q_mask, k, 1) = '1',
                    k,
                    FALSE,
                    NOW() - INTERVAL '70 days' + (i * INTERVAL '5 days') + (j * INTERVAL '1 hour'),
                    NOW(),
                    NULL,
                    question_id
                );
            END LOOP;

            INSERT INTO quizzing.quiz_question (
                "Id", "Position", "ValueScore", "Deleted",
                "CreatedAt", "UpdatedAt", "DeletedAt", "QuizId", "QuestionId"
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
-- 7. ADDITIONAL QUIZZES (draft and archived for variety)
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
-- 8. ADDITIONAL MATCHES (expired and pending for variety)
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

-- ============================================================
-- 9. MATCH_ATTEMPTS
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
-- max.maximus: completed (x5)
(
    '50000000-0000-0000-0000-000000000001'::uuid,
    NOW() - INTERVAL '45 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '55 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '3 minutes',
    'max.maximus', 'completed', 100.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000001'::uuid
),
(
    '50000000-0000-0000-0000-000000000002'::uuid,
    NOW() - INTERVAL '35 days' + INTERVAL '7 minutes',
    NOW() - INTERVAL '35 days' + INTERVAL '57 minutes',
    NOW() - INTERVAL '35 days' + INTERVAL '4 minutes',
    'max.maximus', 'completed', 87.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000002'::uuid
),
(
    '50000000-0000-0000-0000-000000000003'::uuid,
    NOW() - INTERVAL '25 days' + INTERVAL '3 minutes',
    NOW() - INTERVAL '25 days' + INTERVAL '52 minutes',
    NOW() - INTERVAL '25 days' + INTERVAL '2 minutes',
    'max.maximus', 'completed', 75.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000003'::uuid
),
(
    '50000000-0000-0000-0000-000000000004'::uuid,
    NOW() - INTERVAL '7 days' + INTERVAL '10 minutes',
    NOW() - INTERVAL '7 days' + INTERVAL '23 minutes',
    NOW() - INTERVAL '7 days' + INTERVAL '8 minutes',
    'max.maximus', 'completed', 75.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000006'::uuid
),
(
    '50000000-0000-0000-0000-000000000005'::uuid,
    NOW() - INTERVAL '6 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '19 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '3 minutes',
    'max.maximus', 'completed', 62.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000007'::uuid
),
-- max.maximus: timeout (x2)
(
    '50000000-0000-0000-0000-000000000006'::uuid,
    NOW() - INTERVAL '4 days' + INTERVAL '2 minutes',
    NOW() - INTERVAL '4 days' + INTERVAL '17 minutes',
    NOW() - INTERVAL '4 days' + INTERVAL '1 minute',
    'max.maximus', 'timeout', 37.50,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000008'::uuid
),
(
    '50000000-0000-0000-0000-000000000013'::uuid,
    NOW() - INTERVAL '4 days' + INTERVAL '1 minute',
    NOW() - INTERVAL '4 days' + INTERVAL '61 minutes',
    NOW() - INTERVAL '4 days',
    'max.maximus', 'timeout', 25.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000004'::uuid
),
-- max.maximus: in_progress (x2)
(
    '50000000-0000-0000-0000-000000000007'::uuid,
    NOW() - INTERVAL '3 days' + INTERVAL '1 minute',
    NULL,
    NOW() - INTERVAL '3 days',
    'max.maximus', 'in_progress', 0.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000009'::uuid
),
(
    '50000000-0000-0000-0000-000000000014'::uuid,
    NOW() - INTERVAL '1 day' + INTERVAL '1 minute',
    NULL,
    NOW() - INTERVAL '1 day',
    'max.maximus', 'in_progress', 0.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000005'::uuid
),
-- student01: completed (x2)
(
    '50000000-0000-0000-0000-000000000011'::uuid,
    NOW() - INTERVAL '90 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '90 days' + INTERVAL '58 minutes',
    NOW() - INTERVAL '90 days' + INTERVAL '3 minutes',
    'student01', 'completed', 75.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000010'::uuid
),
(
    '50000000-0000-0000-0000-000000000015'::uuid,
    NOW() - INTERVAL '89 days' + INTERVAL '10 minutes',
    NOW() - INTERVAL '89 days' + INTERVAL '55 minutes',
    NOW() - INTERVAL '89 days' + INTERVAL '8 minutes',
    'student01', 'completed', 50.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000011'::uuid
),
-- student01: timeout (x2)
(
    '50000000-0000-0000-0000-000000000012'::uuid,
    NOW() - INTERVAL '60 days' + INTERVAL '2 minutes',
    NOW() - INTERVAL '60 days' + INTERVAL '16 minutes',
    NOW() - INTERVAL '60 days' + INTERVAL '1 minute',
    'student01', 'timeout', 25.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000011'::uuid
),
(
    '50000000-0000-0000-0000-000000000016'::uuid,
    NOW() - INTERVAL '6 days' + INTERVAL '3 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '18 minutes',
    NOW() - INTERVAL '6 days' + INTERVAL '1 minute',
    'student01', 'timeout', 12.50,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000007'::uuid
),
-- student01: in_progress (x2)
(
    '50000000-0000-0000-0000-000000000008'::uuid,
    NOW() - INTERVAL '5 days' + INTERVAL '2 minutes',
    NULL,
    NOW() - INTERVAL '5 days',
    'student01', 'in_progress', 0.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000004'::uuid
),
(
    '50000000-0000-0000-0000-000000000017'::uuid,
    NOW() - INTERVAL '2 days' + INTERVAL '2 minutes',
    NULL,
    NOW() - INTERVAL '2 days',
    'student01', 'in_progress', 0.00,
    '33333333-3333-3333-3333-333333333333'::uuid,
    '40000000-0000-0000-0000-000000000003'::uuid
),
-- carlos.ruiz (max.maximus): completed
(
    '50000000-0000-0000-0000-000000000018'::uuid,
    NOW() - INTERVAL '45 days' + INTERVAL '10 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '50 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '8 minutes',
    'carlos.ruiz', 'completed', 80.00,
    '37976960-c868-45d4-b3c2-4967cb46f4b0'::uuid,
    '40000000-0000-0000-0000-000000000001'::uuid
),
-- pedro.villca: completed
(
    '50000000-0000-0000-0000-000000000019'::uuid,
    NOW() - INTERVAL '45 days' + INTERVAL '12 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '52 minutes',
    NOW() - INTERVAL '45 days' + INTERVAL '10 minutes',
    'pedro.villca', 'completed', 80.00,
    'd7f65af9-57a1-4b5b-9685-815770faea7d'::uuid,
    '40000000-0000-0000-0000-000000000001'::uuid
),
-- pedro.villca: segundo intento completed
(
    '50000000-0000-0000-0000-000000000020'::uuid,
    NOW() - INTERVAL '44 days' + INTERVAL '5 minutes',
    NOW() - INTERVAL '44 days' + INTERVAL '45 minutes',
    NOW() - INTERVAL '44 days' + INTERVAL '3 minutes',
    'pedro.villca', 'completed', 40.00,
    'd7f65af9-57a1-4b5b-9685-815770faea7d'::uuid,
    '40000000-0000-0000-0000-000000000001'::uuid
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
