<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("AssignPrivilegeMachineRealtime") }}</span>
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
            :visible.sync="showDialog"
            :before-close="Cancel"
            :close-on-click-modal="false"
            class="dialog-privilege-machine-realtime"
            custom-class="customdialog"
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
                :label="$t('Account')"
                @click.native="focus('ListUserName')"
                prop="ListUserName"
              >
              <el-select
                        ref="ListUserName"
                        filterable
                        multiple
                        collapse-tags
                        clearable
                        :placeholder="$t('SelectAccount')"
                        v-model="ruleForm.ListUserName"
                        :disabled="isEdit"
                      >
                        <el-option
                          v-for="item in listAllUserAccount"
                          :key="item.index"
                          :label="$t(item.name)"
                          :value="item.index"
                        ></el-option>
                      </el-select>
              </el-form-item>
              <el-form-item
                      :label="$t('PrivilegeGroup')"
                      @click.native="focus('PrivilegeGroup')"
                      prop="PrivilegeGroup"
                    >
                      <el-select
                        ref="PrivilegeGroup"
                        :placeholder="$t('PrivilegeGroup')"
                        filterable
                        reserve-keyword
                        collapse-tags
                        v-model="ruleForm.PrivilegeGroup"
                      >
                        <el-option
                          v-for="item in privilegeGroup"
                          :key="item.index"
                          :label="$t(item.value)"
                          :value="item.index"
                        ></el-option>
                      </el-select>
                    </el-form-item>
              <el-form-item 
                v-if="ruleForm.PrivilegeGroup == 1"
                      :label="$t('GroupDevice')"
                      @click.native="focus('ListGroupDeviceIndex')"
                      prop="ListGroupDeviceIndex"
                    >
                      <el-select
                        ref="ListGroupDeviceIndex"
                        :placeholder="$t('SelectGroupDevice')"
                        filterable
                        multiple
                        reserve-keyword
                  collapse-tags
                        clearable
                        v-model="ruleForm.ListGroupDeviceIndex"
                      >
                        <el-option
                          v-for="item in listAllGroupDevice"
                          :key="item.index"
                          :label="$t(item.device)"
                          :value="item.index"
                        ></el-option>
                      </el-select>
                    </el-form-item>
              <el-form-item
                v-if="ruleForm.PrivilegeGroup == 2"
                :label="$t('DeviceModule')"
                @click.native="focus('ListDeviceModule')"
                prop="ListDeviceModule"
              >
                <el-select
                  ref="ListDeviceModule"
                  :placeholder="$t('SelectDeviceModule')"
                  filterable
                  multiple
                  reserve-keyword
                  collapse-tags
                  clearable
                  v-model="ruleForm.ListDeviceModule"
                >
                  <el-option
                    v-for="item in deviceModules"
                    :key="item.index"
                    :label="$t(item.value)"
                    :value="item.index"
                  ></el-option>
                </el-select>
              </el-form-item>
              <el-form-item
                :label="$t('PrivilegeDeviceName')"
                @click.native="focus('ListDeviceSerial')"
                prop="ListDeviceSerial"
              >
                <el-select
                  ref="ListDeviceSerial"
                  :placeholder="$t('SelectDevice')"
                  filterable
                  multiple
                  reserve-keyword
                  collapse-tags
                  clearable
                  v-model="ruleForm.ListDeviceSerial"
                >
                <el-option
                            :key="allDevice"
                            :label="$t(allDevice)"
                            :value="allDevice"
                          ></el-option>
                  <el-option
                    v-for="item in listDevice"
                    :key="item.SerialNumber"
                    :label="$t(item.AliasName)"
                    :value="item.SerialNumber"
                  ></el-option>
                </el-select>
              </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
              <el-button class="btnOK" type="primary" @click="Submit">{{
                $t("OK")
              }}</el-button>
              <el-button class="btnCancel" @click="Cancel">{{
                $t("Cancel")
              }}</el-button>
            </span>
          </el-dialog>
        </div>
        <div>
          <data-table-function-component 
            @insert="Insert"
            @edit="Edit"
            @delete="Delete"
            :showButtonColumConfig="true" :gridColumnConfig.sync="columns"s
          ></data-table-function-component>
          <data-table-component
            :get-data="getData"
            ref="table"
            :columns="columns"
            :selectedRows.sync="rowsObj"
            :isShowPageSize="true"
          ></data-table-component>
        </div>
      </el-main>
    </el-container>
  </div>
</template>
<script src="./assign-privilege-machine-realtime-component.ts"></script>
<style lang="scss">

</style>