<template>
  <div class="tile-layout container" :class="containerClass" v-bind="$attrs">
    <slot name="before-row" />

    <!-- Chaque enfant de .row DOIT être une colonne Bootstrap            -->
    <!--  (ex. <div class="col-12 col-sm-6 col-md-4"> <div class="tile">…) -->
    <div class="row g-4" :class="rowClass">
      <slot />
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  containerClass: { type: [String, Array, Object], default: "" },
  rowClass: { type: [String, Array, Object], default: "" },
});
</script>

<style scoped>
/* ----------------------------------------------------------------- */
/* 1)  Grille : on force chaque enfant de .row à occuper toute sa     */
/*     colonne pour que .tile reçoive bien une largeur déterminée.   */
/* ----------------------------------------------------------------- */
.row > * {
  flex: 0 0 auto;
  width: 100%;
}

/* ----------------------------------------------------------------- */
/* 2)  Tuile carrée, fiable sur TOUS les navigateurs                  */
/* ----------------------------------------------------------------- */
:deep(.tile) {
  position: relative; /* nécessaire pour le ::before        */
  width: 100%; /* prend la largeur de la colonne     */
  aspect-ratio: 1 / 1; /* garde un rapport 1:1 si dispo       */
  cursor: pointer;
}

/* pseudo-élément : 100 % de la largeur ⇒ garantit la hauteur        */
:deep(.tile)::before {
  content: "";
  display: block;
  padding-top: 100%; /* 100 % = carré                      */
}

/*  Contenu absolutisé pour qu’il ne pousse jamais les bords          */
:deep(.tile > *) {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  /* Style visuel inchangé */
  padding: 1.2rem;
  overflow: hidden;
  border: 2px solid #82be20;
  background: #fff;
  transition:
    transform 0.2s,
    box-shadow 0.2s;
}

:deep(.tile:hover > *) {
  transform: translateY(-5px) scale(1.03);
  box-shadow: 0 4px 12px rgba(0 0 0 / 15%);
}

:deep(.tile-icon) {
  font-size: clamp(10rem, 12vw, 14rem);
  color: #198754;
}

/* XS : quelques ajustements d’espace */
@media (max-width: 575.98px) {
  :deep(.tile > *) {
    padding: 0.8rem 0.3rem;
  }
  :deep(.tile-icon) {
    font-size: 2.8rem;
  }
}

/* ============================================
   Mobile-First Responsive Improvements
   ============================================ */

/* Touch-friendly tiles */
:deep(.tile) {
  -webkit-tap-highlight-color: transparent;
  touch-action: manipulation;
}

:deep(.tile:active > *) {
  transform: scale(0.98);
  box-shadow: 0 2px 6px rgba(0 0 0 / 10%);
}

/* Responsive grid gaps */
.row.g-4 {
  --bs-gutter-x: 1.5rem;
  --bs-gutter-y: 1.5rem;
}

@media (max-width: 767.98px) {
  .row.g-4 {
    --bs-gutter-x: 1rem;
    --bs-gutter-y: 1rem;
  }
}

@media (max-width: 575.98px) {
  .row.g-4 {
    --bs-gutter-x: 0.75rem;
    --bs-gutter-y: 0.75rem;
  }
}

/* Tile content responsive */
:deep(.tile .card-title) {
  font-size: clamp(0.9rem, 2.5vw, 1.25rem);
  margin-bottom: 0.25rem;
}

:deep(.tile .text-muted) {
  font-size: clamp(0.7rem, 2vw, 0.875rem);
  line-height: 1.3;
}

/* Landscape mobile - tiles mais compactos */
@media (max-height: 500px) and (orientation: landscape) {
  :deep(.tile) {
    aspect-ratio: 4 / 3;
  }
  
  :deep(.tile)::before {
    padding-top: 75%;
  }
  
  :deep(.tile > *) {
    padding: 0.5rem;
  }
  
  :deep(.tile-icon) {
    font-size: 2rem;
  }
}

/* Container padding responsive */
.tile-layout.container {
  padding-left: 1rem;
  padding-right: 1rem;
}

@media (max-width: 575.98px) {
  .tile-layout.container {
    padding-left: 0.5rem;
    padding-right: 0.5rem;
  }
}
</style>
