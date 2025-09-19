<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("EmailAllowDeclareGuest") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <data-table-function-component
            :showButtonColumConfig="true"
            :gridColumnConfig.sync="columns"
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            @add-excel="AddOrDeleteFromExcel('add')"
            v-bind:listExcelFunction="listExcelFunction"
             id="email_declare__data-table-function"
          >
          </data-table-function-component>

          <data-table-component
            :get-data="getData"
            ref="emailDeclareGuestTable"
            :selectedRows.sync="rowsObj"
            :columns="columns"
            :isShowPageSize="true"
          ></data-table-component>
        </div>

        <div>
          <el-dialog
            :title="isEdit ? $t('EditEmailDeclareGuest') : $t('InsertEmailDeclareGuest')"
            :visible.sync="showDialog"
            custom-class="customdialog"
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
                prop="EmployeeATID"
                :label="$t('SelectEmployee')"
              >
                <app-select-new
                  :disabled="isEdit"
                  :dataSource="employeeFullLookup"
                  displayMember="NameInFilter"
                  valueMember="Index"
                  :allowNull="true"
                  v-model="ruleForm.EmployeeATID"
                  :multiple="false"
                  style="width: 100%"
                  :placeholder="$t('SelectEmployee')"
                  ref="employeeList"
                >
                </app-select-new>
              </el-form-item>
              <el-form-item
              prop="EmailAddressList"
                :label="$t('EmailAddress')"
                label-width="250px"
              >
                <el-select
                  ref="EmailAddressList"
                  v-model="ruleForm.EmailAddressList"
                  multiple
                  filterable
                  clearable
                  reserve-keyword
                  allow-create
                  class="w-100"
                >
                  <el-option
                    v-for="item in ruleForm.EmailAddressList"
                    :key="item"
                    :label="item"
                    :value="item"
                  ></el-option>
                </el-select>
              </el-form-item>
              
              <el-form-item
                @click.native="focus('Description')"
                :label="$t('Description')"
              >
                <el-input
                  ref="Description"
                  type="textarea"
                  :autosize="{ minRows: 3, maxRows: 6 }"
                  v-model="ruleForm.Description"
                  class="InputArea"
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
                  href="/Files/Template_EmailDeclareGuest_Error.xlsx"
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
                <a
                  class="fileTemplate-lbl"
                  href="/Template_EmailDeclareGuest.xlsx"
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
<script src="./email-declare-guest-component.ts"></script>
<style lang="scss">
#email_declare__data-table-function{
  .popupshowcol{
    width: fit-content;
  }
}

</style>
