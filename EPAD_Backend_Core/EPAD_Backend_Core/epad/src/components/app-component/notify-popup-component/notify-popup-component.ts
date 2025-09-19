//import { Component, Vue, Prop, Mixins, PropSync } from "vue-property-decorator";
import { Component, Vue, Mixins, Watch, Prop} from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
import ComponentBase from "@/mixins/application/component-mixins";
import { userNotificationApi, UserNotification ,MessageBody, AddParam } from '@/$api/user-notification-api'


@Component({
    name: 'notify-popup',
    components: { },
})
export default class NotifyPopupComponent extends Mixins(ComponentBase) {
    @Prop({ default: '' }) title: string;
    // Default Functions
   
    mounted() {
        this.loadData();
    }

    created() {
        //this.loadData();
    }
    // User define available
    loading = false;
    showDialog = false;
    listNotification: Array<UserNotification> = [];
    addedParam: Array<AddParam> = [];


    // User define functions
    loadData() {
        this.loading = true;
        this.listNotification = [];
        //this.addedParam.push({Key:"Test", Value:"Test"})
        const params = JSON.parse(JSON.stringify(this.addedParam));
        userNotificationApi.PostMany(params).then((res: any) => {
            this.loading = false;
            if (res.status == 200) {
               
                this.listNotification = res.data;

                if (this.listNotification.length > 0) {
                    let message;
                    this.listNotification.forEach(item => {
                        message = "";
                        switch (item.Type) {
                            case 0:
                                message = this.$t('DialogNotifySubmit');
                                break;
                            case 1:
                                message = this.$t('DialogNotifyApprove');
                                break;
                            case 2:
                                message = this.$t('DialogNotifyReject');
                                break;
                            default:
                                message = this.$t('DialogNotifyOverDate');
                                break;
                        }
                        item.RouteURL = item.Type == 0 ? "/approve-change-department" : "/change-department";// 0 is submit transfer employee , !=0 is approve ỏ reject or orver approve date
                        item.Data.FormatMessage = message + ' - ' + item.Data.Message + ' ' + this.$t('Employee');
                    });

                }
               
            }
            else {
                this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
            }
            this.$emit('getNumberOfNotifyComponent', this.listNotification.length > 0 ? this.listNotification.length : "");
        }).catch((err) => {
            this.loading = false;
            this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
        });
        this.$emit('getNumberOfNotifyComponent', this.listNotification.length > 0 ? this.listNotification.length : "");
    }

    showHideDialog(isShow: boolean) {
        if (isShow == true && this.listNotification.length < 1)
            return;
        this.showDialog = isShow;
    }

    deleteNotification(index) {
        if (this.listNotification.length > 0) {
            const deleteItem: UserNotification = JSON.parse(
                JSON.stringify(this.listNotification[index])
            );
            this.$confirmDelete().then(async () => {
                userNotificationApi.Delete(deleteItem).then((res: any) => {
                    if (res.status == 200) {
                        this.$deleteSuccess();
                    }
                    else {
                        this.$alertSaveError(null, null, null, this.$t('MSG_SystemError').toString());
                    }
                    this.loadData();
                });
            });
        }
    }


    DeleteAll() {
        //const listDelete: Array<UserNotification> = [];
        //for (let index = 0; index < this.listNotification.length; index++) {
        //    if (this.listNotification[index].IsChecked == true) {
        //        listDelete.push(this.listNotification[index]);
        //    }
        //}
        this.DeleteProcess();
    }
    DeleteProcess() {
        this.$confirmDelete().then(async () => {
            userNotificationApi.DeleteAll(this.listNotification).then((res: any) => {
                if (res.status == 200) {
                    this.$deleteSuccess();
                    this.listNotification = [];
                    this.$emit('getNumberOfNotifyComponent', this.listNotification.length > 0 ? this.listNotification.length : "");
                }
                else {

                }
                this.showDialog = false;
            });
        });
    }

    handleCheckAllChange(data) {
        for (let index = 0; index < this.listNotification.length; index++) {
            this.listNotification[index].IsChecked = data;
        }
    }
    
    formatDateField(date) {
        if (date == null || date == "") {
            return "";
        }
        return moment(date).format("YYYY-MM-DD");;
    }
}