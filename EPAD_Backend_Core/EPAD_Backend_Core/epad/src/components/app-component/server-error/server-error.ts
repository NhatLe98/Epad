import { Component, Prop, Vue } from 'vue-property-decorator';

@Component({
  name: 'server-error',
  components: {}
})
export default class ServerError extends Vue {
  @Prop() error;
  waitCount = 6;
  confirmButtonText = '';
  interval: NodeJS.Timeout = null;

  mounted() {
    this.setInterval();
  }

  setInterval() {
    this.interval = setInterval(() => {
      if (this.waitCount > 0) {
        this.waitCount--;
        this.confirmButtonText = `${this.waitCount}s`;
      } else {
        clearInterval(this.interval);
        this.confirmButtonText = `OK`;
      }
    }, 1000);
  }

  closeDialog() {
    this.$store.commit('Misc/closeBrokenConnect');
  }
}
