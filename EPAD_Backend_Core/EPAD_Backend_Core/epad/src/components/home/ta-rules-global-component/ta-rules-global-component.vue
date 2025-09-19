<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TARulesGlobal") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <el-collapse v-model="activeCollapse" style="margin-top: 6px" class="ta-rules-global__collapse">
          <el-form ref="taRulesGlobalForm" :model="ruleForm" :rules="rules" style="height: 100%">
            <el-collapse-item :title="$t('RuleLockAttendanceTimeLabel')" name="a">
              <el-col :span="24">
                <el-col :span="4">
                    <label class="el-form-item__label">{{
                      $t("RuleLockAttendanceTime")
                    }}</label>
                </el-col>
                <el-col :span="6">
                  <el-form-item>
                    <el-select
                      ref="LockAttendanceTime"
                      filterable
                      v-model="ruleForm.LockAttendanceTime"
                      style="width: 100% !important;"
                      clearable
                      :placeholder="$t('SelectRuleLockAttendanceTime')"
                    >
                      <el-option
                        v-for="item in listLockAttendanceTime"
                        :key="item.Value"
                        :label="item.Name"
                        :value="item.Value"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-col>
            </el-collapse-item>
            <el-collapse-item :title="$t('RuleOvertimeCoefficient')" name="b">
              <el-col :span="12">
                <el-col :span="8">
                    <label class="el-form-item__label">{{
                      $t("OvertimeNormalDay")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.OverTimeNormalDay"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
              
              <el-col :span="12">
                <el-col :span="12">
                    <label class="el-form-item__label">{{
                      $t("NightOvertimeNormalDay")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.NightOverTimeNormalDay"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
              <el-col :span="12">
                <el-col :span="8">
                    <label class="el-form-item__label">{{
                      $t("OvertimeLeaveDay")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.OverTimeLeaveDay"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
              
              <el-col :span="12">
                <el-col :span="12">
                    <label class="el-form-item__label">{{
                      $t("NightOvertimeLeaveDay")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.NightOverTimeLeaveDay"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
              <el-col :span="12">
                <el-col :span="8">
                    <label class="el-form-item__label">{{
                      $t("OvertimeHoliday")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.OverTimeHoliday"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
              
              <el-col :span="12">
                <el-col :span="12">
                    <label class="el-form-item__label">{{
                      $t("NightOvertimeHoliday")
                    }} (%)</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.NightOverTimeHoliday"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
            </el-collapse-item>
            <el-collapse-item :title="$t('RuleMaximumAnnualLeaveRegister')" name="c">
              <el-col :span="24">
                <el-col :span="4">
                    <label class="el-form-item__label">{{
                      $t("ByMonth(day)")
                    }}</label>
                </el-col>
                <el-col :span="6">
                  <el-form-item>
                    <el-input-number style="width: 100%;" v-model="ruleForm.MaximumAnnualLeaveRegisterByMonth"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57' :min="0"
                      :max="2000000000"></el-input-number>
                  </el-form-item>
                </el-col>
              </el-col>
            </el-collapse-item>
            <el-collapse-item :title="$t('TimeRangeCalculateNightShift')" name="d">
              <el-col :span="10">
                <el-col :span="10">
                    <label class="el-form-item__label left-label">{{
                      $t("NightShiftStartTime")
                    }}</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item prop="NightShiftStartTime">
                    <el-time-picker
                    format="HH:mm"
                      v-model="ruleForm.NightShiftStartTime"
                      :placeholder="$t('SelectTime')"
                    >
                    </el-time-picker>
                  </el-form-item>
                </el-col>
              </el-col>
              
              <el-col :span="10">
                <el-col :span="10">
                    <label class="el-form-item__label left-label">{{
                      $t("NightShiftEndTime")
                    }}</label>
                </el-col>
                <el-col :span="12">
                  <el-form-item prop="NightShiftEndTime">
                    <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.NightShiftEndTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                  </el-form-item>
                </el-col>
              </el-col>
              <el-col :span="4">
                <el-checkbox v-model="ruleForm.NightShiftOvernightEndTime">
                  {{ $t('NightShiftOvernightEndTime') }}
                </el-checkbox>
              </el-col>
            </el-collapse-item>
            <el-collapse-item :title="$t('AutoCalculateAttendance')" name="i">
              <el-col :span="4">
                <el-checkbox v-model="ruleForm.IsAutoCalculateAttendance">
                  {{ $t('AutoCalculateAttendance') }}
                </el-checkbox>
              </el-col>

              <el-col :span="7"  v-if="ruleForm.IsAutoCalculateAttendance">
                <el-col :span="7">
                    <label class="el-form-item__label left-label">{{
                      $t("ProcessingTimeFrame")
                    }}</label>
                </el-col>
                <el-col :span="17"  v-if="ruleForm.IsAutoCalculateAttendance">
                  <el-form-item prop="NightShiftStartTime">
                    <el-select  v-model="listTimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                  </el-form-item>
                </el-col>
              </el-col>
            </el-collapse-item>
          </el-form>
        </el-collapse>
        <el-button type="success" style="float: right; margin-top: 10px;" @click="Submit" icon="el-icon-circle-check">
            {{ $t('Save') }}
        </el-button>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ta-rules-global-component.ts"></script>
<style lang="scss">
.ta-rules-global__collapse {
  height: calc(100vh - 160px);
  .el-form {
    .el-collapse-item {
      margin-bottom: 5px;
      background-color: white;
      border-radius: 10px;
      box-shadow: rgba(0, 0, 0, 0.25) 0px 10px 10px;
      padding: 10px;

      .el-collapse-item__wrap {
        border: unset;
        padding-left: 20px;
      }

      .el-collapse-item__header{
        background-color: white !important;
        color: black;
        font-size: 16px;
        font-weight: bold;
        border: unset;
      }

      .el-collapse-item__content {
        background-color: white;
      }
    }
  }   

  .left-label {
    width: 100%;
    text-align: left;
  }

  .right-label{
    text-align: right;
  }
}

</style>