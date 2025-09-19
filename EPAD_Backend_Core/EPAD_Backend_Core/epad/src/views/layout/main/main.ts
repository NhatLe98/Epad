import { Component, Mixins } from "vue-property-decorator";
import { PageBase } from "@/mixins/application/page-mixins";
import { Getter, Mutation } from 'vuex-class';
import MenuComponent from '@/views/layout/menu/menu.vue';
import HeaderComponent from '@/views/layout/header/header.vue';
import { EventBus } from '@/$core/event-bus';


@Component({
  name: "main-layout",
  components: { MenuComponent, HeaderComponent }
})
export default class MainLayout extends Mixins(PageBase) {
  @Getter('getAppIsEditing', { namespace: 'Application' }) $appIsEditing;
  @Getter('getActiveForm', { namespace: 'Application' }) $getActiveForm;
  @Mutation("setAppIsEdit", { namespace: "Application" }) $setAppIsEdit;
  @Mutation("setActiveForm", { namespace: "Application" }) $setActiveForm;
  listMenu: Array<any> = [];
  listGroupMenu: Array<any> = [];
  formName: string = '';

  beforeMount() {
    EventBus.$on('changeFormName', this.changeFormNameHandle)

    Misc.readFileAsync('static/variables/group-menu.json').then(menu => {
      this.listGroupMenu = [...menu];
    })
  }

  afterDestroy() {
    EventBus.$off('changeFormName', this.changeFormNameHandle);
  }

  changeFormNameHandle(value){
    this.formName = value;
  }

  getImgUrl(icon) {
    return require('@/assets/icons/NavBar/' + icon);
  }

  mounted() {
    this.$setAppIsEdit(false);
    this.$setActiveForm(null);
    this.checkIdle();
  }
}
