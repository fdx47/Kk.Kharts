<template>
  <div class="access-denied">
    <h1>Acesso Negado</h1>
    <p>Nào tem permissões suficientes para aceder a esta página.</p>
    <p>
      Requer o papel: <strong>{{ requiredRole }}</strong>
    </p>
    <router-link to="/">Voltar à página inicial</router-link>
  </div>
</template>

<script>
export default {
  props: ["role"],
  computed: {
    requiredRole() {
      return this.role || "Administrador";
    },
  },
};
</script>

<style scoped>
.access-denied {
  text-align: center;
  padding: 2rem;
}
</style>
