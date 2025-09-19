import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { parkingLotAccessedApi, ParkingLotAccessedModel, ParkingLotAccessedRequest } from '@/$api/gc-parking-lot-accessed-api';
import { isNullOrUndefined } from 'util';
import * as XLSX from 'xlsx';
import { parkingLotsApi } from "@/$api/gc-parking-lot-api";
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';

@Component({
    name: "parking-lot-accessed",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent, SelectTreeComponent, SelectDepartmentTreeComponent }
})
export default class ParkingLotAccessed extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;

    filterAccessType = [1];
    filterParkingLotIndex: any = [];
    filterFromDate: any = null;
    filterToDate: any = null;
    filterString: any = null;
    filterFormDepartmentIndex: any = [];
    filterFormEmployeeATID: any = [];

    listAccessType = [
        {Index: 1, Name: this.$t('Employee')},
        {Index: 2, Name: this.$t('Customer')}
    ]
    listAllEmployee: any = [];
    listAllEmployeeFilter: any = [];
    listAllCustomer: any = [];
    listAllCustomerFilter: any = [];

    formExcel = {};
    fileName = '';
    importErrorMessage = '';
    dataProcessedExcel = [];
    dataAddExcel = [];
    listExcelFunction = ['AddExcel'];
    showDialogExcel = false;

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

    showDialogImportError = false;
    isAddFromExcel = false;
    isDeleteFromExcel = false;

    parkingLotAccessedModel: ParkingLotAccessedModel = {
        ParkingLotIndex: null,
        EmployeeATID: "",
        EmployeeATIDs: [],
        CustomerIndex: "",
        AccessType: 1,
        FromDate: new Date(),
        ToDate: null,
        Description: '',
        // AreaGroupParentIndex: null
    };
    listParkingLot = [];

    page = 1;

    masterEmployeeFilter = [];

    async beforeMount() {
        // Put this.CreateColumns() on top to make it excute first, cause beforeMount is async so if put it at the end
        // sometime it will finish after mounted finish -> cannot set columns config
        this.CreateColumns();
        this.CreateRules();
        this.Reset();
        await this.getParkingLot();
        await this.getAllEmployee();
        await this.getAllCustomer();
        await this.getDepartmentTree();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
    }
    CreateRules() {
        this.rules = {
            ParkingLotIndex: [
                {
                    required: true,
                    message: this.$t('PleaseInputParkingLot'),
                    trigger: 'change',
                },
            ],
            EmployeeATIDs: [
                {
                    required: true,
                    message: this.parkingLotAccessedModel.AccessType == 1 ? this.$t('PleaseSelectEmployee') : this.$t('PleaseSelectCustomer'),
                    trigger: 'change',
                },
                {
                    trigger: 'change',
                    message: this.parkingLotAccessedModel.AccessType == 1 ? this.$t('PleaseSelectEmployee') : this.$t('PleaseSelectCustomer'),
                    validator: (rule, value: string, callback) => {
                        if (!this.parkingLotAccessedModel.EmployeeATIDs 
                            || (this.parkingLotAccessedModel.EmployeeATIDs 
                            && this.parkingLotAccessedModel.EmployeeATIDs.length < 1)) {
                            // console.log("")
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ],
            // CustomerIDs: [
            //     {
            //         required: true,
            //         message: this.$t('PleaseSelectCustomer'),
            //         trigger: 'change',
            //     },
            //     {
            //         trigger: 'change',
            //         message: this.$t('PleaseSelectCustomer'),
            //         validator: (rule, value: string, callback) => {
            //             if ((!this.parkingLotAccessedModel.EmployeeATIDs || (this.parkingLotAccessedModel.EmployeeATIDs 
            //                 && this.parkingLotAccessedModel.EmployeeATIDs.length < 1)) 
            //             && this.parkingLotAccessedModel.AccessType != 1) {
            //                 // console.log("")
            //                 callback(new Error());
            //             }
            //             callback();
            //         },
            //     },
            // ],
            FromDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectFromDate'),
                    trigger: 'change',
                },
				{
                    trigger: 'change',
					message: this.$t('FromDateCannotLargerToDate'),
					validator: (rule, value: string, callback) => {
                        this.parkingLotAccessedModel.FromDate = new Date(this.parkingLotAccessedModel.FromDate);
                        this.parkingLotAccessedModel.FromDate.setHours(0);
                        this.parkingLotAccessedModel.FromDate.setMinutes(0);
                        this.parkingLotAccessedModel.FromDate.setSeconds(0);
                        this.parkingLotAccessedModel.FromDate.setMilliseconds(0);
                        if(this.parkingLotAccessedModel.ToDate){
                            this.parkingLotAccessedModel.ToDate = new Date(this.parkingLotAccessedModel.ToDate);
                            this.parkingLotAccessedModel.ToDate.setHours(0);
                            this.parkingLotAccessedModel.ToDate.setMinutes(0);
                            this.parkingLotAccessedModel.ToDate.setSeconds(0);
                            this.parkingLotAccessedModel.ToDate.setMilliseconds(0);
                        }
						if (this.parkingLotAccessedModel.FromDate 
							&& this.parkingLotAccessedModel.ToDate 
							&& this.parkingLotAccessedModel.FromDate > this.parkingLotAccessedModel.ToDate) {
							callback(new Error());
						} else {
							callback();
						}
					},
				}
			],
			ToDate: [
				{
                    trigger: 'change',
					message: this.$t('FromDateCannotLargerToDate'),
					validator: (rule, value: string, callback) => {
                        this.parkingLotAccessedModel.FromDate = new Date(this.parkingLotAccessedModel.FromDate);
                        this.parkingLotAccessedModel.FromDate.setHours(0);
                        this.parkingLotAccessedModel.FromDate.setMinutes(0);
                        this.parkingLotAccessedModel.FromDate.setSeconds(0);
                        this.parkingLotAccessedModel.FromDate.setMilliseconds(0);
                        if(this.parkingLotAccessedModel.ToDate){
                            this.parkingLotAccessedModel.ToDate = new Date(this.parkingLotAccessedModel.ToDate);
                            this.parkingLotAccessedModel.ToDate.setHours(0);
                            this.parkingLotAccessedModel.ToDate.setMinutes(0);
                            this.parkingLotAccessedModel.ToDate.setSeconds(0);
                            this.parkingLotAccessedModel.ToDate.setMilliseconds(0);
                        }
						if (this.parkingLotAccessedModel.FromDate 
							&& this.parkingLotAccessedModel.ToDate 
							&& this.parkingLotAccessedModel.FromDate > this.parkingLotAccessedModel.ToDate) {
							callback(new Error());
						} else {
							callback();
						}
					},
				}
			],
        }
    }
    changeUserType(){
        (this.$refs.parkingLotAccessedModel as any).clearValidate();
        setTimeout(() => {
            this.parkingLotAccessedModel = {
                ParkingLotIndex: this.parkingLotAccessedModel.ParkingLotIndex,
                EmployeeATID: "",
                EmployeeATIDs: [],
                CustomerIndex: "",
                AccessType: this.parkingLotAccessedModel.AccessType,
                FromDate: this.parkingLotAccessedModel.FromDate,
                ToDate: this.parkingLotAccessedModel.ToDate,
                Description: '',
                // AreaGroupParentIndex: null
            };
        }, 100);
        // this.checkRuleForForm();
        this.rules.EmployeeATIDs = [
            {
                required: true,
                message: this.parkingLotAccessedModel.AccessType == 1 ? this.$t('PleaseSelectEmployee') : this.$t('PleaseSelectCustomer'),
                trigger: 'change',
            },
            {
                trigger: 'change',
                message: this.parkingLotAccessedModel.AccessType == 1 ? this.$t('PleaseSelectEmployee') : this.$t('PleaseSelectCustomer'),
                validator: (rule, value: string, callback) => {
                    if (!this.parkingLotAccessedModel.EmployeeATIDs 
                        || (this.parkingLotAccessedModel.EmployeeATIDs 
                        && this.parkingLotAccessedModel.EmployeeATIDs.length < 1)) {
                        // console.log("")
                        callback(new Error());
                    }
                    callback();
                },
            },
        ];
    }
    
    // checkRuleForForm() {
    //     if(this.parkingLotAccessedModel.AccessType == 1){
    //         this.rules.EmployeeATIDs = [
    //             {
    //                 required: true,
    //                 message: this.$t('PleaseSelectEmployee'),
    //                 trigger: 'change',
    //             },
    //             {
    //                 trigger: 'change',
    //                 message: this.$t('PleaseSelectEmployee'),
    //                 validator: (rule, value: string, callback) => {
    //                     if ((!this.parkingLotAccessedModel.EmployeeATIDs 
    //                         || (this.parkingLotAccessedModel.EmployeeATIDs 
    //                         && this.parkingLotAccessedModel.EmployeeATIDs.length < 1))) {
    //                         // console.log("")
    //                         callback(new Error());
    //                     }
    //                     callback();
    //                 },
    //             },
    //         ];
    //         this.rules.CustomerIDs = [
                
    //         ];
    //     }else{
    //         this.rules.CustomerIDs = [
    //             {
    //                 required: true,
    //                 message: this.$t('PleaseSelectCustomer'),
    //                 trigger: 'change',
    //             },
    //             {
    //                 trigger: 'change',
    //                 message: this.$t('PleaseSelectCustomer'),
    //                 validator: (rule, value: string, callback) => {
    //                     if ((!this.parkingLotAccessedModel.EmployeeATIDs || (this.parkingLotAccessedModel.EmployeeATIDs 
    //                         && this.parkingLotAccessedModel.EmployeeATIDs.length < 1))) {
    //                         // console.log("")
    //                         callback(new Error());
    //                     }
    //                     callback();
    //                 },
    //             },
    //         ];
    //         this.rules.EmployeeATIDs = [
                
    //         ];
    //     }
    //     console.log(this.rules)
    // }

    CreateColumns() {
        this.columns = [
            {
                prop: 'EmployeeATID',
                label: 'MCC',
                minWidth: 150,
                display: true
            },
            {
                prop: 'EmployeeCode',
                label: 'EmployeeCode',
                minWidth: 150,
                display: true
            },
            {
                prop: 'FullName',
                label: 'FullName',
                minWidth: 150,
                display: true
            },
            {
                prop: 'DepartmentName',
                label: 'DepartmentName',
                minWidth: 150,
                display: true
            },
            {
                prop: 'ParkingLotName',
                label: 'ParkingLotName',
                minWidth: 150,
                display: true
            },
            {
                prop: 'FromDateString',
                label: 'FromDateString',
                minWidth: 150,
                display: true
            },
            {
                prop: 'ToDateString',
                label: 'ToDateString',
                minWidth: 220,
                display: true
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 200,
                display: true
            }
        ];
    }

    Insert() {
        this.showDialog = true;
        // this.checkRuleForForm();
        if (this.isEdit == true) {
            this.Reset();
        }
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
            if(this.parkingLotAccessedModel.AccessType == 1){
                this.parkingLotAccessedModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				    .filter(x => (this.listAllEmployeeFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }else{
                this.parkingLotAccessedModel.EmployeeATIDs = Misc.cloneData(this.masterEmployeeFilter)
				    .filter(x => (this.listAllCustomerFilter as any).some(y => y.Index == x))?.map(x => x.toString()) ?? [];
            }
		}
        this.isEdit = false;
    }
    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            // this.checkRuleForForm();
            this.parkingLotAccessedModel = obj[0];
            this.parkingLotAccessedModel.EmployeeATIDs = [this.parkingLotAccessedModel.EmployeeATID];
            if(this.parkingLotAccessedModel.AccessType == 0){
                this.parkingLotAccessedModel.AccessType = 1;
            }else{
                this.parkingLotAccessedModel.AccessType = 2;
            }
            if((this.rowsObj as any)[0].DepartmentIndex){
                this.filterFormDepartmentIndex = [];
                this.filterFormDepartmentIndex.push((this.rowsObj as any)[0].DepartmentIndex);
            }
            else
            {
                this.filterFormDepartmentIndex = [];
                this.filterFormDepartmentIndex.push(0);
            }
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async Delete() {
        // console.log(this.rowsObj)
        const listIndex: Array<any> = this.rowsObj.map((item: any) => {
            return item.Index;
        });
        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            await this.$confirmDelete().then(async () => {
                await parkingLotAccessedApi.DeleteParkingLotsAccessed(listIndex).then((res: any) => {
                    if (res.status && res.status == 200) {
                        this.$deleteSuccess();
                    }
                    (this.$refs.areaGroupTable as any).getTableData(this.page, null, null);
                })
                .catch(() => { })
                .finally(() => {
                    (this.$refs.parkingLotAccessedTable as any).getTableData(this.page);
                });
            });
        }
    }
    async ConfirmClick() {
        (this.$refs.parkingLotAccessedModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            this.parkingLotAccessedModel.EmployeeATID = this.parkingLotAccessedModel.EmployeeATIDs[0].toString();
            this.parkingLotAccessedModel.CustomerIndex = "empty";
            const submitData = Misc.cloneData(this.parkingLotAccessedModel);
            if(submitData.AccessType == 1){
                submitData.AccessType = 0;
            }else{
                submitData.AccessType = 1;
            }
            submitData.FromDate = new Date(
                moment(submitData.FromDate).format("YYYY-MM-DD")
            );
            submitData.ToDate = new Date(
                moment(submitData.ToDate).format("YYYY-MM-DD")
            );
            if (this.isEdit == false) {
                await parkingLotAccessedApi.AddParkingLotAccessed(submitData).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();

                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await parkingLotAccessedApi.UpdateParkingLotAccessed(submitData).then((res: any) => {
                    if (res.status && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.parkingLotAccessedTable as any).getTableData(this.page);
        });
    }
    async getParkingLot() {
        await parkingLotsApi.GetParkingLotsAll().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.listParkingLot.push({
                        Index: parseInt(arrGroupDevice[i].Index),
                        Name: arrGroupDevice[i].Name
                    });
                }
                // console.log(this.listParkingLot)
            }
        });
    }
    async getDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }
    async getAllEmployee() {
        await hrUserApi.GetAllEmployeeCompactInfoByPermissionImprovePerformance().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
                this.listAllEmployee = data;
                this.listAllEmployeeFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
                }));
                // console.log(this.listAllEmployee)
                // console.log(this.listAllEmployeeFilter)
            }
        });
    }
    async getAllCustomer() {
        await hrCustomerInfoApi.GetAllCustomer().then((res: any) => {
            if (res.status == 200) {
                const data = res.data;
                this.listAllCustomer = data;
                this.listAllCustomerFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
                }));
                // console.log(this.listAllCustomer)
                // console.log(this.listAllCustomerFilter)
            }
        });
    }
    viewData(){
        if(this.filterFromDate){
            this.filterFromDate = new Date(
                moment(this.filterFromDate).format("YYYY-MM-DD")
            );
        }
        if(this.filterToDate){
            this.filterToDate = new Date(
                moment(this.filterToDate).format("YYYY-MM-DD")
            );
        }
        if (this.filterFromDate
            && this.filterToDate
            && this.filterFromDate > this.filterToDate){
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerToDate').toString()).toString();
            return;
        }
        this.page = 1;
        (this.$refs.parkingLotAccessedTable as any).getTableData(this.page);
    }
    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;
        const filterAccessType = [];
        if(this.filterAccessType && this.filterAccessType.length > 0){
            this.filterAccessType.forEach(element => {
                if(element == 1){
                    filterAccessType.push(0);
                }else{
                    filterAccessType.push(1);
                }
            });
        }
        const object: ParkingLotAccessedRequest = ({
            accessType: filterAccessType,
            fromDate: this.filterFromDate,
            toDate: this.filterToDate,
            parkingLotIndex: this.filterParkingLotIndex,
            filter: this.filterString?.trim(),
            page: this.page,
            pageSize: pageSize
        });
        return await parkingLotAccessedApi.GetByFilter(object).then((res) => {
            if(res.data && (res.data as any).data){
                (res.data as any).data.forEach(element => {
                    element.DepartmentName = this.$t(element.DepartmentName);
                });
            }
            return {
                data: (res.data as any).data,
                total: (res.data as any).total,
            };
        });
    }
    Cancel() {
        this.Reset();
        this.showDialog = false;
    }
    async Integrate(){
        this.isLoading = true;
        this.$alert(
            this.$t("EmployeeIntegrationInprocess").toString(),
            this.$t("Notify").toString(),
            null
        );
        parkingLotsApi.RegisterMonthCard().then((res: any) => {
            if (res.status == 200) {
                //this.$saveSuccess();
                this.isLoading = false;
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
    Reset() {
        this.parkingLotAccessedModel = {
            ParkingLotIndex: null,
			EmployeeATID: "",
            EmployeeATIDs: [],
			CustomerIndex: "",
			AccessType: 1,
			FromDate: new Date(),
			ToDate: null,
			Description: '',
            // AreaGroupParentIndex: null
        };
        this.filterFormDepartmentIndex = [];
    }

    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
    }

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
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

    onChangeDepartmentFilter(departments) {
		// console.log(departments);
		this.parkingLotAccessedModel.EmployeeATID = "";
		this.parkingLotAccessedModel.EmployeeATIDs = [];
        if(departments && departments.length > 0){
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex))).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            }));
        }else{
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : "" ))
            }));
        }
	}

    selectAllAccessTypeFilter(value) {
        this.filterAccessType = value;
    }

    selectAllParkingLotFilter(value) {
        this.filterParkingLotIndex = value;
    }

    selectAllEmployeeFilter(value) {
        this.parkingLotAccessedModel.EmployeeATIDs = value;
    }

    selectAllCustomerFilter(value) {
        this.parkingLotAccessedModel.EmployeeATIDs = value;
    }

    async UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            a.CustomerIndex = 'empty';
            if (this.dataAddExcel[0][i].hasOwnProperty('MCC (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['MCC (*)'] + '';
            } else {
                a.EmployeeATID = '';
                // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                // return;
            }
            // if (this.dataAddExcel[0][i].hasOwnProperty('Loại nhân viên (*)')) {
            //     if(this.dataAddExcel[0][i]['Loại nhân viên (*)'] == "Nhân viên"){
            //         a.AccessType = 0;
            //     }else if(this.dataAddExcel[0][i]['Loại nhân viên (*)'] == "Khách"){
            //         a.AccessType = 1;
            //     }else{
            //         a.AccessType = -1;
            //         // this.$alertSaveError(null, null, null, this.$t('EmployeeTypeInvalid').toString()).toString();
            //         // return;
            //     }
            // } else {
            //     a.AccessType = -2;
            // }
            a.AccessType = 0;
            if (this.dataAddExcel[0][i].hasOwnProperty('Nhân viên')) {
                a.FullName = this.dataAddExcel[0][i]['Nhân viên'] + '';
            } else {
                a.FullName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Nhà xe (*)')) {
                a.ParkingLot = this.dataAddExcel[0][i]['Nhà xe (*)'] + '';
            } else {
                a.ParkingLot = '';
                // this.$alertSaveError(null, null, null, this.$t('ParkingLotMayNotBeBlank').toString()).toString();
                // return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Từ ngày (*)')) {
                // a.FromDate = this.dataAddExcel[0][i]['Từ ngày (*)'] + '';
                a.FromDateString = this.dataAddExcel[0][i]['Từ ngày (*)'] + '';
            } else {
                a.FromDateString = '';
                // this.$alertSaveError(null, null, null, this.$t('FromDateRequired').toString()).toString();
                // return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Đến ngày')) {
                // a.ToDate = this.dataAddExcel[0][i]['Đến ngày'] + '';
                a.ToDateString = this.dataAddExcel[0][i]['Đến ngày'] + '';
            } else {
                // a.ToDate = '';
                a.ToDateString = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mô tả')) {
                a.Description = this.dataAddExcel[0][i]['Mô tả'] + '';
            } else {
                a.Description = '';
            }
            
            arrData.push(a);
        }

        // console.log(arrData)

        await parkingLotAccessedApi.ImportParkingLotAccessed(arrData).then((res: any) => {
            // console.log(res)
            if (res.status && res.status == 200) {
                if(res.data){
                    this.$saveSuccess();
                    (this.$refs.parkingLotAccessedTable as any).getTableData(this.page);

                }else{
                    this.importErrorMessage = this.$t('ImportParkingLotAccessedFailed').toString();
                    this.showOrHideImportError(true);
                }
            }
        })
        .finally(() => {
            this.showDialogExcel = false;
        });

        // commandApi.UploadACUserFromExcel(arrData).then((res) => {
        //     if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
        //         (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
        //     }
        //     if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
        //         (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
        //     }
        //     this.showDialogExcel = false;
        //     this.fileName = '';
        //     this.dataAddExcel = [];
        //     this.isAddFromExcel = false;
        //     if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
        //         this.$saveSuccess();
        //     } else {
        //         this.importErrorMessage = this.$t('ImportSyncACUserErrorMessage') + res.data.toString() + " " + this.$t('User');
        //         this.showOrHideImportError(true);
        //     }
        // });
    }
}