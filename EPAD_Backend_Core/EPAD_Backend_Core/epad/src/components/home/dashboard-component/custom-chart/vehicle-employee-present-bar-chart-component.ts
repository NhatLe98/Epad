import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "VehicleEmployeePresentTypeBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class VehicleEmployeePresentBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "vehicle-employee-present-bar-chart";
  @Prop() name;
  logData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = true;
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
      // {
      //   prop: 'DeviceName',
      //   label: this.$t('DeviceName'),
      // },
      {
        prop: 'TimeString',
        label: this.$t('TimeString'),
      },
      {
        prop: 'InOutMode',
        label: this.$t('InOutMode'),
      },
      // {
      //   prop: 'VerifyMode',
      //   label: this.$t('VerifyMode'),
      // },
      {
        prop: 'VehicleTypeName',
        label: this.$t('VehicleTypeName'),
      },
      {
        prop: 'Plate',
        label: this.$t('Plate'),
      }
    ];
    attendanceLogApi.GetTupleFullVehicleEmployeeByDepartment().then((res: any) => {
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
          const vehicles = [];
          this.logData.Item1.forEach(element => {
            if (!vehicles.some(y => y.VehicleType == element.VehicleType)) {
              vehicles.push({ VehicleType: element.VehicleType, VehicleTypeName: element.VehicleTypeName });
            }
          });
          this.logData.Item2.forEach(element => {
            if (!vehicles.some(y => y.VehicleType == element.VehicleType)) {
              vehicles.push({ VehicleType: element.VehicleType, VehicleTypeName: element.VehicleTypeName });
            }
          });
          this.logData.Item3.forEach(element => {
            if (!vehicles.some(y => y.VehicleType == element.VehicleType)) {
              vehicles.push({ VehicleType: element.VehicleType, VehicleTypeName: element.VehicleTypeName });
            }
          });
          // // console.log(vehicles)

          if (vehicles && vehicles.length > 0) {
            const groupLogInData = this.logData.Item1.reduce((grouped, emp) => {
              const key = emp.VehicleType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            const groupLogOutData = this.logData.Item2.reduce((grouped, emp) => {
              const key = emp.VehicleType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            const groupLogRemainData = this.logData.Item3.reduce((grouped, emp) => {
              const key = emp.VehicleType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            // // console.log(groupLogData)

            vehicles.forEach((element, index) => {

              let employeePresentLogIn = groupLogInData[element.VehicleType];
              if(!employeePresentLogIn){
                employeePresentLogIn = [];
              }
              let employeePresentLogOut = groupLogOutData[element.VehicleType];
              if(!employeePresentLogOut){
                employeePresentLogOut = [];
              }
              let employeePresentLogRemain = groupLogRemainData[element.VehicleType];
              if(!employeePresentLogRemain){
                employeePresentLogRemain = [];
              }
              // let logRemainCount = employeePresentLogIn.length - employeePresentLogOut.length;
              // if(logRemainCount < 0){
              //   logRemainCount == 0;
              // }
              //// // console.log(element)
              const departmentName = element.VehicleTypeName;
              if (!chartLabels.includes(departmentName)) {
                chartLabels.push(departmentName);
                dataSetEmployeeIn.dataIds
                  .push(element.VehicleType);
                dataSetEmployeeIn.dataValues
                  .push(employeePresentLogIn?.length ?? 0);
                dataSetEmployeeOut.dataIds
                  .push(element.VehicleType);
                dataSetEmployeeOut.dataValues
                  .push(employeePresentLogOut?.length ?? 0);
                dataSetEmployeeRemain.dataIds
                  .push(element.VehicleType);
                  dataSetEmployeeRemain.dataValues
                  .push(employeePresentLogRemain?.length ?? 0);
                // dataSetEmployeeRemain.dataValues
                //   .push(logRemainCount);
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
      if(chartClickedData.dataSetId == "In"){
        this.explodeData = departmentLogData.Item1.filter(x => x.VehicleType == chartClickedData.dataId);
      }else if(chartClickedData.dataSetId == "Out"){
        this.explodeData = departmentLogData.Item2.filter(x => x.VehicleType == chartClickedData.dataId);
      }else if(chartClickedData.dataSetId == "Remain"){
        this.explodeData = departmentLogData.Item3.filter(x => x.VehicleType == chartClickedData.dataId);
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
