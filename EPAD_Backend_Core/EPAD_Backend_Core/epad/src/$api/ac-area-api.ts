import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class AreaApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllArea() {
        return this.get("GetAllArea");
    }
    public GetAreaAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetAreaAtPage", { params: { page, filter, limit } });
    }
    public AddArea(area: AC_AreaDTO) {
        return this.post("AddArea", area);
    }
    public UpdateArea(area: AC_AreaDTO) {
        return this.post("UpdateArea", area);
    }
    public DeleteArea(area: Array<AC_AreaDTO>) {
        return this.post("DeleteArea", area);
    }
}
export interface AC_AreaDTO {
    Index?: number;
    Name?: string;
    Description?: string;
}

export const areaApi = new AreaApi("AC_Area");