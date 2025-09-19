<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("ListAccessedGroup") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent :showMasterEmployeeFilter="true"/>
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog
            :title="
              isEdit ? $t('EditAccessedGroup') : $t('InsertAccessedGroup')
            "
            custom-class="customdialog"
            :visible.sync="showDialog"
            :before-close="Cancel"
            :close-on-click-modal="false"
          >
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
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
                  v-model="ruleForm.DepartmentIDs"
                  @change="onChangeDepartmentFilter"
                  style="width: 100%"
                  class="hr-employee-info__select-department-tree"
                ></select-department-tree-component>

              </el-form-item>
              <el-form-item
                prop="EmployeeATIDs"
                :label="$t('Employee')"
                style="margin-top: 32px"
              >
                <app-select-new
                  :disabled="isEdit"
                  :dataSource="employeeFullLookup"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="EmployeeATIDs"
                  :multiple="true"
                  style="width: 100%"
                  :placeholder="$t('SelectEmployee')"
                  @getValueSelectedAll="selectAllEmployeeFilter"
                  ref="employeeList"
                >
                </app-select-new>
              </el-form-item>
              <el-form-item prop="GroupIndex" :label="$t('AccessedGroup')">
                <el-select
                  style="width: 100%"
                  v-model="ruleForm.GroupIndex"
                  filterable
                  :clearable="true"
                  :placeholder="$t('AccessedGroup')"
                >
                  <el-option
                    v-for="item in listGroups"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </el-form-item>
              <el-form-item>
                <el-checkbox v-model="syncUser">{{
                  $t("UploadUsersToMachine")
                }}</el-checkbox>
              </el-form-item>
              <el-form-item v-if="syncUser">
                
                <el-select
                  props="selectAuthenMode"
                  v-model="selectAuthenMode"
                  clearable
                  multiple
                  :placeholder="$t('SelectAuthenMode')"
                >
                  <el-option
                    :key="allAuthenModes"
                    :label="$t(allAuthenModes)"
                    :value="allAuthenModes"
                  ></el-option>
                  <el-option
                    v-for="item in listAuthenMode"
                    :key="item.value"
                    :label="$t(item.label)"
                    :value="item.value"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{ $t("Cancel") }}
              </el-button>
              <el-button
                class="btnOK"
                type="primary"
                style="width: 60%;"
                @click="Submit('ruleForm')"
                >{{ $t("AddTimezoneToMachine") }}</el-button
              >
            </span>
          </el-dialog>
        </div>
        <div>
          <!-- <el-select style="float: left; margin-right: 5px;"></el-select> -->
          <data-table-function-component
              @insert="Insert"
              @edit="Edit"
              @delete="Delete"
              style="
                width: calc(100%) !important;
                position: relative;
                top: unset; left: unset;
                margin-bottom: 10px;
                margin-left: 10px;
                margin-right: 0 !important;
              "
              :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
            >
            </data-table-function-component>
          <el-row>
            <el-col :span="4">
              <select-department-tree-component
                :defaultExpandAll="tree.defaultExpandAll"
                :multiple="tree.multiple"
                :placeholder="$t('SelectDepartment')"
                :disabled="tree.isEdit"
                :data="tree.treeData"
                :props="tree.treeProps"
                :isSelectParent="true"
                :checkStrictly="tree.checkStrictly"
                :clearable="tree.clearable"
                :popoverWidth="tree.popoverWidth"
                v-model="SelectedDepartment"
                style="padding-right: 10px; width: 100%"
              ></select-department-tree-component>
            </el-col>
            <el-col :span="4">
              <app-select-new
                :dataSource="listGroups"
                displayMember="label"
                valueMember="value"
                :allowNull="true"
                v-model="filterGroups"
                :multiple="true"
                style="width: 100%; padding-right: 10px;"
                :placeholder="$t('SelectGroup')"
                @getValueSelectedAll="selectAllGroups"
                ref="parkingLotFilter"
              >
              </app-select-new>
            </el-col>

            <el-col :span="4">
              <el-input
                style="
                  padding-bottom: 3px;
                  width: calc(100% - 10px);"
                :placeholder="$t('SearchData')"
                v-model="filter"
                @keyup.enter.native="Search()"
                class="filter-input"
              >
                <i slot="suffix" class="el-icon-search"></i>
              </el-input>
            </el-col>
            <el-col :span="1">
              <el-button
                type="primary"
                class="smallbutton"
                size="small"
                @click="Search"
              >
                {{ $t("View") }}
              </el-button>
            </el-col>
            <el-col :span="2">
              <el-dropdown style=" margin-top: 10px"
                @command="handleCommand"
                trigger="click">
                <span class="el-dropdown-link" style="font-weight: bold">
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

          <data-table-component class="ac-accessed-group__data-table"
            :get-data="getData"
            :showSearch="false"
            ref="table"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :isShowPageSize="true"
          ></data-table-component>
          
        </div>
        <div>
          <el-dialog :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')" custom-class="customdialog"
            :visible.sync="showDialogExcel" @close="AddOrDeleteFromExcel('close')" :close-on-click-modal="false">
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
                <a class="fileTemplate-lbl" href="/Template_AC_EmployeeAccessedGroup.xlsx" download>{{ $t('Download') }}</a>
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
                <a class="fileTemplate-lbl" href="/Files/ACAccessedEmployeeError.xlsx" download>{{ $t('Download') }}</a>
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" type="primary" @click="showOrHideImportError(false)">
                OK
              </el-button>
            </span>
          </el-dialog>
        </div>
        <div class="tab-modal-delete">
      <el-dialog
        custom-class="customdialog"
        :title="$t('DialogOption')"
        :visible.sync="showDialogDeleteUser"
        :before-close="cancelDeleteUser"
        :close-on-click-modal="false"
      >
        <el-form label-width="168px" label-position="top">
          <div style="margin-bottom: 20px">
            <i
              style="font-weight: bold; font-size: larger; color: orange"
              class="el-icon-warning-outline"
            />
            <span style="font-weight: bold">
              {{ $t("DeleteEmployeeCofirm") }}
            </span>
          </div>
          <el-form-item>
            <el-checkbox v-model="isDeleteAccessToMachine">
              {{ $t("DeleteAccessToMachine") }}
            </el-checkbox>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button class="btnCancel" @click="showDialogDeleteUser = false">
            {{ $t("Cancel") }}
          </el-button>
          <el-button type="primary" @click="doDelete">
            {{ $t("OK") }}
          </el-button>
        </span>
      </el-dialog>
    </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./ac-accessed-group-component.ts"></script>
<style lang="scss">
.br {
  display: block; /* makes it have a width */
            content: ""; /* clears default height */
            margin-top: 0; /* change this to whatever height you want it */
}
.el-message-box__message{
  max-height: 350px;
    overflow-y: scroll;
}
.ac-accessed-group__data-table {
  .el-table {
    height: calc(100vh - 255px) !important;
  }
}
</style>
