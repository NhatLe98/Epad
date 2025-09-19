import { Component, Vue } from 'vue-property-decorator';
import HeaderComponent from "@/components/home/header-component/header-component.vue";

@Component({
  name: 'non-privilege',
  components: { HeaderComponent }
})
export default class NonPrivilege extends Vue {
}
