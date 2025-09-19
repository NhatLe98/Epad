import { Component, Vue, Mixins, PropSync, Prop, Watch } from "vue-property-decorator";
import ComponentBase from '@/mixins/application/component-mixins';
import { isNullOrUndefined, log } from "util";
import { ElTable } from 'element-ui/types/table';
import { isNumber } from "@/$core/misc";

@Component({
    name: 'data-table-function',
})

export default class DataTableFunctionComponent extends Mixins(ComponentBase) {
    @Prop({ default: () => [] }) columns: Array<any>
    @Prop({ default: () => Promise.resolve([]) }) getData: Function
    @Prop({ default: () => [] }) selectedRows: Array<any>
    @Prop({ default: true }) showCheckbox: Boolean

    @Prop({ default: false }) isFromDeviceInfo: Boolean
    @Prop({ default: true }) showSearch: Boolean
    @Prop({ default: false }) isFromDeviceInfoForHome: Boolean
    @Prop({ default: false }) isFromHasView: Boolean
    @Prop({ default: false }) isFromMachineLicense: Boolean

    @Prop({ default: false }) isHiddenPaging: Boolean
    @Prop({ default: false }) isShowPageSize: Boolean

    @Prop({ default: '' }) filterPlaceHolder: string

    a = 0;
    b = 0;
    arr = [];
    dataTable = [];
    page = 1;
    total = 0;
    sortParams = [];
    selectObj = [];
    loading = false;
    maxHeight = 400;
    keySearch = null;
    filter = "";
    arrChecked = [];
    pageSize = 50;

    get getDataFilter() {
        if (isNullOrUndefined(this.keySearch)) {
            return this.dataTable;
        }
    }

    MaxHeightTable() {
        if (this.isFromHasView === true) {
            this.maxHeight = window.innerHeight - 250;
        }
        else {
            this.maxHeight = window.innerHeight - 175;
        }
    }

    showExtraDriver(row){
        // console.log(row)
        this.$emit("showExtraDriver", row);
    }

    async getTableData(page) {
        if(!this.pageSize && !isNumber(this.pageSize))
        {
            this.$alert(this.$t('PageSizeOnlyAcceptNumber').toString(), this.$t('Notify').toString(), { type: 'warning' });
            return;
        }
        this.page = page;
        let reqPage = page || this.page;
        this.loading = true;
        try {
            let response = await this.getData({
                page: reqPage,
                filter: this.filter,
                sortParams: this.sortParams,
                pageSize: this.pageSize
            });
            this.dataTable = response.data;
            this.total = response.total;
        } finally {
            this.loading = false;
        }
    }

    onSortChange({ column, prop, order }) {
        if (prop !== null) {
            let shortOrder = order === "ascending" ? "asc" : "desc";
            this.sortParams = [`${prop}|${shortOrder}`];
        } else {
            this.sortParams = [];
        }
        this.getTableData(this.page);
    }

    index(index) {
        const offset = ((this.page - 1) * this.pageSize) + 1;
        return index + offset;
    }

    handleSelectionChange(obj) {
        if (obj.length > this.arrChecked.length) {

            for (let i = 0; i < obj.length; i++) {
                if (this.arrChecked.indexOf(obj[i].SerialNumber) === -1) {
                    this.arrChecked.push(obj[i].SerialNumber)
                }
            }
        }
        else if (obj.length < this.arrChecked.length && this.a !== 1) {
            var arrObjSerial = obj.map(item => item.SerialNumber)
            for (let i = 0; i < this.arrChecked.length; i++) {
                if (arrObjSerial.indexOf(this.arrChecked[i]) === -1) {
                    this.arrChecked[i] = -1
                }
            }
            this.arrChecked = this.arrChecked.filter(item => item != -1)
        }
        this.selectObj = obj
        this.$emit("update:selectedRows", this.selectObj);
    }

    tableRowClassName({ row, rowIndex }) {
        if (this.isFromDeviceInfo === true) {
            if (row.Status == "Offline") {
                return 'offline7'
            }
            else if (row.Status == "Online" || row.Status == "Đang xử lý") {
                return 'online7'
            }
            else {
                return ''
            }
        }
        else if (this.isFromDeviceInfoForHome === true) {
            if (row.Status == "Offline") {
                return 'offline'
            }
            else if (row.Status == "Online" || row.Status == "Đang xử lý") {
                return 'online'
            }
            else {
                return ''
            }
        }
        else if (row.IsOverBodyTemperature == true) {
            return "warningrow";
        }
        else if(this.isFromMachineLicense)
        {
            if (row.HardWareLicenseStatus == "NearExpire") {
                return 'near-expire'
            }
            else if (row.HardWareLicenseStatus == "Expired") {
                return 'expired'
            }
            else {
                return '';
            }
        }
        else {
            return ''
        }
    }

    toggleSelection(rows) {
        rows.forEach(row => {
            (this.$refs.multipleTable as ElTable).toggleRowSelection(this.dataTable[row])
        })
    }

    @Watch('dataTable')
    handler(val, oldVal) {
        if (isNullOrUndefined(val)) return;
        let valSerial = val.map(item => item.SerialNumber)
        let oldValSerial = oldVal.map(item => item.SerialNumber)
        var result = true
        for (let i = 0; i < oldValSerial.length; i++) {
            if (valSerial[i] !== oldValSerial[i]) {
                result = false
                return
            }
        }
        if (result === true) {
            this.a = 1
        }
    }

    created() {
        this.getTableData(1);
    }

    getDate(format, value) {
		//this.doLayout();
        return value != null ? moment(value).format(format) : '';
	}
    getLookup(lookup, key) {
		const dummy = lookup.dataSource[key] || {};
		return dummy[lookup.displayMember] || '';
	}
    
    beforeMount() {
        this.MaxHeightTable();
        window.addEventListener("resize", () => {
            this.MaxHeightTable();
        });
    }

    updated() {
        if (this.a === 1 && this.isFromDeviceInfo === true) {
            var arrDataTable = this.dataTable.map(item => item.SerialNumber)
            var arr = []
            for (let i = 0; i < arrDataTable.length; i++) {
                if (this.arrChecked.indexOf(arrDataTable[i]) !== -1) {
                    arr.push(i)
                }
            }
            this.toggleSelection(arr)
            this.a = 0
        }
        // this will prevent damage table interface cause something like reload columns of table
        this.doLayout();
    }

    doLayout() {
        const dlRefs = this.$refs.multipleTable as any;

        if (dlRefs != undefined || dlRefs != '') {
            try {
                dlRefs.doLayout && dlRefs.doLayout();
            } catch {
                console.log('');
            }
        }
    }

    beforeDestroy() {
        window.removeEventListener("resize", () => { })
    }
    onChangePageSize() {
        this.getTableData(1);
    }
};