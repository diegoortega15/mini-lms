# Guia de Conexão com o Banco de Dados

## Dados de Conexão

- **Servidor:** `localhost,1433` ou `localhost`
- **Autenticação:** SQL Server Authentication
- **Usuário:** `sa`
- **Senha:** `MiniLMS123!`
- **Database:** `MiniLMS`

## DBeaver

1. Abra o DBeaver
2. Clique em "New Database Connection"
3. Selecione "SQL Server"
4. Preencha:
   - Server Host: `localhost`
   - Port: `1433`
   - Database: `MiniLMS`
   - Username: `sa`
   - Password: `MiniLMS123!`
5. Teste a conexão e clique em "OK"

## Queries Úteis

```sql
-- Ver todas as tabelas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Ver dados dos cursos
SELECT * FROM Courses;

-- Ver dados das matrículas
SELECT * FROM Enrollments;

-- Ver dados dos usuários
SELECT * FROM Users;

-- Ver jobs de importação
SELECT * FROM ImportJobs;
```

## Estrutura do Banco

O banco de dados `MiniLMS` contém as seguintes tabelas:

- **Courses** - Cursos disponíveis
- **Users** - Usuários do sistema
- **Enrollments** - Matrículas dos usuários nos cursos
- **ImportJobs** - Histórico de importações via CSV
