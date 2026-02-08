<template>
  <DefaultLayout
    :hideSidebar="true"
    @logout="logout"
    showBack
    @back="goToUsersPage"
  >
    <template #title>Créer un utilisateur</template>
    <div class="container py-4">
      <h2>Ajouter un nouvel utilisateur</h2>
      <form @submit.prevent="handleSubmit" class="mt-4">
        <div class="mb-3">
          <label>Email</label>
          <input v-model="email" type="email" class="form-control" required />
        </div>
        <div class="mb-3">
          <label>Mot de passe</label>
          <input
            v-model="password"
            type="password"
            class="form-control"
            required
            minlength="6"
          />
        </div>
        <button class="btn btn-success" type="submit">Créer</button>
      </form>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuth } from "@/composables/useAuth.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { fetchWithAuth } from "@/services/authService.js";

function goToUsersPage() {
  router.push({ name: "Users" });
}

const email = ref("");
const password = ref("");
const router = useRouter();
const { logout } = useAuth();

const handleSubmit = async () => {
  // Enviar para backend (ajuste o endpoint conforme necessário)
  try {
    const res = await fetchWithAuth(
      "https://kropkontrol.premiumasp.net/api/v1/users",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email: email.value, password: password.value }),
      },
    );

    if (!res.ok) throw new Error("Erreur de création");

    alert("Utilisateur créé avec succès");
    router.push({ name: "UserList" });
  } catch (e) {
    alert("Erreur : " + e.message);
  }
};
</script>
