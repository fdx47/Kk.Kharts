<template>
  <DefaultLayout
    :hideSidebar="true"
    @logout="logout"
    showBack
    @back="goToUsersPage"
  >
    <template #title>Modifier l'utilisateur</template>
    <div class="container py-4">
      <h2>Modifier utilisateur #{{ id }}</h2>

      <form @submit.prevent="handleUpdate" class="mt-4">
        <div class="mb-3">
          <label>Email</label>
          <input v-model="email" type="email" class="form-control" required />
        </div>
        <div class="mb-3">
          <label>Nouveau mot de passe</label>
          <input v-model="password" type="password" class="form-control" />
        </div>
        <button class="btn btn-primary" type="submit">Mettre à jour</button>
      </form>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuth } from "@/composables/useAuth.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { fetchWithAuth } from "@/services/authService.js";

const { logout } = useAuth();
const route = useRoute();
const router = useRouter();

const id = route.params.id;
const email = ref("");
const password = ref("");

function goToUsersPage() {
  router.push({ name: "Users" });
}

onMounted(async () => {
  const res = await fetchWithAuth(
    `https://kropkontrol.premiumasp.net/api/v1/users/${id}`,
  );
  const data = await res.json();
  email.value = data.email;
});

async function handleUpdate() {
  const response = await fetchWithAuth(
    `https://kropkontrol.premiumasp.net/api/v1/users/${id}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email: email.value,
        password: password.value || null,
      }),
    },
  );

  if (response.ok) {
    alert("Utilisateur mis à jour");
    router.push({ name: "UserList" });
  } else {
    const errorData = await response.json();

    // Si l'API a retourné des erreurs de validation
    if (errorData.errors) {
      //Regroupe tous les messages en une seule chaîne
      const messages = Object.values(errorData.errors).flat().join("\n");
      alert(`Erreur(s):\n${messages}`);
    } else if (errorData.message) {
      alert(`Erreur: ${errorData.message}`);
    } else {
      alert("Une erreur inconnue est survenue.");
    }
  }
}
</script>
