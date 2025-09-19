import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { Component, Mixins } from "vue-property-decorator";
import VisualizeTable from "@/components/app-component/visualize-table/visualize-table.vue";
import AppPagination from "@/components/app-component/app-pagination/app-pagination.vue";
import { isNullOrUndefined } from "util";
import LocationPopupComponent from "@/components/app-component/location-popup-component/location-popup-component.vue";
import TabBase from "@/mixins/application/tab-mixins";
import { departmentApi } from "@/$api/department-api";
import * as XLSX from "xlsx";
import { ElForm } from "element-ui/types/form";
import SelectDepartmentTreeComponent from "@/components/app-component/select-department-tree-component/select-department-tree-component.vue";
import SelectTreeComponent from "@/components/app-component/select-tree-component/select-tree-component";
import { taListLocation } from "@/$api/ta-list-location-api";
import {
  TA_LocationByEmployeeDTO,
  taLocationByEmployee,
} from "@/$api/ta-location-by-employee-api";
import { hrUserApi } from "@/$api/hr-user-api";

@Component({
  name: "ta-location-by-employee",
  components: {
    HeaderComponent,
    VisualizeTable,
    AppPagination,
    LocationPopupComponent,
    SelectDepartmentTreeComponent,
    SelectTreeComponent,
  },
})
export default class LocationByEmployeeComponent extends Mixins(TabBase) {
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
  employeeFullLookup = {};
  employeeFullLookupTemp = {};
  employeeFullLookupFilter = {};
  locationFullLookupFilter = {};
  SelectedDepartment = [];
  selectAllEmployeeFilter = [];
  showDialogImportError = false;
  shouldResetColumnSortState = false;
  formExcel = {};
  listExcelFunction = ["AddExcel"];
  dataAddExcel = [];
  importErrorMessage = "";
  isAddFromExcel = false;
  showDialogExcel = false;
  isDeleteFromExcel = false;
  fileName = "";
  listAllEmployee = [];
  listLocation = [];
  rules: any = {};
  filterModel = {
    TextboxSearch: "",
  };
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
  ruleForm: TA_LocationByEmployeeDTO = {
    EmployeeATIDs: [],
    DepartmentList: [],
    LocationIndex: []
  };
  columnDefs = [
    {
      field: "index",
      sortable: true,
      pinned: false,
      headerName: "#",
      width: 80,
      checkboxSelection: true,
      headerCheckboxSelection: true,
      headerCheckboxSelectionFilteredOnly: true,
      display: true,
    },
    {
      field: "DepartmentName",
      headerName: this.$t("Department"),
      pinned: false,
      minWidth: 280,
      sortable: true,
      display: true,
    },
    {
      field: "EmployeeATID",
      headerName: this.$t("MCC"),
      pinned: false,
      minWidth: 300,
      sortable: true,
      display: true,
    },
    {
      field: "FullName",
      headerName: this.$t("Employee"),
      pinned: false,
      minWidth: 300,
      sortable: true,
      display: true,
    },
    {
      field: "LocationName",
      headerName: this.$t("LocationName"),
      pinned: false,
      minWidth: 300,
      sortable: true,
      display: true,
    },
    {
      field: "Address",
      headerName: this.$t("Address"),
      pinned: false,
      minWidth: 300,
      sortable: true,
      display: true,
    },
  ];

  initRule() {
    this.rules = {
      EmployeeATIDs: [
        {
          required: true,
          message: this.$t("PleaseSelectEmployee"),
          trigger: "blur",
        },
      ],
      LocationIndex: [
        {
          required: true,
          message: this.$t("PleaseSelectLocation"),
          trigger: "blur",
        },
      ],
    };
  }

  async beforeMount() {
    this.initRule();
    await this.loadData();
    this.LoadDepartmentTree();
    await this.loadLocationData();
    this.getEmployeesData();
  }

  LoadDepartmentTree() {
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
        this.employeeFullLookup = dictData;
        this.employeeFullLookupTemp = dictData;
        this.employeeFullLookupFilter = dictData;
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

  selectAllEmployeeForm(value) {
    (this.ruleForm as any).EmployeeATIDs = [...value];
    this.$forceUpdate();
    (this.$refs.fixedScheduleEmployee as any).validate();
  }

  displayPopupInsert() {
    this.showDialog = false;
  }
  reset() {
    const obj: TA_LocationByEmployeeDTO = {
      EmployeeATIDs: [],
      DepartmentList: [],
      LocationIndex: []
    };
    this.ruleForm = obj;

    this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee)?.map(
      (x) => ({
        Index: x.EmployeeATID,
        NameInFilter:
          x.EmployeeATID +
          (x.FullName && x.FullName != "" ? " - " + x.FullName : ""),
      })
    );
  }

  async loadData() {
    await taLocationByEmployee
      .GetLocationByEmployeeAtPage(
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

  onChangeDepartmentFilter(departments) {
    this.ruleForm.EmployeeATIDs = [];
    if (departments && departments.length > 0) {
      this.employeeFullLookupFilter = Misc.cloneData(
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
      this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee)?.map(
        (x) => ({
          Index: x.EmployeeATID,
          NameInFilter:
            x.EmployeeATID +
            (x.FullName && x.FullName != "" ? " - " + x.FullName : ""),
        })
      );
    }
  }

  onChangeDepartmentFilterSearch(departments) {
    this.selectAllEmployeeFilter = [];
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

  showOrHideImportError(obj) {
    this.showDialogImportError = obj;
  }
  async AddOrDeleteFromExcel(x) {
    if (x === "add") {
      this.isAddFromExcel = true;
      this.showDialogExcel = true;
      this.fileName = "";
    } else if (x === "close") {
      if (
        !isNullOrUndefined(
          <HTMLInputElement>document.getElementById("fileUpload")
        )
      ) {
        (<HTMLInputElement>document.getElementById("fileUpload")).value = "";
      }
      this.dataAddExcel = [];
      this.isAddFromExcel = false;
      this.isDeleteFromExcel = false;
      this.showDialogExcel = false;
      this.fileName = "";
    }
  }
  processFile(e) {
    if ((<HTMLInputElement>e.target).files.length > 0) {
      var file = (<HTMLInputElement>e.target).files[0];
      this.fileName = file.name;
      if (!isNullOrUndefined(file)) {
        var fileReader = new FileReader();
        var arrData = [];
        fileReader.onload = function(event) {
          var data = event.target.result;
          var workbook = XLSX.read(data, {
            type: "binary",
          });

          workbook.SheetNames.forEach((sheet) => {
            var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
            arrData.push(Array.from(rowObject));
          });
        };
        this.dataAddExcel = arrData;
        fileReader.readAsBinaryString(file);
      }
    }
  }
  UploadDataFromExcel() {
    this.importErrorMessage = "";
    var arrData = [];
    for (let i = 0; i < this.dataAddExcel[0].length; i++) {
      let a = Object.assign({});
      if (this.dataAddExcel[0][i].hasOwnProperty("MCC (*)")) {
        a.EmployeeATID = this.dataAddExcel[0][i]["MCC (*)"] + "";
      } else {
        this.$alertSaveError(
          null,
          null,
          null,
          this.$t("EmployeeATIDIsEmpty").toString()
        ).toString();
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Mã nhân viên")) {
        a.EmployeeCode = this.dataAddExcel[0][i]["Mã nhân viên"] + "";
      } else {
        a.EmployeeCode = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Tên nhân viên")) {
        a.FullName = this.dataAddExcel[0][i]["Tên nhân viên"] + "";
      } else {
        a.FullName = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Địa điểm (*)")) {
        a.LocationName = this.dataAddExcel[0][i]["Địa điểm (*)"] + "";
      }
      arrData.push(a);
    }

    taLocationByEmployee.AddLocationByEmployeeFromExcel(arrData).then((res) => {
      if (
        !isNullOrUndefined(
          <HTMLInputElement>document.getElementById("fileUpload")
        )
      ) {
        (<HTMLInputElement>document.getElementById("fileUpload")).value = "";
      }
      if (
        !isNullOrUndefined(
          <HTMLInputElement>document.getElementById("fileImageUpload")
        )
      ) {
        (<HTMLInputElement>document.getElementById("fileImageUpload")).value = "";
      }
      this.showDialogExcel = false;
      this.fileName = "";
      this.dataAddExcel = [];
      this.isAddFromExcel = false;
      if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") 
      {
        this.$saveSuccess();
        this.loadData();
      } else {
        this.importErrorMessage = this.$t("ImportLocationByEmployeeErrorMessage") + ": " + res.data.toString() + " " + this.$t("Employee");
        this.showOrHideImportError(true);
      }
    });
  }

  async onSubmitClick() {
    (this.$refs.ruleForm as any).validate(async (valid) => {
      if (!valid) return;
      if (this.isEdit) {
        return await taLocationByEmployee
          .UpdateLocationByEmployee(this.ruleForm)
          .then((res) => {
            if (res.data != true) {
              const msg = res.data;
              this.$alert(
                this.$t("MSG_DataRegisterExistedFromTo", {
                  data: msg,
                }).toString(),
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
        return await taLocationByEmployee
          .AddLocationByEmployee(this.ruleForm)
          .then((res) => {
            if (res.data) {
              const msg = res.data;
              this.$alert(
                this.$t("MSG_DataRegisterExistedFromTo", {
                  data: msg,
                }).toString(),
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
      await taLocationByEmployee
        .DeleteLocationByEmployee(listIndex)
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

  handleCommand(command) {
    if (command === "AddExcel") {
      this.AddOrDeleteFromExcel("add");
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
