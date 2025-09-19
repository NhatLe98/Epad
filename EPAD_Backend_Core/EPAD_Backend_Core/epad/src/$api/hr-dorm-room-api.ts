import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class DormRoomApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}

	public GetAllFloorLevel() {
		return this.get("GetAllFloorLevel");
	}
	public GetAllDormRoom() {
		return this.get("GetAllDormRoom");
	}
	public GetDormRoomAtPage(page: number, limit: number, filter: string) {
		return this.get("GetDormRoomAtPage", {
		  params: { page, limit, filter }
		});
	}
	public AddDormRoom(data: DormRoomModel) {
		return this.post('AddDormRoom', data);
	}
	public UpdateDormRoom(data: DormRoomModel) {
		return this.post('UpdateDormRoom', data);
	}
	public DeleteDormRoom(arrGates: Array<any>) {
		return this.delete('DeleteDormRoom', { data: arrGates });
	}
}

export interface DormRoomModel {
	Index: number;
	Code: string;
	Name: string;
	FloorLevelIndex: number;
	Description: string;
}

export const dormRoomApi = new DormRoomApi('HR_DormRoom');
