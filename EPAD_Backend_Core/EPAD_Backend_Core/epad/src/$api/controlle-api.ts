import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";
class ControllerrApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }
    public GetController(page: number, filter: string, limit) {
        return this.get(`GetController?page=${page}&filter=${filter}&limit=${limit}`);
    }

    public AddController(data: Controller) {
        return this.post("AddController", data);
    }

    public UpdateController(data: Controller) {
        return this.put("UpdateController", data);
    }
    
    public DeleteController(index: string) {
        return this.delete(`DeleteController?index=${index}`);
    }
}
export interface Controller {
    Index: number;
    Name: string;
    IPAddress: string;
    Port: string;
    IDController: string;
}
export const controllerrApi = new ControllerrApi("Controller");