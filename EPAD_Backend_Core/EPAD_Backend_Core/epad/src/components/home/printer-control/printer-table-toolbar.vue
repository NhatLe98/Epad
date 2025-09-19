<template>
    <div class="printer-control__toolbar">
        <SearchField @onSearchEnter="onSearchEnter" />
        <el-button type="text" @click="onEditButtonClick">
            <img src="@/assets/icons/Button/Edit.png" alt="edit" />
        </el-button>
        <el-button type="text" @click="onButtonDeleteClick">
            <img src="@/assets/icons/Button/Delete.png" alt="delete" />
        </el-button>
        <div style="flex: 1" />
        <el-button type="primary" @click="onTestPrinterButtonClick">
            In thử
        </el-button>
        <ButtonCreate :click="onButtonCreateClick" />
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
        <EditablePrinterDialog :showDialog.sync="showEditableDialog" :printer="printerWillUpdate"
            @refreshData="refreshData" />
    </div>
</template>
<script lang="ts">
import { printerApi } from '@/$api/printer-api'
import ButtonCreate from '@/components/app-component/button-create/button-create.vue'
import searchField from '@/components/app-component/search-field/search-field.vue'
import { IC_Printer } from '@/models/ic-printer'
import Vue, { PropType } from 'vue'
import EditablePrinterDialog from './editable-printer-dialog.vue'
export default Vue.extend({
    components: { searchField, ButtonCreate, EditablePrinterDialog, },
    props: {
        selectedPrinters: {
            type: Array as PropType<Array<IC_Printer>>,
            required: true,
        },
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
        const printerWillUpdate: IC_Printer = null;
        return {
            showEditableDialog: false,
            printerWillUpdate,
            showConfigGridPanel: false,
            listColumn: [],
        }
    },
    methods: {
        showDialog() {
            this.showEditableDialog = true;
        },
        refreshData() {
            this.$emit('refreshData');
        },
        onButtonDeleteClick() {
            if (this.selectedPrinters.length < 1) {
                return;
            }

            this.$confirm(`Bạn chắc chắn muốn xóa ${this.selectedPrinters.length} máy in đã chọn?`,
                this.$t('Notify').toString(),
                {
                    confirmButtonText: 'OK',
                    cancelButtonText: 'Cancel',
                    type: 'warning'
                })
                .then(() => {
                    printerApi.deleteManyPrinterAsync(this.selectedPrinters.map(x => x.Index)).then(() => {
                        this.refreshData();
                        this.$deleteSuccess();
                    }).catch((err) => {
                        console.log(err);
                    })
                });
        },
        onButtonCreateClick() {
            this.printerWillUpdate = null;
            this.showDialog();
        },
        onEditButtonClick() {
            if (this.selectedPrinters.length === 1) {
                this.printerWillUpdate = this.selectedPrinters[0];
                this.showDialog();
            } else {
                this.printerWillUpdate = null;
                this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());

            }
        },
        onSearchEnter(value: string) {
            this.$emit('onSearchEnter', value);
        },
        onTestPrinterButtonClick() {
            if (this.selectedPrinters.length < 1) {
                this.$alert("Vui lòng chọn máy in", this.$t('Notify').toString());
                return;
            }

            this.selectedPrinters.forEach(printer => {
                printerApi.printTestPageAsync({ printerName: printer.Name }).then(() => {
                    this.$notify({
                        title: this.$t('Notify').toString(),
                        message: `${printer.Name}: In thành công, vui lòng kiểm tra máy in.`,
                        type: 'success',
                    });
                }).catch(err => {
                    console.log(err);
                })
            })
        },
        openCloseGridConfigPanel() {
            this.showConfigGridPanel = !this.showConfigGridPanel;
        },
        saveListCol() {
            const arrTemp = this.gridColumnConfig.map((col, index) => {
                return {
                    ...(col as any),
                    display: this.listColumn[index].display
                };
            });
            this.$emit('update:gridColumnConfig', arrTemp);
            const path = this.$route.path.substr(1);
            const key = `${path}-config-column`;
        
            localStorage.setItem(key, JSON.stringify(this.listColumn));


            setTimeout(() => {
                this.showConfigGridPanel = false;
            }, 200);
        }
    },
    mounted() {
        const path = this.$route.path.substr(1);
        const key = `${path}-config-column`;
        const configColumnByPath = localStorage.getItem(key);
       
        let listCol = [];
        if (!Misc.isEmpty(configColumnByPath)) {
            listCol = JSON.parse(configColumnByPath);
        }
        if (listCol.length === 0) {
            listCol = this.gridColumnConfig.map(col => {
                return {
                    ID: (col as any).ID || (col as any).prop,
                    ColumnName: (col as any).headerName || this.$t((col as any).field),
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
                display: this.listColumn[index].display
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
        width: 200px;
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