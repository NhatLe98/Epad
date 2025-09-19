import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';
import { IC_GroupDevice } from '@/$api/group-device-api';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { AC_AreaDTO, areaApi } from '@/$api/ac-area-api';

@Component({
	name: 'area',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class AreaComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: AC_AreaDTO = {
		Name: '',
		Description: '',
	};
	rules: any = {};

	beforeMount() {
		this.initColumns();
		this.initRule();
	}
	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
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
				prop: 'Name',
				label: 'AreaName',
				minWidth: 100,
				display: true
			},
			{
				prop: 'Description',
				label: 'Description',
				minWidth: 300,
				display: true
			},
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await areaApi.GetAreaAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	reset() {
		const obj: AC_AreaDTO = {};
		this.ruleForm = obj;
	}

	Insert() {
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			console.log('ruleForm', this.ruleForm);
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await areaApi.UpdateArea(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await areaApi
						.AddArea(this.ruleForm)
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
		const obj: AC_AreaDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		console.log(obj);
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await areaApi
					.DeleteArea(obj)
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
		this.showDialog = false;
	}
}
