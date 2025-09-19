import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";

import { departmentAndDeviceApi, IC_DepartmentAndDevice } from "@/$api/department-api";
import { isNullOrUndefined } from 'util';
import { groupDeviceApi, IC_GroupDevice, IC_GroupDeviceDetails } from "@/$api/group-device-api";
import { doorApi } from "@/$api/ac-door-api";
import { areaApi } from "@/$api/ac-area-api";
import { AC_GroupAreaParam, areaDoorApi } from "@/$api/ac-area-door-api";
import { AC_GroupDoorParam, doorDeviceApi } from "@/$api/ac-door-device-api";

@Component({
    name: "DoorDevice",
    components: { HeaderComponent, DataTableComponent }
})
export default class DoorDeviceComponent extends Mixins(ComponentBase) {
    departmentData = [];
    selectedGroupDevicendex = 0;
    data = [];
    value = [];
    maxHeight = 400;
    mounted() {

        this.GetGroupDevice();
    }

    GetGroupDevice() {
        doorApi.GetAllDoor().then((res: any) => {
            if (res.status == 200) {
                const arrGroupDevice = res.data;
                for (let i = 0; i < arrGroupDevice.length; i++) {
                    this.departmentData.push({
                        index: arrGroupDevice[i].value,
                        service: arrGroupDevice[i].label
                    });
                }
            }
        });
    }
    downloadDevice() {
        /* Dòng thứ 2 gán [] trong khi dòng thứ nhất sau khi gọi axios có thay đổi giá trị dòng 2??? */
        // this.data = this.getDevice();
        // this.value = [];
        this.value = [];
        this.data = this.getDevice();
    }
    getDevice() {
        if (this.selectedGroupDevicendex == 0) {
            return;
        }
        const data = [];
        doorDeviceApi
            .GetDeviceInOutDoor(this.selectedGroupDevicendex)
            .then((res: any) => {
                if (res.status == 200) {
                    let arrDevice = res.data;
                    for (let i = 0; i < arrDevice.length; i++) {
                        data.push({
                            key: arrDevice[i].SerialNumber,
                            label: arrDevice[i].DeviceName + "(" + arrDevice[i].IPAddress + ")"
                        });
                        if (arrDevice[i].InDoor == false) {
                            this.value.push(arrDevice[i].SerialNumber);
                        }
                    }
                }
            });
        return data;
    }

    updateDevicesByDepartment() {
        console.log(this.selectedGroupDevicendex);
        if (this.selectedGroupDevicendex == 0) {
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

        console.log(listSerial);

        const dataUpdate: AC_GroupDoorParam = {
            DoorIndex: this.selectedGroupDevicendex,
            ListDevice: listSerial
        };
        
        doorDeviceApi
            .UpdateDoorDeviceDetail(dataUpdate)
            .then(res => {
                this.downloadDevice();
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$notify.success({
                        title: "Thông báo",
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
        this.selectedGroupDevicendex = val.index;
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