import { Component, Vue, Mixins } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TGrid from '@/components/app-component/t-grid/t-grid.vue';
import TabBase from '@/mixins/application/tab-mixins';
import { hrNannyInfoApi } from '@/$api/hr-nanny-info-api';

@Component({
    name: 'hr-nanny-info',
    components: { 't-grid': TGrid },
})
export default class HRNannyInfo extends Mixins(TabBase) {

    async initLookup() {
        await this.getDepartment();
    }
    
    initGridColumns() {
        this.gridColumns = [
            {
                name: 'Avatar',
                dataField: 'Avatar',
                dataType: 'image',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'MCC',
                dataField: 'EmployeeATID',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'EmployeeCode',
                dataField: 'EmployeeCode',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'Employee',
                dataField: 'EmployeeATID',
                dataType: 'lookup',
                fixed: true,
                width: 150,
                lookup: {
                    dataSource: this.employeeLookup,
                    displayMember: 'FullName',
                    valueMember: 'EmployeeATID',
                },
                show: true,
            },
            {
                name: 'Gender',
                dataField: 'Gender',
                dataType: 'lookup',
                fixed: false,
                width: 150,
                show: true,
                lookup: {
                    dataSource: {
                        1: { Index: 1, Name: 'Nam', NameInEng: 'Male' },
                        2: { Index: 2, Name: 'Nữ', NameInEng: 'Female' },
                    },
                    displayMember: 'Name',
                    valueMember: 'Index',
                }
            },
            {
                name: 'BirthDay',
                dataField: 'BirthDay',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'Email',
                dataField: 'Email',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'Phone',
                dataField: 'Phone',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'NameOnMachine',
                dataField: 'NameOnMachine',
                fixed: false,
                width: 150,
                show: true,
            },
        ];
    }

    async loadData() {
        this.dataSource = [];
        await hrNannyInfoApi.GetAllNanny().then(response => {
            const data = response.data;
            this.dataSource = data;
        })
    }

    async getDepartment() {
        return await departmentApi.GetAll().then((res) => {
            const { data } = res as any;
            let arr = JSON.parse(JSON.stringify(data));
            for (let i = 0; i < arr.Value.length; i++) {
                arr.Value[i].value = parseInt(arr.Value[i].value);
            }
            this.listDepartment = arr.Value;
        });
    }

    async onViewClick() {
        //  this.configModel.filterModel = this.filterModel;
        this.$emit('filterModel', this.configModel);
        await this.loadData();
    }
}
