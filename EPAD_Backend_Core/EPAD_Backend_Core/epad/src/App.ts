import { Component, Vue, Prop } from "vue-property-decorator";
import { isNullOrUndefined } from "util";
import { Getter, State } from 'vuex-class';
import ServerError from '@/components/app-component/server-error/server-error.vue';
import { UPDATE_UI } from "@/$core/config";
import { UI_NAME } from "@/$core/config";

if(UPDATE_UI == 'true' && UI_NAME.trim().length > 0){
  import('@/assets/css/updateUI.scss');
  // require('@/assets/css/updateUI.scss');
}

@Component({
  name: "app",
  components: { ServerError }
})
export default class App extends Vue {
  @State('$isLoading', { namespace: 'Misc' }) isLoading;
  @Getter('showBrokenConnect', { namespace: 'Misc' }) showBroken;
  authenticated = false;

  created() {
    if(isNullOrUndefined(localStorage.getItem('lang'))) {
      this.$i18n.locale = "vi"
      localStorage.setItem("lang", "vi")
    }
    else {
      this.$i18n.locale = localStorage.getItem("lang")
    }
  }
  mounted() {
    const tk = localStorage.getItem("access_token");
    if (window.location.pathname === '/resetpassword') {
      let a = window.location.href
      let url = new URL(a);
      let paramEmail = url.searchParams.get("email")
      let paramCode = url.searchParams.get("code")
      this.$router.push({ name: "login", params: { email:  paramEmail, code: paramCode} }).catch(err => {});
    }
    else if(Misc.isEmpty(tk) && (window.location.pathname == '/login-redirect' ||  window.location.pathname == '/login-callback' 
    || window.location.pathname == '/signout-oidc')){
  
    }
    else if (Misc.isEmpty(tk) && window.location.pathname !== '/activate') {
      this.$router.push({ name: "login" }).catch(err => {});
    }
    window.onpopstate = event => {
      if (
        window.localStorage.getItem("access_token") !== null &&
        this.$route.path == "/login"
      ) {
        this.$router.push("/home"); // redirect to home, for example
      }
    }
    Misc.readFileAsync('static/variables/color.json').then(x => {
      const rootElement = document.documentElement;
      if(UPDATE_UI == 'true' && UI_NAME && UI_NAME.trim().length > 0){
        // Vue.use(require("../src/assets/css/updateUI.scss"));

        // const file = document.createElement('link');
        // file.rel = 'stylesheet';
        // file.href = "../src/assets/css/updateUI.scss";
        // document.head.appendChild(file);
        if(!UI_NAME || UI_NAME.trim().length == 0){
          this.$nextTick(() => {
            if(x.topSideBar && x.topSideBar != ''){
              rootElement.style.setProperty('--header-color', x.topSideBar);
            }
            if(x.colorFont && x.colorFont != ''){
              rootElement.style.setProperty('--header-text-color', x.colorFont);
            }
            if(x.leftSideBar && x.leftSideBar != ''){
              rootElement.style.setProperty('--menu-color', x.leftSideBar);
            }
            if(x.masterBackground && x.masterBackground != ''){
              rootElement.style.setProperty('--master-background', x.masterBackground);
            }
            if(x.background && x.background != ''){
              rootElement.style.setProperty('--background', x.background);
            }
            if(x.colorBackground && x.colorBackground != ''){
              rootElement.style.setProperty('--background-color', x.colorBackground);
            }
            if(x.colorItemBackground && x.colorItemBackground != ''){
              rootElement.style.setProperty('--item-background-color', x.colorItemBackground);
            }
  
            if(x.dialogBackground && x.dialogBackground != ''){
              rootElement.style.setProperty('--dialog-background', x.dialogBackground);
            }
            if(x.colorDialogBackground && x.colorDialogBackground != ''){
              rootElement.style.setProperty('--dialog-background-color', x.colorDialogBackground);
            }
            if(x.colorDialogHeaderBackground && x.colorDialogHeaderBackground != ''){
              rootElement.style.setProperty('--dialog-header-background-color', x.colorDialogHeaderBackground);
            }
  
            if(x.treeBackground && x.treeBackground != ''){
              rootElement.style.setProperty('--tree-background', x.treeBackground);
            }
            if(x.colorTreeBackground && x.colorTreeBackground != ''){
              rootElement.style.setProperty('--tree-background-color', x.colorTreeBackground);
            }
  
            if(x.dropdownBackground && x.dropdownBackground != ''){
              rootElement.style.setProperty('--dropdown-background', x.dropdownBackground);
            }
            if(x.colorDropdownBackground && x.colorDropdownBackground != ''){
              rootElement.style.setProperty('--dropdown-background-color', x.colorDropdownBackground);
            }
            if(x.colorCollapseItemBackground && x.colorCollapseItemBackground != ''){
              rootElement.style.setProperty('--collapse-item-background-color', x.colorCollapseItemBackground);
            }
            
            if(x.colorText && x.colorText != ''){
              rootElement.style.setProperty('--text-color', x.colorText);
            }
            if(x.colorScheme && x.colorScheme != ''){
              rootElement.style.setProperty('--color-scheme', x.colorScheme);
            }
            if(x.colorActive && x.colorActive != ''){
              rootElement.style.setProperty('--active-color', x.colorActive); 
            }
            
            if(x.colorIcon && x.colorIcon != ''){
              rootElement.style.setProperty('--icon-color', x.colorIcon); 
            }
            if(x.colorMenuTextActive && x.colorMenuTextActive != ''){
              rootElement.style.setProperty('--menu-text-active-color', x.colorMenuTextActive); 
            }
            if(x.colorPrimaryButtonText && x.colorPrimaryButtonText != ''){
              rootElement.style.setProperty('--primary-button-text', x.colorPrimaryButtonText); 
            }
            if(x.primaryButtonBackground && x.primaryButtonBackground != ''){
              rootElement.style.setProperty('--primary-button-background', x.primaryButtonBackground); 
            }
            if(x.dashboardBackground && x.dashboardBackground != ''){
              rootElement.style.setProperty('--dashboard-background', x.dashboardBackground); 
            }
            if(x.colorDashboardBackground && x.colorDashboardBackground != ''){
              rootElement.style.setProperty('--dashboard-background-color', x.colorDashboardBackground); 
            }
            if(x.colorDashboardItemBackground && x.colorDashboardItemBackground != ''){
              rootElement.style.setProperty('--dashboard-item-background-color', x.colorDashboardItemBackground); 
            }
      
            if(x.colorSelectDropdownItem && x.colorSelectDropdownItem != ''){
              rootElement.style.setProperty('--select-dropdown-item-color', x.colorSelectDropdownItem);
            }
            if(x.colorMenuText && x.colorMenuText != ''){
              rootElement.style.setProperty('--menu-text-color', x.colorMenuText);
            }
            if(x.colorPlaceholder && x.colorPlaceholder != ''){
              rootElement.style.setProperty('--placeholder-color', x.colorPlaceholder);
            }
            if(x.colorTableBorder && x.colorTableBorder != ''){
              rootElement.style.setProperty('--table-border-color', x.colorTableBorder);
            }
            if(x.colorTableHeaderText && x.colorTableHeaderText != ''){
              rootElement.style.setProperty('--table-header-text-color', x.colorTableHeaderText);
            }
            if(x.colorTableBodyText && x.colorTableBodyText != ''){
              rootElement.style.setProperty('--table-body-text-color', x.colorTableBodyText);
            }
            if(x.colorTableHeaderCaret && x.colorTableHeaderCaret != ''){
              rootElement.style.setProperty('--table-header-caret-color', x.colorTableHeaderCaret);
            }
      
            if(x.colorIconFilter && x.colorIconFilter != ''){
              rootElement.style.setProperty('--icon-filter-color', x.colorIconFilter);
      
              // console.log("force color");
              this.$nextTick(() => {
                this.forceChangeColorIcon(x.colorIconFilter);
              });
            }
          });
        }else{
          this.$nextTick(() => {
            if(x.ColorThemes[UI_NAME].topSideBar && x.ColorThemes[UI_NAME].topSideBar != ''){
              rootElement.style.setProperty('--header-color', x.ColorThemes[UI_NAME].topSideBar);
            }
            if(x.ColorThemes[UI_NAME].colorFont && x.ColorThemes[UI_NAME].colorFont != ''){
              rootElement.style.setProperty('--header-text-color', x.ColorThemes[UI_NAME].colorFont);
            }
            if(x.ColorThemes[UI_NAME].leftSideBar && x.ColorThemes[UI_NAME].leftSideBar != ''){
              rootElement.style.setProperty('--menu-color', x.ColorThemes[UI_NAME].leftSideBar);
            }
            if(x.ColorThemes[UI_NAME].masterBackground && x.ColorThemes[UI_NAME].masterBackground != ''){
              rootElement.style.setProperty('--master-background', x.ColorThemes[UI_NAME].masterBackground);
            }
            if(x.ColorThemes[UI_NAME].background && x.ColorThemes[UI_NAME].background != ''){
              rootElement.style.setProperty('--background', x.ColorThemes[UI_NAME].background);
            }
            if(x.ColorThemes[UI_NAME].colorBackground && x.ColorThemes[UI_NAME].colorBackground != ''){
              rootElement.style.setProperty('--background-color', x.ColorThemes[UI_NAME].colorBackground);
            }
            if(x.ColorThemes[UI_NAME].colorItemBackground && x.ColorThemes[UI_NAME].colorItemBackground != ''){
              rootElement.style.setProperty('--item-background-color', x.ColorThemes[UI_NAME].colorItemBackground);
            }
  
            if(x.ColorThemes[UI_NAME].dialogBackground && x.ColorThemes[UI_NAME].dialogBackground != ''){
              rootElement.style.setProperty('--dialog-background', x.ColorThemes[UI_NAME].dialogBackground);
            }
            if(x.ColorThemes[UI_NAME].colorDialogBackground && x.ColorThemes[UI_NAME].colorDialogBackground != ''){
              rootElement.style.setProperty('--dialog-background-color', x.ColorThemes[UI_NAME].colorDialogBackground);
            }
            if(x.ColorThemes[UI_NAME].colorDialogHeaderBackground && x.ColorThemes[UI_NAME].colorDialogHeaderBackground != ''){
              rootElement.style.setProperty('--dialog-header-background-color', x.ColorThemes[UI_NAME].colorDialogHeaderBackground);
            }
  
            if(x.ColorThemes[UI_NAME].treeBackground && x.ColorThemes[UI_NAME].treeBackground != ''){
              rootElement.style.setProperty('--tree-background', x.ColorThemes[UI_NAME].treeBackground);
            }
            if(x.ColorThemes[UI_NAME].colorTreeBackground && x.ColorThemes[UI_NAME].colorTreeBackground != ''){
              rootElement.style.setProperty('--tree-background-color', x.ColorThemes[UI_NAME].colorTreeBackground);
            }
  
            if(x.ColorThemes[UI_NAME].dropdownBackground && x.ColorThemes[UI_NAME].dropdownBackground != ''){
              rootElement.style.setProperty('--dropdown-background', x.ColorThemes[UI_NAME].dropdownBackground);
            }
            if(x.ColorThemes[UI_NAME].colorDropdownBackground && x.ColorThemes[UI_NAME].colorDropdownBackground != ''){
              rootElement.style.setProperty('--dropdown-background-color', x.ColorThemes[UI_NAME].colorDropdownBackground);
            }
            if(x.ColorThemes[UI_NAME].colorCollapseItemBackground && x.ColorThemes[UI_NAME].colorCollapseItemBackground != ''){
              rootElement.style.setProperty('--collapse-item-background-color', x.ColorThemes[UI_NAME].colorCollapseItemBackground);
            }
            
            if(x.ColorThemes[UI_NAME].colorText && x.ColorThemes[UI_NAME].colorText != ''){
              rootElement.style.setProperty('--text-color', x.ColorThemes[UI_NAME].colorText);
            }
            if(x.ColorThemes[UI_NAME].colorScheme && x.ColorThemes[UI_NAME].colorScheme != ''){
              rootElement.style.setProperty('--color-scheme', x.ColorThemes[UI_NAME].colorScheme);
            }
            if(x.ColorThemes[UI_NAME].colorActive && x.ColorThemes[UI_NAME].colorActive != ''){
              rootElement.style.setProperty('--active-color', x.ColorThemes[UI_NAME].colorActive); 
            }
            
            if(x.ColorThemes[UI_NAME].colorIcon && x.ColorThemes[UI_NAME].colorIcon != ''){
              rootElement.style.setProperty('--icon-color', x.ColorThemes[UI_NAME].colorIcon); 
            }
            if(x.ColorThemes[UI_NAME].colorMenuTextActive && x.ColorThemes[UI_NAME].colorMenuTextActive != ''){
              rootElement.style.setProperty('--menu-text-active-color', x.ColorThemes[UI_NAME].colorMenuTextActive); 
            }
            if(x.ColorThemes[UI_NAME].colorPrimaryButtonText && x.ColorThemes[UI_NAME].colorPrimaryButtonText != ''){
              rootElement.style.setProperty('--primary-button-text', x.ColorThemes[UI_NAME].colorPrimaryButtonText); 
            }
            if(x.ColorThemes[UI_NAME].primaryButtonBackground && x.ColorThemes[UI_NAME].primaryButtonBackground != ''){
              rootElement.style.setProperty('--primary-button-background', x.ColorThemes[UI_NAME].primaryButtonBackground); 
            }
            if(x.ColorThemes[UI_NAME].dashboardBackground && x.ColorThemes[UI_NAME].dashboardBackground != ''){
              rootElement.style.setProperty('--dashboard-background', x.ColorThemes[UI_NAME].dashboardBackground); 
            }
            if(x.ColorThemes[UI_NAME].colorDashboardBackground && x.ColorThemes[UI_NAME].colorDashboardBackground != ''){
              rootElement.style.setProperty('--dashboard-background-color', x.ColorThemes[UI_NAME].colorDashboardBackground); 
            }
            if(x.ColorThemes[UI_NAME].colorDashboardItemBackground && x.ColorThemes[UI_NAME].colorDashboardItemBackground != ''){
              rootElement.style.setProperty('--dashboard-item-background-color', x.ColorThemes[UI_NAME].colorDashboardItemBackground); 
            }
      
            if(x.ColorThemes[UI_NAME].colorSelectDropdownItem && x.ColorThemes[UI_NAME].colorSelectDropdownItem != ''){
              rootElement.style.setProperty('--select-dropdown-item-color', x.ColorThemes[UI_NAME].colorSelectDropdownItem);
            }
            if(x.ColorThemes[UI_NAME].colorMenuText && x.ColorThemes[UI_NAME].colorMenuText != ''){
              rootElement.style.setProperty('--menu-text-color', x.ColorThemes[UI_NAME].colorMenuText);
            }
            if(x.ColorThemes[UI_NAME].colorPlaceholder && x.ColorThemes[UI_NAME].colorPlaceholder != ''){
              rootElement.style.setProperty('--placeholder-color', x.ColorThemes[UI_NAME].colorPlaceholder);
            }
            if(x.ColorThemes[UI_NAME].colorTableBorder && x.ColorThemes[UI_NAME].colorTableBorder != ''){
              rootElement.style.setProperty('--table-border-color', x.ColorThemes[UI_NAME].colorTableBorder);
            }
            if(x.ColorThemes[UI_NAME].colorTableHeaderText && x.ColorThemes[UI_NAME].colorTableHeaderText != ''){
              rootElement.style.setProperty('--table-header-text-color', x.ColorThemes[UI_NAME].colorTableHeaderText);
            }
            if(x.ColorThemes[UI_NAME].colorTableBodyText && x.ColorThemes[UI_NAME].colorTableBodyText != ''){
              rootElement.style.setProperty('--table-body-text-color', x.ColorThemes[UI_NAME].colorTableBodyText);
            }
            if(x.ColorThemes[UI_NAME].colorTableHeaderCaret && x.ColorThemes[UI_NAME].colorTableHeaderCaret != ''){
              rootElement.style.setProperty('--table-header-caret-color', x.ColorThemes[UI_NAME].colorTableHeaderCaret);
            }
      
            if(x.ColorThemes[UI_NAME].colorIconFilter && x.ColorThemes[UI_NAME].colorIconFilter != ''){
              rootElement.style.setProperty('--icon-filter-color', x.ColorThemes[UI_NAME].colorIconFilter);
      
              // console.log("force color");
              this.$nextTick(() => {
                this.forceChangeColorIcon(x.ColorThemes[UI_NAME].colorIconFilter);
              });
            }
          });
        }
      }
    });
  }
  forceChangeColorIcon(colorFilterIcon){
    const rgb = hexToRgb(colorFilterIcon);
    if (rgb.length !== 3) {
      alert('Invalid format!');
      return;
    }

    const color = new Color(rgb[0], rgb[1], rgb[2]);
    const solver = new Solver(color);
    const result = solver.solve();

    // or document.querySelector('html');
    const rootElement = document.documentElement;
    rootElement.style.setProperty('--icon-filter', result.filter);
  }
  setAuthenticated(status) {
    this.authenticated = status;
  }
  logout() {
    this.authenticated = false;
  }
}

class Color {
  r;
  g;
  b;
  constructor(r, g, b) {
    this.set(r, g, b);
  }
  
  toString() {
    return `rgb(${Math.round(this.r)}, ${Math.round(this.g)}, ${Math.round(this.b)})`;
  }

  set(r, g, b) {
    this.r = this.clamp(r);
    this.g = this.clamp(g);
    this.b = this.clamp(b);
  }

  hueRotate(angle = 0) {
    angle = angle / 180 * Math.PI;
    const sin = Math.sin(angle);
    const cos = Math.cos(angle);

    this.multiply([
      0.213 + cos * 0.787 - sin * 0.213,
      0.715 - cos * 0.715 - sin * 0.715,
      0.072 - cos * 0.072 + sin * 0.928,
      0.213 - cos * 0.213 + sin * 0.143,
      0.715 + cos * 0.285 + sin * 0.140,
      0.072 - cos * 0.072 - sin * 0.283,
      0.213 - cos * 0.213 - sin * 0.787,
      0.715 - cos * 0.715 + sin * 0.715,
      0.072 + cos * 0.928 + sin * 0.072,
    ]);
  }

  grayscale(value = 1) {
    this.multiply([
      0.2126 + 0.7874 * (1 - value),
      0.7152 - 0.7152 * (1 - value),
      0.0722 - 0.0722 * (1 - value),
      0.2126 - 0.2126 * (1 - value),
      0.7152 + 0.2848 * (1 - value),
      0.0722 - 0.0722 * (1 - value),
      0.2126 - 0.2126 * (1 - value),
      0.7152 - 0.7152 * (1 - value),
      0.0722 + 0.9278 * (1 - value),
    ]);
  }

  sepia(value = 1) {
    this.multiply([
      0.393 + 0.607 * (1 - value),
      0.769 - 0.769 * (1 - value),
      0.189 - 0.189 * (1 - value),
      0.349 - 0.349 * (1 - value),
      0.686 + 0.314 * (1 - value),
      0.168 - 0.168 * (1 - value),
      0.272 - 0.272 * (1 - value),
      0.534 - 0.534 * (1 - value),
      0.131 + 0.869 * (1 - value),
    ]);
  }

  saturate(value = 1) {
    this.multiply([
      0.213 + 0.787 * value,
      0.715 - 0.715 * value,
      0.072 - 0.072 * value,
      0.213 - 0.213 * value,
      0.715 + 0.285 * value,
      0.072 - 0.072 * value,
      0.213 - 0.213 * value,
      0.715 - 0.715 * value,
      0.072 + 0.928 * value,
    ]);
  }

  multiply(matrix) {
    const newR = this.clamp(this.r * matrix[0] + this.g * matrix[1] + this.b * matrix[2]);
    const newG = this.clamp(this.r * matrix[3] + this.g * matrix[4] + this.b * matrix[5]);
    const newB = this.clamp(this.r * matrix[6] + this.g * matrix[7] + this.b * matrix[8]);
    this.r = newR;
    this.g = newG;
    this.b = newB;
  }

  brightness(value = 1) {
    this.linear(value);
  }
  contrast(value = 1) {
    this.linear(value, -(0.5 * value) + 0.5);
  }

  linear(slope = 1, intercept = 0) {
    this.r = this.clamp(this.r * slope + intercept * 255);
    this.g = this.clamp(this.g * slope + intercept * 255);
    this.b = this.clamp(this.b * slope + intercept * 255);
  }

  invert(value = 1) {
    this.r = this.clamp((value + this.r / 255 * (1 - 2 * value)) * 255);
    this.g = this.clamp((value + this.g / 255 * (1 - 2 * value)) * 255);
    this.b = this.clamp((value + this.b / 255 * (1 - 2 * value)) * 255);
  }

  hsl() {
    // Code taken from https://stackoverflow.com/a/9493060/2688027, licensed under CC BY-SA.
    const r = this.r / 255;
    const g = this.g / 255;
    const b = this.b / 255;
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;

    if (max === min) {
      h = s = 0;
    } else {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r:
          h = (g - b) / d + (g < b ? 6 : 0);
          break;

        case g:
          h = (b - r) / d + 2;
          break;

        case b:
          h = (r - g) / d + 4;
          break;
      }
      h /= 6;
    }

    return {
      h: h * 100,
      s: s * 100,
      l: l * 100,
    };
  }

  clamp(value) {
    if (value > 255) {
      value = 255;
    } else if (value < 0) {
      value = 0;
    }
    return value;
  }
}

class Solver {
  target;
  targetHSL;
  reusedColor;
  // constructor(target, baseColor) {
  constructor(target) {
    this.target = target;
    this.targetHSL = target.hsl();
    this.reusedColor = new Color(0, 0, 0);
  }

  solve() {
    const result = this.solveNarrow(this.solveWide());
    return {
      values: result.values,
      loss: result.loss,
      filter: this.cssValue(result.values),
    };
  }

  solveWide() {
    const A = 5;
    const c = 15;
    const a = [60, 180, 18000, 600, 1.2, 1.2];

    let best = { loss: Infinity };
    for (let i = 0; best.loss > 25 && i < 3; i++) {
      const initial = [50, 20, 3750, 50, 100, 100];
      const result = this.spsa(A, a, c, initial, 1000);
      if (result.loss < best.loss) {
        best = result;
      }
    }
    return best;
  }

  solveNarrow(wide) {
    const A = wide.loss;
    const c = 2;
    const A1 = A + 1;
    const a = [0.25 * A1, 0.25 * A1, A1, 0.25 * A1, 0.2 * A1, 0.2 * A1];
    return this.spsa(A, a, c, wide.values, 500);
  }

  spsa(A, a, c, values, iters) {
    const alpha = 1;
    const gamma = 0.16666666666666666;

    let best = null;
    let bestLoss = Infinity;
    const deltas = new Array(6);
    const highArgs = new Array(6);
    const lowArgs = new Array(6);

    for (let k = 0; k < iters; k++) {
      const ck = c / Math.pow(k + 1, gamma);
      for (let i = 0; i < 6; i++) {
        deltas[i] = Math.random() > 0.5 ? 1 : -1;
        highArgs[i] = values[i] + ck * deltas[i];
        lowArgs[i] = values[i] - ck * deltas[i];
      }

      const lossDiff = this.loss(highArgs) - this.loss(lowArgs);
      for (let i = 0; i < 6; i++) {
        const g = lossDiff / (2 * ck) * deltas[i];
        const ak = a[i] / Math.pow(A + k + 1, alpha);
        values[i] = fix(values[i] - ak * g, i);
      }

      const loss = this.loss(values);
      if (loss < bestLoss) {
        best = values.slice(0);
        bestLoss = loss;
      }
    }
    return { values: best, loss: bestLoss };

    function fix(value, idx) {
      let max = 100;
      if (idx === 2 /* saturate */) {
        max = 7500;
      } else if (idx === 4 /* brightness */ || idx === 5 /* contrast */) {
        max = 200;
      }

      if (idx === 3 /* hue-rotate */) {
        if (value > max) {
          value %= max;
        } else if (value < 0) {
          value = max + value % max;
        }
      } else if (value < 0) {
        value = 0;
      } else if (value > max) {
        value = max;
      }
      return value;
    }
  }

  loss(filters) {
    // Argument is array of percentages.
    const color = this.reusedColor;
    color.set(0, 0, 0);

    color.invert(filters[0] / 100);
    color.sepia(filters[1] / 100);
    color.saturate(filters[2] / 100);
    color.hueRotate(filters[3] * 3.6);
    color.brightness(filters[4] / 100);
    color.contrast(filters[5] / 100);

    const colorHSL = color.hsl();
    return (
      Math.abs(color.r - this.target.r) +
      Math.abs(color.g - this.target.g) +
      Math.abs(color.b - this.target.b) +
      Math.abs(colorHSL.h - this.targetHSL.h) +
      Math.abs(colorHSL.s - this.targetHSL.s) +
      Math.abs(colorHSL.l - this.targetHSL.l)
    );
  }

  css(filters) {
    function fmt(idx, multiplier = 1) {
      return Math.round(filters[idx] * multiplier);
    }
    return `filter: invert(${fmt(0)}%) sepia(${fmt(1)}%) saturate(${fmt(2)}%) hue-rotate(${fmt(3, 3.6)}deg) brightness(${fmt(4)}%) contrast(${fmt(5)}%);`;
  }
  
  cssValue(filters) {
    function fmt(idx, multiplier = 1) {
      return Math.round(filters[idx] * multiplier);
    }
    return `invert(${fmt(0)}%) sepia(${fmt(1)}%) saturate(${fmt(2)}%) hue-rotate(${fmt(3, 3.6)}deg) brightness(${fmt(4)}%) contrast(${fmt(5)}%)`;
  }
}

function hexToRgb(hex) {
  // Expand shorthand form (e.g. "03F") to full form (e.g. "0033FF")
  const shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
  hex = hex.replace(shorthandRegex, (m, r, g, b) => {
    return r + r + g + g + b + b;
  });

  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result
    ? [
      parseInt(result[1], 16),
      parseInt(result[2], 16),
      parseInt(result[3], 16),
    ]
    : null;
}
