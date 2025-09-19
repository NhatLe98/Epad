import { Module } from 'vuex';
import { isNullOrUndefined } from 'util';

const Store: Module<any, any> = {
  namespaced: true,
  state: {
    app_is_editing: null,
    app_active_form: null,
    menu_is_collapse: null,
  },
  mutations: {
    setMenuIsCollapse(state, menu_is_collapse) {
      state.menu_is_collapse = menu_is_collapse;
      if (isNullOrUndefined(menu_is_collapse)) {
        localStorage.removeItem('menu_is_collapse');
      }
      else {
        localStorage.setItem('menu_is_collapse', menu_is_collapse);
      }
    },
    setAppIsEdit(state, app_is_editing) {
      state.app_is_editing = app_is_editing;
      if (isNullOrUndefined(app_is_editing)) {
        localStorage.removeItem('app_is_editing');
      }
      else {
        localStorage.setItem('app_is_editing', app_is_editing);
      }
    },

    setActiveForm(state, app_active_form) {
      state.app_active_form = app_active_form;
      if (isNullOrUndefined(app_active_form)) {
        localStorage.removeItem('app_active_form');
      }
      else {
        localStorage.setItem('app_active_form', app_active_form);
      }
    }
  },
  getters: {
    getMenuIsCollapse: state => {
      if(isNullOrUndefined(state.menu_is_collapse)) {
        const dummy = localStorage.getItem('menu_is_collapse')
        if (dummy !== "") {
          state.menu_is_collapse = dummy === 'true'
        }
      }
      return state.menu_is_collapse
    },

    getAppIsEditing: state => {
      if (isNullOrUndefined(state.app_is_editing)) {
        const dummy = localStorage.getItem('app_is_editing');
        if (dummy !== "") {
          state.app_is_editing = dummy === 'true';
        }
      }
      return state.app_is_editing;
    },

    getActiveForm: state => {
      if (isNullOrUndefined(state.app_active_form)) {
        const dummy = localStorage.getItem('app_active_form');
        if (dummy !== "") {
          state.app_active_form = dummy;
        }
        else {
          state.app_active_form = 'Home';
        }
      }
      return state.app_active_form;
    }
  }
};

export { Store as Application }