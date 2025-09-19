<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TAAjustAttendanceLogHistory") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">

        <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
          :placeholder="$t('SelectDepartment')" :data="tree.treeData" :props="tree.treeProps" :isSelectParent="true"
          :checkStrictly="tree.checkStrictly" :clearable="tree.clearable" :popoverWidth="tree.popoverWidth"
          v-model="SelectedDepartment" @change="onChangeDepartmentFilterSearch"
          style="padding: 0 10px; width: 15%; display: inline-block;position: relative;"></select-department-tree-component>
        <app-select-new class="ta-ajust-attendance-log-history__filter-employee" :dataSource="employeeFullLookup" displayMember="NameInFilter" valueMember="Index"
          :allowNull="true" v-model="EmployeeATIDsFilter" :multiple="true" style="width: 15%; padding-right: 10px"
          :placeholder="$t('SelectEmployee')" @getValueSelectedAll="selectAllEmployeeFilter" ref="employeeList">
        </app-select-new>

        <el-date-picker v-model="fromTime" style="width:200px;" type="datetime" format="dd/MM/yyyy HH:mm:ss"
          :placeholder="$t('SelectDateTime')"></el-date-picker>

        <el-date-picker v-model="toTime" style="width:200px;margin-left: 10px;" type="datetime" format="dd/MM/yyyy HH:mm:ss"
          :placeholder="$t('SelectDateTime')"></el-date-picker>
        <el-select v-model="selectedOption" filterable :placeholder="$t('SelectOption')" style="margin-left: 10px" multiple  collapse-tags clearable>
          <el-option v-for="item in allOptionLst" :key="item.value" :label="$t(item.label)"
            :value="item.value"></el-option>
        </el-select>

        <el-input style="padding-bottom:3px; width:238px;padding: 0 0px 0 0; margin-left: 10px; margin-right: 10px"
          :placeholder="$t('InputMCCMNVName')" v-model="filter" @keyup.enter.native="Search()" class="filter-input">
          <i slot="suffix" class="el-icon-search"></i>
        </el-input>

        <el-button id="btnFunction" round @click="Search" style="background-color: #122658; color: white;">
          {{ $t("View") }}
        </el-button>

        <div>
          <data-table-function-component :showButtonInsert="false" :isHiddenEdit="true" :isHiddenDelete="true"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"  style="height: fit-content; display: flex;  top: 20px; width: 10%;
                        margin-right: 0 !important;"></data-table-function-component>
          <data-table-component class="ac-log-history__data-table" ref="table" :showSearch="false" :get-data="getData"
            :columns="columns" :isShowPageSize="true" :showCheckbox="false"></data-table-component>
        </div>
        <!-- <div>
            <el-row>
              <el-col :span="12" class="left">
                <el-button class="classLeft" id="btnFunction" type="primary" round>{{ $t("Export") }}</el-button>
              </el-col>
              <el-col :span="12"></el-col>
            </el-row>
          </div> -->
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ta-ajust-attendance-log-history-component.ts"></script>
<style lang="scss">
.ac-log-history__data-table .el-table {
  height: calc(100vh - 208px) !important;
}

.ta-ajust-attendance-log-history__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.ta-ajust-attendance-log-history__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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