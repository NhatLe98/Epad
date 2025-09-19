import { Component, Vue } from 'vue-property-decorator';
import HeaderComponent from "@/components/home/header-component/header-component.vue";

@Component({
  name: 'page-not-found',
  components: { HeaderComponent }
})
export default class PageNotFound extends Vue {
}
