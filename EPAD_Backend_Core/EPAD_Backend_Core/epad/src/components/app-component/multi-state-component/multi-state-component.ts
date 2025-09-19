import { Component, Vue, Prop, PropSync, Watch } from "vue-property-decorator";
import { Mutation } from 'vuex-class';

@Component({
  name: 'multi-state',
  components: {}
})
export default class MultiStateComponent extends Vue {
  @PropSync('state') currentState: any;
  @Prop() menuId: string;
  @Prop({ default: false}) hasTooltip: boolean;
  @Prop({ default: () => [{ name: 'None', icon: 'el-icon-close', title: '', type: 'info' }] }) listState: IStateCollection;
  stateTitle: string = '';
  stateIcon: string = '';
  stateType: string = '';
  @Mutation("setAppIsEdit", { namespace: "Application" }) setAppIsEdit;

  @Watch('currentState.role')
  currentStateChange() {
    const ixOfState = this.listState.findIndex(x => x.name === this.currentState.role);
    // console.log(this.currentState, ixOfState, this.listState)
    this.stateTitle = this.listState[ixOfState].title;
    this.stateIcon = this.listState[ixOfState].icon;
    this.stateType = this.listState[ixOfState].type;
  }

  changeState() {
    const ixOfState = this.listState.findIndex(x => x.name === this.currentState.role);
    if(ixOfState === this.listState.length - 1) {
      this.currentState.role = this.listState[0].name;
    }
    else {
      this.currentState.role = this.listState[ixOfState + 1].name;
    }
    this.setAppIsEdit(true);
    this.$emit("changeState", this.menuId, this.currentState);
    // EventBus.$emit('assign_privilege_state_change', this.data);
  }

  beforeMount() {
    const ixOfState = this.listState.findIndex(x => x.name === this.currentState.role);
    this.stateTitle = this.listState[ixOfState].title;
    this.stateIcon = this.listState[ixOfState].icon;
    this.stateType = this.listState[ixOfState].type;
  }
}
