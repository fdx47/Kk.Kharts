<template>
  <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
    <div class="kk-modal__content">
      <div class="kk-modal__header">
        <h3 class="modal-title">Configuration des alertes</h3>
        <q-btn flat round icon="mdi-close" @click="close" />
      </div>

      <div class="kk-modal__body">
        <div v-for="(threshold, index) in localThresholds" :key="index" class="threshold-item">
          <div class="threshold-header">
            <q-select
              v-model="threshold.variable"
              :options="variableOptions"
              label="Variable"
              dense
              outlined
              class="kk-input"
            />
            <q-btn flat round icon="mdi-delete" color="negative" @click="removeThreshold(index)" />
          </div>

          <div class="threshold-values">
            <q-input
              v-model.number="threshold.minValue"
              type="number"
              label="Min"
              dense
              outlined
              class="kk-input"
            />
            <q-input
              v-model.number="threshold.maxValue"
              type="number"
              label="Max"
              dense
              outlined
              class="kk-input"
            />
          </div>

          <div class="threshold-period">
            <p class="period-label">Période active</p>
            <div class="period-times">
              <q-input
                v-model="threshold.startTime"
                type="time"
                label="Début"
                dense
                outlined
                class="kk-input"
              />
              <q-input
                v-model="threshold.endTime"
                type="time"
                label="Fin"
                dense
                outlined
                class="kk-input"
              />
            </div>
          </div>

          <q-toggle
            v-model="threshold.enabled"
            label="Activer cette alerte"
            color="primary"
          />
        </div>

        <q-btn
          flat
          color="primary"
          icon="mdi-plus"
          label="Ajouter une alerte"
          class="full-width mt-4"
          @click="addThreshold"
        />
      </div>

      <div class="kk-modal__footer">
        <q-btn flat label="Annuler" @click="close" />
        <q-btn
          unelevated
          color="primary"
          label="Enregistrer"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface Threshold {
  variable: string
  minValue: number | null
  maxValue: number | null
  startTime: string
  endTime: string
  enabled: boolean
}

const props = defineProps<{
  modelValue: boolean
  devEui: string
  thresholds: any[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  save: [thresholds: Threshold[]]
}>()

const localThresholds = ref<Threshold[]>([])
const saving = ref(false)

const variableOptions = [
  { label: 'Température', value: 'temperature' },
  { label: 'Humidité', value: 'humidity' },
  { label: 'VWC (Humidité sol)', value: 'vwc' },
  { label: 'EC (Conductivité)', value: 'ec' }
]

watch(() => props.thresholds, (newVal) => {
  localThresholds.value = newVal.map((t: any) => ({
    variable: t.variable || 'temperature',
    minValue: t.minValue ?? null,
    maxValue: t.maxValue ?? null,
    startTime: t.startTime || '00:00',
    endTime: t.endTime || '23:59',
    enabled: t.enabled !== false
  }))
}, { immediate: true })

function addThreshold() {
  localThresholds.value.push({
    variable: 'temperature',
    minValue: null,
    maxValue: null,
    startTime: '00:00',
    endTime: '23:59',
    enabled: true
  })
}

function removeThreshold(index: number) {
  localThresholds.value.splice(index, 1)
}

function close() {
  emit('update:modelValue', false)
}

async function save() {
  saving.value = true
  try {
    emit('save', localThresholds.value)
  } finally {
    saving.value = false
  }
}
</script>

<style lang="scss" scoped>
.kk-modal__content {
  background: #ffffff;
  border-radius: 20px 20px 0 0;
  max-height: 90vh;
  overflow-y: auto;
  min-width: 320px;
  color: #1e293b;
}

.kk-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);

  .modal-title {
    font-size: 18px;
    font-weight: 700;
    margin: 0;
    color: #1e293b;
  }
}

.kk-modal__body {
  padding: 20px;
}

.kk-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.threshold-item {
  background: #f1f5f9;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 16px;
}

.threshold-header {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  margin-bottom: 12px;

  .kk-input {
    flex: 1;
  }
}

.threshold-values {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 12px;
}

.threshold-period {
  margin-bottom: 12px;

  .period-label {
    font-size: 12px;
    opacity: 0.6;
    margin-bottom: 8px;
  }

  .period-times {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
  }
}

.mt-4 {
  margin-top: 16px;
}

.full-width {
  width: 100%;
}
</style>
