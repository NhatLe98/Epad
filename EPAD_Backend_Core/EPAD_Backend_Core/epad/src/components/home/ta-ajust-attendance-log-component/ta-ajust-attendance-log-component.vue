<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TAAjustAttendanceLog") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" @savebtn="Savebtn" :showButtonSave="showbtnSave"
          :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
          style="
            width: calc(100%) !important;
            position: relative;
            top: unset;
            left: unset;
            margin-bottom: 10px;
           padding-left: 10px;
            margin-right: 0 !important;
          ">
        </data-table-function-component>
        <div class="tab" style="height: calc(100vh - 185px); margin-top: 40px">
          <div>         
            <el-row>
              <el-col :span="4">
                <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="tree.multiple"
                  :placeholder="$t('SelectDepartment')"  :data="tree.treeData"
                  :props="tree.treeProps" :isSelectParent="true" :checkStrictly="tree.checkStrictly"
                  :clearable="tree.clearable" :popoverWidth="tree.popoverWidth" v-model="SelectedDepartment"  @change="onChangeDepartmentFilterSearch"
                  style="padding-right: 10px; width: 100%"></select-department-tree-component>
              </el-col>
              <el-col :span="4">
             
             <app-select-new
              class="ta-adjust-attendance-log__filter-employee"
               :dataSource="employeeFullLookup"
               displayMember="NameInFilter"
               valueMember="Index"
               :allowNull="true"
               v-model="EmployeeATIDsFilter"
               :multiple="true"
               style="width: 100%; padding-right: 10px"
               :placeholder="$t('SelectEmployee')"
               @getValueSelectedAll="selectAllEmployeeFilterGet"
               ref="employeeList"
             >
             </app-select-new>
         
         </el-col>
              <el-col :span="3" style="margin-left: 10px">
                <el-date-picker v-model="ruleObject.FromDate" type="date" :placeholder="$t('FromDateString')" format="dd/MM/yyyy"
                  style="width: 100%" :clearable="false" :editable="false">
                </el-date-picker>
              </el-col>
              <el-col :span="3" style="margin-left: 10px">
                <el-date-picker v-model="ruleObject.ToDate" type="date" :placeholder="$t('ToDateString')" format="dd/MM/yyyy"
                  style="width: 100%" :clearable="false" :editable="false">
                </el-date-picker>
              </el-col>
              <el-col :span="4" style="margin-left: 10px">
              <el-input
                style="padding-bottom: 3px; float: left; width: 238px"
                :placeholder="$t('InputMCCMNVName')"
                v-model="filter"
                class="filter-input"
                @keyup.enter.native="viewData"
              >
                <i
                  slot="suffix"
                  class="el-icon-search"
                  @click="viewData"
                ></i>
              </el-input>
            </el-col>
              <el-col :span="1" style="margin-left: 10px">
                <el-button type="primary" class="smallbutton" size="small" @click="viewData">
                  {{ $t("View") }}
                </el-button>
              </el-col>
              <el-col :span="2">
                <el-dropdown style="margin-top: 10px" @command="handleCommand" trigger="click">
                  <span class="el-dropdown-link" style="font-weight: bold">
                    . . .<span class="more-text">{{ $t("More") }}</span>
                  </span>
                  <el-dropdown-menu slot="dropdown">
                    <el-dropdown-item v-for="(item, index) in listExcelFunction" :key="index" :command="item">
                      {{ $t(item) }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </el-dropdown>
              </el-col>
            </el-row>
          </div>
          <div class="tab-modal">
            <el-dialog custom-class="customdialog" width="1000px" :title="isEdit
                ? $t('EditAjustAttendanceLog')
                : $t('InsertAjustAttendanceLog')
              " :visible.sync="showDialog" :close-on-click-modal="false" :before-close="onCancelClick">
              <el-form class="h-600" :model="ruleForm" :rules="rules" ref="ajustAttendanceLogRef" label-width="168px"
                label-position="top">
                <el-row>
                  <el-col :span="11">
                    <el-form-item :label="$t('Department')" prop="Department" :placeholder="$t('SelectDepartment')">
                      <!-- <select-department v-model="formModel.DepartmentIndex"
                                       :multiple="false"
                                       :disabled="isEdit"></select-department> -->
                      <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll" :multiple="true"
                        :placeholder="$t('SelectDepartment')" :disabled="isEdit" :data="tree.treeData"
                        :props="tree.treeProps" :isSelectParent="true" :clearable="tree.clearable"
                        :popoverWidth="tree.popoverWidth" v-model="ruleForm.DepartmentIDs"
                        @change="onChangeDepartmentFilter" style="width: 100%"
                        class="hr-employee-info__select-department-tree"></select-department-tree-component>
                    </el-form-item>
                  </el-col>
                  <el-col :span="11" :offset="1">
                    <el-form-item prop="EmployeeATID" :label="$t('Employee')">
                      <app-select-new :disabled="isEdit" :dataSource="employeeFullLookup" displayMember="NameInFilter"
                        valueMember="Index" :allowNull="true" v-model="ruleForm.EmployeeATID" style="width: 100%"
                        :placeholder="$t('SelectEmployee')" @getValueSelectedAll="getValueSelectedAll"
                        ref="employeeList">
                      </app-select-new>
                    </el-form-item>
                  </el-col>
                </el-row>
                <el-row>
                  <el-col :span="11">
                    <el-form-item :label="$t('Day')" @click.native="focus('AttendanceDate')" prop="AttendanceDate">
                      <el-date-picker format="dd/MM/yyyy" v-model="ruleForm.AttendanceDate" type="date"  :disabled="isEdit"
                        :placeholder="$t('Day')"></el-date-picker>
                    </el-form-item>
                  </el-col>
                  <el-col :span="11" :offset="1">
                    <el-form-item :label="$t('Hour')" @click.native="focus('AttendanceTime')" prop="AttendanceTime">
                      <el-time-picker v-model="ruleForm.AttendanceTime">
                      </el-time-picker>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-row>
                  <el-col :span="11">
                    <el-form-item :label="$t('AttendanceVerifyMode')" @click.native="focus('AttendanceVerifyMode')"
                      prop="AttendanceVerifyMode">
                      <el-select filterable :placeholder="$t('SelectAttendanceVerifyMode')"  :disabled="isEdit"
                        v-model="ruleForm.VerifyMode">
                        <el-option v-for="item in attendanceVerifyModeLst" :key="item.value" :label="$t(item.label)"
                          :value="item.value"></el-option>
                      </el-select>
                    </el-form-item>
                  </el-col>
                  <el-col :span="11" :offset="1">
                    <el-form-item :label="$t('InOutModeString')" @click.native="focus('InOutModeString')" 
                      prop="InOutModeString">
                      <el-select filterable :placeholder="$t('SelectInOutMode')" v-model="ruleForm.InOutMode"  :disabled="isEdit">
                        <el-option v-for="item in allInOutMode" :key="item.value" :label="$t(item.label)"
                          :value="item.value"></el-option>
                      </el-select>
                    </el-form-item>
                  </el-col>
                </el-row>
                <el-row>
                  <el-col :span="11">
                    <el-form-item :label="$t('Device')" @click.native="focus('SerialNumber')" prop="SerialNumber">
                      <el-select filterable :placeholder="$t('SelectDevice')" v-model="ruleForm.SerialNumber"  :disabled="isEdit">
                        <el-option v-for="item in deviceLst" :key="item.value" :label="$t(item.label)"
                          :value="item.value"></el-option>
                      </el-select>
                    </el-form-item>
                  </el-col>
                </el-row>
                <el-row>
                  <el-col :span="24">
                    <el-form-item prop="Note" :label="$t('Note')">
                      <el-input ref="Note" type="textarea" :autosize="{ minRows: 3, maxRows: 6 }"  :disabled="isEdit"
                        v-model="ruleForm.Note" class="InputArea"></el-input>
                    </el-form-item>
                  </el-col>

                </el-row>
              </el-form>

              <span slot="footer" class="dialog-footer">
                <el-button class="btnCancel" @click="onCancelClick">
                  {{ $t("Cancel") }}
                </el-button>
                <el-button class="btnOK" type="primary" @click="Submit">
                  {{ $t("OK") }}
                </el-button>
              </span>
            </el-dialog>
          </div>
          <div class="tab-grid">
            <VisualizeTable v-loading="isLoading" :columnDefs="columns.filter((x) => x.display)" :rowData="dataSource"
              :rowHeight="40" @onSelectionChange="onSelectionChange" :heightScale="260" :isKeepIndexAscending="true"
              :shouldResetColumnSortState="shouldResetColumnSortState" style="height: calc(100vh - 248px) !important;" />
            <AppPagination :page.sync="page" :pageSize.sync="pageSize" :getData="loadData" :total="total"  ref="taAjustAttendacenLogPagination" />
          </div>
        </div>

        <div>
          <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" :visible.sync="showDialogImportError"
            @close="showOrHideImportError(false)" :close-on-click-modal="false">
            <el-form label-width="168px" label-position="top">
              <el-form-item>
                <div class="box">
                  <label>
                    <span>{{ importErrorMessage }}</span>
                  </label>
                </div>
              </el-form-item>
              <el-form-item>
                <a class="fileTemplate-lbl" href="/Files/AjustAttendanceLogError.xlsx" download>{{ $t('Download') }}</a>
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                OK
              </el-button>
            </span>
          </el-dialog>
        </div>

        <div class="tab-modal-excel">
      <!--Dialog chosse excel-->
      <el-dialog
        :title=" $t('AddFromExcel') "
        custom-class="customdialog"
        :visible.sync="showDialogExcel"
        @close="AddOrDeleteFromExcel('close')"
      >
        <el-form
          :model="formExcel"
          ref="formExcel"
          label-width="168px"
          label-position="top"
        >
          <el-form-item :label="$t('SelectFile')">
            <div class="box">
              <input
                ref="fileInput"
                accept=".xls, .xlsx"
                type="file"
                name="file-3[]"
                id="fileUpload"
                class="inputfile inputfile-3"
                @change="processFile($event)"
              />
              <label for="fileUpload">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="20"
                  height="17"
                  viewBox="0 0 20 17"
                >
                  <path
                    d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z"
                  />
                </svg>
                <!-- <span>Choose a file&hellip;</span> -->
                <span>{{ $t("ChooseAExcelFile") }}</span>
              </label>
              <span v-if="fileName === ''" class="fileName">
                {{ $t("NoFileChoosen") }}
              </span>
              <span v-else class="fileName">{{ fileName }}</span>
            </div>
          </el-form-item>
          
          <div >
            
            <el-form-item :label="$t('DownloadTemplate')">
              <a
                class="fileTemplate-lbl"
                href="/Template_TimeLog.xlsx"
                download
                >{{ $t("Download") }}</a
              >
            </el-form-item>
          </div>
        
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button
            class="btnOK"
            type="primary"
            @click="UploadDataFromExcel"
          >
            {{ $t("OK") }}
          </el-button>
        
        </span>
      </el-dialog>
    </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ta-ajust-attendance-log-component.ts"></script>
<style lang="scss">
.hozitalClass {
  display: flex;
  flex-direction: row;
}

.hozitalClass:after {
  content: "";
  flex: 1 1;
  border-bottom: 1px solid #000;
  margin: auto;
}

.ta-adjust-attendance-log__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
  top: 50% !important;
  max-width: 100% !important;

  span span:first-child {
    width: max-content;
    max-width: calc(100% - 20%);
    margin-left: 1px;

    .el-select__tags-text {
      vertical-align: top;
      width: max-content;
      max-width: 90%;
      display: inline-block;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }
}

.ta-adjust-attendance-log__filter-employee .el-select__tags:has(span span:nth-child(2)) {
  top: 50% !important;
  max-width: 80% !important;

  span span:first-child {
    width: max-content;
    max-width: calc(100% - 30%);
    margin-left: 1px;

    .el-select__tags-text {
      vertical-align: top;
      width: max-content;
      max-width: 80%;
      display: inline-block;
      text-overflow: ellipsis;
      overflow: hidden;
    }
  }

  span span:nth-child(2) {
    max-width: 30%;
    margin-left: 1px;

    .el-select__tags-text {
      width: 100%;
      max-width: 100% !important;
    }
  }
}
</style>