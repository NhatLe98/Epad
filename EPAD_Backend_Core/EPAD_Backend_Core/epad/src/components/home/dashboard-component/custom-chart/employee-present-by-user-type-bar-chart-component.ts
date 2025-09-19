import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "EmployeePresentByUserTypeBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class EmployeePresentByUserTypeBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "employee-present-by-user-type-bar-chart";
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
        prop: 'UserTypeName',
        label: this.$t('UserTypeName'),
      }
    ];
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
          const userTypes = [];
          this.logData.Item1.forEach(element => {
            if (!userTypes.some(y => y.UserType == element.UserType)) {
              userTypes.push({ UserType: element.UserType, UserTypeName: element.UserTypeName });
            }
          });
          this.logData.Item2.forEach(element => {
            if (!userTypes.some(y => y.UserType == element.UserType)) {
              userTypes.push({ UserType: element.UserType, UserTypeName: element.UserTypeName });
            }
          });
          this.logData.Item3.forEach(element => {
            if (!userTypes.some(y => y.UserType == element.UserType)) {
              userTypes.push({ UserType: element.UserType, UserTypeName: element.UserTypeName });
            }
          });
          // // console.log(userTypes)

          if (userTypes && userTypes.length > 0) {
            const groupLogInData = this.logData.Item1.reduce((grouped, emp) => {
              const key = emp.UserType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            const groupLogOutData = this.logData.Item2.reduce((grouped, emp) => {
              const key = emp.UserType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            const groupLogRemainData = this.logData.Item3.reduce((grouped, emp) => {
              const key = emp.UserType;

              if (!grouped[key]) {
                grouped[key] = [];
              }

              grouped[key].push(emp);

              return grouped;
            }, {} as { [key: number]: [] });
            // // console.log(groupLogData)

            userTypes.forEach((element, index) => {

              let employeePresentLogIn = groupLogInData[element.UserType];
              if(!employeePresentLogIn){
                employeePresentLogIn = [];
              }
              let employeePresentLogOut = groupLogOutData[element.UserType];
              if(!employeePresentLogOut){
                employeePresentLogOut = [];
              }
              // let employeePresentLogRemain = groupLogRemainData[element.UserType];
              // if(!employeePresentLogRemain){
              //   employeePresentLogRemain = [];
              // }
              let logRemainCount = employeePresentLogIn.length - employeePresentLogOut.length;
              if(logRemainCount < 0){
                logRemainCount = 0;
              }
              //// // console.log(element)
              const departmentName = element.UserTypeName;
              if (!chartLabels.includes(departmentName)) {
                chartLabels.push(departmentName);
                dataSetEmployeeIn.dataIds
                  .push(element.UserType);
                dataSetEmployeeIn.dataValues
                  .push(employeePresentLogIn?.length ?? 0);
                dataSetEmployeeOut.dataIds
                  .push(element.UserType);
                dataSetEmployeeOut.dataValues
                  .push(employeePresentLogOut?.length ?? 0);
                dataSetEmployeeRemain.dataIds
                  .push(element.UserType);
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
      if(chartClickedData.dataSetId == "In"){
        this.explodeData = departmentLogData.Item1.filter(x => x.UserType == chartClickedData.dataId);
      }else if(chartClickedData.dataSetId == "Out"){
        this.explodeData = departmentLogData.Item2.filter(x => x.UserType == chartClickedData.dataId);
      }else if(chartClickedData.dataSetId == "Remain"){
        this.explodeData = departmentLogData.Item3.filter(x => x.UserType == chartClickedData.dataId);
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
