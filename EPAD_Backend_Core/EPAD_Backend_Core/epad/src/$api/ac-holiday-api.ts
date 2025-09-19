import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class HolidayApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllHoliday() {
        return this.get("GetAllHoliday");
    }
    public GetHolidayAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetHolidayAtPage", { params: { page, filter, limit } });
    }
    public AddHoliday(door: AC_HolidayDTO) {
        return this.post("AddHoliday", door);
    }
    public UpdateHoliday(door: AC_HolidayDTO) {
        return this.post("UpdateHoliday", door);
    }
    public DeleteHoliday(door: Array<AC_HolidayDTO>) {
        return this.post("DeleteHoliday", door);
    }
}
export interface AC_HolidayDTO {
    UID?: number;
    HolidayName: string;
    DoorIndex?: number;
    DoorIndexes?: Array<number>;
    TimeZone?: number;
    StartDate: Date;
    EndDate: Date;
    TimezoneRange?: number;
    StartDateString?: string;
    EndDateString?: string;
}

export const holidayApi = new HolidayApi("AC_Holiday");