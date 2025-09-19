import { isNullOrUndefined } from 'util';
import {Token} from '@/store/token';
import {store} from '@/store';
const ACCESS_TOKEN_KEY = 'access_token';

export const getToken = () => {
    // console.log('this token', store.getters['Token/token']);
    // return store.getters['Token/token'];
    // return Token.getters.getToken;
    return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export const setToken = (token) => {
    // store.commit('Token/setToken', token);
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
    if(isNullOrUndefined(token)){
        localStorage.removeItem(ACCESS_TOKEN_KEY);
    }
}

export default { getToken, setToken };