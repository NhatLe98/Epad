import { Component, Model, Prop, Vue, Watch } from "vue-property-decorator";
import { Getter } from 'vuex-class';

@Component
export default class TabBase extends Vue {
    @Getter('employeeLookup', { namespace: 'HumanResource' }) employeeLookup: any[];
    @Getter('departmentLookup', { namespace: 'HumanResource' }) departmentLookup: any[];
    @Getter('positionLookup', { namespace: 'HumanResource' }) positionLookup: any[];
    @Model() configModel;
    tabConfig = [];
    listDepartment = [];
    selectedDepartment = [];
    gridColumns: Array<IColumnConfig> = [];
    dataSource = [];
    selectedRows = [];
    showDialog = false;
    isEdit = false;

    total = 0;
    page = 1;
    pageSize = 20;
    userType = 1;

    formModel: any = {}
    formRules = {}

    showDialogDeleteUser = false;
    isDeleteOnDevice = false;

    async beforeMount() {
        await this.initLookup();
        this.initGridColumns();
        this.initFormModel();
        this.initFormRules();
        this.initCustomize();
    }

    mounted() {
        this.loadData();
    }

    async initLookup() {
    }

    initGridColumns() {
    }

    initFormModel() {
    }

    initFormRules() {
    }

    async initCustomize() {
    }

    loadData() {
    }


    onViewClick() {
        console.log(`tab view click`)
    }

    onPageChange(page) {
        this.page = page;
        this.loadData();
    }

    onPageSizeChange(pageSize) {
        this.pageSize = pageSize;
        this.loadData();
    }

    onInsertClick() {
        this.formModel = {};
        this.isEdit = false;
        this.showDialog = true;
    }

    onEditClick() {
        this.formModel = this.selectedRows[0];
        this.isEdit = true;
        this.showDialog = true;
    }

    onDeleteClick() {
        this.showDialogDeleteUser = true;
    }

    onSubmitClick() {

    }

    onCancelClick() {
        this.showDialog = false;
        this.selectedRows = [];
        this.loadData();
    }

    cancelDeleteUser() {
        this.showDialogDeleteUser = false;
    }

    @Watch('selectedRows')
    selectedRowsChange(value) {
        this.$emit('selectedRowKeys', value);
    }
}
