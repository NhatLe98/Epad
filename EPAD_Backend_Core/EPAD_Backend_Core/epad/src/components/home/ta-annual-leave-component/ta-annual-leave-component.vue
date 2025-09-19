<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("TAAnnualLeave") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog
            :title="isEdit ? $t('EditAnnualLeave') : $t('InsertAnnualLeave')"
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
                  :disabled="isEdit"
                  :placeholder="$t('SelectDepartment')"
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
                  :dataSource="employeeFullLookupFilter"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  :disabled="isEdit"
                  v-model="ruleForm.EmployeeATIDs"
                  :multiple="true"
                  style="width: 100%"
                  :placeholder="$t('SelectEmployee')"
                  @getValueSelectedAll="selectAllEmployeeFilter"
                  ref="employeeList"
                >
                </app-select-new>
              </el-form-item>
              <el-form-item
                :label="$t('AnnualLeave')"
                prop="AnnualLeave"
                @click.native="focus('AnnualLeave')"
              >
               
                <el-form-item
                      prop="TheoryWorkedTimeByShift"
                      @click.native="focus('TheoryWorkedTimeByShift')"
                    >
                      <el-input-number id="theory-work-time-shift__input-number"
                      
                        style="width: 100%;"
                        ref="AnnualLeave"
                        v-model="ruleForm.AnnualLeave"
                        onkeypress='return (event.charCode >= 48 && event.charCode <= 57) || event.charCode == 46'
                        :min="0"
                        :max="2000000000"
                        styled="height: 50px"
                      ></el-input-number>
                    </el-form-item>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{ $t("Cancel") }}
              </el-button>
              <el-button
                class="btnOK"
                type="primary"
                @click="Submit('ruleForm')"
                >{{ $t("OK") }}</el-button
              >
            </span>
          </el-dialog>
        </div>
        <div>
          <!-- <data-table-function-component
            @insert="Insert" @edit="Edit" @delete="Delete"
            v-bind:listExcelFunction="listExcelFunction"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
            >
            </data-table-function-component> -->

          <data-table-function-component
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            style="
              width: calc(100%) !important;
              position: relative;
              top: unset;
              left: unset;
              margin-bottom: 10px;
              margin-left: 10px;
              margin-right: 0 !important;
            "
            :showButtonColumConfig="true"
            :gridColumnConfig.sync="columns"
          >
          </data-table-function-component>

          <el-row>
            <el-col :span="4">
              <select-department-tree-component
                :defaultExpandAll="tree.defaultExpandAll"
                :multiple="tree.multiple"
                :placeholder="$t('SelectDepartment')"
                :data="tree.treeData"
                :props="tree.treeProps"
                :isSelectParent="true"
                :checkStrictly="tree.checkStrictly"
                :clearable="tree.clearable"
                :popoverWidth="tree.popoverWidth"
                v-model="SelectedDepartment"
                @change="onChangeDepartmentFilterSearch"
                style="padding-right: 10px; width: 100%"
              ></select-department-tree-component>
            </el-col>
            <el-col :span="4">
                <app-select-new
                class="ta-annual-leave__filter-employee"
                  :dataSource="employeeFullLookup"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="selectAllEmployeeFilter"
                  :multiple="true"
                  style="width: 100%; padding-right: 10px"
                  :placeholder="$t('SelectEmployee')"
                  ref="employeeList"
                >
                </app-select-new>
            
            </el-col>
            <el-col :span="4">
              <el-input
                style="padding-bottom: 3px; float: left; width: 238px"
                :placeholder="$t('InputMCCMNVName')"
                v-model="filter"
                class="filter-input"
                @keyup.enter.native="Search"
              >
                <i
                  slot="suffix"
                  class="el-icon-search"
                  @click="Search"
                ></i>
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
              <el-dropdown
                style="margin-top: 10px"
                @command="handleCommand"
                trigger="click"
              >
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
          <data-table-component
          class="ta-annual-leave__data-table"
            :get-data="getData"
            ref="table"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :showSearch="false"
            :isShowPageSize="true"
          ></data-table-component>
        </div>
      </el-main>
    </el-container>
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
                <a class="fileTemplate-lbl" href="/Template_TA_AnualLeave.xlsx" download>{{ $t('Download') }}</a>
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
                <a class="fileTemplate-lbl" href="/Files/AnnualLeaveError.xlsx" download>{{ $t('Download') }}</a>
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
  </div>
</template>
  <script src="./ta-annual-leave-component.ts"></script>
  <style lang="scss">

.ta-annual-leave__data-table {
  .el-table {
    height: calc(100vh - 230px) !important;
    margin-top: 10px !important;
  }
}
.el-input-number__decrease,.el-input-number__increase{
  height: calc(100% - 2px);
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.ta-annual-leave__filter-employee .el-select__tags:not(:has(span span:nth-child(2))) {
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

.ta-annual-leave__filter-employee .el-select__tags:has(span span:nth-child(2)) {
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