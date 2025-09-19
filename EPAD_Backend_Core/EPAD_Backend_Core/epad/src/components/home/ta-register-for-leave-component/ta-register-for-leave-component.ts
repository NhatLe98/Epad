import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';

import TARegisterForLeave from './register-for-leave/register-for-leave.vue';
import TARegisterBusinessTrip from './register-business-trip/register-business-trip.vue';
import TARegisterEmployeeShift from './register-employee-shift/register-employee-shift.vue';
import AppLayout from '@/components/app-component/app-layout/app-layout.vue';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { userTypeApi } from '@/$api/user-type-api';
import { employeeInfoApi } from '@/$api/employee-info-api';

@Component({
    name: 'ta-register-for-leave-component',
    components: {
         'ta-register-for-leave': TARegisterForLeave,
         'register-business-trip': TARegisterBusinessTrip,
         'register-employee-shift': TARegisterEmployeeShift,
        // 'hr-employee-info': HREmployeeInfo,
        AppLayout,
    },
})
export default class UserManagementComponent extends Mixins(ComponentBase) {
    tabConfig: ITabCollection = [];
    tabActive = 0;
    clientName: string;
    inActiveLst: any[];
    
    beforeMount() {
        this.intiTabConfig();
        this.tabConfig = this.tabConfig.map((e, ix) => {
            return { ...e, active: this.tabActive === ix }
        });
    }

    intiTabConfig() {
        this.tabConfig = [
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
                id: 1
            },
            {
                tabName: "RegisterForLeave",
                title: 'Đăng ký nghỉ',
                componentName: 'ta-register-for-leave',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showAdd: true,
                id: 2
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
                showAdd: true,
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

    async onIntegrateClick() {
        this.isLoading = true;
        this.$alert(
            this.$t("EmployeeIntegrationInprocess").toString(),
            this.$t("Notify").toString(),
            null
        );
        employeeInfoApi.RunIntegrate().then((res: any) => {
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
