<template>
  <div id="totalEmployeePresent">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("GeneralMonitoringScreen") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <div :class="{ 'full-screen fullscreen-container': isFullScreen() }" style="overflow-y: hidden;">
        <el-main style="margin-top: 0; padding: 5px;" :class="{ 'el-main__full-screen': isFullScreen() }">
          <div class="main general-monitoring-screen" 
          :style="isFullscreenOn ? 'height: calc(100vh);' : 'height: calc(100vh - 40px);'">
            <div style="height: 10%;">
              <el-button @click="toggleFullScreen" style="float: right;">
                <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg"
                  style="width: 18px; height: 18px" />
                <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg"
                  style="width: 18px; height: 18px" />
              </el-button>
            </div>
            <div style="height: 90%;">
              <el-row style="height: 25%">
                <el-col :span="11">
                  <div style="text-align: center;">
                    <span style="font-weight: bold; font-size: 36px;">{{ $t('TotalUser') }}:</span><br />
                    <span style="font-weight: bold; font-size: 36px;">{{
                      totalEmployee
                    }}</span>
                    <span style="font-size: 36px;">{{
                      ' ' + $t('user')
                    }}</span>
                  </div>
                </el-col>
                <el-col :span="2">
                  <p></p>
                </el-col>
                <el-col :span="11" style="height: 100%">
                  <div style="text-align: center;">
                    <span style="font-weight: bold; font-size: 36px;">{{ $t('LogTime') }}:</span><br />
                    <span style="font-weight: bold; font-size: 36px;">{{ strNow }}</span>
                  </div>
                </el-col>
              </el-row>
              <el-row style="height:75%">
                <el-col class="colBorder" :span="11">
                  <el-table height="calc(90%)" class="general-monitoring-screen__table"
                    style="margin-top: 0; text-align: center;"
                    :data="tblData.filter((_, idx) => idx % 2 === 0)" @row-click="rowClicked" border
                    empty-text=" ">
                    <el-table-column prop="Department" :label="$t('Department')"></el-table-column>
                    <el-table-column label="Số lượng">
                      <template #default="scope">
                        {{ scope.row.Employees.length }}
                      </template>
                    </el-table-column>
                  </el-table>
                </el-col>
                <el-col class="colBorder" :span="2">
                  <p></p>
                </el-col>
                <el-col class="colBorder" :span="11">
                  <el-table height="calc(90%)" class="general-monitoring-screen__table"
                    style="margin-top: 0; text-align: center;"
                    :data="tblData.filter((_, idx) => idx % 2 !== 0)" @row-click="rowClicked" border
                    empty-text=" ">
                    <el-table-column prop="Department" :label="$t('Department')"></el-table-column>
                    <el-table-column label="Số lượng">
                      <template #default="scope">
                        {{ scope.row.Employees.length }}
                      </template>
                    </el-table-column>
                  </el-table>
                </el-col>
              </el-row>
            </div>
          </div>
          <!-- dialog -->
          <el-dialog :title="$t('EmployeeInfo')" :visible.sync="dialogEmployeeInfo">
            <el-row :gutter="10">
              <el-col :span="24">
                <span>{{ $t("Department") }}: {{ selectedDepartmentName }}</span>
              </el-col>
              <el-col :span="24">
                <span>{{ $t("TotalUser") }}: {{ selectedNumberOfEmployee }}</span>
              </el-col>

              <el-col :span="24" v-if="dialogLoading == true" style="font-size: 25px; margin: 20px 50%"><i
                  class="el-icon-loading"></i></el-col>

              <el-col :span="24" style="margin-top: 10px" v-if="dialogLoading == false" class="table-dialog">
                <el-table height="300" class="general-monitoring-screen__table-detail" :data="tblDataDetail" border empty-text=" " :row-class-name="tableRowDetailClassName" ref="dataTableRef" >
                  <el-table-column
                    prop="Index"
                    label="STT"
                    width="50"
                  ></el-table-column>
                  <el-table-column
                    prop="EmployeeATID"
                    :label="$t('EmployeeATID')"
                    width="120"
                  ></el-table-column>
                  <el-table-column
                    prop="FullName"
                    :label="$t('FullName')"
                  ></el-table-column>
                  <el-table-column
                    prop="Position"
                    :label="$t('Position')"
                  ></el-table-column>
                  <el-table-column
                    prop="TimeIn"
                    :label="$t('TimeIn')"
                  ></el-table-column>
                  <el-table-column
                    prop="ViolationInfo"
                    :label="$t('ViolationInfoIfHave')"
                  ></el-table-column>
                </el-table>
              </el-col>
            </el-row>
          </el-dialog>
        </el-main>
      </div>
    </el-container>
  </div>
</template>
<script lang="ts" src="./general-monitoring-screen.ts"></script>

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

.table-dialog {
  tr th div{
    padding-left: 5px !important;
  }
}

.general-monitoring-screen {
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

.general-monitoring-screen .el-tabs__header{
  margin: 0 !important;
  height: unset;
}

.general-monitoring-screen__table td div,
.general-monitoring-screen__table th div {
  padding-left: 10px !important;
  // text-align: inherit !important;
}

.general-monitoring-screen__table th {
  color: rgb(19, 56, 152);

  div {
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

.general-monitoring-screen__table-detail .warning-row td{
	background-color: yellow !important;
}

.v-modal {
  background-color: transparent !important;
}
</style>
