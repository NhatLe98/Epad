<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("DevicesByService") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome devicebyservice">
        <el-row class="rowdeviceby" :gutter="20">
          <el-col :span="7" class="table-device-by">
              <el-table :data="serviceData"
              :height="maxHeight"
                highlight-current-row @current-change="handleCurrentChange"
                style="cursor:pointer;height:100%;"
                class="deviceby not-table-text-color"
              >
                <el-table-column prop="service" :label="$t('ListService')"></el-table-column>
              </el-table>
          </el-col>
          <el-col :span="17" class="transfer-device-by">
            <el-transfer v-model="value"
            :style="'height:' + maxHeight + 'px'"
            :titles="titles"
            :data="data"
            ></el-transfer>
          </el-col>
        </el-row>

        <el-row style="margin-top:10px;">
          <el-col :span="7">
            <!-- <el-button type="primary" @click="downloadDevice">{{$t("DownloadListDevice")}}</el-button> -->
            <p></p>
          </el-col>
          <el-col :span="17">
            <el-button type="success" @click="updateDevicesByService" style="float:right;">{{$t("Save")}}</el-button>
          </el-col>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>
<style>
.devicebyservice .el-table {
  margin-top: 0;
}
.devicebyservice .el-table.deviceby {
  border: 1px solid #ebeef5;
  
}
.devicebyservice .el-table__header-wrapper th {
  background-color: #F5F7FA;
}
.devicebyservice .el-transfer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.devicebyservice .el-transfer .el-transfer-panel {
  flex-grow: 1;
  width: 100%;
  height: 100%;
}
.devicebyservice .el-transfer .el-transfer__buttons {
  text-align: center;
  padding: 0 5px;
}
.devicebyservice .el-transfer-panel__body {
  height: calc(100% - 40px);
}
.devicebyservice .el-checkbox-group.el-transfer-panel__list {
  height: 100%;
}
/* .el-table th {
  background-color: #F5F7FA !important;
}

.el-transfer-panel {
  width: 45% !important;
}

.el-transfer__buttons {
  width: 8% !important;
}

.el-transfer__buttons .el-button {
  width: 100%;
}

.el-transfer-panel {
  height: 100%;
}

.el-transfer-panel__body {
height: calc(100% - 40px) !important;
}

.el-transfer-panel__list {
  height: 100% !important;
}

.el-main {
  overflow: hidden !important;
}
.el-table {
  margin-top: 0;
}
.rowdeviceby {
  height: 100%;
}
.table-device-by {
  height: 100%;
}
.el-table__body-wrapper {
  height: calc(100vh - 150px);
  overflow: auto !important;
} */

</style>
<script src="./devices-by-service-component.ts"></script>
