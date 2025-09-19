import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { userPrivilegeApi } from '@/$api/user-privilege-api';
import { privilegeDetailApi } from '@/$api/privilege-detail-api';
import { isNullOrUndefined } from 'util';
import { Mutation } from 'vuex-class';

const HeaderComponent = () => import("@/components/home/header-component/header-component.vue");
const DataTableComponent = () => import("@/components/home/data-table-component/data-table-component.vue");
const MatrixCheckboxComponent = () => import("@/components/app-component/matrix-checkbox-component/matrix-checkbox-component.vue")
const MatrixCheckboxMenuComponent = () => import("@/components/app-component/matrix-checkbox-menu-component/matrix-checkbox-menu-component.vue")

@Component({
    name: "assign-privilege-user-group",
    components: { HeaderComponent, DataTableComponent, MatrixCheckboxComponent, MatrixCheckboxMenuComponent }
})
export default class AssignPrivilegeUserGroupComponent extends Mixins(ComponentBase) {
    @Mutation("setAppIsEdit", { namespace: "Application" }) setAppIsEdit;
    groupUser: IMatrixColumn = [];
    listMenu: IMatrixList = [];
    checked = true;
    isLoaded = true;
    usingBasicMenu: boolean = true;
    

    listState = [
        { name: 'None', icon: 'el-icon-close', title: 'StateNone', type: 'info' },
        { name: 'ReadOnly', icon: 'el-icon-minus', title: 'StateReadOnly', type: 'primary' },
        { name: 'Full', icon: 'el-icon-check', title: 'StateFullPrivilege', type: 'success' },
        { name: 'Edit', icon: 'el-icon-edit', title: 'StateEdit', type: 'warning' }
    ];

    listRoleBase: Array<IStateRole> = [];
    async beforeMount() {
        this.groupUser = [];
        this.listMenu = [];
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            
            this.getGroupUser();
            this.getListMenu();
        });
        
        // this.groupUser = [
        //   { id: 1, name: "Admin" },
        //   { id: 2, name: "Manager" },
        //   { id: 3, name: "User" }
        // ];
        // this.listMenu = [
        //   { id: 'AssignPrivilegeUserGroup', name: this.$t('AssignPrivilegeUserGroup').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] },
        //   { id: 'Service', name: this.$t('Service').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] },
        //   { id: 'Device', name: this.$t('Device').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'None'}, { group_id: 3, role: 'Full'}] },
        //   { id: 'HistoryUser', name: this.$t('HistoryUser').toString(), roles: [{ group_id: 1, role: 'ReadOnly'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] }
        // ];

       
    }

    changeState(data: any) {
        const role = this.listMenu[data.row].roles[data.col].role;
        const ixOfState = this.listState.findIndex(x => x.name === role);
        if (ixOfState === this.listState.length - 1) {
            this.listMenu[data.row].roles[data.col].role = this.listState[0].name;
        }
        else {
            this.listMenu[data.row].roles[data.col].role = this.listState[ixOfState + 1].name;
        }
    }

    getGroupUser() {
        userPrivilegeApi.GetAllUserPrivilege()
            .then(res => {
                const groupUserData = res.data;
                groupUserData.forEach((e: any) => {
                    this.groupUser.push({
                        id: e.Index,
                        name: e.Name
                    });
                    this.listRoleBase.push({
                        group_id: e.Index,
                        role: 'None'
                    });
                });
            })
            .catch()
    }

    getMenu() {
        const menuData = [];
        Misc.readFileAsync('static/variables/menu.json').then(data => {
            Object.assign(menuData, data);
        });
        return menuData;
    }

    getListMenu() {
        // const menuData = await this.getMenu();
        privilegeDetailApi.GetPrivilegeDetail().then(res => {
            const listPrivilege = res.data;

            let menuData = [{ id: '1', key: 'Home', name: 'Home', path: 'home', icon: 'home.png', level: 1, view: true, isLeaf: true }];
            let menuURL = '';
            if (this.usingBasicMenu === true) {
                menuURL = 'static/variables/group-menu-basic.json';
            } else {
                menuURL = 'static/variables/group-menu.json';
            }
            Misc.readFileAsync(menuURL).then(menuGroupData => {
                // console.log(menuGroupData)
                // Iterate group menu lv 1
                menuGroupData.forEach((m: any) => {
                    // console.log(m)
                    if(m.group_key && m.group_key == "Dashboard"){
                        let menuItems1 = m.list_menu.filter(x => !x.list_menu || (x.list_menu && x.list_menu.length == 0));
                        if(menuItems1 && menuItems1.length > 0){
                            menuItems1 = menuItems1.map(menuItem => {
                                return { view: true, level: 1, ...menuItem, isLeaf: true};
                            });
                            menuData = menuData.concat(menuItems1);
                        }
                    }else if(m.group_key && m.group_key != "Dashboard" && m.group_key != "Home"){
                        menuData.push({ ...m, id: m?.group_id ?? m.id, key: m?.group_key ?? m.key, 
                            name: m?.group_name ?? m.name, path: m?.group_path ?? m.path, icon: m?.group_icon ?? m.icon, 
                            show: m?.group_show ?? m.show, view: true, level: 1 });
                        let menuItems1 = m.list_menu.filter(x => !x.list_menu || (x.list_menu && x.list_menu.length == 0));
                        if(menuItems1 && menuItems1.length > 0){
                            menuItems1 = menuItems1.map(menuItem => {
                                return { view: true, level: 2, ...menuItem, isLeaf: true, parentId: m?.group_id ?? m.id };
                            });
                            menuData = menuData.concat(menuItems1);
                        }
                    }
                    
                    // Check if menu lv 2 has child menu lv 3
                    if(m.hasOwnProperty("list_menu") && m.list_menu.some(obj => obj.hasOwnProperty("list_menu"))){
                        m.list_menu.forEach(element => {
                            if(element.list_menu){
                                menuData.push({ ...element, id: element?.group_id ?? element.id, key: element?.group_key ?? element.key, 
                                    name: element?.group_name ?? element.name, path: element?.group_path ?? element.path, 
                                    icon: element?.group_icon ?? element.icon, show: element?.group_show ?? element.show,
                                    view: true, level: 2, parentId: m?.group_id ?? m.id });
                                // menuData = menuData.concat(element.list_menu.filter(x => !x.list_menu));
                                let menuItems2 = element.list_menu.filter(x => !x.list_menu || (x.list_menu && x.list_menu.length == 0));
                                if(menuItems2 && menuItems2.length > 0){
                                    menuItems2 = menuItems2.map(menuItem => {
                                        return { view: true, level: 3, ...menuItem, isLeaf: true, 
                                            parentId: element?.group_id ?? element.id };
                                    });
                                    menuData = menuData.concat(menuItems2);
                                }
                            }
                            // Check if menu lv 3 has child menu lv 4
                            if(element.hasOwnProperty("list_menu") && element.list_menu.some(obj => obj.hasOwnProperty("list_menu"))){
                                element.list_menu.forEach(child1 => {
                                    if(child1.list_menu){
                                        menuData.push({ ...child1, id: child1?.group_id ?? child1.id, 
                                            key: child1?.group_key ?? child1.key, 
                                            name: child1?.group_name ?? child1.name, path: child1?.group_path ?? child1.path, 
                                            icon: child1?.group_icon ?? child1.icon, show: child1?.group_show ?? child1.show,
                                            view: true, level: 3, parentId: element?.group_id ?? element.id });
                                        let addChildMenu1 = child1.list_menu.filter(x => 
                                            !x.list_menu || (x.list_menu && x.list_menu.length == 0));
                                        if(addChildMenu1 && addChildMenu1.length > 0){
                                            addChildMenu1 = addChildMenu1.map(menuItem => {
                                                return { view: true, level: 4, ...menuItem, isLeaf: true, 
                                                    parentId: child1?.group_id ?? child1.id };
                                            });
                                            menuData = menuData.concat(addChildMenu1);
                                        }
                                        // Check if menu lv 4 has child menu lv 5
                                        if(child1.hasOwnProperty("list_menu") && child1.list_menu.some(obj => obj.hasOwnProperty("list_menu"))){
                                            child1.list_menu.forEach(child2 => {
                                                if(child2.list_menu){
                                                    menuData.push({ ...child2, id: child2?.group_id ?? child2.id, 
                                                        key: child2?.group_key ?? child2.key, 
                                                        name: child2?.group_name ?? child2.name, 
                                                        path: child2?.group_path ?? child2.path, 
                                                        icon: child2?.group_icon ?? child2.icon, 
                                                        show: child2?.group_show ?? child2.show,
                                                        view: true, level: 4, parentId: child1?.group_id ?? child1.id });
                                                    let addChildMenu2 = child2.list_menu.filter(x => 
                                                        !x.list_menu || (x.list_menu && x.list_menu.length == 0));
                                                    if(addChildMenu2 && addChildMenu2.length > 0){
                                                        addChildMenu2 = addChildMenu2.map(menuItem => {
                                                            return { view: true, level: 5, ...menuItem, isLeaf: true, 
                                                                parentId: child2?.group_id ?? child2.id };
                                                        });
                                                        menuData = menuData.concat(addChildMenu2);
                                                    }  
                                                }                                          
                                            });
                                        }
                                    }
                                });
                            }
                        });
                    }
                });


                menuData.forEach((m: any) => {
                    const privilegeByMenu = listPrivilege.filter(x => x.FormName === m.key);
                    const groupRole = privilegeByMenu.map(x => {
                        return {
                            group_id: x.PrivilegeIndex,
                            role: x.Role
                        };
                    });
                    // let cloneListRole = Object.assign(this.listRoleBase, groupRole);
                    const cloneListRoleNew = JSON.parse(JSON.stringify(this.listRoleBase));
                    cloneListRoleNew.forEach(e => {
                        const group = groupRole.find(x => x.group_id === e.group_id);
                        if (!isNullOrUndefined(group)) {
                            e.role = group.role;
                        }
                    })

                    this.listMenu.push({
                        id: m.key,
                        name: this.$t(m.name).toString(),
                        roles: cloneListRoleNew,
                        level: m.level,
                        view: m.view,
                        isLeaf: m?.isLeaf ?? false,
                        parentId: m.parentId,
                        menuId: m.id
                    });
                });
            });
        })
            .then(() => {
                this.isLoaded = true;
            })
    }

    SaveData() {
        const param = this.listMenu.filter(x => x.isLeaf == true).map(x => {
            const roles = x.roles.map(r => {
                return {
                    PrivilegeId: r.group_id,
                    State: r.role
                }
            });

            return {
                Menu: x.id,
                Roles: roles
            }
        });

        privilegeDetailApi.UpdatePrivilegeDetail(param).then(res => {
            this.setAppIsEdit(false);
            this.$saveSuccess();
        });
        // const param = [
        //   {
        //     Menu: 'AssignPrivilegeUserGroup',
        //     Roles: [
        //       {
        //         PrivilegeId: 1,
        //         State: 'None'
        //       }
        //     ]                     
        //   }
        // ]
    }
}
