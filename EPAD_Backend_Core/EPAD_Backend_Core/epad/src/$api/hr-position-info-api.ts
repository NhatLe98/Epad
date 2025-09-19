import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse} from "@/$core/base-api";

class HR_PositionInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetHRPositionInfoAtPage(page: number, filter: string, pageSize: number): Promise<BaseResponse> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        return this.get("GetHRPositionInfoAtPage", { params: params });
    }

    public GetAllHRPositionInfo(): Promise<BaseResponse> {
        return this.get("Get_HR_PositionInfos");
    }
    public AddHRPositionInfo(param: HR_PositionInfo) {
        return this.post('Post_HR_PositionInfo', param);
    }

    public UpdateHRPositionInfo(positionIndex: string, data: any):Promise<any> {
        return this.put(`Put_HR_PositionInfo/${positionIndex}`, data);
    }

    public DeleteHRPositionInfo(positionIndex: string): Promise<any>{
        return this.delete(`Delete_HR_PositionInfo/${positionIndex}`);
    }

    public DeleteHRPositionMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteHRPostionInfoMulti', { data: e });
    }
}
export interface HR_PositionInfo {
    Index?: string;
    Name?: string;
    Code?: string;
    NameInEng?: string;
    Description?: string;
    MaxOverTimeInYear?:number
}

export const hrPositionInfoApi = new HR_PositionInfoApi("HR_PositionInfo");
