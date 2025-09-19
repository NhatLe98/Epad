import { Component, Vue, Mixins, Watch } from 'vue-property-decorator';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import moment from 'moment';
import { LocaleMessage } from 'vue-i18n';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import ComponentBase from '@/mixins/application/component-mixins';
import { truckerDriverLogApi, VehicleMonitoringHistoryModel } from '@/$api/gc-trucker-driver-log-api';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import HeaderComponent from "@/components/home/header-component/header-component.vue";

@Component({
	name: 'truck-monitoring-history',
	components: { DataTableComponent, SelectTreeComponent, SelectDepartmentTreeComponent, 
		DataTableFunctionComponent, HeaderComponent }
})
export default class TruckMonitoringHistoryPage extends Mixins(ComponentBase) {
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
	ruleObject = {
		FromTime: new Date(moment().format('YYYY-MM-DD 00:00:00')),
		ToTime: new Date(moment().format('YYYY-MM-DD 23:59:59')),
		Page: 1,
		PageSize: 50,
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


	updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.truck-monitoring-history__custom-function-bar');  
        let childNodes = Array.from(component1.childNodes);

        const component5 = document.querySelector('.truck-monitoring-history__data-table-function');
		if(childNodes.length > 0 && childNodes[childNodes.length - 1].childNodes.length > 0 
			&& component5.childNodes.length > 0
		){
			const configColumnNode = component5.childNodes[component5.childNodes.length - 1];
			(childNodes[childNodes.length - 1] as HTMLElement).style.display = "flex";
			childNodes[childNodes.length - 1].appendChild(component5.childNodes[component5.childNodes.length - 1]);
		}
    }

	async beforeMount() {
		this.createColumnHeader();
		await this.loadLookup();
		
	}

	mounted() {
		this.updateFunctionBarCSS();
		const access_token = localStorage.getItem('access_token');
		if(access_token){
			// if (!this.roleByRoute  || (this.roleByRoute && !this.roleByRoute.length)) {
			// 	store.dispatch(ActionTypes.LOAD_ROLE_WITH_ROUTE,this.$route.meta.formName);
			// 	console.log(this.roleByRoute);
			// }
		}
		
	}

	async loadLookup() {
		this.listemployeeFiltered = Object.keys(this.employeeFullLookup).map((key) => this.employeeFullLookup[key]);
	}

	createColumnHeader() {
		this.columns = [
			{
				label: 'TripCode', 
				prop: 'TripCode',
				Fixed: false,
				minWidth: 200, 
				display: true,
			},
			{
				label: 'DriverName',
				prop: 'DriverName',
				Fixed: false,
				width: 200, 
				display: true
			},
			{
				label: 'DriverCode',
				prop: 'DriverCode',
				Fixed: false,
				minWidth: 200,
				display: true,
			},
			{
				label: 'DriverPhone',
				prop: 'DriverPhone',
				Fixed: false,
				minWidth: 200,
				display: true,
			},
			{
				label: 'Plate', 
				prop: 'TrailerNumber',
				Fixed: false, 
				width: 200, 
				search: false, 
				display: true
			},
			{
				label: 'LocationFrom',
				prop: 'LocationFrom',
				Fixed: false,
				width: 200,
				search: false,
				display: true
			},
			// {
			// 	label: 'Eta', 
			// 	prop: 'EtaString',
			// 	Fixed: false, 
			// 	width: 200, 
			// 	search: false, 
			// 	display: true
			// },
			{
				label: 'TimeIn',
				prop: 'TimeInString',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				// format: 'DD-MM-YYYY hh:mm',
				// dataType: 'date',
			},
			{
				label: 'TimeOut',
				prop: 'TimeOutString',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				// format: 'DD-MM-YYYY hh:mm',
				// dataType: 'date',

			},
			{
				label: 'MachineIn',
				prop: 'MachineNameIn',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				format: 'DD-MM-YYYY hh:mm',
			},
			{
				label: 'MachineOut',
				prop: 'MachineNameOut',
				Fixed: false,
				width: 200,
				search: false,
				display: true,
				format: 'DD-MM-YYYY hh:mm',

			},
			{
				label: 'ExtraDriver', 
				prop: 'ExtraDriver',
				Fixed: false,
				width: 250, 
				search: false, 
				display: true,
			},
			{
				label: 'ExceptionReason', 
				prop: 'ReasonException',
				Fixed: false,
				width: 250, 
				search: false, 
				display: true,
				dataType: 'translate'
			},
		];
	}

	extraDriverColumns = [
		{
			label: 'ExtraDriverName', 
			prop: 'ExtraDriverName',
			Fixed: false,
			minWidth: 200, 
			display: true,
		},
		{
			label: 'CCCD',
			prop: 'ExtraDriverCode',
			Fixed: false,
			width: 200, 
			display: true
		},
		{
			label: 'BirthDay',
			prop: 'BirthDayString',
			Fixed: false,
			minWidth: 200,
			display: true,
		},
		{
			label: 'CardNumber',
			prop: 'CardNumber',
			Fixed: false,
			minWidth: 200,
			display: true,
		},
	];

	isShowExtraDriver = false;
	extraDriverData = [];
	showExtraDriver(row){
		// console.log(row)
		this.extraDriverData = Misc.cloneData((row.ExtraDriver && row.ExtraDriver.length > 0) ? row.ExtraDriver : []);
		if(this.extraDriverData && this.extraDriverData.length > 0){
			this.extraDriverData.forEach((element) => {
				element.BirthDayString = moment(element.BirthDay).format("DD-MM-YYYY");
			});
		}
		this.isShowExtraDriver = true;
	}

	cancelShowExtraDriver(){
		this.extraDriverData = [];
		this.isShowExtraDriver = false;
	}

	viewData() {
		(this.$refs.table as any).getTableData(this.page, null, null);
	}


	async getData({ page, filter, sortParams, pageSize }) {
		if (moment(this.ruleObject.FromTime).format('YYYY-MM-DD HH:mm:ss') > moment(this.ruleObject.ToTime).format('YYYY-MM-DD HH:mm:ss')) {
			this.$alert(this.$t('MSG_FromDateMustBeLessThanToDate').toString(), this.$t('Notify').toString());
			return;
		}
		this.ruleObject.Page = page;
		this.ruleObject.PageSize = pageSize;
		const submitData = Misc.cloneData(this.ruleObject);
		submitData.FromTime = new Date(
			moment(submitData.FromTime).format("YYYY-MM-DD HH:mm:ss")
		);
		submitData.ToTime = new Date(
			moment(submitData.ToTime).format("YYYY-MM-DD HH:mm:ss")
		);
		submitData.Filter = submitData.TextboxSearch;
		return await truckerDriverLogApi.GetTruckMonitoringHistories(submitData)
			.then((res: any) => {
                const { data } = res as any;
				// console.log(data);
                return {
                    data: data.data,
                    total: data.total
                };
			})
    }

	async exportClick() {
		if (moment(this.ruleObject.FromTime).format('YYYY-MM-DD HH:mm:ss') > moment(this.ruleObject.ToTime).format('YYYY-MM-DD HH:mm:ss')) {
			this.$alert(this.$t('MSG_FromDateMustBeLessThanToDate').toString(), this.$t('Notify').toString());
			return;
		}

		const submitData = Misc.cloneData(this.ruleObject);
		submitData.FromTime = new Date(
			moment(submitData.FromTime).format("YYYY-MM-DD HH:mm:ss")
		);
		submitData.ToTime = new Date(
			moment(submitData.ToTime).format("YYYY-MM-DD HH:mm:ss")
		);
		submitData.Filter = submitData.TextboxSearch;
		await truckerDriverLogApi.ExportTruckMonitoringHistories(this.ruleObject as any)
			.then((res: any) => {
				const fileURL = window.URL.createObjectURL(new Blob([res.data]));
				const fileLink = document.createElement("a");

				fileLink.href = fileURL;
				fileLink.setAttribute("download", `TruckMonitoringHistory_${moment(this.ruleObject.FromTime).format('YYYY-MM-DD')}_${moment(this.ruleObject.ToTime).format('YYYY-MM-DD')}.xlsx`);
				document.body.appendChild(fileLink);

				fileLink.click();
			})
			.catch((err) => {
				this.$alert(this.$t(err).toString(), this.$t('Notify').toString());
			});
	}
}
