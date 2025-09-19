import { commandApi, CommandRequest } from "@/$api/command-api";
import { departmentApi } from '@/$api/department-api';
import { deviceApi } from "@/$api/device-api";
import { employeeInfoApi } from "@/$api/employee-info-api";
import { groupDeviceApi } from '@/$api/group-device-api';
import { hrClassInfoApi } from '@/$api/hr-class-info-api';
import { AddedParam, userInfoApi } from "@/$api/user-info-api";
import { userMasterApi } from "@/$api/user-master-api";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import ComponentBase from "@/mixins/application/component-mixins";
import { compareString } from "@/utils/string-utils";
import { isNullOrUndefined } from "util";
import InfiniteLoading from "vue-infinite-loading";
import { Component, Mixins } from "vue-property-decorator";
import * as XLSX from 'xlsx';
import DownloadUserMasterButton from './download-user-master-button.vue';
import DeleteUserOnMachineButton from './delete-user-on-machine-button.vue';
import { defaultCipherList } from "constants";
import { userTypeApi } from "@/$api/user-type-api";
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { groupApi } from "@/$api/ac-group-api";
import { areaApi } from "@/$api/ac-area-api";
import { doorApi } from "@/$api/ac-door-api";
import { timezoneApi } from "@/$api/ac-timezone-api";
import { acUserMasterApi } from "@/$api/ac-usermaster-api";

@Component({
    name: "ac-sync-user",
    components: {
        HeaderComponent, DataTableComponent, InfiniteLoading, DataTableFunctionComponent,
        DownloadUserMasterButton, DeleteUserOnMachineButton, SelectTreeComponent, SelectDepartmentTreeComponent
    },
})
export default class AutoSynchUserComponent extends Mixins(ComponentBase) {
    // ALL TAB =================================
    dataTree: any = [];
    columns = [];
    columnsPermissions = [];
    expandedKey = [-1];
    showMessage = false;
    filter = "";
    filterTree = "";
    loadingTree = false;
    loadingLazy = false;
    showDialogDownloadUser = false;
    showDialogDownloadUserCompare = false;
    showDialogImportError = false;
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    showAuthorizeModal = false;
    syncUser = false;
    tabClickedIndex = 0;
    usingBasicMenu: boolean = true;
    listAllDepartmentLookup = {};
    listAllDepartment = [];
    listAllClassLookup = {};
    listAllClass = [];
    listIPAddress = null;
    listAllTimeZone = null;
    listAllGroup = [];
    listAllDoor = [];
    listAllArea = [];

    activeTab = "sync";
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    departmentIds = [];
    selectedArea = [];
    selectedDoor = [];
    allAreaLst = [];
    allDoorLst = [];
    allDoorApiLst = [];
    syncHistoryOperation = [];
    selectedOperation = [];
    syncHistoryViewMode = [
        { Index: 0, Name: 'ViewAll' },
        { Index: 1, Name: 'ViewLatestData' }
    ];
    selectedViewMode = 0;

    // TAB 1 =================================
    page = 1;
    dataDB: any = [];
    dataDBCurrent: any = [];
    DBLength = 0;
    selectedRows = [];
    selectMachine = [];
    arrEmps: any = [];
    isCheckAllTable = false;
    isOnline = false;
    isOverwriteUserMaster = false;
    addedParams: Array<AddedParam> = [];
    formExcel = {};
    fileName = '';
    importErrorMessage = '';
    dataProcessedExcel = [];
    dataAddExcel = [];
    listExcelFunction = ['AutoSelectExcel', 'AddExcel'];
    showDialogExcel = false;
    maxHeight = 400;
    pageTab1 = 1;
    pageSizeTab1 = 50;
    totalTab1 = 0;
    selectedTimeZone = null;
    selectedACGroup: any = '';
    // TAB 2 =================================
    selectServiceOption: any = [];
    isSelectAllMachine = false;
    showDialogPrivilege = false;
    showDialogAuthenMode = false;
    selectUserPrivilege: any = '';
    selectAuthenMode: any = '';
    selectGroup: any = '';
    isUsingArea = true;
    selectArea: any = '';
    selectDoor: any = '';
    isChoose = true;
    listPrivileges = [
        {
            'value': '0',
            'label': this.$t("StandardPrivilege").toString()
        },
        {
            'value': '3',
            'label': this.$t("AdminPrivilege").toString(),
        },
        {
            value: '4',
            label: this.$t('RegisterPrivilege').toString(),
        }
    ];
    listAuthenMode: any = [];
    selectedGroup: any = '';
    selectedDevice: any = [];
    dataListDevice: any = [];
    listGroupDevice: any = [];
    maxHeighttransfer = 400;

    // TAB 3 ===================================
    filterCompare = '';
    pageUserInfo = 1;
    pageUserMaster = 1;
    loadingLazyUserInfo = false;
    loadingLazyUserMaster = false;
    dataDBUserMaster: any = [];
    dataDBUserInfo: any = [];
    dataDBCurrentUserMaster: any = [];
    dataDBCurrentUserInfo: any = [];
    selectedUserMasterRows = [];
    selectedUserInfoRows = [];
    selectMachineCompare = [];
    selectMachineOption: any = [];
    selectMachineCompareOption: any = [];
    isCheckAllUserMasterTable = false;
    isCheckAllUserInfoTable = false;
    baseOnCompare = "BaseOnDatabase";
    columCompare = [];
    columnsSyncHistory = [];

    countUserMaster = 0;
    countUserMasterCard = 0;
    countUserMasterPass = 0;
    countUserMasterFinger = 0;
    countUserMasterFace = 0;
    isUsingTimeZone = false;
    countUser = 0;
    countUserCard = 0;
    countUserPass = 0;
    countUserFinger = 0;
    countUserFace = 0;

    selectUserType = [1];
    selectUserTypeOption = [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Customer' },
        { value: 3, label: 'Student' },
        { value: 4, label: 'Parent' },
        { value: 5, label: 'Nanny' },
        { value: 6, label: 'Contractor' },
        { value: 7, label: 'Teacher' },
    ]
    listWorkingStatus = [0];
    selectDepartment = [];
    selectContactDepartment = [];
    selectClass = [];
    loadListTree = false;

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
    treeDataDefault = [];

    treeSyncHistory = {
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


    //rulePrivileges: any = {};


    // Function ALL TAB ==============================
    commandRequest: CommandRequest = {
        Action: "",
        ListSerial: [],
        ListUser: [],
        FromTime: null,
        ToTime: null,
        Privilege: 0,
        AuthenMode: [],
        IsOverwriteData: false,
        EmployeeType: 0,
        IsUsingTimeZone: false,
        TimeZone: [],
        Group: 0
    };

    commandRequestTz: CommandRequest = {
        Action: "",
        ListSerial: [],
        ListUser: [],
        FromTime: null,
        ToTime: null,
        Privilege: 0,
        AuthenMode: [],
        IsOverwriteData: false,
        EmployeeType: 0,
        IsUsingTimeZone: false,
        TimeZone: [],
        Group: 0
    };

    tab1Loading = false;

    handleClick() {
        if (this.activeTab != "sync") {
            const tabFunctionName = `${this.activeTab.toString()}Function`;
            const tabObj = this.$refs[tabFunctionName] as any;
            (tabObj as any).reloadConfig(this.activeTab.toString());
        }
    }

    async getAllArea() {
        areaApi.GetAllArea().then(res => {
            const { data } = res as any;
            this.allAreaLst = data;

        });
    }

    async getAllDoor() {
        doorApi.GetAllDoor().then(res => {
            const { data } = res as any;
            this.allDoorLst = data;
            this.allDoorApiLst = data;
        })
    }

    async GetACOperation() {
        await acUserMasterApi.GetACOperation().then((res: any) => {
            if (res.status == 200 && res.data) {
                this.syncHistoryOperation = res.data;

                // // console.log(this.listDormAccessMode)
            }
        });
    }

    onAreaChange(val) {
        // console.log(val);
        if (val.length == 0) {
            this.allDoorLst = [...this.allDoorApiLst];
            this.selectedDoor = [];
        } else {
            this.selectedDoor = [];
            this.allDoorLst = this.allDoorApiLst.filter(x => val.includes(x.areaId));
        }
    }

    async Search() {
        this.page = 1;
        (this.$refs.table as any).getTableData(this.page, null, null);
    }

    async getSyncHistoryData({ page, filter, sortParams, pageSize }) {
        return await acUserMasterApi
            .GetACSync(
                page,
                moment(this.fromTime).format("YYYY-MM-DD HH:mm:ss"),
                moment(this.toTime).format("YYYY-MM-DD HH:mm:ss"),
                this.filter,
                this.departmentIds,
                this.selectedDoor,
                this.selectedArea,
                pageSize,
                this.selectedViewMode,
                this.selectedOperation
            )
            .then(res => {
                const { data } = res as any;
                if (data.data && data.data.length > 0) {
                    data.data.forEach((item) => {
                        if (item.OperationString) {
                            item.OperationString = this.$t(item.OperationString).toString();
                        }
                    });
                }
                return {
                    data: data.data,
                    total: data.total
                };
            })
    }

    async beforeMount() {
        this.initColumnEmployee();
        this.initColumnPermission();
        this.mapColumnFromLocalStorage();
        this.columCompare = [
            {
                prop: "EmployeeATID",
                label: "EmployeeATID",
                minWidth: "100",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeCode",
                label: "EmployeeCode",
                minWidth: "150",
                fixed: false,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "IPAddress",
                label: "IPAddress",
                minWidth: "150",
                display: false
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: false
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "DepartmentName",
                label: "DepartmentName",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];

        this.columnsSyncHistory = [
            {
                prop: 'EmployeeATID',
                label: 'EmployeeATID',
                minWidth: '80',
                fixed: true,
                display: true
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
                prop: 'DepartmentName',
                label: 'Department',
                width: '220',
                display: true
            },
            {
                prop: 'TimezoneName',
                label: 'Timezone',
                minWidth: '180',
                display: true
            },

            {
                prop: 'AreaName',
                label: 'Area',
                minWidth: '180',
                display: true
            },
            {
                prop: 'DoorName',
                label: 'Door',
                minWidth: '180',
                display: true
            },
            {
                prop: 'OperationString',
                label: 'Operation',
                minWidth: '180',
                display: true
            },
            {
                prop: 'UpdatedDateString',
                label: 'Time',
                minWidth: '180',
                display: true
            }
        ];

        await this.LoadDepartmentTree();
        // this.LoadDepartment();
        this.LoadClassInfo();
        this.LoadDeviceCombobox();
        this.MaxHeightTable();
        this.LoadGroupDevice();
        this.LoadEmployeeType();
        this.getAuthenMode();
        this.LoadTimezone();
        this.LoadGroup();
        this.LoadDoor();
        this.LoadArea();
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });
        this.loadImportExcel();
        await this.GetACOperation();
        await this.getAllArea();
        await this.getAllDoor();
    }
    mapColumnFromLocalStorage() {
        const listColumnJson = localStorage.getItem('DongBoNguoiDung');
        if (listColumnJson !== null) {
            this.columns = JSON.parse(listColumnJson);
        }
    }

    loadImportExcel() {
        timezoneApi.ExportInfoSyncAcUser().then(res => {
            console.log(res);
        });
    }

    onChangeOnline() {
        if (this.isOnline) {
            this.selectMachineOption = this.selectMachineOption.filter(x => x.status);
        } else {
            this.LoadDeviceCombobox();
        }
    }
    onChangeUsingTimeZone() {

    }

    getAllGroup() {

    }

    ChangeViewColumn(data) {
        console.log(data);
        // localStorage.setItem('DongBoNguoiDung', JSON.stringify(this.columns));
        // this.Tab1View();
        const lstColumns = [];
        data.map(col => {
            lstColumns.push({
                prop: col.prop,
                label: col.label,
                minWidth: col.minWidth,
                fixed: col.fixed,
                display: col.display
            })
        })
        setTimeout(xcol => this.AssignSortale(lstColumns), 1000);
        // this.columns = lstColumns;
    }

    LoadEmployeeType() {
        userTypeApi.GelAllUserType().then(res => {
            const inActiveLst = res.data.filter(x => x.Status != 'Active').map(x => x.UserTypeId);
            if (inActiveLst.length > 0) {
                this.selectUserTypeOption = this.selectUserTypeOption.filter(x => !inActiveLst.includes(x.value));
                console.log(this.selectUserTypeOption);
            }
        });
    }
    AssignSortale(table) {
        //this.columns = table;
    }

    MaxHeightTable() {
        this.maxHeight = window.innerHeight - 262;
        this.maxHeighttransfer = window.innerHeight - 222;
    }


    initColumn() {
        this.columns = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "UserCode",
                minWidth: "150",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeCode",
                label: "EmployeeCode",
                minWidth: "150",
                fixed: false,
                display: true
            },
            {
                prop: "NRIC",
                label: "NRIC",
                minWidth: "150",
                fixed: false,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: true
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "DepartmentName",
                label: "DepartmentName",
                minWidth: "200",
                display: true
            },
            {
                prop: "Status",
                label: "Status",
                minWidth: "200",
                display: true,
                formatter: (params) => `${params.Status === 'IsWorking' ? this.$t('IsWorking').toString() : this.$t('StoppedWork').toString()}`
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "ContactPerson",
                label: "ContactPerson",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];
    }
    
    initColumnEmployee() {
        this.columns = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "EmployeeATID",
                minWidth: "100",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeCode",
                label: "EmployeeCode",
                minWidth: "150",
                fixed: false,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: false
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "DepartmentName",
                label: "DepartmentName",
                minWidth: "200",
                display: true
            },
            {
                prop: "Status",
                label: "Status",
                minWidth: "200",
                display: true,
                formatter: (params) => `${params.Status === 'IsWorking' ? this.$t('IsWorking').toString() : this.$t('StoppedWork').toString()}`
            },
            {
                prop: "Note",
                label: "Note",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];
    }

    initColumnCustomer() {
        this.columns = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "UserCode",
                minWidth: "120",
                fixed: true,
                display: true
            },
            {
                prop: "NRIC",
                label: "NRIC",
                minWidth: "150",
                fixed: false,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: false
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "ContactPerson",
                label: "ContactPerson",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];
    }

    initColumnParent() {
        this.columns = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "UserCode",
                minWidth: "120",
                fixed: true,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: false
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "Student",
                label: "Student",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];
    }

    initColumnStudent() {
        this.columns = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "UserCode",
                minWidth: "120",
                fixed: true,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "SerialNumber",
                label: "SerialNumber",
                minWidth: "150",
                display: false
            },
            {
                prop: "NameOnMachine",
                label: "NameOnMachine",
                minWidth: "200",
                display: true
            },
            {
                prop: "AliasName",
                label: "AliasName",
                minWidth: "200",
                display: false
            },
            {
                prop: "ClassName",
                label: "Class",
                minWidth: "200",
                display: true
            },
            {
                prop: "CardNumber",
                label: "CardNumber",
                minWidth: "200",
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "Password",
                label: "Password",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger1",
                label: "Finger1",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger2",
                label: "Finger2",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger3",
                label: "Finger3",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger4",
                label: "Finger4",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger5",
                label: "Finger5",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger6",
                label: "Finger6",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger7",
                label: "Finger7",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger8",
                label: "Finger8",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger9",
                label: "Finger9",
                minWidth: "200",
                display: true
            },
            {
                prop: "Finger10",
                label: "Finger10",
                minWidth: "200",
                display: true
            },
            {
                prop: "FaceTemplate",
                label: "FaceTemplate",
                minWidth: "200",
                display: true
            },
        ];
    }

    initColumnPermission() {
        this.columnsPermissions = [
            {
                prop: "Index",
                label: "Index",
                minWidth: "50",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeATID",
                label: "UserCode",
                minWidth: "120",
                fixed: true,
                display: true
            },
            {
                prop: "EmployeeCode",
                label: "EmployeeCode",
                minWidth: "120",
                fixed: true,
                display: true
            },
            {
                prop: "FullName",
                label: "FullName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "DepartmentName",
                label: "DepartmentName",
                minWidth: "180",
                fixed: false,
                display: true
            },
            {
                prop: "PrivilegeName",
                label: "Privilege",
                minWidth: "200",
                display: true
            },
            {
                prop: "UpdatedDate",
                label: "UpdatedDate",
                dataType: "date",
                minWidth: "200",
                display: true
            },
        ];
    }
    async mounted() {
        this.DBLength = 0;

        // this.MachineLength = this.dataMachine.length;
        this.Tab1View();
    }

    beforeUpdate() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
        })

        if (this.selectMachine.indexOf("SelectAll") !== -1) {
            this.selectMachine = [...this.selectMachineOption].map(
                (item) => item.value
            );
        }
        if (this.selectMachine.indexOf("DeselectAll") !== -1) {
            this.selectMachine = [];
        }
        if (this.selectAuthenMode.indexOf("SelectAll") !== -1) {
            this.selectAuthenMode = [...this.listAuthenMode].map(
                (item) => item.value
            );
        }
        if (this.selectAuthenMode.indexOf("DeselectAll") !== -1) {
            this.selectAuthenMode = [];
        }
        if (this.selectMachineCompare.indexOf("SelectAll") !== -1) {
            this.selectMachineCompare = [...this.selectMachineCompareOption].map(
                (item) => item.value
            );
        }

        if (this.selectMachineCompare.indexOf("DeselectAll") !== -1) {
            this.selectMachineCompare = [];
        }

        if (this.selectDepartment.indexOf("DeselectAll") !== -1) {
            this.selectDepartment = [];
        }
        if (this.selectDepartment.indexOf("SelectAll") !== -1) {
            this.selectDepartment = [...this.listAllDepartment].map(
                (item) => item.value
            );
        }

        if (this.selectClass.indexOf("DeselectAll") !== -1) {
            this.selectClass = [];
        }
        if (this.selectClass.indexOf("SelectAll") !== -1) {
            this.selectClass = [...this.listAllClass].map(
                (item) => item.Index
            );
        }

    }


    onChangePageSizeTab1(pageSize) {
        this.pageSizeTab1 = pageSize;
        this.Tab1View();
    }
    Tab1ViewChangePage(page) {
        this.pageTab1 = page;
        this.dataDBCurrent = this.dataDB.slice((this.pageTab1 - 1) * this.pageSizeTab1, this.pageTab1 * this.pageSizeTab1);
    }
    onInfinite() {
        if (
            this.dataDBCurrent.length > this.page * 40 - 1 &&
            this.dataDBCurrent.length !== 0
        ) {
            this.page = this.page + 1;
            this.loadingLazy = true;
            setTimeout(() => {
                const arrToAddTable = this.dataDB.slice(
                    (this.page - 1) * 40,
                    this.page * 40
                );
                if (isNullOrUndefined(this.dataDBCurrent)) {
                    this.dataDBCurrent = [];
                }
                this.loadingLazy = false;
                var arrIndex = [];

                arrToAddTable.forEach((item) => {
                    this.dataDBCurrent.push(item);
                    arrIndex.push(this.dataDBCurrent.length - 1);
                });
                if (this.isCheckAllTable === true) {
                    arrIndex.forEach((item) => {
                        (this.$refs.multipleTable as any).toggleRowSelection(
                            this.dataDBCurrent[item],
                            true
                        );
                    });
                }

            }, 250);
        }
    }

    async LoadDeviceCombobox() {
        return await deviceApi.GetDeviceAll().then((res) => {
            this.selectMachineOption = (res.data as any[]).sort((a, b) => compareString(a.label, b.label));
            this.selectMachineCompareOption = [...this.selectMachineOption];
        });
    }

    async LoadDepartmentTree() {
        this.loadListTree = true;
        let userType = this.selectUserType.join(",");
        await departmentApi.GetDepartmentTreeEmployeeScreen(userType).then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
                this.treeSyncHistory.treeData = res.data;
            }
        });
        this.loadListTree = false;
    }

    async LoadDepartment() {
        return await departmentApi.GetDepartment().then((res) => {
            const { data } = res as any;
            for (let i = 0; i < data.length; i++) {
                data[i].value = parseInt(data[i].value);
            }
            this.listAllDepartment = data;
        });
    }

    // async LoadTimeZone(){
    //     return await departmentApi.GetAllTimeZone().then((res) => {
    //         const { data } = res as any;
    //         this.listAllTimeZone = data;
    //     })
    // }

    async LoadArea() {
        return await areaApi.GetAllArea().then((res) => {
            const { data } = res as any;
            this.listAllArea = data;
        })
    }

    async LoadGroup() {
        return await groupApi.GetAllGroup().then((res) => {
            const { data } = res as any;
            this.listAllGroup = data;
        })
    }

    async LoadDoor() {
        return await doorApi.GetAllDoor().then((res) => {
            const { data } = res as any;
            this.listAllDoor = data;
        })
    }


    async LoadClassInfo() {
        return await hrClassInfoApi.GetAllHRClassInfo().then((res) => {
            const { data } = res as any;
            this.listAllClass = data;
        });
    }

    getIconClass(type, gender) {
        switch (type) {
            case "Company":
                return "el-icon-office-building";
            case "Department":
                return "el-icon-s-home";
            case "Employee":
                if (isNullOrUndefined(gender) || gender === "Other") {
                    return "el-icon-s-custom employee-other";
                } else if (gender === "Male") {
                    return "el-icon-s-custom employee-male";
                } else {
                    return "el-icon-s-custom employee-female";
                }
        }
    }

    tableRowClassName({ row, rowIndex }) {
        if (rowIndex % 2 == 0) {
            return "warning-row";
        } else {
            return "success-row";
        }
    }

    handleTabClick(tab, event) {
        this.tabClickedIndex = tab.index;
    }

    loadingEffect(x) {
        const loading = this.$loading({
            lock: true,
            text: "Loading",
            spinner: "el-icon-loading",
            background: "rgba(0, 0, 0, 0.7)",
        });
        setTimeout(() => {
            loading.close();
        }, x);
    }

    async cancelDialog() {
        this.showDialogAuthenMode = false;
        this.showOrHideDialogDownloadUser(false);
        this.showOrHideDialogDownloadUserCompare(false);
    }

    async resetControl() {
        this.selectDepartment = [];
        this.selectClass = [];
        this.dataDB = [];
        this.dataDBCurrent = [];
        this.listWorkingStatus = [];
    }

    onChangeUserTypeSelect() {
        if(this.selectUserType.includes(-1)){
            this.selectUserType = this.selectUserTypeOption.map(x => x.value);
        }else if(this.selectUserType.includes(-2)){
            this.selectUserType = [];
        }
        this.LoadDepartmentTree();
        this.resetControl();
        this.initColumn();
        // switch (this.selectUserType) {
        //     case 1:
        //         this.initColumnEmployee();
        //         break;
        //     default:
        //         this.initColumnCustomer();
        //         break;
        //     // case 3:
        //     //     this.initColumnParent();
        //     //     break;
        //     // case 4:
        //     //     this.initColumnStudent();
        //     //     break;
        //     // default:
        //     //     this.initColumnEmployee();
        //     //     break;

        // }
        this.Tab1View();
    }

    UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Timezone (*)')) {
                a.Timezone = this.dataAddExcel[0][i]['Timezone (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('TimezoneMayNotBeBlank').toString()).toString();
                return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Khu vực kiểm soát')) {
                a.AreaName = this.dataAddExcel[0][i]['Khu vực kiểm soát'] + '';
            } else {
                a.AreaName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Cửa kiểm soát')) {
                a.DoorName = this.dataAddExcel[0][i]['Cửa kiểm soát'] + '';
            } else {
                a.DoorName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Ghi đè vào dữ liệu gốc') && (this.dataAddExcel[0][i]['Ghi đè vào dữ liệu gốc'] == 'x' || this.dataAddExcel[0][i]['Ghi đè vào dữ liệu gốc'] == 'X')) {
                a.isOverwriteUserMaster = true;
            } else {
                a.isOverwriteUserMaster = false;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
            } else {
                a.EmployeeCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên nhân viên')) {
                a.EmployeeName = this.dataAddExcel[0][i]['Tên nhân viên'] + '';
            } else {
                a.EmployeeName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban')) {
                a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban'] + '';
            } else {
                a.DepartmentName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Đồng bộ thông tin nhân viên lên thiết bị') && (this.dataAddExcel[0][i]['Đồng bộ thông tin nhân viên lên thiết bị'] == 'x' || this.dataAddExcel[0][i]['Đồng bộ thông tin nhân viên lên thiết bị'] == 'X')) {
                a.IsIntegrateToMachine = true;
            } else {
                a.IsIntegrateToMachine = false;
            }
            arrData.push(a);
        }

        commandApi.UploadACUserFromExcel(arrData).then((res) => {
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
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {

                this.selectArea = [];
                this.selectDoor = [];
                this.isUsingArea = false;
                this.InsertToMachine1();

                this.$saveSuccess();
            } else {
                this.importErrorMessage = this.$t('ImportSyncACUserErrorMessage') + res.data.toString() + " " + this.$t('User');
                this.showOrHideImportError(true);
            }
        });

    }

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }


    // Function TAB 1 =======================================
    async showOrHideDialogDownloadUser(showOrHide) {
        if (showOrHide) {
            this.isOverwriteUserMaster = false;
            this.commandRequest.ListSerial = [...this.selectMachine];
            if ([...this.commandRequest.ListSerial].length == 0) {
                this.$alert(
                    this.$t("PleaseSelectMachine").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" }
                );
                return false;
            }
        }


        this.showDialogDownloadUser = showOrHide;
    }

    async DownloadUserMaster() {

        this.commandRequest.Action = "Download user on machine";

        var listEmployeeATID;
        if (this.isCheckAllTable === true) {
            listEmployeeATID = this.dataDB.map((e) => e.EmployeeATID);
            listEmployeeATID.filter((e) => {
                return !this.commandRequest.ListUser.includes(e);
            });
        } else {
            listEmployeeATID = this.selectedRows.map((e) => e.EmployeeATID);
            listEmployeeATID.filter((e) => {
                return !this.commandRequest.ListUser.includes(e);
            });
        }
        this.commandRequest.IsOverwriteData = this.isOverwriteUserMaster;
        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectMachine];

        if ([...this.commandRequest.ListSerial].length == 0) {
            this.$alert(
                this.$t("PleaseSelectMachine").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return false;
        }
        this.showOrHideDialogDownloadUser(false);

        return await commandApi
            .DownloadAllUserMaster(this.commandRequest)
            .then((res) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$saveSuccess();
                }
            });


        //return await commandApi
        //    .DownloadAllUser(this.commandRequest)
        //    .then((res) => {
        //        if (!isNullOrUndefined(res.status) && res.status === 200) {
        //            this.$saveSuccess();
        //        }
        //    });

    }
    handleWorkingInfoChange(workingInfo: any) {
        this.listWorkingStatus = workingInfo;
        //this.getEmployees();
    }
    selectedEmployeeATIDs: string[] = [];
    handleSelectionChange(obj) {
        this.selectedRows = obj;
        this.DBLength = this.selectedRows.length;
        this.SelectAllTable(obj);
        if (this.isCheckAllTable === true) {
            this.selectedEmployeeATIDs = this.dataDB.map(
                (e) => e.EmployeeATID
            );
        } else {
            this.selectedEmployeeATIDs = this.selectedRows.map(
                (e) => e.EmployeeATID
            );
        }
    }

    async Filter() {
        const key = (this.$refs.tree as any)
            .getCheckedNodes()
            .filter((e) => e.Type == 'Employee')
            .map((e) => e.EmployeeATID);
        if (!isNullOrUndefined(key)) {
            //this.DBLength = [...key].length;
            return await userMasterApi
                .GetUserMachineInfo(key, this.filter, this.listWorkingStatus)
                .then((res) => {
                    const data = res.data as any;
                    this.page = 1;
                    this.dataDB = data;
                    this.dataDBCurrent = this.dataDB.slice(0, this.page * 40);
                });
        }
    }

    async Tab1View() {
        const departmentIndexs = [...this.selectDepartment];
        const classIndexs = [...this.selectClass];
        this.pageTab1 = 1;
        this.tab1Loading = true;
        let dataResult = [];

        if(!this.selectUserType || (this.selectUserType && this.selectUserType.length == 0)){
            this.tab1Loading = false;
            return;
        }
        
        if(this.selectUserType.includes(1)){
            this.tab1Loading = true;
            await userMasterApi.GetAllUserMachineInfo(departmentIndexs, this.filter, this.listWorkingStatus)
            .then(async (res) => {
                const data = res.data as any;
                this.page = 1;
                dataResult = dataResult.concat(res.data);
                if(this.selectUserType.some(x => x != 1)){
                    const arrayUserTypeNotEmployee = this.selectUserType.filter(x => x != 1);
                    await userMasterApi.GetAllCustomerMachineInfoByMultipleType(this.filter, arrayUserTypeNotEmployee, departmentIndexs)
                    .then((customerRes) => {
                        const data = customerRes.data as any;
                        this.page = 1;
                        dataResult = dataResult.concat(customerRes.data);
                        if(dataResult && dataResult.length > 0){
                            this.dataDB = dataResult;
                            if(this.activeTab == 'sync' && this.listWorkingStatus.length == 1){
                                if(this.listWorkingStatus[0] == 0){
                                    this.dataDB = this.dataDB.filter(x => x.Status == 'IsWorking');
                                }else{
                                    this.dataDB = this.dataDB.filter(x => x.Status == 'StoppedWork');
                                }
                            }
                            for(let i = 1; i <= this.dataDB.length; i++){
                                this.dataDB.Index = i;
                            }
                            this.dataDB.map((x, index) => x.Index = (index + 1));
                            this.dataDB.map(x => x.UpdatedDate = x.UpdatedDate ? moment(x.UpdatedDate).format("DD-MM-YYYY HH:mm:ss") : null);
                            this.totalTab1 = this.dataDB.length;
                            this.dataDBCurrent = this.dataDB.slice((this.pageTab1 - 1) * this.pageSizeTab1, this.pageTab1 * this.pageSizeTab1);
                        }
                        this.tab1Loading = false;                            
                    }).finally( () => {
   
                    });

                    // arrayUserTypeNotEmployee.forEach(async (element, index) => {
                    //     userMasterApi.GetAllCustomerMachineInfo(this.filter, element, departmentIndexs)
                    //     .then(async (customerRes) => {
                    //         const data = customerRes.data as any;
                    //         this.page = 1;
                    //         dataResult = dataResult.concat(customerRes.data);
                    //         console.log(index, dataResult)
                    //         if(index == (arrayUserTypeNotEmployee.length - 1)){
                    //             if(dataResult && dataResult.length > 0){
                    //                 this.dataDB = dataResult;
                    //                 for(let i = 1; i <= this.dataDB.length; i++){
                    //                     this.dataDB.Index = i;
                    //                 }
                    //                 this.dataDB.map(x => x.Index++);
                    //                 this.dataDB.map(x => x.UpdatedDate = x.UpdatedDate ? moment(x.UpdatedDate).format("DD-MM-YYYY HH:mm:ss") : null);
                    //                 this.totalTab1 = this.dataDB.length;
                    //                 this.dataDBCurrent = this.dataDB.slice((this.pageTab1 - 1) * this.pageSizeTab1, this.pageTab1 * this.pageSizeTab1);
                    //             }
                    //             this.tab1Loading = false;
                    //         }else{

                    //         }                                
                    //     }).finally( () => {
                            
                    //     });
                    // });
                }else{
                    if(dataResult && dataResult.length > 0){
                        this.dataDB = dataResult;
                        if(this.activeTab == 'sync' && this.listWorkingStatus.length == 1){
                            if(this.listWorkingStatus[0] == 0){
                                this.dataDB = this.dataDB.filter(x => x.Status == 'IsWorking');
                            }else{
                                this.dataDB = this.dataDB.filter(x => x.Status == 'StoppedWork');
                            }
                        }
                        for(let i = 1; i <= this.dataDB.length; i++){
                            this.dataDB.Index = i;
                        }
                        this.dataDB.map((x, index) => x.Index = (index + 1));
                        this.dataDB.map(x => x.UpdatedDate = x.UpdatedDate ? moment(x.UpdatedDate).format("DD-MM-YYYY HH:mm:ss") : null);
                        this.totalTab1 = this.dataDB.length;
                        this.dataDBCurrent = this.dataDB.slice((this.pageTab1 - 1) * this.pageSizeTab1, this.pageTab1 * this.pageSizeTab1);
                    }
                    this.tab1Loading = false;
                }
            }).finally(() => {
                
            });
        }else if(this.selectUserType.some(x => x != 1)){
            const arrayUserTypeNotEmployee = this.selectUserType.filter(x => x != 1);
            userMasterApi.GetAllCustomerMachineInfoByMultipleType(this.filter, arrayUserTypeNotEmployee, departmentIndexs)
            .then(async (customerRes) => {
                const data = customerRes.data as any;
                this.page = 1;
                dataResult = dataResult.concat(customerRes.data);
                if(dataResult && dataResult.length > 0){
                    this.dataDB = dataResult;
                    if(this.activeTab == 'sync' && this.listWorkingStatus.length == 1){
                        if(this.listWorkingStatus[0] == 0){
                            this.dataDB = this.dataDB.filter(x => x.Status == 'IsWorking');
                        }else{
                            this.dataDB = this.dataDB.filter(x => x.Status == 'StoppedWork');
                        }
                    }
                    for(let i = 1; i <= this.dataDB.length; i++){
                        this.dataDB.Index = i;
                    }
                    this.dataDB.map((x, index) => x.Index = (index + 1));
                    this.dataDB.map(x => x.UpdatedDate = x.UpdatedDate ? moment(x.UpdatedDate).format("DD-MM-YYYY HH:mm:ss") : null);
                    this.totalTab1 = this.dataDB.length;
                    this.dataDBCurrent = this.dataDB.slice((this.pageTab1 - 1) * this.pageSizeTab1, this.pageTab1 * this.pageSizeTab1);
                }
                this.tab1Loading = false;                               
            }).finally( () => {

            });
        }
    }

    async InsertToMachine1() {
        let message = '';
        const self = this;

        this.commandRequest.Action = "Upload timezone";

        this.commandRequest.ListUser = [];
        this.commandRequest.AreaLst = this.selectArea;
        this.commandRequest.DoorLst = this.selectDoor;
        this.commandRequest.IsUsingArea = this.isUsingArea;
        this.showDialogAuthenMode = false;
        try {
            await commandApi.UploadTimeZone(this.commandRequest).then((res) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$notify({
                        type: 'success',
                        title: 'Thông báo từ thiết bị',
                        dangerouslyUseHTMLString: true,
                        message: self.$t("SendRequestSuccess").toString(),
                        customClass: 'notify-content',
                        duration: 8000
                    });
                }
            }).catch((err) => {
                this.$alertSaveError(null, err);
            });

            return true;
        } catch (error) {
            this.$alertSaveError(null, error);
            return false;
        }
    }

    async InsertToMachine() {
        let message = '';
        const self = this;

        this.commandRequest.Action = "Upload user to machine";
        let listEmployeeATID: string[] = [];
        if (this.isCheckAllTable) {
            listEmployeeATID = this.dataDB.map(
                (e) => e.EmployeeATID
            );
        } else {
            listEmployeeATID = this.selectedRows.map(
                (e) => e.EmployeeATID
            );
        }
        listEmployeeATID.filter((e) => {
            return !this.commandRequest.ListUser.includes(e);
        });
        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectMachine];
        this.commandRequest.AuthenMode = [...this.selectAuthenMode];
        // this.commandRequest.EmployeeType = this.selectUserType;
        this.commandRequest.EmployeeType = 1;
        this.commandRequest.Group = this.selectedACGroup;
        this.commandRequest.AreaLst = this.selectArea;
        this.commandRequest.DoorLst = this.selectDoor;
        this.commandRequest.IsUsingArea = this.isUsingArea;
        this.commandRequest.TimeZone = [this.selectedTimeZone];
        this.commandRequest.IsOverwriteData = this.isOverwriteUserMaster;
        if ([...this.commandRequest.ListUser].length == 0) {
            this.$alert(
                this.$t("PleaserSelectUser").toString(),
                this.$t("Notify").toString(),
                {
                    type: "warning",
                }
            );
            return false;
        }
        this.showDialogAuthenMode = false;
        try {
            this.commandRequestTz.ListUser = [];
            this.commandRequestTz.AreaLst = this.selectArea;
            this.commandRequestTz.DoorLst = this.selectDoor;
            this.commandRequestTz.IsUsingArea = this.isUsingArea;
            this.commandRequestTz.TimeZone = [this.selectedTimeZone];
            this.showDialogAuthenMode = false;
            try {

                await commandApi.UploadTimeZone(this.commandRequestTz).then((res) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$notify({
                            type: 'success',
                            title: 'Thông báo từ thiết bị',
                            dangerouslyUseHTMLString: true,
                            message: self.$t("SendRequestSuccess").toString(),
                            customClass: 'notify-content',
                            duration: 8000
                        });
                    }
                }).catch((err) => {
                    this.$alertSaveError(null, err);
                });

                await commandApi.UploadACUsers(this.commandRequest).then((res) => {
                    message = `<p class="notify-content">${self.$t("SendRequestSuccess").toString()}</p>`;
                    this.$notify({
                        type: 'success',
                        title: 'Thông báo từ thiết bị',
                        dangerouslyUseHTMLString: true,
                        message: message,
                        customClass: 'notify-content',
                        duration: 8000
                    });
                    // })


                }).catch((err) => {
                    this.$alertSaveError(null, err);
                });

                if (this.syncUser) {
                    await commandApi.UploadUsers(this.commandRequest).then(async (res) => {
                        if (!isNullOrUndefined(res.status) && res.status === 200) { }

                    }).catch((err) => {
                        this.$alertSaveError(null, err);
                    });
                }

            } catch (error) {
                this.$alertSaveError(null, error);

            }


            return true;
        } catch (error) {
            this.$alertSaveError(null, error);
            return false;
        }
    }

    async DeleteOnMachine() {
        this.commandRequest.Action = "Delete user on machine";
        var listEmployeeATID
        if (this.isCheckAllTable === true) {
            listEmployeeATID = this.dataDB.map((e) => e.EmployeeATID);
        }
        else {
            listEmployeeATID = this.selectedRows.map((e) => e.EmployeeATID);
        }
        listEmployeeATID.filter((e) => {
            return !this.commandRequest.ListUser.includes(e);
        });

        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectMachine];
        // this.commandRequest.EmployeeType = this.selectUserType;

        //if ([...this.commandRequest.ListUser].length == 0) {
        //    this.$alert(
        //        this.$t("PleaserSelectUser").toString(),
        //        this.$t("Notify").toString(),
        //        {
        //            type: "warning",
        //        }
        //    );
        //    return false;
        //}

        if ([...this.commandRequest.ListSerial].length == 0) {
            this.$alert(
                this.$t("PleaseSelectMachine").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return false;
        }

        if ([...this.commandRequest.ListUser].length == 0) {
            this.$confirmDelete(
                this.$t("ConfirmDeleteAllUserOnMachine").toString(),
                this.$t("Notify").toString(),
                {
                    type: "warning",
                }).then(async () => {
                    return await commandApi
                        .DeleteAllUser(this.commandRequest)
                        .then((res) => {
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                            }
                        })
                        .catch((err) => {
                            this.$alertSaveError(null, err);
                        });
                });
        } else {
            return await commandApi
                .DeleteUserById(this.commandRequest)
                .then((res) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$saveSuccess();
                    }
                })
                .catch((err) => {
                    this.$alertSaveError(null, err);
                });
        }
    }

    SelectAllTable(val) {
        if (this.selectedRows.length === this.dataDBCurrent.length) {
            this.isCheckAllTable = true;
            this.DBLength = this.dataDB.length;
        } else {
            this.isCheckAllTable = false;
            this.DBLength = this.selectedRows.length;
        }

    }

    get allMachines() {
        if (
            this.selectMachine.length ===
            [...this.selectMachineOption].map((item) => item.value).length
        ) {
            return "DeselectAll";
        } else {
            return "SelectAll";
        }

    }

    get allDepartments() {
        if (
            this.selectDepartment.length === [...this.listAllDepartment].map((item) => item.value).length
        ) {
            return "DeselectAll";
        } else {
            return "SelectAll";
        }

    }

    get allClass() {
        if (
            this.selectClass.length === [...this.listAllClass].map((item) => item.Index).length
        ) {
            return "DeselectAll";
        } else {
            return "SelectAll";
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

    async AutoSelectFromExcel() {
        this.dataProcessedExcel = [];
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
            } else {
                a.EmployeeCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên nhân viên')) {
                a.FullName = this.dataAddExcel[0][i]['Tên nhân viên'] + '';
            } else {
                a.FullName = '';
            }

            this.dataProcessedExcel.push(a);
        }

        var listEmployeeATID = this.dataProcessedExcel.map((e) => e.EmployeeATID);
        listEmployeeATID.filter((e) => {
            return !this.commandRequest.ListUser.includes(e);
        });

        this.showDialogExcel = false;
        (<HTMLInputElement>document.getElementById('fileUpload')).value = '';

        if ([...listEmployeeATID].length > 0) {

            return await userMasterApi
                .GetUserMachineInfo(listEmployeeATID, this.filter,this.listWorkingStatus )
                .then((res) => {
                    const data = res.data as any;
                    this.page = 1;
                    this.dataDB = data;
                    this.dataDBCurrent = this.dataDB.slice(0, this.page * 40);
                    this.DBLength = [...listEmployeeATID].length;
                });

        }
        //this.checkedExcelItem();
    }

    async checkedExcelItem() {
        this.dataDBCurrent.forEach((item) => {
            (this.$refs.multipleTable as any).toggleRowSelection(
                item,
                true
            );
        })
    }

    handleCommand(command) {
        if (command === 'AutoSelectExcel') {
            this.isChoose = true;
            this.ShowOrHideDialogExcel('open');
        } else if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
            this.isChoose = false;
        }

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


    // Function TAB 2 =================================
    async getAuthenMode() {
        return await deviceApi.GetDeviceAuthenMode().then((res) => {
            var arr = [...JSON.parse(JSON.stringify(res.data))];
            var arr_1 = [];
            for (let i = 0; i < arr.length; i++) {
                arr_1.push({ value: arr[i].value, label: arr[i].label });
            }
            this.listAuthenMode = arr_1;
        });
    }

    async LoadGroupDevice() {
        return await groupDeviceApi.GetGroupDevice().then((res) => {

            this.listGroupDevice = res.data;
            this.listGroupDevice.unshift({ label: this.$t("All"), value: '0' });
        });
    }


    async LoadTimezone() {
        return await timezoneApi.GetAllTimezone().then((res) => {

            this.listAllTimeZone = res.data;
        });
    }

    async showOrHideDialogAuthenMode(showOrHide) {
        this.syncUser = false;
        this.selectAuthenMode = '';
        var listEmployeeATID;
        if (this.isCheckAllTable === true) {
            listEmployeeATID = this.dataDB.map(
                (e) => e.EmployeeATID
            );
        } else {
            listEmployeeATID = this.selectedRows.map(
                (e) => e.EmployeeATID
            );
        }
        listEmployeeATID.filter((e) => {
            return !this.commandRequest.ListUser.includes(e);
        });
        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectMachine];
        if ([...this.commandRequest.ListUser].length == 0) {
            this.$alert(
                this.$t("PleaserSelectUser").toString(),
                this.$t("Notify").toString(),
                {
                    type: "warning",
                }
            );
            return false;
        }
        // if ([...this.commandRequest.ListSerial].length == 0) {
        //     this.$alert(
        //         this.$t("PleaseSelectMachine").toString(),
        //         this.$t("Notify").toString(),
        //         { type: "warning" }
        //     );
        //     return false;
        // }

        this.showDialogAuthenMode = showOrHide;

    }
    async showDialogAuthorize() {
        this.showAuthorizeModal = true;
    }
    async cancelDialogAuthorize() {
        this.showAuthorizeModal = false;
    }
    async UpdateUserPrivilege() {
        if ([...this.selectedEmployeeATIDs].length == 0) {
            this.$alert(
                this.$t("PleaseChooseTreeEmployee").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return;
        }
        if (isNullOrUndefined(this.selectUserPrivilege) || this.selectUserPrivilege == '') {
            this.$alert(
                this.$t("SelectPrivilege").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return;
        }

        if (isNullOrUndefined(this.selectedDevice) || [...this.selectedDevice].length == 0) {
            this.$alert(
                this.$t("PleaseSelectMachine").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return;
        }

        this.addedParams = [];
        var listEmployeeATID = [...this.selectedEmployeeATIDs];

        listEmployeeATID.filter((e) => {
            return !this.commandRequest.ListUser.includes(e);
        });

        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectedDevice];
        this.commandRequest.Privilege = this.selectUserPrivilege;

        return await commandApi
            .UploadPrivilegeUsers(this.commandRequest)
            .then((res) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$saveSuccess();
                }
            })
            .catch((err) => {
                this.$alertSaveError(null, err);
            });
    }

    async handleGroupDeviceChange() {
        this.selectedDevice = [];
        this.dataListDevice = [];
        this.getDevice();
    }

    getDevice() {
        if (this.selectedGroup == '') {
            return;
        }

        groupDeviceApi.GetDeviceByGroup(this.selectedGroup).then((res: any) => {
            if (res.status == 200) {
                let arrDevice = res.data;
                arrDevice.forEach((item) => {
                    this.dataListDevice.push({
                        key: item.SerialNumber,
                        label: item.AliasName + "(" + item.IPAddress + ")"
                    });

                });
            }
        });

    }

    get allAuthenModes() {
        if (
            this.selectUserPrivilege.length ===
            [...this.listPrivileges].map((item) => item.value).length
        ) {
            return "DeselectAll";
        } else {
            return "SelectAll";
        }

    }




    // Function TAB 3 =================================
    handleSelectionUserMasterChange(obj) {
        this.selectedUserMasterRows = obj;
        this.DBLength = this.selectedUserMasterRows.length;
    }

    handleSelectionUserInfoChange(obj) {
        this.selectedUserInfoRows = obj;
        this.DBLength = this.selectedUserInfoRows.length;
    }

    selectMachineCompareChange(obj) {
        this.FilterUserInfo();
    }

    selectBaseOnCompareChange(obj) {
        if ([...this.selectMachineCompare].length > 0) {
            this.FilterUserInfo();
        }
    }

    async FilterUserCompare() {
        this.FilterUserMaster();
        this.FilterUserInfo();
    }


    async FilterUserMaster() {
        const departmentIndexes = [...this.selectDepartment];
        const classIndexes = [...this.selectClass];
        this.dataDBUserMaster = [];

        if(this.selectUserType.includes(1)){
            await userMasterApi.GetAllUserMachineInfo(departmentIndexes, this.filterCompare, this.listWorkingStatus)
            .then((res) => {
                const data = res.data as any;
                this.pageUserMaster = 1;
                this.dataDBUserMaster = this.dataDBUserMaster.concat(data);
                // this.dataDBUserMaster = data;
                // this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
            }).then(async (res) => {
                if(this.selectUserType.some(x => x != 1)){
                    const arrayUserTypeNotEmployee = this.selectUserType.filter(x => x != 1);
                    await userMasterApi.GetAllCustomerMachineInfoByMultipleType(this.filterCompare, arrayUserTypeNotEmployee, departmentIndexes)
                    .then((res) => {
                        const data = res.data as any;
                        this.pageUserMaster = 1;
                        this.dataDBUserMaster = this.dataDBUserMaster.concat(data);
                        // this.dataDBUserMaster = data;
                        // this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
                    }).then((res) => {
                        this.CountUserMaster();
                        if(this.dataDBUserMaster && this.dataDBUserMaster.length > 0){
                            this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
                        }
                    });
                }else{
                    this.CountUserMaster();
                    if(this.dataDBUserMaster && this.dataDBUserMaster.length > 0){
                        this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
                    }
                }
            });
        }else if(this.selectUserType.some(x => x != 1)){
            const arrayUserTypeNotEmployee = this.selectUserType.filter(x => x != 1);
            await userMasterApi.GetAllCustomerMachineInfoByMultipleType(this.filterCompare, arrayUserTypeNotEmployee, departmentIndexes)
            .then((res) => {
                const data = res.data as any;
                this.pageUserMaster = 1;
                this.dataDBUserMaster = this.dataDBUserMaster.concat(data);
                if(this.dataDBUserMaster && this.dataDBUserMaster.length > 0){
                    this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
                }
                // this.dataDBUserMaster = data;
                // this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
            }).then((res) => {
                this.CountUserMaster();
                if(this.dataDBUserMaster && this.dataDBUserMaster.length > 0){
                    this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
                }
            });
        }

        // switch (this.selectUserType) {
        //     case 1:
        //         return await userMasterApi
        //             .GetAllUserMachineInfo(departmentIndexes, this.filterCompare, this.listWorkingStatus)
        //             .then((res) => {
        //                 const data = res.data as any;
        //                 this.pageUserMaster = 1;
        //                 this.dataDBUserMaster = data;
        //                 this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
        //                 console.log(data);
        //             }).then((res) => {
        //                 this.CountUserMaster();
        //             });
        //     default:
        //         return await userMasterApi
        //             .GetAllCustomerMachineInfo(this.filterCompare, this.selectUserType, departmentIndexes)
        //             .then((res) => {
        //                 const data = res.data as any;
        //                 this.pageUserMaster = 1;
        //                 this.dataDBUserMaster = data;
        //                 this.dataDBCurrentUserMaster = this.dataDBUserMaster.slice(0, this.pageUserMaster * 40);
        //             }).then((res) => {
        //                 this.CountUserMaster();
        //             });

        // }
    }


    async FilterUserInfo() {

        if (!isNullOrUndefined(this.selectMachineCompare) && [...this.selectMachineCompare].length > 0) {
            //this.DBLength = [...key].length;
            const listDevice = [...this.selectMachineCompare];
            this.addedParams = [];
            console.log(this.baseOnCompare);
            if (this.baseOnCompare == "BaseOnDatabase") {
                this.addedParams.push({ Key: "ListEmployeeATID", Value: this.dataDBUserMaster.map(x => x.EmployeeATID) });
            }
            this.addedParams.push({ Key: "ListDevice", Value: listDevice });
            this.addedParams.push({ Key: "Filter", Value: this.filterCompare });
            //this.addedParams.push({ Key: "Filter", Value: '' });
            //this.addedParams.push({ Key:"BaseOn", Value:this.baseOnCompare });

            return await userInfoApi
                .GetUserMachineInfo(this.addedParams)
                .then((res) => {
                    const data = res.data as any;
                    this.pageUserInfo = 1;
                    this.dataDBUserInfo = data;
                    this.dataDBCurrentUserInfo = this.dataDBUserInfo.slice(0, this.pageUserInfo * 40);
                }).then((res) => {
                    this.CountUser();
                });
        } else {
            this.pageUserInfo = 1;
            this.dataDBUserInfo = [];
            this.dataDBCurrentUserInfo = [];
            this.CountUser();
        }
    }

    async CountUserMaster() {
        this.countUserMaster = [...this.dataDBUserMaster].length;
        this.countUserMasterCard = this.dataDBUserMaster.filter((item) => item.CardNumber != null && item.CardNumber != "0" && item.CardNumber.length > 0).length;
        this.countUserMasterPass = this.dataDBUserMaster.filter((item) => item.Password != null && item.Password != "0" && item.Password.length > 0).length;
        this.countUserMasterFace = this.dataDBUserMaster.filter((item) => item.FaceTemplate != null && item.FaceTemplate > 0).length;
        var fingerUM1, fingerUM2, fingerUM3, fingerUM4, fingerUM5, fingerUM6, fingerUM7, fingerUM8, fingerUM9, fingerUM10 = 0;
        fingerUM1 = this.dataDBUserMaster.filter((item) => item.Finger1 > 0).length;
        fingerUM2 = this.dataDBUserMaster.filter((item) => item.Finger2 > 0).length;
        fingerUM3 = this.dataDBUserMaster.filter((item) => item.Finger3 > 0).length;
        fingerUM4 = this.dataDBUserMaster.filter((item) => item.Finger4 > 0).length;
        fingerUM5 = this.dataDBUserMaster.filter((item) => item.Finger5 > 0).length;
        fingerUM6 = this.dataDBUserMaster.filter((item) => item.Finger6 > 0).length;
        fingerUM7 = this.dataDBUserMaster.filter((item) => item.Finger7 > 0).length;
        fingerUM8 = this.dataDBUserMaster.filter((item) => item.Finger8 > 0).length;
        fingerUM9 = this.dataDBUserMaster.filter((item) => item.Finger9 > 0).length;
        fingerUM10 = this.dataDBUserMaster.filter((item) => item.Finger10 > 0).length;
        this.countUserMasterFinger = fingerUM1 + fingerUM2 + fingerUM3 + fingerUM4 + fingerUM5 + fingerUM6 + fingerUM7 + fingerUM8 + fingerUM9 + fingerUM10;
    }

    async CountUser() {
        this.countUser = [...this.dataDBUserInfo].length;
        this.countUserCard = this.dataDBUserInfo.filter((item) => item.CardNumber != null && item.CardNumber != "0" && item.CardNumber.length > 0).length;
        this.countUserPass = this.dataDBUserInfo.filter((item) => item.Password != null && item.Password != "0" && item.Password.length > 0).length;
        this.countUserFace = this.dataDBUserInfo.filter((item) => item.FaceTemplate != null && item.FaceTemplate > 0).length;

        var fingerU1, fingerU2, fingerU3, fingerU4, fingerU5, fingerU6, fingerU7, fingerU8, fingerU9, fingerU10 = 0;
        fingerU1 = this.dataDBUserInfo.filter((item) => item.Finger1 > 0).length;
        fingerU2 = this.dataDBUserInfo.filter((item) => item.Finger2 > 0).length;
        fingerU3 = this.dataDBUserInfo.filter((item) => item.Finger3 > 0).length;
        fingerU4 = this.dataDBUserInfo.filter((item) => item.Finger4 > 0).length;
        fingerU5 = this.dataDBUserInfo.filter((item) => item.Finger5 > 0).length;
        fingerU6 = this.dataDBUserInfo.filter((item) => item.Finger6 > 0).length;
        fingerU7 = this.dataDBUserInfo.filter((item) => item.Finger7 > 0).length;
        fingerU8 = this.dataDBUserInfo.filter((item) => item.Finger8 > 0).length;
        fingerU9 = this.dataDBUserInfo.filter((item) => item.Finger9 > 0).length;
        fingerU10 = this.dataDBUserInfo.filter((item) => item.Finger10 > 0).length;
        this.countUserFinger = fingerU1 + fingerU2 + fingerU3 + fingerU4 + fingerU5 + fingerU6 + fingerU7 + fingerU8 + fingerU9 + fingerU10;

    }

    async showOrHideDialogDownloadUserCompare(showOrHide) {

        if (showOrHide) {
            this.isOverwriteUserMaster = false;
            this.commandRequest.ListSerial = [...this.selectMachineCompare];
            if ([...this.commandRequest.ListSerial].length == 0) {
                this.$alert(
                    this.$t("PleaseSelectMachine").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" }
                );
                return false;
            }
        }


        this.showDialogDownloadUserCompare = showOrHide;
    }

    async DownloadUserInfo() {

        this.commandRequest.Action = "Download user on machine";

        var listEmployeeATID;
        if (this.isCheckAllTable === true) {
            listEmployeeATID = this.dataDB.map((e) => e.EmployeeATID);
            listEmployeeATID.filter((e) => {
                return !this.commandRequest.ListUser.includes(e);
            });
        } else {
            listEmployeeATID = this.selectedUserInfoRows.map((e) => e.EmployeeATID);
            listEmployeeATID.filter((e) => {
                return !this.commandRequest.ListUser.includes(e);
            });
        }

        this.commandRequest.ListUser = [...listEmployeeATID];
        this.commandRequest.ListSerial = [...this.selectMachineCompare];

        if ([...this.commandRequest.ListSerial].length == 0) {
            this.$alert(
                this.$t("PleaseSelectMachine").toString(),
                this.$t("Notify").toString(),
                { type: "warning" }
            );
            return false;
        }
        this.showOrHideDialogDownloadUserCompare(false);

        return await commandApi
            .DownloadAllUser(this.commandRequest)
            .then((res) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$saveSuccess();
                }
            });

    }

    get allMachineCompares() {
        if (
            this.selectMachineCompare.length ===
            [...this.selectMachineCompareOption].map((item) => item.value).length
        ) {
            return "DeselectAll";
        } else {
            return "SelectAll";
        }

    }

    async ExportDeviceToExcel() {
        if (this.selectedEmployeeATIDs && this.selectMachineCompare && this.selectMachineCompare.length > 0) {
            this.addedParams = [];
            if (this.baseOnCompare == "BaseOnDatabase") {
                this.addedParams.push({ Key: "ListEmployeeATID", Value: this.selectedEmployeeATIDs });
            } else {
                this.addedParams.push({ Key: "ListEmployeeATID", Value: "" });
            }
            this.addedParams.push({ Key: "ListDevice", Value: this.selectMachineCompare });
            this.addedParams.push({ Key: "Filter", Value: this.filterCompare });

            return await userInfoApi
                .ExportUserMachineInfo(this.addedParams)
                .then((res) => {
                    // const data = res.data as any;
                    this.downloadFile(res.data.toString())
                });
        }

    }

    downloadFile(filePath) {
        var link = document.createElement('a');
        link.href = filePath;
        link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
        link.click();
    }

    SelectAllUserMasterTable(val) {

        if (this.selectedUserMasterRows.length === this.dataDBCurrentUserMaster.length) {
            this.isCheckAllUserMasterTable = true;
        } else {
            this.isCheckAllUserMasterTable = false;
        }

    }

    SelectAllUserInfoTable(val) {

        if (this.selectedUserInfoRows.length === this.dataDBCurrentUserInfo.length) {
            this.isCheckAllUserInfoTable = true;
        } else {
            this.isCheckAllUserInfoTable = false;
        }

    }

    GetListChildrent(object) {
        if (
            !Misc.isEmpty(object.ListChildrent) &&
            object.ListChildrent.length > 150
        ) {
            //var arrTemp = [...object.ListChildrent]
            //delete object['ListChildrent']
            var arrTemp = [];
            for (
                let i = 0;
                i < Math.ceil(object.ListChildrent.length / 100);
                i++
            ) {
                let calcFirstNumber = i * 100 + 1;
                let calcLastNumber =
                    (i + 1) * 100 < object.ListChildrent.length
                        ? (i + 1) * 100
                        : object.ListChildrent.length;
                arrTemp.push(
                    Object.assign(
                        {},
                        {
                            Name: calcFirstNumber + "-" + calcLastNumber,
                            ListChildrent: object.ListChildrent.slice(
                                calcFirstNumber - 1,
                                calcLastNumber
                            ),
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

    onInfiniteLazyLoadUserInfo() {

        if (
            this.dataDBCurrentUserInfo.length > this.pageUserInfo * 40 - 1 &&
            this.dataDBCurrentUserInfo.length !== 0
        ) {
            this.pageUserInfo = this.pageUserInfo + 1;
            this.loadingLazyUserInfo = true;
            setTimeout(() => {
                const arrToAddTable = this.dataDBUserInfo.slice(
                    (this.pageUserInfo - 1) * 40,
                    this.pageUserInfo * 40
                );
                if (isNullOrUndefined(this.dataDBCurrentUserInfo)) {
                    this.dataDBCurrentUserInfo = [];
                }
                this.loadingLazyUserInfo = false;
                var arrIndex = [];

                arrToAddTable.forEach((item) => {
                    this.dataDBCurrentUserInfo.push(item);
                    arrIndex.push(this.dataDBCurrentUserInfo.length - 1);
                });
                //if (this.isCheckAllTable === true) {
                //    arrIndex.forEach((item) => {
                //        (this.$refs.multipleTable as any).toggleRowSelection(
                //            this.dataDBCurrent[item],
                //            true
                //        );
                //    });
                //}

            }, 250);
        }
    }

    onInfiniteLazyLoadUserMaster() {

        if (
            this.dataDBCurrentUserMaster.length > this.pageUserMaster * 40 - 1 &&
            this.dataDBCurrentUserMaster.length !== 0
        ) {
            this.pageUserMaster = this.pageUserMaster + 1;
            this.loadingLazyUserMaster = true;
            setTimeout(() => {
                const arrToAddTable = this.dataDBUserMaster.slice(
                    (this.pageUserMaster - 1) * 40,
                    this.pageUserMaster * 40
                );
                if (isNullOrUndefined(this.dataDBCurrentUserMaster)) {
                    this.dataDBCurrentUserInfo = [];
                }
                this.loadingLazyUserMaster = false;
                var arrIndex = [];

                arrToAddTable.forEach((item) => {
                    this.dataDBCurrentUserMaster.push(item);
                    arrIndex.push(this.dataDBCurrentUserMaster.length - 1);
                });
                //if (this.isCheckAllTable === true) {
                //    arrIndex.forEach((item) => {
                //        (this.$refs.multipleTable as any).toggleRowSelection(
                //            this.dataDBCurrent[item],
                //            true
                //        );
                //    });
                //}

            }, 250);
        }
    }

    async getListIPAddress(serialNumbers: string) {
        const response = await deviceApi.GetIPAddressBySerialNumbers(serialNumbers);
        this.listIPAddress = response.data;
        return response.data;
    }

}
