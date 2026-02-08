<!-- src/components/LazySnapshotCard.vue -->
<template>
  <div ref="observerEl" class="lazy-chart-card">
    <SnapshotCard v-if="isVisible" ref="snapshotRef" v-bind="props" />
  </div>
</template>

<script setup>
import {
  ref,
  onMounted,
  onUnmounted,
  defineAsyncComponent,
  watch,
  nextTick,
} from "vue";

const SnapshotCard = defineAsyncComponent(() => import("./SnapshotCard.vue"));

const props = defineProps({
  devEui: String,
  title: String,
  labelMap: Object,
  variables: { type: Array, default: () => [] },
  device: { type: Object, default: () => ({}) },
  intervalDays: { type: Number, default: undefined },
  startDate: { type: String, default: undefined },
  endDate: { type: String, default: undefined },
  startDateTime: { type: [String, Date], default: undefined },
  endDateTime: { type: [String, Date], default: undefined },
});

const isVisible = ref(false);
const observerEl = ref(null);
const snapshotRef = ref(null);
let observer;

// Met à jour le graphique dès que la carte devient visible
watch(
  isVisible,
  async (visible) => {
    if (visible && snapshotRef.value) {
      // s'assurer que le composant enfant est monté
      await nextTick();
      snapshotRef.value.updateChart();
    }
  },
  { immediate: true },
);

// Relance une mise à jour quand le device devient disponible
watch(
  () => props.device,
  async () => {
    if (isVisible.value && snapshotRef.value) {
      await nextTick();
      snapshotRef.value.updateChart();
    }
  },
);

onMounted(() => {
  const root = observerEl.value?.closest(".content") || null;
  observer = new IntersectionObserver(
    (entries) => {
      if (entries[0].isIntersecting) {
        isVisible.value = true;
        observer.disconnect();
      }
    },
    { root },
  );
  if (observerEl.value) observer.observe(observerEl.value);
});

onUnmounted(() => {
  if (observer) observer.disconnect();
});
</script>

<style scoped>
.lazy-chart-card {
  min-height: 200px;
}
</style>
