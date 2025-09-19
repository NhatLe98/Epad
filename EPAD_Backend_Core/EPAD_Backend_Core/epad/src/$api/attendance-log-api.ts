import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'

class AttendanceLogApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }
        
    public GetLastedAttendanceLog(): Promise<any> {
        return this.get('GetLastedAttendanceLog');
    }

    public GetLastedACAttendanceLog(): Promise<any> {
        return this.get('GetLastedACAttendanceLog');
    }

    public GetLogLast7Days(): Promise<any> {
        return this.get('GetLogLast7Days');
    }

    public GetFullWorkingEmployeeByRootDepartment(): Promise<any> {
        return this.get('GetFullWorkingEmployeeByRootDepartment');
    }

    public GetFullWorkingEmployeeByDepartment(): Promise<any> {
        return this.get('GetFullWorkingEmployeeByDepartment');
    }

    public UpdateLatestEmergencyAttendance(): Promise<any> {
        return this.post('UpdateLatestEmergencyAttendance');
    }

    public GetIntegratedVehicleLog(): Promise<any> {
        return this.get('GetIntegratedVehicleLog');
    }

    public GetTruckDriverLog(): Promise<any> {
        return this.get('GetTruckDriverLog');
    }

    public GetEmergencyLog(): Promise<any> {
        return this.get('GetEmergencyLog');
    }

    public GetTupleFullWorkingEmployeeByRootDepartment(): Promise<any> {
        return this.get('GetTupleFullWorkingEmployeeByRootDepartment');
    }

    public GetTupleFullWorkingEmployeeByUserType(): Promise<any> {
        return this.get('GetTupleFullWorkingEmployeeByUserType');
    }

    public GetTupleFullWorkingEmployeeByDepartment(): Promise<any> {
        return this.get('GetTupleFullWorkingEmployeeByDepartment');
    }

    public GetTupleFullVehicleEmployeeByDepartment(): Promise<any> {
        return this.get('GetTupleFullVehicleEmployeeByDepartment');
    }

    public GetLogsByDoor(): Promise<any> {
        return this.get('GetLogsByDoor');
    }

    public GetRemainInLogs(): Promise<any> {
        return this.get('GetRemainInLogs');
    }

    public GetSystemDateTime(): Promise<any> {
        return this.get('GetSystemDateTime');
    }

    public GetLastedRealtimeAttendanceLog(): Promise<any> {
        return this.get('GetLastedRealtimeAttendanceLog');
    }

    public GetAtPageAttendanceLog(page: number, fromDate: string, toDate: string, filter: string, employee: Array<string>, limit): Promise<BaseResponse> {
        return this.post("GetAtPageAttendanceLog", { page, fromDate, toDate, filter, employee, limit });
    }
    public ImportAttendanceLog(file) {
        return this.post("ImportAttendanceLog", { file });
    }

    public ExportAttendanceLog(addedParams: Array<AddedParam>) {
        return this.post("ExportAttendanceLog", addedParams );
    }

    public DeleteAttendanceLogTemp() {
         return this.post("DeleteAttendanceLogTemp");
    }

    public GetLastedACOpenDoor(): Promise<any> {
        return this.get('GetLastedACOpenDoor');
    }

    public GetACAttendanceLogByFilter(page: number, fromDate: string, toDate: string, filter: string, departmentIds: Array<number>, listArea: Array<number>, listDoor: Array<number>, limit): Promise<BaseResponse> {
        return this.post("GetACAttendanceLogByFilter", { page, fromDate, toDate, filter, departmentIds, limit, listArea, listDoor });
    }

    public IntegrateLog(previousDay: number){
        return this.post("RunIntegrateLogManual", { params: { previousDay} })
    }

    public GetDoorStatus(): Promise<any> {
        return this.get('GetDoorStatus');
    }

    public GetEmergencyAndEvacuation(): Promise<any> {
        return this.get('GetEmergencyAndEvacuation');
    }

}
export interface AddedParam {
    Key: string,
    Value: any
}
export const attendanceLogApi = new AttendanceLogApi('AttendanceLog');