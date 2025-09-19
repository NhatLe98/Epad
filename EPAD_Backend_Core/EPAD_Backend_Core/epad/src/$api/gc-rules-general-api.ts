import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class RulesGeneralApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetRulesGeneralByCompanyIndex() {
		return this.get('GetRulesGeneralByCompanyIndex');
	}
	public GetRuleGeneralLogByRuleGeneralIndex(rulesGeneralIndex: number) {
		return this.get('GetRuleGeneralLogByRuleGeneralIndex', { params: { rulesGeneralIndex } });
	}
	public AddRulesGeneral(data: GeneralRulesModel) {
		return this.post('AddRulesGeneral', data);
	}
	public UpdateRulesGeneral(data: GeneralRulesModel) {
		return this.put('UpdateRulesGeneral', data);
	}
	public DeleteRulesGeneral(index: number) {
		return this.delete('DeleteRulesGeneral', { params: { index: index } });
	}

	public AddRulesGeneralLog(data: Array<RulesGeneralLogRequestModel>) {
		return this.post('AddRulesGeneralLog', data);
	}
	
	public GetRulesGeneralRunWithoutScreen() {
		return this.get('GetRulesGeneralRunWithoutScreen');
	}
}

export interface GeneralRulesModel {
	Index: number;
	Name: string;
	NameInEng: string;
	FromDate: Date;
	ToDate: Date | null;
	StartTimeDay: string;
	MaxAttendanceTime: number;
	IsUsing: boolean;
	PresenceTrackingTime: number;
}

export interface RulesGeneralLogRequestModel {
	Index: number;

	AreaGroupIndex: string | Array<number>;
	UseDeviceMode: boolean;
	UseSequenceLog: boolean;
	UseMinimumLog: boolean;
	UseTimeLog: boolean;
	UseMode: number;
	MinimumLog: number;

	FromEarlyDate: Date | null;
	FromDate: Date | null;
	ToDate: Date | null;
	ToLateDate: Date | null;
	FromIsNextDay: boolean;
	ToIsNextDay: boolean;
	ToLateIsNextDay: boolean;

	RuleGeneralIndex: number;
}

export const rulesGeneralApi = new RulesGeneralApi('GC_Rules_General');
