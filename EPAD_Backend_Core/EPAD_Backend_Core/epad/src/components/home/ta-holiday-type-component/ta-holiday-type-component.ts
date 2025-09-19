import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { TA_HolidayDTO, holidayApi } from '@/$api/ta-holiday-api';

@Component({
	name: 'holiday',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class HolidayTypeComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: TA_HolidayDTO = {
		Name: '',
		HolidayDate: new Date(),
	};
	rules: any = {};

	beforeMount() {
		this.initColumns();
		this.initRule();
	}
	initRule() {
		this.rules = {
			Code: [
				{
					required: true,
					message: this.$t('PleaseInputHolidayTypeCode'),
					trigger: 'blur',
				},
				{
					required: true,
					trigger: 'change',
					validator: (rule, value: any, callback) => {
						if(!value || !value.trim()){
							callback(new Error(this.$t('PleaseInputHolidayTypeCode').toString()));
						}
						callback();
					},
				}
			],
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputHolidayNameType'),
					trigger: 'blur',
				},
				{
					required: true,
					trigger: 'change',
					validator: (rule, value: any, callback) => {
						if(!value || !value.trim()){
							callback(new Error(this.$t('PleaseInputHolidayNameType').toString()));
						}
						callback();
					},
				}
			],
			HolidayDate: [
				{
					required: true,
					message: this.$t('PleaseSelectDate'),
					trigger: 'blur',
				},
			],
		};
	}

	mounted() {
		
	}

	initColumns(){
		this.columns = [
			{
				prop: 'Code',
				label: 'HolidayTypeCode',
				minWidth: 200,
				display: true
			},
			{
				prop: 'Name',
				label: 'HolidayNameType',
				minWidth: 300,
				display: true
			},
            {
				prop: 'HolidayDateString',
				label: 'DateHoliday',
				minWidth: 300,
				display: true
			},
            {
				prop: 'IsPaidWhenNotWorking',
				label: 'CalculateWhenNotWork',
				minWidth: 300,
				display: true,
				dataType: "yesno"
			},
            {
				prop: 'IsRepeatAnnually',
				label: 'LoopEveryYear',
				minWidth: 300,
				display: true,
				dataType: "yesno"
			},
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await holidayApi.GetHolidayAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	reset() {
		const obj: TA_HolidayDTO = {
			Name: '',
			Code: '',
			HolidayDate: new Date(),
			HolidayDateString: '',
			IsPaidWhenNotWorking: false,
			IsRepeatAnnually: false,
		};
		this.ruleForm = obj;
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			// console.log('ruleForm', this.ruleForm);
			if (!valid) return;
			else {
				this.ruleForm.HolidayDateString = moment(this.ruleForm.HolidayDate).format("YYYY-MM-DD");
				if (this.isEdit == true) {
					return await holidayApi.UpdateHoliday(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await holidayApi
						.AddHoliday(this.ruleForm)
						.then((res) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						})
						.catch(() => {});
				}
			}
		});
	}

	Edit() {
		this.isEdit = true;
		var obj = JSON.parse(JSON.stringify(this.rowsObj));

		if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString());
		} else if (obj.length === 1) {
			this.showDialog = true;
			this.ruleForm = obj[0];
		} else {
			this.$alertSaveError(null, null, null, this.$t('ChooseUpdate').toString());
		}
	}


	async Delete() {
		const obj = JSON.parse(JSON.stringify(this.rowsObj.map(x => x.Index)));
		// console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await holidayApi
					.DeleteHoliday(obj)
					.then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
					})
					.catch(() => {});
			});
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.reset();
		this.showDialog = false;
	}
}
