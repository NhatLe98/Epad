<template>
    <div id="totalEmployeePresent">
      <el-container>
        <el-header>
          <el-row>
            <el-col :span="12" class="left">
              <span id="FormName">{{ $t("AttendanceAndEvacuation") }}</span>
            </el-col>
            <el-col :span="12">
              <HeaderComponent />
            </el-col>
          </el-row>
        </el-header>
        <div :class="{ 'full-screen fullscreen-container': isFullScreen() }" style="overflow-y: hidden;">
          <el-main style="margin-top: 0; padding: 5px;" :class="{ 'el-main__full-screen': isFullScreen() }">
            <div class="main attendance-and-evacuation" 
            :style="isFullscreenOn ? 'height: calc(100vh);' : 'height: calc(100vh - 40px);'">
              <el-tabs v-model="activeTab" @tab-click="handleClick" style="height: calc(100% - 20px);">
                <el-tab-pane :label="$t('Areas')" name="area" style="height: 100%;">
                  <div style="height: 10%;">
                    <el-button @click="toggleFullScreen" style="float: right;">
                      <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg"
                        style="width: 18px; height: 18px" />
                      <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg"
                        style="width: 18px; height: 18px" />
                    </el-button>
                  </div>
                  <div style="height: 90%;">
                    <el-row style="height: 15%">
                      <el-col :span="11">
                        <div style="text-align: center;">
                          <span style="font-weight: bold; font-size: 42px;">{{ $t('Employee') }}</span><br />
                          
                        </div>
                      </el-col>
                      <el-col :span="2">
                        <p></p>
                      </el-col>
                      <el-col :span="11" style="height: 100%">
                        <div style="text-align: center;">
                          <span style="font-weight: bold; font-size: 42px;">{{ $t('Other') }}</span><br />
                        </div>
                      </el-col>
                    </el-row>
                    <el-row style="height:75%">
                      <el-col class="colBorder" :span="11">
                        <el-table height="calc(100%)" 
                          class="attendance-and-evacuation__table attendance-and-evacuation__area-table"
                          style="margin-top: 0; text-align: center;"
                          :data="tblGroupDeviceLogData" border resizable
                          empty-text=" ">
                          <el-table-column :label="$t('STT')" width="60">
                            <template #default="scope">
                              {{ scope.$index + 1}}
                            </template>
                          </el-table-column>
                          <el-table-column width="250" prop="DepartmentName" :label="$t('DepartmentOrPart')"></el-table-column>
                         
                          <el-table-column width="auto" prop="InCom" :label="$t('InCompany')"></el-table-column>
                          <el-table-column width="auto" prop="Attendance" :label="$t('Working')"></el-table-column>
                          <el-table-column width="auto" prop="Absent" :label="$t('Absent')"></el-table-column>
                        </el-table>
                      </el-col>
                      <el-col class="colBorder" :span="2">
                        <p></p>
                      </el-col>
                      <el-col class="colBorder" :span="11">
                        <el-table height="calc(100%)" 
                          class="attendance-and-evacuation__table attendance-and-evacuation__area-table"
                          style="margin-top: 0; text-align: center;"
                          :data="tblGroupDeviceLogDatas" border resizable
                          empty-text=" ">
                          <el-table-column :label="$t('STT')" width="60">
                            <template #default="scope">
                              {{ scope.$index + 1}}
                            </template>
                          </el-table-column>
                          <el-table-column width="250" prop="DepartmentName" :label="$t('DepartmentOrPart')"></el-table-column>
                         
                          <el-table-column width="auto" prop="InCom" :label="$t('InCompany')"></el-table-column>
                          <el-table-column width="auto" prop="Attendance" :label="$t('Working')"></el-table-column>
                          <el-table-column width="auto" prop="Absent" :label="$t('Absent')"></el-table-column>
                        </el-table>
                      </el-col>
                    </el-row>
                  </div>
                </el-tab-pane>
             
              </el-tabs>
            </div>
            <!-- dialog -->
            <el-dialog :title="$t('EmployeeInfo')" :visible.sync="dialogEmployeeInfo">
              <el-row :gutter="10">
                <el-col :span="24">
                  <span v-if="activeTab == 'department'">{{ $t("Department") }}: {{ selectedDepartmentName }}</span>
                  <span v-else>{{ $t("Areas") }}: {{ selectedDepartmentName }}</span>
                </el-col>
                <el-col :span="24">
                  <span>{{ $t("TotalWorkingEmployee") }}: {{ selectedNumberOfEmployee }}</span>
                </el-col>
  
                <el-col :span="24" v-if="dialogLoading == true" style="font-size: 25px; margin: 20px 50%"><i
                    class="el-icon-loading"></i></el-col>
  
                <el-col :span="24" style="margin-top: 10px" v-if="dialogLoading == false" class="table-dialog">
                  <el-table height="300" class="attendance-and-evacuation__table" :data="tblDataDetail" border empty-text=" "
                    ref="dataTableRef">
                    <el-table-column prop="Index" label="STT" width="50"></el-table-column>
                    <el-table-column prop="EmployeeATID" :label="$t('EmployeeATID')" width="120"></el-table-column>
                    <el-table-column prop="FullName" :label="$t('FullName')"></el-table-column>
                    <el-table-column prop="PositionName" :label="$t('Position')"></el-table-column>
                    <el-table-column prop="TimeString" :label="$t('TimeIn')"></el-table-column>
                  </el-table>
                </el-col>
              </el-row>
            </el-dialog>
          </el-main>
        </div>
      </el-container>
    </div>
  </template>
  <script lang="ts" src="./attendance-and-evacuation-component.ts"></script>
  
  <style lang="scss">
  .main {
    box-shadow: none;
    -webkit-box-shadow: none;
  }
  
  .spCenter {
    margin-top: auto;
    margin-bottom: auto;
    cursor: pointer;
  }
  
  .colBorder {
    height: 100%;
  }
  
  .el-table__row {
    cursor: pointer;
  }
  
  #totalEmployeePresent {
    .el-dialog {
      width: 60%;
    }
  }
  
  .attendance-and-evacuation {
    .table-dialog {
      .el-table--enable-row-hover .el-table__body tr:hover>td {
        background-color: #87a8e0 !important;
      }
  
      .el-table__body tr:hover>td {
        background-color: #87a8e0 !important;
      }
  
      .el-table__body tr.current-row>td {
        background-color: #87a8e0 !important;
      }
    }
  }
  
  .el-tabs {
    .el-tabs__header {
      height: 10%;
    }
  
    .el-tabs__content {
      height: 90%;
    }
  }
  
  .attendance-and-evacuation .el-tabs__header{
    margin: 0 !important;
    height: unset;
  }
  
  .attendance-and-evacuation__table td div,
  .attendance-and-evacuation__table th div {
    padding-left: 10px !important;
    // text-align: inherit !important;
  }
  
  .attendance-and-evacuation__table th {
    color: rgb(19, 56, 152) !important;
  
    div {
      color: rgb(19, 56, 152) !important;
      font-weight: bolder !important;
    }
  }
  
  .fullscreen-container {
    width: 100vw;
    height: 100vh;
    position: fixed;
    top: 0;
    left: 0;
    background-color: #fff;
    transition: all 0.3s;
  }
  
  .full-screen {
    width: 100%;
    height: 100%;
    overflow: hidden;
  }
  
  .attendance-monitoring .el-tabs__header{
    margin: 0 !important;
    height: unset;
  }
  
  .attendance-and-evacuation__area-table tbody tr {
    height: calc(5vh);
    td {
      height: 100%;
      div {
        text-align: center;
        font-size: 0.85vw !important;
        height: 100%;
        line-height: 0.85vw !important;
        span {
          font-size: inherit;
          span {
            display: block;
            height: 0.85vw;
            margin-top: 5px;
          }
        }
      }
    }
  }
  
  .attendance-and-evacuation__area-table thead tr {
    height: calc(60px);
    th {
      height: 100%;
      padding-right: 5px;
      div {
        text-align: center;
        font-size: 0.9vw !important;
        height: 100%;
        // 8px is default padding top and bottom
        line-height: calc(60px - 8px) !important;
      }
    }
  }
  
  .attendance-and-evacuation__area-table td div,
  .attendance-and-evacuation__area-table th div {
    padding-left: 10px !important;
    // text-align: inherit !important;
  }
  
  .attendance-and-evacuation__area-table th {
    color: rgb(19, 56, 152) !important;
  
    div {
      color: rgb(19, 56, 152) !important;
      font-weight: bolder !important;
    }
  }
  
  .v-modal {
    background-color: transparent !important;
  }
  </style>
  