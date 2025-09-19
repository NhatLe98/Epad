import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class PrivilegeDepartmentApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDepartmentsByPrivilegeIndex(privilegeIndex: number) {
        return this.get("GetDepartmentsByPrivilegeIndex", { params: { privilegeIndex } });
    }
    public UpdatePrivilegeAndDepartments(param: PrivilegeAndDepartmentsParam) {
        return this.post("UpdatePrivilegeAndDepartments", param);
    }
}
export interface PrivilegeAndDepartmentsParam {
    PrivilegeIndex: number;
    ListDepartmentIndexs: number[];
}
export const privilegeDepartmentApi = new PrivilegeDepartmentApi("PrivilegeDepartment");