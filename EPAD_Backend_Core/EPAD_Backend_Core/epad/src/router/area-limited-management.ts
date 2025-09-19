import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'area-limited',
        path: 'area-limited',
        component: () => import('@/components/home/area-limited-component/area-limited-component.vue'),
        meta: { guest: true, title: 'parking-lot', formName: 'ParkingLot', checkPrivilege: true }
    },
    {
        name: 'area-limited-monitoring-screen',
        path: 'area-limited-monitoring-screen',
        component: () => import('@/components/home/area-limited-monitoring-screen-component/area-limited-monitoring-screen-component.vue'),
        meta: { guest: true, title: 'area-limited-monitoring-screen', formName: 'AreaLimitedMonitoring', checkPrivilege: true }
    }
];

export default routes;
