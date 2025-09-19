import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'parking-lot',
        path: 'parking-lot',
        component: () => import('@/components/home/parking-lot-component/parking-lot.vue'),
        meta: { guest: true, title: 'parking-lot', formName: 'ParkingLot', checkPrivilege: true }
    },
    {
        name: 'parking-lot-accessed',
        path: 'parking-lot-accessed',
        component: () => import('@/components/home/parking-lot-accessed-component/parking-lot-accessed.vue'),
        meta: { guest: true, title: 'parking-lot-accessed', formName: 'ParkingLotAccessed', checkPrivilege: true }
    },
    {
        name: 'employee-vehicle',
        path: 'employee-vehicle',
        component: () => import('@/components/home/employee-vehicle-component/employee-vehicle.vue'),
        meta: { guest: true, title: 'employee-vehicle', formName: 'EmployeeVehicle', checkPrivilege: true }
    },
];

export default routes;
