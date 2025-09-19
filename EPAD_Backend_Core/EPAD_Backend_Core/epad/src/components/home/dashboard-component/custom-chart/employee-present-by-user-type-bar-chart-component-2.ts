import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';

@Component({
  name: "EmployeePresentByUserTypeBarChart2",
  components: { BarChart, CustomLegendBarChart },
})
export default class EmployeePresentByUserTypeBarChart2Component extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "employee-present-by-user-type-bar-chart-2";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = false;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;
  clientName: string;
  load = false;
  totalValue = 0;
  chartSetColors = ['#81007f','red','orange','#113a92'];
  totalType = 5;

  beforeMount() {
    Misc.readFileAsync('static/variables/common-utils.json').then(x => {
      this.clientName = x.ClientName;
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
  });

   
  }

  getData(){
    this.totalValue = 0;
    this.load = false;
    attendanceLogApi.GetTupleFullWorkingEmployeeByUserType().then((res: any) => {
      // // console.log(res)
      if (res && res.data) {
        const chartSets = res.data["Item1"];
        this.totalType = res.data["Item1"].length;
        const chartLabels = [];
        const dataSetEmployee = {
          dataSetId: "Employee",
          dataIds: [],
          dataNames: this.$t('Employee'),
          dataValues: [],
          color: this.chartSetColors,
          valuesColor: "white",
        };

        if (res.data) {
          this.logData = res.data;

          chartSets.forEach((element, index) => {
          
            if(element == 'Customer' && this.clientName == 'Mondelez'){
              chartLabels.push(this.$t('CustomerAndNT24'));
            }else{
              chartLabels.push(this.$t(element));
            }
           
            dataSetEmployee.dataIds
              .push(element);
            dataSetEmployee.dataValues
              .push(this.logData["Item" + (index + 2)]?.length ?? 0);
            this.totalValue += (this.logData["Item" + (index + 2)]?.length ?? 0);
          });
        }
        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetEmployee,
        ];

      }
      this.$nextTick(() => {
        this.load = true;
        this.$emit('totalEmployeePresent', this.totalValue);
        this.$emit('dataEmployeePresent', this.logData);
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
     console.log(chartClickedData)
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
      }else if(chartClickedData.dataId == "ExtraDriver"){
        this.explodeData = logsData.Item5;
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
