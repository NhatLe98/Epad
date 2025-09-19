import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'ac-sync-user',
        path: 'ac-sync-user',
        component: () => import('@/components/home/ac-sync-user-component/ac-synch-user-component.vue'),
        meta: { guest: true, title: 'ac-sync-user', formName: 'ACSynchUser', checkPrivilege: true }
    },
    {
        name: 'ac-area',
        path: 'ac-area',
        component: () => import('@/components/home/ac-area-component/ac-area-component.vue'),
        meta: { guest: true, title: 'ac-area', formName: 'ACArea', checkPrivilege: true }
    },
    {
        name: 'ac-door',
        path: 'ac-door',
        component: () => import('@/components/home/ac-door-component/ac-door-component.vue'),
        meta: { guest: true, title: 'ac-door', formName: 'ACDoor', checkPrivilege: true }
    },
    {
        name: 'ac-door-setting',
        path: 'ac-door-setting',
        component: () => import('@/components/home/ac-door-setting-component/ac-door-setting-component.vue'),
        meta: { guest: true, title: 'ac-door-setting', formName: 'ACDoorSetting', checkPrivilege: true }
    },
    {
        name: 'ac-area-door',
        path: 'ac-area-door',
        component: () => import('@/components/home/ac-area-door-component/ac-area-door-component.vue'),
        meta: { guest: true, title: 'ac-area-door', formName: 'ACAreaDoor', checkPrivilege: true }
    },
    {
        name: 'ac-door-device',
        path: 'ac-door-device',
        component: () => import('@/components/home/ac-door-device-component/ac-door-device-component.vue'),
        meta: { guest: true, title: 'ac-door-device', formName: 'ACDoorDevice', checkPrivilege: true }
    },
    {
        name: 'ac-timezone',
        path: 'ac-timezone',
        component: () => import('@/components/home/ac-timezone-component-new/ac-timezone-component-new.vue'),
        meta: { guest: true, title: 'ac-timezone', formName: 'ACTimezone', checkPrivilege: true }
    },
    {
        name: 'ac-group',
        path: 'ac-group',
        component: () => import('@/components/home/ac-group-component/ac-group-component.vue'),
        meta: { guest: true, title: 'ac-group', formName: 'ACGroup', checkPrivilege: true }
    },
    {
        name: 'ac-log-monitoring',
        path: 'ac-log-monitoring',
        component: () => import('@/components/home/ac-log-monitoring-component/ac-log-monitoring-component.vue'),
        meta: { guest: true, title: 'ac-log-monitoring', formName: 'ACLogMonitoring', checkPrivilege: true }
    },
    {
        name: 'ac-log-history',
        path: 'ac-log-history',
        component: () => import('@/components/home/ac-log-history-component/ac-log-history-component.vue'),
        meta: { guest: true, title: 'ac-log-history', formName: 'ACLogHistory', checkPrivilege: true }
    },
    {
        name: 'ac-door-management',
        path: 'ac-door-management',
        component: () => import('@/components/home/ac-door-management-component/ac-door-management-component.vue'),
        meta: { guest: true, title: 'ac-door-management', formName: 'ACDoorManagement', checkPrivilege: true }
    },
    {
        name: 'ac-holiday',
        path: 'ac-holiday',
        component: () => import('@/components/home/ac-holiday-component/ac-holiday-component.vue'),
        meta: { guest: true, title: 'ac-holiday', formName: 'ACHoliday', checkPrivilege: true }
    },
    // {
    //     name: 'ac-usermaster-log',
    //     path: 'ac-usermaster-log',
    //     component: () => import('@/components/home/ac-usermaster-component/ac-usermaster-component.vue'),
    //     meta: { guest: true, title: 'ac-usermaster', formName: 'ACUserMasterLog', checkPrivilege: true }
    // },
    {
        name: 'ac-accessed-group',
        path: 'ac-accessed-group',
        component: () => import('@/components/home/ac-accessed-group-component/ac-accessed-group-component.vue'),
        meta: { guest: true, title: 'ac-accessed-group', formName: 'ACAccessedGroup', checkPrivilege: true }
    },
    {
        name: 'ac-department-accessed-group',
        path: 'ac-department-accessed-group',
        component: () => import('@/components/home/ac-department-accessed-group-component/ac-department-accessed-group-component.vue'),
        meta: { guest: true, title: 'ac-department-accessed-group', formName: 'ACDepartmentAccessedGroup', checkPrivilege: true }
    },
];

export default routes;
