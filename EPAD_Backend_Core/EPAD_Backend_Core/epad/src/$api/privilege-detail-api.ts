import { AxiosRequestConfig } from "axios";
import { BaseApi } from '@/$core/base-api'
import { promises } from 'dns';

class PrivilegeDetailApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }

    public GetPrivilegeDetail(): Promise<any> {
        return this.get('GetPrivilegeDetail');
    }

    public UpdatePrivilegeDetail(lsparam: any): Promise<any> {
        return this.post('UpdatePrivilegeDetail', lsparam);
    }

    public GetCurrentPrivilege(): Promise<any> {
        return this.get('GetCurrentPrivilege');
    }

    public CheckPrivilege(formName: string): Promise<any> {
        return this.get('CheckPrivilege', {params: { formName}});
    }

    public CheckPrivilegeFull(formName: string): Promise<any> {
        return this.get('CheckPrivilegeFull', {params: { formName}});
    }
}

export const privilegeDetailApi = new PrivilegeDetailApi('PrivilegeDetail');