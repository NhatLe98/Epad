import { AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_AjustAttendanceLogApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAjustAttendanceLogAtPage(data: TA_AjustAttendanceLogParam): Promise<any> {
        return this.post('GetAjustAttendanceLogAtPage', data);
    }
    public AddAjustAttendanceLog(data) {
        return this.post('AddAjustAttendanceLog', data);
    }

    public UpdateAjustAttendanceLog(data) {
        return this.post('UpdateAjustAttendanceLog', data);
    }
    public DeleteAjustAttendanceLog(log) {
        return this.post('DeleteAjustAttendanceLog',log);
    }
    public UpdateAjustAttendanceLogLst(data) {
        return this.post('UpdateAjustAttendanceLogLst', data);
    }
    public AddAjustAttendanceLogFromExcel(data){
        return this.post("AddAjustAttendanceLogFromExcel", data);
    }
}

export interface TA_AjustAttendanceLogParam {
    Filter?: string;
    Page?: number;
    Limit?: number;
    Departments?: Array<number>;
    EmployeeATIDs?: Array<string>;
    FromDate?: string;
    ToDate?: string;
}

export const taAjustAttendanceLog = new TA_AjustAttendanceLogApi("TA_AjustAttendanceLog");
