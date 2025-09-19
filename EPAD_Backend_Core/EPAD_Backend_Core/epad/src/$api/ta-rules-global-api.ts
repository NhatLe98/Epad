import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_RulesGlobalApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetRulesGlobal() {
		return this.get('GetRulesGlobal');
	}
	public UpdateRulesGlobal(data: TA_RulesGlobalDTO) {
		return this.put('UpdateRulesGlobal', data);
	}
}

export interface TA_RulesGlobalDTO {
  Index?: number;
  MaximumAnnualLeaveRegisterByMonth?: number;
  LockAttendanceTime?: number;
  OverTimeNormalDay?: number;
  NightOverTimeNormalDay?: number;
  OverTimeLeaveDay?: number;
  NightOverTimeLeaveDay?: number;
  OverTimeHoliday?: number;
  NightOverTimeHoliday?: number;
  NightShiftStartTime?: Date;
  NightShiftEndTime?: Date;
  NightShiftOvernightEndTime?: boolean;
  NightShiftStartTimeString?: string;
  NightShiftEndTimeString?: string;
  IsAutoCalculateAttendance?: boolean;
  ListTimePos?: Array<string>;
  TimePos?: string;
}

export const taRulesGlobalApi = new TA_RulesGlobalApi("TA_RulesGlobal");
