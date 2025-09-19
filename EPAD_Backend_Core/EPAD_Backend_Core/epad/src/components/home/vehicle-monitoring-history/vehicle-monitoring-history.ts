import { Component, Vue, Mixins, Watch } from 'vue-property-decorator';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import moment from 'moment';
import { LocaleMessage } from 'vue-i18n';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import ComponentBase from '@/mixins/application/component-mixins';
import { GatesMonitoringHistoryModel, gatesMonitoringHistoryApi } from '@/$api/gc-gates-monitoring-history-api';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { rulesWarningApi } from '@/$api/gc-rules-warning-api';
import { lineApi } from '@/$api/gc-lines-api';
import { VehicleMonitoringHistoryModel, vehicleMonitoringHistoryApi } from '@/$api/gc-vehicle-monitoring-history-api';
import HeaderComponent from '@/components/home/header-component/header-component.vue';

@Component({
	name: 'vehicle-monitoring-history',
	components: { DataTableComponent, SelectTreeComponent, SelectDepartmentTreeComponent, HeaderComponent }
})
export default class VehicleMonitoringHistoryPage extends Mixins(ComponentBase) {
	//@Getter('getRoleByRoute', { namespace: 'RoleResource' }) roleByRoute: any;
	form : any = {};
	columns = [];
	listData = [];
	listLines = [];
	linesSelected = [];
	page = 1;
	pageSize = 40;
	filter = '';
	total = 0;
	listDepartment = [];
	listDepartmentLookup = {};
	employeeFullLookup = {};
	listemployeeFiltered = [];
	rulesWarningGroup = {};
	EmployeeATIDs = [];
	rulesWarning = [];
	ruleObject: VehicleMonitoringHistoryModel = {
		FromTime: new Date(moment().format('YYYY-MM-DD 00:00:00')),
		ToTime: new Date(moment().format('YYYY-MM-DD 23:59:59')),
		EmployeeIndexes: [],
		DepartmentIndexes: [],
		StatusLog: '',
		Page: 1,
		PageSize: 50,
		Filter: '',
	};
	exportText: LocaleMessage = '';
	statusList = [
        {Value: 'Success', Name: this.$t('Success')},
        {Value: 'Fail', Name: this.$t('Fail')}
    ]
	clearable = true;
	defaultExpandAll = false;
	multiple = true;
	placeholder = "";
	disabled = false;
	checkStrictly = false;
	popoverWidth = 400;
	treeData = [];
	treeProps = {
		value: 'ID',
		children: 'ListChildrent',
		label: 'Name',
	};
	listLine = [];

	vehicleType = [
		{ Name: this.$t('MotorBike'), Index: 1 },
		{ Name: this.$t('Bicycle'), Index: 2 },
		{ Name: this.$t('ElectricBicycle'), Index: 3 },
		{ Name: this.$t('Car'), Index: 4 },
	];

	masterEmployeeFilter = [];

	mounted() {
		const access_token = localStorage.getItem('access_token');
		if(access_token){
			// if (!this.roleByRoute  || (this.roleByRoute && !this.roleByRoute.length)) {
			// 	store.dispatch(ActionTypes.LOAD_ROLE_WITH_ROUTE,this.$route.meta.formName);
			// 	console.log(this.roleByRoute);
			// }
		}
		
	}
	async beforeMount() {
		this.createColumnHeader();
		await this.loadLines();
		await this.loadLookup();
		const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => (this.listemployeeFiltered as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
		}
	}

	async loadLookup() {
		await this.getListDepartment();
		await this.getEmployeesData();
		await this.getRulesWarningGroup();
		this.listemployeeFiltered = Object.keys(this.employeeFullLookup).map((key) => this.employeeFullLookup[key]);
	}

	createColumnHeader() {
		this.columns = [
			{
				label: 'EmployeeATID', 
				prop: 'EmployeeATID',
				Fixed: false,
				minWidth: 200, 
				display: true,
			},
			{
				label: 'EmployeeCode',
				prop: 'EmployeeCode',
				Fixed: false,
				minWidth: 200,
				display: true,
			},
			{
				label: 'FullName',
				prop: 'CustomerName',
				Fixed: false,
				width: 200, 
				display: true
			},
			{
				label: 'Department',
				prop: 'DepartmentName',
				Fixed: false,
				width: 200,
				search: false,
				display: true
			},
			{
				label: 'VehicleType', 
				prop: 'VehicleType',
				Fixed: false, 
				width: 200, 
				search: false, 
				display: true,
				dataType: 'translate'
			},
			{
				label: 'Plate', 
				prop: 'Plate',
				Fixed: false, 
				width: 200, 
				search: false, 
				display: true
			},
			{
				label: 'TimeIn',
				prop: 'FromTime',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				format: 'DD-MM-YYYY HH:mm:ss',
				dataType: 'date',
			},
			{
				label: 'TimeOut',
				prop: 'ToTime',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				format: 'DD-MM-YYYY HH:mm:ss',
				dataType: 'date',

			},
			{
				label: 'Reason', 
				prop: 'Reason',
				Fixed: false, 
				width: 200, 
				search: false,
				display: true,
				dataType: 'translate'
			},
			{
				label: 'Note', 
				prop: 'Note',
				Fixed: false,
				width: 250, 
				search: false, 
				display: true,
				dataType: 'translate'
			}
		];
	}

	viewData() {
		(this.$refs.table as any).getTableData(this.page, null, null);
	}


	async getData({ page, filter, sortParams, pageSize }) {
		this.ruleObject.EmployeeIndexes = this.EmployeeATIDs;
		if (moment(this.ruleObject.FromTime).format('YYYY-MM-DD HH:mm:ss') > moment(this.ruleObject.ToTime).format('YYYY-MM-DD HH:mm:ss')) {
			this.$alert(this.$t('MSG_FromDateMustBeLessThanToDate').toString(), this.$t('Notify').toString());
			return;
		}
		this.ruleObject.Page = page;
		this.ruleObject.PageSize = pageSize;
		this.ruleObject.Filter = this.ruleObject.Filter;
		const submitData = Misc.cloneData(this.ruleObject);
		submitData.FromTime = new Date(
			moment(submitData.FromTime).format("YYYY-MM-DD HH:mm:ss")
		);
		submitData.ToTime = new Date(
			moment(submitData.ToTime).format("YYYY-MM-DD HH:mm:ss")
		);
		return await vehicleMonitoringHistoryApi.GetVehicleMonitoringHistories(submitData)
			.then((res: any) => {
                const { data } = res as any;
				console.log(data);
                return {
                    data: data.data,
                    total: data.total
                };
			})
    }

	async loadLines() {
        await lineApi.GetAllLineBasic().then((res: any) => {
            if (res.status == 200 && res.statusText == "OK") {
                this.listLine = res.data;
            }
        });
    }

	async getListDepartment() {
		await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
			if (res.status == 200) {
				this.treeData = res.data;
				console.log(this.treeData);
			}
		});
	}

	async getEmployeesData() {
		await hrUserApi.GetAllEmployeeCompactInfo().then((res: any) => {
			if (res.status == 200 ) {
				const data = res.data;
				const dictData = {};
				data.forEach((e: any) => {
					dictData[e.EmployeeATID] = {
						Index: e.EmployeeATID,
						Name: `${e.FullName}`,
						NameInEng: `${e.FullName}`,
						Code: e.EmployeeATID,
						Department: e.Department,
						Position: e.Position,
						DepartmentIndex: e.DepartmentIndex,
						NameInFilter: `${e.EmployeeATID} - ${e.FullName}`,
					};
				});
				this.employeeFullLookup = dictData;
			}
		});
	}

	selectAllEmployeeFilter(value) {
		this.EmployeeATIDs = [...value];
		this.ruleObject.EmployeeIndexes = [...value];
	}
	
	//change
	onDeparmentChange(departments) {
		this.EmployeeATIDs = [];
		const emps = Object.keys(this.employeeFullLookup).map((key) => this.employeeFullLookup[key]);
		this.listemployeeFiltered = emps;
		if (departments.length > 0)
			this.listemployeeFiltered = emps.filter(s => s.DepartmentIndex && departments.indexOf(s.DepartmentIndex) > -1);

	}

	async getRulesWarningGroup() {
		await rulesWarningApi.GetRulesWarningGroup().then((res: any) => {
			const data = res.data;
			const dictData = {};
			data.forEach((e: any) => {
				dictData[e.Index] = {
					Index: e.Index,
					Name: e.Name,
					Code: e.Code
				};
			});
			this.rulesWarningGroup = dictData;
			rulesWarningApi.GetRulesWarningByCompanyIndex().then((res: any) => {
				const arrTemp = [];
				res.data.forEach((item) => {
					const warning = this.rulesWarningGroup[item.RulesWarningGroupIndex] || [];
					const a = Object.assign(item, {
						GroupName: warning.Name,
						GroupCode: warning.Code
					});
					arrTemp.push(a);
				});
				this.rulesWarning = arrTemp;
			});
		});
	}

	async exportClick() {
		if (moment(this.ruleObject.FromTime).format('YYYY-MM-DD HH:mm:ss') > moment(this.ruleObject.ToTime).format('YYYY-MM-DD HH:mm:ss')) {
			this.$alert(this.$t('MSG_FromDateMustBeLessThanToDate').toString(), this.$t('Notify').toString());
			return;
		}

		if (this.ruleObject == null) {
			this.ruleObject = {
				FromTime: new Date(),
				ToTime: new Date(),
				EmployeeIndexes: [],
				DepartmentIndexes: [],
				StatusLog: '',
				Page: 1,
				PageSize: 10000,
				Filter: ''
			};
		}
		this.ruleObject.EmployeeIndexes = this.EmployeeATIDs;	
		this.exportText = this.$t('ExportingExcell');
		await vehicleMonitoringHistoryApi.ExportVehicleHistory(this.ruleObject)
			.then((res: any) => {
				const fileURL = window.URL.createObjectURL(new Blob([res.data]));
				const fileLink = document.createElement("a");

				fileLink.href = fileURL;
				fileLink.setAttribute("download", `VehicleMonotoringHistory${moment(this.ruleObject.FromTime).format('YYYY-MM-DD')}_${moment(this.ruleObject.ToTime).format('YYYY-MM-DD')}.xlsx`);
				document.body.appendChild(fileLink);

				fileLink.click();
			})
			.catch((err) => {
				this.$alert(this.$t(err).toString(), this.$t('Notify').toString());
			});

		this.exportText = this.$t('ExportExcell')
	}
	// onEmployeeChange(employees) {
	// 	//const subMeals = Object.keys(this.listSubMealsLookup).map((key) => this.listSubMealsLookup[key]);
	// 	const fullEmps = Object.keys(this.employeeFullLookup).map((key) => this.employeeFullLookup[key]);
	// 	const emps = fullEmps.filter(x => employees.indexOf(x.Index) > -1);
	// 	const empDeps = emps.map(x => x.DepartmentIndex);
	// 	const empPos = emps.filter(x => x.Position != '').map(x => x.Position);
	// 	//this.listemployeeFiltered = subMeals;
	// 	// if (employees.length > 0)
	// 	// 	this.listOptionSubMeals = this.listSubMeals.filter(s => this.intersection([empDeps, s.DepartmentIndex]).length > 0
	// 	// 		&& this.intersection([empPos, s.PositionName.split(',')]).length > 0
	// 	// 		&& (this.form.CanteenIndex > 0 && s.CanteenIndex.indexOf(this.form.CanteenIndex) > -1));
	// }
}
