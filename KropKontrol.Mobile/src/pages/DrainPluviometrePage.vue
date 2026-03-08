<template>
  <q-page class="q-pa-md column gap-md">
    <q-card flat bordered>
      <q-card-section class="row q-col-gutter-md">
        <div class="col-12 col-md-3">
          <q-input v-model="form.devEui" label="DevEUI" dense outlined />
        </div>
        <div class="col-12 col-md-3">
          <q-input v-model="form.startDate" label="Début (UTC ou offset)" dense outlined type="datetime-local" />
        </div>
        <div class="col-12 col-md-3">
          <q-input v-model="form.endDate" label="Fin (UTC ou offset)" dense outlined type="datetime-local" />
        </div>
        <div class="col-12 col-md-3">
          <q-input v-model.number="form.waterUsedLiters" label="Eau utilisée (L)" dense outlined type="number" min="0" step="0.1" />
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn color="primary" label="Calculer" :loading="loading" @click="onCalculate" />
      </q-card-actions>
    </q-card>

    <q-card v-if="result" flat bordered>
      <q-card-section>
        <div class="text-h6">Résultat</div>
      </q-card-section>
      <q-separator />
      <q-card-section class="row q-col-gutter-md">
        <div class="col-12 col-md-3"><strong>DevEUI:</strong> {{ result.devEui }}</div>
        <div class="col-12 col-md-3"><strong>Intervalle:</strong> {{ result.startAt }} → {{ result.endAt }}</div>
        <div class="col-12 col-md-3"><strong>Pulses:</strong> {{ result.pulseCount }}</div>
        <div class="col-12 col-md-3"><strong>Val/pulse (L):</strong> {{ result.valuePerPulse }}</div>
        <div class="col-12 col-md-3"><strong>Eau utilisée (L):</strong> {{ result.waterUsedLiters }}</div>
        <div class="col-12 col-md-3"><strong>Mesuré (L):</strong> {{ result.measuredLiters }}</div>
        <div class="col-12 col-md-3"><strong>Drainage (L):</strong> {{ result.drainageLiters }}</div>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Notify } from 'quasar'
import { getDrainage } from '@/services/drainPluviometreApi'

interface DrainageResult {
  devEui: string
  startAt: string
  endAt: string
  waterUsedLiters: number
  valuePerPulse: number
  pulseCount: number
  measuredLiters: number
  drainageLiters: number
}

const form = ref({
  devEui: '',
  startDate: '',
  endDate: '',
  waterUsedLiters: 0
})

const loading = ref(false)
const result = ref<DrainageResult | null>(null)

async function onCalculate() {
  if (!form.value.devEui || !form.value.startDate || !form.value.endDate) {
    Notify.create({ type: 'warning', message: 'Preencha DevEUI, início e fim.' })
    return
  }

  loading.value = true
  result.value = null
  try {
    const response = await getDrainage(
      form.value.devEui.trim(),
      form.value.startDate,
      form.value.endDate,
      form.value.waterUsedLiters ?? 0
    )
    result.value = response.data as DrainageResult
  } catch (err: any) {
    const msg = err?.response?.data?.message || 'Erro ao calcular drenagem'
    Notify.create({ type: 'negative', message: msg })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.gap-md {
  row-gap: 12px;
  column-gap: 12px;
}
</style>
