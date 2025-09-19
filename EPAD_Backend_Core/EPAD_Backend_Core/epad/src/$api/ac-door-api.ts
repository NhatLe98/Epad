import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class DoorApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllDoor() {
        return this.get("GetAllDoor");
    }
    public GetDoorAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetDoorAtPage", { params: { page, filter, limit } });
    }
    public AddDoor(door: AC_DoorDTO) {
        return this.post("AddDoor", door);
    }
    public UpdateDoor(door: AC_DoorDTO) {
        return this.post("UpdateDoor", door);
    }
    public DeleteDoor(door: Array<AC_DoorDTO>) {
        return this.post("DeleteDoor", door);
    }
    public AddDoorSetting(door: AC_DoorDTO) {
        return this.post("AddDoorSetting", door);
    }
    public UpdateDoorSetting(door: AC_DoorDTO) {
        return this.post("UpdateDoorSetting", door);
    }
    public DeleteDoorSetting(door: Array<AC_DoorDTO>) {
        return this.post("DeleteDoorSetting", door);
    }
}
export interface AC_DoorDTO {
    Index?: number;
    Name?: string;
    Description?: string;
    SerialNumberLst?: Array<string>;
    AreaIndexes?: Array<number>;
    DoorIndexes?: Array<number>;
    AreaIndex?: number;
    DoorOpenTimezoneUID?: number;
    Timezone?: number;
    DoorSettingDescription?: string;
}

export const doorApi = new DoorApi("AC_Door");