<template>
  <div>
    <el-dialog
      :title="printer ? $t('Edit') : $t('Insert')"
      custom-class="customdialog editable-printer__dialog"
      :visible.sync="kShowDialog"
      :before-close="beforeCloseDialog"
      :close-on-click-modal="false"
    >
      <el-form
        class="formScroll"
        :model="kPrinter"
        :rules="rules"
        ref="form"
        label-width="168px"
        label-position="top"
        @keyup.enter.native="submit"
      >
        <el-form-item :label="$t('Name')" prop="Name">
          <el-input v-model="kPrinter.Name"></el-input>
        </el-form-item>
        <el-form-item :label="$t('IPAddress')" prop="IPAddress">
          <el-input v-model="kPrinter.IPAddress"></el-input>
        </el-form-item>
        <el-form-item :label="$t('Port')" prop="Port">
          <el-input v-model="kPrinter.Port"></el-input>
        </el-form-item>
        <el-form-item :label="$t('SerialNumber')" prop="SerialNumber">
          <el-input v-model="kPrinter.SerialNumber" :disabled="printer" ></el-input>
        </el-form-item>
        <el-form-item :label="$t('Model')" prop="PrinterModel">
          <el-input v-model="kPrinter.PrinterModel"></el-input>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button class="btnCancel" @click="cancelDialog">
          {{ $t("Cancel") }}
        </el-button>
        <el-button class="btnOK" type="primary" @click="submit">
          {{ $t("OK") }}
        </el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script lang="ts">
import Vue, { PropType } from "vue";
import { IC_Printer } from "@/models/ic-printer";
import { printerApi } from "@/$api/printer-api";
import { isIPAddress, isPort } from "@/utils/validation-utils";

export default Vue.extend({
  props: {
    printer: {
      type: Object as PropType<IC_Printer>,
      required: false,
    },
    showDialog: {
      type: Boolean,
      default: false,
    },
  },
  watch: {
    kShowDialog() {
      this.$emit("update:showDialog", this.kShowDialog);
    },
    showDialog() {
      this.kShowDialog = this.showDialog;
      (this.$refs.form as any).resetFields();
      if (this.kShowDialog) {
        this.initializePrinter();
      }
    },
    printer() {
      this.initializePrinter();
    },
  },
  data() {
    const rules = {
      Name: [
        {
          required: true,
          message: this.$t("PleaseInputName"),
          trigger: "blur",
        },
      ],
      IPAddress: [
        {
          required: true,
          message: this.$t("PleaseInputIpAddress"),
          trigger: "blur",
        },
        {
          message: this.$t("IPAddressIsInvalid"),
          validator: (rule, value: string, callback) => {
            if (!isIPAddress(value)) {
              callback(new Error());
            }
            callback();
          },
          trigger: "change",
        },
      ],
      Port: [
        {
          required: true,
          message: this.$t("PleaseInputPort"),
          trigger: "blur",
        },
        {
          message: this.$t("PortIsInvalid"),
          validator: (rule, value: string, callback) => {
            if (!isPort(value)) {
              callback(new Error());
            }
            callback();
          },
          trigger: "change",
        },
      ],
      SerialNumber: [
        {
          required: true,
          message: this.$t("PleaseInputSerialNumber"),
          trigger: "blur",
        },
      ],
    };
    const kPrinter = {
      Name: "",
      SerialNumber: "",
      IPAddress: "",
      PrinterModel: "",
      Port: "",
    };
    return {
      kShowDialog: false,
      rules,
      kPrinter,
    };
  },
  methods: {
    submit() {
      (this.$refs.form as any).validate(async (valid) => {
        if (valid == false) {
          return;
        }

        const isEdit = Boolean(this.printer);
        if (isEdit) {
          printerApi
            .updatePrinterAsync({
              Index: this.printer.Index,
              Port: +this.kPrinter.Port,
              Name: this.kPrinter.Name,
              SerialNumber: this.kPrinter.SerialNumber,
              IPAddress: this.kPrinter.IPAddress,
              PrinterModel: this.kPrinter.PrinterModel,
            })
            .then(() => {
              this.$saveSuccess();
              this.$emit("refreshData");
              this.kShowDialog = false;
            })
            .catch((res) => {
              console.log(res);
            });
        } else {
          printerApi
            .createPrinterAsync({
              ...this.kPrinter,
              Port: +this.kPrinter.Port,
            })
            .then(() => {
              this.$saveSuccess();
              this.$emit("refreshData");
              this.kShowDialog = false;
            })
            .catch((res) => {
              console.log(res);
            });
        }
      });
    },
    confirmDialog() {
      this.kShowDialog = false;
    },
    cancelDialog() {
      this.kShowDialog = false;
    },
    beforeCloseDialog() {
      this.kShowDialog = false;
    },
    initializePrinter() {
      if (this.printer) {
        this.kPrinter = {
          ...this.printer,
          Port: this.printer.Port.toString(),
        };
      } else {
        this.resetData();
      }
    },
    resetData() {
      this.kPrinter = {
        Name: "",
        SerialNumber: "",
        IPAddress: "",
        PrinterModel: "",
        Port: "",
      };
    },
  },
});
</script>
<style>
.editable-printer__dialog {
  width: 600px;
}
</style>