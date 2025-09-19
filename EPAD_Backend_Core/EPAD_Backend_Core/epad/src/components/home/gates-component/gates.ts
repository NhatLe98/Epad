import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { lineApi, LineModel } from '@/$api/gc-lines-api';
import { gatesLinesApi } from '@/$api/gc-gates-lines-api';
import { relayControllerApi } from '@/$api/relay-controller-api';
import { gatesApi, GatesModel, LinesParam as GateLineModel, LineController as GateLineController } from '@/$api/gc-gates-api';
import { cameraApi } from '@/$api/camera-api';
import { deviceApi } from '@/$api/device-api';
import { isNullOrUndefined } from 'util';

@Component({
    name: "gates",
    components: { HeaderComponent, DataTableFunctionComponent, DataTableComponent }
})
export default class Gates extends Mixins(ComponentBase) {
    columns = [];
    rowsObj = [];
    isEdit = false;
    showDialogGate = false;
    showDialogLine = false;
    rules = null;
    gatesModel: GatesModel = null;
    linesModel: LineModel = null;
    gateLinesModel: GateLineModel = null;
    latestLineModel: GateLineModel = null;
    page = 1;

    isShowLineDeviceInfo = true;

    linesTitles = [];

    viewingGate: any;
    viewingLine: any;
    viewingLineName: any;

    selectedGate = [];
    selectedLine = [];
    selectedUnselectLine = [];
    
    linesData = [];
    allLinesData = [];

    gatesLinesData = [];

    listGate = [];
    listDevice = [];
    listCamera = [];

    listControllerType = [{Index: 0, Name: this.$t('In')}, {Index: 1, Name: this.$t('Out')}]
    listController = [];
    listControllerIn = [];
    listControllerOut = [];
    listControllerInChannel = [];
    listControllerOutChannel = [];

    selectedControllerIn = [];
    selectedControllerOut = [];
    selectedCamera = [];
    selectedDevice = [];

    LineControllers = [{ControllerIndex: null,
        ControllerType: null,
        OpenChannel: null,
        CloseChannel: null}];

    created(){
        this.gateLinesModel = {
            Index: 0,
            Name: '',
            Description: '',
            CameraInIndex: [],
            CameraOutIndex: [],
            DeviceInSerial: [],
            DeviceOutSerial: [],
            LineControllersIn: [{
                ControllerIndex: null,
                OpenChannel: null,
                CloseChannel: null
            }],
            LineControllersOut: [{
                ControllerIndex: null,
                OpenChannel: null,
                CloseChannel: null
            }]
        };
    }

    async beforeMount() {
        this.ResetGate();
        this.CreateRules();
        this.CreateColumns();
        await this.GetGatesAll().then(() => {
            if(this.listGate && this.listGate.length > 0){
                this.viewingGate = this.listGate[0];
                (this.$refs.gatesTable as any).setCurrentRow(this.listGate[0]);
            }
        });
        await this.GetAllLineData();
        await this.GetAllCamera();
        await this.GetAllDevice();
        await this.GetAllController();
        await this.getAllGatesLines();
    }

    async GetGatesAll(){
        await gatesApi.GetGatesAll().then((res: any) => {
            this.listGate = [];
            if (res.status == 200 && res.data && res.data.data && res.data.data.length > 0) {
                this.listGate = res.data.data;
                // this.viewingGate = this.listGate[0];
            }
        });
    }

    async getAllGatesLines(){
        await gatesLinesApi.GetAllGatesLines().then((res: any) => {
            this.gatesLinesData = [];
            if (res.status == 200 && res.data && res.data.length > 0) {
                this.gatesLinesData = res.data;

                if(this.viewingGate && this.gatesLinesData && this.gatesLinesData.length > 0){
                    const gateLines = this.gatesLinesData.filter(x => x.GateIndex == this.viewingGate.Index);
                    // console.log(gateLines)
                    if(gateLines && gateLines.length > 0){
                        this.selectedLine = gateLines.map(x => x.LineIndex);
                    }
                }

                if(this.selectedLine && this.selectedLine.length > 0){
                    this.viewingLine = this.selectedLine[0];
                }else if(this.linesData && this.linesData.length > 0){
                    this.viewingLine = this.linesData[0].Index;
                }
                this.handleChangeLine();
                this.reLoadControllerChannel();

                // console.log(this.gatesLinesData)
            }
        });
    }

    // async GetAllLineBasic(){
    //     await lineApi.GetAllLineBasic().then((res: any) => {
    //         if(res.status == 200 && res.data && res.data){
    //             this.allLinesData = res.data;
    //             this.linesData = res.data.map(x => ({
    //                 key: x.Index,
    //                 label: x.Name,
    //                 disabled: false,
    //             }));
    //         }
    //     });
    // }

    async GetAllLineData(){
        await lineApi.GetAll().then((res: any) => {
            // console.log(res)
            this.allLinesData = [];
            this.linesData = [];
            if(res.status == 200 && res.data && res.data.data && res.data.data.length > 0){
                this.allLinesData = res.data.data;
                this.linesData = res.data.data.map(x => ({
                    key: x.Index,
                    label: x.Name,
                    disabled: false,
                }));
                // console.log(this.linesData)
            }
        });
    }

    async GetAllCamera(){
        await cameraApi.GetAllCamera().then((res: any) => {
            this.listCamera = [];
            if (res.status == 200 && res.data && res.data.length > 0) {
                this.listCamera = res.data;
            }
        });
    }

    async GetAllDevice(){
        await deviceApi.GetAllDevice().then((res: any) => {
            this.listDevice = [];
            if (res.status == 200 && res.data && res.data.length > 0) {
                this.listDevice = res.data;
            }
        });
    }

    async GetAllController(){
        await relayControllerApi.GetAllController().then((res: any) => {
            this.listController = [];
            if(res.status == 200 && res.data && res.data.length > 0){
                this.listController = res.data;
                this.listController.forEach(element => {
                    if(element.ListChannel){
                        element.ListChannel.forEach(channel => {
                            channel.Name = "Channel " + channel.Index;
                        });
                    }
                });
                this.listControllerIn = Misc.cloneData(this.listController);
                this.listControllerOut = Misc.cloneData(this.listController);
            }
        });
    }

    async mounted(){
        this.linesTitles = [
            this.$t("ListLinesUnselect"),
            this.$t("ListLinesSelected")
        ];

        const transfer = document.querySelector('.lines-transfer');
        const panels = document.querySelectorAll('.lines-transfer .el-transfer-panel');
        const transferButtons = document.querySelectorAll('.lines-transfer .el-transfer__buttons');

        panels.forEach(panel => panel.remove());

        transfer.append(panels[1], transferButtons[0], panels[0]);
    }

    updated(){
        const panelItems = document.querySelectorAll('.lines-transfer .el-transfer-panel .el-transfer-panel__body .el-transfer-panel__list .el-transfer-panel__item .el-checkbox__label');

        if(panelItems && panelItems.length > 0){
            panelItems.forEach(element => {
                element.parentElement.style.marginRight = "0";

                (element as HTMLElement).addEventListener('click', (event: any) => {
                    event.preventDefault();
                });

                if(element.childNodes && element.childNodes.length > 1){
                    element.removeChild(element.childNodes[1]);
                }

                const spanDetail = document.createElement("span");
                spanDetail.innerText = this.$t('SeeDetail...').toString();
                spanDetail.style.position = "absolute";
                spanDetail.style.right = "5px";
                spanDetail.style.color = "blue";
                spanDetail.style.cursor = "pointer";
                spanDetail.style.zIndex = "99";
                spanDetail.style.fontWeight = "bold";
                spanDetail.addEventListener('mouseenter', (event: any) => {
                    spanDetail.style.textDecoration = "underline";
                });
                spanDetail.addEventListener('mouseleave', (event: any) => {
                    spanDetail.style.textDecoration = "none";
                });
                spanDetail.addEventListener('click', (event: any) => {                    
                    this.viewingLine = event.srcElement.parentNode.childNodes[0].getAttribute("data_key");
                    // console.log(event, "click")
                    // console.log(event.srcElement.parentNode.childNodes[0].getAttribute("data_key"))
                    this.viewingLineName = this.allLinesData.find(x => x.Index == this.viewingLine).Name;
                    // console.log(this.allLinesData)
                    this.gateLinesModel = Misc.cloneData(this.allLinesData.find(x => x.Index == this.viewingLine));
                    if(!this.gateLinesModel.LineControllersIn || this.gateLinesModel.LineControllersIn.length == 0){
                        this.gateLinesModel.LineControllersIn.push({
                            ControllerIndex: null,
                            OpenChannel: null,
                            CloseChannel: null
                        });
                    }
                    if(!this.gateLinesModel.LineControllersOut || this.gateLinesModel.LineControllersOut.length == 0){
                        this.gateLinesModel.LineControllersOut.push({
                            ControllerIndex: null,
                            OpenChannel: null,
                            CloseChannel: null
                        });
                    }
                    // console.log(this.gateLinesModel)
                    this.isShowLineDeviceInfo = true;
                    // console.log(this.viewingLine)
                });
                spanDetail.className = "el-checkbox__label__detail";

                element.appendChild(spanDetail);
            });
        }
    }

    handleChangeLine(){
        this.viewingLineName = this.allLinesData.find(x => x.Index == this.viewingLine).Name;
        this.gateLinesModel = Misc.cloneData(this.allLinesData.find(x => x.Index == this.viewingLine));
        if(!this.gateLinesModel.LineControllersIn || this.gateLinesModel.LineControllersIn.length == 0){
            this.gateLinesModel.LineControllersIn.push({
                ControllerIndex: null,
                OpenChannel: null,
                CloseChannel: null
            });
        }
        if(!this.gateLinesModel.LineControllersOut || this.gateLinesModel.LineControllersOut.length == 0){
            this.gateLinesModel.LineControllersOut.push({
                ControllerIndex: null,
                OpenChannel: null,
                CloseChannel: null
            });
        }
        // console.log(this.gateLinesModel)
        this.isShowLineDeviceInfo = true;
        // console.log(this.viewingLine)
    }

    hideLineDeviceInfo(){
        // this.isShowLineDeviceInfo = false;
        // this.viewingLine = 0;
    }

    renderLabel(h, option) {
        return h('span', { attrs: { data_key: option.key } }, option.label);
    }

    addItemControllerIn() {
        this.gateLinesModel.LineControllersIn.push({
            ControllerIndex: null,
            OpenChannel: null,
            CloseChannel: null
        });
    }

    removeItemDetailControllerIn(indexRemoveItem) {
        if (this.latestLineModel.LineControllersIn.length > 1) {
            const latestModel = Misc.cloneData(this.latestLineModel);
            latestModel.LineControllersIn.splice(indexRemoveItem, 1);
            this.latestLineModel = latestModel;
        }
        if (this.gateLinesModel.LineControllersIn.length > 1) {
            this.gateLinesModel.LineControllersIn.splice(indexRemoveItem, 1);
        }
    }

    addItemControllerOut() {
        this.gateLinesModel.LineControllersOut.push({
            ControllerIndex: null,
            OpenChannel: null,
            CloseChannel: null
        });
    }

    removeItemDetailControllerOut(indexRemoveItem) {
        if (this.latestLineModel.LineControllersOut.length > 1) {
            const latestModel = Misc.cloneData(this.latestLineModel);
            latestModel.LineControllersOut.splice(indexRemoveItem, 1);
            this.latestLineModel = latestModel;
        }
        if (this.gateLinesModel.LineControllersOut.length > 1) {
            this.gateLinesModel.LineControllersOut.splice(indexRemoveItem, 1);
        }
    }

    @Watch("gateLinesModel", {deep: true})
    reLoadControllerChannel(){
        // // console.log("reload")
        // console.log(JSON.stringify(this.latestLineModel))
        // console.log(JSON.stringify(this.gateLinesModel))
        const selectedOtherLines = this.allLinesData.filter(x => x.Index != this.viewingLine);
        if(this.gateLinesModel){
            this.listControllerInChannel = [];
            this.listControllerOutChannel = [];
            if(this.gateLinesModel.LineControllersIn){
                this.gateLinesModel.LineControllersIn.forEach((element, index) => {
                    if(element.ControllerIndex && this.latestLineModel 
                        && this.viewingLine == this.latestLineModel.Index
                        && this.latestLineModel.LineControllersIn 
                        && this.latestLineModel.LineControllersIn[index]
                        && this.latestLineModel.LineControllersIn[index].ControllerIndex
                        && this.latestLineModel.LineControllersIn[index].ControllerIndex != element.ControllerIndex){
                        element.OpenChannel = null;
                        element.CloseChannel = null;
                    }
                    if(element.ControllerIndex > 0){
                        const controllerChannels = this.listController.find(x => x.Index == element.ControllerIndex);
                        if(controllerChannels){
                            const controllerChannelArray = Misc.cloneData(controllerChannels.ListChannel);
                            this.listControllerInChannel.push(controllerChannelArray);
                        }
                    }

                    if(!element.ControllerIndex || (element.ControllerIndex && element.ControllerIndex == 0)){
                        element.OpenChannel = null;
                        element.CloseChannel = null;
                    }

                    if(element.OpenChannel && element.OpenChannel.toString() == ""){
                        element.OpenChannel = null;
                    }
                    if(element.CloseChannel && element.CloseChannel.toString() == ""){
                        element.CloseChannel = null;
                    }
                });

                this.selectedControllerIn = this.gateLinesModel.LineControllersIn.map(x => x.ControllerIndex);
                if(selectedOtherLines && selectedOtherLines.length > 0){
                    selectedOtherLines.forEach(element => {
                        const selectedControllerIn = element.LineControllersIn.map(x => x.ControllerIndex);
                        const selectedControllerOut = element.LineControllersOut.map(x => x.ControllerIndex);
                        this.selectedControllerIn = this.selectedControllerIn
                            .concat(selectedControllerIn);
                        this.selectedControllerIn = this.selectedControllerIn
                            .concat(selectedControllerOut);
                    });
                }
            }

            if(this.gateLinesModel.LineControllersOut){
                this.gateLinesModel.LineControllersOut.forEach((element, index) => {
                    if(element.ControllerIndex && this.latestLineModel 
                        && this.viewingLine == this.latestLineModel.Index
                        && this.latestLineModel.LineControllersOut 
                        && this.latestLineModel.LineControllersOut[index] 
                        && this.latestLineModel.LineControllersOut[index].ControllerIndex
                        && this.latestLineModel.LineControllersOut[index].ControllerIndex != element.ControllerIndex){
                        element.OpenChannel = null;
                        element.CloseChannel = null;
                    }
                    if(element.ControllerIndex > 0){
                        const controllerChannels = this.listController.find(x => x.Index == element.ControllerIndex);
                        if(controllerChannels){
                            const controllerChannelArray = Misc.cloneData(controllerChannels.ListChannel);
                            this.listControllerOutChannel.push(controllerChannelArray);
                        }
                    }

                    if(!element.ControllerIndex || (element.ControllerIndex && element.ControllerIndex == 0)){
                        element.OpenChannel = null;
                        element.CloseChannel = null;
                    }

                    if(element.OpenChannel && element.OpenChannel.toString() == ""){
                        element.OpenChannel = null;
                    }
                    if(element.CloseChannel && element.CloseChannel.toString() == ""){
                        element.CloseChannel = null;
                    }
                });

                this.selectedControllerOut = this.gateLinesModel.LineControllersOut.map(x => x.ControllerIndex);
                if(selectedOtherLines && selectedOtherLines.length > 0){
                    selectedOtherLines.forEach(element => {
                        const selectedControllerIn = element.LineControllersIn.map(x => x.ControllerIndex);
                        const selectedControllerOut = element.LineControllersOut.map(x => x.ControllerIndex);
                        this.selectedControllerOut = this.selectedControllerOut
                            .concat(selectedControllerIn);
                        this.selectedControllerOut = this.selectedControllerOut
                            .concat(selectedControllerOut);
                    });
                }
            }

            this.selectedCamera = this.gateLinesModel.CameraInIndex;
            this.selectedCamera = this.selectedCamera.concat(this.gateLinesModel.CameraOutIndex);
            this.selectedDevice = this.gateLinesModel.DeviceInSerial;
            this.selectedDevice = this.selectedDevice.concat(this.gateLinesModel.DeviceOutSerial);
            if(selectedOtherLines && selectedOtherLines.length > 0){
                selectedOtherLines.forEach(element => {
                    const selectedCameraIn = element.CameraInIndex;
                    const selectedCameraOut = element.CameraOutIndex;
                    this.selectedCamera = this.selectedCamera.concat(selectedCameraIn);
                    this.selectedCamera = this.selectedCamera.concat(selectedCameraOut);

                    const selectedDeviceIn = element.DeviceInSerial;
                    const selectedDeviceOut = element.DeviceOutSerial;
                    this.selectedDevice = this.selectedDevice.concat(selectedDeviceIn);
                    this.selectedDevice = this.selectedDevice.concat(selectedDeviceOut);
                });
            }
        }else{
            this.gateLinesModel = {
                Index: this.viewingLine,
                Name: '',
                Description: '',
                CameraInIndex: [],
                CameraOutIndex: [],
                DeviceInSerial: [],
                DeviceOutSerial: [],
                LineControllersIn: [{
                    ControllerIndex: null,
                    OpenChannel: null,
                    CloseChannel: null
                }],
                LineControllersOut: [{
                    ControllerIndex: null,
                    OpenChannel: null,
                    CloseChannel: null
                }]
            };
        }

        this.latestLineModel = Misc.cloneData(this.gateLinesModel);
        // // console.log(this.latestLineModel)
    }

    CreateRules() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputName'),
                    trigger: 'change',
                },
            ],
        }
    }
    CreateColumns() {
        this.columns = [
            {
                prop: 'Name',
                label: 'Name',
                minWidth: 150,
                display: true
            },
            {
                prop: 'Description',
                label: 'Description',
                minWidth: 150,
                display: true
            },
            {
                prop: 'CreatedDate',
                label: 'CreatedDate',
                minWidth: 220,
                display: true
            },
            {
                prop: 'UpdatedDate',
                label: 'UpdatedDate',
                minWidth: 220,
                display: true
            },
            {
                prop: 'UpdatedUser',
                label: 'UpdatedUser',
                minWidth: 200,
                display: true
            }
        ];
    }

    InsertGate() {
        this.showDialogGate = true;
        if (this.isEdit == true) {
            this.ResetGate();
        }
        this.gatesModel = {
            Index: 0,
            Name: '',
            Description: '',
            IsMandatory: true,
            Lines: [],
            LineDevice: null
        };
        // this.gateLinesModel.LineControllersIn = [{
        //     ControllerIndex: null,
        //     OpenChannel: null,
        //     CloseChannel: null
        // }];
        // this.gateLinesModel.LineControllersOut = [{
        //     ControllerIndex: null,
        //     OpenChannel: null,
        //     CloseChannel: null
        // }];
        this.isEdit = false;

    }

    EditGate() {
        this.isEdit = true;
        // console.log(this.selectedGate)
        if (this.selectedGate.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (this.selectedGate.length == 1) {
            this.showDialogGate = true;
            this.gatesModel = Misc.cloneData(this.listGate.find(x => x.Index == this.selectedGate[0].Index));
            // // console.log(this.gatesModel)
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async DeleteGate() {
        const listIndex: Array<number> = this.selectedGate.map((item: any) => {
            return item.Index;
        });

        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            await this.$confirmDelete().then(() => {
                gatesApi.DeleteGates(listIndex).then((res: any) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { })
                .finally(async () => {
                    await this.GetGatesAll();
                });
            });
        }
    }
    async ConfirmClickGate() {
        (this.$refs.gatesModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            const currentViewGate = Misc.cloneData(this.viewingGate);
            if (this.isEdit == false) {
                await gatesApi.AddGates(this.gatesModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetGate();
                    this.showDialogGate = false;
                });
            }
            else {
                await gatesApi.UpdateGates(this.gatesModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetGate();
                    this.showDialogGate = false;
                });
            }
            await this.GetGatesAll();
            if(this.listGate && this.listGate.length > 0){
                this.viewingGate = this.listGate.find(x => x.Index == currentViewGate.Index);
                (this.$refs.gatesTable as any).setCurrentRow(this.viewingGate);
            }
        });
    }

    CancelGate() {
        this.ResetGate();
        this.showDialogGate = false;
    }

    UpdateGateLineDeviceInfo(){
        const data: GatesModel = {
            Index: this.viewingGate?.Index ?? 0,
            Name: this.viewingGate?.Name ?? '',
            Description: this.viewingGate?.Description ?? '',
            IsMandatory: this.viewingGate?.IsMandatory ?? false,
            Lines: this.selectedLine,
            LineDevice: this.gateLinesModel
        };

        const gateLineDeviceInfo = Misc.cloneData(data);
        if(!gateLineDeviceInfo.Lines){
            gateLineDeviceInfo.Lines = [];
        }
        if(gateLineDeviceInfo.LineDevice){
            if(!gateLineDeviceInfo.LineDevice.LineControllersIn){
                gateLineDeviceInfo.LineDevice.LineControllersIn = [];
            }else if(gateLineDeviceInfo.LineDevice.LineControllersIn.length > 0){
                gateLineDeviceInfo.LineDevice.LineControllersIn.forEach(element => {
                    if(!element.ControllerIndex){
                        element.ControllerIndex = 0;
                    }
                    if(!element.OpenChannel){
                        element.OpenChannel = 0;
                    }
                    if(!element.CloseChannel){
                        element.CloseChannel = 0;
                    }
                });
            }

            if(!gateLineDeviceInfo.LineDevice.LineControllersOut){
                gateLineDeviceInfo.LineDevice.LineControllersOut = [];
            }else if(gateLineDeviceInfo.LineDevice.LineControllersOut.length > 0){
                gateLineDeviceInfo.LineDevice.LineControllersOut.forEach(element => {
                    if(!element.ControllerIndex){
                        element.ControllerIndex = 0;
                    }
                    if(!element.OpenChannel){
                        element.OpenChannel = 0;
                    }
                    if(!element.CloseChannel){
                        element.CloseChannel = 0;
                    }
                });
            }
        }
        // console.log(gateLineDeviceInfo)
        gatesApi.UpdateGateLineDevice(gateLineDeviceInfo).then((res: any) => {
            if (res.status != undefined && res.status === 200) {
                this.$saveSuccess();
            }
        }).finally(async () => {
            await this.GetAllLineData();
            await this.getAllGatesLines();
        });
    }

    ResetGate() {
        this.gatesModel = {
            Index: 0,
            Name: '',
            Description: '',
            IsMandatory: true,
            Lines: [],
            LineDevice: null
        };
        // this.gateLinesModel = {
        //     Index: 0,
		// 	Name: '',
		// 	Description: '',
        //     DeviceInSerial: [],
        //     DeviceOutSerial: [],
        //     CameraInIndex: [],
        //     CameraOutIndex: [],
        //     LineControllersIn: [{
        //         ControllerIndex: null,
        //         OpenChannel: null,
        //         CloseChannel: null
        //     }],
        //     LineControllersOut: [{
        //         ControllerIndex: null,
        //         OpenChannel: null,
        //         CloseChannel: null
        //     }]
        // };
    }

    InsertLine() {
        this.showDialogLine = true;
        if (this.isEdit == true) {
            this.ResetLine();
        }
        this.linesModel = {
            Index: 0,
            Name: '',
            Description: '',
            LineForCustomer: false,
            LineForCustomerIssuanceReturnCard: false,
            LineForDriver: false,
            LineForDriverIssuanceReturnCard: false,
        };
        // this.gateLinesModel.LineControllersIn = [{
        //     ControllerIndex: null,
        //     OpenChannel: null,
        //     CloseChannel: null
        // }];
        // this.gateLinesModel.LineControllersOut = [{
        //     ControllerIndex: null,
        //     OpenChannel: null,
        //     CloseChannel: null
        // }];
        this.isEdit = false;

    }

    EditLine() {
        this.isEdit = true;
        // console.log(this.selectedUnselectLine)
        if (this.selectedUnselectLine.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (this.selectedUnselectLine.length == 1) {
            this.showDialogLine = true;
            this.linesModel = Misc.cloneData(this.allLinesData.find(x => x.Index == this.selectedUnselectLine[0]));
            // // console.log(this.gatesModel)
        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }
    async DeleteLine() {
        const listIndex: Array<number> = this.selectedUnselectLine;

        if (listIndex.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
        } else {
            await this.$confirmDelete().then(() => {
                lineApi.DeleteLines(listIndex).then((res: any) => {
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => { })
                .finally(async () => {
                    await this.GetAllLineData();
                    await this.getAllGatesLines();
                });
            });
        }
    }
    async ConfirmClickLine() {
        (this.$refs.linesModel as any).validate(async (valid) => {
            if (valid == false) {
                return;
            }
            if (this.isEdit == false) {
                await lineApi.AddLine(this.linesModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetLine();
                    this.showDialogLine = false;
                });
            }
            else {
                await lineApi.UpdateLine(this.linesModel).then((res: any) => {
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.ResetLine();
                    this.showDialogLine = false;
                });
            }
            await this.GetAllLineData();
            await this.getAllGatesLines();
        });
    }

    CancelLine() {
        this.ResetLine();
        this.showDialogLine = false;
    }

    ResetLine() {
        this.linesModel = {
            Index: 0,
            Name: '',
            Description: '',
            LineForCustomer: false,
            LineForCustomerIssuanceReturnCard: false,
            LineForDriver: false,
            LineForDriverIssuanceReturnCard: false,
        };
        // this.gateLinesModel = {
        //     Index: 0,
		// 	Name: '',
		// 	Description: '',
        //     DeviceInSerial: [],
        //     DeviceOutSerial: [],
        //     CameraInIndex: [],
        //     CameraOutIndex: [],
        //     LineControllersIn: [{
        //         ControllerIndex: null,
        //         OpenChannel: null,
        //         CloseChannel: null
        //     }],
        //     LineControllersOut: [{
        //         ControllerIndex: null,
        //         OpenChannel: null,
        //         CloseChannel: null
        //     }]
        // };
    }

    handleUnselectChange(event){
        // console.log(event)
        this.selectedUnselectLine = event;
    }

    handleSelectedChange(event){
        // console.log(event)
    }

    handleCurrentChange(currentRow, oldRow) {
        // console.log(currentRow, oldRow)
        if(currentRow && (!this.viewingGate || (this.viewingGate && this.viewingGate.Index != currentRow.Index))){
            this.selectedLine = [];
            this.viewingGate = currentRow;
            if(this.gatesLinesData && this.gatesLinesData.length > 0){
                const gateLines = this.gatesLinesData.filter(x => x.GateIndex == this.viewingGate.Index);
                // console.log(gateLines)
                if(gateLines && gateLines.length > 0){
                    this.selectedLine = gateLines.map(x => x.LineIndex);
                }
            }
            if(this.selectedLine && this.selectedLine.length > 0){
                this.viewingLine = this.selectedLine[0];
            }else if(this.linesData && this.linesData.length > 0){
                this.viewingLine = this.linesData[0].key;
            }
            this.handleChangeLine();
        }
        // console.log(this.viewingGate)
    }

    handleSelectChange(selection, row){
        // console.log(selection, row)
        this.selectedGate = selection;
        // console.log(this.selectedGate)
    }

    handleSelectAllChange(selection){
        // console.log(selection)
        this.selectedGate = selection;
        // console.log(this.selectedGate)
    }
}