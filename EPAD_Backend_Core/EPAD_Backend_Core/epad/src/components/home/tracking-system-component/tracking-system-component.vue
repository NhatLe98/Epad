<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TrackingSystems") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="change-department">
        <el-row class="tracking-system__custom-function-bar">
          <el-col :span="3" style="margin-right: 10px;">
            <el-date-picker
              v-model="fromTime"
              type="datetime"
              :placeholder="$t('SelectDateTime')"
            ></el-date-picker>
          </el-col>
          <el-col :span="3" style="margin-right: 10px;">
            <el-date-picker 
              v-model="toTime" 
              type="datetime" 
              :placeholder="$t('SelectDateTime')">
            </el-date-picker>
          </el-col>
          <el-col :span="1">
            <el-button id="btnFunction" round @click="Search" style="background-color: #122658; color: white;">
              {{ $t("View") }}
            </el-button>
          </el-col>
          <el-col :span="1" style="float: right;">
            <data-table-function-component
                        :showButtonInsert="false"
                        :isHiddenEdit="true" :isHiddenDelete="true"
                        :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
                        style="height: fit-content; display: flex; position: relative; top: 0; width: 100%;
                        margin-right: 0 !important;"
                      ></data-table-function-component>
          </el-col>
        </el-row>

        <div>
          <data-table-component class="tracking-system__data-table"
            ref="table"
            :get-data="getData"
            :columns="columns"
            :showCheckbox="false"
          >></data-table-component>
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
<script src="./tracking-system-component.ts"></script>
<style lang=scss>
.tracking-system__data-table {
  .filter-input {
    margin-right: 10px;
  }
  .el-table {
    height: calc(100vh - 194px) !important;
  }
}
</style>
