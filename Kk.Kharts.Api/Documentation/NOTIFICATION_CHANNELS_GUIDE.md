# Guide des canaux de notification (Telegram / Pushover)

## Objectif
Ce document décrit comment configurer les préférences de notification des utilisateurs, ainsi que les prérequis techniques pour Telegram et Pushover. Il s'adresse aux administrateurs support et aux exploitants qui gèrent les comptes clients.

---

## 1. Préférences utilisateur
Les préférences sont stockées sur l'entité `User` :
- `NotificationPreference` (enum extensible à flags) :
  - `Aucun` : l'utilisateur ne reçoit rien.
  - `Telegram`, `Pushover`, `Email` : envoient via un seul canal.
  - `TelegramEtPushover`, `TelegramEtEmail`, `PushoverEtEmail` : deux canaux simultanément.
  - `Tous` : Telegram + Pushover + Email en parallèle (ordre : Telegram → Pushover → Email).
  - Pour ajouter un nouveau canal (SMS, WhatsApp…), basta criar o `INotificationChannel` correspondente, adicionar o flag no enum e registrá-lo no router.
- `TelegramUserId` / `TelegramUsername` : définis via la commande `/link` du bot.
- `Pushover` : objet embarqué `PushoverSettings` (voir section 2).

> ℹ️ Par défaut, `NotificationPreference` vaut `Telegram`. Pensez à le modifier explicitement pour activer Pushover.

---

## 2. Configuration Pushover
Chaque utilisateur peut renseigner ses paramètres Pushover (colonne JSON `PushoverSettings`). Champs disponibles :

| Champ              | Obligatoire | Description                                                                      |
|--------------------|-------------|----------------------------------------------------------------------------------|
| `AppToken`         | Oui         | Token applicatif Pushover propre à l'utilisateur/entreprise (obligatoire).       |
| `UserKey`          | Oui         | Clé utilisateur Pushover (commence par `uQiR...`).                               |
| `Sound`            | Non         | Son Pushover (ex: `siren`, `bike`).                                              |
| `Device`           | Non         | Filtrer sur un device Pushover spécifique.                                       |
| `Title`            | Non         | Titre par défaut des notifications.                                              |
| `MessageTemplate`  | Non         | Template optionnel contenant le placeholder `{message}`.                         |
| `Priority`         | Non         | Priorité Pushover (0 par défaut).                                                |
| `RetrySeconds`     | Non         | Requis si `Priority=2`.                                                          |
| `ExpireSeconds`    | Non         | Requis si `Priority=2`.                                                          |

### Bonnes pratiques
1. **AppToken & UserKey obligatoires** : les deux champs doivent être renseignés dans la base, aucun fallback `appsettings` n'est appliqué.
2. **Utiliser `{message}` dans `MessageTemplate`** : le contenu final remplacera ce placeholder. Exemple :
   ```
   🔔 Alerte KropKontrol
   {message}
   ```
3. **Tester les sons** : Pushover filtre certains sons selon la plateforme. Préférer les sons standards (`pushover`, `siren`).
4. **Priorité 2 (Emergency)** : fixer `RetrySeconds` (>=30) et `ExpireSeconds` (<=10800) pour respecter les règles Pushover.

---

## 3. Processus de liaison Telegram
1. L'utilisateur démarre le bot `@kropkontrol_bot` et exécute `/link`.
2. Il fournit le code affiché dans l'application web.
3. Une fois lié, `TelegramUserId` et `TelegramLinkedAt` sont renseignés côté API.
4. Vérifier que le rôle / la compagnie de l'utilisateur lui donnent accès aux devices souhaités.

Si `TelegramUserId` est absent, le dispatcher tentera automatiquement Pushover (si configuré) ou loguera un avertissement.

---

## 4. Flux d'envoi
1. Chaque alerte métier (alarme, capteur offline/online, DeviceMonitorService) construit un `AlertNotification` (titre, corps HTML/TXT, actions).
2. `NotificationRouter` lit `NotificationPreference` et sélectionne les canaux enregistrés (`INotificationChannel`).
   - **Telegram** : rendu via `ITelegramNotificationRenderer`, envoi via `ITelegramService`.
   - **Pushover** : rendu via `IPushoverNotificationRenderer`, envoi via `IPushoverService`.
   - **Fallbacks automatiques** :
     - Pushover incomplet → reroute vers Telegram (si un chat est lié).
     - Mode "TelegramEtPushover" : tentative Telegram puis Pushover.
3. Chaque canal journalise son résultat (`NotificationChannelResult`), ce qui facilite l'audit et les relances ciblées.

---

## 5. Checklist déploiement / support
1. **Secrets appsettings** : `Pushover:AppToken` doit être défini dans les environnements non-prod et prod.
2. **Droits bot Telegram** : s'assurer que les bots (`DebugBot`, `ClientBot`) restent membres des groupes/tops nécessaires.
3. **Formation support** : communiquer ce guide à l'équipe support pour renseigner correctement Pushover lors de l'onboarding.
4. **Monitoring** : surveiller les logs `Notification Telegram/Pushover envoyée` et les warnings `MissingData` pour détecter les comptes mal configurés.
5. **Vérifications base** :
   - `user_pushover_settings` n'a des lignes que si un utilisateur configure Pushover.
   - Requête contrôle :
     ```sql
     SELECT u.id, u.email, ups.app_token, ups.user_key
     FROM kropKharts.users u
     LEFT JOIN kropKharts.user_pushover_settings ups ON u.id = ups.user_id
     WHERE u.notification_preference <> 1 AND (ups.app_token IS NULL OR ups.user_key IS NULL);
     ```
     Cette requête doit retourner zéro ligne avant passage en production.

---

## 6. FAQ
- **Q : Peut-on définir plusieurs devices Pushover ?**
  - R : Pushover accepte une liste séparée par virgules dans `device`. Documenter clairement ce format auprès du client.
- **Q : Comment désactiver temporairement toutes les notifications ?**
  - R : Régler `NotificationPreference` sur `Aucun`. Aucune notification n'est envoyée et aucun fallback n'est tenté.
- **Q : Que se passe-t-il si le message Telegram est trop long ?**
  - R : `ITelegramService` découpe déjà les longs messages (>4k). Le dispatcher réutilise donc des contenus sûrs.

---

## 7. API REST (admin/support)
Pour industrialiser la configuration des préférences, utiliser le contrôleur `UserPreferencesController` :

| Méthode / URL | Rôle requis | Description |
|---------------|-------------|-------------|
| `GET /api/v1/user-preferences/{userId}` | Root, SuperAdmin, Admin | Récupère le snapshot complet (préférence, scope, Pushover). |
| `GET /api/v1/user-preferences/me` | Authentifié | Retourne les propres préférences de l'utilisateur connecté.
| `PUT /api/v1/user-preferences/{userId}/notification-channel` | Root, SuperAdmin, Admin | Modifie `NotificationPreference` d'un utilisateur.
| `PUT /api/v1/user-preferences/me/notification-channel` | Authentifié | Permet à l'utilisateur de choisir Telegram, Pushover, etc.
| `PUT /api/v1/user-preferences/{userId}/access-scope` | Root, SuperAdmin | Ajuste `AccessLevel` et, au besoin, `CompanyId`.
| `PUT /api/v1/user-preferences/{userId}/pushover` | Root, SuperAdmin, Admin, UserRW | Met à jour ou supprime les réglages Pushover d'un utilisateur.
| `PUT /api/v1/user-preferences/me/pushover` | Authentifié | Permet de renseigner ses propres clés AppToken/UserKey.
| `GET /api/v1/user-preferences/pushover/metadata` | Authentifié | Retourne les listes de sons/priorités et les contraintes retry/expire pour construire un sélecteur UI.

Toutes les requêtes doivent être envoyées en JSON. Exemple (mise à jour Pushover) :
```json
PUT /api/v1/user-preferences/42/pushover
{
  "appToken": "apxxxxxxxx",
  "userKey": "uQiR-xxxxxxxx",
  "sound": "siren",
  "messageTemplate": "🔔 Alerte KropKontrol\n{message}",
  "priority": 1
}
```

Dernière mise à jour : 25/01/2026.
