-- Script de Exploração do Banco MiniLMS
-- Execute essas queries no seu SGBD após conectar

-- ===========================================
-- 1. VERIFICAR ESTRUTURA DAS TABELAS
-- ===========================================

-- Listar todas as tabelas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- ===========================================
-- 2. ESTRUTURA DA TABELA COURSES
-- ===========================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Courses'
ORDER BY ORDINAL_POSITION;

-- ===========================================
-- 3. ESTRUTURA DA TABELA USERS
-- ===========================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- ===========================================
-- 4. ESTRUTURA DA TABELA ENROLLMENTS
-- ===========================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Enrollments'
ORDER BY ORDINAL_POSITION;

-- ===========================================
-- 5. ESTRUTURA DA TABELA IMPORTJOBS
-- ===========================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ImportJobs'
ORDER BY ORDINAL_POSITION;

-- ===========================================
-- 6. CONSULTAR DADOS EXISTENTES
-- ===========================================

-- Ver todos os cursos
SELECT * FROM Courses;

-- Ver todos os usuários
SELECT * FROM Users;

-- Ver todas as matrículas
SELECT * FROM Enrollments;

-- Ver histórico de importações
SELECT * FROM ImportJobs;

-- ===========================================
-- 7. CONSULTAS ÚTEIS PARA ANÁLISE
-- ===========================================

-- Contagem de registros por tabela
SELECT 'Courses' as Tabela, COUNT(*) as Quantidade FROM Courses
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'Enrollments', COUNT(*) FROM Enrollments
UNION ALL
SELECT 'ImportJobs', COUNT(*) FROM ImportJobs;

-- Cursos ativos vs inativos
SELECT 
    IsActive,
    COUNT(*) as Quantidade
FROM Courses 
GROUP BY IsActive;

-- ===========================================
-- 8. CONSULTAS RELACIONAIS
-- ===========================================

-- Matrículas com dados do usuário e curso
SELECT 
    u.Name as NomeUsuario,
    u.Email,
    c.Title as NomeCurso,
    c.Category as Categoria,
    e.EnrolledAt as DataMatricula
FROM Enrollments e
INNER JOIN Users u ON e.UserId = u.Id
INNER JOIN Courses c ON e.CourseId = c.Id
ORDER BY e.EnrolledAt DESC;

-- Usuários por curso
SELECT 
    c.Title as Curso,
    COUNT(e.UserId) as TotalMatriculas
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
GROUP BY c.Id, c.Title
ORDER BY TotalMatriculas DESC;

-- ===========================================
-- 9. DADOS DE EXEMPLO PARA TESTE
-- ===========================================

-- Inserir usuário de teste (descomente para usar)
/*
INSERT INTO Users (Name, Email, CreatedAt, UpdatedAt)
VALUES ('João Silva', 'joao.silva@email.com', GETDATE(), GETDATE());

INSERT INTO Users (Name, Email, CreatedAt, UpdatedAt)
VALUES ('Maria Santos', 'maria.santos@email.com', GETDATE(), GETDATE());
*/

-- Matricular usuário em curso (descomente para usar)
/*
INSERT INTO Enrollments (UserId, CourseId, EnrolledAt)
VALUES (1, 1, GETDATE());

INSERT INTO Enrollments (UserId, CourseId, EnrolledAt)
VALUES (2, 1, GETDATE());

INSERT INTO Enrollments (UserId, CourseId, EnrolledAt)
VALUES (1, 2, GETDATE());
*/
