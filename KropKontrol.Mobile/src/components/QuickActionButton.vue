<template>
  <button class="kk-quick-action" @click="$emit('click')">
    <div class="action-icon" :style="{ backgroundColor: `var(--q-${color})15`, color: `var(--q-${color})` }">
      <q-icon :name="icon" />
    </div>
    <span class="action-label">{{ label }}</span>
  </button>
</template>

<script setup lang="ts">
defineProps<{
  icon: string
  label: string
  color?: string
}>()

defineEmits<{
  click: []
}>()
</script>

<style lang="scss" scoped>
.kk-quick-action {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 12px 8px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.08);
  transition: all 0.2s ease;
  cursor: pointer;

  &:active {
    transform: scale(0.95);
    background: rgba(255, 255, 255, 0.08);
  }

  .action-icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
  }

  .action-label {
    font-size: 10px;
    font-weight: 600;
    opacity: 0.8;
    text-align: center;
    line-height: 1.2;
  }
}
</style>
