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
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { TA_TimeAttendanceLogDTO, TA_TimeAttendanceLogParam, taTimeAttendanceLogApi } from '@/$api/ta-time-attendance-log-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';

@Component({
	name: 'handle-calculate-attendance',
	components: {
		HeaderComponent, DataTableComponent, DataTableFunctionComponent, SelectDepartmentTreeComponent, VisualizeTable,
		AppPagination, TableToolbar
	},
})


export default class HandleCalculateAttendance extends Mixins(TabBase) {
	page = 1;
	lock = false;
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
	filterModel = { ListEmployeeATID: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date(), SelectedDepartmentFilter: [], SelectedEmployeeATIDFilter: [] };

	ruleObject = {};
	SelectedDepartment = [];
	rules: any = {};
	filter = '';
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
	FromTime = moment(new Date().setHours(0, 0, 0)).format('YYYY-MM-DD HH:mm:ss');
	ToTime = moment(new Date().setHours(23, 59, 59)).format('YYYY-MM-DD HH:mm:ss');
	@Prop({ default: () => [] }) departmentData: [];
	@Prop({ default: () => [] }) listEmployeeATID: [];

	@Prop({ default: () => [] }) departmentFilter: [];
	@Prop({ default: () => [] }) employeeFilter: [];
    @Watch("departmentFilter")
	dataDepartmentFilterChange(){
		this.SelectedDepartment = this.departmentFilter;
		
	}
	@Watch("employeeFilter")
	dataEmployeeFilterChange(){
		this.EmployeeATIDs = this.employeeFilter;
		
	}

	onChangeClick(){
		console.log("register-leave")
		this.loadData();
	}

	masterEmployeeFilter = [];
	
	columnDefs = [
		{
			field: 'index',
			sortable: true,
			pinned: true,
			headerName: '#',
			width: 80,
			checkboxSelection: true,
			headerCheckboxSelection: true,
			headerCheckboxSelectionFilteredOnly: true,
			display: true
		},

		{
			field: 'EmployeeATID',
			headerName: this.$t('MCC'),
			pinned: true,
			width: 150,
			sortable: true,
			display: true
		},
		{
			field: 'EmployeeCode',
			headerName: this.$t('EmployeeCode'),
			pinned: true,
			width: 150,
			sortable: true,
			display: true
		},
		{
			field: 'FullName',
			headerName: this.$t('FullName'),
			pinned: true,
			width: 150,
			sortable: true,
			display: true
		},
		{
			field: 'DepartmentName',
			headerName: this.$t('Department'),
			pinned: true,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Shift'),
			field: 'ShiftName',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Day'),
			field: 'Date',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('In'),
			field: 'CheckIn',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('Out'),
			field: 'CheckOut',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('TotalWork'),
			field: 'TotalWorkingDay',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('TotalWorkingTime'),
			field: 'TotalWorkingTime',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('LeaveAttendance'),
			field: 'TotalDayOff',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('HolidayAttendance'),
			field: 'TotalHoliday',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('MissionAttendance'),
			field: 'TotalBusinessTrip',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('TotalNormalWorkingHour'),
			field: 'TotalWorkingTimeNormal',
			pinned: false,
			width: 210,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('NightWorkingHours'),
			field: 'TotalWorkingTimeNight',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('OvertimeNormalDay'),
			field: 'TotalOverTimeNormal',
			pinned: false,
			width: 210,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('NightOvertimeNormalDay'),
			field: 'TotalOverTimeNightNormal',
			pinned: false,
			width: 230,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('OvertimeLeaveDay'),
			field: 'TotalOverTimeDayOff',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('NightOvertimeLeaveDay'),
			field: 'TotalOverTimeNightDayOff',
			pinned: false,
			width: 210,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('OvertimeHoliday'),
			field: 'TotalOverTimeHoliday',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('NightOvertimeHoliday'),
			field: 'TotalOverTimeNightHoliday',
			pinned: false,
			width: 210,
			sortable: true,
			display: true
		},
		// {
		// 	headerName: this.$t('NightOvertimeLeaveDay'),
		// 	field: 'TotalOverTimeNightHoliday',
		// 	pinned: false,
		// 	width: 210,
		// 	sortable: true,
		// 	display: true
		// },
		{
			headerName: this.$t('LateEntry(minute)'),
			field: 'CheckInLate',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},
		{
			headerName: this.$t('EarlyOut(minute)'),
			field: 'CheckOutEarly',
			pinned: false,
			width: 150,
			sortable: true,
			display: true
		},

	];

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

		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
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
	}

	initColumns() {
		var date = new Date();
		this.FromTime = moment(new Date(date.getFullYear(), date.getMonth(), 1)).format('YYYY-MM-DD HH:mm:ss');
		this.ToTime = moment(new Date(date.getFullYear(), date.getMonth() + 1, 0)).format('YYYY-MM-DD HH:mm:ss');
	}

	selectAllEmployeeFilter(value) {
		this.EmployeeATIDs = [...value];
		this.$forceUpdate();
	}

	async btnView() {
		this.filterModel.SelectedDepartmentFilter = this.SelectedDepartment;
        this.filterModel.SelectedEmployeeATIDFilter = this.EmployeeATIDs;
        this.$emit('filterModel', this.filterModel);
		this.page = 1;
		(this.$refs.taHandleCalculatePagination as any).page = this.page;
		(this.$refs.taHandleCalculatePagination as any).lPage = this.page;
		this.getData();
	}

	async loadData() {
		if (!this.lock) return;
		this.getData();
	}

	calculateAttendance() {
		const request: TA_TimeAttendanceLogDTO = {
			FromDate: moment(this.FromTime).format('YYYY-MM-DD'),
			ToDate: moment(this.ToTime).format('YYYY-MM-DD'),
			EmployeeATIDs: this.EmployeeATIDs
		}
		this.isLoading = true;
		return taTimeAttendanceLogApi.CaculateAttendance(request).then((res) => {
			this.showDialog = false;
			this.reset();
			this.getData();
			// if (!isNullOrUndefined(res.status) && res.status === 200) {
			// 	const message = `<p class="notify-content">${this.$t("SendRequestCaculateAttendanceSuccess").toString()}</p>`;
			// 	this.$notify({
			// 		type: 'success',
			// 		title: 'Thông báo',
			// 		dangerouslyUseHTMLString: true,
			// 		message: message,
			// 		customClass: 'notify-content',
			// 		duration: 8000
			// 	});
			// }
		}).finally(() => {
			this.isLoading = false;
		});
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
			this.employeeFullLookup = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)))?.map(x => ({
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


	async getData() {
		if (this.FromTime != null && this.ToTime != null
			&& this.FromTime > this.ToTime) {
			this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
			this.isLoading = false;
			return;
		}
		this.isLoading = true;
		const filter: TA_TimeAttendanceLogParam = {
			Departments: this.SelectedDepartment,
			EmployeeATIDs: this.EmployeeATIDs,
			FromDate: moment(this.FromTime).format('YYYY-MM-DD'),
			ToDate: moment(this.ToTime).format('YYYY-MM-DD'),
			
			Page: this.page,
			PageSize: this.pageSize,
			Filter: this.filter
		}
		return taTimeAttendanceLogApi.GetCaculateAttendanceData(filter).then((res) => {
			const { data } = res as any;
			if (res.status == 200 && data) {
				this.dataSource = data.data.map((x, idx) => {
					return {
						...x,
						index: idx + 1 + (this.page - 1) * this.pageSize
					}
				});
				this.total = data.total;
			}
		}).finally(() => {
			this.isLoading = false;
		});;
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
