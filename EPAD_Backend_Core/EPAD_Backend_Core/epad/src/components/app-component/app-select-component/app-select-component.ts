import { Component, Vue, Prop, Model, Watch } from 'vue-property-decorator';

@Component({
	name: 'app-select-component',
	components: { },
})
export default class AppSelectComponent extends Vue {
    @Model() value: any;
    @Prop({ default: () => [] }) dataSource: any;
    @Prop({ default: () => [] }) dataDisabled: any;
    @Prop({ default: 'Name' }) displayMember: string;
    @Prop({ default: 'Index' }) valueMember: string;
    @Prop({ default: '' }) label: string;
    @Prop({ default: '' }) placeholder: string;
    @Prop({ default: true }) allowNull: boolean;
    @Prop({ default: true }) filterable: boolean;
    @Prop({ default: false }) clearable: boolean;
    @Prop({ default: false }) disabled: boolean;

    get isMultiple() {
        const multiple: any = this.$attrs.multiple;
        return multiple != null || multiple != undefined;
    }

    get isCollapseTags() {
        const collapseTags: any = this.$attrs.collapseTags;
        return collapseTags != null || collapseTags != undefined;
    }

    keys: any = {}

    beforeMount(){
        this.keys = Object.assign(
            {},
            this.keys,
            { label: this.displayMember, value: this.valueMember }
        );
    }
    checkDisabled(value){
        const item = this.dataDisabled.find(item => item == value);
        if (item != null) {
            return true;
        }else{
            return false;
        }
    }
}
