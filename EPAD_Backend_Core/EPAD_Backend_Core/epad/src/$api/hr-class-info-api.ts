import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse} from "@/$core/base-api";

class HR_ClassInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetHRClassInfoAtPage(page: number, filter: string, pageSize: number): Promise<BaseResponse> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        return this.get("GetHRClassInfoAtPage", { params: params });
    }

    public GetAllHRClassInfo(): Promise<BaseResponse> {
        return this.get("Get_HR_ClassInfos");
    }
    public AddHRClassInfo(param: HR_ClassInfo) {
        return this.post('Post_HR_ClassInfo', param);
    }

    public UpdateHRClassInfo(classIndex: string, data: any):Promise<any> {
        return this.put(`Put_HR_ClassInfo/${classIndex}`, data);
    }

    public DeleteHRClassInfo(classIndex: string): Promise<any>{
        return this.delete(`Delete_HR_ClassInfo/${classIndex}`);
    }

    public DeleteHRClassMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteClassInfoMulti', { data: e });
    }
}
export interface HR_ClassInfo {
    Index?: string;
    Name?: string;
    Code?: string;
    NameInEng?: string;
    Description?: string;
}

export const hrClassInfoApi = new HR_ClassInfoApi("HR_ClassInfo");
