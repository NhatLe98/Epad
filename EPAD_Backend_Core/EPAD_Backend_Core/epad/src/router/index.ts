import Home from '@/views/home/home.vue';
import Router, { RouteConfig } from 'vue-router';
import Vue from 'vue';
import { isNullOrUndefined } from 'util';
import { privilegeDetailApi } from '@/$api/privilege-detail-api';
import { store } from '@/store';
import { Tree } from 'element-ui';
import { error } from 'console';

Vue.use(Router);

let config: RouteConfig[] = [];
let routerName = ['thsLayout','hanwhaCameraTest'];
const IgnoreRoute = ['GetCurrentPrivilege', 'ChangePass', 'GetUserAccountInfo', 'GetRealTimeServerLink', 'CheckPrivilege'];

const files = require.context('.', false, /^((?!index).)*.ts$/, 'sync');

let isUsingNewAttendanceMonitoring = false;
let clientName = "";
Misc.readFileAsync('static/variables/common-utils.json').then(x => {
    if(x.UsingNewAttendanceMonitoring){
        isUsingNewAttendanceMonitoring = x.UsingNewAttendanceMonitoring;
    }
    if(x.ClientName){
        clientName = x.ClientName;
    }
});

files.keys().forEach(fileName => {
    const dummy = files(fileName);
    const router = dummy.default || dummy;

    config = config.concat(router);
});

const routes: RouteConfig[] = [
    {
        name: 'login',
        path: '/login',
        component: () => import('@/views/login/login.vue'),
        meta: { title: 'login', checkPrivilege: false }
    },
    {
        name: 'login-redirect',
        path: '/login-redirect',
        component: () => import('@/views/login-oidc/login-redirect/login-redirect.vue'),
        meta: { title: 'login', formName: '', checkPrivilege: false }
    },
    {
        name: 'login-callback',
        path: '/login-callback',
        component: () => import('@/views/login-oidc/login-callback/login-callback.vue'),
        meta: { title: 'login', formName: '', checkPrivilege: false }
    },
    {
        name: 'signout-oidc',
        path: '/signout-oidc',
        component: () => import('@/views/login-oidc/signout-oidc/signout-oidc.vue'),
        meta: { title: 'login', formName: '', checkPrivilege: false }
    },
    {
        name: 'activate',
        path: '/activate',
        component: () => import('@/views/activate/activate.vue'),
        meta: { title: 'activate', checkPrivilege: false, requiresAuth: false }
    },
    {
        path: '/',
        component: Home,
        meta: { requiresAuth: true, checkPrivilege: false },
        children: config.concat([
            {
                name: 'personal-access-token',
                path: '/personal-access-token',
                component: () => import('@/views/personal-access-token/personal-access-token.vue'),
                meta: { title: 'non-privilege', checkPrivilege: false }
            },
            {
                name: 'non-privilege',
                path: '/non-privilege',
                component: () => import('@/views/layout/non-privilege/non-privilege.vue'),
                meta: { title: 'non-privilege', checkPrivilege: false }
            },
            {
                path: '*',
                component: () => import('@/views/layout/page-not-found/page-not-found.vue'),
                meta: { guest: true, backdrop: true, checkPrivilege: false }
            }
        ])
    }
];

const AppRoute = new Router({
    mode: 'history',
    base: process.env.BASE_URL,
    routes
});

AppRoute.beforeEach(async (to, { }, next) => {
    const access_token = localStorage.getItem('access_token');
    if (to.path === "/activate") {
        return next();
    }
    if (to.path === '/login-redirect') {
        return next();
    }
    if (to.path === '/login-callback') {
        return next();
    } 
    if (to.path === '/signout-oidc') {
        return next();
    }
    if (to.path !== '/login' && Misc.isEmpty(access_token)) {
        return next('login');
    } else if (['', '/'].some((p) => p === to.path)) {
        return next({
            path: clientName == "Mondelez" ? '/factory-user-monitoring' : '/dashboard',
        });
    } else {
        await loadResource(next, to, true);
    }
});


async function loadResource(next, to, requireAuth) {
    if (true === requireAuth && to.path !== '/login') {
        await store.dispatch('HumanResource/loadResource').catch((error) => {
            console.error('Lỗi khi load resource', error);
            return next({
                path: '/login',
                query: { redirect: to.path },
            });
        });

        await store.dispatch('AppHost/loadConfig').catch((error) => {
            console.error('Lỗi khi load config', error);
            return next({
                path: '/login',
                query: { redirect: to.path },
            });
        });
    }

    if (!Misc.isEmpty(next) && next instanceof Function) {
        // console.log(clientName, to)
        if (to.meta.checkPrivilege == false || !Misc.isEmpty(to.meta.role)) {
            if(clientName == "Mondelez" && (to.path == "/home" || to.path == "/dashboard")){
                return next('/factory-user-monitoring');
            }else{
                return next();
            }
        }
        else {
            await privilegeDetailApi.CheckPrivilege(to.meta.formName).then(rs => {
                if (rs.data == true || routerName.includes(to.meta.formName)) {
                    if(to.path == "/attendance-monitoring" && isUsingNewAttendanceMonitoring){
                        next('/attendance-monitoring-2');
                    }else if(clientName == "Mondelez" && (to.path == "/home" || to.path == "/dashboard")){
                        next('/factory-user-monitoring');
                    }else{
                        next();
                    }
                }
                else {
                    next('/non-privilege');
                }
            })
                .catch(() => {
                    clientName == "Mondelez" ? next('/factory-user-monitoring') : next('/dashboard');
                })
        }
    }
}

AppRoute.afterEach((to, from) => {
    sessionStorage.setItem('app_form_active', to.meta.formName);
})

export { AppRoute, IgnoreRoute };
