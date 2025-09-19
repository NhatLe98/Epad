import { Component, Vue, PropSync, Prop } from "vue-property-decorator";
import { isNullOrUndefined } from 'util';
@Component({
  name: "integrate-log-realtime-config",
  methods: { isNullOrUndefined }
})
export default class IntegrateLogRealTimeConfigComponent extends Vue {
  @PropSync('configModel') config: any;
  focus(x) {
    var theField = eval('this.$refs.' + x)
    theField.focus()
  }
}