import { Component, Vue, Mixins } from "vue-property-decorator";
import ComponentBase from "@/mixins/application/component-mixins";
import HeaderComponent from "@/components/home/header-component/header-component.vue";
import { serviceApi, IC_Service } from "@/$api/service-api";

@Component({
    name: "list-service",
    components: { HeaderComponent },
})
export default class ListServiceComponent extends Mixins(ComponentBase) {
    listService: Array<any> = [];
    beforeMount() {}

    mounted() {
        this.getData();
    }

    async getData() {
        return await serviceApi.GetServiceForDownload().then((res) => {
            const { data } = res as any;
            this.listService = [...data];
        });
    }

    async downLoadSettingService(obj: IC_Service) {
        var link = "";
        await serviceApi.DownloadSettingService(obj).then((res) => {
            link = `${res.data}`;
        });
        window.open(link, "_blank");
    }
}
