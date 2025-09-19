import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { Component, Mixins } from "vue-property-decorator";
import VisualizeTable from "@/components/app-component/visualize-table/visualize-table.vue";
import AppPagination from "@/components/app-component/app-pagination/app-pagination.vue";
import { isNullOrUndefined } from "util";
import LocationPopupComponent from "@/components/app-component/location-popup-component/location-popup-component.vue";
import TabBase from "@/mixins/application/tab-mixins";
import { departmentApi } from "@/$api/department-api";
import { ElForm } from "element-ui/types/form";
import SelectDepartmentTreeComponent from "@/components/app-component/select-department-tree-component/select-department-tree-component.vue";
import {
  TA_LocationByDepartmentDTO,
  taLocationByDepartment,
} from "@/$api/ta-location-by-department-api";
import SelectTreeComponent from "@/components/app-component/select-tree-component/select-tree-component";
import { taListLocation } from "@/$api/ta-list-location-api";

@Component({
  name: "ta-location-by-department",
  components: {
    HeaderComponent,
    VisualizeTable,
    AppPagination,
    LocationPopupComponent,
    SelectDepartmentTreeComponent,
    SelectTreeComponent,
  },
})
export default class LocationByDepartmentComponent extends Mixins(TabBase) {
  page = 1;
  showDialog = false;
  disabled = false;
  isEdit = false;
  rowsObj = [];
  EmployeeATIDs = [];
  columns = [];
  departments = [];
  isLoading = false;
  loadingTree = false;
  shouldResetColumnSortState = false;
  filterModel = {
    TextboxSearch: "",
  };
  employeeFullLookup = {};
  locationFullLookupFilter = {};
  listAllEmployee = [];
  listLocation = [];
  tree = {
    employeeList: [],
    clearable: true,
    defaultExpandAll: false,
    multiple: true,
    placeholder: "",
    disabled: false,
    checkStrictly: true,
    popoverWidth: 400,
    treeData: [],
    treeProps: {
      value: "ID",
      children: "ListChildrent",
      label: "Name",
    },
  };
  ruleForm: TA_LocationByDepartmentDTO = {
    DepartmentList: [],
    LocationIndex: [],
  };
  rules: any = {};

  columnDefs = [
    {
      field: "index",
      sortable: true,
      pinned: false,
      headerName: "#",
      width: 200,
      checkboxSelection: true,
      headerCheckboxSelection: true,
      headerCheckboxSelectionFilteredOnly: true,
      display: true,
    },
    {
      field: "DepartmentName",
      headerName: this.$t("Department"),
      pinned: false,
      minWidth: 400,
      sortable: true,
      display: true,
    },
    {
      field: "LocationName",
      headerName: this.$t("LocationName"),
      pinned: false,
      minWidth: 400,
      sortable: true,
      display: true,
    },
    {
      field: "Address",
      headerName: this.$t("Address"),
      pinned: false,
      minWidth: 400,
      sortable: true,
      display: true,
    },
  ];

  initRule() {
    this.rules = {
      DepartmentList: [
        {
          required: true,
          message: this.$t("PleaseInputDepartment"),
          trigger: "blur",
        }
      ],
      LocationIndex: [
        {
          required: true,
          message: this.$t("PleaseSelectLocation"),
          trigger: "blur",
        }
      ],
    };
  }

  async beforeMount() {
    this.initRule();
    await this.loadData();
    this.LoadDepartmentTree();
    await this.loadLocationData();
  }

  onChangeDepartmentFilter(departments) {
    this.EmployeeATIDs = [];
    if (departments && departments.length > 0) {
      this.employeeFullLookup = Misc.cloneData(
        this.listAllEmployee.filter((x) =>
          departments.includes(x.DepartmentIndex)
        )
      )?.map((x) => ({
        Index: x.EmployeeATID,
        NameInFilter:
          x.EmployeeATID +
          (x.FullName && x.FullName != "" ? " - " + x.FullName : ""),
      }));
    } else {
      this.employeeFullLookup = Misc.cloneData(this.listAllEmployee)?.map(
        (x) => ({
          Index: x.EmployeeATID,
          NameInFilter:
            x.EmployeeATID +
            (x.FullName && x.FullName != "" ? " - " + x.FullName : ""),
        })
      );
    }
  }

  onChangeLocationFilter(locations) {
    if (locations && locations.length > 0) {
      const location = this.listLocation.find(
        (loc) => loc.LocationIndex === locations[0]
      );
      if (location) {
        this.ruleForm.LocationIndex = location.LocationIndex;
      }
    }
  }

  LoadDepartmentTree() {
    departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
      if (res.status == 200) {
        this.tree.treeData = res.data;
      }
    });
  }

  async loadLocationData() {
    taListLocation
      .GetListLocationAtPage(
        this.page,
        this.pageSize,
        this.filterModel.TextboxSearch
      )
      .then((res: any) => {
        if (res.status == 200 && res.data) {
          this.listLocation = res.data.data.map((location) => ({
            ...location,
            Address: location.Address,
            LocationIndex: location.LocationIndex,
          }));
        }
      });
  }

  displayPopupInsert() {
    this.showDialog = false;
  }
  reset() {
    const obj: TA_LocationByDepartmentDTO = {
      DepartmentList: [],
      LocationIndex: []
    };
    this.ruleForm = obj;
  }

  async loadData() {
    await taLocationByDepartment
      .GetLocationByDepartmentAtPage(
        this.page,
        this.pageSize,
        this.filterModel.TextboxSearch
      )
      .then((res: any) => {
        if (res.status == 200 && res.data) {
          this.dataSource = res.data.data.map((x, idx) => ({
            ...x,
            index: idx + 1 + (this.page - 1) * this.pageSize,
          }));
          this.total = res.data.total;
        }
      });
  }

  onSelectionChange(selectedRows: any[]) {
    this.selectedRows = selectedRows;
  }

  onInsertClick() {
    this.reset();
    this.showDialog = true;
    this.isEdit = false;
  }

  async onViewClick() {
    this.page = 1;
    await this.loadData();
  }

  async onSubmitClick() {
    (this.$refs.ruleForm as any).validate(async (valid) => {
      if (!valid) return;
      if (this.isEdit) {
        return await taLocationByDepartment
          .UpdateLocationByDepartment(this.ruleForm)
          .then((res) => {
            if (res.data != true) {
              const msg = res.data;
              this.$alert(
                this.$t("LocationDeclaredInThePreviousDepartment", {
                  data: msg,
                }).toString() + ". " + this.$t("PleaseCheckAgain").toString(),
                this.$t("Notify").toString(),
                { dangerouslyUseHTMLString: true }
              );
            } else {
              this.loadData();
              this.showDialog = false;
              this.reset();
              if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
              }
            }
          });
      } else {
        return await taLocationByDepartment
          .AddLocationByDepartment(this.ruleForm)
          .then((res) => {
            if (res.data) {
              this.$alert(
                this.$t("DepartmentDeclared").toString() +
                  ": " +
                  `${res.data}` +
                  this.$t("PleaseCheckAgain").toString(),
                this.$t("Notify").toString(),
                { dangerouslyUseHTMLString: true }
              );
            } else {
              this.loadData();
              this.showDialog = false;
              this.reset();
              if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
              }
            }
          })
          .catch(() => {});
      }
    });
  }

  onEditClick() {
    this.isEdit = true;
    var obj = JSON.parse(JSON.stringify(this.selectedRows));
    if (obj.length > 1) {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("MSG_SelectOnlyOneRow").toString()
      );
    } else if (obj.length === 1) {
      this.ruleForm = obj[0];
      this.showDialog = true;
    } else {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("ChooseUpdate").toString()
      );
    }
  }

  async Delete() {
    const listIndex = this.selectedRows.map((e) => e.Index);
    if (listIndex.length < 1) {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("ChooseRowForDelete").toString()
      );
    } else {
      await taLocationByDepartment
        .DeleteLocationByDepartment(listIndex)
        .then((res) => {
          if (!isNullOrUndefined(res.status) && res.status === 200) {
            this.$deleteSuccess();
            this.loadData();
            this.selectedRows = [];
            this.$deleteSuccess();
          }
        })
        .catch(() => {})
        .finally(() => {
          this.showDialogDeleteUser = false;
        });
    }
  }

  focus(x) {
    var theField = eval("this.$refs." + x);
    theField.focus();
  }

  onCancelClick() {
    var ref = <ElForm>this.$refs.ruleForm;
    ref.resetFields();
    this.showDialog = false;
  }
}
