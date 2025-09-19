import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_UserApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetEmployeeLookup(): Promise<any> {
        return this.get("GetEmployeeLookup");
    }

    public GetAllEmployeeCompactInfoByPermission(): Promise<any> {
        return this.get("GetAllEmployeeCompactInfoByPermission");
    }

    public GetAllEmployeeCompactInfoByPermissionImprovePerformance(): Promise<any> {
        return this.get("GetAllEmployeeCompactInfoByPermissionImprovePerformance");
    }

    public GetEmployeeAndDepartmentLookup(): Promise<any> {
        return this.get('GetEmployeeAndDepartmentLookup');
    }
    public GetAllHR_User(): Promise<any> {
        return this.get("Get_HR_Users");
    }

    public GetHR_User(employeeATID: string): Promise<any> {
        return this.get("Get_HR_User/" + employeeATID);
    }
    public GetUserByCCCD(cccd: string): Promise<any> {
        return this.get("GetUserByCCCD/" + cccd);
    }

    public GetAllEmployeeCompactInfo(): Promise<any> {
        return this.get("GetAllEmployeeCompactInfo");
    }
    public GetAllEmployeeTypeUserCompactInfo(): Promise<any> {
        return this.get("GetAllEmployeeTypeUserCompactInfo");
    }
    public GetAllNote(): Promise<any> {
        return this.get("GetAllNote");
    }
}

export const hrUserApi = new HR_UserApi("HR_User");
