import { AxiosRequestConfig } from "axios";
import { BaseApi } from '@/$core/base-api'

class SystemCommandApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig){
        super(_module, config);
    }

    public GetPage(filter: string) {
        return this.get('GetPage', { params: { filter } });
    }

    public PostPage(addedParams: Array<AddedParam>) {
        return this.post('PostPage', addedParams);
    }

    public Delete(addedParams: IC_SystemCommandDTO) {
        return this.post('Delete', addedParams);
    }

    public DeleteList(addedParams: Array<IC_SystemCommandDTO>) {
        return this.post('DeleteList', addedParams);
    }

    public async DeleteByIds(listSystemCommandIndex: string[]) {
        return this.post("DeleteByIds", listSystemCommandIndex);
    }

    public async DeactivateSystemCommands(listSystemCommandIndex: string[]) {
        return this.post("DeactivateSystemCommands", listSystemCommandIndex);
    }

    public GetMany(addedParams: Array<AddedParam>) {
        return this.post('GetMany', addedParams);
    }

    public ReloadCaching() {
        return this.post('ReloadCaching');
    }

    public async renewCommandAsync(commandIndex: number) {
        await this.post('RenewSystemCommand', null, {
            params: {
                systemCommandIndex: commandIndex,
            }
        });;
    }

}
export interface AddedParam {
    Key: string;
    Value: object | string | number | null;

}
export interface IC_SystemCommandDTO {
    Index?: number;
    UpdatedUser?: string;
    CommandName: string;
    SerialNumber: string
    SystemCommandStatus: string;
    ExcutedTime?: Date
    CreatedDate? : Date
    Error?: string
    GroupIndex?: number
}

export const systemCommandApi = new SystemCommandApi('SystemCommand');