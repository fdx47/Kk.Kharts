# 🔐 Guide d'Authentification API KropKontrol

## Introduction

L'API KropKontrol utilise deux méthodes d'authentification :
- **JWT Bearer** : Pour les utilisateurs (frontend, applications mobiles)
- **API Key** : Pour les passerelles IoT

---

## 🔑 Authentification JWT

### 1. Obtenir un Token

**Endpoint :** `POST /api/Auth/login`

**Corps de la requête :**
```json
{
  "email": "votre@email.com",
  "password": "votreMotDePasse"
}
```

**Réponse :**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "expiresAt": "2024-01-10T12:00:00Z"
}
```

### 2. Utiliser le Token

Ajoutez le token dans l'en-tête `Authorization` de chaque requête :

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🧪 Utilisation dans Swagger UI

### Étape 1 : Se Connecter

1. Ouvrez Swagger UI (généralement `https://votre-api/swagger`)
2. Trouvez l'endpoint `POST /api/Auth/login`
3. Cliquez sur "Try it out"
4. Entrez vos identifiants
5. Cliquez sur "Execute"
6. **Copiez le token** de la réponse (sans les guillemets)

### Étape 2 : Autoriser

1. Cliquez sur le bouton **"Authorize"** 🔓 en haut à droite
2. Dans le champ "Value", collez **uniquement le token** (sans "Bearer ")
3. Cliquez sur "Authorize"
4. Cliquez sur "Close"

### ⚠️ Erreurs Courantes

| Erreur | Cause | Solution |
|--------|-------|----------|
| 401 Unauthorized | Token manquant ou invalide | Vérifiez que vous avez bien autorisé avec le token |
| 401 Unauthorized | Token expiré | Reconnectez-vous pour obtenir un nouveau token |
| 401 Unauthorized | "Bearer " inclus dans le champ | Ne mettez QUE le token, sans "Bearer " |
| 403 Forbidden | Permissions insuffisantes | Votre rôle n'a pas accès à cette ressource |

---

## 🔄 Rafraîchir le Token

Quand le token expire, utilisez le refresh token :

**Endpoint :** `POST /api/Auth/refresh`

**Corps :**
```json
{
  "token": "ancien_token_jwt",
  "refreshToken": "votre_refresh_token"
}
```

---

## 📡 Authentification API Key (Passerelles IoT)

Pour les passerelles IoT, utilisez l'en-tête `KropKontrol` :

```
KropKontrol: votre_api_key
```

Cette méthode est réservée aux endpoints de réception de données des capteurs.

---

## 🛠️ Dépannage

### Le token ne fonctionne pas

1. **Vérifiez l'expiration** : Les tokens expirent après un certain temps (configuré dans l'API)
2. **Vérifiez le format** : Le token doit commencer par `eyJ`
3. **Vérifiez les espaces** : Pas d'espaces avant ou après le token
4. **Reconnectez-vous** : Obtenez un nouveau token via `/api/Auth/login`

### Erreur 401 persistante

1. Vérifiez que l'endpoint nécessite bien une authentification JWT
2. Certains endpoints utilisent l'API Key au lieu du JWT
3. Vérifiez que votre compte utilisateur est actif

### Tester le Token

Vous pouvez décoder votre token sur [jwt.io](https://jwt.io) pour vérifier :
- La date d'expiration (`exp`)
- L'émetteur (`iss`)
- L'audience (`aud`)

---

## 📋 Exemple Complet avec cURL

### Login
```bash
curl -X POST "https://votre-api/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

### Requête Authentifiée
```bash
curl -X GET "https://votre-api/api/Devices" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

---

## 📋 Exemple avec JavaScript/Fetch

```javascript
// Login
const loginResponse = await fetch('https://votre-api/api/Auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'user@example.com', password: 'password123' })
});
const { token } = await loginResponse.json();

// Requête authentifiée
const devicesResponse = await fetch('https://votre-api/api/Devices', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const devices = await devicesResponse.json();
```

---

## 🔒 Bonnes Pratiques de Sécurité

1. **Ne partagez jamais** votre token ou refresh token
2. **Stockez les tokens** de manière sécurisée (pas en localStorage pour les apps web sensibles)
3. **Utilisez HTTPS** pour toutes les communications
4. **Déconnectez-vous** quand vous avez terminé
5. **Changez régulièrement** votre mot de passe

---

*Documentation KropKontrol API - Version 1.0*
