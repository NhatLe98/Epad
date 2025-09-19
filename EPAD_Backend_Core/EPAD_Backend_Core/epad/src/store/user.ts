import { Module } from 'vuex';
import { isNullOrUndefined } from 'util';

const Store: Module<any, any> = {
    namespaced: true,
    state: {
        user: null
    },
    mutations: {
        setUser(state, user){
            state.user = user;
            if(isNullOrUndefined(user)){
                localStorage.removeItem('user');
            }
            else {
                localStorage.setItem('user', user);
            }
        }
    },
    getters: {
        getUser: state => {
            if(isNullOrUndefined(state.user)){
                const dummy = localStorage.getItem('user');
                if(dummy.toString() !== ""){
                    state.user = dummy;
                }
            }
            return state.user;
        }
    }
};

export { Store as User }