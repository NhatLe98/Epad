import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_DriverInfoApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetDriverAtPage(param): Promise<any> {
    return this.post('GetDriverAtPage', param);
  }

  public Post_HR_DriverInfo(data: any): Promise<any> {
    return this.post('Post_HR_DriverInfo', data);
  }

  public Put_HR_DriverInfo(data: any): Promise<any> {
    return this.post('Put_HR_DriverInfo', data);
  }
  
  public ExportToExcel(data: any): Promise<any> {
    return this.post('ExportToExcel', data, {responseType: 'blob'});
  }

  public DeleteDriverInfo(data: any): Promise<any> {
    return this.post('DeleteDriverInfo', data);
  }

  public AddDriveInfoFromExcel(data: any): Promise<any> {
    return this.post('AddDriveInfoFromExcel', data);
  }
}

export const hrDriverInfoApi = new HR_DriverInfoApi("HR_DriverInfo");
