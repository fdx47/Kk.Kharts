# Debug Bot - KropKontrolDEV (@KropKontrolDEV_bot)

## Descrição

Bot interno para a equipa de desenvolvimento. Envia alertas de sistema, erros, status de dispositivos e mensagens de startup para o canal de debug.

**⚠️ Este bot é apenas para uso interno. Não partilhar com clientes.**

---

## Configuração

### Token e Credenciais
```
Bot Username: @KropKontrolDEV_bot
Bot Token: 7750790227:AAFyFKBiw7nnuQrxzgoYYz-yzDKV6qg53Yg
```

### Webhook
```
URL: https://kropkontrol.premiumasp.net/api/v1/webhooks/debug/{secret}
Secret: kKteLeGRam47kkTElEgraM47
```

### Configurar Webhook
```
https://api.telegram.org/bot7750790227:AAFyFKBiw7nnuQrxzgoYYz-yzDKV6qg53Yg/setWebhook?url=https://kropkontrol.premiumasp.net/api/v1/webhooks/debug/kKteLeGRam47kkTElEgraM47
```

### Verificar Webhook
```
https://api.telegram.org/bot7750790227:AAFyFKBiw7nnuQrxzgoYYz-yzDKV6qg53Yg/getWebhookInfo
```

---

## appsettings.json

```json
"Telegram": {
    "_comment_DebugBot": "Bot interno (@KropKontrolDEV_bot) - Alertas, erros, startup, comandos admin",
    "DebugBotToken": "7750790227:AAFyFKBiw7nnuQrxzgoYYz-yzDKV6qg53Yg",
    "DebugChatId": "-1002549895669",
    "DebugWebhookSecret": "kKteLeGRam47kkTElEgraM47",
    "ErrorsTopicId": 16932,
    "DebugTopicId": 16937,
    "DeviceStatusTopicId": 17222
}
```

---

## Comandos Disponíveis

| Comando | Descrição |
|---------|-----------|
| `/last` | Última comunicação de todos os dispositivos |
| `/lastseen [X]` | Dispositivos sem dados nos últimos X minutos (padrão: 20) |
| `/offline [X]` | Dispositivos offline há mais de X minutos (padrão: 15) |
| `/inactive` | Dispositivos marcados como inativos |
| `/createuserdemo FirstName LastName Password [Days]` | Criar utilizador demo |
| `/stats` | Estatísticas de utilização |
| `/help` | Mostrar ajuda |

---

## Mensagens Automáticas

O Debug Bot envia automaticamente:

1. **Startup** - `🚀 KropKharts API Started` quando a API inicia
2. **Erros SDI-12** - Alertas de erros de sensores (tópico ErrorsTopicId)
3. **Status de Dispositivos** - Alterações de estado (tópico DeviceStatusTopicId)
4. **Logs de Debug** - Informações de debug (tópico DebugTopicId)

---

## Tópicos do Canal

| Tópico | ID | Descrição |
|--------|-----|-----------|
| Erros SDI-12 | 16932 | Alertas de erros de sensores |
| Debug | 16937 | Logs e informações de debug |
| Status Dispositivos | 17222 | Alterações de estado de dispositivos |

---

## Ficheiros Relacionados

| Ficheiro | Descrição |
|----------|-----------|
| `Controllers/DebugBotController.cs` | Controller para comandos do bot debug |
| `Services/Telegram/TelegramService.cs` | Serviço que usa `_debugBotClient` para mensagens de sistema |
| `Services/Telegram/TelegramOptions.cs` | Configurações (DebugBotToken, DebugChatId, etc.) |

---

## Segurança

- **Token:** Nunca expor publicamente
- **Secret:** Usar secret complexo no webhook
- **Acesso:** Apenas equipa interna no canal de debug
- **Dados:** Pode conter informações sensíveis (IPs, erros, etc.)
