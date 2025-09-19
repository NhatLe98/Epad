import { AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_AjustTimeAttendanceLogHistoryApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAjustAttendanceLogHistoryAtPage(data: TA_AjustAttendanceLogHistoryParam): Promise<any> {
        return this.post('GetAjustAttendanceLogHistoryAtPage', data);
    }
}

export interface TA_AjustAttendanceLogHistoryParam {
    Filter?: string;
    Page?: number;
    Limit?: number;
    Departments?: Array<number>;
    EmployeeATIDs?: Array<string>;
    FromDate?: string;
    ToDate?: string;
    Operators?: Array<number>;
}

export const taAjustAttendanceLogHistory = new TA_AjustTimeAttendanceLogHistoryApi("TA_AjustAttendanceLogHistory");
