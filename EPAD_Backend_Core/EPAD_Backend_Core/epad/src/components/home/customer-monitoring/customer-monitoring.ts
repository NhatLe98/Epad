import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import axios from 'axios';
import https from 'https';
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';

import { configApi } from "@/$api/config-api";
import { EzFile, EzFileImage, ezPortalFileApi } from "@/$api/ez-portal-file-api";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import SelectTreeComponent from '@/components/app-component/select-tree-component/select-tree-component.vue';
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { LogParam, WalkerHistoryInfo, WalkerInfo, realtimeApi } from "@/$api/realtime-api";
import { store } from "@/store";
import { lineApi } from "@/$api/gc-lines-api";
import { hrCustomerInfoApi } from "@/$api/hr-customer-info-api";
import { hrUserApi } from "@/$api/hr-user-api";
import { relayControllerApi } from "@/$api/relay-controller-api";
import {
    WarningRulesModel, EmailScheduleModel, EmailScheduleRequestModel, ControllerWarningModel,
    ControllerWarningRequestModel, rulesWarningApi
} from '@/$api/gc-rules-warning-api';

import * as mime from 'mime-types';
import { timeLogApi } from "@/$api/gc_timelog-api";
import { rulesGeneralApi } from "@/$api/gc-rules-general-api";

@Component({
    name: 'customer-monitoring-page',
    components: {HeaderComponent},
})
export default class CustomerMonitoringPage extends Mixins(ComponentBase) {
    fullscreen = false;
    roleByRoute: any;
    columnInGrid: any[] = [];
    employeeLookup: any[];
    maxRow = 7;
    monitorModel: WalkerInfo = null;
    showHistory = true;
    showConfigPanel = false;
    tableHistoryHeight = "330";
    manualControl = true;
    listHistory: WalkerHistoryInfo[] = [];
    realtimeConnection: signalR.HubConnection;
    realtimeConnected = false;
    attendanceLogRealtimeConnection: signalR.HubConnection;
    attendanceLogRealtimeConnected = false;
    realtimeServer = "";
    server = "";
    clientHeight = 0;
    rdInOut = "";
    lineIndex = "";
    listLine = [];
    avatarHeight = 300;
    isProcessInfo = false;
    note = "";
    cccd = "";
    exceptionReason = "";
    classInfo = "border-no-data";
    errorClass = "";
    nameActives = ['1'];
    ListInfoSample = [
        { Title: "EmployeeATID", Data: "" },
        { Title: "FullName", Data: "" },
        { Title: "Department", Data: "" },
        // { Title: "Position", Data: "" }
    ];
    columns = [];
    rulesWarningGroup = {};
    rulesWarning = [];

    Base64_Encoding = "data:image/jpeg;base64,";

    imageDefault = '';
    linesData = [];
    allLinesData = [];

    lineData = {CameraIn: null, CameraOut: null, DeviceIn: null, DeviceOut: null, 
        LineControllersIn: null, LineControllersOut: null};
    lineDeviceData = [];
    dialogLineDeviceData = false;

    connectingDevice = false;
    disConnectingDevice = false;
    realtimeCameraType;
    pageId = Date.now().toString();

    arrWebSocket = [];
    arrWebSocketInterval = [];

    wisenetWaveServerConfig = {};
    wisenetWaveToken = '';
    wisenetWaveCamera = [];

    SOI = new Uint8Array(2);
    CONTENT_LENGTH = "Content-length";
    TYPE_JPEG = "image/jpeg";
    jpeg = "jpeg";
    stopStreamCameraIn = true;
    stopStreamCameraOut = true;
    streamSrcCameraIn = "";
    streamSrcCameraOut = "";

    reloadHistoryTable = false;
    isException = false;
    isFoundData = false;
    usingGeneralRule = false;
    async beforeMount() {
        this.dialogLineDeviceData = false;
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.realtimeCameraType = x.RealtimeCameraType;
            if(this.realtimeCameraType == "Wisenet"){
                this.getWisenetWaveServerConfig();
            }
        });

        window.addEventListener('beforeunload', (event) => {
            console.log("Close socket")
            if(this.arrWebSocket && this.arrWebSocket.length > 0){
                this.arrWebSocket.forEach(element => {
                    element.close();
                });
                this.arrWebSocket = [];
            }
    
            if(this.arrWebSocketInterval && this.arrWebSocketInterval.length > 0){
                this.arrWebSocketInterval.forEach(element => {
                    clearInterval(element);
                });
                this.arrWebSocketInterval = [];
            }

            let urlIn = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/RemoveWebsocketByPageId?id=` 
                + this.pageId;
            // console.log('url in is: ' + urlIn);

            let removeWebSocket = new WebSocket(urlIn);

            removeWebSocket.onmessage = (event) => {
                console.log(event)
            }

            removeWebSocket.onopen = () => {
                console.log('removeWebSocket in opened');
            };
            
            removeWebSocket.onclose = () => {
                console.log('removeWebSocket in closed');
            };

            // // Cancel the event as stated by the standard.
            // event.preventDefault();
            // // Chrome requires returnValue to be set.
            // event.returnValue = '';
        });
        
        this.loadImageDefault();
        this.setColumns();
        this.createMonitorObject();
        this.connectServer();
        this.getWisenetWaveServerConfig();
        await this.loadLines();
        this.maxRow = 10;
        const savedConfig = this.columns;
        this.columnInGrid = savedConfig;

        await this.getRulesWarningGroup();
        await this.getRulesWarning();
        await this.GetAllLineData();
        await this.getRulesGeneralUsing();
    }

    setInterval: ReturnType<typeof setInterval> = setInterval(() => {
		if (this.$route.path == "/customer-monitoring-page") {
		  realtimeApi.ConnectToServer().then((res: any) => {
			return res;
		  }).catch(err => {
			if (err?.response?.status === 401) {
			  this.$router.push('/login').catch((err) => { });
			}
		  })
		} else {
		  clearInterval(this.setInterval);
		}
	
	  }, 60000);// Call API stay connected 60000 ms => 1 minute


    async searchUserByCCCD(){
        if(!this.cccd || this.cccd == ''){
            this.$alert(
                this.$t("PleaseInputCCCD").toString(),
                this.$t("Warning").toString(),
                { type: "warning" }
            );
			return;
        }
        this.isFoundData = false;
        await hrUserApi.GetUserByCCCD(this.cccd).then((res: any) => {
            if(res.status == 200 && res.data){
                this.classInfo = "border-no-data";
                if(res.data.IsExpired == null){
                    // this.createMonitorObject();
                    // this.monitorModel.RegisterImage = this.imageDefault;
                    // this.monitorModel.VerifyImage = this.imageDefault;

                    this.isFoundData = true;
                    this.createMonitorObject();
                    this.monitorModel.ObjectType = this.getObjectTypeName(res.data.EmployeeType).toString();
                    this.monitorModel.CardNumber = res.data?.CardNumber ?? '';
                    this.monitorModel.ListInfo = [
                        { Title: "EmployeeATID", Data: res.data.EmployeeATID },
                        { Title: "FullName", Data: res.data.FullName },
                        { Title: "Department", Data: res.data.Department },
                    ];
                    if(res.data.Avatar && res.data.Avatar != ''){
                        this.monitorModel.RegisterImage = this.Base64_Encoding + res.data.Avatar;
                    }else{
                        this.monitorModel.RegisterImage = this.imageDefault;
                    }
                    this.monitorModel.VerifyImage = this.imageDefault;
                    if(res.data.EmployeeType == 2){
                        this.monitorModel.Error = this.$t('CustomerNotInTime').toString();
                    }else if(res.data.EmployeeType == 8){
                        this.monitorModel.Error = this.$t('TripCodeNotActivateYet').toString();
                    }else{
                        this.monitorModel.Error = this.$t('EmployeeStoppedWorking').toString();
                    }
                }else if(res.data.IsExpired === false){
                    this.isFoundData = true;
                    this.createMonitorObject();
                    this.monitorModel.ObjectType = this.getObjectTypeName(res.data.EmployeeType).toString();
                    this.monitorModel.CardNumber = res.data?.CardNumber ?? '';
                    this.monitorModel.ListInfo = [
                        { Title: "EmployeeATID", Data: res.data.EmployeeATID },
                        { Title: "FullName", Data: res.data.FullName },
                        { Title: "Department", Data: res.data.Department },
                    ];
                    if(res.data.Avatar && res.data.Avatar != ''){
                        this.monitorModel.RegisterImage = this.Base64_Encoding + res.data.Avatar;
                    }else{
                        this.monitorModel.RegisterImage = this.imageDefault;
                    }
                    this.monitorModel.VerifyImage = this.imageDefault;
                    if(res.data.IsInBlackList){
                        this.monitorModel.Error = this.$t('EmployeeInBlackList').toString();
                    }
                }else if(res.data.IsExpired === true){  
                    // this.createMonitorObject();
                    // this.monitorModel.RegisterImage = this.imageDefault;
                    // this.monitorModel.VerifyImage = this.imageDefault;

                    this.isFoundData = true;
                    this.createMonitorObject();
                    this.monitorModel.ObjectType = this.getObjectTypeName(res.data.EmployeeType).toString();
                    this.monitorModel.CardNumber = res.data?.CardNumber ?? '';
                    this.monitorModel.ListInfo = [
                        { Title: "EmployeeATID", Data: res.data.EmployeeATID },
                        { Title: "FullName", Data: res.data.FullName },
                        { Title: "Department", Data: res.data.Department },
                    ];
                    if(res.data.Avatar && res.data.Avatar != ''){
                        this.monitorModel.RegisterImage = this.Base64_Encoding + res.data.Avatar;
                    }else{
                        this.monitorModel.RegisterImage = this.imageDefault;
                    }
                    this.monitorModel.VerifyImage = this.imageDefault;
                    if(res.data.EmployeeType == 2){
                        this.monitorModel.Error = this.$t('CustomerNotInTime').toString();
                    }else if(res.data.EmployeeType == 8){
                        this.monitorModel.Error = this.$t('TripCodeExpired').toString();
                    }else{
                        this.monitorModel.Error = this.$t('EmployeeStoppedWorking').toString();
                    } 
                }
            }else{
                this.monitorModel.ObjectType = "";
                this.monitorModel.Error = this.$t('UserNotFound').toString();
                this.monitorModel.ListInfo = [];
                this.monitorModel.RegisterImage = this.imageDefault;
                this.monitorModel.VerifyImage = this.imageDefault;
                this.isFoundData = false;
            }
        });
    }

    getObjectTypeName(value){
        switch (value) {
            case 1: return this.$t('Employee'); break;
            case 2: return this.$t('Guest'); break;
            case 3: return this.$t('Student'); break;
            case 4: return this.$t('Parent'); break;
            case 5: return this.$t('Nanny'); break;
            case 6: return this.$t('Contractor'); break;
            case 7: return this.$t('Teacher'); break;
            case 8: return this.$t('Driver'); break;
            default: return this.$t('Employee'); break;
        }
    }

    closeLineDeviceDialog(){
        this.dialogLineDeviceData = false;
    }

    showOrHideLineDeviceDataDialog(){
        this.getLineDeviceData();
        this.dialogLineDeviceData = !this.dialogLineDeviceData;
    }

    getLineDeviceData(){
        this.lineDeviceData = [];
        if(this.lineData.CameraIn){
            this.lineDeviceData.push({
                Index: this.lineData.CameraIn.Index,
                Name: this.lineData.CameraIn.Name, 
                IP: this.lineData.CameraIn.IPAddress,
                Port: this.lineData.CameraIn.Port,
                DeviceType: this.$t('Camera').toString(),
                AccessType: this.$t('In').toString(),
                Status: this.lineData.CameraIn.Status
            });
        }
        if(this.lineData.CameraOut){
            this.lineDeviceData.push({
                Index: this.lineData.CameraOut.Index,
                Name: this.lineData.CameraOut.Name, 
                IP: this.lineData.CameraOut.IPAddress,
                Port: this.lineData.CameraOut.Port,
                DeviceType: this.$t('Camera').toString(),
                AccessType: this.$t('Out').toString(),
                Status: this.lineData.CameraOut.Status
            });
        }
        if(this.lineData.DeviceIn && this.lineData.DeviceIn.length > 0){
            this.lineData.DeviceIn.forEach(element => {
                this.lineDeviceData.push({
                    Index: element.Index,
                    Name: element.Name, 
                    IP: element.IPAddress,
                    Port: element.Port,
                    DeviceType: this.$t('AttendanceDevice').toString(),
                    AccessType: this.$t('In').toString(),
                    Status: element.Status
                });
            });
        }
        if(this.lineData.DeviceOut && this.lineData.DeviceOut.length > 0){
            this.lineData.DeviceOut.forEach(element => {
                this.lineDeviceData.push({
                    Index: element.Index,
                    Name: element.Name, 
                    IP: element.IPAddress,
                    Port: element.Port,
                    DeviceType: this.$t('AttendanceDevice').toString(),
                    AccessType: this.$t('Out').toString(),
                    Status: element.Status
                });
            });
        }
        if(this.lineData.LineControllersIn && this.lineData.LineControllersIn.length > 0){
            this.lineData.LineControllersIn.forEach(element => {
                this.lineDeviceData.push({
                    Index: element.Index,
                    Name: element.Name, 
                    IP: element.IPAddress,
                    Port: element.Port,
                    DeviceType: this.$t('Controller').toString(),
                    AccessType: this.$t('In').toString(),
                    Status: element.Status
                });
            });
        }
        if(this.lineData.LineControllersOut && this.lineData.LineControllersOut.length > 0){
            this.lineData.LineControllersOut.forEach(element => {
                this.lineDeviceData.push({
                    Index: element.Index,
                    Name: element.Name, 
                    IP: element.IPAddress,
                    Port: element.Port,
                    DeviceType: this.$t('Controller').toString(),
                    AccessType: this.$t('Out').toString(),
                    Status: element.Status
                });
            });
        }
        this.lineDeviceData.forEach((element, index) => {
            element.Index = index + 1;
        });
    }

    async reloadLineDeviceConnect(){
        await this.GetAllLineData();
        if(this.lineDeviceData && this.lineDeviceData.length > 0){
            this.lineDeviceData.forEach(element => {
                element.Status = this.$t('Checking').toString() + "...";
            });
        }
        if(!this.lineIndex || this.lineIndex.toString() == '0'){
            this.connectingDevice = true;
            setTimeout(() => {
                this.connectingDevice = false;
            }, 1000);            
        }else{
            this.cbbLineChanged(this.lineIndex);
        }
    }

    async GetAllLineData(){
        await lineApi.GetAll().then((res: any) => {
            // // console.log(res)
            this.allLinesData = [];
            this.linesData = [];
            if(res.status == 200 && res.data && res.data.data && res.data.data.length > 0){
                this.allLinesData = res.data.data;
                this.linesData = res.data.data.map(x => ({
                    key: x.Index,
                    label: x.Name,
                    disabled: false,
                }));
                // console.log(this.allLinesData)
            }
        });
    }

    loadImageDefault() {
        Misc.readFileAsync("static/variables/image_default.json").then(value => {
            this.imageDefault = value?.[0]?.["photo"] ?? "";
            this.monitorModel.VerifyImage = this.imageDefault;
            this.monitorModel.RegisterImage = this.imageDefault;
        });
    }

    setColumns() {
        this.columns = [
            {
                "Name": "Object",
                "DataField": "ObjectType",
                "Fixed": false,
                "Show": true,
                "Width": 100
            },
            {
                "Name": "MCC",
                "DataField": "EmployeeATID",
                "Fixed": false,
                "Show": true,
                "Width": 100
            },
            {
                "Name": "FullName",
                "DataField": "FullName",
                "Fixed": true,
                "Show": true,
                "Width": 150
            },
            {
                "Name": "Department",
                "DataField": "Department",
                "Fixed": false,
                "Show": true,
                "Width": 150
            },
            {
                "Name": "InOut",
                "DataField": "InOut",
                "Fixed": true,
                "Show": true,
                "DataType": "inout",
                "Width": 100
            },
            {
                "Name": "LogTime",
                "DataField": "CheckTime",
                "Fixed": false,
                "Show": true,
                "Width": 150,
                "DataType": "date",
                "Format": "YYYY-MM-DD HH:mm:ss"
            },
            {
                "Name": "Status",
                "DataField": "Success",
                "Fixed": true,
                "Show": true,
                "DataType": "status",
                "Width": 100
            },
            // {
            //     "Name": "Position",
            //     "DataField": "Position",
            //     "Fixed": false,
            //     "Show": true,
            //     "Width": 250
            // },
            // {
            //     "Name": "Object",
            //     "DataField": "ObjectType",
            //     "Fixed": true,
            //     "Show": true,
            //     "Width": 200
            // },
            {
                "Name": "Approve",
                "DataField": "ApproveStatus",
                "Fixed": false,
                "Show": true,
                "Width": 100
            },
            {
                "Name": "ViolationInfo",
                "DataField": "Error",
                "Fixed": false,
                "Show": true,
                "Width": 150
            },
            {
                "Name": "Note",
                "DataField": "Note",
                "Fixed": false,
                "Show": true,
                "Width": 100
            },
            {
                "Name": "RegisterImage",
                "DataField": "RegisterImage",
                "Fixed": false,
                "Show": true,
                "Width": 100,
                "DataType": "image"
            },{
                "Name": "VerifyImage",
                "DataField": "VerifyImage",
                "Fixed": false,
                "Show": true,
                "Width": 100,
                "DataType": "image"
            }
        ];
    }
    connectServer() {
        configApi.GetRealTimeServerLink().then((res: any) => {
            this.realtimeServer = res.data;
            this.connectToAttendanceLogRealTimeServer(this.realtimeServer);
        });
        configApi.GetServerLink().then((res: any) => {
            this.server = res.data;
        });
    }

    getWisenetWaveServerConfig() {
        configApi.GetWisenetWaveServerConfig().then((res: any) => {
            if(res && res.data){
                this.wisenetWaveServerConfig = res.data;
                // console.log(this.wisenetWaveServerConfig)
                this.connectWisenetWaveServer();
            }
        });
    }
    connectWisenetWaveServer() {
        axios.post('https://' + (this.wisenetWaveServerConfig as any).cloudAddress + '/rest/v2/login/sessions', {
            "username": (this.wisenetWaveServerConfig as any).username,
            "password": (this.wisenetWaveServerConfig as any).password,
            "setCookie": true,
        }).then(async (res: any) => {
            // console.log(res)
            if(res && res.status && res.status == 200){
                // console.log(res)
                if(res.request.responseURL && res.request.responseURL.split("/")[2] 
                && res.request.responseURL.split("/")[2] != (this.wisenetWaveServerConfig as any).cloudAddress){
                    axios.post('https://' + res.request.responseURL.split("/")[2] + '/rest/v2/login/sessions', {
                        "username": (this.wisenetWaveServerConfig as any).username,
                        "password": (this.wisenetWaveServerConfig as any).password,
                        "setCookie": true,
                    }).then(async (res: any) => {
                        // console.log(res)
                        if(res && res.status && res.status == 200){
                            (this.wisenetWaveServerConfig as any).cloudAddress = res.request.responseURL.split("/")[2];
                            this.wisenetWaveToken = res.data.token;
                            document.cookie = "x-runtime-guid=" + this.wisenetWaveToken;
                            // console.log(this.wisenetWaveToken);
                            this.getWisenetWaveServerCamera();
                        }
                    });
                }else{
                    // console.log(this.wisenetWaveToken);
                    this.wisenetWaveToken = res.data.token;
                    document.cookie = "x-runtime-guid=" + this.wisenetWaveToken;
                    this.getWisenetWaveServerCamera();
                }
            }
        });
    }
    getWisenetWaveServerCamera() {
        let instance = axios.create({
            httpsAgent: new https.Agent({  
              rejectUnauthorized: false,
            })
        });

        instance.defaults.headers.common['Authorization'] = `Bearer ${this.wisenetWaveToken}`;

        instance.get('https://' + (this.wisenetWaveServerConfig as any).cloudAddress + '/rest/v2/devices')
        .then((devicesRes: any) => {
        if(devicesRes && devicesRes.data){
            this.wisenetWaveCamera = devicesRes.data;
            // console.log(this.wisenetWaveCamera)
        }
        }).catch((err) => {
            // // console.log(err)
            // // console.log(err.response)
        });
    }
    getLength = (headers) => {
        let contentLength = -1;
        headers.split("\n").forEach((header, _) => {
            const pair = header.split(":");
            if (pair[0] === this.CONTENT_LENGTH) {
                contentLength = pair[1];
            }
        });
        return contentLength;
    };
    turnOnStreamCameraIn(camera, index){
        this.SOI[0] = 0xff;
        this.SOI[1] = 0xd8;
        const headers = new Headers();
        headers.append('Authorization', `Bearer ${this.wisenetWaveToken}`);
        // console.log(this.wisenetWaveToken)
        // API stream không thể gọi bằng axios GET thông thường, phải dùng fetch để lấy ra kết quả liên tục 
        // và render dưới dạng ảnh jpeg
        // Note: không thể gán API stream trực tiếp cho thuộc tính "src" của thẻ image hoặc video 
        // vì không thể set trực tiếp header chứa bearer token authorization 
        const controller = new AbortController();
        const signal = controller.signal;
        fetch('https://' + (this.wisenetWaveServerConfig as any).cloudAddress + '/media/' + camera + '.mpjpeg', 
        {
          headers, signal
        }).then((res) => {
            // console.log(res)
            let headers = "";
            let contentLength = -1;
            let imageBuffer = null;
            let bytesRead = 0;
        
            // calculating fps. This is pretty lame. Should probably implement a floating window function.
            let frames = 0;
        
            setInterval(() => {
                // // // console.log("fps : " + frames);
                frames = 0;
            }, 1000);
        
            const reader = res.body.getReader();
        
            let read = () => {
                reader.read().then(({done, value}) => {
                // // // console.log(value);
                if(value.length > 0){
                    // console.log(String.fromCharCode(...value));
                    try {
                        var restRes = JSON.parse(String.fromCharCode(...value));
                        if(restRes && restRes.errorString && restRes.errorString != ""){
                            console.log("cameraIn", restRes)
                            this.lineData.CameraIn.Status = "Offline";
                            return;
                        }
                    } catch (e) {
                        // console.log(e)
                    }
                }
                if(done){
                    // // // console.log("end");
                    return;
                }
        
                for (let index = 0; index < value.length; index++) {
                    // we've found start of the frame. Everything we've read till now is the header.
                    if (value[index] === this.SOI[0] && value[index + 1] === this.SOI[1]) {
                    // // // console.log('header found : ' + newHeader);
                    contentLength = this.getLength(headers);
                    // // // console.log("Content Length : " + newContentLength);
                    imageBuffer = new Uint8Array(
                        new ArrayBuffer(contentLength)
                    );
                    }
                    // we're still reading the header.
                    if (contentLength <= 0) {
                    headers += String.fromCharCode(value[index]);
                    }
                    // we're now reading the jpeg.
                    else if (bytesRead < contentLength) {
                    imageBuffer[bytesRead++] = value[index];
                    }
                    // we're done reading the jpeg. Time to render it.
                    else {
                    // // // console.log("jpeg read with bytes : " + bytesRead);
                    // document.getElementById("motionjpeg").setAttribute('src',URL.createObjectURL(
                    //   new Blob([imageBuffer], { type: this.TYPE_JPEG })
                    // ));
                    // this.streamSrcCameraIn = URL.createObjectURL(
                    //     new Blob([imageBuffer], { type: this.TYPE_JPEG })
                    // );
                    this.streamSrcCameraIn = btoa(String.fromCharCode.apply(null, imageBuffer));
                    // // console.log(this.streamSrcCameraIn)
                    
                    frames++;
                    contentLength = 0;
                    bytesRead = 0;
                    headers = "";
                    }
                }
                if(this.stopStreamCameraIn){
                    // document.getElementById("motionjpeg").setAttribute('src', "");
                    this.streamSrcCameraIn = "";
                    controller.abort();
                }else{
                    read();
                }
                });
            };
            if(this.stopStreamCameraIn){
                // document.getElementById("motionjpeg").setAttribute('src', "");
                this.streamSrcCameraIn = "";
                controller.abort();
            }else{
                read();
            }
        }).catch((ex) => {
            console.log(ex)
            console.log(ex.response)
            this.lineData.CameraIn.Status = "Offline";
        });
    }
    turnOnStreamCameraOut(camera, index){
        this.SOI[0] = 0xff;
        this.SOI[1] = 0xd8;
        const headers = new Headers();
        headers.append('Authorization', `Bearer ${this.wisenetWaveToken}`);
        // API stream không thể gọi bằng axios GET thông thường, phải dùng fetch để lấy ra kết quả liên tục 
        // và render dưới dạng ảnh jpeg
        // Note: không thể gán API stream trực tiếp cho thuộc tính "src" của thẻ image hoặc video 
        // vì không thể set trực tiếp header chứa bearer token authorization 
        const controller = new AbortController();
        const signal = controller.signal;
        fetch('https://' + (this.wisenetWaveServerConfig as any).cloudAddress + '/media/' + camera + '.mpjpeg', 
        {
          headers, signal
        }).then((res) => {
            // // // console.log(res)
            let headers = "";
            let contentLength = -1;
            let imageBuffer = null;
            let bytesRead = 0;
        
            // calculating fps. This is pretty lame. Should probably implement a floating window function.
            let frames = 0;
        
            setInterval(() => {
                // // // console.log("fps : " + frames);
                frames = 0;
            }, 1000);
        
            const reader = res.body.getReader();
        
            let read = () => {
                reader.read().then(({done, value}) => {
                // // // console.log(value);
                if(value.length > 0){
                    // console.log(String.fromCharCode(...value));
                    try {
                        var restRes = JSON.parse(String.fromCharCode(...value));
                        if(restRes && restRes.errorString && restRes.errorString != ""){
                            console.log("cameraOut", restRes)
                            this.lineData.CameraOut.Status = "Offline";
                            return;
                        }
                    } catch (e) {
                        // console.log(e)
                    }
                }
                if(done){
                    // // // console.log("end");
                    return;
                }
        
                for (let index = 0; index < value.length; index++) {
                    // we've found start of the frame. Everything we've read till now is the header.
                    if (value[index] === this.SOI[0] && value[index + 1] === this.SOI[1]) {
                    // // // console.log('header found : ' + newHeader);
                    contentLength = this.getLength(headers);
                    // // // console.log("Content Length : " + newContentLength);
                    imageBuffer = new Uint8Array(
                        new ArrayBuffer(contentLength)
                    );
                    }
                    // we're still reading the header.
                    if (contentLength <= 0) {
                    headers += String.fromCharCode(value[index]);
                    }
                    // we're now reading the jpeg.
                    else if (bytesRead < contentLength) {
                    imageBuffer[bytesRead++] = value[index];
                    }
                    // we're done reading the jpeg. Time to render it.
                    else {
                    // // // console.log("jpeg read with bytes : " + bytesRead);
                    // document.getElementById("motionjpeg").setAttribute('src',URL.createObjectURL(
                    //   new Blob([imageBuffer], { type: this.TYPE_JPEG })
                    // ));
                    // this.streamSrcCameraOut = URL.createObjectURL(
                    //     new Blob([imageBuffer], { type: this.TYPE_JPEG })
                    // );
                    this.streamSrcCameraOut = btoa(String.fromCharCode.apply(null, imageBuffer));
                    // // console.log(this.streamSrcCameraOut)

                    frames++;
                    contentLength = 0;
                    bytesRead = 0;
                    headers = "";
                    }
                }
                if(this.stopStreamCameraOut){
                    // document.getElementById("motionjpeg").setAttribute('src', "");
                    this.streamSrcCameraOut = "";
                    controller.abort();
                }else{
                    read();
                }
                });
            };
            if(this.stopStreamCameraOut){
                // document.getElementById("motionjpeg").setAttribute('src', "");
                this.streamSrcCameraOut = "";
                controller.abort();
            }else{
                read();
            }
        }).catch((ex) => {
            console.log(ex)
            console.log(ex.response)
            this.lineData.CameraOut.Status = "Offline";
        });
    }

    loadWisenetCameraStream(lineData) {
        if(lineData){
            if(lineData.CameraIn && lineData.CameraIn.length > 0){
                if(lineData.CameraIn[0].Serial != ""){
                    const camera = this.wisenetWaveCamera.find(x => x.id == "{" + lineData.CameraIn[0].Serial + "}");
                    // console.log(camera)
                    if(camera){
                        this.stopStreamCameraIn = false;
                        // console.log("turnOnStreamCameraIn")
                        this.turnOnStreamCameraIn(camera[0].id.substring(1, camera[0].id.length - 1), lineData.CameraIn[0].Index);
                    }
                }else{
                    this.lineData.CameraIn.Status = "Offline";
                }
            }
            if(lineData.CameraOut && lineData.CameraOut.length > 0){
                if(lineData.CameraOut[0].Serial != ""){
                    const camera = this.wisenetWaveCamera.find(x => x.id == "{" + lineData.CameraOut[0].Serial + "}");
                    // console.log(camera)
                    if(camera){
                        this.stopStreamCameraOut = false;
                        // console.log("turnOnStreamCameraOut")
                        this.turnOnStreamCameraOut(camera[0].id.substring(1, camera[0].id.length - 1), lineData.CameraOut[0].Index);
                    }
                }else{
                    this.lineData.CameraOut.Status = "Offline";
                }
            }
        }
    }

    async setSrcCameraIn(src){
        this.streamSrcCameraIn = src;
    }

    async setSrcCameraOut(src){
        this.streamSrcCameraOut = src;
    }

    async loadHikVisionCameraStream(lineData) {
        if(this.arrWebSocket && this.arrWebSocket.length > 0){
            this.arrWebSocket.forEach(element => {
                element.close();
            });
            this.arrWebSocket = [];
        }

        if(this.arrWebSocketInterval && this.arrWebSocketInterval.length > 0){
            this.arrWebSocketInterval.forEach(element => {
                clearInterval(element);
            });
            this.arrWebSocketInterval = [];
        }

        if(lineData){
            // let isConnectingCamera = false;
            if(lineData.CameraIn && lineData.CameraIn.length > 0){
                // isConnectingCamera = true;
                let urlIn = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/InitializeWebSocket?id=` 
                + this.pageId + `&action=add&cameraIndex=` 
                + lineData.CameraIn[0].Index.toString();
                // console.log('url in is: ' + urlIn);

                let webSocketIn = new WebSocket(urlIn);

                this.arrWebSocket.push(webSocketIn);

                webSocketIn.onmessage = (event) => {
                    // console.log("in", event.data)
                    if(event.data == "ErrorConnect"){
                        this.lineData.CameraIn.Status = "Offline";
                        console.log("camera in ErrorConnect")
                        webSocketIn.close();
                    }else{
                        this.setSrcCameraIn(event.data); 
                    }
                }

                webSocketIn.onopen = () => {
                    console.log('WebSocket in opened');
                };
                
                webSocketIn.onclose = () => {
                    console.log('WebSocket in closed');

                    const index = this.arrWebSocket.indexOf(webSocketIn);
                    if (index > -1) {
                        this.arrWebSocket.splice(index, 1);
                    }
                };

                let intervalId = setInterval(async () => {
                    var checkSocketUrl = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/CheckStatusWebsocket?id=`
                        + this.pageId + `&action=check&cameraIndex=`
                        + lineData.CameraIn[0].Index.toString();
                    var checkSocket = new WebSocket(checkSocketUrl);

                    checkSocket.onmessage = (event) => {
                        console.log(event)
                    }

                    checkSocket.onopen = () => {
                        console.log('CheckSocket opened');
                    };

                    checkSocket.onclose = () => {
                        console.log('CheckSocket closed');
                    };
                }, 10000);
                this.arrWebSocketInterval.push(intervalId);
            }

            await new Promise(resolve => setTimeout(resolve, 1000));

            if(lineData.CameraOut && lineData.CameraOut.length > 0){
                // isConnectingCamera = true;
                let urlOut = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/InitializeWebSocket?id=`
                + this.pageId + `&action=add&cameraIndex=` 
                + lineData.CameraOut[0].Index.toString();
                // console.log('url out is: ' + urlOut);

                let webSocketOut = new WebSocket(urlOut);

                this.arrWebSocket.push(webSocketOut);

                webSocketOut.onmessage = (event) => {
                    // console.log("out", event.data)
                    if(event.data == "ErrorConnect"){
                        this.lineData.CameraOut.Status = "Offline";
                        console.log("camera out ErrorConnect")
                        webSocketOut.close();
                    }else{
                        this.setSrcCameraOut(event.data);
                    }                    
                }

                webSocketOut.onopen = () => {
                    console.log('WebSocket out opened');
                };
                
                webSocketOut.onclose = () => {
                    console.log('WebSocket out closed');

                    const index = this.arrWebSocket.indexOf(webSocketOut);
                    if (index > -1) {
                        this.arrWebSocket.splice(index, 1);
                    }
                };

                let intervalId = setInterval(async () => {
                    var checkSocketUrl = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/CheckStatusWebsocket?id=`
                        + this.pageId + `&action=check&cameraIndex=`
                        + lineData.CameraOut[0].Index.toString();
                    var checkSocket = new WebSocket(checkSocketUrl);

                    checkSocket.onmessage = (event) => {
                        console.log(event)
                    }

                    checkSocket.onopen = () => {
                        console.log('CheckSocket opened');
                    };

                    checkSocket.onclose = () => {
                        console.log('CheckSocket closed');
                    };
                }, 10000);
                this.arrWebSocketInterval.push(intervalId);
            }

            // if(isConnectingCamera){
            //     this.connectingDevice = true;
            //     await new Promise(res => setTimeout(res, 10000)).then(() => {
            //         this.connectingDevice = false;
            //     });
            // }
        }
    }

    mounted() {
        const access_token = localStorage.getItem('access_token');
        if (access_token) {
            if (!this.roleByRoute || (this.roleByRoute && !this.roleByRoute.length)) {
            }
        }
        this.changeSizeElement();
    }

    changeSizeElement() {
        const avatarWidth = document.getElementById("divAvatar").offsetWidth;
        this.avatarHeight = avatarWidth + 30;
    }

    async getListData() {
        // just ignore Mixin load data
    }

    async loadHistoryData(lineIndex) {
        if (lineIndex == "0" || lineIndex == "") {
            this.listHistory = [];
            return;
        }
        await timeLogApi.GetWalkerMonitoringHistoryListByLineIndex(lineIndex, 30).then((res: any) => {
            if (res.status == 200) {
                const listData: Array<WalkerHistoryInfo> = res.data;
                // this.listHistory = res.data;
                this.listHistory = listData.map(e => Object.assign(e, { ApproveStatus: e.ApproveStatus == "0" ? "Waiting" : "Approved" }));
                if(this.monitorModel && this.monitorModel.Index > 0){
                    const indexToMove = this.listHistory.findIndex(item => item.LogIndex === this.monitorModel.Index);
                    if(indexToMove > -1){
                        this.listHistory = [
                            this.listHistory[indexToMove],
                            ...this.listHistory.slice(0, indexToMove),
                            ...this.listHistory.slice(indexToMove + 1)
                        ];
                    }
                }
                this.tableHistoryHeight = this.isFullscreenOn ? "400" : "370";

            }
        });
    }

    async loadLines() {
        await lineApi.GetAllLineBasic().then((res: any) => {
            if (res.status == 200 && res.statusText == "OK") {
                this.listLine = res.data;
            }
        });
    }

    handleChangeLine(){
        this.isException = false;
        this.cccd = "";
        this.exceptionReason = "";
        this.isFoundData = false;
        this.cbbLineChanged(this.lineIndex);
    }

    async cbbLineChanged(val) {
        // console.log('dsa');
        this.lineData = {CameraIn: null, CameraOut: null, DeviceIn: null, DeviceOut: null, 
            LineControllersIn: null, LineControllersOut: null};
        if(!val || (val && val == "")){
            this.disConnectingDevice = true;

            setTimeout(() => {
                this.disConnectingDevice = false;
                this.getLineDeviceData();
            }, 10000);
            if(this.realtimeCameraType == "Hikvision"){
                if(this.arrWebSocket && this.arrWebSocket.length > 0){
                    this.arrWebSocket.forEach(element => {
                        element.close();
                    });
                    this.arrWebSocket = [];
                }
                if(this.arrWebSocketInterval && this.arrWebSocketInterval.length > 0){
                    this.arrWebSocketInterval.forEach(element => {
                        clearInterval(element);
                    });
                    this.arrWebSocketInterval = [];
                }
    
                let urlIn = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/RemoveWebsocketByPageId?id=` 
                + this.pageId;
                // console.log('url in is: ' + urlIn);
        
                let removeWebSocket = new WebSocket(urlIn);
        
                removeWebSocket.onmessage = (event) => {
                    console.log(event)
                }
        
                removeWebSocket.onopen = () => {
                    console.log('removeWebSocket in opened');
                };
                
                removeWebSocket.onclose = () => {
                    console.log('removeWebSocket in closed');
                };                
            }

            this.realtimeConnected = false;
            this.stopStreamCameraIn = true;
            this.stopStreamCameraOut = true;
        }else{
            this.connectingDevice = true;

            // await new Promise(res => setTimeout(res, 10000)).then(() => {
            //     this.connectingDevice = false;
            // });
            setTimeout(() => {
                this.connectingDevice = false;
                this.getLineDeviceData();
            }, 10000);

            if(this.allLinesData && this.allLinesData.length > 0){
                var lineData = this.allLinesData.find(x => x.Index == val);
                this.lineData.CameraIn = lineData.CameraIn && lineData.CameraIn.length > 0 ? lineData.CameraIn[0] : null;
                if(this.lineData.CameraIn){
                    this.lineData.CameraIn.Status = "Offline";
                }
                this.lineData.CameraOut = lineData.CameraOut && lineData.CameraOut.length > 0 ? lineData.CameraOut[0] : null;
                if(this.lineData.CameraOut){
                    this.lineData.CameraOut.Status = "Offline";
                }

                this.lineData.DeviceIn = lineData.DeviceIn;
                this.lineData.DeviceOut = lineData.DeviceOut;
                this.lineData.LineControllersIn = lineData.LineControllersIn;
                this.lineData.LineControllersOut = lineData.LineControllersOut;

                let controllerIndexes = [];
                if(this.lineData.LineControllersIn && this.lineData.LineControllersIn.length > 0){
                    controllerIndexes = controllerIndexes.concat(this.lineData.LineControllersIn.map(x => x.Index));
                }
                if(this.lineData.LineControllersOut && this.lineData.LineControllersOut.length > 0){
                    controllerIndexes = controllerIndexes.concat(this.lineData.LineControllersOut.map(x => x.Index));
                }
                if(controllerIndexes && controllerIndexes.length > 0){
                    await relayControllerApi.TelnetMultipleRelayController(controllerIndexes).then((res: any) => {
                        if(res.data){
                            if(this.lineData.LineControllersIn && this.lineData.LineControllersIn.length > 0){
                                this.lineData.LineControllersIn.forEach(element => {
                                    if(res.data[element.Index]){
                                        element.Status = res.data[element.Index] ? this.$t('Online').toString() : this.$t('Offline').toString();
                                    }else{
                                        element.Status = this.$t('Offline').toString();
                                    }
                                });
                            }
                            if(this.lineData.LineControllersOut && this.lineData.LineControllersOut.length > 0){
                                this.lineData.LineControllersOut.forEach(element => {
                                    if(res.data[element.Index]){
                                        element.Status = res.data[element.Index] ? this.$t('Online').toString() : this.$t('Offline').toString();
                                    }else{
                                        element.Status = this.$t('Offline').toString();
                                    }
                                });
                            }
                        }
                    })
                }

                if(lineData){
                    switch (this.realtimeCameraType) {
                        case 'Hikvision':
                            if(this.realtimeCameraType == "Hikvision"){
                                if(this.arrWebSocket && this.arrWebSocket.length > 0){
                                    this.arrWebSocket.forEach(element => {
                                        element.close();
                                    });
                                    this.arrWebSocket = [];
                                }
                                if(this.arrWebSocketInterval && this.arrWebSocketInterval.length > 0){
                                    this.arrWebSocketInterval.forEach(element => {
                                        clearInterval(element);
                                    });
                                    this.arrWebSocketInterval = [];
                                }
                    
                                let urlIn = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/RemoveWebsocketByPageId?id=` 
                                + this.pageId;
                                // console.log('url in is: ' + urlIn);
                        
                                let removeWebSocket = new WebSocket(urlIn);
                        
                                removeWebSocket.onmessage = (event) => {
                                    console.log(event)
                                }
                        
                                removeWebSocket.onopen = () => {
                                    console.log('removeWebSocket in opened');
                                };
                                
                                removeWebSocket.onclose = () => {
                                    console.log('removeWebSocket in closed');
                                };                               
                            }
    
                            await this.loadHikVisionCameraStream(lineData);
                            break;
                        case 'Wisenet':
                            this.loadWisenetCameraStream(lineData);
                            break;
                    }
                }
            }
        }    
        this.connectToRealTimeServer(this.realtimeServer, val);
        await this.loadHistoryData(val);    
    }

    createGridColumns() {

    }

    createMonitorObject() {
        this.monitorModel = {
            Index: 0,
            EmployeeATID: "",
            CheckTime: "",
            Success: false,
            InOut: 0,
            Error: "",
            CompanyIndex: 0,
            LineIndex: 0,
            ListInfo: [],
            ObjectType: "",
            RegisterImage: "",
            VerifyImage: ""
        };
    }

    async OpenDoorClicked(action: string) {
        if (!this.isException && this.monitorModel.Index == 0) {
            return;
        }
        if(this.isException && !this.isFoundData){
            this.$notify({
                title: this.$t('Notify').toString(),
                message: this.$t('UserNotFound').toString(),
                type: 'error',
            });
            return;
        }
        if (action == "Open" && (!this.isException && this.monitorModel.Success == false && this.note == "")
        || (this.isException && this.isFoundData && (!this.exceptionReason || this.exceptionReason == ''))) {
            this.$notify({
                title: this.$t('Notify').toString(),
                message: this.$t('MSG_ReasonIsRequired').toString(),
                type: 'error',
            });
            return;
        }
        if(!this.rdInOut || this.rdInOut == ''){
            this.$notify({
                title: this.$t('Notify').toString(),
                message: this.$t('PleaseSelectInOutMode').toString(),
                type: 'error',
            });
            return;
        }
        console.log(this.monitorModel);
        const param: LogParam = {
            Index: this.monitorModel?.Index ?? 0,
            InOut: parseInt(this.rdInOut),
            OpenController: action == "Open" ? true : false,
            Note: this.note,
            LineIndex: (this.lineIndex && this.lineIndex != '') ? parseInt(this.lineIndex) : 0,
            UserName: localStorage.getItem("username"),
            IsException: this.isException,
            ExceptionReason: this.exceptionReason,
            EmployeeATID: this.monitorModel.ListInfo.find(x => x.Title == "EmployeeATID").Data,
            CardNumber: this.monitorModel?.CardNumber ?? ''
        };

        await realtimeApi.UpdateLogStatus(param).then(async (res: any) => {
            if (res.status == 200 && res.data == "") {
                this.classInfo = "border-approved";
                if (this.errorClass == "red-background") this.errorClass = "";
                this.$notify({
                    title: this.$t('Notify').toString(),
                    message: action == "Open" ? this.$t('OpenBarrierSuccess').toString() : this.$t('CloseBarrierSuccess').toString(),
                    type: 'success',
                });
            }
            else {
                if (res.data == "OpenBarrierFailed") {
                    this.classInfo = "border-approved";
                }
                else {
                    this.classInfo = "border-unapprove";
                }
                const msg = action == "Open" ? this.$t('OpenBarrierFailed').toString() : this.$t('CloseBarrierFailed').toString();
                this.$notify({
                    title: this.$t('Notify').toString(),
                    message: msg,
                    type: 'error',
                });
            }
            await this.loadHistoryData(this.lineIndex);
        });
    }

    async OpenDoorAuto() {
        if (this.monitorModel.Index == 0 || this.monitorModel.Success == false) {
            return;
        }
        const param: LogParam = {
            Index: this.monitorModel.Index,
            InOut: parseInt(this.rdInOut),
            OpenController: true,
            Note: 'SYSTEM_AUTO',
            LineIndex: parseInt(this.lineIndex),
            UserName: localStorage.getItem("username")
        };
        await realtimeApi.UpdateLogStatusAuto(param).then(async (res: any) => {
            if (res.status == 200) {
                this.classInfo = "border-approved";
                this.$notify({
                    title: this.$t('Notify').toString(),
                    message: this.$t('OpenBarrierSuccess').toString(),
                    type: 'success',
                });
            }
            else {
                if (res.MessageCode == "OpenBarrierFailed") {
                    this.classInfo = "border-approved";
                }
                else {
                    this.classInfo = "border-unapprove";
                }
                const msg = this.$t('OpenBarrierFailed').toString();
                this.$notify({
                    title: this.$t('Notify').toString(),
                    message: msg,
                    type: 'error',
                });
            }
            await this.loadHistoryData(this.lineIndex);
        });
    }

    async checkRulesWarning(data) {
        // console.log("checkRulesWarning")
        if ((data.Error != null && data.Error != "") && data.CustomerIndex != 0) {
            await hrCustomerInfoApi.GetCustomerById(data.EmployeeATID).then(async (res: any) => {
                const data = res.data;
                if (data.IsVip) {
                    await this.handlerError(data, false, false);
                    await this.handlerError(data, true, true);
                }
            });
        } else if (data.CustomerIndex != 0) {
            await hrCustomerInfoApi.GetCustomerById(data.EmployeeATID).then(async (res: any) => {
                const data = res.data;
                if (data.IsVip) {
                    await this.handlerError(data, true, false);
                }
            });
        } else if (data.Error != null && data.Error != "") {
            await this.handlerError(data, false, false);
        }
    }

    async handlerError(data, isVip, skipEmail: boolean) {
        let error = data.Error;
        // console.log(error)
        if (isVip) {
            error = "IsVipCustomer";
        }
        const warning = this.rulesWarning.find(e => e.GroupCode == error);
        if (warning != null) {
            // // console.log(warning)
            if (warning.UseSpeaker || warning.UseLed) {
                await realtimeApi.CallControllerByParam(warning.Index, data.Index).then((res: any) => {
                    if (res.status == 200 && res.data) {
                        // console.log("Call Controller Success");
                    }
                    else {
                        this.$notify({
                            title: this.$t('Notify').toString(),
                            message: this.$t('CallControllerFailed').toString() + "\n" + this.$t(res.data.MessageCode).toString(),
                            type: 'error',
                        });
                    }
                })
                    .catch((err) => {
                        this.$notify({
                            title: this.$t('Notify').toString(),
                            message: this.$t('CallControllerFailed').toString() + "\n" + err.toString(),
                            type: 'error',
                        });
                    });
            }

            if (warning.UseEmail) {
                if (warning.EmailSendType == 1) { //Gửi theo lịch

                } else { // Gửi ngay
                    if (skipEmail == false) {
                        await rulesWarningApi.CheckAndSendMail(warning.Email, data).then((res: any) => {
                            // console.log("CheckAndSendMail", res);
                        });
                    }

                }
            }

            if (warning.UseComputerSound) {
                if (warning.ComputerSoundPath != "") {
                    const ezFile = JSON.parse(warning.ComputerSoundPath);
                    await ezPortalFileApi.GetFilePath(ezFile[0]).then((res: any) => {
                        const data = res.data;
                        const snd = new Audio("data:audio/wav;base64," + data);
                        snd.play();
                    });
                }
            }

            // if (warning.UseChangeColor) {
            //     this.errorClass = "red-background";
            // }
        }

    }

    async updateDashboard(data) {
        console.log("updateDashboard", data)
        this.isProcessInfo = false;
        this.note = "";
        this.classInfo = data.Success == true ? "border-approved": "border-unapprove";
        this.errorClass = "";

        this.monitorModel = { ...data };

        if (!Misc.isEmpty(data.RegisterImage)) {
            this.monitorModel.RegisterImage = this.Base64_Encoding + data.RegisterImage;
            // if (data.RegisterImage.startsWith('[')) {
            //     try {
            //         const ezFilesUser: EzFile[] = JSON.parse(data.RegisterImage);
            //         const img = ezFilesUser.map(img => ({ name: img.Name, url: img.Url } as EzFileImage));
            //         if (img.length > 0) {
            //             this.monitorModel.RegisterImage = img[0].url;
            //             await ezPortalFileApi.GetFilePath({ Name: img[0].name, Url: img[0].url }).then((done: any) => {
            //                 const bytes = done.data.Data;
            //                 const url = 'data:' + mime.lookup(name) + ';base64,' + bytes;
            //                 this.monitorModel.RegisterImage = url;
            //             });
            //         }
            //     } catch (error) {
            //         this.monitorModel.RegisterImage = data.RegisterImage
            //     }
            // }
            // else {

            // }
        } else {
            this.monitorModel.RegisterImage = this.imageDefault;
        }

        if (!Misc.isEmpty(data.VerifyImage)) {
            this.monitorModel.VerifyImage = this.Base64_Encoding + data.VerifyImage;
        } else {
            this.monitorModel.VerifyImage = this.imageDefault;
        }

        this.rdInOut = this.monitorModel.InOut.toString();
        if (this.manualControl == false && this.usingGeneralRule == false) {
            await this.OpenDoorAuto();
            
        }else{
            await this.addRowToTable(this.monitorModel);
        }
    }

    async addRowToTable(logModel) {
        const logImage = Misc.cloneData(this.monitorModel.VerifyImage);
        await timeLogApi.GetWalkerMonitoringHistoryByLogIndex(logModel.Index).then((res: any) => {
            if (res.status == 200) {
                const data: WalkerHistoryInfo = res.data;
                data.VerifyImage = logImage;
                data.ApproveStatus = data.ApproveStatus == "0" ? "Waiting" : "Approved";
                if (this.listHistory.length > this.maxRow) {
                    this.listHistory.pop();
                }
                this.listHistory.unshift(data);
            }
        });
    }

    showProgress(data) {
        if (data.Form == "WalkerMonitoring") {
            this.createMonitorObject();
            this.isProcessInfo = true;
        }
    }

    showError(error) {
        if (this.$route.path == "/customer-monitoring-page") {
            // console.log(`onReceiveError`, error)
        this.isProcessInfo = false;
        this.$notify({
            title: this.$t('Notify').toString(),
            message: this.$t(error).toString(),
            type: 'error',
        });
        }
    }

    connectToRealTimeServer(link, lineIndex) {
        // this.realtimeConnection = new signalR.HubConnectionBuilder()
        //     .withUrl(link + "/monitoringHub")
        //     .configureLogging(signalR.LogLevel.Information)
        //     .build();
        console.log(link);
        this.realtimeConnection = new HubConnectionBuilder()
            .withUrl(link + "/monitoringHub")
            .configureLogging(LogLevel.Information)
            .build();

        // receive data from server
        this.realtimeConnection.on("ReceiveCustomerWalkerLog", async data => {
            // console.log('Receive Walker log', data);
            this.isException = false;
            this.cccd = "";
            this.exceptionReason = "";
            this.isFoundData = false;
            if(data.InOut == 1){
                if(this.streamSrcCameraIn && this.streamSrcCameraIn != ""){
                    data.VerifyImage = this.streamSrcCameraIn;
                }
            }
            if(data.InOut == 2){
                if(this.streamSrcCameraOut && this.streamSrcCameraOut != ""){
                    data.VerifyImage = this.streamSrcCameraOut;
                }
            }
            if(this.lineIndex == data.LineIndex)
            {
                await this.saveImageCamera(data);
                await this.updateDashboard(data);
                await this.checkRulesWarning(data);
            }
        });

        this.realtimeConnection.on("ReceiveError", data => {
            console.log('Receive error', data);
            this.showError(data);
        });

        let companyIndex = localStorage.getItem("company_index");
        if (companyIndex == null) {
            companyIndex = "2";
        }
        this.realtimeConnection
            .start()
            .then(() => {
                this.realtimeConnected = true;
                // console.log(this.realtimeConnected)
                this.realtimeConnection
                    .invoke("AddUserToGroup", companyIndex, lineIndex, self.localStorage.getItem("user") + "_customerMonitoring")
                    .catch(err => {
                        // console.log('AddUserToGroup', err);
                    });
                // console.log('Start Hub')
            })
            .catch(err => {
                this.realtimeConnected = false;
                console.log(err.toString());
            });

        this.realtimeConnection.onclose(() => {
            this.realtimeConnected = false;
            this.connectToRealTimeServer(link, lineIndex);
        });
    }

    async saveImageCamera(data){
        // console.log("saveImageCamera")
        if(data.InOut == 1){
            if(this.streamSrcCameraIn && this.streamSrcCameraIn != ""){
                await realtimeApi.SaveImageFromBase64(this.streamSrcCameraIn.toString(), this.jpeg, 
                data.EmployeeATID + "_" + moment(data.CheckTime, "DD/MM/YYYY HH:mm:ss").format("DDMMYYYYHHmmss") + "_In").
                then((res: any) => {
                    // console.log(res)
                });
            }
        }
        if(data.InOut == 2){
            if(this.streamSrcCameraOut && this.streamSrcCameraOut != ""){
                await realtimeApi.SaveImageFromBase64(this.streamSrcCameraOut.toString(), this.jpeg, 
                data.EmployeeATID + "_" + moment(data.CheckTime, "DD/MM/YYYY HH:mm:ss").format("DDMMYYYYHHmmss") + "_Out").
                then((res: any) => {
                    // console.log(res)
                });
            }
        }
    }

    beforeDestroy() {
        if(this.arrWebSocket && this.arrWebSocket.length > 0){
            this.arrWebSocket.forEach(element => {
                element.close();
            });
            this.arrWebSocket = [];
        }

        if(this.arrWebSocketInterval && this.arrWebSocketInterval.length > 0){
            this.arrWebSocketInterval.forEach(element => {
                clearInterval(element);
            });
            this.arrWebSocketInterval = [];
        }

        let urlIn = `ws://` + this.server.replace("http://", "") + `/api/RealtimeCamera/RemoveWebsocketByPageId?id=` 
        + this.pageId;
        // console.log('url in is: ' + urlIn);

        let removeWebSocket = new WebSocket(urlIn);

        removeWebSocket.onmessage = (event) => {
            console.log(event)
        }

        removeWebSocket.onopen = () => {
            console.log('removeWebSocket in opened');
        };
        
        removeWebSocket.onclose = () => {
            console.log('removeWebSocket in closed');
        };

        this.stopStreamCameraIn = true;
        this.stopStreamCameraOut = true;
    }

    connectToAttendanceLogRealTimeServer(link) {
        // this.realtimeConnection = new signalR.HubConnectionBuilder()
        //     .withUrl(link + "/monitoringHub")
        //     .configureLogging(signalR.LogLevel.Information)
        //     .build();
        this.attendanceLogRealtimeConnection = new HubConnectionBuilder()
            .withUrl(link + "/attendanceHub")
            .configureLogging(LogLevel.Information)
            .build();

        // receive data from server
        this.attendanceLogRealtimeConnection.on("ReceiveReloadWarningRule", async data => {
            // console.log('ReceiveReloadWarningRule', data);
            await this.getRulesWarningGroup();
            await this.getRulesWarning();
        });

        let companyIndex = localStorage.getItem("company_index");
        if (companyIndex == null) {
            companyIndex = "2";
        }
        this.attendanceLogRealtimeConnection
            .start()
            .then(() => {
                this.attendanceLogRealtimeConnected = true;
                this.attendanceLogRealtimeConnection
                    .invoke("AddUserToGroup", companyIndex, self.localStorage.getItem("user"))
                    .catch(err => {
                        // console.log('AddUserToGroup', err);
                    });
                // console.log('Start attendanceLog Hub')
            })
            .catch(err => {
                this.attendanceLogRealtimeConnected = false;
                // console.log(err.toString());
            });

        this.attendanceLogRealtimeConnection.onclose(() => {
            this.attendanceLogRealtimeConnected = false;
            this.connectToAttendanceLogRealTimeServer(link);
        });
    }

    isFullscreenOn: boolean = false;

    toggleFullScreen(): void {
        if (this.isFullScreen()) {
            this.exitFullScreen();
            this.isFullscreenOn = false;

        } else {
            this.enterFullScreen();
            this.isFullscreenOn = true;
        }
        this.reloadHistoryTable = true;
        this.$nextTick(() => {
            this.reloadHistoryTable = false;
        });
    }

    isFullScreen(): boolean {
        return !!(
            document.fullscreenElement ||
            document.webkitFullscreenElement ||
            document.mozFullScreenElement ||
            document.msFullscreenElement
        );
    }

    enterFullScreen(): void {
        const el = document.documentElement as any;
        if (el.requestFullscreen) {
            el.requestFullscreen();
        } else if (el.webkitRequestFullscreen) {
            el.webkitRequestFullscreen();
        } else if (el.mozRequestFullScreen) {
            el.mozRequestFullScreen();
        } else if (el.msRequestFullscreen) {
            el.msRequestFullscreen();
        }
    }

    exitFullScreen(): void {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
    }


    toggleConfigPanel() {
        this.showConfigPanel = !this.showConfigPanel;
    }

    fullscreenChange(fullscreen) {
        this.fullscreen = fullscreen;
        if (fullscreen) {
            this.tableHistoryHeight = "450";
        }
    }

    async getRulesWarning() {
        await rulesWarningApi.GetRulesWarningByCompanyIndex().then((res: any) => {
            const arrTemp = [];
            res.data.forEach((item) => {
                const warning = this.rulesWarningGroup[item.RulesWarningGroupIndex] || {};
                const a = Object.assign(item, {
                    GroupName: warning.Name,
                    GroupCode: warning.Code
                });
                arrTemp.push(a);
            });
            this.rulesWarning = arrTemp;
        });
    }

    async getRulesWarningGroup() {
        await rulesWarningApi.GetRulesWarningGroup().then((res: any) => {
            const data = res.data;
            const dictData = {};
            data.forEach((e: any) => {
                dictData[e.Index] = {
                    Index: e.Index,
                    Name: e.Name,
                    Code: e.Code
                };
            });
            this.rulesWarningGroup = dictData;
        });
    }

    async getRulesGeneralUsing() {
        await rulesGeneralApi.GetRulesGeneralRunWithoutScreen().then((res: any) => {
            this.usingGeneralRule =  res.data;
        });
    }
}