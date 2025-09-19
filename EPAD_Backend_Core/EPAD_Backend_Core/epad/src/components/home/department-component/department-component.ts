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


@Component({
	name: 'department',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent },
})
export default class DepartmentComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	columns = [];
	rowsObj = [];
	departments = [];
	isEdit = false;
	disabled = false;
	listExcelFunction = ['AddExcel'];
	isAddFromExcel = false;
	isDeleteFromExcel = false;
	showDialogImportError = false;
	importErrorMessage = "";
	showDialogExcel = false;
	clientName = '';
	fileName = '';
	dataAddExcel = [];
	ruleForm: IC_Department = {
		Name: '',
		Location: '',
		Description: '',
		Code: '',
		ParentIndex: null,
		IsDriverDepartment: null,
		IsContractorDepartment: null
	};
	rules: any = {};

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
	isDisableCheckbox = false;
	formExcel = {};
	initRule() {
		this.rules = {
			Code: [
				{
					required: true,
					message: this.$t('PleaseInputDepartmentCode'),
					trigger: 'blur',
				},
			],
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputDepartmentName'),
					trigger: 'blur',
				},
			],
		};
	}
	beforeMount() {
		Misc.readFileAsync('static/variables/common-utils.json').then(x => {
			this.clientName = x.ClientName;
			this.setColumns();
		})

		this.initRule();
		this.LoadDepartmentTree();
	}
	mounted() {
		this.getComboDepartment();
	}

	LoadDepartmentTree() {
		departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.tree.treeData = res.data;
				this.tree.treeData[0].ListChildrent = this.tree.treeData[0].ListChildrent.filter(x => x.ID != 0);
			}
		});

	}

	setColumns() {
		if (this.clientName == 'Mondelez') {
			this.columns = [
				{
					prop: 'Name',
					label: 'DepartmentName',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'Location',
					label: 'Location',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'ParentName',
					label: 'DepartmentParent',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'IsContractorDepartmentName',
					label: 'ContractorDepartment',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'IsDriverDepartmentName',
					label: 'DriverDepartment',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'Description',
					label: 'Description',
					minWidth: '180',
					sortable: true,
					display: true
				},
			];
		} else {
			this.columns = [
				{
					prop: 'Name',
					label: 'DepartmentName',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'Location',
					label: 'Location',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'ParentName',
					label: 'DepartmentParent',
					minWidth: '120',
					sortable: true,
					display: true
				},
				{
					prop: 'Description',
					label: 'Description',
					minWidth: '180',
					sortable: true,
					display: true
				},
			];
		}
	}

	reset() {
		const obj: IC_Department = { IsContractorDepartment: false, IsDriverDepartment: false };
		this.ruleForm = obj;
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

	onChangeDepartmentParent(department) {
		console.log(department)
		if (department) {
			this.isDisableCheckbox = true;
		} else {
			this.isDisableCheckbox = false;
		}
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
			// if (this.dataAddExcel[0][i].hasOwnProperty('Mã phòng ban (*)')) {
			// 	a.Code = this.dataAddExcel[0][i]['Mã phòng ban (*)'] + '';
			// } else {
			// 	this.$alertSaveError(null, null, null, this.$t('DeparmentCodeMayNotBeBlank').toString()).toString();
			// 	return;
			// }
			if (this.dataAddExcel[0][i].hasOwnProperty('Tên phòng ban (*)')) {
				a.Name = this.dataAddExcel[0][i]['Tên phòng ban (*)'] + '';
			} else {
				this.$alertSaveError(null, null, null, this.$t('DepartmentNameMayNotBeBlank').toString()).toString();
				return;
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Vị trí')) {
				a.Location = this.dataAddExcel[0][i]['Vị trí'] + '';
			} else {
				a.Location = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban cha')) {
				a.ParentName = this.dataAddExcel[0][i]['Phòng ban cha'] + '';
			} else {
				a.ParentName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Diễn giải')) {
				a.Description = this.dataAddExcel[0][i]['Diễn giải'] + '';
			} else {
				a.Description = '';
			}
			if (this.dataAddExcel[0][i]['Phòng ban nhà thầu'] == 'x' || this.dataAddExcel[0][i]['Phòng ban nhà thầu'] == 'X') {
				a.IsContractorDepartment = true;
			} else {
				a.IsContractorDepartment = false;
			}
			if (this.dataAddExcel[0][i]['Phòng ban tài xế'] == 'x' || this.dataAddExcel[0][i]['Phòng ban tài xế'] == 'X') {
				a.IsDriverDepartment = true;
			} else {
				a.IsDriverDepartment = false;
			}
			arrData.push(a);
		}

		departmentApi.AddDepartmentFromExcel(arrData).then((res) => {
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
				this.importErrorMessage = this.$t('ImportDepartmentErrorMessage') + res.data.toString() + " " + this.$t('Department');
				this.showOrHideImportError(true);
			}
		});
	}

	showOrHideImportError(obj) {
		this.showDialogImportError = obj;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await departmentApi.GetDepartmentAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	Insert() {
		this.reset();
		this.showDialog = true;
		this.isEdit = false;
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					departmentApi.UpdateDepartment(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					departmentApi.AddDepartment(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
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
			this.showDialog = true;
			this.ruleForm = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}

	async Delete() {
		const obj: IC_Department[] = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(() => {
				departmentApi
					.DeleteDepartment(obj)
					.then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
					})
					.catch((er) => {
						console.log(er.response.data);
					});
			});
		}
	}

	async getComboDepartment() {
		return await departmentApi.GetDepartment().then((res) => {
			//this.departments = res.data as any;
			var arr = [...JSON.parse(JSON.stringify(res.data))];
			var arr_1 = [];
			for (let i = 0; i < arr.length; i++) {
				arr_1.push({ value: parseInt(arr[i].value), label: arr[i].label });
			}
			this.departments = arr_1;
		});
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
