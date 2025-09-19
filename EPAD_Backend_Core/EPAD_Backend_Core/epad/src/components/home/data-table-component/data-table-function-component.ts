import { Component, Vue, Mixins, PropSync, Prop, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { Getter } from 'vuex-class';
import { isNullOrUndefined, log } from "util";

@Component({
    name: "data-table"
})
export default class DataTableComponent extends Mixins(ComponentBase) {
    @Getter('getMenuIsCollapse', { namespace: 'Application' }) $menuIsCollapse;
    @Prop({ default: () => [] }) listExcelFunction: Array<string>
    @Prop({ default: false }) has1MoreBtn: Boolean
    @Prop({ default: false }) isHiddenEdit: Boolean
    @Prop({ default: false }) isHiddenDelete: Boolean
    @Prop({ default: false }) isHiddenSearch: Boolean
    @Prop({ default: false }) showButtonColumConfig: Boolean
    @Prop({ default: true }) showButtonInsert: Boolean
    @Prop({ default: false }) showButtonIntegrate: Boolean
    @Prop({ default: false }) showButtonSave: Boolean

    @Prop({ default: false }) showButtonCustom: Boolean
    @Prop({ default: "" }) buttonCustomText: string
    @Prop({ default: "" }) buttonCustomIcon: string
    @Prop({ default: () => [] }) gridColumnConfig: Array<any>
    showConfigGridPanel = false;
    listColumn: any = [];
    customConfigPathName = "";

    // @Watch("gridColumnConfig")
    reloadConfig(name) {
        // console.log("reload", name)
        this.customConfigPathName = name;
        let path = this.$route.path.substr(1);
        if (this.customConfigPathName && this.customConfigPathName != "") {
            path = path + "-" + this.customConfigPathName;
        }
        const key = `${path}-config-column`;
        const configColumnByPath = localStorage.getItem(key);

        let listCol = [];
        if (!Misc.isEmpty(configColumnByPath)) {
            listCol = JSON.parse(configColumnByPath);
            // console.log(listCol)
        }
        if (listCol.length === 0) {
            listCol = this.gridColumnConfig.map(col => {
                return {
                    ID: col.ID || col.prop || col.dataField,
                    ColumnName: col.ColumnName || this.$t(col.label) || this.$t(col.name),
                    display: true,
                    Fixed: col.fixed || false
                }
            });
            localStorage.setItem(key, JSON.stringify(listCol));
        }
        this.listColumn = listCol;
        // nạp lại config từ localStorage lên UI
        const arrTemp = this.gridColumnConfig.map((col, index) => {
            return {
                ...col,
                display: this.listColumn[index].display,
                show: this.listColumn[index].show,
            };
        });
        this.$emit('update:gridColumnConfig', arrTemp);
    }

    reloadDefaultConfig() {
        // console.log("reload default")
        let path = this.$route.path.substr(1);
        const key = `${path}-config-column`;

        let listCol = [];
        listCol = this.gridColumnConfig.map(col => {
            return {
                ID: col.ID || col.prop || col.dataField,
                ColumnName: col.ColumnName || this.$t(col.label) || this.$t(col.name),
                display: true,
                Fixed: col.fixed || false
            }
        });
        localStorage.setItem(key, JSON.stringify(listCol));
        this.listColumn = listCol;
        // nạp lại config từ localStorage lên UI
        const arrTemp = this.gridColumnConfig.map((col, index) => {
            return {
                ...col,
                display: this.listColumn[index].display,
                show: this.listColumn[index].show,
            };
        });
        this.$emit('update:gridColumnConfig', arrTemp);
    }

    mounted() {
        let path = this.$route.path.substr(1);
        if (this.customConfigPathName && this.customConfigPathName != "") {
            path = path + "-" + this.customConfigPathName;
        }
        const key = `${path}-config-column`;
        const configColumnByPath = localStorage.getItem(key);

        let listCol = [];
        if (!Misc.isEmpty(configColumnByPath)) {
            listCol = JSON.parse(configColumnByPath);
        }
        if (listCol.length === 0 || listCol.length != this.gridColumnConfig.length) {
            listCol = this.gridColumnConfig.map(col => {
                return {
                    ID: col.ID || col.prop || col.dataField || col.field,
                    ColumnName: col.ColumnName || this.$t(col.label) || this.$t(col.name) || this.$t(col.headerName),
                    display: true,
                    Fixed: col.fixed || false
                }
            });
            localStorage.setItem(key, JSON.stringify(listCol));
        }
        this.listColumn = listCol;
        // nạp lại config từ localStorage lên UI
        const arrTemp = this.gridColumnConfig.map((col, index) => {
            return {
                ...col,
                display: this.listColumn[index].display,
                show: this.listColumn[index].show,
            };
        });
        this.$emit('update:gridColumnConfig', arrTemp);
    }

    openCloseGridConfigPanel() {
        this.showConfigGridPanel = !this.showConfigGridPanel;
    }

    saveListCol() {
        console.table(this.gridColumnConfig);
        console.table(this.listColumn);
        const arrTemp = this.gridColumnConfig.map((col, index) => {
            return {
                ...col,
                display: this.listColumn[index]?.display ?? false,
                show: this.listColumn[index]?.show ?? false,
            };
        });
        this.$emit('update:gridColumnConfig', arrTemp);
        let path = this.$route.path.substr(1);
        if (this.customConfigPathName && this.customConfigPathName != "") {
            path = path + "-" + this.customConfigPathName;
        }
        const key = `${path}-config-column`;

        localStorage.setItem(key, JSON.stringify(this.listColumn));


        setTimeout(() => {
            this.showConfigGridPanel = false;
        }, 200);
    }

    get getSrcForGear() {
        return this.showConfigGridPanel === false ? '@/assets/icons/function-bar/gear.svg' : '@/assets/icons/function-bar/cross.svg';
    }
    Integrate() {
        this.$emit('integrate')
    }
    Insert() {
        this.$emit('insert')
    }
    Edit() {
        this.$emit('edit')
    }
    Delete() {
        this.$emit('delete')
        // this.$confirmDelete().then(async () => {
        // });
    }
    SaveBtn() {
        this.$emit('savebtn')
    }
    CustomButtonClick() {
        this.$emit('custombuttonclick')
    }
    Restart() {
        this.$emit('restart')
    }
    handleCommand(command) {
        if (command === 'AddExcel') {
            this.$emit('add-excel')
        }
        else if (command === 'ExportExcel') {
            this.$emit('export-excel')
        }
        else if (command === 'DeleteExcel') {
            this.$emit('delete-excel')
        }
        else if (command === 'UpdateLicense') {
            this.$emit('upload-hardware-license');
        }
        else if (command === 'CheckImageFromCamera') {
            this.$emit('check-image');
        }
        else if (command === 'AutoSelectExcel') {
            this.$emit('auto-select-excel');
        }
    }

    get getHasMore() {
        if (!isNullOrUndefined(this.listExcelFunction) && this.listExcelFunction.length > 0) {
            return true
        }
        else {
            return false
        }
    }
};