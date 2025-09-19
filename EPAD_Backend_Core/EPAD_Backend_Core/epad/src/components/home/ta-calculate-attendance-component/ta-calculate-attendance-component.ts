import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';

import HRCustomerInfo from './hr-customer-info/hr-customer-info.vue';
import HREmployeeInfo from './hr-employee-info/hr-employee-info.vue';
import HRNannyInfo from './hr-nanny-info/hr-nanny-info.vue';
import HRParentInfo from './hr-parent-info/hr-parent-info.vue';
import HRStudentInfo from './hr-student-info/hr-student-info.vue';
import AppLayout from '@/components/app-component/app-layout/app-layout.vue';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import HandleCalculateAttendanceComponent from './ta-handle-calculate-attendance/ta-handle-calculate-attendance.vue';
import SyntheticAttendanceComponent from './ta-synthetic-attendance/ta-synthetic-attendance.vue';
import AjustTimeAttendanceCompnent from './ta-ajust-time-attendance-log/ta-ajust-time-attendance-log.vue';

@Component({
    name: 'ta-calculate-attendance',
    components: {
        'ta-handle-calculate-attendance': HandleCalculateAttendanceComponent,
        'ta-synthetic-attendance': SyntheticAttendanceComponent,
        'ta-ajust-time-attendance-log':AjustTimeAttendanceCompnent,
        AppLayout,
    },
})
export default class CalculateAttendanceComponent extends Mixins(ComponentBase) {
    tabConfig: ITabCollection = [];
    tabActive = 0;
    clientName: string;
    inActiveLst: any[];
    beforeMount() {

        this.intiTabConfig();

    }

   
    intiTabConfig() {
        this.tabConfig = [
            {
                tabName: "HandleCalculateAttendanceComponent",
                title: 'Xử lý tính công',
                componentName: 'ta-handle-calculate-attendance',
                selectedRowKeys: [],
                showMore: true,
                showDelete: false,
                showEdit: false,
                active: true,
                showIntegrate: false,
                showAdd: false,
                id: 1
            },
            {
                tabName: "SyntheticAttendanceComponent",
                title: 'Xem dữ liệu công tổng hợp',
                componentName: 'ta-synthetic-attendance',
                selectedRowKeys: [],
                showMore: true,
                showDelete: false,
                showEdit: false,
                active: false,
                showIntegrate: false,
                showAdd: false,
                id: 2
            },
            {
                tabName: "AjustTimeAttendanceComponent",
                title: 'Điều chỉnh kết quả tính công',
                componentName: 'ta-ajust-time-attendance-log',
                selectedRowKeys: [],
                showMore: true,
                showDelete: false,
                showEdit: false,
                active: false,
                showIntegrate: false,
                showAdd: false,
                id: 3
            }
          
        ]
    }


    handleTabClick(tab, event) {
        this.tabActive = parseInt(tab.index);
        this.tabConfig = this.tabConfig.map((e, ix) => {
            return { ...e, active: this.tabActive === ix }
        });
        const tabObj = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        // console.log(tabObj);
        (tabObj.$refs.customerDataTableFunction as any).reloadConfig(this.tabConfig[this.tabActive].tabName);
    }

    onInsertClick() {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.listContactInfoFormApi = [];
        tab.onInsertClick();
    }

    async onEditClick(tabInfo) {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.listContactInfoFormApi = [];
        let EmployeeATID = tabInfo?.selectedRowKeys?.[0]?.EmployeeATID ?? 0;
        // call api dưa theo id để lấy HR_ContactInfo
        await hrEmployeeInfoApi.GetUserContactInfoById(parseInt(EmployeeATID))
            .then(response => {
                tab.listContactInfoFormApi = response.data;
            })
            .catch((error) => {
                console.warn(error)
            })

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

    setFilterModel(e, ix) {
        this.tabConfig[ix].filterModel = e;
    }
}
