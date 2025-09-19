import { Component, Vue, Mixins,Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';

import {ICameraParam,cameraApi} from '@/$api/camera-api';
import { isNullOrUndefined } from 'util';

@Component({
    name: "camera-control",
    components: { HeaderComponent,DataTableFunctionComponent,DataTableComponent }
})
export default class CameraControl extends Mixins(ComponentBase) {
    listExcelFunction = ["CheckImageFromCamera"];
    columns=[];
    rowsObj = [];
    isEdit = false;
    showDialog = false;
    showCheckImageDialog = false;
    rules = null;
    cameraModel: ICameraParam = null;
    page = 1;
    arrCameraInfo = [];
    selectedCamera="";
    selectedChannel = "101";
    messageCheck = "";
    cameraCheckImageLink="";
    cameraTypeList=[];

    beforeMount(){
        this.Reset();
        this.CreateRules();
        this.CreateColumns();
        this.cameraTypeList = [
            {Value: "Picture",Dislay:"Picture"},
            {Value: "ANPR",Dislay:"ANPR"}
        ];
    }
    CreateRules(){
        this.rules = {
            Name:[
                { required: true, message:   this.$t("PleaseInputCameraName"), trigger: 'blur' }
            ],
            IpAddress:[
                { required: true, message:   this.$t("PleaseInputIpAddress"), trigger: 'blur' }
            ],
            Port:[
                { required: true, message:   this.$t("PleaseInputPort"), trigger: 'blur' }
            ]
        }
    }
    CreateColumns(){
        this.columns=[
            {
                prop: 'Name',
                label: 'CameraName',
                minWidth: 150,
                fixed: true,
                display: true
            },
            {
                prop: 'IpAddress',
                label: 'IPAddress',
                minWidth: 120,
                fixed: true,
                display: true
            },
            {
                prop: 'Port',
                label: 'Port',
                minWidth: 100,
                display: true
            },
            {
                prop: 'Serial',
                label: 'SerialNumber',
                minWidth: 120,
                display: true
            },
            {
                prop: 'Type',
                label: 'CameraType',
                minWidth: 140,
                display: true
            },
            {
                prop: 'UserName',
                label: 'UserLogin',
                minWidth: 220,
                display: true
            },
            {
                prop: 'Description',
                label: 'Note',
                minWidth: 200,
                display: true
            }
        ];
    }

    Insert(){
        this.showDialog = true;
        if(this.isEdit == true){
            this.Reset();
        }
		this.isEdit = false;
		
    }
    Edit(){
        this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length == 1) {
			this.showDialog = true;
			this.cameraModel = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
    }
    async Delete(){
        const listIndex: Array<number>=this.rowsObj.map((item:any)=>{
            return item.Index;
        });
        
		if (listIndex.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
            await this.$confirmDelete().then(() => {
                cameraApi.DeleteCamera(listIndex).then((res: any) => {
                    (this.$refs.table as any).getTableData(this.page, null, null);
                    if (!isNullOrUndefined(res.status) && res.status === 200) {
                        this.$deleteSuccess();
                    }
                })
                .catch(() => {});
            });			
		}
    }
    ExportToExcel(){
        // not implement
    }
    CheckImageClicked(){
        this.showCheckImageDialog = true;
        this.messageCheck="";
        this.cameraCheckImageLink="";
    }
    ViewClick(){
        if(this.selectedCamera=="" || this.selectedChannel==""){
            return;
        }
        cameraApi.GetCameraPictureByCameraIndex(this.selectedCamera,this.selectedChannel).then((res: any)=>{
            if (!isNullOrUndefined(res.status) && res.status === 200) {
                if(res.data.Success==true){
                  
                    this.messageCheck="";
                    this.cameraCheckImageLink = res.data.Link;
                }
                else{
                    this.messageCheck=res.data.Error;
                    this.cameraCheckImageLink="";
                }
                console.log(res.data);
            }
        });
    }
    async ConfirmClick(){
        (this.$refs.form as any).validate(async (valid) => {
            if(valid == false){
                return;
            }
            if(this.isEdit==false){
                await cameraApi.AddCamera(this.cameraModel).then((res: any)=>{
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog=false;
                });
            }
            else{
                await cameraApi.UpdateCamera(this.cameraModel).then((res: any)=>{
                    if (res.status != undefined && res.status === 200) {
                        this.$saveSuccess();
                    }
                    this.Reset();
                    this.showDialog=false;
                });
            }
            (this.$refs.table as any).getTableData(this.page, null, null);
        });
    }
    async getData({ page, filter, sortParams, pageSize }){
        this.page = page;
		return await cameraApi.GetCameraAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			const arrTemp = [];
			data.data.forEach((item) => {
                const customObject = Object.assign(item, {});
                customObject.Type = this.$t(customObject.Type);
				arrTemp.push(customObject);
            });
            this.arrCameraInfo = arrTemp;
            this.selectedCamera="";
			return {
				data: arrTemp,
				total: data.total,
			};
		});
    }
    Cancel(){
        this.showDialog = false;
    }
    Reset(){
        this.cameraModel = {
            Index: 0,
            Name: '',
            Serial: '',
            IpAddress: '',
            Port: '',
            UserName: '',
            Password: '',
            Description: '',
            Type:'Picture'
        };
    }
}