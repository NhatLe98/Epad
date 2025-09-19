import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";
import { filter } from 'vue/types/umd';

@Component({
  name: "AttendanceLogTimesBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class AttendanceLogTimesBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  @Prop() chartId: any;
  id = "attendance-log-times-bar-chart";
  @Prop() name;
  @Prop() dataConfig: any;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = true;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;

  isSafe = true;
  isDanger = false;

  isLeaveDay(dateString) {
    const leaveDays = this.dataConfig.leaveDay;
    const [day, month, year] = dateString.split('/').map(Number);
    const date = new Date(year, month - 1, day);
    const dayOfWeek = date.getDay();
    return leaveDays.includes(dayOfWeek);
  }

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
    attendanceLogApi.GetLogLast7Days().then((res: any) => {
      //console.log(res)
      if(res && res.data){
        this.logData = res.data;
        const chartLabels = [];
        const dataSetAttendanceLogs = {
          dataSetId: "attendanceLogs",
          dataIds: [],
          dataNames: this.$t('AttendanceLogs'),
          dataValues: [],
          color: "blue",
          valuesColor: "orange",
        };

        let entries = Object.entries(res.data);
        entries.map(([key, val], index) => {
          chartLabels.push(key);
          dataSetAttendanceLogs.dataIds.push(key);
          dataSetAttendanceLogs.dataValues
            .push((val as any)?.length ?? 0);
        });

        let isSafe = true;
        let isDanger = false;
        if(this.dataConfig){
          const leaveDayIndexes = dataSetAttendanceLogs.dataIds.reduce((indices, date, index) => {
            if (this.isLeaveDay(date)) {
              indices.push(index);
            }
            return indices;
          }, []);
  
          const filterValues = Misc.cloneData(dataSetAttendanceLogs.dataValues.filter((_, index) => !leaveDayIndexes.includes(index)));

          if(filterValues && filterValues.length > 0){
            for(let i = 0; i < filterValues.length; i++){
              if(i == 0 || i == (filterValues.length - 1)) {
                continue;
              }
              let comparePreviousAndCurrent = 0;
              let compareCurrentAndNext = 0;
  
              if(filterValues[i - 1] == 0 && filterValues[i] == 0){
                comparePreviousAndCurrent = 0;
              }else if(filterValues[i - 1] == 0){
                // comparePreviousAndCurrent = Math.abs(filterValues[i - 1] / filterValues[i]);
                comparePreviousAndCurrent = 1;
              }else if(filterValues[i] == 0){
                // comparePreviousAndCurrent = Math.abs(filterValues[i] / filterValues[i - 1]);
                comparePreviousAndCurrent = 1;
              }else{
                comparePreviousAndCurrent = Math.abs(filterValues[i - 1] / filterValues[i]);
              }

              if(filterValues[1] == 0 && filterValues[i + 1] == 0){
                compareCurrentAndNext = 0;
              }else if(filterValues[i] == 0){
                // compareCurrentAndNext = Math.abs(filterValues[i] / filterValues[i + 1]);
                compareCurrentAndNext = 1;
              }else if(filterValues[i + 1] == 0){
                // compareCurrentAndNext = Math.abs(filterValues[i + 1] / filterValues[i]);
                compareCurrentAndNext = 1;
              }else{
                compareCurrentAndNext = Math.abs(filterValues[i] / filterValues[i + 1]);
              }
  
              if((comparePreviousAndCurrent > (this.dataConfig.safeDifferenceLogPercent / 100) 
                && compareCurrentAndNext > (this.dataConfig.safeDifferenceLogPercent / 100))){
                  isSafe = false;
              }
              if((comparePreviousAndCurrent > (this.dataConfig.dangerDifferenceLogPercent / 100) 
                && compareCurrentAndNext > (this.dataConfig.dangerDifferenceLogPercent / 100))){
                  isDanger = true;
                  isSafe = false;
              }
            }
          }
          this.isSafe = isSafe;
          this.isDanger = isDanger;
        }

        this.$emit("chartLevelBackgroundColor", this.chartId, this.isSafe, this.isDanger);

        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetAttendanceLogs,
        ];

        //console.log(this.chartLabels)
        //console.log(this.chartSets)
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
      this.explodeData = Misc.cloneData(this.logData[chartClickedData.dataId]);
      if(this.explodeData && this.explodeData.length > 0){
        this.explodeData.forEach((element, index) => {
          element.VerifyMode = this.$t(element.VerifyMode).toString();
          element.InOutMode = this.$t(element.InOutMode).toString();
        });
      }
      console.log(this.logData)
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
