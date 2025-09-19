import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { deviceHistoryApi } from '@/$api/device-history-api';
import { attendanceLogApi } from "@/$api/attendance-log-api";
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "InOutLogByDeviceBarChart",
  components: { CustomLegendBarChart },
})
export default class InOutLogByDeviceBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "in-out-log-by-device-bar-chart";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = true;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;

  beforeMount(){
    this.explodeColumns = [
      {
        prop: 'EmployeeATID',
        label: this.$t('EmployeeATID'),
      },
      {
        prop: 'EmployeeCode',
        label: this.$t('EmployeeCode'),
      },
      {
        prop: 'FullName',
        label: this.$t('FullName'),
      },
      {
        prop: 'DeviceName',
        label: this.$t('DeviceName'),
      },
      {
        prop: 'DoorName',
        label: this.$t('Door'),
      },
      {
        prop: 'TimeString',
        label: this.$t('TimeString'),
      },
      {
        prop: 'InOutMode',
        label: this.$t('InOutMode'),
      }
      ,
      {
        prop: 'VerifyMode',
        label: this.$t('VerifyMode'),
      }
    ];

    attendanceLogApi.GetLogsByDoor().then((res: any) => {
      //console.log(res)

      const chartLabels = [];
      const dataSetInLogs = {
        dataSetId: "In",
        dataIds: [],
        dataNames: this.$t('In'),
        dataValues: [],
        color: "blue",
        valuesColor: "red",
      };
      const dataSetOutLogs = {
        dataSetId: "Out",
        dataIds: [],
        dataNames: this.$t('Out'),
        dataValues: [],
        color: "red",
        valuesColor: "blue",
      };
      const dataSetRemainInLogs = {
        dataSetId: "RemainIn",
        dataIds: [],
        dataNames: this.$t('RemainIn'),
        dataValues: [],
        color: "orange",
        valuesColor: "lawngreen",
      };

      if(res.data){
        this.logData = res.data;
        if(this.logData.Item1 && this.logData.Item1.length > 0){
          this.logData.Item1 = this.logData.Item1.filter(x => x.DoorIndex != 0);
        }
        if(this.logData.Item2 && this.logData.Item2.length > 0){
          this.logData.Item2 = this.logData.Item2.filter(x => x.DoorIndex != 0);
        }
        if(this.logData.Item3 && this.logData.Item3.length > 0){
          this.logData.Item3 = this.logData.Item3.filter(x => x.DoorIndex != 0);
        }
        //console.log(this.logData)

        const inDevice = this.logData.Item1.map(item => 
          ({SerialNumber: item.SerialNumber, DeviceName: (item.DeviceName && item.DeviceName != "") 
          ? item.DeviceName : item.SerialNumber}));
        const outDevice = this.logData.Item2.map(item => 
          ({SerialNumber: item.SerialNumber, DeviceName: (item.DeviceName && item.DeviceName != "") 
          ? item.DeviceName : item.SerialNumber})); 
        const remainDevice = this.logData.Item3.map(item => 
          ({SerialNumber: item.SerialNumber, DeviceName: (item.DeviceName && item.DeviceName != "") 
          ? item.DeviceName : item.SerialNumber}));

        const allDevice = inDevice.concat(outDevice, remainDevice);

        const uniqueMap = new Map();
        for (const val of allDevice) {
          if (!uniqueMap.has(val.SerialNumber)) {
            uniqueMap.set(val.SerialNumber, val);
          }
        }

        const devices = Array.from(uniqueMap.values());
        console.log(devices)

        if(devices && devices.length > 0)
        {
          devices.forEach((element, index) => {
            //console.log(element)
            let deviceName = element.DeviceName
            if (!chartLabels.includes(deviceName)) {
              chartLabels.push(deviceName);
            }
            dataSetInLogs.dataIds.push(element.SerialNumber);
            dataSetOutLogs.dataIds.push(element.SerialNumber);
            dataSetRemainInLogs.dataIds.push(element.SerialNumber);

            dataSetInLogs.dataValues.push(this.logData.Item1.filter(x => x.SerialNumber == element.SerialNumber)?.length ?? 0);
            dataSetOutLogs.dataValues.push(this.logData.Item2.filter(x => x.SerialNumber == element.SerialNumber)?.length ?? 0);
            dataSetRemainInLogs.dataValues.push(this.logData.Item3.filter(x => x.SerialNumber == element.SerialNumber)?.length ?? 0);
          });
        }

        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetInLogs,
          dataSetOutLogs,
          // dataSetRemainInLogs,
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
      if(chartClickedData.dataSetId == "In"){
        this.explodeData = Misc.cloneData(this.logData.Item1.filter(x => x.SerialNumber == chartClickedData.dataId));
      }else if(chartClickedData.dataSetId == "Out"){
        this.explodeData = Misc.cloneData(this.logData.Item2.filter(x => x.SerialNumber == chartClickedData.dataId));
      }else{
        this.explodeData = Misc.cloneData(this.logData.Item3.filter(x => x.SerialNumber == chartClickedData.dataId));
      }
      if(this.explodeData && this.explodeData.length > 0){
        this.explodeData.forEach((element, index) => {
          element.VerifyMode = this.$t(element.VerifyMode).toString();
          element.InOutMode = this.$t(element.InOutMode).toString();
        });
      }
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
