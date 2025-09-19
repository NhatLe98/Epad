import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from '@/$core/base-api'

class ConfigByGroupMachineApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public GetAllConfigByGroupMachine(groupDeviceIndex: number): Promise<any> {
        return this.get('GetAllConfigByGroupMachine', { params: { groupDeviceIndex } });
    }
    public SaveConfig(configCollection: IConfigCollectionGroupDevice, groupIndex: Number) {
        let arrConfig: Array<IConfig> = [];
        Object.keys(configCollection).forEach(key => {
            arrConfig.push(configCollection[key]);
        });
        return this.post('UpdateGroupDeviceConfig', { Data: configCollection, GroupIndex: groupIndex });
    }
}

export interface IConfig {
    Title: string;
    EventType: string;
    TimePos: Array<string>;
    Email: Array<string>;
    SendMailWhenError: boolean;
    AlwaysSend: boolean;
    PreviousDays?: number;
    WriteToDatabase?: boolean;
    WriteToFile?: boolean;
    WriteToFilePath?: string;
    LinkAPI?: string;
    DeleteLogAfterSuccess?: boolean;
    TitleEmailSuccess: string;
    BodyEmailSuccess: string;
    TitleEmailError: string;
    BodyEmailError: string;
}

export interface IConfigCollectionGroupDevice {
    DOWNLOAD_LOG: IConfig;
    DELETE_LOG: IConfig;
    START_MACHINE: IConfig;
    DOWNLOAD_USER: IConfig;
    // RE_PROCESSING_REGISTERCARD: IConfig;
    // DOWNLOAD_PARKING_LOG: IConfig;
}


export const configByGroupMachineApi = new ConfigByGroupMachineApi('ConfigByGroupMachine');