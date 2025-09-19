import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'

class AuditApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetPage(filter: string,limit){
        return this.get("GetPage", { params: { filter,limit } });
    }
    public DeleteAudit(index: number) {
        return this.delete(`Delete?index=${index}`);
    }
    public DeleteList(addedParams: Array<IC_Audit>) {
        return this.post('DeleteList', addedParams);
    }
    public PostPage(addedParams: Array<AddedParam>) {
        return this.post("PostPage", addedParams);
    }
}

export interface AddedParam {
    Key: string;
    Value: any;

}
export interface IC_Audit {
    Index: number;
    UserName: string;
    DateTime: string;
    StateString: string;
    TableName: string;
    AffectedColumns?: string;
    Description?: string;
}
export const auditApi = new AuditApi("Audit");