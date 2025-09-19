import { Component, Model, Prop, Vue, Watch } from 'vue-property-decorator';

@Component({
  name: 't-button-bar',
})
export default class TButtonBar extends Vue {
    @Prop( { default: () => { hideAddButton: true }, required: true }) model: ITab;

    get showInsert() {
        if(Misc.isEmpty(this.model.showAdd)) this.model.showAdd = true;
        return true === this.model.showAdd;
    }

    get showEdit() {
        return true === this.model.showEdit && this.model.selectedRowKeys.length === 1;
    }

    get showDelete() {
        return true === this.model.showDelete && this.model.selectedRowKeys.length > 0;
    }

    get showMore() {
        return true === this.model.showMore && this.moreFunctions.length > 0;
    }

    get moreFunctions() {
        return this.model.moreFunctions || [];
    }

    get rowSelected() {
        return this.model.selectedRowKeys;
    }

    get showIntegrate(){
        return true === this.model.showIntegrate;
    }

    get showReadGoogleSheet(){
        return true === this.model.showReadGoogleSheet;
    }

    handleCommand(command) {
        this.$emit('onCommand', command.cmd)
    }
}
