import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { IRelayController, IChannel, relayControllerApi } from "@/$api/relay-controller-api";
import { isNullOrUndefined } from 'util';
import { ELOOP } from "constants";

@Component({
    name: "RelayController",
    components: { HeaderComponent }
})
export default class RelayController extends Mixins(ComponentBase) {
    channelData: IChannel = {
        Index: 0,
        NumberOfSecondsOff: 0,
        ChannelStatus: true,
        SignalType: 0
    }

    relayTypeLst = [
        {
            name: "Output",
            value: 0
        }, 
        {
            name: "Input",
            value: 1
        }, 
        {
            name: "Input & Output",
            value: 2
        }
    ]

    controllerData: IRelayController = {
        Index: 0,
        Name: '',
        IPAddress: '',
        Port: 0,
        RelayType: 'ModbusTCP',
        Description: '',
        ListChannel: [],
        ChannelIndex: this.channelData.Index,
        ChannelSeconds: this.channelData.NumberOfSecondsOff,
        ChannelStatus: this.channelData.ChannelStatus,
        SignalType: 0
    };
    isInOutPut: boolean = false;

    tblController: Array<IRelayController> = [];
    beforeMount() {
        this.getController();
    }

    addControllerClick() {
        this.controllerData = {
            Index: -1,
            Name: 'Controller name',
            IPAddress: '',
            Port: 502,
            RelayType: "ModbusTCP",
            Description: '',
            SignalType: 0,
            ListChannel: [],
            ChannelIndex: this.channelData.Index,
            ChannelSeconds: this.channelData.NumberOfSecondsOff,
            ChannelStatus: this.channelData.ChannelStatus
        };
        relayControllerApi.AddController(this.controllerData).then((res: any) => {
            if (!isNullOrUndefined(res.status) && res.status === 200) {

                this.controllerData.Index = res.data.Index;
                this.tblController.push(this.controllerData);
                //set current row
                let controllerTable: any = this.$refs.controllerTable;
                controllerTable.setCurrentRow(this.controllerData);
            }

        });
    }
    removeControllerClick() {
        if (this.controllerData.Index > -1) {
            this.$confirmDelete().then(async () => {
                relayControllerApi.RemoveController(this.controllerData).then((res: any) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                        this.getController();
                    }
                });
            });
        }
    }
    addChannelClick(type) {
        if (this.controllerData == null || this.controllerData.Index == 0) {
            return;
        }
        let maxIndex: number = 0;
        var listChannel = this.controllerData.ListChannel?.filter(x => x.SignalType.toString() == type.toString());
        for (let index = 0; index < listChannel.length; index++) {
            if (maxIndex < listChannel[index].Index) {
                maxIndex = listChannel[index].Index;
            }
        }

        let channel: IChannel = {
            Index: maxIndex + 1,
            NumberOfSecondsOff: 4,
            ChannelStatus: false,
            SignalType: type
        };
        this.controllerData.ListChannel.push(channel);
    }

    onChangeSingalType(e){
        if(e.toString() == "2"){
           this.isInOutPut = true;
        }else{
            this.isInOutPut = false;
        }
    }

    updateControllerClick() {
        let arrIndex: Array<number> = [];
        let error: boolean = false;
        if(this.controllerData.SignalType != 2){
            this.controllerData.ListChannel = this.controllerData.ListChannel.filter(x => x.SignalType == 0);
        }
        // for (let index = 0; index < this.controllerData.ListChannel.length; index++) {
        //     if (arrIndex.find(x => x == this.controllerData.ListChannel[index].Index) != null) {
        //         error = true;
        //         break;
        //     }
        //     arrIndex.push(this.controllerData.ListChannel[index].Index);
        // }
        // if (error == true) {
        //     this.$alertSaveError(null, null, null, "Channel is exists");
        //     return;
        // }
        relayControllerApi.UpdateController(this.controllerData).then((res: any) => {
            if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.$saveSuccess();
            }

        });

    }
    getController() {
        relayControllerApi.GetAllController().then((res: any) => {
            if (!isNullOrUndefined(res.status) && res.status === 200) {
                this.tblController = res.data;
            }
        });
    }

    handleCurrentController(val) {
        this.controllerData = val;

        if (this.controllerData.ListChannel.length == 0) {
            this.controllerData.ChannelIndex = this.channelData.Index;
            this.controllerData.ChannelSeconds = this.channelData.NumberOfSecondsOff;
            this.controllerData.ChannelStatus = this.channelData.ChannelStatus;
        }

        if(val.SignalType == "2"){
            this.isInOutPut = true;
         }else{
             this.isInOutPut = false;
         }
        //console.log(this.controllerData);
        this.connectToDevice();
    }
    handleCurrentChannel(val) {
        if (val == null) {
            this.controllerData.ChannelIndex = this.channelData.Index;
            this.controllerData.ChannelSeconds = this.channelData.NumberOfSecondsOff;
            this.controllerData.ChannelStatus = this.channelData.ChannelStatus;
            return;
        }

        this.controllerData.ChannelIndex = val.Index;
        this.controllerData.ChannelSeconds = val.NumberOfSecondsOff;
        this.controllerData.ChannelStatus = val.ChannelStatus;

    }
    deleteChannelClick() {

        let currentIndex = -1;
        for (let index = 0; index < this.controllerData.ListChannel.length; index++) {
            if (this.controllerData.ListChannel[index].Index == this.controllerData.ChannelIndex) {
                currentIndex = index;
                break;
            }
        }
        if (currentIndex > -1) {
            this.controllerData.ListChannel.splice(currentIndex, 1);
        }
    }

    @Watch("controllerData.ChannelSeconds")
    dataChanged(val, oldVal) {
        for (let index = 0; index < this.controllerData.ListChannel.length; index++) {
            if (this.controllerData.ListChannel[index].Index == this.controllerData.ChannelIndex) {
                this.controllerData.ListChannel[index].NumberOfSecondsOff = val;
            }

        }
    }

    connectToDevice() {
        if (this.controllerData != null) {
            relayControllerApi.GetChannelStatus(this.controllerData).then((res: any) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    for (let i = 0; i < res.data.ListChannel.length; i++) {
                        this.controllerData.ListChannel[i].ChannelStatus = res.data.ListChannel[i].ChannelStatus;
                    }

                } else {
                    this.$alert(
                        this.$t("DeviceNotFound").toString(),
                        this.$t("Notify").toString(),
                        {
                            type: "warning",
                        }
                    );
                }
            });
        }
    }

    changeRelayStatus(keyIndex) {
        if (this.controllerData.RelayType == "ModbusTCP") {
            const channel = this.controllerData.ListChannel[keyIndex - 1];
            const controler: IRelayController = {
                Index: this.controllerData.Index,
                Name: this.controllerData.Name,
                IPAddress: this.controllerData.IPAddress,
                Port: this.controllerData.Port,
                RelayType: this.controllerData.RelayType,
                Description: this.controllerData.Description,
                ListChannel: [channel],
                ChannelIndex: this.controllerData.ChannelIndex,
                ChannelSeconds: this.controllerData.ChannelSeconds,
                ChannelStatus: this.controllerData.ChannelStatus,
                SignalType: this.controllerData.SignalType,
            }
            relayControllerApi.SetOnOrOffDevice(controler).then((res: any) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //this.$deleteSuccess();
                    //this.getController();
                } else {
                    this.$alert(
                        this.$t("DeviceNotFound").toString(),
                        this.$t("Notify").toString(),
                        {
                            type: "warning",
                        }
                    );
                }
            });
        } else {

            relayControllerApi.SetOnOrOffDevice(this.controllerData).then((res: any) => {
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    //this.$deleteSuccess();
                    //this.getController();
                } else {
                    this.$alert(
                        this.$t("DeviceNotFound").toString(),
                        this.$t("Notify").toString(),
                        {
                            type: "warning",
                        }
                    );
                }
            });
        }


    }
}