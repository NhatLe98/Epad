import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";

import axios from "axios";
import DataTableComponent from "@/components/home/data-table-component/data-table-component.vue";
import DepartmentTreeComponent from "@/components/app-component/department-tree-component/department-tree-component.vue";
import { isNullOrUndefined } from 'util';

import { userPrivilegeApi } from '@/$api/user-privilege-api';
import { privilegeDepartmentApi,PrivilegeAndDepartmentsParam } from '@/$api/privilege-department-api';


@Component({
  name: "AssignPrivilegeDepartment",
  components: { HeaderComponent, DataTableComponent,DepartmentTreeComponent }
})
export default class AssignPrivilegeDepartment extends Mixins(ComponentBase) {
    listUserPrivilege = [];
    selectedUserGroup = 0;
    maxHeight = 400;

    async beforeMount() {
        this.maxHeight = window.innerHeight - 155;
        this.getGroupUser();
        
    }

    getGroupUser() {
        userPrivilegeApi.GetAllUserPrivilege().then((res: any) => {
            if (res.status == 200) {
              const arrUserPrivilege = res.data;
              for (let i = 0; i < arrUserPrivilege.length; i++) {
                this.listUserPrivilege.push({
                  index: arrUserPrivilege[i].Index,
                  name: arrUserPrivilege[i].Name
                });
              }
            }
          });
    }
    updateDepartmentByUserPrivilege(){
        if(this.selectedUserGroup>0){
            const departmentTree = this.$refs.departmentTree as any;
            const arrSelectedIndex = departmentTree.getCheckedKeys();
            console.log(arrSelectedIndex);
            const param: PrivilegeAndDepartmentsParam={
                PrivilegeIndex: this.selectedUserGroup,
                ListDepartmentIndexs: []
            };
            for (let index = 0; index < arrSelectedIndex.length; index++) {
                if(isNaN(parseFloat(arrSelectedIndex[index])) == false ){
                    param.ListDepartmentIndexs.push(parseInt(arrSelectedIndex[index]));
                }
                
            }
            privilegeDepartmentApi.UpdatePrivilegeAndDepartments(param).then((res: any)=>{
                if (res.status == 200) {
                    this.$saveSuccess();
                }
                else{
                    this.$alertSaveError(null, null, null, this.$t("MSG_SaveError").toString())
                }
            });
        }
    }
    getDepartmentsByUserPrivilege(privilegeIndex: number){
        privilegeDepartmentApi.GetDepartmentsByPrivilegeIndex(privilegeIndex).then((res: any)=>{
            if (res.status == 200) {
                const departmentTree = this.$refs.departmentTree as any;
                
                departmentTree.setCheckedNode(res.data);
            }
        });
    }
    handleCurrentChange(val) {
        this.selectedUserGroup = val.index;
        this.getDepartmentsByUserPrivilege(this.selectedUserGroup);
    }
}
