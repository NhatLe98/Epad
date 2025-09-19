import { Component, Vue, Prop, Mixins, PropSync } from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
import ComponentBase from "@/mixins/application/component-mixins";
import OAuthServices from "@/$core/oauth-services";
import {attendanceLogApi} from '@/$api/attendance-log-api'
import {fileApi,UploadFileResult,ImportProcessParam} from '@/$api/file-api'

@Component({
    name: 'import-popup'
})
export default class ImportPopupComponent extends Mixins(ComponentBase) {
    @Prop({default: ''}) title: string;
    @Prop({default: ''}) classProcess: string;
    @Prop({default: 1}) numberOfFiles: number;

    showDialog:boolean = false;
    linkUpload = "";
    paramHeader=null;
    txtImportResult="";
    fileImport: any;
    listFilePath = [];
    isProcess = false;

    mounted(){
        this.fileImport=null;

    }
    beforeMount() {
        this.paramHeader=this.createHeader();
        Misc.readFileAsync('static/variables/app.host.json').then(x => {
            this.linkUpload = x.Domain + "/" + x.ApiEndpoint + "/File/UserAddFile"; 
        })
    }
    showHideDialog(isShow: boolean){
        this.showDialog = isShow;
    }
    fileChanged(file, fileList){
        
        this.fileImport = file;
    }
    uploadFileSuccess(result,file, fileList){
        if(result.Success == false){
            this.$alertSaveError(null, null, null, this.$t(result.Error).toString());
            return;
        }
        this.listFilePath.push(result.Path);
        this.txtImportResult = "";
    }
    
    importProcess(){
        const param: ImportProcessParam={
            ProcessClass: this.classProcess,
            ListFilePath: this.listFilePath
        };
        this.isProcess = true;
        fileApi.ImportProcess(param).then((res: any)=>{
            this.isProcess=false;
            if (res.status == 200) {
                this.txtImportResult = res.data;
                const upload = this.$refs.upload as any;
                upload.clearFiles();
                this.listFilePath = [];
            }else{
                this.$alertSaveError(null, null, null, this.$t('ImportFailed').toString());
            }
        })
        .catch((err)=>{
            this.isProcess=false;
        });
        
    }
    removeFileSuccess(file, fileList){
        //remove file on server
        const fileResult: UploadFileResult={
            Success: file.response.Success,
            Error: "",
            Path: file.response.Path
        };
        fileApi.UserRemoveFile(fileResult).then((res: any)=>{
            if (res.status == 200) {
                this.listFilePath = this.listFilePath.filter((data)=>{
                    return data!=fileResult.Path;
                });
            }
        });
    }
    createHeader(){
        const param = {'Authorization': `bearer ${OAuthServices.getToken()}`};
        return param;
       
    }
    
}