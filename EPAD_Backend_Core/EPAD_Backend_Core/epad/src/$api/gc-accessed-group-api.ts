import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class AccessedGroupApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetAccessedGroup(page: number, filter: string, pageSize: number) {
		return this.get('GetAccessedGroup', { params: { page, filter, pageSize } });
	}
	public GetAccessedGroupAll() {
		return this.get('GetAccessedGroupAll');
    }
	public GetAccessedGroupNormal() {
		return this.get('GetAccessedGroupNormal');
    }
	public GetAccessedGroupByCode(code: string){
		return this.get('GetAccessedGroupByCode', { params: { code }});
	}
	public AddAccessedGroup(data: AccessedGroupModel) {
	
		return this.post('AddAccessedGroup', data);
	}
	public UpdateAccessedGroup(data: AccessedGroupModel) {
		return this.post('UpdateAccessedGroup', data);
	}
	public DeleteAccessedGroup(arrArea: Array<any>) {
		return this.delete('DeleteAccessedGroup', { data: arrArea });
	}

	public GetDataRulesParkingLot() {
		return this.get('GetDataRulesParkingLot');
    }
	public GetDataRulesGeneralAccess() {
		return this.get('GetDataRulesGeneralAccess');
    }
}

export interface AccessedGroupModel {
	Index: number;
	Name: string;
	NameInEng: string;
	// ParkingLotRuleIndex: string;
	GeneralAccessRuleIndex: string;
	// AccessedGroupParentIndex: number;
	Description: string;
}
export interface RequestModel{
	AccessedGroup: AccessedGroupModel;
	GroupDevice: Array<any>;
}
export const accessedGroupApi = new AccessedGroupApi('GC_AccessedGroup');
