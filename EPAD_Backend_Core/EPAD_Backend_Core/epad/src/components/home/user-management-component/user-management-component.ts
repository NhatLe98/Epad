import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';

import HRCustomerInfo from './hr-customer-info/hr-customer-info.vue';
import HREmployeeInfo from './hr-employee-info/hr-employee-info.vue';
import HRNannyInfo from './hr-nanny-info/hr-nanny-info.vue';
import HRParentInfo from './hr-parent-info/hr-parent-info.vue';
import HRStudentInfo from './hr-student-info/hr-student-info.vue';
import AppLayout from '@/components/app-component/app-layout/app-layout.vue';
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { userTypeApi } from '@/$api/user-type-api';
import HRTeacherInfo from './hr-teacher-info/hr-teacher-info.vue';
import HRContractorInfo from './hr-contractor-info/hr-contractor-info.vue';
import HRDriverInfo from './hr-driver-info/hr-driver-info.vue';
import { employeeInfoApi } from '@/$api/employee-info-api';

@Component({
    name: 'user-management',
    components: {
        'hr-customer-info': HRCustomerInfo,
        'hr-employee-info': HREmployeeInfo,
        'hr-nanny-info': HRNannyInfo,
        'hr-parent-info': HRParentInfo,
        'hr-student-info': HRStudentInfo,
        'hr-teacher-info': HRTeacherInfo,
        'hr-contractor-info': HRContractorInfo,
        'hr-driver-info':HRDriverInfo,
        AppLayout,
    },
})
export default class UserManagementComponent extends Mixins(ComponentBase) {
    tabConfig: ITabCollection = [];
    tabActive = 0;
    clientName: string;
    inActiveLst: any[];
    isDoneLoadTabConfig = true;
    beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;
            this.isDoneLoadTabConfig = false;
            if (this.clientName == "") {
                this.intiTabConfig();
                userTypeApi.GelAllUserType().then(res => {
                    this.inActiveLst = res.data.filter(x => x.Status != 'Active').map(x => x.UserTypeId);
                    if (this.inActiveLst.length > 0) {
                        this.tabConfig = this.tabConfig.filter(x => !this.inActiveLst.includes(x.id));
                        this.isDoneLoadTabConfig = true;
                    }
                });
            } else if (this.clientName == "MAY") {
                this.intiTabConfig_May();
                this.isDoneLoadTabConfig = true;
            } else if (this.clientName == "Ortholite") {
                this.intiTabConfig_Ortholite();
                this.isDoneLoadTabConfig = true;

            } else if(this.clientName == "Swanbay"){
                this.intiTabConfigSwanbay();
                userTypeApi.GelAllUserType().then(res => {
                    this.inActiveLst = res.data.filter(x => x.Status != 'Active').map(x => x.UserTypeId);
                    if (this.inActiveLst.length > 0) {
                        this.tabConfig = this.tabConfig.filter(x => !this.inActiveLst.includes(x.id));
                        this.isDoneLoadTabConfig = true;
                    }
                });
            }
            else {

                this.intiTabConfig();
                userTypeApi.GelAllUserType().then(res => {
                    this.inActiveLst = res.data.filter(x => x.Status != 'Active').map(x => x.UserTypeId);
                    if (this.inActiveLst.length > 0) {
                        this.tabConfig = this.tabConfig.filter(x => !this.inActiveLst.includes(x.id));
                        this.isDoneLoadTabConfig = true;
                    }
                });
            }
        })
    }

    intiTabConfig_Ortholite() {
        this.tabConfig = [
            {
                tabName: "UserManagement",
                title: this.$t("UserManagement").toString(),
                componentName: 'hr-employee-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: true,
                showIntegrate: true
            },
            {
                tabName: "CustomerManagement1",
                title: this.$t('CustomerManagement').toString(),
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
            },
        ]
    }
    intiTabConfig() {
        this.tabConfig = [
            {
                tabName: "UserManagement",
                title: 'Quản lý nhân viên',
                componentName: 'hr-employee-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: true,
                showIntegrate: true,
                id: 1
            },
            {
                tabName: "CustomerManagement",
                title: 'Quản lý khách',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                showReadGoogleSheet: true,
                id: 2
            },

            {
                tabName: "NannyManagement",
                title: 'Quản lý bảo mẫu',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 5
            },
            {
                tabName: "ParentManagement",
                title: 'Quản lý phụ huynh',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 4
            },
            {
                tabName: "StudentManagement",
                title: 'Quản lý học sinh',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 3
            },
            {
                tabName: "TeacherManagement",
                title: 'Quản lý giáo viên',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 7
            },
            {
                tabName: "ContractorManagement",
                title: 'Quản lý nhà thầu',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 6
            },
            {
                tabName: "DriverManagement",
                title: 'Quản lý tài xế',
                componentName: 'hr-driver-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: false,
                id: 8
            }
        ]
    }

    intiTabConfigSwanbay() {
        this.tabConfig = [
            {
                tabName: "UserManagement",
                title: 'Quản lý nhân viên',
                componentName: 'hr-employee-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: true,
                showIntegrate: true,
                showAdd: false,
                id: 1
            },
            {
                tabName: "CustomerManagement",
                title: 'Quản lý khách',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 2
            },

            {
                tabName: "NannyManagement",
                title: 'Quản lý bảo mẫu',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 5
            },
            {
                tabName: "ParentManagement",
                title: 'Quản lý phụ huynh',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 4
            },
            {
                tabName: "StudentManagement",
                title: 'Quản lý học sinh',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 3
            },
            {
                tabName: "TeacherManagement",
                title: 'Quản lý giáo viên',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 7
            },
            {
                tabName: "ContractorManagement",
                title: 'Quản lý nhà thầu',
                componentName: 'hr-customer-info',
                selectedRowKeys: [],
                showMore: false,
                showDelete: false,
                showEdit: false,
                active: false,
                showAdd: false,
                id: 6
            }
        ]
    }

    intiTabConfig_May() {
        this.tabConfig = [
            {
                tabName: "UserManagement",
                title: 'Quản lý khách hàng',
                componentName: 'hr-employee-info',
                selectedRowKeys: [],
                showMore: true,
                showDelete: true,
                showEdit: true,
                active: true,
                id: 1
            },
            // {
            //     tabName: "CustomerManagement",
            //     title: 'Quản lý khách',
            //     componentName: 'hr-customer-info',
            //     selectedRowKeys: [],
            //     showMore: true,
            //     showDelete: true,
            //     showEdit: true,
            //     active: false,
            //     id: 2
            // },
            /*
            // {
            //     tabName: "NannyManagement1",
            //     title: 'Quản lý bảo mẫu',
            //     componentName: 'hr-nanny-info',
            //     selectedRowKeys: [],
            //     showMore: false,
            //     showDelete: false,
            //     showEdit: false,
            //     active: false,
            //     showAdd: false
            // },
            // */
            // {
            //     tabName: "ParentManagement1",
            //     title: 'Quản lý phụ huynh',
            //     componentName: 'hr-parent-info',
            //     selectedRowKeys: [],
            //     showMore: true,
            //     showDelete: true,
            //     showEdit: true,
            //     active: false,
            // },
            // {
            //     tabName: "StudentManagement",
            //     title: 'Quản lý học sinh',
            //     componentName: 'hr-customer-info',
            //     selectedRowKeys: [],
            //     showMore: true,
            //     showDelete: true,
            //     showEdit: true,
            //     active: false,
            //     id: 3
            // }
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

    onReadGoogleSheetClick() {
        const tab = this.$refs[`${this.tabConfig[this.tabActive].tabName}-tab-ref`][0] as any;
        tab.listContactInfoFormApi = [];
        tab.onReadGoogleSheetClick();
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
