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
  name: "CustomLegendBarChart",
  components: { Bar },
})
export default class CustomLegendBarChartComponent extends Mixins(ComponentBase) {
  chartData: any;
  chartOptions: any;
  @Prop() index: any;
  legendId: any;
  @Prop({ default: () => "" }) id: any;
  @Prop({ default: () => "" }) name: any;
  @Prop() dataNames: any;
  @Prop({ default: () => [] }) dataSets: any;
  @Prop({ default: () => [] }) dataLabels: any;
  @Prop({ default: () => true }) isDisplayName: any;
  @Prop({ default: () => true }) isDisplayCustomLegend: any;
  @Prop({ default: () => false }) onClickToExplode: any;
  @Prop({ default: () => false }) isAllowExportExplode: any;
  @Prop({ default: () => false }) stacked: any;
  @Prop({ default: () => true }) labelsDisplay: any;
  @Prop({ default: () => false }) valuesDisplay: any;
  @Prop({ default: () => false }) randomBarColorInSet: any;
  @Prop({ default: () => true }) tooltipEnable: any;
  @Prop({ default: () => '' }) valuesFormat: any;
  @Prop({ default: () => 'black' }) XValueColor: any;
  @Prop({ default: () => 'black' }) YValueColor: any;
  @Prop({ default: () => 'black' }) customLegendColor: any;
  @Prop() explodeColumns: any;
  @Prop() explodeData: any;
  @Prop() isShowExplodeData = false;
  explodeName = '';
  chartWidth = 100;

  chartId: {
    type: String,
    default: 'custom-legend-bar-chart'
  }
  datasetIdKey: {
    type: String,
    default: 'label'
  }
  cssClasses: {
    default: 'custom-legend-bar-chart-css',
    type: String
  }
  styles: {
    type: Object,
    default: () => {}
  }
  plugins: any;
  
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

  htmlLegendPlugin = {
    id: 'htmlLegend',
    thisVue: this,
    afterUpdate(chart, args, options) {
      if(this.thisVue.isDisplayCustomLegend){
        const legendComponent = document.getElementById(options.containerID);
        if(legendComponent){
          legendComponent.innerHTML = null;
        }
  
        // Reuse the built-in legendItems generator
        const items = chart.options.plugins.legend.labels.generateLabels(chart);
    
        items.forEach((item, index) => {
          const elCol = document.createElement('div');
          elCol.style.marginLeft = '10px';
          elCol.style.overflow = 'hidden';
          elCol.style.whiteSpace = 'nowrap';
          elCol.style.textOverflow = 'ellipsis';
          elCol.setAttribute("class", "el-col el-col-6");
          elCol.setAttribute("title", item.text);
          elCol.onclick = () => {
            const {type} = chart.config;
            if (type === 'pie' || type === 'doughnut') {
              // Pie and doughnut charts only have a single dataset and visibility is per item
              chart.toggleDataVisibility(item.index);
            } else {
              chart.setDatasetVisibility(item.datasetIndex, !chart.isDatasetVisible(item.datasetIndex));
            }
            chart.update();
          };
  
          // Color box
          const boxSpan = document.createElement('span');
          boxSpan.style.background = item.fillStyle;
          boxSpan.style.borderColor = item.strokeStyle;
          boxSpan.style.borderWidth = item.lineWidth + 'px';
          boxSpan.style.display = 'inline-block';
          boxSpan.style.height = '15px';
          boxSpan.style.marginRight = '10px';
          boxSpan.style.width = '15px';
          boxSpan.style.border = '1px solid black';
    
          // Text
          const textContainer = document.createElement('span');
          textContainer.style.color = this.thisVue.customLegendColor;
          textContainer.style.fontSize = '12px';
          textContainer.style.margin = '0';
          textContainer.style.padding = '0';
          textContainer.style.overflow = 'hidden';
          textContainer.style.whiteSpace = 'nowrap';
          textContainer.style.textOverflow = 'ellipsis';
          textContainer.style.textDecoration = item.hidden ? 'line-through' : '';
    
          const text = document.createTextNode(item.text);
          textContainer.appendChild(text);
  
          elCol.appendChild(boxSpan);
          elCol.appendChild(textContainer);
          legendComponent.appendChild(elCol);
        });
      }
    }
  };

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
    this.legendId = this.id + '_' + this.index;
    const arrayLabels = [];
    this.dataLabels.forEach(element => {
      arrayLabels.push(this.splitString(element));
    });
    this.chartData = {
      labels: arrayLabels,
      datasets: [],
    }
    if(this.dataLabels && this.dataLabels.length > 6){
      this.chartWidth = (Math.floor(this.dataLabels.length/6) + 1) * 100;
    }

    // console.log(this.dataLabels)
    // console.log(this.dataSets)

    this.dataSets.forEach((item) => {
      let color: any = this.randomColor();
      if(this.randomBarColorInSet){
        color = [];
        item.dataValues.forEach(element => {
          color.push(this.randomColor());
        });
      }
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
            color: item?.valuesColor ?? 'black'
          },
        }
      )
    });

    const thisVue = this;
    this.chartOptions = {
      layout: {
        padding: {
          top: 40,
        }
      },
      plugins: {
        tooltip: {
          enabled: thisVue.tooltipEnable,
          callbacks: {
            title: (context) => {
              // console.log(context)
              return context[0].label.replaceAll(',',' ');
            },
            label: (context) => {
              // console.log(context)
              return context.label.replaceAll(',',' ') + ": " + context.raw;
            },
          }
        },
        htmlLegend: {
          // ID of the container to put the legend in
          containerID: this.legendId,
        },
        legend: {
          display: false,
          position: 'top',
          width: 30,
        },
        datalabels: {
          // thisVuePluginDataLabels: this,
          display: this.valuesDisplay,
          color: 'black',
          anchor: 'end',
          font: {
            weight: 'bold',
            size: 14
          },
          padding: function(value) {
            // console.log(value)
          },
          align: 'top',
          formatter: function(value, context) {
            // console.log("context formatter",context)
            if(thisVue.valuesFormat == 'percent'){
              return (value > 0 ? (value / context.dataset.data.reduce((partialSum, a) => partialSum + a, 0) * 100).toFixed(2) + '%' : '');
            }
            return (value > 0 ? value : '');
          }
        }
      },
      categoryPercentage: 0.8,
      barPercentage: 1,
      // barThickness: (!this.stacked ? (90 / this.dataSets.length) : 90),
      scales: {
        x: {
          stacked: this.stacked,
          grid: {
            display: false
          },
          ticks: {
            color: thisVue.XValueColor
          },
        },
        y: {
          stacked: this.stacked,
          ticks: {
            stepSize: 1,
            beginAtZero: true,
            color: thisVue.YValueColor
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
          this.explodeName = this.dataLabels[legendItem[0].index];
        }
      }
    }
    this.plugins = [this.htmlLegendPlugin];
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
