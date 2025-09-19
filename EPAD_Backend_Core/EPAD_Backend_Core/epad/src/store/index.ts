import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

const allModules = require.context('.', true, /^((?!index).)*.ts$/, 'sync');
const loaded = loadModule();

const store = new Vuex.Store({
    modules: loaded.modules
  });

if (module.hot) {
    const accepts = allModules.keys();
    module.hot.accept(accepts, () => {
      store.hotUpdate({
        modules: loadModule().modules
      });
    });
  }

function loadModule() {
    const dummy = {};
    let persisted: string[] = [];
  
    allModules.keys().forEach(file => {
      const temp = allModules(file);
      Object.assign(dummy, temp);
    });
    return { modules: dummy, persisted: persisted };
  }

  export { store };
