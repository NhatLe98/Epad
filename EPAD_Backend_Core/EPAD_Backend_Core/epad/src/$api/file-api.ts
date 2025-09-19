import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'

class FileApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }

    public UserAddFile(file) {
        return this.post("UserAddFile", { file });
    }

    public ImportProcess(param: ImportProcessParam) {
        return this.post("ImportProcess", param);
    }
    public UserRemoveFile(fileResult) {
        return this.post("UserRemoveFile", fileResult);
    }
}

export interface UploadFileResult {
    Success: boolean;
    Error: string;
    Path: string;
}
export interface ImportProcessParam {
    ListFilePath: string[];
    ProcessClass: string;
}

export const fileApi = new FileApi('File');