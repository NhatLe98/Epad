import { Component, Prop, Vue } from "vue-property-decorator";

@Component({
    name: "context-menu"
})

export default class ContextMenu extends Vue {
    @Prop({ default: true }) icon: Boolean;
    @Prop({
        default: () => [
            { icon: 'el-icon-edit', name: 'Edit', action: 'edit' },
            { icon: 'el-icon-setting', name: 'Setting', action: 'setting' }
        ]
    }) menu: Array<any>;
    @Prop({ default: () => { } }) resolve: Function;


    status = false;

    fnHandler(item) {
        if (item.children && item.children.length > 0) {
            return false;
        }
        this.status = false;
        this.resolve(item.action);
    }

    beforeDestroy() {
        document.body.removeChild(this.$el);
    }

    mounted() {
        this.$nextTick(function () {
            this.status = true;
        });
    }
};
