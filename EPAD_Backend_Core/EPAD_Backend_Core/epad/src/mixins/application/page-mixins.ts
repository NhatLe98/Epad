import { Component, Prop, Vue, Watch } from "vue-property-decorator";
import { Notification } from "element-ui";
import request from "@/constant/request";
let req = new request();
export type NotificationPosition =
  | "top-left"
  | "top-right"
  | "bottom-left"
  | "bottom-right";
@Component
export class PageBase extends Vue {
  idleTime = 1 * 60 * 100000;
  isLoading = false;
  fullPage = true;
  brcolor = "#35495e";
  color = "white";
  loader = "bars";
  firebaseApp = null;
  firestore = null;
  usingSSO = "";
  checkIdle() {
    var router = this.$router;
    var ctx = this;
    this.onInactive(this.idleTime, function () {
      ctx.notify(
        "Thông báo",
        "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!",
        "i",
        "tr"
      );
      self.localStorage.removeItem("usr");
      self.localStorage.removeItem("pwd");
      self.localStorage.removeItem("access_token");
      self.localStorage.removeItem("masterEmployeeFilter");
      router.push({ name: "login" }).catch(err => { });
    });
    var token = self.localStorage.getItem("access_token");
    if (token == null && this.$router.currentRoute.name !== "login-redirect") {
    Misc.readFileAsync('static/variables/app.host.json').then(x => {
      this.usingSSO = x.UsingSSO;
      if(!this.usingSSO){
        router.push({ name: "login" }).catch(err => { });
      }
      });
    }
  }
  onInactive(ms: any, cb: any) {
    var wait = setTimeout(cb, ms);
    var router = this.$router;
    document.onmousemove = document.onmousedown = document.onmouseup = document.onkeydown = document.onkeyup = document.onfocus = function () {
      clearTimeout(wait);
      if (router.currentRoute.name === "login") {
        return;
      }
      wait = setTimeout(cb, ms);
    };

  }

  notify(title: string, message: string, type: string, position: string) {
    var pos: NotificationPosition = "top-right";
    switch (position) {
      case "top-left":
      case "topleft":
      case "tl":
        pos = "top-left";
        break;
      case "top-right":
      case "topright":
      case "tr":
        pos = "top-right";
        break;
      case "bottom-left":
      case "bottomleft":
      case "bl":
        pos = "bottom-left";
        break;
      case "bottom-right":
      case "bottomright":
      case "br":
        pos = "bottom-right";
        break;
      default:
        pos = "top-right";
    }
    var opt = {
      title: title,
      message: message,
      position: pos
    };
    switch (type) {
      case "info":
      case "i":
        Notification.info(opt);
        break;
      case "success":
      case "s":
        Notification.success(opt);
        break;
      case "warn":
      case "w":
        Notification.warning(opt);
        break;
      case "error":
      case "e":
        Notification.error(opt);
        break;
      default:
        Notification.info(opt);
    }
  }

  // getAjax(url: string, callback: any) {
  //   req.getAjax(url, callback);
  // }
  // postAjax(url: string, callback: any) {
  //   req.postAjax(url, callback);
  // }
  // getXMLHttp(url: string, callback: any) {
  //   req.getXMLHttp(url, callback);
  // }
}
