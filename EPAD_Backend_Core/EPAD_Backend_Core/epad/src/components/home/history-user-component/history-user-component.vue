<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("HistoryUser") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="history-user">
        <el-row class="history-user__custom-function-bar">
          <el-col :span="3">
            <el-select
                reserve-keyword
                v-model="filteredStatus"
                :placeholder="$t('Status')"
                @change="onChangeFilteredStatus"
                multiple
                filterable
                clearable
                default-first-option
                style="float: left; margin-right: 10px"
              >
                <el-option
                  v-for="item in statusOptions"
                  :key="item"
                  :label="$t(item)"
                  :value="item"
                ></el-option>
              </el-select>
          </el-col>
          <el-col :span="3">
            <el-date-picker
                v-model="fromTime"
                type="datetime"
                style="margin-right: 10px"
                id="inputFromDate"
                :placeholder="$t('SelectDateTime')"
              ></el-date-picker>
        </el-col>
        <el-col :span="3">
          <el-date-picker
                v-model="toTime"
                style="margin-right: 10px"
                type="datetime"
                :placeholder="$t('SelectDateTime')"
              ></el-date-picker>
        </el-col>
        <el-col :span="1">
          <el-button type="primary" @click="View" style="height: 32px;"
            class="history-user__view-btn">{{
                $t("View")
              }}</el-button>
        </el-col>
        </el-row>
        <div class="table">
            <data-table-function-component class="history-user__data-table-function"
              @delete="Delete"
              v-bind:isHiddenEdit="true"
              v-bind:isHiddenDelete="false"
              v-bind:showButtonCustom="false"
              v-bind:showButtonInsert="false"
              :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
            ></data-table-function-component>
            <data-table-component class="history-user__data-table"
              ref="table"
              :get-data="getData"
              :columns="columns"
              :showCheckbox="true"
              v-bind:isFromHasView="true"
              :selectedRows.sync="rowsObj"
              :isShowPageSize="true"
            ></data-table-component>
          </div>
        <!-- <div>
                  <el-row>
                    <el-col :span="12" class="left">
                      <el-button class="classLeft" id="btnFunction" type="primary" round>{{ $t("Export") }}</el-button>
                    </el-col>
                    <el-col :span="12"></el-col>
                  </el-row>
                </div>-->
      </el-main>
    </el-container>
  </div>
</template>
<script src="./history-user-component.ts"></script>
<style lang="scss">
// .history-user__custom-function-bar {
//   display: flex;
//   flex-wrap: wrap;
//   position: relative;
// }
// .history-user__custom-function-bar > * {
//   flex: 0 1 auto; 
// }
// .history-user__data-table {
//   // position: relative;
// }

.history-user {
  margin-top: 0;
}
.history-user__data-table {
  .filter-input {
    margin-right: 10px;
  }
  .el-table {
    margin-top: 0 !important;
    height: calc(100vh - 194px) !important;
  }
}
.history-user__view-btn{
  span{
    line-height: 18px;
  }
}
.history-user__data-table-function {
  position: unset !important;
  display: flex !important;
  margin-bottom: 5px;
  justify-content: space-between !important;
  width: 100% !important;
}
</style>
