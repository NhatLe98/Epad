import { Component, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";
import { isNullOrUndefined } from "util";
import { Form as ElForm } from "element-ui";
import { hrUserApi } from "@/$api/hr-user-api";
import * as XLSX from "xlsx";
import { departmentApi } from "@/$api/department-api";
import moment from "moment";
import SelectDepartmentTreeComponent from "@/components/app-component/select-department-tree-component/select-department-tree-component.vue";
import SelectTreeComponent from "@/components/app-component/select-tree-component/select-tree-component.vue";
import {
  employeeStoppedApi,
  IC_EmployeeStoppedDTO,
} from "@/$api/employee-stopped-api";

@Component({
  name: "employee-stopped",
  components: {
    HeaderComponent,
    DataTableComponent,
    DataTableFunctionComponent,
    SelectDepartmentTreeComponent,
    SelectTreeComponent,
  },
})
export default class EmployeeStoppedComponent extends Mixins(ComponentBase) {
  page = 1;
  showDialog = false;
  columns = [];
  rowsObj = [];
  departments = [];
  isEdit = false;
  disabled = false;
  formExcel = {};
  listExcelFunction = ["AddExcel"];
  dataAddExcel = [];
  isAddFromExcel = false;
  showDialogExcel = false;
  isDeleteFromExcel = false;
  showDialogImportError = false;
  listAllEmployee = [];
  importErrorMessage = "";
  employeeFullLookup = {};
  employeeFullLookupTemp = {};
  employeeFullLookupFilter = {};
  fileName = "";
  listAllEmployeeStopped: any = [];
  SelectedDepartment = [];
  selectAllEmployeeFilter = [];
  stoppedDateFilter = new Date();
  ruleEmployeeStopped: IC_EmployeeStoppedDTO = {
    EmployeeATIDs: [],
    StoppedDate: new Date(),
    Reason: "",
  };
  rules: any = {};

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
      value: "ID",
      children: "ListChildrent",
      label: "Name",
    },
  };

  beforeMount() {
    this.initRule();
    this.setColumns();
    this.getEmployeesData();
    this.LoadDepartmentTree();
  }

  setColumns() {
    this.columns = [
      {
        prop: "EmployeeATID",
        label: "MCC",
        minWidth: 200,
        display: true,
      },
      {
        prop: "DepartmentName",
        label: "Department",
        minWidth: 300,
        display: true,
      },
      {
        prop: "FullName",
        label: "Employee",
        minWidth: 300,
        display: true,
      },
      {
        prop: "StoppedDateString",
        label: "StoppedDate",
        minWidth: 300,
        display: true,
      },
      {
        prop: "Reason",
        label: "Reason",
        minWidth: 300,
        display: true,
      },
    ];
  }

  initRule() {
    this.rules = {
      EmployeeATIDs: [
        {
          required: true,
          message: this.$t("PleaseSelectEmployee"),
          trigger: "blur",
        },
      ],
      StoppedDate: [
        {
          required: true,
          message: this.$t("PleaseInputStoppedDate"),
          trigger: "blur",
        },
      ],
      Reason: [
        {
          required: true,
          message: this.$t("PleaseInputStoppedReason"),
          trigger: "blur",
        },
      ],
    };
  }

  mounted() {
  }

  async getData({ page, filter, sortParams, pageSize }) {
    this.page = page;
    return await employeeStoppedApi
      .GetEmployeeStopped(page, filter, pageSize)
      .then((res: any) => {
        if (res.status == 200) {
          const data = res as any;
          const arrTemp = [];
          data.data.data.forEach((item) => {
            const a = Object.assign(item, {
              StoppedDateString: moment(item.StoppedDate).format("DD-MM-YYYY"),
            });
            arrTemp.push(a);
          });
          return {
            data: arrTemp,
            total: data.data.total,
          };
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
        fileReader.onload = function (event) {
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
        a.EmployeeATID = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Phòng ban")) {
        a.Name = this.dataAddExcel[0][i]["Phòng ban"] + "";
      } else {
        a.Name = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Họ tên (*)")) {
        a.FullName = this.dataAddExcel[0][i]["Họ tên (*)"] + "";
      } else {
        a.FullName = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Ngày nghỉ việc (*)")) {
        a.StoppedDate = this.dataAddExcel[0][i]["Ngày nghỉ việc (*)"] + "";
      } else {
        a.StoppedDate = "";
      }
      if (this.dataAddExcel[0][i].hasOwnProperty("Lý do (*)")) {
        a.Reason = this.dataAddExcel[0][i]["Lý do (*)"] + "";
      } else {
        this.$alertSaveError(
          null,
          null,
          null,
          this.$t("ReasonForLeavingCannotBeLeftBlank").toString()
        ).toString();
      }
      arrData.push(a);
    }

    employeeStoppedApi.AddEmployeeStoppedFromExcel(arrData).then((res) => {
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
        (<HTMLInputElement>document.getElementById("fileImageUpload")).value =
          "";
      }
      (this.$refs.table as any).getTableData(this.page, null, null);
      this.showDialogExcel = false;
      this.fileName = "";
      this.dataAddExcel = [];
      this.isAddFromExcel = false;
      if (
        !isNullOrUndefined(res.status) &&
        res.status === 200 &&
        res.data == ""
      ) {
        this.$saveSuccess();
      } else {
        this.importErrorMessage =
          this.$t("ImportEmployeeStoppedErrorMessage") +
          res.data.toString() +
          " " +
          this.$t("EmployeeStopped");
        this.showOrHideImportError(true);
      }
    });
  }
  showOrHideImportError(obj) {
    this.showDialogImportError = obj;
  }

  displayPopupInsert() {
    this.showDialog = false;
  }

  Search() {
    (this.$refs.table as any).getTableData(1, null, null);
  }

  selectAllEmployeeForm(value) {
    (this.ruleEmployeeStopped as any).EmployeeATIDs = [...value];
    this.$forceUpdate();
    (this.$refs.fixedScheduleEmployee as any).validate();
  }

  onChangeDepartmentFilter(departments) {
    this.ruleEmployeeStopped.EmployeeATIDs = [];
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

  LoadDepartmentTree() {
    departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
      if (res.status == 200) {
        this.tree.treeData = res.data;
      }
    });
  }

  reset() {
    const obj: any = {
      EmployeeATIDs: [],
      StoppedDate: new Date(),
      Reason: "",
    };
    this.ruleEmployeeStopped = obj;

    this.employeeFullLookupFilter = Misc.cloneData(this.listAllEmployee)?.map(
      (x) => ({
        Index: x.EmployeeATID,
        NameInFilter:
          x.EmployeeATID +
          (x.FullName && x.FullName != "" ? " - " + x.FullName : ""),
      })
    );
  }

  Insert() {
    this.showDialog = true;
    this.isEdit = false;
    this.reset();
  }

  async Submit() {
    this.ruleEmployeeStopped.StoppedDateString = moment(
      new Date(this.ruleEmployeeStopped.StoppedDate)
    ).format("DD-MM-YYYY");

    (this.$refs.ruleEmployeeStopped as any).validate(async (valid) => {
      if (!valid) return;
      else {
        if (this.isEdit == true) {
          return await employeeStoppedApi
            .UpdateEmployeeStopped(this.ruleEmployeeStopped)
            .then((res) => {
              (this.$refs.table as any).getTableData(this.page, null, null);
              this.showDialog = false;
              this.reset();
              if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
              }
            });
        } else {
          return await employeeStoppedApi
            .AddEmployeeStopped(this.ruleEmployeeStopped)
            .then((res) => {
              (this.$refs.table as any).getTableData(this.page, null, null);
              this.showDialog = false;
              this.reset();
              if (res.status === 200 && res.data) {
                const msg = res.data;
                this.$alert(
                  this.$t("MSG_DataRegisterExistedFromTo", {
                    data: msg,
                  }).toString(),
                  this.$t("Notify").toString(),
                  { dangerouslyUseHTMLString: true }
                );
              } else if (res.status === 200) {
                this.$saveSuccess();
              }
            })
            .catch(() => {});
        }
      }
    });
  }

  Edit() {
    this.isEdit = true;
    var obj = JSON.parse(JSON.stringify(this.rowsObj));
    if (obj.length > 1) {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("MSG_SelectOnlyOneRow").toString()
      );
    } else if (obj.length === 1) {
      this.showDialog = true;
      this.ruleEmployeeStopped = obj[0];
      this.ruleEmployeeStopped.EmployeeATIDs = [
        this.ruleEmployeeStopped.EmployeeATID,
      ];
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
    const obj = JSON.parse(JSON.stringify(this.rowsObj.map((x) => x.Index)));
    if (obj.length < 1) {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("ChooseRowForDelete").toString()
      );
    } else {
      this.$confirmDelete().then(async () => {
        await employeeStoppedApi
          .DeleteEmployeeStopped(obj)
          .then((res) => {
            (this.$refs.table as any).getTableData(this.page, null, null);
            if (!isNullOrUndefined(res.status) && res.status === 200) {
              this.$deleteSuccess();
            }
          })
          .catch(() => {});
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

  Cancel() {
    var ref = <ElForm>this.$refs.ruleEmployeeStopped;
    ref.resetFields();
    this.showDialog = false;
  }
}
