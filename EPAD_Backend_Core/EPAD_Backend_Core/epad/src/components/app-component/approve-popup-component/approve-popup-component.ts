import { Component, Vue, Prop, Mixins, PropSync } from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
import ComponentBase from "@/mixins/application/component-mixins";

import {employeeTransferApi,WaitingApproveResult} from '@/$api/employee-transfer-api'


@Component({
    name: 'approve-popup'
})
export default class ApprovePopupComponent extends Mixins(ComponentBase) {
    @Prop({default: ''}) title: string;

    showDialog:boolean = false;
    loading = false;
    ListDetails = [];
    waitingApproveList: Array<WaitingApproveResult> = [];

    beforeMount(){
        
    }
    mounted(){
        this.loadData();
    }
    loadData(){
        this.loading = true;
        employeeTransferApi.GetWaitingApproveList().then((res: any)=>{
            this.loading = false;
            if (res.status == 200) {
                this.waitingApproveList = res.data;
                
            }
            else{
                this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
            }
        }).catch((err)=>{
            this.loading = false;
            this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
        });
    }
    formatDateField(date){
        if(date == null || date == ""){
            return "";
        }
        return moment(date).format("YYYY-MM-DD");;
    }
    getTransferType(data: number){
        if(data == 1){
            return  this.$t('TransferLabor').toString();
        }
        else{
            return  this.$t('ChangeDepartment').toString();
        }
    }
    showHideDialog(isShow: boolean){
        this.showDialog = isShow;
        if(isShow == true){
            this.loadData();
        }
    }
    Delete(index){
        const listDelete: Array<WaitingApproveResult> = [];
        listDelete.push(this.waitingApproveList[index]);
        this.DeleteProcess(listDelete);
        
    }
    DeleteAll(){
        const listDelete: Array<WaitingApproveResult> = [];
        for (let index = 0; index < this.waitingApproveList.length; index++) {
            if(this.waitingApproveList[index].IsChecked==true){
                listDelete.push(this.waitingApproveList[index]);
            }
        }
        this.DeleteProcess(listDelete);
    }
    DeleteProcess(listWaitingApproveDelete: Array<WaitingApproveResult>){
        this.$confirmDelete().then(async () => {
            employeeTransferApi.DeleteApproveEvent(listWaitingApproveDelete).then((res: any)=>{
                if (res.status == 200) {
                    this.$deleteSuccess();

                }
                else{

                }
                this.loadData();
            });
        });
    }
    handleCheckAllChange(data){
        for (let index = 0; index < this.waitingApproveList.length; index++) {
            this.waitingApproveList[index].IsChecked = data;
            
        }
        
    }
    
}