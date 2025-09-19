import { Component, Vue, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { employeeInfoApi, IC_EmployeeInfo, Finger, AddedParam } from '@/$api/employee-info-api';
import { departmentApi } from '@/$api/department-api';
import { isNullOrUndefined } from 'util';
import { Form as ElForm } from 'element-ui';
import * as XLSX from 'xlsx';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import moment from 'moment';
import * as mime from 'mime-types';
import { fail } from 'assert';

@Component({
    name: 'employee',
    components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class EmployeeComponent extends Mixins(ComponentBase) {
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    showDialog = false;
    showDialogImportError = false;
    importErrorMessage = "";
    isLoading = false;
    isEdit = false;
    excelFile = null;
    columns = [];
    options = [];
    rowsObj = [];
    page = 1;
    formExcel = {};
    dataAddExcel = [];
    dataImage = '';
    listExcelFunction = ['AddExcel', 'ExportExcel', 'DeleteExcel'];
    fileName = '';
    fileImageName = '';
    filterDepartmentId : Array<number> = [];
    isDepartMent = true;
    isJoinedDate = true;
    showDialogExcel = false;
    errorUpload = false;
    isDeleteOnDevice = false;
    showDialogDeleteUser = false;
    addedParams: Array<AddedParam> = [];
    listFinger: Array<Finger> = [];
    filter = '';
    ruleForm: IC_EmployeeInfo = {
        EmployeeATID: '',
        EmployeeCode: '',
        Password: '',
        Biometrics: '',
        FullName: '',
        CardNumber: '',
        NameOnMachine: '',
        Gender: null,
        DepartmentIndex: null,
        _DepartmentName: '',
        _Gender: '',
        JoinedDate: null,
        ImageUpload: '',
        ListFinger: []
    };
    rules: any = {};
    maxPageSize = 1000;
    chunkSize = 200;

    mounted() {
        this.getDepartment();
        this.setColumns();
        this.setFingers();
    }

    beforeMount() {
        this.initRule();
    }
    initRule() {
        this.rules = {
            EmployeeATID: [
                {
                    required: true,
                    message: this.$t('PleaseInputEmployeeATID'),
                    trigger: 'change',
                },
                {
                    trigger: 'change',
                    message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                    validator: (rule, value: string, callback) => {
                        if (/^\d+$/.test(value) === false && isNullOrUndefined(value) === false) {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ],
            CardNumber: [
                {
                    trigger: 'change',
                    message: this.$t('CardNumberOnlyAcceptNumericCharacters'),
                    validator: (rule, value: string, callback) => {
                        if (/^\d+$/.test(value) === false && isNullOrUndefined(value) === false && value !== '') {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ],
            JoinedDate: [
                {
                    required: true,
                    message: this.$t('PleaseSelectDayJoinedDate'),
                    trigger: 'change',
                },
            ],
            Department: [
                {
                    trigger: 'change',
                    message: this.$t('SelectDepartment'),
                    validator: (rule, value: string, callback) => {
                        if (isNullOrUndefined(this.ruleForm.DepartmentIndex) && this.isEdit == true) {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ]
        };
    }

    setColumns() {
        this.columns = [
            {
                prop: 'EmployeeATID',
                label: 'EmployeeATID',
                minWidth: 100,
                fixed: true,
                display:true
            },
            {
                prop: 'EmployeeCode',
                label: 'EmployeeCode',
                minWidth: 140,
                fixed: true,
                display:true
            },
            {
                prop: 'FullName',
                label: 'FullName',
                minWidth: 180,
                fixed: true,
                display: true
            },
            {
                prop: 'CardNumber',
                label: 'CardNumber',
                minWidth: 150,
                display: true
            },
            {
                prop: 'NameOnMachine',
                label: 'NameOnMachine',
                minWidth: 170,
                display: true
            },
            {
                prop: '_Gender',
                label: 'Gender',
                minWidth: 120,
                display: true
            },
            {
                prop: '_DepartmentName',
                label: 'Department',
                minWidth: 150,
                display: true
            },
            {
                prop: 'JoinedDate',
                label: 'JoinedDate',
                minWidth: 150,
                display: true
            },
            {
                prop: 'UpdatedDate',
                label: 'UpdatedDate',
                minWidth: 150,
                display: true
            }
        ];
    }
    setFingers() {
        for (var i = 1; i <= 10; i++) {
            this.listFinger.push({ FocusFinger: false, ID: i, Template: "", ImageFinger: "" });
        }
    }
    getImgUrl(image) {
        return require('@/assets/images/' + image);
    }
    async getDepartment() {
        return await departmentApi.GetDepartment().then((res) => {
            const { data } = res as any;
           // let arr = JSON.parse(JSON.stringify(data));
            for (let i = 0; i < data.length; i++) {
                data[i].value = parseInt(data[i].value);
            }
            this.options = data;
        });
    }

    async getData({ page, filter, sortParams, pageSize }) {
        this.filter = filter;
        if (pageSize > this.maxPageSize) return this.getDataChunks({ page, filter, sortParams, pageSize });

        this.page = page;
        return await employeeInfoApi.GetEmployeeAtPage(page, filter, this.filterDepartmentId, pageSize).then((res) => {
            const { data } = res as any;
            const arrTemp = [];
            data.data.forEach((item) => {
                const a = Object.assign(item, {
                    JoinedDate: moment(item.JoinedDate).format('YYYY-MM-DD'),
                    UpdatedDate: item.UpdatedDate != null ? moment(item.UpdatedDate).format('YYYY-MM-DD') : ""
                });
                arrTemp.push(a);
            });
            return {
                data: arrTemp,
                total: data.total,
            };
            
        });
    }

    async getDataChunks({ page, filter, sortParams, pageSize }) {
        let total = 0;
        const arrTemp = [];

        const chunks = Math.floor(pageSize / this.chunkSize);
        let _page = ((page - 1) * chunks) + 1;

        this.isLoading = true;
        for (let i = 0; i < chunks; i++) {
            await employeeInfoApi.GetEmployeeAtPage(_page, filter, this.filterDepartmentId, this.chunkSize).then((res) => {
                const { data } = res as any;
                data.data.forEach((item) => {
                    const a = Object.assign(item, {
                        JoinedDate: moment(item.JoinedDate).format('YYYY-MM-DD'),
                        UpdatedDate: item.UpdatedDate != null ? moment(item.UpdatedDate).format('YYYY-MM-DD'):""
                    });
                    arrTemp.push(a);

                });
                this.isLoading = false;
                _page++;
                total = data.total;
            });
        }
        this.isLoading = false;
        return {
            data: arrTemp,
            total: total
        };
    }

    async onChangeDepartment() {
        (this.$refs.table as any).getTableData(this.page);
    }
    reset() {
        var obj: IC_EmployeeInfo = {};
        this.ruleForm = obj;
    }

    Insert() {
        this.showDialog = true;
        this.isEdit = false;
        this.isDepartMent = false;
        this.isJoinedDate = false;
        this.reset();
    }

    async submitObj() {
        this.ruleForm.JoinedDate = new Date(moment(this.ruleForm.JoinedDate).format('YYYY-MM-DD'));
        (this.$refs.ruleForm as any).validate(async (valid) => {
            if (!valid) return;
            else {
                if (this.isEdit == true) {
                    await employeeInfoApi.UpdateEmployee(this.ruleForm).then((res) => {
                        (this.$refs.table as any).getTableData(this.page, null, null);
                        this.showDialog = false;
                        this.reset();
                        if (!isNullOrUndefined(res.status) && res.status === 200) {
                            this.$saveSuccess();
                        }
                    });
                } else {
                    await employeeInfoApi
                        .AddEmployee(this.ruleForm)
                        .then((res) => {
                            (this.$refs.table as any).getTableData(this.page, null, null);
                            this.showDialog = false;
                            this.reset();
                            if (!isNullOrUndefined(res.status)) {
                                if (!isNullOrUndefined(res.status) && res.status === 200) {
                                    this.$saveSuccess();
                                }
                            }
                        })
                        .catch();
                }
            }
        });
    }

    getEmployeeFinger(employeeATID) {
        employeeInfoApi.GetEmployeeFinger(employeeATID).then(res => {
            const { data } = res as any;
            for (var i = 0; i < data.length; i++) {
                if (!isNullOrUndefined(data[i]) && data[i].length > 0) {
                    this.listFinger[i].ImageFinger = this.getImgUrl('fingerprint.png');
                }
            }
            //this.listFinger = data.data;
        });
    }

    Edit() {
        this.isEdit = true;
        this.isDepartMent = true;
        this.isJoinedDate = true;
        var obj = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length > 1) {
            this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
        } else if (obj.length == 1) {
            this.showDialog = true;
            this.ruleForm = obj[0];
            this.isDepartMent = this.ruleForm.DepartmentIndex == 0 || this.ruleForm.DepartmentIndex == null ? false : true;

            this.getEmployeeFinger(obj[0].EmployeeATID);

        } else {
            this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
        }
    }

    Delete() {
        const obj: IC_EmployeeInfo[] = JSON.parse(JSON.stringify(this.rowsObj));

        this.addedParams = [];
        this.addedParams.push({ Key: "ListEmployeeATID", Value: obj.map(e => e.EmployeeATID) });
        this.addedParams.push({ Key: "IsDeleteOnDevice", Value: this.isDeleteOnDevice });
        employeeInfoApi
            .DeleteEmployee(this.addedParams)
            .then((res) => {
                (this.$refs.table as any).getTableData(this.page);
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.showOrHideDialogDeleteUser(false);
                    this.$deleteSuccess();
                } else {
                    this.$alert(
                        this.$t("deleteEmployeeError").toString(),
                        this.$t("Notify").toString(),
                        { type: "warning" }
                    );
                }
            })
            .catch(() => {
                this.$alert(
                    this.$t("deleteEmployeeError").toString(),
                    this.$t("Notify").toString(),
                    { type: "warning" }
                );
            });
        //this.$confirmDelete().then(async () => {

        //});

    }
    showOrHideDialogDeleteUser(showOrHide) {
        const obj: IC_EmployeeInfo[] = JSON.parse(JSON.stringify(this.rowsObj));
        if (obj.length < 1) {
            this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());

        } else {
            this.showDialogDeleteUser = showOrHide;
        }
    }
    cancelDialogDeleteUser() {
        this.isDeleteOnDevice = false;
    }
    Cancel() {
        this.ruleForm.ImageUpload = '';
        var ref = <ElForm>this.$refs.ruleForm;
        ref.resetFields();
        this.showDialog = false;
        this.fileName = '';
        this.fileImageName = '';
        this.isDeleteOnDevice = false;
        if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
            (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
        }
        if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
            (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
        }
        this.reset();
    }
    focus(x) {
        var theField = eval('this.$refs.' + x);
        theField.focus();
    }

    AddOrDeleteFromExcel(x) {
        if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            this.isDeleteFromExcel = false;
            this.showDialogExcel = false;
            this.fileName = '';
        } else if (x === 'add') {
            this.isAddFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        } else if (x === 'delete') {
            this.isDeleteFromExcel = true;
            this.showDialogExcel = true;
            this.fileName = '';
        }
    }

    processFile(e) {
        if ((<HTMLInputElement>e.target).files.length > 0) {
            var file = (<HTMLInputElement>e.target).files[0];
            this.fileName = file.name;
            if (!isNullOrUndefined(file)) {
                var fileReader = new FileReader();
                var arrData = [];
                fileReader.onload = function (event) {
                    var data = event.target.result;
                    var workbook = XLSX.read(data, {
                        type: 'binary',
                    });

                    workbook.SheetNames.forEach((sheet) => {
                        var rowObject = XLSX.utils.sheet_to_json(workbook.Sheets[sheet]);
                        // var arr = Array.from(rowObject)
                        // arrData.push(arr)
                        arrData.push(Array.from(rowObject));
                    });
                };
                this.dataAddExcel = arrData;
                fileReader.readAsBinaryString(file);
            }
        }
    }

    getBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }
    async processImageFile(e) {
        if ((<HTMLInputElement>e.target).files.length > 0) {
            var file = (<HTMLInputElement>e.target).files[0];
            if (file.size > 25000) //20kb
            {
                this.ruleForm.ImageUpload = "";
                this.errorUpload = true;
            } else {
                if (!isNullOrUndefined(file)) {
                    this.fileImageName = file.name;
                    this.ruleForm.ImageUpload = "";
                    await this.getBase64(file).then(
                        data => {
                            this.ruleForm.ImageUpload = data.toString().substring(data.toString().indexOf(',') + 1);
                            this.errorUpload = false;
                            this.$forceUpdate();
                        }
                    );
                }
            }
        }
    }

    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }

    UploadDataFromExcel() {
        this.importErrorMessage = "";
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a = Object.assign({});
            if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a.EmployeeATID = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            } else {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDMayNotBeBlank').toString()).toString();
                return;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã nhân viên')) {
                a.EmployeeCode = this.dataAddExcel[0][i]['Mã nhân viên'] + '';
            } else {
                a.EmployeeCode = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Họ tên')) {
                a.FullName = this.dataAddExcel[0][i]['Họ tên'] + '';
            } else {
                a.FullName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã thẻ')) {
                a.CardNumber = this.dataAddExcel[0][i]['Mã thẻ'] + '';
            } else {
                a.CardNumber = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên trên máy')) {
                a.NameOnMachine = this.dataAddExcel[0][i]['Tên trên máy'] + '';
            } else {
                a.NameOnMachine = '';
            }
            if (this.dataAddExcel[0][i]['Giới tính (Nam)'] == 'x' || this.dataAddExcel[0][i]['Giới tính (Nam)'] == 'X') {
                a.Gender = 1;
            } else {
                a.Gender = 0;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Phòng ban')) {
                a.DepartmentName = this.dataAddExcel[0][i]['Phòng ban'] + '';
            } else {
                a.DepartmentName = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tổ')) {
                a.TeamName = this.dataAddExcel[0][i]['Tổ'] + '';
            } else {
                a.TeamName =  '';
            }
            arrData.push(a);
        }

        employeeInfoApi.AddEmployeeFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }
            (this.$refs.table as any).getTableData(this.page, null, null);
            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$saveSuccess();
            } else {
                this.importErrorMessage = this.$t('ImportEmployeeErrorMessage') + res.data.toString() + " "+this.$t('Employee');
                this.showOrHideImportError(true);
            }
        });
    }


    DeleteDataFromExcel() {
        var arrData = [];
        var regex = /^\d+$/;
        for (let i = 0; i < this.dataAddExcel[0].length; i++) {
            let a;
            if (regex.test(this.dataAddExcel[0][i]['Mã chấm công (*)']) === false) {
                this.$alertSaveError(null, null, null, this.$t('EmployeeATIDOnlyAcceptNumericCharacters').toString()).toString();
                return;
            } else if (this.dataAddExcel[0][i].hasOwnProperty('Mã chấm công (*)')) {
                a = this.dataAddExcel[0][i]['Mã chấm công (*)'] + '';
            }
            arrData.push(a);
        }

        this.addedParams = [];
        this.addedParams.push({ Key: "ListEmployeeATID", Value: arrData });
        this.addedParams.push({ Key: "IsDeleteOnDevice", Value: this.isDeleteOnDevice });
        employeeInfoApi
            .DeleteEmployeeFromExcel(this.addedParams)
            .then((res) => {
                if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileUpload')))) {
                    (<HTMLInputElement>document.getElementById('fileUpload')).value = '';
                }
                if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                    (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
                }
                (this.$refs.table as any).getTableData(this.page, null, null);
                this.showDialogExcel = false;
                this.fileName = '';
                this.isDeleteFromExcel = false;
                this.dataAddExcel = [];
                if (!isNullOrUndefined(res.status) && res.status === 200) {
                    this.$deleteSuccess();
                }
            })
            .catch(() => { });
    }

    async ExportToExcel() {

        this.addedParams = [];
        this.addedParams.push({ Key: "ListDepartment", Value: this.filterDepartmentId });
        this.addedParams.push({ Key: "Filter", Value: this.filter });
        await employeeInfoApi.ExportToExcel(this.addedParams).then((res) => {
            const filePath = "Employee.xlsx";
            this.downloadFile(res.data.toString())
        });
    }

    downloadFile(filePath) {
            var link = document.createElement('a');
            link.href = filePath;
            link.download = filePath.substr(filePath.lastIndexOf('/') + 1);
            link.click();
        }
    // Finger pRINT
    showOrHideRegisterFingerDialog() {
        this.showRegisterFingerDialog = true;
        this.resetParam();
        this.websopen();
    }
    cancelRegisterFingerDialog() {
        this.showRegisterFingerDialog = false;
        this.websclose();
        this.listFinger = [];
        this.setFingers();
    }
    submitRegisterFinger() {
        this.ruleForm.ListFinger = this.listFinger.map(e => (e.Template));
        this.showRegisterFingerDialog = false;
    }
    showRegisterFingerDialog = false;
    wsUri = "ws://127.0.0.1:22003";
    websocket;
    currentIndex: number = 0;
    DeviceInfo: string = this.$t('NotConnectedDevice').toString();
    ConnectedDevice: boolean = false;
    template1: string = "";
    template2: string = "";
    registercount: number = 0;
    resetParam() {
        this.registercount = 0;
        this.template1 = "";
        this.template2 = "";
    }
    onFocusFinger(index) {

        if (this.currentIndex != 0) {
            this.closedev();
        }
        this.listFinger.forEach(function (item) {
            item.FocusFinger = false;
        });
        this.listFinger[index - 1].FocusFinger = true;
        this.currentIndex = index - 1;
        this.opendev();
        //this.getinfo();
    }
    onOpen(event) {
        console.log(event.data);
    }
    onClose(event) {
        console.log(event.data);
    }
    onError(event) {
        console.log(event.data);
    }
    onMessage(event) {
        if (event.data != undefined) {
            var jsondata = JSON.parse(event.data);
            if (jsondata.datatype === "image") {
                var tempImg = jsondata.data.jpg_base64
                if (tempImg == undefined || tempImg == '') {
                    return false
                }
                var strImgData = "data:image/jpg;base64,";
                strImgData += tempImg;
                this.listFinger[this.currentIndex].ImageFinger = strImgData;
                //document.getElementById('imgData').src = strImgData;
            }
            if (jsondata.datatype === "template") {

                this.listFinger[this.currentIndex].Template = jsondata.data.template;
                //this.registercount += 1;
                //if (this.registercount == 1) {
                //    this.template1 = jsondata.data.template;
                //    this.template2 = jsondata.data.template;
                //}
                //if (this.registercount == 2) {
                //    this.template2 = jsondata.template;
                //}
                // console.log(event.data);
            }
            else {
                if (jsondata.ret == 0 && jsondata.function == "open") {
                    this.ConnectedDevice = true;
                    this.DeviceInfo = this.$t('ConnectedFingerDevice').toString();
                } else if (jsondata.ret == -10007) {
                    this.ConnectedDevice = false;
                    this.DeviceInfo = this.$t('NotConnectedDevice').toString();
                } else if (jsondata.ret < 0) {
                    this.DeviceInfo = this.$t('NotConnectedDevice').toString();
                }
            }
            //console.log(event.data);
            //console.log(this.template1);
            //console.log(this.template2);
            //if (this.registercount == 1) {
            //    console.log("here");
            //    console.log(this.template1);
            //    console.log(this.template2);
            //    this.registercount = 0;
            //    this.verity();

            //}
        }
        else {
            this.DeviceInfo = this.$t('NotConnectedDevice').toString();
        }
    }

    doSend(message) {
        this.websocket.send(message);
    }
    websopen() {
        this.websocket = new WebSocket(this.wsUri);
        this.websocket.onopen = (event) => {
            this.onOpen(event);
        }
        this.websocket.onclose = (event) => {
            this.onClose(event);
        }

        this.websocket.onmessage = (event) => {
            this.onMessage(event);
        }


        this.websocket.onerror = (event) => {
            this.onError(event);
        }
    }
    reconnect() {
        this.resetParam();
        this.closedev();
        this.opendev();
    }
    websclose() {
        this.websocket.close();
    }
    getinfo() {
        this.doSend("{\"module\":\"common\",\"function\":\"info\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }
    register() {
        this.doSend("{\"module\":\"fingerprint\",\"function\":\"register\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }
    disregister() {
        this.doSend("{\"module\":\"fingerprint\",\"function\":\"cancelregister\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
    }
    verity() {
        //console.log(this.template1);
        //console.log(this.template2);
        var str = "{\"module\":\"fingerprint\",\"function\":\"verify\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":{" + "\"template1\":\"" + this.template1 + "\",\"template2\":\"" + this.template2 + "\"}}"
        this.doSend(str);
    }
    opendev() {

        var str = "{\"module\":\"fingerprint\",\"function\":\"open\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}"
        this.doSend(str);
    }
    closedev() {
        if (this.websocket.readyState != WebSocket.CLOSED && this.websocket.readyState != WebSocket.CLOSING) {
            this.doSend("{\"module\":\"fingerprint\",\"function\":\"close\",\"msgid\":\"" + this.currentIndex + "\",\"parameter\":\"\"}");
        }
    }
}
