# 📚 Documentation API KropKontrol

**Version:** v0.3b  
**Base URL:** `https://kropkontrol.premiumasp.net/api/v1`  
**Dernière mise à jour:** 31/12/2024

---

## 🔐 Authentification

L'API utilise deux méthodes d'authentification:

### 1. JWT Bearer Token (Utilisateurs)

Pour les endpoints protégés par `[Authorize]`, incluez le token JWT dans le header:

```http
Authorization: Bearer <votre_token_jwt>
```

### 2. API Key (Passerelles IoT)

Pour les endpoints protégés par `[ApiKeyAuthorizeKk]`, incluez la clé API dans le header:

```http
KropKontrol: <votre_api_key>
```

---

## 📋 Endpoints

### 🔑 Auth - Authentification

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/auth/login` | Authentifie un utilisateur et retourne un token JWT |
| POST | `/auth/refresh-token` | Renouvelle le token JWT avec un refresh token valide |

#### POST /auth/login

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "VotreMotDePasse123!"
}
```

**Response 200:**
```json
{
  "message": "Connexion réussie.",
  "isSuccess": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "refreshTokenExpiryTime": "2024-01-01T12:00:00Z"
}
```

---

### 📡 Devices - Dispositifs

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/devices` | Liste tous les dispositifs accessibles | JWT |
| GET | `/devices/{devEui}` | Détails d'un dispositif | JWT |
| GET | `/devices/{devEui}/battery` | Niveau de batterie | JWT |
| PUT | `/devices/{devEui}/config` | Met à jour la configuration | JWT (Write) |
| POST | `/devices` | Crée un nouveau dispositif | JWT (Root) |
| GET | `/devices/models` | Liste des modèles disponibles | JWT (Root) |
| GET | `/devices/{devEui}/model` | Modèle d'un dispositif | JWT |

#### GET /devices

**Response 200:**
```json
[
  {
    "id": 1,
    "devEui": "24E124454E353385",
    "name": "Capteur Serre 1",
    "description": "Capteur VWC principal",
    "model": 47,
    "battery": 85.5,
    "lastSendAt": "Il y a 2 heures",
    "activeInKropKontrol": true,
    "installationLocation": "Serre Nord",
    "companyName": "Ma Société"
  }
]
```

#### PUT /devices/{devEui}/config

**Request Body:**
```json
{
  "name": "Nouveau nom",
  "description": "Nouvelle description",
  "installationLocation": "Nouvel emplacement",
  "activeInKropKontrol": true
}
```

---

### ⚠️ Alarmes - Seuils d'alerte

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/devices/thresholds-alarms` | Toutes les règles d'alarme | JWT (Root) |
| POST | `/devices/thresholds-alarms` | Enregistre des seuils | JWT (Write) |
| GET | `/devices/{devEui}/thresholds-alarms` | Alarmes d'un dispositif | JWT (Write) |

#### POST /devices/thresholds-alarms

**Request Body:**
```json
{
  "24E124454E353385": {
    "mineralVWC": {
      "low": 30,
      "high": 80,
      "hysteresis": 5
    },
    "temperature": {
      "low": 15,
      "high": 35,
      "hysteresis": 2
    }
  }
}
```

---

### 🌡️ EM300 - Capteurs Température/Humidité

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/em300/{devEui}/th` | Données température/humidité | JWT |
| POST | `/em300/th/https-ug65` | Envoie données TH (passerelle) | API Key |
| GET | `/em300/{devEui}/di` | Données entrées digitales | JWT |
| POST | `/em300/di/https-Ug65` | Envoie données DI (passerelle) | API Key |

#### GET /em300/{devEui}/th

**Query Parameters:**
- `startDate` (DateTime): Date de début (ex: 2024-01-01)
- `endDate` (DateTime): Date de fin (ex: 2024-01-31)

**Response 200:**
```json
{
  "devEui": "24E124136D448043",
  "deviceId": 5,
  "name": "Capteur TH Serre",
  "description": "Température et humidité",
  "data": [
    {
      "timestamp": "2024-01-15T10:30:00Z",
      "temperature": 22.5,
      "humidity": 65.3,
      "battery": 92.0
    }
  ]
}
```

---

### 🌱 UC502 - Capteurs VWC/EC (WET-150)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/uc502/{devEui}/wet150` | Données VWC/EC | JWT |
| POST | `/uc502/wet150/` | Envoie données WET-150 | API Key |
| GET | `/uc502/wet150/multisensor` | Données multi-capteur | JWT |
| POST | `/uc502/wet150/multisensor` | Envoie données multi-capteur | API Key |
| GET | `/uc502/{devEui}/modbus` | Données Modbus | JWT |
| POST | `/uc502/modbus` | Envoie données Modbus | API Key |

#### GET /uc502/{devEui}/wet150

**Query Parameters:**
- `startDate` (DateTime): Date de début
- `endDate` (DateTime): Date de fin

**Response 200:**
```json
{
  "devEui": "24E124454E353385",
  "deviceId": 10,
  "name": "VWC Serre 1",
  "description": "Capteur humidité sol",
  "installationLocation": "Rang 5",
  "data": [
    {
      "timestamp": "2024-01-15T10:30:00Z",
      "permittivite": 15.2,
      "ecb": 1.25,
      "soilTemperature": 18.5,
      "mineralVWC": 45.3,
      "organicVWC": 42.1,
      "peatMixVWC": 48.7,
      "coirVWC": 44.2,
      "minWoolVWC": 52.1,
      "perliteVWC": 38.9,
      "mineralECp": 2.1,
      "organicECp": 1.8,
      "battery": 78.5
    }
  ]
}
```

---

### 🏢 Companies - Entreprises

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/companies` | Liste toutes les entreprises | JWT (Root) |
| GET | `/companies/{id}` | Détails d'une entreprise | JWT |
| POST | `/companies` | Crée une entreprise | JWT (Root) |
| PUT | `/companies/{id}` | Met à jour une entreprise | JWT (Root) |
| DELETE | `/companies/{id}` | Désactive une entreprise | JWT (Root) |

---

### 👥 Users - Utilisateurs

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/users` | Liste tous les utilisateurs | JWT |
| GET | `/users/{id}` | Détails d'un utilisateur | JWT (Root) |
| POST | `/users` | Crée un utilisateur | JWT (Root) |
| PUT | `/users/{id}` | Met à jour un utilisateur | JWT (Root) |
| DELETE | `/users/{id}` | Supprime un utilisateur | JWT (Root) |
| PUT | `/users/me` | Met à jour son propre compte | JWT |

---

### 📊 Dashboard - Tableau de bord

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/dashboards/state` | Récupère l'état du dashboard | JWT |
| POST | `/dashboards/state` | Sauvegarde l'état du dashboard | JWT |

---

## 🔒 Rôles et Permissions

| Rôle | Description | Accès |
|------|-------------|-------|
| **Root** | Super administrateur | Tous les endpoints |
| **SuperAdmin** | Administrateur | Gestion entreprise + dispositifs |
| **Admin** | Administrateur local | Gestion dispositifs entreprise |
| **UserRW** | Utilisateur lecture/écriture | Lecture + modification config |
| **User** | Utilisateur lecture seule | Lecture uniquement |
| **Technician** | Technicien | Dispositifs assignés uniquement |
| **Demo** | Compte démo | Accès limité, données anonymisées |

---

## ❌ Codes d'erreur

| Code | Description |
|------|-------------|
| 400 | Bad Request - Données invalides |
| 401 | Unauthorized - Token manquant ou invalide |
| 403 | Forbidden - Accès refusé (permissions insuffisantes) |
| 404 | Not Found - Ressource non trouvée |
| 409 | Conflict - Ressource déjà existante |
| 500 | Internal Server Error - Erreur serveur |
| 503 | Service Unavailable - Base de données temporairement indisponible |

**Format d'erreur standard:**
```json
{
  "message": "Description de l'erreur pour l'utilisateur",
  "details": "Détails techniques (si disponibles)"
}
```

---

## 📝 Notes importantes

1. **Dates**: Toutes les dates sont en UTC (format ISO 8601)
2. **DevEUI**: Identifiant unique de 16 caractères hexadécimaux (ex: `24E124454E353385`)
3. **Batterie**: Valeur en pourcentage (0-100)
4. **VWC**: Volumetric Water Content en pourcentage
5. **EC**: Electrical Conductivity en mS/cm

---

## 🔗 Liens utiles

- **Swagger UI**: `/swagger`
- **OpenAPI JSON**: `/swagger/v1/swagger.json`
- **Support**: cesar@3ctec.fr

