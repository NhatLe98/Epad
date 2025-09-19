import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class VehicleMonitoringHistoryAPI extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetVehicleMonitoringHistories(data: VehicleMonitoringHistoryModel) {
        return this.post('GetVehicleMonitoringHistories', data);
    }
    public ExportVehicleHistory(data: VehicleMonitoringHistoryModel) {
        return this.post('ExportVehicleHistory', data, { responseType: 'blob' });
    }
}

export interface VehicleMonitoringHistoryModel {
    FromTime: Date;
    ToTime: Date;
    EmployeeIndexes: Array<string>;
    DepartmentIndexes: Array<number>;
    StatusLog: string;
    Page: number;
    PageSize: number;
    Filter: string;
}


export const vehicleMonitoringHistoryApi = new VehicleMonitoringHistoryAPI('GC_VehicleMonitoringHistory');
