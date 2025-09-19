import { Module } from 'vuex';
import { isNullOrUndefined } from 'util';

const Store: Module<any, any> = {
    namespaced: true,
    state: {
        appHost: null,
        isUsingBasicMenu: null,
        maxDeviceNumber: null
    },
    mutations: {
        setAppHost(state, appHost) {
            state.appHost = appHost;
            if (isNullOrUndefined(appHost)) {
                localStorage.removeItem('app_host');
            }
            else {
                localStorage.setItem('app_host', appHost);
            }
        },
        setBasicMenu(state, data) {
            state.isUsingBasicMenu = data.UsingBasicMenu;
            state.maxDeviceNumber = data.MaxDeviceNumber;
        }
    },
    getters: {
        getAppHost: state => {
            if (isNullOrUndefined(state.appHost)) {
                const dummy = localStorage.getItem('app_host');
                if (isNullOrUndefined(dummy)) {


                }
                if (dummy?.toString() !== "") {
                    state.appHost = dummy;
                }
            }
            return state.appHost || {};
        },
        getUsingBasicMenu: state => {
            return state.isUsingBasicMenu || true;
        },
        getMaxDeviceNumber: state => {
            return state.maxDeviceNumber || 3;
        }
    },
    actions: {
        async loadResource({ state, commit }) {
            const { appHost } = state;
            if (isNullOrUndefined(appHost) || Object.keys(appHost).length === 0) {
                const data = await Misc.readFileAsync('static/variables/app.host.json');
                commit('setAppHost', JSON.stringify(data));
            }
        },
        async loadConfig({ state, commit }) {
            const { appConfig } = state;
            if (isNullOrUndefined(appConfig) || Object.keys(appConfig).length === 0) {
                const data = await Misc.readFileAsync('static/variables/common-utils.json');
                commit('setBasicMenu', data);
            }
        }
    }
};

export { Store as AppHost }