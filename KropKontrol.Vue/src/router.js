import { createRouter, createWebHistory } from "vue-router";
import {
  getUserRoleFromToken,
  isTokenExpired,
  refreshTokenIfNeeded,
} from "./services/authService.js";
import { toast } from "vue3-toastify";
import LandingPage from "./views/LandingPage.vue";
import LoginPage from "./views/LoginPage.vue";
import DefaultLayout from "./views/layout/DefaultLayout.vue";

// import ClimateReport   from './views/ClimateReport.vue';
// import UserConfig      from './views/UserConfig.vue';

const base = import.meta.env.VITE_BASE_PATH || "/";

// Debug navigation helper (enabled in dev builds)
const NAV_DEBUG = !import.meta.env.PROD;
const logNav = (...args) => {
  if (NAV_DEBUG) console.log("[NavGuard]", ...args);
};

const routes = [
  {
    path: "/",
    name: "Landing",
    component: LandingPage,
    meta: { requiresAuth: true },
  },
  {
    path: "/snapshot",
    name: "Snapshot",
    component: () => import("./views/SnapshotPage.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: () => import("./views/DashboardPage.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/export",
    name: "DataExport",
    component: () => import("./views/ExportDataPage.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/report-watering",
    name: "WateringReport",
    component: () => import("./views/WateringReport.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/analyse-fertilisation",
    name: "AnalyseFertilisation",
    component: () => import("./views/AnalyseFertilisationPage.vue"),
    meta: { requiresAuth: true, role: "Root" },
  },
  {
    path: "/krop-konfigurator",
    name: "KropKonfigurator",
    component: () => import("./views/KropKonfiguratorPage.vue"),
    meta: { requiresAuth: true, role: "Root" },
  },
  {
    path: "/default-layout",
    name: "DefaultLayout",
    component: DefaultLayout,
    meta: { requiresAuth: true },
  },
  {
    path: "/SpecificChart",
    name: "SpecificChart",
    component: () => import("./views/SpecificChartPage.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/climate-report",
    name: "ClimateReport",
    component: () => import("./views/ClimateReport.vue"),
    meta: { requiresAuth: true, role: "Root" },
  },
  //  ****** Users ******
  {
    path: "/users",
    name: "Users",
    component: () => import("@/views/UsersPage.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/users/create",
    name: "UserCreate",
    component: () => import("@/views/users/UserCreate.vue"),
    meta: { requiresAuth: true, role: "Root" },
  },
  {
    path: "/users/list",
    name: "UserList",
    component: () => import("@/views/users/UserList.vue"),
    meta: { requiresAuth: true, role: "Root" },
  },
  {
    path: "/users/:id/edit",
    name: "UserSettings",
    component: () => import("@/views/users/UserSettings.vue"),
    meta: { requiresAuth: true },
    props: true,
  },
  //  ****** Devices ******
  {
    path: "/kapteurs/association",
    name: "KapteurAssociation",
    component: () => import("@/views/KapteurAssociation.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/kapteurs/kapteurAlarm",
    name: "KapteurAlarm",
    component: () => import("@/views/KapteurAlarm.vue"),
    meta: { requiresAuth: true, disallowRole: "Technician" },
  },
  {
    path: "/faq",
    name: "Faq",
    component: () => import("./views/FaqPage.vue"),
    meta: { requiresAuth: true },
  },

  // --- Placeholders pour modules à venir ---
  // {
  //   path: '/user-config',
  //   name: 'UserConfig',
  //   component: UserConfig, // ou lazy: () => import('./views/UserConfig.vue')
  //   meta: { requiresAuth: true }
  // },
  {
    path: "/login",
    name: "Login",
    component: LoginPage,
  },

  // route pour quand non autorisé
  {
    path: "/unauthorized",
    name: "Unauthorized",
    component: {
      template: `<div><h1>Accès refusé</h1><p>Vous n'avez pas la permission de voir cette page.</p></div>`,
    },
  },
];

const router = createRouter({
  history: createWebHistory(base),
  routes,
});

router.beforeEach(async (to, from, next) => {
  logNav("beforeEach start", {
    from: from.fullPath,
    to: to.fullPath,
    requiresAuth: !!to.meta?.requiresAuth,
    role: to.meta?.role,
    disallowRole: to.meta?.disallowRole,
  });
  const preRole = getUserRoleFromToken();
  const preExpired = isTokenExpired();
  logNav("pre-check", { role: preRole, tokenExpired: preExpired });
  // Always try to refresh before checking auth/role to avoid logout
  // if the access token is expired but the refresh token is still valid.
  await refreshTokenIfNeeded();

  const userRole = getUserRoleFromToken();
  const tokenExpired = isTokenExpired();
  logNav("post-refresh-check", { role: userRole, tokenExpired });

  if (to.meta.requiresAuth && (!userRole || tokenExpired)) {
    logNav("redirect -> Login", {
      reason: !userRole ? "no role" : "token expired",
    });
    return next({ name: "Login" });
  }

  /*
  if (to.meta.role && to.meta.role !== userRole) {
    logNav('redirect -> Landing (role mismatch)', { required: to.meta.role, have: userRole });
    toast.error('Vous n\'avez pas la permission d\'accéder à cette page.', { autoClose: 8000 });
    return next({ name: 'Landing' });
  }*/

  if (to.meta.role && to.meta.role !== userRole) {
    logNav("redirect -> Landing (role mismatch)", {
      required: to.meta.role,
      have: userRole,
    });
    setTimeout(() => {
      toast.error("Vous n'avez pas la permission d'accéder à cette page.", {
        autoClose: 1000,
      });
    }, 100);
    return next({ name: "Landing" });
  }

  if (to.meta.disallowRole && to.meta.disallowRole === userRole) {
    logNav("redirect -> Landing (disallowed role)", {
      disallowed: to.meta.disallowRole,
      have: userRole,
    });
    setTimeout(() => {
      toast.error("Vous n'avez pas la permission d'accéder à cette page.", {
        autoClose: 1000,
      });
    }, 100);
    return next({ name: "Landing" });
  }

  logNav("allow -> next()", { to: to.fullPath });
  next();
});

export { router };
