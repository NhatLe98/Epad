<template >
  <div style="overflow: hidden;">
    <el-container id="bgHome">
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("InOutTimeRules") }}</span>
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
                property="FromDateString"
                :label="$t('RulesList')"
              ></el-table-column>
            </el-table>
          </el-col>

          <el-col class="colDetail" :span="18">
            <div class="in-out-time-rule__divScrollCollapse" v-if="ruleObject != null">
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
                      prop="FromDate"
                      :label="$t('AppliedDate')"
                      style="margin-top: 4px"
                    >
                      <el-date-picker
                        :placeholder="$t('SelectTime')"
                          v-model="ruleObject.FromDate"
                          style="width: 50%"
                          type="date"
                          format="dd/MM/yyyy"
                        >
                      </el-date-picker>
                    </el-form-item>
                    <el-form-item :label="$t('Description')">
                      <el-input v-model="ruleObject.Description" style="width: 50%"></el-input>
                    </el-form-item>
                  </el-collapse-item>
                  <el-collapse-item
                    :title="$t('RuleAccessTimeInOut')"
                    name="b"
                  >
                    <!-- vào -->
                    <el-form-item :label="$t('CheckInTime')" prop="CheckInTime">
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
                        :max="2000"
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
                        :max="2000"
                        :min="0"
                      ></el-input-number>
                    </el-form-item>
                    <!-- ra -->
                    <el-form-item :label="$t('CheckOutTime')"  prop="CheckOutTime">
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
                        :max="2000"
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
                        :max="2000"
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

<script src="./in-out-time-rules-component.ts" />
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
.in-out-time-rule__divScrollCollapse {
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

</style>
