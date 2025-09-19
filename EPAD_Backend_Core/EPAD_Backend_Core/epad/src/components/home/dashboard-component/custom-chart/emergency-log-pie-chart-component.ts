import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import CustomLegendPieChart from "@/components/home/dashboard-component/chart/custome-legend-pie-chart-component.vue";
import CustomLegendPieChartVer2 from "@/components/home/dashboard-component/chart/custome-legend-pie-chart-component-ver-2.vue";

@Component({
  name: "EmergencyLogPieChart",
  components: { CustomLegendPieChart, CustomLegendPieChartVer2 },
})
export default class EmergencyLogPieChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "emergency-log-pie-chart-component";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartIds: any;
  chartValues: any;
  onClickToExplode = false;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;

  colorData = [];

  randomHSLColorWithMainColor(mainHColor){
    const minH = mainHColor - 20;
    const maxH = mainHColor + 20;
    let h = Math.floor(Math.random() * (maxH - minH + 1)) + minH;
    do {
      h = Math.floor(Math.random() * (maxH - minH + 1)) + minH;
    } while (h % 5 !== 0);
    const s = Math.floor(Math.random() * (100 - 70 + 1)) + 70;
    const l = Math.floor(Math.random() * (80 - 30 + 1)) + 30;
    // console.log("hsl(" + h + "," + s + "%," + l + "%)")
    return "hsl(" + h + "," + s + "%," + l + "%)";
  }

  updateEmergencyData(){
    if(this.load){
      this.getData();
    }
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
      // {
      //   prop: 'InOutMode',
      //   label: this.$t('InOutMode'),
      // },
      // {
      //   prop: 'VerifyMode',
      //   label: this.$t('VerifyMode'),
      // },
      {
        prop: 'DepartmentName',
        label: this.$t('DepartmentName'),
      }
    ];
    this.getData();
  }

  getData(){
    this.load = false;
    const chartColorsValue = ["#33e7f2","#d1f00f","#f23378","#fd6510"];
    attendanceLogApi.GetEmergencyLog().then((res: any) => {
      this.logData = [];
      if(res.data){
        this.logData = res.data;
        // console.log(this.logData)
        // //console.log(this.employeeData)

        const groupEmployee = this.logData.reduce((grouped, emp) => {
          const key = emp.SerialNumber.toString();
        
          if (!grouped[key]) {
            grouped[key] = [];
          }
        
          grouped[key].push(emp);
        
          return grouped;
        }, {} as { [key: string]: [] });

        this.chartIds = [];
        this.chartLabels = [];
        this.chartValues = [];
        const chartIds = [];
        const chartLabels = [];
        const chartValues = [];

        // chartIds.push(0);
        // chartLabels.push(this.$t('NoDepartment'));
        // chartValues.push(groupEmployee["0"] ? groupEmployee["0"].length : 0);

        let colorIndex = 0;
        this.logData.forEach((element, index) => {
          if(element.SerialNumber 
            // && element.DepartmentName 
            && !chartIds.includes(element.SerialNumber) 
            // && !chartLabels.includes(element.DepartmentName)
          ){
              const countValue = groupEmployee[element.SerialNumber]?.length ?? 0
              if(countValue > 0){
                chartIds.push(element.SerialNumber);
                chartLabels.push(element.DeviceName);
                chartValues.push(countValue);
                this.colorData.push(chartColorsValue[colorIndex]);
                colorIndex++;
                if(colorIndex >= 4){
                  colorIndex = 0;
                }
              }
          }
        });

        this.chartIds = chartIds;
        this.chartLabels = chartLabels;
        this.chartValues = chartValues;
      }
      this.$nextTick(() => {
        this.load = true;
        this.$emit('dataEmergencyValues', this.chartIds, this.chartLabels, this.chartValues, this.colorData);
        this.$emit('dataEmergency', this.logData);
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

  handleChartClicked(chartClickedData){
    // //console.log(chartClickedData)
    // //console.log(chartClickedData.chartIndex)

    if(chartClickedData){
      this.explodeData = this.logData.filter(x => x.SerialNumber == chartClickedData.dataId);
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
