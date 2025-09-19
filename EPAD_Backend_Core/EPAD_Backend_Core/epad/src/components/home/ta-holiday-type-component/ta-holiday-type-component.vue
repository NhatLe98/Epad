<template>
    <div id="bgHome">
      <el-container>
        <el-header>
          <el-row>
            <el-col :span="12" class="left">
              <span id="FormName">{{ $t("TAHolidayType") }}</span>
            </el-col>
            <el-col :span="12">
              <HeaderComponent />
            </el-col>
          </el-row>
        </el-header>
        <el-main class="bgHome">
          <div>
            <el-dialog :title="isEdit ? $t('EditHolidayType') : $t('InsertHolidayType')" 
            custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
              <el-form
                :model="ruleForm"
                :rules="rules"
                ref="ruleForm"
                label-width="168px"
                label-position="top"
              >
                <el-form-item :label="$t('HolidayTypeCode')" prop="Code" @click.native="focus('Code')">
                  <el-input ref="Code" v-model="ruleForm.Code"></el-input>
                </el-form-item>
                <el-form-item :label="$t('HolidayNameType')" prop="Name" @click.native="focus('Name')">
                  <el-input ref="Name" v-model="ruleForm.Name"></el-input>
                </el-form-item>
                <el-form-item :label="$t('DateHoliday')" prop="HolidayDate" @click.native="focus('HolidayDate')">
                  <el-date-picker v-model="ruleForm.HolidayDate" type="date"></el-date-picker>
                </el-form-item>
                <el-form-item >
                    <el-checkbox v-model="ruleForm.IsPaidWhenNotWorking">{{
                                            $t("CalculateWhenNotWork")
                                        }}</el-checkbox>
                </el-form-item>
                <el-form-item >
                    <el-checkbox v-model="ruleForm.IsRepeatAnnually">{{
                                            $t("LoopEveryYear")
                                        }}</el-checkbox>
                </el-form-item>
              </el-form>
              <span slot="footer" class="dialog-footer">
                <el-button class="btnCancel" @click="Cancel">
                  {{
                  $t("Cancel")
                  }}
                </el-button>
                <el-button class="btnOK" type="primary" @click="Submit('ruleForm')">{{ $t("OK") }}</el-button>
              </span>
            </el-dialog>
          </div>
          <div>
            <data-table-function-component
            @insert="Insert" @edit="Edit" @delete="Delete"
            v-bind:listExcelFunction="listExcelFunction"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
            >
            </data-table-function-component>
            <data-table-component
              :get-data="getData"
              ref="table"
              :columns="columns"
              :selectedRows.sync="rowsObj"
              :isShowPageSize="true"
              :filterPlaceHolder="$t('InputCodeAndName')"
            ></data-table-component>
          </div>
         
        </el-main>
      </el-container>
    </div>
  </template>
  <script src="./ta-holiday-type-component.ts"></script>