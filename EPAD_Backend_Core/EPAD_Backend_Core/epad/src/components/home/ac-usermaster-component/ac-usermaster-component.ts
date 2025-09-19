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
import { userMasterApi } from "@/$api/user-master-api";
import { acUserMasterApi } from "@/$api/ac-usermaster-api";
import { ElForm } from "element-ui/types/form";
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
    selectedArea = [];
    selectedDoor = [];
    allDoorApiLst = [];
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
                    prop: 'TimezoneName',
                    label: 'Timezone',
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
                    prop: 'UpdatedDateString',
                    label: 'Time',
                    minWidth: '180',
                    display: true
                }
              
               
          
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
        
    }

    async LoadDepartmentTree() {
        await departmentApi.GetDepartmentTreeEmployeeScreen().then((res: any) => {
            if (res.status == 200) {
                this.tree.treeData = res.data;
            }
        });
    }

    async getData({ page, filter, sortParams, pageSize }) {
        return await acUserMasterApi
        .GetACSync(
            page,              
                moment(this.fromTime).format("YYYY-MM-DD HH:mm:ss"),
                moment(this.toTime).format("YYYY-MM-DD HH:mm:ss"),
                this.filter,
                this.departmentIds,
                this.selectedDoor,
                this.selectedArea,
                pageSize,
                0,
                null
                )
            .then(res => {
                this.loading = false;
                const { data } = res as any;
                return {
                    data: data.data,
                    total: data.total
                };
                // var ref =(this.$refs.table as any) ;
                // ref.dataTable = data.data;
                // ref.total = data.total;
            })
        //this.page = page;
        // return await acUserMasterApi
        //     .GetACSync(
        //         page,              
        //         moment(this.fromTime).format("YYYY-MM-DD HH:mm:ss"),
        //         moment(this.toTime).format("YYYY-MM-DD HH:mm:ss"),
        //         filter,
        //         this.departmentIds,
        //         this.selectedDoor,
        //         pageSize
        //         )
        //     .then(res => {
        //         const { data } = res as any;
        //         return {
        //             data: data.data,
        //             total: data.total
        //         };
        //     });
    }

    onAreaChange(val){
        console.log(val);
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
