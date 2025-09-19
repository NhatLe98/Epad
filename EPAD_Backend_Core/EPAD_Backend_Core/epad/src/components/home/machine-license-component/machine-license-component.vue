<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("MachineLicense") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main class="bgHome">
        <div>
          <el-dialog
            :title="isEdit ? $t('EditMachine') : $t('InsertMachine')"
            :visible.sync="showDialog"
            :before-close="Cancel"
            class="dialog-machine"
          >
            <el-form
              :model="ruleForm"
              :rules="rules"
              ref="ruleForm"
              label-width="168px"
              label-position="top"
              @keyup.enter.native="Submit"
            >
              <el-row>
                <el-col :span="12" style="padding-right: 30px">
                  <el-form-item
                    :label="$t('AliasName')"
                    @click.native="focus('AliasName')"
                  >
                    <el-input
                      ref="AliasName"
                      v-model="ruleForm.AliasName"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('IPAddress')"
                    @click.native="focus('IPAddress')"
                  >
                    <el-input
                      ref="IPAddress"
                      v-model="ruleForm.IPAddress"
                    ></el-input>
                  </el-form-item>
                </el-col>
                <el-col :span="12" style="padding-right: 30px">
                  <el-form-item :label="$t('UseForModule')">
                    <el-radio-group
                      v-model="ruleForm.DeviceModule"
                      size="mini"
                      class="radio-device-module"
                    >
                      <el-radio
                        v-for="item in deviceModules"
                        :key="item.index"
                        :label="item.index"
                      >
                        {{ $t(item.value) }}</el-radio
                      >
                    </el-radio-group>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row>
                <el-col :span="12" style="padding-right: 30px">
                  <el-form-item
                    :label="$t('DeviceId')"
                    @click.native="focus('DeviceId')"
                  >
                    <el-input
                      ref="DeviceId"
                      v-model="ruleForm.DeviceId"
                    ></el-input>
                  </el-form-item>

                  <el-form-item
                    :label="$t('Port')"
                    @click.native="focus('Port')"
                    prop="Port"
                  >
                    <el-input ref="Port" v-model="ruleForm.Port"></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('MaxAttendanceLogCapacity')"
                    @click.native="focus('AttendanceLogCapacity')"
                    prop="AttendanceLogCapacity"
                  >
                    <el-input ref="AttendanceLogCapacity" v-model="ruleForm.AttendanceLogCapacity"
                    onkeypress='return event.charCode >= 48 && event.charCode <= 57'></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('MaxUserCapacity')"
                    @click.native="focus('UserCapacity')"
                    prop="UserCapacity"
                  >
                    <el-input ref="UserCapacity" v-model="ruleForm.UserCapacity"
                    onkeypress='return event.charCode >= 48 && event.charCode <= 57'></el-input>
                  </el-form-item>
                  <el-col :span="12" style="padding-right: 30px">
                      <el-form-item
                    :label="$t('DeviceStatus')"
                    @click.native="focus('DeviceStatus')"
                    prop="DeviceStatus"
                  >
                    <el-select
                      ref="DeviceStatus"
                      :placeholder="$t('SelectStatus')"
                      v-model="ruleForm.DeviceStatus"
                    >
                      <el-option
                        v-for="item in deviceStatus"
                        :key="item.index"
                        :label="$t(item.value)"
                        :value="item.index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                    </el-col>
                    <el-col :span="12" style="padding-right: 30px">
                       
                  <el-form-item
                    :label="$t('DeviceType')"
                    @click.native="focus('DeviceType')"
                    prop="DeviceType"
                  >
                    <el-select
                      ref="DeviceType"
                      :placeholder="$t('SelectMachine')"
                      v-model="ruleForm.DeviceType"
                    >
                      <el-option
                        v-for="item in machineType"
                        :key="item.index"
                        :label="$t(item.value)"
                        :value="item.index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                    </el-col>
                    <el-form-item
                    :label="$t('Note')"
                    @click.native="focus('Note')"
                  >
                    <el-input
                      ref="Note"
                      type="textarea" :autosize="{ minRows: 3, maxRows: 6 }"
                      v-model="ruleForm.Note"
                    ></el-input>
                  </el-form-item>
               
                </el-col>
                <el-col :span="12" style="padding-right: 30px">
                  <el-form-item
                    :label="$t('SerialNumber')"
                    prop="SerialNumber"
                    @click.native="focus('SerialNumber')"
                  >
                    <el-input
                      ref="SerialNumber"
                      :disabled="isEdit ? true : false"
                      v-model="ruleForm.SerialNumber"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('ConnectionCode')"
                    @click.native="focus('ConnectionCode')"
                  >
                    <el-input
                      ref="ConnectionCode"
                      v-model="ruleForm.ConnectionCode"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('MaxFingerCapacity')"
                    @click.native="focus('FingerCapacity')"
                    prop="FingerCapacity"
                  >
                    <el-input ref="FingerCapacity" v-model="ruleForm.FingerCapacity"
                    onkeypress='return event.charCode >= 48 && event.charCode <= 57'></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('MaxFaceCapacity')"
                    @click.native="focus('FaceCapacity')"
                    prop="FaceCapacity"
                  >
                    <el-input ref="FaceCapacity" v-model="ruleForm.FaceCapacity"
                    onkeypress='return event.charCode >= 48 && event.charCode <= 57'></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('DeviceModel')"
                    @click.native="focus('DeviceModel')"
                    prop="DeviceModel"
                  >
                  <el-select
                        ref="DeviceModel"
                        filterable
                        :placeholder="$t('Service')"
                        v-model="ruleForm.DeviceModel"
                      >
                        <el-option
                          v-for="item in listAllProducer"
                          :key="item.Value"
                          :label="$t(item.Name)"
                          :value="item.Value"
                        ></el-option>
                      </el-select>
                    <!-- <el-input
                      ref="DeviceModel"
                      v-model="ruleForm.DeviceModel"
                    ></el-input> -->
                  </el-form-item>
                </el-col>

                <el-col :span="6" class="selection-service">
                  <el-row :span="6">
                    <el-form-item
                      :label="$t('Service')"
                      @click.native="focus('Service')"
                      prop="ServiceID"
                    >
                      <el-select
                        ref="ServiceType"
                        filterable
                        :placeholder="$t('Service')"
                        v-model="ruleForm.ServiceID"
                      >
                        <el-option
                          v-for="item in listAllService"
                          :key="item.index"
                          :label="$t(item.service)"
                          :value="item.index"
                        ></el-option>
                      </el-select>
                    </el-form-item>
                  </el-row>
                </el-col>

                <el-col :span="6" class="selection-service">
                  <el-row>
                    <el-form-item
                      :label="$t('GroupDevice')"
                      @click.native="focus('GroupDevice')"
                      prop="GroupDeviceID"
                    >
                      <el-select
                        ref="GroupDevice"
                        :placeholder="$t('GroupDevice')"
                        filterable
                        v-model="ruleForm.GroupDeviceID"
                      >
                        <el-option
                          v-for="item in listAllDevice"
                          :key="item.index"
                          :label="$t(item.device)"
                          :value="item.index"
                        ></el-option>
                      </el-select>
                    </el-form-item>
                  </el-row>
                </el-col>
              </el-row>

              <!-- <el-row >
                <el-col :span="24">
                  <el-form-item style="margin-left: 15px;">
                    <el-checkbox v-model="ruleForm.UseSDK">{{ $t("UseSDK") }}</el-checkbox>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row >
                <el-col :span="24">
                  <el-form-item style="margin-left: 15px;" prop="UsePush">
                    <el-checkbox v-model="ruleForm.UsePush">{{ $t("UsePush") }}</el-checkbox>
                  </el-form-item>
                </el-col>
              </el-row>-->
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button
                class="btnOK"
                type="primary"
                @click="SubmitAndNext"
                style="width: 200px"
                >{{ $t("SaveNext") }}</el-button
              >
              <el-button class="btnOK" type="primary" @click="Submit">{{
                $t("Save")
              }}</el-button>

              <el-button class="btnCancel" @click="Reset">{{
                $t("Cancel")
              }}</el-button>
              <el-button class="btnCancel" @click="Cancel">{{
                $t("Close")
              }}</el-button>
            </span>
          </el-dialog>

          <el-dialog
            :title="$t('UpdateLicense')"
            custom-class="customdialog"
            :visible.sync="showDialogImportLicense"
            :destroy-on-close="true"
          >
            <el-upload
              :auto-upload="false"
              :multiple="true"
              :on-change="uploadLicenseFile"
              accept=".bin"
              action
              drag
              class="el-upload-full"
              ref="uploadLicense"
            >
              <i class="el-icon-upload"></i>
              <div class="el-upload__text">
                {{ $t("DragFileHereOr") }}
                <em>{{ $t("ClickToUpload") }}</em>
              </div>
              <div class="el-upload__tip" slot="tip">
                {{ this.$t("BinFileSize5kb") }}
              </div>
            </el-upload>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="closePopupUpload">{{
                $t("Cancel")
              }}</el-button>
              <el-button
                class="btnOK"
                type="primary"
                @click="SubmitUploadLicense"
                >{{ $t("Upload") }}</el-button
              >
            </span>
          </el-dialog>
        </div>
        <div>
          <data-table-function-component
          :showButtonInsert="false"
                                :isHiddenEdit="true" :isHiddenDelete="true"
                                :showButtonColumConfig="true" :gridColumnConfig.sync="columns"
          ></data-table-function-component>
          <data-table-component
            :get-data="getData"
            ref="table"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :isShowPageSize="true"
            :showCheckbox="false"
            :isFromMachineLicense="true"
          ></data-table-component>
        </div>
        <div>
          <el-dialog
            :title="$t('AddFromExcel')"
            custom-class="customdialog"
            :visible.sync="showDialogImportMachine"
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
                    @change="processFile($event.target.files)"
                  />
                  <label for="fileUpload"
                    ><svg
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
                  href="/Template_IC_Device.xlsx"
                  download
                  >{{ $t("Download") }}</a
                >
              </el-form-item>
            </el-form>

            <span slot="footer" class="dialog-footer">
              <el-button class="btnCancel" @click="closeDialogImportMachine">
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
<script src="./machine-license-component.ts"></script>
<style>

</style>