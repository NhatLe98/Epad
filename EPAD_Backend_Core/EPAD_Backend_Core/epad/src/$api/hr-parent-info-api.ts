import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_ParentInfoApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetParentAtPage(filter: string, page: number, pageSize: number): Promise<any> {
        const params = new URLSearchParams();
        params.append('filter', filter.toString());
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());

        return this.get('GetParentAtPage', { params: params })

    }

    public GetAllParent(): Promise<any> {
        return this.get('Get_HR_ParentInfos');
    }

    public AddParent(data: any): Promise<any> {
        return this.post('Post_HR_ParentInfo', data);
    }

    public UpdateParent(employeeATID: string, data: any): Promise<any> {
        return this.put(`Put_HR_ParentInfo/${employeeATID}`, data);
    }

    public DeleteParentMulti(e: Array<string>): Promise<any> {
        return this.delete('DeleteParentMulti', { data: e });
    }
    public AddParentFromExcel(arrEmployee): Promise<any> {
        return this.post("AddParentFromExcel", arrEmployee)
    }

    public ExportToExcel(addedParams: Array<AddedParam>) {
        return this.post("ExportToExcel", addedParams);
    }
}
export interface AddedParam {
    Key: string;
    Value: any;

}
export const hrParentInfoApi = new HR_ParentInfoApi("HR_ParentInfo");
