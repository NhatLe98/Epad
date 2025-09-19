import { split } from 'lodash';
import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { TA_RulesGlobalDTO, taRulesGlobalApi } from '@/$api/ta-rules-global-api';

@Component({
	name: 'RulesGlobalType',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class RulesGlobalComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: TA_RulesGlobalDTO = {
		LockAttendanceTime: null,
		MaximumAnnualLeaveRegisterByMonth: 0,
		OverTimeNormalDay: 0,
		NightOverTimeNormalDay: 0,
		OverTimeLeaveDay: 0,
		NightOverTimeLeaveDay: 0,
		OverTimeHoliday: 0,
		NightOverTimeHoliday: 0,
		ListTimePos: [],
		IsAutoCalculateAttendance: null
	};
	rules: any = {};
	listLockAttendanceTime = [];
	timePosOption: Array<string> = [];
	activeCollapse = ['a','b','c','d','e'];
	listTimePos = [];
	beforeMount() {
		this.initRule();
		this.initTimePosData();
		for(let i = 1; i <= 31; i++){
			this.listLockAttendanceTime.push({Value: i, Name: i});
		}
		this.ruleForm.LockAttendanceTime = this.listLockAttendanceTime[0].Value;
	 this.getData();
	}
	initRule() {
		this.rules = {
			NightShiftStartTime: [
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if(!this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							callback(new Error(this.$t('PleaseSelectStartTime').toString()));
						}else if(this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							const startValue = new Date();
							startValue.setHours((this.ruleForm.NightShiftStartTime as Date).getHours(), 
							(this.ruleForm.NightShiftStartTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftStartTime as Date).getSeconds());
							const endValue = new Date();
							endValue.setHours((this.ruleForm.NightShiftEndTime as Date).getHours(), 
							(this.ruleForm.NightShiftEndTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftEndTime as Date).getSeconds());
							if(this.ruleForm.NightShiftOvernightEndTime){
								endValue.setDate(endValue.getDate() + 1);
							}
							const start = (this.ruleForm.NightShiftStartTime as Date) 
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = (this.ruleForm.NightShiftEndTime as Date) 
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							}
						}
						callback();
					},
				},
			],
			NightShiftEndTime: [
				{
					trigger: 'change',
					validator: (rule, value: string, callback) => {
						if(this.ruleForm.NightShiftStartTime && !this.ruleForm.NightShiftEndTime){
							callback(new Error(this.$t('PleaseSelectEndTime').toString()));
						}else if(this.ruleForm.NightShiftStartTime && this.ruleForm.NightShiftEndTime){
							const startValue = new Date();
							startValue.setHours((this.ruleForm.NightShiftStartTime as Date).getHours(), 
							(this.ruleForm.NightShiftStartTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftStartTime as Date).getSeconds());
							const endValue = new Date();
							endValue.setHours((this.ruleForm.NightShiftEndTime as Date).getHours(), 
							(this.ruleForm.NightShiftEndTime as Date).getMinutes(), 
							(this.ruleForm.NightShiftEndTime as Date).getSeconds());
							if(this.ruleForm.NightShiftOvernightEndTime){
								endValue.setDate(endValue.getDate() + 1);
							}
							const start = (this.ruleForm.NightShiftStartTime as Date) 
								? Math.trunc(startValue.getTime() / 1000) : 0;
							const end = (this.ruleForm.NightShiftEndTime as Date) 
								? Math.trunc(endValue.getTime() / 1000) : 0;
							if (start > 0 && end > 0 && start > end) {
								callback(new Error(this.$t('StartTimeCannotLargerThanEndTime').toString()));
							}
						}
						callback();
					},
				},
			],
		};
	}

	mounted() {
	
	}

	initTimePosData() {
        for (let i = 0; i < 24; i++) {
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
            this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
        }
    }
	@Watch("ruleForm", {deep: true})
	handleChangeForm(){
		(this.$refs.taRulesGlobalForm as any).validate();
	}

	async getData() {
		await taRulesGlobalApi.GetRulesGlobal().then((res) => {
			// console.log(res)
			if(res.status == 200 && res.data){
				this.ruleForm = res.data;
				this.ruleForm.LockAttendanceTime = this.ruleForm.LockAttendanceTime > 0 ? this.ruleForm.LockAttendanceTime : null;
				this.ruleForm.MaximumAnnualLeaveRegisterByMonth = this.ruleForm.MaximumAnnualLeaveRegisterByMonth || 0;
				this.ruleForm.OverTimeNormalDay = this.ruleForm.OverTimeNormalDay || 0;
				this.ruleForm.NightOverTimeNormalDay = this.ruleForm.NightOverTimeNormalDay || 0;
				this.ruleForm.OverTimeLeaveDay = this.ruleForm.OverTimeLeaveDay || 0;
				this.ruleForm.NightOverTimeLeaveDay = this.ruleForm.NightOverTimeLeaveDay || 0;
				this.ruleForm.OverTimeHoliday = this.ruleForm.OverTimeHoliday || 0;
				this.ruleForm.NightOverTimeHoliday = this.ruleForm.NightOverTimeHoliday || 0;
				if(this.ruleForm.NightShiftStartTime){
					this.ruleForm.NightShiftStartTime = new Date(this.ruleForm.NightShiftStartTime);
				}
				if(this.ruleForm.NightShiftEndTime){
					this.ruleForm.NightShiftEndTime = new Date(this.ruleForm.NightShiftEndTime);
				}
				this.ruleForm.IsAutoCalculateAttendance = this.ruleForm.IsAutoCalculateAttendance;
				if (this.ruleForm.TimePos) {
					this.listTimePos = this.ruleForm.TimePos.split(";");
				  }
			}
		});
	}

	reset() {
		const obj: TA_RulesGlobalDTO = {
			MaximumAnnualLeaveRegisterByMonth: 0,
			OverTimeNormalDay: 0,
			NightOverTimeNormalDay: 0,
			OverTimeLeaveDay: 0,
			NightOverTimeLeaveDay: 0,
			OverTimeHoliday: 0,
			NightOverTimeHoliday: 0,
			};
		this.ruleForm = obj;
	}

	async Submit() {
		(this.$refs.taRulesGlobalForm as any).validate(async (valid) => {
			// console.log('ruleForm', this.ruleForm);
			if (!valid) return;
			else {
				if(this.ruleForm.NightShiftStartTime){
					this.ruleForm.NightShiftStartTimeString = moment(this.ruleForm.NightShiftStartTime).format("YYYY-MM-DD HH:mm:ss");
				}
				if(this.ruleForm.NightShiftEndTime){
					this.ruleForm.NightShiftEndTimeString = moment(this.ruleForm.NightShiftEndTime).format("YYYY-MM-DD HH:mm:ss");
				}
				if(this.listTimePos){
					this.ruleForm.TimePos = this.listTimePos.join(";");
				}
				await taRulesGlobalApi.UpdateRulesGlobal(this.ruleForm).then((res) => {
					// console.log(res);
					if(res.status && res.status == 200 && res.data){
						this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
					}else{
						this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
					}
				}).finally(() => {
					this.getData();
				})
			}
		});
	}
}
