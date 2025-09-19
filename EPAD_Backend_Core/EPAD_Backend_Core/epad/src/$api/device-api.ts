import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class DeviceApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDeviceAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.get("GetDeviceAtPage", { params: { page, filter, limit } });
    }
    public AddDevice(device: IC_Device) {
        return this.post("AddDevice", device);
    }

    public UpdateDevice(device: IC_Device) {
        return this.post("UpdateDevice", device);
    }

    public DeleteDevice(device: Array<IC_Device>) {
        return this.post("DeleteDevice", device);
    }
    public GetDeviceAuthenMode() {
        return this.get("GetDeviceAuthenMode");
    }
    public GetDeviceAll() {
        return this.get("GetDeviceAll");
    }
    public GetAllDevice() {
        return this.get("GetAllDevice");
    }
    public GetListDeviceInfo() {
        return this.get("GetListDeviceInfo");
    }
    public GetDeviceAllPrivilege() {
        return this.get("GetDeviceAllPrivilege");
    }
    public GetAllDevicePrivilege(): Promise<any> {
        return this.get('GetAllDevicePrivilege');
    }

    public ImportDevice(devices) {
        return this.post('ImportDevice', devices);
    }
    
    public GetIPAddressBySerialNumbers(serialNumbers: string) {
        return this.get("GetIPAddressBySerialNumbers", {
            params: { serialNumbers }
        });
    }

    public GetProducerEnumList(){
        return this.get('GetProducerEnumList');
    }

    public GetDepartmentsBySerialNumber(serialNumber){
        return this.get("GetDepartmentsBySerialNumber", { params: { serialNumber } });
    }

    public UpdateDeviceAndDepartments(param: DeviceAndDepartmentsParam) {
        return this.post("UpdateDeviceAndDepartments", param);
    }

}

export interface IC_Device {
    SerialNumber?: string;
    AliasName?: string;
    IPAddress?: string;
    Port?: number;
    DeviceType?: Array<string>;
    UseSDK?: false;
    UsePush?: false;
    DeviceName?: string;
    IsSDK?: string;
    IsPush?: string;
    UserCount?: number;
    FingerCount?: number;
    AttendanceLogCount?: number;
    UserCapacity?: number;
    AttendanceLogCapacity?: number;
    FingerCapacity?: number;
    FaceCapacity?: number;
    LastConnection?: string;
    Status?: string;
    DeviceStatus?: number;
    ConnectionCode?: string;
    DeviceId?: string;
    DeviceModel?: string;
    DeviceModule?: string;
    ListRunningCommand?: Array<string>;
    Note?: string;
    IsUsingOffline?: boolean
}

export interface DeviceAndDepartmentsParam {
    SerialNumber: string;
    ListDepartmentIndexs: Array<number>;
}

export const deviceApi = new DeviceApi("Device");
