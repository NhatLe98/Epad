import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'employee',
        path: 'employee',
        component: () => import('@/components/home/employee-component/employee-component.vue'),
        meta: { guest: true, title: 'employee', formName: 'Employee', checkPrivilege: true }
    },
    {
        name: 'change-department',
        path: 'change-department',
        component: () => import('@/components/home/change-department-component/change-department-component.vue'),
        meta: { guest: true, title: 'change-department', formName: 'ChangeDepartment', checkPrivilege: true }
    },
    {
        name: 'excused-absent',
        path: 'excused-absent',
        component: () => import('@/components/home/excused-absent-component/excused-absent-component.vue'),
        meta: { guest: true, title: 'excused-absent', formName: 'ExcusedAbsent', checkPrivilege: true }
    },
    {
        name: 'excused-late-entry',
        path: 'excused-late-entry',
        component: () => import('@/components/home/excused-late-entry-component/excused-late-entry-component.vue'),
        meta: { guest: true, title: 'excused-late-entry', formName: 'ExcusedLateEntry', checkPrivilege: true }
    },
    {
        name: 'dorm-room',
        path: 'dorm-room',
        component: () => import('@/components/home/dorm-room-component/dorm-room-component.vue'),
        meta: { guest: true, title: 'dorm-room', formName: 'DormRoom', checkPrivilege: true }
    },
    {
        name: 'dorm-register',
        path: 'dorm-register',
        component: () => import('@/components/home/dorm-register-component/dorm-register-component.vue'),
        meta: { guest: true, title: 'dorm-register', formName: 'DormRegister', checkPrivilege: true }
    },
    {
        name: 'approve-change-department',
        path: 'approve-change-department',
        component: () => import('@/components/home/approve-change-department-component/approve-change-department-component.vue'),
        meta: { guest: true, title: 'approve-change-department', formName: 'ApproveChangeDepartment', checkPrivilege: true }
    },
    {
        name: 'auto-sync-user',
        path: 'auto-sync-user',
        component: () => import('@/components/home/auto-synch-user-component/auto-synch-user-component.vue'),
        meta: { guest: true, title: 'auto-sync-user', formName: 'AutoSynchUser', checkPrivilege: true }
    },
    {
        name: 'user-management-centralization',
        path: 'user-management-centralization',
        component: () => import('@/components/home/user-management-component/user-management-component.vue'),
        meta: { guest: true, title: 'user-management', formName: 'UserManagementCentralization', checkPrivilege: false }
    },
    {
        name: 'department',
        path: 'department',
        component: () => import('@/components/home/department-component/department-component.vue'),
        meta: { guest: true, title: 'department', formName: 'Department', checkPrivilege: true }
    },
    {
        name: 'employee-type',
        path: 'employee-type',
        component: () => import('@/components/home/employee-type-component/employee-type.vue'),
        meta: { guest: true, title: 'employee-type', formName: 'EmployeeType', checkPrivilege: true }
    },
    {
        name: 'login-account',
        path: 'login-account',
        component: () => import('@/components/home/login-account-component/login-account-component.vue'),
        meta: { guest: true, title: 'login-account', formName: 'LoginAccount', checkPrivilege: true }
    },
    {
        name: 'group-account',
        path: 'group-account',
        component: () => import('@/components/home/group-account-component/group-account-component.vue'),
        meta: { guest: true, title: 'group-account', formName: 'GroupAccount', checkPrivilege: true }
    },
    {
        name: 'assign-privilege',
        path: 'assign-privilege',
        component: () => import('@/components/home/assign-privilege-user-group-component/assign-privilege-user-group-component.vue'),
        meta: { guest: true, title: 'assign-privilege', formName: 'AssignPrivilegeUserGroup', checkPrivilege: true }
    },
    {
        name: 'assign-privilege-device',
        path: 'assign-privilege-device',
        component: () => import('@/components/home/assign-privilege-device-component/assign-privilege-device-component.vue'),
        meta: { guest: true, title: 'assign-privilege-device', formName: 'AssignPrivilegeDevice', checkPrivilege: true }
    },
    {
        name: 'assign-privilege-department',
        path: 'assign-privilege-department',
        component: () => import('@/components/home/assign-privilege-department-component/assign-privilege-department-component.vue'),
        meta: { guest: true, title: 'assign-privilege-department', formName: 'AssignPrivilegeDepartment', checkPrivilege: true }
    },
    {
        name: 'assign-privilege-machine-realtime',
        path: 'assign-privilege-machine-realtime',
        component: () => import('@/components/home/assign-privilege-machine-realtime-component/assign-privilege-machine-realtime-component.vue'),
        meta: { guest: true, title: 'assign-privilege-machine-realtime', formName: 'AssignPrivilegeMachineRealtime', checkPrivilege: true }
    },
    {
        name: 'working-info',
        path: 'working-info',
        component: () => import('@/components/home/working-info/working-info.vue'),
        meta: { guest: true, title: 'working-info', formName: 'WorkingInfo', checkPrivilege: true }
    },
    {
        name: 'class-info-management',
        path: 'class-info-management',
        component: () => import('@/components/home/user-management-component/hr-class-info/hr-class-info.vue'),
        meta: { guest: true, title: 'class-info-management', formName: 'ClassInfoManagement', checkPrivilege: false }
    },
    {
        name: 'card-info-management',
        path: 'card-info-management',
        component: () => import('@/components/home/user-management-component/hr-card-info/hr-card-info.vue'),
        meta: { guest: true, title: 'card-info-management', formName: 'CardInfoManagement', checkPrivilege: false }
    },
    {
        name: 'position-info-management',
        path: 'position-info-management',
        component: () => import('@/components/home/user-management-component/hr-position-info/hr-position-info.vue'),
        meta: { guest: true, title: 'position-info-management', formName: 'PositionInfoManagement', checkPrivilege: false }
    },
    {
        name: 'user-type',
        path: 'user-type',
        component: () => import('@/components/home/user-type-component/user-type-component.vue'),
        meta: { guest: true, title: 'user-type', formName: 'UserType', checkPrivilege: false }
    },
    {
        name: 'division',
        path: 'division',
        component: () => import('@/components/home/division-component/division-component.vue'),
        meta: { guest: true, title: 'division', formName: 'Division', checkPrivilege: false }
    },
    {
        name: 'gc-customer-info',
        path: 'gc-customer-info',
        component: () => import('@/components/home/gc-customer-info-component/gc-customer-info-component.vue'),
        meta: { guest: true, title: 'gc-customer-info', formName: 'GCCustomerInfo', checkPrivilege: true }
    },
    {
        name: 'customer-card',
        path: 'customer-card',
        component: () => import('@/components/home/customer-card-component/customer-card-component.vue'),
        meta: { guest: true, title: 'customer-card-component', formName: 'CustomerDriverCard', checkPrivilege: true }
    },
    {
        name: 'black-list',
        path: 'black-list',
        component: () => import('@/components/home/black-list-component/black-list.vue'),
        meta: { guest: true, title: 'black-list-component', formName: 'BlackList', checkPrivilege: true }
    },
    {
        name: 'location-operator',
        path: 'location-operator',
        component: () => import('@/components/home/ic-location-operator-component/ic-location-operator-component.vue'),
        meta: { guest: true, title: 'location-operator-component', formName: 'LocationOperator', checkPrivilege: true }
    },
    {
      name: "employee-stopped",
      path: "employee-stopped",
      component: () =>
        import(
          "@/components/home/employee-stopped-component/employee-stopped-component.vue"
        ),
      meta: {
        guest: true,
        title: "employee-stopped",
        formName: "EmployeeStopped",
        checkPrivilege: true,
      },
    },
    {
        name: 'user-guide-page',
        path: 'user-guide-page',
        component: () => import('@/components/home/user-guide-page-component/user-guide-page-component.vue'),
        meta: { guest: true, title: 'user-guide-page-component', formName: 'UserGuide', checkPrivilege: true }
    },
    {
        name: 'email-declare-guest',
        path: 'email-declare-guest',
        component: () => import('@/components/home/email-declare-guest-component/email-declare-guest-component.vue'),
        meta: { guest: true, title: 'email-declare-guest-component', formName: 'EmailDeclareGuest', checkPrivilege: true }
    },
];

export default routes;
