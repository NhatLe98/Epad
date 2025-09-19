import { Component, Mixins, Prop } from "vue-property-decorator";
import { PageBase } from "@/mixins/application/page-mixins";
import { Getter, Mutation } from 'vuex-class';
import { EventBus } from '@/$core/event-bus';
import { isNullOrUndefined } from 'util';
const MenuComponent = () => import('@/views/layout/menu/menu.vue')
import { configApi } from "@/$api/config-api";
import { Message } from 'element-ui';
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

@Component({
    name: "home",
    components: { MenuComponent, HeaderComponent }
})
export default class Home extends Mixins(PageBase) {
    @Getter('getAppIsEditing', { namespace: 'Application' }) $appIsEditing;
    @Getter('getActiveForm', { namespace: 'Application' }) $getActiveForm;
    @Getter('getUsingBasicMenu', { namespace: 'AppHost' }) $getUsingBasicMenu;
    @Mutation("setAppIsEdit", { namespace: "Application" }) $setAppIsEdit;
    @Mutation("setActiveForm", { namespace: "Application" }) $setActiveForm;
    listMenu: Array<any> = [];

    @Getter("getMenuIsCollapse", { namespace: "Application" }) $menuIsCollapse;
    @Mutation("setMenuIsCollapse", { namespace: "Application" }) $setMenuIsCollapse;

    listGroupMenu: Array<any> = [];
    formName: string = '';
    usingBasicMenu: boolean = true;
    clientName: string;
    collapse = false;
    connection;
    isConnect;
    beforeMount() {
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.clientName = x.ClientName;
        })

        if (isNullOrUndefined(localStorage.getItem("collapse"))) {
            localStorage.setItem("collapse", "false")
        }
        else {
            if (localStorage.getItem("collapse") == "true") {
                this.collapse = true
            }
            else {
                this.collapse = false
            }
        }
        EventBus.$on('changeFormName', this.changeFormNameHandle)
        if (this.clientName == "" || this.clientName == "Ortholite" || this.clientName == "PSV" || this.clientName == "Swanbay") {
            if (this.usingBasicMenu === true) {
                Misc.readFileAsync('static/variables/group-menu-basic.json').then(menu => {
                    this.listGroupMenu = [...menu];
                })
            } else {
                Misc.readFileAsync('static/variables/group-menu.json').then(menu => {
                    this.listGroupMenu = [...menu];
                })
            }
        } else if (this.clientName == "MAY") {
            Misc.readFileAsync('static/variables/group-menu-may.json').then(menu => {
                this.listGroupMenu = [...menu];
            })
        } 
    }

    afterDestroy() {
        EventBus.$off('changeFormName', this.changeFormNameHandle);
    }

    changeFormNameHandle(value) {
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

    get hasTreeView() {
        if (this.$route.fullPath === '/change-department' || this.$route.fullPath === '/auto-sync-user' || this.$route.fullPath === '/view-log-attend') {
            return true
        }
        else {
            return false
        }
    }

    collapseHandle() {
        this.$setMenuIsCollapse(!this.collapse)
        this.collapse = !this.collapse
        localStorage.setItem("collapse", this.collapse + "")
    }
}
