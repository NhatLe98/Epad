import { Component, Mixins } from 'vue-property-decorator';
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

@Component({
	name: 'timezone',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class TimezoneComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	timePosOption: Array<string> = [];
	ruleForm: any = {};
	rules: any = {};
	showDialogAuthenMode = false;
	isUsingArea = true;
	listAllDoor = [];
	listAllArea = [];
	selectArea: any = '';
	selectDoor: any = '';
	IntegrateTimezone = false;

	// Function ALL TAB ==============================
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

	beforeMount() {
		this.initColumns();
		this.initRule();
		this.initTimePosData();
		this.LoadDoor();
		this.LoadArea();
	}


	initTimePosData() {
		for (let i = 0; i < 24; i++) {
			this.timePosOption.push(`${i.toString().padStart(2, '0')}:00`);
			this.timePosOption.push(`${i.toString().padStart(2, '0')}:30`);
		}
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
		};
	}

	mounted() {
		
	}

	initColumns(){
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

	displayPopupInsert() {
		this.showDialog = false;
	}

	async LoadArea() {
		return await areaApi.GetAllArea().then((res) => {
			const { data } = res as any;
			this.listAllArea = data;
		})
	}


	async LoadDoor() {
		return await doorApi.GetAllDoor().then((res) => {
			const { data } = res as any;
			this.listAllDoor = data;
		})
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await timezoneApi.GetTimezoneAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	reset() {
		const obj: AC_AreaDTO = {};
		this.ruleForm = obj;
		this.IntegrateTimezone = false;
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async InsertToMachine() {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Upload timezone";

		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = this.selectArea;
		this.commandRequest.DoorLst = this.selectDoor;
		this.commandRequest.IsUsingArea = this.isUsingArea;
		this.showDialogAuthenMode = false;
		try {
			await commandApi.UploadTimeZone(this.commandRequest).then((res) => {
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

	async DeleteToMachine(id) {
		let message = '';
		const self = this;

		this.commandRequest.Action = "Delete timezone";

		this.commandRequest.ListUser = [];
		this.commandRequest.AreaLst = this.selectArea;
		this.commandRequest.DoorLst = this.selectDoor;
		this.commandRequest.IsUsingArea = this.isUsingArea;
		this.commandRequest.TimeZone = [id];
		this.showDialogAuthenMode = false;
		try {
			await commandApi.DeleteTimezoneById(this.commandRequest).then((res) => {
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

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await timezoneApi.UpdateTimezone(this.ruleForm).then((res) => {
						if (res.data == "") {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
								this.selectArea = [];
								this.selectDoor = [];
								this.isUsingArea = false;
								this.InsertToMachine();
							}
						} else {
							const ress: any = res.data;
							let strs = '';
							ress.responseData.forEach(element => {
								let err = '';
								element.DayOfWeek.forEach((val, key, arr) => {
									if (Object.is(arr.length - 1, key)) {
										// execute last item logic
										err += this.$t(val) + ' ';
									} else {
										err += this.$t(val) + ',';
									}

								});
								err += this.$t(element.Name) + "<br\>";
								strs += err;
							});
							// const fullMessage = this.$t(ress.MessageDetail, {
							// 	TimeOfWeek: this.$t(ress.responseData)
							// });
							this.$alert(strs.toString(), this.$t('Warning').toString(), {
								confirmButtonText: 'OK',
								dangerouslyUseHTMLString: true
							});
						}
					});
				} else {
					return await timezoneApi
						.AddTimezone(this.ruleForm)
						.then((res) => {
							if (res.data == "") {
								(this.$refs.table as any).getTableData(this.page, null, null);
								this.showDialog = false;

								if (!isNullOrUndefined(res.status) && res.status === 200) {
									this.$saveSuccess();
									if (this.IntegrateTimezone) {
										this.selectArea = [];
										this.selectDoor = [];
										this.isUsingArea = false;
										this.InsertToMachine();
									}
								}
								this.reset();
							}
							else {
								const ress: any = res.data;
								let strs = '';
								ress.responseData.forEach(element => {
									let err = '';
									element.DayOfWeek.forEach((val, key, arr) => {
										if (Object.is(arr.length - 1, key)) {
											// execute last item logic
											err += this.$t(val) + ' ';
										} else {
											err += this.$t(val) + ',';
										}

									});
									err += this.$t(element.Name) + "<br\>";
									strs += err;
								});
								// const fullMessage = this.$t(ress.MessageDetail, {
								// 	TimeOfWeek: this.$t(ress.responseData)
								// });
								this.$alert(strs.toString(), this.$t('Warning').toString(), {
									confirmButtonText: 'OK',
									dangerouslyUseHTMLString: true
								});
							}

						})
						.catch((err) => {
							console.log(err);
						});
				}
			}
		});
	}

	async cancelDialog() {
		this.showDialogAuthenMode = false;
	}

	async copyAllProperties() {
		const keyArr = ["MonStart1", "MonEnd1", "MonStart2", "MonEnd2", "MonStart3", "MonEnd3"];
		const dayOfWeekArr = ["Sun", "Tues", "Wed", "Thurs", "Fri", "Sat"];

		for (var key in this.ruleForm) {
			if (Object.prototype.hasOwnProperty.call(this.ruleForm, key) && keyArr.includes(key)) {
				var val = this.ruleForm[key];
				var index = keyArr.indexOf(key);
				if (keyArr[index].includes("Start")) {
					var sliceKey = keyArr[index].slice(3, 9);
					for (var keyDay in dayOfWeekArr) {
						this.ruleForm[dayOfWeekArr[keyDay] + sliceKey] = val;
					}
				} else {
					var sliceKey = keyArr[index].slice(3, 7);
					for (var keyDay in dayOfWeekArr) {
						this.ruleForm[dayOfWeekArr[keyDay] + sliceKey] = val;
					}
				}
			}
		}
		this.ruleForm = { ...this.ruleForm };
	}

	Integrate() {
		this.selectArea = [];
		this.selectDoor = [];
		this.isUsingArea = false;
		this.InsertToMachine();
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			timezoneApi.GetTimezoneByID(obj[0].UID)
				.then((res: any) => {
					if (res.status == 200) {
						this.ruleForm = res.data;
						const uid = res.data.UIDIndex.split(',');
						if (uid && uid.length == 3) {
							this.ruleForm.UID1 = uid[0];
							this.ruleForm.UID2 = uid[1];
							this.ruleForm.UID3 = uid[2];
						}
					}
				});

		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}


	async Delete() {
		const obj: AC_AreaDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await timezoneApi
					.DeleteTimezone(obj)
					.then((res:any) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.selectArea = [];
							this.selectDoor = [];
							this.isUsingArea = false;
							 this.DeleteToMachine(res.data.UIDIndex);


							this.$deleteSuccess();
						}
					})
					.catch(() => { });
			});
		}
	}

	focus(x) {
		console.log(x);
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
}
