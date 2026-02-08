# 🤖 Guide d'Utilisation du Bot Telegram KropKontrol

## Introduction

Le bot Telegram KropKontrol vous permet de surveiller vos capteurs agricoles directement depuis Telegram. Vous pouvez consulter les données en temps réel, visualiser des graphiques et recevoir des alertes automatiques.

---

## 🚀 Démarrage Rapide

### 1. Trouver le Bot

Recherchez le bot dans Telegram ou utilisez le lien direct fourni par votre administrateur.

### 2. Démarrer le Bot

Envoyez la commande `/start` pour afficher le menu principal.

### 3. Lier Votre Compte

**Important :** Pour accéder à vos capteurs personnels, vous devez lier votre compte Telegram à votre compte KropKontrol.

```
/link votre@email.com votreMotDePasse
```

**Exemple :**
```
/link jean.dupont@entreprise.fr MonMotDePasse123
```

> ⚠️ **Sécurité :** Le message contenant votre mot de passe sera automatiquement supprimé après la liaison.

---

## 📋 Commandes Disponibles

### 🔑 Gestion du Compte

| Commande | Description |
|----------|-------------|
| `/link email mot_de_passe` | Lier votre compte Telegram à KropKontrol |
| `/unlink` | Délier votre compte |

### ℹ️ Informations Générales

| Commande | Description |
|----------|-------------|
| `/start` | Afficher le menu principal |
| `/help` | Afficher l'aide et la liste des commandes |
| `/status` | Voir l'état général du système |

### 📡 Capteurs

| Commande | Description |
|----------|-------------|
| `/devices` | Lister vos capteurs |
| `/last [devEui]` | Voir la dernière lecture d'un capteur |
| `/offline` | Voir les capteurs hors ligne |
| `/battery` | Voir l'état des batteries |

### 📊 Graphiques

| Commande | Description |
|----------|-------------|
| `/chart` | Afficher la liste des capteurs pour choisir un graphique |

### 🔔 Alertes

| Commande | Description |
|----------|-------------|
| `/alerts` | Voir les alertes actives |

---

## 📱 Utilisation Détaillée

### Consulter Vos Capteurs

1. Envoyez `/devices`
2. Une liste de vos capteurs s'affiche avec leur état
3. Cliquez sur un capteur pour voir ses détails

### Voir la Dernière Lecture

**Méthode 1 :** Utilisez la commande avec le DevEUI
```
/last A84041B4D1850ABC
```

**Méthode 2 :** Utilisez `/last` sans argument et sélectionnez un capteur dans la liste

### Afficher un Graphique

1. Envoyez `/chart` - une liste de vos capteurs s'affiche
2. Cliquez sur le capteur souhaité
3. Sélectionnez le type de données :
   - 🌡️ **Température** - Température du sol
   - 💧 **VWC** - Contenu volumétrique en eau
   - ⚡ **EC** - Conductivité électrique
4. Sélectionnez la période :
   - 6h, 12h, 24h, 48h
   - 7 jours, 30 jours

> 💡 **Astuce:** Vous n'avez plus besoin de mémoriser les DevEUI ! Cliquez simplement sur le nom du capteur.

### Notifications Automatiques

Une fois votre compte lié, vous recevrez automatiquement :
- ⚠️ **Alertes de seuil** - Quand une valeur dépasse les limites configurées
- 📡 **Alertes hors ligne** - Quand un capteur ne communique plus
- ✅ **Résolutions** - Quand une alerte est résolue

---

## 🔐 Sécurité

### Liaison de Compte

- Votre compte Telegram ne peut être lié qu'à **un seul** compte KropKontrol
- Un compte KropKontrol ne peut être lié qu'à **un seul** compte Telegram
- Utilisez `/unlink` pour délier votre compte si nécessaire

### Accès aux Données

- Vous ne pouvez voir que les capteurs de votre entreprise
- Les utilisateurs avec accès "Entreprise et Filiales" voient également les capteurs des filiales
- Les administrateurs (root) ont accès à tous les capteurs

### Accès Temporaire pour Support Technique

Une commande spéciale est disponible pour les administrateurs **Root** afin de générer un mot de passe temporaire (valide pour 20 minutes par défaut) et permettre à un technicien d'accéder à l'interface d'un utilisateur non-root.

```
/generatepassword [durée_en_minutes]
```

| Paramètre | Description |
|-----------|-------------|
| `durée_en_minutes` *(optionnel)* | Doit être compris entre 5 et 120 minutes. Si non fourni, la durée par défaut est 20 minutes. |

#### Étapes côté administrateur Root
1. Vérifier que le compte Telegram est lié (commande `/link`).
2. Exécuter `/generatepassword` (ex.: `/generatepassword 30`).
3. Copier le code généré (format `KK-XXXX-XXXX-XXXX`) et le transmettre au technicien par un canal sécurisé.
4. Supprimer le message pour éviter toute fuite de données.

#### Étapes côté technicien
1. Aller sur l'écran de connexion de l'application KropKontrol.
2. Entrer l'email du client (non-root) et le code temporaire dans le champ mot de passe.
3. Se connecter. Le code est à usage unique et sera immédiatement révoqué.

> ⚠️ Les codes temporaires ne fonctionnent pas pour les comptes Root et sont enregistrés dans la télémétrie (émission, consommation, échecs) pour audit.

---

## ❓ FAQ

### Je ne vois pas mes capteurs

1. Vérifiez que votre compte est bien lié avec `/link`
2. Vérifiez que vous utilisez les bonnes identifiants
3. Contactez votre administrateur si le problème persiste

### Le bot ne répond pas

1. Vérifiez votre connexion internet
2. Essayez d'envoyer `/start`
3. Si le problème persiste, le service peut être temporairement indisponible

### Comment changer de compte lié ?

1. Déliez votre compte actuel : `/unlink`
2. Liez le nouveau compte : `/link nouveau@email.com motDePasse`

### Mes alertes ne fonctionnent pas

Les alertes sont envoyées uniquement si :
- Votre compte est lié
- Vous avez accès au capteur concerné
- Des règles d'alerte sont configurées dans le système

---

## 📞 Support

Pour toute question ou problème :
- Contactez votre administrateur système
- Consultez la documentation en ligne

---

## 🔄 Mises à Jour

### Version Actuelle

- ✅ Liaison de compte sécurisée
- ✅ Consultation des capteurs personnels
- ✅ Graphiques en temps réel
- ✅ Notifications d'alertes automatiques
- ✅ Interface en français

---

*Documentation KropKontrol Bot Telegram - Version 1.0*
