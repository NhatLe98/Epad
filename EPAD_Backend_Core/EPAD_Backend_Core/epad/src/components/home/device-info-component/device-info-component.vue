<template>
  <div id="bgHome" v-loading="loading">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("DeviceInfo") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="$t('ChooseTimePeriod')" :visible.sync="isShowAttendance" 
            custom-class="customdialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form
              :model="TimeForm"
              :rules="rule"
              ref="TimeForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="DownloadAttendanceData"
            >
            <el-form-item :label="$t('')" prop="IsDownloadFull">
                <el-checkbox v-model="TimeForm.IsDownloadFull"
                  >{{ $t("DownloadFull") }}
                </el-checkbox>
              </el-form-item>
              <el-form-item :label="$t('FromTime')" prop="FromTime">
                <el-date-picker v-model="TimeForm.FromTime" 
                 type="datetime"
                 :disabled="TimeForm.IsDownloadFull"></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('ToTime')" prop="ToTime">
                <el-date-picker v-model="TimeForm.ToTime"
                type="datetime"
                 :disabled="TimeForm.IsDownloadFull"></el-date-picker>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">{{ $t("Cancel") }}</el-button>
              <el-button class="btnOK" type="primary" @click="DownloadAttendanceData">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>
          <el-dialog :title="$t('ChooseTimePeriod')" :visible.sync="isDeleteAttendance" 
          custom-class="customdialog" :before-close="CancelDeleteAttendance" :close-on-click-modal="false">
            <el-form
              :model="DeleteAttendaceTimeForm"
              :rules="rule"
              ref="DeleteAttendaceTimeForm"
              label-width="168px"
              label-position="top"
            >
            <el-form-item :label="$t('')" prop="IsDeleteAll">
                <el-checkbox v-model="DeleteAttendaceTimeForm.IsDeleteAll"
                  >{{ $t("DeleteAllAttendanceLog") }}
                </el-checkbox>
              </el-form-item>
              <el-form-item :label="$t('FromTime')" prop="FromTime">
                <el-date-picker v-model="DeleteAttendaceTimeForm.FromTime" 
                 type="datetime"
                 :disabled="DeleteAttendaceTimeForm.IsDeleteAll"></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('ToTime')" prop="ToTime">
                <el-date-picker v-model="DeleteAttendaceTimeForm.ToTime"
                type="datetime"
                 :disabled="DeleteAttendaceTimeForm.IsDeleteAll"></el-date-picker>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="CancelDeleteAttendance">{{ $t("Cancel") }}</el-button>
              <el-popover
                placement="top"
                v-model="openingConfirmDeletePopup">
                <span>{{ $t('MSG_Confirm') }}</span>
                <span style="cursor: pointer; float: right;" @click="openingConfirmDeletePopup = false;">X</span>
                <p style="margin-top: 15px;"><i class="el-icon-warning" style="color: orange; font-size: 20px;"></i> {{ $t('DataWillBePernamentDelete_PleaseInputPassword') }}</p>
                <el-input v-model="confirmDeletePassword" type="password"></el-input>
                <div style="text-align: right; margin-top: 5px">
                  <el-button size="mini" 
                    style="margin-right: 5px;" class="btnCancel"
                    @click="CloseDeleteAttendanceLogForm">{{$t('MSG_No')}}</el-button>
                  <el-button type="primary" size="mini" @click="DeleteAttendanceData">{{$t('Delete')}}</el-button>
                </div>
                <el-button class="btnOK" type="primary" slot="reference">{{ $t("OK") }}</el-button>
              </el-popover>
            </span>
          </el-dialog>
        </div>
        <div>
          <DeviceInfoTable :selectedRows.sync="rowsObj">
            <template #toolbarExtend>
              <div class="button-function">
                <el-button type="primary" @click="CheckManualConnect">
                    {{ $t("CheckManualConnect") }}
                </el-button>
                <el-button type="primary" @click="DownloadDeviceInfo">
                    {{ $t("LoadInfo") }}
                </el-button>
                <el-button style="margin-left: 10px" type="primary"
                   :loading="isWaitingDownloadAttendanceResponse"
                @click="FormAttendanceData">
                    {{ $t("LoadDataAttendanceLog") }}
                </el-button>
                <el-button style="margin-left: 10px" type="primary"
                @click="FormDeleteAttendanceData">
                    {{ $t("DeleteDataAttendanceLog") }}
                </el-button>
                <el-button type="primary" @click="RestartDevice">
                    {{ $t("RestartDevice") }}
                </el-button>
                 <el-button type="primary" @click="SetDeviceTime">
                    {{ $t("SetDeviceTime") }}
                </el-button>
              </div>
            </template>
          </DeviceInfoTable>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./device-info-component.ts"></script>

<style scoped>
.button-function {
  margin-right: 24px;
  width: fit-content;
  display: flex;
  justify-content: space-between;
  height: 36px;
  position: absolute;
  top: 29px;
  right: 12px;
}
.button-function .el-button {
  margin-left: 10px;
}
</style>
