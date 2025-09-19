import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class WorkingInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public WorkingInfo(
        page: number,
        filter: string,
        fromDate: string,
        toDate: string
    ): Promise<BaseResponse> {
        return this.get("GetWorkingInforAtPage", { params: { page, filter, fromDate, toDate } });
    }

    public GetTransferLast7Days(): Promise<BaseResponse> {
        return this.get("GetTransferLast7Days");
    }

    public AddWorkingInfo(workingInfo: IC_WorkingInfo) {
        return this.post("AddWorkingInfo", workingInfo);
    }
    public AddWorkingInfos(workingInfos: Array<IC_WorkingInfo>) {
        return this.post("AddWorkingInfos", workingInfos);
    }
    public AddListWorkingInfo(workingInfos: IC_WorkingInfo, ArrEmployeeATID: Array<string>) {
        var workingInfo = Object.assign(workingInfos, { ArrEmployeeATID });
        return this.post("AddListWorkingInfo", workingInfo);
    }
    public DeleteWorkingInfo(index: number) {
        return this.delete(`DeleteWorkingInfo?index=${index}`);
    }
    public DeleteWorkingInfos(index: string) {
        return this.delete(`DeleteWorkingInfos?index=${index}`);
    }

    public PostPage(addedParams: Array<AddedParam>) {
        return this.post("PostPage", addedParams);
    }
}

export interface AddedParam {
    Key: string;
    Value: object | string | number | null;

}
export interface IC_WorkingInfo {
    Index: number;
    EmployeeATID: string;
    DepartmentIndex: string;
    FromDate: Date;
    ToDate?: Date;
}
export const workingInfoApi = new WorkingInfoApi("WorkingInfo");