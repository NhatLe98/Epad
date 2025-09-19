<template>
  <div id="totalEmployeePresent">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("AttendanceMonitoring") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <div :class="{ 'full-screen fullscreen-container': isFullscreenOn }" style="overflow-y: hidden;">
        <el-main style="margin-top: 0; padding: 5px;" :class="{ 'el-main__full-screen': isFullscreenOn }">
          <div class="main attendance-monitoring" 
          :style="isFullscreenOn ? 'height: calc(100vh);' : 'height: calc(100vh - 40px);'">
            <div style="height: 8%;">
              <el-button @click="toggleFullScreen" style="float: right;">
                <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg"
                  style="width: 18px; height: 18px" />
                <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg"
                  style="width: 18px; height: 18px" />
              </el-button>
            </div>
            <div style="height: 92%;">
              <el-row class="attendance-monitoring__header" style="height: 25%; background-color: aliceblue; display: flex; align-items: center; color: darkblue;">
                <el-col :span="11" style="height: 80%;">
                  <div style="text-align: center; height: 100%; display: flex; 
                  flex-direction: column; justify-content: space-around">
                    <span style="font-weight: bold; font-size: 36px;">{{ $t('TotalUserInFactory') }}</span>
                    <div>
                      <span style="font-weight: bold; font-size: 36px;">{{
                        totalEmployee
                      }}</span>
                      <span style="font-size: 36px;">{{
                        ' ' + $t('user')
                      }}</span>
                    </div>
                  </div>
                </el-col>
                <el-col :span="2">
                  <p></p>
                </el-col>
                <el-col :span="11" style="height: 80%;">
                  <div style="text-align: center; height: 100%; display: flex; 
                  flex-direction: column; justify-content: space-around">
                    <span style="font-weight: bold; font-size: 36px;">{{ $t('LogTime') }}</span><br />
                    <span style="font-size: 36px;">{{ strNow }}</span>
                  </div>
                </el-col>
              </el-row>
              <el-row class="attendance-monitoring__body" style="height: calc(75% - 100px); margin-top: 50px;">
                <el-col :span="14" 
                style="height: 100%; background-color: aliceblue; border-radius: 15px; overflow-y: auto;">
                  <!-- <el-col :span="12" v-for="(item, index) in tblData.filter((_, idx) => idx < 6)" -->
                  <el-col :span="12" v-for="(item, index) in tblData"
                    style="height: calc(100% / 3); padding: 10px; color: darkblue;"
                    :style="index % 2 == 0 ? 'padding-left: 20px;' : 'padding-right: 20px;'"
                    class="attendance-morning__gate-amount">
                    <div style="height: 100%; background-color: rgb(157 171 245 / 50%); border-radius: 15px; 
                    box-shadow: 3px 3px 6px 3px lightgray; cursor: pointer;" 
                    :class="itemClassName(item)"
                    @click="rowClicked(item, null, null)">
                      <el-row style="height: 35%; text-align: center; font-weight: 600; font-size: 1vw;">
                        <el-col :span="14" style="height: 100%; align-content: center;">
                          {{ $t('Gate') }}
                        </el-col>
                        <el-col :span="10" style="height: 100%; align-content: center;">
                          {{ $t('Amount') }}
                        </el-col>
                      </el-row>
                      <el-row style="height: 65%; text-align: center; font-size: 1.75vw; font-weight: bold;">
                        <el-col :span="14" style="height: 100%; align-content: center;">
                          {{ item.Area }}
                        </el-col>
                        <el-col :span="10" style="height: 100%; align-content: center; font-size: 2.25vw;">
                          {{ item.Employees.length }}
                        </el-col>
                      </el-row>
                    </div>
                  </el-col>
                </el-col>
                <el-col :span="10"
                style="height: 100%; padding: 0 20px 0 20px;">
                  <el-row style="height: 100%;">
                    <el-col :span="12"
                    style="height: calc(100% / 2); padding: 0 10px 10px 10px;">
                      <div class="attendance-monitoring__count" 
                        >
                        <div class="count-title">{{ $t('TotalEmployee') }}</div>
                        <div class="count-content">{{ employeeCount }}</div>
                      </div>
                    </el-col>
                    <el-col :span="12"
                    style="height: calc(100% / 2); padding: 0 10px 10px 10px;">
                      <div class="attendance-monitoring__count" 
                        >
                        <div class="count-title">{{ $t('TotalCustomer') }}</div>
                        <div class="count-content">{{ customerCount }}</div>
                      </div>
                    </el-col>
                    <el-col :span="12"
                    style="height: calc(100% / 2); padding: 20px 10px 0 10px;">
                      <div class="attendance-monitoring__count" 
                        >
                        <div class="count-title">{{ $t('TotalContractor') }}</div>
                        <div class="count-content">{{contractorCount}}</div>
                      </div>
                    </el-col>
                  </el-row>
                </el-col>
              </el-row>
            </div>
          </div>
          <!-- dialog -->
          <el-dialog :title="$t('EmployeeInfo')" :visible.sync="dialogEmployeeInfo">
            <el-row :gutter="10">
              <el-col :span="24">
                <span>{{ $t("Gate") }}: </span><span v-html="selectedDepartmentName"></span>
              </el-col>
              <el-col :span="24">
                <span>{{ $t("TotalUser") }}: {{ selectedNumberOfEmployee }}</span>
              </el-col>

              <el-col :span="24" v-if="dialogLoading == true" style="font-size: 25px; margin: 20px 50%"><i
                  class="el-icon-loading"></i></el-col>

              <el-col :span="24" style="margin-top: 10px" v-if="dialogLoading == false" class="table-dialog">
                <el-table height="300" class="attendance-monitoring__table-detail" :data="tblDataDetail" border empty-text=" " 
                  :row-class-name="tableRowDetailClassName"  ref="dataTableRef">
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
<script lang="ts" src="./attendance-monitoring-2.ts"></script>

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

.attendance-monitoring {
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

.attendance-monitoring .el-tabs__header{
  margin: 0 !important;
  height: unset;
}

.attendance-monitoring__table tbody tr {
  height: calc(100px);
  td {
    height: 100%;
    div {
      text-align: center;
      font-size: 36px !important;
      height: 100%;
      line-height: 46px !important;
      span {
        font-size: inherit;
        span {
          display: block;
          height: 36px;
          margin-top: 5px;
          line-height: 26px;
        }
      }
    }
  }
}

.attendance-monitoring__table thead tr {
  height: calc(60px);
  th {
    height: 100%;
    div {
      text-align: center;
      font-size: 36px !important;
      height: 100%;
      // 8px is default padding top and bottom
      line-height: calc(60px - 8px) !important;
    }
  }
}

.attendance-monitoring__table td div,
.attendance-monitoring__table th div {
  padding-left: 10px !important;
  // text-align: inherit !important;
}

.attendance-monitoring__table th {
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

.attendance-monitoring__table .warning-row td,
.attendance-monitoring__table-detail .warning-row td{
	background-color: yellow !important;
}

.v-modal {
  background-color: transparent !important;
}

.warning-class {
  background-color: yellow !important;
}

.attendance-monitoring__count{
  background-color: aliceblue; 
  height: 100%; 
  border-radius: 15px; 
  text-align: center; 
  font-size: 1.25vw; 
  position: relative;
  .count-title{
    height: 35%; 
    align-content: center;
  }
  .count-content{
    height: 65%; 
    position: absolute; 
    left: 50%; 
    top: 45%; 
    transform: translate(-50%, 0); 
    font-size: 2.5vw; 
    font-weight: bold;
  }
}
</style>
