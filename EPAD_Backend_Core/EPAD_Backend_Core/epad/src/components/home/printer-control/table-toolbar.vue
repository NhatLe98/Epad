<template>
    <div class="printer-control__toolbar">
        <el-button @click="openCloseGridConfigPanel" v-if="showButtonColumConfig">
            <img v-show="showConfigGridPanel === false" src="@/assets/icons/function-bar/adjust.svg" style="width: 18px; height: 18px" />
            <img class="no-filter" v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/cross.svg" style="width: 18px; height: 18px" />
        </el-button>
        <div class="popupshowcol" v-show="showConfigGridPanel">
            <div class="switch-wapper">
                <div class="col-item" v-for="col in listColumn" :key="col.ID">
                    <el-switch v-if="col.ColumnName != 'index' && col.ColumnName != '#'" 
                    :disabled="col.Fixed" v-model="col.display" :active-text="col.ColumnName">

                    </el-switch>
                </div>
            </div>
            <el-button @click="saveListCol" style="margin-right: 10px; margin-top: 10px">
                <img v-show="showConfigGridPanel === true" src="@/assets/icons/function-bar/foursquare-check-in.svg" style="width: 18px; height: 18px" />
            </el-button>
        </div>
    </div>
</template>
<script lang="ts">
import Vue, { PropType } from 'vue'
export default Vue.extend({
    components: {  },
    props: {
        gridColumnConfig: {
            type: Array,
            required: false,
        },
        showButtonColumConfig: {
            type: Boolean,
            required: false,
            default: true
        },
    },
    data() {
        return {
            showConfigGridPanel: false,
            listColumn: [],
            customConfigPathName: "",
        }
    },
    methods: {
        reloadConfig(name){
            // console.log("reload", name)
            this.customConfigPathName = name;
            let path = this.$route.path.substr(1);
            if(this.customConfigPathName && this.customConfigPathName != ""){
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
                        ID: (col as any).ID || (col as any).prop || (col as any).dataField,
                        ColumnName: (col as any).headerName || this.$t((col as any).field) || this.$t((col as any).ColumnName) 
                            || this.$t((col as any).label) || this.$t((col as any).name),
                        display: true,
                        Fixed: (col as any).fixed || false
                    }
                });
                localStorage.setItem(key, JSON.stringify(listCol));
            }
            this.listColumn = listCol;
            // nạp lại config từ localStorage lên UI
            const arrTemp = this.gridColumnConfig.map((col, index) => {
                return {
                    ...(col as any),
                    display: this.listColumn[index].display,
                    show: this.listColumn[index].show,
                };
            });
            this.$emit('update:gridColumnConfig', arrTemp);
        },
        openCloseGridConfigPanel() {
            this.showConfigGridPanel = !this.showConfigGridPanel;
        },
        saveListCol() {
            const arrTemp = this.gridColumnConfig.map((col, index) => {
                return {
                    ...(col as any),
                    display: this.listColumn[index].display,
                    show: this.listColumn[index].show,
                };
            });
            this.$emit('update:gridColumnConfig', arrTemp);
            let path = this.$route.path.substr(1);
            if(this.customConfigPathName && this.customConfigPathName != ""){
                path = path + "-" + this.customConfigPathName;
            }

            const key = `${path}-config-column`;
        
            localStorage.setItem(key, JSON.stringify(this.listColumn));


            setTimeout(() => {
                this.showConfigGridPanel = false;
            }, 200);
        }
    },
    mounted() {
        let path = this.$route.path.substr(1);
        if(this.customConfigPathName && this.customConfigPathName != ""){
            path = path + "-" + this.customConfigPathName;
        }
        const key = `${path}-config-column`;
        const configColumnByPath = localStorage.getItem(key);
       
        let listCol = [];
        if (!Misc.isEmpty(configColumnByPath)) {
            listCol = JSON.parse(configColumnByPath);
        }
        if (listCol.length === 0) {
            listCol = this.gridColumnConfig.map(col => {
                return {
                    ID: (col as any).ID || (col as any).prop || (col as any).dataField,
                    ColumnName: (col as any).headerName || this.$t((col as any).field) || this.$t((col as any).ColumnName) 
                        || this.$t((col as any).label) || this.$t((col as any).name),
                    display: true,
                    Fixed: (col as any).fixed || false
                }
            });
            localStorage.setItem(key, JSON.stringify(listCol));
        }
        this.listColumn = listCol;
        // nạp lại config từ localStorage lên UI
        const arrTemp = this.gridColumnConfig.map((col, index) => {
            return {
                ...(col as any),
                display: this.listColumn[index].display,
                show: this.listColumn[index].show,
            };
        });
        this.$emit('update:gridColumnConfig', arrTemp);
    }
})
</script>
<style lang="scss" scoped>
.printer-control__toolbar {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.el-button--text {
    margin-right: 0;
}
.popupshowcol {
        z-index: 10;
        position: absolute;
        top: 40px;
        right: 5px;
        height: fit-content;
        width: fit-content;
        background-color: whitesmoke;
        border-radius: 10px;
        border: 0.5px solid #bdbdbd;
        box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
        padding: 10px 5px 10px 10px;
        display: flex;
        flex-direction: column;
        align-items: flex-end;

        .switch-wapper {
            height: fit-content;
            max-height: 40vh;
            overflow: auto;
            width: 100%;
        }
    }
</style>