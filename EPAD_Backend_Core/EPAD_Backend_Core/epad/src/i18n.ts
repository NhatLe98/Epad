// import Vue from 'vue'
// import VueI18n, { LocaleMessages } from 'vue-i18n'

// Vue.use(VueI18n)

// function loadLocaleMessages (): LocaleMessages {
//   const locales = require.context('./locales', true, /[A-Za-z0-9-_,\s]+\.json$/i)
//   const messages: LocaleMessages = {}
//   locales.keys().forEach(key => {
//     const matched = key.match(/([A-Za-z0-9-_]+)\./i)
//     if (matched && matched.length > 1) {
//       const locale = matched[1]
//       messages[locale] = locales(key)
//     }
//   })
//   return messages
// }

// export default new VueI18n({
//   silentTranslationWarn: true,
//   locale: process.env.VUE_APP_I18N_LOCALE || 'vi',
//   fallbackLocale: process.env.VUE_APP_I18N_FALLBACK_LOCALE || 'en',
//   messages: loadLocaleMessages()
// })
import { isNullOrUndefined } from 'util';
import Vue from 'vue';
import VueI18n, { LocaleMessages } from 'vue-i18n';
import _ from 'lodash';

Vue.use(VueI18n);

const localesLang = require.context(
  './locales',
  true,
  /[A-Za-z0-9-_,\s]+\.json$/i
);

function loadLocaleMessages(): LocaleMessages {
  let messages: LocaleMessages = {};
  localesLang.keys().forEach((key) => {
    //Comment if want to have difference translate
    if (key !== './may/vi.json') {
      const dummy = localesLang(key);
      const local = getLocal(key);
      if (!isNullOrUndefined(local)) {
        messages = _.merge({ [local]: dummy }, messages);
      }
    }
  });

  const elLang = require.context(
    `element-ui/lib/locale/lang/`,
    false,
    /[A-Za-z0-9-_,\s]+\.js$/i
  );
  elLang.keys().forEach((elKey) => {
    const local = getLocal(elKey);
    if (!isNullOrUndefined(local)) {
      _.merge(messages[local], elLang(elKey).default);
    }
  });
  return messages;
}

function getLocal(key: string) {
  const keyPattern = /([A-Za-z0-9-_]+)\./i;
  const matched = key.match(keyPattern);
  if (matched && matched.length > 1) {
    return matched[1];
  }
}

const i18n = new VueI18n({
  locale: process.env.VUE_APP_I18N_LOCALE || 'vi',
  fallbackLocale: process.env.VUE_APP_I18N_FALLBACK_LOCALE || 'vi',
  messages: loadLocaleMessages(),
  silentTranslationWarn: true,
});

if (module.hot) {
  const accepts = localesLang.keys();
  module.hot.accept(accepts, () => {
    const localMess = loadLocaleMessages();
    Object.keys(localMess).forEach((keys) => {
      i18n.mergeLocaleMessage(keys, localMess[keys]);
    });
  });
}

export default i18n;
