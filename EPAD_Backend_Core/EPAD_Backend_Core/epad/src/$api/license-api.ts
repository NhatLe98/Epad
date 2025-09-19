import {  AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class LicenseApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetID2(): Promise<any> {
    return this.get("GetID2");
  }
  public ActivateLicense(request) {
    return this.post("ActivateLicense", request);
  }

  public AddHardwareLicense(param){
    return this.post("AddHardwareLicense", param);
  }
  
  public GetVersion(){
    return this.get("GetVersion");
  }
}

export const licenseApi = new LicenseApi("License");
