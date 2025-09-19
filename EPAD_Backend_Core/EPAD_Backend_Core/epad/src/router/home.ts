import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
  {
    name: 'home',
    path: 'home',
    component: () => import('@/components/home/home-component/home-component.vue'),
    meta: { guest: true, title: 'home', formName: 'Home', checkPrivilege: false }
  }
];

export default routes;
