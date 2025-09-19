import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { TA_LeaveDateTypeDTO, leaveDateTypeApi } from '@/$api/ta-leave-type-api';

@Component({
	name: 'leaveType',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class LeaveTypeComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: TA_LeaveDateTypeDTO = {
		Code: '',
		Name: '',
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
					message: this.$t('PleaseInputLeaveTypeCode'),
					trigger: 'blur',
				},
				{
					required: true,
					trigger: 'change',
					validator: (rule, value: any, callback) => {
						if(!value || !value.trim()){
							callback(new Error(this.$t('PleaseInputLeaveTypeCode').toString()));
						}
						callback();
					},
				}
			],
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputLeaveTypeName'),
					trigger: 'blur',
				},
				{
					required: true,
					trigger: 'change',
					validator: (rule, value: any, callback) => {
						if(!value || !value.trim()){
							callback(new Error(this.$t('PleaseInputLeaveTypeName').toString()));
						}
						callback();
					},
				}
			],
		};
	}

	mounted() {

	}

	initColumns() {
		this.columns = [
			{
				prop: 'Code',
				label: 'LeaveTypeCode',
				minWidth: 200,
				display: true
			},
			{
				prop: 'Name',
				label: 'LeaveTypeName',
				minWidth: 300,
				display: true
			},
			{
				prop: 'IsWorkedTimeHoliday',
				label: 'WorkedTimeHoliday',
				minWidth: 300,
				display: true,
				dataType: "yesno"
			},
			{
				prop: 'IsPaidLeave',
				label: 'PaidLeave',
				minWidth: 300,
				display: true,
				dataType: "yesno"
			},
			{
				prop: 'IsOptionHoliday',
				label: 'OptionHoliday',
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
		return await leaveDateTypeApi.GetLeaveDateTypeAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});

	}

	reset() {
		const obj: TA_LeaveDateTypeDTO = {
			Name: '',
			Code: '',
			IsOptionHoliday: false,
			IsPaidLeave: false,
			IsWorkedTimeHoliday: false,
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
				if (this.isEdit == true) {
					return await leaveDateTypeApi.UpdateLeaveDateType(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await leaveDateTypeApi
						.AddLeaveDateType(this.ruleForm)
						.then((res) => {
							(this.$refs.table as any).getTableData(this.page, null, null);
							this.showDialog = false;
							this.reset();
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						})
						.catch(() => { });
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
				await leaveDateTypeApi
					.DeleteLeaveDateType(obj)
					.then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess();
						}
					})
					.catch(() => { });
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
