import { isArray } from 'lodash';
import { Component, Vue, Prop, Model, Watch } from 'vue-property-decorator';

@Component({
	name: 'app-select-new-component',
	components: { }
})
export default class AppSelectNewComponent extends Vue {
    @Model() value: Array<any>;
    @Prop({ default: () => [] }) dataSource: any;
    @Prop({ default: 'Name' }) displayMember: string;
    @Prop({ default: 'Id' }) valueMember: string;
    @Prop({ default: '' }) label: string;
    @Prop({ default: '' }) placeholder: string;
    @Prop({ default: true }) allowNull: boolean;
    @Prop({ default: true }) filterable: boolean;
    @Prop({ default: false }) disabled: boolean;
	isShowBtnSelectedAll = true;
    // isShowBtnSelect = false;
    employeeList = [];

    changeVisible(visible){
        this.$emit('changeVisible', visible);
    }
    
    onSelectChange(object) {
        // console.log(object);
        this.$emit("onChange", object);
        this.$emit('getValueSelectedAll', object);
        // console.log("object", object);
        // console.log("dataSource", this.dataSource);
		if(isArray(this.dataSource) && object.length < this.dataSource.length) {
			this.isShowBtnSelectedAll = true;
		}
        else if(!isArray(this.dataSource) && object.length < Object.entries(this.dataSource).length){
            // console.log(Object.entries(this.dataSource))
            this.isShowBtnSelectedAll = true;
        }
		else {
			this.isShowBtnSelectedAll = false;
		}
    }

    get isMultiple() {
        const multiple: any = this.$attrs.multiple;
        return true === multiple;
    }

    get isCollapseTags() {
        const multiple: any = this.$attrs['collapse-tags'];
        return true === multiple;
    }
    
    selectAll() {
        if(this.value && this.value.length > 0) {
            while(this.value.length) {
                this.value.pop();
            }
        }

        if(isArray(this.dataSource)) {
            this.dataSource.forEach((obj, Id, array) => {
                if(
                    obj?.Id &&
                    obj?.IdentityNumber && 
                    obj?.CardNumber
                ) {
                    this.value.push(obj?.IdentityNumber)
                }
                else if(obj?.Id) {
                    this.value.push(obj?.Id);
                    // this.$emit('getValueSelectedAll', this?.value);
                }
                else if(obj?.Index) {
                    this.value.push(obj?.Index);
                    // this.$emit('getValueSelectedAll', this?.value);
                }
                else if(obj?.value) {
                    this.value.push(obj?.value);
                    // this.$emit('getValueSelectedAll', this?.value);
                }
                else if(obj?.IdentityNumber) {
                    this.value.push(obj?.IdentityNumber);
                    // this.$emit('getValueSelectedAll', this?.value);
                }
                else if(obj?.CardNumber) {
                    this.value.push(obj?.CardNumber);
                    // this.$emit('getValueSelectedAll', this?.value);
                }
                else if(obj?.CardName) {
                    this.value.push(obj?.CardName)
                }
            })
            this.$emit('getValueSelectedAll', this?.value);
            this.isShowBtnSelectedAll = false;
        }
        if (!isArray(this.dataSource)) {
            const emps = Object.keys(this.dataSource).map((key) => this.dataSource[key]);
            this.value = emps.map(x => x.Index);
            this.$emit('getValueSelectedAll', this?.value);
            this.isShowBtnSelectedAll = false;
        }
        this.$emit("onChange", this.value);
    }

    DeselectAll() {
        if(this.value){
            while(this.value.length) {
                this.value.pop();
            }
        }
        this.$emit('getValueSelectedAll', []);
		this.isShowBtnSelectedAll = true;
	}

    keys: any = {}
    beforeMount(){
        this.keys = Object.assign(
            {},
            this.keys,
            { label: this.displayMember, value: this.valueMember }
        );
    }
}
