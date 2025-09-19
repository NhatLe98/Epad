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
import ImageCellRendererVisualizeTable from '@/components/app-component/visualize-table/image-cell-renderer-visualize-table.vue';
import { commandApi } from '@/$api/command-api';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { userTypeApi } from '@/$api/user-type-api';
import TableToolbar from '@/components/home/printer-control/table-toolbar.vue';
import { hrUserApi } from '@/$api/hr-user-api';
import { taBusinessRegistrationApi, TA_BusinessRegistration, TA_BusinessRegistrationDTO, BusinessRegistrationModel, BusinessType } 
    from '@/$api/ta-business-registration-api';

@Component({
    name: 'register-business-trip',
    components: {
        VisualizeTable,
        AppPagination,
        ImageCellRendererVisualizeTable,
        SelectDepartmentTreeComponent,
        TableToolbar
    },
})
export default class RegisterBusinessTrip extends Mixins(TabBase) {
    @Prop({ default: () => false }) showMore: boolean;
    shouldResetColumnSortState = false;

    filterDepartment = [];
    formDepartment = [];

    fileImageName = '';
    errorUpload = false;
    fileList = [];
    filterModel = { ListEmployeeATID: [], TextboxSearch: '', FromDate: new Date(), ToDate: new Date(), SelectedDepartmentFilter: [], SelectedEmployeeATIDFilter: [] };
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
	employeeFullLookupForm = {};
	employeeFullLookupFilter = {};
	DepartmentIDss = [];
    listAllEmployee = [];
    TypeBusinessTrip = [];
    @Prop({ default: () => [] }) departmentData: [];
	@Prop({ default: () => [] }) listEmployeeATID: [];

    @Prop({ default: () => [] }) departmentFilter: [];
	@Prop({ default: () => [] }) employeeFilter: [];

    masterEmployeeFilter = [];
    
    @Watch("departmentFilter")
	dataDepartmentFilterChange(){
		this.filterDepartment = this.departmentFilter;
		
	}
	@Watch("employeeFilter")
	dataEmployeeFilterChange(){
		this.filterModel.ListEmployeeATID = this.employeeFilter;
		
	}

	onChangeClick(){
        this.getEmployeesData();
		console.log("register-leave")
		this.loadData();
	}

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
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'EmployeeCode',
            headerName: this.$t('EmployeeCode'),
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('FullName'),
            field: 'FullName',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            field: 'DepartmentName',
            headerName: this.$t('Department'),
            pinned: false,
            width: 170,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('DateRegisterBusiness'),
            field: 'BusinessDateString',
            pinned: false,
            width: 200,
            sortable: true,
            // cellRenderer: params => `${moment(params.value).format('DD-MM-YYYY')}`,
            display: true
        },
        // {
        //     headerName: this.$t('TotalWork'),
        //     field: 'TotalWork',
        //     pinned: false,
        //     width: 150,
        //     sortable: true,
        //     display: true
        // },
        {
            headerName: this.$t('TypeBusinessTrip'),
            field: 'BusinessTypeName',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        // {
        //     headerName: this.$t('Type'),
        //     field: 'TypeBusinessTripDetail',
        //     pinned: false,
        //     width: 150,
        //     sortable: true,
        //     display: true
        // },
        {
            headerName: this.$t('StartTime'),
            field: 'FromTimeString',
            pinned: false,
            width: 150,
            sortable: true,
            // cellRenderer: params => `${moment(params.value).format('HH:mm')}`,
            display: true
        },
        {
            headerName: this.$t('EndTime'),
            field: 'ToTimeString',
            pinned: false,
            width: 150,
            sortable: true,
            // cellRenderer: params => `${moment(params.value).format('HH:mm')}`,
            display: true
        },
        {
            headerName: this.$t('WorkPlace'),
            field: 'WorkPlace',
            // dataType: 'lookup',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
        {
            headerName: this.$t('Reason'),
            field: 'Description',
            // dataType: 'lookup',
            pinned: false,
            width: 150,
            sortable: true,
            display: true
        },
    ];

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
				this.employeeFullLookupFilter = dictData;
				this.employeeFullLookupForm = dictData;
				this.employeeFullLookupTemp = dictData;
        (this.formModel as any).FromDate = new Date();
        (this.formModel as any).ToDate = new Date();
        this.initFormRules();
        // this.LoadDepartmentTree();
        // this.getEmployeesData();
        await taBusinessRegistrationApi.GetBusinessType().then((res: any) => {
            if(res && res.status == 200 && res.data){
                this.TypeBusinessTrip = res.data.map(x => {
                    return {
                        Index: x.Index,
                        Name: this.$t(x.Value)
                    }
                });
            }
        });

        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
		if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.filterModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookupFilter.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
    }

    async initLookup() {
        //await this.getDepartment();
        // this.LoadDepartmentTree();
       
    }

    LoadDepartmentTree() {
        departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });

    }

    selectAllEmployeeFilter(value) {
		this.filterModel.ListEmployeeATID = [...value];
	}

    selectAllEmployeeForm(value) {
		(this.formModel as any).ListEmployeeATID = [...value];
        this.$forceUpdate();
        (this.$refs.registerBusinessModel as any).validate();
	}

    onChangeDepartmentFilter(departments) {
		this.filterModel.ListEmployeeATID = [];
		if (departments && departments.length > 0) {
            const filterEmployees = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)));
            if(filterEmployees && filterEmployees.length > 0){
                this.employeeFullLookupFilter = filterEmployees.map(x => ({
                    Index: x.EmployeeATID,
                    NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
                }));
            }else{
                this.employeeFullLookupFilter = {};
            }
		} else {
			this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
	}

    onChangeDepartmentForm(departments) {
		// delete this.ruleForm.ListEmployeeATID; 
		(this.formModel as any).ListEmployeeATID = [];
        if (departments && departments.length > 0) {
            const filterEmployees = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex)));
            if(filterEmployees && filterEmployees.length > 0){
                this.employeeFullLookupForm = filterEmployees.map(x => ({
                    Index: x.EmployeeATID,
                    NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
                }));
            }else{
                this.employeeFullLookupForm = {};
            }
		} else {
			this.employeeFullLookupForm = Misc.cloneData(this.listAllEmployee).map(x => ({
				Index: x.EmployeeATID,
				NameInFilter: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
			}));
		}
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
				this.employeeFullLookupForm = dictData;
				this.employeeFullLookupFilter = dictData;
				this.employeeFullLookupTemp = dictData;
			}
		});
	}
    
    initFormRules() {
        this.formRules = {
            ListEmployeeATID: [
                {
                    required: true,
                    trigger: 'change',
                    message: this.$t('PleaseSelectEmployee'),
                    validator: (rule, value: string, callback) => {
                        if (!(this.formModel as any).ListEmployeeATID || 
                            ((this.formModel as any).ListEmployeeATID && (this.formModel as any).ListEmployeeATID.length == 0)) {
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
                {
					trigger: 'change',
					message: this.$t('FromDateCannotLargerThanToDate'),
					validator: (rule, value: string, callback) => {
						const startDate = new Date((this.formModel as any).FromDate);
                        startDate.setHours(0);
                        startDate.setMinutes(0);
                        startDate.setSeconds(0);
                        const endDate = new Date((this.formModel as any).ToDate);
                        endDate.setHours(0);
                        endDate.setMinutes(0);
                        endDate.setSeconds(0);
						const start = (this.formModel as any).FromDate 
							? Math.trunc(startDate.getTime() / 1000) : 0;
						const end = (this.formModel as any).ToDate 
							? Math.trunc(endDate.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
            ],
            ToDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectToDate'),
                    trigger: 'change',
                },
                {
					trigger: 'change',
					message: this.$t('FromDateCannotLargerThanToDate'),
					validator: (rule, value: string, callback) => {
                        const startDate = new Date((this.formModel as any).FromDate);
                        startDate.setHours(0);
                        startDate.setMinutes(0);
                        startDate.setSeconds(0);
                        const endDate = new Date((this.formModel as any).ToDate);
                        endDate.setHours(0);
                        endDate.setMinutes(0);
                        endDate.setSeconds(0);
						const start = (this.formModel as any).FromDate 
							? Math.trunc(startDate.getTime() / 1000) : 0;
						const end = (this.formModel as any).ToDate 
							? Math.trunc(endDate.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
            ],
            BusinessDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectBusinessDate'),
                    trigger: 'change',
                },
            ],
            BusinessType: [
                {
                    required: true,
                    message: this.$t('PleaseSelectTypeBusinessTrip'),
                    trigger: 'change',
                },
            ],
            FromTime: [
				{
					required: true,
					message: this.$t('PleaseSelectStartTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('StartTimeCannotLargerThanToTime'),
					validator: (rule, value: string, callback) => {
                        const startValue = new Date();
						startValue.setHours(((this.formModel as any).FromTime as Date).getHours(), 
                        ((this.formModel as any).FromTime as Date).getMinutes(), 
                        ((this.formModel as any).FromTime as Date).getSeconds());
						const endValue = new Date();
						endValue.setHours(((this.formModel as any).ToTime as Date).getHours(), 
                        ((this.formModel as any).ToTime as Date).getMinutes(), 
                        ((this.formModel as any).ToTime as Date).getSeconds());
						const start = ((this.formModel as any).FromTime as Date) 
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = ((this.formModel as any).ToTime as Date) 
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
			ToTime: [
				{
					required: true,
					message: this.$t('PleaseSelectEndTime'),
					trigger: 'change',
				},
				{
					trigger: 'change',
					message: this.$t('StartTimeCannotLargerThanToTime'),
					validator: (rule, value: string, callback) => {
						const startValue = new Date();
						startValue.setHours(((this.formModel as any).FromTime as Date).getHours(), 
                        ((this.formModel as any).FromTime as Date).getMinutes(), 
                        ((this.formModel as any).FromTime as Date).getSeconds());
						const endValue = new Date();
						endValue.setHours(((this.formModel as any).ToTime as Date).getHours(), 
                        ((this.formModel as any).ToTime as Date).getMinutes(), 
                        ((this.formModel as any).ToTime as Date).getSeconds());
						const start = ((this.formModel as any).FromTime as Date) 
							? Math.trunc(startValue.getTime() / 1000) : 0;
						const end = ((this.formModel as any).ToTime as Date) 
							? Math.trunc(endValue.getTime() / 1000) : 0;
						if (start > 0 && end > 0 && start > end) {
							callback(new Error());
						}
						callback();
					},
				},
			],
        };

    }

    @Watch("formModel", {deep: true})
	handleChangeForm(){
        (this.$refs.registerBusinessModel as any).validate();
	}

    onInsertClick(){
        this.formModel = {FromDate: new Date(), ToDate: new Date()};
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.formModel.ListEmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookupForm.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
        this.isEdit = false;
        this.showDialog = true;
    }

    onEditClick() {
        this.isEdit = true;
        this.formModel = Misc.cloneData(this.selectedRows[0]);
        (this.formModel as any).ListEmployeeATID = [(this.formModel as any).EmployeeATID];
        if((this.formModel as any).DepartmentIndex && (this.formModel as any).DepartmentIndex > 0){
            this.formDepartment = [(this.formModel as any).DepartmentIndex];
        }
        if((this.formModel as any).FromTime){
            (this.formModel as any).FromTime = new Date((this.formModel as any).FromTime);
        }
        if((this.formModel as any).ToTime){
            (this.formModel as any).ToTime = new Date((this.formModel as any).ToTime);
        }
        this.showDialog = true;
        setTimeout(() => {
            (this.$refs.registerBusinessModel as any).clearValidate();
            (this.$refs.registerBusinessModel as any).validate();
        }, 200);
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
        this.isLoading = true;

        if (this.filterModel.FromDate != null) {
            this.filterModel.FromDate = new Date(moment(this.filterModel.FromDate).format('YYYY-MM-DD'));
        }
        if (this.filterModel.ToDate != null) {
            this.filterModel.ToDate = new Date(moment(this.filterModel.ToDate).format('YYYY-MM-DD'));
        }
        if (this.filterModel.FromDate != null && this.filterModel.ToDate != null
            && this.filterModel.FromDate > this.filterModel.ToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerThanToDate').toString()).toString();
            this.isLoading = false;
            return;
        }
        const filterParam: BusinessRegistrationModel = {
            EmployeeATID: null,
            BusinessDate: null,
            BusinessType: null,
            FromTime: null,
            ToTime: null,
            FromDateString: this.filterModel.FromDate ? moment(this.filterModel.FromDate).format("YYYY-MM-DD") : '',
            ToDateString: this.filterModel.ToDate ? moment(this.filterModel.ToDate).format("YYYY-MM-DD") : '',
            ListDepartmentIndex: this.filterDepartment,
            ListEmployeeATID: this.filterModel.ListEmployeeATID,
            Filter: this.filterModel.TextboxSearch,
            Page: this.page,
            PageSize: this.pageSize
        };
        await taBusinessRegistrationApi.GetBusinessRegistration(filterParam).then((res: any) => {
            // console.log(res)
            if(res.status == 200 && res.data){
                this.dataSource = res.data.data.map((x, idx) => {
                    return {
                        ...x,
                        index: idx + 1 + (this.page - 1) * this.pageSize,
                        BusinessTypeName: this.$t(x.BusinessTypeName),
                    }
                });
                this.total = res.data.total;
            }
        }).finally(() => {
            this.shouldResetColumnSortState = !this.shouldResetColumnSortState;
        });
        this.isLoading = false;
    }

    async doDelete() {
        const selectedIndex = JSON.parse(JSON.stringify(this.selectedRows.map(x => x.Index)));
        await taBusinessRegistrationApi.DeleteBusinessRegistration(selectedIndex).then(async (res) => {
            // console.log(res)
            if(res.status && res.status == 200 && res.data){
                this.$deleteSuccess();
                // this.$alert(this.$t('DeleteSuccess').toString(), this.$t('Notify').toString());
                this.selectedRows = [];
            }else{
                this.$deleteError();
                // this.$alert(this.$t('DeleteFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
            }
            // this.$deleteSuccess();
        })
        .catch(() => { })
        .finally(() => { 
            this.showDialogDeleteUser = false;
            this.loadData();
        })
    }

    async onViewClick() {
        this.filterModel.SelectedDepartmentFilter = this.filterDepartment;
        this.filterModel.SelectedEmployeeATIDFilter = this.filterModel.ListEmployeeATID;
        this.$emit('filterModel', this.filterModel);
        this.page = 1;
        (this.$refs.registerBusinessPagination as any).page = this.page;
        (this.$refs.registerBusinessPagination as any).lPage = this.page;
        await this.loadData();
    }

    async onSubmitClick() {
        (this.$refs.registerBusinessModel as any).validate(async (valid) => {
            if (!valid) return;
            (this.formModel as any).FromDateString = moment((this.formModel as any).FromDate).format('YYYY-MM-DD');
            (this.formModel as any).ToDateString = moment((this.formModel as any).ToDate).format('YYYY-MM-DD');
            (this.formModel as any).BusinessDateString = moment((this.formModel as any).BusinessDate).format('YYYY-MM-DD');
            (this.formModel as any).FromTimeString = moment((this.formModel as any).FromTime).format('YYYY-MM-DD HH:mm');
            (this.formModel as any).ToTimeString = moment((this.formModel as any).ToTime).format('YYYY-MM-DD HH:mm');
            // (this.formModel as any).FromDate = new Date((this.formModel as any).FromDateString);
            // (this.formModel as any).ToDate = new Date((this.formModel as any).ToDateString);
            // (this.formModel as any).BusinessDate = new Date((this.formModel as any).BusinessDateString);
            // if ((this.formModel as any).FromDate && (this.formModel as any).ToDate && (this.formModel as any).ToDate < (this.formModel as any).FromDate) {
            //     this.$alertSaveError(null, null, null, this.$t('JoinedDateCannotLargerThanToDate').toString()).toString();
            //     return;
            // }
            if((this.formModel as any).index){
                delete (this.formModel as any).index;
            }
            if((this.formModel as any).Index && (this.formModel as any).Index > 0){
                await taBusinessRegistrationApi.UpdateBusinessRegistration(this.formModel).then((res: any) => {
                    // console.log(res)
                    if(res.status && res.status == 200 && res.data){
                        if(typeof res.data == 'object'){
                            let message = '';
                            for (const [key, value] of Object.entries(res.data)) {
                                message += this.$t('Employee') + " " + key + ": <br>";
                                (value as any).forEach(element => {
                                    message += "- " + this.$t(element) + "<br>";   
                                });
                            }
                            this.$alert(message, this.$t('Warning').toString(), { 
                                type: "warning", 
                                dangerouslyUseHTMLString: true 
                            });
                        }else{
                            this.$saveSuccess();
                            // this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
                            this.showDialog = false;
                        }
                    }else{
                        this.$saveError();
                        // this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
                    }
                }).finally(() => {
                    this.loadData();
                });
            }else{
                await taBusinessRegistrationApi.AddBusinessRegistration(this.formModel).then((res: any) => {
                    // console.log(res)
                    if(res.status && res.status == 200 && res.data){
                        if(typeof res.data == 'object'){
                            let message = '';
                            for (const [key, value] of Object.entries(res.data)) {
                                message += this.$t('Employee') + " " + key + ": <br>";
                                (value as any).forEach(element => {
                                    message += "- " + this.$t(element) + "<br>";   
                                });
                            }
                            this.$alert(message, this.$t('Warning').toString(), { 
                                type: "warning", 
                                dangerouslyUseHTMLString: true 
                            });
                        }else{
                            this.$saveSuccess();
                            // this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
                            this.showDialog = false;
                        }
                    }else{
                        this.$saveError();
                        // this.$alert(this.$t('UpdateFailed').toString(), this.$t('Warning').toString(), { type: "warning" });
                    }
                }).finally(() => {
                    this.loadData();
                });
            }
        })
        this.selectedRows = [];
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
    listExcelFunction = ['AddExcel'];

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    onCancelClick() {
        this.onChangeDepartmentForm([]);
        (this.$refs.registerBusinessModel as any).resetFields();
        (this.$refs.registerBusinessModel as any).clearValidate();
        this.showDialog = false;
        this.selectedRows = [];
        this.formModel = {};
        (this.formModel as any).FromDate = new Date();
        (this.formModel as any).ToDate = new Date();
        this.formDepartment = [];
        this.loadData();
    }

    UploadDataFromExcel() {

        this.importErrorMessage = "";
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            // if (regex.test(this.dataAddExcel[0][i]["Mã chấm công (*)"]) === false) {
            //     this.$alertSaveError(null, null, null, this.$t("EmployeeATIDOnlyAcceptNumericCharacters").toString()).toString();
            //     return;
            // } else 
            if (this.dataAddExcel[0][i].hasOwnProperty("MCC (*)")) {
                a.EmployeeATID = this.dataAddExcel[0][i]["MCC (*)"] + "";
            } else {
                // this.$alertSaveError(null, null, null, this.$t("EmployeeATIDMayNotBeBlank").toString()).toString();
                // return;
                a.EmployeeATID = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("MNV")) {
                a.EmployeeCode = this.dataAddExcel[0][i]["MNV"] + "";
            } else {
                a.EmployeeCode = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Họ tên")) {
                a.FullName = this.dataAddExcel[0][i]["Họ tên"] + "";
            } else {
                a.FullName = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Từ ngày (*)")) {
                a.FromDateString = this.dataAddExcel[0][i]["Từ ngày (*)"] + "";
            } else {
                a.FromDateString = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Đến ngày (*)")) {
                a.ToDateString = this.dataAddExcel[0][i]["Đến ngày (*)"] + "";
            } else {
                a.ToDateString = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Cả ngày") && this.dataAddExcel[0][i]["Cả ngày"] != "") {
                a.BusinessType = BusinessType.BusinessAllShift;
            } else {
                a.BusinessType = BusinessType.BusinessFromToTime;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Từ giờ")) {
                a.FromTimeString = this.dataAddExcel[0][i]["Từ giờ"] + "";
            } else {
                a.FromTimeString = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Đến giờ")) {
                a.ToTimeString = this.dataAddExcel[0][i]["Đến giờ"] + "";
            } else {
                a.ToTimeString = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Nơi công tác")) {
                a.WorkPlace = this.dataAddExcel[0][i]["Nơi công tác"] + "";
            } else {
                a.WorkPlace = "";
            }
            if (this.dataAddExcel[0][i].hasOwnProperty("Lý do")) {
                a.Description = this.dataAddExcel[0][i]["Lý do"] + "";
            } else {
                a.Description = "";
            }
            arrData.push(a);
        }
        taBusinessRegistrationApi.AddBusinessRegistrationFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }

            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            //// console.log(res)
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                //// console.log("Import success")
                this.$saveSuccess();
                this.loadData();
            } else {
                this.importErrorMessage = this.$t('ImportBusinessRegisterErrorMessage') + res.data.toString() + " " + this.$t('Row');
                //// console.log("Import error, show popup import error file download")
                this.showOrHideImportError(true);
                this.loadData();
            }
        });

    }
    
    downloadFile(filePath) {
        var link = document.createElement('a');
        link.href = filePath;
        link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
        link.click();
    }

    AddOrDeleteFromExcel(x) {
        if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            this.isDeleteFromExcel = false;
            this.showDialogExcel = false;
            this.fileName = '';
        } else if (x === 'add') {
            this.isAddFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        }
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
    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
    }
    processFile(e) {
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
                        if(sheet == "Data"){
                            var range = XLSX.utils.decode_range(workbook.Sheets[sheet]['!ref']);
                            range.s.r = 1; // <-- zero-indexed, so setting to 1 will skip row 0
                            workbook.Sheets[sheet]['!ref'] = XLSX.utils.encode_range(range);
                        }
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // var arr = Array.from(rowObject)
                        // arrData.push(arr)
                        arrData.push(Array.from(rowObject));
                    });
                };
                this.dataAddExcel = arrData;
                fileReader.readAsBinaryString(file);
                (document.getElementById("fileUploadBusiness") as any).value = null;
            }
        }
    }

    focus(x) {
        var theField = eval('this.$refs.' + x)
        theField.focus()
    }

    //#endregion

}
