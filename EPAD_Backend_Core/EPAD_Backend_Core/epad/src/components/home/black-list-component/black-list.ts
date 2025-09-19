import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import { hrCustomerInfoApi } from '@/$api/hr-customer-info-api';
import { isNullOrUndefined } from 'util';
import * as XLSX from 'xlsx';
import { parkingLotsApi } from "@/$api/gc-parking-lot-api";
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { BlackListModel, BlackListRequest, RemoveBlackListModel, blackListApi } from "@/$api/gc-black-list-api";

@Component({
    name: "black-list",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent, SelectTreeComponent, SelectDepartmentTreeComponent }
})
export default class BlackList extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    rules = null;
    rules_Remove = null;

    filterAccessType = [1];
    filterParkingLotIndex: any = [];
    filterFromDate = new Date;
    filterToDate: any = null;
    filterString: any = null;
    filterFormDepartmentIndex: any = [];
    filterFormEmployeeATID: any = [];

    listAccessType = [
        { Index: 1, Name: this.$t('Employee') },
        { Index: 2, Name: this.$t('Customer') }
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

    blackListModel: BlackListModel = {
        Index: null,
        IsEmployeeSystem: false,
        FullName: "",
        Nric: "",
        EmployeeATID: "",
        FromDate: new Date(),
        Reason: '',
        ToDate: new Date()
        // AreaGroupParentIndex: null
    };

    removeBlackListModel: RemoveBlackListModel = {
        Index: null,
        ToDate: new Date(),
        ReasonRemoveBlackList: '',
        // AreaGroupParentIndex: null
    };
    listParkingLot = [];
    showDialogRemoveBlackList = false;
    page = 1;
    
    masterEmployeeFilter = [];

    async beforeMount() {
        // Put this.CreateColumns() on top to make it excute first, cause beforeMount is async so if put it at the end
        // sometime it will finish after mounted finish -> cannot set columns config
        this.CreateColumns();
        this.CreateRules();
        this.CreateRules_Remove();
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
            Nric: [
                {
                    required: true,
                    message: this.$t('PleaseInputNRIC'),
                    trigger: 'change',
                },
            ],

            EmployeeATID: [
                {
                    required: true,
                    message: this.$t('PleaseSelectEmployee'),
                    trigger: 'change',
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
                	message: this.$t('FromDateCannotLargerToDate'),
                	validator: (rule, value: string, callback) => {
                        this.blackListModel.FromDate = new Date(this.blackListModel.FromDate);
                        this.blackListModel.FromDate.setHours(0);
                        this.blackListModel.FromDate.setMinutes(0);
                        this.blackListModel.FromDate.setSeconds(0);
                        this.blackListModel.FromDate.setMilliseconds(0);
                        if(this.blackListModel.ToDate){
                            this.blackListModel.ToDate = new Date(this.blackListModel.ToDate);
                            this.blackListModel.ToDate.setHours(0);
                            this.blackListModel.ToDate.setMinutes(0);
                            this.blackListModel.ToDate.setSeconds(0);
                            this.blackListModel.ToDate.setMilliseconds(0);
                        }
                		if (this.blackListModel.FromDate 
                			&& this.blackListModel.ToDate 
                			&& this.blackListModel.FromDate > this.blackListModel.ToDate) {
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
                        this.blackListModel.FromDate = new Date(this.blackListModel.FromDate);
                        this.blackListModel.FromDate.setHours(0);
                        this.blackListModel.FromDate.setMinutes(0);
                        this.blackListModel.FromDate.setSeconds(0);
                        this.blackListModel.FromDate.setMilliseconds(0);
                        if(this.blackListModel.ToDate){
                            this.blackListModel.ToDate = new Date(this.blackListModel.ToDate);
                            this.blackListModel.ToDate.setHours(0);
                            this.blackListModel.ToDate.setMinutes(0);
                            this.blackListModel.ToDate.setSeconds(0);
                            this.blackListModel.ToDate.setMilliseconds(0);
                        }
            			if (this.blackListModel.FromDate 
            				&& this.blackListModel.ToDate 
            				&& this.blackListModel.FromDate > this.blackListModel.ToDate) {
            				callback(new Error());
            			} else {
            				callback();
            			}
            		},
            	}
            ],
        }
    }

    CreateRules_Remove() {
        this.rules_Remove = {
            ReasonRemoveBlackList: [
                {
                    required: true,
                    message: this.$t('PleaseInputReasonRemoveBlackList'),
                    trigger: 'change',
                },
            ],

            ToDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectToDate'),
                    trigger: 'change',
                }, ,
                {

                    trigger: 'change',
                    message: this.$t('FromDateCannotLargerToDate'),
                    validator: (rule, value: string, callback) => {
                        this.blackListModel.FromDate = new Date(this.blackListModel.FromDate);
                        this.blackListModel.FromDate.setHours(0);
                        this.blackListModel.FromDate.setMinutes(0);
                        this.blackListModel.FromDate.setSeconds(0);
                        this.blackListModel.FromDate.setMilliseconds(0);
                        if (this.removeBlackListModel.ToDate) {
                            this.removeBlackListModel.ToDate = new Date(this.removeBlackListModel.ToDate);
                            this.removeBlackListModel.ToDate.setHours(0);
                            this.removeBlackListModel.ToDate.setMinutes(0);
                            this.removeBlackListModel.ToDate.setSeconds(0);
                            this.removeBlackListModel.ToDate.setMilliseconds(0);
                        }
                        if (this.blackListModel.FromDate
                            && this.removeBlackListModel.ToDate
                            && this.blackListModel.FromDate > this.removeBlackListModel.ToDate) {
                            callback(new Error());
                        } else {
                            callback();
                        }
                    },
                }
            ],
        }
    }

    changeUser(employeeATID) {
        (this.$refs.blackListModel as any).clearValidate();
        const nRIC_Info = this.listAllEmployee.filter(x => x.EmployeeATID == employeeATID);
        this.blackListModel.Nric = nRIC_Info ? nRIC_Info.map(x => x.NRIC).toString() : "";

    }

    changeUsingEmployeeSystem() {
        (this.$refs.blackListModel as any).clearValidate();
        this.blackListModel.FullName = "";
        this.blackListModel.EmployeeATID = "";
        this.blackListModel.Nric = "";
    }

    CreateColumns() {
        this.columns = [
            {
                prop: 'EmployeeATID',
                label: 'MCC',
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
                prop: 'Nric',
                label: 'CMND/CCCD/Passport',
                minWidth: 200,
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
                prop: 'Reason',
                label: 'Reason',
                minWidth: 200,
                display: true
            },
            {
                prop: 'ReasonRemove',
                label: 'ReasonRemoveBlackList',
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
			this.blackListModel.EmployeeATID = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => (this.listAllEmployeeFilter as any).some(y => y.Index == x))?.map(x => x.toString())[0] ?? null;
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
            (this.$refs.blackListModel as any).clearValidate();
            this.blackListModel = obj[0];

        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    RemoveBlackList() {
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.removeBlackListModel.ToDate = obj[0].ToDate;
            this.removeBlackListModel.ReasonRemoveBlackList = obj[0].ReasonRemove;
            this.blackListModel.FromDate = obj[0].FromDate;
            this.showDialogRemoveBlackList = true;
            (this.$refs.removeBlackListModel as any).clearValidate();

        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    Cancel_Remove() {
        this.Reset();
        this.showDialogRemoveBlackList = false;
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
                await blackListApi.DeleteBlackList(listIndex).then((res: any) => {
                    if (res.status && res.status == 200) {
                        this.$deleteSuccess();
                    }
                    (this.$refs.blackListTable as any).getTableData(this.page, null, null);
                })
                    .catch(() => { })
                    .finally(() => {
                        (this.$refs.blackListTable as any).getTableData(this.page);
                    });
            });
        }
    }

    async ConfirmClick_Remove() {
        (this.$refs.removeBlackListModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }

            const blackListIndex = this.rowsObj.map(x => x.Index)[0];
            const submitData = Misc.cloneData(this.removeBlackListModel);
            submitData.ToDate = new Date(
                moment(submitData.ToDate).format("YYYY-MM-DD")
            );
            submitData.Index = blackListIndex;

            await blackListApi.RemoveEmployeeInBlackList(submitData).then((res: any) => {
                if (res.status && res.status === 200) {
                    this.$saveSuccess();
                }
                this.Reset();
                this.showDialogRemoveBlackList = false;
            });
            (this.$refs.blackListTable as any).getTableData(this.page);
        });
    }


    async ConfirmClick() {
        (this.$refs.blackListModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }

            const submitData = Misc.cloneData(this.blackListModel);
            submitData.FromDate = new Date(
                moment(submitData.FromDate).format("YYYY-MM-DD")
            );

            submitData.ToDate = new Date(
                moment(submitData.ToDate).format("YYYY-MM-DD")
            );
            
            if (this.isEdit == false) {
                await blackListApi.AddBlackList(submitData).then((res: any) => {
                    if (res.status != null && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            else {
                await blackListApi.UpdateBlackList(submitData).then((res: any) => {
                    if (res.status != null && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog = false;
                });
            }
            (this.$refs.blackListTable as any).getTableData(this.page);
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
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
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
                if(data && data.length > 0){
					data.forEach(element => {
						element.ContactDepartmentName = (element.ContactDepartmentName && element.ContactDepartmentName != '') 
						? this.$t(element.ContactDepartmentName).toString() : '';
					});
				}
                this.listAllCustomer = data;
                this.listAllCustomerFilter = data.map(x => ({
                    Index: x.EmployeeATID,
                    FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
                }));
                // console.log(this.listAllCustomer)
                // console.log(this.listAllCustomerFilter)
            }
        });
    }
    viewData() {
        if (this.filterFromDate) {
            this.filterFromDate = new Date(
                moment(this.filterFromDate).format("YYYY-MM-DD")
            );
        }
        if (this.filterToDate) {
            this.filterToDate = new Date(
                moment(this.filterToDate).format("YYYY-MM-DD")
            );
        }
        if (this.filterFromDate
            && this.filterToDate
            && this.filterFromDate > this.filterToDate) {
            this.$alertSaveError(null, null, null, this.$t('FromDateCannotLargerToDate').toString()).toString();
            return;
        }
        this.page = 1;
        (this.$refs.blackListTable as any).getTableData(this.page);
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page;

        const object: BlackListRequest = ({
            fromDate: this.filterFromDate,
            toDate: this.filterToDate,
            filter: this.filterString?.trim(),
            page: this.page,
            pageSize: pageSize
        });
        return await blackListApi.GetByFilter(object).then((res) => {
            if (res.data && (res.data as any).data) {
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
        this.showDialogRemoveBlackList = false;
    }
    async Integrate() {
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
        this.blackListModel = {
            Index: null,
            EmployeeATID: "",
            FullName: "",
            Nric: "",
            IsEmployeeSystem: false,
            FromDate: new Date(),
            Reason: '',
            ToDate: new Date()
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
        this.blackListModel.EmployeeATID = "";
        if (departments && departments.length > 0) {
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee.filter(x => departments.includes(x.DepartmentIndex))).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        } else {
            this.listAllEmployeeFilter = Misc.cloneData(this.listAllEmployee).map(x => ({
                Index: x.EmployeeATID,
                FullName: (x.EmployeeATID + ((x.FullName && x.FullName != "") ? (" - " + x.FullName) : ""))
            }));
        }
    }

    selectAllAccessTypeFilter(value) {
        this.filterAccessType = value;
    }

    selectAllParkingLotFilter(value) {
        this.filterParkingLotIndex = value;
    }


    async UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            a.CustomerIndex = 'empty';
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã người dùng (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã người dùng (*)'] + '';
            } else {
                a.EmployeeATID = '';
                // this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                // return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('CMND/CCCD/Passport (*)')) {
                a.Nric = this.dataAddExcel[0][i]['CMND/CCCD/Passport (*)'] + '';
            } else {
                a.Nric = '';
            }

            if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
                a.FullName = this.dataAddExcel[0][i]['Họ tên'] + '';
            } else {
                a.FullName = '';
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Lý do')) {
                a.Reason = this.dataAddExcel[0][i]['Lý do'] + '';
            } else {
                a.Reason = '';
            }

            arrData.push(a);
        }

        // console.log(arrData)

        await blackListApi.ImportBlackList(arrData).then((res: any) => {
            // console.log(res)
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$saveSuccess();
                (this.$refs.blackListTable as any).getTableData(this.page)
            }
            else {
                this.importErrorMessage = this.$t('ImportBlackListErrorMessage') + res.data.toString() + " " + this.$t('Row');
                this.showOrHideImportError(true);
            }
        }).finally(() => {
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