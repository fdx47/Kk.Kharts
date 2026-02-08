# Kk.Kharts.Maui - KropKontrol Mobile App

Aplicação móvel .NET MAUI para monitorização de sensores IoT KropKontrol.

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** com separação clara de responsabilidades:

```
Kk.Kharts.Maui/
├── Converters/          # Value converters para XAML bindings
├── Models/              # Modelos de domínio e DTOs
├── Services/            # Serviços de aplicação (API, Auth, Navigation)
├── ViewModels/          # ViewModels com MVVM pattern
├── Views/               # Páginas XAML
└── Resources/           # Recursos (estilos, imagens, fontes)
```

### Padrões Utilizados

- **MVVM** - Model-View-ViewModel com CommunityToolkit.Mvvm
- **Dependency Injection** - Injeção de dependências nativa do MAUI
- **Repository Pattern** - Abstração de acesso a dados
- **Result Pattern** - Tratamento de erros sem exceções

## 📱 Funcionalidades

- ✅ **Autenticação** - Login com JWT e refresh token automático
- ✅ **Dashboard** - Visão geral dos dispositivos
- ✅ **Lista de Kapteurs** - Pesquisa e filtros
- ✅ **Detalhes do Dispositivo** - Configuração e informações
- ✅ **Gráficos** - Visualização de dados com LiveCharts2
- ✅ **Configurações** - Perfil e logout

## 🛠️ Tecnologias

- **.NET 9.0** - Framework base
- **.NET MAUI** - UI cross-platform
- **CommunityToolkit.Mvvm** - MVVM helpers e source generators
- **CommunityToolkit.Maui** - Extensões MAUI
- **LiveChartsCore** - Gráficos interativos
- **System.IdentityModel.Tokens.Jwt** - Manipulação de JWT

## 📦 Dependências

O projeto reutiliza `Kk.Kharts.Shared` para DTOs, garantindo consistência com a API.

## 🚀 Plataformas Suportadas

- **Android** 7.0+ (API 24)
- **iOS** 15.0+
- **Windows** 10.0.17763.0+
- **macOS** 15.0+ (Mac Catalyst)

## 🔧 Configuração

### API Base URL

Editar em `Services/ApiSettings.cs`:

```csharp
public const string BaseUrl = "https://kropkontrol.premiumasp.net/api/v1";
```

### Fontes

Descarregar as fontes Poppins do Google Fonts e colocar em `Resources/Fonts/`:
- Poppins-Regular.ttf
- Poppins-SemiBold.ttf
- Poppins-Bold.ttf

## 📝 Build

```bash
# Restaurar pacotes
dotnet restore

# Build para Android
dotnet build -f net9.0-android

# Build para Windows
dotnet build -f net9.0-windows10.0.19041.0

# Build para iOS (requer macOS)
dotnet build -f net9.0-ios
```

## 🎨 Cores da Marca

| Cor | Hex | Uso |
|-----|-----|-----|
| Primary | `#82BE20` | Cor principal KropKontrol |
| PrimaryDark | `#6BAA1A` | Hover/pressed states |
| PrimaryLight | `#E8F5D6` | Backgrounds |

## 📄 Licença

Propriedade de 3ctec e StratBerries.
