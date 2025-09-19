import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class TruckDriverLogApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetAllCardNumber() {
		return this.get('GetAllCardNumber');
	}
  public GetCardNumberById(id: string) {
		return this.get('GetCardNumberById', { params: { id } });
	}
  public GetCardNumberByNumber(number: string) {
		return this.get('GetCardNumberByNumber', { params: { number } });
	}
  public IsTruckDriverIn(tripCode: string) {
		return this.get('IsTruckDriverIn', { params: { tripCode } });
	}
  public IsTruckDriverInNotOut(tripCode: string) {
		return this.get('IsTruckDriverInNotOut', { params: { tripCode } });
	}
  public IsTruckDriverOut(tripCode: string) {
		return this.get('IsTruckDriverOut', { params: { tripCode } });
	}
  public GetTruckDriverInfoByTripCode(tripCode: string) {
		return this.get('GetTruckDriverInfoByTripCode', { params: { tripCode } });
	}
  public GetTransitTruckDriverInfoByVehiclePlate(vehiclePlate: string) {
		return this.get('GetTransitTruckDriverInfoByVehiclePlate', { params: { vehiclePlate } });
	}
  public SaveTruckDriverLog(data: TruckDriverLogModel) {
		return this.post('SaveTruckDriverLog', data);
	}
  public SaveExtraTruckDriverLog(data: Array<ExtraTruckDriverLogModel>) {
		return this.post('SaveExtraTruckDriverLog', data);
	}
  public DeleteExtraTruckDriverLog(index: number) {
		return this.delete('DeleteExtraTruckDriverLog', { params: {index: index} });
	}
  public GetTruckMonitoringHistories(data: VehicleMonitoringHistoryModel) {
    return this.post('GetTruckMonitoringHistories', data);
  }
  public ExportTruckMonitoringHistories(data: VehicleMonitoringHistoryModel) {
    return this.post('ExportTruckMonitoringHistories', data, { responseType: 'blob' });
  }
  public InfoTruckDriverTemplateImport(){
    return this.get('InfoTruckDriverTemplateImport');
  }
  public GetDriverByCCCD(cccd: string): Promise<any> {
    return this.get("GetDriverByCCCD/" + cccd);
  }
  public GetDriverByCardNumber(cardNumber: string): Promise<any> {
    return this.get("GetDriverByCardNumber/" + cardNumber);
  }
  public ReturnCard(data: ReturnDriverCardModel): Promise<any> {
    return this.post('ReturnCard', data);
  }
}

export interface ReturnDriverCardModel {
  TripCode: string;
  CardNumber: string;
  Description: string;
  SerialNumber: string;
}

export interface VehicleMonitoringHistoryModel {
  FromTime: Date;
  ToTime: Date;
  EmployeeIndexes: Array<string>;
  DepartmentIndexes: Array<number>;
  RulesWarningIndexes: Array<number>;
  StatusLog: string;
  Page: number;
  PageSize: number;
  Filter: string;
}

export interface TruckDriverLogModel {
  Index?: number;
	TripCode?: string;
  Time?: Date;
  TimeString?: string;
  InOutMode?: number;
  InOutModeString?: string;
  MachineSerial?: string;
  CardNumber?: string;
  ExtraDriver?: Array<ExtraTruckDriverLogModel>;
}

export interface ExtraTruckDriverLogModel {
  Index?: number;
  TripCode?: number;
	ExtraDriverName?: string;
	ExtraDriverCode?: string;
	BirthDay?: Date;
  CardNumber?: string;
  IsInactive?: boolean;
  BirthDayString?: string;
}

export enum InOutMode {
  Input,
  Output
}

export const truckerDriverLogApi = new TruckDriverLogApi('GC_TruckDriverLog');
