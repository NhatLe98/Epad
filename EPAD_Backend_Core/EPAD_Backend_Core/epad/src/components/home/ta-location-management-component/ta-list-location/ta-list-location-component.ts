import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { Component, Mixins } from "vue-property-decorator";
import TableToolbar from "@/components/home/printer-control/table-toolbar.vue";
import VisualizeTable from "@/components/app-component/visualize-table/visualize-table.vue";
import AppPagination from "@/components/app-component/app-pagination/app-pagination.vue";
import { isNullOrUndefined } from "util";
import LocationPopupComponent from "@/components/app-component/location-popup-component/location-popup-component.vue";
import TabBase from "@/mixins/application/tab-mixins";
import {
  TA_ListLocation,
  taListLocation,
} from "@/$api/ta-list-location-api";
import { ElForm } from "element-ui/types/form";

@Component({
  name: "ta-list-location",
  components: {
    HeaderComponent,
    VisualizeTable,
    AppPagination,
    TableToolbar,
    LocationPopupComponent,
  },
})
export default class ListLocationComponent extends Mixins(TabBase) {
  page = 1;
  showDialog = false;
  disabled = false;
  isEdit = false;
  rowsObj = [];
  columns = [];
  isLoading = false;
  shouldResetColumnSortState = false;
  showMapDialog = false;
  mapUrl = "";
  isAdding = false;
  filter = "";
  filterModel = {
    TextboxSearch: "",
  };
  ruleForm: TA_ListLocation = {
    LocationName: "",
    Address: "",
    Coordinates: "0,0",
    Radius: "20",
    Description: "",
  };

  rules: any = {};

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
      minWidth: 360,
      sortable: true,
      display: true,
    },
    {
      field: "Coordinates",
      headerName: this.$t("Coordinates"),
      pinned: false,
      minWidth: 250,
      sortable: true,
      display: true,
      cellRenderer: this.coordinatesCellRenderer,
    },
    {
      field: "Radius",
      headerName: this.$t("Radius"),
      pinned: false,
      minWidth: 250,
      sortable: true,
      display: true,
    },
    {
      field: "Description",
      headerName: this.$t("Description"),
      pinned: false,
      minWidth: 300,
      sortable: true,
      display: true,
    },
  ];

  coordinatesCellRenderer(params) {
    const coordinates = params.data.Coordinates.split(",");
    const lat = coordinates[0];
    const lng = coordinates[1];
    return `<a href="javascript:void(0);" data-lat="${lat}" data-lng="${lng}" class="view-coordinates">${this.$t("ViewCoordinates")}</a>`;
  }

  initRule() {
    this.rules = {
      LocationName: [
        {
          required: true,
          message: this.$t("PleaseInputLocationName"),
          trigger: "blur",
        },
      ],
      Coordinates: [
        {
          required: true,
          message: this.$t("PleaseInputCoordinates"),
          trigger: "blur",
        },
        {
          validator: (rule, value, calback) => {
            const coordinatesPattern = /^-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?$/;
            if (!coordinatesPattern.test(value)) {
              calback(this.$t("PleaseEnterValidCoordinates"));
            } else {
              calback();
            }
          },
          trigger: "blur",
        },
      ],
      Radius: [
        {
          required: true,
          message: this.$t("PleaseInputRadius"),
          trigger: "blur",
        },
        {
          validator: (rule, value, callback) => {
            if (isNaN(value)) {
              callback(this.$t("PleaseEnterValidNumber"));
            } else if (value < 20) {
              callback(this.$t("RadiusCannotBeLessThan20"));
            } else {
              callback();
            }
          },
          trigger: "blur",
        },
        {
          validator: (rule, value, callback) => {
            if (value < 20) {
              this.ruleForm.Radius = "20";
            }
            callback();
          },
          trigger: "blur",
        },
      ],
    };
  }

  async beforeMount() {
    this.initRule();
    this.addCoordinatesClickListener();
    await this.loadData();
  }

  addCoordinatesClickListener() {
    this.$nextTick(() => {
      const container = this.$el;
      container.addEventListener("click", (event) => {
        const targetElement = event.target as HTMLElement;
        if (targetElement.classList.contains("view-coordinates")) {
          const lat = targetElement.getAttribute("data-lat");
          const lng = targetElement.getAttribute("data-lng");
          if (lat && lng) {
            this.viewCoordinates(lat, lng);
          }
        }
      });
    });
  }

  viewCoordinates(lat, lng) {
    this.mapUrl = `https://www.google.com/maps?q=${lat},${lng}&z=15&output=embed`;
    this.showMapDialog = true;
  }

  displayPopupInsert() {
    this.showDialog = false;
    this.showMapDialog = false;
  }
  reset() {
    const obj: TA_ListLocation = {
      LocationName: "",
      Address: "",
      Coordinates: "0,0",
      Radius: "20",
      Description: "",
    };
    this.ruleForm = obj;
  }

  async loadData() {    
    return taListLocation
      .GetListLocationAtPage(this.page, this.pageSize, this.filterModel.TextboxSearch)
      .then((res: any) => {
        const { data } = res as any;
        if (res.status == 200 && res.data) {
          this.dataSource = data.data.map((x, idx) => {
            return {
              ...x,
              index: idx + 1 + (this.page - 1) * this.pageSize,
              LocationIndex: x.LocationIndex,
            };
          });
          this.total = data.total;
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
      if (this.isEdit == true) {
        return await taListLocation
          .UpdateLocation(this.ruleForm)
          .then((res) => {
            if (res.data != true) {
              this.$alert(
                this.$t("LocationExisted").toString() +
                  ". " +
                  this.$t("PleaseCheckAgain").toString(),
                this.$t("Notify").toString(),
                { dangerouslyUseHTMLString: true }
              );
            } else {
              this.loadData();
              this.showMapDialog = false;
              this.showDialog = false;
              this.reset();
              if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
              }
            }
          });
      } else {
        return await taListLocation
          .AddLocation(this.ruleForm)
          .then((res) => {
            if (res.data) {
              this.$alert(
                this.$t("LocationExisted").toString() +
                  ". " +
                  this.$t("PleaseCheckAgain").toString(),
                this.$t("Notify").toString(),
                { dangerouslyUseHTMLString: true }
              );
            } else {
              this.loadData();
              this.showMapDialog = false;
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
      this.showMapDialog = false;
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
    const listIndex = this.selectedRows.map((e) => e.LocationIndex);
    if (listIndex.length < 1) {
      this.$alertSaveError(
        null,
        null,
        null,
        this.$t("ChooseRowForDelete").toString()
      );
    } else {
      await taListLocation
        .DeleteLocation(listIndex)
        .then((res) => {
          console.log("🚀 ~ ListLocationComponent ~ .then ~ res:", res)
          if (res.data) {
            const msg = res.data;
            this.$alert(
              this.$t("LocationIsUsed", {
                data: msg,
              }).toString() + this.$t("PleaseCheckAgain").toString(),
              this.$t("Notify").toString(),
              { dangerouslyUseHTMLString: true }
            );
          } else {
            this.loadData();
            this.selectedRows = [];
            this.$deleteSuccess();
            if (!isNullOrUndefined(res.status) && res.status === 200) {
              this.$saveSuccess();
            }
          }
        })
        .catch(() => {})
        .finally(() => {
          this.showDialogDeleteUser = false;
          this.showMapDialog = false;
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
    this.showMapDialog = false;
  }

  onCancelLocationClick() {
    this.showMapDialog = false;
  }
}