<template>
  <div id="bgHome">
    <el-row>
      <el-col :span="5" class="Tree">
        <el-input class="change-department__input-tree"
          style="
            padding: 5px;
            position: absolute;
            width: 20%;
            height: 30px;
            z-index: 1000;
          "
          :placeholder="$t('SearchData')"
          v-model="filterTree"
          @keyup.enter.native="filterTreeData()"
        >
          <i slot="suffix" class="el-icon-search" @click="filterTreeData()"></i>
        </el-input>
        <el-tree class="table-text-color"
          style="margin-top: 50px"
          :data="treeData"
          :props="{ label: 'Name', children: 'ListChildrent' }"
          :filter-node-method="filterNode"
          :default-expanded-keys="expandedKey"
          :default-checked-keys="defaultChecked"
          node-key="ID"
          @check="nodeCheck"
          ref="tree"
          show-checkbox
          highlight-current
          v-loading="loadingTree"
        >
          <template slot-scope="scoped">
            <div>
              <i :class="getIconClass(scoped.data.Type, scoped.data.Gender)" />
              <span class="ml-5">{{ scoped.data.Name }}</span>
            </div>
          </template>
        </el-tree>
      </el-col>
      <el-col :span="19">
        <el-container>
          <el-header>
            <el-row>
              <el-col :span="10" class="left">
                <span id="FormName">
                  {{ $t("ChangeDepartment") }}
                </span>
              </el-col>
              <el-col :span="14">
                <HeaderComponent :showMasterEmployeeFilter="true"/>
              </el-col>
            </el-row>
          </el-header>

          <el-main class="bgHomeHasView">
            <el-row class="change-department__custom-function-bar">
              <el-col :span="3" style="margin-right: 10px;">
                <el-date-picker
                :placeholder="$t('FromDateString')"
                  v-model="fromDate"
                  type="date"
                  id="inputFromDate"
                ></el-date-picker>
              </el-col>
              <el-col :span="3" style="margin-right: 10px;">
                <el-date-picker
                :placeholder="$t('ToDateString')"
                  v-model="toDate"
                  type="date"
                  id="inputToDate"
                >
                </el-date-picker>
              </el-col>
              <el-col :span="1">
                <el-button
                  type="primary"
                  size="small"
                  class="smallbutton"
                  @click="View"
                >
                  {{ $t("View") }}
                </el-button>
              </el-col>
            </el-row>
            
            <!--<el-col :span="6" style="padding-right: 10px">
                            <select-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                                   :multiple="tree.multiple"
                                                   :placeholder="tree.placeholder"
                                                   :disabled="tree.isEdit"
                                                   :data="tree.treeData"
                                                   :props="tree.treeProps"
                                                   :checkStrictly="tree.checkStrictly"
                                                   :clearable="tree.clearable"
                                                   :popoverWidth="tree.popoverWidth"
                                                   v-model="tree.employeeList"></select-tree-component>
                        </el-col>-->
            <div>
              <approve-popup-component
                :title="$t('WaitingApproveList')"
                ref="approvePopup"
              ></approve-popup-component>
              <el-dialog
                :title="
                  isEdit
                    ? $t('EditTransferDepartment')
                    : $t('InsertTransferDepartment')
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
                  @keyup.enter.native="Submit"
                >
                  <el-form-item
                    :label="$t('DepartmentTransfer')"
                    @click.native="focus('DepartmentTransfer')"
                    prop="NewDepartment"
                  >
                    <!-- <el-select
                      ref="DepartmentTransfer"
                      v-model="ruleForm.NewDepartment"
                    >
                      <el-option
                        v-for="(item, index) in comboDepartment"
                        :key="index"
                        :label="item.label"
                        :value="item.value"
                      ></el-option>
                    </el-select> -->
                    <select-department-tree-component :defaultExpandAll="tree.defaultExpandAll"
                                    :multiple="false"
                                    :placeholder="$t('SelectDepartment')"
                                    :disabled="tree.isEdit"
                                    :data="tree.treeData"
                                    :props="tree.treeProps"
                                    :checkStrictly="true"
                                    :isSelectParent="true"
                                    :clearable="tree.clearable"
                                    :popoverWidth="tree.popoverWidth"
                                    v-model="ruleForm.NewDepartment"
                                    style="width: 100%; margin-bottom: 15px;"
                                    ></select-department-tree-component>
                  </el-form-item>
                  <el-form-item :label="$t('DateTransfer')" prop="FromTime">
                    <el-date-picker
                      v-model="ruleForm.FromTime"
                      type="date"
                      :picker-options="fromDatePickerOptions"
                    ></el-date-picker>
                  </el-form-item>
                  <el-form-item prop="TemporaryTransfer">
                    <el-checkbox v-model="ruleForm.TemporaryTransfer">
                      <span class="label-checkbox">
                        {{ $t("TemporaryTransfer") }}
                      </span>
                    </el-checkbox>
                  </el-form-item>
                  <el-form-item
                    :label="$t('DateBack')"
                    v-if="ruleForm.TemporaryTransfer == true"
                    prop="ToTime"
                  >
                    <el-date-picker
                      v-model="ruleForm.ToTime"
                      type="date"
                      :picker-options="fromDatePickerOptions"
                    ></el-date-picker>
                  </el-form-item>
                  <el-form-item prop="RemoveFromOldDepartment">
                    <el-checkbox v-model="ruleForm.RemoveFromOldDepartment">
                      <span class="label-checkbox">
                        {{ $t("RemoveFromOldDepartment") }}
                      </span>
                    </el-checkbox>
                  </el-form-item>
                  <el-form-item prop="AddOnNewDepartment">
                    <el-checkbox v-model="ruleForm.AddOnNewDepartment">
                      <span class="label-checkbox">
                        {{ $t("AddOnNewDepartment") }}
                      </span>
                    </el-checkbox>
                  </el-form-item>
                  <el-form-item
                    :label="$t('Description')"
                    @click.native="focus('Description')"
                    prop="Description"
                  >
                    <el-input
                      ref="Description"
                      type="textarea"
                      v-model="ruleForm.Description"
                      :autosize="{
                        minRows: 3,
                        maxRows: 6,
                      }"
                    ></el-input>
                  </el-form-item>
                  <el-form-item>
                    {{
                      $t("CountTransferUsersMessage", {
                        numberOfUser: key.length
                      })
                    }}
                  </el-form-item>
                </el-form>
                <span slot="footer" class="dialog-footer">
                  <el-button class="btnCancel" @click="Cancel">
                    {{ $t("Cancel") }}
                  </el-button>
                  <el-button class="btnOK" type="primary" @click="Submit">
                    {{ $t("OK") }}
                  </el-button>
                </span>
              </el-dialog>
            </div>
            <div class="table">
              <data-table-function-component class="change-department__data-table-function"
                @insert="Insert"
                @edit="Edit"
                @delete="Delete"
                @custombuttonclick="DisplayWaitingApprove"
                v-bind:listExcelFunction="listExcelFunction"
                v-bind:isHiddenEdit="true"
                v-bind:isHiddenDelete="true"
                v-bind:showButtonCustom="true"
                v-bind:buttonCustomText="$t('ShowWaitingApproveList')"
                @add-excel="AddOrDeleteFromExcel('add')"
                @export-excel="Export"
                :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
              >
              </data-table-function-component>
              <data-table-component class="change-department__data-table"
                :get-data="getData"
                ref="table"
                :columns="columns"
                :selectedRows.sync="rowsObj"
                v-bind:isFromHasView="true"
                :isShowPageSize="true"
              >
              </data-table-component>
            </div>
            <div>
              <el-dialog
                :title="$t('AddFromExcel')"
                custom-class="customdialog"
                :visible.sync="showDialogExcel"
                @close="AddOrDeleteFromExcel('close')"
                :close-on-click-modal="false"
              >
                <el-form
                  :model="formExcel"
                  ref="formExcel"
                  label-width="168px"
                  label-position="top"
                >
                  <el-form-item :label="$t('SelectFile')">
                    <!-- <label for="fileUpload">Click me to upload image</label> -->
                    <div class="box">
                      <input
                        ref="fileInput"
                        accept=".xls, .xlsx"
                        type="file"
                        name="file-3[]"
                        id="fileUpload"
                        class="inputfile inputfile-3"
                        @change="processFile($event)"
                        @click="(e) => e.target.value = null"
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
                        <span>
                          {{ $t("ChooseAExcelFile") }}
                        </span>
                      </label>
                      <span v-if="fileName === ''" class="fileName">
                        {{ $t("NoFileChoosen") }}
                      </span>
                      <span v-else class="fileName">
                        {{ fileName }}
                      </span>
                    </div>
                  </el-form-item>
                  <el-form-item :label="$t('DownloadTemplate')">
                    <a
                      class="fileTemplate-lbl"
                      href="/Template_Employee_Transfer.xlsx"
                      download
                      >{{ $t("Download") }}</a
                    >
                  </el-form-item>
                  <el-form-item
                    :label="$t('Result')"
                    @click.native="focus('InputMessage')"
                    prop="Description"
                  >
                    <el-input
                      ref="InputMessage"
                      id="InputMessage"
                      type="textarea"
                      v-model="resultAddExcel"
                      readonly
                      :autosize="{
                        minRows: 6,
                        maxRows: 12,
                      }"
                    ></el-input>
                  </el-form-item>
                </el-form>

                <span slot="footer" class="dialog-footer">
                  <el-button
                    class="btnCancel"
                    @click="AddOrDeleteFromExcel('close')"
                  >
                    {{ $t("Cancel") }}
                  </el-button>
                  <el-button
                    class="btnOK"
                    type="primary"
                    @click="AddEmployeeTransferFromExcel"
                  >
                    {{ $t("OK") }}
                  </el-button>
                </span>
              </el-dialog>
            </div>
          </el-main>
        </el-container>
      </el-col>
    </el-row>
  </div>
</template>
<script src="./change-department-component.ts"></script>

<style lang="scss">
.m-l-10 {
  margin-left: 10px !important;
}
.change-department__data-table-function {
  position: unset !important;
  display: flex !important;
  margin-bottom: 5px;
  justify-content: flex-end !important;
  width: 100% !important;
}
.change-department__data-table {
  .filter-input {
    margin-right: 10px;
  }
  .el-table {
    height: calc(100vh - 215px) !important;
    .el-table__fixed
    .el-table__body-wrapper {
      height: 100% !important;
    }
  }
}
.change-department__input-tree {
        .el-input__suffix{
            top: unset !important;
            margin-right: 5px;
        }
    }
</style>
