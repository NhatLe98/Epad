import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { gatesLinesApi, GatesLines } from '@/$api/gc-gates-lines-api';

import { isNullOrUndefined } from 'util';

@Component({
  name: "gates-detail",
  components: { HeaderComponent, DataTableComponent }
})
export default class GatesDetailComponent extends Mixins(ComponentBase) {
  gatesData = [];
  selectedGateIndex = 0;
  data = [];
  value = [];
  maxHeight = 400;
  mounted() {
    this.getDepartment();
  }

  getDepartment() {
    gatesLinesApi.GetAllGates().then((res: any) => {
      if (res.status == 200) {
        const arrService = res.data;
        for (let i = 0; i < arrService.length; i++) {
          this.gatesData.push({
            index: arrService[i].Index,
            gate: arrService[i].Name
          });
        }
      }
    });
  }
  downloadGate() {
    /* Dòng thứ 2 gán [] trong khi dòng thứ nhất sau khi gọi axios có thay đổi giá trị dòng 2??? */
    // this.data = this.getDevice();
    // this.value = [];
    this.value = [];
    this.data = this.getGates();
  }
  getGates() {
    if (this.selectedGateIndex == 0) {
      return;
    }
    const data = [];
    gatesLinesApi
    .GetByGateIndex(this.selectedGateIndex)
    .then((res: any) => {
      if (res.status == 200) {
        let arrDevice = res.data;
        for (let i = 0; i < arrDevice.length; i++) {
          data.push({
            key: arrDevice[i].LineIndex,
            label: arrDevice[i].LineName
          });
          if (arrDevice[i].InGroup == false) {
            this.value.push(arrDevice[i].LineIndex);
          }
        }
      }
    });
    return data;
  }

  updateGatesLines() {
    if (this.selectedGateIndex == 0) {
      return;
    }
    const arrSource = this.data;
    const arrUnSelect = this.value;
    const listSerial: Array<string> = [];
    for (let i = 0; i < arrSource.length; i++) {
      if (arrUnSelect.includes(arrSource[i].key) == false) {
        listSerial.push(arrSource[i].key);
      }
    }

    const dataUpdate: GatesLines = {
      GateIndex: this.selectedGateIndex,
      ListLineIndex: listSerial
    };

    gatesLinesApi
    .UpdateByGateIndex(dataUpdate.GateIndex, dataUpdate.ListLineIndex)
    .then(res => {
      this.downloadGate();
      if(!isNullOrUndefined(res.status) && res.status === 200) {
        this.$notify.success({
          title: this.$t('Notify').toString(),
          message: this.$t("UpdateDataSuccessfully").toString(),
          position: "top-right"
        });
      }
      // this.downloadDevice();
    });
  }
  handleCurrentChange(val) {
    this.data = [];
    this.value = [];
    this.selectedGateIndex = val.index;
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