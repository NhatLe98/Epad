import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class GatesApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetGatesAll() {
		return this.get('GetGatesAll');
    }
	public GetGateByDevice() {
		return this.get('GetGateByDevice');
    }
	public AddGates(data: GatesModel) {
		return this.post('AddGates', data);
	}
	public UpdateGates(data: GatesModel) {
		return this.post('UpdateGates', data);
	}
	public DeleteGates(arrGates: Array<any>) {
		return this.delete('DeleteGates', { data: arrGates });
	}
	public UpdateGateLineDevice(data: GatesModel) {
		return this.post('UpdateGateLineDevice', data);
	}

	public GetGateLinesAsTree() {
		return this.get('GetGateLinesAsTree');
    }
}

export interface GatesModel {
	Index: number;
	Name: string;
	Description: string;
	IsMandatory: boolean;
	Lines: Array<number>;
	LineDevice: LinesParam;
}

export interface LinesParam {
	Index: number;
	Name: string;
	Description: string;
	DeviceInSerial: Array<string>;
	DeviceOutSerial: Array<string>;
	CameraInIndex: Array<number>;
	CameraOutIndex: Array<number>;
	LineControllersIn: Array<LineController>;
  	LineControllersOut: Array<LineController>;
}

export interface LineController {
	ControllerIndex: number;
	OpenChannel: number;
	CloseChannel: number;
}
export interface GateTree {
    ID: string;
    Code: string;
    Name: string;
    Type: string;
    ParentIndex: number;

    ListChildren: GateTree[];
}
export const gatesApi = new GatesApi('GC_Gates');
