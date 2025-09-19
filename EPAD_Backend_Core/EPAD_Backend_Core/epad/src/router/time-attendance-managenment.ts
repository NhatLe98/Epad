import { RouteConfig } from 'vue-router';

const routes: RouteConfig[] = [
    {
        name: 'ta-rule-by-shift',
        path: 'ta-rule-by-shift',
        component: () => import('@/components/home/ta-rule-by-shift/ta-rule-by-shift.vue'),
        meta: { guest: true, title: 'ta-rule-by-shift', formName: 'TARuleByShift', checkPrivilege: true }
    },
    {
        name: 'ta-shift',
        path: 'ta-shift',
        component: () => import('@/components/home/ta-shift-component/ta-shift-component.vue'),
        meta: { guest: true, title: 'ta-shift', formName: 'TAShift', checkPrivilege: true }
    },
    {
        name: 'ta-holiday-type',
        path: 'ta-holiday-type',
        component: () => import('@/components/home/ta-holiday-type-component/ta-holiday-type-component.vue'),
        meta: { guest: true, title: 'ta-holiday-type', formName: 'TAHolidayType', checkPrivilege: true }
    },
    {
        name: 'ta-leave-type',
        path: 'ta-leave-type',
        component: () => import('@/components/home/ta-leave-type-component/ta-leave-type-component.vue'),
        meta: { guest: true, title: 'ta-leave-type', formName: 'TALeaveType', checkPrivilege: true }
    },
    {
        name: 'ta-location-management',
        path: 'ta-location-management',
        component: () => import('@/components/home/ta-location-management-component/ta-location-management-component.vue'),
        meta: { guest: true, title: 'ta-location-management', formName: 'TALocationManagement', checkPrivilege: true }
    },
    {
        name: 'ta-rules-global',
        path: 'ta-rules-global',
        component: () => import('@/components/home/ta-rules-global-component/ta-rules-global-component.vue'),
        meta: { guest: true, title: 'ta-rules-global', formName: 'TARulesGlobal', checkPrivilege: true }
    },
    {
        name: 'ta-annual-leave',
        path: 'ta-annual-leave',
        component: () => import('@/components/home/ta-annual-leave-component/ta-annual-leave-component.vue'),
        meta: { guest: true, title: 'ta-annual-leave', formName: 'TAAnnualLeave', checkPrivilege: true }
    },
    {
        name: 'ta-fixed-schedule',
        path: 'ta-fixed-schedule',
        component: () => import('@/components/home/ta-fixed-schedule-component/ta-fixed-schedule-component.vue'),
        meta: { guest: true, title: 'ta-fixed-schedule', formName: 'TAFixedSchedule', checkPrivilege: true }
    },
    {
        name: 'ta-use-quick-screen',
        path: 'ta-use-quick-screen',
        component: () => import('@/components/home/ta-use-quick-screen-component/ta-use-quick-screen-component.vue'),
        meta: { guest: true, title: 'ta-use-quick-screen', formName: 'TAUseQuickScreen', checkPrivilege: true }
    },
    {
        name: 'ta-register-for-leave',
        path: 'ta-register-for-leave',
        component: () => import('@/components/home/ta-register-for-leave-component/ta-register-for-leave-component.vue'),
        meta: { guest: true, title: 'ta-register-for-leave', formName: 'TARegisterForLeave', checkPrivilege: true }
    },
    {
        name: 'ta-ajust-attendance-log',
        path: 'ta-ajust-attendance-log',
        component: () => import('@/components/home/ta-ajust-attendance-log-component/ta-ajust-attendance-log-component.vue'),
        meta: { guest: true, title: 'ta-ajust-attendance-log', formName: 'TAAjustAttendanceLog', checkPrivilege: true }
    },
    {
        name: 'ta-calculate-attendance',
        path: 'ta-calculate-attendance',
        component: () => import('@/components/home/ta-calculate-attendance-component/ta-calculate-attendance-component.vue'),
        meta: { guest: true, title: 'ta-calculate-attendance', formName: 'TACalculateAttendance', checkPrivilege: true }
    },
    {
        name: 'ta-ajust-attendance-log-history',
        path: 'ta-ajust-attendance-log-history',
        component: () => import('@/components/home/ta-ajust-attendance-log-history-component/ta-ajust-attendance-log-history-component.vue'),
        meta: { guest: true, title: 'ta-ajust-attendance-log-history', formName: 'TAAjustAttendanceLogHistory', checkPrivilege: true }
       
    }
];

export default routes;
