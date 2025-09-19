<template>
  <div v-if="authModes.length > 0">
    <el-button
      class="classRight el-button__loading"
      id="btnFunction"
      type="primary"
      :loading="disabledDeleteOnMachineButton"
      round
      @click="onButtonDeleteClick"
      >{{ $t("DeleteOnDevice") }}</el-button
    >
    <el-dialog
      :title="$t('DeleteOnDevice')"
      :visible.sync="showDialog"
      :before-close="() => (showDialog = false)"
      custom-class="customdialog"
      :close-on-click-modal="false"
    >
      <el-form label-width="168px" label-position="top">
      
        <el-form-item>
          <el-radio-group v-model="isUsingArea">
            <el-radio :label="true">
              {{ $t("AccessArea") }}
            </el-radio>
            <el-radio :label="false">
              {{ $t("AccessDoor") }}
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="isUsingArea">
          <el-select
            props="selectArea"
            v-model="selectArea"
            clearable
            multiple
            :placeholder="$t('SelectArea')"
          >
            <el-option
              v-for="item in listArea"
              :key="item.value"
              :label="$t(item.label)"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item v-else>
          <el-select
            props="selectDoor"
            v-model="selectDoor"
            clearable
            multiple
            :placeholder="$t('SelectDoor')"
          >
            <el-option
              v-for="item in listDoor"
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
        <el-button type="primary" @click="deleteOnMachine">
          {{ $t("DeleteOnDevice") }}
        </el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script lang="ts">
import { commandApi } from "@/$api/command-api";
import { deviceApi } from "@/$api/device-api";
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
    listArea: {
      type: Array as PropType<Array<any>>,
      default: [],
    },
    listDoor: {
      type: Array as PropType<Array<any>>,
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
      selectedTimeZone: null,
      selectArea: [],
      selectDoor: [],
      isUsingArea: false,
      isOverwriteUserMaster: false,
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
    async getListIPAddress(serialNumbers: string) {
      const response = await deviceApi.GetIPAddressBySerialNumbers(
        serialNumbers
      );
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
        console.log(this.listEmployeeATID);
        if (this.listEmployeeATID.length === 0) {
          this.$alert(
            this.$t("PleaserSelectUser").toString(),
            this.$t("Notify").toString(),
            {
              type: "warning",
            }
          );
          return;
        } else {
         
          await commandApi.DeleteACUsers({
            ListSerial: this.listSerialNumber,
            ListUser: this.listEmployeeATID,
            Action: action,
            AuthenMode: this.selectedAuthModes,
            AreaLst : this.selectArea,
            DoorLst: this.selectDoor,
            IsUsingArea : this.isUsingArea
          });
        }
        message = self.$t("SendRequestSuccess").toString();
        message = `<p class="notify-content">${message}</p>`;
        this.$notify({
          type: "success",
          title: "Thông báo từ thiết bị",
          dangerouslyUseHTMLString: true,
          message: self.$t("SendRequestSuccess").toString(),
          customClass: "notify-content",
          duration: 8000,
        });
      } catch (err) {
        this.$alertSaveError(null, err);
      } finally {
        setTimeout(() => {
          this.disabledDeleteOnMachineButton = false;
        }, 600);
      }
    },
    onButtonDeleteClick() {
      if (this.listEmployeeATID.length === 0) {
        this.$alert(
          this.$t("PleaserSelectUser").toString(),
          this.$t("Notify").toString(),
          {
            type: "warning",
          }
        );
        return;
      } else {
        this.showDialog = true;
      }
    },
  },
});
</script>