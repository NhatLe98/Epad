import { Component, Vue, Prop, PropSync } from "vue-property-decorator";

const MultiStateComponent = () => import("@/components/app-component/multi-state-component/multi-state-component.vue");

interface IStateModel {
  type: string;
  name: string;
}

@Component({
  name: 'matrix',
  components: {MultiStateComponent}
})
export default class MatrixComponent extends Vue {
  @PropSync('YList') YList: IMatrixList;
  @PropSync('XList') XList: IMatrixColumn;
  @PropSync('isLoading') loading: boolean;
  checked = true;
  indeterminate: boolean = true;
  @Prop({default: ''}) matrixLabel: string;

}
