import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
  {
    name: 'system-config',
    path: 'system-config',
    component: () => import('@/components/home/system-config-component/system-config-component-2.vue'),
    meta: { guest: true, title: 'system-config', formName: 'SystemConfig', checkPrivilege: true }
  },
  {
    name: 'service',
    path: 'service',
    component: () => import('@/components/home/service-component/service-component.vue'),
    meta: { guest: true, title: 'service', formName: 'Service', checkPrivilege: true }
  },
  {
    name: 'device-by-service',
    path: 'device-by-service',
    component: () => import('@/components/home/service-component/devices-by-service-component.vue'),
    meta: { guest: true, title: 'device-by-service', formName: 'DevicesByService', checkPrivilege: true }
  },
  {
    name: 'history-user',
    path: 'history-user',
    component: () => import('@/components/home/history-user-component/history-user-component.vue'),
    meta: { guest: true, title: 'history-user', formName: 'HistoryUser', checkPrivilege: true }
    },
    {
        name: 'system-command',
        path: 'system-command',
        component: () => import('@/components/home/system-command-component/system-command-component.vue'),
        meta: { guest: true, title: 'system-command', formName: 'SystemCommand', checkPrivilege: true }
    },
  {
    name: 'tracking-system',
    path: 'tracking-system',
    component: () => import('@/components/home/tracking-system-component/tracking-system-component.vue'),
    meta: { guest: true, title: 'tracking-system', formName: 'TrackingSystem', checkPrivilege: true }
  },
  {
    name: 'list-service',
    path: 'list-service',
    component: () => import('@/components/home/list-service-component/list-service-component.vue'),
    meta: { guest: true, title: 'list-service', formName: 'ListService', checkPrivilege: true }
  },
  {
    name: 'system-config-group-device',
    path: 'system-config-group-device',
    component: () => import('@/components/home/system-config-group-device/system-config-group-device.vue'),
    meta: { guest: true, title: 'system-config-group-device', formName: 'SystemConfigGroupDevice', checkPrivilege: false }
  },
  {
    name: 'personal-access-token',
    path: 'personal-access-token',
    component: () => import('@/components/home/personal-access-token-component/personal-access-token-component.vue'),
    meta: { guest: true, title: 'personal-access-token', formName: 'PersonalAccessToken', checkPrivilege: true }
  },
  {
    name: 'tracking-integrate',
    path: 'tracking-integrate',
    component: () => import('@/components/home/tracking-integrate-component/tracking-integrate-component.vue'),
    meta: { guest: true, title: 'tracking-integrate', formName: 'TrackingIntegrate', checkPrivilege: true }
  },
  {
    name: 'device-history',
    path: 'device-history',
    component: () => import('@/components/home/device-history-component/device-history-component.vue'),
    meta: { guest: true, title: 'device-history', formName: 'DeviceHistory', checkPrivilege: true }
  }
];

export default routes;
