<template>
  <q-page class="error-page">
    <div class="error-content">
      <q-icon name="mdi-alert-circle-outline" size="80px" color="warning" />
      <h1>404</h1>
      <p>Page non trouvée</p>
      <q-btn
        unelevated
        color="primary"
        label="Retour à l'accueil"
        icon="mdi-home"
        @click="goHome"
      />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goHome() {
  router.push({ name: 'dashboard' })
}
</script>

<style lang="scss" scoped>
.error-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.error-content {
  text-align: center;

  h1 {
    font-size: 64px;
    font-weight: 800;
    margin: 24px 0 8px;
  }

  p {
    font-size: 18px;
    opacity: 0.6;
    margin: 0 0 32px;
  }
}
</style>
