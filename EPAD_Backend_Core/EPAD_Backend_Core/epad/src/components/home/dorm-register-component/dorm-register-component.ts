import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { departmentApi, IC_Department } from '@/$api/department-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import * as XLSX from 'xlsx';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { excusedAbsentApi, ExcusedAbsentRequest, HR_ExcusedAbsent } from '@/$api/excused-absent-api';
import { dormRegisterApi, DormRegisterRequest, FormDormActivity, DormRegisterViewModel } from '@/$api/dorm-register-api';
import { dormRoomApi } from '@/$api/hr-dorm-room-api';
import { hrUserApi } from '@/$api/hr-user-api';

@Component({
	name: 'dorm-register',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent },
})
export default class DormRegisterComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	disabled = false;
	listExcelFunction = ['AddExcel'];
	isAddFromExcel = false;
	isDeleteFromExcel = false;
	showDialogImportError = false;
	importErrorMessage = "";
	showDialogExcel = false;
	fileName = '';
	dataAddExcel = [];
	ruleForm: DormRegisterViewModel = {
		Index: 0,
		EmployeeATID: null,
		EmployeeATIDs: [],
		FromDate: new Date(),
		ToDate: new Date(),
		RegisterDate: new Date(),
		StayInDorm: true,
		DormRoomIndex: null,
		DepartmentIndex: null,
		DormEmployeeCode: null,
		DormLeaveIndex: null,
		DormRegisterRation: [],
		DormRegisterActivity: []
	};
	formExcel = {};
	rules: any = {};
	fromDate = moment(new Date()).format("YYYY-MM-DD");
	toDate = moment(new Date()).format("YYYY-MM-DD");
	nowDate = moment(new Date()).format("YYYY-MM-DD");
	selectDepartment = [];
	selectDormRoom = [];
	formIsRegisterRation = false;
	formIsRegisterActivity = false;
	formDepartment = [];
	formDormRation = [];
	formDormActivity: Array<FormDormActivity> = [{
		DormActivityIndex: null,
		DormAccessMode: null,
		Error: null
	}];
	listAllEmployee = [];
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
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
	};
	listDormRoom = [];
	listDormActivity = [];
	listDormRation = [];
	listDormLeaveType = [];
	listDormAccessMode = [];

	masterEmployeeFilter = [];

	initRule() {
		this.rules = {
			EmployeeATID: [
				{
					required: true,
					message: this.$t('PleaseSelectEmployee'),
					trigger: 'change',
				}
			],
			FromDate: [
				{
					required: true,
					message: this.$t('PleaseSelectFromDate'),
					trigger: 'change',
				},
				// {
				// 	required: true,
				// 	trigger: 'change',
				// 	message: this.$t('PlaeseSelectSaturdayOrSunday'),
				// 	validator: (rule, value: any, callback) => {
				// 		if (this.ruleForm.FromDate.getDay() != 0 && this.ruleForm.FromDate.getDay() != 6) {
				// 			callback(new Error());
				// 		}
				// 		callback();
				// 	},
				// },
				{
					trigger: 'change',
					message: this.$t('FromDateCannotLargerToDate'),
					validator: (rule, value: string, callback) => {
						this.ruleForm.FromDate = new Date(this.ruleForm.FromDate);
						this.ruleForm.FromDate.setHours(0);
						this.ruleForm.FromDate.setMinutes(0);
						this.ruleForm.FromDate.setSeconds(0);
						this.ruleForm.FromDate.setMilliseconds(0);

						this.ruleForm.ToDate = new Date(this.ruleForm.ToDate);
						this.ruleForm.ToDate.setHours(0);
						this.ruleForm.ToDate.setMinutes(0);
						this.ruleForm.ToDate.setSeconds(0);
						this.ruleForm.ToDate.setMilliseconds(0);

						if (this.ruleForm.FromDate
							&& this.ruleForm.ToDate
							&& this.ruleForm.FromDate > this.ruleForm.ToDate) {
							callback(new Error());
						} else {
							callback();
						}
					},
				}
			],
			ToDate: [
				{
					required: true,
					message: this.$t('PleaseSelectToDate'),
					trigger: 'change',
				},
				// {
				// 	required: true,
				// 	trigger: 'change',
				// 	message: this.$t('PlaeseSelectSaturdayOrSunday'),
				// 	validator: (rule, value: any, callback) => {
				// 		if (this.ruleForm.ToDate.getDay() != 0 && this.ruleForm.ToDate.getDay() != 6) {
				// 			callback(new Error());
				// 		}
				// 		callback();
				// 	},
				// },
				{
					trigger: 'change',
					message: this.$t('FromDateCannotLargerToDate'),
					validator: (rule, value: string, callback) => {
						this.ruleForm.FromDate = new Date(this.ruleForm.FromDate);
						this.ruleForm.FromDate.setHours(0);
						this.ruleForm.FromDate.setMinutes(0);
						this.ruleForm.FromDate.setSeconds(0);
						this.ruleForm.FromDate.setMilliseconds(0);

						this.ruleForm.ToDate = new Date(this.ruleForm.ToDate);
						this.ruleForm.ToDate.setHours(0);
						this.ruleForm.ToDate.setMinutes(0);
						this.ruleForm.ToDate.setSeconds(0);
						this.ruleForm.ToDate.setMilliseconds(0);

						if (this.ruleForm.FromDate
							&& this.ruleForm.ToDate
							&& this.ruleForm.FromDate > this.ruleForm.ToDate) {
							callback(new Error());
						} else {
							callback();
						}
					},
				}
			],
		};
		if (this.ruleForm.StayInDorm) {
			delete this.rules.DormLeaveIndex;
			this.rules.DormRoom = [
				{
					required: true,
					trigger: 'change',
					message: this.$t('PleaseSelectDormRoom'),
					validator: (rule, value: any, callback) => {
						if (!this.ruleForm.DormRoomIndex || this.ruleForm.DormRoomIndex == 0 || (this.ruleForm.DormRoomIndex && this.ruleForm.DormRoomIndex < 1)) {
							callback(new Error());
						}
						callback();
					},
				}];
			this.rules.DormEmployeeCode = [{
				required: true,
				message: this.$t('PleaseInputDormEmployeeCode'),
				trigger: 'change',
			}];
			this.rules.DormRation = [
				{
					required: true,
					trigger: 'change',
					message: this.$t('PleaseSelectDormRation'),
					validator: (rule, value: any, callback) => {
						if (!this.formDormRation || (this.formDormRation && this.formDormRation.length < 1)) {
							callback(new Error());
						}
						callback();
					},
				}];
			this.rules.DormActivity = [
				{
					required: true,
					trigger: 'change',
					validator: (rule, value: any, callback) => {
						if (!this.formDormActivity || (this.formDormActivity && this.formDormActivity.length < 1)) {
							callback(new Error(this.$t("PleaseSelectDormActivity").toString()));
						}else if(this.formDormActivity.length == 1 
							&& (!this.formDormActivity[0].DormActivityIndex || this.formDormActivity[0].DormActivityIndex <= 0)
							&& (!this.formDormActivity[0].DormAccessMode || this.formDormActivity[0].DormAccessMode.length <= 0)){
								callback(new Error(this.$t("PleaseSelectDormActivity").toString()));
						}else{
							this.formDormActivity.forEach(element => {
								element.Error = null;
								if ((!element.DormActivityIndex || element.DormActivityIndex <= 0)
									&& element.DormAccessMode && element.DormAccessMode.length > 0) {
									element.Error = this.$t("PleaseSelectDormActivity").toString();
									callback(new Error(" "));
								}
								if (element.DormActivityIndex && element.DormActivityIndex > 0
									&& (!element.DormAccessMode || element.DormAccessMode.length < 1)) {
									element.Error = this.$t("PleaseSelectInOutMode").toString();
									callback(new Error(" "));
								}
								this.$forceUpdate();
							});
						}
						callback();
					},
				}];
		} else {
			delete this.rules.DormRoom;
			delete this.rules.DormEmployeeCode;
			delete this.rules.DormRation;
			delete this.rules.DormActivity;
			this.rules.DormLeave = [
				{
					required: true,
					trigger: 'change',
					message: this.$t('PleaseSelectDormLeave'),
					validator: (rule, value: any, callback) => {
						if (!this.ruleForm.DormLeaveIndex || this.ruleForm.DormLeaveIndex == 0 || (this.ruleForm.DormLeaveIndex && this.ruleForm.DormLeaveIndex < 1)) {
							callback(new Error());
						}
						callback();
					},
				}];
		}
	}

	deleteRow(index, row) {
		if (this.formDormActivity.length <= 1) return;
		if (index < 0) return;

		this.formDormActivity.splice(index, 1);
	}
	addRow(index, row) {
		this.formDormActivity.push({
			DormActivityIndex: null,
			DormAccessMode: null,
			Error: null
		});
	}

	checkDisabled(value) {
		const item = this.formDormActivity.find(item => item.DormActivityIndex == value);
		if (item != null) {
			return true;
		} else {
			return false;
		}
	}

	// selectAllEmployeeFilter(value) {
	// 	this.ruleForm.EmployeeATIDs = [...value];
	// }

	async beforeMount() {
		this.setColumns();
		this.initRule();
		await this.LoadDepartmentTree();
		await this.getDormRoomData();
		await this.getDormRationData();
		await this.getDormActivityData();
		await this.getDormAccessModeData();
		await this.getDormLeaveTypeData();
		await this.getEmployeesData();
		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
	}
	mounted() {
		this.updateFunctionBarCSS();
	}

	updateFunctionBarCSS() {
		//// Get all child in custom function bar
		const component1 = document.querySelector('.dorm-register__custom-function-bar');
		// // // console.log(component1.childNodes)
		const component2 = document.querySelector('.dorm-register__data-table');
		let childNodes = Array.from(component1.childNodes);
		const component5 = document.querySelector('.dorm-register__data-table-function');
		(component5 as HTMLElement).style.width = "100%";
		(component5 as HTMLElement).style.display = "flex";
		(component5 as HTMLElement).style.margin = "0 0 15px 0";
		(component5 as HTMLElement).style.paddingLeft = "24px";
		(component5 as HTMLElement).style.position = "relative";
		(component5 as HTMLElement).style.top = "0";
		// childNodes.push(component5);
		//// Insert all child in custom function bar to after filter bar of table
		childNodes.forEach((element, index) => {
			component2.insertBefore(element, component2.childNodes[index + 1]);
		});
		component2.insertBefore(component5, component2.childNodes[0]);
		// (document.querySelector('.dorm-register__data-table-function') as HTMLElement).style.height = "0";
	}

	async getDormRoomData() {
		await dormRoomApi.GetAllDormRoom().then((res: any) => {
			if (res.status == 200 && res.data) {
				this.listDormRoom = res.data;
				// // console.log(this.listDormRoom)
			}
		});
	}

	async getDormRationData() {
		await dormRegisterApi.GetDormRation().then((res: any) => {
			if (res.status == 200 && res.data) {
				this.listDormRation = res.data;
				// // console.log(this.listDormRation)
			}
		});
	}

	async getDormActivityData() {
		await dormRegisterApi.GetDormActivity().then((res: any) => {
			if (res.status == 200 && res.data) {
				this.listDormActivity = res.data;
				// // console.log(this.listDormActivity)
			}
		});
	}

	async getDormAccessModeData() {
		await dormRegisterApi.GetDormAccessMode().then((res: any) => {
			if (res.status == 200 && res.data) {
				this.listDormAccessMode = res.data;
				if (this.listDormAccessMode && this.listDormAccessMode.length > 0) {
					this.listDormAccessMode.forEach(element => {
						element.Name = this.$t(element.Name);
					});
				}
				// // console.log(this.listDormAccessMode)
			}
		});
	}

	async getDormLeaveTypeData() {
		await dormRegisterApi.GetDormLeaveType().then((res: any) => {
			if (res.status == 200 && res.data) {
				this.listDormLeaveType = res.data;
				// // console.log(this.listDormLeaveType)
			}
		});
	}

	async getEmployeesData() {
		await hrUserApi.GetAllEmployeeCompactInfoByPermissionImprovePerformance().then((res: any) => {
			if (res.status == 200) {
				const data = res.data;
				const dictData = {};
				this.listAllEmployee = data;
				data.forEach((e: any) => {
					dictData[e.EmployeeATID] = {
						Index: e.EmployeeATID,
						Name: `${e.FullName}`,
						NameInEng: `${e.FullName}`,
						NameInFilter: `${e.EmployeeATID} - ${e.FullName}`,
						Code: e.EmployeeATID,
						Department: e.Department,
						Position: e.Position,
						DepartmentIndex: e.DepartmentIndex,
					};
				});
				this.employeeFullLookup = dictData;
				this.employeeFullLookupTemp = dictData;
			}
		});
	}

	onChangeDepartmentFilter(departments) {
		// delete this.ruleForm.EmployeeATIDs; 
		this.ruleForm.EmployeeATIDs = [];
		if (departments && departments.length > 0) {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex))).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		} else {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
	}

	async LoadDepartmentTree() {
		await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.tree.treeData = res.data;
			}
		});

	}

	setColumns() {
		this.columns = [
			{
				prop: 'EmployeeATID',
				label: 'ID',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'FullName',
				label: 'FullName',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'DepartmentName',
				label: 'Class',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'RegisterDateString',
				label: 'Day',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'StayInDormString',
				label: 'StayInDorm',
				minWidth: '150',
				sortable: true,
				display: true
			},
			{
				prop: 'DormRoomName',
				label: 'Room',
				minWidth: '180',
				sortable: true,
				display: true
			},
			{
				prop: 'DormEmployeeCode',
				label: 'CodeString',
				minWidth: '180',
				sortable: true,
				display: true
			},
			{
				prop: 'DormRegisterRationName',
				label: 'RegisterRation',
				minWidth: '180',
				sortable: true,
				display: true
			},
			{
				prop: 'DormRegisterActivityName',
				label: 'RegisterActivity',
				minWidth: '180',
				sortable: true,
				display: true
			},
			{
				prop: 'RegisterLeaveName',
				label: 'RegisterLeave',
				minWidth: '180',
				sortable: true,
				display: true
			},
		];
	}

	reset() {
		const obj: DormRegisterViewModel = {
			Index: 0,
			EmployeeATID: null,
			EmployeeATIDs: [],
			FromDate: new Date(),
			ToDate: new Date(),
			RegisterDate: new Date(),
			StayInDorm: true,
			DormRoomIndex: null,
			DepartmentIndex: null,
			DormEmployeeCode: null,
			DormLeaveIndex: null,
			DormRegisterRation: [],
			DormRegisterActivity: []
		};
		this.ruleForm = obj;
		this.formDepartment = [];
		this.formIsRegisterRation = false;
		this.formIsRegisterActivity = false;
		this.formDormRation = [];
		this.formDormActivity = [{
			DormActivityIndex: null,
			DormAccessMode: null,
			Error: null
		}];
		this.onChangeDepartmentFilter(this.formDepartment);
	}

	handleCommand(command) {
		if (command === 'AddExcel') {
			this.AddOrDeleteFromExcel('add');
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


	processFile(e) {
		// // console.log(e.target.files[0]['type']);
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
						// // console.log(workbook.Sheets[sheet])
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

	UploadDataFromExcel() {
		this.importErrorMessage = "";
		var arrData = [];
		for (let i = 0; i < this.dataAddExcel[0].length; i++) {
			if(i <= 1){
				continue;
			}
			// // console.log(this.dataAddExcel[0][i]);
			let a = Object.assign({});
			if (this.dataAddExcel[0][i].hasOwnProperty('Mã ID (*)')) {
				a.EmployeeATID = this.dataAddExcel[0][i]['Mã ID (*)'] + '';
			} else {
				a.EmployeeATID = "";
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
				a.FullName = this.dataAddExcel[0][i]['Họ tên'] + '';
			} else {
				a.FullName = "";
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Lớp')) {
				a.DepartmentName = this.dataAddExcel[0][i]['Lớp'] + '';
			} else {
				a.DepartmentName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Từ ngày (*)')) {
				a.FromDateString = this.dataAddExcel[0][i]['Từ ngày (*)'] + '';
			} else {
				a.FromDateString = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Đến ngày (*)')) {
				a.ToDateString = this.dataAddExcel[0][i]['Đến ngày (*)'] + '';
			} else {
				a.ToDateString = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Ở lại')) {
				var stayInDormValue = this.dataAddExcel[0][i]['Ở lại'];
				a.StayInDorm = stayInDormValue.toString().toLowerCase() == "x" ? true : false;
			} else {
				a.StayInDorm = false;
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Phòng (*)')) {
				var dormRoomValue = this.dataAddExcel[0][i]['Phòng (*)'];
				a.DormRoomIndex = this.listDormRoom.find(x => x.Name == dormRoomValue)?.Index ?? null;
				a.DormRoomName = dormRoomValue;
			} else {
				a.DormRoomIndex = null;
				a.DormRoomName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Mã số (*)')) {
				a.DormEmployeeCode = this.dataAddExcel[0][i]['Mã số (*)'] + '';
			} else {
				a.DormEmployeeCode = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Đăng ký cơm')) {
				if(!a.DormRegisterRation){
					a.DormRegisterRation = [];
				}
				a.DormRegisterRation.push({
					DormRegisterIndex: null,
					DormRationIndex: this.listDormRation.find(x => x.Name == "Sáng")?.Index ?? null,
					DormRationName: "Sáng"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY')) {
				if(!a.DormRegisterRation){
					a.DormRegisterRation = [];
				}
				a.DormRegisterRation.push({
					DormRegisterIndex: null,
					DormRationIndex: this.listDormRation.find(x => x.Name == "Trưa")?.Index ?? null,
					DormRationName: "Trưa"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_1')) {
				if(!a.DormRegisterRation){
					a.DormRegisterRation = [];
				}
				a.DormRegisterRation.push({
					DormRegisterIndex: null,
					DormRationIndex: this.listDormRation.find(x => x.Name == "Chiều")?.Index ?? null,
					DormRationName: "Chiều"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Đăng ký hoạt động')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Siêu thị")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Ra")?.Index ?? null,
					DormActivityName: "Siêu thị"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_2')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Siêu thị")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Vào")?.Index ?? null,
					DormActivityName: "Siêu thị"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_3')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Đi lễ")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Ra")?.Index ?? null,
					DormActivityName: "Đi lễ"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_4')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Đi lễ")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Vào")?.Index ?? null,
					DormActivityName: "Đi lễ"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_5')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Bóng đá")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Ra")?.Index ?? null,
					DormActivityName: "Bóng đá"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('__EMPTY_6')) {
				if(!a.DormRegisterActivity){
					a.DormRegisterActivity = [];
				}
				a.DormRegisterActivity.push({
					DormRegisterIndex: null,
					DormActivityIndex: this.listDormActivity.find(x => x.Name == "Bóng đá")?.Index ?? null,
					DormAccessMode: this.listDormAccessMode.find(x => x.Name == "Vào")?.Index ?? null,
					DormActivityName: "Bóng đá"
				});
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Đăng ký di chuyển (*)')) {
				var dormLeaveValue = this.dataAddExcel[0][i]['Đăng ký di chuyển (*)'];
				a.DormLeavename = dormLeaveValue;
				a.DormLeaveIndex = this.listDormLeaveType.find(x => x.Name == dormLeaveValue)?.Index ?? null;
			}else{
				a.DormLeaveIndex = null;
				a.DormLeaveName = '';
			}
			arrData.push(a);
		}
		// // console.log(arrData)
		dormRegisterApi.AddDormRegisterFromExcel(arrData).then((res) => {
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
				this.importErrorMessage = this.$t('ImportAbsentErrorMessage') + res.data.toString() + " " + this.$t('AbsentInfo');
				this.showOrHideImportError(true);
			}
			(this.$refs.table as any).getTableData(this.page, null, null);
		});
	}

	showOrHideImportError(obj) {
		this.showDialogImportError = obj;
	}

	searchData() {
		if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
			this.$alert(
				this.$t("FromDateCannotLargerThanToDate").toString(),
				this.$t("Notify").toString(),
				{ type: "warning" }
			);
		} else {
			this.page = 1;
			(this.$refs.table as any).getTableData(this.page);
		}
	}

	changeStayInDorm() {
		(this.$refs.ruleForm as any).clearValidate();
		this.initRule();
		this.$forceUpdate();
		(this.$refs.ruleForm as any).validate();
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		const requestParam: DormRegisterRequest = {
			page: page, filter: filter, limit: pageSize,
			from: moment(this.fromDate).format("YYYY-MM-DD"),
			to: moment(this.toDate).format("YYYY-MM-DD"),
			departments: this.selectDepartment,
			dormRooms: this.selectDormRoom
		};
		return await dormRegisterApi.GetDormRegisterAtPage(requestParam).then((res) => {
			const { data } = res as any;
			data.data.forEach(element => {
				// element.RegisterDateString = moment(element.RegisterDate).format("DD-MM-YYYY");
				element.StayInDormString = element.StayInDorm ? this.$t("Yes") : this.$t("No");
			});
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	Insert() {
		this.reset();
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.ruleForm.EmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => (this.employeeFullLookup as any).some(y => y.Index == x))?.map(x => x.toString())[0] ?? null;
		}
		this.showDialog = true;
		this.isEdit = false;
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.formDormRation && this.formDormRation.length > 0) {
					this.ruleForm.DormRegisterRation = this.formDormRation.map((item, index) => {
						return {
							DormRegisterIndex: this.ruleForm.Index,
							DormRationIndex: item,
							DormRationName: null
						};
					});
				}
				if (this.formDormActivity && this.formDormActivity.length > 0) {
					this.ruleForm.DormRegisterActivity = this.formDormActivity.reduce((act, { DormActivityIndex, DormAccessMode }) => {
						if (DormAccessMode && DormAccessMode.length > 0) {
							DormAccessMode.forEach(item => {
								act.push({
									DormRegisterIndex: this.ruleForm.Index,
									DormActivityIndex: DormActivityIndex,
									DormAccessMode: item
								});
							});
						}

						return act;
					}, []);
				}

				this.ruleForm.FromDateString = moment(this.ruleForm.FromDate).format("DD-MM-YYYY");
				this.ruleForm.ToDateString = moment(this.ruleForm.ToDate).format("DD-MM-YYYY");
				// // console.log(this.ruleForm.RegisterDate)
				if (this.isEdit == true) {
					dormRegisterApi.UpdateDormRegister(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
							(this.$refs.table as any).getTableData(this.page, null, null);
						}
					});
				} else {
					dormRegisterApi.AddDormRegister(this.ruleForm).then((res) => {
						// // // console.log(res);
						if (res && res.data && !(res.data as any).Item1) {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
								(this.$refs.table as any).getTableData(this.page, null, null);
							}
						} else if ((res.data as any).Item1 && (res.data as any).Item1 == "DormRegisterIsExist") {
							var message = this.$t("StudentAlreadyHaveDormRegister").toString();
							if ((res.data as any).Item2 && (res.data as any).Item2.length > 0) {
								message += "<br/>";
								(res.data as any).Item2.forEach(element => {
									// // console.log(this.employeeFullLookupTemp[element.EmployeeATID])
									message += "- " + element.EmployeeATID + " "
										+ (this.employeeFullLookupTemp[element.EmployeeATID]?.Name ?? "") + " - "
										+ moment(element.RegisterDate).format("DD-MM-YYYY") + "<br/>";
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
					});
				}
			}
		});
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length == 1) {
			this.ruleForm = obj[0];
			this.ruleForm.EmployeeATIDs = [obj[0].EmployeeATID];
			if (obj[0].DormRegisterRation && obj[0].DormRegisterRation.length > 0) {
				this.formDormRation = obj[0].DormRegisterRation.map(x => x.DormRationIndex);
				if (this.formDormRation && this.formDormRation.length > 0) {
					this.formIsRegisterRation = true;
				}
			}
			if (obj[0].DormRegisterActivity && obj[0].DormRegisterActivity.length > 0) {
				this.formDormActivity = Object.values(obj[0].DormRegisterActivity.reduce((acc, { DormActivityIndex, DormAccessMode }) => {
					if (!acc[DormActivityIndex]) {
						acc[DormActivityIndex] = {
							DormActivityIndex,
							DormAccessMode: []
						};
					}
					acc[DormActivityIndex].DormAccessMode.push(DormAccessMode);

					if (acc[DormActivityIndex].DormAccessMode.length === 0) {
						delete acc[DormActivityIndex];
					}

					return acc;
				}, {}));
				if (this.formDormActivity && this.formDormActivity.length > 0) {
					this.formIsRegisterActivity = true;
				}
			}
			if (this.ruleForm.DormRoomIndex <= 0) {
				this.ruleForm.DormRoomIndex = null;
			}
			if (this.ruleForm.DormLeaveIndex <= 0) {
				this.ruleForm.DormLeaveIndex = null;
			}
			this.formDepartment = [obj[0].DepartmentIndex];
			this.initRule();
			setTimeout(() => {
				this.showDialog = true;
			}, 500);
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	async Delete() {
		const obj: number[] = JSON.parse(JSON.stringify(this.rowsObj.map(x => x.Index)));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(() => {
				dormRegisterApi
					.DeleteDormRegister(obj)
					.then((res) => {
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
						(this.$refs.table as any).getTableData(this.page, null, null);
					})
					.catch((er) => {
						console.log(er.response.data);
					});
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}
	Cancel() {
		this.reset();
		// var ref = <ElForm>this.$refs.ruleForm;
		// ref.resetFields();
		this.showDialog = false;
	}
}
