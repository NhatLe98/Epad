import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class AreaDoorApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetDoorInOutArea(areaIndex: number) {
        return this.get("GetDoorInOutArea", {
            params: { areaIndex }
        });
    }

    public UpdateAreaDoorDetail(groupdevice: AC_GroupAreaParam) {
        return this.post("UpdateAreaDoorDetail", groupdevice);
    }
}

export interface AC_GroupAreaParam {
    AreaIndex: number;
    ListDoor: Array<string>;
}

export const areaDoorApi = new AreaDoorApi("AC_AreaDoor");