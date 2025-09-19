import { Component, Mixins } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue';

import { Form as ElForm } from 'element-ui';
import { isNullOrUndefined } from 'util';
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue';
import { areaApi } from '@/$api/ac-area-api';
import { doorDeviceApi } from '@/$api/ac-door-device-api';
import { timezoneApi } from '@/$api/ac-timezone-api';
import { AC_AreaLimitedDTO, areaLimitedApi } from '@/$api/ac-area-limited-api';
import { doorApi } from '@/$api/ac-door-api';

@Component({
	name: 'area-limited',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class AreaLimitedComponent extends Mixins(ComponentBase) {
	page = 1;
	showDialog = false;
	showMessage = false;
	checked = false;
	columns = [];
	rowsObj = [];
	isEdit = false;
	listExcelFunction = [];
	ruleForm: AC_AreaLimitedDTO = {
		Name: '',
		Description: '',
		DoorIndexes: []
	};
	rules: any = {};
	listAllArea: [];
	listDevices: [];
	listTimezone: [];
	listRange = [
		{ label: 'Mốc thời gian 1', value: 1 },
		{ label: 'Mốc thời gian 2', value: 2 },
		{ label: 'Mốc thời gian 3', value: 3 }
	]
	allDoorLst = [];
	async beforeMount() {
		this.initColumns();
		this.initRule();
		await this.LoadArea();
		await this.getAllDoor();
	}

	async LoadArea() {
		return await areaApi.GetAllArea().then((res) => {
			const { data } = res as any;
			this.listAllArea = data;
		})
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
				label: 'Name',
				minWidth: 120,
				display: true
			},
			{
				prop: 'DoorName',
				label: 'DoorName',
				minWidth: 120,
				display: true
			},
			{
				prop: 'Description',
				label: 'Description',
				minWidth: 100,
				display: true
			},
		];
	}

	displayPopupInsert() {
		this.showDialog = false;
	}

	async getData({ page, filter, sortParams, pageSize }) {
		this.page = page;
		return await areaLimitedApi.GetAreaLimitedAtPage(page, filter, pageSize).then((res) => {
			const { data } = res as any;
			return {
				data: data.data,
				total: data.total,
			};
		});
	}

	async getAllDoor() {
		doorApi.GetAllDoor().then(res => {
			const { data } = res as any;
			this.allDoorLst = data;
		})
	}

	reset() {
		const obj: AC_AreaLimitedDTO = {
			Name: '',
			Description: '',
			DoorIndexes: [],
		};
		this.ruleForm = obj;
	}

	async Insert() {
		this.listDevices = [];
		this.showDialog = true;
		this.isEdit = false;
		this.reset();
	}

	async Submit() {
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) return;
			else {
				if (this.isEdit == true) {
					return await areaLimitedApi.UpdateAreaLimited(this.ruleForm).then((res) => {
						(this.$refs.table as any).getTableData(this.page, null, null);
						this.showDialog = false;
						this.reset();
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$saveSuccess();
						}
					});
				} else {
					return await areaLimitedApi
						.AddAreaLimited(this.ruleForm)
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

	async Edit() {
		this.listDevices = [];
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
		const obj: AC_AreaLimitedDTO[] = JSON.parse(JSON.stringify(this.rowsObj));
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString());
		} else {
			this.$confirmDelete().then(async () => {
				await areaLimitedApi
					.DeleteAreaLimited(obj)
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
		this.showDialog = false;
	}
}
