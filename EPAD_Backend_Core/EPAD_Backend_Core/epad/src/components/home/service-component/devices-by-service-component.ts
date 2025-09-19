import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { serviceApi } from "@/$api/service-api";
import { isNullOrUndefined } from 'util';

@Component({
  name: "devicesbyservice",
  components: { HeaderComponent, DataTableComponent }
})
export default class DevicesByServiceComponent extends Mixins(ComponentBase) {
  /** Parameters */
  data = [];
  value = [];
  serviceData = [];
  selectedServiceIndex = 0;
  maxHeight = 400;
  connection;
  titles = [];
  mounted() {
    this.titles = [
      this.$t("ListDevicesSelected"),
      this.$t("ListDevicesUnSelected")
    ];
    this.getService();
  }

  getService() {
    serviceApi.GelAllService().then((res: any) => {
      if (res.status == 200) {
        let arrService = res.data;
        for (let i = 0; i < arrService.length; i++) {
          this.serviceData.push({
            index: arrService[i].Index,
            service: arrService[i].Name
          });
        }
      }
    });
  }
  getDevice() {
    if (this.selectedServiceIndex == 0) {
      return;
    }
    const data = [];
    serviceApi
      .GetDevicesInOutService(this.selectedServiceIndex)
      .then((res: any) => {
        if (res.status == 200) {
          let arrDevice = res.data;
          for (let i = 0; i < arrDevice.length; i++) {
            data.push({
              key: arrDevice[i].SerialNumber,
              label: arrDevice[i].AliasName + "(" + arrDevice[i].IPAddress + ")"
            });
              if (arrDevice[i].InService == false) {
              this.value.push(arrDevice[i].SerialNumber);
            }
          }
        }
      });
    return data;
  }

  downloadDevice() {
    this.data = this.getDevice();
    this.value = [];
  }

  updateDevicesByService() {
    if (this.selectedServiceIndex == 0) {
      return;
    }
    const arrSource = this.data;
    const arrUnSelect = this.value;
    const listSerial = [];
    for (let i = 0; i < arrSource.length; i++) {
      if (arrUnSelect.includes(arrSource[i].key) == false) {
        listSerial.push(arrSource[i].key);
      }
    }

    const dataUpdate = {
      Index: this.selectedServiceIndex,
      Name: "",
      Description: null,
      ListDeviceSerial: listSerial
    };
    serviceApi.UpdateService(dataUpdate).then((res: any) => {
      this.downloadDevice();
      if(!isNullOrUndefined(res.status) && res.status === 200) {
        this.$notify.success({
          title: "Thông báo",
          message: this.$t("UpdateDataSuccessfully").toString(),
          position: "top-right"
        });
      }
    });
  }
  handleCurrentChange(val) {
    this.data = [];
    this.value = [];
    this.selectedServiceIndex = val.index;
    this.data = this.getDevice();
  }
  MaxHeightTable() {
    this.maxHeight = window.innerHeight - 155
  }

  beforeMount() {
    this.maxHeight = window.innerHeight - 155
    window.addEventListener("resize", () => {
      this.MaxHeightTable()
    });
  }
}
