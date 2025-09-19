import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_RulesShiftApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetRulesShiftByCompanyIndex() {
		return this.get('GetRulesShiftByCompanyIndex');
	}
	public GetRulesShiftByIndex(RulesShiftIndex: number) {
		return this.get('GetRulesShiftByIndex', { params: { RulesShiftIndex } });
	}
	public AddRulesShift(data: TA_Rules_ShiftDTO) {
		return this.post('AddRulesShift', data);
	}
  public IsRuleUsing(index: number) {
		return this.get('IsRuleUsing', { params: { index } });
	}
	public UpdateRulesShift(data: TA_Rules_ShiftDTO) {
		return this.put('UpdateRulesShift', data);
	}
	public DeleteRulesShift(index: number) {
		return this.delete('DeleteRulesShift', { params: { index: index } });
	}
}

export interface TA_Rules_ShiftDTO {
  Index?: number;
  Name: string;
  Description?: string;
  RuleInOut?: number;
  RuleInOutOther?: number;
  EarliestAttendanceRangeTime: Date;
  EarliestAttendanceRangeTimeString?: string;
  LatestAttendanceRangeTime: Date;
  LatestAttendanceRangeTimeString?: string;
  CheckOutOvernightTime: boolean;
  AllowedDoNotAttendance: boolean;
  MissingCheckInAttendanceLogIs?: number;
  MissingCheckOutAttendanceLogIs?: number;
  LateCheckInMinutes?: number;
  EarlyCheckOutMinutes?: number;
  MaximumAnnualLeaveRegisterByMonth?: number;
  MaximumAnnualLeaveRegisterByYear?: number;
  RoundingWorkedTime: boolean;
  RoundingWorkedTimeNum?: number;
  RoundingWorkedTimeType?: number;
  RoundingOTTime: boolean;
  RoundingOTTimeNum?: number;
  RoundingOTTimeType?: number;
  RoundingWorkedHour: boolean;
  RoundingWorkedHourNum?: number;
  RoundingWorkedHourType?: number;
  RuleInOutTime?: Array<any>;
}

export interface TA_Rules_Shift_InOut {
  RuleShiftIndex?: number;
  TimeMode?: number;
  FromTime?: Date;
  FromOvernightTime: boolean;
  ToTime?: Date;
  ToOvernightTime: boolean;
  FromTimeString: string;
  ToTimeString: string;
}

export const taRulesShiftApi = new TA_RulesShiftApi("TA_Rules_Shift");
