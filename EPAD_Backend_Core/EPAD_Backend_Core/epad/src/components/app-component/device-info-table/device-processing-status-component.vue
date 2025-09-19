<template>
    <el-tooltip v-if="deviceStatus" effect="light" placement="right">
        <div slot="content">
            Lệnh đang chờ: {{deviceStatus.CommandsLeft}}
            <br />
            Lệnh hiện tại: {{$t(deviceStatus.CurrentCommand)}}
            <br />
            Thời gian thực hiện: {{processingTime}}
            <br />
            Log: {{$t(`DeviceStatus.${deviceStatus.Log}`)}}
            
        </div>
        
        <el-progress type="circle" :percentage="deviceStatus.Percent" :width="40"></el-progress>
    </el-tooltip>
    <span v-else :class="{
      deviceOnline: onlineStatus == 'Online',
      deviceOffline: onlineStatus == 'Offline'
    }">{{ onlineStatus }}</span>
</template>
<script lang="ts">
import { DeviceProcessStatus } from '@/models/command-process-status';
import Vue from 'vue';
import CountTime from '@/components/app-component/count-time/count-time.vue';
import { fHMS } from '@/utils/datetime-utils';
import { SignalRConnection } from '@/startup/hubConnection';

export default Vue.extend({
  name: 'DeviceProcessingStatus',
  components: {
    CountTime,
  },
  computed: {
    onlineStatus() {
      const status = (this as any).params.data.Status;
      if (status === 'Đang xử lý') return 'Online';
      return status; 
    }
  },
  data() {
    const deviceStatus: DeviceProcessStatus = null;
    return {
      processingTime: '',
      deviceStatus,
      timeCountInterval: null,
    }
  },
  methods: {
    formatProcessingTime() {
        this.processingTime = this.deviceStatus?.Time ? fHMS(new Date(this.deviceStatus.Time), new Date()) : '';
    },
  },
  mounted() {
    const serialNumber: string = (this as any).params.data.SerialNumber;

    const makeConnection = () => SignalRConnection.connection.on(serialNumber, (message: string) => {
        this.deviceStatus = JSON.parse(message);
        if (!this.timeCountInterval) {
          this.timeCountInterval = setInterval(() => {
            this.formatProcessingTime();
          }, 1000);
        }
        this.deviceStatus.Percent > 100 && (this.deviceStatus = null);
    });

    makeConnection();
  },
})
</script>
<style>
.deviceOnline {
  color: #3BD97F !important;
  font-weight: 600 !important;
}

.deviceOffline {
  color: #5C6AFF !important;
  font-weight: 600 !important;
}
</style>