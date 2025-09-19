import { Component, Vue, Prop, PropSync } from "vue-property-decorator";
const MultiStateComponent = () => import("@/components/app-component/multi-state-component/multi-state-component.vue");

@Component({
  name: 'matrix-checkbox-menu',
  components: {MultiStateComponent}
})
export default class MatrixCheckboxMenuComponent extends Vue {
  @PropSync('listData') list: IMatrixList;
  @PropSync('columns') tableHead: IMatrixColumn;
  @PropSync('isLoading') loading: boolean;
  @Prop({ default: () => [{ name: 'None', icon: 'el-icon-close', title: '', type: 'info' }] }) listState: IStateCollection;
  @Prop({default: false})
  fromDevice: Boolean
  maxHeight = 400;

  MaxHeightTable() {
    this.maxHeight = window.innerHeight - 150
  }


  beforeMount() {
    this.MaxHeightTable()
    window.addEventListener("resize", () => {
      this.MaxHeightTable()
    });
    // console.log(this.list)
  }

  changeState(id, value){
    // console.log("changeState", id, value)
    this.updateChildMenu(id, value);
    this.$nextTick(() => {
      this.updateMenu();
    });
  }

  reAssignMenuRoleView = false;
  updated(){
    // console.log("updated")
    if(this.list && this.list.length > 0 
      && !this.reAssignMenuRoleView
    ){
      this.reAssignMenuRoleView = true;
      this.updateMenu();
    }
  }

  updateChildMenu(id, value){
    const childMenus = this.list.filter(x => x.parentId == id);
    if(childMenus && childMenus.length > 0){
      childMenus.forEach(element => {
        const menuRole = element.roles.find(x => x.group_id == value.group_id);
        if(menuRole){
          menuRole.role = value.role;
        }
        if(this.list.some(x => x.parentId && x.parentId != "" && x.parentId == element.menuId)){
          this.updateChildMenu(element.menuId, value);
        }
      });
    }
  }

  updateMenu(){
    let assignParentModuleRole = [];
    this.list.forEach(element => {
      if(element.parentId && element.parentId != ""){
        const parentRole = {parentId: element.parentId, roles: []};
        if(element.roles && element.roles.length > 0){
          element.roles.forEach(roleElement => {
            parentRole.roles.push({group_id: roleElement.group_id, role: roleElement.role})
          });
        }
        if(parentRole.roles && parentRole.roles.length > 0){
          if(!assignParentModuleRole.some(x => x.parentId == parentRole.parentId)){
            let childMenus = this.list.filter(y => y.parentId == parentRole.parentId);
            if(childMenus && childMenus.length > 0){
              parentRole.roles.forEach(element => {
                const childRoles = childMenus.flatMap(user => 
                  user.roles.filter(role => role.group_id === element.group_id)
                );
                if(childRoles.every(c => c.role == "Full")){
                  element.role = "Full";
                }else if(childRoles.every(c => c.role == "None")){
                  element.role = "None";
                }else if(childRoles.some(c => c.role == "Full" || c.role == "Edit")){
                  element.role = "Edit";
                }else{
                  element.role = "ReadOnly";
                }
              });
            }
            assignParentModuleRole.push(parentRole);
          }
        }
      }
    });

    if(assignParentModuleRole && assignParentModuleRole.length > 0){
      this.reAssignParentMenuRole(assignParentModuleRole);
    }
  }

  reAssignParentMenuRole(arr){
    // console.log(arr)
    if(arr && arr.length){
      arr.forEach(element => {
        const existedParentRole = this.list.find(x => x.menuId == element.parentId);
        if(existedParentRole){
          existedParentRole.roles = element.roles;
          if(existedParentRole.parentId && existedParentRole.parentId != ""){
            this.reAssignNestedParentMenuRole(existedParentRole.parentId);
          }
        }
      });
      // console.log(this.list)
    }
  }

  reAssignNestedParentMenuRole(parentId){
    const nestParenMenuRole = this.list.find(x => x.menuId == parentId);
    if(nestParenMenuRole){
      nestParenMenuRole.roles.forEach(element => {
        const childRoles = Misc.cloneData(this.list.filter(x => x.parentId == parentId)).flatMap(user => 
          user.roles.filter(role => role.group_id === element.group_id)
        );
        if(childRoles.every(c => c.role == "Full")){
          element.role = "Full";
        }else if(childRoles.every(c => c.role == "None")){
          element.role = "None";
        }else if(childRoles.some(c => c.role == "Full" || c.role == "Edit")){
          element.role = "Edit";
        }else{
          element.role = "ReadOnly";
        }
      });
      if(nestParenMenuRole.parentId && nestParenMenuRole.parentId != ""){
        this.reAssignNestedParentMenuRole(nestParenMenuRole.parentId);
      }
    }
  }

  afterDestroy() {
    window.removeEventListener('resize', () => {});
  }
  // handleResizeGrid() {
  //   const clientHeight = this.$root.$el.clientHeight;
  //   this.maxHeight = clientHeight - 220;
  // }
  get getLabel() {
    if(this.fromDevice){
        return 'Danh sách máy / Nhóm người dùng';
    }
    else {
      return 'Menu / Nhóm người dùng';
    }

  }
}
