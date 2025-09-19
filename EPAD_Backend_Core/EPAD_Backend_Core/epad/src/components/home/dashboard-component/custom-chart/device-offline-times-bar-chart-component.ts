import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { deviceHistoryApi } from '@/$api/device-history-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";
@Component({
  name: "DeviceOfflineTimesBarChart",
  components: { CustomLegendBarChart },
})
export default class DeviceOfflineTimesBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  @Prop() chartId: any;
  id = "device-offline-times-bar-chart";
  @Prop() name;
  @Prop() dataConfig: any;
  deviceData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = true;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;

  isSafe = true;
  isDanger = false;

  customRound(value) {
    let temp = value * 100;
    let decimal = temp % 1;
    temp = decimal >= 0.5 ? Math.ceil(temp) : Math.floor(temp);
    return temp / 100;
  }

  checkConsecutiveTrueDates(arr) {
    // Sort the array by date
    arr.sort((a, b) => new Date(a.DateTimeFormat).getTime() - new Date(b.DateTimeFormat).getTime());
    
    for (let i = 0; i < arr.length - 1; i++) {
      const currentDate = new Date(new Date(arr[i].DateTimeFormat).setHours(0, 0, 0, 0));
      const nextDate = new Date(new Date(arr[i + 1].DateTimeFormat).setHours(0, 0, 0, 0));
      
      if (
        (nextDate.getTime() - currentDate.getTime()) / (1000 * 60 * 60 * 24) === 1
      ) {
        return true;
      }
    }
  
    return false;
  }

  beforeMount(){
    this.explodeColumns = [
      {
          prop: 'SerialNumber',
          label: this.$t('SerialNumber'),
      },
      {
          prop: 'DeviceName',
          label: this.$t('DeviceName'),
      },
      {
          prop: 'Date',
          label: this.$t('Date'),
      }
    ];

    deviceHistoryApi.GetDeviceHistoryLast7Days().then((res: any) => {
      // //console.log(res)
      if(res.data && res.data.data && res.data.data.length > 0){
        this.deviceData = res.data.data;

        const chartLabels = [];
        const dataSetDeviceOfflineTimes = {
          dataSetId: "Offline",
          dataIds: [],
          dataNames: this.$t('Offline'),
          dataValues: [],
          color: "red",
          valuesColor: "blue",
        };

        const deviceSerials = this.deviceData.map(x => x.SerialNumber);
        //console.log(deviceSerials)

        if(deviceSerials && deviceSerials.length > 0)
        {
          const groupDeviceOfflineTimes = this.deviceData.reduce((grouped, emp) => {
            const key = emp.SerialNumber;

            if (!grouped[key]) {
              grouped[key] = [];
            }

            grouped[key].push(emp);

            return grouped;
          }, {} as { [key: string]: [] });

          let countDeviceOffline = 0;
          let anyDeviceContinousOffline = false;
          deviceSerials.forEach((element, index) => {
            //console.log(element)
            let deviceName = element;
            const device = this.deviceData.find(x => x.SerialNumber == element);
            if(device && device.DeviceName && device.DeviceName != ''){
              deviceName = device.DeviceName;
            }
            if(!chartLabels.includes(deviceName)){
              chartLabels.push(deviceName);
              dataSetDeviceOfflineTimes.dataIds
                .push(element);
              const deviceOffline = Misc.cloneData(groupDeviceOfflineTimes[element].filter(x => x.Status == "Offline"));
              dataSetDeviceOfflineTimes.dataValues
                .push(deviceOffline?.length ?? 0);
              if(deviceOffline && deviceOffline.length > 0){
                countDeviceOffline++;
                if(deviceOffline.length > 1){
                  if(this.checkConsecutiveTrueDates(deviceOffline)){
                    anyDeviceContinousOffline = true;
                  }
                }
              }
            }
          });
          if(this.dataConfig && (this.customRound(countDeviceOffline / deviceSerials.length) * 100) > 0){
            this.isSafe = false;
            if(anyDeviceContinousOffline 
              || (this.customRound(countDeviceOffline / deviceSerials.length) * 100) > this.dataConfig.warningAmountOfflinePercent){
              this.isDanger = true;
            }
          }
        }

        this.$emit("chartLevelBackgroundColor", this.chartId, this.isSafe, this.isDanger);

        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetDeviceOfflineTimes
        ];
      }
      this.$nextTick(() => {
        this.load = true;
      });
    });
  }

  textChartColor = null;
  mounted(){
    Misc.readFileAsync('static/variables/color.json').then(x => {
      if(UPDATE_UI == 'true'){
if(!UI_NAME || UI_NAME.trim().length == 0){
        this.textChartColor = x.colorText;
}else{
        this.textChartColor = x.ColorThemes[UI_NAME].colorText;
}
      }
    });
  }

  handleChartClicked(chartClickedData){
    //console.log(chartClickedData)

    if(chartClickedData){
      this.explodeData = this.deviceData.filter(x => x.SerialNumber == chartClickedData.dataId 
        && x.Status == chartClickedData.dataSetId);
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
