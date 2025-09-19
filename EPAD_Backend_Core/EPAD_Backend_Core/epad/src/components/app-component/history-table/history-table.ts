import { Vue, Prop, Component } from 'vue-property-decorator';
import { Getter } from 'vuex-class';
import * as mime from 'mime-types';

@Component({
    name: 'history-table',
    components: {}
})
export default class HistoryTable extends Vue {
    @Prop({ default: () => [] }) columns: any[];
    @Prop({ default: () => [] }) data: any[];
    @Prop({ default: 500 }) tableHeight: any;
    @Prop({ default: false }) fullScreen: boolean;
    
    dialogViewVisible = false;
    srcImage="";
    
    get visibleColumns() {
        return this.columns.filter(col => col.Show);
    }

    getStatus(value) {
        return value ? this.$t('Success') : this.$t('Fail');
    }
    getInOutData(value) {
        return value==1 ? this.$t('In') : this.$t('Out');
    }
    getLookup(lookup, key) {
        const dummy = lookup.DataSource[key] || {};
        return dummy[lookup.DisplayMember] || '';
    }

    getDate(format, value) {
        const fmt: string = this.$i18n.locale === 'vi'
            ? (format || 'dd-MM-yyyy').replace('MM-dd', 'dd-MM')
            : (format || 'MM-dd-yyyy').replace('dd-MM', 'MM-dd');
        return value != null ? moment(value).format(fmt) : '';
    }

    doLayout() {
        (this.$refs['history-table'] as any).doLayout();
    }

    async viewImage(data: string) {
        this.srcImage = "";
        this.dialogViewVisible = true;
        if (!Misc.isEmpty(data)) {
            if(data.includes("data:image/jpeg;base64,")){
                this.srcImage = data;
            }else{
                this.srcImage = 'data:image/jpeg;base64,' + data;
            }
            //console.log(this.srcImage)

            // if(data.startsWith('[')) {
            //     try {
            //         const ezFilesUser: EzFile[] = JSON.parse(data);
            //         const img = ezFilesUser.map(img => ({ name: img.Name, url: img.Url } as EzFileImage));
            //         if (img.length > 0) {
            //             this.srcImage = img[0].url;
            //             await ezPortalFileApi.GetFilePath({ Name: img[0].name, Url: img[0].url }).then((done: any) => {
            //                 const bytes = done.data.Data;
            //                 const url = 'data:' + mime.lookup(name) + ';base64,' + bytes;
            //                 this.srcImage = url;
            //             });
            //         }
            //     } catch (error) {
            //         this.srcImage = 'data:image/png;base64,' + data
            //     }
            // }
            // else if(data.startsWith('data:')){
            //     this.srcImage = data;
            // }
            // else {
            //     this.srcImage = 'data:image/png;base64,' + data
            // }
        }
    }

    tableRowClassName({row, rowIndex}) {
        if (row.Success === true) {
            return 'success-row';
        }
        else {
            return 'warning-row';
        } 
        
    }

    getColumnClassName(state) {
        let className = '';
        if (state === false) className += 'hid';

        if (this.fullScreen) {
            className += ' fs-fz-active'
        }
        else {
            className += ' fs-fz-deactive'
        }
        return className;
    }
}