import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class GatesMonitoringHistoryAPI extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetGatesMonitoringHistories(data: GatesMonitoringHistoryModel) {
        return this.post('GetGatesMonitoringHistories', data);
    }
    public ExportHExportGatesHistoryistory(data: GatesMonitoringHistoryModel) {
        return this.post('ExportGatesHistory', data, { responseType: 'blob' });
    }
}

export interface GatesMonitoringHistoryModel {
    FromTime: Date;
    ToTime: Date;
    EmployeeIndexes: Array<string>;
    DepartmentIndexes: Array<number>;
    RulesWarningIndexes: Array<number>;
    StatusLog: string;
    Page: number;
    PageSize: number;
}


export const gatesMonitoringHistoryApi = new GatesMonitoringHistoryAPI('GC_GatesMonitoringHistory');
