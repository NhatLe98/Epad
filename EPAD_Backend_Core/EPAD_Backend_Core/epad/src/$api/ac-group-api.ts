import { AxiosError, AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class GroupApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllGroup() {
        return this.get("GetAllGroup");
    }
    public GetGroupAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
        return this.getItemsWithPaging("GetGroupAtPage", { params: { page, filter, limit } });
    }
    public AddGroup(area: any) {
        return this.post("AddGroup", area);
    }
    public UpdateGroup(area: any) {
        return this.post("UpdateGroup", area);
    }
    public DeleteGroup(area: Array<any>) {
        return this.post("DeleteGroup", area);
    }
}


export const groupApi = new GroupApi("AC_Group");