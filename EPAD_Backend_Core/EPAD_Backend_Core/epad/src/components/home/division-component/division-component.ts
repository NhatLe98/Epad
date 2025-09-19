import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import { EZHR_URL } from "@/$core/config";
@Component({
    name: 'division',
    components: {
  
    },
})
export default class DivisionComponent extends Mixins(ComponentBase) {
    page = 1;
    showDialog = false;
    showMessage = false;
    checked = false;
    columns = [];
    rowsObj = [];
    isEdit = false;
    listExcelFunction = [];
 
    rules: any = {};

    beforeMount() {
        this.$router.push({ name: "home" })
        window.open(EZHR_URL, '_blank');
    }

   
    
}
