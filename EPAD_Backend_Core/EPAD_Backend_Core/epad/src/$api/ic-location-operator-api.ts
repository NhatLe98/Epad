import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class LocationOperatorApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetLocationOperatorAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
    return this.getItemsWithPaging("GetLocationOperatorAtPage", { params: { page, filter, limit } });
  }

  public AddLocationOperator(userType: IC_LocationOperator) {
    return this.post("AddLocationOperator", userType);
  }

  public UpdateLocationOperator(userType: IC_LocationOperator) {
    return this.post("UpdateLocationOperator", userType);
  }

  public DeleteLocationOperator(userType: Array<IC_LocationOperator>) {
    return this.post("DeleteLocationOperator", userType);
  }

}

export interface IC_LocationOperator {
  Index?: number;
  Name?: string;
  Department?: string;
}
export const locationOperatorApi = new LocationOperatorApi("IC_LocationOperator");
