import { ConfigServiceProvider } from '@/$core/config-provider'
const cfg = ConfigServiceProvider();

export const API_HOST = `${cfg.api_host}/`;
export const API_URL = `${cfg.api_host}/${cfg.api_endpoint}`;
export let UPDATE_UI = `${cfg.updateUI}`;
export let UI_NAME = `${cfg.uiName != 'Default' ? cfg.uiName : ''}`;
export const PUSH_NOTIFICATION_URL = `${cfg.push_notification_url}`;
export const EZHR_URL = `${cfg.ezHrUrl}`;
// export const API_URL = "https://localhost:44356/api";
export default { API_URL, API_HOST };