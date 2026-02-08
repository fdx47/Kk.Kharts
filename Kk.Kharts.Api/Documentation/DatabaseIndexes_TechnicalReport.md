# KropKontrol IoT - Relatório Técnico de Índices de Base de Dados

**Versão:** 1.0  
**Data:** 2026-01-19  
**Autor:** Senior Architect Analysis  
**Sistema:** Kk.Kharts.Api - SQL Server 2022

---

## 1. Resumo Executivo

Este documento apresenta a análise técnica completa do sistema **KropKontrol IoT** e a implementação de índices de base de dados otimizados para SQL Server 2022. A decisão de implementar índices baseia-se na análise profunda de:

- 15 Controllers
- 11 Repositories  
- 20+ Services (incluindo 2 BackgroundServices críticos)
- 23 Entity Configurations
- Padrões de query LINQ e SQL gerado pelo Entity Framework Core

**Decisão: ÍNDICES SÃO NECESSÁRIOS E FORAM IMPLEMENTADOS**

---

## 2. Análise do Sistema

### 2.1 Arquitectura de Dados

| Tabela | Tipo | Volume Estimado | Crescimento |
|--------|------|-----------------|-------------|
| `wets_150` | Telemetria IoT | Alto (milhões) | ~1440 registos/dia/dispositivo |
| `em300_th` | Telemetria IoT | Alto (milhões) | ~1440 registos/dia/dispositivo |
| `uc502_modbus` | Telemetria IoT | Médio-Alto | ~1440 registos/dia/dispositivo |
| `wet150_multisensor_*` | Telemetria IoT | Médio | ~1440 registos/dia/dispositivo |
| `devices` | Configuração | Baixo (~100-1000) | Lento |
| `alarm_rules` | Configuração | Baixo (~500) | Lento |
| `users` | Transacional | Baixo (~100) | Lento |
| `device_status_notifications` | Transacional | Médio | ~24/dia/dispositivo offline |

### 2.2 Padrões de Acesso Críticos Identificados

#### **Alta Frequência (a cada payload IoT - segundos)**
```csharp
// DeviceRepository.cs - Lookup por DevEui
await _dbContext.Devices.FirstOrDefaultAsync(d => d.DevEui == devEui);

// DeviceRepository.cs - Update de status
UPDATE devices SET battery=@p0, last_send_at=@p1, last_seen_at=@p2 WHERE dev_eui=@p3
```

#### **Média Frequência (a cada 15 minutos - AlarmEvaluatorService)**
```csharp
// AlarmEvaluatorService.cs - Última leitura por dispositivo
await db.Uc502Wet150s
    .Where(e => e.DeviceId == rule.DeviceId)
    .OrderByDescending(e => e.Timestamp)
    .FirstOrDefaultAsync();

await db.Em300ths
    .Where(e => e.DeviceId == rule.DeviceId)
    .OrderByDescending(e => e.Timestamp)
    .FirstOrDefaultAsync();
```

#### **Média Frequência (a cada hora - DeviceMonitorService)**
```csharp
// DeviceMonitorService.cs - Dispositivos inativos
await dbContext.Devices
    .Include(d => d.Company)
    .Where(d => d.ActiveInKropKontrol && d.LastSeenAt < cutoff && !d.HasCommunicationAlarm)
    .ToListAsync();
```

#### **Sob Demanda (consultas de utilizador)**
```csharp
// Uc502Service.cs - Range temporal por dispositivo
data.Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
    .OrderBy(x => x.Timestamp)

// MiniAppController.cs - Última leitura por lista de DevEuis
await db.Em300ths
    .Where(x => devEuis.Contains(x.DevEui))
    .GroupBy(x => x.DevEui)
    .Select(g => g.OrderByDescending(x => x.Timestamp).First())
```

---

## 3. Índices Implementados

### 3.1 Tabela: `devices`

| Índice | Tipo | Colunas | Include | Justificação |
|--------|------|---------|---------|--------------|
| `IX_devices_dev_eui` | Unique NC | `dev_eui` | - | Lookup por DevEui em cada payload IoT |
| `IX_devices_societe_id` | NC | `societe_id` | `dev_eui`, `name`, `description`, `battery`, `last_seen_at`, `active_in_kropkontrol` | Listagem de dispositivos por empresa |
| `IX_devices_monitoring` | NC Composto | `active_in_kropkontrol`, `last_seen_at`, `has_comm_alarm` | `dev_eui`, `name`, `description`, `societe_id` | DeviceMonitorService (execução horária) |

### 3.2 Tabelas de Telemetria

| Tabela | Índice | Colunas | Justificação |
|--------|--------|---------|--------------|
| `wets_150` | `IX_wets_150_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries de gráficos com range temporal |
| `wets_150` | `IX_wets_150_device_id` | `device_id`, `timestamp DESC` | AlarmEvaluatorService - última leitura |
| `em300_th` | `IX_em300_th_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries de gráficos com range temporal |
| `em300_th` | `IX_em300_th_device_id` | `device_id`, `timestamp DESC` | AlarmEvaluatorService - última leitura |
| `uc502_modbus` | `IX_uc502_modbus_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries de gráficos com range temporal |
| `wet150_multisensor_2` | `IX_wet150_multisensor_2_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries MultiSensor |
| `wet150_multisensor_3` | `IX_wet150_multisensor_3_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries MultiSensor |
| `wet150_multisensor_4` | `IX_wet150_multisensor_4_dev_eui_timestamp` | `dev_eui`, `timestamp DESC` | Queries MultiSensor |

### 3.3 Outras Tabelas

| Tabela | Índice | Colunas | Include |
|--------|--------|---------|---------|
| `alarm_rules` | `IX_alarm_rules_enabled_device` | `enabled`, `device_id` | `dev_eui`, `property_name`, `low_value`, `high_value`, `hysteresis`, `is_alarm_active` |
| `alarm_rules` | `IX_alarm_rules_dev_eui` | `dev_eui` | `device_id`, `enabled`, `property_name` |
| `users` | `IX_users_email` | `email` (Unique) | - |
| `users` | `IX_users_company_id` | `company_id` | `nom`, `email`, `role`, `access_level` |
| `companies` | `IX_companies_parent` | `parent_company_id` | `name` |
| `technician_devices` | `IX_technician_devices_technician` | `technician_id` | `device_id` |
| `technicians` | `IX_technicians_user_id` | `user_id` | - |
| `dashboards` | `IX_dashboards_user_created` | `user_id`, `created_at DESC` | - |
| `wet150_sdi12_metadata` | `IX_wet150_sdi12_metadata_dev_eui` | `dev_eui` | - |

---

## 4. Ganhos Esperados

### 4.1 Latência

| Operação | Antes (estimado) | Depois (estimado) | Melhoria |
|----------|------------------|-------------------|----------|
| Lookup device por DevEui | 50-200ms (table scan) | 1-5ms (index seek) | **95-99%** |
| Última leitura telemetria | 500ms-2s (scan millions) | 5-20ms (index seek) | **97-99%** |
| Listagem dispositivos/empresa | 100-500ms | 10-50ms | **90%** |
| Query range temporal (7 dias) | 2-10s | 100-500ms | **95%** |

### 4.2 Throughput

| Métrica | Impacto Esperado |
|---------|------------------|
| Payloads IoT processados/segundo | +200-400% |
| Requests API concorrentes | +150-300% |
| Background services (AlarmEvaluator) | Ciclo de 15min → <1min real |

### 4.3 Uso de Recursos

| Recurso | Impacto |
|---------|---------|
| CPU | -40-60% em operações de leitura |
| Logical Reads | -90-99% nas queries indexadas |
| Physical Reads | -80-95% (dados em cache mais eficientemente) |
| Lock contention | -50% (queries mais rápidas = locks mais curtos) |

---

## 5. Custos e Trade-offs

### 5.1 Impacto em Escritas

| Tabela | Índices | Overhead INSERT | Overhead UPDATE |
|--------|---------|-----------------|-----------------|
| `wets_150` | 2 | +15-25% | +10-15% |
| `em300_th` | 2 | +15-25% | +10-15% |
| `devices` | 3 | +20-30% | +15-20% |
| `alarm_rules` | 2 | +10-15% | +10-15% |

**Mitigação:** O overhead de escrita é aceitável dado que:
- Telemetria: ~1 INSERT/minuto/dispositivo vs. múltiplas leituras/segundo
- A melhoria de 95%+ nas leituras compensa o overhead de 15-25% nas escritas

### 5.2 Espaço em Disco

| Categoria | Estimativa |
|-----------|------------|
| Índices tabelas telemetria | +30-50% do tamanho das tabelas |
| Índices tabelas configuração | +5-10MB total |
| **Total estimado** | +40-60% do tamanho atual da BD |

### 5.3 Custos de Manutenção

| Tarefa | Frequência | Duração Estimada |
|--------|------------|------------------|
| Reorganize (fragmentação <30%) | Semanal | 5-15 min |
| Rebuild (fragmentação >30%) | Mensal | 15-60 min (ONLINE) |
| Update Statistics | Semanal | 2-5 min |

---

## 6. Impacto na Escalabilidade

### 6.1 Crescimento da Base de Dados

| Cenário | Sem Índices | Com Índices |
|---------|-------------|-------------|
| 10 dispositivos, 1 ano | Queries <1s | Queries <100ms |
| 100 dispositivos, 1 ano | Queries 5-30s | Queries <500ms |
| 1000 dispositivos, 1 ano | Queries timeout | Queries <2s |

### 6.2 Concorrência

| Métrica | Impacto |
|---------|---------|
| Conexões simultâneas suportadas | +100-200% |
| Deadlock probability | -70% (transações mais curtas) |
| Background service stability | Muito melhorada |

### 6.3 Picos de Carga IoT

Os índices são especialmente críticos em cenários típicos de IoT:

- **Startup massivo:** Todos os dispositivos a enviar após falha de rede
- **Consultas de dashboard:** Múltiplos utilizadores a visualizar gráficos
- **AlarmEvaluatorService:** 15 em 15 minutos, avalia TODAS as regras activas

---

## 7. Boas Práticas de Manutenção

### 7.1 Script de Manutenção Semanal

```sql
-- Executar em janela de manutenção (domingo 03:00)
DECLARE @Schema NVARCHAR(50) = 'kropkharts';

-- 1. Reorganizar índices com fragmentação 10-30%
SELECT 
    'ALTER INDEX [' + i.name + '] ON [' + s.name + '].[' + t.name + '] REORGANIZE;' AS cmd
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
JOIN sys.tables t ON i.object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE ps.avg_fragmentation_in_percent BETWEEN 10 AND 30
  AND i.name IS NOT NULL;

-- 2. Rebuild índices com fragmentação >30%
SELECT 
    'ALTER INDEX [' + i.name + '] ON [' + s.name + '].[' + t.name + '] REBUILD WITH (ONLINE = ON);' AS cmd
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
JOIN sys.tables t ON i.object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE ps.avg_fragmentation_in_percent > 30
  AND i.name IS NOT NULL;

-- 3. Atualizar estatísticas
EXEC sp_updatestats;
```

### 7.2 Monitorização de Índices

```sql
-- Índices não utilizados (candidatos a remoção após 30 dias)
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    ius.user_seeks,
    ius.user_scans,
    ius.user_lookups,
    ius.user_updates
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats ius 
    ON i.object_id = ius.object_id AND i.index_id = ius.index_id
WHERE OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
  AND i.name IS NOT NULL
  AND (ius.user_seeks + ius.user_scans + ius.user_lookups) = 0
ORDER BY ius.user_updates DESC;

-- Missing indexes sugeridos pelo SQL Server
SELECT 
    migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) AS improvement_measure,
    mid.statement AS table_name,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns
FROM sys.dm_db_missing_index_groups mig
JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
ORDER BY improvement_measure DESC;
```

### 7.3 Jobs Automáticos Recomendados

| Job | Schedule | Descrição |
|-----|----------|-----------|
| `KK_IndexMaintenance_Weekly` | Domingo 03:00 | Reorganize/Rebuild conforme fragmentação |
| `KK_Statistics_Weekly` | Domingo 04:00 | sp_updatestats |
| `KK_IndexUsage_Monthly` | 1º dia mês 06:00 | Relatório de índices não utilizados |

---

## 8. Riscos Operacionais

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Execução durante pico | Baixa | Alto | ONLINE=ON em todos os índices |
| Espaço insuficiente | Média | Alto | Monitorar espaço antes de rebuild |
| Lock escalation | Baixa | Médio | Rebuild em horário de baixa carga |
| Timeout em criação | Baixa | Baixo | Script idempotente (IF NOT EXISTS) |

---

## 9. Instruções de Execução

### 9.1 Pré-requisitos

1. Backup completo da base de dados
2. Verificar espaço em disco (mínimo +50% do tamanho atual)
3. Executar em janela de manutenção ou usar `ONLINE=ON`

### 9.2 Execução

```powershell
# Via sqlcmd
sqlcmd -S servidor -d KropKontrol -i DatabaseIndexes_KropKontrol.sql -o index_creation.log

# Via SSMS
# Abrir ficheiro e executar (F5)
```

### 9.3 Validação Pós-Execução

```sql
-- Verificar índices criados
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc,
    i.is_unique
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE 'IX_%'
ORDER BY t.name, i.name;
```

---

## 10. Conclusão

A implementação destes índices é **tecnicamente necessária** e **altamente recomendada** para:

1. **Performance:** Redução de 95%+ na latência das queries críticas
2. **Escalabilidade:** Suporte a crescimento de 10x no volume de dados
3. **Estabilidade:** Background services a funcionar dentro dos SLAs
4. **Experiência do Utilizador:** Dashboards e gráficos responsivos

O overhead de escritas (+15-25%) e espaço (+40-60%) é um trade-off aceitável face aos ganhos significativos em leitura, que representam >90% das operações num sistema IoT de monitorização.

---

**Aprovado por:** _________________________  
**Data:** _________________________  
**Ambiente:** ☐ Desenvolvimento ☐ Staging ☐ Produção
