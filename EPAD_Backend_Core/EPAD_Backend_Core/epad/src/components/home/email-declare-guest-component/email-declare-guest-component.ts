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
import { hrUserApi } from '@/$api/hr-user-api';
import { emailDeclareGuestApi, HR_EmailDeclareGuest } from '@/$api/email-declare-guest-api';


@Component({
	name: 'email-declare-guest-component',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectTreeComponent, SelectDepartmentTreeComponent },
})
export default class EmailDeclareGuestComponent extends Mixins(ComponentBase) {
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
	ruleForm: HR_EmailDeclareGuest = {
		EmployeeATID: '',
		EmailAddressList: [],
		Description: ''
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
	listAllEmployee = [];
	employeeFullLookupTemp = {};
	employeeFullLookup = {};

	initRule() {
		this.rules = {
			EmployeeATID: [
				{
					required: true,
					message: this.$t('PleaseSelectEmployee'),
					trigger: 'blur',
				},
			],
			EmailAddressList: [
				{
					required: true,
					message: this.$t('EmailIsRequired'),
					trigger: 'blur',
				}
			],
		};
	}
	async beforeMount() {
		this.setColumns();
		await this.getEmployeesData();
		this.initRule();
		// this.LoadDepartmentTree();
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


	setColumns() {
		this.columns = [
			{
				prop: 'EmployeeATID',
				label: 'EmployeeATID',
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
				label: 'DepartmentName',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'EmailAddress',
				label: 'EmailAddress',
				minWidth: '120',
				sortable: true,
				display: true
			},
			{
				prop: 'TimeUpdate',
				label: 'TimeUpdateNewest',
				minWidth: '150',
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

	reset() {
		const obj: HR_EmailDeclareGuest = {
			EmployeeATID: '',
			EmailAddressList: [],
			Description: ''
		};
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
        var regex = /^\d+$/;
		for (let i = 0; i < this.dataAddExcel[0].length; i++) {
			let a = Object.assign({});

			if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                return;
            }
			if (this.dataAddExcel[0][i].hasOwnProperty('Họ và tên')) {
				a.FullName = this.dataAddExcel[0][i]['Họ và tên'] + '';
			} else {
				a.FullName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban')) {
				a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban'] + '';
			} else {
				a.DepartmentName = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Email (*)')) {
				a.EmailAddress = this.dataAddExcel[0][i]['Email (*)'] + '';
			} else {
				a.EmailAddress = '';
			}
			if (this.dataAddExcel[0][i].hasOwnProperty('Ghi chú')) {
				a.Description = this.dataAddExcel[0][i]['Ghi chú'] + '';
			} else {
				a.Description = '';
			}
			
			arrData.push(a);
		}

		emailDeclareGuestApi.AddEmailDeclareGuestFromExcel(arrData).then((res) => {
			if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
				(<HTMLInputElement>document.getElementById('fileUpload')).value = '';
			}
			if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
				(<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
			}
			(this.$refs.emailDeclareGuestTable as any).getTableData(this.page, null, null);
			this.showDialogExcel = false;
			this.fileName = '';
			this.dataAddExcel = [];
			this.isAddFromExcel = false;
			if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
				this.$saveSuccess();
			} else {
				this.importErrorMessage = this.$t('ImportEmailDeclareGuestErrorMessage') + res.data.toString() + " " + this.$t('Email');
				this.showOrHideImportError(true);
			}
		});
	}

	showOrHideImportError(obj) {
		this.showDialogImportError = obj;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await emailDeclareGuestApi.GetEmailDeclareGuestAtPage(page, filter, pageSize).then((res) => {
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
		var regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
	
		// arr_Email.push(this.configCollection.RE_PROCESSING_REGISTERCARD.Email)
	
		var isValidEmail = true
		if (this.ruleForm.EmailAddressList.length > 0) {
			var isInvalidEmail = this.ruleForm.EmailAddressList.some(item => !regex.test(item))
			if (isInvalidEmail === true) {
				this.$alertSaveError(null, null, null, this.$t("InvalidEmail").toString());
				return;
			}
		  }

		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					emailDeclareGuestApi.UpdateEmailDeclareGuest(this.ruleForm).then((res) => {
						(this.$refs.emailDeclareGuestTable as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					emailDeclareGuestApi.AddEmailDeclareGuest(this.ruleForm).then((res) => {
						(this.$refs.emailDeclareGuestTable as any).getTableData(this.page, null, null);
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
		const listIndex: Array<any> = this.rowsObj.map((item: any) => {
            return item.Index;
        });
		
		if (listIndex.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(() => {
				emailDeclareGuestApi
					.DeleteEmailDeclareGuest(listIndex)
					.then((res) => {
						(this.$refs.emailDeclareGuestTable as any).getTableData(this.page, null, null);
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
