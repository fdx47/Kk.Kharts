import { ref, watch } from "vue";

/**
 * Lie une ref Vue à une clé localStorage.
 *
 * Pour les objets, remplacez l'instance plutôt que de muter ses propriétés
 * afin de déclencher la persistance.
 */
export function useLocalStorage(key, defaultValue) {
  let initial = defaultValue;
  try {
    const raw = localStorage.getItem(key);
    if (raw !== null) initial = JSON.parse(raw);
  } catch {}

  const state = ref(initial);

  watch(
    state,
    (val) => {
      try {
        localStorage.setItem(key, JSON.stringify(val));
      } catch {}
    },
    { deep: true },
  );

  return state;
}

// **Ajoute cette ligne** pour t’assurer qu’il y a aussi un export par défaut
export default useLocalStorage;
