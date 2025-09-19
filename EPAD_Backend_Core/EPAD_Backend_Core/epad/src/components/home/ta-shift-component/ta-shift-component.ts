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
import { taRulesShiftApi } from "@/$api/ta-rules-shift-api";
import { taShiftApi, TA_ShiftDTO } from "@/$api/ta-shift-api";
import { setTimeout } from 'timers';

@Component({
	name: 'tashift',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class TAShiftComponent extends Mixins(ComponentBase) {
	page = 1;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	timePosOption: Array<string> = [];
	ruleForm: TA_ShiftDTO = {
		Name: '',
		Code: '',
		IsPaidHolidayShift: false,
		PaidHolidayStartTime: null,
		PaidHolidayEndTime: null,
		PaidHolidayEndOvernightTime: false,
		CheckInTime: null,
		CheckOutTime: null,
		CheckOutOvernightTime: false,
		IsBreakTime: false,
		BreakStartTime: null,
		BreakEndTime: null,
		BreakStartOvernightTime: false,
		BreakEndOvernightTime: false,
		IsOTFirst: false,
		OTStartTimeFirst: null,
		OTEndTimeFirst: null,
		IsOT: false,
		OTStartTime: null,
		OTEndTime: null,
		OTStartOvernightTime: false,
		OTEndOvernightTime: false,
		AllowLateInMinutes: 0,
		AllowEarlyOutMinutes: 0,
		TheoryWorkedTimeByShift: 0
	};
	rules: any = {};
	tblShift: any = [];
	fullTblShift: any = [];
	selectUserTypeOption = [];
	missingCheckInAttendanceLogIsOption = [
		{ value: 1, label: this.$t('Absent') },
		{ value: 2, label: this.$t('LateEntry') }
	];
	missingCheckOutAttendanceLogIsOption = [
		{ value: 1, label: this.$t('Absent') },
		{ value: 2, label: this.$t('EarlyOut') }
	];

	async beforeMount() {
		this.reset();
		this.initRule();
		this.initTimePosData();
		await this.getRulesShift().then(() => {
			this.reset();
		});
		await this.getAllData().then(() => {
			if (this.tblShift && this.tblShift.length > 0) {
				this.ruleForm = Misc.cloneData(this.tblShift[0]);
				(this.$refs.taShiftTable as any).setCurrentRow(this.tblShift[0]);
			}
		});
	}

	initTimePosData() {
		for (let i = 0; i < 24; i++) {
			this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
			this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
		}
	}

	initRule() {
		this.rules = {
			Code: [
				{
					required: true,
					message: this.$t('PleaseInputShiftCode'),
					trigger: 'change',
				},
			],
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'change',
				},
			],
			RulesShiftIndex: [
				{
					required: true,
					message: this.$t('PleaseSelectRule'),
					trigger: 'change',
				},
			],
			TheoryWorkedTimeByShift: [
				// {
				// 	required: true,
				// 	message: this.$t('PleaseSelectTheoryWorkedTimeByShift'),
				// 	trigger: 'change',
				// },
				{
					trigger: 'change',
					message: this.$t('PleaseSelectTheoryWorkedTimeByShift'),
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.TheoryWorkedTimeByShift == null || this.ruleForm.TheoryWorkedTimeByShift == undefined
							|| this.ruleForm.TheoryWorkedTimeByShift < 0) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			PaidHolidayStartTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsPaidHolidayShift && !this.ruleForm.PaidHolidayStartTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTStartTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('StartTimeCannotLargerThanEndTime'),
					validator: (rule, value: string, callback) => {
						let startValue = null;
						if(this.ruleForm.PaidHolidayStartTime){
							startValue = new Date();
							startValue.setHours(this.ruleForm.PaidHolidayStartTime.getHours(),
							this.ruleForm.PaidHolidayStartTime.getMinutes(), 0, 0);
						}
						let endValue = null;
						if(this.ruleForm.PaidHolidayEndTime){
							endValue = new Date();
							endValue.setHours(this.ruleForm.PaidHolidayEndTime.getHours(),
							this.ruleForm.PaidHolidayEndTime.getMinutes(), 0, 0);
						}
						const start = this.ruleForm.PaidHolidayStartTime
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.PaidHolidayEndTime
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.PaidHolidayEndOvernightTime && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			PaidHolidayEndTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsPaidHolidayShift && !this.ruleForm.PaidHolidayEndTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTEndTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('StartTimeCannotLargerThanEndTime'),
					validator: (rule, value: string, callback) => {
						let startValue = null;
						if(this.ruleForm.PaidHolidayStartTime){
							startValue = new Date();
							startValue.setHours(this.ruleForm.PaidHolidayStartTime.getHours(),
							this.ruleForm.PaidHolidayStartTime.getMinutes(), 0, 0);
						}
						let endValue = null;
						if(this.ruleForm.PaidHolidayEndTime){
							endValue = new Date();
							endValue.setHours(this.ruleForm.PaidHolidayEndTime.getHours(),
							this.ruleForm.PaidHolidayEndTime.getMinutes(), 0, 0);
						}
						const start = this.ruleForm.PaidHolidayStartTime
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.PaidHolidayEndTime
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.PaidHolidayEndOvernightTime && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			BreakStartTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsBreakTime && !this.ruleForm.BreakStartTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputBreakStartTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.BreakStartTime){
								startValue = new Date();
								startValue.setHours(this.ruleForm.BreakStartTime.getHours(),
								this.ruleForm.BreakStartTime.getMinutes(), 0, 0);
								if (this.ruleForm.BreakStartOvernightTime) {
									startValue.setDate(startValue.getDate() + 1);
								}
							}
							let endValue = null;
							if(this.ruleForm.BreakEndTime){
								endValue = new Date();
								endValue.setHours(this.ruleForm.BreakEndTime.getHours(),
								this.ruleForm.BreakEndTime.getMinutes(), 0, 0);
								if (this.ruleForm.BreakEndOvernightTime) {
									endValue.setDate(endValue.getDate() + 1);
								}
							}
							const start = this.ruleForm.BreakStartTime
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.BreakEndTime
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 
								// && !this.ruleForm.BreakEndOvernightTime 
								&& start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (start > 0 && startCheckIn > 0 && start < startCheckIn) {
								callback(new Error(this.$t('BreakTimeMustInCheckInOutTime').toString()));
							}
							callback();
						}
						callback();
					},
				},
			],
			BreakEndTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsBreakTime && !this.ruleForm.BreakEndTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputBreakEndTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.BreakStartTime){
								startValue = new Date();
								startValue.setHours(this.ruleForm.BreakStartTime.getHours(),
								this.ruleForm.BreakStartTime.getMinutes(), 0, 0);
								if (this.ruleForm.BreakStartOvernightTime) {
									startValue.setDate(startValue.getDate() + 1);
								}
							}
							let endValue = null;
							if(this.ruleForm.BreakEndTime){
								endValue = new Date();
								endValue.setHours(this.ruleForm.BreakEndTime.getHours(),
								this.ruleForm.BreakEndTime.getMinutes(), 0, 0);
								if (this.ruleForm.BreakEndOvernightTime) {
									endValue.setDate(endValue.getDate() + 1);
								}
							}
							const start = this.ruleForm.BreakStartTime
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.BreakEndTime
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 
								// && !this.ruleForm.BreakEndOvernightTime 
								&& start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (end > 0 && endCheckOut > 0 && end > endCheckOut) {
								callback(new Error(this.$t('BreakTimeMustInCheckInOutTime').toString()));
							}
							callback();
						}
						callback();
					},
				},
			],
			OTStartTimeFirst: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsOTFirst && !this.ruleForm.OTStartTimeFirst) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTStartTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.OTStartTimeFirst){
								startValue = new Date();
								startValue.setHours(this.ruleForm.OTStartTimeFirst.getHours(),
								this.ruleForm.OTStartTimeFirst.getMinutes(), 0, 0);
							}
							let endValue = null;
							if(this.ruleForm.OTEndTimeFirst){
								endValue = new Date();
								endValue.setHours(this.ruleForm.OTEndTimeFirst.getHours(),
								this.ruleForm.OTEndTimeFirst.getMinutes(), 0, 0);
							}
							
							const start = this.ruleForm.OTStartTimeFirst
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.OTEndTimeFirst
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (start > 0 && startCheckIn > 0 && start < startCheckIn) {
								callback(new Error(this.$t('OTTimeMustInCheckInOutTime').toString()));
							}
							callback();
						}
						callback();
					},
				},
			],
			OTEndTimeFirst: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsOTFirst && !this.ruleForm.OTEndTimeFirst) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTEndTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.OTStartTimeFirst){
								startValue = new Date();
								startValue.setHours(this.ruleForm.OTStartTimeFirst.getHours(),
								this.ruleForm.OTStartTimeFirst.getMinutes(), 0, 0);
							}
							let endValue = null;
							if(this.ruleForm.OTEndTimeFirst){
								endValue = new Date();
								endValue.setHours(this.ruleForm.OTEndTimeFirst.getHours(),
								this.ruleForm.OTEndTimeFirst.getMinutes(), 0, 0);
							}

							const start = this.ruleForm.OTStartTimeFirst
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.OTEndTimeFirst
								? Math.trunc(endValue.getTime() / 1000) : 0;

							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (end > 0 && endCheckOut > 0 && end > endCheckOut) {
								callback(new Error(this.$t('OTTimeMustInCheckInOutTime').toString()));
							}

							//-------------------------------------------------------------------------------
							let startOTValue = null;
							if(this.ruleForm.OTStartTime){
								startOTValue = new Date();
								startOTValue.setHours(this.ruleForm.OTStartTime.getHours(),
								this.ruleForm.OTStartTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTStartOvernightTime) {
									startOTValue.setDate(endValue.getDate() + 1);
								}
							}

							let endOTValue = null;
							if(this.ruleForm.OTEndTime){
								endOTValue = new Date();
								endOTValue.setHours(this.ruleForm.OTEndTime.getHours(),
								this.ruleForm.OTEndTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTEndOvernightTime) {
									endOTValue.setDate(endValue.getDate() + 1);
								}
							}

							const startOT = this.ruleForm.OTStartTime
								? Math.trunc(startOTValue.getTime() / 1000) : 0;

							const endOT = this.ruleForm.OTEndTime
								? Math.trunc(endOTValue.getTime() / 1000) : 0;

							if (!this.ruleForm.OTStartOvernightTime && startOT > 0 && end > 0 && startOT < end) {
								callback(new Error(this.$t('OTFirstCannotLargerThanOT').toString()));
							} 
							callback();
						}
						callback();
					},
				},
			],
			OTStartTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsOT && !this.ruleForm.OTStartTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTStartTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.OTStartTime){
								startValue = new Date();
								startValue.setHours(this.ruleForm.OTStartTime.getHours(),
								this.ruleForm.OTStartTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTStartOvernightTime) {
									startValue.setDate(startValue.getDate() + 1);
								}
							}
							let endValue = null;
							if(this.ruleForm.OTEndTime){
								endValue = new Date();
								endValue.setHours(this.ruleForm.OTEndTime.getHours(),
								this.ruleForm.OTEndTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTEndOvernightTime) {
									endValue.setDate(endValue.getDate() + 1);
								}
							}
							const start = this.ruleForm.OTStartTime
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.OTEndTime
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 
								// && !this.ruleForm.OTEndOvernightTime 
								&& start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (start > 0 && startCheckIn > 0 && start < startCheckIn) {
								callback(new Error(this.$t('OTTimeMustInCheckInOutTime').toString()));
							}
							callback();
						}
						callback();
					},
				},
			],
			OTEndTime: [
				{
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.IsOT && !this.ruleForm.OTEndTime) {
							callback(new Error());
						}
						callback();
					},
					message: this.$t('PleaseInputOTEndTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.CheckInTime && this.ruleForm.CheckOutTime) {
							const startCheckInValue = new Date();
							startCheckInValue.setHours(this.ruleForm.CheckInTime.getHours(),
								this.ruleForm.CheckInTime.getMinutes(), 0, 0);
							const endCheckOutValue = new Date();
							endCheckOutValue.setHours(this.ruleForm.CheckOutTime.getHours(),
								this.ruleForm.CheckOutTime.getMinutes(), 0, 0);
							if (this.ruleForm.CheckOutOvernightTime) {
								endCheckOutValue.setDate(endCheckOutValue.getDate() + 1);
							}
							const startCheckIn = this.ruleForm.CheckInTime
								? Math.trunc(startCheckInValue.getTime() / 1000) : 0;
							const endCheckOut = this.ruleForm.CheckOutTime
								? Math.trunc(endCheckOutValue.getTime() / 1000) : 0;

							let startValue = null;
							if(this.ruleForm.OTStartTime){
								startValue = new Date();
								startValue.setHours(this.ruleForm.OTStartTime.getHours(),
								this.ruleForm.OTStartTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTStartOvernightTime) {
									startValue.setDate(startValue.getDate() + 1);
								}
							}
							let endValue = null;
							if(this.ruleForm.OTEndTime){
								endValue = new Date();
								endValue.setHours(this.ruleForm.OTEndTime.getHours(),
								this.ruleForm.OTEndTime.getMinutes(), 0, 0);
								if (this.ruleForm.OTEndOvernightTime) {
									endValue.setDate(endValue.getDate() + 1);
								}
							}
							const start = this.ruleForm.OTStartTime
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = this.ruleForm.OTEndTime
								? Math.trunc(endValue.getTime() / 1000) : 0;
								console.log(start, end, startCheckIn, endCheckOut)
								console.log(new Date(start), new Date(end), new Date(startCheckIn), new Date(endCheckOut))
								console.log(new Date(start).getSeconds(), new Date(end).getSeconds(), 
								new Date(startCheckIn).getSeconds(), new Date(endCheckOut).getSeconds())
								console.log(new Date(start).getMilliseconds(), new Date(end).getMilliseconds(), 
								new Date(startCheckIn).getMilliseconds(), new Date(endCheckOut).getMilliseconds())
							if (start > 0 && end > 0 
								// && !this.ruleForm.OTEndOvernightTime 
								&& start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							} else if (end > 0 && endCheckOut > 0 && end > endCheckOut) {
								callback(new Error(this.$t('OTTimeMustInCheckInOutTime').toString()));
							}
							callback();
						}
						callback();
					},
				},
			],
			CheckInTime: [
				{
					required: true,
					message: this.$t('PleaseInputCheckInTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('CheckInTimeCannotLargerCheckOutTime'),
					validator: (rule, value: string, callback) => {
						let startValue = null;
						if(this.ruleForm.CheckInTime){
							startValue = new Date();
							startValue.setHours(this.ruleForm.CheckInTime.getHours(),
							this.ruleForm.CheckInTime.getMinutes(), this.ruleForm.CheckInTime.getSeconds(), 0);
						}
						let endValue = null;
						if(this.ruleForm.CheckOutTime){
							endValue = new Date();
							endValue.setHours(this.ruleForm.CheckOutTime.getHours(),
							this.ruleForm.CheckOutTime.getMinutes(), this.ruleForm.CheckOutTime.getSeconds(), 0);
						}
						const start = this.ruleForm.CheckInTime
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.CheckOutTime
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.CheckOutOvernightTime && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			CheckOutTime: [
				{
					required: true,
					message: this.$t('PleaseInputCheckOutTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('CheckInTimeCannotLargerCheckOutTime'),
					validator: (rule, value: string, callback) => {
						let startValue = null;
						if(this.ruleForm.CheckInTime){
							startValue = new Date();
							startValue.setHours(this.ruleForm.CheckInTime.getHours(),
							this.ruleForm.CheckInTime.getMinutes(), this.ruleForm.CheckInTime.getSeconds(), 0);
						}
						let endValue = null;
						if(this.ruleForm.CheckOutTime){
							endValue = new Date();
							endValue.setHours(this.ruleForm.CheckOutTime.getHours(),
							this.ruleForm.CheckOutTime.getMinutes(), this.ruleForm.CheckOutTime.getSeconds(), 0);
						}
						const start = this.ruleForm.CheckInTime
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = this.ruleForm.CheckOutTime
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && !this.ruleForm.CheckOutOvernightTime && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
		};
	}

	mounted() {

	}

	handleCurrentController(val) {
		// console.log(val)
		// this.reset();
		// this.tblShift = Misc.cloneData(this.fullTblShift).map(obj => {
		// 	return { 
		// 		...obj, 
		// 		PaidHolidayStartTime: obj.PaidHolidayStartTime && obj.PaidHolidayStartTime != '' ? new Date(obj.PaidHolidayStartTime) : '',
		// 		PaidHolidayEndTime: obj.PaidHolidayEndTime && obj.PaidHolidayEndTime != '' ? new Date(obj.PaidHolidayEndTime) : '',
		// 		CheckInTime: obj.CheckInTime && obj.CheckInTime != '' ? new Date(obj.CheckInTime) : '',
		// 		CheckOutTime: obj.CheckOutTime && obj.CheckOutTime != '' ? new Date(obj.CheckOutTime) : '',
		// 		BreakStartTime: obj.BreakStartTime && obj.BreakStartTime != '' ? new Date(obj.BreakStartTime) : '',
		// 		BreakEndTime: obj.BreakEndTime && obj.BreakEndTime != '' ? new Date(obj.BreakEndTime) : '',
		// 		OTStartTime: obj.OTStartTime && obj.OTStartTime != '' ? new Date(obj.OTStartTime) : '',
		// 		OTEndTime: obj.OTEndTime && obj.OTEndTime != '' ? new Date(obj.OTEndTime) : '',
		// 		AllowLateInMinutes: obj.AllowLateInMinutes || 0,
		// 		AllowEarlyOutMinutes: obj.AllowEarlyOutMinutes || 0,
		// 		TheoryWorkedTimeByShift: obj.TheoryWorkedTimeByShift || 0,
		// 	};
		// });
		if (val && val.Index) {
			this.ruleForm = Misc.cloneData(this.tblShift.find(x => x.Index == val.Index));
			this.ruleForm.PaidHolidayStartTime = this.ruleForm.PaidHolidayStartTime ? new Date(this.ruleForm.PaidHolidayStartTime) : null;
			this.ruleForm.PaidHolidayEndTime = this.ruleForm.PaidHolidayEndTime ? new Date(this.ruleForm.PaidHolidayEndTime) : null;
			this.ruleForm.CheckInTime = this.ruleForm.CheckInTime ? new Date(this.ruleForm.CheckInTime) : null;
			this.ruleForm.CheckOutTime = this.ruleForm.CheckOutTime ? new Date(this.ruleForm.CheckOutTime) : null;
			this.ruleForm.BreakStartTime = this.ruleForm.BreakStartTime ? new Date(this.ruleForm.BreakStartTime) : null;
			this.ruleForm.BreakEndTime = this.ruleForm.BreakEndTime ? new Date(this.ruleForm.BreakEndTime) : null;
			this.ruleForm.OTStartTimeFirst = this.ruleForm.OTStartTimeFirst ? new Date(this.ruleForm.OTStartTimeFirst) : null;
			this.ruleForm.OTEndTimeFirst = this.ruleForm.OTEndTimeFirst ? new Date(this.ruleForm.OTEndTimeFirst) : null;
			this.ruleForm.OTStartTime = this.ruleForm.OTStartTime ? new Date(this.ruleForm.OTStartTime) : null;
			this.ruleForm.OTEndTime = this.ruleForm.OTEndTime ? new Date(this.ruleForm.OTEndTime) : null;
			this.ruleForm.AllowLateInMinutes = this.ruleForm.AllowLateInMinutes || 0;
			this.ruleForm.AllowEarlyOutMinutes = this.ruleForm.AllowEarlyOutMinutes || 0;
			this.ruleForm.TheoryWorkedTimeByShift = this.ruleForm.TheoryWorkedTimeByShift || 0;
		}
	}

	@Watch("ruleForm", { deep: true })
	handleChangeForm() {
		(this.$refs.ruleForm as any).validate();
		if (this.ruleForm.IsPaidHolidayShift) {
			this.ruleForm.TheoryWorkedTimeByShift = 0;
		}
	}

	async getAllData() {
		await taShiftApi.GetShiftByCompanyIndex().then((res) => {
			if (res.status && res.status == 200 && res.data && (res.data as any).length > 0) {
				this.tblShift = (res.data as any).map(obj => {
					return {
						...obj,
						PaidHolidayStartTime: obj.PaidHolidayStartTime && obj.PaidHolidayStartTime != '' ? new Date(obj.PaidHolidayStartTime) : null,
						PaidHolidayEndTime: obj.PaidHolidayEndTime && obj.PaidHolidayEndTime != '' ? new Date(obj.PaidHolidayEndTime) : null,
						CheckInTime: obj.CheckInTime && obj.CheckInTime != '' ? new Date(obj.CheckInTime) : null,
						CheckOutTime: obj.CheckOutTime && obj.CheckOutTime != '' ? new Date(obj.CheckOutTime) : null,
						BreakStartTime: obj.BreakStartTime && obj.BreakStartTime != '' ? new Date(obj.BreakStartTime) : null,
						BreakEndTime: obj.BreakEndTime && obj.BreakEndTime != '' ? new Date(obj.BreakEndTime) : null,
						OTStartTime: obj.OTStartTime && obj.OTStartTime != '' ? new Date(obj.OTStartTime) : null,
						OTEndTime: obj.OTEndTime && obj.OTEndTime != '' ? new Date(obj.OTEndTime) : null,
						OTStartTimeFirst: obj.OTStartTimeFirst && obj.OTStartTimeFirst != '' ? new Date(obj.OTStartTimeFirst) : null,
						OTEndTimeFirst: obj.OTEndTimeFirst && obj.OTEndTimeFirst != '' ? new Date(obj.OTEndTimeFirst) : null,
						AllowLateInMinutes: obj.AllowLateInMinutes || 0,
						AllowEarlyOutMinutes: obj.AllowEarlyOutMinutes || 0,
						TheoryWorkedTimeByShift: obj.TheoryWorkedTimeByShift || 0,
					};
				});
				this.fullTblShift = Misc.cloneData(this.tblShift);
				this.tblShift = this.tblShift.filter(x => x.Index > 0);
			} else {
				this.tblShift = [];
				this.fullTblShift = [];
			}
		});
	}

	async getRulesShift() {
		await taRulesShiftApi.GetRulesShiftByCompanyIndex().then((res) => {
			if (res.status && res.status == 200 && res.data && (res.data as any).length > 0) {
				this.selectUserTypeOption = (res.data as any).map(obj => {
					return {
						Index: obj.Index,
						Name: obj.Name
					};
				});
			}
		});
	}

	reset() {
		const obj: TA_ShiftDTO = {
			Name: '',
			Code: '',
			IsPaidHolidayShift: false,
			PaidHolidayStartTime: null,
			PaidHolidayEndTime: null,
			PaidHolidayEndOvernightTime: false,
			CheckInTime: null,
			CheckOutTime: null,
			CheckOutOvernightTime: false,
			IsBreakTime: false,
			BreakStartTime: null,
			BreakEndTime: null,
			BreakStartOvernightTime: false,
			BreakEndOvernightTime: false,
			IsOTFirst: false,
			OTStartTimeFirst: null,
			OTEndTimeFirst: null,
			IsOT: false,
			OTStartTime: null,
			OTEndTime: null,
			OTStartOvernightTime: false,
			OTEndOvernightTime: false,
			AllowLateInMinutes: 0,
			AllowEarlyOutMinutes: 0,
			TheoryWorkedTimeByShift: 0
		};
		if (this.selectUserTypeOption && this.selectUserTypeOption.length > 0) {
			obj.RulesShiftIndex = this.selectUserTypeOption[0].Index;
		}
		this.ruleForm = obj;
	}

	Insert() {
		this.isEdit = false;
		this.reset();
		(this.$refs.taShiftTable as any).setCurrentRow(null);
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			// console.log(valid)
			if (!valid) return;
			else {
				this.ruleForm.PaidHolidayStartTimeString = this.ruleForm.PaidHolidayStartTime ? moment(this.ruleForm.PaidHolidayStartTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.PaidHolidayEndTimeString = this.ruleForm.PaidHolidayEndTime ? moment(this.ruleForm.PaidHolidayEndTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.CheckInTimeString = this.ruleForm.CheckInTime ? moment(this.ruleForm.CheckInTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.CheckOutTimeString = this.ruleForm.CheckOutTime ? moment(this.ruleForm.CheckOutTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.BreakStartTimeString = this.ruleForm.BreakStartTime ? moment(this.ruleForm.BreakStartTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.BreakEndTimeString = this.ruleForm.BreakEndTime ? moment(this.ruleForm.BreakEndTime).format("YYYY-MM-DD HH:mm:ss") : '';

				this.ruleForm.OTStartTimeFirstString = this.ruleForm.OTStartTimeFirst ? moment(this.ruleForm.OTStartTimeFirst).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.OTEndTimeFirstString = this.ruleForm.OTEndTimeFirst ? moment(this.ruleForm.OTEndTimeFirst).format("YYYY-MM-DD HH:mm:ss") : '';

				this.ruleForm.OTStartTimeString = this.ruleForm.OTStartTime ? moment(this.ruleForm.OTStartTime).format("YYYY-MM-DD HH:mm:ss") : '';
				this.ruleForm.OTEndTimeString = this.ruleForm.OTEndTime ? moment(this.ruleForm.OTEndTime).format("YYYY-MM-DD HH:mm:ss") : '';

				if (this.ruleForm.Index && this.ruleForm.Index > 0) {
					taShiftApi.UpdateShift(this.ruleForm).then((res: any) => {
						// console.log(res)
						if (res.status && res.status == 200 && res.data) {
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
							this.getAllData().then(() => {
								const currentIndexRow = this.tblShift.findIndex(x => x.Index == this.ruleForm.Index);
								if (currentIndexRow >= 0) {
									(this.$refs.taShiftTable as any).setCurrentRow(this.tblShift[currentIndexRow]);
								}
							});
						} else {
							this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
						}
					}).finally(async () => {

					});
				} else {
					taShiftApi.AddShift(this.ruleForm).then((res: any) => {
						// console.log(res)
						if (res.status && res.status == 200 && res.data) {
							this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
							this.getAllData().then(() => {
								(this.$refs.taShiftTable as any).setCurrentRow(this.tblShift[this.tblShift.length - 1]);
							});
						} else {
							this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
						}
					}).finally(async () => {

					});
				}
			}
		});
	}

	async Delete() {
		if (!this.ruleForm.Index || (this.ruleForm.Index && this.ruleForm.Index == 0)) {
			this.$alert(this.$t('PleaseSelectRuleToDelete').toString(), this.$t('Warning').toString(), { type: "warning" });
		} else {
			this.$confirmDelete().then(async () => {
				await taShiftApi.DeleteShift(this.ruleForm.Index).then((res: any) => {
					// console.log(res)
					if (res.status && res.status == 200 && res.data) {
						this.$alert(this.$t('DeleteSuccess').toString(), this.$t('Notify').toString());
					} else {
						this.$alert(this.$t('DeleteFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
					}
				}).finally(async () => {
					await this.getAllData().then(() => {
						if (this.tblShift && this.tblShift.length > 0) {
							this.ruleForm = this.tblShift[0];
							(this.$refs.taShiftTable as any).setCurrentRow(this.tblShift[0]);
						}
					});
				});
			});
		}
	}

	focus(x) {
		// console.log(x);
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.reset();
	}
}
