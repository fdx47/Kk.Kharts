import { ref } from 'vue'

const toasts = ref([])
let toastId = 0

export const useToast = () => {
  const show = (message, type = 'success', duration = 3000) => {
    const id = toastId++
    const toast = {
      id,
      message,
      type,
      visible: true
    }

    toasts.value.push(toast)

    if (duration > 0) {
      setTimeout(() => {
        removeToast(id)
      }, duration)
    }

    return id
  }

  const success = (message, duration = 3000) => show(message, 'success', duration)
  const error = (message, duration = 5000) => show(message, 'error', duration)
  const warning = (message, duration = 4000) => show(message, 'warning', duration)
  const info = (message, duration = 3000) => show(message, 'info', duration)

  const removeToast = (id) => {
    const index = toasts.value.findIndex(t => t.id === id)
    if (index > -1) {
      toasts.value.splice(index, 1)
    }
  }

  const clear = () => {
    toasts.value = []
  }

  return {
    toasts,
    show,
    success,
    error,
    warning,
    info,
    removeToast,
    clear
  }
}
