import { Component, Vue, Prop, Model } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';

@Component({
    name: 'select-department',
    components: {},
})
export default class DepartmentSelectComponent extends Vue {
    @Model() value: any;
    @Prop({ default: true }) filterable: boolean;
    @Prop({ default: true }) clearable: boolean;
    @Prop({ default: false }) allowEmpty: boolean;
    @Prop({ default: false }) defaultVal: boolean;
    @Prop({ default: false }) allowSelectAll: boolean;
    @Prop({ default: 'SelectDepartment' }) placeholder: string;
    @Prop({ default: null }) data: any;
    @Prop({ default: false }) reset: boolean;
    @Prop({ default: true }) allowRemember: boolean;

    lang = 'en';
    loadedData = [];

    get dataRules() {
        if (this.reset) {
            this.$emit('value', null);
        }
        return this.data || this.loadedData;
    }

    get isMultiple() {
        const multiple: any = this.$attrs.multiple;
        return true === multiple;
    }

    beforeMount() {
        const locales = localStorage.getItem('lang');
        if (locales != null) {
            this.lang = locales.toString();
        }
    }

    async mounted() {
        if (this.data === null)
            await this.loadCbb();
    }

    async loadCbb() {
        await departmentApi.GetAll()
            .then((rep: any) => {
                const data = rep.data.Value;
                this.loadedData = data.map(e => {
                    return {
                        label: e.label,
                        value: parseInt(e.value)
                    }
                });
            }).catch((error) => console.error(error));
    }

    selectAll() {
        const allData = this.dataRules.map((e) => e.ID);
        this.$emit('value', allData);
    }
}
