import { ref } from "vue";
import { getDevices } from "@/services/apiService.js";

const devices = ref([]);
let pending = null;
let loaded = false;

export function useDevices() {
  async function loadDevices(force = false) {
    if (loaded && !force) {
      return devices.value;
    }
    if (!pending || force) {
      pending = getDevices()
        .then((list) => {
          devices.value = list;
          loaded = true;
          return devices.value;
        })
        .catch((err) => {
          loaded = false;
          devices.value = [];
          throw err;
        })
        .finally(() => {
          pending = null;
        });
    }
    return pending;
  }

  function resetDevices() {
    devices.value = [];
    loaded = false;
    pending = null;
  }

  return { devices, loadDevices, resetDevices };
}

export default useDevices;
