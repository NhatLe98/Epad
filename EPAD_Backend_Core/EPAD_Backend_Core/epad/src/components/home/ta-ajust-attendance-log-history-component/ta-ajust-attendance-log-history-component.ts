import { Component, Vue } from "vue-property-decorator";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import moment from "moment";
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from "@/$api/department-api";
import { hrUserApi } from "@/$api/hr-user-api";
import { TA_AjustAttendanceLogHistoryParam, taAjustAttendanceLogHistory } from "@/$api/ta-ajust-attendance-log-history-api";
@Component({
    name: "ajust-attendance-log-history",
    components: { HeaderComponent, DataTableComponent, SelectDepartmentTreeComponent, DataTableFunctionComponent }
})
export default class AjustAttendanceLogHistoryComponent extends Vue {
    columns = [];
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    data: any = [];
    allOptionLst = [
        { value: 1, label: "Add" },
        { value: 2, label: "Update" },
        { value: 3, label: "Delete" },
        { value: 4, label: "Import" },
    ];
    selectedArea = [];
    selectedDoor = [];
    loading = false;
    page = 1;
    filter = "";
    departmentIds = [];
    EmployeeATIDsFilter = [];
    employeeFullLookupTemp = {};
    employeeFullLookup = {};
    SelectedDepartment = [];
    listAllEmployee = [];
    selectedOption = [];
    selectUserTypeOption = [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Customer' },
        { value: 3, label: 'Student' },
        { value: 4, label: 'Parent' },
        { value: 5, label: 'Nanny' },
        { value: 6, label: 'Contractor' },
        { value: 7, label: 'Teacher' },
    ]
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
    setColumn() {
        this.columns = [
           
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
                minWidth: '180',
                fixed: true,
                display: true
            },
            {
                prop: 'Day',
                label: 'Day',
                minWidth: '180',
                display: true
            },
            {
                prop: 'OriginalData',
                label: 'OriginalData',
                minWidth: '180',
                display: true
            },
            {
                prop: 'ModifiedData',
                label: 'ModifiedData',
                minWidth: '180',
                display: true
            },
            {
                prop: 'VerifyModeString',
                label: 'VerifyMode',
                minWidth: '180',
                display: true,
                dataType: 'translate'
            },
            {
                prop: 'InOutModeString',
                label: 'InOutMode',
                minWidth: '180',
                display: true
            },
            {
                prop: 'DeviceName',
                label: 'DeviceName',
                minWidth: '180',
                display: true
            },
            {
                prop: 'OperatorString',
                label: 'Operator',
                minWidth: '180',
                display: true,
                dataType: 'translate'
            },
            
            {
                prop: 'UpdatedUser',
                label: 'UpdatedUser',
                minWidth: '180',
                display: true
            },
           
            {
                prop: 'UpdatedDateString',
                label: 'UpdatedDate',
                minWidth: '180',
                display: true
            
            },
        ];
    }

    masterEmployeeFilter = [];

    async beforeMount() {
        this.setColumn();
        await this.LoadDepartmentTree();
        await this.getEmployeesData();
        await this.Search();
        const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
		if (jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0) {
			this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
		}
        if(this.masterEmployeeFilter && this.masterEmployeeFilter.length > 0){
			this.EmployeeATIDsFilter = Misc.cloneData(this.masterEmployeeFilter)
				.filter(x => this.employeeFullLookup.hasOwnProperty(x.toString()))?.map(x => x.toString()) ?? [];
		}
    }

    mounted() {

    }

    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }


    selectAllEmployeeFilter(value) {
        console.log(value)
        this.EmployeeATIDsFilter = [...value];
    }

    onChangeDepartmentFilterSearch(departments) {

        this.EmployeeATIDsFilter = [];
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
        this.loading = true;
        const param: TA_AjustAttendanceLogHistoryParam = {
            Departments: this.SelectedDepartment,
            EmployeeATIDs: this.EmployeeATIDsFilter,
            Filter: this.filter,
            FromDate: moment(this.fromTime).format("YYYY-MM-DD HH:mm:ss"),
            ToDate: moment(this.toTime).format("YYYY-MM-DD HH:mm:ss"),
            Operators: this.selectedOption,
            Page: page,
            Limit: pageSize
        }
        return await taAjustAttendanceLogHistory
            .GetAjustAttendanceLogHistoryAtPage(
                param
            )
            .then(res => {
                this.loading = false;
                const { data } = res as any;
                return {
                    data: data.data,
                    total: data.total
                };
            })

    }



    async Search() {
        this.page = 1;
        (this.$refs.table as any).getTableData(this.page, null, null);

    }
}
