import Vue from "vue";
import App from "./App.vue";
import "./registerServiceWorker";
import { AppRoute } from "./router";
import { store } from "./store";
import i18n from "./i18n";
import Vuelidate from "vuelidate";
import element from "element-ui";
import "element-ui/lib/theme-chalk/index.css";
import locale from "element-ui/lib/locale/lang/vi";
import "../src/assets/css/style.css";
import "@/components/app-component";
import "./$extensions";
import ElSearchTablePagination from "el-search-table-pagination";
import ContextMenu from '@/components/app-component/context-menu';
import { setupHubConnection } from "./startup/hubConnection";
Vue.use(Vuelidate);
Vue.use(ElSearchTablePagination);
Vue.use(element, { size: "small", locale });
Vue.use(ContextMenu);
setupHubConnection();
Vue.config.productionTip = false;
const vm = new Vue({
  router: AppRoute,
  store,
  i18n,
  render: h => h(App)
}).$mount("#app");
window.VueInstance = vm;
