import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class HolidayApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllHoliday() {
        return this.get("GetAllHoliday");
    }
    public GetHolidayByIndex(HolidayIndex: number) {
		return this.get('GetHolidayByIndex', { params: { HolidayIndex } });
	}
    public GetHolidayAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetHolidayAtPage", { params: { page, filter, limit } });
    }
    public AddHoliday(param: TA_HolidayDTO) {
        return this.post("AddHoliday", param);
    }
    public UpdateHoliday(param: TA_HolidayDTO) {
        return this.post("UpdateHoliday", param);
    }
    public DeleteHoliday(index: Array<any>) {
        return this.delete("DeleteHoliday", { data: index });
    }
}
export interface TA_HolidayDTO {
    Index?: number;
    Code?: string;
    Name?: string;
    HolidayDate?: Date;
    HolidayDateString?: string;
    IsPaidWhenNotWorking?: boolean;
    IsRepeatAnnually?: boolean;
}

export const holidayApi = new HolidayApi("TA_Holiday");