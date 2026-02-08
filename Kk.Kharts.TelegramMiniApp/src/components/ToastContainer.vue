<template>
  <div class="fixed bottom-4 right-4 z-50 space-y-2 max-w-sm">
    <transition-group name="toast" tag="div">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['toast', `toast-${toast.type}`]"
        role="alert"
      >
        <div class="flex items-center gap-3">
          <span class="text-xl">{{ getIcon(toast.type) }}</span>
          <span class="flex-1">{{ toast.message }}</span>
          <button
            class="text-lg opacity-70 hover:opacity-100"
            @click="removeToast(toast.id)"
          >
            ✕
          </button>
        </div>
      </div>
    </transition-group>
  </div>
</template>

<script setup>
import { useToast } from '../services/toastService'

const { toasts, removeToast } = useToast()

const getIcon = (type) => {
  const icons = {
    success: '✓',
    error: '✕',
    warning: '⚠',
    info: 'ℹ'
  }
  return icons[type] || 'ℹ'
}
</script>

<style scoped>
.toast {
  padding: 1rem;
  border-radius: 0.75rem;
  backdrop-filter: blur(10px);
  animation: slideIn 0.3s ease-out;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.toast-success {
  background: rgba(34, 197, 94, 0.9);
  color: white;
  border: 1px solid rgba(34, 197, 94, 1);
}

.toast-error {
  background: rgba(239, 68, 68, 0.9);
  color: white;
  border: 1px solid rgba(239, 68, 68, 1);
}

.toast-warning {
  background: rgba(234, 179, 8, 0.9);
  color: white;
  border: 1px solid rgba(234, 179, 8, 1);
}

.toast-info {
  background: rgba(59, 130, 246, 0.9);
  color: white;
  border: 1px solid rgba(59, 130, 246, 1);
}

@keyframes slideIn {
  from {
    transform: translateX(400px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  transform: translateX(400px);
  opacity: 0;
}

.toast-leave-to {
  transform: translateX(400px);
  opacity: 0;
}
</style>
