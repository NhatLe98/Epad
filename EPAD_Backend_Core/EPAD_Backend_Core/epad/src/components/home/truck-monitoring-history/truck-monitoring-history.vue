<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TruckMonitoringHistoryPage") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome devicebysomething">
        
      <el-row class="truck-monitoring-history__custom-function-bar" style="height: 40px !important;">
        <el-col :span="3" style="margin-left: 10px;">
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
        <el-col :span="3" style="margin-left: 10px;">
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
        <el-col :span="5">
          <el-input
            style="margin-left: 10px"
            :placeholder="$t('InputTripCodeDriverNameDriverCodePlate')"
            suffix-icon="el-icon-search"
            v-model="ruleObject.TextboxSearch"
            @keyup.enter.native="viewData()"
            class="filter-input"
          ></el-input>
        </el-col>
        <el-col :span="1" style="margin-left: 20px;">
          <el-button type="primary" style="height: 32px !important;" @click="viewData()">{{
            $t("View")
          }}</el-button>
        </el-col>
        <el-col :span="3" style="float: right">
          <el-button type="primary" @click="exportClick()">{{
            $t("ExportExcel")
          }}</el-button>
        </el-col>
      </el-row>
      <el-row>
        <data-table-function-component class="truck-monitoring-history__data-table-function"
          :showButtonInsert="false" :isHiddenEdit="true" :isHiddenDelete="true" :isHiddenSearch="true"
          :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
          >
          </data-table-function-component>
        <data-table-component class="truck-monitoring-history__data-table"
          :get-data="getData"
          ref="table"
          :columns="columns"
          :selectedRows.sync="rowsObj"
          :isShowPageSize="true"
          :showSearch="false"
          @showExtraDriver="showExtraDriver"
        ></data-table-component>
      </el-row>
      <el-dialog
        :title="$t('ExtraDriver')"
        :visible.sync="isShowExtraDriver"
        :before-close="cancelShowExtraDriver"
        :close-on-click-modal="false"
        width="50%"
        :append-to-body="true">
        <div>
          <!-- <el-button @click="exportToExcel">{{$t('Export')}}</el-button> -->
          <el-table
          height="40vh"
          :data="extraDriverData">
          <el-table-column v-for="col in extraDriverColumns" :key="col.ID"
            :prop="col.prop"
            :label="$t(col.label)"
            min-width="180">
          </el-table-column>
        </el-table>
        </div>
      </el-dialog>
    </el-main>
    </el-container>
  </div>
</template>

<script src="./truck-monitoring-history.ts" />

<style lang="scss">
.truck-monitoring-history__data-table {
  .el-table{
    height: calc(100vh - 189px) !important;
    margin-top: 11px;
  }

  .el-table__empty-text {
    width: auto;
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
  }
}
.truck-monitoring-history__department-select-tree {
  height: 32px !important;
  span .el-popover__reference-wrapper .el-input input {
    height: 32px !important;
  }
}

.truck-monitoring-history_employee-select
  .el-select__tags:has(span span:nth-child(1)) {
  top: 50% !important;
  //max-width: 90% !important;
  span span:first-child {
    width: max-content;
    max-width: calc(100% - 20%);
    margin-left: 1px;
    .el-select__tags-text {
      width: 100%;
      max-width: 100% !important;
    }
  }
}

.truck-monitoring-history_employee-select
  .el-select__tags:has(span span:nth-child(2)) {
  top: 50% !important;
  max-width: 80% !important;
  span span:first-child {
    width: max-content;
    max-width: calc(100% - 40%);
    margin-left: 1px;
    .el-select__tags-text {
      width: 100%;
      max-width: 100% !important;
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
