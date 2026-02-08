-- =============================================
-- Script: Criar tabela cache_versions
-- Descrição: Sistema de invalidação de cache com versioning
-- Data: 2026-01-03
-- =============================================

-- Verificar se a tabela já existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[kropKharts].[cache_versions]') AND type in (N'U'))
BEGIN
    -- Criar tabela cache_versions
    CREATE TABLE [kropKharts].[cache_versions] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [local_storage_version] INT NOT NULL DEFAULT 1,
        [indexed_db_version] INT NOT NULL DEFAULT 1,
        [cache_version] INT NOT NULL DEFAULT 1,
        [updated_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [updated_by] NVARCHAR(100) NULL
    );

    PRINT 'Tabela [kropKharts].[cache_versions] criada com sucesso.';

    -- Criar índice para performance
    CREATE INDEX IX_cache_versions_updated_at ON [kropKharts].[cache_versions]([updated_at] DESC);
    PRINT 'Índice IX_cache_versions_updated_at criado com sucesso.';

    -- Inserir registro inicial
    INSERT INTO [kropKharts].[cache_versions] 
        ([local_storage_version], [indexed_db_version], [cache_version], [updated_at], [updated_by])
    VALUES 
        (1, 1, 1, GETUTCDATE(), 'System');

    PRINT 'Registro inicial inserido com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela [kropKharts].[cache_versions] já existe. Nenhuma ação necessária.';
END
GO
