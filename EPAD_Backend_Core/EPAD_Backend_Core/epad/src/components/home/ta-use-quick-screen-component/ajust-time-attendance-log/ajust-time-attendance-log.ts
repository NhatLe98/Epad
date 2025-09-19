import { Component, Vue, Mixins, Model, Watch, Prop } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TabBase from '@/mixins/application/tab-mixins';
import { employeeInfoApi, Finger } from '@/$api/employee-info-api';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { hrPositionInfoApi } from '@/$api/hr-position-info-api';
import { employeeTypesApi } from '@/$api/ic-employee-type-api';
import { isNullOrUndefined } from "util";
import * as XLSX from 'xlsx';
import { store } from '@/store';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import { commandApi } from '@/$api/command-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { userTypeApi } from '@/$api/user-type-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import { hrUserApi } from '@/$api/hr-user-api';
import { EmployeeShiftModel, EmployeeShiftRequest, taEmployeeShiftApi } from '@/$api/ta-employee-shift-api';
import { taShiftApi } from '@/$api/ta-shift-api';
import comboBox from '@/components/app-component/app-select-table-component/app-select-table-component.vue'
import { TA_AjustTimeAttendanceLogParam, taAjustTimeAttendanceLog } from '@/$api/ta-ajust-time-attendance-log-api';
@Component({
	name: 'ajust-time-attendance-log',
	components: {
		VisualizeTable,
		AppPagination,
		SelectDepartmentTreeComponent,
		TableToolbar,
		comboBox
	},
})
export default class AjustTimeAttendanceLog extends Mixins(TabBase) {
	//===== for WS Register finger
	showFingerDialog = false;
	wsUri = "ws://127.0.0.1:22003";
	websocket;
	currentIndex: number = 0;
	DeviceInfo: string = this.$t('NotConnectedDevice').toString();
	ConnectedDevice: number = 0;
	template1: string = "";
	template2: string = "";
	registerCount: number = 0;
	listFinger: Array<Finger> = [];
	listAllPosition = [];
	listAllPositionLookup = {};
	columns = [];
	ruleForm: any = {}
	ListFingerRequest: any;
	clientName: string;
	@Prop({ default: () => false }) showMore: boolean;
	value = '';
	filter = '';
	lock = false;
	shouldResetColumnSortState = false;
	//=====================
	fileImageName = '';
	errorUpload = false;
	fileList = [];
	filterModel = { DepartmentIds: [], EmployeeATIDs: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date(), SelectedDepartmentFilter: [], SelectedEmployeeATIDFilter: [] };
	newInputContactInfo = {
		Name: '',
		Email: '',
		Phone: ''
	};
	listEmployeeType = [];
	currentSelectEmployeeATID = '';
	listTemplateFinger = [] as any;
	listWorkingStatus = [0];
	listContactInfoFormApi = [];
	isLoading = false;
	fromDate = new Date();
	toDate = new Date();
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
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
	DepartmentIDss = [];
	listAllEmployee = [];

	masterEmployeeFilter = [];

	dataSource = [];
	listShift: any[];
	EmployeeATIDs = [];
	columnDefs = [];
	listColumn = [];
	listData = [];
	rowsObj = [];

	@Prop({ default: () => [] }) departmentData: [];
	@Prop({ default: () => [] }) listEmployeeATID: [];

	@Prop({ default: () => [] }) departmentFilter: [];
	@Prop({ default: () => [] }) employeeFilter: [];
    @Watch("departmentFilter")
	dataDepartmentFilterChange(){
		this.filterModel.DepartmentIds = this.departmentFilter;
	}

	@Watch("employeeFilter")
	dataEmployeeFilterChange(){
		this.filterModel.EmployeeATIDs = this.employeeFilter;
	}

	onChangeClick(){
		console.log("ajust-attendance-log")
		this.loadData();
	}

	onSelectionChange(selectedRows: any[]) {
		// // console.log(selectedRows);
		this.selectedRows = selectedRows;
	}
	async beforeMount() {
		this.tree.treeData = this.departmentData;
				const dictData = {};
				this.listAllEmployee = this.listEmployeeATID;
				this.listEmployeeATID.forEach((e: any) => {
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

		var date = new Date();
		this.filterModel.FromDate = new Date(date.getFullYear(), date.getMonth(), 1);
		this.filterModel.ToDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
		// await this.LoadDepartmentTree();
		// await this.getEmployeesData();
		await this.getShiftList();
		this.initFormRules();

		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.filterModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
	}

	mounted() {
		this.lock = true;
		this.listColumn = [
			{ index: 0, name: this.$t('MCC'), dataField: 'EmployeeATID' },
			{ index: 1, name: this.$t('EmployeeCode'), dataField: 'EmployeeCode' },
			{ index: 2, name: this.$t('FullName'), dataField: 'FullName' },
			{ index: 3, name: this.$t('Department'), dataField: 'Department' },
			{ index: 50, name: this.$t('TotalWork'), dataField: 'TotalWorkingDay' },
			{ index: 51, name: this.$t('WorkingDay(X)'), dataField: 'WorkingDay' },
			{ index: 52, name: this.$t('AnnualLeave(P)'), dataField: 'AnnualLeave' },
			{ index: 53, name: this.$t('Leave(V)'), dataField: 'Leave' },
			{ index: 54, name: this.$t('NoSalaryLeave(KL)'), dataField: 'NoSalaryLeave' },
			{ index: 55, name: this.$t('BusinessTrip(CT)'), dataField: 'BusinessTrip' },
		]
	}

	LoadDepartmentTree() {
		departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.tree.treeData = res.data;
			}
		});
	}

	selectAllEmployeeFilter(value) {
		this.filterModel.EmployeeATIDs = [...value];
		this.$forceUpdate();
	}

	async getShiftList() {
		await taAjustTimeAttendanceLog.GetAllRegistrationType().then((res) => {
			if (res.status && res.status == 200 && res.data && (res.data as any).length > 0) {
				this.listShift = Misc.cloneData((res.data as any));
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
			}
		});
	}

	initFormRules() {
		this.formRules = {
			EmployeeATID: [
				{
					required: true,
					message: this.$t('PleaseInputEmployeeATID'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
					validator: (rule, value: string, callback) => {
						if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			CardNumber: [
				{
					trigger: 'change',
					message: this.$t('CardNumberOnlyAcceptNumericCharacters'),
					validator: (rule, value: string, callback) => {
						if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			FromDate: [
				{
					required: true,
					message: this.$t('PleaseSelectFromDate'),
					trigger: 'change',
				},
			],
			ToDate: [
				{
					required: true,
					message: this.$t('PleaseSelectToDate'),
					trigger: 'change',
				},
			],
			DepartmentIndex: [
				{
					required: true,
					message: this.$t('SelectDepartment'),
					trigger: 'change'
				},
			],
			FullName: [
				{
					required: true,
					message: this.$t('PleaseInputFullName'),
					trigger: 'change',
				},
			],
			TypeOfLeaveDate: [
				{
					required: true,
					message: this.$t('PleaseInputTypeOfLeaveDate'),
					trigger: 'change',
				},
			],
			DurationType: [
				{
					required: true,
					message: this.$t('PleaseInputDurationType'),
					trigger: 'change',
				},
			],
		};
	}

	onEditClick() {
		this.formModel = this.selectedRows[0];
		if (this.clientName == 'MAY') {
			this.formModel['PositionIndex'] = this.formModel['EmployeeType']
		}
		this.isEdit = true;
		this.showDialog = true;
	}

	async getDepartment() {
		return await departmentApi.GetAll().then((res) => {
			const { data } = res as any;
			let arr = JSON.parse(JSON.stringify(data));
			for (let i = 0; i < arr.Value.length; i++) {
				arr.Value[i].value = parseInt(arr.Value[i].value);
			}
			this.listDepartment = arr.Value;
		});
	}

	async loadData() {
		if (!this.lock) return;
		this.isLoading = true;

		if (this.filterModel.FromDate != null) {
			this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
		}
		if (this.filterModel.ToDate != null) {
			this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
		}
		let maxDate = new Date(this.filterModel.FromDate);

		maxDate.setMonth(maxDate.getMonth() + 1);
		if (maxDate.getDate() !== this.filterModel.FromDate.getDate()) {
			maxDate = new Date(this.filterModel.FromDate);
			const date = (maxDate.getFullYear().toString() + "-" + (maxDate.getMonth() + 3).toString() + "-1");
			maxDate = new Date(date);
			maxDate.setDate(maxDate.getDate() - 1);
		}

		if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
			&& this.filterModel.FromDate > this.filterModel.ToDate) {
			this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
			this.isLoading = false;
			return;
		}

		if (this.filterModel.ToDate >= maxDate) {
			this.$alertSaveError(null, null, null, this.$t('PleaseChooseMaximum1Month').toString()).toString();
			this.isLoading = false;
			return;
		}

		const request: TA_AjustTimeAttendanceLogParam = {
			FromDate: moment(this.filterModel.FromDate).format('YYYY-MM-DD'),
			ToDate: moment(this.filterModel.ToDate).format('YYYY-MM-DD'),
			EmployeeATIDs: this.filterModel.EmployeeATIDs,
			Departments: this.filterModel.DepartmentIds,
			Filter: this.filter
		}
		await taAjustTimeAttendanceLog.GetAjustTimeAttendanceLogAtPage(request).then((res: any) => {
			const Data = res.data;
			this.listColumn = [];
			Data.Columns.forEach(item => {
				const col = {
					index: item.Index,
					name: this.$t(item.Name),
					dataField: item.Code,
				};
				this.listColumn.push(col);
			});

			this.listData = Data.Rows;
		}).finally(() => {
			this.isLoading = false;
		});
	}

	onChangeDepartmentFilter(departments) {
		this.filterModel.EmployeeATIDs = [];
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

	async saveData() {
		const a = {
			Json: JSON.stringify(this.rowsObj)
		}
		await taAjustTimeAttendanceLog
			.UpdateAjustTimeAttendanceLog(a)
			.then(async (res) => {
				this.loadData();
				this.selectedRows = [];
				this.$saveSuccess();
			})
			.catch(() => { })
			.finally(() => { this.showDialogDeleteUser = false; })

	}
	getShiftIndex(value) {
		return (value != undefined && value != "") ? value : 0;
	}

	async doDelete() {
		const selectedEmp = this.selectedRows.map(e => e.EmployeeATID);
		await hrEmployeeInfoApi
			.DeleteEmployeeMulti(selectedEmp)
			.then(async (res) => {

				if (this.isDeleteOnDevice) {
					await commandApi.DeleteUserByIdFromUserId(selectedEmp).then((res) => {

					})
						.catch(() => { })

				}
				this.loadData();
				this.selectedRows = [];
				this.$deleteSuccess();
			})
			.catch(() => { })
			.finally(() => { this.showDialogDeleteUser = false; })


	}

	async onViewClick() {
		// console.log('this.filterModel',this.filterModel)
		this.filterModel.SelectedDepartmentFilter = this.filterModel.DepartmentIds;
        this.filterModel.SelectedEmployeeATIDFilter = this.filterModel.EmployeeATIDs;
		this.$emit('filterModel', this.filterModel);
		console.log('this.configModel', this.filterModel)
		this.page = 1;
		
		await this.loadData();
	}

	async getApiContactInfo() {
		await hrEmployeeInfoApi.GetAllEmployee().then((response) => {
			// this.listAllPosition = response.data;
			// // console.log(response.data)
			// this.listAllPosition.forEach(e => {
			//     this.listAllPositionLookup[e.Index] = e;
			// })
		});
	}

	async getEmployeeType() {
		await employeeTypesApi.GetUsingEmployeeType().then((res: any) => {
			if (res.status && res.status == 200) {
				this.listEmployeeType = res.data;
				// // console.log(this.listEmployeeType)
			}
		});
	}

	deleteContactInfo(name) {
		if (this.listContactInfoFormApi.length > 0) {
			for (let index = 0; index < this.listContactInfoFormApi.length; index++) {
				let valueExistInArray = this.listContactInfoFormApi[index]?.Name.indexOf(name);

				if (valueExistInArray > -1) {
					this.listContactInfoFormApi.splice(valueExistInArray, 1);
				}
			}
		}
	}

	//#region Import excel
	importErrorMessage = "";
	showImportExcel = true;
	isAddFromExcel = false;
	isDeleteFromExcel = false;
	addedParams = [];
	formExcel = {};
	dataAddExcel = [];
	fileName = '';
	showDialogExcel = false;
	dataProcessedExcel = [];
	showDialogImportError = false;
	listExcelFunction = ['AddExcel', 'DeleteExcel', 'ExportExcel'];

	showOrHideImportError(obj) {
		this.showDialogImportError = obj;
	}

	onCancelClick() {
		(this.$refs.employeeFormModel as any).resetFields();
		this.showDialog = false;
		this.selectedRows = [];
		this.loadData();
	}

	ShowOrHideDialogExcel(x) {
		if (x == 'open') {
			this.dataAddExcel = [];
			this.fileName = '';
			this.showDialogExcel = true;
		}
		else {
			(<HTMLInputElement>document.getElementById('fileUpload')).value = '';
			this.showDialogExcel = false;
		}
	}

	handleWorkingInfoChange(workingInfo: any) {
		this.listWorkingStatus = workingInfo;
		//this.getEmployees();
	}

	focus(x) {
		var theField = eval('this.$refs.' + x)
		theField.focus()
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

	handleSelectionChange(obj) {
		this.rowsObj = obj
	}
}
