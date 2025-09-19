import { Component, Vue, Mixins } from 'vue-property-decorator';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import ComponentBase from '@/mixins/application/component-mixins';
import { employeeInfoApi } from '@/$api/employee-info-api';
import { isNullOrUndefined } from 'util';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import moment from 'moment';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { workingInfoApi, IC_WorkingInfo, AddedParam } from '@/$api/working-info-api';
import { departmentApi } from '@/$api/department-api';
import { ElForm } from 'element-ui/types/form';
import { valHooks } from 'jquery';
@Component({
    name: 'working-info',
    components: {
        HeaderComponent,
        DataTableComponent,
        DataTableFunctionComponent,
    },
})
export default class WorkingInfoComponent extends Mixins(ComponentBase) {
    loadingTree = false;
    treeData: any = [];
    filterTree = "";
    key: any = [];
    expandedKey = [-1];
    page = 1;
    fromDate = moment(new Date()).format('YYYY-MM-DD');
    toDate = moment(new Date()).format('YYYY-MM-DD');
    rowsObj = [];
    columns = [];
    showDialog = false;
    listExcelFunction = ['AddExcel'];
    isEdit = false;
    comboDepartment = [];
    addedParams: Array<AddedParam> = [];
    arrData: any;
    defaultChecked = [];
    masterEmployeeFilter = [];

    setColumns() {
        this.columns = [
            {
                prop: 'EmployeeATID',
                label: 'EmployeeATID',
                minWidth: 150,
                fixed: true,
                display: true
            },
            {
                prop: 'FullName',
                label: 'FullName',
                minWidth: 200,
                fixed: true,
                display: true
            },
            {
                prop: 'NewDepartmentName',
                label: 'DepartmentName',
                minWidth: 200,
            },
            {
                prop: 'FromDate',
                label: 'WorkingFromDate',
                minWidth: 160,
                display: true
            },
            {
                prop: 'ToDate',
                label: 'WorkingToDate',
                minWidth: 160,
                display: true
            },
        ];
    }
    ArrEmployeeATID = [];
    ruleForm: IC_WorkingInfo = {
        Index: 0,
        EmployeeATID: '',
        FromDate: null,
        ToDate: null,
        DepartmentIndex: '',
    };
    rules: any = {};

    beforeMount() {
        this.loadDepertmentTree();
        this.initRule();
    }

    mounted() {
        this.setColumns();
        this.updateFunctionBarCSS();
    }

    updateFunctionBarCSS() {
        //// Get all child in custom function bar
        const component1 = document.querySelector('.working-info__custom-function-bar');  
        // console.log(component1.childNodes)
        const component2 = document.querySelector('.working-info__data-table'); 
        // console.log(component2.childNodes)
        let childNodes = Array.from(component1.childNodes);
        // const component3 = document.querySelector('.history-user__data-table-function');
        // childNodes.push(component3);
        const component5 = document.querySelector('.working-info__data-table-function');
        (component5 as HTMLElement).style.width = "100%";
        (component5 as HTMLElement).style.display = "flex";
        (component5 as HTMLElement).style.justifyContent = "flex-end";
        (component5 as HTMLElement).style.float = "right";
        childNodes.push(component5);
        //// Insert all child in custom function bar to after filter bar of table
        childNodes.forEach((element, index) => {
            component2.insertBefore(element, component2.childNodes[index + 1]);
        }); 
        (document.querySelector('.working-info__data-table-function') as HTMLElement).style.height = "0";
    }

    initRule() {
        this.rules = {
            DepartmentIndex: [
                {
                    required: true,
                    message: this.$t('PleaseSelectDepartmentTransfer'),
                    trigger: 'blur',
                },
            ],
            FromDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectDayTransfer'),
                    trigger: 'blur',
                },
            ],
        };
    }
    filterNode(value, data) {
        if (!value) return true;
        return (data.Name.indexOf(value) !== -1 || (!isNullOrUndefined(data.EmployeeATID) && data.EmployeeATID.indexOf(value) !== -1));

    }
    async filterTreeData() {
        this.loadingTree = true;
        (this.$refs.tree as any).filter(this.filterTree);
        this.loadingTree = false;
    }
    getPage({ page, filter, sortParams,pageSize }) {
        this.page = page;
        this.addedParams = [];
        if (!isNullOrUndefined(filter) && filter != '') {
            this.addedParams.push({ Key: "Filter", Value: filter });
        }
        this.addedParams.push({ Key: "PageIndex", Value: page });
        this.addedParams.push({ Key: "FromDate", Value: this.fromDate });
        this.addedParams.push({ Key: "ToDate", Value: this.toDate });
        this.addedParams.push({ Key: "PageSize",Value: pageSize});
        //this.addedParams.push({ Key: "Status", Value: 1 });

        return workingInfoApi.PostPage(this.addedParams).then((res: any) => {
            let listData = res.data;
            listData.data.forEach((item) => {
                item.ToDate = (isNullOrUndefined(item.ToDate) == false) ? moment(item.ToDate).format('DD-MM-YYYY') : '';
                item.FromDate = moment(item.FromDate).format('DD-MM-YYYY');
            });
            return {
                data: listData.data,
                total: listData.total,
            };
        });

    }

    async loadDepertmentTree() {
        return await employeeInfoApi
            .GetEmployeeAsTree()
            .then((res) => {
                this.loadingTree = false;
                const data = res.data as any;
                this.treeData = data;
                if(this.treeData){
					this.arrData = this.flattenArray(this.treeData);
					const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
					if(jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0){
						this.masterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
						this.key = this.masterEmployeeFilter;
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
                //console.log(this.treeData);
                // this.treeData[0] = this.GetListChildrent(data[0]);
            })
            .catch(() => {
                this.loadingTree = false;
            });
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

    async nodeCheck(e) {
        this.loadingEffect(500);
        //this.key = (this.$refs.tree as any)
        //	.getCheckedNodes()
        //	.filter((e) => e.Type == 'Employee')
        //	.map((e) => e.EmployeeATID);
        if (!this.filterTree) {
            this.key = (this.$refs.tree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee')
                .map((e) => e.EmployeeATID);
        }
        else {
            this.key = (this.$refs.tree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee' && (e.Name.indexOf(this.filterTree) !== -1) || (!isNullOrUndefined(e.EmployeeATID) && e.EmployeeATID.indexOf(this.filterTree) !== -1))
                .map((e) => e.EmployeeATID);
        }
        console.log(this.key)
    }

    loadingEffect(x) {
        const loading = this.$loading({
            lock: true,
            text: 'Loading',
            spinner: 'el-icon-loading',
            background: 'rgba(0, 0, 0, 0.7)',
        });
        setTimeout(() => {
            loading.close();
        }, x);
    }

    getIconClass(type, gender) {
        switch (type) {
            case 'Company':
                return 'el-icon-office-building';
                break;
            case 'Department':
                return 'el-icon-s-home';
                break;
            case 'Employee':
                if (isNullOrUndefined(gender) || gender === 'Other') {
                    return 'el-icon-s-custom employee-other';
                } else if (gender === 'Male') {
                    return 'el-icon-s-custom employee-male';
                } else {
                    return 'el-icon-s-custom employee-female';
                }
        }
    }

    async View() {
        if (Date.parse(this.fromDate) > Date.parse(this.toDate)) {
            this.$alert(this.$t('PleaseCheckTheCondition').toString(), this.$t('Notify').toString(), { type: 'warning' });
        } else {
            this.page = 1;
            (this.$refs.table as any).getTableData(this.page, null, null);
            // return await workingInfoApi
            // 	.WorkingInfo(1, '', moment(this.fromDate).format('YYYY-MM-DD'), moment(this.toDate).format('YYYY-MM-DD'))
            // 	.then((res) => {
            // 		let { data } = res;
            // 		return {
            // 			data: data.data,
            // 			total: data.total,
            // 		};
            // 	});
            // .then(() => {
            // 	(this.$refs.table as any).getTableData(this.page, null, null);
            // });
        }
    }
}
