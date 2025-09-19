import { Component, Prop, Vue, Watch } from "vue-property-decorator";
import { Notification } from "element-ui";
import request from "@/constant/request.ts";
let req = new request();
export type NotificationPosition =
  | "top-left"
  | "top-right"
  | "bottom-left"
  | "bottom-right";
@Component
export default class ComponentBase extends Vue {
  isLoading = false;
  fullPage = true;
  brcolor = "#35495e";
  color = "white";
  loader = "bars";

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
