import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
  {
    name: 'machine',
    path: 'machine',
    component: () => import('@/components/home/machine-component/machine-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'Machine', checkPrivilege: true }
  },
  {
    name: 'machine-license',
    path: 'machine-license',
    component: () => import('@/components/home/machine-license-component/machine-license-component.vue'),
    meta: { guest: true, title: 'machine-license', formName: 'MachineLicense', checkPrivilege: true }
  },
  {
    name: 'device-info',
    path: 'device-info',
    component: () => import('@/components/home/device-info-component/device-info-component.vue'),
    meta: { guest: true, title: 'device-info', formName: 'DeviceInfo', checkPrivilege: true }
  },
  {
    name: 'device-by-department',
    path: 'device-by-department',
    component: () =>import('@/components/home/device-component/device-by-department-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'DevicesByDepartment', checkPrivilege: true }
  },
  {
    name: 'log-monitoring-online',
    path: 'log-monitoring-online',
    component: () => import('@/components/home/log-monitoring-component/log-monitoring-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'LogMonitoring', checkPrivilege: true }
  },
  {
    name: 'log-realtime-monitoring-online',
    path: 'log-realtime-monitoring-online',
    component: () => import('@/components/home/log-monitoring-component/log-realtime-monitoring-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'LogRealtimeMonitoring', checkPrivilege: true }
  },
  {
    name: 'total-employee-present',
    path: 'total-employee-present',
    component: () => import('@/components/home/total-employee-present/total-employee-present.vue'),
    meta: { guest: true, title: 'machine', formName: 'TotalEmployeePresent', checkPrivilege: true }
  },
  {
    name: 'view-log-attend',
    path: 'view-log-attend',
    component: () => import('@/components/home/view-log-attend-component/view-log-attend-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'ViewLogAttend', checkPrivilege: true }
  },
  {
    name: 'group-device',
    path: 'group-device',
    component: () => import('@/components/home/group-device-component/group-device-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'GroupDevice', checkPrivilege: true }
  },
  {
    name: 'list-group-device',
    path: 'ListGroupDevice',
    component: () => import('@/components/home/list-group-device-component/list-group-device-component.vue'),
    meta: { guest: true, title: 'machine', formName: 'ListGroupDevice', checkPrivilege: true }
  }
  ,
  {
    name: 'controller',
    path: 'Controller',
    component: () => import('@/components/home/controller-component/controller-component.vue'),
    meta: { guest: true, title: 'controller', formName: 'Controller', checkPrivilege: true }
  },
  {
    name: 'relay-controller',
    path: 'RelayController',
    component: () => import('@/components/home/relay-controller-component/relay-controller.vue'),
    meta: { guest: true, title: 'relaycontroller', formName: 'RelayController', checkPrivilege: true }
  },
  {
    name: 'camera-control',
    path: 'CameraControl',
    component: () => import('@/components/home/camera-control-component/camera-control.vue'),
    meta: { guest: true, title: 'CameraControl', formName: 'CameraControl', checkPrivilege: true }
  },
  {
    name: 'printer-control',
    path: 'PrinterControl',
    component: () => import('@/components/home/printer-control/index.vue'),
    meta: { guest: false, title: 'PrinterControl', formName: 'PrinterControl', checkPrivilege: true }
  },
  {
    name: 'thsLayout',
    path: 'thsLayout',
    component: () => import('@/views/thsLayout/thsLayout.vue'),
    meta: { guest: false, title: 'thsLayout', formName: 'thsLayout', checkPrivilege: true }
  },
  {
    name: 'hanwhaCameraTest',
    path: 'hanwhaCameraTest',
    component: () => import('@/views/hanwhaCameraTest/hanwhaCameraTest.vue'),
    meta: { guest: false, title: 'hanwhaCameraTest', formName: 'hanwhaCameraTest', checkPrivilege: true }
  }
];

export default routes;
