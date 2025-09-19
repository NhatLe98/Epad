import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_ContractorInfoApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetContractorAtPage( page: number, pageSize: number): Promise<any> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());


    return this.get('GetContractorAtPage', { params: params })
}
  public GetContractor(employeeATID: string): Promise<any> {
    return this.get('Get_HR_ContractorInfo/' + employeeATID);
  }
}

export const hrContractorInfoApi = new HR_ContractorInfoApi("HR_ContractorInfo");
