import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class RelayControllerApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
      super(_module, config);
    }

    public GetAllController() {
        return this.get("GetAllController");
    }
    public AddController(controller: IRelayController) {
        return this.post("AddController", controller);
    }
    public UpdateController(controller: IRelayController) {
        return this.post("UpdateController", controller);
    }
    public RemoveController(controller: IRelayController) {
        return this.post("RemoveController", controller);
    }
    public SetOnOrOffDevice(controller: IRelayController) {
        return this.post("SetOnOrOffDevice", controller);
    }
    public GetChannelStatus(controller: IRelayController) {
        return this.post("GetChannelStatus", controller);
    }
    public TelnetMultipleRelayController(lsparam: Array<any>) {
        return this.post("TelnetMultipleRelayController", lsparam);
    }
}

export interface IRelayController {
    Index:number;
    Name: string;
    IPAddress:string;
    Port: number;
    RelayType: string;
    Description:string;
    ListChannel: Array<IChannel>;
    ChannelIndex: number;
    ChannelSeconds: number;
    ChannelStatus: boolean;
    SignalType: number;
}

export interface IChannel {
    Index: number;
    NumberOfSecondsOff: number;
    ChannelStatus: boolean;
    SignalType: number;
}
  
export const relayControllerApi = new RelayControllerApi("Relay");