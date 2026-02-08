import { createI18n } from 'vue-i18n'

const messages = {
  fr: {
    common: {
      loading: 'Chargement...',
      error: 'Erreur',
      success: 'Succès',
      cancel: 'Annuler',
      save: 'Enregistrer',
      delete: 'Supprimer',
      confirm: 'Confirmer',
      back: 'Retour',
      refresh: 'Actualiser',
      search: 'Rechercher',
      noData: 'Aucune donnée',
      offline: 'Hors connexion'
    },
    auth: {
      login: 'Connexion',
      logout: 'Déconnexion',
      email: 'Email',
      password: 'Mot de passe',
      loginWithTelegram: 'Entrer via Telegram',
      loginWithEmail: 'Connexion par email',
      notLinked: 'Compte non lié',
      linkAccount: 'Utilisez /link dans le bot Telegram',
      invalidCredentials: 'Identifiants incorrects',
      welcomeBack: 'Bon retour'
    },
    nav: {
      dashboard: 'Tableau de bord',
      devices: 'Capteurs',
      alerts: 'Alertes',
      support: 'Support',
      settings: 'Paramètres'
    },
    dashboard: {
      title: 'Tableau de bord',
      totalDevices: 'Total capteurs',
      online: 'En ligne',
      offline: 'Hors ligne',
      activeAlerts: 'Alertes actives',
      lastReadings: 'Dernières lectures',
      quickActions: 'Actions rapides'
    },
    devices: {
      title: 'Capteurs',
      searchPlaceholder: 'Rechercher un capteur...',
      noDevices: 'Aucun capteur trouvé',
      lastSeen: 'Dernière communication',
      battery: 'Batterie',
      temperature: 'Température',
      humidity: 'Humidité',
      vwc: 'VWC (Humidité sol)',
      ec: 'EC (Conductivité)',
      configureAlerts: 'Configurer alertes',
      viewHistory: 'Voir historique'
    },
    alerts: {
      title: 'Alertes',
      noAlerts: 'Aucune alerte active',
      acknowledge: 'Acquitter',
      mute: 'Silencier',
      muteAll: 'Tout silencier',
      critical: 'Critique',
      warning: 'Avertissement',
      info: 'Information'
    },
    support: {
      title: 'Support',
      subtitle: 'Besoin d\'aide ?',
      messagePlaceholder: 'Décrivez votre problème...',
      send: 'Envoyer',
      sent: 'Message envoyé !',
      minLength: 'Message trop court (min 10 caractères)',
      maxLength: 'Message trop long (max 5000 caractères)'
    },
    settings: {
      title: 'Paramètres',
      profile: 'Profil',
      appearance: 'Apparence',
      theme: 'Thème',
      themeDark: 'Sombre',
      themeLight: 'Clair',
      themeAuto: 'Automatique',
      language: 'Langue',
      notifications: 'Notifications',
      pushNotifications: 'Notifications push',
      hapticFeedback: 'Retour haptique',
      about: 'À propos',
      version: 'Version'
    },
    time: {
      justNow: 'À l\'instant',
      secondsAgo: 'Il y a {n} secondes',
      minuteAgo: 'Il y a 1 minute',
      minutesAgo: 'Il y a {n} minutes',
      hourAgo: 'Il y a 1 heure',
      hoursAgo: 'Il y a {n}h {m}m',
      dayAgo: 'Il y a 1 jour',
      daysAgo: 'Il y a {n} jours'
    }
  },
  en: {
    common: {
      loading: 'Loading...',
      error: 'Error',
      success: 'Success',
      cancel: 'Cancel',
      save: 'Save',
      delete: 'Delete',
      confirm: 'Confirm',
      back: 'Back',
      refresh: 'Refresh',
      search: 'Search',
      noData: 'No data',
      offline: 'Offline'
    },
    auth: {
      login: 'Login',
      logout: 'Logout',
      email: 'Email',
      password: 'Password',
      loginWithTelegram: 'Enter via Telegram',
      loginWithEmail: 'Login with email',
      notLinked: 'Account not linked',
      linkAccount: 'Use /link in the Telegram bot',
      invalidCredentials: 'Invalid credentials',
      welcomeBack: 'Welcome back'
    },
    nav: {
      dashboard: 'Dashboard',
      devices: 'Sensors',
      alerts: 'Alerts',
      support: 'Support',
      settings: 'Settings'
    },
    dashboard: {
      title: 'Dashboard',
      totalDevices: 'Total sensors',
      online: 'Online',
      offline: 'Offline',
      activeAlerts: 'Active alerts',
      lastReadings: 'Last readings',
      quickActions: 'Quick actions'
    },
    devices: {
      title: 'Sensors',
      searchPlaceholder: 'Search sensor...',
      noDevices: 'No sensors found',
      lastSeen: 'Last seen',
      battery: 'Battery',
      temperature: 'Temperature',
      humidity: 'Humidity',
      vwc: 'VWC (Soil moisture)',
      ec: 'EC (Conductivity)',
      configureAlerts: 'Configure alerts',
      viewHistory: 'View history'
    },
    alerts: {
      title: 'Alerts',
      noAlerts: 'No active alerts',
      acknowledge: 'Acknowledge',
      mute: 'Mute',
      muteAll: 'Mute all',
      critical: 'Critical',
      warning: 'Warning',
      info: 'Information'
    },
    support: {
      title: 'Support',
      subtitle: 'Need help?',
      messagePlaceholder: 'Describe your issue...',
      send: 'Send',
      sent: 'Message sent!',
      minLength: 'Message too short (min 10 characters)',
      maxLength: 'Message too long (max 5000 characters)'
    },
    settings: {
      title: 'Settings',
      profile: 'Profile',
      appearance: 'Appearance',
      theme: 'Theme',
      themeDark: 'Dark',
      themeLight: 'Light',
      themeAuto: 'Auto',
      language: 'Language',
      notifications: 'Notifications',
      pushNotifications: 'Push notifications',
      hapticFeedback: 'Haptic feedback',
      about: 'About',
      version: 'Version'
    },
    time: {
      justNow: 'Just now',
      secondsAgo: '{n} seconds ago',
      minuteAgo: '1 minute ago',
      minutesAgo: '{n} minutes ago',
      hourAgo: '1 hour ago',
      hoursAgo: '{n}h {m}m ago',
      dayAgo: '1 day ago',
      daysAgo: '{n} days ago'
    }
  },
  pt: {
    common: {
      loading: 'Carregando...',
      error: 'Erro',
      success: 'Sucesso',
      cancel: 'Cancelar',
      save: 'Guardar',
      delete: 'Eliminar',
      confirm: 'Confirmar',
      back: 'Voltar',
      refresh: 'Atualizar',
      search: 'Pesquisar',
      noData: 'Sem dados',
      offline: 'Sem ligação'
    },
    auth: {
      login: 'Entrar',
      logout: 'Sair',
      email: 'Email',
      password: 'Palavra-passe',
      loginWithTelegram: 'Entrar via Telegram',
      loginWithEmail: 'Entrar com email',
      notLinked: 'Conta não vinculada',
      linkAccount: 'Use /link no bot Telegram',
      invalidCredentials: 'Credenciais inválidas',
      welcomeBack: 'Bem-vindo de volta'
    },
    nav: {
      dashboard: 'Painel',
      devices: 'Sensores',
      alerts: 'Alertas',
      support: 'Suporte',
      settings: 'Definições'
    },
    dashboard: {
      title: 'Painel',
      totalDevices: 'Total sensores',
      online: 'Online',
      offline: 'Offline',
      activeAlerts: 'Alertas ativos',
      lastReadings: 'Últimas leituras',
      quickActions: 'Ações rápidas'
    },
    devices: {
      title: 'Sensores',
      searchPlaceholder: 'Pesquisar sensor...',
      noDevices: 'Nenhum sensor encontrado',
      lastSeen: 'Última comunicação',
      battery: 'Bateria',
      temperature: 'Temperatura',
      humidity: 'Humidade',
      vwc: 'VWC (Humidade solo)',
      ec: 'EC (Condutividade)',
      configureAlerts: 'Configurar alertas',
      viewHistory: 'Ver histórico'
    },
    alerts: {
      title: 'Alertas',
      noAlerts: 'Sem alertas ativos',
      acknowledge: 'Reconhecer',
      mute: 'Silenciar',
      muteAll: 'Silenciar todos',
      critical: 'Crítico',
      warning: 'Aviso',
      info: 'Informação'
    },
    support: {
      title: 'Suporte',
      subtitle: 'Precisa de ajuda?',
      messagePlaceholder: 'Descreva o seu problema...',
      send: 'Enviar',
      sent: 'Mensagem enviada!',
      minLength: 'Mensagem muito curta (mín 10 caracteres)',
      maxLength: 'Mensagem muito longa (máx 5000 caracteres)'
    },
    settings: {
      title: 'Definições',
      profile: 'Perfil',
      appearance: 'Aparência',
      theme: 'Tema',
      themeDark: 'Escuro',
      themeLight: 'Claro',
      themeAuto: 'Automático',
      language: 'Idioma',
      notifications: 'Notificações',
      pushNotifications: 'Notificações push',
      hapticFeedback: 'Feedback háptico',
      about: 'Sobre',
      version: 'Versão'
    },
    time: {
      justNow: 'Agora mesmo',
      secondsAgo: 'Há {n} segundos',
      minuteAgo: 'Há 1 minuto',
      minutesAgo: 'Há {n} minutos',
      hourAgo: 'Há 1 hora',
      hoursAgo: 'Há {n}h {m}m',
      dayAgo: 'Há 1 dia',
      daysAgo: 'Há {n} dias'
    }
  }
}

const i18n = createI18n({
  legacy: false,
  locale: localStorage.getItem('kk_language') || 'fr',
  fallbackLocale: 'fr',
  messages
})

export default i18n
