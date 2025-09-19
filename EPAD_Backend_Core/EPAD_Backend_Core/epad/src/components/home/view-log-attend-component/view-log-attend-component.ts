import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';

import moment from 'moment';
import { isNullOrUndefined, isBuffer } from 'util';
import { employeeInfoApi } from '@/$api/employee-info-api';
import { employeeTransferApi, IC_EmployeeTransfer, AddedParam } from '@/$api/employee-transfer-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { departmentApi } from '@/$api/department-api';
import ImportPopupComponent from "@/components/app-component/import-popup-component/import-popup-component.vue";
import { privilegeDetailApi } from '@/$api/privilege-detail-api';

@Component({
	name: 'view-log-attend',
	components: { HeaderComponent, DataTableComponent,DataTableFunctionComponent,ImportPopupComponent },
})
export default class ViewLogAttendComponent extends Mixins(ComponentBase) {
	page = 1;
	filter = '';
	fromDate = moment(new Date().setHours(0, 0, 0)).format('YYYY-MM-DD HH:mm:ss');
	toDate = moment(new Date().setHours(23, 59, 59)).format('YYYY-MM-DD HH:mm:ss');
	expandedKey = [-1];
	showDialog = false;
	isEdit = false;
	rowsObj = [];
	loadingTree = false;
	treeData: any = [];
	filterTree = "";
	comboDepartment: any = [];
	columns = [];
	key: any = [];
    ArrEmployeeATID = [];
	titlePopupImport = this.$t('Import').toString();
	addedParams: Array<AddedParam> = [];
	isShowAttendance = false;
	PreviousDays: 0;
	isFull = false;

	ruleForm: IC_EmployeeTransfer = {
		EmployeeATID: '',
		NewDepartment: null,
		FromTime: null,
		ToTime: null,
		IsFromTime: '',
		IsToTime: '',
		OldDepartment: null,
		RemoveFromOldDepartment: false,
		AddOnNewDepartment: false,
		IsSync: null,
		Description: '',
	};

	arrData: any;
    defaultChecked = [];
    masterEmployeeFilter = [];

	rules: any = {};
	setColumns() {
		this.columns = [
			{
				prop: 'EmployeeATID',
				label: 'EmployeeATID',
				minWidth: '80',
				fixed: true,
				display:true
			},
			{
				prop: 'EmployeeCode',
				label: 'EmployeeCode',
				minWidth: '150',
				fixed: true,
				display: true
			},
			{
				prop: 'FullName',
				label: 'FullName',
				minWidth: '180',
				fixed: true,
				display: true
			},
			{
				prop: 'Time_',
				label: 'Time',
				minWidth: '180',
				format: '{0:MM/dd/yyyy HH:mm}',
				display: true
			},
			{
				prop: 'AliasName',
				label: 'AliasName',
				minWidth: '180',
				display: true
			},
			{
				prop: 'SerialNumber',
				label: 'SerialNumber',
				minWidth: '180',
				display: true
			},
			{
				prop: 'DepartmentName',
				label: 'DepartmentName',
				width: '220',
				display: true
			},
			{
				prop: 'InOutMode',
				label: 'InOutMode',
				minWidth: '180',
				display: true
			},
			{
				prop: 'VerifyMode',
				label: 'VerifyMode',
				minWidth: '180',
				display: true
			},
			{
				prop: 'FaceMask',
				label: 'FaceMask',
				minWidth: '180',
				display: true
			},
			{
				prop: 'BodyTemperature',
				label: 'BodyTemperature',
				minWidth: '180',
				display: true
			}
			//,
			//{
			//	prop: 'IsOverBodyTemperature',
			//	label: 'IsOverBodyTemperature',
			//	minWidth: '180',
			//	display: false
			//}
		];
	}
	created() {
		this.setColumns();
	}
	beforeMount() {
		this.loadDepertmentTree();
		this.loadComboDepartment();
		this.initRule();
		this.checkRole();
	}
	RunIntegrateLog(){
		this.isShowAttendance = !this.isShowAttendance;
	}
	mounted() {
		
	}

	checkRole(){
		 privilegeDetailApi.CheckPrivilegeFull('ViewLogAttend').then(res => {
			this.isFull = res.data;
		 })
	}



	DownloadAttendanceData(){
		attendanceLogApi.IntegrateLog(this.PreviousDays).then((res: any) => {
            if (res.status == 200) {
				this.isShowAttendance = false;
                this.$saveSuccess();
                
            } else {
                this.$alertSaveError(
                    null,
                    null,
                    null,
                    this.$t("MSG_IntegrateError").toString()
                );
                this.isLoading = false;
            }
        });

	}
	initRule() {
		this.rules = {
			NewDepartment: [
				{
					required: true,
					message: this.$t('PleaseSelectDepartmentTransfer'),
					trigger: 'blur',
				},
			],
			FromTime: [
				{
					required: true,
					message: this.$t('PleaseSelectDayTransfer'),
					trigger: 'blur',
				},
			],
			ToTime: [
				{
					required: true,
					message: this.$t('PleaseSelectDayTransfer'),
					trigger: 'blur',
				},
			],
		};
	}

	filterNode(value, data) {
		if (!value) return true;
		return (data.Name.indexOf(value) !== -1 || (!isNullOrUndefined(data.EmployeeATID) && data.EmployeeATID.indexOf(value) !== -1));

	}
	async filterTreeData() {
		this.loadingTree = true;
		(this.$refs.tree as any).filter(this.filterTree);
		this.loadingTree = false;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		
		return await attendanceLogApi
			.GetAtPageAttendanceLog(page, moment(this.fromDate).format('YYYY-MM-DD HH:mm:ss'), moment(this.toDate).format('YYYY-MM-DD HH:mm:ss'), this.filter, [...this.key],pageSize)
			.then((res) => {
				let { data } = res;
				let ar = [];
				data.data.forEach((element) => {
					ar.push(
						Object.assign(element, {
							Time_: moment(element.Time).format('YYYY-MM-DD HH:mm:ss'),
							FaceMask: this.$t(element.FaceMask).toString(),
							VerifyMode: this.$t(element.VerifyMode).toString(),
						})
					);
				});
				return {
					data: ar,
					total: data.total,
				};
			});
	}

	async View() {
		if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(this.$t('PleaseCheckTheCondition').toString(), this.$t('Notify').toString(), { type: 'warning' });
            return;
        }
        
        (this.$refs.table as any).getTableData(1,null,null);
    }
    Import(){
        const popupImport = this.$refs.popupImport as any;
        popupImport.showHideDialog(true);
    }
	async Export() {
		this.addedParams = [];
		this.addedParams.push({ Key: "FromDate", Value: moment(this.fromDate).format('YYYY-MM-DD HH:mm:ss') });
		this.addedParams.push({ Key: "ToDate", Value: moment(this.toDate).format('YYYY-MM-DD HH:mm:ss') });
		this.addedParams.push({ Key: "Filter", Value: this.filter });
		this.addedParams.push({ Key: "ListEmployeeATID",Value: [...this.key] });

		await attendanceLogApi.ExportAttendanceLog(this.addedParams).then( (res) => {
			  this.downloadFile(res.data.toString());
		});
	}
	 downloadFile(filePath) {
		var link = document.createElement('a');
		link.href = filePath;
		link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
		link.click();
		attendanceLogApi.DeleteAttendanceLogTemp().then(ress => {
			console.log('dasdas');
		});
	}
	async loadDepertmentTree() {
		this.loadingTree = true;
		return await employeeInfoApi
			.GetEmployeeAsTree(8)
			.then((res) => {
				this.loadingTree = false;
				const data = res.data as any;
				this.treeData = data;
				if(this.treeData){
					this.arrData = this.flattenArray(this.treeData);
					const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
					if(jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0){
						this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
						this.key = this.masterEmployeeFilter;
						this.defaultChecked = this.arrData.filter(x => this.masterEmployeeFilter.includes(x.EmployeeATID))
							?.map(x => x.ID) ?? [];
                        if(this.defaultChecked && this.defaultChecked.length > 0){
                            const result = this.findParentID(this.defaultChecked);
                            if(result && result.length > 0){
                                setTimeout(() => {
                                    const tree = (this.$refs.tree as any);
                                    result.forEach(element => {
                                        // tree.store.nodesMap[element].checked = true;
                                        tree.store.nodesMap[element].expanded = true;
                                        tree.store.nodesMap[element].loaded = true;
                                    });
                                }, 500);
                            }
                        }
					}
				}
				//this.treeData[0] = this.GetListChildrent(data[0]);
			})
			.catch(() => {
				this.loadingTree = false;
			});
	}

	flattenArray(data, parentId = null, result = []) {
        const cloneData = Misc.cloneData(data);
        cloneData.forEach(item => {
            // Create a copy of the item to avoid mutating the original data
            // const newItem = { ...item };
            const newItem = Misc.cloneData(item);

            // Set the parentIndex property
            newItem.ParentID = parentId;

            // Remove the children property from the new item
            delete newItem.ListChildrent;

            // Add the new item to the result array
            result.push(newItem);

            // Get the current item's index in the result array
            const currentIndex = item.ID;

            // If the item has a children array, recursively flatten it
            if (Array.isArray(item.ListChildrent) && item.ListChildrent.length > 0) {
                this.flattenArray(item.ListChildrent, currentIndex, result);
            }

            // delete item.ListChildrent;
        });

        return result;
    }

    findParentID(arrID){
		let result = [];
		const parentIDs = this.arrData.filter(x => arrID.includes(x.ID))?.map(x => x.ParentID) ?? [];
		if(parentIDs && parentIDs.length > 0){
			result = result.concat(parentIDs);
			if(this.arrData.some(x => parentIDs.includes(x.ID) && x.ParentID)){
				const nestParentIDs = this.findParentID(parentIDs);
				if(nestParentIDs && nestParentIDs.length > 0){
					result = result.concat(nestParentIDs);
				}
			}
		}
		result = [...new Set(result)];
		return result;
	}

	async loadComboDepartment() {
		return await departmentApi.GetDepartment().then((res) => {
			let a = JSON.parse(JSON.stringify(res.data));
			for (let i = 0; i < a.length; i++) {
				a[i].value = parseInt(a[i].value);
			}
			this.comboDepartment = a;
		});
	}

	async nodeCheck(e) {
		this.loadingEffect(500);
		//this.key = (this.$refs.tree as any)
		//	.getCheckedNodes()
		//	.filter((e) => isNullOrUndefined(e.ListChildrent))
		//	.map((e) => e.EmployeeATID);
		if (!this.filterTree) {
			this.key = (this.$refs.tree as any)
				.getCheckedNodes()
				.filter((e) => e.Type == 'Employee' )
				.map((e) => e.EmployeeATID);
		}
		else {
			this.key = (this.$refs.tree as any)
				.getCheckedNodes()
				.filter((e) => e.Type == 'Employee' && (e.Name.indexOf(this.filterTree) !== -1) || (!isNullOrUndefined(e.EmployeeATID) && e.EmployeeATID.indexOf(this.filterTree) !== -1))
				.map((e) => e.EmployeeATID);
		}
	}

	getIconClass(type, gender) {
		switch (type) {
			case 'Company':
				return 'el-icon-office-building';
				break;
			case 'Department':
				return 'el-icon-s-home';
				break;
			case 'Employee':
				if (isNullOrUndefined(gender) || gender === 'Other') {
					return 'el-icon-s-custom employee-other';
				} else if (gender === 'Male') {
					return 'el-icon-s-custom employee-male';
				} else {
					return 'el-icon-s-custom employee-female';
				}
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}
	loadingEffect(x) {
		const loading = this.$loading({
			lock: true,
			text: 'Loading',
			spinner: 'el-icon-loading',
			background: 'rgba(0, 0, 0, 0.7)',
		});
		setTimeout(() => {
			loading.close();
		}, x);
	}

	handleChange() {
		this.ruleForm.RemoveFromOldDepartment = !this.ruleForm.RemoveFromOldDepartment;
	}

	GetListChildrent(object) {
		if (!Misc.isEmpty(object.ListChildrent) && object.ListChildrent.length > 150) {
			//var arrTemp = [...object.ListChildrent]
			//delete object['ListChildrent']
			var arrTemp = [];
			for (let i = 0; i < Math.ceil(object.ListChildrent.length / 100); i++) {
				let calcFirstNumber = i * 100 + 1;
				let calcLastNumber = (i + 1) * 100 < object.ListChildrent.length ? (i + 1) * 100 : object.ListChildrent.length;
				arrTemp.push(
					Object.assign(
						{},
						{
							Name: calcFirstNumber + '-' + calcLastNumber,
							ListChildrent: object.ListChildrent.slice(calcFirstNumber - 1, calcLastNumber),
						},
						{}
					)
				);
			}
			object.ListChildrent = arrTemp;
		}
		if (!Misc.isEmpty(object.ListChildrent)) {
			object.ListChildrent.forEach((item) => {
				this.GetListChildrent(item);
			});
		}
		return object;
	}
}
