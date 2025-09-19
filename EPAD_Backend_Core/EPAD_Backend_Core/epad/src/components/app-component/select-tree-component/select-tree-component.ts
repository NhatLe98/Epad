import { Component, Vue, Prop, Watch, Mixins, Model } from 'vue-property-decorator';
import Emitter from 'element-ui/lib/mixins/emitter';
import {
    addResizeListener,
    removeResizeListener
} from 'element-ui/lib/utils/resize-event';
import ComponentBase from '@/mixins/application/component-mixins';
@Component({
    name: 'select-tree-component',
    components: {}
})
export default class SelectTreeComponent extends Mixins(ComponentBase) {
    @Model() value: Array<any>;
    @Prop() data: Array<any>;
    @Prop() defaultExpandAll: boolean;
    @Prop() multiple: boolean;
    @Prop() placeholder: string;
    @Prop() disabled: boolean;
    @Prop() props: any;
    @Prop() checkStrictly: boolean;
    @Prop() clearable: boolean;
    @Prop() popoverWidth: number;
    // {
    //     value: 'value',
    //     label: 'label',
    //     children: 'children',
    //     disabled: 'disabled',
    //     isLeaf: 'isLeaf',
    // };
    nodeKey = '';
    lazy = false;
    iconClass = '';
    indent = 0;
    accordion = false;
    autoExpandParent = true;
    renderAfterExpand = false;
    // [el-tree] forwarding parameters end
    placement = 'bottom-start';
    size = Vue.prototype.$ELEMENT ? Vue.prototype.$ELEMENT.size : '';
    values: any;
    get propsValue(): string {
        return this.nodeKey || this.props.value || 'value';
    }
    get propsLabel() {
        return this.props.label || 'label';
    }
    get propsIsLeaf() {
        return this.props.isLeaf || 'isLeaf';
    }
    get defaultExpandedKeys() {
        return Array.isArray(this.value)
            ? this.value
            : this.value || this.value === 0
                ? [this.value]
                : [];
    }
    visible = false;
    selectedLabel = '';
    minWidth = 0;
    beforeMount() {

    }
    created() {
        if (this.multiple && !Array.isArray(this.value)) {
            throw new Error(
                '[el-select-tree] props `value` must be Array if use multiple!'
            );
        }
    }
    mounted() {
        this.setSelected();
        addResizeListener(this.$el, this.handleResize);
    }
    beforeDestroy() {
        if (this.$el && this.handleResize) {
            removeResizeListener(this.$el, this.handleResize);
        }
    }
    // load(){

    // }
    // filterNodeMethod(){

    // }
    renderContent(h, { node, data, store }) {
        // console.log("renderContent", h);
        let icon = '';
        if (data.Type == "Company") {
            icon = "el-icon-office-building"
        }else if(data.Type == "Department"){
            icon = "el-icon-s-home"
        }else if (data.Type == "Employee") {
            icon = "el-icon-s-custom";
        }
        if (icon == "") {
            return h('span', null, [h('span', null, data.Name)]);
        }
        return h('span', {attrs: { style: 'custom-tree-node' }}, [h('i', {attrs: { style: 'margin-right:5px', class: icon }}, null), h('span', null, data.Name)]);
    }
    valueChange(value, node) {
        this.$emit('change', value, node);
		this.$emit('value', value);
    }
    clear() {
        this.visible = false;
        if (this.multiple) {
            this.valueChange([], null);
            this.$nextTick(() => {
                (this.$refs.elTree as any).setCheckedKeys([]);
            });
        } else {
            this.valueChange('', null);
        }
        this.$emit('clear');
    }
    
    handleScroll() {
        this.$refs.scrollbar && (this.$refs.scrollbar as any).handleScroll();
    }
    nodeClick(data, node, component) {
        const children = data[this.props.children];
        const value = data[this.propsValue];
        if (
            ((children && children.length) ||
                (this.lazy && !data[this.propsIsLeaf])) &&
            !this.checkStrictly
        ) {
            component.handleExpandIconClick();
        } else if (!this.multiple && !data.disabled) {
            if (value !== this.value) {
                this.valueChange(value, data);
                this.selectedLabel = data[this.propsLabel];
            }
            this.visible = false;
        }
    }
    checkChange() {
        const elTree = (this.$refs.elTree as any);
        const leafOnly = !this.checkStrictly;
        const keys = elTree.getCheckedKeys(leafOnly);
        const nodes = elTree.getCheckedNodes(leafOnly);
        this.valueChange(keys, nodes);
        this.setMultipleSelectedLabel();
    }
    setSelected() {
        this.$nextTick(() => {
            const elTree = (this.$refs.elTree as any);
            if (this.multiple) {
                elTree.setCheckedKeys(this.value);
                this.setMultipleSelectedLabel();
            } else {
                const selectedNode = elTree.getNode(this.value);
                this.selectedLabel = selectedNode
                    ? selectedNode.data[this.propsLabel]
                    : '';
            }
        });
    }
    setMultipleSelectedLabel() {
        const elTree = (this.$refs.elTree as any);
        const selectedNodes = elTree.getCheckedNodes(!this.checkStrictly);
        this.selectedLabel = selectedNodes
            .map((item) => item[this.propsLabel])
            .join(',');
    }
    treeItemClass(data) {
        return {
            'is-selected': this.multiple
                ? false
                : data[this.propsValue] === this.value,
            'is-disabled': data.disabled
        };
    }
    handleResize() {
        // set the `tree` default `min-width`
        // border's width is 2px
        this.minWidth = this.$el.clientWidth - 2;
    }
    @Watch('value') handlerValueChange() {
        this.setSelected();
        // trigger parent `el-form-item` validate event

        // this.dispatch('ElFormItem', 'el.form.change');
    }
    @Watch('data') handlerDataChange() {
        this.setSelected();
    }
}