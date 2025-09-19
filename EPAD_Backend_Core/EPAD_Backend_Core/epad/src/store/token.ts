import { Module } from 'vuex';
import { isNullOrUndefined } from 'util';

const Store: Module<any, any> = {
  namespaced: true,
  state: {
    token: null
  },
  mutations: {
    setToken(state, token) {
      state.token = token;
      if (isNullOrUndefined(token)) {
        localStorage.removeItem('access_token');
      }
      else {
        localStorage.setItem('access_token', token);
      }
    }
  },
  getters: {
    getToken: state => {
      if (isNullOrUndefined(state.token)) {
        const dummy = localStorage.getItem('access_token');
        if (dummy.toString() !== "") {
          state.token = dummy;
        }
      }
      return state.token;
    }
  }
};

export { Store as Token }