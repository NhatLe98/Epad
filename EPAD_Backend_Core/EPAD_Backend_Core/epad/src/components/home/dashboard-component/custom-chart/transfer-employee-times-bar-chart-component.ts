import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import BarChart from "@/components/home/dashboard-component/chart/bar-chart-component.vue";
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { workingInfoApi } from '@/$api/working-info-api';
import { attendanceLogApi } from '@/$api/attendance-log-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "TransferEmployeeTimesBarChart",
  components: { BarChart, CustomLegendBarChart },
})
export default class TransferEmployeeTimesBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "transfer-employee-times-bar-chart";
  @Prop() name;
  transferData: any;
  chartLabels: any;
  chartSets: any;
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
      },
      {
          prop: 'DepartmentTransfer',
          label: this.$t('DepartmentTransfer'),
      },
      {
          prop: 'FromDateString',
          label: this.$t('FromDateString'),
      },
      {
          prop: 'ToDateString',
          label: this.$t('ToDateString'),
      }
    ];
    workingInfoApi.GetTransferLast7Days().then((res: any) => {
      // console.log(res)
      if(res && res.data){
        this.transferData = res.data;
        const chartLabels = [];
        const dataSetTransferEmployees = {
          dataSetId: "transferEmployees",
          dataIds: [],
          dataNames: this.$t('Transfer'),
          dataValues: [],
          color: "blue",
          valuesColor: "yellow",
        };
        const dataSetNoTransferEmployees = {
          dataSetId: "noTransferEmployees",
          dataIds: [],
          dataNames: this.$t('NoTransfer'),
          dataValues: [],
          color: "red",
          valuesColor: "lawngreen",
        };

        let entries = Object.entries(res.data);
        entries.map(([key, val], index) => {
          chartLabels.push(key);
          dataSetTransferEmployees.dataIds.push(key + "_transfer");
          dataSetTransferEmployees.dataValues
            .push((val as any).Item2?.length ?? 0);
          dataSetNoTransferEmployees.dataIds.push(key + "_noTransfer");
          dataSetNoTransferEmployees.dataValues
            .push((val as any).Item3?.length  ?? 0);
        });

        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetTransferEmployees,
          dataSetNoTransferEmployees, 
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
      if(chartClickedData.dataId.includes("_transfer")){
        const key = chartClickedData.dataId.split('_transfer')[0];
        this.explodeData = this.transferData[key].Item2;
      }else if(chartClickedData.dataId.includes("_noTransfer")){
        const key = chartClickedData.dataId.split('_noTransfer')[0];
        this.explodeData = this.transferData[key].Item3;
      }
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
