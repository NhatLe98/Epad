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
  name: "CustomLegendPieChart",
  components: { Pie },
})
export default class CustomLegendPieChartComponent extends Mixins(ComponentBase) {
  chartData: any;
  chartOptions: any;
  @Prop() index: any;
  @Prop({ default: () => "" }) id: any;
  legendId: any;
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
    default: 'custom-legend-pie-chart'
  }
  datasetIdKey: {
    type: String,
    default: 'label'
  }
  cssClasses: {
    default: 'custom-legend-pie-chart-css',
    type: String
  }
  styles: {
    type: Object,
    default: () => {}
  }
  plugins: any;

  load = true;

  htmlLegendPlugin = {
    getOrCreateLegendList(chart, id){
      const legendContainer = document.getElementById(id);
      let listContainer = legendContainer.querySelector('ul');
    
      if (!listContainer) {
        listContainer = document.createElement('ul');
        listContainer.style.height = '100%';
        listContainer.style.display = 'flex';
        listContainer.style.flexDirection = 'column';
        listContainer.style.overflowY = 'scroll';
        listContainer.style.margin = '0';
        listContainer.style.padding = '0';
    
        legendContainer.appendChild(listContainer);
      }
    
      return listContainer;
    },
    id: 'htmlLegend',
    afterUpdate(chart, args, options) {
      const ul = this.getOrCreateLegendList(chart, options.containerID);
  
      // Remove old legend items
      while (ul.firstChild) {
        ul.firstChild.remove();
      }
  
      // Reuse the built-in legendItems generator
      const items = chart.options.plugins.legend.labels.generateLabels(chart);
  
      items.forEach(item => {
        const li = document.createElement('li');
        li.style.alignItems = 'center';
        li.style.cursor = 'pointer';
        li.style.display = 'flex';
        li.style.flexDirection = 'row';
        li.style.marginLeft = '10px';
        li.style.marginTop = '5px';
        li.style.marginBottom = '5px';
  
        li.onclick = () => {
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
        boxSpan.style.height = '20px';
        boxSpan.style.marginRight = '20px';
        boxSpan.style.width = '20px';
        boxSpan.style.border = '1px solid black';
        boxSpan.style.flexShrink = '0';
  
        // Text
        const textContainer = document.createElement('p');
        textContainer.style.color = item.fontColor;
        textContainer.style.fontSize = '12px';
        textContainer.style.lineHeight = '20px';
        textContainer.style.margin = '0';
        textContainer.style.padding = '0';
        textContainer.style.overflow = 'hidden';
        textContainer.style.whiteSpace = 'nowrap';
        textContainer.style.textOverflow = 'ellipsis';
        textContainer.style.textDecoration = item.hidden ? 'line-through' : '';
  
        const text = document.createTextNode(item.text);
        textContainer.appendChild(text);
        li.setAttribute("title", item.text);
  
        li.appendChild(boxSpan);
        li.appendChild(textContainer);
        ul.appendChild(li);
      });
    }
  };

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
    this.legendId = this.id + '_' + this.index;
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
        htmlLegend: {
          // ID of the container to put the legend in
          containerID: this.legendId,
        },
        legend: {
          display: false,
          position: 'right',
          width: 30,
          scrollable: true
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
    this.plugins = [this.htmlLegendPlugin];
  }

  mounted(){
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
