import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_ShiftApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetShiftByCompanyIndex() {
		return this.get('GetShiftByCompanyIndex');
	}
	public GetShiftByIndex(ShiftIndex: number) {
		return this.get('GetShiftByIndex', { params: { ShiftIndex } });
	}
	public AddShift(data: TA_ShiftDTO) {
		return this.post('AddShift', data);
	}
	public UpdateShift(data: TA_ShiftDTO) {
		return this.put('UpdateShift', data);
	}
	public DeleteShift(index: number) {
		return this.delete('DeleteShift', { params: { index: index } });
	}
}

export interface TA_ShiftDTO {
  Index?: number;
  Code: string;
  Name: string;
  Description?: string;
  RulesShiftIndex?: number;
  IsPaidHolidayShift: boolean;
  PaidHolidayStartTime: Date;
  PaidHolidayEndTime: Date;
  PaidHolidayEndOvernightTime: boolean;
  CheckInTime: Date;
  CheckOutTime: Date;
  CheckOutOvernightTime: boolean;
  IsBreakTime: boolean;
  BreakStartTime: Date;
  BreakEndTime: Date;
  BreakStartOvernightTime: boolean;
  BreakEndOvernightTime: boolean;
  IsOTFirst: boolean;
  OTStartTimeFirst: Date;
  OTEndTimeFirst: Date;
  IsOT: boolean;
  OTStartTime: Date;
  OTEndTime: Date;
  OTStartOvernightTime: boolean;
  OTEndOvernightTime: boolean;
  AllowLateInMinutes?: number;
  AllowEarlyOutMinutes?: number;
  TheoryWorkedTimeByShift?: any;
  PaidHolidayStartTimeString?: string;
  PaidHolidayEndTimeString?: string;
  CheckInTimeString?: string;
  CheckOutTimeString?: string;
  BreakStartTimeString?: string;
  BreakEndTimeString?: string;
  OTStartTimeFirstString?: string;
  OTEndTimeFirstString?: string;
  OTStartTimeString?: string;
  OTEndTimeString?: string;
}

export const taShiftApi = new TA_ShiftApi("TA_Shift");
