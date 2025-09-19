import { Module } from 'vuex';

const Store: Module<any, any> = {
  namespaced: true,
  state: {
    nav: '',
    reLogin: false,
    $isLoading: 0,
    showBroken: false,
    errorStack: false
  },
  mutations: {
    setReLogin(state, value) {
      state.reLogin = value;
    },
    incrementLoading(state) {
      if (state.$isLoading < 0) {
        state.$isLoading = 1;
      } else {
        state.$isLoading += 1;
      }
    },
    decrementLoading(state) {
      if (state.$isLoading > 0) {
        state.$isLoading -= 1;
      }
    },
    brokenConnect(state) {
      if (true !== state.showBroken) {
        state.showBroken = true;
      }
    },
    closeBrokenConnect(state) {
        state.showBroken = false;
    },
    onErrorStack(state) {
        if(true !== state.errorStack){
            state.errorStack = true;
        }
    },
    offErrorStack(state) {
        state.errorStack = false;
    },
  },
  getters: {
    isLoading(state) {
      return state.$isLoading > 0;
    },
    reLogin(state) {
      return state.reLogin;
    },
    showBrokenConnect(state) {
      return state.showBroken;
    },
    isErrorStack(state) {
        return state.errorStack;
    }
  },
};

export { Store as Misc };
