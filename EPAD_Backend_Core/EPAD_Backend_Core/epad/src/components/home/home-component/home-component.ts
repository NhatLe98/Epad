import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import DeviceInfoForHomeComponent from "@/components/home/device-info-component/device-info-for-home-component.vue";
import LiveMachineComponent from "@/components/home/log-monitoring-component/livemachine-component.vue";

@Component({
  name: "home",
  components: { HeaderComponent, DeviceInfoForHomeComponent, LiveMachineComponent }
})
export default class HomeComponent extends Mixins(ComponentBase) {
  DateTime = "";
  mounted() {
    const date = new Date();
    this.DateTime = date.toString();
  }
}
