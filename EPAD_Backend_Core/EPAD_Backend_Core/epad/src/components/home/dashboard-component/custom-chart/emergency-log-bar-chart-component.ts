import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';

@Component({
  name: "EmergencyLogBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class EmergencyLogBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "emergency-log-bar-chart";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = false;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;
  totalValue = 0;

  beforeMount() {
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
      },
      {
        prop: 'VerifyMode',
        label: this.$t('VerifyMode'),
      },
      {
        prop: 'DepartmentName',
        label: this.$t('DepartmentName'),
      }
    ];
    this.getData();
  }

  clearEmergencyData(event){
    event.stopPropagation();
    attendanceLogApi.UpdateLatestEmergencyAttendance().then((res: any) => {
      setTimeout(() => {
        this.$deleteSuccess();
        this.getData();
      }, 1000);
    });
  }

  getData(){
    this.totalValue = 0;
    this.load = false;
    attendanceLogApi.GetEmergencyLog().then((res: any) => {    
      // // console.log(res)
      let groupEmployee;
      const chartSets = [];
      const chartLabels = [];
      const dataSetEmployee = {
        dataSetId: "Employee",
        dataIds: [],
        dataNames: this.$t('Employee'),
        dataValues: [],
        color: null,
        valuesColor: "white",
      };
      if (res && res.data) {

        this.chartSets = [];
        if (res.data) {
          this.logData = res.data;

          groupEmployee = this.logData.reduce((grouped, emp) => {
            const key = emp.SerialNumber.toString();
          
            if (!grouped[key]) {
              grouped[key] = [];
            }
          
            grouped[key].push(emp);
          
            return grouped;
          }, {} as { [key: string]: [] });

          Object.entries(groupEmployee).forEach((element, index) => {
            chartSets.push(element[0]);
            chartLabels.push(this.$t(element[0]));

            dataSetEmployee.dataIds
              .push(element[1]);
            dataSetEmployee.dataValues
              .push((element[1] as any)?.length ?? 0);

            this.totalValue += ((element[1] as any)?.length ?? 0);
          });
        }
        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetEmployee,
        ];

      }
      this.$nextTick(() => {
        this.load = true;
        this.$emit('dataEmergencyValues', chartSets, chartLabels, dataSetEmployee.dataValues, null);
        this.$emit('dataEmergency', this.logData);
      });
    });
  }

  reloadInterval = null;
  mounted(){
    this.reloadInterval = setInterval(() => {
      if(this.load){
        this.getData();
        this.$nextTick(() => {
          this.$forceUpdate();
        });
      }
    }, 60000); 
  }

  beforeUnmount(){
    clearInterval(this.reloadInterval);
  }

  format(percentage) {
    return '';
  }

  handleChartClicked(chartClickedData) {
    // console.log(chartClickedData)
    if (chartClickedData) {
      // const logsData = Misc.cloneData(this.logData);
      // if(chartClickedData.dataId == "Employee"){
      //   this.explodeData = logsData.Item1;
      // }else if(chartClickedData.dataId == "Customer"){
      //   this.explodeData = logsData.Item2;
      // }else if(chartClickedData.dataId == "Contractor"){
      //   this.explodeData = logsData.Item3;
      // }else if(chartClickedData.dataId == "Driver"){
      //   this.explodeData = logsData.Item4;
      // }else if(chartClickedData.dataId == "ExtraDriver"){
      //   this.explodeData = logsData.Item5;
      // }
      
      // if (this.explodeData && this.explodeData.length > 0) {
      //   this.explodeData.forEach((element, index) => {
      //     element.VerifyMode = this.$t(element.VerifyMode).toString();
      //     element.InOutMode = this.$t(element.InOutMode).toString();
      //   });
      // }
      // // console.log(this.logData)
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data) {
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
