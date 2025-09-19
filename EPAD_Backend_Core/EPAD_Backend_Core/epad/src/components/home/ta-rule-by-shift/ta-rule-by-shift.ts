import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import { timezoneApi } from '@/$api/ac-timezone-api';
import { doorApi } from '@/$api/ac-door-api';
import { commandApi, CommandRequest } from "@/$api/command-api";
import { taRulesShiftApi, TA_Rules_ShiftDTO } from "@/$api/ta-rules-shift-api";

@Component({
	name: 'timezone',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class RuleByShiftComponent extends Mixins(ComponentBase) {
	page = 1;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];

	tblRule = [];
	fullTblRule = [];

	ruleForm: TA_Rules_ShiftDTO = {
		Name: '',
		EarliestAttendanceRangeTime: new Date(),
		LatestAttendanceRangeTime: new Date(),
		CheckOutOvernightTime: false,
		AllowedDoNotAttendance: false,
		RoundingWorkedTime: false,
		RoundingOTTime: false,
		RoundingWorkedHour: false,
		RuleInOut: 1,
		RuleInOutOther: 2,
		MissingCheckInAttendanceLogIs: 1,
		MissingCheckOutAttendanceLogIs: 1,
		LateCheckInMinutes: 0,
		EarlyCheckOutMinutes: 0,
		RoundingWorkedTimeNum: 0,
		RoundingWorkedTimeType: 3,
		RoundingOTTimeNum: 0,
		RoundingOTTimeType: 3,
		RoundingWorkedHourNum: 0,
		RoundingWorkedHourType: 3
	};
	rules: any = {};

	ruleShiftInOut = [
		{TimeMode: 0, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null},
		{TimeMode: 1, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null}
	];

	selectRoundOption = [
		{value: 1, label: this.$t('RoundUp')},
		{value: 2, label: this.$t('RoundDown')},
		{value: 3, label: this.$t('RoundToTheNearestValue')}
	]
	missingCheckInAttendanceLogIsOption = [
		{ value: 1, label: this.$t('Absent') },
        { value: 2, label: this.$t('LateEntry')}
	]
	missingCheckOutAttendanceLogIsOption = [
		{ value: 1, label: this.$t('Absent') },
        { value: 2, label: this.$t('EarlyOut')}
	]

	activeCollapse = ['a','b','c','d','e','f'];

	deleteRuleShiftInOut(index, row, timeMode) {
		if (this.ruleShiftInOut.length <= 1) return;
		if (index < 0) return;

		// this.ruleShiftInOut.splice(index, 1);
		this.ruleShiftInOut.splice(this.ruleShiftInOut.indexOf(row), 1);
	}
	addRuleShiftInOut(index, row, timeMode) {
		if(timeMode == 0){
			this.ruleShiftInOut.push({
				TimeMode: 0,
				FromTime: null,
				FromOvernightTime: false,
				ToTime: null,
				ToOvernightTime: false,
				Error: null
			});
		}else{
			this.ruleShiftInOut.push({
				TimeMode: 1,
				FromTime: null,
				FromOvernightTime: false,
				ToTime: null,
				ToOvernightTime: false,
				Error: null
			});
		}
	}

	async beforeMount() {
		this.reset();
		this.initRule();
		await this.getAllData().then(() => {
			if(this.tblRule && this.tblRule.length > 0){
				this.ruleForm = Misc.cloneData(this.tblRule[0]);
				if(this.ruleForm.RuleInOutTime && this.ruleForm.RuleInOutTime.length > 0){
					this.ruleShiftInOut = this.ruleForm.RuleInOutTime;
					this.ruleShiftInOut.forEach(element => {
						element.FromTime = new Date(element.FromTime);
						element.ToTime = new Date(element.ToTime);
					});
				}else{
					this.ruleShiftInOut = [
						{TimeMode: 0, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null},
						{TimeMode: 1, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null}
					];
				}
				(this.$refs.taRulesShiftTable as any).setCurrentRow(this.tblRule[0]);
			}
		});	
	}

	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'change',
				},
			],
			EarliestAttendanceRangeTime: [
				{
					required: true,
					message: this.$t('PleaseInputEarliestAttendanceRangeTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('EarliestTimeCannotLargerThanLatestTime'),
					validator: (rule, value: string, callback) => {
						const startValue = new Date();
						startValue.setHours(this.ruleForm.EarliestAttendanceRangeTime.getHours(), 
							this.ruleForm.EarliestAttendanceRangeTime.getMinutes(), this.ruleForm.EarliestAttendanceRangeTime.getSeconds());
						const endValue = new Date();
						endValue.setHours(this.ruleForm.LatestAttendanceRangeTime.getHours(), 
							this.ruleForm.LatestAttendanceRangeTime.getMinutes(), this.ruleForm.LatestAttendanceRangeTime.getSeconds());
						const start = this.ruleForm.EarliestAttendanceRangeTime 
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.LatestAttendanceRangeTime 
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.CheckOutOvernightTime && start > end) {
							callback(new Error());
						}else{
							callback();
						}
					},
				},
			],
			LatestAttendanceRangeTime: [
				{
					required: true,
					message: this.$t('PleaseInputLatestAttendanceRangeTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('EarliestTimeCannotLargerThanLatestTime'),
					validator: (rule, value: string, callback) => {
						const startValue = new Date();
						startValue.setHours(this.ruleForm.EarliestAttendanceRangeTime.getHours(), 
							this.ruleForm.EarliestAttendanceRangeTime.getMinutes(), this.ruleForm.EarliestAttendanceRangeTime.getSeconds());
						const endValue = new Date();
						endValue.setHours(this.ruleForm.LatestAttendanceRangeTime.getHours(), 
							this.ruleForm.LatestAttendanceRangeTime.getMinutes(), this.ruleForm.LatestAttendanceRangeTime.getSeconds());
						const start = this.ruleForm.EarliestAttendanceRangeTime 
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.LatestAttendanceRangeTime 
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.CheckOutOvernightTime && start > end) {
							callback(new Error());
						}else{
							callback();
						}
					},
				},
			],
			RuleShiftInOut: [
			{
				required: true,
				trigger: 'change',
				validator: (rule, value: any, callback) => {
					if (!this.ruleShiftInOut 
						|| (this.ruleShiftInOut && this.ruleShiftInOut.length < 1)) {
						callback(new Error(this.$t("PleaseSelectTime").toString()));
					}else{
						let isError = false;
						this.ruleShiftInOut.forEach(element => {
							element.Error = null;                            
							if(!element.FromTime){
								element.Error = this.$t("PleaseSelectTime").toString();
								// callback(new Error(" "));
								isError = true;
							}
							if(!element.ToTime){
								element.Error = this.$t("PleaseSelectTime").toString();
								// callback(new Error(" "));
								isError = true;
							}
							if(!isError){
								const fromTime = new Date();
								fromTime.setHours(element.FromTime.getHours(), 
									element.FromTime.getMinutes(), element.FromTime.getSeconds());
								if(element.FromOvernightTime){
									fromTime.setDate(fromTime.getDate() + 1);
								}
								const toTime = new Date();
								toTime.setHours(element.ToTime.getHours(), 
									element.ToTime.getMinutes(), element.ToTime.getSeconds());
								if(element.ToOvernightTime){
									toTime.setDate(toTime.getDate() + 1);
								}

								const start = element.FromTime 
									? Math.trunc(fromTime.getTime() / 1000 / 60) : 0;
								const end = element.ToTime 
									? Math.trunc(toTime.getTime() / 1000 / 60) : 0;

								if(start > 0 && end > 0 && start > end){
									element.Error = this.$t("FromTimeCannotLargerThanToTime").toString();
									// callback(new Error(" "));
									isError = true;
								}
							}
							this.$forceUpdate();
						});
						for (let i = 0; i < this.ruleShiftInOut.length; i++) {
							if(this.ruleShiftInOut[i].Error && this.ruleShiftInOut[i].Error != ""){
								continue;
							}
							const fromTime = new Date();
							fromTime.setHours(this.ruleShiftInOut[i].FromTime.getHours(), 
							this.ruleShiftInOut[i].FromTime.getMinutes(), this.ruleShiftInOut[i].FromTime.getSeconds());
							if(this.ruleShiftInOut[i].FromOvernightTime){
								fromTime.setDate(fromTime.getDate() + 1);
							}
							const toTime = new Date();
							toTime.setHours(this.ruleShiftInOut[i].ToTime.getHours(), 
							this.ruleShiftInOut[i].ToTime.getMinutes(), this.ruleShiftInOut[i].ToTime.getSeconds());
							if(this.ruleShiftInOut[i].ToOvernightTime){
								toTime.setDate(toTime.getDate() + 1);
							}

							const start = this.ruleShiftInOut[i].FromTime 
								? Math.trunc(fromTime.getTime() / 1000 / 60) : 0;
							const end = this.ruleShiftInOut[i].ToTime 
								? Math.trunc(toTime.getTime() / 1000 / 60) : 0;

							for (let j = 0; j < this.ruleShiftInOut.length; j++) {
								if(i == j || !this.ruleShiftInOut[j].FromTime || !this.ruleShiftInOut[j].ToTime){
									continue;
								}
								const range1 = { from: this.ruleShiftInOut[i].FromTime, to: this.ruleShiftInOut[i].ToTime };
								const range2 = { from: this.ruleShiftInOut[j].FromTime, to: this.ruleShiftInOut[j].ToTime };

								const fromTimeCompare = new Date();
								fromTimeCompare.setHours(this.ruleShiftInOut[j].FromTime.getHours(), 
								this.ruleShiftInOut[j].FromTime.getMinutes(), this.ruleShiftInOut[j].FromTime.getSeconds());
								if(this.ruleShiftInOut[j].FromOvernightTime){
									fromTimeCompare.setDate(fromTimeCompare.getDate() + 1);
								}
								const toTimeCompare = new Date();
								toTimeCompare.setHours(this.ruleShiftInOut[j].ToTime.getHours(), 
								this.ruleShiftInOut[j].ToTime.getMinutes(), this.ruleShiftInOut[j].ToTime.getSeconds());
								if(this.ruleShiftInOut[j].ToOvernightTime){
									toTimeCompare.setDate(toTimeCompare.getDate() + 1);
								}

								const startCompare = this.ruleShiftInOut[j].FromTime 
									? Math.trunc(fromTimeCompare.getTime() / 1000 / 60) : 0;
								const endCompare = this.ruleShiftInOut[j].ToTime 
									? Math.trunc(toTimeCompare.getTime() / 1000 / 60) : 0;
								
								if (
									// (range1.from <= range2.from && range2.from <= range1.to) ||
									// (range1.from <= range2.to && range2.to <= range1.to) ||
									// (range2.from <= range1.from && range1.from <= range2.to) ||
									// (range2.from <= range1.to && range1.to <= range2.to)
									(start <= startCompare && startCompare <= end) ||
									(start <= endCompare && endCompare <= end) ||
									(startCompare <= start && start <= endCompare) ||
									(startCompare <= end && end <= endCompare)
								) {
									if(!this.ruleShiftInOut[i].Error || this.ruleShiftInOut[i].Error == ""){
										this.ruleShiftInOut[i].Error = this.$t("TimeRangeExisted").toString();
									}
									// callback(new Error(" "));
									isError = true;
									// return true;
								}
							}
						}
						// return false;
						this.$forceUpdate();
						if(isError){
							callback(new Error(" "));
						}
					}
					callback();
				},
			}],
		};
	}

	// @Watch("ruleForm", {deep: true})
	// @Watch("ruleShiftInOut", {deep: true})
	handleChangeForm(){
		// (this.$refs.ruleForm as any).validateField('EarliestAttendanceRangeTime');
		// (this.$refs.ruleForm as any).validateField('LatestAttendanceRangeTime');
		(this.$refs.ruleForm as any).validate();
	}

	mounted() {

	}

	initColumns() {
		this.columns = [
			{
				prop: 'Name',
				label: 'Name',
				minWidth: 100,
				display: true
			},
			{
				prop: 'UIDIndex',
				label: 'UID',
				minWidth: 100,
				display: true
			},
			{
				prop: 'Description',
				label: 'Description',
				minWidth: 100,
				display: true
			},
		];
	}

	handleChangeRule(val) {
		// this.reset();
		// this.tblRule = Misc.cloneData(this.fullTblRule).map(obj => {
		// 	return { 
		// 		...obj, 
		// 		RuleInOutOther: obj.RuleInOut > 1 ? obj.RuleInOut : null, 
		// 		RuleInOut: obj.RuleInOut > 1 ? 0 : obj.RuleInOut,
		// 		EarliestAttendanceRangeTime: new Date(obj.EarliestAttendanceRangeTime),
		// 		LatestAttendanceRangeTime: new Date(obj.LatestAttendanceRangeTime),
		// 	};
		// });
		if(val && val.Index){
			this.ruleForm = Misc.cloneData(this.tblRule.find(x => x.Index == val.Index));
			this.ruleForm.EarliestAttendanceRangeTime = new Date(this.ruleForm.EarliestAttendanceRangeTime);
			this.ruleForm.LatestAttendanceRangeTime = new Date(this.ruleForm.LatestAttendanceRangeTime);
			if(this.ruleForm.RuleInOutTime && this.ruleForm.RuleInOutTime.length > 0){
				this.ruleShiftInOut = this.ruleForm.RuleInOutTime;
				this.ruleShiftInOut.forEach(element => {
					element.FromTime = new Date(element.FromTime);
					element.ToTime = new Date(element.ToTime);
				});
			}else{
				this.ruleShiftInOut = [
					{TimeMode: 0, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null},
					{TimeMode: 1, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null}
				];
			}
		}
	}

	async getAllData() {
		await taRulesShiftApi.GetRulesShiftByCompanyIndex().then((res) => {
			if(res.status && res.status == 200 && res.data && (res.data as any).length > 0){
				this.tblRule = (res.data as any).map(obj => {
					return { 
						...obj, 
						RuleInOutOther: obj.RuleInOut > 1 ? obj.RuleInOut : 2, 
						RuleInOut: obj.RuleInOut > 1 ? 0 : obj.RuleInOut,
						EarliestAttendanceRangeTime: new Date(obj.EarliestAttendanceRangeTime),
						LatestAttendanceRangeTime: new Date(obj.LatestAttendanceRangeTime),
						LateCheckInMinutes: obj.LateCheckInMinutes || 0,
						EarlyCheckOutMinutes: obj.EarlyCheckOutMinutes || 0,
						RoundingWorkedTimeNum: obj.RoundingWorkedTimeNum || 0,
						RoundingOTTimeNum: obj.RoundingOTTimeNum || 0,
						RoundingWorkedHourNum: obj.RoundingWorkedHourNum || 0,
					};
				});
				this.fullTblRule = Misc.cloneData(this.tblRule);
				// console.log(this.tblRule);
			}else{
				this.tblRule = [];
				this.fullTblRule = [];
			}
		});
	}

	reset() {
		const current1= new Date();
		const current2 = new Date();
		const obj: TA_Rules_ShiftDTO = {
			Name: '',
			EarliestAttendanceRangeTime: current1,
			LatestAttendanceRangeTime: current2,
			CheckOutOvernightTime: false,
			AllowedDoNotAttendance: false,
			RoundingWorkedTime: false,
			RoundingOTTime: false,
			RoundingWorkedHour: false,
			RuleInOut: 1,
			RuleInOutOther: 2,
			MissingCheckInAttendanceLogIs: 1,
			MissingCheckOutAttendanceLogIs: 1,
			LateCheckInMinutes: 0,
			EarlyCheckOutMinutes: 0,
			RoundingWorkedTimeNum: 0,
			RoundingWorkedTimeType: 3,
			RoundingOTTimeNum: 0,
			RoundingOTTimeType: 3,
			RoundingWorkedHourNum: 0,
			RoundingWorkedHourType: 3
		};
		this.ruleForm = obj;
		this.ruleForm.EarliestAttendanceRangeTime.setHours(0,0,0);
		this.ruleForm.LatestAttendanceRangeTime.setHours(23,59,59);
		this.ruleShiftInOut = [
			{TimeMode: 0, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null},
			{TimeMode: 1, FromTime: null, FromOvernightTime: false, ToTime: null, ToOvernightTime: false, Error: null}
		];
	}

	Insert() {
		this.isEdit = false;
		this.reset();
		(this.$refs.taRulesShiftTable as any).setCurrentRow(null);
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				this.ruleForm.EarliestAttendanceRangeTimeString = moment(this.ruleForm.EarliestAttendanceRangeTime).format("YYYY-MM-DD HH:mm:ss");
				this.ruleForm.LatestAttendanceRangeTimeString = moment(this.ruleForm.LatestAttendanceRangeTime).format("YYYY-MM-DD HH:mm:ss");
				
				const oldRuleInOut = this.ruleForm.RuleInOut;
				if(this.ruleForm.RuleInOut == 0){
					this.ruleForm.RuleInOut = this.ruleForm.RuleInOutOther;
				}
				this.ruleForm.LateCheckInMinutes = this.ruleForm.LateCheckInMinutes || 0;
				this.ruleForm.EarlyCheckOutMinutes = this.ruleForm.EarlyCheckOutMinutes || 0;
				this.ruleForm.RoundingWorkedTimeNum = this.ruleForm.RoundingWorkedTimeNum || 0;
				this.ruleForm.RoundingOTTimeNum = this.ruleForm.RoundingOTTimeNum || 0;
				this.ruleForm.RoundingWorkedHourNum = this.ruleForm.RoundingWorkedHourNum || 0;

				if(this.ruleShiftInOut && this.ruleShiftInOut.length > 0){
					this.ruleForm.RuleInOutTime = Misc.cloneData(this.ruleShiftInOut);
					this.ruleForm.RuleInOutTime = this.ruleForm.RuleInOutTime.filter(x =>
						x.FromTime && x.ToTime
					);
					this.ruleForm.RuleInOutTime.forEach(element => {
						element.FromTimeString = moment(element.FromTime).format("YYYY-MM-DD HH:mm:ss")
						element.ToTimeString = moment(element.ToTime).format("YYYY-MM-DD HH:mm:ss")
					});
				}
				
				if(this.ruleForm.Index && this.ruleForm.Index > 0){
					taRulesShiftApi.UpdateRulesShift(this.ruleForm).then((res: any) => {
						// console.log(res)
						if(res.status && res.status == 200 && res.data){
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
							this.ruleForm.RuleInOut = oldRuleInOut;
							this.getAllData().then(() => {
								const currentIndexRow = this.tblRule.findIndex(x => x.Index == this.ruleForm.Index);
								if(currentIndexRow >= 0){
									(this.$refs.taRulesShiftTable as any).setCurrentRow(this.tblRule[currentIndexRow]);
								}
							});
						}else{
							this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
						}
					}).finally(async () => {

					});
				}else{
					taRulesShiftApi.AddRulesShift(this.ruleForm).then((res: any) => {
						// console.log(res)
						if(res.status && res.status == 200 && res.data){
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
							this.ruleForm.RuleInOut = oldRuleInOut;
							this.getAllData().then(() => {
								(this.$refs.taRulesShiftTable as any).setCurrentRow(this.tblRule[this.tblRule.length - 1]);
							});
						}else{
							this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
						}
					}).finally(async () => {

					});
				}
			}
		});
	}

	async Delete() {
		if(!this.ruleForm.Index || (this.ruleForm.Index && this.ruleForm.Index == 0)){
			this.$alert(this.$t('PleaseSelectRuleToDelete').toString(), this.$t('Warning').toString(), { type: "warning" });
		}else{
			await taRulesShiftApi.IsRuleUsing(this.ruleForm.Index).then((res: any) => {
				// console.log(res.data)
				if(res.data){
					this.$alert(this.$t('RuleIsUsing').toString(), this.$t('Warning').toString(), { type: "warning" });
				}else{
					this.$confirmDelete().then(async () => {
						await taRulesShiftApi.DeleteRulesShift(this.ruleForm.Index).then((res: any) => {
							// console.log(res)
							if(res.status && res.status == 200 && res.data){
								this.$alert(this.$t('DeleteSuccess').toString(), this.$t('Notify').toString());
							}else{
								this.$alert(this.$t('DeleteFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
							}
						}).finally(async () => {
							await this.getAllData().then(() => {
								if(this.tblRule && this.tblRule.length > 0){
									this.ruleForm = this.tblRule[0];
									(this.$refs.taRulesShiftTable as any).setCurrentRow(this.tblRule[0]);
								}
							});	
						});
					});
				}
			})
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.reset();
	}
}
