<template>
  <nav 
    v-if="isMobile" 
    class="mobile-nav"
    :class="{ 'mobile-nav--hidden': isHidden }"
  >
    <div class="mobile-nav__container">
      <button
        v-for="item in navItems"
        :key="item.name"
        class="mobile-nav__item"
        :class="{ 
          'mobile-nav__item--active': isActive(item),
          'mobile-nav__item--disabled': item.disabled
        }"
        @click="navigate(item)"
        :disabled="item.disabled"
        :title="item.label"
      >
        <i :class="['mobile-nav__icon', item.icon]"></i>
        <span class="mobile-nav__label">{{ item.label }}</span>
        <span v-if="item.badge" class="mobile-nav__badge">{{ item.badge }}</span>
      </button>
    </div>
  </nav>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useResponsive } from '@/composables/useResponsive.js';

const props = defineProps({
  /** Items de navegação customizados */
  items: {
    type: Array,
    default: null,
  },
  /** Esconder a navegação (útil durante scroll) */
  hidden: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['navigate']);

const route = useRoute();
const router = useRouter();
const { isMobile } = useResponsive();

// Items de navegação padrão
const defaultNavItems = [
  { 
    name: 'Snapshot', 
    label: 'Início', 
    icon: 'bi bi-house-fill',
    path: '/snapshot',
  },
  { 
    name: 'Dashboard', 
    label: 'Gráficos', 
    icon: 'bi bi-graph-up',
    path: '/dashboard',
  },
  { 
    name: 'DataExport', 
    label: 'Export', 
    icon: 'bi bi-download',
    path: '/export',
  },
  { 
    name: 'Users', 
    label: 'Config', 
    icon: 'bi bi-gear-fill',
    path: '/users',
  },
];

const navItems = computed(() => props.items || defaultNavItems);

// Controle de visibilidade durante scroll
const isHidden = computed(() => props.hidden);

// Verifica se o item está ativo
function isActive(item) {
  if (item.path) {
    return route.path.startsWith(item.path);
  }
  if (item.name) {
    return route.name === item.name;
  }
  return false;
}

// Navegação
function navigate(item) {
  if (item.disabled) return;
  
  emit('navigate', item);
  
  if (item.action && typeof item.action === 'function') {
    item.action();
    return;
  }
  
  if (item.name) {
    router.push({ name: item.name });
  } else if (item.path) {
    router.push(item.path);
  }
}
</script>

<style scoped>
.mobile-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1050;
  background: #ffffff;
  border-top: 1px solid #e9ecef;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.08);
  padding-bottom: env(safe-area-inset-bottom, 0);
  transition: transform 0.3s ease;
}

.mobile-nav--hidden {
  transform: translateY(100%);
}

.mobile-nav__container {
  display: flex;
  justify-content: space-around;
  align-items: stretch;
  max-width: 500px;
  margin: 0 auto;
  padding: 0;
}

.mobile-nav__item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  padding: 8px 4px 10px;
  background: transparent;
  border: none;
  color: #6c757d;
  font-size: 0.65rem;
  font-weight: 500;
  text-decoration: none;
  cursor: pointer;
  transition: color 0.2s ease, background-color 0.2s ease;
  position: relative;
  min-height: 56px;
  -webkit-tap-highlight-color: transparent;
}

.mobile-nav__item:active {
  background-color: rgba(130, 190, 32, 0.1);
}

.mobile-nav__item--active {
  color: #82be20;
}

.mobile-nav__item--active .mobile-nav__icon {
  transform: scale(1.1);
}

.mobile-nav__item--disabled {
  opacity: 0.4;
  pointer-events: none;
}

.mobile-nav__icon {
  font-size: 1.25rem;
  line-height: 1;
  transition: transform 0.2s ease;
}

.mobile-nav__label {
  display: block;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 1.2;
}

.mobile-nav__badge {
  position: absolute;
  top: 4px;
  right: 50%;
  transform: translateX(12px);
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  background: #dc3545;
  color: #fff;
  font-size: 0.6rem;
  font-weight: 700;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Landscape mobile - mais compacto */
@media (max-height: 500px) and (orientation: landscape) {
  .mobile-nav__item {
    min-height: 44px;
    padding: 6px 4px 8px;
  }
  
  .mobile-nav__icon {
    font-size: 1.1rem;
  }
  
  .mobile-nav__label {
    font-size: 0.6rem;
  }
}

/* Extra small screens */
@media (max-width: 350px) {
  .mobile-nav__label {
    font-size: 0.58rem;
  }
  
  .mobile-nav__icon {
    font-size: 1.1rem;
  }
}
</style>
