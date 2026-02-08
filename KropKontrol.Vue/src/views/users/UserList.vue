<template>
  <DefaultLayout
    :hideSidebar="true"
    @logout="logout"
    showBack
    @back="goToUsersPage"
  >
    <template #title>Liste des utilisateurs</template>
    <div class="container py-4">
      <h2>Utilisateurs</h2>
      <table class="table mt-3">
        <thead>
          <tr>
            <th>ID</th>
            <th>Email</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id">
            <td>{{ user.id }}</td>
            <td>{{ user.email }}</td>
            <td>
              <button
                class="btn btn-sm btn-warning me-2"
                @click="editUser(user.id)"
              >
                Modifier
              </button>
              <button
                class="btn btn-sm btn-danger"
                @click="deleteUser(user.id)"
              >
                Supprimer
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { onMounted, ref } from "vue";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "@/composables/useAuth.js";
import { useRouter } from "vue-router";
import { fetchWithAuth } from "@/services/authService.js";

const users = ref([]);
const { logout } = useAuth();
const router = useRouter();

function goToUsersPage() {
  router.push({ name: "Users" });
}

const fetchUsers = async () => {
  const res = await fetchWithAuth(
    "https://kropkontrol.premiumasp.net/api/v1/users",
  );
  users.value = await res.json();
};

function editUser(id) {
  router.push({ name: "UserSettings", params: { id } });
}

async function deleteUser(id) {
  if (!confirm("Supprimer cet utilisateur ?")) return;

  await fetchWithAuth(`https://kropkontrol.premiumasp.net/api/v1/users/${id}`, {
    method: "DELETE",
  });

  await fetchUsers(); // atualiza lista
}

onMounted(fetchUsers);
</script>
