import { areaGroupApi } from '@/$api/gc-area-group-api';
import { GateTree, gatesApi } from '@/$api/gc-gates-api';
import { ruleInOutTimeApi, RulesInOutTimeParam } from '@/$api/rules-in-out-time-api';
import { Component, Mixins, Vue, Watch } from 'vue-property-decorator';
import HeaderComponent from '@/components/home/header-component/header-component.vue';

import { Getter } from 'vuex-class';
@Component({
    name: 'in-out-time-rules',
    components: { HeaderComponent },
})
export default class InOutTimeRules extends Vue {
    listRules: Array<RulesInOutTimeParam> = [];
    validationObject = null;
    ruleSelected = 0;
    activeCollapse = [];
    ruleObject: RulesInOutTimeParam = null;
    defaultValue: Date = null;
    areaGroupList = [];
    errorAreaGroup = "";
    gatelineData;
    defaultLine = {};
    logonUsername = '';

    async beforeMount() {
    }
    async mounted() {
        await this.getAllRules(0);
        this.generateValidation();

        this.defaultValue = new Date();
        this.defaultValue.setHours(8);
        this.defaultValue.setMinutes(0);
        this.defaultValue.setSeconds(0);
    }

    addRule() {
        this.ruleObject = {
            Index: 0,
            FromDate: new Date(),
            FromDateString: "",
            Description: "",
            CheckInTime: null,
            MaxEarlyCheckInMinute: 0,
            MaxLateCheckInMinute: 0,
            CheckOutTime: null,
            MaxEarlyCheckOutMinute: 0,
            MaxLateCheckOutMinute: 0,
        };
        this.activeCollapse = ['a', 'b'];

        // ruleInOutTimeApi.AddRuleInOutTime(this.ruleObject).then((res: any) => {
        //     if (res.status == 200) {
        //         this.getAllRules(0);
        //     }
        //     else {
        //         this.ruleObject = null;
        //         alert(this.ruleObject);
        //     }
        // }).catch((ex) => {
        //     this.ruleObject = null;
        // });
    }
    updateResult() {
        (this.$refs.form as any).validate(async (valid) => {
            if (valid) {
                // console.log("file: in-out-time-rules-component.ts:141 ~ GeneralAccessRules ~ ruleObject:", this.ruleObject)

                const submitData = Misc.cloneData(this.ruleObject);
                if (this.ruleObject.FromDate) {
                    submitData.FromDate =
                        moment(new Date(submitData.FromDate)).format("DD-MM-YYYY")
                }
                if (this.ruleObject.CheckInTime) {
                    submitData.CheckInTimeString =
                        moment(new Date(submitData.CheckInTime)).format("DD-MM-YYYY HH:mm:ss")
                }
                if (this.ruleObject.CheckOutTime) {
                    submitData.CheckOutTimeString =
                        moment(new Date(submitData.CheckOutTime)).format("DD-MM-YYYY HH:mm:ss")
                }
                ruleInOutTimeApi.UpdateRuleInOutTime(submitData).then((res: any) => {
                    if (res.status == 200) {
                        this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
                        this.getAllRules(0);
                    } else {
                        this.$alert(this.$t(res.data.MessageCode).toString(), this.$t('Warning').toString());
                    }
                });
            }
        });
    }
    deleteRule() {
        this.$confirm(this.$t("AreYouSureWantToDelete?").toString(), this.$t("Warning").toString(), {
			confirmButtonText: this.$t("Delete").toString(),
			cancelButtonText: this.$t("MSG_No").toString(),
			type: 'warning'
		}).then(() => {
            ruleInOutTimeApi.DeleteRuleInOutTime(this.ruleObject.Index).then((res: any) => {
                if (res.status == 200 && res.statusText == "OK") {
                    this.$alert(this.$t('DeleteDataSuccessfully').toString(), this.$t('Notify').toString());
                    this.getAllRules(0);
                    this.ruleSelected = 0;
                    this.ruleObject = null;
                }
            });
        });

    }
    generateValidation() {
        this.validationObject = {
            FromDate: [
                { required: true, message: this.$t('PleaseSelectDate').toString(), trigger: 'blur' }
            ],
            CheckInTime: [
                { required: true, message: this.$t('PleaseSelectTime').toString(), trigger: 'blur' }
            ],
            CheckOutTime: [
                { required: true, message: this.$t('PleaseSelectTime').toString(), trigger: 'blur' }
            ]
        };
    }
    async getAllRules(index: number) {
        await ruleInOutTimeApi.GetAllRules().then((res: any) => {
            if (res.status == 200 && res.statusText == "OK") {
                this.listRules = res.data as Array<RulesInOutTimeParam>;
                this.listRules.forEach(element => {
                    element.FromDateString = Misc.cloneData(element.FromDate);
                    element.FromDate = moment(element.FromDate, "DD/MM/YYYY").toDate();
                });
                if (index != 0) {
                    const object = res.data.find(e => e.Index == index);
                    this.ruleObject = object;
                    const tblRules = this.$refs.tblRules as any;
                    tblRules.setCurrentRow(this.ruleObject);
                }
            }
        });
    }

    currentRowChanged(val, oldVal) {
        this.ruleSelected = val.Index;
        this.ruleObject = val;
    }
}