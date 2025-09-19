<template>
  <div class="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("EmployeeStopped") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog
            :title="isEdit ? $t('Edit') : $t('Insert')"
            custom-class="customdialog"
            :close-on-click-modal="false"
            :visible.sync="showDialog"
            :before-close="Cancel"
          >
            <el-form
              :model="ruleEmployeeStopped"
              :rules="rules"
              ref="ruleEmployeeStopped"
              label-width="168px"
              label-position="top"
              @key.enter.native="Submit"
            >
              <el-form-item
                @click.native="focus('Department')"
                :label="$t('Department')"
                prop="Department"
                :placeholder="$t('SelectDepartment')"
              >
                <select-department-tree-component
                  :defaultExpandAll="tree.defaultExpandAll"
                  :multiple="true"
                  :disable="isEdit"
                  :placeholder="$t('SelectDepartment')"
                  :data="tree.treeData"
                  :props="tree.treeProps"
                  :isSelectParent="true"
                  :checkStrictly="true"
                  :clearable="tree.clearable"
                  @change="onChangeDepartmentFilter"
                  v-mode="ruleEmployeeStopped.EmployeeATIDs"
                  :popoverWidth="tree.popoverWidth"
                  style="margin-bottom: 20px; width: 100%"
                >
                </select-department-tree-component>
              </el-form-item>
              <el-form-item
                :label="$t('Employee')"
                prop="EmployeeATIDs"
                @click.native="focus('Employee')"
              >
                <app-select-new
                  :disabled="isEdit"
                  :dataSource="employeeFullLookupFilter"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="ruleEmployeeStopped.EmployeeATIDs"
                  :multiple="true"
                  @getValueSelectedAll="selectAllEmployeeFilter"
                  :placeholder="$t('SelectEmployee')"
                  ref="employeeList"
                  style="width: 100%"
                >
                </app-select-new>
              </el-form-item>

              <el-form-item
                :label="$t('StoppedDate')"
                prop="StoppedDate"
                @click.native="focus('StoppedDate')"
              >
                <el-date-picker
                  type="date"
                  ref="StoppedDate"
                  v-model="ruleEmployeeStopped.StoppedDate"
                ></el-date-picker>
              </el-form-item>
              <el-form-item
                :label="$t('Reason')"
                prop="Reason"
                @click.native="focus('Reason')"
              >
                <el-input
                  type="textarea"
                  class="InputArea"
                  ref="Reason"
                  v-model="ruleEmployeeStopped.Reason"
                ></el-input>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="Cancel">
                {{ $t("Cancel") }}
              </el-button>
              <el-button class="btnOK" type="primary" @click="Submit">{{
                $t("OK")
              }}</el-button>
            </span>
          </el-dialog>
        </div>

        <div>
          <el-dialog
            :title="$t('DialogHeaderTitle')"
            custom-class="customdialog"
            :visible.sync="showDialogImportError"
            @close="showOrHideImportError(false)"
            :close-on-click-modal="false"
          >
            <el-form label-width="168px" label-position="top">
              <el-form-item>
                <div class="box">
                  <label>
                    <span>{{ importErrorMessage }}</span>
                  </label>
                </div>
              </el-form-item>
              <el-form-item>
                <a
                  class="fileTemplate-lbl"
                  href="/Files/Template_EmployeeStopped_Error.xlsx"
                  download
                  >{{ $t("Download") }}</a
                >
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button
                class="btnOK"
                type="primary"
                @click="showOrHideImportError(false)"
              >
                OK
              </el-button>
            </span>
          </el-dialog>
        </div>
        <div>
          <data-table-function-component
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            @add-excel="AddOrDeleteFromExcel('add')"
            v-bind:listExcelFunction="listExcelFunction"
            :showButtonColumConfig="true"
            :gridColumnConfig.sync="columns"
          >
          </data-table-function-component>

          <data-table-component
            :get-data="getData"
            ref="table"
            :selectedRows.sync="rowsObj"
            :columns="columns"
            :isShowPageSize="true"
          ></data-table-component>
        </div>

        <div>
          <el-dialog
            :title="isAddFromExcel ? $t('AddFromExcel') : $t('DeleteFromExcel')"
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
                <div class="box">
                  <input
                    ref="fileInput"
                    accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel"
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
                    <span>{{ $t("ChooseAExcelFile") }}</span>
                  </label>
                  <span v-if="fileName === ''" class="fileName">{{
                    $t("NoFileChoosen")
                  }}</span>
                  <span v-else class="fileName">{{ fileName }}</span>
                </div>
              </el-form-item>
              <el-form-item :label="$t('DownloadTemplate')">
                <a
                  class="fileTemplate-lbl"
                  href="/Template_IC_EmployeeStopped.xlsx"
                  download
                  >{{ $t("Download") }}</a
                >
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

<script src="./employee-stopped-component.ts"></script>