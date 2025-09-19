import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class LeaveTypeApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllLeaveDateType() {
        return this.get("GetAllLeaveDateType");
    }
    public GetLeaveDateTypeByIndex(LeaveDateTypeIndex: number) {
		return this.get('GetLeaveDateTypeByIndex', { params: { LeaveDateTypeIndex } });
	}
    public GetLeaveDateTypeAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetLeaveDateTypeAtPage", { params: { page, filter, limit } });
    }
    public AddLeaveDateType(param: TA_LeaveDateTypeDTO) {
        return this.post("AddLeaveDateType", param);
    }
    public UpdateLeaveDateType(param: TA_LeaveDateTypeDTO) {
        return this.post("UpdateLeaveDateType", param);
    }
    public DeleteLeaveDateType(index: Array<any>) {
        return this.delete("DeleteLeaveDateType", { data: index });
    }
}
export interface TA_LeaveDateTypeDTO {
    Index?: number;
    Code?: string;
    Name?: string;
    IsWorkedTimeHoliday?: boolean;
    IsPaidLeave?: boolean;
    IsOptionHoliday?: boolean;
}

export const leaveDateTypeApi = new LeaveTypeApi("TA_LeaveDateType");