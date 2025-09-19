<template>
    <div>
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{
                            $t("DownloadServiceSettingCollection")
                        }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main>
                <el-row :gutter="10">
                    <el-col
                        v-for="(item, index) in listService"
                        :key="index"
                        :value="item"
                        :xs="24"
                        :sm="12"
                        :md="8"
                    >
                        <div class="box" @click="downLoadSettingService(item)" v-if="item.ServiceType !=='PUSHInterfaceService'">
                            <img
                                src="../../../assets/background/service.png"
                                style="width:80px;height:80px;"
                            />
                            <span>{{ $t("Name") }}: {{ item.Name }}</span>
                            <span v-if="item.ServiceType ? true : false"
                                >{{ $t("ServiceType") }}:
                                {{ item.ServiceType }}</span
                            >
                            <span v-if="item.Index ? true : false"
                                >Index: {{ item.Index }}</span
                            >
                        </div>
                        <div class="box disable" v-else>
                            <img
                                src="../../../assets/background/service.png"
                                style="width:80px;height:80px;"
                            />
                            <span>{{ $t("Name") }}: {{ item.Name }}</span>
                            <span v-if="item.ServiceType ? true : false"
                                >{{ $t("ServiceType") }}:
                                {{ item.ServiceType }}</span
                            >
                            <span v-if="item.Index ? true : false"
                                >Index: {{ item.Index }}</span
                            >
                        </div>
                    </el-col>
                </el-row>
            </el-main>
        </el-container>
    </div>
</template>

<script src="./list-service-component.ts"></script>
<style scoped>
    .bgHome {
        margin: inherit !important;
    }
    .box {
        margin: auto;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.15);
        min-height: 150px;
        border-radius: 5px;
        margin-bottom: 10px;
        background-color: white;
    }
    .box:hover {
        cursor: pointer;
        background-color: lavender;
    }
    .box.disable:hover {
        cursor: not-allowed;
    }
</style>
