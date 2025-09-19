import { AxiosRequestConfig } from "axios";
import { BaseApi } from '@/$core/base-api'
import { UserSyncAuthMode } from "@/constant/user-sync-auth-mode";
import { TargetDownloadUser } from "@/constant/target-download-user";

class CommandApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }

    public DownloadLogFromToTime(params: CommandRequest) {
        return this.post('GetCommandAtPage', params);
    }

    public DownloadAllUser(params: CommandRequest) {
        return this.post('DownloadAllUser', params);
    }

    public DownloadUserById(params: CommandRequest) {
        return this.post('DownloadUserById', params);
    }

    public DownloadAllUserMaster(params: CommandRequest) {
        return this.post('DownloadAllUserMaster', params);
    }

    public async downloadUserMaster(payload: {
        AuthModes: UserSyncAuthMode[],
        SerialNumbers: string[],
        IsOverwriteData: boolean,
        TargetDownloadUser: TargetDownloadUser,
        EmployeeATIDs?: string[],
    }) {
        await this.post(payload.EmployeeATIDs?.length ? "DownloadUserMasterById" : "DownloadAllUserMaster", {
            ...payload,
            EmployeeATIDs: payload.EmployeeATIDs ?? [],
        });

    }

    public DownloadUserMasterById(params: CommandRequest) {
        return this.post('DownloadUserMasterById', params);
    }

    public DeleteAllUser(params: CommandRequest) {
        return this.post('DeleteAllUser', params);
    }

    public DeleteAllFingerPrint(params: CommandRequest) {
        return this.post('DeleteAllFingerPrint', params);
    }

    public DeleteUserById(params: CommandRequest) {
        return this.post('DeleteUserById', params);
    }

    public UploadUsers(params: CommandRequest) {
        return this.post('UploadUsers', params);
    }

    public UploadACUsers(params: CommandRequest) {
        return this.post('UploadACUsers', params);
    }

    public UploadACByDepartment(params: CommandRequest) {
        return this.post('UploadACByDepartment', params);
    }

    public GetDeviceInfo(params: CommandRequest) {
        return this.post('GetDeviceInfo', params);
    }
    public RestartDevice(params: CommandRequest) {
        return this.post('RestartDevice', params);
    }
    public UnlockDoor(params: CommandRequest) {
        return this.post('UnlockDoor', params);
    }
    public GetAttendanceData(params: CommandRequest) {
        return this.post('DownloadLogFromToTime', params);
    }
    public DeleteAttendanceData(params: CommandRequest) {
        return this.post('DeleteLogFromToTime', params);
    }
    public RestartService(params: Array<Number>) {
        return this.post('RestartService', params);
    }

    public RestartServiceByDevice(params: Array<string>) {
        return this.post('RestartServiceByDevice', params);
    }
    public UploadPrivilegeUsers(params: CommandRequest) {
        return this.post('UploadPrivilegeUsers', params);
    }

    public SetDeviceTime(params: CommandRequest){
        return this.post('SetDeviceTime', params);
    }
    
    public DeleteUserByIdFromUserId(e: Array<string>): Promise<any> {
        return this.post('DeleteUserByIdFromUserId',  e);
    }

    public UploadTimeZone(params: CommandRequest){
        return this.post('UploadTimeZone', params);
    }

    public UploadAccHoliday(params: CommandRequest){
        return this.post('UploadAccHoliday', params);
    }

    public DeleteAllHoliday(params: CommandRequest){
        return this.post('DeleteAllHoliday', params);
    }

    public DeleteACUsers(params: CommandRequest){
        return this.post('DeleteACUsers', params);
    }
    
    public UploadACUserFromExcel(arrDepartment) {
        return this.post("UploadACUserFromExcel", arrDepartment)
    }

    public SetDoorSetting(params: CommandRequest) {
        return this.post("SetDoorSetting", params)
    }
    
    public DeleteTimezoneById(params: CommandRequest) {
        return this.post("DeleteTimezoneById", params)
    }

    public DeleteACUsersByDoor(params: CommandRequest) {
        return this.post("DeleteACUsersByDoor", params)
    }
    public DeleteACDepartmentByDoor(params: CommandRequest) {
        return this.post("DeleteACDepartmentByDoor", params)
    }
    public UploadACAccessedEmployeeFromExcel(arrDepartment) {
        return this.post("UploadACAccessedEmployeeFromExcel", arrDepartment)
    }

    public UploadACUsersWhenUpdateDoor(params: CommandRequest) {
        return this.post('UploadACUsersWhenUpdateDoor', params);
    }

    public UploadACUsersWhenUpdateDoorBySerial(params: CommandRequest) {
        return this.post('UploadACUsersWhenUpdateDoorBySerial', params);
    }
}

export interface CommandRequest {
    Action: string;
    ListSerial: Array<string>;
    ListUser: Array<string>;
    ListDepartment?: Array<number>;
    FromTime?: any;
    ToTime?: any;
    GroupDeviceIndex?: number;
    Privilege?: number;
    AuthenMode?: Array<string>;
    IsOverwriteData?: boolean;
    EmployeeType?: number;
    IsDownloadFull?: boolean;
    IsDeleteAll?: boolean;
    IsUsingTimeZone?: boolean;
    TimeZone?: Array<string>;
    Group?: number;
    IsUsingArea?:boolean;
    AreaLst?:  Array<string>;
    DoorLst?:  Array<string>;
    AutoOffSecond?: number;
    TimezoneStr?: string;
    EmployeeAccessedGroup?:  Array<number>;
}


export const commandApi = new CommandApi('Command');