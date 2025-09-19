import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { Bar } from 'vue-chartjs/legacy';
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
} from 'chart.js';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { setTimeout } from "timers";
import XLSX from 'xlsx';

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, ChartDataLabels)

@Component({
  name: "BarChart",
  components: { Bar },
})
export default class BarChartComponent extends Mixins(ComponentBase) {
  chartData: any;
  chartOptions: any;
  @Prop() index: any;
  @Prop({ default: () => "" }) id: any;
  @Prop({ default: () => "" }) name: any;
  @Prop() dataNames: any;
  @Prop({ default: () => [] }) dataSets: any;
  @Prop({ default: () => [] }) dataLabels: any;
  @Prop({ default: () => false }) onClickToExplode: any;
  @Prop({ default: () => false }) isAllowExportExplode: any;
  @Prop({ default: () => false }) stacked: any;
  @Prop({ default: () => true }) labelsDisplay: any;
  @Prop({ default: () => false }) valuesDisplay: any;
  @Prop() explodeColumns: any;
  @Prop() explodeData: any;
  @Prop() isShowExplodeData = false;
  explodeName = '';
  chartWidth = 100;

  chartId: {
    type: String,
    default: 'bar-chart'
  }
  datasetIdKey: {
    type: String,
    default: 'label'
  }
  cssClasses: {
    default: 'bar-chart-css',
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

  splitString(string) {
    const chunks = [];
    let numSpaces = 0;
    let lastIndex = 0;

    for (let i = 0; i < string.length; i++) {
      if (string[i] === ' ') {
        numSpaces++;
        
        if (numSpaces === 2) {
          chunks.push(string.substring(lastIndex, i));
          lastIndex = i + 1;
          numSpaces = 0;
        }
      }
    }

    chunks.push(string.substring(lastIndex));
  
    return chunks;
  }
  
  beforeMount() {
    const arrayLabels = [];
    this.dataLabels.forEach(element => {
      arrayLabels.push(this.splitString(element));
    });
    this.chartData = {
      labels: arrayLabels,
      datasets: [],
    }
    if(this.dataLabels && this.dataLabels.length > 5){
      this.chartWidth = (Math.floor(this.dataLabels.length/5) + 1) * 100;
    }

    // console.log(this.dataLabels)
    // console.log(this.dataSets)

    this.dataSets.forEach((item) => {
      let color = this.randomColor();
      if(item.color){
        color = item.color;
      }
      this.chartData.datasets.push(
        {
          chartId: this.id,
          chartName: this.name,
          label: item.dataNames,
          setId: item.dataSetId,
          dataid: item.dataIds,
          backgroundColor: color,
          data: item.dataValues,
          datalabels: {
            color: item?.valuesColor ?? '#33cc5e'
          }
        }
      )
    });

    this.chartOptions = {
      plugins: {
        tooltip: {
          callbacks: {
            title: (context) => {
              return context[0].label.replaceAll(',',' ');
            }
          }
        },
        legend: {
          display: this.labelsDisplay,
          position: 'top',
          width: 30,
        },
        datalabels: {
          display: this.valuesDisplay,
          color: '#33cc5e',
          anchor: 'center',
          font: {
            weight: 'bold',
            size: 14
          },
          align: 'top'
        }
      },
      categoryPercentage: 0.8,
      barPercentage: 1,
      // barThickness: (!this.stacked ? (90 / this.dataSets.length) : 90),
      scales: {
        x: {
          stacked: this.stacked,
        },
        y: {
          stacked: this.stacked,
          ticks: {
            stepSize: 1,
            beginAtZero: true,
          },
        },
      },
      responsive: true,
      maintainAspectRatio: false,
      onClick: (evt, legendItem, legend) => {
        // console.log(evt)
        // console.log(legendItem)
        // console.log(legendItem[0].index)
        // console.log(legendItem[0].element.$context.dataset)
        // console.log(legendItem[0].element.$context.dataset.data)
        // console.log(legendItem[0].element.$context.dataset.data[legendItem[0].index])
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
            dataSetId: legendItem[0].element.$context.dataset.setId,
            dataId: legendItem[0].element.$context.dataset.dataid[legendItem[0].index],
            dataLabel: this.dataLabels[legendItem[0].index],
            dataValue: legendItem[0].element.$context.dataset.data[legendItem[0].index],
          });
        }

        this.explodeName = this.dataLabels[legendItem[0].index];
      }
    }
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
