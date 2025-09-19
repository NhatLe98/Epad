import { ConfigService } from './config-service';

export const ConfigServiceProvider = () => {
    const cfg = new ConfigService();
    const browserWindows = window || {};
    const browserWindowsCfg = browserWindows['__env'] || {};

    for (const key in browserWindowsCfg) {
        if (browserWindowsCfg.hasOwnProperty(key)) {
            cfg[key] = window['__env'][key];
        }
    }
    return cfg;
}

export default { ConfigServiceProvider: ConfigService }