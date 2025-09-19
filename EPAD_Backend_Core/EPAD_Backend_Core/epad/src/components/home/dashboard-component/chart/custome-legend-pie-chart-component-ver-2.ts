import { Component, Vue, Mixins, Watch, Prop } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { Pie, Doughnut } from 'vue-chartjs/legacy';
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
import {helpers, color, desaturate, lighten, darken, rotate, rgbString} from 'chart.js/helpers';

ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale)

@Component({
  name: "CustomLegendPieChartVer2",
  components: { Pie, Doughnut },
})
export default class CustomLegendPieChartVer2Component extends Mixins(ComponentBase) {
  chartData: any;
  chartOptions: any;
  @Prop() index: any;
  @Prop({ default: () => "" }) id: any;
  legendId: any;
  @Prop({ default: () => "" }) name: any;
  @Prop({ default: () => [] }) dataIds: any;
  @Prop({ default: () => [] }) dataLabels: any;
  @Prop({ default: () => [] }) dataValues: any;
  @Prop({ default: () => true }) isDisplayName: any;
  @Prop({ default: () => true }) isDisplayCustomLegend: any;
  @Prop({ default: () => true }) isRandomColor : any;
  @Prop({ default: () => false }) isApplyMainColor : any;
  @Prop({ default: () => 240 }) mainHColor : any;
  @Prop({ default: () => false }) isRandomRGB : any;
  @Prop({ default: () => [] }) colorData : any;
  @Prop({ default: () => 40 }) padding : any;
  @Prop() colorDataLabel: any;
  @Prop({ default: () => false }) isSyncDataColorLabel: any;
  @Prop({ default: () => false }) isDisplayDataLabel: any;
  @Prop({ default: () => 'end' }) dataLabelPosition: any;
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
    default: 'custom-legend-pie-chart-ver-2'
  }
  datasetIdKey: {
    type: String,
    default: 'label'
  }
  cssClasses: {
    default: 'custom-legend-pie-chart-ver-2-css',
    type: String
  }
  styles: {
    type: Object,
    default: () => {}
  }
  plugins: any;

  load = true;
  isFirstRender = true;

  totalvalue = 0;

  htmlLegendPlugin = {
    thisVue: this,
    getOrCreateLegendList(chart, id){
      const legendContainer = document.getElementById(id);
      let listContainer = legendContainer.querySelector('ul');
    
      if (!listContainer) {
        listContainer = document.createElement('ul');
        listContainer.style.height = '100%';
        listContainer.style.display = 'flex';
        listContainer.style.flexDirection = 'column';
        listContainer.style.overflowY = 'auto';
        listContainer.style.margin = '0';
        listContainer.style.padding = '0';
    
        legendContainer.appendChild(listContainer);
      }
    
      return listContainer;
    },
    id: 'htmlLegend',
    // beforeUpdate(chart, args, options) {
    //   console.log("before update")
    // },
    // afterDraw(chart, args, options){
    //   console.log("afterDraw")
    // },
    // afterRender(chart, args, options){
    //   console.log("afterRender")
    // },
    // afterDatasetDraw(chart, args, options){
    //   console.log("afterDatasetDraw")
    // },
    afterUpdate(chart, args, options) {
      if(this.thisVue.isDisplayCustomLegend){
        // console.log("after update")
        // if(chart.$datalabels && chart.$datalabels._labels){
        //   chart.$datalabels._labels.forEach((element, index) => {
        //     this.thisVue.totalvalue += this.thisVue.dataValues[element._index];
        //   });
        //   console.log("total", this.thisVue.totalvalue)
        //   // chart.$datalabels._labels.forEach((element, index) => {
        //   //   console.log(this.thisVue.dataValues[element._index])
        //   //   console.log((this.thisVue.dataValues[element._index] / this.thisVue.totalvalue) * 100)
        //   //   if(((this.thisVue.dataValues[element._index] / this.thisVue.totalvalue) * 100) > 3){
        //   //     console.log(true)
        //   //     chart.$datalabels._labels[index]._config.display = true;
        //   //     chart.$datalabels._labels[index]._model.display = true;
        //   //   }else{
        //   //     console.log(false)
        //   //     chart.$datalabels._labels[index]._config.display = false;
        //   //     chart.$datalabels._labels[index]._model.display = false;
        //   //   }
        //   // });
        // }

        // console.log(chart)
        // console.log(options)

        const ul = this.getOrCreateLegendList(chart, options.containerID);

        // Remove old legend items
        while (ul.firstChild) {
          ul.firstChild.remove();
        }
    
        // Reuse the built-in legendItems generator
        const items = chart.options.plugins.legend.labels.generateLabels(chart);
    
        items.forEach((item, index) => {
          const boxColor = this.thisVue.isRandomRGB ? ("linear-gradient(to right, " + this.thisVue.colorData[index] 
            + ", " + color(this.thisVue.colorData[index]).desaturate(0).darken(0).rotate(180).lighten(0).rgbString() + ")")
            : this.thisVue.colorData[index];
          
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
          boxSpan.style.background = boxColor;
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

        // chart.data.datasets.forEach((dataset) => {
        //   console.log(chart.getDatasetMeta(0));
        //   var metaDatum = [];
        //   dataset.data.forEach((value, index) => {
        //     const element = chart.getDatasetMeta(0).data[index];
        //     console.log(element)
        //     // const rectangle = chart.canvas.getBoundingClientRect();
        //     // console.log(rectangle)
        //     // const point = element.getCenterPoint();
        //     // console.log(point.x, point.y)

        //     // const mouseMoveEvent = new MouseEvent('mousemove', {
        //     //   clientX: rectangle.left + point.x,
        //     //   clientY: rectangle.top + point.y
        //     // });
        //     // const mouseOutEvent = new MouseEvent('mouseout');

        //     // chart.canvas.dispatchEvent(mouseMoveEvent);
        //     // chart.canvas.dispatchEvent(mouseOutEvent);

        //     // chart.updateHoverStyle([element], null, true);
        //     // chart.render();

        //     // chart.setActiveElements([element]);
        //     // chart.updateHoverStyle([element], 'dataset', true);
        //     // chart.render();

        //     metaDatum.push(chart.getDatasetMeta(0).data[index]);
        //   });
        //   // chart.updateHoverStyle(metaDatum, 'dataset', true);
        // });
      }
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

  // randomColor(){
  //   var letters = '0123456789ABCDEF';
  //   var color = '#';
  //   for (var i = 0; i < 6; i++) {
  //     color += letters[Math.floor(Math.random() * 16)];
  //   }
  //   return color;
  // }

  randomColor() {
    let color;
    let isValid = false;
  
    while (!isValid) {
      let letters = '0123456789ABCDEF';
      color = '#';
      for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
      }
  
      // Convert the color string to RGB values
      let r = parseInt(color.substr(1, 2), 16);
      let g = parseInt(color.substr(3, 2), 16);
      let b = parseInt(color.substr(5, 2), 16);
  
      // Check if the color is within the valid range
      let brightness = (r * 299 + g * 587 + b * 114) / 1000;
      isValid = (brightness >= 60 && brightness <= 200);
    }
  
    return color;
  }

  randomRGBColor(){
    const r = Math.floor(Math.random() * 255);
    const g = Math.floor(Math.random() * 255);
    const b = Math.floor(Math.random() * 255);
    return "rgb(" + r + "," + g + "," + b + ")";
  }

  randomRGBColorExcludeBlackAndWhite() {
    let r, g, b;
    
    do {
      r = Math.floor(Math.random() * 256);
      g = Math.floor(Math.random() * 256);
      b = Math.floor(Math.random() * 256);
    } while ((r === 255 && g === 255 && b === 255) || (r === 0 && g === 0 && b === 0));
  
    return "rgb(" + r + "," + g + "," + b + ")";
  }

  randomHSLColor(){
    //Math.floor(Math.random() * (360 - 240 + 1)) + 240
    const h = Math.floor(Math.random() * (360 - 240 + 1)) + 240;
    const s = Math.floor(Math.random() * (100 - 70 + 1)) + 70;
    const l = Math.floor(Math.random() * (80 - 30 + 1)) + 30;
    return "hsl(" + h + "," + s + "%," + l + "%)";
  }

  randomHSLColorWithMainColor(){
    const minH = this.mainHColor - 20;
    const maxH = this.mainHColor + 20;
    let h = Math.floor(Math.random() * (maxH - minH + 1)) + minH;
    do {
      h = Math.floor(Math.random() * (maxH - minH + 1)) + minH;
    } while (h % 5 !== 0);
    const s = Math.floor(Math.random() * (100 - 70 + 1)) + 70;
    const l = Math.floor(Math.random() * (80 - 30 + 1)) + 30;
    // console.log("hsl(" + h + "," + s + "%," + l + "%)")
    return "hsl(" + h + "," + s + "%," + l + "%)";
  }

  createRadialGradient3(context, c1, c2, c3) {
    const cache = new Map();
    let width = null;
    let height = null;

    const chartArea = context.chart.chartArea;
    if (!chartArea) {
      // This case happens on initial chart load
      return;
    }
  
    const chartWidth = chartArea.right - chartArea.left;
    const chartHeight = chartArea.bottom - chartArea.top;
    if (width !== chartWidth || height !== chartHeight) {
      cache.clear();
    }
    let gradient = cache.get(c1 + c2 + c3);
    if (!gradient) {
      // Create the gradient because this is either the first render
      // or the size of the chart has changed
      width = chartWidth;
      height = chartHeight;
      const centerX = (chartArea.left + chartArea.right) / 2;
      const centerY = (chartArea.top + chartArea.bottom) / 2;
      const r = Math.min(
        (chartArea.right - chartArea.left) / 2,
        (chartArea.bottom - chartArea.top) / 2
      );
      const ctx = context.chart.ctx;
      gradient = ctx.createRadialGradient(centerX, centerY, 0, centerX, centerY, r);
      gradient.addColorStop(0, c1);
      gradient.addColorStop(0.5, c2);
      gradient.addColorStop(1, c3);
      cache.set(c1 + c2 + c3, gradient);
    }
  
    return gradient;
  }

  calculateTotalValue(){
    this.totalvalue = 0;
    this.dataValues.forEach((element, index) => {
      this.totalvalue += element;
    });
  }
  
  beforeMount() {
    this.plugins = [this.htmlLegendPlugin];

    this.legendId = this.id + '_' + this.index;
    // console.log("AAA")
    if(this.isRandomColor){
      this.colorData = [];
      this.dataValues.forEach(() => {
        if(this.isApplyMainColor){
          // console.log("apply")
          this.colorData.push(this.randomHSLColorWithMainColor());
        }else{
          this.colorData.push(this.randomColor());
        }
      });
    }
    const thisVue = this;
    this.chartData = {
      labels: this.dataLabels,
      datasets: [
        {
          chartId: this.id,
          chartName: this.name,
          dataid: this.dataIds,
          backgroundColor: function(context) {
            // console.log("element arc context", context)
            if(!thisVue.isRandomRGB){
              return thisVue.colorData[context.dataIndex];
            }else{
              let c = thisVue.colorData[context.dataIndex];
              if (!c) {
                return;
              }
              
              // if (context.active) {
              //   c = helpers.getHoverColor(c);
              // }
  
              const start = color(c).lighten(0).rgbString();
              const mid = color(c).desaturate(0).darken(0).rotate(180).lighten(0).rgbString();
              const end = color(c).lighten(0).rgbString();
              return thisVue.createRadialGradient3(context, start, mid, end);
            }
          },
          borderWidth: 0,
          data: this.dataValues,
        }
      ],
    }

    this.chartOptions = {
      layout: {
        padding: thisVue.padding,
      },
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
          // display: !this.isDisplayDataLabel ? 'auto' : false,
          display:function(context) {
            thisVue.calculateTotalValue();
            return !thisVue.isDisplayDataLabel ? (
              ((thisVue.dataValues[context.dataIndex] / thisVue.totalvalue) * 100) > 3 
              ? (thisVue.dataValues[context.dataIndex] > thisVue.dataValues[context.dataIndex + 1] 
                || thisVue.dataValues[context.dataIndex] > thisVue.dataValues[context.dataIndex - 1]
                ? true : 'auto') : 'auto'
            ) : false;
          },

          //// Hard custom display context -> not work
          // display: function(context) {
          //   console.log(context)
            
          //   if(context.chart && context.chart.$datalabels 
          //     && context.chart.$datalabels._labels && context.chart.$datalabels._labels.length > 0){
          //     console.log("aaa", context.chart.$datalabels._labels)
          //     thisVue.totalvalue = 0;
          //     context.chart.$datalabels._labels.forEach((element, index) => {
          //       if(index == 0){
          //         console.log("start count")
          //       }
          //       thisVue.totalvalue += thisVue.dataValues[element._index];
          //     });
          //     console.log("total", thisVue.totalvalue)

          //     // return true;
          //     return ((thisVue.dataValues[context.dataIndex] / thisVue.totalvalue) * 100) > 3;
          //   }else{
          //     return false;
          //   }
          // },

          // color: '#33cc5e', // green
          color: function(context) {
            if(thisVue.isSyncDataColorLabel){
              if(!thisVue.isRandomRGB){
                return thisVue.colorData[context.dataIndex];
              }else{
                const ctx = context.chart.ctx;
                const gradient = ctx.createLinearGradient(0, 10, 10, 10);
                gradient.addColorStop(0, thisVue.colorData[context.dataIndex]);
                gradient.addColorStop(1, color(thisVue.colorData[context.dataIndex]).desaturate(0).darken(0).rotate(180).lighten(0).rgbString());
                // return "linear-gradient(to right, " + thisVue.colorData[context.dataIndex] 
                //   + ", " + color(thisVue.colorData[context.dataIndex]).desaturate(0).darken(0).rotate(180).lighten(0).rgbString() + ")";
                return gradient;
              }
            }else{
              return thisVue.colorDataLabel != null ? thisVue.colorDataLabel : 'black';
            }
          },
          anchor: this.dataLabelPosition,
          font: {
            weight: 'bold',
            size: 12
          },
          align: this.dataLabelPosition,
          formatter: function(value, context) {
            return (thisVue.dataLabels[context.dataIndex].length > 10 
              ? (thisVue.dataLabels[context.dataIndex].substring(0, 10) + '...') : thisVue.dataLabels[context.dataIndex]) 
              + " " + '\n' + ((value / thisVue.totalvalue) * 100).toFixed(2) + '%';
          }
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
          this.explodeName = this.dataLabels[legendItem[0].index];
        }
        // else{
        //   this.$emit('clickedItem', null);
        // }

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
      },
    }
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
