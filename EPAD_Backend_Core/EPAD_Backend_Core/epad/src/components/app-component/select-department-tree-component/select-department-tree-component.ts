import { Component, Vue, Prop, Watch, Mixins, Model } from 'vue-property-decorator';
import Emitter from 'element-ui/lib/mixins/emitter';
import {
    addResizeListener,
    removeResizeListener
} from 'element-ui/lib/utils/resize-event';
import ComponentBase from '@/mixins/application/component-mixins';
@Component({
    name: 'select-department-tree-component',
    components: {}
})
export default class SelectDepartmentTreeComponent extends Mixins(ComponentBase) {
    @Model() value: Array<any>;
    latestCheckedValue: any;
    latestKeys: any;
    latestNodes: any;
    @Prop() data: Array<any>;
    @Prop() defaultExpandAll: boolean;
    @Prop() multiple: boolean;
    @Prop() placeholder: string;
    @Prop() disabled: boolean;
    @Prop() props: any;
    @Prop() checkStrictly: boolean;
    @Prop() isSelectParent: boolean;
    @Prop() fixedLabel: string;
    @Prop() clearable: boolean;
    @Prop() popoverWidth: number;
    parentSelectedValue: any;
    parentSelectedNode: any;
    allSelectedValue: any;
    allSelectedNodd: any;
    // {
    //     value: 'value',
    //     label: 'label',
    //     children: 'children',
    //     disabled: 'disabled',
    //     isLeaf: 'isLeaf',
    // };
    popperTop = 0;
    popperLeft = 0;
    nodeKey = '';
    lazy = false;
    iconClass = '';
    indent = 0;
    accordion = false;
    autoExpandParent = true;
    renderAfterExpand = false;
    filterText = '';
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
                '[el-select-department-tree] props `value` must be Array if use multiple!'
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
    
    @Watch('filterText')
    onTextFilter(val){
        const tree =  this.$refs.elTree as any;
        tree.filter(val);
    }

    filterNode(value, data) {
        if (!value) return true;
        return data.Name.toLowerCase().indexOf(value.toLowerCase()) !== -1;
    }

    getPopperLocate(){
        const popper = document.getElementsByClassName('el-select-department-tree__popover');
        const visiblePopperArr = [];
        for (let i = 0; i < popper.length; i++) {
            if (!popper[i].getAttribute('aria-hidden')) {
                visiblePopperArr.push(popper[i]);
            } else if (popper[i].getAttribute('aria-hidden') === 'false') {
                visiblePopperArr.push(popper[i]);
            }
        }
        let visiblePopper = visiblePopperArr[0];
        if(visiblePopperArr.length > 1){
            visiblePopper = visiblePopperArr[1];
        }

        const rect = visiblePopper.getBoundingClientRect();
        if(rect.top > 0){
            this.popperTop = rect.top;
        }
        if(rect.left > 0){
            this.popperLeft = rect.left;
        }
        const customeTreeNodeTooltip = document.getElementsByClassName('custom-tree-node__tooltip');
        for (let i = 0; i < customeTreeNodeTooltip.length; i++) {
            (customeTreeNodeTooltip[i] as HTMLElement).style.top = (this.popperTop - 10).toString() + "px";
            (customeTreeNodeTooltip[i] as HTMLElement).style.left = this.popperLeft.toString() + "px";
            (customeTreeNodeTooltip[i] as HTMLElement).style.width = "fit-content";
        }
    }

    renderContent(h, { node, data, store }) {
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
        return h('span', {attrs: { style: 'custom-tree-node' }}, 
            [h('i', {attrs: { style: 'margin-right:5px', class: icon }}, null), h('span', {attrs: { class: 'custom-tree-node__data' }}, data.Name), 
            h('span', {attrs: { class: 'custom-tree-node__tooltip' }}, data.Name)]);
    }
    valueChangeWithParentSelected(value, node, parentSelectedValue, parentSelectedNode) {
        if(this.isSelectParent){
            this.parentSelectedValue = parentSelectedValue;
            this.parentSelectedNode = parentSelectedNode;
            let allValue = value.concat(parentSelectedValue);
            allValue = allValue.filter(x => x >= 0);
            const allNode = node.concat(parentSelectedNode);
            this.$emit('change', allValue, allNode);
            if(this.multiple){
                if(!Array.isArray(value)){
                    const arrValue = [];
                    arrValue.push(allValue);
                    this.$emit('value', arrValue);
                }else{
                    this.$emit('value', allValue);
                }
            }else{
                this.$emit('value', value);
            }
        }else{
            this.$emit('change', value, node);
            if(this.multiple){
                if(!Array.isArray(value)){
                    const arrValue = [];
                    arrValue.push(value);
                    this.$emit('value', arrValue);
                }else{
                    this.$emit('value', value);
                }
            }else{
                this.$emit('value', value);
            }
        }
    }
    valueChange(value, node) {
        this.$emit('change', value, node);
        if(this.multiple){
            if(!Array.isArray(value)){
                const arrValue = [];
                arrValue.push(value);
                this.$emit('value', arrValue);
            }else{
                this.$emit('value', value);
            }
        }else{
            this.$emit('value', value);
        }
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
    // 触发滚动条的重置
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
            if (value !== this.value && value >= 0) {
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
        this.setMultipleSelectedLabelByKeys(keys);
    }
    handleCheck(node, checked) {
        // const valueCheckedNodes = checked.checkedNodes.filter(x => x.ListChildrent == null || x.ListChildrent == undefined 
        //     || x.ListChildrent != null && x.ListChildrent.length == 0);
        const valueCheckedNodes = checked.checkedNodes;
        const valueCheckedKeys = checked.checkedKeys.filter(x => valueCheckedNodes.some(y => y.ID == x));

        if(this.latestKeys == null || this.latestKeys == undefined){
            this.latestKeys = [];
        }
        if(this.latestNodes == null || this.latestNodes == undefined){
            this.latestNodes = [];
        }
        if(!this.isEqual(this.latestKeys, valueCheckedKeys) || !this.isEqual(this.latestNodes, valueCheckedNodes)){
            this.valueChangeWithParentSelected(valueCheckedKeys, valueCheckedNodes, checked.halfCheckedKeys, checked.halfCheckedNodes);
            this.setMultipleSelectedLabelByKeys(valueCheckedKeys);
            this.latestKeys = valueCheckedKeys;
            this.latestNodes = valueCheckedNodes;
        }
    }
    
    setSelected() {
        this.$nextTick(() => {
            const elTree = (this.$refs.elTree as any);
            if (this.multiple) {
                // if (this.value && this.value.length) {
                //     elTree.setCheckedKeys(this.value);
                // }
                if(this.value == null || this.value == undefined || this.value != null && this.value.length == 0){
                    this.latestKeys = [];
                    elTree.setCheckedKeys([]);
                }else{
                    // if(this.parentSelectedValue && this.parentSelectedValue.length > 0){
                    //     this.value = this.value.filter(x => !this.parentSelectedValue(y => y == x));
                    // }
                    elTree.setCheckedKeys(this.value);
                    const selectedNodes = elTree.getCheckedNodes(!this.checkStrictly).filter(x => this.value.includes(x.ID));
                    elTree.setCheckedNodes(selectedNodes)
                }
                this.setMultipleSelectedLabelByKeys(this.value);
            } else {
                if (this.value != null && this.value != undefined) {
                    // if(this.parentSelectedValue && this.parentSelectedValue.length > 0){
                    //     this.value = this.value.filter(x => !this.parentSelectedValue(y => y == x));
                    // }
                    const selectedNode = elTree.getNode(this.value);
                    this.selectedLabel = selectedNode
                        ? selectedNode.data[this.propsLabel]
                        : '';
                }
                else {
                    this.selectedLabel = '';
                }
            }
        });
    }

    setMultipleSelectedLabelByKeys(keys: any) {
        if(this.fixedLabel && this.fixedLabel.length > 0)
        {
            this.selectedLabel = this.fixedLabel;
            return;
        }
        const elTree = (this.$refs.elTree as any);
        const selectedNodes = elTree.getCheckedNodes(!this.checkStrictly);
        const selectedKeys = elTree.getCheckedKeys(!this.checkStrictly);
        // selectedNodes = selectedNodes.filter(x => this.value.includes(x.ID));
        if(this.parentSelectedValue && this.parentSelectedValue.length && keys){
            keys = keys.concat(this.parentSelectedValue);
        }
        if(selectedNodes == null || selectedNodes == undefined || keys == null || keys == undefined 
            || (keys != null && keys.length == 0) || selectedKeys == null || selectedKeys == undefined
            || (selectedKeys != null && selectedKeys.length == 0)){
            this.latestNodes = [];
            this.selectedLabel = "";
        }else{
            if(keys.length != 0 && keys.length != selectedNodes.length){
                let labelArr = [];
                labelArr = this.filterLabelMultiNode(this.data, keys);
                this.selectedLabel = labelArr.join(',');
            }else{
                this.selectedLabel = selectedNodes
                .map((item) => item[this.propsLabel])
                .join(',');
            }
        }
    }

    setMultipleSelectedLabel() {
        const elTree = (this.$refs.elTree as any);
        const selectedNodes = elTree.getCheckedNodes(!this.checkStrictly);
        // selectedNodes = selectedNodes.filter(x => this.value.includes(x.ID));
        if(selectedNodes == null || selectedNodes == undefined || this.value == null || this.value == undefined 
            || this.value != null && this.value.length == 0){
            this.latestNodes = [];
            this.selectedLabel = "";
        }else{
            if(this.value.length < selectedNodes.length){
                let labelArr = [];
                labelArr = this.filterLabelMultiNode(this.data, this.value);
                this.selectedLabel = labelArr.join(',');
            }else{
                this.selectedLabel = selectedNodes
                .map((item) => item[this.propsLabel])
                .join(',');
            }
        }
    }
    filterLabelMultiNode(data, value){
        let labelArr = [];
        
        data.forEach(x => {
            if(value.includes(x.ID) || value.includes(x.Index)){
                labelArr.push(x.Name);
            }
            if(x.ListChildrent != null && x.ListChildrent.length > 0){
                const childLabelArr = this.filterLabelMultiNode(x.ListChildrent, value);
                labelArr = labelArr.concat(childLabelArr);
            }
        });
        return labelArr;
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
        if(this.fixedLabel && this.fixedLabel.length > 0)
        {
            this.selectedLabel = this.fixedLabel;
            return;
        }
        this.setSelected();
        // trigger parent `el-form-item` validate event
        // this.dispatch('ElFormItem', 'el.form.change');
    }
    @Watch('data') handlerDataChange() {
        this.setSelected();
    }

    isEqual(a, b) {
        return a.length == b.length && // same length and
            a.every( // every element in a
                e1 => b.some( // has a match in b
                    e2 => e1.book == e2.book && e1.price == e2.price
                )
            )
    }
}