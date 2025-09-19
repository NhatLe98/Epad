import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { hrEmployeeInfoApi } from '@/$api/hr-employee-info-api';
import CustomLegendBarChart from "@/components/home/dashboard-component/chart/custom-legend-bar-chart-component.vue";
import { deviceApi } from '@/$api/device-api';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
  name: "DeviceInfoBarChart",
  components: { CustomLegendBarChart },
})
export default class DeviceInfoBarChartComponent extends Mixins(ComponentBase) {
  @Prop() index: any;
  id = "device-info-bar-chart";
  @Prop() name;
  deviceData: any;
  chartLabels: any;
  chartSets: any;
  onClickToExplode = false;
  explodeColumns: any;
  explodeData: any;
  isShowExplodeData = false;

  load = false;

  beforeMount(){
    deviceApi.GetListDeviceInfo().then((res: any) => {
      // //console.log(res)
      if(res.data && res.data.data && res.data.data.length > 0){
        this.deviceData = res.data.data;

        const chartLabels = [];
        const dataSetUser = {
          dataSetId: "UserCount",
          dataIds: [],
          dataNames: this.$t('User'),
          dataValues: [],
          color: "blue",
          valuesColor: "orange",
        };
        const dataSetFinger = {
          dataSetId: "FingerCount",
          dataIds: [],
          dataNames: this.$t('Finger'),
          dataValues: [],
          color: "orange",
          valuesColor: "blue",
        };
        const dataSetFace = {
          dataSetId: "FaceCount",
          dataIds: [],
          dataNames: this.$t('Face'),
          dataValues: [],
          color: "yellow",
          valuesColor: "green",
        };
        res.data.data.forEach((element, index) => {
          chartLabels.push((element.AliasName && element.AliasName != '') ? element.AliasName : element.SerialNumber);
          dataSetUser.dataIds.push(element.SerialNumber);
          dataSetUser.dataValues.push(element?.UserCount ?? 0);
          dataSetFinger.dataIds.push(element.SerialNumber);
          dataSetFinger.dataValues.push(element?.FingerCount ?? 0);
          dataSetFace.dataIds.push(element.SerialNumber);
          dataSetFace.dataValues.push(element?.FaceCount ?? 0);
        });
        this.chartLabels = chartLabels;
        this.chartSets = [
          dataSetUser, dataSetFinger, dataSetFace
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

  handleChartClicked(chartClickedData){
    // //console.log(chartClickedData)

    if(chartClickedData){
      this.isShowExplodeData = true;
    }
  }

  updateIsShowExplodeData(data){
    this.isShowExplodeData = data.isShowExplodeData;
  }
}
