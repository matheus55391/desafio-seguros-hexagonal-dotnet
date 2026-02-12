-- Criar bancos de dados
-- Note: CREATE DATABASE não pode ser executado dentro de transações ou blocos
-- O PostgreSQL vai ignorar se o banco já existir ao usar --if-not-exists não está disponível em todas versões
-- Então simplesmente criamos os bancos. Se já existirem, o script vai falhar mas o container já terá os bancos.

SELECT 'CREATE DATABASE proposta_db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'proposta_db')\gexec
SELECT 'CREATE DATABASE contratacao_db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'contratacao_db')\gexec
