import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import { userPrivilegeApi } from '@/$api/user-privilege-api';
import { deviceApi } from '@/$api/device-api';

import { privilegeDetailApi } from '@/$api/privilege-detail-api';
import { isNullOrUndefined } from 'util';
import {Mutation} from 'vuex-class';
import { privilegeDeviceDetailApi } from '@/$api/privilege-device-detail-api';

const HeaderComponent = () => import("@/components/home/header-component/header-component.vue");
const DataTableComponent = () => import("@/components/home/data-table-component/data-table-component.vue");
const MatrixCheckboxComponent = () => import("@/components/app-component/matrix-checkbox-component/matrix-checkbox-component.vue")

@Component({
  name: "assign-privilege-device",
  components: { HeaderComponent, DataTableComponent, MatrixCheckboxComponent }
})
export default class AssignPrivilegeDeviceComponent extends Mixins(ComponentBase) {
  @Mutation("setAppIsEdit", { namespace: "Application" }) setAppIsEdit;
  groupUser: IMatrixColumn = [];
  listMenu: IMatrixListDevice = [];
  checked = true;
  isLoaded = true;

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
    // this.groupUser = [
    //   { id: 1, name: "Admin" },
    //   { id: 2, name: "Manager" },
    //   { id: 3, name: "User" }
    // ];
    // this.listMenu = [
    //   { privilegeindex: 'AssignPrivilegeUserGroup', name: this.$t('AssignPrivilegeUserGroup').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] },
    //   { privilegeindex: 'Service', name: this.$t('Service').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] },
    //   { privilegeindex: 'Device', name: this.$t('Device').toString(), roles: [{ group_id: 1, role: 'Full'}, { group_id: 2, role: 'None'}, { group_id: 3, role: 'Full'}] },
    //   { privilegeindex: 'HistoryUser', name: this.$t('HistoryUser').toString(), roles: [{ group_id: 1, role: 'ReadOnly'}, { group_id: 2, role: 'ReadOnly'}, { group_id: 3, role: 'None'}] }
    // ];

    this.getGroupUser();
    await this.getListMenu();
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
        console.log(res.data);
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

  async getListMenu() {
    // const menuData = await this.getMenu();
    privilegeDeviceDetailApi.GetPrivilegeDeviceDetail().then(res => {
      const listPrivilege = res.data;

      let menuData = [{ id: '1', key: 'Home', name: 'Home', path: 'home', icon: 'home.png' }];
      deviceApi.GetDeviceAllPrivilege().then(menuGroupData => {
        // (menuGroupData.data as any).forEach((m: any) => {
        //   menuData = menuData.concat(m.list_menu);
        // });
        menuData = (menuGroupData.data as any).map((m: any) => {
          return {
            id: m.value,
            key: m.value,
            name: m.label,
            path: '',
            icon: ''
          }
        });

        menuData.forEach((m: any) => {
          const privilegeByMenu = listPrivilege.filter(x => x.SerialNumber === m.key);
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
            privilegeindex: m.key,
            name: this.$t(m.name).toString(),
            roles: cloneListRoleNew
          });
        });
      });
    })
    .then(() => {
      this.isLoaded = true;
    })
  }

  SaveData() {
    const param = this.listMenu.map(x => {
      const roles = x.roles.map(r => {
        return {
          PrivilegeId: r.group_id,
          State: r.role
        }
      });

      return {
        SerialNumber: x.privilegeindex,
        Roles: roles
      }
    });

    privilegeDeviceDetailApi.InsertOrUpdatePrivilegeDeviceDetail(param).then(res => {
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
