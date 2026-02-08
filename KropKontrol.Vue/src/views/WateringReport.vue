<template>
  <DefaultLayout
    :devices="devices"
    :selectedDevice="selectedDevice"
    showBack
    @select-device="onSelectDevice"
    @back="goLanding"
    @logout="logout"
  >
    <template #title>Rapport d'arrosage - {{ startDate }} -> {{ endDate }}</template>

    <div class="container py-2">
      <div class="row g-2 align-items-end">
        <div class="col-12 col-md-3">
          <label class="form-label">Début</label>
          <input type="date" class="form-control" v-model="startTmp" />
        </div>
        <div class="col-12 col-md-3">
          <label class="form-label">Fin</label>
          <input type="date" class="form-control" v-model="endTmp" />
        </div>
        <div class="col-12 col-md-3">
          <button class="btn btn-success w-100" @click="applyRange" :disabled="!devEui">
            Générer le rapport
          </button>
        </div>
      </div>
    </div>

    <WateringReportPanel :devEui="devEui" :startDate="startDate" :endDate="endDate" />
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useNavigation } from '../composables/useNavigation.js'
import DefaultLayout from '@/views/layout/DefaultLayout.vue'
import { useDevices } from '@/composables/useDevices.js'
import { useAuth } from '../composables/useAuth.js'
import { useVirtualDevices } from '@/composables/useVirtualDevices.js'
import WateringReportPanel from '@/components/WateringReportPanel.vue'

const { goLanding } = useNavigation()
const { logout } = useAuth()

const { devices: fetchedDevices, loadDevices } = useDevices()
const { virtualDevices } = useVirtualDevices()
const devices = computed(() =>
  [...fetchedDevices.value, ...virtualDevices.value].filter(d => Number(d.model ?? d.deviceId) === 47)
)
const devEui = ref('')

const today = new Date()
const d7 = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 6)
const toYmd = d => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`
const startDate = ref(toYmd(d7))
const endDate = ref(toYmd(today))
const startTmp = ref(startDate.value)
const endTmp = ref(endDate.value)

const selectedDevice = computed(() => devices.value.find(d => d.devEui === devEui.value) || null)
function onSelectDevice(dev) { devEui.value = dev.devEui }
function applyRange() {
  if (!startTmp.value || !endTmp.value) return
  if (new Date(startTmp.value) > new Date(endTmp.value)) return
  startDate.value = startTmp.value
  endDate.value = endTmp.value
}

onMounted(async () => {
  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
    await loadDevices().catch(() => {})
  }
})
</script>
