import { Component, Vue, Prop, Mixins, PropSync, Watch } from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
import ComponentBase from "@/mixins/application/component-mixins";
import { employeeInfoApi } from '@/$api/employee-info-api';
import { departmentApi} from '@/$api/department-api'

@Component({
    name: 'alert'
})
export default class AlertComponent extends Mixins(ComponentBase) {

    @Prop({default: 0}) closeSec: number;
    @Prop({default: ''}) title: string;
    @Prop({default: ''}) message: string;
    @Prop({default: null}) type: boolean;

    @Prop({default: false}) show: boolean;

    @Prop({default: false}) enableHiddenScanQR: boolean;
    hiddenScanQR = null;

    beforeMount() {
        // this.LoadDepartmentTree();
    }

    mounted(){
        
    }

    updated(){
        if(this.show && this.closeSec > 0){
            setTimeout(() => {
                this.Cancel();
            }, (this.closeSec * 1000));
        }
        if(this.show && this.enableHiddenScanQR){
            (this.$refs.hiddenScanQR as any).focus();
        }
    }

    Cancel(){
        this.show = false;
        this.closeSec = 0;
        this.type = null;
        if(this.show && this.enableHiddenScanQR){
            (this.$refs.hiddenScanQR as any).blur();
        }
        this.$emit("Cancel");
    }

    changeScanQR(){
        console.log(this.hiddenScanQR)
        if(this.hiddenScanQR.toString().includes('|') && this.hiddenScanQR.toString().split('|').length >= 7){
            const birthday = this.hiddenScanQR.toString().split('|')[3];
            const issuanceDay = this.hiddenScanQR.toString().split('|')[6];
            // console.log(birthday, issuanceDay)
            if(this.isValidDate(birthday) && this.isValidDate(issuanceDay)){
                this.$emit('ScanQR', this.hiddenScanQR.toString(), true);
            }else{
                this.$emit('ScanQR', this.hiddenScanQR.toString(), false);
            }
        }else{
            this.$emit('ScanQR', this.hiddenScanQR.toString(), false);
        }
        this.hiddenScanQR = null;
    }

    isValidDate(dateString) {
        // Check if the string matches the DDMMYYYY format
        if (!/^\d{8}$/.test(dateString)) {
            return false;
        }

        // Extract day, month, and year
        const day = parseInt(dateString.slice(0, 2), 10);
        const month = parseInt(dateString.slice(2, 4), 10) - 1; // JS months are 0-indexed
        const year = parseInt(dateString.slice(4, 8), 10);

        // Create a date object and check if it's valid
        const date = new Date(year, month, day);

        // Check if the date is valid and the components match the input
        return date.getFullYear() === year &&
            date.getMonth() === month &&
            date.getDate() === day &&
            month >= 0 && month <= 11 && // Ensure month is between 0 and 11
            day > 0; // Ensure day is greater than 0
    }

    @Watch('closeSec')
    setCloseDialog(){
        if(this.show && this.closeSec > 0){
            setTimeout(() => {
                this.Cancel();
            }, (this.closeSec * 1000));
        }
    }
}
