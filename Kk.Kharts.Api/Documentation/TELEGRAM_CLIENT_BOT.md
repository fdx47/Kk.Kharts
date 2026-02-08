# Client Bot - KropKontrol (@kropkontrol_bot)

## Descrição

Bot para utilizadores finais (clientes). Permite aos clientes consultar os seus sensores, ver gráficos, receber alertas e gerir a sua conta.

**✅ Este bot é seguro para partilhar com clientes.**

---

## Configuração

### Token e Credenciais
```
Bot Username: @kropkontrol_bot
Bot Token: 8530004732:AAGjNvoVa4zVdxfr_ILSLHyuuLku4bzITgQ
```

### Webhook
```
URL: https://kropkontrol.premiumasp.net/api/v1/webhooks/client/{secret}
Secret: kKcLiEnT47kkCLieNT47
```

### Configurar Webhook
```
https://api.telegram.org/bot8530004732:AAGjNvoVa4zVdxfr_ILSLHyuuLku4bzITgQ/setWebhook?url=https://kropkontrol.premiumasp.net/api/v1/webhooks/client/kKcLiEnT47kkCLieNT47
```

### Verificar Webhook
```
https://api.telegram.org/bot8530004732:AAGjNvoVa4zVdxfr_ILSLHyuuLku4bzITgQ/getWebhookInfo
```

---

## appsettings.json

```json
"Telegram": {
    "_comment_ClientBot": "Bot clientes (@kropkontrol_bot) - Comandos utilizadores finais",
    "ClientBotToken": "8530004732:AAGjNvoVa4zVdxfr_ILSLHyuuLku4bzITgQ",
    "ClientWebhookSecret": "kKcLiEnT47kkCLieNT47"
}
```

---

## Comandos Disponíveis

### Comandos Gerais
| Comando | Descrição |
|---------|-----------|
| `/start` | Menu principal com botões interativos |
| `/help` | Mostrar lista de comandos disponíveis |

### Gestão de Conta
| Comando | Descrição |
|---------|-----------|
| `/link email password` | Associar conta Telegram à conta KropKontrol |
| `/unlink` | Desassociar conta |

### Consulta de Dados
| Comando | Descrição |
|---------|-----------|
| `/devices` | Lista de sensores do utilizador |
| `/chart` | Gráficos dos sensores (com seleção interativa) |
| `/last [devEui]` | Última leitura de um sensor |
| `/alerts` | Alertas ativos |
| `/battery` | Estado das baterias dos sensores |
| `/offline` | Sensores offline |

---

## Fluxo de Utilização

1. **Primeiro acesso:** Cliente envia `/start`
2. **Autenticação:** Cliente usa `/link email password` para associar conta
3. **Consulta:** Cliente pode usar os comandos para ver os seus dados
4. **Desassociar:** Cliente pode usar `/unlink` para remover associação

---

## Características

- **Privacidade:** Cada cliente só vê os seus próprios dados
- **Autenticação:** Requer `/link` antes de aceder aos dados
- **Interface:** Botões interativos para navegação fácil
- **Gráficos:** Geração de gráficos via QuickChart.io
- **Paginação:** Listas longas são paginadas automaticamente

---

## Ficheiros Relacionados

| Ficheiro | Descrição |
|----------|-----------|
| `Controllers/ClientBotController.cs` | Controller para webhook do bot cliente |
| `Services/Telegram/TelegramService.cs` | Serviço que usa `_clientBotClient` para respostas |
| `Services/Telegram/Commands/Handlers/` | Handlers de comandos |
| `Services/Telegram/Commands/Callbacks/` | Handlers de callbacks (botões) |
| `Services/Telegram/TelegramCommandDispatcher.cs` | Dispatcher central de comandos |

---

## Segurança

- **Token:** Nunca expor publicamente
- **Secret:** Usar secret complexo no webhook
- **Dados:** Apenas dados do utilizador autenticado
- **Isolamento:** Completamente separado do bot de debug

---

## Diferenças do Debug Bot

| Aspecto | Debug Bot | Client Bot |
|---------|-----------|------------|
| Público | Equipa interna | Clientes |
| Dados | Todos os dispositivos | Apenas do utilizador |
| Mensagens automáticas | Sim (startup, erros) | Não |
| Comandos admin | Sim | Não |
| Canal/Grupo | Sim (com tópicos) | Não (chat privado) |
