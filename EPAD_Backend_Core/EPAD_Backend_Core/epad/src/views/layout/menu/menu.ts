import { Component, Mixins, Vue, Prop } from "vue-property-decorator";
import { Getter, Mutation } from 'vuex-class';
import { privilegeDetailApi } from '@/$api/privilege-detail-api';
import { AppRoute } from "@/router";
import { isNullOrUndefined } from 'util';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

@Component({
    name: "menu-component"
})
export default class MenuComponent extends Vue {
    @Getter('getAppIsEditing', { namespace: 'Application' }) $appIsEditing;
    @Getter('getActiveForm', { namespace: 'Application' }) $getActiveForm;
    @Mutation("setAppIsEdit", { namespace: "Application" }) $setAppIsEdit;
    @Mutation("setActiveForm", { namespace: "Application" }) $setActiveForm;
    @Prop({ default: false }) collapse: boolean;

    collapse_menu;
    listGroupMenu: Array<IGroupMenu> = [];
    leftMenuHeight: number = 400;
    usingBasicMenu: boolean = true;
    loadPrivilegeMenu: string; 
    clientName: string;
    backGround = '#FFFFFF';
    text = '#2D3042'
    menuText = '#2D3042'
    header = '#FFFFFF';
    ischange = false;  
    currentPath = '';
    activeIndex = "1";
    collapseHandle() {
        this.$emit('collapse-handle')
        //this.collapse_menu = !this.collapse_menu
    }

    isUsingNewAttendanceMonitoring = false;
    async beforeMount() {
        this.collapse_menu = this.collapse;
        await new Promise(async (resolve, reject) => {
            await Misc.readFileAsync('static/variables/color.json').then(x => {
                this.ischange = true;
                if(UPDATE_UI != 'true'){
                    this.backGround = x.defaultLeftSideBar;
                    this.text = x.defaultColorFont;
                    this.menuText = x.defaultColorFont;
                    this.header = x.defaultTopSideBar;
                }
                if(UPDATE_UI == 'true'){
                    if(!UI_NAME || UI_NAME.trim().length == 0){
                        this.backGround = x.leftSideBar && x.leftSideBar != '' ? x.leftSideBar : x.defaultLeftSideBar;
                        this.text = x.colorFont && x.colorFont != '' ? x.colorFont : x.defaultColorFont;
                        this.menuText = x.colorMenuText && x.colorMenuText != '' ? x.colorMenuText : x.defaultColorFont;
                        this.header = x.topSideBar && x.topSideBar != '' ? x.topSideBar : x.defaultTopSideBar;
                    }else{
                        this.backGround = x.ColorThemes[UI_NAME].leftSideBar && x.ColorThemes[UI_NAME].leftSideBar != '' ? x.ColorThemes[UI_NAME].leftSideBar : x.defaultLeftSideBar;
                        this.text = x.ColorThemes[UI_NAME].colorFont && x.ColorThemes[UI_NAME].colorFont != '' ? x.ColorThemes[UI_NAME].colorFont : x.defaultColorFont;
                        this.menuText = x.ColorThemes[UI_NAME].colorMenuText && x.ColorThemes[UI_NAME].colorMenuText != '' ? x.ColorThemes[UI_NAME].colorMenuText : x.defaultColorFont;
                        this.header = x.ColorThemes[UI_NAME].topSideBar && x.ColorThemes[UI_NAME].topSideBar != '' ? x.ColorThemes[UI_NAME].topSideBar : x.defaultTopSideBar;
                    }
                }
                const lefta : HTMLElement =document.querySelector('.el-header');
                lefta.style.backgroundColor = this.header;

                const nav : HTMLElement =document.querySelector('.logoNav');
                nav.style.backgroundColor = this.backGround;

                const nava : HTMLElement =document.querySelector('.botNav');
                nava.style.backgroundColor = this.backGround;
                nav.style.backgroundColor = this.backGround;

                const navaa : HTMLElement =document.querySelector('#FormName');
                navaa.style.color = this.text;

                resolve(x);
            }).catch(x => resolve('a'));
        });
       
        Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.usingBasicMenu = x.UsingBasicMenu;
            this.loadPrivilegeMenu = x.LoadPrivilegeMenu;
            this.clientName = x.ClientName;
            this.isUsingNewAttendanceMonitoring = x.UsingNewAttendanceMonitoring;
            if (this.clientName == "" || this.clientName == "Ortholite" || this.clientName == "PSV" || this.clientName == "Swanbay" || this.clientName == "Mondelez") {
                if (this.usingBasicMenu === true && !this.loadPrivilegeMenu) {
                    Misc.readFileAsync('static/variables/group-menu-basic.json').then(menu => {
                        privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                            const listPri = pri.data as any;
                            this.resolvePrivilegeMenu(listPri, [...menu]);
                            this.resolveMetaRoute(listPri);
                        });
                    });
                } else {
                    if(!this.loadPrivilegeMenu){
                        Misc.readFileAsync('static/variables/group-menu.json').then(menu => {
                            privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                                const listPri = pri.data as any;
                                this.resolvePrivilegeMenu(listPri, [...menu]);
                                this.resolveMetaRoute(listPri);
                            });
                        });
                    }else{
                        if(this.loadPrivilegeMenu == "eAC"){
                            Misc.readFileAsync('static/variables/group-menu-eAC.json').then(menu => {
                                privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                                    const listPri = pri.data as any;
                                    this.resolvePrivilegeMenu(listPri, [...menu]);
                                    this.resolveMetaRoute(listPri);
                                });
                            });
                        }else if(this.loadPrivilegeMenu == "eGCS"){
                            Misc.readFileAsync('static/variables/group-menu-eGCS.json').then(menu => {
                                privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                                    const listPri = pri.data as any;
                                    this.resolvePrivilegeMenu(listPri, [...menu]);
                                    this.resolveMetaRoute(listPri);
                                });
                            });
                        }
                        else if(this.loadPrivilegeMenu == "eHSR"){
                            Misc.readFileAsync('static/variables/group-menu-eHSR.json').then(menu => {
                                privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                                    const listPri = pri.data as any;
                                    this.resolvePrivilegeMenu(listPri, [...menu]);
                                    this.resolveMetaRoute(listPri);
                                });
                            });
                        }
                        else if(this.loadPrivilegeMenu == "eTA"){
                            Misc.readFileAsync('static/variables/group-menu-eTA.json').then(menu => {
                                privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                                    const listPri = pri.data as any;
                                    this.resolvePrivilegeMenu(listPri, [...menu]);
                                    this.resolveMetaRoute(listPri);
                                });
                            });
                        }
                        
                    }
                  
                }
            }
            else if (this.clientName == "MAY") {
                Misc.readFileAsync('static/variables/group-menu-may.json').then(menu => {
                    privilegeDetailApi.GetCurrentPrivilege().then(pri => {
                        const listPri = pri.data as any;
                        this.resolvePrivilegeMenu(listPri, [...menu]);
                        this.resolveMetaRoute(listPri);
                    });
                });
            }
        })

        window.addEventListener('resize', this.handleResize);
        this.handleResize();

    }

    resolveRoute() {

    }

    resolvePrivilegeMenu(listPrivilege, listGroupMenu: Array<IGroupMenu>) {
        this.currentPath = this.$route.name;
        //LV1
        this.listGroupMenu = listGroupMenu.map(gr => {
            let listMenu: Array<IMainMenu> = [];

            //LV2
            gr.list_menu.forEach(m => {
                const checkPri = listPrivilege.some(x => x.FormName === m.key && x.Roles.some(r => ['Full', 'ReadOnly', 'Edit'].indexOf(r) !== -1));
                if (checkPri === true) {
                    listMenu.push({ ...m });
                }

                // Trong tương lai nếu có thời gian nên chuyển đoạn code check menu con thành dạng đệ quy,
                // và phải chuyển cả front-end ở trang menu.vue và assign-privilege-user-group-component.ts
                //LV3
                if((m as any).list_menu && (m as any).list_menu.length > 0){
                    const checkChildPri = listPrivilege.some(y => ((m as any).list_menu.some(e => e.key == y.FormName) 
                        && y.Roles.some(r => ['Full', 'ReadOnly', 'Edit'].indexOf(r) !== -1)) 
                        || ((m as any).list_menu.some(e => e.list_menu && e.list_menu.length > 0)));
                    if(checkChildPri){
                        const cloneMenu = Misc.cloneData(m);
                        cloneMenu.show = true;
                        cloneMenu.list_menu = cloneMenu.list_menu.filter(x => listPrivilege.some(y => y.FormName == x.key 
                            && y.Roles.some(r => ['Full', 'ReadOnly', 'Edit'].indexOf(r) !== -1)) || 
                            (x.list_menu && x.list_menu.length > 0));
                        //LV4
                        if(cloneMenu.list_menu && cloneMenu.list_menu.length > 0){
                            if(cloneMenu.list_menu.some(y => y.list_menu && y.list_menu.length > 0)){
                                cloneMenu.list_menu.forEach(cm => {
                                    if(cm.list_menu && cm.list_menu.length > 0){
                                        cm.show = true;
                                        cm.list_menu = cm.list_menu.filter(x => listPrivilege.some(y => y.FormName == x.key 
                                            && y.Roles.some(r => ['Full', 'ReadOnly', 'Edit'].indexOf(r) !== -1)) || 
                                            (x.list_menu && x.list_menu.length > 0));
                                        //LV5
                                        if(cm.list_menu && cm.list_menu.length > 0){
                                            if(cm.list_menu.some(y => y.list_menu && y.list_menu.length > 0)){
                                                cm.list_menu.forEach(cm2 => {
                                                    cm2.show = true;
                                                    if(cm2.list_menu && cm2.list_menu.length > 0){
                                                        cm2.list_menu = cm2.list_menu.filter(x => listPrivilege.some(y => y.FormName == x.key 
                                                            && y.Roles.some(r => ['Full', 'ReadOnly', 'Edit'].indexOf(r) !== -1)));
                                                    }
                                                });
                                                cm.list_menu = cm.list_menu.filter(x => !x.list_menu || (x.list_menu && x.list_menu.length > 0));
                                            }
                                        }
                                    }
                                });
                                cloneMenu.list_menu = cloneMenu.list_menu.filter(x => !x.list_menu || (x.list_menu && x.list_menu.length > 0));
                            }
                        }
                        if(cloneMenu.list_menu && cloneMenu.list_menu.length > 0){
                            listMenu.push(cloneMenu);
                        }
                    }
                }
            })

            return {
                group_id: gr.group_id,
                group_key: gr.group_key,
                group_name: gr.group_name,
                group_path: gr.group_path,
                group_icon: gr.group_icon,
                group_show: gr.group_show,
                list_menu: listMenu
            }
        })

        this.listGroupMenu = this.listGroupMenu.filter(x => x.list_menu && x.list_menu.length > 0);

        // // Check current path and set active index to menu is that path's id
        // setTimeout(() => {
        //     this.listGroupMenu.forEach((groupMenu, groupIndex) => {
        //         if(groupMenu.group_path == this.$route.name) {
        //             this.activeIndex = groupMenu.group_id;
        //             return;
        //         }else if(groupMenu.list_menu && groupMenu.list_menu.length > 0){
        //             groupMenu.list_menu.forEach((menuLV2, menuLV2Index) => {
        //                 if(menuLV2.path == this.$route.name) {
        //                     this.activeIndex = menuLV2.id;
        //                     return;
        //                 }else if((menuLV2 as any).list_menu && (menuLV2 as any).list_menu.length > 0){
        //                     (menuLV2 as any).list_menu.forEach((menuLV3, menuLV3Index) => {
        //                         if(menuLV3.path == this.$route.name) {
        //                             this.activeIndex = menuLV3.id;
        //                             return;
        //                         }else if((menuLV3 as any).list_menu && (menuLV3 as any).list_menu.length > 0){
        //                             (menuLV3 as any).list_menu.forEach((menuLV4, menuLV4Index) => {
        //                                 if(menuLV4.path == this.$route.name) {
        //                                     this.activeIndex = menuLV4.id;
        //                                     return;
        //                                 }else if((menuLV4 as any).list_menu && (menuLV4 as any).list_menu.length > 0){
        //                                     (menuLV4 as any).list_menu.forEach((menuLV5, menuLV5Index) => {
        //                                         if(menuLV5.path == this.$route.name) {
        //                                             this.activeIndex = menuLV5.id;
        //                                             return;
        //                                         }
        //                                     });
        //                                 }
        //                             });
        //                         }
        //                     });
        //                 }
        //             });
        //         }
        //     });
        //     (this.$refs.menu as any).close(this.activeIndex);
        // }, 100);
    }

    enter(menu){
        if(!this.collapse){
            if(menu.group_id){
                (this.$refs.menu as any).open(menu.group_id);
                const top = document.getElementById(menu.group_id).getBoundingClientRect().top;
                const bottom = document.getElementById(menu.group_id).getBoundingClientRect().bottom;
                const left = document.getElementById(menu.group_id).getBoundingClientRect().left;
                document.getElementById(menu.group_id).classList.add("is-opened");
                // document.getElementById(menu.group_id).querySelector('ul').style.display = "unset";
                document.getElementById(menu.group_id).querySelector('ul').style.top = `${top}px`;
                document.getElementById(menu.group_id).querySelector('ul').style.left = `${left + 238}px`;
    
                setTimeout(() => {
                    if (window.innerHeight < bottom 
                        || (Math.abs(bottom - window.innerHeight)) < document.getElementById(menu.group_id).querySelector('ul').offsetHeight) {
                        document.getElementById(menu.group_id).classList.add('menu-not-inside');
                        const arrClass = document.getElementById(menu.group_id).classList;
                        if (arrClass.value.indexOf('menu-not-inside') === -1) {
                            document.getElementById(menu.group_id).classList.add('menu-not-inside');
                        }
                    }else{
                        document.getElementById(menu.group_id).classList.remove('menu-not-inside');
                    }
                }, 50);
                
            }else if(menu.id){
                (this.$refs.menu as any).open(menu.id);
                const top = document.getElementById(menu.id).getBoundingClientRect().top;
                const bottom = document.getElementById(menu.id).getBoundingClientRect().bottom;
                const left = document.getElementById(menu.id).getBoundingClientRect().left;
                document.getElementById(menu.id).classList.add("is-opened");
                // document.getElementById(menu.id).querySelector('ul').style.display = "unset";
                document.getElementById(menu.id).querySelector('ul').style.top = `${top}px`;
                document.getElementById(menu.id).querySelector('ul').style.left = `${left + 238}px`;
    
                setTimeout(() => {
                    if (window.innerHeight < bottom 
                        || (Math.abs(bottom - window.innerHeight)) < document.getElementById(menu.id).querySelector('ul').offsetHeight) {
                        document.getElementById(menu.id).classList.add('menu-not-inside');
                        const arrClass = document.getElementById(menu.id).classList;
                        if (arrClass.value.indexOf('menu-not-inside') === -1) {
                            document.getElementById(menu.id).classList.add('menu-not-inside');
                        }
                    }else{
                        document.getElementById(menu.id).classList.remove('menu-not-inside');
                    }
                }, 50);
            }
        }
        
    }

    leave(menu){
        if(!this.collapse){
            if(menu.group_id){
                (this.$refs.menu as any).close(menu.group_id);
                document.getElementById(menu.group_id).classList.remove("is-opened");
                // document.getElementById(menu.group_id).querySelector('ul').style.display = "none";
            }else if(menu.id){
                (this.$refs.menu as any).close(menu.id);
                document.getElementById(menu.id).classList.remove("is-opened");
                // document.getElementById(menu.id).querySelector('ul').style.display = "none";
            }
        }
    }

    resolveMetaRoute(listPrivilege) {
        // select all children route "/"
        const allRoute = (AppRoute as any).options.routes[2].children;
        allRoute?.forEach((r, ix) => {
            const pri = listPrivilege.find((x) => x.FormName === r.meta.formName);
            if (isNullOrUndefined(pri)) {
                Object.assign(r.meta, { role: ['None'] });
            }
            else {
                Object.assign(r.meta, { role: pri.Roles });
            }
        });
    }

    afterDestroy() {
        window.removeEventListener('resize', this.handleResize);
    }

    handleResize() {
        const clientHeight = this.$root.$el.clientHeight;
        this.leftMenuHeight = clientHeight;
    }

    getImgUrl(icon) {
        return '/assets/icons/NavBar/' + icon;
    }

    get getMenuHeight() {
        return `${this.leftMenuHeight}px`;
    }
    onClick(ev){
       if(this.ischange){
        setTimeout(() => {
            const lefta : HTMLElement =document.querySelector('.el-header');
            lefta.style.background = this.header;

            const navaa : HTMLElement =document.querySelector('#FormName');
                navaa.style.color = this.text;
        }, 500);
       }
     
        
    }

    async mounted() {
        this.$setAppIsEdit(false);
        this.$setActiveForm(null);

        // const lefta = $('.content-page');
        // if (lefta) {
        //   var ps = lefta.css("background-color", emsConfig.background);
        // }

        //     Misc.readFileAsync('static/variables/app.host.json').then(x => {
        //         usingSSO = x.UsingSSO;
        //         if(usingSSO){
        //           window.location.replace(window.location.origin + "/signout-oidc");
        //         }
        //         else{
        //           if(AppRoute.currentRoute.name !== 'login' && AppRoute.currentRoute.name !== 'login-redirect' && AppRoute.currentRoute.name !== 'signout-oidc'){
        //             AppRoute.push({
        //               path: 'login',
        //               query: { redirect: AppRoute.currentRoute.path }
        //             });
        //           }  
        //         }
        //         });
          
    }

    get getClassMenu() {
        if (this.collapse === true) {
            return "leftMenu collapse"
        }
        else return "leftMenu"
    }

}
