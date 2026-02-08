# KropKontrol Telegram Mini App

Uma aplicação web completa que roda dentro do Telegram, oferecendo acesso total à plataforma KropKontrol IoT.

## 🚀 Funcionalidades

### Dashboard
- Visão geral dos sensores (online/offline)
- Alertas ativos
- Últimas leituras
- Ações rápidas

### Capteurs (Sensores)
- Lista de todos os sensores
- Filtros por estado (online/offline)
- Indicador de bateria
- Navegação para detalhes

### Détails du Capteur
- Valores em tempo real (temperatura, humidade)
- Gráficos interativos (6h, 24h, 7d, 30d)
- Informações do dispositivo
- Partilha de dados
- Configuração de alertas

### Carte (Mapa)
- Mapa interativo com Leaflet
- Localização de todos os sensores
- Marcadores com estado (online/offline)
- Popups com informações

### Alertes
- Lista de alertas ativos
- Histórico de alertas
- Acquittement de alertas
- Indicadores de severidade

### Support
- Interface de chat
- Envio de mensagens ao suporte
- Perguntas frequentes
- Histórico de conversas

### Paramètres
- Informações do utilizador
- Configuração de notificações
- Unidade de temperatura
- Informações da app

## 🛠️ Tecnologias

- **Vue 3** - Framework frontend
- **Vite** - Build tool
- **Pinia** - State management
- **Vue Router** - Navegação
- **TailwindCSS** - Estilos
- **Chart.js** - Gráficos
- **Leaflet** - Mapas
- **Axios** - HTTP client

## 📱 Funcionalidades Telegram

- **Autenticação automática** via Telegram
- **Tema adaptativo** (segue o tema do Telegram)
- **Haptic feedback** (vibração)
- **Popups nativos** (confirmações, alertas)
- **Partilha inline** de dados
- **Safe area** para dispositivos com notch

## 🔧 Instalação

```bash
# Instalar dependências
npm install

# Desenvolvimento
npm run dev

# Build para produção
npm run build
```

## 🌐 Configuração

1. Crie um ficheiro `.env`:
```env
VITE_API_URL=https://kropkontrol.premiumasp.net/api/v1
```

2. Configure o bot no BotFather:
   - Vá a @BotFather
   - Selecione o bot
   - Edit Bot > Bot Settings > Menu Button
   - Configure a URL da Mini App

## 📦 Deploy

1. Build: `npm run build`
2. Upload da pasta `dist/` para o servidor
3. Configure HTTPS (obrigatório para Mini Apps)
4. Configure o Menu Button no BotFather

## 🔗 Integração com o Bot

A Mini App é aberta através do botão de menu do bot ou via link:
```
https://t.me/kropkontrol_bot/app
```

## 📄 Licença

Proprietary - KropKontrol © 2024
