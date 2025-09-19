import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class EmployeeVehicleApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetEmployeeVehicleAll(filter: string, pageIndex: number, pageSize: number) {
		return this.get('GetEmployeeVehicleAll', { params: { filter, pageIndex, pageSize } });
	}
	public GetEmployeeVehicleByFilter(param: EmployeeVehicleRequest | any) {
		return this.post('GetEmployeeVehicleByFilter', param );
	}
	public GetEmployeeVehicle() {
		return this.get('GetEmployeeVehicle');
	}
	public AddEmployeeVehicle(data: EmployeeVehicleRequest) {
		return this.post('AddEmployeeVehicle', data);
	}
	public UpdateEmployeeVehicle(data: any) {
		return this.put('UpdateEmployeeVehicle', data);
	}
	public DeleteEmployeeVehicle(arrIndex: Array<number>) {
		return this.delete('DeleteEmployeeVehicle', { data: arrIndex });
	}
	public ImportEmployeeVehicle(data: any) {
		return this.post('ImportEmployeeVehicle', data);
	}
}
export interface EmployeeVehicleRequest {
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
export const employeeVehicleApi = new EmployeeVehicleApi('GC_EmployeeVehicle');
