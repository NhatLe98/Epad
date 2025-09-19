import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class TimezoneApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllTimezone() {
        return this.get("GetAllTimezone");
    }
    public GetTimezoneAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetTimezoneAtPage", { params: { page, filter, limit } });
    }
   
    public GetTimezoneByID(UID: number) {
        return this.get("GetTimezoneByID", {
            params: { UID }
        });
    }

    public AddTimezone(timezone: any) {
        return this.post("AddTimezone", timezone);
    }
    public UpdateTimezone(timezone: any) {
        return this.post("UpdateTimezone", timezone);
    }
    public DeleteTimezone(timezone: any) {
        return this.post("DeleteTimezone", timezone);
    }
    public ExportInfoSyncAcUser(){
        return this.get("ExportInfoSyncAcUser");
    }
    public GetAllTimezoneAtPage(){
        return this.get("GetAllTimezoneAtPage");
    }
}

export const timezoneApi = new TimezoneApi("AC_Timezone");