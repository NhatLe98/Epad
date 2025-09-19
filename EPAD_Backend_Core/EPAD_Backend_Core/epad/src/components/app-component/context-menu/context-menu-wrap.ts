import { Component, Prop, Vue } from "vue-property-decorator";
import ContextMenu from './context-menu.vue'

@Component({
    name: "contextmenu-wrap", components: { ContextMenu }
})
export default class ContextMenuWrap extends Vue {
    @Prop({ default: () => { } }) customEvent: any;
    @Prop({ default: true }) icon: Boolean;
    @Prop({ default: () => { x: null; y: null } }) axis: any;
    @Prop({
        default: () => [
            { icon: 'el-icon-edit', name: 'Edit', action: 'edit' },
            { icon: 'el-icon-setting', name: 'Setting', action: 'setting' }
        ]
    }) menu: Array<any>;
    @Prop({ default: () => { } }) resolve: Function;

    get style() {
        let x = this.axis.x;
        let y = this.axis.y;
        let menuHeight = this.menu.length * 32;
        let menuWidth = 150;
        const clientW = document.body.clientWidth;
        const clientH = document.body.clientHeight;
        return {
            left: (clientW < x + menuWidth ? x - menuWidth : x) + 'px',
            top: (clientH < y + menuHeight ? y - menuHeight : y) + 'px'
        };
    }

    status = false;

    fnHandler(item) {
        this.status = false;
        if (item.fn) item.fn(this.customEvent);
        this.resolve(item.action);
    }

    beforeDestroy() {
        document.body.removeChild(this.$el);
      }

      mounted() {
        this.$nextTick(function() {
          this.status = true;
        });
      }
};
