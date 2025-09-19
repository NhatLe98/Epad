import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import AppLayout from "@/components/app-component/app-layout/app-layout.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import ListLocationComponent from "./ta-list-location/ta-list-location-component.vue";
import { hrEmployeeInfoApi } from "@/$api/hr-employee-info-api";
import LocationByDepartment from "./ta-location-by-department/ta-location-by-department-component.vue";
import LocationByEmployee from "./ta-location-by-employee/ta-location-by-employee-component.vue";
@Component({
  name: "ta-location-management",
  components: {
    "ta-list-location": ListLocationComponent,
    'ta-location-by-department': LocationByDepartment,
    'ta-location-by-employee': LocationByEmployee,
    AppLayout,
    DataTableComponent,
    DataTableFunctionComponent,
  },
})
export default class LocationManagementComponent extends Mixins(ComponentBase) {
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
        tabName: "ListLocation",
        title: "Danh sách địa điểm",
        componentName: "ta-list-location",
        selectedRowKeys: [],
        showMore: true,
        showDelete: true,
        showEdit: true,
        active: true,
        showIntegrate: false,
        id: 1,
      },
      {
        tabName: "LocationByDepartment",
        title: "Địa điểm theo phòng ban",
        componentName: "ta-location-by-department",
        selectedRowKeys: [],
        showMore: true,
        showDelete: true,
        showEdit: true,
        active: false,
        id: 2,
      },
      {
        tabName: "LocationByEmployee",
        title: "Địa điểm theo nhân viên",
        componentName: "ta-location-by-employee",
        selectedRowKeys: [],
        showMore: true,
        showDelete: true,
        showEdit: true,
        active: false,
        id: 3,
      },
    ];
  }

  handleTabClick(tab, event) {
    this.tabActive = parseInt(tab.index);
    this.tabConfig = this.tabConfig.map((e, ix) => {
      return { ...e, active: this.tabActive === ix };
    });
    const tabObj = this.$refs[
      `${this.tabConfig[this.tabActive].tabName}-tab-ref`
    ][0] as any;
    (tabObj.$refs.customerDataTableFunction as any)?.reloadConfig(
      this.tabConfig[this.tabActive].tabName
    );
  }

  async onEditClick(tabInfo) {
    const tab = this.$refs[
      `${this.tabConfig[this.tabActive].tabName}-tab-ref`
    ][0] as any;
    tab.listContactInfoFormApi = [];
    let EmployeeATID = tabInfo?.selectedRowKeys?.[0]?.EmployeeATID ?? 0;
    await hrEmployeeInfoApi
      .GetUserContactInfoById(parseInt(EmployeeATID))
      .then((response) => {
        tab.listContactInfoFormApi = response.data;
      })
      .catch((error) => {
        console.warn(error);
      });
    tab.onEditClick();
  }

  onInsertClick() {
    const tab = this.$refs[
      `${this.tabConfig[this.tabActive].tabName}-tab-ref`
    ][0] as any;
    tab.listContactInfoFormApi = [];
    tab.onInsertClick();
  }
  onDeleteClick() {
    const tab = this.$refs[
      `${this.tabConfig[this.tabActive].tabName}-tab-ref`
    ][0] as any;
    tab.onDeleteClick();
  }

  onCommand(cmd) {
    const tab = this.$refs[
      `${this.tabConfig[this.tabActive].tabName}-tab-ref`
    ][0] as any;
    tab.onCommand(cmd);
  }

  setSelected(e, ix) {
    this.tabConfig[ix].selectedRowKeys = e;
  }

  setFilterModel(e, ix) {
    this.tabConfig[ix].filterModel = e;
  }
}
