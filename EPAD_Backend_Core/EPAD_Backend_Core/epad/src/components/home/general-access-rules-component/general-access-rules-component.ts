import { areaGroupApi } from '@/$api/gc-area-group-api';
import { GateTree, gatesApi } from '@/$api/gc-gates-api';
import { EmployeeRulesGate, RuleAreaGroup, RulesGeneralAccessParam, ruleGeneralAccessApi } from '@/$api/gc-rules-general-access-api';
import { Component, Mixins, Vue, Watch } from 'vue-property-decorator';

import { Getter } from 'vuex-class';
@Component({
    name: 'general-access-rules',
    components: {},
})
export default class GeneralAccessRules extends Vue {
    listRules: Array<RulesGeneralAccessParam> = [];
    validationObject = null;
    ruleSelected = 0;
    activeCollapse = [];
    ruleObject: RulesGeneralAccessParam = null;
    defaultValue: Date = null;
    areaGroupList = [];
    errorAreaGroup = "";
    gatelineData;
    defaultLine = {};
    logonUsername = '';

    allowCheckOutInWorkingTimeRange = [
        // {FromTime: new Date(), ToTime: new Date(), Error: null}
        {FromTime: null, ToTime: null, Error: null}
    ];

    async beforeMount() {
    }
    async mounted() {
        await this.getAllRules(0);
        await this.getAllAreaGroup();
        this.generateValidation();
        await this.loadGateLineTree();

        this.defaultValue = new Date();
        this.defaultValue.setHours(8);
        this.defaultValue.setMinutes(0);
        this.defaultValue.setSeconds(0);
    }

    async loadGateLineTree() {
        this.gatelineData = [];
        await gatesApi.GetGateLinesAsTree().then((res: any) => {
            if (res.status == 200) {
                const listData: Array<GateTree> = res.data;
                this.gatelineData = listData;
                res.data.forEach(element => {
                    if (element.ListChildrent.length > 0) {
                        const strLineIndexs = element.ListChildrent[0].ID.split("-")[1];
                        this.defaultLine = {
                            RulesCustomerIndex: 0,
                            GateIndex: parseInt(element.ID),
                            LineIndexs: (strLineIndexs == "" ? "" : strLineIndexs + ",")
                        };
                    }
                });
            }
        });
    }

    deleteAllowCheckOutInWorkingTimeRow(index, row) {
		if (this.allowCheckOutInWorkingTimeRange.length <= 1) return;
		if (index < 0) return;

		this.allowCheckOutInWorkingTimeRange.splice(index, 1);
	}
	addAllowCheckOutInWorkingTimeRow(index, row) {
		this.allowCheckOutInWorkingTimeRange.push({
			FromTime: null,
            ToTime: null,
            Error: null
		});
	}

    addRule() {
        this.ruleObject = {
            Index: 0,
            Name: this.$t('RuleName').toString(),
            NameInEng: this.$t('RuleName').toString(),
            CheckInByShift: false,
            CheckInTime: null,
            MaxEarlyCheckInMinute: 0,
            MaxLateCheckInMinute: 0,
            CheckOutByShift: false,
            CheckOutTime: null,
            MaxEarlyCheckOutMinute: 0,
            MaxLateCheckOutMinute: 0,
            AllowFreeInAndOutInTimeRange: false,
            AllowEarlyOutLateInMission: false,
            MissionMaxEarlyCheckOutMinute: 0,
            MissionMaxLateCheckInMinute: 0,
            AdjustByLateInEarlyOut: false,
            EndFirstHaftTime: null,
            BeginLastHaftTime: null,
            AllowInLeaveDay: false,
            AllowInMission: false,
            AllowInBreakTime: false,
            AllowCheckOutInWorkingTime: false,
            AllowCheckOutInWorkingTimeRange: '',
            MaxMinuteAllowOutsideInWorkingTime: 0,
            DenyInLeaveWholeDay: false,
            DenyInMissionWholeDay: false,
            DenyInStoppedWorkingInfo: false,
            CheckLogByAreaGroup: false,
            AreaGroups: [],
            ListGatesInfo: []
        };
        this.allowCheckOutInWorkingTimeRange = [
            {FromTime: null, ToTime: null, Error: null}
        ];
        this.activeCollapse = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];

        ruleGeneralAccessApi.AddRuleGeneral(this.ruleObject).then((res: any) => {
            if (res.status == 200) {
                this.getAllRules(res.data);
            }
            else {
                this.ruleObject = null;
                alert(this.ruleObject);
            }
        }).catch((ex) => {
            this.ruleObject = null;
        });
    }
    updateResult() {
        const tree = this.$refs.gatelineTree as any;
        const selectedData: Array<GateTree> = tree.getCheckedNodes();
        const arrGateLine: Array<EmployeeRulesGate> = [];
        selectedData.forEach(element => {
            let gateIndex = 0;
            let strLineIndexs = "";
            if (element.ParentIndex == 0) {
                gateIndex = parseInt(element.ID);
                strLineIndexs = "";
            }
            else {
                gateIndex = element.ParentIndex;
                strLineIndexs = element.ID.split("-")[1];
            }

            let gateline = arrGateLine.find(x => x.GateIndex == gateIndex);
            if (gateline == undefined) {
                gateline = {
                    RulesGeneralIndex: this.ruleObject.Index,
                    GateIndex: gateIndex,
                    LineIndexs: (strLineIndexs == "" ? "" : strLineIndexs + ",")
                };
                arrGateLine.push(gateline);
            } else {
                gateline.LineIndexs += strLineIndexs + ",";
            }
        });

        (this.$refs.form as any).validate(async (valid) => {
            if (valid) {
                const item = this.ruleObject.AreaGroups.find(e => e.AreaGroupIndex == null || e.AreaGroupIndex === undefined);
                if (this.ruleObject.CheckLogByAreaGroup && item != null) {
                    this.errorAreaGroup = this.$t("PleaseInputAreaGroup").toString();
                } else if (arrGateLine.length < 1) {
                    this.$alert(this.$t('PleaseChooseAreaGroup').toString(), this.$t('Warning').toString());
                } else {
                    this.ruleObject.ListGatesInfo = arrGateLine;

                    console.log("file: general-access-rules-component.ts:141 ~ GeneralAccessRules ~ ruleObject:", this.ruleObject)

                    const submitData = Misc.cloneData(this.ruleObject);
                    if (this.ruleObject.CheckInTime) {
                        submitData.CheckInTime =
                            moment(new Date(submitData.CheckInTime)).format("YYYY-MM-DD HH:mm:ss")
                    }
                    if (this.ruleObject.CheckOutTime) {
                        submitData.CheckOutTime =
                            moment(new Date(submitData.CheckOutTime)).format("YYYY-MM-DD HH:mm:ss")
                    }
                    if (this.ruleObject.EndFirstHaftTime) {
                        submitData.EndFirstHaftTime =
                            moment(new Date(submitData.EndFirstHaftTime)).format("YYYY-MM-DD HH:mm:ss")
                    }
                    if (this.ruleObject.BeginLastHaftTime) {
                        submitData.BeginLastHaftTime =
                            moment(new Date(submitData.BeginLastHaftTime)).format("YYYY-MM-DD HH:mm:ss")
                    }
                    if(this.allowCheckOutInWorkingTimeRange && this.allowCheckOutInWorkingTimeRange.length > 0){
                        const cloneData = Misc.cloneData(this.allowCheckOutInWorkingTimeRange);
                        cloneData.forEach(element => {
                            element.FromTime = moment(new Date(element.FromTime)).format("YYYY-MM-DD HH:mm:ss");
                            element.ToTime = moment(new Date(element.ToTime)).format("YYYY-MM-DD HH:mm:ss");
                        });
                        submitData.AllowCheckOutInWorkingTimeRange = JSON.stringify(cloneData);
                    }
                    ruleGeneralAccessApi.UpdateRuleGeneral(submitData).then((res: any) => {
                        if (res.status == 200) {
                            this.$alert(this.$t('UpdateSuccess').toString(), this.$t('Notify').toString());
                        } else {
                            this.$alert(this.$t(res.data.MessageCode).toString(), this.$t('Warning').toString());
                        }
                    }).finally(async () => {
                        await this.getAllRules(0);
                    });
                }
            }
        });
    }
    deleteRule() {
        this.$confirm(this.$t("AreYouSureWantToDelete?").toString(), this.$t("Warning").toString(), {
			confirmButtonText: this.$t("Delete").toString(),
			cancelButtonText: this.$t("MSG_No").toString(),
			type: 'warning'
		}).then(() => {
            ruleGeneralAccessApi.DeleteRuleGeneral(this.ruleObject.Index).then((res: any) => {
                if (res.status == 200 && res.statusText == "OK") {
                    this.$alert(this.$t('DeleteDataSuccessfully').toString(), this.$t('Notify').toString());
                    this.getAllRules(0);
                }
            });
        });

    }
    CheckShowTableAreaGroup(value) {
        if (value && this.ruleObject.AreaGroups.length == 0) {
            const areaGroup: RuleAreaGroup = {
                AreaGroupIndex: null,
                Priority: 1,
                Rules_GeneralIndex: this.ruleObject.Index
            };
            this.ruleObject.AreaGroups.push(areaGroup);
        }
    }

    changeAllowCheckOutInWorkingTime(){
        this.generateValidation();
    }

    @Watch("ruleObject", {deep: true})
    generateValidation() {
        this.validationObject = {
            Name: [
                { required: true, message: this.$t('PleaseInputName').toString(), trigger: 'blur' }
            ]
        };
        if(this.ruleObject && this.ruleObject.AllowCheckOutInWorkingTime){
            this.validationObject.AllowCheckOutInWorkingTimeRange = [
            {
                required: true,
                trigger: 'change',
                validator: (rule, value: any, callback) => {
                    if (!this.allowCheckOutInWorkingTimeRange 
                        || (this.allowCheckOutInWorkingTimeRange && this.allowCheckOutInWorkingTimeRange.length < 1)) {
                        callback(new Error(this.$t("PleaseSelectTime").toString()));
                    }else{
                        let isError = false;
                        this.allowCheckOutInWorkingTimeRange.forEach((element, index) => {
                            element.Error = null;                          
                            if(!element.FromTime){
	                            element.Error = this.$t("PleaseSelectFromTimes").toString();
                            	callback(new Error(" "));
                                isError = true;
                            }else if(!element.ToTime){
                                element.Error = this.$t("PleaseSelectToTimes").toString();
                            	callback(new Error(" "));
                                isError = true;
                            }
                            else if(element.FromTime && element.ToTime){
                                if(element.FromTime > element.ToTime){
                                    element.Error = this.$t("FromTimesCannotLargerThanToTimes").toString();
                                    callback(new Error(" "));
                                    isError = true;
                                }
                            }
                        });
                        let arrTimesRangeValid = this.allowCheckOutInWorkingTimeRange.filter(x => 
                            !x.Error || x.Error == ''
                        );
                        if(arrTimesRangeValid && arrTimesRangeValid.length > 0){
                            let crossCheck = this.checkTimeCross(arrTimesRangeValid);
                            if(crossCheck && crossCheck.length > 0){
                                crossCheck.forEach(element => {
                                    arrTimesRangeValid[element].Error = this.$t("TimesRangeExisted").toString();
                                    isError = true;
                                    callback(new Error(" "));
                                });
                            }
                        }
                        // if(isError){
                        //     callback(new Error(" "));
                        // }
                        this.$forceUpdate();
                    }
                    callback();
                },
            }];
        }else{
            delete this.validationObject.AllowCheckOutInWorkingTimeRange;
        }
    }

    checkTimeCross(arr){
        let arrCrossIndex = [];
        for (let i = 0; i < arr.length - 1; i++) {
            const currentObj = arr[i];
            const nextObj = arr[i + 1];
        
            const currentFromTime = new Date(new Date().getDay(), new Date().getMonth(), new Date().getFullYear(), currentObj.FromTime.getHours(), currentObj.FromTime.getMinutes(), currentObj.FromTime.getSeconds());
            const currentDateTime = new Date(new Date().getDay(), new Date().getMonth(), new Date().getFullYear(), currentObj.ToTime.getHours(), currentObj.ToTime.getMinutes(), currentObj.ToTime.getSeconds());
            const nextFromTime = new Date(new Date().getDay(), new Date().getMonth(), new Date().getFullYear(), nextObj.FromTime.getHours(), nextObj.FromTime.getMinutes(), nextObj.FromTime.getSeconds());
            const nextDateTime = new Date(new Date().getDay(), new Date().getMonth(), new Date().getFullYear(), nextObj.ToTime.getHours(), nextObj.ToTime.getMinutes(), nextObj.ToTime.getSeconds());
        
            // Check if the time portion of FromTime and DateTime crosses with the next object
            if (
                (currentFromTime <= nextDateTime && currentDateTime >= nextFromTime) ||
                (nextFromTime <= currentDateTime && nextDateTime >= currentFromTime)
            ) {
                // console.log(`Time cross found between objects at indices ${i} and ${i + 1}`);
                arrCrossIndex.push(i);
                arrCrossIndex.push(i + 1);
            }
        }
        if(arrCrossIndex && arrCrossIndex.length > 0){
            return [... new Set(arrCrossIndex)];
        }
        return null;
    }

    async getAllRules(index: number) {
        await ruleGeneralAccessApi.GetAllRules().then((res: any) => {

            if (res.status == 200 && res.statusText == "OK") {

                this.listRules = res.data as Array<RulesGeneralAccessParam>;
                if (index != 0) {
                    const object = res.data.find(e => e.Index == index);
                    this.ruleObject = object;
                    const tblRules = this.$refs.tblRules as any;
                    tblRules.setCurrentRow(this.ruleObject);
                }
            }
        });
    }

    async getAllAreaGroup() {
        await areaGroupApi.GetAreaGroupAll().then((res: any) => {
            if (res.status == 200 && res.data.MessageCode == "ok") {
                this.areaGroupList = res.data.Data.data as Array<RuleAreaGroup>;
            }
        });
    }
    currentRowChanged(val, oldVal) {
        this.ruleSelected = val.Index;
        this.ruleObject = val;
        this.CheckShowTableAreaGroup(this.ruleObject.CheckLogByAreaGroup);

        this.$nextTick(() => {
            (this.$refs.gatelineTree as any).setCheckedKeys([]);
        });

        const arrID = [];
        this.ruleObject.ListGatesInfo.forEach(element => {
            if (element.LineIndexs == "") {
                arrID.push(element.GateIndex);
            }
            else {
                const arrLineIndex: Array<string> = element.LineIndexs.split(",");
                for (let index = 0; index < arrLineIndex.length; index++) {
                    if (arrLineIndex[index] != "") {
                        arrID.push(element.GateIndex + "-" + arrLineIndex[index]);
                    }

                }
            }
        });
        this.$nextTick(() => {
            (this.$refs.gatelineTree as any).setCheckedKeys(arrID);
        });
        if(this.ruleObject.AllowCheckOutInWorkingTimeRange &&  this.ruleObject.AllowCheckOutInWorkingTimeRange != ""){
            this.allowCheckOutInWorkingTimeRange = JSON.parse(this.ruleObject.AllowCheckOutInWorkingTimeRange);
            if(this.allowCheckOutInWorkingTimeRange && this.allowCheckOutInWorkingTimeRange.length > 0){
                this.allowCheckOutInWorkingTimeRange.forEach(element => {
                    element.FromTime = new Date(element.FromTime);
                    element.ToTime = new Date(element.ToTime);
                });
            }
        }else{
            this.allowCheckOutInWorkingTimeRange = [
                // {FromTime: new Date(), ToTime: new Date(), Error: null}
                {FromTime: null, ToTime: null, Error: null}
            ];
        }

    }
    getDataDisabled() {
        return this.ruleObject.AreaGroups.map(({ AreaGroupIndex }) => AreaGroupIndex);
    }
    UpdateIndex() {
        this.ruleObject.AreaGroups.forEach((element, index) => {
            element.Priority = (index + 1);
        });
    }
    deleteRow(index, row) {
        if (this.ruleObject.AreaGroups.length <= 1) return;

        const arrTemp = this.ruleObject.AreaGroups.filter((item) => {
            return item.AreaGroupIndex != row.AreaGroupIndex;
        });

        this.ruleObject.AreaGroups = arrTemp;
        this.UpdateIndex();
        // this.updateModel();
        // this.CheckAllDuplicate();
    }
    addRow(index, row) {
        if (index == 0) {
            this.AddEmptyRow(index);
            this.UpdateIndex();
            // this.updateModel();
        }
    }

    AddEmptyRow(index) {
        this.ruleObject.AreaGroups.unshift({
            Priority: index + 2,
            AreaGroupIndex: null,
            Rules_GeneralIndex: this.ruleObject.Index
        });
    }
}