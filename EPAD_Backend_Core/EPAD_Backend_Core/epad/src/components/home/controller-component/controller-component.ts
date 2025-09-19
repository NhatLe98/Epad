import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import { serviceApi, IC_Service } from "@/$api/service-api";
import { groupDeviceApi, IC_GroupDevice } from "@/$api/group-device-api";
import { controllerrApi, Controller } from "@/$api/controlle-api";

import { commandApi } from "@/$api/command-api";
import { Form as ElForm } from 'element-ui'
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from "@/components/home/data-table-component/data-table-function-component.vue";

@Component({
    name: "machine",
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent }
})
export default class ListGroupDeviceComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    showMessage = false;
    checked = false;
    columns = [];
    rowsObj = [];
    isEdit = false;
    listExcelFunction = [];
    ruleForm: Controller = {
        Index: null,
        Name: "",
        IPAddress: "",
        Port: "",
        IDController: ""
    };
    rules: any = {};

    beforeMount() {
        this.initColumns();
        this.initRule();
    }

    initRule() {
        this.rules = {
            Name: [
                {
                    required: true,
                    message: this.$t('PleaseInputName'),
                    trigger: "blur"
                }
            ],
            IDController: [
                {
                    required: true,
                    message: this.$t('PleaseInputId'),
                    trigger: "blur"
                }
            ],

            IPAddress: [
                {
                    required: true,
                    message: this.$t('IpRequired'),
                    trigger: "blur"
                }
            ],
            Port: [
                {
                    required: true,
                    message: this.$t('PortIsRequired'),
                    trigger: "blur"
                },
                {
                    message: this.$t('PortOnlyAcceptNumericCharacters'),
                    validator: (rule, value: string, callback) => {
                        if (/^\d+$/.test(value) === false && isNullOrUndefined(value) === false && value !== "") {
                          callback(new Error());
                        }
                        callback();
                      }
                }
            ],
            
        };
    }
    mounted() {
        
    }

    initColumns(){
        this.columns = [
            {
                prop: "Name",
                label: "ControllerName",
                minWidth: 200,
                display: true
            },
            {
                prop: "IDController",
                label: "IDController",
                minWidth: 200,
                display: true
            },
            {
                prop: "IPAddress",
                label: "IPAddress",
                minWidth: 200,
                display: true
            },
            {
                prop: "Port",
                label: "Port",
                minWidth: 200,
                display: true
            }
        ];
    }

    displayPopupInsert() {
        this.showDialog = false;
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.page = page
        return await controllerrApi.GetController(page, filter, pageSize).then(res => {
            const { data } = res as any;
            return {
                data: data.data,
                total: data.total
            };
        });
    }

    reset() {
        const obj: Controller = {
            Index: null,
            Name: "",
            IPAddress: "",
            Port: "",
            IDController: ""
        };
        this.ruleForm = obj;
    }

    Insert() {
        this.showDialog = true;
        this.isEdit = false;
        this.reset();
    }

    async Submit() {
        (this.$refs.ruleForm as any).validate(async valid => {
            if (!valid) return;
            else {
                if (this.isEdit == true) {
                    this.ruleForm.Index = this.rowsObj[0].Index;
                    return await controllerrApi
                        .UpdateController(this.ruleForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                            }
                        })
                }
                else if(this.isEdit == false) {
                    
                    this.ruleForm.Index = 0;
                    return await controllerrApi
                        .AddController(this.ruleForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.reset();
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$saveSuccess();
                            }
                        })
                        .catch(() => {

                        })
                }
            }
        });
    }

    Edit() {
        this.isEdit = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));

        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t("MSG_SelectOnlyOneRow").toString());
        } else if (obj.length === 1) {
            this.showDialog = true;
            this.ruleForm = obj[0];
        }
        else {
            this.$alertSaveError(null, null, null, this.$t("ChooseUpdate").toString());
        }
    }
    Restart() {
        const obj: IC_GroupDevice[] = JSON.parse(JSON.stringify(this.rowsObj));

        const arrId: Array<Number> = new Array<Number>();
        obj.forEach(element => {
            arrId.push(element.Index);
        });
        this.showMessage = true;

        setTimeout(() => {
            this.showMessage = false;
        }, 2000);

    }

    async Delete() {
        const obj: IC_GroupDevice[] = JSON.parse(JSON.stringify(this.rowsObj));
        console.log(obj);
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t("ChooseRowForDelete").toString());
        }
        else {
            this.$confirmDelete()
                .then(async () => {
                    await controllerrApi.DeleteController(this.rowsObj.map(t=>t.Index).join(','))
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            if (!isNullOrUndefined(res.status) && res.status === 200) {
                                this.$deleteSuccess();
                            }
                        })
                        .catch(() => {

                        })
                })
        }
    }

    focus(x) {
        var theField = eval('this.$refs.' + x)
        theField.focus()
    }

    Cancel() {
        var ref = <ElForm>this.$refs.ruleForm
        ref.resetFields()
        this.showDialog = false
    }
}

