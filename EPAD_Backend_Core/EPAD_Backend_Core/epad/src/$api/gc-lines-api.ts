import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class LineApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }
  public GetAll() {
    return this.get('GetAll');
  }
  public GetAllLine() {
    return this.get('GetAllLine');
  }
  public GetAllLineBasic() {
    return this.get('GetAllLineBasic');
  }
  public GetLineBySerialNumber(serialNumber: string) {
		return this.get('GetLineBySerialNumber', { params: { serialNumber } });
	}
  public GetAllDevicesLines() {
    return this.get('GetAllDevicesLines');
  }
  public AddLine(data: LineModel) {
    return this.post('AddLine', data);
  }
  public UpdateLine(data: LineModel) {
    return this.put('UpdateLine', data);
  }
  public DeleteLines(arrLineId: Array<any>) {
    return this.delete('DeleteLines', { data: arrLineId });
  }
}

export interface LineModel {
    Index: number;
    Name: string;
    Description: string;
    LineForCustomer: boolean;
    LineForCustomerIssuanceReturnCard: boolean;
    LineForDriver: boolean;
    LineForDriverIssuanceReturnCard: boolean; 
}

export const lineApi = new LineApi('GC_Lines');
