import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'area-group',
        path: 'area-group',
        component: () => import('@/components/home/area-group-component/area-group.vue'),
        meta: { guest: true, title: 'area-group', formName: 'AreaGroup', checkPrivilege: true }
    },
    {
        name: 'gates',
        path: 'gates',
        component: () => import('@/components/home/gates-component/gates.vue'),
        meta: { guest: true, title: 'gates', formName: 'Gates', checkPrivilege: true }
    },
    {
        name: 'accessed-group',
        path: 'accessed-group',
        component: () => import('@/components/home/accessed-group-component/accessed-group.vue'),
        meta: { guest: true, title: 'accessed-group', formName: 'AccessedGroup', checkPrivilege: true }
    },
    {
        name: 'employee-accessed-group',
        path: 'employee-accessed-group',
        component: () => import('@/components/home/employee-accessed-group-component/employee-accessed-group.vue'),
        meta: { guest: true, title: 'employee-accessed-group', formName: 'EmployeeAccessedGroup', checkPrivilege: true }
    },
    {
        name: 'department-accessed-group',
        path: 'department-accessed-group',
        component: () => import('@/components/home/department-accessed-group-component/department-accessed-group.vue'),
        meta: { guest: true, title: 'department-accessed-group', formName: 'DepartmentAccessedGroup', checkPrivilege: true }
    },
    {
        name: 'customer-monitoring-page',
        path: 'customer-monitoring-page',
        component: () => import('@/components/home/customer-monitoring/customer-monitoring.vue'),
        meta: { guest: true, title: 'customer-monitoring-page', formName: 'CustomerMonitoringPage', checkPrivilege: true }
    },
    {
        name: 'general-monitoring-screen',
        path: 'general-monitoring-screen',
        component: () => import('@/components/home/general-monitoring-screen/general-monitoring-screen.vue'),
        meta: { guest: true, title: 'general-monitoring-screen', formName: 'GeneralMonitoringScreen', checkPrivilege: true }
    },
    {
        name: 'attendance-monitoring',
        path: 'attendance-monitoring',
        component: () => import('@/components/home/attendance-monitoring/attendance-monitoring.vue'),
        meta: { guest: true, title: 'attendance-monitoring', formName: 'AttendanceMonitoring', checkPrivilege: true }
    },
    {
        name: 'attendance-monitoring-2',
        path: 'attendance-monitoring-2',
        component: () => import('@/components/home/attendance-monitoring-2/attendance-monitoring-2.vue'),
        meta: { guest: true, title: 'attendance-monitoring-2', formName: 'AttendanceMonitoring', checkPrivilege: true }
    },
    {
        name: 'general-rules',
        path: 'general-rules',
        component: () => import('@/components/home/general-rules-component/general-rules-component.vue'),
        meta: { guest: true, title: 'general-rules', formName: 'GeneralRules', checkPrivilege: true }
    },
    {
        name: 'general-access-rules',
        path: 'general-access-rules',
        component: () => import('@/components/home/general-access-rules-component/general-access-rules-component.vue'),
        meta: { guest: true, title: 'general-access-rules', formName: 'GeneralAccessRules', checkPrivilege: true }
    },
    {
        name: 'in-out-time-rules',
        path: 'in-out-time-rules',
        component: () => import('@/components/home/in-out-time-rules-component/in-out-time-rules-component.vue'),
        meta: { guest: true, title: 'in-out-time-rules', formName: 'InOutTimeRules', checkPrivilege: true }
    },
    {
        name: 'warning-rules',
        path: 'warning-rules',
        component: () => import('@/components/home/warning-rules/warning-rules.vue'),
        meta: { guest: true, title: 'warning-rules', formName: 'WarningRules', checkPrivilege: true }
    },
    {
		name:  "gates-monitoring-history",
		path:  "gates-monitoring-history",
		component: () => import('@/components/home/gates-monitoring-history/gates-monitoring-history.vue'),
		meta: { guest: true, title: 'gates-monitoring-history', formName: 'GatesMonitoringHistory', checkPrivilege: false},
    },
    {
		name:  "vehicle-monitoring-history",
		path:  "vehicle-monitoring-history",
		component: () => import('@/components/home/vehicle-monitoring-history/vehicle-monitoring-history.vue'),
		meta: { guest: true, title: 'vehicle-monitoring-history', formName: 'VehicleMonitoringHistoryPage', checkPrivilege: false},
    },
    {
		name:  "truck-monitoring-history",
		path:  "truck-monitoring-history",
		component: () => import('@/components/home/truck-monitoring-history/truck-monitoring-history.vue'),
		meta: { guest: true, title: 'truck-monitoring-history', formName: 'TruckMonitoringHistoryPage', checkPrivilege: false},
    },
    {
        name: 'truck-driver-in-monitoring',
        path: 'truck-driver-in-monitoring',
        component: () => import('@/components/home/truck-driver-in-monitoring-component/truck-driver-in-monitoring-component.vue'),
        meta: { guest: true, title: 'truck-driver-in-monitoring', formName: 'TruckDriverInMonitoring', checkPrivilege: true }
    },
    {
        name: 'truck-driver-out-monitoring',
        path: 'truck-driver-out-monitoring',
        component: () => import('@/components/home/truck-driver-out-monitoring-component/truck-driver-out-monitoring-component.vue'),
        meta: { guest: true, title: 'truck-driver-out-monitoring', formName: 'TruckDriverOutMonitoring', checkPrivilege: true }
    },
    {
        name: 'factory-user-monitoring',
        path: 'factory-user-monitoring',
        component: () => import('@/components/home/factory-user-monitoring/factory-user-monitoring.vue'),
        meta: { guest: true, title: 'factory-user-monitoring', formName: 'FactoryUserMonitoring', checkPrivilege: true }
    },
    {
        name: 'attendance-and-evacuation',
        path: 'attendance-and-evacuation',
        component: () => import('@/components/home/attendance-and-evacuation-component/attendance-and-evacuation-component.vue'),
        meta: { guest: true, title: 'attendance-and-evacuation', formName: 'AttendanceAndEvacuation', checkPrivilege: true }
    },
];

export default routes;
