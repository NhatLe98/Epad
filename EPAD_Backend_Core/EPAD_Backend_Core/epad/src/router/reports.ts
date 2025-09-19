import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
  {
    name: 'reports',
    path: 'reports',
    component: () => import('@/components/home/report/report.vue'),
    meta: { guest: true, title: 'reports', formName: 'Reports', checkPrivilege: true }
  }
];

export default routes;
