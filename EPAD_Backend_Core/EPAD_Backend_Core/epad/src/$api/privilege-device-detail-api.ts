import { AxiosRequestConfig } from "axios";
import { BaseApi } from '@/$core/base-api'
import { promises } from 'dns';

class PrivilegeDeviceDetailsApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }

    public GetPrivilegeDeviceDetail(): Promise<any> {
        return this.get('GetPrivilegeDeviceDetail');
    }

    public InsertOrUpdatePrivilegeDeviceDetail(param: any): Promise<any> {
        return this.post('InsertOrUpdatePrivilegeDeviceDetail', param);
    }
}

export const privilegeDeviceDetailApi = new PrivilegeDeviceDetailsApi('PrivilegeDeviceDetails');