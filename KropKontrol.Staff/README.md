# KropKontrol - Staff Portal

Portail interne pour les équipes KropKontrol, conçu pour la gestion des outils et services internes.

## Modules Actuels

### 🛡️ Gestionnaire VPN
- 🔐 Authentification sécurisée (Admin/Root uniquement)
- 📋 Gestion complète des profils VPN (CRUD)
- 👥 Attribution des profils aux utilisateurs
- 📤 Upload et téléchargement des fichiers .ovpn
- 📊 Import en masse depuis fichier CSV
- 📍 Suivi des installations et localisations
- 🔍 Recherche et filtres avancés

## Fonctionnalités Communes

- 🌐 Interface responsive et moderne
- 🔐 Authentification JWT sécurisée
- 📱 Design mobile-friendly
- 🎨 Interface en français
- 🔄 Temps réel avec les notifications

## Technologies

- Vue 3 (Composition API)
- Vue Router
- Bootstrap 5
- Axios
- JWT Authentication

## Installation

```bash
npm install
```

## Développement

```bash
npm run dev
```

L'application sera accessible sur `http://localhost:5175`

## Déploiement

Le portail est déployé sous `/staff/` sur le même domaine que KropKontrol principal.

## Configuration

Les variables d'environnement sont définies dans `.env.development` et `.env.production`:

- `VITE_API_BASE_URL`: URL de base de l'API KropKontrol (même API que KropKontrol.Vue)
- `VITE_BASE_PATH`: Chemin de base pour le déploiement (`/staff/` en production)

## Build Production

```bash
npm run build
```

## Accès

Seuls les utilisateurs avec le rôle **Root** peuvent accéder à cette application.

## Déploiement

Le portail est déployé sous `/staff/` sur `www.kropkontrol.com`.

## Structure du Projet

```
src/
├── views/           # Pages de l'application
├── services/        # Services API
├── App.vue          # Composant racine
└── main.js          # Point d'entrée
```

## API Endpoints

L'application consomme les endpoints suivants de l'API KropKontrol:

- `POST /api/v1/auth/login` - Authentification
- `GET /api/v1/vpn-profiles` - Liste des profils
- `POST /api/v1/vpn-profiles` - Créer un profil
- `PUT /api/v1/vpn-profiles/{id}` - Modifier un profil
- `DELETE /api/v1/vpn-profiles/{id}` - Supprimer un profil
- `POST /api/v1/vpn-profiles/{id}/assign` - Attribuer à un utilisateur
- `POST /api/v1/vpn-profiles/{id}/unassign` - Désattribuer
- `POST /api/v1/vpn-profiles/{id}/upload-ovpn` - Upload fichier .ovpn
- `GET /api/v1/vpn-profiles/{id}/download-ovpn` - Télécharger fichier .ovpn
- `POST /api/v1/vpn-profiles/import-csv` - Import CSV

## Licence

Propriétaire - KropKontrol © 2026
