import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";
import { Loading } from 'element-ui';

class EmployeeInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetEmployeeAtPage(page: number, filter, departmentIndex: Array<number>, limit): Promise<BaseResponse> {
        const params = new URLSearchParams();
        departmentIndex.forEach(e => {
            params.append('departmentIndex', e.toString());
        });
        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('limit', limit.toString());
        return this.get("GetEmployeeAtPage", { params: params });
    }

    public GetEmployeeAsTree(userTypeNotUse?: number) {
        if(userTypeNotUse){
            return this.get("GetEmployeeAsTree",  { params: { userTypeNotUse } });
        }else{
            return this.get("GetEmployeeAsTree");
        }
    }

    public ExportTemplateICEmployee() {
        return this.get("ExportTemplateICEmployee");
    }

    public AddEmployee(employee: IC_EmployeeInfo) {
        return this.post("AddEmployee", employee);
    }
    public AddEmployeeFromExcel(arrEmployee) {
        return this.post("AddEmployeeFromExcel", arrEmployee)
    }

    public ExportToExcel(addedParams: Array<AddedParam>) {
    return this.post("ExportToExcel", addedParams, {responseType: 'blob'});
    }

    public UpdateEmployee(employee: IC_EmployeeInfo) {
        return this.post("UpdateEmployee", employee);
    }

    public DeleteEmployee(addedParams: Array<AddedParam>) {
        return this.post("DeleteEmployee", addedParams);
    }
    public DeleteEmployeeFromExcel(addedParams: Array<AddedParam>) {
        return this.post("DeleteEmployeeFromExcel", addedParams)
    }

    public GetEmployeeLookup() {
        return this.get("GetEmployeeLookup");
    }

    public GetEmployeeFinger(employeeATID: string) {
        return this.get("GetEmployeeFinger", { params: { employeeATID } });
    }

    public ExportToExcel1(addedParams: Array<AddedParam>, userType: number) {
        return this.post("ExportToExcel?userType=" + userType.toString(), addedParams, {responseType: 'blob'});
    }
    
    public RunIntegrate() {
        return this.post("RunIntegrate");
    }

    public InfoEmployeeTemplateImport() {
        return this.get("InfoEmployeeTemplateImport");
    }

}
export interface AddedParam {
    Key: string;
    Value: any;

}
export interface IC_EmployeeInfo {
    EmployeeATID?: string;
    EmployeeCode?: string;
    Password?: string;
    Biometrics?: string;
    CardNumber?: string;
    FullName?: string;
    NameOnMachine?: string;
    Gender?: number;
    DepartmentIndex?: number;
    _Gender?: string;
    _DepartmentName?: string;
    JoinedDate?: Date;
    UpdatedDate?:Date
    ImageUpload?: string;
    ListFinger?: Array<string>;
}

export interface Finger {
    ID?: number;
    Template?: string;
    FocusFinger: boolean;
    ImageFinger?: string;
    Quality?: number;
}

export const employeeInfoApi = new EmployeeInfoApi("EmployeeInfo");
