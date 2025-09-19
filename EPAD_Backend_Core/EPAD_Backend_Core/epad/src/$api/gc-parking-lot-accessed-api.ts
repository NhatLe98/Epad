import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class ParkingLotAccessedApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	// public GetByFilter(accessType: number, parkingLotIndex: number, fromDate: Date, toDate: Date, page: number, pageSize: number) {
	// 	return this.get('GetByFilter', { params: { accessType, parkingLotIndex, fromDate, toDate, page, pageSize } });
	// }

	public GetByFilter(param: ParkingLotAccessedRequest) {
		return this.post('GetByFilter', param);
	}
	public AddParkingLotAccessed(data: ParkingLotAccessedModel) {
		return this.post('AddParkingLotAccessed', data);
	}
	public UpdateParkingLotAccessed(data: ParkingLotAccessedModel) {
		return this.put('UpdateParkingLotAccessed', data);
	}
	public ImportParkingLotAccessed(data: any) {
		return this.post('ImportParkingLotAccessed', data);
	}
	public DeleteParkingLotsAccessed(arrParkingLotAccessed: Array<any>) {
		return this.delete('DeleteParkingLotAccessed', { data: arrParkingLotAccessed });
	}
}
export interface ParkingLotAccessedModel {
	ParkingLotIndex: number;
	EmployeeATID: string;
	EmployeeATIDs: Array<any>;
	CustomerIndex: string;
	AccessType: number;
	FromDate: Date;
	ToDate: Date;
	Description: string;
}

export interface ParkingLotAccessedRequest {
	accessType: Array<number>;
	parkingLotIndex: Array<number>;
	fromDate: Date;
	toDate?: Date;
	filter: string;
	page: number;
	pageSize: number;
}

export const parkingLotAccessedApi = new ParkingLotAccessedApi('GC_ParkingLotAccessed');
