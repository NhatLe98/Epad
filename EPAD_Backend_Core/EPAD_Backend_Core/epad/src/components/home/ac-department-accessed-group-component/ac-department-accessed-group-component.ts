import { Component, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import { ac_DepartmentAccessedGroupApi } from '@/$api/ac-department-accessed-group-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from '@/$api/department-api';
import { groupApi } from '@/$api/ac-group-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { commandApi, CommandRequest } from '@/$api/command-api';
import { deviceApi } from '@/$api/device-api';
import * as XLSX from 'xlsx';

@Component({
	name: 'DepartmentAccessedGroup',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent },
})
export default class DepartmentAccessedGroupComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	isDeleteAccessToMachine = false;
	syncUser = false;
	columns = [];
	rowsObj = [];
	filter = '';
	importErrorMessage = '';
	SelectedDepartment = [];
	EmployeeATIDs = [];
	isEdit = false;
	listAuthenMode: any = [];
	listGroups: any[];
	filterGroups: any[] = [];
	listAllEmployee = [];
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
	DepartmentIDss = [];
	showDialogDeleteUser = false;
	ruleForm: any = {
		EmployeeATIDs: []
	};
	fileName = '';
	dataProcessedExcel = [];
	dataAddExcel = [];
	isAddFromExcel = false;
	showDialogExcel = false;
	showDialogImportError = false;
	isDeleteFromExcel = false;
	listExcelFunction = ['AddExcel'];
	rules: any = {};
	oldGroup: '';
	selectAuthenMode: any[] = [];
	commandRequestTz: CommandRequest = {
		Action: "",
		ListSerial: [],
		ListUser: [],
		ListDepartment: [],
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

	commandRequest: CommandRequest = {
		Action: "",
		ListSerial: [],
		ListUser: [],
		ListDepartment: [],
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
	tree = {
		employeeList: [],
		clearable: true,
		defaultExpandAll: false,
		multiple: true,
		placeholder: "",
		disabled: false,
		checkStrictly: false,
		popoverWidth: 400,
		treeData: [],
		treeProps: {
			value: 'ID',
			children: 'ListChildrent',
			label: 'Name',
		},
	}



	beforeMount() {
		this.initColumns();
		this.initRule();
		this.LoadDepartmentTree();
		this.LoadAllGroups();
		this.getAuthenMode();

	}

	processFile(e) {
		console.log(e.target.files[0]['type']);
		if ((<HTMLInputElement>e.target).files.length > 0) {
			var file = (<HTMLInputElement>e.target).files[0];
			this.fileName = file.name;
			if (!isNullOrUndefined(file)) {
				var fileReader = new FileReader();
				var arrData = [];
				fileReader.onload = function (event) {
					var data = event.target.result;
					var workbook = XLSX.read(data, {
						type: 'binary',
					});

					workbook.SheetNames.forEach((sheet) => {
						var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
						// var arr = Array.from(rowObject)
						// arrData.push(arr)
						arrData.push(Array.from(rowObject));
					});
				};
				this.dataAddExcel = arrData;
				fileReader.readAsBinaryString(file);
			}
		}
	}

	async doDelete() {
		const listIndex: Array<number> = this.rowsObj.map((item: any) => {
			return item.Index;
		});
		const action = "Delete user on machine";

		if (this.isDeleteAccessToMachine) {
			await commandApi.DeleteACDepartmentByDoor({
				ListSerial: [],
				ListUser: [],
				ListDepartment: this.ruleForm.DepartmentIDs,
				Action: action,
				AuthenMode: [],
				AreaLst: [],
				DoorLst: [],
				EmployeeAccessedGroup: listIndex,
				IsUsingArea: false
			}).then(async (res) => {
					await ac_DepartmentAccessedGroupApi
						.DeleteDepartmentAccessedGroup(listIndex).then(async (res) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$deleteSuccess();
							}
						})
						.finally(() => { this.showDialogDeleteUser = false; })
				})

		}else{
			await ac_DepartmentAccessedGroupApi
			.DeleteDepartmentAccessedGroup(listIndex).then(async (res) => {
				(this.$refs.table as any).getTableData(this.page, null, null);
				if (!isNullOrUndefined(res.status) && res.status === 200) {
					this.$deleteSuccess();
				}
			})
			.finally(() => { this.showDialogDeleteUser = false; })
		}

	
	}


	UploadDataFromExcel() {
		this.importErrorMessage = "";
		var arrData = [];
		for (let i = 0; i < this.dataAddExcel[0].length; i++) {
			let a = Object.assign({});
			if (this.dataAddExcel[0][i].hasOwnProperty('MCC (*)')) {
				a.EmployeeATID = this.dataAddExcel[0][i]['MCC (*)'] + '';
			} else {
				this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
				return;
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
				a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
			} else {
				a.EmployeeCode = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
				a.EmployeeName = this.dataAddExcel[0][i]['Họ tên'] + '';
			} else {
				a.EmployeeName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Nhóm truy cập (*)')) {
				a.Group = this.dataAddExcel[0][i]['Nhóm truy cập (*)'] + '';
			} else {
				this.$alertSaveError(null, null, null, this.$t('GroupMayNotBeBlank').toString()).toString();
				return;
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Đồng bộ thông tin nhân viên lên thiết bị') && (this.dataAddExcel[0][i]['Đồng bộ thông tin nhân viên lên thiết bị'] == 'x' || this.dataAddExcel[0][i]['Đồng bộ thông tin nhân viên lên thiết bị'] == 'X')) {
				a.IsIntegrateToMachine = true;
			} else {
				a.IsIntegrateToMachine = false;
			}
			arrData.push(a);
		}

		commandApi.UploadACAccessedEmployeeFromExcel(arrData).then((res) => {
			if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
				(<HTMLInputElement>document.getElementById('fileUpload')).value = '';
			}
			if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
				(<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
			}
			(this.$refs.table as any).getTableData(this.page, null, null);
			this.showDialogExcel = false;
			this.fileName = '';
			this.dataAddExcel = [];
			this.isAddFromExcel = false;
			if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
				this.$saveSuccess();
			} else {
				this.importErrorMessage = this.$t('ImportACAccessedEmployeeErrorMessage') + res.data.toString() + " " + this.$t('Employee');
				this.showOrHideImportError(true);
			}
		});
	}

	showOrHideImportError(obj) {
		this.showDialogImportError = obj;
	}


	beforeUpdate() {

		if (this.selectAuthenMode.indexOf("SelectAll") !== -1) {
			this.selectAuthenMode = [...this.listAuthenMode].map(
				(item) => item.value
			);
		}
		if (this.selectAuthenMode.indexOf("DeselectAll") !== -1) {
			this.selectAuthenMode = [];
		}

	}

	get allAuthenModes() {
		if (
			this.selectAuthenMode.length > 0
		) {
			return "DeselectAll";
		} else {
			return "SelectAll";
		}

	}

	initRule() {
		this.rules = {
			GroupIndex: [
				{
					required: true,
					message: this.$t('PleaseInputGroupIndex'),
					trigger: 'blur',
				},
			],
			DepartmentIDs: [
				{
					required: true,
					message: this.$t('PleaseInputDepartment'),
					trigger: 'change',
				},
				{
                    trigger: 'change',
                    message: this.$t('PleaseInputDepartment'),
                    validator: (rule, value: string, callback) => {
                        if (!value || (value && value.length < 1)) {
                            // console.log("")
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

	initColumns() {
		this.columns = [
			{
				prop: 'DepartmentName',
				label: 'Department',
				minWidth: 300,
				display: true,
				dataType: 'translate'
			},
			{
				prop: 'GroupName',
				label: 'Group',
				minWidth: 300,
				display: true
			}
		];
	}

	selectAllGroups(value) {
		this.filterGroups = value;
	}


	displayPopupInsert() {
		this.showDialog = false;
	}

	handleCommand(command) {
		if (command === 'AddExcel') {
			this.AddOrDeleteFromExcel('add');
		}
		else if (command === 'ExportExcel') {
			// this.ExportToExcel();
		}
		else if (command === 'DeleteExcel') {
			this.AddOrDeleteFromExcel('delete');
		}
	}

	async AddOrDeleteFromExcel(x) {
		if (x === 'add') {
			this.isAddFromExcel = true;
			this.showDialogExcel = true;
			this.fileName = '';
		}
		else if (x === 'close') {
			if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
				(<HTMLInputElement>document.getElementById('fileUpload')).value = '';
			}

			this.dataAddExcel = [];
			this.isAddFromExcel = false;
			this.isDeleteFromExcel = false;
			this.showDialogExcel = false;
			this.fileName = '';
		}
	}


	LoadDepartmentTree() {
		departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.tree.treeData = res.data;
			}
		});

	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await ac_DepartmentAccessedGroupApi.GetDepartmentAccessedGroupAtPage(page, this.filter, pageSize, this.SelectedDepartment, this.filterGroups).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	LoadAllGroups() {
		groupApi.GetAllGroup().then((res: any) => {
			if (res.status == 200) {
				this.listGroups = res.data;
			}
		});
	}


	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.Reset();
	}

	async getAuthenMode() {
		return await deviceApi.GetDeviceAuthenMode().then((res) => {
			var arr = [...JSON.parse(JSON.stringify(res.data))];
			var arr_1 = [];
			for (let i = 0; i < arr.length; i++) {
				arr_1.push({ value: arr[i].value, label: arr[i].label });
			}
			this.listAuthenMode = arr_1;
		});
	}


	async Submit() {
		this.commandRequestTz = {
			Action: "",
			ListSerial: [],
			ListUser: [],
			ListDepartment: [],
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

		this.commandRequest = {
			Action: "",
			ListSerial: [],
			ListUser: [],
			ListDepartment: [],
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
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (valid == false) {
				return;
			}
			this.ruleForm.EmployeeATIDs = this.EmployeeATIDs;
			const submitData = Misc.cloneData(this.ruleForm);
			const self = this;
			if (this.isEdit == false) {
				await ac_DepartmentAccessedGroupApi.AddDepartmentAccessedGroup(submitData).then(async (res: any) => {
					if (res.status === 200 && res.data) {
						const msg = res.data;
						this.$alert(
							this.$t("MSG_DataDepartmentAccessGroupExistedFromTo", { data: msg }).toString(),
							this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
						);
					}
					else if (res.status === 200) {
						var door = this.listGroups.find(x => x.value == this.ruleForm.GroupIndex.toString());
						this.commandRequestTz.ListUser = [];
						this.commandRequestTz.AreaLst = [];
						this.commandRequestTz.DoorLst = [door.doorIndex];
						this.commandRequestTz.IsUsingArea = false;
						this.commandRequestTz.TimeZone = [];

						this.commandRequest.Action = "Upload user to machine";

						this.commandRequest.ListUser = this.ruleForm.EmployeeATIDs;
						this.commandRequest.ListDepartment = this.ruleForm.DepartmentIDs;
						this.commandRequest.ListSerial = [];
						this.commandRequest.AreaLst = [];
						this.commandRequest.DoorLst = [door.doorIndex];
						this.commandRequest.IsUsingArea = false;
						this.commandRequest.TimeZone = [door.timezone];
						this.commandRequest.AuthenMode = [...this.selectAuthenMode];
						this.commandRequest.EmployeeType = 1;
						await commandApi.UploadTimeZone(this.commandRequestTz).then(async (res) => {
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$notify({
									type: 'success',
									title: 'Thông báo từ thiết bị',
									dangerouslyUseHTMLString: true,
									message: self.$t("SendRequestSuccess").toString(),
									customClass: 'notify-content',
									duration: 8000
								});

								if (this.syncUser) {
									await commandApi.UploadUsers(this.commandRequest).then(async (res) => {
										await commandApi.UploadACUsers(this.commandRequest).then((res) => {
											const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
											this.$notify({
												type: 'success',
												title: 'Thông báo từ thiết bị',
												dangerouslyUseHTMLString: true,
												message: message,
												customClass: 'notify-content',
												duration: 8000
											});
										});
										const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
										this.$notify({
											type: 'success',
											title: 'Thông báo từ thiết bị',
											dangerouslyUseHTMLString: true,
											message: message,
											customClass: 'notify-content',
											duration: 8000
										});
									});
								} else {
									await commandApi.UploadACUsers(this.commandRequest).then((res) => {
										const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
										this.$notify({
											type: 'success',
											title: 'Thông báo từ thiết bị',
											dangerouslyUseHTMLString: true,
											message: message,
											customClass: 'notify-content',
											duration: 8000
										});
									});
								}
							}
						});
						this.Reset();
						this.$saveSuccess();
						this.Search();

					}

					this.showDialog = false;
				});
			}
			else {
				if (this.oldGroup != '' && this.oldGroup != this.ruleForm.GroupIndex.toString()) {
					await ac_DepartmentAccessedGroupApi.UpdateDepartmentAccessedGroup(submitData).then(async (res: any) => {
						if (res.status === 200 && res.data) {
							const msg = res.data;
							this.$alert(
								this.$t("MSG_DataDepartmentAccessGroupExistedFromTo", { data: msg }).toString(),
								this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
							);
						}else
						if (res.status != undefined && res.status === 200) {
							var door = this.listGroups.find(x => x.value == this.ruleForm.GroupIndex.toString());
							console.log(door, this.ruleForm.EmployeeATID);
							this.commandRequestTz.ListUser = [];
							this.commandRequestTz.AreaLst = [];
							this.commandRequestTz.DoorLst = [door.doorIndex];
							this.commandRequestTz.IsUsingArea = false;
							this.commandRequestTz.TimeZone = [];
	
							this.commandRequest.Action = "Upload user to machine";
	
							this.commandRequest.ListUser = [this.ruleForm.EmployeeATID];
							this.commandRequest.ListDepartment = this.ruleForm.DepartmentIDs;
							this.commandRequest.ListSerial = [];
							this.commandRequest.AreaLst = [];
							this.commandRequest.DoorLst = [door.doorIndex];
							this.commandRequest.IsUsingArea = false;
							this.commandRequest.TimeZone = [door.timezone];
							await commandApi.DeleteACDepartmentByDoor({
								ListSerial: [],
								ListUser: this.ruleForm.EmployeeATID,
								ListDepartment: this.ruleForm.DepartmentIDs,
								Action: 'Delete AC User',
								AuthenMode: [],
								AreaLst: [],
								DoorLst: [],
								EmployeeAccessedGroup: [this.ruleForm.Index],
								IsUsingArea: false
							}).then(async ress => {
								const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
												this.$notify({
													type: 'success',
													title: 'Thông báo từ thiết bị',
													dangerouslyUseHTMLString: true,
													message: message,
													customClass: 'notify-content',
													duration: 8000
												});
							});
							await commandApi.UploadTimeZone(this.commandRequestTz).then(async (res) => {
								if (!isNullOrUndefined(res.status) && res.status === 200) {
									this.$notify({
										type: 'success',
										title: 'Thông báo từ thiết bị',
										dangerouslyUseHTMLString: true,
										message: self.$t("SendRequestSuccess").toString(),
										customClass: 'notify-content',
										duration: 8000
									});
									if (this.syncUser) {
										await commandApi.UploadUsers(this.commandRequest).then(async (res) => {
											await commandApi.UploadACUsers(this.commandRequest).then((res) => {
												const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
												this.$notify({
													type: 'success',
													title: 'Thông báo từ thiết bị',
													dangerouslyUseHTMLString: true,
													message: message,
													customClass: 'notify-content',
													duration: 8000
												});
											});
											const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
											this.$notify({
												type: 'success',
												title: 'Thông báo từ thiết bị',
												dangerouslyUseHTMLString: true,
												message: message,
												customClass: 'notify-content',
												duration: 8000
											});
										});
									}
									else
									{
										await commandApi.UploadACUsers(this.commandRequest).then((res) => {
											const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
											this.$notify({
												type: 'success',
												title: 'Thông báo từ thiết bị',
												dangerouslyUseHTMLString: true,
												message: message,
												customClass: 'notify-content',
												duration: 8000
											});
										});
									}
									
								}
							});
							this.$saveSuccess();
						}
						this.Reset();
						this.showDialog = false;
						this.Search();
					});
				
				}else{
					await ac_DepartmentAccessedGroupApi.UpdateDepartmentAccessedGroup(submitData).then(async (res: any) => {
						if (res.status != undefined && res.status === 200) {
							var door = this.listGroups.find(x => x.value == this.ruleForm.GroupIndex.toString());
							console.log(door, this.ruleForm.EmployeeATID);
							this.commandRequestTz.ListUser = [];
							this.commandRequestTz.AreaLst = [];
							this.commandRequestTz.DoorLst = [door.doorIndex];
							this.commandRequestTz.IsUsingArea = false;
							this.commandRequestTz.TimeZone = [];
	
							this.commandRequest.Action = "Upload user to machine";
	
							this.commandRequest.ListUser = [this.ruleForm.EmployeeATID];
							this.commandRequest.ListDepartment = [this.ruleForm.DepartmentIDs];
							this.commandRequest.ListSerial = [];
							this.commandRequest.AreaLst = [];
							this.commandRequest.DoorLst = [door.doorIndex];
							this.commandRequest.IsUsingArea = false;
							this.commandRequest.TimeZone = [door.timezone];
							
							await commandApi.UploadTimeZone(this.commandRequestTz).then(async (res) => {
								if (!isNullOrUndefined(res.status) && res.status === 200) {
									this.$notify({
										type: 'success',
										title: 'Thông báo từ thiết bị',
										dangerouslyUseHTMLString: true,
										message: self.$t("SendRequestSuccess").toString(),
										customClass: 'notify-content',
										duration: 8000
									});
									if (this.syncUser) {
										await commandApi.UploadUsers(this.commandRequest).then(async (res) => {
											await commandApi.UploadACUsers(this.commandRequest).then((res) => {
												const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
												this.$notify({
													type: 'success',
													title: 'Thông báo từ thiết bị',
													dangerouslyUseHTMLString: true,
													message: message,
													customClass: 'notify-content',
													duration: 8000
												});
											});
											const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
											this.$notify({
												type: 'success',
												title: 'Thông báo từ thiết bị',
												dangerouslyUseHTMLString: true,
												message: message,
												customClass: 'notify-content',
												duration: 8000
											});
										});
									}
									else
									{
										await commandApi.UploadACUsers(this.commandRequest).then((res) => {
											const message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
											this.$notify({
												type: 'success',
												title: 'Thông báo từ thiết bị',
												dangerouslyUseHTMLString: true,
												message: message,
												customClass: 'notify-content',
												duration: 8000
											});
										});
									}
								}
							});
							this.$saveSuccess();
						}
						this.Reset();
						this.showDialog = false;
						this.Search();
					});
				}

				
			}
			(this.$refs.employeeAccessedGroupTable as any).getTableData(this.page, null, null);
		});
	}

	selectAllEmployeeFilter(value) {
		console.log(value)
		this.EmployeeATIDs = [...value];
		this.ruleForm.EmployeeATIDs = [...value];
	}

	onChangeDepartmentForm() {
        (this.$refs.ruleForm as any).clearValidate('DepartmentIDs');

	}

	Reset() {
		this.ruleForm = {};
		this.EmployeeATIDs = [];
		this.isDeleteAccessToMachine = false;
	}

	Search() {
		this.page = 1;
		(this.$refs.table as any).getTableData(this.page, null, null);
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
			this.ruleForm.DepartmentIDs = [this.ruleForm.DepartmentIndex];
			this.ruleForm.GroupIndex = this.ruleForm.GroupIndex.toString();
			this.oldGroup = this.ruleForm.GroupIndex.toString();

		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}

	}


	async Delete() {
		const listIndex: Array<number> = this.rowsObj.map((item: any) => {
			return item.Index;
		});

		if (listIndex.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.isDeleteAccessToMachine = false;
			this.showDialogDeleteUser = true;
			// this.$confirmDelete(
			//     this.$t("ConfirmDeleteAllUserOnMachine").toString(),
			//     this.$t("Notify").toString(),
			//     {
			//         type: "warning",
			//     }).then(async () => {
			//         // return await commandApi
			//         //     .DeleteAllUser(this.commandRequest)
			//         //     .then((res) => {
			//         //         if (!isNullOrUndefined(res.status) && res.status === 200) {
			//         //             this.$saveSuccess();
			//         //         }
			//         //     })
			//         //     .catch((err) => {
			//         //         this.$alertSaveError(null, err);
			//         //     });
			//     });
			// this.$confirmDelete().then(async () => {
			// 	await ac_DepartmentAccessedGroupApi
			// 		.DeleteEmployeeAccessedGroup(listIndex)
			// 		.then((res) => {
			// 			(this.$refs.table as any).getTableData(this.page, null, null);
			// 			if (!isNullOrUndefined(res.status) && res.status === 200) {
			// 				this.$deleteSuccess();
			// 			}
			// 		})
			// 		.catch(() => { });
			// });
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.EmployeeATIDs = [];
		this.showDialog = false;
	}
}
