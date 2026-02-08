// src/composables/useAuth.js...
import { isRootUser } from "../services/roleUtils.js";
if (!import.meta.env.PROD && isRootUser()) {
  console.log("✅ useAuth.js chargé");
}
import { useRouter } from "vue-router";
import { clearAuthData } from "../services/authService.js";
import { inject } from "vue";
import { useDevices } from "./useDevices.js";

export function useAuth() {
  const router = useRouter();
  const { reset: resetVirtualDevices } = inject("virtualDevices");
  const { resetDevices } = useDevices();
  function logout() {
    // Supprime uniquement les informations d'authentification.
    // Les données persistées (graphes, etc.) sont conservées pour
    // permettre leur récupération lors d'une future reconnexion.
    clearAuthData();
    resetVirtualDevices();
    resetDevices();
    // Solution robuste pour les guards : petite attente
    setTimeout(() => {
      router.replace({ name: "Login" });
    }, 30);
  }

  return { logout };
}
