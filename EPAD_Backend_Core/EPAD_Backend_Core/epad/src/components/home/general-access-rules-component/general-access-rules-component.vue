<template >
  <div style="overflow: hidden;">
    <el-container id="bgHome">
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("GeneralAccessRules") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome" style="overflow: hidden;">
        <el-row style="height: calc(100% - 10px);">
          <el-col class="colList" :span="6"  style="height: 600px;overflow: auto;">
            <el-table
              ref="tblRules"
              :data="listRules"
              @current-change="currentRowChanged"
              highlight-current-row
              style="width: 100%;margin-top: 0px;"
            >
              <el-table-column type="index" width="50"></el-table-column>
              <el-table-column
               
                property="Name"
                :label="$t('RulesList')"
              ></el-table-column>
            </el-table>
          </el-col>

          <el-col class="colDetail" :span="18">
            <div class="general-access-rule__divScrollCollapse" v-if="ruleObject != null">
              <el-collapse v-model="activeCollapse" style="margin-top: 6px">
                <el-form
                  ref="form"
                  :model="ruleObject"
                  :rules="validationObject"
                  label-width="230px"
                  label-position="left"
                >
                  <el-collapse-item :title="$t('BasicInfo')" name="a">
                    <el-form-item
                      prop="Name"
                      :label="$t('RuleName')"
                      style="margin-top: 4px"
                    >
                      <el-input
                        v-model="ruleObject.Name"
                        placeholder
                      ></el-input>
                    </el-form-item>
                    <el-form-item :label="$t('EnglishName')">
                      <el-input v-model="ruleObject.NameInEng"></el-input>
                    </el-form-item>
                  </el-collapse-item>
                  <el-collapse-item
                    :title="$t('RuleAccessTimeInOut')"
                    name="b"
                  >
                    <!-- vào -->
                    <!-- <el-form-item :label="$t('CheckInByShift')">
                      <el-switch
                        v-model="ruleObject.CheckInByShift"
                      ></el-switch>
                    </el-form-item> -->
                    <el-form-item :label="$t('CheckInTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.CheckInTime"
                        style="width: 50%"
                        format="HH:mm"
                        :default-value="defaultValue"
                        :disabled="ruleObject.CheckInByShift"
                      >
                      </el-time-picker>
                    </el-form-item>
                    <el-form-item
                      :label="$t('MaximumMinutesNumberForEarlyIn')"
                      style="margin-top: 4px"
                    >
                      <el-input-number
                        v-model="ruleObject.MaxEarlyCheckInMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <el-form-item
                      :label="$t('MaximumMinutesNumberForLatelyIn')"
                      style="margin-top: 4px"
                    >
                      <el-input-number
                        v-model="ruleObject.MaxLateCheckInMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <!-- ra -->
                    <!-- <el-form-item :label="$t('CheckOutByShift')">
                      <el-switch
                        v-model="ruleObject.CheckOutByShift"
                      ></el-switch>
                    </el-form-item> -->
                    <el-form-item :label="$t('CheckOutTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.CheckOutTime"
                        format="HH:mm"
                        style="width: 50%"
                        :default-value="defaultValue"
                        :disabled="ruleObject.CheckOutByShift"
                      ></el-time-picker>
                    </el-form-item>
                    <el-form-item
                      :label="$t('MaximumMinutesNumberForEarlyOut')"
                      style="margin-top: 4px"
                    >
                      <el-input-number
                        v-model="ruleObject.MaxEarlyCheckOutMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <el-form-item
                      :label="$t('MaximumMinutesNumberForLatelyOut')"
                      style="margin-top: 4px"
                    >
                      <el-input-number
                        v-model="ruleObject.MaxLateCheckOutMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <el-form-item style="margin: 0 0 5px 20px"
                      class="allow-free-in-out-in-time-range__form-item"
                    >
                      <el-checkbox
                        v-model="ruleObject.AllowFreeInAndOutInTimeRange"
                      >{{ $t('AllowFreeInAndOutInTimeRange') }}</el-checkbox>
                    </el-form-item>

                    <!-- <el-form-item :label="$t('EndFirstHaftTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.EndFirstHaftTime"
                        style="width: 50%"
                        format="HH:mm"
                        :default-value="defaultValue"
                      >
                      </el-time-picker>
                    </el-form-item>
                    <el-form-item :label="$t('BeginLastHaftTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.BeginLastHaftTime"
                        style="width: 50%"
                        format="HH:mm"
                        :default-value="defaultValue"
                      >
                      </el-time-picker>
                    </el-form-item>

                    <el-form-item :label="$t('ProgressAccordingToRegistration')">
                      <el-switch
                        v-model="ruleObject.AdjustByLateInEarlyOut"
                      ></el-switch>
                    </el-form-item> -->
                  </el-collapse-item>
                  <!-- Các qui định liên quan đến ra giữa giờ có đăng ký -->
                  <el-collapse-item
                    style="display: none;"
                    :title="$t('RuleBreakOutRegister')"
                    name="c"
                  >
                    <el-form-item
                      :label="$t('AllowAccessLeaveTime')"
                      label-width="330px"
                    >
                      <el-switch
                        v-model="ruleObject.AllowInLeaveDay"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item
                      style="margin-bottom: 0 !important;"
                      :label="$t('AllowAccessBussinessTime')"
                      label-width="330px"
                    >
                      <el-switch
                        v-model="ruleObject.AllowInMission"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item style="margin: 0 0 5px 20px"
                      class="allow-early-out-late-in-mission__form-item"
                      v-if="ruleObject.AllowInMission"
                    >
                      <el-checkbox
                        v-model="ruleObject.AllowEarlyOutLateInMission"
                      >{{ $t('AllowEarlyOutLateInMission') }}</el-checkbox>
                    </el-form-item>
                    <el-form-item :label="$t('MaximumMinutesNumberForEarlyOut')" style="margin-left: 20px;"
                    v-if="ruleObject.AllowInMission && ruleObject.AllowEarlyOutLateInMission">
                      <el-input-number
                        v-model="ruleObject.MissionMaxEarlyCheckOutMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <el-form-item :label="$t('MaximumMinutesNumberForLatelyIn')" style="margin-left: 20px;"
                    v-if="ruleObject.AllowInMission && ruleObject.AllowEarlyOutLateInMission">
                      <el-input-number
                        v-model="ruleObject.MissionMaxLateCheckInMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <!-- <el-form-item
                  :label="$t('ChoPhepVaoRaTheoGioDangKyRaGiuaGio')"
                  label-width="330px"
                >
                  <el-switch v-model="ruleObject.AllowInBreakTime"></el-switch>
                </el-form-item> -->
                  </el-collapse-item>
                  <!-- Các qui định liên quan đến ra giữa giờ không đăng ký -->
                  <el-collapse-item
                    :title="$t('RuleBreakOutDontRegister')"
                    name="d"
                  >
                    <el-form-item :label="$t('AllowedOutDuringBusinessHours')">
                      <el-switch @change="changeAllowCheckOutInWorkingTime"
                        v-model="ruleObject.AllowCheckOutInWorkingTime"
                      ></el-switch>
                    </el-form-item>
                    <!-- <el-form-item :label="$t('MaxMinutes')">
                      <el-input-number
                        v-model="ruleObject.MaxMinuteAllowOutsideInWorkingTime"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item> -->
                    <el-form-item prop="AllowCheckOutInWorkingTimeRange" v-if="ruleObject.AllowCheckOutInWorkingTime"
                      class="allow-check-out-in-working-time-range__form-item">
                      <fieldset>
                        <legend>{{$t('PleaseInputTimeAllowCheckOutInWorkingTime')}}</legend>
                        <el-row :gutter="10" style="font-weight: bold;">
                          <el-col :span="8">
                            {{ $t('FromTime') }} <span style="color: red;">*</span>
                          </el-col>
                          <el-col :span="8">
                            {{ $t('ToTime') }} <span style="color: red;">*</span>
                          </el-col>
                        </el-row>
                        <template v-for="(item, index) in allowCheckOutInWorkingTimeRange">
                          <el-row :gutter="10" style="margin-bottom: 10px;">
                            <el-col :span="8">
                              <el-time-picker :class="(item.Error && item.Error != '') ? 'is-error' : 'is-not-error'"
                                v-model="item.FromTime"
                                :placeholder="$t('SelectTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="8">
                              <el-time-picker :class="(item.Error && item.Error != '') ? 'is-error' : 'is-not-error'"
                                v-model="item.ToTime"
                                :placeholder="$t('SelectTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="8">
                              <el-button type="primary" icon="el-icon-plus" size="mini" @click="addAllowCheckOutInWorkingTimeRow(index, item)" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                              <el-button type="warning" icon="el-icon-close" size="mini" @click="deleteAllowCheckOutInWorkingTimeRow(index, item)"
                              v-if="index > 0" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                            </el-col>
                            <el-col :span="24">
                              <span class="text-danger" style="color: red;">{{ item.Error }} </span>
                            </el-col>
                          </el-row>
                        </template>
                      </fieldset>
                    </el-form-item>
                  </el-collapse-item>
                  <!-- Các qui định cấm vào ra -->
                  <!-- <el-collapse-item :title="$t('QuyDinhCamVaoRa')" v-if="logonUsername == 'superadmin@giaiphaptinhhoa.com'" name="e">
                <el-form-item
                  :label="$t('KhongChoPhepVaoKhiDangKyNghiCaNgay')"
                  label-width="340px"
                >
                  <el-switch
                    v-model="ruleObject.DenyInLeaveWholeDay"
                  ></el-switch>
                </el-form-item>
                <el-form-item
                  :label="$t('KhongChoPhepVaoKhiDangKyCongTacCaNgay')"
                  label-width="340px"
                >
                  <el-switch
                    v-model="ruleObject.DenyInMissionWholeDay"
                  ></el-switch>
                </el-form-item>
                <el-form-item
                  :label="$t('KhongChoPhepVaoTrongKhiNghiViec')"
                  label-width="340px"
                >
                  <el-switch
                    v-model="ruleObject.DenyInStoppedWorkingInfo"
                  ></el-switch>
                </el-form-item>
              </el-collapse-item> -->
                  <!-- Các qui định thứ tự điểm danh -->
                  <el-collapse-item
                    :title="$t('RuleOrderOfAttendance')"
                    v-if="logonUsername == 'superadmin@giaiphaptinhhoa.com'"
                    name="f"
                  >
                    <el-form-item
                      :label="$t('CheckLogByShift')"
                      label-width="340px"
                    >
                      <el-switch
                        v-model="ruleObject.CheckLogByShift"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item
                      :label="$t('CheckLogByAreaGroup')"
                      label-width="340px"
                    >
                      <el-switch
                        v-model="ruleObject.CheckLogByAreaGroup"
                        @change="CheckShowTableAreaGroup"
                      ></el-switch>
                      <span
                        style="margin-left: 20px; color: red; font-weight: 700"
                        >{{ errorAreaGroup }}</span
                      >
                    </el-form-item>
                    <el-form-item
                      label=""
                      label-width="0px"
                      v-if="ruleObject.CheckLogByAreaGroup"
                    >
                      <el-table
                        :data="ruleObject.AreaGroups"
                        style="width: 100%; margin-bottom: 20px"
                      >
                        <el-table-column
                          prop="Priority"
                          :label="$t('Priority')"
                          :width="230"
                        >
                          <template slot-scope="scope">
                            <el-input
                              v-model="scope.row.Priority"
                              disabled
                            ></el-input>
                          </template>
                        </el-table-column>
                        <el-table-column
                          prop="AreaGroupIndex"
                          :label="$t('AreaGroup')"
                          :width="210"
                        >
                          <template slot-scope="scope">
                            <app-select
                              :dataSource="areaGroupList"
                              :dataDisabled="getDataDisabled()"
                              displayMember="Name"
                              valueMember="Index"
                              v-model="scope.row.AreaGroupIndex"
                            ></app-select>
                          </template>
                        </el-table-column>
                        <el-table-column prop="Function" label="" width="100">
                          <template slot-scope="scope">
                            <el-button
                              type="warning"
                              icon="el-icon-close"
                              size="mini"
                              @click="deleteRow(scope.$index, scope.row)"
                              v-if="ruleObject.AreaGroups.length > 1"
                              circle
                            ></el-button>

                            <el-button
                              type="primary"
                              icon="el-icon-plus"
                              size="mini"
                              @click="addRow(scope.$index, scope.row)"
                              v-if="scope.$index == 0"
                              circle
                            ></el-button>
                          </template>
                        </el-table-column>
                        <el-table-column prop="Error">
                          <template slot-scope="scope">
                            <span class="text-danger"
                              >{{ scope.row.Error }}
                            </span>
                          </template>
                        </el-table-column>
                      </el-table>
                    </el-form-item>
                  </el-collapse-item>
                  <!-- Các qui định khu vực được phép truy cập -->
                  <el-collapse-item
                    :title="$t('QuyDinhKhuVucDuocPhepTruyCap')"
                    name="g"
                  >
                    <el-form-item
                      prop="ListGatesInfo"
                      :label="$t('AreaListForAcess')"
                      style="margin-top: 4px"
                    >
                      <el-tree
                        :data="gatelineData"
                        show-checkbox
                        node-key="ID"
                        ref="gatelineTree"
                        :props="{ label: 'Name', children: 'ListChildrent' }"
                      >
                      </el-tree>
                    </el-form-item>
                  </el-collapse-item>
                  <el-collapse-item :title="$t('RuleExternalIntegrateFromEzHR')" name="h">
                    <el-form-item :label="$t('CheckInByShift')">
                      <el-switch
                        v-model="ruleObject.CheckInByShift"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item :label="$t('CheckOutByShift')">
                      <el-switch
                        v-model="ruleObject.CheckOutByShift"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item :label="$t('EndFirstHaftTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.EndFirstHaftTime"
                        style="width: 50%"
                        format="HH:mm"
                        :default-value="defaultValue"
                      >
                      </el-time-picker>
                    </el-form-item>
                    <el-form-item :label="$t('BeginLastHaftTime')">
                      <el-time-picker
                      :placeholder="$t('SelectTime')"
                        v-model="ruleObject.BeginLastHaftTime"
                        style="width: 50%"
                        format="HH:mm"
                        :default-value="defaultValue"
                      >
                      </el-time-picker>
                    </el-form-item>

                    <el-form-item :label="$t('ProgressAccordingToRegistration')">
                      <el-switch
                        v-model="ruleObject.AdjustByLateInEarlyOut"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item
                      :label="$t('AllowAccessLeaveTime')"
                      label-width="330px"
                    >
                      <el-switch
                        v-model="ruleObject.AllowInLeaveDay"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item
                      style="margin-bottom: 0 !important;"
                      :label="$t('AllowAccessBussinessTime')"
                      label-width="330px"
                    >
                      <el-switch
                        v-model="ruleObject.AllowInMission"
                      ></el-switch>
                    </el-form-item>
                    <el-form-item style="margin: 0 0 5px 20px"
                      class="allow-early-out-late-in-mission__form-item"
                      v-if="ruleObject.AllowInMission"
                    >
                      <el-checkbox
                        v-model="ruleObject.AllowEarlyOutLateInMission"
                      >{{ $t('AllowEarlyOutLateInMission') }}</el-checkbox>
                    </el-form-item>
                    <el-form-item :label="$t('MaximumMinutesNumberForEarlyOut')" style="margin-left: 20px;"
                    v-if="ruleObject.AllowInMission && ruleObject.AllowEarlyOutLateInMission">
                      <el-input-number
                        v-model="ruleObject.MissionMaxEarlyCheckOutMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <el-form-item :label="$t('MaximumMinutesNumberForLatelyIn')" style="margin-left: 20px;"
                    v-if="ruleObject.AllowInMission && ruleObject.AllowEarlyOutLateInMission">
                      <el-input-number
                        v-model="ruleObject.MissionMaxLateCheckInMinute"
                        style="width: 50%"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                  </el-collapse-item>
                </el-form>
              </el-collapse>
            </div>
          </el-col>
        </el-row>
      </el-main>
      <el-row>
        <el-col :span="6" align="right" style="margin-bottom:10px">
          <el-button 
          type="primary"
          :disabled="this.activeMenuIndex < 1" 
          @click="deleteRule"
          style="margin-right: 5px; background-color: red !important;">
          <i class="el-icon-delete"></i> {{$t("Delete") }}
        </el-button>
        <el-button 
          type="primary" 
          @click="addRule">
          <i class="el-icon-plus"></i> {{ $t("Add") }}
        </el-button>
        </el-col>
        <el-col :span="17" align="right">
          <!-- <el-button
            type="success"
            size="mini"
            icon="el-icon-circle-check"
            @click="submit()"
            >{{ $t("Save") }}</el-button
          > -->
          <el-button type="primary" @click="updateResult" 
          style="background-color: #67C23A !important;">
          <i class="el-icon-circle-check"></i> {{ $t("Save") }}</el-button>
        </el-col>
      </el-row>
  
    </el-container>
  </div>
</template>

<script src="./general-access-rules-component.ts" />
<style lang="scss">
.colList {
  height: 80%;
}
.colDetail {
  height: 100%;
}
.el-table__row {
  cursor: pointer;
}
.el-table__body tr.current-row > td {
  background-color: #f1f1f1;
}
.el-collapse-item__header {
  font-weight: bold;
  margin-left: 10px;
  margin-right: 10px;
  background-color: white;
  color: black;
}
// .el-collapse-item {
//   margin-bottom: 5px;
//   background-color: white;
//   border-radius: 10px;
//   box-shadow: rgba(0, 0, 0, 0.25) 0px 10px 10px;
// }

.el-collapse-item__wrap {
  /*border-radius: 10px;
    border: 1px solid #e8e8e8;*/
}
.el-collapse-item__content {
  padding-bottom: 0px;
  margin-left: 10px;
  margin-right: 10px;
  background-color: white;
}
.general-access-rule__divScrollCollapse {
  height: 100%;
  overflow-y: auto;
  padding-left: 20px;
  padding-bottom: 20px;
  padding-right: 20px;
  // padding-top: 27px;
  .el-collapse {
    .el-form {
      .el-collapse-item {
        margin-bottom: 5px;
        background-color: white;
        border-radius: 10px;
        box-shadow: rgba(0, 0, 0, 0.25) 0px 10px 10px;
      }
    }
  }
  .el-form-item__label {
    font-size: 12px;
    font-weight:500;
  }
}

.allow-free-in-out-in-time-range__form-item{
  margin-left: 0 !important;
  .el-form-item__content {
    margin-left: 0 !important;
    span {
      font-size: 14px !important;
    }
  }
}

.allow-early-out-late-in-mission__form-item {
  .el-form-item__content {
    margin-left: 0 !important;
    span {
      font-size: 12px !important;
    }
  }
}

.allow-check-out-in-working-time-range__form-item {
  .el-form-item__content {
    margin-left:  0 !important;
    font-size: 12px !important;
    fieldset {
      border-radius: 5px;
      border: 1px solid black;
    }
  }
}

.el-form-item.is-error .is-not-error .el-input__inner{
  border-color: #C0C4CC !important;
}

.el-form-item.is-error .is-error .el-input__inner{
  border-color: red !important;
}
</style>
