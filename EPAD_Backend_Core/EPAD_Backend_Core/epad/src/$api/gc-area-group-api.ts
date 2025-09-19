import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class AreaGroupApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetAreaGroups(page: number, filter: string, pageSize: number) {
		return this.get('GetAreaGroups', { params: { page, filter, pageSize } });
	}
	public GetAreaGroupAll() {
		return this.get('GetAreaGroupAll');
    }
	public GetAreaGroupByCode(code: string){
		return this.get('GetAreaGroupByCode', { params: { code }});
	}
	public AddAreaGroup(data: AreaModel, groupDevice: Array<any>) {
		const model: RequestModel = {
			AreaGroup: data,
			GroupDevice: groupDevice
		}
		return this.post('AddAreaGroup', model);
	}
	public UpdateAreaGroup(data: AreaModel, groupDevice: Array<any>) {
		const model: RequestModel = {
			AreaGroup: data,
			GroupDevice: groupDevice
		}
		return this.post('UpdateAreaGroup', model);
	}
	public DeleteAreaGroups(arrArea: Array<any>) {
		return this.delete('DeleteAreaGroups', { data: arrArea });
	}
}

export interface AreaModel {
	Index: number;
	Name: string;
	NameInEng: string;
	Code: string;
	// AreaGroupParentIndex: number;
	Description: string;
}
export interface RequestModel{
	AreaGroup: AreaModel;
	GroupDevice: Array<any>;
}
export const areaGroupApi = new AreaGroupApi('GC_AreaGroup');
