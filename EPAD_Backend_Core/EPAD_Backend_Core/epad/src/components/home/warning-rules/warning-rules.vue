<template>
  <div id="bgHome" class="warning-rules">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("WarningRules") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
      <el-main>
        <el-row style="height: 80vh !important;">
          <el-col :span="8" style="height: 100%; overflow-y: auto;">
            <h5>{{ $t("ListWarningRules") }}</h5>
            <el-menu>
            <template v-for="(col, index) in listData">
              <el-menu-item :key="index" v-bind:class="checkClass(col.RulesWarningGroupIndex)"
                @click="changeFormData(col.RulesWarningGroupIndex)">
                <el-col :span="2">{{ index + 1 }}</el-col>

                <el-col :span="22">{{ col.GroupName }}</el-col>
              </el-menu-item>
              <!-- <el-menu-item :key="index" v-else class="un-active">
              <el-col :span="2">{{ index + 1 }}</el-col>
              <el-col :span="22">{{
                col.GroupName
              }}</el-col>
            </el-menu-item> -->
            </template>
          </el-menu>
          </el-col>
          <el-col :span="16" style="height: 100%; overflow-y: auto;">
            <el-form ref="formref" :model="form" :rules="rule">
              <h5>
                <span class="text-danger">* </span>{{ $t("WarningRulesGroup") }}
                <span class="text-danger">{{ groupError }}</span>
              </h5>
              <el-form-item prop="currentGroup">
                <el-select v-model="currentGroup" :placeholder="$t('PleaseSelectRulesWarningGroup')" style="width: 100%"
                  v-if="isAdd">
                  <slot name="options" v-bind="listGroupSelect">
                    <el-option :key="index" :label="`${item.Name}`" :value="item.Index"
                      v-for="(item, index) in listGroupSelect"></el-option>
                  </slot>
                </el-select>
                <el-select v-model="currentGroup" :placeholder="$t('PleaseSelectRulesWarningGroup')" style="width: 100%"
                  v-else disabled>
                  <el-option :key="currentGroup" :label="currentGroupName" :value="currentGroup"></el-option>
                </el-select>
              </el-form-item>
              <h5>
                <span class="text-danger">* </span>{{ $t("WarningForm") }}
                <span class="text-danger">{{ formError }}</span>
              </h5>
              <el-form-item prop="UseSpeaker">
                <el-checkbox v-model="form.UseSpeaker">{{
                  $t("SpeakerLedLight")
                }}</el-checkbox>
                <span class="text-danger" style="margin-left: 10px">{{
                  errorSpeaker
                }}</span>
              </el-form-item>
              <slot v-if="form.UseSpeaker">
                <div style="margin-left: 20px">
                  <el-form-item prop="UseWarningFocus">
                    <el-checkbox v-model="form.UseSpeakerFocus">{{
                      $t("WarningFocus")
                    }}</el-checkbox>
                  </el-form-item>
                  <slot v-if="form.UseSpeakerFocus">
                    <select-controller-channel-component v-model="listSpeakerWarningFocus" :type="0" :useLine="false"
                      :listDevices="listDevices">
                    </select-controller-channel-component>
                  </slot>
                  <el-form-item prop="UseWarningInPlace">
                    <el-checkbox v-model="form.UseSpeakerInPlace">{{
                      $t("WarningInPlace")
                    }}</el-checkbox>
                  </el-form-item>
                  <slot v-if="form.UseSpeakerInPlace">
                    <select-controller-channel-component v-model="listSpeakerWarningInPlace" :type="0" :useLine="true"
                      :listDevices="listDevices">
                    </select-controller-channel-component>
                  </slot>
                </div>
              </slot>

              <!-- <el-form-item prop="UseLed">
                <el-checkbox v-model="form.UseLed">{{
                  $t("LedLight")
                }}</el-checkbox>
                <span class="text-danger" style="margin-left: 10px">{{
                  errorLed
                }}</span>
              </el-form-item>
              <slot v-if="form.UseLed">
                <div style="margin-left: 20px">
                  <el-form-item prop="UseWarningFocus">
                    <el-checkbox v-model="form.UseLedFocus">{{
                      $t("WarningFocus")
                    }}</el-checkbox>
                  </el-form-item>
                  <slot v-if="form.UseLedFocus">
                    <select-controller-channel-component v-model="listLedWarningFocus" :type="1" :useLine="false"
                      :listDevices="listDevices">
                    </select-controller-channel-component>
                  </slot>
                  <el-form-item prop="UseWarningInPlace">
                    <el-checkbox v-model="form.UseLedInPlace">{{
                      $t("WarningInPlace")
                    }}</el-checkbox>
                  </el-form-item>
                  <slot v-if="form.UseLedInPlace">
                    <select-controller-channel-component v-model="listLedWarningInPlace" :type="1" :useLine="true"
                      :listDevices="listDevices">
                    </select-controller-channel-component>
                  </slot>
                </div>
              </slot> -->

              <!--                   
                :listLines="listLines"
                :listDevices="listDevices"
                :listControllerSelect="listControllerSelect" -->

              <el-form-item prop="UseEmail">
                <el-checkbox v-model="form.UseEmail">{{
                  $t("Email")
                }}</el-checkbox>
              </el-form-item>
              <slot v-if="form.UseEmail">
                <el-form-item :label="$t('EmailAddress')" prop="Email">
                  <el-input v-model="form.Email"></el-input>
                </el-form-item>
                <!-- <span>{{ $t("TimeToSendMail") }}</span> -->
                <el-form-item :label="$t('TimeToSendMail')" prop="EmailSendType">
                  <el-radio-group v-model="form.EmailSendType" style="width: 100%">
                    <el-row>
                      <el-col :span="8">
                        <el-radio :label="0">{{ $t("RightAway") }}</el-radio>
                      </el-col>
                      <el-col :span="14" :offset="2">
                        <el-radio :label="1">{{ $t("FixedSchedule") }}</el-radio>
                      </el-col>
                    </el-row>
                  </el-radio-group>
                </el-form-item>
                <slot v-if="form.EmailSendType == 1">
                  <el-table :data="listEmailScheduler" style="width: 100%; margin-bottom: 20px">
                    <el-table-column prop="Time" :label="$t('Time')" width="350">
                      <template slot-scope="scope">
                        <el-time-picker format="HH:mm" placeholder="Pick a time" v-model="scope.row.Time" style="width: 100%"
                          @change="CheckDuplicateSchedule(scope.row)"></el-time-picker>
                      </template>
                    </el-table-column>
                    <el-table-column prop="DayOfWeekIndex" :label="$t('DayOfWeek')">
                      <template slot-scope="scope">
                        <el-select v-model="scope.row.DayOfWeekIndex" :placeholder="$t('SelectDayOfWeek')"
                          style="width: 100%" @change="CheckDuplicateSchedule(scope.row)">
                          <slot v-bind="listDayOfWeek">
                            <el-option :key="index" :label="$t(item.Name)" :value="item.Index"
                              v-for="(item, index) in listDayOfWeek"></el-option>
                          </slot>
                        </el-select>
                      </template>
                    </el-table-column>
                    <el-table-column prop="Function" label="">
                      <template slot-scope="scope">
                        <el-button type="warning" icon="el-icon-close" size="mini"
                          @click="deleteScheduleSendMail(scope.$index, scope.row)" v-if="listEmailScheduler.length > 1"
                          circle></el-button>

                        <el-button type="primary" icon="el-icon-plus" size="mini"
                          @click="addScheduleSendMail(scope.$index, scope.row)" v-if="scope.$index == 0"
                          circle
                          style="height: 28px !important; margin-left: 5px; margin-top: 1px;"></el-button>
                      </template>
                    </el-table-column>
                    <el-table-column prop="Error">
                      <template slot-scope="scope">
                        <span class="text-danger">{{ scope.row.Error }} </span>
                      </template>
                    </el-table-column>
                  </el-table>
                </slot>
              </slot>

              <el-form-item prop="UseComputerSound">
                <el-checkbox v-model="form.UseComputerSound">{{
                  $t("ComputerSound")
                }}</el-checkbox>
              </el-form-item>
              <slot v-if="form.UseComputerSound">
                <!-- <el-upload
                ref="upload"
                class="upload-demo"
                action="https://jsonplaceholder.typicode.com/posts/"
                :on-preview="handlePreview"
                :on-remove="handleRemove"
                :before-remove="beforeRemove"
                :on-exceed="handleExceed"
                :on-change="fileUploadChange"
                :file-list="fileList"
                :auto-upload="false"
              >
                <el-button size="small" type="primary">{{
                  $t("ChooseFile")
                }}</el-button>
                <div slot="tip" class="el-upload__tip text-danger" v-if="errorSound != ''">
                  {{errorSound}}
                </div>
              </el-upload> -->
                <single-upload-component v-if="isShowFileSelect" v-model="Attachments" @model="handleModel" v-bind:confirmDelete="confirmDelete"
                  v-bind:accept="acceptFile">
                </single-upload-component>
                <span class="text-danger" style="margin-left: 10px">{{
                  errorSoundFile
                }}</span>
              </slot>
              <!-- <el-form-item prop="UseChangeColor">
                <el-checkbox v-model="form.UseChangeColor">{{
                  $t("ChangeColorMonitor")
                }}</el-checkbox>
              </el-form-item> -->
            </el-form>
          </el-col>
        </el-row>
        <el-row>
      <el-col :span="8" align="right">
        <!-- <el-button
          v-if="roleByRoute.PermissionDetail.CanDelete==true"
          :disabled="this.activeMenuIndex < 1"
          type="danger"
          size="mini"
          icon="el-icon-delete"
          @click="del()"
          >{{ $t("Delete") }}</el-button
        >
        <el-button
        v-if="roleByRoute.PermissionDetail.CanCreate"
          type="primary"
          size="mini"
          icon="el-icon-plus"
          @click="add()"
          :disabled="this.listGroupSelect.length == 0"
          >{{ $t("Add") }}</el-button
        > -->
        <el-button 
          type="primary"
          :disabled="this.activeMenuIndex < 1" 
          @click="del()"
          style="margin-right: 5px; background-color: red !important;">
          <i class="el-icon-delete"></i> {{$t("Delete") }}
        </el-button>
        <el-button 
          type="primary" 
          @click="add()"
          :disabled="this.listGroupSelect.length == 0">
          <i class="el-icon-plus"></i> {{ $t("Add") }}
        </el-button>
      </el-col>
      <el-col :span="16" align="right">
        <el-button type="primary" @click="submit()" 
        style="margin-right: 5px; background-color: #67C23A !important;">
        <i class="el-icon-circle-check"></i> {{ $t("Save") }}</el-button>
      </el-col>
    </el-row>
      </el-main>
    </el-container>

  </div>
</template>

<script src="./warning-rules.ts" />

<style lang="scss">
.warning-rules {

  // padding: 0px 0px 0 10px;
  .el-header {
    background-color: #b3c0d1;
    color: #333;
    // line-height: 60px;
  }

  .el-menu {
    color: #333;
    background-color: white;

    // padding-left: 10px;
    h5 {
      padding-left: 40px;
    }

    .el-menu-item {
      padding-left: 10px !important;
      border-bottom: 1px solid #ecf5ff;

      span {
        min-width: 20px;
      }
    }

    .el-menu-item.is-focus {
      color: unset;
      background-color: #b5d8ff !important;
      font-weight: bold;
    }

    .el-menu-item.un-focus {
      color: unset;
      background-color: transparent !important;
      font-weight: unset;
    }
  }

  .form-container {
    padding: 0;
  }

  .el-form {
    background-color: white;
    padding: 10px;
  }

  .el-main {
    padding: 0 0 0 10px;
  }

  .text-danger {
    color: red;
  }

  .child-form-item {
    margin-bottom: unset;
  }

  .upload-demo {
    display: inline-flex;

    li {
      padding: 3.5px 10px;
      border-radius: 30px;
      margin-top: unset;

      .el-icon-close {
        top: 10px;
        right: 10px;
      }
    }

    button {
      margin-right: 20px;
    }

    .el-upload-list__item-status-label {
      top: 5px;
      right: 10px;
    }
  }

  .file-item {
    .name {
      background-color: #f5f7fa;
      width: 100%;
    }
  }
}
</style>
