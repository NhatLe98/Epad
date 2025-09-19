<template>
  <div class="tab" style="height: calc(100vh - 185px)">
    <div>
      <el-row>
        <el-col :span="4">
          <select-department-tree-component
            :defaultExpandAll="tree.defaultExpandAll"
            :multiple="tree.multiple"
            :placeholder="$t('SelectDepartment')"
            :disabled="tree.isEdit"
            :data="tree.treeData"
            :props="tree.treeProps"
            :isSelectParent="true"
            :checkStrictly="tree.checkStrictly"
            :clearable="tree.clearable"
            :popoverWidth="tree.popoverWidth"
            v-model="filterModel.Departments"
            @change="onChangeDepartmentFilter"
            style="width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="5" style="margin-left: 5px">
          <app-select-new class="ta-synthetic-attendance__filter-employee"
            :disabled="isEdit"
            :dataSource="employeeFullLookup"
            displayMember="NameInFilter"
            valueMember="Index"
            :allowNull="true"
            v-model="filterModel.EmployeeATIDs"
            :multiple="true"
            style="width: 100%"
            :placeholder="$t('SelectEmployee')"
            @getValueSelectedAll="selectAllEmployeeFilter"
            ref="employeeList"
          >
          </app-select-new>
        </el-col>
        <el-col :span="3" style="margin-left: 5px">
          <app-select-new class="ta-synthetic-attendance__filter-employee"
            :disabled="isEdit"
            :dataSource="listOptionFilterLog"
            displayMember="label"
            valueMember="value"
            :allowNull="true"
            v-model="filterModel.filerByLog"
            :multiple="false"
            style="width: 100%"
                :placeholder="$t('SelectTypeView')"
            @getValueSelectedAll="selectAllEmployeeFilter"
            ref="employeeList"
          >
          </app-select-new>
        </el-col>
        <el-col :span="3" style="margin-left: 5px">
          <el-date-picker
            v-model="filterModel.FromDate"
            type="date"
            format="dd/MM/yyyy"
            :placeholder="$t('FromDateString')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3" style="margin-left: 5px">
          <el-date-picker
            v-model="filterModel.ToDate"
            format="dd/MM/yyyy"
            type="date"
            :placeholder="$t('ToDateString')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="4" style="margin-left: 5px">
          <el-input
            :placeholder="$t('InputMCCMNVName')"
            v-model="filterModel.TextboxSearch"
            class="filter-input"
            @keyup.enter.native="viewBtn"
          >
            <i slot="suffix" class="el-icon-search" @click="viewBtn"></i>
          </el-input>
        </el-col>
        <el-col :span="1" style="margin-left: 5px">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="viewBtn"
          >
            {{ $t("View") }}
          </el-button>
        </el-col>
      </el-row>
    </div>

    <!-- <div class="tab-grid" style="height: calc(100% - 35px) !important">
      <VisualizeTable
        v-loading="isLoading"
        :columnDefs="columnDefs.filter((x) => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100% - 45px)"
      />
      <AppPagination
      ref="taHandleCalculatePagination"
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      />
    </div> -->
    <div
      class="tab-grid"
      style="height: calc(100% - 10px) !important; margin-top: 10px"
    >
      <el-table
        :data="listData"
        style="width: 100%"
        border
        :height="700"
        @selection-change="handleSelectionChange"
        @row-click="handleRowClick"
        :selected-rows.sync="rowsObj"
        row-key="EmployeeATID"
        v-loading="isLoading"
        class="synthetic-time-attendance__table"
      >
        <template v-for="column in listColumn">
          <el-table-column
            :prop="column.dataField"
            :label="column.name"
            :key="column.dataField"
            align="center"
            :min-width="column.index <= 4 || column.index >= 50 ? 200 : 80"
            v-if="column.index <= 4"
            fixed
          >
          <template slot-scope="scope">
              <span >{{
                scope.row[column.dataField]
              }}</span>
             
            </template>
          </el-table-column>
          <el-table-column
            :prop="column.dataField"
            :label="column.name"
            :key="column.dataField"
            align="center"
            :min-width="column.index <= 4 || column.index >= 50 ? 150 : 80"
            v-else
          >
            <template slot-scope="scope">
              <span >{{
                scope.row[column.dataField]
              }}</span>
             
            </template>
          </el-table-column>
        </template>
      </el-table>
   
    </div>
  </div>
</template>
  <script src="./ta-synthetic-attendance.ts"></script>
  <style   lang="scss" scoped>
.hozitalClass {
  display: flex;
  flex-direction: row;
}
.hozitalClass:after {
  content: "";
  flex: 1 1;
  border-bottom: 1px solid #000;
  margin: auto;
}
.ta-synthetic-attendance__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
  top: 50% !important;
  max-width: 100% !important;

  span span:first-child {
    width: max-content;
    max-width: calc(100% - 20%);
    margin-left: 1px;

    .el-select__tags-text {
      vertical-align: top;
      width: max-content;
      max-width: 90%;
      display: inline-block;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }
}

.ta-synthetic-attendance__filter-employee .el-select__tags:has(span span:nth-child(2)) {
  top: 50% !important;
  max-width: 80% !important;

  span span:first-child {
    width: max-content;
    max-width: calc(100% - 30%);
    margin-left: 1px;

    .el-select__tags-text {
      vertical-align: top;
      width: max-content;
      max-width: 80%;
      display: inline-block;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }

  span span:nth-child(2) {
    max-width: 30%;
    margin-left: 1px;

    .el-select__tags-text {
      width: 100%;
      max-width: 100% !important;
    }
  }
}

/deep/ .synthetic-time-attendance__table{
  width: 100% !important; 
  height: calc(100vh - 260px) !important;
  .el-table__body-wrapper{
    max-height: calc(100vh - 305px) !important;
    .el-table__empty-text{
      position: fixed;
      top: calc(50vh + 51px);
      left: calc(50vw - 260px);
      z-index: 1;
    }
  }
  .el-table__fixed{
    max-height: calc(100vh - 270px) !important;
    .el-table__fixed-body-wrapper{
      top: 49px !important;
      max-height: calc(100vh - 270px) !important;
    }
  }
}
</style>
