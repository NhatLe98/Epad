import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { Pie } from 'vue-chartjs/legacy';
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  CategoryScale
} from 'chart.js'
import { setTimeout } from "timers";
import XLSX from 'xlsx';

ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale)

@Component({
  name: "PieChart",
  components: { Pie },
})
export default class PieChartComponent extends Mixins(ComponentBase) {
  chartData: any;
  chartOptions: any;
  @Prop() index: any;
  @Prop({ default: () => "" }) id: any;
  @Prop({ default: () => "" }) name: any;
  @Prop({ default: () => [] }) dataIds: any;
  @Prop({ default: () => [] }) dataLabels: any;
  @Prop({ default: () => [] }) dataValues: any;
  @Prop({ default: () => false }) onClickToExplode: any;
  @Prop({ default: () => false }) isAllowExportExplode: any;
  // @Prop() explodeApiInstance: any;
  // @Prop() explodeMethod: any;
  // @Prop() explodeParams: any;
  @Prop() explodeColumns: any;
  @Prop() explodeData: any;
  @Prop() isShowExplodeData = false;
  explodeName = '';

  chartId: {
    type: String,
    default: 'pie-chart'
  }
  datasetIdKey: {
    type: String,
    default: 'label'
  }
  cssClasses: {
    default: 'pie-chart-css',
    type: String
  }
  styles: {
    type: Object,
    default: () => {}
  }
  plugins: {
    type: Array<any>,
    default: () => []
  }

  load = true;

  handleClose(done) {
    this.explodeName = '';
    this.isShowExplodeData = false;
    this.$emit('updateIsShowExplodeData', 
        {
          chartIndex: this.index,
          isShowExplodeData: this.isShowExplodeData,
        });
    done();
  }


  // randomColor(){
  //   return "#" + Math.floor(Math.random()*16777215).toString(16);
  // }

  randomColor(){
    const r = Math.floor(Math.random() * 255);
    const g = Math.floor(Math.random() * 255);
    const b = Math.floor(Math.random() * 255);
    return "rgb(" + r + "," + g + "," + b + ")";
  }
  
  beforeMount() {
    // console.log("AAA")
    let colorData = [];
    this.dataValues.forEach(() => {
      colorData.push(this.randomColor());
    });
    this.chartData = {
      labels: this.dataLabels,
      datasets: [
        {
          chartId: this.id,
          chartName: this.name,
          dataid: this.dataIds,
          backgroundColor: colorData,
          data: this.dataValues,
        }
      ],
    }
    this.chartOptions = {
      plugins: {
        legend: {
          display: true,
          position: 'right',
          width: 30,
        },
        datalabels: {
          display: false,
          color: '#33cc5e',
          anchor: 'end',
          font: {
            weight: 'bold',
            size: 12
          },
          align: 'top'
        }
      },
      responsive: true,
      maintainAspectRatio: false,
      onClick: (evt, legendItem, legend) => {
        // console.log(evt)
        // console.log(legendItem)
        // console.log(legendItem[0].index)
        // console.log(legendItem[0].element.$context)
        // console.log(legendItem[0].element.$context.dataset)
        // console.log(legendItem[0].element.$context.dataset.dataid)
        // console.log(legendItem[0].element.$context.dataset.dataid[legendItem[0].index])
        // console.log(legend)

        if(this.onClickToExplode){
          this.$emit('clickedItem', 
          {
            chartIndex: this.index,
            chartId: this.id,
            chartName: this.name,
            itemIndex: legendItem[0].index, 
            dataId: legendItem[0].element.$context.dataset.dataid[legendItem[0].index],
            dataLabel: this.dataLabels[legendItem[0].index],
            dataValue: this.dataValues[legendItem[0].index],
          });
        }
        else{
          this.$emit('clickedItem', null);
        }

        this.explodeName = this.dataLabels[legendItem[0].index];

        // if(this.onClickToExplode){
        //   // console.log(this.explodeParams)
        //   this.isShowExplodeData = false;
        //   if(this.explodeParams && Object.keys(this.explodeParams).length > 0){
        //     const methodArgs = Object.values(this.explodeParams);
        //     this.explodeApiInstance[this.explodeMethod](...methodArgs).then((res: any) => {
        //       if(res.data){
        //         if(res.data.total){
        //           this.explodeData = res.data.data;
        //         }else{
        //           this.explodeData = res.data;
        //         }
        //         this.isShowExplodeData = true;
        //       }
        //       // console.log("have method's params")
        //       // console.log(res)
        //     });
        //   }else{
        //     this.explodeApiInstance[this.explodeMethod]().then((res: any) => {
        //       if(res.data){
        //         if(res.data.total){
        //           this.explodeData = res.data.data;
        //         }else{
        //           this.explodeData = res.data;
        //         }
        //         this.isShowExplodeData = true;
        //       }
        //       // console.log("no method's params")
        //       // console.log(res)
        //     });
        //   }
        // }
      }
    }
  }

  showExplodeData(data){
    // console.log(data)
  }

  refreshChart(){
    this.load = false;
    setTimeout(() => {
      this.load = true;
    }, 500);
  }

  exportToExcel() {
    const data = this.explodeData;
    let formatData = [];
    if(data && data.length){
      for(let i = 0; i < data.length; i++){
        let obj = {};
        this.explodeColumns.forEach(element => {
          const key = this.$t(element.prop).toString();
          if (!obj[key]) {
            obj[key] = []
          }
          obj[key] = data[i][element.prop];
        });
        formatData.push(obj);
      }
    }
    // console.log(data)
    // export json to Worksheet of Excel
    // only array possible
    var dataWs = XLSX.utils.json_to_sheet(formatData) 

    let cellsWidth = [];
    this.explodeColumns.forEach(element => {
      cellsWidth.push({width: 30});
    });
    dataWs['!cols'] = cellsWidth;

    // A workbook is the name given to an Excel file
    var wb = XLSX.utils.book_new() // make Workbook of Excel

    // add Worksheet to Workbook
    // Workbook contains one or more worksheets
    XLSX.utils.book_append_sheet(wb, dataWs, "ExportData") // sheetAName is name of Worksheet

    // export Excel file
    XLSX.writeFile(wb, 'ExportChartData.xlsx') // name of the file is 'book.xlsx'
  }
}
