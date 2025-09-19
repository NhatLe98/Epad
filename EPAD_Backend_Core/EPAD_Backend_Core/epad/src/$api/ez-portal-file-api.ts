import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class EzPortalFileApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetFile(request: EzFile) {
        return this.post("GetFile", request, {
            responseType: "blob"
          });
    }
    public GetFilePath(request: EzFile) {
        return this.post("GetFilePath", request);
    }
    public ExportExcelVehicleMonitoringHistory(request: any) {
        return this.post("ExportExcelVehicleMonitoringHistory", request);
    }
    
}

export interface EzFile {
    Name: string;
    Url: string;
}
export interface EzFileImage {
    name: string;
    url: string;
}
export interface EzFileRequest{
    Index: number;
    Attachments: Array<EzFile>;
}
export interface EzFileRequestWithType{
    Index: number;
    Attachments: Array<EzFile>;
    Type: number;
}
export interface EzFileRequestByListIndexWithType{
    Indexs: Array<number>;
    Attachments: Array<EzFile>;
    Type: number;
}
export const ezPortalFileApi = new EzPortalFileApi("EzPortalFile");