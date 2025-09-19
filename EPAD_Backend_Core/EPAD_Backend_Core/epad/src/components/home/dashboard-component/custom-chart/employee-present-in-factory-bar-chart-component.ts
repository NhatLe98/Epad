import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { gatesApi } from '@/$api/gc-gates-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "EmployeePresentInFactoryBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class EmployeePresentInFactoryBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "employee-present-in-factory-bar-chart";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = true;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  gateData: any;

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
        prop: 'GateName',
        label: this.$t('GateName'),
      }
    ];

    gatesApi.GetGatesAll().then((res: any) => {
      // console.log(res)
      if(res && res.data && res.data.data){
        this.gateData = res.data.data;
        // console.log(this.gateData)
      }
      this.gateData.push({
        Index: 0,
        Name: this.$t('NoGate'),
      })
    });

    attendanceLogApi.GetTupleFullWorkingEmployeeByDepartment().then((res: any) => {
      // // console.log(res)
      if (res && res.data) {
        const chartLabels = [];
        const dataSetEmployeeIn = {
          dataSetId: "In",
          dataIds: [],
          dataNames: this.$t('In'),
          dataValues: [],
          color: "blue",
          valuesColor: "red",
        };
        const dataSetEmployeeOut = {
          dataSetId: "Out",
          dataIds: [],
          dataNames: this.$t('Out'),
          dataValues: [],
          color: "orange",
          valuesColor: "lawngreen",
        };
        const dataSetEmployeeRemain = {
          dataSetId: "Remain",
          dataIds: [],
          dataNames: this.$t('RemainIn'),
          dataValues: [],
          color: "red",
          valuesColor: "blue",
        };

        if (res.data) {
          this.logData = res.data;
          this.logData.Item1.forEach(element => {
            element.DepartmentName = this.$t(element.DepartmentName).toString();
          });
          this.logData.Item2.forEach(element => {
            element.DepartmentName = this.$t(element.DepartmentName).toString();
          });
          this.logData.Item3.forEach(element => {
            element.DepartmentName = this.$t(element.DepartmentName).toString();
          });

          if (this.gateData && this.gateData.length > 0) {
            this.gateData.forEach((element, index) => {
              let groupLogInData = [];
              let groupLogOutData = [];
              let groupLogRemainData = [];
              if(element.Index != 0){
                groupLogInData = Misc.cloneData(this.logData.Item1.filter(x => 
                  x.GateIndex && x.GateIndex.includes(element.Index)));
                groupLogOutData = Misc.cloneData(this.logData.Item2.filter(x => 
                  x.GateIndex && x.GateIndex.includes(element.Index)));
                groupLogRemainData = Misc.cloneData(this.logData.Item3.filter(x => 
                  x.GateIndex && x.GateIndex.includes(element.Index)));
              }else{
                groupLogInData = Misc.cloneData(this.logData.Item1.filter(x => 
                  !x.GateIndex || (x.GateIndex && x.GateIndex.length == 0)));
                groupLogOutData = Misc.cloneData(this.logData.Item2.filter(x => 
                  !x.GateIndex || (x.GateIndex && x.GateIndex.length == 0)));
                groupLogRemainData = Misc.cloneData(this.logData.Item3.filter(x => 
                  !x.GateIndex || (x.GateIndex && x.GateIndex.length == 0)));    
              }
              if(!groupLogInData){
                groupLogInData = [];
              }

              let employeePresentLogIn = groupLogInData;
              if(!employeePresentLogIn){
                employeePresentLogIn = [];
              }
              let employeePresentLogOut = groupLogOutData;
              if(!employeePresentLogOut){
                employeePresentLogOut = [];
              }
              let employeePresentLogRemain = groupLogRemainData;
              if(!employeePresentLogRemain){
                employeePresentLogRemain = [];
              }
              //// // console.log(element)
              const logInCount = employeePresentLogIn?.length ?? 0;
              const logOutCount = employeePresentLogOut?.length ?? 0;
              // const logRemainCount = employeePresentLogRemain?.length ?? 0;
              let logRemainCount = logInCount - logOutCount;
              if(logRemainCount < 0){
                logRemainCount = 0;
              }
              const departmentName = element.Name;
              if (!chartLabels.includes(departmentName) 
                && (logInCount > 0 || logOutCount > 0 || logRemainCount > 0)) {
                chartLabels.push(departmentName);
                dataSetEmployeeIn.dataIds
                  .push(element.Index);
                dataSetEmployeeIn.dataValues
                  .push(logInCount);
                dataSetEmployeeOut.dataIds
                  .push(element.Index);
                dataSetEmployeeOut.dataValues
                  .push(logOutCount);
                dataSetEmployeeRemain.dataIds
                  .push(element.Index);
                dataSetEmployeeRemain.dataValues
                  .push(logRemainCount);
              }
            });
          }

        }
        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetEmployeeIn,
          dataSetEmployeeOut,
          dataSetEmployeeRemain
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

  handleChartClicked(chartClickedData) {
    // // console.log(chartClickedData)
    if(chartClickedData.dataSetId == "Remain") {
      return;
    }
    if (chartClickedData) {
      const departmentLogData = Misc.cloneData(this.logData);
      if(chartClickedData.dataId != 0){
        if(chartClickedData.dataSetId == "In"){
          this.explodeData = departmentLogData.Item1.filter(x => x.GateIndex 
            && x.GateIndex.includes(chartClickedData.dataId));
        }else if(chartClickedData.dataSetId == "Out"){
          this.explodeData = departmentLogData.Item2.filter(x => x.GateIndex 
            && x.GateIndex.includes(chartClickedData.dataId));
        }else if(chartClickedData.dataSetId == "Remain"){
          this.explodeData = departmentLogData.Item3.filter(x => x.GateIndex 
            && x.GateIndex.includes(chartClickedData.dataId));
        }
      }else{
        if(chartClickedData.dataSetId == "In"){
          this.explodeData = departmentLogData.Item1.filter(x => !x.GateIndex 
            || (x.GateIndex && x.GateIndex.length == 0));
        }else if(chartClickedData.dataSetId == "Out"){
          this.explodeData = departmentLogData.Item2.filter(x => !x.GateIndex 
            || (x.GateIndex && x.GateIndex.length == 0));
        }else if(chartClickedData.dataSetId == "Remain"){
          this.explodeData = departmentLogData.Item3.filter(x => !x.GateIndex 
            || (x.GateIndex && x.GateIndex.length == 0));
        }
      }
      if (this.explodeData && this.explodeData.length > 0) {
        this.explodeData.forEach((element, index) => {
          if(element.GateName && element.GateName.length > 0){
            element.GateName = element.GateName.join(', ');
          }else{
            element.GateName = this.$t('NoGate');
          }
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
