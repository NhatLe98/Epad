import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class DeviceHistoryApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetDeviceHistory(page: number, filter: string, fromTime: string, toTime: string, limit: number) {
    return this.get("GetDeviceHistory", {
      params: { page, filter, fromTime, toTime, limit }
    });
  }
  public GetDeviceHistoryLast7Days() {
    return this.get("GetDeviceHistoryLast7Days");
  }
}

export const deviceHistoryApi = new DeviceHistoryApi("IC_DeviceHistory");
