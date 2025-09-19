import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import { doorApi } from '@/$api/ac-door-api';
import { AC_HolidayDTO, holidayApi } from '@/$api/ac-holiday-api';
import { timezoneApi } from '@/$api/ac-timezone-api';
import { commandApi, CommandRequest } from "@/$api/command-api";

@Component({
	name: 'holiday',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class HolidayComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: AC_HolidayDTO = {
		HolidayName: '',
		StartDate: new Date(),
		EndDate: new Date()
	};
	commandRequest: CommandRequest = {
		Action: "",
		ListSerial: [],
		ListUser: [],
		FromTime: null,
		ToTime: null,
		Privilege: 0,
		AuthenMode: [],
		IsOverwriteData: false,
		EmployeeType: 0,
		IsUsingTimeZone: false,
		TimeZone: [],
		Group: 0
	};
	rules: any = {};
	allDoorLst = [];
	allTimezoneLst = [];
	listRange = [
		{ label: 'Loại ngày nghỉ 1', value: 1 },
		{ label: 'Loại ngày nghỉ 2', value: 2 },
		{ label: 'Loại ngày nghỉ 3', value: 3 }
	]

	async beforeMount() {
		this.initColumns();
		this.initRule();
		await this.getAllDoor();
		await this.getAllTimezone();
	}
	initRule() {
		this.rules = {
			HolidayName: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'blur',
				},
			],
			TimezoneRange: [
				{
					required: true,
					message: this.$t('PleaseInputTimezoneRange'),
					trigger: 'blur',
				},
			],
			StartDate: [
				{
					required: true,
					message: this.$t('PleaseInputWorkingFromDate'),
					trigger: 'change',
				},
				{
                    message: this.$t('FromDateCannotSmallerThanCurrentDate'),
                    validator: (rule, value: string, callback) => {
                        const now = moment().format('YYYY-MM-DD');
                        const dCheck = moment(this.ruleForm.StartDate).format('YYYY-MM-DD');

                        if ( dCheck < now ) {
                            callback(new Error());
                        } else {
                            callback();
                        }
                    }
                }
			],
			EndDate: [
				{
					required: true,
					message: this.$t('PleaseInputWorkingToDate'),
					trigger: 'blur',
				},
			],
			DoorIndexes: [
				{
					required: true,
					message: this.$t('PleaseInputDoor'),
					trigger: 'blur',
				},
			],
			HolidayType:[
				{
					required: true,
					message: this.$t('PleaseInputHolidayType'),
					trigger: 'blur',
				},
			],
			TimeZone:[
				{
					required: true,
					message: this.$t('PleaseInputTimeZone'),
					trigger: 'blur',
				},
			]
		
		};
	}

	mounted() {

	}

	initColumns(){
		this.columns = [
			{
				prop: 'HolidayName',
				label: 'Name',
				minWidth: 100,
				display: true
			},
			{
				label: 'WorkingFromDate',
				prop: 'StartDateString',
				width: 150,
				display: true,
			},
			{
				label: 'WorkingToDate',
				prop: 'EndDateString',
				width: 150,
				display: true,
			},		
			{
				prop: 'DoorName',
				label: 'DoorName',
				minWidth: 100,
				display: true
			},
			{
				prop: 'TimezoneName',
				label: 'Timezone',
				minWidth: 100,
				display: true
			},
			{
				prop: 'HolidayTypeName',
				label: 'HolidayTypeName',
				minWidth: 100,
				display: true
			},
			{
				prop: 'LoopName',
				label: 'LoopName',
				minWidth: 100,
				display: true
			}
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getAllDoor() {
		doorApi.GetAllDoor().then(res => {
			const { data } = res as any;
			this.allDoorLst = data;
		})
	}

	async getAllTimezone() {
		timezoneApi.GetAllTimezone().then(res => {
			const { data } = res as any;
			this.allTimezoneLst = data;
		})
	}

	async Integrate() {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Upload holiday";
		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = [];
		this.commandRequest.DoorLst = [];
		this.commandRequest.IsUsingArea = false;
		try {
			await commandApi.UploadAccHoliday(this.commandRequest).then((res) => {

				if (!isNullOrUndefined(res.status) && res.status === 200) {
					this.$notify({
						type: 'success',
						title: 'Thông báo từ thiết bị',
						dangerouslyUseHTMLString: true,
						message: self.$t("SendRequestSuccess").toString(),
						customClass: 'notify-content',
						duration: 8000
					});
				}
			}).catch((err) => {
				this.$alertSaveError(null, err);
			});

			return true;
		} catch (error) {
			this.$alertSaveError(null, error);
			return false;
		}
	}

	async IntegrateWithParam(doorIndex) {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Upload holiday";

		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = [];
		this.commandRequest.DoorLst = doorIndex;
		this.commandRequest.IsUsingArea = false;
		try {
			await commandApi.UploadAccHoliday(this.commandRequest).then((res) => {

				if (!isNullOrUndefined(res.status) && res.status === 200) {
					this.$notify({
						type: 'success',
						title: 'Thông báo từ thiết bị',
						dangerouslyUseHTMLString: true,
						message: self.$t("SendRequestSuccess").toString(),
						customClass: 'notify-content',
						duration: 8000
					});
				}
			}).catch((err) => {
				this.$alertSaveError(null, err);
			});
			return true;
		} catch (error) {
			this.$alertSaveError(null, error);
			return false;
		}
	}

	async DeleteAllHoliday(doorIndex) {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Delete all holiday";

		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = [];
		this.commandRequest.DoorLst = doorIndex;
		this.commandRequest.IsUsingArea = false;
		try {
			await commandApi.DeleteAllHoliday(this.commandRequest).then((res) => {

				if (!isNullOrUndefined(res.status) && res.status === 200) {
					this.$notify({
						type: 'success',
						title: 'Thông báo từ thiết bị',
						dangerouslyUseHTMLString: true,
						message: self.$t("SendRequestSuccess").toString(),
						customClass: 'notify-content',
						duration: 8000
					});
				}
			}).catch((err) => {
				this.$alertSaveError(null, err);
			});

			return true;
		} catch (error) {
			this.$alertSaveError(null, error);
			return false;
		}
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await holidayApi.GetHolidayAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	reset() {
		const obj  = {
			HolidayName: '',
			StartDate: new Date(),
			EndDate: new Date()
		};
		this.ruleForm = obj;
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				let startDate = moment(this.ruleForm.StartDate);
				let endDate = moment(this.ruleForm.EndDate);
				startDate.set({hour:0,minute:0,second:0,millisecond:0});
				endDate.set({hour:0,minute:0,second:0,millisecond:0});
				if ( moment(startDate) > moment(endDate)) {
					this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
					this.isLoading = false;
					return;
				}
				this.ruleForm.StartDateString = moment(this.ruleForm.StartDate).format("YYYY-MM-DD HH:mm:ss");
				this.ruleForm.EndDateString = moment(this.ruleForm.EndDate).format("YYYY-MM-DD HH:mm:ss");
				if (this.isEdit == true) {
					return await holidayApi.UpdateHoliday(this.ruleForm).then(async (res) => {
						var para = this.ruleForm.DoorIndexes;
						await this.IntegrateWithParam(para);
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await holidayApi
						.AddHoliday(this.ruleForm)
						.then(async (res) => {
							var para = this.ruleForm.DoorIndexes;
							await this.IntegrateWithParam(para);
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						})
						.catch(() => { });
				}
			}
		});
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}


	async Delete() {
		const obj: AC_HolidayDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await holidayApi
					.DeleteHoliday(obj)
					.then(async (res) => {
						const para = res.data;
						if(para){
							await this.DeleteAllHoliday(para);
							await this.IntegrateWithParam(para);
						}
					
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
					})
					.catch(() => { });
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
}
