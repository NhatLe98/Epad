<template>
   <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("DoorDevice") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <el-row class="rowdeviceby" :gutter="20">
          <el-col :span="7" class="table-device-by">
            <el-table :data="departmentData"
              :height="maxHeight"
              highlight-current-row @current-change="handleCurrentChange" 
              style="cursor:pointer;"
              class="deviceby"
            >
              <el-table-column prop="service" :label="$t('DoorList')"></el-table-column>
            </el-table>
          </el-col>
          <el-col :span="17" class="transfer-device-by">
            <el-transfer v-model="value"
            :style="'height:' + maxHeight + 'px'"
            :titles="[$t('ListDevicesSelected'), $t('ListDevicesUnSelected')]"
            :data="data"
            >
            </el-transfer>
          </el-col>
        </el-row>
        <el-row style="margin-top:10px;">
          <el-col :span="7">
            <el-button type="primary" @click="downloadDevice">{{$t("DownloadListDevice")}}</el-button>
          </el-col>
          <el-col :span="17">
            <el-button type="success" style="float: right"
            @click="updateDevicesByDepartment"
            >
              {{$t("Save")}}
            </el-button>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<style>
.table-device-by, .table-device-by .el-table {
  margin-top: 0;
}
.el-table.deviceby {
  border: 1px solid #ebeef5;
  
}
.el-table__header-wrapper th {
  background-color: #F5F7FA;
}
.el-transfer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.el-transfer .el-transfer-panel {
  flex-grow: 1;
  width: 100%;
  height: 100%;
}
.el-transfer .el-transfer__buttons {
  text-align: center;
  padding: 0 5px;
}
.el-transfer-panel__body {
  height: calc(100% - 40px);
}
.el-checkbox-group.el-transfer-panel__list {
  height: 100%;
}
.el-button {
  margin-left: 0;
}
/* .el-table th{
  background-color:#F5F7FA !important;
}
.el-transfer-panel{
  width:45% !important;
}
.el-transfer__buttons{
  width:8% !important;
}
.el-transfer__buttons .el-button{
  width:100%;
}
.el-transfer-panel{
  height: 100%;
}
.el-transfer-panel__body{
  height: calc(100% - 40px) !important;
}
.el-transfer-panel__list{
  height: 100% !important;
}
.el-main{
  overflow: hidden !important;
}
.el-table {
  margin-top: 0;
}
.rowdeviceby {
  height: calc(100vh - 150px);
}
.table-device-by {
  height: calc(100vh - 150px);
}
.el-table__body-wrapper {
  height: calc(100vh - 190px);
  overflow: auto !important;
}
div.cell {
  word-break: break-all;
  text-align: justify;
}
.el-table__body {
  width: auto !important;
}
.el-checkbox.el-transfer-panel__item {
  width: fit-content;
} */
</style>
<script src="./ac-door-device-component.ts"></script>
