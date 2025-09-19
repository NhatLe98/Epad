import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { GridLayout, GridItem } from "vue-grid-layout"
import DepartmentEmployeePieChart from "@/components/home/dashboard-component/custom-chart/department-employee-pie-chart-component.vue";
import RootDepartmentEmployeePieChart from "@/components/home/dashboard-component/custom-chart/root-department-employee-pie-chart-component.vue";
import DeviceInfoBarChart from "@/components/home/dashboard-component/custom-chart/device-info-bar-chart-component.vue";
import DeviceOfflineTimesBarChart from "@/components/home/dashboard-component/custom-chart/device-offline-times-bar-chart-component.vue";
import TransferEmployeeTimesBarChart from "@/components/home/dashboard-component/custom-chart/transfer-employee-times-bar-chart-component.vue";
import AttendanceLogTimesBarChart from "@/components/home/dashboard-component/custom-chart/attendance-log-times-bar-chart-component.vue";
import DeviceLogAmountBarChart from "@/components/home/dashboard-component/custom-chart/device-log-amount-bar-chart-component.vue";
import InOutLogByDoorBarChart from "@/components/home/dashboard-component/custom-chart/in-out-log-by-door-bar-chart-component.vue";
import InOutLogByDeviceBarChart from "@/components/home/dashboard-component/custom-chart/in-out-log-by-device-bar-chart-component.vue";
import EmployeePresentByRootDepartmentBarChart from "@/components/home/dashboard-component/custom-chart/employee-present-by-root-department-bar-chart-component.vue";
import EmployeePresentByUserTypeBarChart from "@/components/home/dashboard-component/custom-chart/employee-present-by-user-type-bar-chart-component.vue";
import EmployeePresentByAreaGroupBarChart from "@/components/home/dashboard-component/custom-chart/employee-present-by-area-group-bar-chart-component.vue";
import VehicleEmployeePresentBarChart from "@/components/home/dashboard-component/custom-chart/vehicle-employee-present-bar-chart-component.vue";
import EmployeePresentInFactoryBarChart from "@/components/home/dashboard-component/custom-chart/employee-present-in-factory-bar-chart-component.vue";
import { deviceApi } from '@/$api/device-api';
import { dashboardApi} from '@/$api/dashboard-api';
import XLSX from 'xlsx';

@Component({
  name: "dashboard",
  components: { HeaderComponent, GridLayout, GridItem, 
    DepartmentEmployeePieChart, 
    RootDepartmentEmployeePieChart, 
    DeviceInfoBarChart,
    DeviceOfflineTimesBarChart,
    TransferEmployeeTimesBarChart,
    AttendanceLogTimesBarChart,
    DeviceLogAmountBarChart,
    InOutLogByDoorBarChart, 
    InOutLogByDeviceBarChart,
    EmployeePresentByRootDepartmentBarChart,
    EmployeePresentByUserTypeBarChart,
    EmployeePresentByAreaGroupBarChart,
    VehicleEmployeePresentBarChart,
    EmployeePresentInFactoryBarChart }
})
export default class DashboardComponent extends Mixins(ComponentBase) {
  dashboardModules: any;
  onlineDevicePercentage = 0;
  viewOnlineDevicePercentage = 0;
  onlineDeviceName = '';
  offlineDevicePercentage = 0;
  viewOfflineDevicePercentage = 0;
  offlineDeviceName = '';
  listChart = [];
  listSelectedChart = [];
  dragging = -1;
  layout = [];
  draggable = false;
  resizable = false;

  listDeviceData = [];

  isShowOnOffDeviceData = false;
  listOnOffDeviceData = [];
  onOffDeviceDataColukmns = [
    {
      prop: 'SerialNumber',
      label: this.$t('SerialNumber'),
    },
    {
        prop: 'AliasName',
        label: this.$t('AliasName'),
    },
    {
        prop: 'IPAddress',
        label: this.$t('IPAddress'),
    },
    {
        prop: 'Port',
        label: this.$t('Port'),
    },
    {
        prop: 'LastConnection',
        label: this.$t('LastConnection'),
    }
  ];

  isEdit = false;
  timer = null;
  activeNames = 'Dashboard';

  enableChartLevelBackground = false;

  beforeMount(){

    Misc.readFileAsync('static/variables/common-utils.json').then((res: any) => {
      if(res && res.DashboardModule && res.DashboardModule != ""){
        this.dashboardModules = res.DashboardModule.split(",");
        this.enableChartLevelBackground = res?.EnableChartLevelBackground ?? false;
      }
    });

    Misc.readFileAsync('static/variables/dashboard.json').then((res: any) => {
      if(res && res.length > 0){
        res.forEach(element => {
          this.listChart.push({
            id: element.id,
            name: element.name,
            component: element.component,
            module: element.module,
            selected: 0,
            dataConfig: element.dataConfig
          });
        });
      }
    });

    dashboardApi.GetDashboard().then((res: any) => {
      // console.log(res)
      if(res && res.data){
        this.listSelectedChart = res.data;
      }else{
        var listSelectedChart = localStorage.getItem('epad_dashboard');
        if(listSelectedChart){
          this.listSelectedChart = JSON.parse(listSelectedChart);
        }
      }
      this.listSelectedChart.forEach((element, index) => {
        const chart = this.listChart.find(x => x.id == element.id);
        if(chart){
          element.module = chart.module;
        }
      });
      this.reDrawLayout();
    });

    deviceApi.GetListDeviceInfo().then((res: any) => {
      // console.log(res)
      if(res.data && res.data.data){
        this.listDeviceData = res.data.data;
        if(this.listDeviceData){
          this.listDeviceData.forEach(element => {
            if(element.Status && element.Status != "Offline" && element.Status != "Online"){
              element.Status = "Online";
            }
          });
        }
        const groupDevice = res.data.data.reduce((grouped, device) => {
          const key = device.Status.toString();
        
          if (!grouped[key]) {
            grouped[key] = [];
          }
        
          grouped[key].push(device);
        
          return grouped;
        }, {} as { [key: string]: [] });
        if(!groupDevice.Online){
          groupDevice["Online"] = [];
        }
        if(!groupDevice.Offline){
          groupDevice["Offline"] = [];
        }
        this.onlineDevicePercentage = Math.round(groupDevice.Online.length / res.data.data.length * 100);
        this.offlineDevicePercentage = 100 - this.onlineDevicePercentage;
        this.viewOnlineDevicePercentage = this.onlineDevicePercentage;
        this.viewOfflineDevicePercentage = this.offlineDevicePercentage;

        if(this.onlineDevicePercentage > 0 && this.onlineDevicePercentage < 20){
          this.viewOnlineDevicePercentage = 20;
          this.viewOfflineDevicePercentage = 80;
        }
        if(this.offlineDevicePercentage > 0 && this.offlineDevicePercentage < 20){
          this.viewOfflineDevicePercentage = 20;
          this.viewOnlineDevicePercentage = 80;
        }

        this.onlineDeviceName = groupDevice.Online.length + "/" + res.data.data.length + " - " 
          + this.onlineDevicePercentage + "%";
        this.offlineDeviceName = groupDevice.Offline.length + "/" + res.data.data.length + " - " 
          + this.offlineDevicePercentage + "%";
      }
    });
    this.setTimer();
  }

  setTimer() {
    this.timer = setInterval(() => {
      deviceApi.GetListDeviceInfo().then((res: any) => {
        // console.log(res)
        if(res.data && res.data.data){
          this.listDeviceData = res.data.data;
          const groupDevice = res.data.data.reduce((grouped, device) => {
            const key = device.Status.toString();
          
            if (!grouped[key]) {
              grouped[key] = [];
            }
          
            grouped[key].push(device);
          
            return grouped;
          }, {} as { [key: string]: [] });
          if(!groupDevice.Online){
            groupDevice["Online"] = [];
          }
          if(!groupDevice.Offline){
            groupDevice["Offline"] = [];
          }
          this.onlineDevicePercentage = Math.round(groupDevice.Online.length / res.data.data.length * 100);
          this.offlineDevicePercentage = 100 - this.onlineDevicePercentage;
          this.viewOnlineDevicePercentage = this.onlineDevicePercentage;
          this.viewOfflineDevicePercentage = this.offlineDevicePercentage;

          if(this.onlineDevicePercentage > 0 && this.onlineDevicePercentage < 20){
            this.viewOnlineDevicePercentage = 20;
            this.viewOfflineDevicePercentage = 80;
          }
          if(this.offlineDevicePercentage > 0 && this.offlineDevicePercentage < 20){
            this.viewOfflineDevicePercentage = 20;
            this.viewOnlineDevicePercentage = 80;
          }
  
          this.onlineDeviceName = groupDevice.Online.length + "/" + res.data.data.length + " - " 
            + this.onlineDevicePercentage + "%";
          this.offlineDeviceName = groupDevice.Offline.length + "/" + res.data.data.length + " - " 
            + this.offlineDevicePercentage + "%";
        }
      });
    }, 30000);
  }

  mounted(){

  }

  showOnOffDeviceData(status: string){
    if(status.toLowerCase() == "online"){
      this.listOnOffDeviceData = Misc.cloneData(this.listDeviceData.filter(x => x.Status.toLowerCase() == "online"));
    }else{
      this.listOnOffDeviceData = Misc.cloneData(this.listDeviceData.filter(x => x.Status.toLowerCase() == "offline"));
    }
    this.isShowOnOffDeviceData = true;
  }

  exportOnOffDeviceDataToExcel() {
    const data = this.listOnOffDeviceData;
    let formatData = [];
    if(data && data.length){
      for(let i = 0; i < data.length; i++){
        let obj = {};
        this.onOffDeviceDataColukmns.forEach(element => {
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
    this.onOffDeviceDataColukmns.forEach(element => {
      cellsWidth.push({width: 30});
    });
    dataWs['!cols'] = cellsWidth;

    // A workbook is the name given to an Excel file
    var wb = XLSX.utils.book_new() // make Workbook of Excel

    // add Worksheet to Workbook
    // Workbook contains one or more worksheets
    XLSX.utils.book_append_sheet(wb, dataWs, "ExportData") // sheetAName is name of Worksheet

    // export Excel file
    XLSX.writeFile(wb, 'ExportOnOffDeviceData.xlsx') // name of the file is 'book.xlsx'
  }

  editDashboard(){
    this.draggable = true;
    this.resizable = true;
    this.listSelectedChart.forEach(element => {
      element.static = false;
    });
    this.reDrawLayout();
    this.isEdit = true;
  }

  saveDashboard(){
    this.draggable = false;
    this.resizable = false;
    this.listSelectedChart.forEach(element => {
      element.static = true;
    });
    this.reDrawLayout();
    localStorage.setItem("epad_dashboard", JSON.stringify(this.listSelectedChart));
    let dataSelectedChart = Misc.cloneData(this.listSelectedChart);
    dataSelectedChart.forEach(element => {
      if(element.dataConfig){
        delete element.dataConfig;
      }
    });
    dashboardApi.SaveDashboard(JSON.stringify(this.listSelectedChart)).then((res: any) => {
      if(res.data){
        this.$notify({
          type: 'success',
          title: this.$t('Notify').toString(),
          dangerouslyUseHTMLString: true,
          message: this.$t('MSG_SaveSuccess').toString(),
          customClass: 'notify-content',
          duration: 8000
        });
      }else{
        this.$notify({
          type: 'warning',
          title: this.$t('Warning').toString(),
          dangerouslyUseHTMLString: true,
          message: this.$t('MSG_SaveFailed').toString(),
          customClass: 'notify-content',
          duration: 8000
        });
      }
    });
    this.$nextTick(() => {
      this.isEdit = false;
    });
  }

  itemTitle(item) {
    let result = item.i;
    if (item.static) {
        result += " - Static";
    }
    return result;
  }

  dragStart(index, ev) {
    // ev.dataTransfer.setData('Text', this.id);
    ev.dataTransfer.dropEffect = 'move';
    this.dragging = index;
  }
  dragEnter(ev) {
    /* 
    if (ev.clientY > ev.target.height / 2) {
      ev.target.style.marginBottom = '10px'
    } else {
      ev.target.style.marginTop = '10px'
    }
    */
  }
  dragLeave(ev) {
    /* 
    ev.target.style.marginTop = '2px'
    ev.target.style.marginBottom = '2px'
    */
  }
  dragEnd(ev) {
    this.dragging = -1
  }
  dragFinish(to, ev) {
    this.moveItem(ev, this.dragging, to);
    /*
    ev.target.style.marginTop = '2px'
    ev.target.style.marginBottom = '2px'
    */
  }

  changeLanguage(value){
    this.reDrawLayout();
  }

  handleChartLevelBackgroundColor(id, isSafe, isDanger){
    const updateLayout = this.layout.find(x => x.id == id);
    if(updateLayout){
      updateLayout.isSafe = isSafe;
      updateLayout.isDanger = isDanger;
      updateLayout.updateLevelBackgroundColor = true;
      this.$forceUpdate();
    }
  }

  reDrawLayout(){
    this.layout = [];
    if(this.dashboardModules){
      this.listSelectedChart = this.listSelectedChart.filter(x => this.dashboardModules.includes(x.module));
    }
    // console.log(this.listSelectedChart)
    if(this.listChart && this.listChart.length > 0){
      this.listSelectedChart.forEach((item) => {
        const dashboard = this.listChart.find(x => x.id == item.id);
        if(dashboard){
          item.name = this.$t(dashboard.name).toString();
          item.dataConfig = dashboard.dataConfig;
        }
      });
    }
    this.listSelectedChart.forEach((element, index) => {
        this.layout.push({
          i : index.toString(),
          id: element.id,
          x : element.x,
          y : element.y,
          w : element.w,
          h : element.h,
          static : element.static,
          dataConfig: element.dataConfig
        });
        element.i = index.toString();
        element.name = this.$t(element.name).toString();
    });

    this.listChart.forEach((item) => {
      item.selected = this.listSelectedChart.filter(x => x.id == item.id).length;
    });
  }

  @Watch("layout", {deep: true})
  updateLayout(){
    this.layout.forEach((element, index) => {
      this.listSelectedChart[index].x = element.x;
      this.listSelectedChart[index].y = element.y;
      this.listSelectedChart[index].w = element.w;
      this.listSelectedChart[index].h = element.h;
      this.listSelectedChart[index].static = element.static;
    });
  }

  moveItem(event, from, to) {
    if (to === -1) {
      // console.log("inZone");
      // console.log("indexItem",from);

      if(!this.layout){
        this.layout = [];
      }
      
      const newChart = this.listChart.find(x => x.id == from);
      if(newChart && this.dashboardModules.includes(newChart.module)){
        this.listSelectedChart.push(newChart);
      }
      this.listSelectedChart.forEach((item, index) => {
        const newItem = { ...item };
        newItem.index = index;
        if(!newItem.x){
          newItem.x = (index % 2) * 6;
        }
        if(!newItem.y){
          newItem.y = (index / 2) * 6;
        }
        if(!newItem.w){
          newItem.w = 6;
        }
        if(!newItem.h){
          newItem.h = 8;
        }
        if(!newItem.static){
          newItem.static = false;
        }
        this.listSelectedChart[index] = newItem;
      });
      this.reDrawLayout();

      // console.log(this.listSelectedChart)
    } else {


      // console.log("outZone");
    }
  }

  closeChart(index){
    this.listSelectedChart = this.listSelectedChart.filter(x => x.index != index);
    this.reDrawLayout();
  }

  beforeDestroy() {
    clearInterval(this.timer);
}
}
