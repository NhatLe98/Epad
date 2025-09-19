import {  AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class PrivilegeMachineRealtime extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllPrivilegeMachineRealtime(page: number, filter: string, pageSize): Promise<BaseResponse> {
        return this.get("GetAllPrivilegeMachineRealtime", { params: { filter, page, pageSize } });
    }

    public GetPrivilegeMachineRealtimeByUser(): Promise<BaseResponse> {
        return this.get("GetPrivilegeMachineRealtimeByUser");
    }

    public AddPrivilegeMachineRealtime(device: IC_PrivilegeMachineRealtimeDTO) {
        return this.post("AddPrivilegeMachineRealtime", device);
    }

    public UpdatePrivilegeMachineRealtime(device: IC_PrivilegeMachineRealtimeDTO) {
        return this.post("UpdatePrivilegeMachineRealtime", device);
    }

    public DeletePrivilegeMachineRealtime(device: Array<IC_PrivilegeMachineRealtimeDTO>) {
        return this.post("DeletePrivilegeMachineRealtime", device);
    }

}
export interface IC_PrivilegeMachineRealtimeDTO {
    UserName: string;
    ListUserName: Array<string>;
    PrivilegeGroup: number;
    PrivilegeGroupName: string;
    GroupDeviceIndex: string;
    GroupDeviceName: string;
    ListGroupDeviceIndex: Array<number>;
    ListGroupDeviceName: Array<string>;
    DeviceModule: string;
    DeviceModuleName: string;
    ListDeviceModule: Array<string>;
    ListDeviceModuleName: Array<string>;
    DeviceSerial: string;
    DeviceName: string;
    ListDeviceSerial: Array<string>;
    ListDeviceName: Array<string>; 
}
export const privilegeMachineRealtimeApi = new PrivilegeMachineRealtime("PrivilegeMachineRealtime");