import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';

@Component({
  name: "VehicleInOutBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class VehicleInOutBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "vehicle-in-out-bar-chart-component";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = false;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;
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

  getData(){
    this.load = false;
    attendanceLogApi.GetIntegratedVehicleLog().then((res: any) => {
      // // console.log(res)
      let totalInNotOutToday = 0;
      if (res && res.data) {
        const chartSets = ["Employee","Customer","Contractor"];
        const chartLabels = [];
        const dataSetIn = {
          dataSetId: "In",
          dataIds: [],
          dataNames: this.$t('In'),
          dataValues: [],
          color: '#81007f',
          valuesColor: "white",
        };
        const dataSetOut = {
          dataSetId: "Out",
          dataIds: [],
          dataNames: this.$t('Out'),
          dataValues: [],
          color: '#f23378',
          valuesColor: "white",
        };

        if (res.data) {
          this.logData = res.data;

          chartSets.forEach((element, index) => {
            chartLabels.push(this.$t(element));
            dataSetIn.dataIds
              .push(element);
            dataSetIn.dataValues
              .push(this.logData["Item" + (index + 1)].filter(x => x.InOutMode.includes("In"))?.length ?? 0);
            dataSetOut.dataIds
              .push(element);
            dataSetOut.dataValues
              .push(this.logData["Item" + (index + 1)].filter(x => x.InOutMode.includes("Out"))?.length ?? 0);
          });
        }
        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetIn,
          dataSetOut
        ];

        if(dataSetIn && dataSetIn.dataValues && dataSetIn.dataValues.length > 0){
          dataSetIn.dataValues.forEach(element => {
            totalInNotOutToday += element;
          });
        }
        if(dataSetOut && dataSetOut.dataValues && dataSetOut.dataValues.length > 0){
          dataSetOut.dataValues.forEach(element => {
            totalInNotOutToday -= element;
          });
        }

      }
      this.$nextTick(() => {
        this.load = true;
        this.$emit('totalVehicleInNotOut', this.logData.Item4.length);
        this.$emit('totalVehicleInNotOutToday', totalInNotOutToday);
        this.$emit('dataVehicleInNotOut', this.logData);
      });
    });
  }

  reloadInterval = null;
  mounted(){
    this.reloadInterval = setInterval(() => {
      if(this.load){
        this.getData();
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
      const logsData = Misc.cloneData(this.logData);
      if(chartClickedData.dataId == "Employee"){
        this.explodeData = logsData.Item1;
      }else if(chartClickedData.dataId == "Customer"){
        this.explodeData = logsData.Item2;
      }else if(chartClickedData.dataId == "Contractor"){
        this.explodeData = logsData.Item3;
      }else if(chartClickedData.dataId == "Driver"){
        this.explodeData = logsData.Item4;
      }
      
      if (this.explodeData && this.explodeData.length > 0) {
        this.explodeData.forEach((element, index) => {
          element.VerifyMode = this.$t(element.VerifyMode).toString();
          element.InOutMode = this.$t(element.InOutMode).toString();
        });
      }
      // // console.log(this.logData)
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data) {
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
