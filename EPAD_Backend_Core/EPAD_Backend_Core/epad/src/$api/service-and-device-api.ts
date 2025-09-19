import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class ServiceAndDeviceApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    
    public GetDeviceByService(addedParams : Array<AddedParam>) {
        return this.post("GetMany", addedParams);
    }
}

export interface AddedParam {
    Key: string;
    Value: any | object;

}

export interface ServiceAndDeviceDTO {
    ServiceIndex?: number;
    ServiceType?: string;
    SerialNumber?: string;
}

export const serviceAndDeviceApi = new ServiceAndDeviceApi("ServiceAndDevice");
