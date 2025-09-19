import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TrackingSystemLogApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetSystemLog(page: number, filter: string, fromTime: string, toTime: string) {
    return this.get("GetSystemLog", {
      params: { page, filter, fromTime, toTime }
    });
  }
}

export const trackingSystemLogApi = new TrackingSystemLogApi("SystemLog");
