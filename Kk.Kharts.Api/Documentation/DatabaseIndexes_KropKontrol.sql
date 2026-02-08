-- ============================================================================
-- KropKontrol IoT - Database Indexes Implementation Script
-- SQL Server 2022 Compatible
-- Author: Senior Architect Analysis
-- Date: 2026-01-19
-- Schema: kropkharts (ajustar conforme appsettings)
-- ============================================================================
-- IMPORTANT: Execute em ambiente de manutenção ou com ONLINE=ON para produção
-- ============================================================================

USE [KropKontrol]; -- Ajustar nome da base de dados
GO

-- Define o schema (ajustar conforme configuração)
DECLARE @Schema NVARCHAR(50) = 'kropkharts';

-- ============================================================================
-- 1. TABELA: devices
-- Padrões de acesso identificados:
--   - Lookup por dev_eui (muito frequente - cada payload IoT)
--   - Filtro por societe_id (listagem por empresa)
--   - Filtro por active_in_kropkontrol + last_seen_at (DeviceMonitorService)
--   - Filtro por has_comm_alarm (alertas de comunicação)
-- ============================================================================

-- IX_devices_dev_eui: Índice único para lookups rápidos por DevEui
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_devices_dev_eui' AND object_id = OBJECT_ID('kropkharts.devices'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_devices_dev_eui]
    ON [kropkharts].[devices] ([dev_eui])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_devices_dev_eui';
END
GO

-- IX_devices_societe_id: Filtro por empresa (listagem de dispositivos)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_devices_societe_id' AND object_id = OBJECT_ID('kropkharts.devices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_devices_societe_id]
    ON [kropkharts].[devices] ([societe_id])
    INCLUDE ([dev_eui], [name], [description], [battery], [last_seen_at], [active_in_kropkontrol])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_devices_societe_id';
END
GO

-- IX_devices_monitoring: Índice composto para DeviceMonitorService (executado a cada hora)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_devices_monitoring' AND object_id = OBJECT_ID('kropkharts.devices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_devices_monitoring]
    ON [kropkharts].[devices] ([active_in_kropkontrol], [last_seen_at], [has_comm_alarm])
    INCLUDE ([dev_eui], [name], [description], [societe_id])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_devices_monitoring';
END
GO

-- ============================================================================
-- 2. TABELA: wets_150 (Telemetria UC502 WET150 - tabela de alto volume)
-- Chave primária: (timestamp, dev_eui) - Clustered
-- Padrões de acesso:
--   - Filtro por dev_eui + range de timestamp (consultas de gráficos)
--   - Lookup por device_id (AlarmEvaluatorService)
--   - OrderByDescending(timestamp) para última leitura
-- ============================================================================

-- IX_wets_150_dev_eui_timestamp: Consultas por dispositivo com range temporal
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wets_150_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.wets_150'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wets_150_dev_eui_timestamp]
    ON [kropkharts].[wets_150] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_wets_150_dev_eui_timestamp';
END
GO

-- IX_wets_150_device_id: Lookup por DeviceId (AlarmEvaluatorService - última leitura)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wets_150_device_id' AND object_id = OBJECT_ID('kropkharts.wets_150'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wets_150_device_id]
    ON [kropkharts].[wets_150] ([device_id], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_wets_150_device_id';
END
GO

-- ============================================================================
-- 3. TABELA: em300_th (Telemetria Temperatura/Humidade - alto volume)
-- Chave primária: (timestamp, dev_eui) - Clustered
-- Padrões de acesso similares a wets_150
-- ============================================================================

-- IX_em300_th_dev_eui_timestamp: Consultas por dispositivo com range temporal
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_em300_th_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.em300_th'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_em300_th_dev_eui_timestamp]
    ON [kropkharts].[em300_th] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_em300_th_dev_eui_timestamp';
END
GO

-- IX_em300_th_device_id: Lookup por DeviceId (AlarmEvaluatorService)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_em300_th_device_id' AND object_id = OBJECT_ID('kropkharts.em300_th'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_em300_th_device_id]
    ON [kropkharts].[em300_th] ([device_id], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_em300_th_device_id';
END
GO

-- ============================================================================
-- 4. TABELA: uc502_modbus (Telemetria Modbus - alto volume)
-- Chave primária: (timestamp, dev_eui) - Clustered
-- ============================================================================

-- IX_uc502_modbus_dev_eui_timestamp: Consultas por dispositivo
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_uc502_modbus_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.uc502_modbus'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_uc502_modbus_dev_eui_timestamp]
    ON [kropkharts].[uc502_modbus] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_uc502_modbus_dev_eui_timestamp';
END
GO

-- ============================================================================
-- 5. TABELAS: wet150_multisensor_2, wet150_multisensor_3, wet150_multisensor_4
-- Chave primária: (timestamp, dev_eui) - Clustered
-- ============================================================================

-- wet150_multisensor_2
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wet150_multisensor_2_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.wet150_multisensor_2'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wet150_multisensor_2_dev_eui_timestamp]
    ON [kropkharts].[wet150_multisensor_2] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_wet150_multisensor_2_dev_eui_timestamp';
END
GO

-- wet150_multisensor_3
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wet150_multisensor_3_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.wet150_multisensor_3'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wet150_multisensor_3_dev_eui_timestamp]
    ON [kropkharts].[wet150_multisensor_3] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_wet150_multisensor_3_dev_eui_timestamp';
END
GO

-- wet150_multisensor_4
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wet150_multisensor_4_dev_eui_timestamp' AND object_id = OBJECT_ID('kropkharts.wet150_multisensor_4'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wet150_multisensor_4_dev_eui_timestamp]
    ON [kropkharts].[wet150_multisensor_4] ([dev_eui], [timestamp] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 85);
    PRINT 'Created: IX_wet150_multisensor_4_dev_eui_timestamp';
END
GO

-- ============================================================================
-- 6. TABELA: alarm_rules
-- Padrões de acesso:
--   - Filtro por enabled (regras ativas)
--   - Filtro por dev_eui (regras por dispositivo)
--   - Filtro por device_id (AlarmEvaluatorService)
-- ============================================================================

-- IX_alarm_rules_enabled_device: Regras ativas por dispositivo
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_alarm_rules_enabled_device' AND object_id = OBJECT_ID('kropkharts.alarm_rules'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_alarm_rules_enabled_device]
    ON [kropkharts].[alarm_rules] ([enabled], [device_id])
    INCLUDE ([dev_eui], [property_name], [low_value], [high_value], [hysteresis], [is_alarm_active])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_alarm_rules_enabled_device';
END
GO

-- IX_alarm_rules_dev_eui: Lookup por DevEui
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_alarm_rules_dev_eui' AND object_id = OBJECT_ID('kropkharts.alarm_rules'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_alarm_rules_dev_eui]
    ON [kropkharts].[alarm_rules] ([dev_eui])
    INCLUDE ([device_id], [enabled], [property_name])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_alarm_rules_dev_eui';
END
GO

-- ============================================================================
-- 7. TABELA: users
-- Padrões de acesso:
--   - Login por email (muito frequente)
--   - Lookup por company_id
--   - telegram_user_id já tem índice único filtrado
-- ============================================================================

-- IX_users_email: Login (índice único)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_users_email' AND object_id = OBJECT_ID('kropkharts.users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_users_email]
    ON [kropkharts].[users] ([email])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_users_email';
END
GO

-- IX_users_company_id: Lookup por empresa
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_users_company_id' AND object_id = OBJECT_ID('kropkharts.users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_users_company_id]
    ON [kropkharts].[users] ([company_id])
    INCLUDE ([nom], [email], [role], [access_level])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_users_company_id';
END
GO

-- ============================================================================
-- 8. TABELA: companies
-- Padrões de acesso:
--   - Lookup por parent_company_id (filiais)
-- ============================================================================

-- IX_companies_parent: Lookup de filiais
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_companies_parent' AND object_id = OBJECT_ID('kropkharts.companies'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_companies_parent]
    ON [kropkharts].[companies] ([parent_company_id])
    INCLUDE ([name])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_companies_parent';
END
GO

-- ============================================================================
-- 9. TABELA: technician_devices
-- Padrões de acesso:
--   - Filtro por technician_id (dispositivos do técnico)
-- ============================================================================

-- IX_technician_devices_technician: Dispositivos por técnico
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_technician_devices_technician' AND object_id = OBJECT_ID('kropkharts.technician_devices'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_technician_devices_technician]
    ON [kropkharts].[technician_devices] ([technician_id])
    INCLUDE ([device_id])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_technician_devices_technician';
END
GO

-- ============================================================================
-- 10. TABELA: technicians
-- Padrões de acesso:
--   - Lookup por user_id
-- ============================================================================

-- IX_technicians_user_id: Lookup por UserId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_technicians_user_id' AND object_id = OBJECT_ID('kropkharts.technicians'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_technicians_user_id]
    ON [kropkharts].[technicians] ([user_id])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_technicians_user_id';
END
GO

-- ============================================================================
-- 11. TABELA: dashboards
-- Padrões de acesso:
--   - Filtro por user_id + OrderByDescending(created_at)
-- ============================================================================

-- IX_dashboards_user_created: Dashboard mais recente do utilizador
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_dashboards_user_created' AND object_id = OBJECT_ID('kropkharts.dashboards'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_dashboards_user_created]
    ON [kropkharts].[dashboards] ([user_id], [created_at] DESC)
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_dashboards_user_created';
END
GO

-- ============================================================================
-- 12. TABELA: wet150_sdi12_metadata
-- Padrões de acesso:
--   - Filtro por dev_eui
--   - Filtro por dev_eui + sdi12_index
-- ============================================================================

-- IX_wet150_sdi12_metadata_dev_eui: Lookup por DevEui
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_wet150_sdi12_metadata_dev_eui' AND object_id = OBJECT_ID('kropkharts.wet150_sdi12_metadata'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_wet150_sdi12_metadata_dev_eui]
    ON [kropkharts].[wet150_sdi12_metadata] ([dev_eui])
    WITH (ONLINE = ON, FILLFACTOR = 90);
    PRINT 'Created: IX_wet150_sdi12_metadata_dev_eui';
END
GO

-- ============================================================================
-- ATUALIZAÇÃO DE ESTATÍSTICAS
-- Executar após criação dos índices
-- ============================================================================

PRINT 'Updating statistics for all tables...';
EXEC sp_updatestats;
PRINT 'Statistics updated.';
GO

PRINT '============================================================================';
PRINT 'Index creation completed successfully.';
PRINT 'Next steps:';
PRINT '  1. Monitor index usage with sys.dm_db_index_usage_stats';
PRINT '  2. Schedule weekly index maintenance (rebuild/reorganize)';
PRINT '  3. Update statistics weekly';
PRINT '============================================================================';
GO
