import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
  {
    name: 'dashboard',
    path: 'dashboard',
    component: () => import('@/components/home/dashboard-component/dashboard-component.vue'),
    meta: { guest: true, title: 'dashboard', formName: 'Dashboard', checkPrivilege: false }
  }
];

export default routes;
