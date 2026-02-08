<template>
  <div class="layout-container" :class="{ 'has-bottom-nav': isMobile && showBottomNav }">
    <!-- Overlay para fechar sidebar em mobile -->
    <div 
      v-if="showDevices && isMobile" 
      class="sidebar-overlay"
      @click="showDevices = false"
    ></div>

    <!-- Handle lateral (desktop) -->
    <div
      v-if="!hideSidebar && !isMobile"
      class="sidebar-handle"
      @click="showDevices = !showDevices"
      :style="handleStyle"
    >
      {{ sidebarHandleText }}
    </div>

    <!-- Sidebar -->
    <aside
      v-if="!hideSidebar"
      class="sidebar"
      :class="{ 
        visible: showDevices, 
        'sidebar-mobile': isMobile,
        'sidebar-swipeable': isMobile && isTouchDevice
      }"
      ref="sidebarRef"
      @touchstart="onTouchStart"
      @touchmove="onTouchMove"
      @touchend="onTouchEnd"
    >
      <div class="sidebar-header">
        <h5 class="mb-0">{{ sidebarTitle }}</h5>
        <button 
          v-if="isMobile" 
          class="sidebar-close-btn"
          @click="showDevices = false"
          aria-label="Fermer"
        >
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <ul class="list-group list-group-flush flex-grow-1 sidebar-list">
        <li
          v-for="dev in devices"
          :key="dev.devEui"
          class="list-group-item list-group-item-action device-item"
          :class="{ active: dev.devEui === selectedDevice?.devEui }"
          @click="selectDevice(dev)"
        >
          <span class="device-label">
            {{ dev.description || dev.deviceName || dev.name || dev.devEui }}
            <i
              v-if="dev.isVirtual"
              class="bi bi-diagram-3 ms-1 text-secondary"
              title="Groupe virtuel"
            />
          </span>
          <div class="device-actions">
            <button
              v-if="showAddButton && !deviceHasChart(dev)"
              class="add-chart-btn"
              type="button"
              @click.stop="$emit('add-chart', dev)"
              :title="'Ajouter un graphique pour ' + (dev.description || dev.deviceName || dev.name || dev.devEui)"
            >
              <i class="bi bi-plus-circle"></i>
            </button>
            <span v-else-if="deviceHasChart(dev)" class="has-chart-indicator" title="Graphique actif">
              <i class="bi bi-graph-up"></i>
            </span>
            <button
              v-if="dev.canDelete"
              class="btn btn-link btn-sm text-danger delete-entry-btn"
              type="button"
              @click.stop="$emit('delete-device', dev)"
              title="Supprimer"
            >
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </li>
      </ul>
    </aside>

    <div class="main">
      <!-- Botão flutuante para mostrar header quando escondido em mobile (lado direito) -->
      <button
        v-if="isCompactMode && !headerVisible"
        class="header-show-btn"
        @click="showHeader"
        aria-label="Afficher menu"
      >
        <i class="bi bi-chevron-down"></i>
      </button>
      
      <header
        class="custom-header d-flex align-items-center justify-content-between"
        :class="{ 
          'header-compact': isCompactMode,
          'header-hidden': isCompactMode && !headerVisible
        }"
        @click="toggleHeader"
      >
        <div class="header-side left d-flex align-items-center gap-2">
          <button
            v-if="isMobile && !hideSidebar"
            class="btn btn-light btn-sm toggle-devices-btn touch-target"
            @click.stop="handleMenuClick"
            :title="showDevices ? 'Masquer Kapteurs' : 'Afficher Kapteurs'"
            aria-label="Menu"
          >
            <i :class="showDevices ? 'bi bi-x-lg' : 'bi bi-list'"></i>
          </button>
        </div>

        <div class="header-center flex-grow-1 d-flex justify-content-center">
          <div class="title-container">
            <!-- Modo compacto (mobile/landscape): mostrar nome do dispositivo -->
            <div v-if="isCompactMode" class="mobile-header-content">
              <slot name="mobile-chart-info">
                <!-- Fallback: nome do dispositivo -->
                <span v-if="selectedDevice" class="mobile-device-name">
                  {{ selectedDevice.description || selectedDevice.deviceName || selectedDevice.name || 'DPG' }}
                </span>
                <span v-else class="mobile-app-title">KropKharts</span>
              </slot>
            </div>
            <!-- Desktop: título completo -->
            <h5 v-else class="m-0 title">
              <slot name="title">
                <span>KropKontrol IoT Dashboard</span>
              </slot>
            </h5>
            <div v-if="!isCompactMode" class="subtitle">
              <slot name="subtitle"></slot>
            </div>
          </div>
        </div>

        <div class="header-side right d-flex align-items-center gap-2">
          <!-- Slot para botões de ação do gráfico em modo compacto -->
          <slot v-if="isCompactMode" name="mobile-chart-actions"></slot>
          <!-- FAQ - escondido em mobile pequeno -->
          <router-link
            to="/faq"
            class="btn btn-outline-light btn-sm header-btn desktop-only"
            title="FAQ"
          >
            <i class="bi bi-question-circle"></i>
          </router-link>
          <!-- Botão voltar -->
          <button
            v-if="showBack"
            class="btn btn-outline-light btn-sm header-btn touch-target"
            @click="$emit('back')"
            :title="backTitle"
          >
            <i :class="backIcon"></i>
          </button>
          <!-- Logout - texto apenas em desktop -->
          <button
            class="btn btn-outline-light btn-sm logout-btn header-btn touch-target"
            @click="$emit('logout')"
            title="Déconnexion"
          >
            <i class="bi bi-box-arrow-right mobile-only"></i>
            <span class="desktop-only">Déconnexion</span>
          </button>
        </div>
      </header>

      <main 
        class="content flex-grow-1" 
        :class="{ 'content-with-bottom-nav': isMobile && showBottomNav }"
      >
        <slot />
      </main>
    </div>

    <!-- Bottom Navigation para Mobile -->
    <MobileNavigation 
      v-if="showBottomNav"
      :items="bottomNavItems"
      @navigate="onBottomNavNavigate"
    />
  </div>
</template>

<script setup>
import { ref, toRefs, onMounted, onUnmounted, computed } from "vue";
import { useResponsive } from "@/composables/useResponsive.js";
import MobileNavigation from "@/components/MobileNavigation.vue";

const props = defineProps({
  devices: { type: Array, default: () => [] },
  selectedDevice: { type: Object, default: null },
  devicesWithCharts: { type: Set, default: () => new Set() },
  showBack: { type: Boolean, default: false },
  backIcon: { type: String, default: "bi bi-arrow-left" },
  backTitle: { type: String, default: "Retour" },
  hideSidebar: { type: Boolean, default: false },
  sidebarTitle: { type: String, default: "Kapteurs" },
  showAddButton: { type: Boolean, default: true },
  sidebarDefaultOpen: { type: Boolean, default: false },
  sidebarHandleText: { type: String, default: "Kapteurs" },
  sidebarHandleTop: { type: String, default: "75%" },
  showBottomNav: { type: Boolean, default: false },
  bottomNavItems: { type: Array, default: null },
});

// Verifica se um dispositivo já tem gráfico
function deviceHasChart(dev) {
  return props.devicesWithCharts?.has?.(dev.devEui) ?? false;
}

const emit = defineEmits([
  "select-device",
  "add-chart",
  "logout",
  "back",
  "delete-device",
  "bottom-nav-navigate",
]);

// Responsividade centralizada
const { isMobile, isTouchDevice, isSmallMobile, isLandscape } = useResponsive();

// Modo compacto: mobile OU landscape com altura pequena (telemóvel rodado)
const isCompactMode = computed(() => {
  if (isMobile.value) return true;
  if (isLandscape.value && isTouchDevice.value && window.innerHeight < 500) return true;
  return false;
});

// Header auto-hide em mobile
const headerVisible = ref(true);
let lastScrollY = 0;
let headerTimeout = null;
let autoHideTimeout = null;

// Auto-esconder header após 2 segundos em mobile (após gráfico carregar)
function scheduleAutoHide() {
  if (!isCompactMode.value) return;
  clearTimeout(autoHideTimeout);
  autoHideTimeout = setTimeout(() => {
    if (isCompactMode.value) {
      headerVisible.value = false;
    }
  }, 2000);
}

function handleScroll() {
  if (!isCompactMode.value) return;
  
  const currentScrollY = window.scrollY;
  
  // Esconder ao fazer scroll para baixo, mostrar ao fazer scroll para cima
  if (currentScrollY > lastScrollY && currentScrollY > 60) {
    headerVisible.value = false;
  } else {
    headerVisible.value = true;
    // Re-agendar auto-hide quando mostrar
    scheduleAutoHide();
  }
  
  lastScrollY = currentScrollY;
}

function showHeader() {
  headerVisible.value = true;
  scheduleAutoHide();
}

function toggleHeader() {
  if (!isCompactMode.value) return;
  
  if (headerVisible.value) {
    headerVisible.value = false;
    clearTimeout(autoHideTimeout);
  } else {
    showHeader();
  }
}

const { hideSidebar } = toRefs(props);
const showDevices = ref(props.sidebarDefaultOpen);
const sidebarRef = ref(null);

// Estilos computados
const sidebarWidth = computed(() => (isMobile.value ? "85vw" : "220px"));
const handleStyle = computed(() => ({
  top: props.sidebarHandleTop,
  left: showDevices.value && !isMobile.value ? sidebarWidth.value : "0",
  opacity: showDevices.value && !isMobile.value ? 0.85 : 1,
}));

// Touch/Swipe handling para sidebar
let touchStartX = 0;
let touchStartY = 0;
let touchCurrentX = 0;
const SWIPE_THRESHOLD = 50;

function onTouchStart(e) {
  if (!isMobile.value) return;
  touchStartX = e.touches[0].clientX;
  touchStartY = e.touches[0].clientY;
  touchCurrentX = touchStartX;
}

function onTouchMove(e) {
  if (!isMobile.value) return;
  touchCurrentX = e.touches[0].clientX;
}

function onTouchEnd() {
  if (!isMobile.value) return;
  const deltaX = touchCurrentX - touchStartX;
  // Swipe left para fechar
  if (deltaX < -SWIPE_THRESHOLD && showDevices.value) {
    showDevices.value = false;
  }
}

// Swipe from edge para abrir sidebar
function handleEdgeSwipe(e) {
  if (!isMobile.value || props.hideSidebar) return;
  const startX = e.touches[0].clientX;
  // Apenas se começar na borda esquerda (primeiros 20px)
  if (startX > 20) return;
  
  const handleMove = (moveEvent) => {
    const currentX = moveEvent.touches[0].clientX;
    if (currentX - startX > SWIPE_THRESHOLD) {
      showDevices.value = true;
      cleanup();
    }
  };
  
  const cleanup = () => {
    document.removeEventListener('touchmove', handleMove);
    document.removeEventListener('touchend', cleanup);
  };
  
  document.addEventListener('touchmove', handleMove, { passive: true });
  document.addEventListener('touchend', cleanup, { once: true });
}

// Selecionar device e fechar sidebar em mobile
function selectDevice(dev) {
  emit('select-device', dev);
  if (isMobile.value) {
    showDevices.value = false;
  }
}

// Handler do botão menu - em mobile mostra header se escondido, senão toggle sidebar
function handleMenuClick() {
  if (isCompactMode.value && !headerVisible.value) {
    showHeader();
  } else {
    showDevices.value = !showDevices.value;
  }
}

// Bottom nav navigation
function onBottomNavNavigate(item) {
  emit('bottom-nav-navigate', item);
}

// Fechar sidebar com Escape
function handleKeydown(e) {
  if (e.key === 'Escape' && showDevices.value) {
    showDevices.value = false;
  }
}

onMounted(() => {
  document.addEventListener('touchstart', handleEdgeSwipe, { passive: true });
  document.addEventListener('keydown', handleKeydown);
  window.addEventListener('scroll', handleScroll, { passive: true });
  // Auto-esconder header após carregar em mobile
  scheduleAutoHide();
});

onUnmounted(() => {
  document.removeEventListener('touchstart', handleEdgeSwipe);
  document.removeEventListener('keydown', handleKeydown);
  window.removeEventListener('scroll', handleScroll);
  clearTimeout(headerTimeout);
  clearTimeout(autoHideTimeout);
});
</script>

<style scoped>
/* --- Structure générale --- */
.layout-container {
  position: relative; /* crée le contexte pour le ::before */
  display: flex;
  flex-direction: row;
  min-height: 100vh;
  height: 100dvh;
  overflow: auto;
  align-items: stretch;
}

/* Image de fond via pseudo-élément — ne débordera jamais le header */
.layout-container::before {
  content: "";
  position: fixed; /* fond global */
  inset: 0; /* couvre l'écran */
  background-image: url("/assets/KKv.svg");
  background-repeat: no-repeat;
  background-position: 50% 50%; /* centré */
  background-size: contain; /* garde les proportions */
  opacity: 0.18;
  pointer-events: none;
  z-index: 0; /* pile de base */
}

/* --- Sidebar escamotable --- */
.sidebar {
  position: fixed;
  left: 0;
  top: 0;
  bottom: 0;
  width: 220px;
  min-width: 180px;
  max-width: 300px;
  overflow-y: auto;
  background: #f8f9fa;
  border-right: 1px solid #dee2e6;
  transform: translateX(-100%);
  transition: transform 0.3s ease;
  will-change: transform;
  z-index: 1000; /* au-dessus du fond */
}
.sidebar.visible {
  transform: translateX(0);
}
.sidebar-mobile {
  width: 80vw;
  max-width: 300px;
  min-width: 150px;
}

/* --- Poignée latérale --- */
.sidebar-handle {
  position: fixed;
  top: 50%;
  left: 0;
  transform: translateY(-50%);
  width: auto;
  height: auto;
  padding: 12px 6px;
  background: linear-gradient(180deg, #82be20 0%, #6aa818 100%);
  border-radius: 0 8px 8px 0;
  cursor: pointer;
  z-index: 50;
  writing-mode: vertical-rl;
  text-orientation: mixed;
  color: #fff;
  font-weight: 600;
  letter-spacing: 1px;
  font-size: 0.7rem;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.2);
  transition: left 0.25s ease, opacity 0.25s ease, transform 0.2s ease;
  will-change: left, opacity;
}
.sidebar-handle:hover {
  background: linear-gradient(180deg, #8fcc2a 0%, #72b31c 100%);
  transform: translateY(-50%) scale(1.05);
}
.sidebar-handle:active {
  transform: translateY(-50%) scale(0.95);
}

/* --- Contenu principal --- */
.main {
  display: flex;
  flex-direction: column;
  flex-grow: 1;
  min-width: 0;
  overflow-y: auto;
  position: relative;
  z-index: 1; /* au-dessus du fond */
}

/* --- En-tête --- */
.custom-header {
  display: flex;
  align-items: center;
  justify-content: center;
  position: sticky;
  top: 0;
  background-color: #82be20ff;
  min-height: 60px;
  padding: 0 1rem;
  z-index: 10;
  transition: transform 0.3s ease, min-height 0.3s ease;
}

/* Header compacto em mobile */
.custom-header.header-compact {
  min-height: 44px;
}

/* Header escondido em mobile */
.custom-header.header-hidden {
  transform: translateY(-100%);
  min-height: 0;
  pointer-events: none;
}

/* Botão flutuante para mostrar header em mobile (lado direito) */
.header-show-btn {
  position: fixed;
  top: 8px;
  right: 8px;
  z-index: 20;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: #82be20;
  color: white;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0,0,0,0.25);
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.header-show-btn:active {
  transform: scale(0.95);
  box-shadow: 0 1px 4px rgba(0,0,0,0.2);
}
.header-center {
  flex: 1 1 0%;
  display: flex;
  justify-content: center;
  align-items: center;
  pointer-events: none;
  position: absolute;
  left: 50px; /* Espaço para botão menu à esquerda */
  right: 140px; /* Espaço para botões à direita */
  top: 0;
  bottom: 0;
  z-index: 0;
}
/* AJOUT : Styles pour les nouveaux éléments */
.title-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
.subtitle {
  font-size: 0.75rem;
  font-weight: 400;
  color: rgba(255, 255, 255, 0.9);
  margin-top: -4px; /* Ajustez pour rapprocher du titre */
  pointer-events: auto;
}
/* FIN AJOUT */

.title {
  font-family: "Orbitron", sans-serif;
  font-weight: bold;
  font-size: clamp(1.2rem, 4vw, 2.5rem);
  text-align: center;
  margin: 0;
  pointer-events: auto;
  color: #fff;
  text-shadow:
    0 2px 10px rgba(0, 0, 0, 0.13),
    0 1px 4px rgba(0, 0, 0, 0.19);
}

/* Mobile header content */
.mobile-header-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
  max-width: calc(100% - 120px); /* Deixar espaço para botões */
  margin: 0 auto;
  padding: 0 0.5rem;
}

.mobile-device-name {
  font-size: 0.75rem;
  font-weight: 600;
  color: #fff;
  max-width: 200px;
  white-space: normal;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: center;
  line-height: 1.2;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

.mobile-app-title {
  font-family: "Orbitron", sans-serif;
  font-size: 0.9rem;
  font-weight: bold;
  color: #fff;
}

/* Estilos para botões de ação do gráfico no header mobile */
.header-side.right .chart-action-btn {
  padding: 0.2rem 0.4rem;
  min-width: 28px;
  min-height: 28px;
  font-size: 0.75rem;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: #fff;
  border-radius: 4px;
}

.header-side.right .chart-action-btn:hover {
  background: rgba(255, 255, 255, 0.25);
}
.header-side.right {
  position: absolute;
  top: 0;
  right: 1rem;
  height: 100%;
  display: flex;
  align-items: center;
  gap: 0.7rem;
  z-index: 2;
}

/* --- Responsive --- */
@media (max-width: 991px) {
  .sidebar {
    width: 170px;
    min-width: 120px;
  }
  .header-side.right {
    right: 0.6rem;
    gap: 0.3rem;
  }
}
@media (max-width: 768px) {
  .layout-container {
    flex-direction: column;
  }
  .header-side.right {
    right: 0.2rem;
    gap: 0.2rem;
  }
  .toggle-devices-btn {
    margin-right: 0.1rem;
    margin-left: 0.1rem;
    padding: 0.4rem 0.7rem !important;
  }
}
@media (max-width: 576px) {
  .main {
    padding: 0 !important;
  }
  .content {
    padding: 0.5rem !important;
  }
  .title {
    font-size: 0.99rem;
  }
  .header-side.right {
    right: 0.05rem;
  }
  /* AJOUT : Réduire la taille du sous-titre sur petit écran */
  .subtitle {
    font-size: 0.65rem;
    margin-top: -2px;
  }
}

/* --- Bouton + --- */
.sidebar-header {
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  background: #f3f3f3;
}

.device-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.device-label {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  flex: 1 1 auto;
  min-width: 0;
}

/* Botão para adicionar gráfico */
.add-chart-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  color: #82be20;
  font-size: 1.1rem;
  padding: 0.25rem;
  cursor: pointer;
  opacity: 0.7;
  transition: opacity 0.2s ease, transform 0.15s ease, color 0.2s ease;
}

.add-chart-btn:hover {
  opacity: 1;
  color: #6aa015;
  transform: scale(1.15);
}

.add-chart-btn:active {
  transform: scale(0.95);
}

/* Indicador de gráfico ativo */
.has-chart-indicator {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #82be20;
  font-size: 0.9rem;
  padding: 0.25rem;
  opacity: 0.6;
}

.device-item.active .has-chart-indicator {
  opacity: 1;
}

.delete-entry-btn {
  color: #dc3545 !important;
  padding: 0;
  margin-left: 0.25rem;
  display: inline-flex;
  align-items: center;
}

/* --- Overlay para sidebar mobile --- */
.sidebar-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* --- Sidebar header com botão fechar --- */
.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  background: #f3f3f3;
}

.sidebar-close-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  border-radius: 50%;
  color: #666;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.sidebar-close-btn:hover {
  background: rgba(0, 0, 0, 0.1);
  color: #333;
}

/* --- Device actions container --- */
.device-actions {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  flex-shrink: 0;
}

/* --- Touch targets para mobile --- */
.touch-target {
  min-width: 44px;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* --- Header compacto para mobile --- */
.header-compact {
  min-height: 50px;
  padding: 0 0.5rem;
}

.header-compact .title {
  font-size: clamp(0.9rem, 3.5vw, 1.5rem);
}

/* --- Header buttons --- */
.header-btn {
  padding: 0.35rem 0.6rem;
}

/* --- Visibility utilities --- */
.mobile-only {
  display: none !important;
}

.desktop-only {
  display: inline !important;
}

@media (max-width: 767.98px) {
  .mobile-only {
    display: inline !important;
  }
  
  .desktop-only {
    display: none !important;
  }
  
  .header-btn {
    padding: 0.5rem;
  }
  
  /* Device items mais touch-friendly */
  .device-item {
    min-height: 48px;
    padding: 0.75rem 1rem;
  }
  
  .add-chart-btn {
    font-size: 1.3rem;
    padding: 0.35rem;
  }
  
  .has-chart-indicator {
    font-size: 1rem;
  }
  
  /* Sidebar mais larga em mobile */
  .sidebar-mobile {
    width: 85vw;
    max-width: 320px;
  }
}

/* --- Content com bottom nav --- */
.content-with-bottom-nav {
  padding-bottom: calc(56px + env(safe-area-inset-bottom, 0px));
}

/* --- Layout com bottom nav --- */
.has-bottom-nav {
  padding-bottom: 0;
}

/* --- Content padding responsivo --- */
.content {
  padding: 1rem;
  padding-left: 1.5rem; /* Espaço para o sidebar-handle */
}

@media (max-width: 767.98px) {
  .content {
    padding: 0.75rem;
    padding-left: 1.25rem;
  }
}

@media (max-width: 575.98px) {
  .content {
    padding: 0.5rem;
    padding-left: 1rem;
  }
}

/* --- Header side left --- */
.header-side.left {
  position: absolute;
  left: 0.5rem;
  top: 0;
  height: 100%;
  display: flex;
  align-items: center;
  z-index: 2;
}

@media (max-width: 767.98px) {
  .header-side.left {
    left: 0.25rem;
  }
}

/* --- Toggle devices button melhorado --- */
.toggle-devices-btn {
  background: rgba(255, 255, 255, 0.9);
  border: none;
  border-radius: 8px;
  font-size: 1.25rem;
  color: #333;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.toggle-devices-btn:hover,
.toggle-devices-btn:focus {
  background: #fff;
}

.toggle-devices-btn:active {
  transform: scale(0.95);
}

/* Mobile landscape: esconder completamente elementos do header quando escondido */
@media (max-height: 500px) and (orientation: landscape) {
  .header-hidden .header-side.left,
  .header-hidden .toggle-devices-btn {
    display: none !important;
    visibility: hidden !important;
    opacity: 0 !important;
  }
  
  /* Garantir que não há resíduos visíveis */
  .header-hidden {
    overflow: hidden;
    height: 0 !important;
    min-height: 0 !important;
    padding: 0 !important;
  }
}
</style>
