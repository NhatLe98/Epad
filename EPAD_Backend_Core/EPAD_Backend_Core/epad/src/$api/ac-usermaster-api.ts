import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class ACUserMasterApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }


    public GetACSync(page: number, fromDate: string, toDate: string, filter: string, departmentIds: Array<number>, 
        listDoor: Array<number>,listArea: Array<number>, limit, viewMode, viewOperation): Promise<BaseResponse> {
        return this.post("GetACSync", { page, fromDate, toDate, filter, departmentIds, limit, listDoor, listArea, viewMode, viewOperation });
    }
   
    public GetACOperation() {
        return this.get("GetACOperation");
    }
}


export const acUserMasterApi = new ACUserMasterApi("AC_UserMaster");