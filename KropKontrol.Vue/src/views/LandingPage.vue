ï»¿<!-- src/views/LandingPage.vue -->
<template>
  <DefaultLayout
    :hideSidebar="true"
    showBack
    @back="goSnapshot"
    @logout="logout"
  >
    <template #title> KropKontrol IoT Dashboard </template>

    <!-- Grille personnalisÃ©e centrÃ©e -->
    <TileLayout
      class="py-4"
      row-class="justify-content-center align-items-start"
    >
      <!-- Colonne 1 -->
      <div class="col-12 col-md-4 d-flex flex-column gap-4">
        <div class="card tile h-100" @click="goTo('Dashboard')">
          <div class="card-body text-center">
            <i
              class="bi bi-bar-chart-line tile-icon"
              style="color: #82be20ff"
            ></i>
            <h5 class="card-title mt-2">Kapteurs Agrométéo</h5>
            <div class="small text-muted mt-1">
              Visualisez les données de vos sondes
            </div>
          </div>
        </div>
        <div class="card tile h-100" @click="goTo('DataExport')">
          <div class="card-body text-center">
            <i
              class="bi bi-download tile-icon"
              style="color: #82be20ff"
            ></i>
            <h5 class="card-title mt-2">Export CSV</h5>
            <div class="small text-muted mt-1">
              Téléchargez vos mesures au format CSV.
            </div>
          </div>
        </div>
      </div>

      <!-- Colonne 2 -->
      <div class="col-12 col-md-4 d-flex flex-column gap-4">
        <div class="card tile h-100" @click="goTo('SpecificChart')">
          <div class="card-body text-center">
            <i
              class="bi bi-graph-up-arrow tile-icon"
              style="color: #82be20ff"
            ></i>
            <h5 class="card-title mt-2">Graphiks personnalisés</h5>
            <div class="small text-muted mt-1">
              Créez et visualisez vos graphiques.
            </div>
          </div>
        </div>
        <div class="card tile h-100" @click="goTo('WateringReport')">
          <div class="card-body text-center">
            <i
              class="bi bi-droplet-half tile-icon"
              style="color: #82be20ff"
            ></i>
            <h5 class="card-title mt-2">Rapport d'arrosage</h5>
            <div class="small text-muted mt-1">
              Génère un résumé des irrigations sur une période personnalisée.
            </div>
          </div>
        </div>
      </div>

      <!-- Colonne 3 -->
      <div class="col-12 col-md-4 d-flex flex-column gap-4">
        <div class="card tile h-100" @click="goTo('Users')">
          <div class="card-body text-center">
            <i class="bi bi-gear tile-icon" style="color: #82be20ff"></i>
            <h5 class="card-title mt-2">Konfiguration</h5>
            <div class="small text-muted mt-1">Utilisateurs et Kapteurs</div>
          </div>
        </div>
        <div
          class="card tile h-100"
          v-if="canAccessKonfigurator"
          @click="goTo('KropKonfigurator')"
        >
          <div class="card-body text-center">
            <i class="bi bi-tools tile-icon" style="color: #82be20ff"></i>
            <h5 class="card-title mt-2">Krop Konfigurator</h5>
            <div class="small text-muted mt-1">
              Paramétrez vos parcelles et Kapteurs virtuels.
            </div>
          </div>
        </div>
      </div>
    </TileLayout>

    <!-- Footer -->
    <div class="login-footer-text text-center mb-3">
      <small class="text-muted">
        KropKontrol by
        <a
          href="https://3ctec.fr/"
          target="_blank"
          rel="noopener"
          class="text-decoration-none text-secondary"
        >
          3ctec
        </a>
        and
        <a
          href="http://stratberries.com"
          target="_blank"
          rel="noopener"
          class="text-decoration-none text-secondary"
        >
          StratBerries
        </a>
        â€” {{ currentYear }}
      </small>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "../composables/useAuth.js";
import TileLayout from "@/components/TileLayout.vue";
import { useNavigation } from "../composables/useNavigation.js";
import { isRootUser } from "../services/roleUtils.js";

const router = useRouter();
const { goSnapshot } = useNavigation();
const currentYear = ref(new Date().getFullYear());
const { logout } = useAuth();
const canAccessKonfigurator = ref(isRootUser());

function goTo(name) {
  router.push({ name });
}
</script>

<style scoped>
.landing-tiles {
  position: relative;
  z-index: 1;
  width: 100%;
  flex: 1 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  padding-bottom: 1rem;
}
.landing-tiles .col-md-4 {
  max-width: 420px;
}
.card-title {
  font-size: clamp(1.1rem, 2.5vw, 1.45rem);
  margin-top: 0.5rem;
}
.login-footer-text {
  position: relative;
  z-index: 1;
  font-size: 0.97rem;
  padding: 0.7rem 0.2rem;
  margin-bottom: 0.2rem;
  background: rgba(255, 255, 255, 0.91);
  width: 100%;
  box-sizing: border-box;
}
@media (max-width: 575.98px) {
  .card-title {
    font-size: 1.08rem;
  }
  .login-footer-text {
    font-size: 0.83rem;
    padding: 0.4rem 0.1rem;
  }
}
</style>
