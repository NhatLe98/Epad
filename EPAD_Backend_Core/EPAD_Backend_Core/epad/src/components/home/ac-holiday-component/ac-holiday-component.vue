<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListHoliday") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditHoliday') : $t('InsertHoliday')" custom-class="customdialog"
            :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top">
              <el-form-item :label="$t('HolidayName')" prop="HolidayName" @click.native="focus('Name')">
                <el-input ref="Name" v-model="ruleForm.HolidayName"></el-input>
              </el-form-item>
              <el-form-item :label="$t('WorkingFromDate')" prop="StartDate" @click.native="focus('StartDate')">
                <el-date-picker v-model="ruleForm.StartDate" type="date"></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('WorkingToDate')" prop="EndDate" @click.native="focus('EndDate')">
                <el-date-picker v-model="ruleForm.EndDate" type="date"></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('Door')" prop="DoorIndexes" @click.native="focus('DoorIndexes')">
                <el-select multiple filterable :placeholder="$t('SelectDoor')" v-model="ruleForm.DoorIndexes"
                  style="padding: 0 5px;">
                  <el-option v-for="item in allDoorLst" :key="item.value" :label="$t(item.label)"
                    :value="item.value"></el-option>
                </el-select>
              </el-form-item>
              <el-form-item :label="$t('Timezone')" prop="TimeZone" @click.native="focus('TimeZone')">
                <el-select filterable :placeholder="$t('SelectTimezone')" v-model="ruleForm.TimeZone"
                  style="padding: 0 5px;">

                  <el-option v-for="item in allTimezoneLst" :key="item.value" :label="$t(item.label)"
                    :value="item.value"></el-option>
                </el-select>
              </el-form-item>

              <el-form-item :label="$t('HolidayType')" prop="HolidayType" @click.native="focus('HolidayType')">
                <el-select filterable :placeholder="$t('SelectHolidayType')" v-model="ruleForm.HolidayType"
                  style="padding: 0 5px;">

                  <el-option v-for="item in listRange" :key="item.value" :label="$t(item.label)"
                    :value="item.value"></el-option>
                </el-select>
              </el-form-item>
              <el-form-item class="mt-3">
                    <el-checkbox v-model="ruleForm.Loop">{{ $t('LoopEveryYear')}}</el-checkbox>
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
          <data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" :showButtonIntegrate="true" @integrate="Integrate"
            v-bind:listExcelFunction="listExcelFunction" :showButtonColumConfig="true" :gridColumnConfig.sync="columns">
          </data-table-function-component>
          <data-table-component :get-data="getData" ref="table" :columns="columns" :selectedRows.sync="rowsObj"
            :isShowPageSize="true"></data-table-component>
            
        </div>

      </el-main>
    </el-container>
  </div>
</template>
<script src="./ac-holiday-component.ts"></script>