import { Component, Vue, Mixins } from 'vue-property-decorator';
import { departmentApi } from '@/$api/department-api';
import TGrid from '@/components/app-component/t-grid/t-grid.vue';
import TabBase from '@/mixins/application/tab-mixins';
import { hrParentInfoApi } from '@/$api/hr-parent-info-api';
import { hrStudentInfoApi } from '@/$api/hr-student-info-api';
import { hrClassInfoApi } from '@/$api/hr-class-info-api';
import { isNullOrUndefined } from "util";
import * as XLSX from 'xlsx';

@Component({
    name: 'hr-parent-info',
    components: { 't-grid': TGrid },
})
export default class HRParentInfo extends Mixins(TabBase) {
    fileImageName = '';
    errorUpload = false;
    fileList = [];
    filterModel = { TextboxSearch: '' }

    allClass = [];
    allClassLookup = {};

    allStudent = [];
    allStudentLookup = [];

    async initLookup() {
        this.initClassLookup();
    }

    async initClassLookup() {
        await hrStudentInfoApi.GetAllStudent()
            .then(response => {
                this.allStudent = response.data;
                this.allStudent.forEach(e => {
                    this.allStudentLookup[e.EmployeeATID] = e;
                })
            });
        await hrClassInfoApi.GetAllHRClassInfo()
            .then(response => {
                this.allClass = response.data;
                this.allClass.forEach(e => {
                    this.allClassLookup[e.Index] = e;
                })
            });
    }

    initGridColumns() {
        this.gridColumns = [
            {
                name: 'Avatar',
                dataField: 'Avatar',
                dataType: 'image',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'UserCode',
                dataField: 'EmployeeATID',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'FullName',
                dataField: 'FullName',
                fixed: true,
                width: 150,
                show: true,
            },
            {
                name: 'ParentOfStudents',
                dataField: 'StudentStr',
                fixed: false,
                dataType: "viewDetailPopup",
                width: 180,
                show: true,
            },
            {
                name: 'Email',
                dataField: 'Email',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'Phone',
                dataField: 'Phone',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'BirthDay',
                dataField: 'BirthDay',
                dataType: 'date',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'CardNumber',
                dataField: 'CardNumber',
                fixed: false,
                width: 150,
                show: true,
            },
            {
                name: 'NameOnMachine',
                dataField: 'NameOnMachine',
                fixed: false,
                width: 150,
                show: true,
            }
        ];
    }

    initFormRules() {
        this.formRules = {
            EmployeeATID: [
                {
                    required: true,
                    message: this.$t('PleaseInputUserCode'),
                    trigger: 'change',
                },
                {
                    trigger: 'change',
                    message: this.$t('EmployeeATIDOnlyAcceptNumericCharacters'),
                    validator: (rule, value: string, callback) => {
                        if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
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
                        if (Misc.isEmpty(value) === false && Misc.isNumber(value) === false) {
                            callback(new Error());
                        }
                        callback();
                    },
                },
            ],
            FullName: [
                {
                    required: true,
                    message: this.$t('PleaseInputFullName'),
                    trigger: 'change',
                }
            ],
        };
    }

    async loadData() {
        this.dataSource = [];
        await hrParentInfoApi.GetParentAtPage(this.filterModel.TextboxSearch, this.page, this.pageSize).then(response => {
            const { data, total } = response.data;
            this.dataSource = data;
            const studentsLookup = this.allStudentLookup;
            this.dataSource.map(function (item) {
                item.StudentStr = "";
                if (item.Students != "" && item.Students != null) {
                    item.Students = item.Students.split(",");
                    item.StudentStr = studentsLookup.filter(x => item.Students.indexOf(x.EmployeeATID) > -1).map(x => x.FullName).join(", ");
                } else {
                    item.Students = '';
                }
            })
            this.total = total;
        })
    }

    async doDelete() {
        const selectedEmp = this.selectedRows.map(e => e.EmployeeATID);
        await hrParentInfoApi
            .DeleteParentMulti(selectedEmp)
            .then((res) => {
                this.selectedRows = [];
                this.$deleteSuccess();
            })
            .catch(() => { })
            .finally(() => { this.showDialogDeleteUser = false; })

    }

    async onViewClick() {
        //  this.configModel.filterModel = this.filterModel;
        this.$emit('filterModel', this.configModel);
        await this.loadData();
    }

    async onSubmitClick() {
        (this.$refs.parentFormModel as any).validate(async (valid) => {
            if (!valid) return;
            const atid = (this.formModel as any).EmployeeATID;
            (this.formModel as any).BirthDay = new Date(moment((this.formModel as any).BirthDay).format('YYYY-MM-DD'));

            const object = Object.assign(this.formModel);
            object.Students = object.Students.join(",");
            if (object.BirthDay != null) {
                object.YearOfBirth = parseInt(moment(object.BirthDay).format("YYYY"));
                object.MonthOfBirth = parseInt(moment(object.BirthDay).format("MM"));
                object.DayOfBirth = parseInt(moment(object.BirthDay).format("DD"));
            }

            if (this.isEdit) {
                await hrParentInfoApi.UpdateParent(atid, object).then(() => {
                    this.$saveSuccess();
                    this.showDialog = false;
                });
            }
            else {
                await hrParentInfoApi.AddParent(object).then(() => {
                    this.$saveSuccess();
                    this.showDialog = false;
                });
            }
        });
    }

    //#region handle avatar
    getBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    getArrayBuffer(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsArrayBuffer(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    handleBeforeUploadAvatar(file) {

    }

    async onChangeAvatar(file, fileList) {
        const originCountFileList = fileList.length;
        if (fileList.length > 1) {
            fileList.splice(1, fileList.length);
        }
        const fileRaw = file.raw;
        const isJPG = fileRaw.type === 'image/jpeg';
        const isLt2M = fileRaw.size / 1024 / 1024 < 2;

        if (!isJPG) {
            this.$message.error('Avatar picture must be JPG format!');
            if (originCountFileList == 1) fileList.splice(0, 1);
            return;
        }
        if (!isLt2M) {
            this.$message.error('Avatar picture size can not exceed 2MB!');
            if (originCountFileList == 1) fileList.splice(0, 1);
            return;
        }

        fileList[0] = file;

        this.fileImageName = fileRaw.name;
        await this.getArrayBuffer(fileRaw).then(
            data => {
                Object.assign(this.formModel, { Avatar: Misc.arrayBufferToBase64(data) });
                this.errorUpload = false;
                this.$forceUpdate();
            }

        )
            .catch(e => console.log(e));
    }

    onRemoveAvatar(file, fileList) {
        Object.assign(this.formModel, { ImageUpload: '', Avatar: null });
        this.errorUpload = false;
        this.$forceUpdate();
    }
    //#endregion

    //#region Import excel
    importErrorMessage = "";
    showDialogImportError = false;
    showImportExcel = true;
    isAddFromExcel = false;
    isDeleteFromExcel = false;
    addedParams = [];
    formExcel = {};
    dataAddExcel = [];
    fileName = '';
    showDialogExcel = false;
    dataProcessedExcel = [];
    listExcelFunction = ['AddExcel', 'DeleteExcel', 'ExportExcel'];
    showOrHideImportError(obj) {
        this.showDialogImportError = obj;
    }
    UploadDataFromExcel() {
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Mã phụ huynh')) {
                a.EmployeeCode = this.dataAddExcel[0][i]['Mã phụ huynh'] + '';
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Mật khẩu')) {
                a.Password = this.dataAddExcel[0][i]['Mật khẩu'] + '';
            } else {
                a.Password = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên trên máy')) {
                a.NameOnMachine = this.dataAddExcel[0][i]['Tên trên máy'] + '';
            } else {
                a.NameOnMachine = '';
            }
            if (this.dataAddExcel[0][i]['Giới tính (Nam)'] == 'x') {
                a.Gender = 1;
            } else {
                a.Gender = 0;
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Email')) {
                a.Email = this.dataAddExcel[0][i]['Email'] + '';
            } else {
                a.Email = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Số DT')) {
                a.PhoneNumber = this.dataAddExcel[0][i]['Số DT'] + '';
            } else {
                a.PhoneNumber = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Ngày sinh (ngày/tháng/năm)')) {
                a.DateOfBirth = this.dataAddExcel[0][i]['Ngày sinh (ngày/tháng/năm)'] + '';
            } else {
                a.DateOfBirth = '';
            }
            if (this.dataAddExcel[0][i].hasOwnProperty('Phụ huynh của (Mã học sinh)')) {
                a.StudentCode = this.dataAddExcel[0][i]['Phụ huynh của (Mã học sinh)'] + '';
            } else {
                a.StudentCode = '';
            }

            arrData.push(a);
        }
        hrParentInfoApi.AddParentFromExcel(arrData).then((res) => {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileParentUpload')))) {
                (<HTMLInputElement>document.getElementById('fileParentUpload')).value = '';
            }
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileImageUpload')))) {
                (<HTMLInputElement>document.getElementById('fileImageUpload')).value = '';
            }

            this.showDialogExcel = false;
            this.fileName = '';
            this.dataAddExcel = [];
            this.isAddFromExcel = false;
            if (!isNullOrUndefined(res.status) && res.status === 200 && res.data == "") {
                this.$saveSuccess();
                this.loadData();
            } else {
                this.importErrorMessage = this.$t('ImportParentErrorMessage') + res.data.toString() + " " + this.$t('Parent');
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

    }
    async ExportToExcel() {
        this.addedParams = [];
        this.addedParams.push({ Key: "Filter", Value: this.filterModel.TextboxSearch });
        await hrParentInfoApi.ExportToExcel(this.addedParams).then((res) => {
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
    AddOrDeleteFromExcel(x) {
        if (x === 'close') {
            if (!isNullOrUndefined((<HTMLInputElement>document.getElementById('fileParentUpload')))) {
                (<HTMLInputElement>document.getElementById('fileParentUpload')).value = '';
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

    ShowOrHideDialogExcel(x) {
        if (x == 'open') {
            this.dataAddExcel = [];
            this.fileName = '';
            this.showDialogExcel = true;
        }
        else {
            (<HTMLInputElement>document.getElementById('fileParentUpload')).value = '';
            this.showDialogExcel = false;
        }

    }
    handleCommand(command) {
        if (command === 'AddExcel') {
            this.AddOrDeleteFromExcel('add');
        }
        else if (command === 'ExportExcel') {
            this.ExportToExcel();
        }
        else if (command === 'DeleteExcel') {
            this.AddOrDeleteFromExcel('delete');
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
    async AutoSelectFromExcel() {
        this.dataProcessedExcel = [];
        var regex = /^\d+$/;
        if (this.dataAddExcel.length == 0) {
            return this.$alertSaveError(null, null, null, this.$t('NoFileUpload').toString()).toString();

        }
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
            if (this.dataAddExcel[0][i].hasOwnProperty('Tên nhân viên')) {
                a.FullName = this.dataAddExcel[0][i]['Tên nhân viên'] + '';
            } else {
                a.FullName = '';
            }
            this.dataProcessedExcel.push(a);
        }
        // Handle after upload 
    }
    //#endregion

    //#region Show detail popup when click on student
    showDetailPopup = false;
    gridColumnsDetail = [
        {
            name: 'Avatar',
            dataField: 'Avatar',
            dataType: 'image',
            fixed: false,
            width: 150,
            show: true,
        },
        {
            name: 'UserCode',
            dataField: 'EmployeeATID',
            fixed: false,
            width: 150,
            show: true,
        },
        {
            name: 'FullName',
            dataField: 'FullName',
            fixed: false,
            width: 150,
            show: true,
        },
        {
            name: 'Gender',
            dataField: 'Gender',
            dataType: 'lookup',
            fixed: false,
            width: 150,
            show: true,
            lookup: {
                dataSource: {
                    1: { Index: 1, Name: 'Nam', NameInEng: 'Male' },
                    0: { Index: 0, Name: 'Nữ', NameInEng: 'Female' },
                },
                displayMember: 'Name',
                valueMember: 'Index',
            }
        },
        {
            name: 'Class',
            dataField: 'ClassID',
            dataType: 'lookup',
            fixed: false,
            width: 150,
            show: true,
            lookup: {
                dataSource: this.allClassLookup,
                displayMember: 'Name',
                valueMember: 'Index',
            }
        },
        {
            name: 'Password',
            dataField: 'Password',
            fixed: false,
            width: 150,
            show: true,
        },
        {
            name: 'CardNumber',
            dataField: 'CardNumber',
            fixed: false,
            width: 150,
            show: true,
        },
        {
            name: 'NameOnMachine',
            dataField: 'NameOnMachine',
            fixed: false,
            width: 150,
            show: true,
        },
    ];
    dataSourceDetail = [];
    closeDetailPopup() {
        this.showDetailPopup = false;
    }
    viewDetailPopup(item) {
        this.dataSourceDetail = [];
        if (item.Students.length > 0) {
            item.Students.map(x => {
                if (x != undefined && x != "") {
                    this.dataSourceDetail.push(this.allStudentLookup[x])
                }
            })
        }
        this.showDetailPopup = true;
    }
    //#endregion
}
