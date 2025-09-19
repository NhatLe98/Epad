import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class CameraApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllCamera(): Promise<BaseResponse> {
        return this.get("GetAllCamera");
    }
    public GetCameraAtPage(page: number, filter, limit): Promise<BaseResponse> {
        return this.get("GetCameraAtPage", { params: { page, filter, limit } });
    }
    public AddCamera(param: ICameraParam) {
        return this.post("AddCamera", param);
    }
    public UpdateCamera(param: ICameraParam) {
        return this.post("UpdateCamera", param);
    }
    public GetCameraPictureByCameraIndex(cameraIndex,channel) {
        return this.get("GetCameraPictureByCameraIndex", { params: { cameraIndex, channel } });
    }
    public DeleteCamera(param: Array<number>) {
        return this.post("DeleteCamera", param);
      }
}
export interface ICameraParam {
    
    Index: number;
    Name: string;
    Serial: string;
    IpAddress: string;
    Port: string;
    UserName: string;
    Password: string;
    Description: string;
    Type: string;
}

export const cameraApi = new CameraApi("Camera");