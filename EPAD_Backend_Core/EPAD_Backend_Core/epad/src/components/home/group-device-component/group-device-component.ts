import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";

import { departmentAndDeviceApi, IC_DepartmentAndDevice } from "@/$api/department-api";
import { isNullOrUndefined } from 'util';
import { groupDeviceApi, IC_GroupDevice, IC_GroupDeviceDetails } from "@/$api/group-device-api";

@Component({
    name: "DevicesByDepartment",
    components: { HeaderComponent, DataTableComponent }
})
export default class GroupDeviceComponent extends Mixins(ComponentBase) {
    departmentData = [];
    selectedGroupDevicendex = 0;
    data = [];
    value = [];
    maxHeight = 400;
    mounted() {
        // this.titles = [
        //   this.$t("ListDevicesSelected"),
        //   this.$t("ListDevicesUnSelected")
        // ];
        this.GetGroupDevice();
    }

    GetGroupDevice() {
        groupDeviceApi.GetGroupDevice().then((res: any) => {
            console.log('arrGroupDevice', res.data);

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
        groupDeviceApi
            .GetDevicesInOutGroupDevice(this.selectedGroupDevicendex)
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

    updateDevicesByDepartment() {
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

        const dataUpdate: IC_GroupDeviceDetails = {
            GroupDeviceIndex: this.selectedGroupDevicendex,
            ListDeviceSerial: listSerial
        };
        
        groupDeviceApi
            .UpdateGroupDeviceDetail(dataUpdate)
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