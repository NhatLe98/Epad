<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("GatesMonitoringHistory") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome devicebysomething">
        
      <el-row >
        <el-col :span="3">
          <select-department-tree-component
            class="gates-monitoring-history__department-select-tree"
            :placeholder="$t('SelectDepartment')"
            :multiple="multiple"
            :defaultExpandAll="defaultExpandAll"
            :disabled="isEdit"
            :isSelectParent="true"
            :data="treeData"
            :props="treeProps"
            :checkStrictly="checkStrictly"
            :clearable="clearable"
            :popoverWidth="popoverWidth"
            v-model="ruleObject.DepartmentIndexes"
            @change="onDeparmentChange"
          ></select-department-tree-component>
        </el-col>
        <el-col :span="4" style="margin-left: 10px;">
          <app-select-new class="gates-monitoring-history_employee-select"
                  :dataSource="listemployeeFiltered"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="EmployeeATIDs"
                  :multiple="true"
                  style="width: 100%;"
                  :placeholder="$t('SelectEmployee')"
                  @getValueSelectedAll="selectAllEmployeeFilter"
                  ref="employeeList"
                >
                </app-select-new>
        </el-col>
        <el-col :span="4" style="margin-left: 10px;">
          <el-date-picker
            v-model="ruleObject.FromTime"
            type="datetime"
            :placeholder="$t('FromDate')"
            style="width: 100%;"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="4" style="margin-left: 10px;">
          <el-date-picker
            v-model="ruleObject.ToTime"
            type="datetime"
            :placeholder="$t('ToDate')"
            style="width: 100%;"
            :clearable="false"
            :editable="false"
          >
          </el-date-picker>
        </el-col>
        <el-col :span="4" style="margin-left: 10px;" >
          <app-select
          style="width: 100%;"
            :dataSource="statusList"
            :placeholder="$t('Status')"
            v-model="ruleObject.StatusLog"
            displayMember="Name"
            valueMember="Value"
          >
          </app-select>
        </el-col>
        <el-col :span="1" style="margin-left: 10px;">
          <el-button type="primary" @click="viewData()">{{
            $t("View")
          }}</el-button>
        </el-col>
        <el-col :span="2" style="float: right">
          <el-button type="primary" @click="exportClick()">{{
            $t("ExportExcel")
          }}</el-button>
        </el-col>
      </el-row>
      <el-row>
        <data-table-component class="gates-monitoring-history__data-table"
          :get-data="getData"
          ref="table"
          :columns="columns"
          :selectedRows.sync="rowsObj"
          :isShowPageSize="true"
          :showSearch="false"
        ></data-table-component>
      </el-row>
    </el-main>
    </el-container>
  </div>
</template>

<script src="./gates-monitoring-history.ts" />\

<style lang="scss">
.gates-monitoring-history__data-table {
  .el-table{
    height: calc(100vh - 205px) !important;
  }

  .el-table__empty-text {
    width: auto;
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
  }
}
.gates-monitoring-history__department-select-tree {
  height: 32px !important;
  span .el-popover__reference-wrapper .el-input input {
    height: 32px !important;
  }
}

.gates-monitoring-history_employee-select {
  .el-select__tags:has(span span:nth-child(1)) {
    top: 50% !important;
    max-width: 90% !important;

    span span:first-child {
      width: max-content;
      max-width: calc(100% - 20%);
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
  }
}

.gates-monitoring-history_employee-select {
  .el-select__tags:has(span span:nth-child(2)) {
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
}
</style>
