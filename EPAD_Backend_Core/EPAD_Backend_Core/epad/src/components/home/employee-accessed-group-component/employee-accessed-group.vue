<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("EmployeeAccessedGroup") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-row class="approve-change-department__custom-function-bar">
              <el-col :span="5">
                  <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                      :multiple="tree.multiple" :placeholder="$t('SelectDepartment')" :disabled="tree.isEdit"
                      :data="tree.treeData" :props="tree.treeProps" :isSelectParent="true"
                      :checkStrictly="tree.checkStrictly" :clearable="tree.clearable"
                      :popoverWidth="tree.popoverWidth" v-model="filterDepartment" style="margin-right: 10px"
                      @change="onChangeDepartmentFilter"></select-department-tree-component>
                  <!-- <select-department v-else v-model="filterModel.SelectedDepartment"
                              clearable
                              multiple
                              collapse-tags
                              style="padding: 0 5px; width: 100%"></select-department> -->
              </el-col>
              <el-col :span="5">
                <app-select-new class="employee-accessed-group__filter-employee" :dataSource="employeeFullLookupFilter" displayMember="NameInFilter"
                    valueMember="Index" :allowNull="true" v-model="filterModel.ListEmployeeATID"
                    :multiple="true" style="margin-right: 10px; width: calc(100% - 10px)"
                    :placeholder="$t('SelectEmployee')" @getValueSelectedAll="selectAllEmployeeFilter"
                    ref="employeeList">
                </app-select-new>
                <!-- <select-department v-else v-model="filterModel.SelectedDepartment"
                            clearable
                            multiple
                            collapse-tags
                            style="padding: 0 5px; width: 100%"></select-department> -->
            </el-col>
            <el-col :span="3">
                <el-date-picker style="margin-right: 10px" v-model="filterModel.FromDate"
                    format="dd/MM/yyyy" type="date" :placeholder="$t('WorkingFromDate')">
                </el-date-picker>
            </el-col>
            <el-col :span="3">
                <el-date-picker style="margin-right: 10px" v-model="filterModel.ToDate" format="dd/MM/yyyy"
                    type="date" :placeholder="$t('WorkingToDate')">
                </el-date-picker>
            </el-col>
            <el-col :span="4">
              <el-button type="primary" class="smallbutton" size="small"
                  @click="onViewClick">
                  {{ $t("View") }}
              </el-button>
            </el-col>
          </el-row>

          <data-table-function-component class="employee-accessed-group__data-table-function"
            :showButtonColumConfig="true"
            :gridColumnConfig.sync="columns"
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            v-bind:listExcelFunction="listExcelFunction"
            @add-excel="AddOrDeleteFromExcel('add')"
            @auto-select-excel="AutoSelectFromExcel('open')"
          >
          </data-table-function-component>
      
          <data-table-component class="employee-accessed-group__data-table"
            :get-data="getData"
            ref="employeeAccessedGroupTable"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :isShowPageSize="true"
          ></data-table-component>
          
        </div>
        <!-- dialog insert -->
        <div>
          <el-dialog
            :title="isEdit ? $t('Edit') : $t('Insert')"
            style="margin-top: 20px !important"
            custom-class="customdialog"
            :visible.sync="showDialog"
            :before-close="Cancel"
            :close-on-click-modal="false"
          >
            <el-form
              class="formScroll"
              :model="accessedGroupModel"
              :rules="rules"
              ref="accessedGroupModel"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="Submit"
            >
            <el-form-item
            :label="$t('Department')"
            prop="DepartmentIndex"
            :placeholder="$t('SelectDepartment')"
          >
            <!-- <select-department v-model="formModel.DepartmentIndex"
                                 :multiple="false"
                                 :disabled="isEdit"></select-department> -->
            <select-department-tree-component
              :defaultExpandAll="tree.defaultExpandAll"
              :multiple="true"
              :placeholder="$t('SelectDepartment')"
              :disabled="isEdit"
              :data="tree.treeData"
              :props="tree.treeProps"
              :isSelectParent="true"
              :clearable="tree.clearable"
              :popoverWidth="tree.popoverWidth"
              v-model="accessedGroupModel.DepartmentIDs"
              @change="onChangeDepartmentForm"
              style="width: 100%"
              class="hr-employee-info__select-department-tree"
            ></select-department-tree-component>

          </el-form-item>
              <el-form-item prop="EmployeeATIDs" :label="$t('Employee')" style="margin-top:32px">
                <app-select-new :disabled="isEdit" :dataSource="employeeFullLookup" 
                displayMember="NameInFilter" valueMember="Index"
                :allowNull="true" v-model="accessedGroupModel.EmployeeATIDs" :multiple="true" style="width: 100%"
                :placeholder="$t('SelectEmployee')" @getValueSelectedAll="selectAllEmployeeFilter" ref="employeeFormList">
                </app-select-new>
              </el-form-item>

              <el-form-item :label="$t('FromTime')" prop="FromDate">
                <el-date-picker
                  :disabled="isEdit"
                  :clearable="false"
                  ref="FromDate"
                  value-type="format"
                  v-model="accessedGroupModel.FromDate"
                  type="date"
                  id="inputFromDate"
                ></el-date-picker>
              </el-form-item>
              <el-form-item :label="$t('ToTime')" prop="ToDate">
                <el-date-picker
                  ref="ToTime"
                  v-model="accessedGroupModel.ToDate"
                  type="date"
                ></el-date-picker>
              </el-form-item>
              <el-form-item prop="AccessedGroupIndex" :label="$t('AccessedGroup')">
                <el-select
                  style="width: 100%"
                  v-model="accessedGroupModel.AccessedGroupIndex"
                  filterable
                  :clearable="true"
                  :placeholder="$t('AccessedGroup')"
                >
                  <el-option
                    v-for="item in accessedGroupList"
                    :key="item.Index"
                    :label="item.Name"
                    :value="item.Index"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{ $t("Cancel") }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="ConfirmClick">
                {{ $t("OK") }}
              </el-button>
            </span>
          </el-dialog>
        </div>
      </el-main>
    </el-container>
    <div>
      <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')" custom-class="customdialog"
          :visible="showDialogExcel && !isChoose" @close="AddOrDeleteFromExcel('close')">
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
                          <span>{{ $t("ChooseAExcelFile") }}</span>
                      </label>
                      <span v-if="fileName === ''" class="fileName">{{
                          $t("NoFileChoosen")
                      }}</span>
                      <span v-else class="fileName">{{ fileName }}</span>
                  </div>
              </el-form-item>
              <el-form-item :label="$t('DownloadTemplate')">
                  <a class="fileTemplate-lbl" href="/Template_EmployeeAccessedGroup.xlsx" download>{{ $t("Download") }}</a>
              </el-form-item>
          </el-form>

          <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="AddOrDeleteFromExcel('close')">
                  {{ $t("Cancel") }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="UploadDataFromExcel">
                  {{ $t("OK") }}
              </el-button>
          </span>
      </el-dialog>

      <el-dialog
        :title="$t('AutoSelectExcel')"
        custom-class="customdialog"
        :visible.sync="showDialogAutoSelectExcel"
        :close-on-click-modal="false"
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
                id="fileAutoSelect"
                class="inputfile inputfile-3"
                @change="processAutoSelectFile($event)"
              />
              <label for="fileAutoSelect">
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
                <span>{{ $t("ChooseAExcelFile") }}</span>
              </label>
              <span v-if="fileName === ''" class="fileName">
                {{ $t("NoFileChoosen") }}
              </span>
              <span v-else class="fileName">{{ fileName }}</span>
            </div>
          </el-form-item>
          <el-form-item :label="$t('DownloadTemplate')">
            <a
              class="fileTemplate-lbl"
              href="/Template_ChooseEmployeeSync.xlsx"
              download
              >{{ $t("Download") }}</a
            >
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="AutoSelectFromExcel('close')">
            {{ $t("Cancel") }}
          </el-button>
          <el-button class="btnOK" type="primary" @click="ProcessAutoSelectFromExcel">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
  </div>

  <div>
      <el-dialog :title="$t('DialogHeaderTitle')" custom-class="customdialog" 
      :visible.sync="showDialogImportError" :close-on-click-modal="false"
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
                  <a class="fileTemplate-lbl" href="/Files/Template_EmployeeAccessedGroup_Error.xlsx" download>{{ $t('Download')
                  }}</a>
              </el-form-item>
          </el-form>

          <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                  OK
              </el-button>
          </span>
      </el-dialog>
  </div>
  </div>
</template>
<script src="./employee-accessed-group.ts"></script>
<style lang="scss">
.el-dialog {
  margin-top: 20px !important;
}

.employee-accessed-group__data-table {
  .filter-input {
      margin-right: 10px;
  }
  .el-table {
    margin-top: 10px;
    height: calc(100vh - 231px) !important;
  }
  .employee-accessed-group__data-table-function {
      .datatable-function{
          .group-btn{
              button{
                  height: 35px !important;
              }
          }
      }
  }
  .approve-change-department__custom-function-bar {
    width: 100%;
    display: flex;
    padding-top: 10px;
  }
}

.employee-accessed-group__filter-employee
  .el-select__tags:has(span span:nth-child(2)) {
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