<template>
  <div v-if="authModes.length > 0">
    <el-button
      class="classRight el-button__loading"
      id="btnFunction"
      type="primary"
      :loading="disabledDeleteOnMachineButton"
      round
      @click="onButtonDeleteClick"
      >{{ $t("DeleteOnMachine") }}</el-button
    >
    <el-dialog
      :title="$t('DeleteOnMachine')"
      :visible.sync="showDialog"
      :before-close="() => (showDialog = false)"
      custom-class="customdialog"
      :close-on-click-modal="false"
    >
      <el-form class="user-sync-form">
        <h4 v-if="listEmployeeATID.length">
          {{
            $t("AutoSyncUser.DeleteUserById.Title", {
              numberOfUser: listEmployeeATID.length,
              numberOfMachine: listSerialNumber.length,
            })
          }}
        </h4>
        <h4 v-else>
          {{
            $t("AutoSyncUser.DeleteAllUser.Title", {
              numberOfMachine: listSerialNumber.length,
            })
          }}
        </h4>
        <el-form-item class="auth-modes">
          <el-select
            style="width: 100%"
            reserve-keyword
            size="large"
            v-model="selectedAuthModes"
            clearable
            multiple
            :placeholder="`${$t('SelectAuthenMode')} (${$t(
              'DefaultDeleteUserOnMachine'
            )})`"
          >
            <el-option
              :label="
                $t(
                  selectedAuthModes.length < authModes.length
                    ? 'SelectAll'
                    : 'DeselectAll'
                )
              "
              value="SelectAll"
            />
            <el-option
              v-for="item in authModes"
              :key="item.value"
              :label="$t(item.label)"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button class="btnCancel" @click="showDialog = false">
          {{ $t("Cancel") }}
        </el-button>
        <el-button type="primary" @click="deleteOnMachine" v-if="listEmployeeATID.length > 0">
          {{ $t("DeleteOnMachine") }}
        </el-button>
        <el-popover v-if="!listEmployeeATID || listEmployeeATID.length == 0"
          placement="top"
          v-model="openingConfirmDeletePopup">
          <span>{{ $t('MSG_Confirm') }}</span>
          <span style="cursor: pointer; float: right;" @click="CloseDeleteAttendanceLogForm()">X</span>
          <p style="margin-top: 15px;"><i class="el-icon-warning" style="color: orange; font-size: 20px;"></i> {{ $t('DataWillBePernamentDelete_PleaseInputPassword') }}</p>
          <el-input v-model="confirmDeletePassword" type="password"></el-input>
          <div style="text-align: right; margin-top: 5px">
            <el-button size="mini" 
              style="margin-right: 5px;" class="btnCancel"
              @click="CloseDeleteAttendanceLogForm">{{$t('MSG_No')}}</el-button>
              <el-button type="primary" @click="deleteOnMachine">
                {{ $t("DeleteOnMachine") }}
              </el-button>
          </div>
          <el-button class="btnOK" type="primary" slot="reference">{{ $t("OK") }}</el-button>
        </el-popover>
      </span>
    </el-dialog>
  </div>
</template>
<script lang="ts">
import { commandApi } from "@/$api/command-api";
import { deviceApi } from "@/$api/device-api";
import { userAccountApi } from "@/$api/user-account-api";
import Vue, { PropType } from "vue";

export default Vue.extend({
  name: "DeleteUserOnMachineButton",
  props: {
    listSerialNumber: {
      type: Array as PropType<Array<string>>,
      required: true,
    },
    listEmployeeATID: {
      type: Array as PropType<Array<string>>,
      default: [],
    },
  },
  watch: {
    selectedAuthModes(newVal) {
      if (newVal.find((x) => x === "SelectAll")) {
        if (this.selectedAuthModes.length <= this.authModes.length) {
          this.selectedAuthModes = this.authModes.map((x) => x.value);
        } else {
          this.selectedAuthModes.length = 0;
        }
      }
    },
  },
  data() {
    return {
      disabledDeleteOnMachineButton: false,
      showDialog: false,
      selectedAuthModes: [],
      listIPAddress: null,
      confirmDeletePassword: '',
      openingConfirmDeletePopup: false,
      authModes: [
        {
          value: "Finger",
          label: "AuthenFinger",
        },
        {
          value: "CardNumber",
          label: "AuthenCardNumber",
        },
        {
          value: "Password",
          label: "AuthenPassword",
        },
        {
          value: "Face",
          label: "AuthenFace",
        },
      ],
    };
  },
  methods: {
    CloseDeleteAttendanceLogForm(){
        this.openingConfirmDeletePopup = false; 
        this.confirmDeletePassword = '';
    },
    async getListIPAddress(serialNumbers: string) {
        const response = await deviceApi.GetIPAddressBySerialNumbers(serialNumbers);
        this.listIPAddress = response.data;
        return response.data;
    },
    async deleteOnMachine() {
      var self = this;
      var message = "";
      const action = "Delete user on machine";
      this.showDialog = false;
      this.disabledDeleteOnMachineButton = true;
      try {
        if (this.listEmployeeATID.length === 0) {
          await userAccountApi.CheckValidPassword(localStorage.getItem('user'), this.confirmDeletePassword).then(async (res) => {
            this.confirmDeletePassword = '';
            if (!res.data) {
              this.$alertSaveError(null, null, this.$t('Warning').toString(), this.$t('WrongPassword').toString());
              return;
            }else{
              await commandApi.DeleteAllUser({
                Action: action,
                ListSerial: this.listSerialNumber,
                ListUser: this.listEmployeeATID,
                AuthenMode: this.selectedAuthModes,
              });
              this.getListIPAddress(this.listSerialNumber.toString()).then(() => {
                  this.listIPAddress.forEach(element => {
                      message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
                  });
                  message = `<p class="notify-content">${message}</p>`;
                  this.$notify({
                      type: 'success',
                      title: 'Thông báo từ thiết bị',
                      dangerouslyUseHTMLString: true,
                      message: message,
                      customClass: 'notify-content',
                      duration: 8000
                  });
              })
            }
          })
          .finally(() => {
            this.openingConfirmDeletePopup = false;
          });
        } else {
          await commandApi.DeleteUserById({
            ListSerial: this.listSerialNumber,
            ListUser: this.listEmployeeATID,
            Action: action,
            AuthenMode: this.selectedAuthModes,
          }).finally(() => {
            this.openingConfirmDeletePopup = false;
          });;
          this.getListIPAddress(this.listSerialNumber.toString()).then(() => {
              this.listIPAddress.forEach(element => {
                  message += element.toString() + ": " + self.$t("SendRequestSuccess").toString() + "<br/>";
              });
              message = `<p class="notify-content">${message}</p>`;
              this.$notify({
                  type: 'success',
                  title: 'Thông báo từ thiết bị',
                  dangerouslyUseHTMLString: true,
                  message: message,
                  customClass: 'notify-content',
                  duration: 8000
              });
          })
        }
        // this.listSerialNumber.forEach((element) => {
        //   message +=
        //     element.toString() +
        //     ": " +
        //     self.$t("SendRequestSuccess").toString() +
        //     "<br/>";
        // });
        // message = `<p class="notify-content">${message}</p>`;
        // this.$notify({
        //   type: "success",
        //   title: this.$t("NotificationFromDevice").toString(),
        //   dangerouslyUseHTMLString: true,
        //   message: message,
        //   customClass: "notify-content",
        //   duration: 8000,
        // });
      } catch (err) {
        this.$alertSaveError(null, err);
      } finally {
        setTimeout(() => {
          this.disabledDeleteOnMachineButton = false;
        }, 600);
      }
    },
    onButtonDeleteClick() {
      if (!this.listSerialNumber.length) {
        this.$alert(
          this.$t("PleaseSelectMachine").toString(),
          this.$t("Notify").toString(),
          { type: "warning" }
        );
      } else {
        this.showDialog = true;
      }
    },
  },
});
</script>