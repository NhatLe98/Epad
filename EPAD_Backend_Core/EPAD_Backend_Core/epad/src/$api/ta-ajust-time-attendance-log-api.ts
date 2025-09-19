import { AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_AjustTimeAttendanceLogApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAjustTimeAttendanceLogAtPage(data: TA_AjustTimeAttendanceLogParam): Promise<any> {
        return this.post('GetAjustTimeAttendanceLogAtPage', data);
    }

    public GetAllRegistrationType(): Promise<any> {
        return this.get('GetAllRegistrationType');
    }

    public UpdateAjustTimeAttendanceLog(data){
        return this.post('UpdateAjustTimeAttendanceLog', data);
    }
}

export interface TA_AjustTimeAttendanceLogParam {
    Filter?: string;
    Page?: number;
    Limit?: number;
    Departments?: Array<number>;
    EmployeeATIDs?: Array<string>;
    FromDate?: string;
    ToDate?: string;
}

export const taAjustTimeAttendanceLog = new TA_AjustTimeAttendanceLogApi("TA_AjustTimeAttendanceLog");
