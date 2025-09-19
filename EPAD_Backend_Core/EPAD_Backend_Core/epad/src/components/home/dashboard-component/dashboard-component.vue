<template>
  <div id="bgDashboard">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("Home") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent @changeLang="changeLanguage"/>
          </el-col>
        </el-row>
      </el-header>
      
      <el-main class="bgDashboard main" style="padding-bottom: 0;">
        <el-row>
          <el-col :span="6" v-if="isEdit">
            <div style="width: 100%; height: calc(100vh - 100px); border: 1px solid transparent; overflow-y: auto;">
              <el-collapse :v-model="module" v-for="(module, i) in dashboardModules" v-bind:key="module">
                <el-collapse-item :title="$t('Dashboard') + ' ' + module" :name="'Dashboard ' + module" 
                  class="dashboard-collapse">
                  <div v-for="(item, i) in listChart" v-bind:key="item.id" v-if="item.module == module"
                    draggable="true" @dragstart="dragStart(item.id, $event)" @dragover.prevent 
                    @dragenter="dragEnter" @dragleave="dragLeave" @dragend="dragEnd" 
                    @drop="dragFinish(i, $event)"
                    style="cursor: pointer; margin-top: 5px; margin-bottom: 5px; padding-left: 5px;"
                    :title="$t(item.name)">
                      <span style="display: inline-block; width: 75%; white-space: nowrap; overflow: hidden;
                        text-overflow: ellipsis; ">{{ $t(item.name) }}</span>
                      <span style="height: 25px;width: 25px;background-color: aqua;border-radius: 50%; 
                        display: inline-block; text-align: center; right: 0 !important; float: right; margin-right: 5px;"
                      >
                        {{ item.selected }}
                      </span>
                  </div>
                </el-collapse-item>
              </el-collapse>
            </div>
          </el-col>
          <el-col :span="isEdit ? 18 : 24">
            <div style="width: 100%; height: calc(5vh); border: 1px solid transparent;">
              <el-button v-if="!isEdit" style="float: right;" type="primary" @click="editDashboard">{{ $t('Edit') }}</el-button>
              <el-button v-if="isEdit" style="float: right;" type="primary" @click="saveDashboard">{{ $t('Save') }}</el-button>
            </div>
            <div style="width: 100%; height: calc(10vh); border: 1px solid transparent; overflow-y: hidden;">
              <el-row>
                <el-col v-if="onlineDevicePercentage > 0" :style="{width: `${viewOnlineDevicePercentage}% !important`}"
                  style="cursor: pointer;">
                  <div style="text-align: center; border-bottom: 5px solid blue; color: blue; font-weight: bold; 
                    margin-right: 5px;" @click="showOnOffDeviceData('online')">
                    {{ $t('OnlineMachine') }}<br/>
                    <span style="color: black; font-weight: normal;">{{ onlineDeviceName }}</span>
                  </div>
                </el-col>
                <el-col v-if="offlineDevicePercentage > 0" :style="{width: `${viewOfflineDevicePercentage}% !important`}" 
                  style="cursor: pointer;">
                  <div style="text-align: center;  border-bottom: 5px solid red; color: red; font-weight: bold;"
                    @click="showOnOffDeviceData('offline')">
                    {{ $t('OfflineMachine') }}<br/>
                    <span style="color: black; font-weight: normal;">{{ offlineDeviceName }}</span>
                  </div>
                </el-col>
              </el-row>
            </div>
            <!-- <span>{{ layout }}</span><br/>
            <span>{{ listSelectedChart }}</span>             -->
            <div style="width: 100%; height: calc(85vh - 90px); border: 1px solid transparent; overflow-y: scroll; 
              background-color: #d4d8dc; border-radius: 10px;" 
              @dragover.prevent @drop="dragFinish(-1, $event)">
              <grid-layout :layout.sync="layout"
                :col-num="12"
                :row-height="30"
                :is-draggable="true"
                :is-resizable="true"
                :vertical-compact="true"
                :use-css-transforms="true"
                :auto-size="true"
              >
                <grid-item v-for="(item, index) in layout"
                  v-bind:key="item.id"
                  :static="item.static"
                  :x="item.x"
                  :y="item.y"
                  :w="item.w"
                  :h="item.h"
                  :i="index.toString()"
                  style="-moz-user-select: -moz-none;-khtml-user-select: none;-webkit-user-select: none;
                    -ms-user-select: none;user-select: none;"
                  :style="enableChartLevelBackground ? (item.dataConfig && item.updateLevelBackgroundColor ? (item.isDanger ? ('background-color: ' + item.dataConfig.dangerBackgroundColor  + ' !important') 
                    : (!item.isSafe ? ('background-color: ' + item.dataConfig.warningBackgroundColor  + ' !important') 
                    : ('background-color: ' + item.dataConfig.safeBackgroundColor  + ' !important'))) : '') : ''"
                >
                <div style="width: 100% !important; height: 5% !important;">
                  <span v-if="isEdit" style="float: right;margin-right: 5px; cursor: pointer;" @click="closeChart(listSelectedChart[index].index)">X</span>
                </div>
                  <component
                    style="width: 100% !important; height: 95% !important;"
                    :is="listSelectedChart[index].component"
                    :index="listSelectedChart[index].index"
                    :name="listSelectedChart[index].name"
                    :chartId="listSelectedChart[index].id"
                    :dataConfig="listSelectedChart[index].dataConfig"
                    @chartLevelBackgroundColor="handleChartLevelBackgroundColor"
                  />
                </grid-item>
              </grid-layout>
            </div>
          </el-col>
        </el-row>
      </el-main>
    </el-container>

    <el-dialog
      :title="$t('ListDeviceName')"
      :visible.sync="isShowOnOffDeviceData"
      width="70%"
      :append-to-body="true">
      <div>
        <el-button @click="exportOnOffDeviceDataToExcel">{{$t('Export')}}</el-button>
        <el-table
          height="50vh"
          :data="listOnOffDeviceData">
          <el-table-column v-for="col in onOffDeviceDataColukmns" :key="col.ID"
            :prop="col.prop"
            :label="col.label"
            min-width="180">
          </el-table-column>
        </el-table>
      </div>
    </el-dialog>
  </div>
</template>
<script src="./dashboard-component.ts"></script>

<style scoped>
.el-collapse{
  border: 0;
  .dashboard-collapse{
  .el-collapse-item__header{
    background-color: transparent !important;
    color: black;
    font-weight: bold;
  }
  .el-collapse-item__content{
    background-color: transparent;
  }
  .el-collapse-item__wrap{
    border: 0;
  }
}
}
.vue-grid-layout {
    .vue-grid-item{
      background-color: white;
      border-radius: 10px;
      /* box-shadow: 0 4px 8px 0 gray; */
    }
}

.vue-grid-item:not(.vue-grid-placeholder) {
    background: white !important;
    /* border: 1px solid black; */
}

.vue-grid-item .resizing {
    opacity: 0.9;
}

.vue-grid-item.static {
    background: white !important;
}
.vue-grid-item .static {
    background: white !important;
}

.vue-grid-item .text {
    font-size: 24px;
    text-align: center;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    margin: auto;
    height: 100%;
    width: 100%;
}

.vue-grid-item .no-drag {
    height: 100%;
    width: 100%;
}

.vue-grid-item .minMax {
    font-size: 12px;
}

.vue-grid-item .add {
    cursor: pointer;
}

.vue-draggable-handle {
    position: absolute;
    width: 20px;
    height: 20px;
    top: 0;
    left: 0;
    background-position: bottom right;
    padding: 0 8px 8px 0;
    background-repeat: no-repeat;
    background-origin: content-box;
    box-sizing: border-box;
    cursor: pointer;
}
</style>