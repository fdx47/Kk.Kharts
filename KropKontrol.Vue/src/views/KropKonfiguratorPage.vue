<template>







  <DefaultLayout







    :hideSidebar="false"







    :devices="sidebarParcels"







    :selectedDevice="selectedSidebarParcel"







    :showAddButton="false"







    showBack







    @back="goLanding"







    @logout="logout"







    @select-device="selectParcelFromSidebar"







    @delete-device="requestParcelDeletion"







  >







    <template #title>Krop Konfigurator</template>







    <div class="container py-3 krop-konfigurator">







      <div class="row g-3 align-items-start">







        <div class="col-12 col-lg-7">







          <div class="card shadow-sm">







            <div class="card-body">







              <h5 class="card-title mb-3">Déclaration d'une nouvelle parcelle</h5>







              <div







                v-if="feedbackMessage"







                :class="[







                  'alert',







                  feedbackType === 'success' ? 'alert-success' : 'alert-warning',







                  'py-2',







                  'mb-3',







                ]"







              >







                {{ feedbackMessage }}







              </div>







              <form @submit.prevent="saveParcel" class="row g-3">







                <div class="col-12">







                  <label for="parcel-name" class="form-label">Nom de la parcelle</label>







                  <input







                    id="parcel-name"







                    v-model="form.parcelName"







                    type="text"







                    class="form-control"







                    :class="{ 'is-invalid': errors.parcelName }"







                    placeholder="Par exemple : Tunnel 4 ou Serre Est"







                  />







                  <div v-if="errors.parcelName" class="invalid-feedback">







                    {{ errors.parcelName }}







                  </div>







                </div>







                <div class="col-12 col-md-6">







                  <label for="parcel-species" class="form-label">Espèce</label>







                  <select







                    id="parcel-species"







                    v-model="form.species"







                    class="form-select"







                    :class="{ 'is-invalid': errors.species }"







                  >







                    <option value="" disabled>Sélectionnez une Espèce</option>







                    <option







                      v-for="option in speciesOptions"







                      :key="option.value"







                      :value="option.value"







                    >







                      {{ option.label }}







                    </option>







                  </select>







                  <div v-if="errors.species" class="invalid-feedback">







                    {{ errors.species }}







                  </div>







                </div>







                <div class="col-12 col-md-6">







                  <label for="parcel-culture-type" class="form-label">Type de culture</label>







                  <select







                    id="parcel-culture-type"







                    v-model="form.cultureType"







                    class="form-select"







                    :class="{ 'is-invalid': errors.cultureType }"







                  >







                    <option value="" disabled>Sélectionnez un type</option>







                    <option







                      v-for="option in cultureTypeOptions"







                      :key="option.value"







                      :value="option.value"







                    >







                      {{ option.label }}







                    </option>







                  </select>







                  <div v-if="errors.cultureType" class="invalid-feedback">







                    {{ errors.cultureType }}







                  </div>







                </div>







                <div class="col-12 col-md-6">







                  <label for="parcel-planting-date" class="form-label"







                    >Date de plantation</label







                  >







                  <input







                    id="parcel-planting-date"







                    v-model="form.plantingDate"







                    type="date"







                    class="form-control"







                    :class="{ 'is-invalid': errors.plantingDate }"







                  />







                  <div v-if="errors.plantingDate" class="invalid-feedback">







                    {{ errors.plantingDate }}







                  </div>







                </div>







                <div class="col-12 col-md-6">







                  <label for="parcel-harvest-date" class="form-label"







                    >Fin supposée de culture</label







                  >







                  <input







                    id="parcel-harvest-date"







                    v-model="form.harvestDate"







                    type="date"







                    class="form-control"







                    :class="{ 'is-invalid': errors.harvestDate }"







                  />







                  <div v-if="errors.harvestDate" class="invalid-feedback">







                    {{ errors.harvestDate }}







                  </div>







                </div>







                <div class="col-12">







                  <label for="parcel-substrate" class="form-label">Type de substrat</label>







                  <select







                    id="parcel-substrate"







                    v-model="form.substrateType"







                    class="form-select"







                  >







                    <option value="" disabled>Choisissez un type de substrat</option>







                    <option v-for="option in substrateOptions" :key="option.value" :value="option.value">







                      {{ option.label }}







                    </option>







                  </select>







                </div>







                <div class="col-12">







                  <label class="form-label">Kapteurs associés à  la parcelle</label>







                  <div class="device-selector border rounded p-3">







                    <div







                      v-if="isLoadingDevices"







                      class="d-flex align-items-center gap-2 text-muted small"







                    >







                      <span class="spinner-border spinner-border-sm" role="status" />







                      <span>Chargement des Kapteurs disponibles...</span>







                    </div>







                    <div







                      v-else-if="deviceOptions.length === 0"







                      class="text-muted small"







                    >







                      Aucun Kapteur disponible pour le moment.







                    </div>







                    <div v-else class="row row-cols-1 row-cols-md-2 g-2">







                      <div v-for="option in deviceOptions" :key="option.value" class="col">







                        <div class="form-check">







                          <input







                            :id="`device-${option.value}`"







                            v-model="form.devices"







                            class="form-check-input"







                            type="checkbox"







                            :value="option.value"







                          />







                          <label class="form-check-label" :for="`device-${option.value}`">







                            <span class="fw-semibold">{{ option.label }}</span>







                            <span class="d-block text-muted small">{{ option.subLabel }}</span>







                          </label>







                        </div>







                      </div>







                    </div>







                  </div>







                </div>







                <div class="col-12 d-flex justify-content-end">







                  <button class="btn btn-success" type="submit">







                    Enregistrer la parcelle







                  </button>







                </div>







              </form>







            </div>







          </div>







        </div>







        <div class="col-12 col-lg-5">







          <div class="card shadow-sm">







            <div class="card-body">







              <h5 class="card-title mb-3">Détails de la parcelle</h5>







              <div v-if="!selectedParcel" class="text-muted small">







                Sélectionnez une parcelle dans la barre latérale pour afficher ses informations.







              </div>







              <div v-else class="d-flex flex-column gap-2">







                <div>







                  <div class="fw-semibold">{{ selectedParcel.parcelName || "Parcelle sans nom" }}</div>







                  <div class="small text-muted">ID {{ selectedParcel.id }}</div>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Espèce</span>







                  <span class="detail-value">{{ selectedParcel.species || "Non renseignée" }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Type de culture</span>







                  <span class="detail-value">{{ selectedParcel.cultureType || "Non renseigné" }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Date de plantation</span>







                  <span class="detail-value">{{ formatDate(selectedParcel.plantingDate) }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Fin de culture</span>







                  <span class="detail-value">{{ formatDate(selectedParcel.harvestDate) }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Substrat</span>







                  <span class="detail-value">{{ selectedParcel.substrateType || "Non renseigné" }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Kapteurs associés</span>







                  <span class="detail-value">{{ selectedParcel.devices?.length ? deviceLabelList(selectedParcel.devices) : "Aucun Kapteur associé" }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Créée le</span>







                  <span class="detail-value">{{ formatDateTime(selectedParcel.createdAt) }}</span>







                </div>







                <div class="detail-row">







                  <span class="detail-label">Dernière mise à jour</span>







                  <span class="detail-value">{{ formatDateTime(selectedParcel.updatedAt) }}</span>







                </div>







              </div>







            </div>







          </div>







        </div>







      </div>







    </div>







  </DefaultLayout>







</template>







<script setup>







import { reactive, ref, computed, onMounted, inject } from "vue";







import DefaultLayout from "@/views/layout/DefaultLayout.vue";







import { useNavigation } from "../composables/useNavigation.js";







import { useAuth } from "../composables/useAuth.js";







import { useDevices } from "@/composables/useDevices.js";







import { getSubstrateFieldMapping, GROUP_VAR_SEPARATOR } from "@/utils/getGroupLabelMap.js";







const STORAGE_KEY = "krop-konfigurator-parcelles";







const { goLanding } = useNavigation();







const { logout } = useAuth();







const { devices: fetchedDevices, loadDevices } = useDevices();







const virtualDevicesApi = inject("virtualDevices", null);







const speciesOptions = [







  { value: "Aubergine", label: "Aubergine" },







  { value: "Concombre", label: "Concombre" },







  { value: "Fraise", label: "Fraise" },







  { value: "Framboise", label: "Framboise" },







  { value: "Poivron", label: "Poivron" },







  { value: "Tomate", label: "Tomate" },







];







const cultureTypeOptions = [







  { value: "Production de fruit", label: "Production de fruit" },







  { value: "Production de plant", label: "Production de plant" },







];







const substrateOptions = [







  { value: "Minerale", label: "Minérale" },







  { value: "Organique", label: "Organique" },







  { value: "Melange de Tourbe", label: "Mélange de Tourbe" },







  { value: "Fibre de Coco", label: "Fibre de Coco" },







  { value: "Laine Minerale", label: "Laine Minérale" },







  { value: "Perlite", label: "Perlite" },







];







const form = reactive({







  parcelName: "",







  species: "",







  cultureType: "",







  plantingDate: "",







  harvestDate: "",







  substrateType: "",







  devices: [],







});







const errors = reactive({







  parcelName: "",







  species: "",







  cultureType: "",







  plantingDate: "",







  harvestDate: "",







});







const feedbackMessage = ref("");







const feedbackType = ref("success");







const parcels = ref([]);







const isLoadingDevices = ref(false);







const selectedParcelId = ref(null);







const deviceOptions = computed(() =>







  [...(fetchedDevices.value || [])]







    .filter(







      (device) =>







        !!device?.devEui && device.devEui !== "0000000000000000"







    )







    .map((device) => ({







      value: device.devEui,







      label: device.description || device.deviceName || device.name || device.devEui,







      subLabel: device.devEui,







    }))







    .sort((a, b) => a.label.localeCompare(b.label))







);







const deviceLabelMap = computed(() => {







  const map = new Map();







  for (const option of deviceOptions.value) {







    map.set(option.value, option.label);







  }







  return map;







});







const savedParcels = computed(() =>







  [...parcels.value].sort(







    (a, b) => new Date(b.updatedAt || b.createdAt).getTime() - new Date(a.updatedAt || a.createdAt).getTime()







  )







);







const sidebarParcels = computed(() =>







  savedParcels.value.map((parcel) => ({







    devEui: parcel.id,







    description: parcel.parcelName || "Parcelle sans nom",







    canDelete: true,







  })),







);







const selectedParcel = computed(() =>







  parcels.value.find((parcel) => parcel.id === selectedParcelId.value) || null,







);







const selectedSidebarParcel = computed(() =>







  sidebarParcels.value.find((item) => item.devEui === selectedParcelId.value) || null,







);







function resetErrors() {







  errors.parcelName = "";







  errors.species = "";







  errors.cultureType = "";







  errors.plantingDate = "";







  errors.harvestDate = "";







}







function validateForm() {







  resetErrors();







  let valid = true;







  if (!form.parcelName.trim()) {







    errors.parcelName = "Le nom de la parcelle est obligatoire.";







    valid = false;







  }







  if (!form.species) {







    errors.species = "Veuillez sélectionner une Espèce.";







    valid = false;







  }







  if (!form.cultureType) {







    errors.cultureType = "Veuillez sélectionner un type de culture.";







    valid = false;







  }







  if (!form.plantingDate) {







    errors.plantingDate = "La date de plantation est obligatoire.";







    valid = false;







  }







  if (form.harvestDate && form.plantingDate && form.harvestDate < form.plantingDate) {







    errors.harvestDate = "La date de fin doit étre postérieure é la plantation.";







    valid = false;







  }







  return valid;







}







function generateParcelId() {







  if (typeof crypto !== "undefined" && crypto.randomUUID) {







    return crypto.randomUUID();







  }







  const rnd = Math.random().toString(16).slice(2, 10);







  return `parcel-${Date.now()}-${rnd}`;







}







function persistParcels() {







  try {







    localStorage.setItem(STORAGE_KEY, JSON.stringify(parcels.value));







  } catch (err) {







    console.error("KropKonfigurator - impossible de sauvegarder les parcelles", err);







    feedbackType.value = "warning";







    feedbackMessage.value =







      "La parcelle a été créée mais n'a pas pu étre sauvegardée dans le navigateur.";







  }







}







function saveParcel() {







  feedbackMessage.value = "";







  feedbackType.value = "success";







  if (!validateForm()) {







    feedbackType.value = "warning";







    feedbackMessage.value = "Merci de compléter les champs obligatoires avant d'enregistrer.";







    return;







  }







  const timestamp = new Date().toISOString();







  const parcel = {







    id: generateParcelId(),







    parcelName: form.parcelName.trim(),







    species: form.species,







    cultureType: form.cultureType,







    plantingDate: form.plantingDate,







    harvestDate: form.harvestDate,







    substrateType: form.substrateType,







    devices: [...form.devices],







    createdAt: timestamp,







    updatedAt: timestamp,







  };







  parcels.value = [parcel, ...parcels.value];







  selectedParcelId.value = parcel.id;







  persistParcels();







  feedbackType.value = "success";







  const baseMessage = `Parcelle "${parcel.parcelName || parcel.id}" sauvegardée avec succès.`;







  const virtualDeviceMessage = handleVirtualGroupCreation(parcel);







  feedbackMessage.value = virtualDeviceMessage ? `${baseMessage} ${virtualDeviceMessage}` : baseMessage;







  resetForm();







}







function handleVirtualGroupCreation(parcel) {

  if (!virtualDevicesApi || !Array.isArray(parcel.devices) || parcel.devices.length <= 1) {

    return "";

  }



  try {

    const { addGroup, updateGroup, groups } = virtualDevicesApi;

    if (!addGroup || !updateGroup || !groups) return "";



    const mapping = getSubstrateFieldMapping(parcel.substrateType);

    const defaultVariablesFromSubstrate =

      mapping && Array.isArray(parcel.devices)

        ? Array.from(

            new Set(

              parcel.devices.flatMap((devEui) => {

                const keys = [];

                if (mapping.vwc)

                  keys.push(`${mapping.vwc}${GROUP_VAR_SEPARATOR}${devEui}`);

                if (mapping.ec)

                  keys.push(`${mapping.ec}${GROUP_VAR_SEPARATOR}${devEui}`);

                return keys;

              }),

            ),

          ).filter(Boolean)

        : [];

    const defaultVariableSet = new Set(defaultVariablesFromSubstrate);



    parcel.devices.forEach((devEui) => {

      const device =

        (fetchedDevices.value || []).find((d) => d.devEui === devEui) || null;

      const modelNumber = Number(device?.model ?? device?.deviceId);

      if (Number.isNaN(modelNumber)) return;

      if (modelNumber === 2 || modelNumber === 7) {

        defaultVariableSet.add(`temperature${GROUP_VAR_SEPARATOR}${devEui}`);

        defaultVariableSet.add(`humidity${GROUP_VAR_SEPARATOR}${devEui}`);

      }

      if (modelNumber === 7) {

        defaultVariableSet.add(`volumeDelta_mm${GROUP_VAR_SEPARATOR}${devEui}`);

      }

    });



    const metadata = {

      parcel: {

        id: parcel.id,

        name: parcel.parcelName || parcel.id,

        species: parcel.species,

        cultureType: parcel.cultureType,

        substrateType: parcel.substrateType,

        plantingDate: parcel.plantingDate,

        harvestDate: parcel.harvestDate,

      },

      defaultVariables: Array.from(defaultVariableSet),

    };



    const deviceModels = {};

    parcel.devices.forEach((devEui) => {

      const device = (fetchedDevices.value || []).find((d) => d.devEui === devEui);

      if (device && device.model != null) {

        deviceModels[devEui] = device.model;

      }

    });



    const payload = {

      id: parcel.id,

      name: parcel.parcelName || parcel.id,

      devEuis: [...parcel.devices],

      deviceModels,

      metadata,

    };



    const existingGroup = groups.value?.find((g) => g.id === parcel.id);

    if (existingGroup) {

      updateGroup(parcel.id, payload);

    } else {

      addGroup(payload);

    }



    return " Un Kapteur virtuel a été créé pour cette parcelle.";

  } catch (err) {

    console.error("KropKonfigurator - impossible de créer le Kapteur virtuel", err);

    return "";

  }

}



function resetForm() {







  form.parcelName = "";







  form.species = "";







  form.cultureType = "";







  form.plantingDate = "";







  form.harvestDate = "";







  form.substrateType = "";







  form.devices = [];







}







function loadParcelsFromStorage() {







  try {







    const raw = localStorage.getItem(STORAGE_KEY);







    if (!raw) return;







    const parsed = JSON.parse(raw);







    if (Array.isArray(parsed)) {







      parcels.value = parsed.map((parcel) => ({







        ...parcel,







        devices: Array.isArray(parcel.devices) ? parcel.devices : [],







      }));







      const firstParcelId = parcels.value.length ? parcels.value[0].id : null;







      if (!firstParcelId) {







        selectedParcelId.value = null;







      } else if (







        !selectedParcelId.value ||







        !parcels.value.some((p) => p.id === selectedParcelId.value)







      ) {







        selectedParcelId.value = firstParcelId;







      }







    }







  } catch (err) {







    console.error("KropKonfigurator - lecture du stockage impossible", err);







  }







}







function formatDate(value) {







  if (!value) return "Non renseigné";







  const date = new Date(value);







  if (Number.isNaN(date.getTime())) return value;







  return date.toLocaleDateString("fr-FR");







}







function formatDateTime(value) {







  if (!value) return "Non renseignée";







  const date = new Date(value);







  if (Number.isNaN(date.getTime())) return value;







  return date.toLocaleString("fr-FR");







}







function deviceLabelList(selectedIds = []) {







  return selectedIds







    .map((id) => deviceLabelMap.value.get(id) || id)







    .join(", ");







}







function selectParcelFromSidebar(dev) {







  selectedParcelId.value = dev?.devEui || null;







}







function requestParcelDeletion(dev) {







  const parcelId = dev?.devEui;







  if (!parcelId) return;







  const parcel = parcels.value.find((p) => p.id === parcelId);







  if (!parcel) return;







  if (!confirm(`Supprimer la parcelle "${parcel.parcelName || parcelId}" ?`)) return;







  parcels.value = parcels.value.filter((p) => p.id !== parcelId);







  persistParcels();







  if (virtualDevicesApi?.removeGroup) {







    virtualDevicesApi.removeGroup(parcelId);







  }







  const remaining = savedParcels.value;







  selectedParcelId.value = remaining.length ? remaining[0].id : null;







  feedbackType.value = "success";







  feedbackMessage.value = `Parcelle "${parcel.parcelName || parcelId}" supprimée.`;







}







onMounted(async () => {







  loadParcelsFromStorage();







  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {







    isLoadingDevices.value = true;







    await loadDevices().catch((err) => {







      console.error("KropKonfigurator - impossible de charger les devices", err);







    });







    isLoadingDevices.value = false;







  }







});







</script>







<style scoped>







.krop-konfigurator .card {







  border: none;







}







.krop-konfigurator .card-title {







  color: #33691e;







  font-weight: 600;







}







.device-selector {







  min-height: 72px;







  background: #fafafa;







}







.parcel-list .list-group-item {







  border-color: rgba(0, 0, 0, 0.05);







}







</style>







