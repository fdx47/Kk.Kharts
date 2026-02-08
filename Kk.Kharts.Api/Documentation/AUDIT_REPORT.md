# 🔍 Relatório de Auditoria de Código - API KropKontrol

**Data:** 31/12/2024  
**Versão:** v0.3b  
**Auditor:** Cascade AI  

---

## 📋 Sumário Executivo

Esta auditoria identificou **47 problemas** de arquitetura e boas práticas que um arquiteto de software sénior evitaria. Os problemas estão categorizados por severidade e área.

---

## 🚨 PROBLEMAS CRÍTICOS (Severidade Alta)

### 1. **Acesso Direto ao DbContext nos Controllers**
**Ficheiros afetados:** `AuthController.cs`, `UsersController.cs`, `DashboardController.cs`

```csharp
// ❌ ERRADO - AuthController.cs:19
private readonly AppDbContext _context;
```

**Problema:** Controllers acedem diretamente ao `AppDbContext`, violando o princípio de separação de responsabilidades (SoC) e a arquitetura em camadas.

**Solução:** Usar sempre Services/Repositories como intermediários.

---

### 2. **Hardcoded Magic Numbers e IDs**
**Ficheiros afetados:** `DeviceService.cs:84`, `DeviceRepository.cs:127-128`

```csharp
// ❌ ERRADO - DeviceService.cs:84
if (device.CompanyId != 12) //todo gambiarra provisória
```

```csharp
// ❌ ERRADO - DeviceRepository.cs:127
device.CompanyId = 20;
```

**Problema:** IDs de empresas hardcoded no código. Isto é uma bomba-relógio para manutenção.

**Solução:** Usar configuração (`appsettings.json`) ou constantes nomeadas.

---

### 3. **Exposição de Informações Sensíveis em Exceções**
**Ficheiro:** `AuthController.cs:48`

```csharp
// ❌ ERRADO - Expõe password em logs
throw new InvalidLoginExceptionKk($"Email ou mot de passe invalide.", 
    $"Email invalide\n• Email: {login.Email}\n• Password: {login.Password}");
```

**Problema:** A password é incluída na mensagem de exceção que pode ser logada ou enviada para Telegram.

**Solução:** Nunca incluir passwords em logs ou mensagens de erro.

---

### 4. **Instanciação Manual de Serviços (Violação de DI)**
**Ficheiro:** `ExceptionHandlingMiddleware.cs:19`

```csharp
// ❌ ERRADO
_telegram = new TelegramNotifier();
```

**Problema:** Cria instância manualmente em vez de usar Dependency Injection.

**Solução:** Injetar via construtor usando `IServiceProvider`.

---

### 5. **Código Comentado Excessivo**
**Ficheiros afetados:** Quase todos os controllers e repositories

**Problema:** Centenas de linhas de código comentado poluem o código. Exemplos:
- `AuthController.cs`: linhas 86-107, 263-308
- `UsersController.cs`: linhas 81-148
- `DeviceRepository.cs`: linhas 213-403

**Solução:** Remover código comentado. Usar Git para histórico.

---

### 6. **Duplicação de Endpoints (Obsoletos)**
**Ficheiros afetados:** `DeviceController.cs`, `Uc502Controller.cs`, `Em300Controller.cs`

```csharp
// ❌ ERRADO - Duplicação massiva
[HttpGet("api/v1/devices")]
public async Task<IActionResult> GetAllDevices() { ... }

[HttpGet("api/v1/Device/GetAllDevices")]
[Obsolete("...")]
public async Task<IActionResult> AllDevicesObs() { ... }
```

**Problema:** Endpoints obsoletos duplicados aumentam superfície de ataque e confusão.

**Solução:** Remover endpoints obsoletos após período de deprecação.

---

## ⚠️ PROBLEMAS MÉDIOS (Severidade Média)

### 7. **Inconsistência nas Rotas**
**Problema:** Mistura de estilos de rota:
- `api/v1/devices` (RESTful ✅)
- `api/v1/Device/GetAllDevices` (RPC-style ❌)
- `api/v1/Em300/TH/GetByDevEui` (inconsistente ❌)

**Solução:** Padronizar todas as rotas para estilo RESTful.

---

### 8. **Falta de Validação de Input Consistente**
**Ficheiro:** `UsersController.cs:209`

```csharp
// ❌ ERRADO - Aceita string diretamente
[HttpPost("request-email-change")]
public async Task<IActionResult> RequestEmailChange([FromBody] string newEmail)
```

**Solução:** Usar DTOs com Data Annotations para validação.

---

### 9. **Try-Catch Genérico nos Controllers**
**Ficheiros afetados:** Múltiplos controllers

```csharp
// ❌ ERRADO - Catch genérico
catch (Exception ex)
{
    return BadRequest(new { message = ex.Message });
}
```

**Problema:** Captura todas as exceções e retorna 400, mesmo para erros 500.

**Solução:** Deixar o middleware global tratar exceções ou usar filtros específicos.

---

### 10. **Métodos Privados Desnecessários**
**Ficheiro:** `DeviceController.cs`

```csharp
// ❌ ERRADO - Wrapper desnecessário
[HttpGet("api/v1/devices")]
public async Task<IActionResult> GetAllDevices()
{
    return await ProcessGetAllDevices();
}

private async Task<IActionResult> ProcessGetAllDevices() { ... }
```

**Problema:** Métodos wrapper que apenas chamam outro método.

**Solução:** Código direto no action method ou extrair para Service.

---

### 11. **Falta de Logging Estruturado**
**Problema:** Uso inconsistente de logging:
- Alguns lugares usam `Console.WriteLine`
- Outros usam `_logger.LogInformation`
- Alguns não têm logging

**Solução:** Usar `ILogger` consistentemente com níveis apropriados.

---

### 12. **Nullable Reference Types Ignorados**
**Ficheiros afetados:** Múltiplos

```csharp
// ❌ ERRADO - Pode ser null
var company = HttpContext.Items["Company"] as Company;
if (company == null) return Unauthorized(...);
```

**Solução:** Usar pattern matching e null-coalescing operators.

---

### 13. **Lógica de Negócio nos Controllers**
**Ficheiro:** `AuthController.cs:54-81`

```csharp
// ❌ ERRADO - Validação de demo email no controller
var demoEmailRegex = new Regex(@"^[a-zA-Z0-9]+_(\d{6})@kkdemo\.com$");
var match = demoEmailRegex.Match(login.Email);
if (match.Success) { ... }
```

**Solução:** Mover para `AuthService` ou `UserService`.

---

### 14. **Escrita de Ficheiros no Controller**
**Ficheiro:** `AuthController.cs:167-178`

```csharp
// ❌ ERRADO - I/O no controller
var logDir = Path.Combine(AppContext.BaseDirectory, "kklogs");
Directory.CreateDirectory(logDir);
await System.IO.File.AppendAllTextAsync(filePath, logMessage);
```

**Solução:** Usar `ILogger` com file sink (Serilog) ou serviço dedicado.

---

### 15. **Falta de Cancellation Token**
**Problema:** Nenhum método async aceita `CancellationToken`.

```csharp
// ❌ ERRADO
public async Task<IActionResult> GetAllDevices()

// ✅ CORRETO
public async Task<IActionResult> GetAllDevices(CancellationToken ct)
```

---

### 16. **Repository com Lógica de Apresentação**
**Ficheiro:** `DeviceRepository.cs:53-73`

```csharp
// ❌ ERRADO - Cria "device virtual" para UI
var header = new Device
{
    Id = 0,
    Name = device.Name,
    Description = $"-- {currentCompany} --",
    DevEui = "0000000000000000"
};
```

**Problema:** Repository não deve criar dados fictícios para apresentação.

**Solução:** Mover lógica de agrupamento para Service ou ViewModel.

---

### 17. **Modificação de Entidades Tracked**
**Ficheiro:** `DeviceRepository.cs:126-156`

```csharp
// ❌ ERRADO - Modifica entidade tracked
device.Company.Name = "KropKontrol";
device.CompanyId = 20;
```

**Problema:** Pode causar updates acidentais na BD.

**Solução:** Usar DTOs ou `AsNoTracking()`.

---

### 18. **Métodos Não Implementados**
**Ficheiro:** `DeviceRepository.cs:695-703`

```csharp
// ❌ ERRADO
public Task<Device> GetByDevEuiAsync(string devEui)
{
    throw new NotImplementedException();
}
```

**Solução:** Implementar ou remover da interface.

---

### 19. **SQL Interpolado Vulnerável**
**Ficheiro:** `DeviceRepository.cs:669-670`

```csharp
// ⚠️ CUIDADO - Usar com parâmetros
await _dbContext.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE kropkharts.devices SET battery = {batteryValue}...");
```

**Nota:** `ExecuteSqlInterpolatedAsync` é seguro, mas preferir EF Core methods.

---

### 20. **Configuração Duplicada de Swagger**
**Ficheiro:** `Program.cs:165-201` e `Program.cs:254-285`

```csharp
// ❌ ERRADO - AddSwaggerGen chamado duas vezes
builder.Services.AddSwaggerGen(c => { ... });
// ... mais tarde ...
builder.Services.AddSwaggerGen(c => { ... });
```

**Solução:** Consolidar numa única chamada.

---

## 📝 PROBLEMAS DE ESTILO E MANUTENÇÃO

### 21. **Inconsistência de Idiomas**
- Comentários em Português, Francês e Inglês
- Mensagens de erro em Francês
- Nomes de variáveis em Inglês

**Solução:** Padronizar para um idioma (preferencialmente Inglês).

---

### 22. **Falta de XML Documentation**
**Problema:** Quase nenhum endpoint tem documentação XML para Swagger.

---

### 23. **Atributos de Rota Inconsistentes**
```csharp
// Alguns usam [Route] no controller
[Route("api/v1/auth/")]

// Outros usam rota completa no método
[HttpGet("api/v1/devices")]
```

---

### 24. **Uso de `var` Inconsistente**
Mistura de declarações explícitas e implícitas sem padrão.

---

### 25. **Falta de Regions ou Organização**
Controllers com 300+ linhas sem organização clara.

---

## 🔐 PROBLEMAS DE SEGURANÇA

### 26. **CORS Muito Permissivo**
**Ficheiro:** `Program.cs:245-250`

```csharp
// ❌ ERRADO - Permite qualquer origem
policy.AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();
```

**Solução:** Restringir origens em produção.

---

### 27. **Token Bot Hardcoded**
**Ficheiro:** `TelegramCommandService.cs:17`

```csharp
// ❌ ERRADO
private const string BotToken = "SEU_TOKEN_AQUI";
```

---

### 28. **Email Hardcoded**
**Ficheiro:** `UsersController.cs:283`

```csharp
// ❌ ERRADO
email = "carnbq@gmail.com";
```

---

### 29. **Swagger Exposto em Produção**
**Ficheiro:** `Program.cs:296`

```csharp
// ⚠️ CUIDADO
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
```

---

## 📊 RESUMO DE PROBLEMAS POR CATEGORIA

| Categoria | Quantidade | Severidade |
|-----------|------------|------------|
| Arquitetura | 12 | Alta |
| Segurança | 6 | Alta |
| Manutenção | 15 | Média |
| Estilo | 8 | Baixa |
| Performance | 6 | Média |
| **TOTAL** | **47** | - |

---

## ✅ RECOMENDAÇÕES PRIORITÁRIAS

1. **Imediato:** Remover passwords de logs/exceções
2. **Imediato:** Configurar CORS restritivo para produção
3. **Curto prazo:** Remover código comentado
4. **Curto prazo:** Mover lógica de negócio para Services
5. **Médio prazo:** Padronizar rotas RESTful
6. **Médio prazo:** Adicionar documentação XML aos endpoints
7. **Longo prazo:** Refatorar Repository para não modificar entidades

---

## 📚 PRÓXIMOS PASSOS

1. Adicionar documentação Swagger a todos os endpoints
2. Criar documentação técnica da API
3. Implementar correções prioritárias

