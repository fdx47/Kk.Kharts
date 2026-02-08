// scr/main.js

import { createApp } from "vue";
import App from "./App.vue";
import { router } from "./router.js";
import { useVirtualDevices } from "./composables/useVirtualDevices.js";

import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import "./assets/responsive.css";

import Vue3Toastify from "vue3-toastify";
import "vue3-toastify/dist/index.css";

import { isRootUser } from "./services/roleUtils.js";

// Filter console logs for non-Root users: keep warn/error, hide log/info/debug
(function installConsoleFilter() {
  try {
    const isRoot = () => isRootUser();
    const orig = {
      log: console.log.bind(console),
      info: console.info.bind(console),
      debug: (console.debug || console.log).bind(console),
      group: (console.group || console.log).bind(console),
      groupCollapsed: (console.groupCollapsed || console.log).bind(console),
      groupEnd: (console.groupEnd || (() => {})).bind(console),
      table: (console.table || console.log).bind(console),
      trace: (console.trace || console.log).bind(console),
    };
    console.log = (...args) => {
      if (isRoot()) orig.log(...args);
    };
    console.info = (...args) => {
      if (isRoot()) orig.info(...args);
    };
    console.debug = (...args) => {
      if (isRoot()) orig.debug(...args);
    };
    console.group = (...args) => {
      if (isRoot()) orig.group(...args);
    };
    console.groupCollapsed = (...args) => {
      if (isRoot()) orig.groupCollapsed(...args);
    };
    console.groupEnd = (...args) => {
      if (isRoot()) orig.groupEnd(...args);
    };
    console.table = (...args) => {
      if (isRoot()) orig.table(...args);
    };
    console.trace = (...args) => {
      if (isRoot()) orig.trace(...args);
    };
  } catch {}
})();

const app = createApp(App);
// initialise les groupes virtuels dès le démarrage
const virtualDevices = useVirtualDevices();
app.provide("virtualDevices", virtualDevices);
app.use(router);

app.use(Vue3Toastify, {
  autoClose: 10000,
});

app.mount("#app");
