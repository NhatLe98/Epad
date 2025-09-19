import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class DoorDeviceApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDeviceInOutDoor(doorIndex: number) {
        return this.get("GetDeviceInOutDoor", {
            params: { doorIndex }
        });
    }

    public UpdateDoorDeviceDetail(groupdevice: AC_GroupDoorParam) {
        return this.post("UpdateDoorDeviceDetail", groupdevice);
    }
}

export interface AC_GroupDoorParam {
    DoorIndex: number;
    ListDevice: Array<string>;
}

export const doorDeviceApi = new DoorDeviceApi("AC_DoorDevice");