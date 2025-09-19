// import Vue from 'vue';
// import { camelCase, upperFirst } from 'lodash';


// const autoLoadComponent = require.context('.', true, /^((?!data-table|header|matrix-checkbox).)*-component.vue$/, 'sync');
// export const formCollection: IFormCollection = [];


// autoLoadComponent.keys().forEach(fileName => {
//     const componentConfig = autoLoadComponent(fileName);
//     const componentName = upperFirst(
//         camelCase(
//           // Gets the file name regardless of folder depth
//           fileName
//             .split('/')
//             .pop()
//             .replace('component', '')
//             .replace(/\.\w+$/, '')
//         )
//       );
//     Vue.component(
//         componentName,
//         componentConfig.default || componentConfig
//     );
//     const form: IFormEntry = {
//         formName: componentName.replace('Component', ''),
//         componentName: componentName,
//         active: false,
//         isShow: true
//     }
//     if (form.formName.includes('Home')) {
//         form.default = true;
//         form.active = true;
//     }
//     formCollection.push(form);
// })

// export default { formCollection };