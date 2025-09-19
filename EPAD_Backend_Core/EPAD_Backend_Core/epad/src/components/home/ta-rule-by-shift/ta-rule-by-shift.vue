<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TARuleByShift") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome ta-rules-shift-el-main">
        <el-row :gutter="10" style="height: 100%">
          <el-col :span="5" style="height: 100%; overflow-y: auto;">
            <el-table
              ref="taRulesShiftTable"
              :data="tblRule"
              highlight-current-row
              @current-change="handleChangeRule"
              style="cursor: pointer; height: 100%"
              class="ta-rule-shift-table"
            >
              <el-table-column
                prop="Name"
                :label="$t('ListRule')"
              ></el-table-column>
            </el-table>
          </el-col>
          <el-col :span="19" style="height: 100%; overflow-y: auto;">
            <el-collapse v-model="activeCollapse" style="margin-top: 6px" class="ta-rule-by-shift__collapse">
              <el-form ref="ruleForm" :model="ruleForm" :rules="rules" style="height: 100%">
                <el-row>
                  <el-collapse-item :title="$t('BasicInfo')" name="a">
                    <el-col :span="24">
                      <el-col :span="3">
                        <el-form-item>
                          <label class="el-form-item__label required">{{
                            $t("RuleName")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="21">
                        <el-form-item prop="Name" @click.native="focus('Name')" >
                          <el-input ref="Name" v-model="ruleForm.Name"></el-input>
                        </el-form-item>
                      </el-col>
                    </el-col>
                    <el-col :span="24">
                      <el-col :span="3">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("Descriptions")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="21">
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
                  </el-collapse-item>

                  <el-collapse-item :title="$t('RuleInOut')" name="b">
                    <el-col :span="24">
                      <el-form-item>
                        <div>
                          <el-radio-group v-model="ruleForm.RuleInOut">
                          <el-row>
                            <el-col :span="24">
                              <el-radio :label="1">
                                {{ $t("FirstValidInLastValidOut") }}
                              </el-radio></el-col>
                              <el-col :span="24">
                              <el-radio :label="0" style="margin-top: 10px">
                                {{ $t("Other") }}
                              </el-radio></el-col>
                          </el-row>
                        </el-radio-group>
                        </div>
                        <div>
                          <el-radio-group v-model="ruleForm.RuleInOutOther" 
                          v-if="ruleForm.RuleInOut != null && ruleForm.RuleInOut != 1" 
                          style="padding-left: 20px;">
                          <el-row>
                            <el-col :span="24" style="margin-top: 10px">
                              <el-radio :label="2">
                                {{ $t("ByInOutOfDevice") }}
                              </el-radio></el-col
                            >
                            <el-col :span="24" style="margin-top: 10px">
                              <el-radio :label="3">
                                {{ $t("ByRespectivelyData") }}
                              </el-radio></el-col
                            >
                            <!-- <el-col :span="24" style="margin-top: 10px">
                              <el-radio :label="4">
                                {{ $t("ByRangeTime") }}
                              </el-radio></el-col
                            > -->
                          </el-row>
                        </el-radio-group>
                        </div>
                      </el-form-item>
                    </el-col>
                    <el-col :span="24" v-if="ruleForm.RuleInOut == 0 && ruleForm.RuleInOutOther == 4"
                      style="padding-left: 40px;">
                      <el-form-item prop="RuleShiftInOut" class="ignore-error">
                        <el-row :gutter="10" >
                          <el-col :span="8" style="font-weight: bold;">
                            {{ $t('TimeInLabel') }}
                          </el-col>
                        </el-row>
                        <template v-for="(item, index) in ruleShiftInOut.filter(x => x.TimeMode == 0)">
                          <el-row :gutter="10" style="margin-bottom: 10px;" :class="(item.Error && item.Error != '') ? 'error-row' : ''">
                            <el-col :span="6">
                              <el-time-picker
                                v-model="item.FromTime"
                                :placeholder="$t('StartTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="3">
                              <el-checkbox v-if="item.TimeMode == 1" v-model="item.FromOvernightTime">{{
                                $t("NextDay")
                              }}</el-checkbox>
                              <span v-else style="opacity: 0; cursor: default;">a</span>
                            </el-col>
                            <el-col :span="6">
                              <el-time-picker
                                v-model="item.ToTime"
                                :placeholder="$t('EndTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="3">
                              <el-checkbox v-model="item.ToOvernightTime">{{
                                $t("NextDay")
                              }}</el-checkbox>
                            </el-col>
                            <el-col :span="6">
                              <el-button type="primary" icon="el-icon-plus" size="mini" @click="addRuleShiftInOut(index, item, 0)" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                              <el-button type="warning" icon="el-icon-close" size="mini" @click="deleteRuleShiftInOut(index, item, 0)"
                              v-if="index > 0" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                            </el-col>
                            <el-col :span="24">
                              <span class="text-danger" style="color: red;">{{ item.Error }} </span>
                            </el-col>
                          </el-row>
                        </template>
                        <el-row :gutter="10" >
                          <el-col :span="8" style="font-weight: bold;">
                            {{ $t('TimeOutLabel') }}
                          </el-col>
                        </el-row>
                        <template v-for="(item, index) in ruleShiftInOut.filter(x => x.TimeMode != 0)">
                          <el-row :gutter="10" style="margin-bottom: 10px;" :class="(item.Error && item.Error != '') ? 'error-row' : ''">
                            <el-col :span="6">
                              <el-time-picker
                                v-model="item.FromTime"
                                :placeholder="$t('StartTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="3">
                              <el-checkbox v-if="item.TimeMode == 1" v-model="item.FromOvernightTime">{{
                                $t("NextDay")
                              }}</el-checkbox>
                              <span v-else style="opacity: 0; cursor: default;">a</span>
                            </el-col>
                            <el-col :span="6">
                              <el-time-picker
                                v-model="item.ToTime"
                                :placeholder="$t('EndTime')"
                                format="HH:mm"
                                style="width: 100%;"
                              >
                              </el-time-picker>
                            </el-col>
                            <el-col :span="3">
                              <el-checkbox v-model="item.ToOvernightTime">{{
                                $t("NextDay")
                              }}</el-checkbox>
                            </el-col>
                            <el-col :span="6">
                              <el-button type="primary" icon="el-icon-plus" size="mini" @click="addRuleShiftInOut(index, item, 1)" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                              <el-button type="warning" icon="el-icon-close" size="mini" @click="deleteRuleShiftInOut(index, item, 1)"
                              v-if="index > 0" circle
                                style="height: 32px !important; width: 32px !important; margin-left: 5px;">
                              </el-button>
                            </el-col>
                            <el-col :span="24">
                              <span class="text-danger" style="color: red;">{{ item.Error }} </span>
                            </el-col>
                          </el-row>
                        </template>
                      </el-form-item>
                    </el-col>
                  </el-collapse-item>
                
                  <el-collapse-item :title="$t('RuleGetMaximumTimeOfLog')" name="c">
                    <el-col :span="24">
                      <el-col :span="8">
                        <el-form-item>
                          <label class="el-form-item__label left-label required">{{
                            $t("EarliestAttendanceRangeTime")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="16">
                        <el-form-item
                          prop="EarliestAttendanceRangeTime"
                        >
                          <el-time-picker
                            v-model="ruleForm.EarliestAttendanceRangeTime"
                            :placeholder="$t('SelectTime')"
                          >
                          </el-time-picker>
                        </el-form-item>
                      </el-col>
                    </el-col>
                    <el-col :span="24">
                    <el-col :span="8">
                      <el-form-item>
                        <label class="el-form-item__label left-label required">{{
                          $t("LatestAttendanceRangeTime")
                        }}</label>
                      </el-form-item>
                    </el-col>
                    <el-col :span="16">
                      <el-form-item
                        prop="LatestAttendanceRangeTime"
                      >
                        <el-time-picker
                          v-model="ruleForm.LatestAttendanceRangeTime"
                          :placeholder="$t('SelectTime')"
                        >
                        </el-time-picker>
                        <el-checkbox v-model="ruleForm.CheckOutOvernightTime" style="margin-left: 20px;">
                          {{ $t("CheckOutOvernightTime") }}
                        </el-checkbox>
                      </el-form-item>
                    </el-col>
                  </el-col>
                  </el-collapse-item>
                  
                  <el-collapse-item :title="$t('RuleMissingAttendanceLog')" name="d">
                    <el-col :span="24">
                      <el-col :span="4">
                        <el-checkbox v-model="ruleForm.AllowedDoNotAttendance" style="color: black; margin-bottom: 10px;">{{
                          $t("AllowedDoNotAttendance")
                        }}</el-checkbox>
                      </el-col>
                    </el-col>
                    <el-col :span="24" v-if="!ruleForm.AllowedDoNotAttendance">
                      <el-col :span="7">
                        <el-form-item>
                          <label class="el-form-item__label left-label">{{
                            $t("MissingCheckInAttendanceLogIs")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6">
                        <el-form-item>
                          <el-select v-model="ruleForm.MissingCheckInAttendanceLogIs" filterable>
                            <el-option
                              v-for="item in missingCheckInAttendanceLogIsOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                      </el-col>
                      <el-col :span="4" v-if="ruleForm.MissingCheckInAttendanceLogIs == 2">
                        <el-form-item>
                          <label class="el-form-item__label left-label right-label">{{
                            $t("MinutesNumberForLateIn")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6" v-if="ruleForm.MissingCheckInAttendanceLogIs == 2">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.LateCheckInMinutes"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="2000000000"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                    </el-col>
                    <el-col :span="24" v-if="!ruleForm.AllowedDoNotAttendance">
                      <el-col :span="7">
                        <el-form-item>
                          <label class="el-form-item__label left-label">{{
                            $t("MissingCheckOutAttendanceLogIs")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6">
                        <el-form-item>
                          <el-select v-model="ruleForm.MissingCheckOutAttendanceLogIs" filterable>
                            <el-option
                              v-for="item in missingCheckOutAttendanceLogIsOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                      </el-col>
                      <el-col :span="4" v-if="ruleForm.MissingCheckOutAttendanceLogIs == 2">
                        <el-form-item>
                          <label class="el-form-item__label left-label right-label">{{
                            $t("MinutesNumberForEarlyOut")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6" v-if="ruleForm.MissingCheckOutAttendanceLogIs == 2">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;" 
                            v-model="ruleForm.EarlyCheckOutMinutes"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="2000000000"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                    </el-col>
                  </el-collapse-item>
                  
                  <!-- <el-collapse-item :title="$t('RuleMaximumAnnualLeaveRegister')" name="e">
                    <el-col :span="24">
                      <el-col :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("ByMonth(day)")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.MaximumAnnualLeaveRegisterByMonth"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="31"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                      <el-col :offset="1" :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("ByYear(day)")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="6">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.MaximumAnnualLeaveRegisterByYear"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="366"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                    </el-col>
                  </el-collapse-item> -->
                  
                  <el-collapse-item :title="$t('RuleRoundAttendaceTime')" name="f">
                    <el-col :span="24">
                      <el-col :span="4">
                        <el-checkbox v-model="ruleForm.RoundingWorkedTime">{{
                          $t("RoundingWorkedTime")
                        }}</el-checkbox>
                      </el-col>
                    </el-col>
                    <el-col :span="24" v-if="ruleForm.RoundingWorkedTime" style="margin-top: 10px;">
                      <el-col :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("DecimalDigits")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.RoundingWorkedTimeNum"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="2000000000"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                      <el-col :offset="1" :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("RoundingMethod")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-select style="width: 100%;" v-model="ruleForm.RoundingWorkedTimeType" filterable>
                            <el-option
                              v-for="item in selectRoundOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                      </el-col>
                    </el-col>
                    <el-col :span="24">
                      <el-col :span="4">
                        <el-checkbox v-model="ruleForm.RoundingOTTime">{{
                          $t("RoundingOTTime")
                        }}</el-checkbox>
                      </el-col>
                    </el-col>
                    <el-col :span="24" v-if="ruleForm.RoundingOTTime" style="margin-top: 10px;">
                      <el-col :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("DecimalDigits")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.RoundingOTTimeNum"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="2000000000"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                      <el-col :offset="1" :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("RoundingMethod")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-select style="width: 100%;" v-model="ruleForm.RoundingOTTimeType" filterable>
                            <el-option
                              v-for="item in selectRoundOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                      </el-col>
                    </el-col>
                    <el-col :span="24">
                      <el-col :span="4">
                        <el-checkbox v-model="ruleForm.RoundingWorkedHour">{{
                          $t("RoundingWorkedHour")
                        }}</el-checkbox>
                      </el-col>
                    </el-col>
                    <el-col :span="24" v-if="ruleForm.RoundingWorkedHour" style="margin-top: 10px;">
                      <el-col :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("DecimalDigits")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-input-number
                            style="width: 100%;"
                            v-model="ruleForm.RoundingWorkedHourNum"
                            onkeypress='return event.charCode >= 48 && event.charCode <= 57'
                            :min="0"
                            :max="2000000000"
                          ></el-input-number>
                        </el-form-item>
                      </el-col>
                      <el-col :offset="1" :span="4">
                        <el-form-item>
                          <label class="el-form-item__label">{{
                            $t("RoundingMethod")
                          }}</label>
                        </el-form-item>
                      </el-col>
                      <el-col :span="7">
                        <el-form-item>
                          <el-select style="width: 100%;" v-model="ruleForm.RoundingWorkedHourType" filterable>
                            <el-option
                              v-for="item in selectRoundOption"
                              :key="item.value"
                              :label="$t(item.label)"
                              :value="item.value"
                            ></el-option>
                          </el-select>
                        </el-form-item>
                      </el-col>
                    </el-col>
                  </el-collapse-item>                
                </el-row>
              </el-form>
            </el-collapse>
          </el-col>
        </el-row>
      </el-main>
      <el-row style="margin-top: 5px;">
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
<script src="./ta-rule-by-shift.ts"></script>
  
<style lang="scss">
.ta-rules-shift-el-main {
  .customdialog {
    width: 1200px;
  }

  .ta-rule-by-shift__collapse {
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
          background-color: white;
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
}

.el-form-item__label {
  font-weight: 200;
}

.required:after {
	content: '*';
	color: red;
}

.ta-rule-shift-table{
  .current-row {
    td{
        background-color: #B9B9B9 !important;
    }
  }
}

.ignore-error.is-error {
  .el-row{
    .el-input__inner {
      border-color: #C0C4CC;
    }
  }
  .el-row.error-row{
    .el-input__inner {
      border-color: red;
    }
  }
}
</style>
  