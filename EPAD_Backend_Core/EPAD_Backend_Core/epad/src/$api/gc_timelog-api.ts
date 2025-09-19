import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class TimeLogApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public async beginGetLogInGateMandatory() {
        const res = await this.get<any>('GetLogInGateMandatory');
        return res.data as LogInGateMandatory[];
    }

    public async updateLogInGateMandatoryByRule(){
        return await this.post<any>('UpdateLogInGateMandatoryByRule');
    }

  public GetWalkerMonitoringHistoryListByLineIndex(lineIndex: number, size: number) {
    return this.get('GetWalkerMonitoringHistoryListByLineIndex', { params: { lineIndex, size } });
  }

  public GetWalkerMonitoringHistoryByLogIndex(logIndex: number) {
    return this.get('GetWalkerMonitoringHistoryByLogIndex', { params: { logIndex } });
  }
}
export const timeLogApi = new TimeLogApi('GC_TimeLog');

export class LogInGateMandatory {
    Action: string;
    ApproveStatus: string;
    CompanyIndex: string;
    CustomerIndex: number;
    Department: string;
    DepartmentIndex: number;
    EmployeeATID: string;
    Error?: any;
    ExtendData: string;
    FullName: string;
    GateIndex: number;
    InOutMode: number;
    Index: number;
    LineIndex: number;
    LogType: string;
    MachineSerial: string;
    Note: string;
    ObjectAccessType: string;
    PlatesRegistered?: any;
    Position: string;
    SpecifiedMode?: any;
    Status: number
    SystemTime: string;
    Time: string;
    UpdatedDate: string;
    UpdatedUser: string;
}
