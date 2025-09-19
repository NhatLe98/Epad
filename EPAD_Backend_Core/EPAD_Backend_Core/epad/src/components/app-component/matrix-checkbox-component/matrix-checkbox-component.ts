import { Component, Vue, Prop, PropSync } from "vue-property-decorator";
const MultiStateComponent = () => import("@/components/app-component/multi-state-component/multi-state-component.vue");

@Component({
  name: 'matrix-checkbox',
  components: {MultiStateComponent}
})
export default class MatrixCheckboxComponent extends Vue {
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