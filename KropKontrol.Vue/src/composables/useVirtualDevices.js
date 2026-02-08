import { computed } from "vue";
import useLocalStorage from "./useLocalStorage.js";
import { getUserIdFromToken } from "../services/authService.js";

let groups;

function initGroups() {
  const STORAGE_KEY = "virtualGroups_" + (getUserIdFromToken() ?? "guest");
  groups = useLocalStorage(STORAGE_KEY, []);
}

export function useVirtualDevices() {
  if (!groups) initGroups();

  const virtualDevices = computed(() =>
    groups.value.map((g) => ({
      devEui: `group-${g.id}`,
      description: g.name,
      isVirtual: true,
      group: g,
    })),
  );

  function addGroup(group) {
    groups.value.push({
      id: group.id || Date.now().toString(),
      name: group.name,
      devEuis: group.devEuis || [],
      deviceModels: group.deviceModels || {},
      metadata: group.metadata || null,
    });
  }

  function updateGroup(id, payload) {
    const idx = groups.value.findIndex((g) => g.id === id);
    if (idx !== -1) groups.value[idx] = { ...groups.value[idx], ...payload };
  }

  function removeGroup(id) {
    groups.value = groups.value.filter((g) => g.id !== id);
  }

  const api = {
    get groups() {
      return groups;
    },
    set groups(val) {
      groups = val;
    },
    virtualDevices,
    addGroup,
    updateGroup,
    removeGroup,
    reset,
  };

  function reset() {
    initGroups();
    api.groups = groups;
  }

  return api;
}

export default useVirtualDevices;
