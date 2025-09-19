<template>
  <div ref="recaptchaContainer"></div>
</template>

<script lang="ts">
import {Component, Prop, Vue} from 'vue-property-decorator'
/**
 * defaultSiteKey returns the default recaptcha key to use,
 * if none is passed as a prop to the component.
 * 
 * This looks in the environment for `VUE_APP_RECAPTCHA_SITE_KEY`.
 * See https://cli.vuejs.org/guide/mode-and-env.html
 */
function defaultSiteKey() {
  return process.env.VUE_APP_RECAPTCHA_SITE_KEY || 'site-key-not-defined'
}
/**
 * loadRecaptcha loads the recatpcha script and invokes your callback
 * then it is ready to be used.
 */
function loadRecaptcha(callback: () => void) {
  let win = window as any
  if (win && !win.grecaptcha) {
    let recaptchaScript = document.createElement('script')
    document.head.appendChild(recaptchaScript)
    recaptchaScript.onload = () => {
      // At this point the recaptcha script has loaded but the
      // code in it as not completed loading. Fortunately there's
      // a "ready" function in there that can tell us when it's done.
      let win = window as any
      //console.log('NOTE: script loaded: ', win.grecaptcha)
      win.grecaptcha.ready(() => {
        callback()
      })
    }
    recaptchaScript.setAttribute('src', 'https://www.google.com/recaptcha/api.js')
  } else {
    callback()
  }
}
/**
 * The Recaptcha2 component can be dropped in anywhere to show a Recaptcha
 * "I'm not a robot" challenge.
 * 
 * Prop (optional) "siteKey": the Recaptcha site key. If not provided, this
 * will use environment setting VUE_APP_RECAPTCHA_SITE_KEY.
 * 
 * Event @change(boolean): invoked when the test "is a human" passes or expires.
 * When the test passes, true is passed. When the test expires, false is passed.
 * 
 * Event @expired(void): invoked when the test has expired. The @change event is
 * sent prior to this.
 * 
 * Event @passed(key): invoked after the test has passed and after @change(true)
 * has been sent. The parameter `key` is the value of the textarea element buried
 * within the recaptcha, which you can also get with:
 * 
 *     document.getElementById('g-recaptcha-response').value
 *
 * (Though that ID would be different if more than one recaptcha had been created.)
 * 
 * Note that they key is not verified by this component, but a component that
 * receives the @passed event could then invoke server-side validation.
 */
@Component
export default class extends Vue {
  @Prop({default: defaultSiteKey()}) private siteKey!: string
  passed: boolean = false
  widgetId: string|undefined = undefined
  get widgetCreated(): boolean {
    return this.widgetId !== undefined
  }
  // mounted() {
  //   loadRecaptcha(() => this.mountRecaptcha())
  // }
  mounted() {
    const recaptchaApi: any = (window as any).grecaptcha
    let container = this.$refs.recaptchaContainer
    this.widgetId = recaptchaApi.render(container, {
      callback: () => {
        // The unique "result" that can be checked on the server-side, if desired.
        // See https://stackoverflow.com/a/45765020/963195
        const result = recaptchaApi.getResponse(this.widgetId)
        this.passed = true
        this.$emit('change', this.passed)
        this.$emit('passed', result) // if caller wants to test signature server-side
      },
      'expired-callback': () => {
        this.passed = false
        this.$emit('change', this.passed)
        this.$emit('expired')
      },
      sitekey: this.siteKey
    })
  }
}
</script>