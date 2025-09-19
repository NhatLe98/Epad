<template>
  <div id="factoryUserMonitoring">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("FactoryUserMonitoring") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <div :class="{ 'full-screen fullscreen-container': isFullscreenOn }" style="overflow-y: hidden;">
        <el-main style="margin-top: 0; padding: 5px;background-color: #2d3139;" 
        :class="{ 'el-main__full-screen': isFullscreenOn }">
          <div class="main factory-user-monitoring" 
          :style="isFullscreenOn ? 'height: calc(100vh);' : 'height: calc(100vh - 40px);'"
          style="margin-top: 0;">
            <div style="height: 100%;" v-if="!reload">

              <el-row v-if="!excludeVehicle" style="height: calc(100% - 2vh - 50px);" :gutter="20">
                <el-col class="factory-user-monitoring__dashboard-item" :span="12" style="height: 50%; padding: 10px;" id="employeePresentWrapper">
                  <el-row 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; padding: 5px; 
                  background-color: #22262e;">
                    <el-col :span="24"
                    style="height: 100%;display: block;">
                      <div style="height: 20%; color: #fb6511 !important;">
                        <span 
                        style="font-size: 2vw; font-weight: bold; text-transform: uppercase;">
                          {{ $t('TotalUser') }}
                        </span>
                        <!-- <br/>
                        {{ $t('currentlyInFactory') }} -->
                      </div>
                      <el-row style="height: 80%;">
                        <el-col :span="6" 
                        style="height: calc(100% - 30px); margin-top: 30px;">
                          <div style="height: 40%; display: flex; justify-content: center;">
                            <img src="@/assets/icons/multiple-users-silhouette-orange.png" 
                            style="height: 100%;"/>
                          </div>
                          <div style="height: 60%; display: flex; justify-content: center; flex-direction: column;
                          color: #fb6511 !important;">
                            <span
                            style="font-size: 1.5vw; font-weight: bold;text-transform: uppercase; text-align: center;">
                              {{$t('TotalAmount')}}
                            </span>
                            <span
                            style="font-size: 2vw; font-weight: bold; text-transform: uppercase; text-align: center;">
                              {{ totalEmployeePresent }}
                            </span>
                          </div>
                        </el-col>
                        <el-col :span="18"
                        style="height: 100%;">
                          <EmployeePresentByUserTypeBarChart2
                            @totalEmployeePresent="setTotalEmployeePresent"
                            @dataEmployeePresent="setDataEmployeePresent"
                            ref="factoryUserMonitoringPresentChart"
                            style="height: 100%;"
                            :index="1"
                            :name="$t('TotalUser')"
                            ></EmployeePresentByUserTypeBarChart2>
                            
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col class="factory-user-monitoring__dashboard-item" :span="12" style="height: 50%; padding: 10px;" >
                  <el-row 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; 
                  padding: 5px; background-color: #22262e; position: relative;">
                    <el-col :span="24"
                    style="height: 100%;display: block;">
                      <el-row style="height: 100%;">
                        <el-col :span="14" class="inEmergencyPresent"
                        style="height: 100%;">
                        <span 
                        style="font-size: 2vw; font-weight: bold; text-transform: uppercase; color: #f23378 !important;">
                          {{ $t('Emergency') }}
                        </span>
                          <EmergencyLogPieChart
                          @dataEmergencyValues="setEmergencyDataValues"
                          @dataEmergency="setDataEmergencyPresent"
                          style="height: calc(100% - 2vw - 10px);"
                          :index="2"
                          :name="$t('Emergency')"
                          ></EmergencyLogPieChart>
                        </el-col>
                        <el-col :span="10"
                          style="height: 100%;">
                          <div class="notInEmergencyPresent"
                          style="height: calc(35%);display: flex;justify-content: center;flex-direction: column;align-items: center;">
                            <div 
                            style="height: 60%;">
                              <img src="@/assets/icons/human-three-red.png" 
                              style="height: 100%;"/>
                            </div>
                            <div 
                            style="height: 40%; font-weight: bold; display: flex; flex-direction: column; 
                            align-items: center;color: #f23378 !important;">
                              <span style="font-size: 1.5vw; text-transform: uppercase;">{{ $t('Absence') }}</span>
                              <span style="font-size: 1.5vw;">{{ (totalEmployeePresent - totalEmergencyPresent) }}</span>
                            </div>
                          </div>
                          <div class="inEmergencyPresent"
                          style="height: calc(65%); display: flex; justify-content: center; flex-direction: column;
                          font-size: 1vw;">
                            <div v-for="(item, index) in emergencyPresentDataValues"
                            :style="'color:' + item.color">
                              <span 
                              :style="'background-color:' + item.color"
                              style="height: 1vh;
                              width: 1vh;
                              border-radius: 50%;
                              display: inline-block;"></span> <span>{{ item.label }}</span>: {{ item.value }}
                            </div>
                            <div style="font-size: 1.2vw; font-weight: bold; margin-top: 5px;">
                              {{ $t('TotalPresent') }}: {{ totalEmergencyPresent }}
                            </div>
                          </div>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col class="factory-user-monitoring__dashboard-item" :span="12" style="height: 50%; padding: 10px;" id="truckPresentWrapper">
                  <div 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; 
                  padding: 5px 15px 5px 60px; background-color: #22262e;">
                    <el-row style="height: 20%;">
                      <img src="@/assets/icons/truck-white.png" 
                        style="height: 100%;"/>
                      <span 
                      style="font-size: 2vw; font-weight: bold; text-transform: uppercase; float: right;">
                        {{ $t('Truck') }}
                      </span>
                    </el-row>
                    <div style="height: 80%; display: flex; flex-direction: column; align-items: start;">
                      <div style="height: calc(100% / 3); margin-bottom: 5px; width: 100%;">
                        <span 
                        style="font-size: 1.35vw; font-weight: bold; margin-bottom: 5px; display: block;">
                          {{ $t('TotalVehicleInFactory') }}
                        </span>
                        <div style="display: flex; justify-content: start">
                          <el-progress class="no-padding-right override-height" :color="'#f23378'" :text-inside="false"
                            :percentage="((truckDriverLogs.Item4.length / truckDriverLogs.Item1.length) * 100)" 
                            :format="formatNull"
                            style="width: 80%; height: 4vh;"
                          >
                          </el-progress>
                          <span 
                          style="margin-left: 10px; font-weight: bold; font-size: 4vh; line-height: 4vh;color: #f23378;">
                            {{ truckDriverLogs.Item4.length }}
                          </span>
                        </div>
                      </div>
                      <div style="height: calc(100% / 3); width: 100%;">
                        <span 
                        style="font-size: 1.15vw; margin-bottom: 5px; display: block;">
                          {{ $t('TotalVehicleInFromStartToday') }}
                        </span>
                        <div style="display: flex; justify-content: start">
                          <el-progress class="no-padding-right override-height" :color="'#113a92'" :text-inside="false"
                            :percentage="((truckDriverLogs.Item2.length / truckDriverLogs.Item1.length) * 100)" 
                            :format="formatNull"
                            style="width: 60%; height: 3vh;"
                          >
                          </el-progress>
                          <span 
                          style="margin-left: 10px; font-weight: bold; font-size: 3vh; line-height: 3vh;color: #113a92;">
                            {{ truckDriverLogs.Item2.length }}
                          </span>
                        </div>
                      </div>
                      <div style="height: calc(100% / 3); width: 100%;">
                        <span 
                        style="font-size: 1.15vw; margin-bottom: 5px; display: block;">
                          {{ $t('TotalVehicleOutFromStartToday') }}
                        </span>
                        <div style="display: flex; justify-content: start">
                          <el-progress class="no-padding-right override-height" :color="'#81007f'" :text-inside="false"
                            :percentage="((truckDriverLogs.Item3.length / truckDriverLogs.Item1.length) * 100)" 
                            :format="formatNull"
                            style="width: 60%; height: 3vh;"
                          >
                          </el-progress>
                          <span 
                          style="margin-left: 10px; font-weight: bold; font-size: 3vh; line-height: 3vh; color: #81007f;">
                            {{ truckDriverLogs.Item3.length }}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </el-col>
                <el-col class="factory-user-monitoring__dashboard-item" :span="12" style="height: 50%; padding: 10px;" id="integratedVehiclePresentWrapper">
                  <el-row 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; 
                  padding: 5px 15px 5px 30px; background-color: #22262e; position: relative;">
                    <el-col :span="16" style="height: 100%;">
                      <div style="height: 80%;">
                        <VehicleInOutBarChart
                        @totalVehicleInNotOut="setTotalVehicleInNotOut"
                        @totalVehicleInNotOutToday="setTotalVehicleInNotOutToday"
                        @dataVehicleInNotOut="setDataVehicleInNotOut"
                      style="height: calc(100%);"
                        :index="3"
                        :name="$t('Motorbike')"
                        ></VehicleInOutBarChart>
                      </div>
                      <el-row>
                        <el-col :span="4">
                          <span style="font-size: 3.75vh; font-weight: bold;">{{ totalVehicleInNotOutToday }}</span>
                        </el-col>
                        <el-col :span="20">
                          <span style="font-size: 2.5vh; font-weight: bold;">
                            {{ $t('AmountVehicleInOutToday') }}
                          </span>
                          <br/>
                          <span>
                            ({{ $t('countFromStartTodayTillNow') }})
                          </span>
                        </el-col>
                      </el-row>
                    </el-col>
                    <el-col :span="8" style="height: 100%;">
                      <div style="height: 20%;">
                        <span 
                        style="font-size: 2vw; font-weight: bold; text-transform: uppercase; float: right;">
                          {{ $t('2wheeler') }}
                        </span>
                      </div>
                      <div style="height: 30%; display: flex; justify-content: center;">
                        <img src="@/assets/icons/motor-white.png" 
                        style="height: 100%;"/>
                      </div>
                      <div style="height: 50%; display: flex; flex-direction: column; justify-content: center;
                      align-items: center;">
                        <span 
                        style="font-size: 2vw; font-weight: bold; text-transform: uppercase;">
                          {{ $t('TotalAmount') }}
                        </span>
                        <span>
                          {{ $t('vehicleCurrentlyInFactory') }}
                        </span>
                        <span 
                        style="font-size: 2.5vw; font-weight: bold; color: #fdfdfd;">
                          {{ totalVehicleInNotOut }}
                        </span>
                      </div>
                    </el-col>
                  </el-row>
                </el-col> 
              </el-row>
              <el-row v-if="excludeVehicle" style="height: calc(100% - 2vh - 50px);" :gutter="20">
                <el-col class="factory-user-monitoring__dashboard-item" :span="24" style="height: 50%; padding: 10px;" id="employeePresentWrapper">
                  <el-row 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; padding: 5px; 
                  background-color: #22262e;">
                    <el-col :span="24"
                    style="height: 100%;display: block;">
                      <div style="height: 20%; color: #fb6511 !important;">
                        <span 
                        style="font-size: 2vw; font-weight: bold; text-transform: uppercase;">
                          {{ $t('TotalUser') }}
                        </span>
                        <!-- <br/>
                        {{ $t('currentlyInFactory') }} -->
                      </div>
                      <el-row style="height: 80%;">
                        <el-col :span="6" 
                        style="height: calc(100% - 30px); margin-top: 30px;">
                          <div style="height: 40%; display: flex; justify-content: center;">
                            <img src="@/assets/icons/multiple-users-silhouette-orange.png" 
                            style="height: 100%;"/>
                          </div>
                          <div style="height: 60%; display: flex; justify-content: center; flex-direction: column;
                          color: #fb6511 !important;">
                            <span
                            style="font-size: 1.5vw; font-weight: bold;text-transform: uppercase; text-align: center;">
                              {{$t('TotalAmount')}}
                            </span>
                            <span
                            style="font-size: 2vw; font-weight: bold; text-transform: uppercase; text-align: center;">
                              {{ totalEmployeePresent }}
                            </span>
                          </div>
                        </el-col>
                        <el-col :span="18"
                        style="height: 100%;">
                          <EmployeePresentByUserTypeBarChart2
                            @totalEmployeePresent="setTotalEmployeePresent"
                            @dataEmployeePresent="setDataEmployeePresent"
                            ref="factoryUserMonitoringPresentChart"
                            style="height: 100%;"
                            :index="1"
                            :name="$t('TotalUser')"
                            ></EmployeePresentByUserTypeBarChart2>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-col>
                <el-col class="factory-user-monitoring__dashboard-item" :span="24" style="height: 50%; padding: 10px;" >
                  <el-row 
                  style="height: 100%; border: 3px #22262e solid; border-radius: 15px; 
                  padding: 5px; background-color: #22262e; position: relative;">
                  <div style="height: 10%; color: #fb6511 !important;">
                    <span 
                    style="font-size: 2vw; font-weight: bold; text-transform: uppercase; color: #f23378 !important;">
                      {{ $t('AreaLimited') }}
                    </span>
                  </div>
                    <el-col :span="24"
                    style="height: 90%;display: block;">
                      <el-row style="height: 100%;">
                        <el-col :span="24" class="inEmergencyPresent"
                        style="height: 100%;">
                          <el-col :span="6" style="height: 100%;">
                            <div class="notInEmergencyPresent"
                            style="height: 50%; display: flex;justify-content: center;flex-direction: column;align-items: center;">
                              <div 
                              style="height: 40%;">
                                <img src="@/assets/icons/human-three-red.png" 
                                style="height: 100%;"/>
                              </div>
                              <div 
                              style="height: 40%; font-weight: bold; display: flex; flex-direction: column; 
                              align-items: center;color: #f23378 !important;">
                                <span style="font-size: 1.5vw; text-transform: uppercase;">{{ $t('Absence') }}</span>
                                <span style="font-size: 1.5vw;">{{ (totalEmployeePresent - totalEmergencyPresent) }}</span>
                              </div>
                            </div>
                            <div
                            style="height: 50%; display: flex;justify-content: center;flex-direction: column;align-items: center;">
                              <div 
                              style="height: 40%;">
                                <img src="@/assets/icons/human-three-white.png" 
                                style="height: 100%;transform: scale(2);"/>
                              </div>
                              <div 
                              style="height: 40%; font-weight: bold; display: flex; flex-direction: column; 
                              align-items: center;color: #ffffff !important;">
                                <span style="font-size: 1.5vw; text-transform: uppercase;">{{ $t('Working') }}</span>
                                <span style="font-size: 1.5vw;">{{ totalEmergencyPresent }}</span>
                              </div>
                            </div>
                          </el-col>
                          <el-col :span="18" style="height: 100%;">
                            <EmergencyLogBarChart
                            @dataEmergencyValues="setEmergencyDataValues"
                            @dataEmergency="setDataEmergencyPresent"
                            style="height: calc(100%);"
                            :index="2"
                            :name="$t('Emergency')"
                            ></EmergencyLogBarChart>
                          </el-col>
                        </el-col>
                      </el-row>
                    </el-col>
                  </el-row>
                </el-col>
              </el-row>
              <el-row style="height: 2vh; display: flex; align-items: center; color: #fdfdfd;">
                <el-col :span="10" style="height: 80%;">
                  <!-- <div style="text-align: center; height: 100%;">
                    <span style="font-weight: bold; font-size: 1.5vw;">{{ $t('Day') }}: {{ strDateNow }}</span><br />
                  </div> -->
                </el-col>
                <el-col :span="10" style="height: 80%;">
                  <!-- <div style="text-align: center; height: 100%;">
                    <span style="font-weight: bold; font-size: 1.5vw;">{{ $t('Hour') }}: {{ strTimeNow }}</span><br />
                  </div> -->
                </el-col>
                <el-col :span="4" style="align-self: start;">
                  <div style="display: flex; align-items: end; justify-content: end;">
                    <el-button type="primary"
                    style="margin-right: 5px;"
                    :disabled="reload"
                    :style="!reload ? 'background-color: rgb(19, 56, 152) !important;' : 'background-color: rgb(19 56 152 / 50%) !important; '"
                    @click="reloadData">
                      {{ (!reload ? $t('Update') : ($t('Updating') + '...')) }}
                    </el-button>
                    <el-button @click="toggleFullScreen">
                      <img v-show="isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_off.svg"
                        style="width: 18px; height: 18px" />
                      <img v-show="!isFullscreenOn" src="@/assets/icons/function-bar/fullscreen_on.svg"
                        style="width: 18px; height: 18px" />
                    </el-button>
                  </div>
                </el-col>
              </el-row>
              
            </div>
            <el-dialog
            :class="isErrorExplodeData ? 'error-dialog' : ''"
            :title="titleExplode"
            :visible.sync="isShowExplode"
            :before-close="handleCloseDialog"
            :close-on-click-modal="false"
            width="70%"
            :append-to-body="true">
            <div>
              <el-button @click="exportToExcel">{{$t('Export')}}</el-button>
              <el-table
              height="50vh"
              :data="explodeData">
              <el-table-column v-for="col in explodeColumns" :key="col.ID"
                :prop="col.prop"
                :label="col.label"
                min-width="180">
              </el-table-column>
            </el-table>
            </div>
          </el-dialog>
          </div>
        </el-main>
      </div>
    </el-container>
  </div>
</template>
<script lang="ts" src="./factory-user-monitoring.ts"></script>

<style lang="scss">
.main {
  box-shadow: none;
  -webkit-box-shadow: none;
}

.factory-user-monitoring .el-loading-mask {
  background-color: #22262e;
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

.v-modal {
  background-color: transparent !important;
}

.el-progress.no-padding-right {
  .el-progress-bar{
    padding: 0 !important;
  }
}
.el-progress.override-height {
  .el-progress-bar{
    height: 100% !important;
    .el-progress-bar__outer{
      height: 100% !important;
    }
  }
}
.error-dialog{
  .el-dialog{
    .el-dialog__header{
      .el-dialog__title{
        color: red;
      }
    }
  }
}

#factoryUserMonitoring{
  color: #fdfdfd !important;
}

.el-progress-bar__outer{
  background-color: #2d3139;
}
</style>
