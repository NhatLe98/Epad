import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class PositionApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetAll() {
        return this.get("GetAll");
    }

}

export interface IC_Position {
    Index?: number
    Name?: string;
    NameInEng?: string;
    Description?: string;
}


export const positionApi = new PositionApi("IC_Position");
