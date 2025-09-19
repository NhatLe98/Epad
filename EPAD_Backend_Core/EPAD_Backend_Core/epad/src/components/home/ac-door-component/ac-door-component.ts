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
import { commandApi, CommandRequest } from '@/$api/command-api';

@Component({
	name: 'door',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class DoorComponent extends Mixins(ComponentBase) {
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
		SerialNumberLst: []
	};
	lstSerialNumber: string[] = [];
	rules: any = {};
	listAllArea: [];
	listDevices: [];
	listTimezone: [];
	listRange = [
		{ label: 'Mốc thời gian 1', value: 1 },
		{ label: 'Mốc thời gian 2', value: 2 },
		{ label: 'Mốc thời gian 3', value: 3 }
	]
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
		await this.LoadAllTimeZone();
	}

	async LoadArea() {
		return await areaApi.GetAllArea().then((res) => {
			const { data } = res as any;
			this.listAllArea = data;
		})
	}

	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'blur',
				},
			],
			DoorOpenTimezoneUID: [
				{
					message: this.$t("PleaseInputDoorOpenTimezone"),
					validator: (rule, value: string, callback) => {
						if (this.ruleForm.Timezone != 0 && this.ruleForm.Timezone != undefined) {
							if (!this.ruleForm.DoorOpenTimezoneUID || this.ruleForm.DoorOpenTimezoneUID == 0) {
								callback(new Error());
							} else {
								callback();
							}
						} else {
							callback();
						}
					},
					trigger: "change",
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

	initColumns() {
		this.columns = [
			{
				prop: 'Name',
				label: 'DoorName',
				minWidth: 120,
				display: true
			},
			{
				prop: 'AreaName',
				label: 'Area',
				minWidth: 120,
				display: true
			},
			{
				prop: 'DeviceListName',
				label: 'Device',
				minWidth: 180,
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
				data: data.data,
				total: data.total,
			};
		});
	}

	reset() {
		const obj: AC_DoorDTO = {
			Name: '',
			Description: '',
			AreaIndex: null,
			SerialNumberLst: []
		};
		this.ruleForm = obj;
	}

	async Insert() {
		this.listDevices = [];
		this.showDialog = true;
		this.isEdit = false;

		await this.GetAllDeviceById(0);

		this.reset();
	}


	async GetAllDeviceById(id) {
		doorDeviceApi.GetDeviceInOutDoor(id).then(res => {
			const { data } = res as any;
			this.listDevices = data;
			if (this.ruleForm.Index !== 0) {
				var lst = data.filter(x => x.InDoor);
				this.ruleForm.SerialNumberLst = lst.map(x => x.SerialNumber);
				this.lstSerialNumber = this.ruleForm.SerialNumberLst;
			} else {
				this.ruleForm.SerialNumberLst = [];
				this.lstSerialNumber = [];
			}
			this.$forceUpdate();
		})
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.ruleForm.SerialNumberLst.length > 2) {
					this.$alertSaveError(null, null, null, this.$t("PleaseChooseMaximumTwoDevices").toString())
					return;
				}
				if (this.isEdit == true) {
					 return await doorApi.UpdateDoor(this.ruleForm).then(async (res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						console.log(this.lstSerialNumber);
						console.log(this.ruleForm.SerialNumberLst);
						const arr = this.ruleForm.SerialNumberLst.filter(x => !this.lstSerialNumber.includes(x));
						console.log(arr);
						if (arr && arr.length > 0) {
							let message = '';
							const self = this;
							try {
								await commandApi.UploadTimeZone({
									Action: "Upload timezone",
									ListUser: [],
									ListSerial: arr,
									AreaLst: [],
									DoorLst: [],
								}).then((res) => {
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
							} catch (error) {
								this.$alertSaveError(null, error);
							}

							try {
								await commandApi.UploadACUsersWhenUpdateDoorBySerial({
									Action: "Upload user to machine",
									ListUser: [],
									ListSerial: arr,
									AreaLst: [],
									DoorLst: [this.ruleForm.Index.toString()]
								}).then((res) => {
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
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							// this.InsertToMachine();
							this.$saveSuccess();
						}
						this.reset();
					});
				} else {
					return await doorApi
						.AddDoor(this.ruleForm)
						.then((res: any) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;

							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.ruleForm.Index = res.data.Index;
								// this.InsertToMachine();
								this.$saveSuccess();
							}
							this.reset();
						})
						.catch(() => { });
				}
			}
		});
	}

	async Edit() {
		this.listDevices = [];
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = Misc.cloneData(obj[0]);
			this.ruleForm.DoorOpenTimezoneUID = this.ruleForm.DoorOpenTimezoneUID == 0 ? null : this.ruleForm.DoorOpenTimezoneUID;
			
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
		await this.GetAllDeviceById(obj[0].Index);
	}

	async InsertToMachine() {
		let message = '';
		const self = this;
		this.commandRequest.Action = "Set doorsetting";
		this.commandRequest.ListUser = [];
		this.commandRequest.TimezoneStr = this.ruleForm.Timezone.toString();
		this.commandRequest.DoorLst = [this.ruleForm.Index.toString()];


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
		const obj: AC_DoorDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await doorApi
					.DeleteDoor(obj)
					.then((res) => {
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
