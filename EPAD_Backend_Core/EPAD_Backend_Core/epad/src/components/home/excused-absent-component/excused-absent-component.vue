<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListExcusedAbsent") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog :title="isEdit ? $t('EditExcusedAbsent') : $t('InsertExcusedAbsent')" :visible.sync="showDialog"
            custom-class="customdialog" :before-close="Cancel" :close-on-click-modal="false">
            <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top"
              @keyup.enter.native="Submit">
              <el-form-item :label="$t('Class')">
                <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="true"
                                    :placeholder="$t('SelectClass')"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :isSelectParent="true"
                                    :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    @change="onChangeDepartmentFilter"
                                    v-model="formDepartment"
                                    style="width: 100%"
                                    :disabled="isEdit"
                                    ></select-department-tree-component>
              </el-form-item>
              <el-form-item prop="EmployeeATIDs" :label="$t('Student')" style="margin-top:32px">
                <app-select-new :disabled="isEdit" :dataSource="employeeFullLookup" displayMember="NameInFilter" valueMember="Index"
                :allowNull="true" v-model="ruleForm.EmployeeATIDs" :multiple="true" style="width: 100%"
                :placeholder="$t('SelectStudent')" @getValueSelectedAll="selectAllEmployeeFilter" ref="employeeList">
                </app-select-new>
              </el-form-item>
              <el-form-item prop="AbsentDate" :label="$t('Day')">
                <el-date-picker
                :placeholder="$t('Day')"
                  v-model="ruleForm.AbsentDate"
                  :disabled="isEdit"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item prop="ExcusedAbsentReasonIndex"  :label="$t('Reason')">
                <el-select ref="ExcusedAbsentReasonIndex" :placeholder="$t('SelectReason')" v-model="ruleForm.ExcusedAbsentReasonIndex"
                  clearable filterable reserve-keyword>
                  <el-option v-for="item in listExcusedAbsentReason" :key="item.Index" :label="$t(item.Name)"
                    :value="item.Index"></el-option>
                </el-select>
              </el-form-item>
              <el-form-item prop="Description"  :label="$t('Description')">
                <el-input ref="Description" type="textarea" :autosize="{ minRows: 3, maxRows: 6 }"
                  v-model="ruleForm.Description" class="InputArea"></el-input>
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{
                    $t("Cancel")
                }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="Submit">{{ $t("OK") }}</el-button>
            </span>
          </el-dialog>

        </div>
        <div>
          <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" :visible.sync="showDialogImportError"
            @close="showOrHideImportError(false)">
            <el-form label-width="168px" label-position="top">
              <el-form-item>
                <div class="box">
                  <label>
                    <span>{{ importErrorMessage }}</span>
                  </label>
                </div>
              </el-form-item>
              <el-form-item>
                <a class="fileTemplate-lbl" href="/Files/Template_NoAttendance_Error.xlsx" download>{{ $t('Download') }}</a>
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                OK
              </el-button>
            </span>
          </el-dialog>
        </div>
        <div>
          <el-row class="excused-absent__custom-function-bar">
            <el-col :span="3" style="margin-right: 10px;">
              <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="tree.multiple"
                                    :placeholder="$t('SelectClass')"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :isSelectParent="true"
                                    :checkStrictly="tree.checkStrictly"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    v-model="selectDepartment"
                                    style="width: 100%"
                                    ></select-department-tree-component>
              </el-col>
              <el-col :span="3" style="margin-right: 10px;">
              <el-date-picker :placeholder="$t('FromDateString')" v-model="fromDate"
                                              type="date"
                                              id="inputFromDate"></el-date-picker>
              </el-col>
              <el-col :span="3" style="margin-right: 10px;">
              <el-date-picker :placeholder="$t('ToDateString')" v-model="toDate"
                                              type="date"
                                              id="inputToDate"></el-date-picker>
              </el-col>
              <el-col :span="1">
              <el-button type="primary"
                                          size="small"
                                          class="smallbutton"
                                          @click="searchData">{{ $t("View") }}</el-button>
              </el-col>
              <el-col :span="2">
                <el-dropdown
                        style="margin-left: 10px;"
                        @command="handleCommand"
                        trigger="click"
                      >
                        <span
                          class="el-dropdown-link"
                          style="font-weight: bold"
                        >
                          . . .<span class="more-text">{{ $t("More") }}</span>
                        </span>

                        <el-dropdown-menu slot="dropdown">
                          <el-dropdown-item
                            v-for="(item, index) in listExcelFunction"
                            :key="index"
                            :command="item"
                          >
                            {{ $t(item) }}
                          </el-dropdown-item>
                        </el-dropdown-menu>
                      </el-dropdown>
              </el-col>
          </el-row>                        
        </div>
        <div>
          <data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" 
          class="excused-absent__data-table-function" :showButtonColumConfig="true" :gridColumnConfig.sync="columns">
          </data-table-function-component>

          <data-table-component :get-data="getData" ref="table" :selectedRows.sync="rowsObj" :columns="columns"
            class="excused-absent__data-table"
            :isShowPageSize="true"></data-table-component>

        </div>

        <div>
          <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')" custom-class="customdialog"
            :visible.sync="showDialogExcel" @close="AddOrDeleteFromExcel('close')">
            <el-form :model="formExcel" ref="formExcel" label-width="168px" label-position="top">
              <el-form-item :label="$t('SelectFile')">
                <div class="box">
                  <input ref="fileInput"
                    accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
                    type="file" name="file-3[]" id="fileUpload" class="inputfile inputfile-3"
                    @change="processFile($event)" />
                  <label for="fileUpload">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="17" viewBox="0 0 20 17">
                      <path
                        d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z" />
                    </svg>
                    <!-- <span>Choose a file&hellip;</span> -->
                    <span>{{ $t('ChooseAExcelFile') }}</span>
                  </label>
                  <span v-if="fileName === ''" class="fileName">{{ $t('NoFileChoosen') }}</span>
                  <span v-else class="fileName">{{ fileName }}</span>

                </div>
              </el-form-item>
              <el-form-item :label="$t('DownloadTemplate')">
                <a class="fileTemplate-lbl" href="/Template_NoAttendance.xlsx" download>{{ $t('Download') }}</a>
              </el-form-item>
          
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
                {{ $t('Cancel') }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="UploadDataFromExcel">
                {{ $t('OK') }}
              </el-button>
            </span>
          </el-dialog>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./excused-absent-component.ts"></script>
<style lang="scss">
.excused-absent__data-table {
  .filter-input {
    margin-right: 10px;
  }
}

.excused-absent__data-table {
  .el-table {
    height: calc(100vh - 224px) !important;
  }
  .el-dropdown {
    height: 32px !important;
    .el-dropdown-link {
      line-height: 32px;
    }
  }
}
</style>
