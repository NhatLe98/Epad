import {  AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class TA_ListLocationApi extends BaseApi {
  public constructor(_module: string, _config?: AxiosRequestConfig){
    super(_module, _config);
  }
  public GetListLocationAtPage(page: number, limit: number, filter: string): Promise<BaseResponse> {
    return this.get("GetListLocationAtPage", { params: { page, limit, filter } });
  }
  
	public AddLocation(data: TA_ListLocation) {
		return this.post('AddLocation', data);
	}
	public UpdateLocation(data: TA_ListLocation) {
		return this.put('UpdateLocation', data);
	}
  public DeleteLocation(data: Array<any>) {
    return this.delete('DeleteLocation', { data: data });
  }
}
export interface TA_ListLocation {
  LocationIndex?: number;
  LocationName?: string;
  Address?: string;
  Coordinates?: string;
  Radius?: string;
  Description?: string;
}

export interface ListLocationRequest {
  page: number; 
  pageSize: number;
  filter: string;
}

export const taListLocation = new TA_ListLocationApi("TA_ListLocation");