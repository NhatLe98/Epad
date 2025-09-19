import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class UserMaster extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetUserMachineInfo(listUser: Array<String>, filter, ListWorkingStatus: Array<number>) {
        return this.post("GetUserMachineInfo", { listUser, filter,ListWorkingStatus });
    }

    public GetAllUserMachineInfo(departmentIndexs: Array<number>, filter, ListWorkingStatus: Array<number>): Promise<BaseResponse> {
        // const params = new URLSearchParams();
        // params.append('filter', filter.toString());
        // departmentIndex.forEach(e => {
        //     params.append('departmentIndexs', e.toString());
        // });
        
        return this.post("GetAllUserMachineInfo", { departmentIndexs, filter, ListWorkingStatus });
    }

    public GetAllStudentMachineInfo(classIndex: Array<string>, filter): Promise<BaseResponse> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString()); 
        classIndex.forEach(e => {
            params.append('classIndexs', e.toString());
        });
        return this.get("GetAllStudentMachineInfo", { params: params });
    }

    public GetAllParentMachineInfo( filter): Promise<BaseResponse> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        return this.get("GetAllParentMachineInfo", { params: params });
    }

    public GetAllCustomerMachineInfo(filter, employeeType, departmentIndexs ): Promise<BaseResponse> {
      
        return this.post("GetAllCustomerMachineInfo", { departmentIndexs, filter, employeeType });
    }

    public GetAllCustomerMachineInfoByMultipleType(filter, listEmployeeType, departmentIndexs ): Promise<BaseResponse> {
      
        return this.post("GetAllCustomerMachineInfoByMultipleType", { departmentIndexs, filter, listEmployeeType });
    }

    public GetUserMachineInfoCompare(listUser: Array<String>, filter) {
        return this.post("GetUserMachineInfoCompare", { listUser, filter });
    }
    //public UpdateUserPrivilege(addedParams: Array<AddedParam>) {
    //    return this.post("UpdateUserPrivilege", addedParams );
    //}
}
export interface AddedParam {
    Key: string;
    Value: any | object;
}
export const userMasterApi = new UserMaster("UserMaster");
