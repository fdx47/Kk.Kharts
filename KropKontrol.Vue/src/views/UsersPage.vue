<!-- /src/views/UsersPage.vue-->
<template>
  <DefaultLayout
    :hideSidebar="true"
    @logout="logout"
    showBack
    @back="goToLandingPage"
  >
    <template #title>Gestion des utilisateurs et Kapteurs</template>

    <TileLayout class="py-4">
      <template #before-row>
        <h2 class="mb-4">Options Utilisateurs</h2>
      </template>
      <div
        class="col-12 col-md-4"
        v-for="option in userOptions"
        :key="option.name"
      >
        <div class="card tile" @click="goToOption(option.route)">
          <div class="card-body text-center">
            <i :class="[option.icon, 'tile-icon']" style="color: #82be20ff"></i>
            <h5 class="card-title mt-2">{{ option.title }}</h5>
            <p class="small text-muted mt-1">{{ option.description }}</p>
          </div>
        </div>
      </div>
    </TileLayout>

    <TileLayout class="py-4">
      <template #before-row>
        <h2 class="mb-4">Options Kapteurs</h2>
      </template>
      <div
        class="col-12 col-sm-6 col-md-4"
        v-for="option in kapteurOptions"
        :key="option.name"
      >
        <div class="card tile" @click="goToOption(option.route)">
          <div class="card-body text-center">
            <i :class="[option.icon, 'tile-icon']" style="color: #82be20ff"></i>
            <h5 class="card-title mt-2">{{ option.title }}</h5>
            <p class="small text-muted mt-1">{{ option.description }}</p>
          </div>
        </div>
      </div>
    </TileLayout>
  </DefaultLayout>
</template>

<script setup>
import { useRouter } from "vue-router";
import { useAuth } from "@/composables/useAuth.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { getUserIdFromToken } from "@/services/authService.js";
import TileLayout from "@/components/TileLayout.vue";

const router = useRouter();
const { logout } = useAuth();
const userId = getUserIdFromToken();

function goToLandingPage() {
  router.push({ name: "Landing" });
}

const userOptions = [
  {
    name: "create",
    title: "Créer un utilisateur",
    description: "Ajoutez un nouvel utilisateur",
    icon: "bi bi-person-plus",
    route: { name: "UserCreate" },
  },
  {
    name: "list",
    title: "Liste des utilisateurs",
    description: "Voir et gérer les utilisateurs existants",
    icon: "bi bi-people",
    route: { name: "UserList" },
  },
  {
    name: "settings",
    title: "Paramètres utilisateur",
    description: "Modifier les paramètres utilisateurs",
    icon: "bi bi-gear",
    route: { name: "UserSettings", params: { id: userId } },
  },
];

const kapteurOptions = [
  {
    name: "kapteurAssociation",
    title: "Association de Kapteurs",
    description: "Associer des Kapteurs entre eux",
    icon: "bi bi-link-45deg",
    route: { name: "KapteurAssociation" },
  },
  {
    name: "kapteurAlarm",
    title: "Configuration spécifique des Kapteurs",
    description: "Renommer les Kapteurs et configurer les alertes",
    icon: "bi bi-gear",
    route: { name: "KapteurAlarm" },
  },
];

function goToOption(option) {
  if (option.name === "UserSettings" && !option.params?.id) {
    console.error('Paramètre "id" manquant pour UserSettings');
    return;
  }

  router.push({
    name: option.name,
    params: option.params || {},
  });
}
</script>
