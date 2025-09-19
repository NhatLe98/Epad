import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class GroupDeviceApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetGroupDevice() {
        return this.get("GetGroupDevice");
    }
    public GetDevicesInOutGroupDevice(groupIndex: number) {
        return this.get("GetDevicesInOutGroupDevice", {
            params: { groupIndex }
        });
    }
    public GetDeviceByGroup(groupIndex: number) {
        return this.get("GetDeviceByGroup", {
            params: { groupIndex }
        });
    }
    public GetGroupDeviceResult() {
        return this.get("GetGroupDeviceResult");
    }
    public UpdateGroupDeviceDetail(groupdevice: IC_GroupDeviceDetails) {
        return this.post("UpdateGroupDeviceDetail", groupdevice);
    }
    public GetGroupDeviceAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetGroupDeviceAtPage", { params: { page, filter, limit } });
    }
    public AddGroupDevice(groupDevice: IC_GroupDevice) {
        return this.post("AddGroupDevice", groupDevice);
    }
    public UpdateGroupDevice(groupDevice: IC_GroupDevice) {
        return this.post("UpdateGroupDevice", groupDevice);
    }
    public DeleteGroupDevice(groupDevice: Array<IC_GroupDevice>) {
        return this.post("DeleteGroupDevice", groupDevice);
    }
}
export interface IC_GroupDevice {
    Index?: number;
    Name?: string;
    Description?: string;
}
export interface IC_GroupDeviceDetails {
    GroupDeviceIndex: number;
    ListDeviceSerial: Array<string>;
}
export const groupDeviceApi = new GroupDeviceApi("GroupDevice");