import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import CustomLegendPieChart from "@/components/home/dashboard-component/chart/custome-legend-pie-chart-component.vue";

@Component({
  name: "RootDepartmentEmployeePieChart",
  components: { CustomLegendPieChart },
})
export default class RootDepartmentEmployeePieChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "root-department-employee-pie-chart";
  @Prop() name;
  employeeData: any;
  chartLabels: any;
  chartIds: any;
  chartValues: any;
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
      }
    ];

    hrEmployeeInfoApi.GetEmployeeInfoByUserRootDepartment().then((res: any) => {
      if(res.data && res.data.length > 0){
        this.employeeData = res.data;
        // //console.log(this.employeeData)

        const groupEmployee = this.employeeData.reduce((grouped, emp) => {
          const key = emp.DepartmentIndex.toString();
        
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

        chartIds.push(0);
        chartLabels.push(this.$t('NoDepartment'));
        chartValues.push(groupEmployee["0"] ? groupEmployee["0"].length : 0);

        this.employeeData.forEach((element, index) => {
          if(element.DepartmentIndex && element.DepartmentName 
            && !chartIds.includes(element.DepartmentIndex) && !chartLabels.includes(element.DepartmentName)){
              chartIds.push(element.DepartmentIndex);
              chartLabels.push(element.DepartmentName);
              chartValues.push(groupEmployee[element.DepartmentIndex]?.length ?? 0);
          }
        });

        this.chartIds = chartIds;
        this.chartLabels = chartLabels;
        this.chartValues = chartValues;
      }
      this.$nextTick(() => {
        this.load = true;
      });
    });
  }

  mounted(){
    
  }

  handleChartClicked(chartClickedData){
    // //console.log(chartClickedData)

    if(chartClickedData){
      this.explodeData = this.employeeData.filter(x => x.DepartmentIndex == chartClickedData.dataId);
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
