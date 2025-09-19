import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class GatesLinesApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}

	public GetAllGates() {
		return this.get('GetAllGates');
	}

	public GetAllGatesLines() {
		return this.get('GetAllGatesLines');
	}

	public GetByGateIndex(gateIndex: number) {
		return this.get('GetByGateIndex', { params: { gateIndex } });
	}

	public UpdateByGateIndex(GateIndex: number, ListLineIndex: Array<number>) {
		return this.put('UpdateByGateIndex', { GateIndex, ListLineIndex });
	}
}

export const gatesLinesApi = new GatesLinesApi('GC_Gates_Lines');

export interface GatesLines {
    GateIndex: number;
    ListLineIndex: Array<any>;
}
