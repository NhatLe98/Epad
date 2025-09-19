import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class ServiceApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GelAllService() {
    return this.get("GetAllService");
  }
  public GetDevicesInOutService(serviceIndex: number) {
    return this.get("GetDevicesInOutService", { params: { serviceIndex } });
  }
  public GetServiceAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
    console.log(page, filter, limit)
    return this.getItemsWithPaging("GetServiceAtPage", { params: { page, filter, limit } });
  }

  public AddService(service: IC_Service) {
    return this.post("AddService", service);
  }

  public UpdateService(service: IC_Service) {
    return this.post("UpdateService", service);
  }

  public DeleteService(service: Array<IC_Service>) {
    return this.post("DeleteService", service);
  }

  public DownloadSettingService(service: IC_Service) {
    return this.post("DownloadSettingService", service);
    }
  public GetServiceForDownload() {
      return this.get("GetAllServiceForDownload");
    }
}

export interface IC_Service {
  Index?: number;
  Name?: string;
  Description?: string;
  ListDeviceSerial?: Array<string>;
  ServiceType?: string;
}

export const serviceApi = new ServiceApi("Service");
