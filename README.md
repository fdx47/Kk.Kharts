# KropKontrol - Système de Gestion IoT Agricole

## Description

KropKontrol est une solution complète de gestion et de surveillance des cultures agricoles basée sur l'Internet des Objets (IoT). Le système permet de collecter, analyser et visualiser en temps réel les données provenant de capteurs déployés dans les champs.

## Fonctionnalités Principales

### 🌱 Surveillance des Cultures
- **Capteurs Multi-Sondes** : Support des capteurs WET150 pour la mesure de l'humidité du sol, la température et la conductivité électrique
- **Stations Météorologiques** : Intégration des stations UC502 pour la collecte de données climatiques
- **Alertes en Temps Réel** : Système de notifications Telegram pour les conditions critiques

### 🔒 Gestion des Accès
- **Authentification JWT** : Système sécurisé avec tokens et refresh tokens
- **Contrôle d'Accès Basé sur les Rôles** : Différenciation entre utilisateurs Root, Admin et standards
- **Identifiants Obfusqués** : Utilisation de HashIDs pour protéger contre l'énumération des ressources

### 📊 Visualisation et Analyse
- **Tableau de Bord Temps Réel** : Interface web moderne avec Vue.js
- **Rapports Historiques** : Analyse des tendances et export des données
- **Cartographie** : Représentation géographique des capteurs et des parcelles

## Architecture Technique

### Backend (API REST)
- **Framework** : ASP.NET Core 10
- **Base de Données** : PostgreSQL avec Entity Framework Core
- **Messagerie** : Telegram Bot API pour les notifications
- **Cache** : Support intégré pour le caching des données fréquemment accédées

### Frontend
- **Application Web** : Vue.js 3 avec Quasar Framework
- **Application Mobile** : MAUI pour iOS et Android
- **Mini-App Telegram** : Interface légère pour un accès rapide

### Infrastructure
- **Hébergement** : Compatible Docker pour le déploiement cloud
- **CI/CD** : Workflows GitHub Actions pour l'intégration continue

## Sécurité

### Protection des Identifiants
Le système utilise **HashIDs** pour obfusquer les identifiants numériques internes :

- **Longueur Minimale** : 12 caractères
- **Sel Unique** : `$KropKontrol$2025!!!2026!!!`
- **Compatibilité** : Endpoints legacy (IDs numériques) marqués comme obsolètes

### Points de Terminaison API

#### Version Actuelle (v1 avec HashIDs)
```
GET    /api/v1/companies/{hash}      # Récupérer une entreprise par HashID
PUT    /api/v1/companies/{hash}      # Mettre à jour une entreprise
DELETE /api/v1/companies/{hash}      # Désactiver une entreprise
```

#### Version Legacy (Obsolète)
```
GET    /api/v1/companies/{id:int}    [OBSOLÈTE]
PUT    /api/v1/companies/{id:int}    [OBSOLÈTE]
DELETE /api/v1/companies/{id:int}    [OBSOLÈTE]
```

> ⚠️ Les endpoints utilisant des IDs numériques sont obsolètes. Veuillez migrer vers les endpoints HashIDs.

### JWT Claims
Le token JWT inclut désormais :
- `SocieteId` : ID numérique de l'entreprise (legacy)
- `companyPublicId` : HashID de l'entreprise (recommandé)

## Prérequis

- **.NET 10 SDK**
- **PostgreSQL 15+**
- **Node.js 20+** (pour le frontend)
- **Docker** (optionnel, pour le déploiement)

## Installation

### 1. Cloner le Repository
```bash
git clone https://github.com/fdx47/KK.Kharts-IoT.git
cd KK.Kharts-IoT
```

### 2. Configuration Backend
```bash
cd Kk.Kharts.Api
cp appsettings.example.json appsettings.json
# Éditer appsettings.json avec vos paramètres
```

### 3. Base de Données
```bash
dotnet ef database update
```

### 4. Lancement
```bash
dotnet run --project Kk.Kharts.Api
```

## Structure du Projet

```
KK.Kharts-IoT/
├── Kk.Kharts.Api/              # API REST principale
├── Kk.Kharts.Shared/            # DTOs et entités partagées
├── Kk.Kharts.Api.Tests/         # Tests unitaires et d'intégration
├── KropKontrol.Vue/             # Application web (Vue.js)
├── KropKontrol.Mobile/          # Application mobile (MAUI)
├── Kk.Kharts.TelegramMiniApp/   # Mini-application Telegram
└── Kk.StoreAndForward/          # Service de stockage local
```

## Contribution

Les contributions sont les bienvenues ! Veuillez :
1. Forker le projet
2. Créer une branche (`git checkout -b feature/AmazingFeature`)
3. Commiter vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Pousser vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## Licence

Ce projet est sous licence propriétaire. Tous droits réservés.

## Contact

- **Projet** : [github.com/fdx47/KK.Kharts-IoT](https://github.com/fdx47/KK.Kharts-IoT)
- **Support** : Contactez l'équipe technique via le canal Telegram

---

<p align="center">Développé avec ❤️ pour l'agriculture intelligente</p>
