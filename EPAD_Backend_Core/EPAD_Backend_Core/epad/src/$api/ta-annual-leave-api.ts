import { AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_AnnualLeaveApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAnnualLeaveAtPage(data: TA_AnnualLeaveParam): Promise<any> {
        return this.post('GetAnnualLeaveAtPage', data);
    }

    public AddAnnualLeave(param) {
        return this.post("AddAnnualLeave", param);
    }
    public UpdateAnnualLeave(param) {
        return this.post("UpdateAnnualLeave", param);
    }
    public DeleteAnnualLeave(index: Array<any>) {
        return this.delete("DeleteAnnualLeave", { data: index });
    }
    public AddAnnualLeaveFromExcel(param){
        return this.post("AddAnnualLeaveFromExcel", param);
    }
}

export interface TA_AnnualLeaveParam {
    Filter?: string;
    Page?: number;
    Limit?: number;
    Departments?: Array<number>;
    EmployeeATIDs?: Array<string>;
}

export const taAnnualLeaveApi = new TA_AnnualLeaveApi("TA_AnnualLeave");
