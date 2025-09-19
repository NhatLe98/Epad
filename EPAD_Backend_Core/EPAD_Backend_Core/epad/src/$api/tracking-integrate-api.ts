import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi } from "@/$core/base-api";

class TrackingIntegrateApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetHistoryTrackingIntegrate(page: number, filter: string, fromTime: string, toTime: string, limit: number) {
    return this.get("GetHistoryTrackingIntegrate", {
      params: { page, filter, fromTime, toTime, limit }
    });
  }
}

export const trackingIntegrateApi = new TrackingIntegrateApi("HistoryTrackingIntegrate");
