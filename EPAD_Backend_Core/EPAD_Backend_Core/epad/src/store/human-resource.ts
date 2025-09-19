/* eslint-disable */
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { positionApi } from '@/$api/position-api';

import { Module } from 'vuex';

const Store: Module<any, any> = {
    namespaced: true,
    state: {
        employee: [],
        employeeLookup: {},

        department: [],
        departmentLookup: {},

        position: [],
        positionLookup: {},
    },
    mutations: {
        setEmployee(state, data) {
            state.employee = data;
        },

        setEmployeeLookup(state, data) {
            state.employeeLookup = data;
        },
        setEmployeeOnlyLookup(state, data) {
            state.employeeOnlyLookup = data;
        },

        setDepartment(state, data) {
            state.department = data;
        },

        setDepartmentLookup(state, data) {
            state.departmentLookup = data;
        },

        setPosition(state, data) {
            state.position = data;
        },

        setPositionLookup(state, data) {
            state.positionLookup = data;
        },
    },

    getters: {
        employee: (state) => state.employee || [],
        employeeLookup: (state) => state.employeeLookup || {},
        department: (state) => state.department || [],
        departmentLookup: (state) => state.departmentLookup || [],
        position: (state) => state.position || [],
        positionLookup: (state) => state.positionLookup || []
    },

    actions: {
        async loadResource({ state, dispatch }) {
            const { employee, department, position } = state;
            if (Misc.isEmpty(employee)) {
                await dispatch('loadEmployeeLookup');
            }
            if (Misc.isEmpty(department)) {
                await dispatch('loadDepartmentLookup');
            }
            // if (Misc.isEmpty(position)) {
            //     await dispatch('loadPositionLookup');
            // }
        },

        async loadEmployeeLookup({ state, commit }) {

            return await hrUserApi.GetEmployeeLookup().then((rep: any) => {
                const data = rep.data;
                const dictData = {};
                data.forEach((e: any) => {
                    dictData[e.EmployeeATID] = {
                        EmployeeATID: e.EmployeeATID,
                        EmployeeType: e.EmployeeType,
                        FullName: e.FullName,
                        BirthDay: e.BirthDay,
                        Email: e.Email,
                        Phone: e.Phone,
                        Gender: e.Gender
                    };
                });
                commit('setEmployee', data);
                commit('setEmployeeLookup', dictData);
            });
        },

        async loadDepartmentLookup({ state, commit }) {
            return await departmentApi.GetAll().then((rep: any) => {
                const data = rep.data.Value;
                const dictData = {};
                data.forEach((e: any) => {
                    dictData[e.value] = {
                        Index: e.value,
                        Name: e.label
                    };
                })
                commit('setDepartment', data.map(e => {
                    return {
                        Index: e.value,
                        Name: e.label
                    }
                }));
                commit('setDepartmentLookup', dictData);
            })
        },

        async loadPositionLookup({ state, commit }) {
            return await positionApi.GetAll().then((rep: any) => {
                const data = rep.data.Value;
                const dictData = {};
                data.forEach((e: any) => {
                    dictData[e.value] = {
                        Index: e.value,
                        Name: e.label
                    };
                })
                commit('setPosition', data.map(e => {
                    return {
                        Index: e.value,
                        Name: e.label
                    }
                }));
                commit('setPositionLookup', dictData);
            })
        },
    },
};

export { Store as HumanResource };
