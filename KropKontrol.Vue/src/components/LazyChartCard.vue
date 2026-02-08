<template>
  <div ref="observerEl" class="lazy-chart-card">
    <ChartCard
      v-if="isVisible"
      v-bind="{
        devEui,
        suffix,
        title,
        labelMap,
        variables,
        startDate,
        endDate,
        startDateTime,
        endDateTime,
        intervalDays,
        showIntervalControls,
        device,
        series,
        seriesLabels,
        sunAnnotationOffset,
        sunriseAnnotationOffset,
        sunsetAnnotationOffset,
        baseTime,
        showDrainage,
      }"
      @delete="$emit('delete')"
      @duplicate="$emit('duplicate')"
      @updated="(payload) => $emit('updated', payload)"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, defineAsyncComponent } from "vue";
const ChartCard = defineAsyncComponent(() => import("./ChartCard.vue"));

const props = defineProps({
  devEui: String,
  suffix: String,
  title: String,
  labelMap: Object,
  variables: { type: Array, default: () => [] },
  startDate: {
    type: String,
    default: () => new Date(Date.now() - 86400000).toISOString().slice(0, 10),
  },
  endDate: {
    type: String,
    default: () => new Date().toISOString().slice(0, 10),
  },
  startDateTime: { type: [String, Date], default: null },
  endDateTime: { type: [String, Date], default: null },
  intervalDays: { type: Number, default: () => 1.5 },
  showIntervalControls: { type: Boolean, default: undefined },
  device: { type: Object, default: () => ({}) },
  series: { type: Array, default: null },
  seriesLabels: { type: Array, default: () => [] },
  sunAnnotationOffset: { type: Number, default: 120 },
  sunriseAnnotationOffset: { type: Number, default: 120 },
  sunsetAnnotationOffset: { type: Number, default: 120 },
  baseTime: { type: [Date, String], default: null },
  showDrainage: { type: Boolean, default: false },
});

const emit = defineEmits(["delete", "duplicate", "updated"]);

const isVisible = ref(false);
const observerEl = ref(null);
let observer;

onMounted(() => {
  observer = new IntersectionObserver(
    (entries) => {
      if (entries[0].isIntersecting) {
        isVisible.value = true;
      } else {
        isVisible.value = false;
      }
    },
    { threshold: 0.1 },
  );
  if (observerEl.value) observer.observe(observerEl.value);
});

onUnmounted(() => {
  if (observer && observerEl.value) {
    observer.unobserve(observerEl.value);
    observer.disconnect();
  }
});
</script>

<style scoped>
.lazy-chart-card {
  min-height: 200px;
}
</style>
