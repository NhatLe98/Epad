<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("GeneralRules") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>

      <el-main class="bgHome">
        <el-form
          ref="formref"
          :model="ruleGeneral"
          :rules="rules"
          label-width="230px"
          label-position="left"
        >
          <el-row class="left_list">
         
            <!-- v-bind:class="checkClass(col.Index)" -->
            <el-col class="colList" :span="6"  style="height: 800px; overflow: hidden;">
              <el-table
                ref="tblRules"
                :data="listData"
                @current-change="changeFormData"
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
            <el-col class="colDetail" span="15" style="margin-left:35px;">
              <h3>
                {{ $t("BasicInfo") }}
                <span class="text-danger">{{ groupError }}</span>
              </h3>
              <el-form-item
                prop="Name"
                :label="$t('RuleName')"
                style="margin-top: 4px"
              >
                <el-input v-model="ruleGeneral.Name" placeholder></el-input>
              </el-form-item>
              <el-form-item :label="$t('EnglishName')">
                <el-input v-model="ruleGeneral.NameInEng"></el-input>
              </el-form-item>
              <el-form-item :label="$t('AppliedDate')" prop="FromDate">
                <el-date-picker
                  :placeholder="$t('AppliedDate')"
                  v-model="ruleGeneral.FromDate"
                  style="width: 100%"
                  type="date"
                  :default-value="defaultValue"
                >
                </el-date-picker>
              </el-form-item>
          
              <!-- Rule check overtime -->
              <el-form-item :label="$t('StartTimeDay')">
                <el-time-picker
                  :placeholder="$t('SelectTime')"
                  v-model="ruleGeneral.StartTimeDay"
                  style="width: 50%"
                  format="HH:mm"
                ></el-time-picker>
              </el-form-item>
              <el-form-item
                :label="$t('MaxAttendanceTime')"
                style="margin-top: 4px"
              >
                <el-input-number
                  v-model="ruleGeneral.MaxAttendanceTime"
                  style="width: 50%"
                  :min="0"
                ></el-input-number>
              </el-form-item>
              <el-form-item
                :label="$t('PresenceTrackingTime')"
                style="margin-top: 4px"
              >
                <el-input-number
                  v-model="ruleGeneral.PresenceTrackingTime"
                  style="width: 50%"
                  :min="0"
                ></el-input-number>
              </el-form-item>
              <el-form-item :label="$t('IsBypassRule')">
                <el-switch v-model="ruleGeneral.IsBypassRule"></el-switch>
              </el-form-item>
              <el-form-item :label="$t('IsUsingGeneralRule')">
                <el-switch v-model="ruleGeneral.IsUsing"></el-switch>
              </el-form-item>
              
              <el-form-item :label="$t('RunWithoutScreen')">
                <el-switch v-model="ruleGeneral.RunWithoutScreen "></el-switch>
              </el-form-item>
              <el-form-item :label="$t('IgnoreInLog')">
                <el-switch v-model="ruleGeneral.IgnoreInLog"></el-switch>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-main>
      <el-row>
        <el-col :span="7" align="right" style="margin-bottom:10px">
          <el-button 
          type="primary"
          :disabled="this.activeMenuIndex < 1" 
          @click="del()"
          style="margin-right: 5px; background-color: red !important;">
          <i class="el-icon-delete"></i> {{$t("Delete") }}
        </el-button>
        <el-button 
          type="primary" 
          @click="add()">
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
          <el-button type="primary" @click="submit()" 
          style="margin-right: 5px; background-color: #67C23A !important;">
          <i class="el-icon-circle-check"></i> {{ $t("Save") }}</el-button>
        </el-col>
      </el-row>
    </el-container>
  </div>
</template>
<script src="./general-rules-component.ts"></script>
<style lang="scss">
.formScroll {
  height: 55vh;
  overflow-y: auto;
}

.el-dialog {
  margin-top: 20px !important;
}
.colDetail .el-form-item__label{
  font-weight: 100
}
</style>