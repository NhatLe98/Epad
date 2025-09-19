import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class CustomerVehicleApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetCustomerVehicleAll(filter: string, pageIndex: number, pageSize: number) {
		return this.get('GetCustomerVehicleAll', { params: { filter, pageIndex, pageSize } });
	}
	public GetCustomerVehicleByFilter(param: CustomerVehicleRequest | any) {
		return this.post('GetCustomerVehicleByFilter', param );
	}
	public GetCustomerVehicle() {
		return this.get('GetCustomerVehicle');
	}
	public AddCustomerVehicle(data: CustomerVehicleRequest) {
		return this.post('AddCustomerVehicle', data);
	}
	public UpdateCustomerVehicle(data: any) {
		return this.put('UpdateCustomerVehicle', data);
	}
	public DeleteCustomerVehicle(arrIndex: Array<number>) {
		return this.delete('DeleteCustomerVehicle', { data: arrIndex });
	}
	public ImportCustomerVehicle(data: any) {
		return this.post('ImportCustomerVehicle', data);
	}
}
export interface CustomerVehicleRequest {
	EmployeeATID: string;
	EmployeeATIDs: Array<any>;
	DepartmentIndexes?: Array<any>;
	Type: number;
	StatusType: number;
	Plate: string;
	Branch: string;
    Color: string;
	Description: string;
	Filter?: string;
	PageIndex?: number;
	PageSize?: number;
}
export const customerVehicleApi = new CustomerVehicleApi('GC_CustomerVehicle');
