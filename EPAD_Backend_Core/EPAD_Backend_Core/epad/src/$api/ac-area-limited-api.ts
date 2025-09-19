import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class AreaLimitedApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllAreaLimited() {
        return this.get("GetAllAreaLimited");
    }
    public GetAreaLimitedAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetAreaLimitedAtPage", { params: { page, filter, limit } });
    }
    public AddAreaLimited(area: AC_AreaLimitedDTO) {
        return this.post("AddAreaLimited", area);
    }
    public UpdateAreaLimited(area: AC_AreaLimitedDTO) {
        return this.post("UpdateAreaLimited", area);
    }
    public DeleteAreaLimited(area: Array<AC_AreaLimitedDTO>) {
        return this.post("DeleteAreaLimited", area);
    }
}
export interface AC_AreaLimitedDTO {
    Index?: number;
    Name?: string;
    DoorIndexes?: Array<number>;
    Description?: string;
}

export const areaLimitedApi = new AreaLimitedApi("AC_AreaLimited");