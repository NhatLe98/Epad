import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_DoorDTO, doorApi } from '@/$api/ac-door-api';
import { areaApi } from '@/$api/ac-area-api';
import { doorDeviceApi } from '@/$api/ac-door-device-api';
import { timezoneApi } from '@/$api/ac-timezone-api';
import { commandApi,CommandRequest } from '@/$api/command-api';

@Component({
	name: 'door-setting',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class DoorSettingComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: AC_DoorDTO = {
		Name: '',
		Description: '',
		AreaIndex: null,
		SerialNumberLst: [],
		AreaIndexes: [],
		DoorIndexes: [],
		Timezone: null
	};

	rules: any = {};
	listAllArea = [];
	listDoor = [];
	listAllDoor = [];
	listTimezone = [];

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

	async beforeMount() {
		this.initColumns();
		this.initRule();
		await this.LoadArea();
		await this.getAllDoor();
		await this.LoadAllTimeZone();
	}

	async LoadArea() {
		return await areaApi.GetAllArea().then((res) => {
			const { data } = res as any;
			this.listAllArea = data;
		})
	}

	async getAllDoor() {
		doorApi.GetAllDoor().then(res => {
			const { data } = res as any;
			this.listDoor = data;
			this.listAllDoor = data;
		})
	}

	changeFormAreaIndexes(){
		// console.log(this.listDoor)
		// console.log(this.ruleForm);
		if(this.ruleForm.AreaIndexes && this.ruleForm.AreaIndexes.length > 0){
			this.listDoor = this.listAllDoor.filter(x => 
				this.ruleForm.AreaIndexes.includes((x as any).areaId));
		}else{
			this.listDoor = this.listAllDoor;
		}
	}

	initRule() {
		this.rules = {
			DoorIndexes: [
				{
					required: true,
					trigger: 'change',
					message: this.$t("PleaseInputDoor"),
					validator: (rule, value: string, callback) => {
						if (!this.ruleForm.DoorIndexes || this.ruleForm.DoorIndexes.length == 0) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
			Timezone: [
				{
					required: true,
					trigger: 'change',
					message: this.$t("PleaseInputTimeZone"),
					validator: (rule, value: string, callback) => {
						if (!this.ruleForm.Timezone || this.ruleForm.Timezone == 0) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
		};
	}

	async LoadAllTimeZone() {
		return await timezoneApi.GetAllTimezone().then(res => {
			const { data } = res as any;
			this.listTimezone = data;
		})
	}

	mounted() {
		
	}

	initColumns(){
		this.columns = [
			{
				prop: 'Name',
				label: 'DoorName',
				minWidth: 120,
				display: true
			},
			{
				prop: 'TimezoneName',
				label: 'TimezoneString',
				minWidth: 100,
				display: true
			},
			{
				prop: 'DoorSettingDescription',
				label: 'Description',
				minWidth: 100,
				display: true
			},
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await doorApi.GetDoorAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data.filter(x => x.Timezone && x.Timezone > 0),
				total: data.total,
			};
		});
	}

	reset() {
		const obj: AC_DoorDTO = {
			Name: '',
			Description: '',
			AreaIndex: null,
			SerialNumberLst: [],
			AreaIndexes: [],
			DoorIndexes: [],
			Timezone: null
		};
		this.ruleForm = obj;
	}

	async Insert() {
		this.showDialog = true;
		this.isEdit = false;

		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await doorApi.UpdateDoorSetting(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
					
					
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.InsertToMachine();
							this.$saveSuccess();
						}
						this.reset();
					});
				} else {
					return await doorApi
						.AddDoorSetting(this.ruleForm)
						.then((res: any) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								if (res && (!res.data || (res.data && !(res.data as any).Item1))) {
									this.InsertToMachine();
									this.$saveSuccess();
								} else if ((res.data as any).Item1 && (res.data as any).Item1 == "DoorSettingIsExisted") {
									var message = this.$t("DoorSettingIsExisted").toString();
									if ((res.data as any).Item2 && (res.data as any).Item2.length > 0) {
										message += "<br/>";
										(res.data as any).Item2.forEach(element => {
											message += "- " + element.Name + "<br/>";
										});
									}
									const fullMessage = `<p class="notify-content">${message}</p>`
									// this.$notify({
									// 	type: 'warning',
									// 	title: this.$t('Warning').toString(),
									// 	dangerouslyUseHTMLString: true,
									// 	message: message,
									// 	customClass: 'notify-content',
									// 	duration: 8000
									// });
									this.$alert(fullMessage, this.$t('Warning').toString(), {
										confirmButtonText: 'OK',
										dangerouslyUseHTMLString: true
									});
								}
							}
							this.reset();
						})
						.catch(() => { });
				}
			}
		});
	}

	async Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
			this.ruleForm.AreaIndexes = [this.ruleForm.AreaIndex];
			this.ruleForm.DoorIndexes = [this.ruleForm.Index];
			this.ruleForm.Timezone = this.ruleForm.Timezone == 0 ? null : this.ruleForm.Timezone;
			console.log(this.ruleForm)
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	async InsertToMachine() {
		let message = '';
		const self = this;
		this.commandRequest.Action = "Set doorsetting";
		this.commandRequest.ListUser = [];
		this.commandRequest.TimezoneStr = this.ruleForm.Timezone.toString();
		this.commandRequest.DoorLst = this.ruleForm.DoorIndexes.map(x => x.toString());


		try {
			await commandApi.SetDoorSetting(this.commandRequest).then((res) => {
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

	async DeleteToMachine(lst) {
		let message = '';
		const self = this;
		this.commandRequest.Action = "Set doorsetting";
		this.commandRequest.ListUser = [];
		this.commandRequest.TimezoneStr = '0';
		this.commandRequest.DoorLst = lst;


		try {
			await commandApi.SetDoorSetting(this.commandRequest).then((res) => {
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


	async Delete() {
		const obj = JSON.parse(JSON.stringify(this.rowsObj));
		const listIndex: Array<number> = this.rowsObj.map((item: any) => {
            return item.Index;
        });
		// console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await doorApi
					.DeleteDoorSetting(obj)
					.then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						console.log(res);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.DeleteToMachine(listIndex);

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
		// var ref = <ElForm>this.$refs.ruleForm;
		// ref.resetFields();
		this.reset();
		this.changeFormAreaIndexes();
		this.showDialog = false;
	}
}
