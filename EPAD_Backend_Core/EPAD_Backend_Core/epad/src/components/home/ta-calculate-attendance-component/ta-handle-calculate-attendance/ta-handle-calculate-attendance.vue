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
            v-model="SelectedDepartment"
            @change="onChangeDepartmentFilter"
            style="width: 100%"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="4" style="margin-left: 10px">
          <app-select-new class="ta-handle-calculate-attendance__filter-employee"
            :disabled="isEdit"
            :dataSource="employeeFullLookup"
            displayMember="NameInFilter"
            valueMember="Index"
            :allowNull="true"
            v-model="EmployeeATIDs"
            :multiple="true"
            style="width: 100%"
            :placeholder="$t('SelectEmployee')"
            @getValueSelectedAll="selectAllEmployeeFilter"
            ref="employeeList"
          >
          </app-select-new>
        </el-col>
        <el-col :span="3" style="margin-left: 10px">
          <el-date-picker
            v-model="FromTime"
            type="date"
            format="dd/MM/yyyy"
            :placeholder="$t('FromDateString')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="3" style="margin-left: 10px">
          <el-date-picker
            v-model="ToTime"
            type="date"
            format="dd/MM/yyyy"
            :placeholder="$t('ToDateString')"
            style="width: 100%"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="4" style="margin-left: 10px">
          <el-input
            :placeholder="$t('InputMCCMNVName')"
            v-model="filter"
            class="filter-input"
            @keyup.enter.native="btnView"
          >
            <i slot="suffix" class="el-icon-search" @click="btnView"></i>
          </el-input>
        </el-col>
        <el-col :span="1" style="margin-left: 10px">
          <el-button
            type="primary"
            class="smallbutton"
            size="small"
            @click="btnView"
          >
            {{ $t("View") }}
          </el-button>
        </el-col>
        
        <el-col :span="1" style="margin-left: 10px">
          <el-tooltip
            class="item"
            effect="dark"
            :content="$t('PleaseSelectEmployeeToCalculateAttendance')"
            placement="bottom-end"
            v-if="!EmployeeATIDs || EmployeeATIDs.length == 0"
          >
            <el-button plain @click="calculateAttendance" disabled>
              {{ $t("CalculateAttendance") }}
            </el-button>
          </el-tooltip>

          <el-button plain @click="calculateAttendance" v-else>
            {{ $t("CalculateAttendance") }}
          </el-button>
        </el-col>
        <el-col :span="1" style="margin-left: 10px">
        <data-table-function-component :showButtonInsert="false" :isHiddenEdit="true" :isHiddenDelete="true"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columnDefs"  style="height: fit-content; display: flex;  top: 0; width: 10%;
                        margin-right: 0 !important;"></data-table-function-component>
                        </el-col>
      </el-row>
    </div>

    <div class="tab-grid" style="margin-top: 10px;">
      <VisualizeTable
        v-loading="isLoading"
        :columnDefs="columnDefs.filter((x) => x.display)"
        :rowData="dataSource"
        :rowHeight="40"
        @onSelectionChange="onSelectionChange"
        :heightScale="260"
        :isKeepIndexAscending="true"
        :shouldResetColumnSortState="shouldResetColumnSortState"
        style="height: calc(100vh - 272px)"
      />
      <AppPagination
      ref="taHandleCalculatePagination"
        :page.sync="page"
        :pageSize.sync="pageSize"
        :getData="loadData"
        :total="total"
      />
    </div>
  </div>
</template>
  <script src="./ta-handle-calculate-attendance.ts"></script>
  <style lang="scss">
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

.ta-handle-calculate-attendance__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.ta-handle-calculate-attendance__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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
</style>