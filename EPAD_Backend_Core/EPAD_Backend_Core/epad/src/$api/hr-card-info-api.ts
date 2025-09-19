import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse} from "@/$core/base-api";

class HR_CardInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetHRCardInfoAtPage(page: number, filter: string, pageSize: number): Promise<BaseResponse> {
        const params = new URLSearchParams();
        params.append('page', page.toString());
        params.append('filter', filter.toString());
        params.append('pageSize', pageSize.toString());
        return this.get("GetHRCardInfoAtPage", { params: params });
    }
    public AddHRCardInfo(param: HR_CardInfo) {
        return this.post('Post_HR_CardNumberInfo', param);
    }

    public UpdateHRCardInfo(cardIndex: number, data: any):Promise<any> {
        return this.put(`Put_HR_CardInfo/${cardIndex}`, data);
    }

    public DeleteHRCardInfo(cardIndex: number): Promise<any>{
        return this.delete(`Delete_HR_CardInfo/${cardIndex}`);
    }

    public DeleteHRCardMulti(cardIndex: Array<number>): Promise<any> {
        return this.delete('DeleteCardMultiByCardIndex', { data: cardIndex });
    }
}
export interface HR_CardInfo {
    Index?: number;
    EmployeeATID?: string;
    CardNumber?: string;
    IsActive?: boolean;
    Status?: string;
    UpdatedDate?:Date
}

export const hrCardInfoApi = new HR_CardInfoApi("HR_CardNumberInfo");
