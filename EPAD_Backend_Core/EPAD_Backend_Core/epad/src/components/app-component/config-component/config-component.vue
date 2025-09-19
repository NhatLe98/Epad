<template>
  <el-row :gutter="20">
    <el-form class="config">
      <el-col :span="24">
        <el-form-item>
          <el-collapse>
            <el-collapse-item :title="$t(config.Title)" :name="config.Title">
              <el-row :gutter="20" style="padding-top: 10px">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType != 'DELETE_SYSTEM_COMMAND' &&
                    config.EventType != 'GENERAL_SYSTEM_CONFIG' &&
                    config.EventType != 'ECMS_DEFAULT_MEAL_CARD_DEPARTMENT' &&
                    config.EventType !=
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.EventType != 'SEND_MAIL_WHEN_DEVICE_OFFLINE' &&
                    config.EventType != 'CREATE_DEPARTMENT_IMPORT_EMPLOYEE' && config.EventType != 'CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET'
                  "
                >
                  <el-form-item
                    @click.native="focus('Time')"
                    :label="$t('Time')"
                    label-width="250px"
                  >
                    <el-select
                      ref="Time"
                      v-model="config.TimePos"
                      multiple
                      filterable
                      clearable
                      allow-create
                      class="w-100"
                    >
                      <el-option
                        v-for="item in timePosOption"
                        :key="item"
                        :label="item"
                        :value="item"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="config.EventType == 'EMPLOYEE_SHIFT_INTEGRATE'"
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.IsOverwriteData">{{
                    $t("IntegrateByTime")
                  }}</el-checkbox>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="config.EventType == 'GENERAL_SYSTEM_CONFIG'"
              >
                <el-row :gutter="20" style="padding-top: 10px">
                  <el-col :span="23">
                    <el-form-item
                      :label="$t('BodyTemperature')"
                      label-width="260px"
                    >
                      <el-input
                        ref="BodyTemperature"
                        v-model="config.BodyTemperature"
                      ></el-input>
                    </el-form-item>
                  </el-col>
                </el-row>
              </el-row>
              <el-row
                :gutter="20"
                v-if="config.EventType == 'ADD_OR_DELETE_USER'"
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.AutoIntegrate">{{
                    $t("AutoIntegrate")
                  }}</el-checkbox>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="config.EventType == 'ADD_OR_DELETE_USER'"
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.IntegrateWhenNotInclareDepartment">{{
                    $t("IntegrateWhenNotInclareDepartment")
                  }}</el-checkbox>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="
                  config.EventType == 'EMPLOYEE_INTEGRATE' ||
                  config.EventType == 'EMPLOYEE_SHIFT_INTEGRATE'
                "
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.UsingDatabase">{{
                    $t("UsingDatabase")
                  }}</el-checkbox>
                </el-col>
              </el-row>
              <el-row :gutter="20" v-if="(config.EventType == 'DOWNLOAD_USER' || config.EventType == 'ADD_OR_DELETE_USER') && !isHideMachineSerialSelect">
                <el-col :span="23">
                  <el-form-item
                    @click.native="focus('SerialNumbers')"
                  :label="config.EventType == 'DOWNLOAD_USER' ? $t('SerialNumber') :$t('DefaultMachine') "
                    label-width="250px"
                  >
                    <el-select
                      ref="SerialNumbers"
                      v-model="config.ListSerialNumber"
                      multiple
                      filterable
                      clearable
                      class="w-100"
                    >
                      <el-option
                          v-for="item in serialNumberOption"
                        :key="item.value"
                        :label="item.label"
                        :value="item.value"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20" v-if="config.EventType == 'AUTO_DELETE_BLACKLIST' ">
                <el-col :span="23">
                  <el-form-item
                    @click.native="focus('SerialNumbers')"
                    :label="$t('GroupDeviceName')"
                    label-width="250px"
                  >
                    <el-select
                      ref="SerialNumbers"
                      v-model="config.ListSerialNumber"
                      multiple
                      filterable
                      clearable
                      class="w-100"
                    >
                      <el-option
                        v-for="item in groupDeviceOption"
                        :key="item.value"
                        :label="item.label"
                        :value="item.value"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20" v-if="config.EventType == 'DOWNLOAD_USER'">
                <el-col :span="4">
                  <el-checkbox v-model="config.IsOverwriteData">{{
                    $t("SyncOverwriteUserMaster")
                  }}</el-checkbox>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="config.EventType === 'DELETE_SYSTEM_COMMAND'"
              >
                <el-col :span="23" class="config-day">
                  <el-form-item
                    class="deletelog"
                    @click.native="focus('DeleteAfterHours')"
                    :label="getLabel"
                    label-width="250px"
                  >
                    <el-input
                      ref="DeleteAfterHours"
                      v-model="config.AfterHours"
                      onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                    ></el-input>
                  </el-form-item>
                  <span class="day-in-config">{{ $t("afterhours") }}</span>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="
                  config.EventType == 'EMPLOYEE_SHIFT_INTEGRATE' &&
                  config.UsingDatabase == true &&
                  config.IsOverwriteData
                "
              >
                <el-col :span="10">
                  <el-form-item
                    :label="$t('WorkingFromDate')"
                    label-width="250px"
                  >
                    <el-date-picker
                      format="MM/dd/yyyy"
                      v-model="config.FromDate"
                      type="date"
                      :placeholder="$t('WorkingFromDate')"
                    >
                    </el-date-picker>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item
                    :label="$t('WorkingToDate')"
                    label-width="100px"
                  >
                    <el-date-picker
                      format="MM/dd/yyyy"
                      v-model="config.ToDate"
                      type="date"
                      :placeholder="$t('WorkingToDate')"
                    >
                    </el-date-picker>
                  </el-form-item>
                </el-col>
              </el-row>
              <!-- <el-row
                :gutter="20"
                v-if="config.EventType == 'EMPLOYEE_SHIFT_INTEGRATE' && config.UsingDatabase == true && clientName !== 'Ortholite'"
              >
              <el-col :span="23" class="config-day">
                 
                  <el-form-item
                   
                    @click.native="focus('DownloadFromPreviousDay')"
                    :label="getLabel"
                    label-width="250px"
                  >
                    <el-input
                      ref="DownloadFromPreviousDay"
                      v-model="config.PreviousDays"
                    ></el-input>
                  </el-form-item>

                  <span class="day-in-config">{{ $t(dayToCurrentDay) }}</span>
                </el-col>
              </el-row> -->
              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'INTEGRATE_LOG'"
                >
                  <el-form-item :label="$t('SoftwareType')" label-width="250px">
                    <el-select
                      v-model="config.SoftwareType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listSoftware"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('UserName')"
                    :label="$t('Account')"
                    label-width="250px"
                  v-if="config.SoftwareType == 2"
                  >
                    <el-input ref="UserName" v-model="config.UserName"></el-input>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('Password')"
                    :label="$t('Password')"
                    label-width="250px"
                    v-if="config.SoftwareType == 2"
                  >
                    <el-input ref="Password" v-model="config.Password"></el-input>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'RE_PROCESSING_REGISTERCARD'"
                >
               
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  

                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'DOWNLOAD_PARKING_LOG'"
                >
               
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  

                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'INTEGRATE_INFO_TO_OFFLINE'"
                >
               
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  

                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'EMPLOYEE_INTEGRATE_TO_DATABASE' && config.SoftwareType != 2"
                >
                  <el-form-item :label="$t('SoftwareType')" label-width="250px">
                    <el-select
                      v-model="config.SoftwareType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listSoftware"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('Token')"
                    :label="$t('Token')"
                    label-width="250px"
                  
                  >
                    <el-input ref="Token" v-model="config.Token"></el-input>
                  </el-form-item>
                

                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col 
                  :span="23"
                  v-if="config.EventType == 'EMPLOYEE_INTEGRATE_TO_DATABASE' && config.SoftwareType == 2"
                >
                  <el-form-item :label="$t('SoftwareType')" label-width="250px">
                    <el-select
                      v-model="config.SoftwareType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listSoftware"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('UserName')"
                    :label="$t('Account')"
                    label-width="250px"
                  
                  >
                    <el-input ref="UserName" v-model="config.UserName"></el-input>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('Password')"
                    :label="$t('Password')"
                    label-width="250px"
                  
                  >
                    <el-input ref="Password" v-model="config.Password"></el-input>
                  </el-form-item>

                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="config.EventType == 'LOG_INTEGRATE_TO_DATABASE'"
                >
                  <el-form-item :label="$t('SoftwareType')" label-width="250px">
                    <el-select
                      v-model="config.SoftwareType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listSoftware"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('LinkIntegrate')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkIntegrate" v-model="config.LinkAPIIntegrate"></el-input>
                  </el-form-item>
                  <el-form-item
                    @click.native="focus('Token')"
                    :label="$t('Token')"
                    label-width="250px"
                  >
                    <el-input ref="Token" v-model="config.Token"></el-input>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="
                  config.EventType == 'INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL' &&
                  config.UsingDatabase == true &&
                  clientName !== ''
                "
              >
                <el-col :span="10">
                  <el-form-item
                    :label="$t('WorkingFromDate')"
                    label-width="250px"
                  >
                    <el-date-picker
                      format="MM/dd/yyyy"
                      v-model="config.FromDate"
                      type="date"
                      :placeholder="$t('WorkingFromDate')"
                    >
                    </el-date-picker>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item
                    :label="$t('WorkingToDate')"
                    label-width="100px"
                  >
                    <el-date-picker
                      format="MM/dd/yyyy"
                      v-model="config.ToDate"
                      type="date"
                      :placeholder="$t('WorkingToDate')"
                    >
                    </el-date-picker>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="config.EventType == 'ECMS_DEFAULT_MEAL_CARD_DEPARTMENT'"
              >
                <el-col :span="23">
                  <el-form-item
                    :label="$t('Department')"
                    prop="Department"
                    :placeholder="$t('SelectDepartment')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.DepartmentIndex"
                      :placeholder="$t('SelectDepartment')"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in departmentOption"
                        :key="item.value"
                        :label="item.label"
                        :value="item.value"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="
                  !isNullOrUndefined(config.LinkAPI) &&
                  config.UsingDatabase == false
                "
              >
                <el-col :span="23">
                  <el-form-item
                    @click.native="focus('LinkAPI')"
                    :label="$t('Link API')"
                    label-width="250px"
                  >
                    <el-input ref="LinkAPI" v-model="config.LinkAPI"></el-input>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="
                  config.EventType == 'EMPLOYEE_SHIFT_INTEGRATE' &&
                  !config.IsOverwriteData
                "
              >
                <el-col :span="23" class="config-day">
                  <el-form-item
                    @click.native="focus('DownloadFromPreviousDay')"
                    :label="getLabel"
                    label-width="250px"
                  >
                    <el-input-number
                      ref="DownloadFromPreviousDay"
                      v-model="config.PreviousDays"
                      style="width: 100%"
                    ></el-input-number>
                  </el-form-item>

                  <span class="day-in-config">{{ $t(dayToCurrentDay) }}</span>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="
                  !isNullOrUndefined(config.PreviousDays) &&
                  config.EventType !== 'EMPLOYEE_SHIFT_INTEGRATE' &&
                  config.EventType !== 'EMPLOYEE_INTEGRATE_TO_DATABASE' &&
                  config.EventType !== 'AUTO_DELETE_BLACKLIST' 
                "
              >
                <el-col :span="23" class="config-day">
                  <el-form-item
                    v-if="config.EventType === 'DELETE_LOG'"
                    class="deletelog"
                    @click.native="focus('DownloadFromPreviousDay')"
                    :label="getLabel"
                    label-width="250px"
                  >
                    <el-input
                      ref="DownloadFromPreviousDay"
                      v-model="config.PreviousDays"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    v-else
                    @click.native="focus('DownloadFromPreviousDay')"
                    :label="getLabel"
                    label-width="250px"
                  >
                    <el-input
                      ref="DownloadFromPreviousDay"
                      v-model="config.PreviousDays"
                    ></el-input>
                  </el-form-item>

                  <span class="day-in-config">{{ $t(dayToCurrentDay) }}</span>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="!isNullOrUndefined(config.WriteToDatabase)"
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.WriteToDatabase">{{
                    $t("WriteToDatabase")
                  }}</el-checkbox>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="!isNullOrUndefined(config.WriteToDatabase)"
              >
                <el-col :span="4">
                  <el-checkbox v-model="config.WriteToFile">{{
                    $t("WriteToFile")
                  }}</el-checkbox>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="!isNullOrUndefined(config.WriteToFilePath) && config.EventType == 'INTEGRATE_LOG'"
              >
              <el-col :span="23">
                  <el-form-item
                    :label="$t('FileType')"
                    label-width="250px"
                    v-if="config.WriteToFilePath"
                    @click.native="focus('FileType')"
                  >
                    <!-- <el-input
                      :placeholder="$t('FileType')"
                      v-model="config.FileType"
                    ></el-input> -->

                    <el-select
                      ref="FileType"
                      v-model="config.FileType"
                      filterable
                      clearable
                      reserve-keyword
                      class="w-100"
                    >
                      <el-option
                      v-for="item in listFileType"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>    
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
                :gutter="20"
                v-if="!isNullOrUndefined(config.WriteToDatabase)"
              >
                <el-col :span="23">
                  <el-form-item
                    :label="$t('PathToFile')"
                    label-width="250px"
                    v-if="config.WriteToFile"
                  >
                    <el-input
                      :placeholder="$t('PathToFile')"
                      v-model="config.WriteToFilePath"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>
            
              <el-row :gutter="20">
                <!-- TODO: In future will implement send mail when delete system command, currently temporary disable email input-->
                <el-col
                  :span="23"
                  v-if="
                    config.EventType != 'ECMS_DEFAULT_MEAL_CARD_DEPARTMENT' &&
                    config.EventType !=
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.EventType != 'DELETE_SYSTEM_COMMAND' &&
                    config.EventType != 'CREATE_DEPARTMENT_IMPORT_EMPLOYEE' && config.EventType != 'CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET'
                  "
                >
                  <el-form-item
                    @click.native="focus('SendResultToEMail')"
                    :label="$t('SendResultToEMail')"
                    label-width="250px"
                  >
                    <el-select
                      ref="SendResultToEMail"
                      v-model="config.Email"
                      multiple
                      filterable
                      clearable
                      reserve-keyword
                      allow-create
                      class="w-100"
                    >
                      <el-option
                        v-for="item in emailOption"
                        :key="item"
                        :label="item"
                        :value="item"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="config.EventType === 'DELETE_SYSTEM_COMMAND'"
              >
                <el-col :span="23" v-if="config.Email.length > 0">
                  <el-checkbox v-model="config.SendEmailWithFile">{{
                    $t("SendEmailWithFile")
                  }}</el-checkbox>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="
                  config.Email.length != 0 &&
                  isNullOrUndefined(config.NotShowEmailDetail) &&
                  config.EventType != 'SEND_MAIL_WHEN_DEVICE_OFFLINE'
                "
              >
                <el-col :span="23">
                  <el-form-item>
                    <el-checkbox v-model="config.SendMailWhenError">{{
                      $t("SendMailWhenError")
                    }}</el-checkbox>
                  </el-form-item>
                  <el-form-item
                    :label="$t('TitleEmailError')"
                    label-width="250px"
                    v-if="config.SendMailWhenError"
                  >
                    <el-input
                      :placeholder="$t('TitleEmailError')"
                      v-model="config.TitleEmailError"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('BodyEmailError')"
                    label-width="250px"
                    v-if="config.SendMailWhenError"
                  >
                    <el-input
                      type="textarea"
                      :placeholder="$t('BodyEmailError')"
                      v-model="config.BodyEmailError"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="
                  config.Email.length != 0 &&
                  config.EventType == 'SEND_MAIL_WHEN_DEVICE_OFFLINE'
                "
              >
                <el-col :span="23">
                  <el-form-item :label="$t('TitleEmail')" label-width="250px">
                    <el-input
                      :placeholder="$t('TitleEmail')"
                      v-model="config.TitleEmailError"
                      maxlength="500"
                    ></el-input>
                  </el-form-item>
                  <el-form-item :label="$t('BodyEmail')" label-width="250px">
                    <el-input
                      type="textarea"
                      :placeholder="$t('BodyEmail')"
                      v-model="config.BodyEmailError"
                      maxlength="1000"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row
                :gutter="20"
                v-if="
                  config.Email.length != 0 &&
                  isNullOrUndefined(config.NotShowEmailDetail) &&
                  config.EventType != 'GENERAL_SYSTEM_CONFIG' &&
                  config.EventType != 'SEND_MAIL_WHEN_DEVICE_OFFLINE'
                "
              >
                <el-col :span="23">
                  <el-form-item>
                    <el-checkbox v-model="config.AlwaysSend">{{
                      `${$t("AlwaysSendAfter")} ${$t(config.EventType)}`
                    }}</el-checkbox>
                  </el-form-item>
                  <el-form-item
                    :label="$t('TitleEmailSuccess')"
                    label-width="250px"
                    v-if="config.AlwaysSend"
                  >
                    <el-input
                      :placeholder="$t('TitleEmailSuccess')"
                      v-model="config.TitleEmailSuccess"
                    ></el-input>
                  </el-form-item>
                  <el-form-item
                    :label="$t('BodyEmailSuccess')"
                    label-width="250px"
                    v-if="config.AlwaysSend"
                  >
                    <el-input
                      type="textarea"
                      :placeholder="$t('BodyEmailSuccess')"
                      v-model="config.BodyEmailSuccess"
                    ></el-input>
                  </el-form-item>
                </el-col>
              </el-row>

              <!-- <el-row :gutter="20" v-if="!isNullOrUndefined(config.DeleteLogAfterSuccess)">
          <el-col :span="23">
            <el-form-item :label="$t('DeleteLogAfterDownloadSuccess')" label-width="250px">
              <el-checkbox style="margin-left: 0px;" v-model="config.DeleteLogAfterSuccess" />
            </el-form-item>
          </el-col>
    </el-row> -->

              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType == 'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA'
                  "
                >
                  <el-form-item
                    :label="$t('RemoveStoppedWorkingEmployeesData')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.RemoveStoppedWorkingEmployeesType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listRemoveStoppedWorkingEmployeesType"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.RemoveStoppedWorkingEmployeesType == 1
                  "
                >
                  <el-form-item :label="$t('ChooseDay')" label-width="250px">
                    <el-input-number
                      :min="0"
                      style="width: 100%"
                      v-model="config.RemoveStoppedWorkingEmployeesDay"
                      placeholder=""
                      onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                    ></el-input-number>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.RemoveStoppedWorkingEmployeesType == 2
                  "
                >
                  <el-form-item
                    :label="$t('ChooseDayInWeek')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.RemoveStoppedWorkingEmployeesWeek"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listDayInWeek"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.RemoveStoppedWorkingEmployeesType == 3
                  "
                >
                  <el-form-item
                    :label="$t('ChooseDayInMonth')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.RemoveStoppedWorkingEmployeesMonth"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listDayInMonth"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    (config.RemoveStoppedWorkingEmployeesType == 2 ||
                      config.RemoveStoppedWorkingEmployeesType == 3)
                  "
                >
                  <el-form-item :label="$t('Time')" label-width="250px">
                    <el-time-picker
                      v-model="config.RemoveStoppedWorkingEmployeesTime"
                      :placeholder="$t('SelectTime')"
                      format="HH:mm"
                      style="width: 100%"
                    >
                    </el-time-picker>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType == 'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA'
                  "
                >
                  <el-form-item
                    :label="$t('ShowStoppedWorkingEmployeesData')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.ShowStoppedWorkingEmployeesType"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listShowStoppedWorkingEmployeesType"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.ShowStoppedWorkingEmployeesType == 1
                  "
                >
                  <el-form-item :label="$t('ChooseDay')" label-width="250px">
                    <el-input-number
                      :min="0"
                      style="width: 100%"
                      v-model="config.ShowStoppedWorkingEmployeesDay"
                      placeholder=""
                      onkeypress="return event.charCode >= 48 && event.charCode <= 57"
                    ></el-input-number>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.ShowStoppedWorkingEmployeesType == 2
                  "
                >
                  <el-form-item
                    :label="$t('ChooseDayInWeek')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.ShowStoppedWorkingEmployeesWeek"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listDayInWeek"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    config.ShowStoppedWorkingEmployeesType == 3
                  "
                >
                  <el-form-item
                    :label="$t('ChooseDayInMonth')"
                    label-width="250px"
                  >
                    <el-select
                      v-model="config.ShowStoppedWorkingEmployeesMonth"
                      filterable
                      clearable
                    >
                      <el-option
                        v-for="item in listDayInMonth"
                        :key="item.Index"
                        :label="item.Name"
                        :value="item.Index"
                      ></el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="20">
                <el-col
                  :span="23"
                  v-if="
                    config.EventType ==
                      'MANAGE_STOPPED_WORKING_EMPLOYEES_DATA' &&
                    (config.ShowStoppedWorkingEmployeesType == 2 ||
                      config.ShowStoppedWorkingEmployeesType == 3)
                  "
                >
                  <el-form-item :label="$t('Time')" label-width="250px">
                    <el-time-picker
                      v-model="config.ShowStoppedWorkingEmployeesTime"
                      :placeholder="$t('SelectTime')"
                      format="HH:mm"
                      style="width: 100%"
                    >
                    </el-time-picker>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row
              :gutter="20"
              v-if="config.EventType == 'CREATE_DEPARTMENT_IMPORT_EMPLOYEE'"
            >
              <el-col :span="4">
                <el-checkbox v-model="config.AutoCreateDepartmentImportEmployee">{{
                  $t("AutoCreateDepartmentImportEmployee")
                }}</el-checkbox>
              </el-col>
            </el-row>

            <el-row
            :gutter="20"
            v-if="config.EventType == 'CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET'"
          >
          <el-col :span="23">
            <el-form-item
            @click.native="focus('SendResultToEMail')"
            :label="$t('EmailAllowImportGoogleForm')"
            label-width="250px"
          >
            <el-select
              ref="SendResultToEMail"
              v-model="config.EmailAllowImportGoogleSheet"
              multiple
              filterable
              clearable
              reserve-keyword
              allow-create
              class="w-100"
            >
              <el-option
                v-for="item in emailOption"
                :key="item"
                :label="item"
                :value="item"
              ></el-option>
            </el-select>
          </el-form-item>
          </el-col>
          </el-row>
          <slot></slot>
            </el-collapse-item>
          </el-collapse>
        </el-form-item>
      </el-col>
    </el-form>
  </el-row>
</template>
<script src="./config-component.ts"></script>

<style scoped>
.el-checkbox {
  margin-left: 250px;
}


</style>
