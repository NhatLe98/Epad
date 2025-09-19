import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class BlackListApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	// public GetByFilter(accessType: number, parkingLotIndex: number, fromDate: Date, toDate: Date, page: number, pageSize: number) {
	// 	return this.get('GetByFilter', { params: { accessType, parkingLotIndex, fromDate, toDate, page, pageSize } });
	// }

	public GetByFilter(param: BlackListRequest) {
		return this.post('GetByFilter', param);
	}
	public AddBlackList(data: BlackListModel) {
		return this.post('AddBlackList', data);
	}
	public UpdateBlackList(data: BlackListModel) {
		return this.put('UpdateBlackList', data);
	}
	public ImportBlackList(data: any) {
		return this.post('ImportBlackList', data);
	}
	public DeleteBlackList(arrBlackList: Array<any>) {
		return this.delete('DeleteBlackList', { data: arrBlackList });
	}
	public RemoveEmployeeInBlackList(data: RemoveBlackListModel) {
		return this.post('RemoveEmployeeInBlackList', data);
	}
}
export interface BlackListModel {
	Index: number;
	IsEmployeeSystem: boolean;
	FullName: string;
	Nric: string;
	EmployeeATID: string;
	Reason: string;
	FromDate: Date;
	ToDate:Date;
}

export interface RemoveBlackListModel {
	Index: number;
	ToDate: Date;
	ReasonRemoveBlackList: string;
}

export interface BlackListRequest {
	fromDate: Date;
	toDate?: Date;
	filter: string;
	page: number;
	pageSize: number;
}

export const blackListApi = new BlackListApi('GC_BlackList');
