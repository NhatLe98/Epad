import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import AppLayout from '@/components/app-component/app-layout/app-layout.vue';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import FixedDeparmentSchedule from './ta-fixed-department-schedule/ta-fixed-department-schedule.vue';
import FixedEmployeeSchedule from './ta-fixed-employee-schedule/ta-fixed-employee-schedule.vue';
import HrEmpoloyeeInfo from './employee-info/employee-info.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { departmentApi } from '@/$api/department-api';
import { hrUserApi } from '@/$api/hr-user-api';
import RegisterForLeave from './register-for-leave/register-for-leave.vue';
import AjustAttendanceLog from './ajust-attendance-log/ajust-attendance-log.vue';
import HandleCalculateAttendance from './handle-calculate-attendance/handle-calculate-attendance.vue';
import AjustTimeAttendanceLog from './ajust-time-attendance-log/ajust-time-attendance-log.vue';
import SyntheticAttendance from './synthetic-attendance/synthetic-attendance.vue';
import ReportPage from './report/report.vue';
import RegisterBusinessTrip from './register-business-trip/register-business-trip.vue';
import RegisterEmployeeShift from './register-employee-shift/register-employee-shift.vue';
@Component({
    name: 'ta-use-quick-screen',
    components: {
        'ta-fixed-department-schedule': FixedDeparmentSchedule,
        'ta-fixed-employee-schedule': FixedEmployeeSchedule,
        'register-employee-shift': RegisterEmployeeShift,
        'employee-info': HrEmpoloyeeInfo,
        'register-for-leave': RegisterForLeave,
        'register-business-trip': RegisterBusinessTrip,
        'ajust-attendance-log': AjustAttendanceLog,
        'handle-calculate-attendance': HandleCalculateAttendance,
        'ajust-time-attendance-log': AjustTimeAttendanceLog,
        'synthetic-attendance': SyntheticAttendance,
        'report': ReportPage,
        AppLayout,
        DataTableComponent,
        DataTableFunctionComponent,

    },
})
export default class FixedScheduleComponent extends Mixins(ComponentBase) {
    tabConfig: ITabCollection = [];
    tabActive = 0;
    clientName: string;
    inActiveLst: any[];
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
    employeeFullLookupFilter = {};
    employeeFullLookup = {};
    listAllEmployee = [];
    hideTextInTab = false;
    departmentFilter = [];
    employeeFilter = [];

    loading = false;
    async beforeMount() {
        this.loading = true;
        await this.LoadDepartmentTree();
        await this.getEmployeesData();
        await Misc.readFileAsync('static/variables/tab-config-eTA.json').then(res => {
            if (res && res.length > 0) {
                res.forEach(element => {
                    this.tabConfig.push({
                        tabName: element.tabName,
                        title: this.$t(element.title).toString(),
                        componentName: element.componentName,
                        selectedRowKeys: element.selectedRowKeys,
                        showAdd: element.showAdd,
                        showMore: element.showMore,
                        showDelete: element.showDelete,
                        showEdit: element.showEdit,
                        active: element.active,
                        showIntegrate: element.showIntegrate,
                        iconImage: element.iconImage, // Define the icon class here
                        listEmployeeATID: this.listAllEmployee,
                        departmentData: this.tree.treeData,
                        id: element.id
                    });
                });
            }
            // this.tabConfig = x.ClientName;
        });
        this.loading = false;
        // this.intiTabConfig();
    }

    intiTabConfig() {
        this.tabConfig = [
            {
                tabName: "HrEmpoloyeeInfo",
                title: 'Khai báo thông tin nhân viên',
                componentName: 'employee-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: true,
                showIntegrate: false,
                iconImage: '/assets/icons/NavBar/usermanagement.png', // Define the icon class here
                listEmployeeATID: this.listAllEmployee,
                departmentData: this.tree.treeData,
                id: 1
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: 'Khai báo lịch làm việc theo nhân viên',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 2
            },
            {
                tabName: "RegisterEmployeeShift",
                title: 'Bảng phân ca',
                componentName: 'register-employee-shift',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 3
            },
            {
                tabName: "RegisterForLeave",
                title: 'Đăng ký nghỉ',
                componentName: 'register-for-leave',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 4
            },
            {
                tabName: "RegisterBusinessTrip",
                title: 'Đăng ký công tác',
                componentName: 'register-business-trip',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 5
            },
            {
                tabName: "AjustAttendanceLog",
                title: 'Điều chỉnh dữ liệu điểm danh',
                componentName: 'ajust-attendance-log',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 6
            },
            {
                tabName: "HandleCalculateAttendance",
                title: 'Xử lý tính công',
                componentName: 'handle-calculate-attendance',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 7
            },
            {
                tabName: "SyntheticAttendance",
                title: 'Xem dữ liệu công tổng hợp',
                componentName: 'synthetic-attendance',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 8
            },
            {
                tabName: "AjustTimeAttendanceLog",
                title: 'Điều chỉnh kết quả tính công',
                componentName: 'ajust-time-attendance-log',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 9
            },
            {
                tabName: "Report",
                title: 'Báo cáo',
                componentName: 'report',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png', // Define the icon class here
                departmentData: this.tree.treeData,
                listEmployeeATID: this.listAllEmployee,
                id: 10
            },
        ]
    }

    intiTabConfigHideTextInTab() {
        this.tabConfig = [
            {
                tabName: "FixedDepartmentSchedule",
                title: '',
                componentName: 'hr-employee-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: true,
                showIntegrate: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 1
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: '',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 2
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: '',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 2
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: '',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 2
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: '',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 2
            },
            {
                tabName: "FixedEmployeeSchedule",
                title: '',
                componentName: 'ta-fixed-employee-schedule',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                iconImage: '/assets/icons/NavBar/employeemanagement.png',
                id: 2
            }
        ]
    }

    clickHideTextInTab() {
        if (this.hideTextInTab == false) {
            this.intiTabConfigHideTextInTab();
            this.hideTextInTab = true;
        } else {
            this.intiTabConfig();
            this.hideTextInTab = false;
        }
    }

    handleTabClick(tab, event) {
        this.tabActive = parseInt(tab.index);
        this.tabConfig = this.tabConfig.map((e, ix) => {
            return { ...e, active: this.tabActive === ix }
        });
        const tabObj = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        // console.log(tabObj);
        (tabObj.$refs.customerDataTableFunction as any).reloadConfig(this.tabConfig[this.tabActive].tabName);
        tabObj.onChangeClick();
    }

    onInsertClick() {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.listContactInfoFormApi = [];
        tab.onInsertClick();
    }

    // onChangeClick() {
    //     const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
    //     tab.listContactInfoFormApi = [];
    //     tab.onChangeClick();
    // }

    async onEditClick(tabInfo) {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.listContactInfoFormApi = [];
        let EmployeeATID = tabInfo?.selectedRowKeys?.[0]?.EmployeeATID ?? 0;

        tab.onEditClick();
    }

    onDeleteClick() {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.onDeleteClick();
    }

    onCommand(cmd) {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.onCommand(cmd);
    }

    setSelected(e, ix) {
        //console.log(`call edit quan ly nguoi dung`, e)
        this.tabConfig[ix].selectedRowKeys = e;
    }

    setDataDepartmentFilter(data) {
        this.departmentFilter = data;
    }

    setFilterModel(e, ix) {
        this.departmentFilter = e.SelectedDepartmentFilter;
        this.employeeFilter = e.SelectedEmployeeATIDFilter;
        this.tabConfig[ix].filterModel = e;
    }

    async LoadDepartmentTree() {
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
                this.employeeFullLookupFilter = dictData;
                this.employeeFullLookup = dictData;
                this.employeeFullLookupTemp = dictData;
            }
        });
    }
}
