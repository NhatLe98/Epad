<template>
  <AgGridVue
    :style="{
      width: '100%',
      height: (heightAfterScale ? heightAfterScale : maxHeight) + 'px',
    }"
    class="ag-theme-alpine"
    :rowBuffer="rowBuffer"
    :columnDefs="columnDefs"
    :rowData="rowData"
    :height="heightAfterScale ? heightAfterScale : maxHeight"
    rowSelection="multiple"
    :rowHeight="rowHeight"
    :getRowStyle="getRowStyle"
    @grid-ready="onGridReady"
    @selectionChanged="onSelectionChange"
    @sortChanged="onSortChanged"
    @first-data-rendered="onFirstDataRendered"
    :columnTypes="columnTypes"
    v-loading="loading"
    :context="context"
    :localeText="{
      noRowsToShow: $t('NoData'),
    }"
  ></AgGridVue>
</template>
<script lang="ts">
import Vue from "vue";
import { AgGridVue } from "ag-grid-vue";
import "ag-grid-community/dist/styles/ag-grid.css";
import "ag-grid-community/dist/styles/ag-theme-alpine.css";
import { param } from "jquery";

export default Vue.extend({
  name: "VisualizeTable",
  props: {
    columnDefs: {
      type: Array,
      required: true,
    },
    rowData: {
      type: Array,
      required: true,
    },
    loading: {
      type: Boolean,
      required: false,
      default: false,
    },
    rowHeight: {
      type: Number,
      required: false,
      default: 32,
    },
    maxHeight: {
      type: Number,
      required: false,
      default: 450,
    },
    heightScale: {
      type: Number,
      required: false,
    },
    rowBuffer: {
      type: Number,
      required: false,
    },
    isKeepIndexAscending: {
      type: Boolean,
      required: false,
      default: false,
    },
    shouldResetColumnSortState: {
      type: Boolean,
      required: false,
      default: false,
    },
  },
  components: {
    AgGridVue,
  },

  data() {
    return {
      gridApi: null,
      gridColumnApi: null,
      originalRowData: null,
      resizeEvt: null,
      heightAfterScale: 0,
      columnTypes: null,
      context: null,
      getRowStyle: null,
    };
  },
  mounted() {
    if (this.heightScale) {
      this.heightAfterScale = window.innerHeight - this.heightScale;
      this.resizeEvt = () => {
        this.heightAfterScale = window.innerHeight - this.heightScale;
      };
      window.addEventListener("resize", this.resizeEvt);
    }
  },
  beforeMount() {
    this.context = {
      componentParent: this,
    };
  },
  beforeDestroy() {
    if (this.heightScale) {
      window.removeEventListener("resize", this.resizeEvt);
    }
  },
  created() {
    this.getRowStyle = (params) => {
      if (params.data.IsWarning) {
        return { backgroundColor: "#FEFBB3" };
      }
    };

    this.columnTypes = {
      editableColumn: {
        cellClass: (params) => {
          if (params.data.IsError) {
            return "red";
          }
          if (params.data.IsWarning) {
            return "yellow";
          }
        },

        cellStyle: (params) => {
          if (params.data.IsWarning) {
            return { backgroundColor: "#FEFBB3" };
          }
        },
      },
      centerContent: {
        headerClass: () => {
          return "center";
        },

        cellClass: (params) => {
          return "center";
        },

        cellStyle: (params) => {
          return { textAlign: "center", justifyContent: "center" };
        },
      },
    };
  },
  updated() {
    var a = document.getElementsByClassName("ag-center-cols-viewport");
    if (a != null && a.length > 0) {
      for (let i = 0; i < a.length; i++) {
        if (a[i].scrollWidth > a[i].clientWidth) {
          // (a[i] as HTMLElement).style.display = 'inline-block';
          if (a[i].children != null && a[i].children.length > 0) {
            for (let j = 0; j < a[i].children.length; j++) {
              (a[i].children[j] as HTMLElement).style.display = "inline-block";
            }
          }
        } else {
          // (a[i] as HTMLElement).style.width = '100%';
          if (a[i].children != null && a[i].children.length > 0) {
            for (let j = 0; j < a[i].children.length; j++) {
              (a[i].children[j] as HTMLElement).style.display = "contents";
            }
          }
        }
      }
    }
  },
  methods: {
    isCellEditable(params) {
      return true;
    },
    parentMethod() {
      this.gridApi.stopEditing();
    },
    onSelectionChange() {
      this.$emit("onSelectionChange", this.gridApi.getSelectedRows());
    },
    onGridReady(params) {
      this.gridApi = params.api;
      this.gridColumnApi = params.columnApi;
    },
    onFirstDataRendered() {
      // Clone new row data to original data array when first time render data
      this.originalRowData = Array.from(this.rowData);
    },
    onSortChanged(event) {
      if (this.isKeepIndexAscending) {
        // Get index array of rowData before sort
        const minIndex = Math.min(...this.rowData.map((x) => (x as any).index));
        // Get the sorted row data
        const sortedRowData = event.api.getModel().rowsToDisplay;
        // Re-assign index in ascending order for row data
        sortedRowData.forEach((element, index) => {
          element.data.index = minIndex + index;
        });
        // Update the underlying data source with the sorted data
        this.rowData.splice(0, this.rowData.length);
        this.rowData.push(...sortedRowData.map((x) => x.data));
      }
      if (
        this.gridColumnApi.columnModel.columnsForQuickFilter.every(
          (x) => !x.sort
        ) &&
        this.originalRowData != null &&
        this.originalRowData.length > 0
      ) {
        this.rowData.splice(0, this.rowData.length, ...this.originalRowData);
        const minIndex = Math.min(...this.rowData.map((x) => (x as any).index));
        // Get the sorted row data
        const sortedRowData = Array.from(this.rowData);
        // Re-assign index in ascending order for row data
        sortedRowData.forEach((element, index) => {
          (element as any).index = minIndex + index;
        });
        // Update the underlying data source with the sorted data
        this.rowData.splice(0, this.rowData.length, ...sortedRowData);
      }
    },
  },
  watch: {
    shouldResetColumnSortState(newVal) {
      // Reset column sort state
      this.gridApi.columnModel.columnsForQuickFilter.forEach((element) => {
        element.sort = null;
        element.sortIndex = null;
      });
      this.gridColumnApi.columnModel.columnsForQuickFilter.forEach(
        (element) => {
          element.sort = null;
          element.sortIndex = null;
        }
      );
      // Hide arrow sort icon in view
      var testarray = document.getElementsByClassName("ag-header-label-icon");
      for (var i = 0; i < testarray.length; i++) {
        testarray[i].className += " ag-hidden";
      }
      // Clone new row data to original data array
      this.originalRowData = Array.from(this.rowData);
      if (this.originalRowData != null && this.originalRowData.length > 0) {
        this.rowData.splice(0, this.rowData.length, ...this.originalRowData);
        const minIndex = Math.min(...this.rowData.map((x) => (x as any).index));
        // Get the sorted row data
        const sortedRowData = Array.from(this.rowData);
        // Re-assign index in ascending order for row data
        sortedRowData.forEach((element, index) => {
          (element as any).index = minIndex + index;
        });
        // Update the underlying data source with the sorted data
        this.rowData.splice(0, this.rowData.length, ...sortedRowData);
      }
    },
  },
});
</script>
<style lang="scss">
.ag-theme-alpine {
  $--color-divider: rgb(240, 240, 240);

  .ag-cell,
  .ag-cell > span {
    @extend .ag-table-cell;
    font-weight: normal;
    font-family: "Noto Sans";
    box-sizing: border-box;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: normal;
    padding: 8px;
    display: flex;
    align-items: center;
  }

  .ag-cell > span {
    padding: 0;
  }

  .ag-row-odd {
    background-color: #fff;
  }

  .ag-row-selected {
    background-color: unset;
  }

  .ag-row-hover {
    background-color: #f5f7fa;
  }

  .ag-table-cell {
    font-size: 12px;
    line-height: 16px;
    color: #606266;
  }

  .ag-header-cell {
    padding-left: 8px;
    padding-right: 8px;
  }

  .ag-row {
    border-color: $--color-divider;
  }

  .ag-header {
    background-color: white;
    border-bottom-color: $--color-divider;
  }

  .ag-pinned-left-header {
    border-right-color: $--color-divider;
  }

  .ag-cell.ag-cell-last-left-pinned:not(.ag-cell-range-right):not(
      .ag-cell-range-single-cell
    ) {
    border-right-color: $--color-divider;
  }

  .ag-root-wrapper {
    border: none;
  }

  .red > span {
    color: red;
  }

  .ag-header-cell.center {
    .ag-header-cell-label {
      justify-content: center;
    }
  }

  span.ag-header-cell-text {
    @extend .ag-table-cell;
    color: #909399;
    font-style: normal;
    font-weight: 600;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    user-select: none;
  }

  .ag-unselectable {
    user-select: unset;
  }
}
</style>
