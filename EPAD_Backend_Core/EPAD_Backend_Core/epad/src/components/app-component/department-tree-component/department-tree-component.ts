import { Component, Vue, Prop, Mixins, PropSync, Watch } from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
import ComponentBase from "@/mixins/application/component-mixins";
import { employeeInfoApi } from '@/$api/employee-info-api';
import { departmentApi} from '@/$api/department-api'

@Component({
    name: 'department-tree'
})
export default class DepartmentTreeComponent extends Mixins(ComponentBase) {
    @Prop({default: true}) showEmployee: boolean
    @PropSync('selectedEmployee') internalSelectedEmployee: any;
    @PropSync('selectedDepartment') internalSelectedDepartment: any;
    filterTree = "";
    loadingTree = false;
    dataTree = [];
    valTree = [];
    arrExpandedKeys=[-1];

    arrData: any;
    defaultChecked = [];
    masterEmployeeFilter = [];

    beforeMount() {
        this.LoadDepartmentTree();
    }

    flattenArray(data, parentId = null, result = []) {
        const cloneData = Misc.cloneData(data);
        cloneData.forEach(item => {
            // Create a copy of the item to avoid mutating the original data
            // const newItem = { ...item };
            const newItem = Misc.cloneData(item);

            // Set the parentIndex property
            newItem.ParentID = parentId;

            // Remove the children property from the new item
            delete newItem.ListChildrent;

            // Add the new item to the result array
            result.push(newItem);

            // Get the current item's index in the result array
            const currentIndex = item.ID;

            // If the item has a children array, recursively flatten it
            if (Array.isArray(item.ListChildrent) && item.ListChildrent.length > 0) {
                this.flattenArray(item.ListChildrent, currentIndex, result);
            }

            // delete item.ListChildrent;
        });

        return result;
    }

    findParentID(arrID){
		let result = [];
		const parentIDs = this.arrData.filter(x => arrID.includes(x.ID))?.map(x => x.ParentID) ?? [];
		if(parentIDs && parentIDs.length > 0){
			result = result.concat(parentIDs);
			if(this.arrData.some(x => parentIDs.includes(x.ID) && x.ParentID)){
				const nestParentIDs = this.findParentID(parentIDs);
				if(nestParentIDs && nestParentIDs.length > 0){
					result = result.concat(nestParentIDs);
				}
			}
		}
		result = [...new Set(result)];
		return result;
	}
    
    filterNode(value, data) {
        if (!value) return true;
       
        return ((!isNullOrUndefined(data.Name) && data.Name.toLowerCase().indexOf(value.toLowerCase()) !== -1)
            || (!isNullOrUndefined(data.EmployeeATID) && (data.EmployeeATID.toLowerCase().indexOf(value.toLowerCase()) !== -1 
            || value.toLowerCase().includes(data.EmployeeATID.toLowerCase())))
            || (!isNullOrUndefined(data.EmployeeCode) && data.EmployeeCode.toLowerCase().indexOf(value.toLowerCase()) !== -1)
            || (!isNullOrUndefined(data.NRIC) && data.NRIC.toLowerCase().indexOf(value.toLowerCase()) !== -1));

    }

    @Watch('filterTree')
    filterTreeData() {
        this.loadingTree = true;
        (this.$refs.tree as any).filter(this.filterTree);
        this.loadingTree = false;
    }

    LoadDepartmentTree() {
        if(this.showEmployee === true){
            employeeInfoApi.GetEmployeeAsTree().then((response: any) => {
                this.dataTree = response.data;
                if(this.dataTree){
					this.arrData = this.flattenArray(this.dataTree);
					const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
					if(jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0){
						this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
						this.defaultChecked = this.arrData.filter(x => this.masterEmployeeFilter.includes(x.EmployeeATID))
							?.map(x => x.ID) ?? [];
                        if(this.defaultChecked && this.defaultChecked.length > 0){
                            const result = this.findParentID(this.defaultChecked);
                            if(result && result.length > 0){
                                setTimeout(() => {
                                    const tree = (this.$refs.tree as any);
                                    result.forEach(element => {
                                        // tree.store.nodesMap[element].checked = true;
                                        tree.store.nodesMap[element].expanded = true;
                                        tree.store.nodesMap[element].loaded = true;
                                    });
                                }, 500);
                            }
                        }
					}
				}
            });
        }
        else{
            departmentApi.GetDepartmentTree().then((res: any)=>{
                if (res.status == 200) {
                    this.dataTree = res.data;
                }
            });
        }
    }

    getCheckedKeys(){
        const tree = this.$refs.tree as any;
        const arrCheckedNode = tree.getCheckedNodes();
        let arrID=[];
        arrCheckedNode.forEach(element => {
            arrID.push(element.ID);
        });
        return arrID;
    }

    getSelectedEmployee(){
        const tree = this.$refs.tree as any;
        const arrCheckedNode = tree.getCheckedNodes();
        const arrID = arrCheckedNode.filter(e => e.Type === 'Employee').map(e => e.EmployeeATID);
        return arrID;
    }

    getSelectedDepartment(){
        const tree = this.$refs.tree as any;
        const arrCheckedNode = tree.getCheckedNodes();
        const arrID = arrCheckedNode.filter(e => e.Type === 'Department').map(e => e.ID);
        return arrID;
    }

    setCheckedNode(arrKey){
        const tree = this.$refs.tree as any;
        tree.setCheckedKeys(arrKey);
    }

    getIconClass(type, gender) {
        switch (type) {
            case 'Company': return 'el-icon-office-building'; break;
            case 'Department': return 'el-icon-s-home'; break;
            case 'Employee':
                if(this.showEmployee == true){
                    if (isNullOrUndefined(gender) || gender === "Other") {
                        return 'el-icon-s-custom employee-other';
                    }
                    else if (gender === "Male") {
                        return 'el-icon-s-custom employee-male';
                    }
                    else {
                        return 'el-icon-s-custom employee-female';
                    }
                }
        }
    }

    getIconSrc1(type) {
        switch (type) {
            case 'Company': return require('@/assets/icons/Common/company.png'); break;
            case 'Department': return require('@/assets/icons/Common/department.png'); break;
            case 'Employee': return require('@/assets/icons/Common/user.png'); break;
        }
    }

    getIconSrc(type) {
        switch (type) {
            case 'Company': return 'assets/icons/Common/company.png'; break;
            case 'Department': return 'assets/icons/Common/department.png'; break;
            case 'Employee': return 'assets/icons/Common/employee.png'; break;
        }
    }

}
