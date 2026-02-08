<template>
  <DefaultLayout showBack @back="goLanding" @logout="logout">
    <template #title>Analyse Fertilisation</template>

    <div class="fertilisation-page p-4">
      <!-- Upload des fichiers PDF par type -->
      <div class="row mb-4" v-for="type in pdfTypes" :key="type">
        <div class="col-3 d-flex align-items-center">
          <label :for="`file-${type}`" class="form-label mb-0">{{
            type
          }}</label>
        </div>
        <div class="col-9">
          <input
            :id="`file-${type}`"
            type="file"
            accept="application/pdf"
            class="form-control"
            @change="onFileChange($event, type)"
            :disabled="isProcessing"
          />
        </div>
      </div>

      <!-- Bouton de traitement -->
      <div class="mb-5">
        <button
          class="btn btn-success"
          @click="processAll"
          :disabled="isProcessing"
        >
          {{ isProcessing ? "Analyse en cours..." : "Lancer l'analyse" }}
        </button>
      </div>

      <!-- Barre de progression -->
      <div v-if="isProcessing" class="mb-4">
        <div class="progress">
          <div
            class="progress-bar"
            role="progressbar"
            :style="{ width: progressValue + '%' }"
            :aria-valuenow="Math.round(progressValue)"
            aria-valuemin="0"
            aria-valuemax="100"
          >
            {{ Math.round(progressValue) }}%
          </div>
        </div>
      </div>

      <!-- Tableau récapitulatif -->
      <div v-if="!isProcessing && extractedData.length">
        <h4 class="mt-4 mb-3">Résultats extraits</h4>
        <div class="table-responsive">
          <table class="table table-bordered bg-white fertilisation-table">
            <thead>
              <tr>
                <th rowspan="2">Type</th>
                <th colspan="2" class="table-section">Qualité d’eau</th>
                <th colspan="11" class="table-section">Macroéléments</th>
                <th colspan="7" class="table-section">Microéléments</th>
              </tr>
              <tr>
                <th>EC</th>
                <th>pH</th>
                <th>NH₄</th>
                <th>K</th>
                <th>Na</th>
                <th>Ca</th>
                <th>K/Ca</th>
                <th>Mg</th>
                <th>NO₃</th>
                <th>Cl</th>
                <th>S</th>
                <th>HCO₃</th>
                <th>P</th>
                <th>Fe</th>
                <th>Mn</th>
                <th>Zn</th>
                <th>B</th>
                <th>Cu</th>
                <th>Mo</th>
                <th>Al</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="result in extractedData" :key="result.type">
                <td>{{ result.type }}</td>
                <td>{{ result.info.EC ?? "-" }}</td>
                <td>{{ result.info.pH ?? "-" }}</td>
                <td>{{ result.info.NH4 ?? "-" }}</td>
                <td>{{ result.info.K ?? "-" }}</td>
                <td>{{ result.info.Na ?? "-" }}</td>
                <td>{{ result.info.Ca ?? "-" }}</td>
                <td>{{ result.info["K/Ca"] ?? "-" }}</td>
                <td>{{ result.info.Mg ?? "-" }}</td>
                <td>{{ result.info.NO3 ?? "-" }}</td>
                <td>{{ result.info.Cl ?? "-" }}</td>
                <td>{{ result.info.S ?? "-" }}</td>
                <td>{{ result.info.HCO3 ?? "-" }}</td>
                <td>{{ result.info.P ?? "-" }}</td>
                <td>{{ result.info.Fe ?? "-" }}</td>
                <td>{{ result.info.Mn ?? "-" }}</td>
                <td>{{ result.info.Zn ?? "-" }}</td>
                <td>{{ result.info.B ?? "-" }}</td>
                <td>{{ result.info.Cu ?? "-" }}</td>
                <td>{{ result.info.Mo ?? "-" }}</td>
                <td>{{ result.info.Al ?? "-" }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <p v-else-if="!isProcessing" class="text-muted">
        Aucune donnée analysée pour le moment.
      </p>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref } from "vue";
import { useNavigation } from "../composables/useNavigation.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { usePdfAnalysisWater } from "@/composables/usePdfAnalysisWater.js";
import { useAuth } from "../composables/useAuth.js";

// Navigation
const { goLanding } = useNavigation();
const { logout } = useAuth();

// Types de PDF à gérer
const pdfTypes = [
  "Apport",
  "Drain",
  "Substrat",
  "Jeune feuille",
  "Vieille feuille",
];
const rawFiles = ref({});
const isProcessing = ref(false);
const progressValue = ref(0);

// Composable d'analyse PDF
const { extractedData, chartData, processFiles } = usePdfAnalysisWater();

// Gestion du changement de fichier
function onFileChange(event, type) {
  const file = event.target.files[0];
  if (file && file.type === "application/pdf" && !isProcessing.value) {
    rawFiles.value[type] = file;
  }
}

// Lance l'analyse de tous les PDFs chargés
function processAll() {
  isProcessing.value = true;
  processFiles(rawFiles.value, (pct) => {
    progressValue.value = pct;
  }).finally(() => {
    isProcessing.value = false;
  });
}
</script>

<style scoped>
.fertilisation-page {
  max-width: 900px;
  margin: 0 auto;
  overflow-x: hidden;
  padding: 1.7rem 1.5rem 1rem 1.5rem;
}

@media (max-width: 991px) {
  .fertilisation-page {
    max-width: 98vw;
    padding: 1rem 0.3rem;
  }
}

/* Responsive pour la grid formulaire */
.row {
  margin-left: 0;
  margin-right: 0;
}
@media (max-width: 767.98px) {
  .row {
    display: flex;
    flex-direction: column;
    gap: 0.8rem;
  }
  .col-3,
  .col-9 {
    width: 100% !important;
    flex: 0 0 100%;
    max-width: 100%;
    padding-left: 0 !important;
    padding-right: 0 !important;
  }
  .fertilisation-page {
    padding: 0.8rem 0.1rem 0.2rem 0.1rem;
  }
}

/* Table responsive */
.table-responsive {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  margin-bottom: 0.7rem;
}

.fertilisation-table {
  width: 1100px; /* min width to allow scrolling on mobile */
  min-width: 900px;
  max-width: 100vw;
  table-layout: fixed;
  font-size: 0.95rem;
  border-collapse: collapse;
}

@media (max-width: 1200px) {
  .fertilisation-table {
    width: 900px;
    font-size: 0.93rem;
  }
}
@media (max-width: 767px) {
  .fertilisation-table {
    width: 720px;
    font-size: 0.91rem;
  }
}

/* Les cellules restent lisibles même sur mobile */
.fertilisation-table th,
.fertilisation-table td {
  white-space: normal !important;
  padding: 0.3rem 0.13rem;
  text-align: center;
  word-break: break-word;
  font-size: inherit;
}
.fertilisation-table th {
  font-size: 1.02em;
}

.table-section {
  background: #eafaf3;
  color: #198754;
  font-weight: bold;
  border-bottom: 2px solid #26b673;
}

/* Adaptation boutons et textes */
.btn-success,
.btn {
  font-size: 1rem;
  padding: 0.47rem 1.2rem;
}
@media (max-width: 600px) {
  .btn,
  .btn-success {
    width: 100%;
    font-size: 1.04rem;
    padding: 0.5rem 0.1rem;
  }
}

h4 {
  font-size: 1.23rem;
  font-weight: 600;
  color: #26b673;
}
@media (max-width: 767px) {
  h4 {
    font-size: 1.1rem;
  }
}

.progress {
  height: 1.25rem;
  font-size: 1rem;
}

.text-muted {
  font-size: 1rem;
}

.placeholder-message {
  font-size: 1.12rem;
  color: #26b673;
  min-height: 35vh;
}

@media (max-width: 600px) {
  .placeholder-message {
    font-size: 1rem;
    min-height: 15vh;
  }
}
:deep(.sidebar) {
  display: none !important;
}
:deep(.toggle-devices-btn) {
  display: none !important;
}
</style>
