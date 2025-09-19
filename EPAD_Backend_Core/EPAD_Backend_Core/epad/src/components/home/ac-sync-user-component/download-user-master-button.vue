<template>
  <div v-if="authModes.length > 0">
    <el-button
      type="primary"
      class="smallbutton el-button__loading"
      size="small"
      :loading="disabledDownloadUserButton"
      @click="onButtonDownloadClick"
      >{{ $t("DownloadUser") }}</el-button
    >
    <el-dialog
      :title="$t('SynchUserOnDevice')"
      :visible.sync="showDialogDownloadUser"
      :before-close="cancelDialog"
      custom-class="customdialog"
    >
      <el-form class="user-sync-form">
        <div v-if="isOverwriteUserMaster" class="warning-message">
          <i
            style="font-weight: bold; font-size: larger; color: orange"
            class="el-icon-warning-outline"
          />
          <span>{{ $t("SynchUserMasterHint") }}</span>
        </div>
        <el-form-item>
          <el-radio-group class="radio-group" v-model="isOverwriteUserMaster">
            <el-radio :label="false">
              {{ $t("SyncNotOverwriteUserMaster") }}</el-radio
            >
            <el-radio :label="true">
              {{ $t("SyncOverwriteUserMaster") }}</el-radio
            >
          </el-radio-group>
        </el-form-item>
        <el-form-item class="auth-modes">
          <el-select
            style="width: 100%"
            reserve-keyword
            size="large"
            v-model="selectedAuthModes"
            clearable
            multiple
            :placeholder="`${$t('SelectAuthenMode')} (${$t(
              'DefaultSelectAll'
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
        <div style="text-align: left; font-weight: bold; margin-bottom: 8px">
          {{ $t("UpdateDataFor") }}
          <span v-if="listEmployeeATID.length">
            {{ `${listEmployeeATID.length} ${$t("User")}`.toLowerCase() }}
          </span>
        </div>
        <el-form-item>
          <el-radio-group
            class="radio-group"
            v-model="isUpdateAllUser"
            v-if="!listEmployeeATID.length"
          >
            <el-radio :label="true">{{
              $t("TargetDownloadUser.AllUser")
            }}</el-radio>
            <el-radio :label="false" style="text-align: left">
              <span>
                {{ $t("TargetDownloadUser.NewUsers") }}
              </span>
              <div style="margin-top: 5px">
                {{ `(${$t("UserNotExistOnDatabase")})` }}
              </div>
            </el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button class="btnCancel" @click="showDialogDownloadUser = false">
          {{ $t("Cancel") }}
        </el-button>
        <el-button type="primary" @click="downloadUserMaster">
          {{ $t("DownloadUser") }}
        </el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script lang="ts">
import Vue, { PropType } from "vue";
import { commandApi, CommandRequest } from "@/$api/command-api";
import { deviceApi } from "@/$api/device-api";
import { UserSyncAuthMode } from "@/constant/user-sync-auth-mode";
import { TargetDownloadUser } from "@/constant/target-download-user";
import { mapState } from "vuex";

export default Vue.extend({
  props: {
    listEmployeeATID: {
      type: Array as PropType<Array<string>>,
      default: [],
    },
    listSelectedMachineSerialNumber: {
      type: Array as PropType<Array<string>>,
      required: true,
    },
  },
  computed: {
    ...mapState("Application", ["screenPrivilegeList"]),
    ...mapState("User", ["isAdmin"]),
  },
  data() {
    const commandRequest: CommandRequest = {
      Action: "",
      ListSerial: [],
      ListUser: [],
      FromTime: null,
      ToTime: null,
      Privilege: 0,
      AuthenMode: [],
      IsOverwriteData: false,
    };
    return {
      disabledDownloadUserButton: false,
      showDialogDownloadUser: false,
      isOverwriteUserMaster: false,
      isUpdateAllUser: true,
      selectedAuthModes: [],
      authModes: [
        {
          value: UserSyncAuthMode.Password,
          label: "AuthenPassword",
        },
        {
          value: UserSyncAuthMode.CardNumber,
          label: "AuthenCardNumber",
        },
        {
          value: UserSyncAuthMode.Finger,
          label: "AuthenFinger",
        },
        {
          value: UserSyncAuthMode.Face,
          label: "AuthenFace",
        },
      ],
      commandRequest,
      listIPAddress: null,
    };
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
  methods: {
    onButtonDownloadClick() {
      if (!this.listSelectedMachineSerialNumber.length) {
        this.$alert(
          this.$t("PleaseSelectMachine").toString(),
          this.$t("Notify").toString(),
          { type: "warning" }
        );
      } else {
        this.showDialogDownloadUser = true;
      }
    },
     async getListIPAddress(serialNumbers: string) {
        const response = await deviceApi.GetIPAddressBySerialNumbers(serialNumbers);
        this.listIPAddress = response.data;
        return response.data;
    },
    cancelDialog() {
      this.showDialogDownloadUser = false;
    },
    async downloadUserMaster() {
      var self = this;
      var message = "";
      this.commandRequest.Action = "Download user on machine";

      this.commandRequest.IsOverwriteData = this.isOverwriteUserMaster;
      this.commandRequest.ListUser = [...this.listEmployeeATID];
      this.commandRequest.ListSerial = [
        ...this.listSelectedMachineSerialNumber,
      ];
      this.showDialogDownloadUser = false;

      this.disabledDownloadUserButton = true;
      commandApi
        .downloadUserMaster({
          AuthModes:
            this.selectedAuthModes.length > 0
              ? this.selectedAuthModes
              : this.authModes.map((x) => x.value),
          SerialNumbers: this.listSelectedMachineSerialNumber,
          IsOverwriteData: this.isOverwriteUserMaster,
          TargetDownloadUser: this.isUpdateAllUser
            ? TargetDownloadUser.AllUser
            : TargetDownloadUser.NewUsers,
          EmployeeATIDs: this.listEmployeeATID,
        })
        .then(() => {
          this.getListIPAddress(this.listSelectedMachineSerialNumber.toString()).then(() => {
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
        
        })
        .finally(() => {
          this.disabledDownloadUserButton = false;
        });
    },
  },
});
</script>

<style>
.el-input__inner,
.user-sync-form .el-form-item__content {
  width: 100%;
}
.el-dialog__header {
  text-align: left;
}
</style>
<style scoped lang="scss">
.user-sync-form {
  margin-top: 16px;
  display: flex;
  justify-content: center;
  align-items: center;
  flex-direction: column;
  > div {
    width: 100%;
  }

  .radio-group {
    display: flex;
    margin-left: 16px;
  }
  .auth-modes,
  .radio-group {
    margin-right: 0;
  }
  .warning-message {
    word-break: keep-all;
    font-weight: bold;
    text-align: left;
    flex-direction: column;
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: baseline;
    gap: 8px;
    line-height: 1.2rem;
    margin-bottom: 8px;
  }
}
</style>
