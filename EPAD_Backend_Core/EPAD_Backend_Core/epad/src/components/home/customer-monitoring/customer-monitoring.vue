<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("CustomerMonitoringPage") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <div
        class="wrapper"
        ref="fullscreen"
        background="#EEE"
        :class="{ 'full-screen fullscreen-container': isFullscreenOn }"
      >
        <div class="container customer-monitoring">
          <el-dialog :visible="dialogLineDeviceData" @close="closeLineDeviceDialog">
            <el-table height="300" class="attendance-monitoring__table-detail" :data="lineDeviceData" empty-text=" " 
              :row-class-name="tableRowDetailClassName" ref="dataTableRef">
              <el-table-column
                prop="Index"
                label="STT"
                width="50"
              ></el-table-column>
              <el-table-column
                prop="DeviceType"
                :label="$t('DeviceType')"
              ></el-table-column>
              <el-table-column
                prop="Name"
                :label="$t('MachineName')"
              ></el-table-column>
              <el-table-column
                prop="AccessType"
                :label="$t('Type')"
              ></el-table-column>
              <el-table-column
                prop="IP"
                :label="$t('IPAddress')"
              ></el-table-column>
              <el-table-column
                prop="Status"
                :label="$t('Status')"
              ><template slot-scope="scope">
                <span :style="scope.row.Status == 'Offline' ? 'color: red;' 
                : (scope.row.Status.includes($t('Checking')) ? 'color: orange;' : 'color: #67C23A;')">
                  {{ scope.row.Status }}
                </span>
              </template></el-table-column>
            </el-table>
            <el-row style="margin-top: 5px; display: flex; justify-content: end;">
              <el-button class="button__item-background-color"
              :disabled="connectingDevice || disConnectingDevice"
              style="background-color: white !important; color: darkblue; border-radius: 10px; 
              outline: 2px solid darkblue; font-weight: bold; width: 5vw;"
              @click="reloadLineDeviceConnect">
                {{ $t('Reload') }}
              </el-button>
              <el-button class="button__item-background-color" type="primary"
              style="margin-left: 10px; font-weight: bold; width: 5vw; outline: 2px solid darkblue; border-radius: 10px;"
              @click="closeLineDeviceDialog">
              {{ $t('Close') }}
            </el-button>
            </el-row>
          </el-dialog>
          <el-row class="alert-side">
            <el-col :span="6" class="container-center">
              <el-select
                class="item-center"
                style="width: 60%; padding: 0 12px 0 12px;"
                v-model="lineIndex"
                @change="handleChangeLine"
                filterable
                :clearable="true"
                :popper-append-to-body="false"
                :placeholder="$t('SelectLine')"
                :disabled="connectingDevice || disConnectingDevice"
              >
                <el-option
                  v-for="item in listLine"
                  :key="item.Index"
                  :label="item.Name"
                  :value="item.Index"
                ></el-option>
              </el-select>
            </el-col>
            <el-col :span="15" class="container-center">
              <el-row style="height: 100%;">
                <el-col :span="14" style="height: 100%; width: fit-content;">
                  <span
                    class="item-center"
                    style="font-weight: bold; text-align: center; display: block; width: 100%; padding-right: 10px; padding-top: 3px;"
                  >
                    <!-- <el-alert
                      :title="
                        disableCbbLine == 4 ? $t('CannotConnectCameraOut') : (disableCbbLine == 3 ? $t('CannotConnectCameraIn') : 
                        (disableCbbLine == 2 ? $t('DisconnectingFromCamera') : (disableCbbLine == 1 ? $t('ChangingConnectDevice') 
                          : (realtimeConnected == true
                            ? $t('ConnectToMonitoringServerSuccessfully')
                            : $t('NotConnectServerMonitoring')))))
                      "
                      :type="disableCbbLine != 0 ? 'warning' : (realtimeConnected == true ? 'success' : 'error')"
                      show-icon
                      :closable="false"
                    ></el-alert> -->
                    <el-alert
                      :title="
                        (realtimeConnected == true
                            ? $t('ConnectToMonitoringServerSuccessfully')
                            : $t('NotConnectServerMonitoring'))"
                      :type="realtimeConnected ? 'success' : 'error'"
                      show-icon
                      :closable="false"
                    ></el-alert>
                  </span>
                </el-col>
                <el-col :span="10" style="height: 100%; width: fit-content;">
                  <span
                  v-if="lineIndex && lineIndex > 0"
                    class="item-center"
                    style="font-weight: bold; text-align: center; display: block; width: 100%; padding-top: 3px;"
                  >
                  <el-alert
                    v-if="connectingDevice"
                    :title="$t('ChangingConnectDevice')"
                      :type="'warning'"
                      show-icon
                      :closable="false"
                    ></el-alert>
                  <el-alert
                    v-if="!connectingDevice && ((lineData.CameraIn && lineData.CameraIn.Status == 'Offline') 
                  || (lineData.CameraOut && lineData.CameraOut.Status == 'Offline')                     
                  || (lineData.DeviceIn && lineData.DeviceIn.length > 0 && lineData.DeviceIn.some(x => x.Status == 'Offline')) 
                  || (lineData.DeviceOut && lineData.DeviceOut.length > 0 && lineData.DeviceOut.some(x => x.Status == 'Offline')))"
                      :title="$t('ConnectDeviceFailed')"
                      :type="'error'"
                      show-icon
                      :closable="false"
                    ></el-alert>
                    <el-alert
                    v-if="!connectingDevice && !((lineData.CameraIn && lineData.CameraIn.Status == 'Offline') 
                  || (lineData.CameraOut && lineData.CameraOut.Status == 'Offline')                     
                  || (lineData.DeviceIn && lineData.DeviceIn.length > 0 && lineData.DeviceIn.some(x => x.Status == 'Offline')) 
                  || (lineData.DeviceOut && lineData.DeviceOut.length > 0 && lineData.DeviceOut.some(x => x.Status == 'Offline')))"
                    :title="$t('ConnectDeviceSuccess')"
                      :type="'success'"
                      show-icon
                      :closable="false"
                    ></el-alert>
                  </span>
                  <span
                  v-if="disConnectingDevice"
                    class="item-center"
                    style="font-weight: bold; text-align: center; display: block; width: 100%; padding-top: 3px;"
                  >
                  <el-alert
                    :title="$t('DisconnectingFromCamera')"
                      :type="'warning'"
                      show-icon
                      :closable="false"
                    ></el-alert>
                  </span>
                </el-col>
                <el-col :span="4" class="container-center">
                  <el-link
                    class="item-center"
                    style="padding-left: 12px; text-decoration: underline;"
                    type="primary"
                    @click="showOrHideLineDeviceDataDialog"
                  >
                    {{ $t('SeeDetail') }}
                  </el-link>
                </el-col>
                  </el-row>
                </el-col>
            <el-col :span="1" class="container-center">
              <!-- <el-link
                class="item-center"
                style="padding-left: 12px; text-decoration: underline;"
                type="primary"
                @click="showOrHideLineDeviceDataDialog"
              >
                {{ $t('SeeDetail') }}
              </el-link> -->
            </el-col>
            <el-col :span="2" class="container-center">
              <el-button
                class="item-center"
                style="font-size: 1vw; line-height: 1vw; padding: 4px; float: right; width: 40px; height: 100%;"
                plain
                @click="toggleFullScreen"
              >
                <i
                  :class="[
                    isFullscreenOn ? 'el-icon-close' : 'el-icon-full-screen',
                  ]"
                ></i>
              </el-button>
            </el-col>
          </el-row>

          <el-row class="content-side">
            <el-col :span="6" class="avatar-container">
              <div class="avatar-container horizon-center" id="divAvatar">
                <el-image
                  :src="monitorModel.RegisterImage"
                  fit="fill"
                  v-bind:style="{ height: avatarHeight + 'px' }"
                  class="avatar"
                ></el-image>
                <!-- <span>Ảnh khi đăng ký</span> -->
              </div>
            </el-col>

            <el-col :span="12" :class="classInfo">
              <template v-if="isProcessInfo == false">
                <div
                  class="row-monitoring"
                  style="padding-top: 0 !important;"
                  :class="[
                    isFullscreenOn
                      ? 'fs-font-size-active'
                      : 'fs-font-size-deactive',
                  ]"
                >
                  <el-row>
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-label"
                      ><b>{{ $t("Object") }} </b></span
                    >
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-value">{{ $t(monitorModel.ObjectType) }}</span>
                  </el-row>
                </div>
                <template v-if="monitorModel != null">
                  <div
                    class="row-monitoring"
                    v-for="row in monitorModel.ListInfo"
                    :key="row.Title"
                    :class="[
                      isFullscreenOn
                        ? 'fs-font-size-active'
                        : 'fs-font-size-deactive',
                    ]"
                  >
                    <el-row>
                      <span :class="[
                        isFullscreenOn
                          ? 'font-size-text-active'
                          : 'font-size-text-deactive',
                      ]" class="row-monitoring-label"><b>{{ row.Title == 'EmployeeATID' ? $t('MCC') : $t(row.Title) }} </b></span
                      >
                      <span :class="[
                        isFullscreenOn
                          ? 'font-size-text-active'
                          : 'font-size-text-deactive',
                      ]" class="row-monitoring-value">{{ row.Data }}</span>
                    </el-row>
                  </div>
                </template>
                <template v-if="monitorModel.ListInfo[0] == null">
                  <div
                    class="row-monitoring"
                    v-for="row in ListInfoSample"
                    :key="row.Title"
                    :class="[
                      isFullscreenOn
                        ? 'fs-font-size-active'
                        : 'fs-font-size-deactive',
                    ]"
                  >
                    <el-row>
                      <span :class="[
                        isFullscreenOn
                          ? 'font-size-text-active'
                          : 'font-size-text-deactive',
                      ]" class="row-monitoring-label"
                        ><b>{{ row.Title == 'EmployeeATID' ? $t('MCC') : $t(row.Title) }} </b></span
                      >
                      <span :class="[
                        isFullscreenOn
                          ? 'font-size-text-active'
                          : 'font-size-text-deactive',
                      ]" class="row-monitoring-value">{{ row.Data }}</span>
                    </el-row>
                  </div>
                </template>
                <div
                  class="row-monitoring"
                  :class="[
                    isFullscreenOn
                      ? 'fs-font-size-active'
                      : 'fs-font-size-deactive',
                  ]"
                >
                  <el-row>
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-label"
                      ><b>{{ $t("ViolationInfo") }} </b></span
                    >
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-value" style="font-size: 1.5vw !important;">{{ $t(monitorModel.Error) }}</span>
                  </el-row>
                </div>
                <div v-if="!isException"
                  class="row-monitoring"
                  :class="[
                    isFullscreenOn
                      ? 'fs-font-size-active'
                      : 'fs-font-size-deactive',
                  ]"
                >
                  <el-row>
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-label"
                      ><b>{{ $t("Note") }} </b></span
                    >
                    <el-input
                    :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" 
                      class="fs-font-size-deactive row-monitoring-value__input"
                      type="text"
                      :rows="2"
                      v-model="note"
                    >
                    </el-input>
                  </el-row>
                </div>
                <el-checkbox v-model="isException" class="customer-monitoring__exception-checkbox">{{
                  $t("Exception")
                }}</el-checkbox>
                <div v-if="isException"
                  class="row-monitoring"
                  :class="[
                    isFullscreenOn
                      ? 'fs-font-size-active'
                      : 'fs-font-size-deactive',
                  ]"
                >
                  <el-row>
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-label"
                      ><b>{{ $t("CCCD") }} </b></span
                    >
                    <el-input
                    :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" 
                      class="fs-font-size-deactive row-monitoring-value__input customer-monitoring__exception_cccd_search"
                      type="text"
                      :rows="2"
                      :placeholder="$t('PleaseInputCCCDAndPressEnterToSearch')"
                      v-model="cccd"
                      @keyup.enter.native="searchUserByCCCD" 
                    >
                    </el-input>
                  </el-row>
                </div>
                <div v-if="isException"
                  class="row-monitoring"
                  :class="[
                    isFullscreenOn
                      ? 'fs-font-size-active'
                      : 'fs-font-size-deactive',
                  ]"
                >
                  <el-row>
                    <span :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" class="row-monitoring-label"
                      ><b>{{ $t("ExceptionReason") }} </b></span
                    >
                    <el-input
                    :class="[
                      isFullscreenOn
                        ? 'font-size-text-active'
                        : 'font-size-text-deactive',
                    ]" 
                      class="fs-font-size-deactive row-monitoring-value__input"
                      type="text"
                      :rows="2"
                      v-model="exceptionReason"
                    >
                    </el-input>
                  </el-row>
                </div>
              </template>
              <div v-else class="div-process">
                <span>Đang xử lý thông tin ...</span>
              </div>
            </el-col>

            <el-col :span="6" class="avatar-container">
              <div class="avatar-container horizon-center">
                <el-image
                  fit="fill"
                  v-bind:style="{ height: avatarHeight + 'px' }"
                  class="avatar verify-image"
                  :src="monitorModel.VerifyImage"
                ></el-image>

                <label style="font-weight: bold">{{
                  monitorModel.CheckTime
                }}</label>
                <el-checkbox
                  style="margin-left: 20px"
                  v-model="manualControl"
                  >{{ $t("OpenManual") }}</el-checkbox
                >
                <br />
                <div style="width: 100%">
                  <el-radio v-model="rdInOut" label="1">
                    {{ $t("In") }}
                  </el-radio>
                  <el-radio v-model="rdInOut" label="2">
                    {{ $t("Out") }}
                  </el-radio>
                </div>
                <el-button
                  type="warning"
                  style="width: 45%"
                  :disabled="manualControl == false || (monitorModel.Index == 0 && !this.isException) 
                    || (this.isException && !this.isFoundData)"
                  @click="OpenDoorClicked('Close')"
                  >{{ $t("CloseBarrier") }}</el-button
                >
                <el-button
                  type="success"
                  style="width: 45%; margin-left: 10px"
                  :disabled="manualControl == false || (monitorModel.Index == 0 && !this.isException) 
                    || (this.isException && !this.isFoundData)"
                  @click="OpenDoorClicked('Open')"
                  >{{ $t("OpenBarrier") }}</el-button
                >
              </div>
            </el-col>
          </el-row>

          <el-row class="list-side" :class="isException ? 'list-side__exception' : ''">
            <span :class="[
                        isFullscreenOn
                          ? 'font-size-text-active'
                          : 'font-size-text-deactive',
                      ]" style="font-weight: bold; font-size: 1.3vw;">{{ $t('InOutHistory') }}</span>
            <history-table v-if="!reloadHistoryTable"
            class="monitoring-history-table"
                  :columns="columnInGrid"
                  :data="listHistory"
                  :tableHeight="tableHistoryHeight"
                  :fullScreen="isFullscreenOn"
                >
                </history-table>
            <!-- <el-collapse>
              <el-collapse-item>
                <history-table
                  style="height: 100%"
                  :columns="columnInGrid"
                  :data="listHistory"
                  :tableHeight="tableHistoryHeight"
                  :fullScreen="isFullscreenOn"
                >
                </history-table>
              </el-collapse-item>
            </el-collapse> -->
          </el-row>
        </div>
      </div>
    </el-container>
  </div>
</template>

<script src="./customer-monitoring.ts" />

<style lang="scss" >
.customer-monitoring__exception_cccd_search{
  input::placeholder{
    font-size: 1.1vw;
  }
}

.customer-monitoring__exception-checkbox{
  .el-checkbox__label{
    font-size: 20px !important;
  }
}

.customer-monitoring {
  background-color: rgba(192, 210, 217, 0.25);
  .popup-show-col {
    bottom: auto;
    color: black;
  }
  .el-collapse-item__header {
    display: block;
  }
  .el-collapse-item__arrow {
    margin: 0 8px;
  }
  .el-collapse-item__content {
    padding-left: 8px;
  }
}

.wrapper {
  position: relative;
  height: 100%;
  overflow-y: hidden;
  .container {
    height: 100%;
    width: 100%;
    .row-monitoring {
      width: 100%;
      padding: 8px;
      // border-radius: 10px;
      // border: 1px solid #b5b4b1;
      // margin-top: 5px;
      &:first-child {
        margin-top: 0px;
      }
      .el-row{
        display: flex;
      }
    }
    .fs-font-size-active {
      font-size: 1.7vw;
    }

    .fs-font-size-deactive {
      font-size: 1.7vw;
    }

    .font-size-text-active{
      font-size: 1.7vw;
    }

    .font-size-text-deactive {
      font-size: 1.7vw;
    }

    .font-size-text-active.row-monitoring-label {
      display: block;
      width: 15vw;
    }

    .font-size-text-deactive.row-monitoring-label {
      display: block;
      width: 15vw;
    }

    .row-monitoring-value{
      text-overflow: ellipsis;
      white-space: nowrap;
      overflow: hidden;
    }

    .font-size-text-active.row-monitoring-value {
      display: block;
      width: calc(100% - 15vw);
      border-radius: 4px;
      border: 1px solid #DCDFE6;
      height: 48px;
      line-height: 48px;
      background-color: white;
    }

    .font-size-text-deactive.row-monitoring-value {
      display: block;
      width: calc(100% - 15vw);
      border-radius: 4px;
      border: 1px solid #DCDFE6;
      height: 40px;
      line-height: 40px;
      background-color: white;
    }

    .font-size-text-active.row-monitoring-value__input {
      display: block;
      width: calc(100% - 15vw);
      .el-input__inner{
        font-size: inherit;
        height: 48px;
        line-height: 48px;
        padding: 0;
        color: black;
      }
    }

    .font-size-text-deactive.row-monitoring-value__input {
      display: block;
      width: calc(100% - 15vw);
      .el-input__inner{
        font-size: inherit;
        height: 40px;
        line-height: 40px;
        padding: 0;
        color: black;
      }
    }

    .el-card {
      margin: 20px;
    }
    .bot-side {
      height: fit-content;
      position: absolute;
      bottom: 0;
      width: 100%;
    }
  }
  .btn-fullscreen {
    position: absolute;
    right: 10px;
    top: 0px;
    width: 26px;
    height: 26px;
    padding: 0;
    font-size: 18px;
    line-height: 26px;
    text-align: center;
    outline: none;
  }
  .monitoring-history-table{
    height: calc(100% - 1.3vw - 20px); 
    margin-top: 10px;
    
    .el-table{
      height: 100% !important;
      .el-table__fixed-header-wrapper,
      .el-table__header-wrapper{
        thead{
          tr{
            th{
              background-color: lightgray;
              color: darkblue;
            }
          }
        }
      }
    }

    .el-table-column--selection .cell,
    .el-table th > .cell,
    .el-table .cell {
      padding: 0 10px 0 10px !important;
    }
  }
  .list-side__exception{
    height: calc(100vh - 565px);
  }
}

.customer-monitoring:has(.border-unapprove),
.customer-monitoring:has(.border-approved){
  .list-side{
    height: calc(100vh - 605px) !important;
  }
}

.wrapper.full-screen{
  .monitoring-history-table{
    height: calc(100% - 1.3vw + 30px); 
    .el-table{
      height: 100% !important;
    }
  }
  .list-side__exception{
    height: calc(100vh - 585px);
  }

  .customer-monitoring:has(.border-unapprove),
  .customer-monitoring:has(.border-approved){
    .list-side{
      height: calc(100vh - 625px) !important;
    }
  }
}
.fullscreen {
  overflow-y: hidden !important;
}

.time {
  font-size: 13px;
  color: #999;
}

.bottom {
  margin-top: 13px;
  line-height: 12px;
}
.content-side {
  padding-top: 10px;
}
.list-side {
  // height: 400px;
  height: calc(100vh - 550px);
  margin-top: 10px;
  margin-left: 10px;
  margin-right: 10px;
}
.alert-side {
  height: 40px;
  // border: 1px solid #b5b4b1;
}
.container-center {
  height: 100%;
  display: table;
}
.item-center {
  display: table-cell;
  vertical-align: middle;
}
.button {
  padding: 0;
  float: right;
}

.image {
  width: 100%;
  display: block;
}

.avatar-container {
  height: 100%;
  text-align: center;
}
.avatar {
  width: 100%;
  // border: 1px solid #b5b4b1;
}
.verify-image img{
  object-fit: fill !important;
}
.horizon-center {
  width: 94%;
  margin: auto;
}

.employee-avatar {
  width: 100%;
  display: block;
}

.clearfix:before,
.clearfix:after {
  display: table;
  content: "";
}

.clearfix:after {
  clear: both;
}

.popup-show-col {
  z-index: 10;
  position: absolute;
  bottom: 0px;
  right: 45px;
  height: fit-content;
  width: 230px;
  background-color: whitesmoke;
  border-radius: 10px;
  border: 0.5px solid #bdbdbd;
  box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
  padding: 0px 5px 10px 10px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  .switch-wapper {
    height: fit-content;
    max-height: 40vh;
    overflow: auto;
    width: 100%;
  }
}

.ui-switch__label-text {
  font-size: 12px;
}
.div-process {
  width: 100%;
  text-align: center;
  font-style: italic;
  font-weight: 600;
  font-size: 20px;
  margin-top: 20px;
  color: red;
}
.border-nodata {
  padding: 10px;
  border: 10px sold;
}
.border-approved {
  padding: 10px;
  border: 10px solid #54ff82;
}
.border-unapprove {
  padding: 10px;
  border: 10px solid #ebb563;
}
.red-background {
  background-color: #cc0000;
  color: #fff !important;
  .el-link--inner {
    color: #fff !important;
  }
  .el-checkbox__label {
    color: #fff !important;
  }
  .el-radio__label {
    color: #fff !important;
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

.v-modal{
  background-color: transparent;
  display: none;
}
</style>
