<template>
  <div>
    <el-dialog
      :title="$t('Notify')"
      :visible.sync="visible"
      width="20%"
      height="20%"
      center
    >
      <span
        v-if="
          services.find(
            (x) => x.restartServiceStatus === RestartServiceStatus.Waiting
          )
        "
        >{{ $t("RequestIsProcessingBySystem") }}</span
      >
      <span v-else>{{
        $t("MSG_Success") +
        ` ${
          services.filter(
            (x) => x.restartServiceStatus === RestartServiceStatus.Success
          ).length
        }/${services.length}`
      }}</span>
      <ul>
        <li v-for="service of services" :key="service.Index">
          <div
            v-if="service.restartServiceStatus === RestartServiceStatus.Waiting"
          >
            <span>
              {{ service.Name }}
            </span>
            <i class="el-icon-loading" />
          </div>

          <div v-else style="display: flex; align-items: center; gap: 4px">
            <span>
              {{ service.Name }}
            </span>
            <i
              v-if="
                service.restartServiceStatus === RestartServiceStatus.Success
              "
              class="el-notification__icon el-icon-success icon-success"
            />
            <el-tooltip v-else :content="$t('TimeoutError')">
              <i class="el-notification__icon el-icon-warning icon-warning" />
            </el-tooltip>
          </div>
        </li>
      </ul>
    </el-dialog>
  </div>
</template>
<script lang="ts">
import { configApi } from "@/$api/config-api";
import { IC_Service } from "@/$api/service-api";
import Vue, { PropType } from "vue";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { PUSH_NOTIFICATION_URL } from "@/$core/config";
import { SignalRConnection } from "@/startup/hubConnection";
enum RestartServiceStatus {
  Waiting,
  Success,
  Timeout,
}
export default Vue.extend({
  props: {
    showDialog: {
      type: Boolean,
      default: false,
    },
    listSelectedService: {
      type: Array as PropType<Array<IC_Service>>,
      default: () => [],
    },
  },
  watch: {
    visible() {
      this.$emit("update:showDialog", this.visible);
    },
    showDialog() {
      this.visible = this.showDialog;
      if (this.visible) {
        this.services = this.listSelectedService.map((s) => ({
          ...s,
          restartServiceStatus: RestartServiceStatus.Waiting,
        }));
        setTimeout(() => {
          this.services.forEach((s) => {
            if (s.restartServiceStatus === RestartServiceStatus.Waiting) {
              s.restartServiceStatus = RestartServiceStatus.Timeout;
            }
          });
        }, 30000);
      }
    },
  },
  data() {
    const connection = null;
    return {
      visible: this.showDialog,
      services: [],
      RestartServiceStatus,
      connection,
    };
  },
  mounted() {
    SignalRConnection.connection.on("onStartService", (serviceIndex: string) => {
        this.services.forEach((service) => {
          service.Index == serviceIndex &&
            (service.restartServiceStatus = RestartServiceStatus.Success);
        });
      });
   
  }
});
</script>
<style>
.icon-success,
.icon-warning {
  font-size: 14px;
  height: 14px;
  width: 14px;
}
.icon-success {
  color: #67c23a;
}

.icon-warning {
  color: #e6a23c;
}
</style>