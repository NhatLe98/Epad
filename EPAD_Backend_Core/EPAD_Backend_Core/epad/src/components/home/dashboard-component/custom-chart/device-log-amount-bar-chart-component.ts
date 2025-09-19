import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { deviceApi } from '@/$api/device-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "DeviceLogAmountBarChart",
  components: { CustomLegendBarChart },
})
export default class DeviceLogAmountBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  @Prop() chartId: any;
  id = "device-log-amount-bar-chart";
  @Prop() name;
  @Prop() dataConfig: any;
  deviceData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = false;
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

  beforeMount(){
    deviceApi.GetListDeviceInfo().then((res: any) => {
      //console.log(res)
      if(res.data && res.data.data && res.data.data.length > 0){
        this.deviceData = res.data.data;

        const chartLabels = [];
        const dataSetAttendanceLogCount = {
          dataSetId: "AttendanceLogCount",
          dataIds: [],
          dataNames: this.$t('Used'),
          dataValues: [],
          color: "blue",
          valuesColor: "yellow",
        };
        const dataSetAttendanceLogRemain = {
          dataSetId: "AttendanceLogRemain",
          dataIds: [],
          dataNames: this.$t('Remain'),
          dataValues: [],
          color: "red",
          valuesColor: "lawngreen",
        };

        let arrLogUsedPercent = [];
        res.data.data.forEach((element, index) => {
          let logUsedPercent = 0;
          var attendanceLogCapacity = element?.AttendanceLogCapacity ?? 0;
          var attendanceLogCount = attendanceLogCapacity != 0 ? (element?.AttendanceLogCount ?? 0) : 0;
          var attendanceLogRemain = attendanceLogCapacity != 0 
            ? ((attendanceLogCapacity - attendanceLogCount) < 0 
            ? 0 : attendanceLogCapacity - attendanceLogCount) : 0;

          if(attendanceLogCapacity != 0){
            logUsedPercent = this.customRound(attendanceLogCount / attendanceLogCapacity) * 100;
          }
          arrLogUsedPercent.push(logUsedPercent);
            
          chartLabels.push((element.AliasName && element.AliasName != '') ? element.AliasName : element.SerialNumber);
          dataSetAttendanceLogCount.dataIds.push(element.SerialNumber);
          dataSetAttendanceLogCount.dataValues.push(attendanceLogCount);
          dataSetAttendanceLogRemain.dataIds.push(element.SerialNumber);
          dataSetAttendanceLogRemain.dataValues.push(attendanceLogRemain);
        });

        if(this.dataConfig){
          if(arrLogUsedPercent.some(y => y > this.dataConfig.dangerLogUsedPercent)){
            this.isDanger = true;
            this.isSafe = false;
          }
          if(arrLogUsedPercent.some(y => y > this.dataConfig.safeLogUsedPercent && y <= this.dataConfig.dangerLogUsedPercent)){
            this.isSafe = false;
          }
        }
        this.$emit("chartLevelBackgroundColor", this.chartId, this.isSafe, this.isDanger);

        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetAttendanceLogCount, dataSetAttendanceLogRemain
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
    // //console.log(chartClickedData)

    if(chartClickedData){
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
