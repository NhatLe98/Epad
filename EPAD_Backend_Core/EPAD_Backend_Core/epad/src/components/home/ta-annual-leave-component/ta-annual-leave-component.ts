import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { TA_AnnualLeaveParam, taAnnualLeaveApi } from '@/$api/ta-annual-leave-api';
import * as XLSX from 'xlsx';

@Component({
	name: 'annualLeave',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent },
})
export default class AnnualLeaveComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	EmployeeATIDs = [];
	isEdit = false;
	fileName = '';
	dataProcessedExcel = [];
	dataAddExcel = [];
	isAddFromExcel = false;
	showDialogExcel = false;
	showDialogImportError = false;
	isDeleteFromExcel = false;
	formExcel = {};
	listExcelFunction = ['AddExcel'];
	ruleForm: any = {
		DepartmentIDs: [],
		EmployeeATIDs: [],
		AnnualLeave: 0
	}
	importErrorMessage = '';
	filter = '';
	rules: any = {};
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
	employeeFullLookupFilter = {};
	filterModel = {};
	DepartmentIDss = [];
	DepartmentIDs = [];
	listAllEmployee = [];
	SelectedDepartment = [];
	selectAllEmployeeFilter = [];
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

	masterEmployeeFilter = [];

	async beforeMount() {
		this.initColumns();
		this.initRule();
		this.LoadDepartmentTree();
		await this.getEmployeesData();
		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.selectAllEmployeeFilter = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
	}
	initRule() {
		this.rules = {
			EmployeeATIDs: [
				{
					required: true,
					message: this.$t('PleaseSelectEmployee'),
					trigger: 'blur',
				},
				{
					trigger: 'blur',
					message: this.$t('PleaseSelectEmployee'),
					validator: (rule, value: string, callback) => {
						if (!value || (value && value.length < 1)) {
							// console.log("")
							callback(new Error());
						}
						callback();
					},
				},
			],
			AnnualLeave: [
				{
					required: true,
					message: this.$t('PleaseInputAnnualLeave'),
					trigger: 'blur',
				},
			]
		};
	}

	mounted() {

	}

	initColumns() {
		this.columns = [
			{
				prop: 'EmployeeATID',
				label: 'MCC',
				minWidth: 300,
				display: true
			},
			{
				prop: 'EmployeeCode',
				label: 'EmployeeCode',
				minWidth: 300,
				display: true
			},
			{
				prop: 'FullName',
				label: 'FullName',
				minWidth: 300,
				display: true
			},
			{
				prop: 'DepartmentName',
				label: 'Department',
				minWidth: 200,
				display: true
			},
			{
				prop: 'AnnualLeave',
				label: 'AnnualLeave',
				minWidth: 300,
				display: true
			},
		];
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		const param: TA_AnnualLeaveParam = {
			Departments: this.SelectedDepartment,
			EmployeeATIDs: this.selectAllEmployeeFilter,
			Filter: this.filter,
			Limit: pageSize,
			Page: page
		}
		return await taAnnualLeaveApi.GetAnnualLeaveAtPage(param).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	Search() {
		(this.$refs.table as any).getTableData(1, null, null);
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
			if (this.dataAddExcel[0][i].hasOwnProperty('Phép năm (*)')) {
				a.AnnualLeave = this.dataAddExcel[0][i]['Phép năm (*)'] + '';
			} else {
				this.$alertSaveError(null, null, null, this.$t('AnnualLeaveMayNotBeBlank').toString()).toString();
				return;
			}
			
			arrData.push(a);
		}
		taAnnualLeaveApi.AddAnnualLeaveFromExcel(arrData).then((res) => {
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
				this.importErrorMessage = this.$t('ImportAnnualLeaveErrorMessage') + res.data.toString() + " " + this.$t('AnnualLeave');
				this.showOrHideImportError(true);
			}
		});
	}

	showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

	onChangeDepartmentFilter(departments) {
		this.ruleForm.EmployeeATIDs = [];
		if (departments && departments.length > 0) {
			this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		} else {
			this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee)?.map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
	}

	onChangeDepartmentFilterSearch(departments) {
		this.selectAllEmployeeFilter = [];
		if (departments && departments.length > 0) {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		} else {
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee)?.map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
	}

	LoadDepartmentTree() {
		departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.tree.treeData = res.data;
			}
		});

	}

	async getEmployeesData() {
		await hrUserApi.GetAllEmployeeCompactInfo().then((res: any) => {
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
				this.employeeFullLookupFilter = dictData;
			}
		});
	}
	
	reset() {
		const obj: any =  {
			DepartmentIDs: [],
			EmployeeATIDs: [],
			AnnualLeave: 0
		};
		this.ruleForm = obj;
		this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee)?.map(x => ({
			Index: x.EmployeeATID,
			NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
		}));
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.ruleForm.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => (this.employeeFullLookupFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
		}
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			console.log('ruleForm', this.ruleForm);
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await taAnnualLeaveApi.UpdateAnnualLeave(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await taAnnualLeaveApi
						.AddAnnualLeave(this.ruleForm)
						.then((res) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (res.status === 200 && res.data) {
								const msg = res.data;
								this.$alert(
									this.$t("MSG_DataRegisterExistedFromTo", { data: msg }).toString(),
									this.$t("Notify").toString(), { dangerouslyUseHTMLString: true }
								);
							}
							else if (res.status === 200) {
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
			console.log(this.ruleForm);
			this.ruleForm.EmployeeATIDs = [this.ruleForm.EmployeeATID];
			this.ruleForm.DepartmentIDs = [this.ruleForm.DepartmentIndex];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}


	async Delete() {
		const obj = JSON.parse(JSON.stringify(this.rowsObj.map(x => x.Index)));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await taAnnualLeaveApi
					.DeleteAnnualLeave(obj)
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
