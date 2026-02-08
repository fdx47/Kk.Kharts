<template>
  <div class="space-y-3">
    <div v-for="n in count" :key="n" class="tg-card animate-pulse">
      <div v-if="type === 'device'" class="flex items-center justify-between gap-3">
        <div class="flex items-center gap-3 flex-1">
          <div class="w-4 h-4 rounded-full bg-tg-secondary-bg"></div>
          <div class="flex-1">
            <div class="h-4 bg-tg-secondary-bg rounded w-3/4 mb-2"></div>
            <div class="h-3 bg-tg-secondary-bg rounded w-1/2"></div>
          </div>
        </div>
        <div class="text-right">
          <div class="h-4 bg-tg-secondary-bg rounded w-16 mb-2"></div>
          <div class="h-3 bg-tg-secondary-bg rounded w-12"></div>
        </div>
      </div>
      
      <div v-else-if="type === 'metric'" class="text-center">
        <div class="h-4 bg-tg-secondary-bg rounded w-1/3 mx-auto mb-2"></div>
        <div class="h-8 bg-tg-secondary-bg rounded w-1/2 mx-auto mb-2"></div>
        <div class="h-3 bg-tg-secondary-bg rounded w-1/4 mx-auto"></div>
      </div>
      
      <div v-else-if="type === 'alert'" class="space-y-2">
        <div class="flex items-center gap-2">
          <div class="w-3 h-3 rounded-full bg-tg-secondary-bg"></div>
          <div class="h-4 bg-tg-secondary-bg rounded flex-1"></div>
        </div>
        <div class="h-3 bg-tg-secondary-bg rounded w-2/3"></div>
      </div>
      
      <div v-else class="space-y-2">
        <div class="h-4 bg-tg-secondary-bg rounded w-full"></div>
        <div class="h-4 bg-tg-secondary-bg rounded w-5/6"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  count: {
    type: Number,
    default: 3
  },
  type: {
    type: String,
    enum: ['device', 'metric', 'alert', 'generic'],
    default: 'generic'
  }
})
</script>

<style scoped>
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
