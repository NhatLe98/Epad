import { Component, Ref, Mixins, Watch } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { deviceApi, IC_Device } from '@/$api/device-api';
import { userAccountApi } from '@/$api/user-account-api';
import { privilegeMachineRealtimeApi, IC_PrivilegeMachineRealtimeDTO } from '@/$api/privilege-machine-realtime-api';
import { licenseApi } from '@/$api/license-api';
import { Form as ElForm } from 'element-ui';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import * as XLSX from 'xlsx';
import { isNullOrUndefined } from 'util';
import { serviceApi } from '@/$api/service-api';
import { groupDeviceApi } from '@/$api/group-device-api';

@Component({
    name: 'user-guide-page-component',
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class MachineComponent extends Mixins(ComponentBase) {
    listGuide = [];
    beforeMount() {
        Misc.readFileAsync('static/variables/user-guide.json').then(res => {
            if(res && res.length > 0){
                res.forEach(element => {
                  this.listGuide.push({
                    link: element.link,
                    title: element.title
                  });
                });
              }
        });
    }
}
