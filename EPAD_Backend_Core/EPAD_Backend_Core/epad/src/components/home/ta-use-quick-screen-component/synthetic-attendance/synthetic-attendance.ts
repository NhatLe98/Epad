import { Component, Mixins, Prop, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';
import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import TabBase from '@/mixins/application/tab-mixins';
import VisualizeTable from '@/components/app-component/visualize-table/visualize-table.vue';
import AppPagination from '@/components/app-component/app-pagination/app-pagination.vue';
import { TA_AjustTimeAttendanceLogParam, taAjustTimeAttendanceLog } from '@/$api/ta-ajust-time-attendance-log-api';
import { TA_TimeAttendanceLogParam, taTimeAttendanceLogApi } from '@/$api/ta-time-attendance-log-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
@Component({
	name: 'synthetic-attendance',
	components: {
		HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent, VisualizeTable,
		AppPagination, TableToolbar
	},
})

export default class SyntheticAttendance extends Mixins(TabBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isLoading = false;
	EmployeeATIDs = [];
	
	isEdit = false;
	listExcelFunction = [];
	ruleForm: AC_AreaDTO = {
		Name: '',
		Description: '',
	};
	allShift = [
		{ value: 1, label: 'Hành Chính' }
	]
	ruleObject = {};
	SelectedDepartment = [];
	rules: any = {};
	employeeFullLookupTemp = {};
	employeeFullLookup = {};
	DepartmentIDss = [];
	listAllEmployee = [];
	shouldResetColumnSortState = false;
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

	dataSource = [];
	listShift: any[];
	columnDefs = [];
	listColumn = [];
	listData = [];
	filterModel = { Departments: [], EmployeeATIDs: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date(), SelectedDepartmentFilter: [], SelectedEmployeeATIDFilter: [], filerByLog: null  };
	lock = false;
	listOptionFilterLog = [
		{ value: 1, label: this.$t('TotalHour'), },
		{ value: 2, label: this.$t('TotalHourNormal') },
		{ value: 3, label: this.$t('OverTime') },
		{ value: 4, label: this.$t('TotalHourNormal&OverTime') },
		{ value: 5, label: this.$t('CheckInLate') },
		{ value: 6, label: this.$t('CheckOutEarly') },
		{ value: 7, label: this.$t('HourByLogInOut') }
	]

	@Prop({ default: () => [] }) departmentData: [];
	@Prop({ default: () => [] }) listEmployeeATID: [];

	@Prop({ default: () => [] }) departmentFilter: [];
	@Prop({ default: () => [] }) employeeFilter: [];

	masterEmployeeFilter = [];

    @Watch("departmentFilter")
	dataDepartmentFilterChange(){
		this.filterModel.Departments = this.departmentFilter;
		
	}
	@Watch("employeeFilter")
	dataEmployeeFilterChange(){
		this.filterModel.EmployeeATIDs = this.employeeFilter;
		
	}

	onChangeClick(){
		this.loadData();
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
		this.initColumns();
		this.initRule();
		// this.LoadDepartmentTree();
		// this.getEmployeesData();

		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.filterModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
	}
	initRule() {
		this.rules = {
			Department: [
				{
					required: true,
					message: this.$t('PleaseInputDepartment'),
					trigger: 'blur',
				},
			],
			FromDate: [
				{
					required: true,
					message: this.$t('PleaseSelectFromDate'),
					trigger: 'change',
				},
			]
		};
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

	initColumns() {
		var date = new Date();
		this.filterModel.FromDate = new Date(date.getFullYear(), date.getMonth(), 1);
		this.filterModel.ToDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);

	}

	viewBtn(){
		this.filterModel.SelectedDepartmentFilter = this.filterModel.Departments;
        this.filterModel.SelectedEmployeeATIDFilter = this.filterModel.EmployeeATIDs;
		this.$emit('filterModel', this.filterModel);
		this.page = 1;
		this.loadData();
	}

	async loadData() {
		if(!this.lock) return;
		this.isLoading = true;
		this.columnDefs = [];
		this.dataSource = [];
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

		if(this.filterModel.ToDate >= maxDate){
			this.$alertSaveError(null, null, null, this.$t('PleaseChooseMaximum1Month').toString()).toString();
			this.isLoading = false;
			return;
		}
		
		const request: TA_TimeAttendanceLogParam = {
			FromDate: moment(this.filterModel.FromDate).format('YYYY-MM-DD'),
			ToDate: moment(this.filterModel.ToDate).format('YYYY-MM-DD'),
			EmployeeATIDs: this.filterModel.EmployeeATIDs,
			Departments: this.filterModel.Departments,
			Filter: this.filterModel.TextboxSearch,
			FilterByType: this.filterModel.filerByLog
		}
		await taTimeAttendanceLogApi.GetSyntheticAttendanceData(request).then((res: any) => {
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
		});
		this.isLoading = false;
	}


	handleCommand(command) {
		// if (command === 'AddExcel') {
		// 	this.AddOrDeleteFromExcel('add');
		// }
		// else if (command === 'ExportExcel') {
		// 	// this.ExportToExcel();
		// }
		// else if (command === 'DeleteExcel') {
		// 	this.AddOrDeleteFromExcel('delete');
		// }
	}

	displayPopupInsert() {
		this.showDialog = false;
	}


	onSelectionChange(selectedRows: any[]) {
		// // console.log(selectedRows);
		this.selectedRows = selectedRows;
	}

	onChangeDepartmentFilter(departments) {
		// delete this.ruleForm.EmployeeATIDs; 
		this.EmployeeATIDs = [];
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


	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await areaApi.GetAreaAtPage(page, filter, pageSize).then((res) => {
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
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			console.log('ruleForm', this.ruleForm);
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await areaApi.UpdateArea(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await areaApi
						.AddArea(this.ruleForm)
						.then((res) => {
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
		const obj: AC_AreaDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await areaApi
					.DeleteArea(obj)
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
