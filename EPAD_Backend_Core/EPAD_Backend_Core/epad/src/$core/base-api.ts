import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from "axios";
import { isNullOrUndefined } from "util";
import OAuthServices from "./oauth-services";
import { API_URL } from "@/$core/config";
// import { store } from '@/store';
import { AppRoute, IgnoreRoute } from "@/router";

export class BaseApi {
  private _Vue: Vue;
  private module: string;
  private api: AxiosInstance;
  private useDefaultErrorHandler: boolean;
  private customHandler: boolean;
  public constructor(_module: string, config?: AxiosRequestConfig) {
    this._Vue = window.VueInstance;
    this.module = _module;
    var isLoading = null
    var usingSSO = '';
    if (isNullOrUndefined(config)) {
      config = {
        baseURL: `${API_URL}/${this.module}/`
      };
    } else if (!config.hasOwnProperty("baseURL")) {
      config.baseURL = `${API_URL}/${this.module}/`;
    }
    this.api = axios.create(config);

    this.api.interceptors.request.use(
      (param: AxiosRequestConfig) => {
        if (param.url !== "SendResetPasswordCode") {
          // isLoading = Loading.service({
          //   fullscreen: true,
          //   lock: true,
          //   text: 'Đang xử lý',
          //   spinner: 'el-icon-loading',
          //   background: 'rgba(0, 0, 0, 0.5)'
          // });
        }

        if (OAuthServices.getToken()) {
          (param.headers.Authorization = `bearer ${OAuthServices.getToken()}`),
            (param.headers["Cache-Control"] = "no-cache");
        }

        const app_form_active = sessionStorage.getItem("app_form_active");
        if (!isNullOrUndefined(app_form_active) && !IgnoreRoute.some(igr => igr === param.url)) {
          param.headers["form-name"] = app_form_active;
        }
        return param;
      },
      err => {
        return Promise.reject(err);
      }
    );

    this.api.interceptors.response.use(
      response => {
        if (!isNullOrUndefined(isLoading)) {
          isLoading.close();
        }
        return response;
      },
      error => {
        this._Vue = window.VueInstance;
        const errorStack = this._Vue.$store.getters['Misc/isErrorStack'];
        console.log('errorStack', errorStack);
        if (!isNullOrUndefined(isLoading)) {
          isLoading.close();
        }

        if (error.response !== null && error.response !== undefined) {
          if (this.customHandler) return Promise.reject(error);
          if (false == errorStack) {
            this._Vue.$store.commit('Misc/onErrorStack');
          }
          else {
            return Promise.reject(error);
          }

          // console.log(`error.response`, error.response)

          switch (error.response.status) {
            case 400:
              window.VueInstance.$apiAlertRequestError(
                null,
                null,
                window.VueInstance.$t("Notify").toString(),
                window.VueInstance.$t(error.response.data).toString()
              );
              break;
            case 401:
              console.log("LOGOUT_BASE", AppRoute.currentRoute.name);
              if (AppRoute.currentRoute.name == "gc-customer-info" || AppRoute.currentRoute.name == "general-monitoring-screen" || AppRoute.currentRoute.name == "customer-monitoring-page"
                || AppRoute.currentRoute.name == "truck-driver-in-monitoring" || AppRoute.currentRoute.name == "truck-driver-out-monitoring"
                || AppRoute.currentRoute.name == "factory-user-monitoring" || AppRoute.currentRoute.name == "attendance-and-evacuation") 
              {
                  console.log("AppRoute.currentRoute.name", AppRoute.currentRoute.name);
                  this._Vue.$store.commit('Misc/offErrorStack')
                  OAuthServices.setToken(null);
              }
              else {
                window.VueInstance.$confirm(
                  window.VueInstance.$t("SessionTimeOut").toString(),
                  window.VueInstance.$t("Notify").toString())
                  .then(() => {
                    localStorage.removeItem("masterEmployeeFilter");
                    this._Vue.$store.commit('Misc/offErrorStack')
                    OAuthServices.setToken(null);
                    Misc.readFileAsync('static/variables/app.host.json').then(x => {
                      usingSSO = x.UsingSSO;
                      if (usingSSO) {
                        window.location.replace(window.location.origin + "/signout-oidc");
                      }
                      else {
                        if (AppRoute.currentRoute.name !== 'login' && AppRoute.currentRoute.name !== 'login-redirect' && AppRoute.currentRoute.name !== 'signout-oidc') {
                          AppRoute.push({
                            path: 'login',
                            query: { redirect: AppRoute.currentRoute.path }
                          });
                        }
                      }
                    });
                  });
              }
              break;
            case 403:
              window.VueInstance.$apiAlertRequestError(
                null,
                null,
                window.VueInstance.$t("Notify").toString(),
                window.VueInstance.$t("MSG_NotPrivilege").toString()
              );
              break;
            case 404:
              window.VueInstance.$apiAlertRequestError(
                null,
                null,
                window.VueInstance.$t("Notify").toString(),
                window.VueInstance.$t(error.response.data).toString()
              );
              break;
            case 409:
              window.VueInstance.$apiAlertRequestError(
                null,
                null,
                window.VueInstance.$t("Notify").toString(),
                window.VueInstance.$t(error.response.data).toString()
              );
              break;
            case 500:
              window.VueInstance.$apiAlertRequestError(
                null,
                null,
                window.VueInstance.$t("Notify").toString(),
                window.VueInstance.$t("MSG_SystemError").toString()
              );
              break;
          }
        } else {
          this._Vue.$store.commit('Misc/brokenConnect');
        }
        return Promise.reject(error);
      }
    );
  }

  public getUri(config?: AxiosRequestConfig): string {
    return this.api.getUri(config);
  }

  public request<T, R = AxiosResponse<T>>(
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.request(config);
  }

  public get<T, R = AxiosResponse<T>>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.get(url, config);
  }

  public put<T, R = AxiosResponse<T>>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.put(url, data, config);
  }

  public post<T, R = AxiosResponse<T>>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.post(url, data, config);
  }

  public delete<T, R = AxiosResponse<T>>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.delete(url, config);
  }

  public addItem<T, R = AxiosResponse<T>>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.post(url, data, config);
  }

  public updateItem<T, R = AxiosResponse<T>>(
    url: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.post(url, data, config).then();
  }
  public getItemsWithPaging<T, R = AxiosResponse<T>>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<R> {
    return this.api.get(url, config);
  }

  public useCustomErrorHandler(value) {
    this.customHandler = value;
  }
}

export class BaseResponse {
  data: any;
  total: number;
}
