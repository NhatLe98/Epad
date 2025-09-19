import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class UserAccountLogApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetHistoryUserByFromToTime(
    page: number,
    fromTime: string,
    toTime: string
  ) {
    return this.get("GetHistoryUserByFromToTime", {
      params: { page, fromTime, toTime }
    });
  }
}

export const userAccountLogApi = new UserAccountLogApi("LogProcess");
