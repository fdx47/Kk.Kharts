/**
 * useResponsive.js
 * 
 * Composable centralizado para gestão de responsividade.
 * Fornece breakpoints, estados de dispositivo e utilitários para UI adaptativa.
 * 
 * Breakpoints seguem o padrão Bootstrap 5:
 * - xs: < 576px (mobile portrait)
 * - sm: >= 576px (mobile landscape)
 * - md: >= 768px (tablet)
 * - lg: >= 992px (desktop)
 * - xl: >= 1200px (large desktop)
 * - xxl: >= 1400px (extra large)
 */

import { ref, computed, onMounted, onUnmounted, readonly } from 'vue';
import debounce from 'lodash/debounce';

// Breakpoints em pixels (Bootstrap 5)
export const BREAKPOINTS = {
  xs: 0,
  sm: 576,
  md: 768,
  lg: 992,
  xl: 1200,
  xxl: 1400,
};

// Estado global reativo (singleton para evitar múltiplos listeners)
const windowWidth = ref(typeof window !== 'undefined' ? window.innerWidth : 1024);
const windowHeight = ref(typeof window !== 'undefined' ? window.innerHeight : 768);
const isListenerAttached = ref(false);
let listenerCount = 0;

function handleResize() {
  windowWidth.value = window.innerWidth;
  windowHeight.value = window.innerHeight;
}

const debouncedResize = debounce(handleResize, 100);

function attachListener() {
  if (typeof window === 'undefined') return;
  listenerCount++;
  if (!isListenerAttached.value) {
    window.addEventListener('resize', debouncedResize);
    isListenerAttached.value = true;
    handleResize(); // Sync inicial
  }
}

function detachListener() {
  listenerCount--;
  if (listenerCount <= 0 && isListenerAttached.value) {
    window.removeEventListener('resize', debouncedResize);
    isListenerAttached.value = false;
    listenerCount = 0;
  }
}

/**
 * Composable principal para responsividade
 */
export function useResponsive() {
  // Lifecycle hooks para gestão do listener
  onMounted(attachListener);
  onUnmounted(detachListener);

  // Breakpoint atual
  const currentBreakpoint = computed(() => {
    const w = windowWidth.value;
    if (w >= BREAKPOINTS.xxl) return 'xxl';
    if (w >= BREAKPOINTS.xl) return 'xl';
    if (w >= BREAKPOINTS.lg) return 'lg';
    if (w >= BREAKPOINTS.md) return 'md';
    if (w >= BREAKPOINTS.sm) return 'sm';
    return 'xs';
  });

  // Estados de dispositivo
  const isMobile = computed(() => windowWidth.value < BREAKPOINTS.md);
  const isTablet = computed(() => windowWidth.value >= BREAKPOINTS.md && windowWidth.value < BREAKPOINTS.lg);
  const isDesktop = computed(() => windowWidth.value >= BREAKPOINTS.lg);
  const isSmallMobile = computed(() => windowWidth.value < BREAKPOINTS.sm);
  const isLargeDesktop = computed(() => windowWidth.value >= BREAKPOINTS.xl);

  // Orientação
  const isLandscape = computed(() => windowWidth.value > windowHeight.value);
  const isPortrait = computed(() => windowHeight.value >= windowWidth.value);

  // Touch detection
  const isTouchDevice = computed(() => {
    if (typeof window === 'undefined') return false;
    return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
  });

  // Utilitários de comparação
  const isUp = (breakpoint) => windowWidth.value >= BREAKPOINTS[breakpoint];
  const isDown = (breakpoint) => windowWidth.value < BREAKPOINTS[breakpoint];
  const isBetween = (min, max) => 
    windowWidth.value >= BREAKPOINTS[min] && windowWidth.value < BREAKPOINTS[max];

  // Valores responsivos
  const responsiveValue = (values) => {
    const bp = currentBreakpoint.value;
    // Procura o valor mais próximo (fallback para breakpoints menores)
    const order = ['xxl', 'xl', 'lg', 'md', 'sm', 'xs'];
    const startIdx = order.indexOf(bp);
    for (let i = startIdx; i < order.length; i++) {
      if (values[order[i]] !== undefined) return values[order[i]];
    }
    return values.default ?? values.xs ?? Object.values(values)[0];
  };

  // Número de colunas para grids
  const gridColumns = computed(() => responsiveValue({
    xs: 1,
    sm: 2,
    md: 2,
    lg: 3,
    xl: 4,
    xxl: 4,
  }));

  // Tamanho de fonte base responsivo
  const baseFontSize = computed(() => responsiveValue({
    xs: 14,
    sm: 14,
    md: 15,
    lg: 16,
    xl: 16,
    xxl: 16,
  }));

  // Padding/spacing responsivo
  const spacing = computed(() => responsiveValue({
    xs: { sm: '0.25rem', md: '0.5rem', lg: '0.75rem', xl: '1rem' },
    sm: { sm: '0.25rem', md: '0.5rem', lg: '1rem', xl: '1.25rem' },
    md: { sm: '0.5rem', md: '0.75rem', lg: '1.25rem', xl: '1.5rem' },
    lg: { sm: '0.5rem', md: '1rem', lg: '1.5rem', xl: '2rem' },
  }));

  // Altura do header responsiva
  const headerHeight = computed(() => responsiveValue({
    xs: 50,
    sm: 54,
    md: 58,
    lg: 60,
    xl: 64,
  }));

  // Altura da bottom navigation (mobile only)
  const bottomNavHeight = computed(() => isMobile.value ? 56 : 0);

  // Safe area para conteúdo (considerando header e bottom nav)
  const contentSafeArea = computed(() => ({
    top: headerHeight.value,
    bottom: bottomNavHeight.value,
    height: windowHeight.value - headerHeight.value - bottomNavHeight.value,
  }));

  return {
    // Dimensões
    windowWidth: readonly(windowWidth),
    windowHeight: readonly(windowHeight),
    
    // Breakpoints
    currentBreakpoint,
    BREAKPOINTS,
    
    // Estados de dispositivo
    isMobile,
    isTablet,
    isDesktop,
    isSmallMobile,
    isLargeDesktop,
    isTouchDevice,
    
    // Orientação
    isLandscape,
    isPortrait,
    
    // Utilitários
    isUp,
    isDown,
    isBetween,
    responsiveValue,
    
    // Valores pré-calculados
    gridColumns,
    baseFontSize,
    spacing,
    headerHeight,
    bottomNavHeight,
    contentSafeArea,
  };
}

/**
 * Hook para media queries CSS em JavaScript
 * @param {string} query - Media query CSS (ex: '(min-width: 768px)')
 */
export function useMediaQuery(query) {
  const matches = ref(false);

  if (typeof window !== 'undefined') {
    const mediaQuery = window.matchMedia(query);
    matches.value = mediaQuery.matches;

    const handler = (e) => { matches.value = e.matches; };
    
    onMounted(() => {
      mediaQuery.addEventListener('change', handler);
    });
    
    onUnmounted(() => {
      mediaQuery.removeEventListener('change', handler);
    });
  }

  return readonly(matches);
}

/**
 * Hook para detectar preferências do sistema
 */
export function useSystemPreferences() {
  const prefersReducedMotion = useMediaQuery('(prefers-reduced-motion: reduce)');
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)');
  const prefersHighContrast = useMediaQuery('(prefers-contrast: high)');

  return {
    prefersReducedMotion,
    prefersDarkMode,
    prefersHighContrast,
  };
}

export default useResponsive;
