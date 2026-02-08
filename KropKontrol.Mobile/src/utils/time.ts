

export function formatTimeAgo(date) {
    if (!date) return 'Jamais'

    const past = new Date(date)
    if (Number.isNaN(past.getTime())) return 'Jamais'

    const diffMs = Date.now() - past.getTime()
    const seconds = Math.floor(diffMs / 1000)
    const minutes = Math.floor(seconds / 60)
    const hours = Math.floor(minutes / 60)
    const days = Math.floor(hours / 24)
    const months = Math.floor(days / 30)
    const years = Math.floor(days / 365)

    if (minutes < 1) return `Il y a ${seconds} secondes`
    if (minutes < 2) return 'Il y a 1 minute'
    if (hours < 1) return `Il y a ${minutes} minutes`
    if (hours < 24) return `Il y a ${hours}h ${minutes % 60 < 10 ? '0' : ''}${minutes % 60}m`
    if (days < 2) return 'Il y a 1 jour'
    if (days < 30) return `Il y a ${days} jours`
    if (days < 60) return 'Il y a 1 mois'
    if (days < 365) return `Il y a ${months} mois`
    if (days < 730) return 'Il y a 1 an'
    return `Il y a ${years} ans`
}

