<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TAShift") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome ta-shift-el-main">
        <el-row class="rowdeviceby" :gutter="10" style="height: 100%">
          <el-col :span="5" style="height: 100%">
            <el-table
              highlight-current-row
              ref="taShiftTable"
              :data="tblShift"
              @current-change="handleCurrentController"
              style="cursor: pointer; height: 100%"
              class="ta-shift-table"
            >
              <el-table-column
                prop="Name"
                :label="$t('TAShift')"
              ></el-table-column>
            </el-table>
          </el-col>
          <el-col :span="19" style="height: 100%; overflow-y: auto;">
            <el-form ref="ruleForm" :model="ruleForm" :rules="rules" style="height: 100%">
              <el-row>
                <h4>
                  {{ $t("BasicInfo") }}
                </h4>
                <el-col :span="24">
                  <el-col :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("ShiftCode")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="20">
                    <el-form-item
                      prop="Code"
                      @click.native="focus('ShiftCode')"
                    >
                      <el-input
                        ref="Code"
                        v-model="ruleForm.Code"
                      ></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24">
                  <el-col :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("ShiftName")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="20">
                    <el-form-item
                      prop="Name"
                      @click.native="focus('Name')"
                    >
                      <el-input
                        ref="Name"
                        v-model="ruleForm.Name"
                      ></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24">
                  <el-col :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label">{{
                        $t("Descriptions")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="20">
                    <el-form-item
                      prop="Description"
                      @click.native="focus('Description')"
                    >
                      <el-input
                        ref="Description"
                        v-model="ruleForm.Description"
                      ></el-input>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24">
                  <el-col :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("AppliedRule")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="20">
                    <el-form-item prop="RulesShiftIndex">
                      <el-select
                        v-model="ruleForm.RulesShiftIndex"
                        filterable
                        style="width: 100%"
                      >
                        <el-option
                          v-for="item in selectUserTypeOption"
                          :key="item.Index"
                          :label="$t(item.Name)"
                          :value="item.Index"
                        ></el-option>
                      </el-select>
                    </el-form-item>
                  </el-col>
                </el-col>
                <h4>
                  {{ $t("ShiftInformation") }}
                </h4>
                <el-col :span="24">
                  <el-col :span="4">
                    <el-checkbox v-model="ruleForm.IsPaidHolidayShift" class="bold-label">
                      {{ $t("PaidHolidayShift") }}
                    </el-checkbox>
                  </el-col>
                </el-col>
                <el-col :span="24" v-if="ruleForm.IsPaidHolidayShift" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("StartOTTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="PaidHolidayStartTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.PaidHolidayStartTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("EndOTTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="PaidHolidayEndTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.PaidHolidayEndTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item>
                      <el-checkbox v-model="ruleForm.PaidHolidayEndOvernightTime">{{
                        $t("EndOvernightTime")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                </el-col>

                <el-col :span="24" style="margin-top: 20px;" v-if="!ruleForm.IsPaidHolidayShift">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("CheckInTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="CheckInTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.CheckInTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("CheckOutTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="CheckOutTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.CheckOutTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item>
                      <el-checkbox v-model="ruleForm.CheckOutOvernightTime">{{
                        $t("CheckOutOvernightTime")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24" style="margin-top: 20px;" v-if="!ruleForm.IsPaidHolidayShift">
                  <el-col :span="4">
                    <el-checkbox v-model="ruleForm.IsBreakTime" class="bold-label">{{
                      $t("HaveBreakTime")
                    }}</el-checkbox>
                  </el-col>
                </el-col>
                <el-col :span="24" v-if="ruleForm.IsBreakTime && !ruleForm.IsPaidHolidayShift" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("BeginBreakTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                 
                  <el-col :span="4">
                    <el-form-item prop="BreakStartTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.BreakStartTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item prop="BreakStartOvernightTime">
                      <el-checkbox v-model="ruleForm.BreakStartOvernightTime">{{
                        $t("NextDay")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("EndBreakTime")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="BreakEndTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.BreakEndTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item prop="BreakEndOvernightTime">
                      <el-checkbox v-model="ruleForm.BreakEndOvernightTime">{{
                        $t("NextDay")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                  <el-col :span="24">
                    <el-form-item style="margin-bottom: 0; margin-top: 10px;">
                      <!-- <el-checkbox v-model="ruleForm.DetermineBreakTimeByAttendanceLog">{{
                        $t("DetermineBreakTimeByAttendanceLog")
                      }}</el-checkbox> -->
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24" style="margin-top: 20px;" v-if=!ruleForm.IsPaidHolidayShift>
                  <el-col :span="4">
                    <el-checkbox v-model="ruleForm.IsOTFirst" class="bold-label">{{
                      $t("HaveOTFirst")
                    }}</el-checkbox>
                  </el-col>
                </el-col>
                <el-col :span="24" v-if="ruleForm.IsOTFirst && !ruleForm.IsPaidHolidayShift" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("BeginOT")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="OTStartTimeFirst">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.OTStartTimeFirst"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("EndOT")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="OTEndTimeFirst">
                      <el-time-picker
                        format="HH:mm"
                        v-model="ruleForm.OTEndTimeFirst"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                </el-col>
                <!-- OT Sau -->
                <el-col :span="24" style="margin-top: 20px;" v-if=!ruleForm.IsPaidHolidayShift>
                  <el-col :span="4">
                    <el-checkbox v-model="ruleForm.IsOT" class="bold-label">{{
                      $t("HaveOTLater")
                    }}</el-checkbox>
                  </el-col>
                </el-col>
                <el-col  style="margin-top: 10px;" v-if="ruleForm.IsOT && !ruleForm.IsPaidHolidayShift" :span="5">
                  <el-form-item>
                    <label class="el-form-item__label left-label">{{
                      $t("BreakTimeBeforeOT")
                    }}</label>
                  </el-form-item>
                </el-col>
                <el-col v-if="ruleForm.IsOT && !ruleForm.IsPaidHolidayShift" :span="4">
                  <el-form-item>
                    <el-input-number
                      v-model="ruleForm.BreakTimeBeforeOT"
                      onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                      :min="0"
                      :max="2000000000"
                      style="width: 100%"
                    ></el-input-number>
                  </el-form-item>
                </el-col>
                <el-col :span="24" v-if="ruleForm.IsOT && !ruleForm.IsPaidHolidayShift" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("BeginOT")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="OTStartTime">
                      <el-time-picker
                      format="HH:mm"
                        v-model="ruleForm.OTStartTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item prop="OTStartOvernightTime">
                      <el-checkbox v-model="ruleForm.OTStartOvernightTime">{{
                        $t("NextDay")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="4">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("EndOT")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item prop="OTEndTime">
                      <el-time-picker
                        format="HH:mm"
                        v-model="ruleForm.OTEndTime"
                        :placeholder="$t('SelectTime')"
                      >
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="3" style="margin-left: 10px;">
                    <el-form-item prop="OTEndOvernightTime">
                      <el-checkbox v-model="ruleForm.OTEndOvernightTime">{{
                        $t("NextDay")
                      }}</el-checkbox>
                    </el-form-item>
                  </el-col>
                </el-col>

                <el-col :span="24" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label">{{
                        $t("AllowedLatelyIn(minute)")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item>
                      <el-input-number
                        v-model="ruleForm.AllowLateInMinutes"
                        onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                        :min="0"
                        :max="2000000000"
                        style="width: 100%"
                      ></el-input-number>
                    </el-form-item>
                  </el-col>
                  <el-col style="margin-left: 20px;" :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label">{{
                        $t("AllowedEarlyOut(minute)")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item>
                      <el-input-number
                        v-model="ruleForm.AllowEarlyOutMinutes"
                        onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                        :min="0"
                        :max="2000000000"
                        style="width: 100%"
                      ></el-input-number>
                    </el-form-item>
                  </el-col>
                </el-col>
                <el-col :span="24" style="margin-top: 20px;">
                  <el-col :span="5">
                    <el-form-item>
                      <label class="el-form-item__label left-label required">{{
                        $t("TheoryWorkedTimeByShift")
                      }}</label>
                    </el-form-item>
                  </el-col>
                  <el-col :span="4">
                    <el-form-item
                      prop="TheoryWorkedTimeByShift"
                      @click.native="focus('TheoryWorkedTimeByShift')"
                    >
                      <el-input-number id="theory-work-time-shift__input-number"
                        :disabled="ruleForm.IsPaidHolidayShift"
                        style="width: 100%;"
                        ref="TheoryWorkedTimeByShift"
                        v-model="ruleForm.TheoryWorkedTimeByShift"
                        onkeypress='return (event.charCode >= 48 && event.charCode <= 57) || event.charCode == 46'
                        :min="0"
                        :max="2000000000"
                        
                      ></el-input-number>
                    </el-form-item>
                  </el-col>
                </el-col>
              </el-row>
            </el-form>
          </el-col>
        </el-row>
      </el-main>
      <el-row>
        <el-col :span="5" align="right" style="margin-bottom:10px">
          <el-button 
          type="primary"
          @click="Delete"
          style="margin-right: 5px; background-color: red !important;">
          <i class="el-icon-delete"></i> {{$t("Delete") }}
        </el-button>
        <el-button 
          type="primary" 
          @click="Insert">
          <i class="el-icon-plus"></i> {{ $t("Add") }}
        </el-button>
        </el-col>
        <el-col :span="19" align="right">
          <el-button type="primary" @click="Submit" 
          style="background-color: #67C23A !important; margin-right: 10px;">
          <i class="el-icon-circle-check"></i> {{ $t("Save") }}</el-button>
        </el-col>
      </el-row>
    </el-container>
  </div>
</template>
<script src="./ta-shift-component.ts"></script>
  
<style lang="scss">
.ta-shift-el-main {
  .customdialog {
    width: 1200px;
  }

  .left-label {
    width: 100%;
    text-align: left;
  }

  .right-label{
    text-align: right;
  }

  .bold-label {
    font-weight: bold;
  }
}
.el-form-item__label {
  font-weight: 200;
}
.required:after {
  content: "*";
  color: red;
}
.ta-shift-table{
  .current-row {
    td{
        background-color: #B9B9B9 !important;
    }
  }
}
</style>
  