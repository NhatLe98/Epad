import { Component, Vue, Mixins, Watch } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { trackingSystemLogApi } from "@/$api/tracking-system-log-api";
import moment from "moment";
import { attendanceLogApi } from "@/$api/attendance-log-api";
import { areaApi } from "@/$api/ac-area-api";
import { doorApi } from "@/$api/ac-door-api";
import SelectDepartmentTreeComponent from '@/components/app-component/select-department-tree-component/select-department-tree-component.vue';
import { departmentApi } from "@/$api/department-api";
@Component({
    name: "log-history",
    components: { HeaderComponent, DataTableComponent, SelectDepartmentTreeComponent, DataTableFunctionComponent }
})
export default class LogHistoryComponent extends Vue {
    columns = [];
    fromTime = moment(new Date()).format("YYYY-MM-DD 00:00:00");
    toTime = moment(new Date()).format("YYYY-MM-DD 23:59:59");
    data: any = [];
    allAreaLst = [];
    allDoorLst = [];
    allDoorApiLst = [];
    selectedArea = [];
    selectedDoor = [];
    loading = false;
    page = 1;
    filter = "";
    departmentIds = [];
    selectUserTypeOption = [
        { value: 1, label: 'Employee' },
        { value: 2, label: 'Customer' },
        { value: 3, label: 'Student' },
        { value: 4, label: 'Parent' },
        { value: 5, label: 'Nanny' },
        { value: 6, label: 'Contractor' },
        { value: 7, label: 'Teacher' },
    ]
    tree = {
        employeeList: [],
        clearable: true,
        defaultExpandAll: false,
        multiple: true,
        placeholder: "",
        disabled: false,
        checkStrictly: false,
        popoverWidth: 400,
        treeData: [],
        treeProps: {
            value: 'ID',
            children: 'ListChildrent',
            label: 'Name',
        },
    }
    setColumn() {
        this.columns = [
                {
                    prop: 'EmployeeATID',
                    label: 'EmployeeATID',
                    minWidth: '80',
                    fixed: true,
                    display:true
                },
                {
                    prop: 'EmployeeCode',
                    label: 'EmployeeCode',
                    minWidth: '150',
                    fixed: true,
                    display: true
                },
                {
                    prop: 'FullName',
                    label: 'FullName',
                    minWidth: '180',
                    fixed: true,
                    display: true
                },
                {
                    prop: 'DepartmentName',
                    label: 'Department',
                    width: '220',
                    display: true
                },
                {
                    prop: 'TimeString',
                    label: 'Time',
                    minWidth: '180',
                    display: true
                },
                {
                    prop: 'AreaName',
                    label: 'Area',
                    minWidth: '180',
                    display: true
                },
                {
                    prop: 'DoorName',
                    label: 'Door',
                    minWidth: '180',
                    display: true
                },
                {
                    prop: 'AliasName',
                    label: 'AliasName',
                    minWidth: '180',
                    display: true
                },
               
               
                {
                    prop: 'InOutMode',
                    label: 'InOutMode',
                    minWidth: '180',
                    display: true
                },
          
        ];
    }

    async beforeMount() {
        this.setColumn();
        await this.getAllArea();
        await this.getAllDoor();
        await this.LoadDepartmentTree();
        await this.Search();
    }

    mounted() {
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.ac-log-history__custom-function-bar');  
        let childNodes = Array.from(component1.childNodes);

        const component5 = document.querySelector('.ac-log-history__data-table-function');
        (component5 as HTMLElement).style.width = "100%";
        (component5 as HTMLElement).style.display = "flex";
        (component5 as HTMLElement).style.justifyContent = "left";
        // childNodes.push(component5);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            if(index <= 1){
                (element as HTMLElement).style.height = '32px';
            }
            component5.insertBefore(element, component5.childNodes[index + 1]);
        }); 

        (component5.childNodes[component5.childNodes.length - 1] as HTMLElement).style.marginLeft = 'auto';
    }

    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.loading = true;
        return await attendanceLogApi
            .GetACAttendanceLogByFilter(
                this.page,              
                moment(this.fromTime).format("YYYY-MM-DD HH:mm:ss"),
                moment(this.toTime).format("YYYY-MM-DD HH:mm:ss"),
                this.filter,
                this.departmentIds,
                this.selectedArea,
                this.selectedDoor,
                50
                )
            .then(res => {
                this.loading = false;
                const { data } = res as any;
                return {
                    data: data.data,
                    total: data.total
                };
            })
          
    }

    onAreaChange(val){
        if(val.length == 0){
            this.allDoorLst = [...this.allDoorApiLst];
            this.selectedDoor = [];
        }else{
            this.selectedDoor = [];
            this.allDoorLst = this.allDoorApiLst.filter(x => val.includes(x.areaId));
        }
    }

    async getAllArea(){
        areaApi.GetAllArea().then(res => {
            const { data } = res as any;
            this.allAreaLst = data;
        });
    }

    async getAllDoor(){
        doorApi.GetAllDoor().then(res => {
            const { data } = res as any;
            this.allDoorLst = data;
            this.allDoorApiLst = data;
        })
    }

    async Search() {
        this.page = 1;
        (this.$refs.table as any).getTableData(this.page, null, null);
        
    }
}
