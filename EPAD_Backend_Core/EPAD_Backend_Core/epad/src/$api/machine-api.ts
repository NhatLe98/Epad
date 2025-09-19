import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class MachineApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetMachineAtPage(page: number): Promise<BaseResponse> {
    return this.getItemsWithPaging("GetMachineAtPage", { params: { page } });
  }

  public AddMachine(machine: IC_Machine) {
    return this.post("AddMachine", machine);
  }

  public UpdateMachine(machine: IC_Machine) {
    return this.post("UpdateMachine", machine);
  }

  public DeleteMachine(machine: IC_Machine) {
    return this.post("DeleteMachine", machine);
  }
}

export interface IC_Machine {
  SerialNumber: string;
  AliasName?: string;
  IPAddress?: string;
  Port?: number;
  DeviceType?: Array<string>;
  UseSDK?: false;
  UsePush?: false;
}

export const machineApi = new MachineApi("Machine");
