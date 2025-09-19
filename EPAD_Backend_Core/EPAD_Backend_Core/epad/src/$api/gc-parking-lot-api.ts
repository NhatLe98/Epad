import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class ParkingLotApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetParkingLots(page: number, filter: string, pageSize: number) {
		return this.get('GetParkingLots', { params: { page, filter, pageSize } });
	}
	public GetParkingLotsAll() {
		return this.get('GetParkingLotsAll');
    }
	public AddParkingLot(data: ParkingLotModel) {
		return this.post('AddParkingLot', data);
	}
	public UpdateParkingLot(data: ParkingLotModel) {
		console.log("update", data)
		return this.put('UpdateParkingLot', data);
	}
	public DeleteParkingLots(arrParkingLots: Array<any>) {
		return this.delete('DeleteParkingLot', { data: arrParkingLots });
	}
	public RegisterMonthCard(){
		return this.post("RegisterMonthCard", null);
	}
}

export interface ParkingLotModel {
	Index: number;
	Code: string;
	Name: string;
	NameInEng: string;
	Capacity: number;
	Description: string;
}
// export interface ParkingLotTree {
//     ID: string;
//     Code: string;
//     Name: string;
//     Type: string;
//     ParentIndex: number;

//     ListChildren: ParkingLotTree[];
// }
export const parkingLotsApi = new ParkingLotApi('GC_ParkingLot');
