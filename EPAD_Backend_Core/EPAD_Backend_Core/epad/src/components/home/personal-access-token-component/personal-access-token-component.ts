import { Component, Vue, Mixins } from 'vue-property-decorator'
import ComponentBase from '@/mixins/application/component-mixins'
import HeaderComponent from '@/components/home/header-component/header-component.vue'
import DataTableComponent from '@/components/home/data-table-component/data-table-component.vue'
import { settingApi, PersonalAccessTokenModel } from '@/$api/setting-api'
import { isNullOrUndefined } from 'util'
import { Form as ElForm } from 'element-ui'
import DataTableFunctionComponent from '@/components/home/data-table-component/data-table-function-component.vue'
import moment from 'moment'

@Component({
	name: 'personal-access-token',
	components: { HeaderComponent, DataTableComponent, DataTableFunctionComponent },
})
export default class PersonalAccessTokenComponent extends Mixins(ComponentBase) {
	showDialog = false
	ruleForm: any = {}
	rules: any = {}
	page = 1
	columns = []
	rowsObj = []
	listExcelFunction = []
	showDialogToken = false
	tokenForm: any = {
		token: '',
	}

	beforeMount() {
		this.initForm()
		this.initRule()
	}
	mounted() {
		this.setColumns()
	}
	initForm() {
		this.ruleForm = {
			Name: '',
			Scopes: '',
			Note: '',
			ExpiredDate: moment(new Date()).format('YYYY-MM-DD'),
		}
	}
	initRule() {
		this.rules = {
			Name: [
				{
					required: true,
					message: this.$t('PleaseInputName'),
					trigger: 'change',
				},
			],
			// Scopes: [
			// 	{
			// 		required: true,
			// 		message: this.$t('PleaseInputScopes'),
			// 		trigger: 'change',
			// 	},
			// ],
			ExpiredDate: [
				{
					required: true,
					message: this.$t('PleaseSelectExpiredDate'),
					trigger: 'change',
				},
			],
		}
	}

	setColumns() {
		this.columns = [
			{
				prop: 'Name',
				label: 'Name',
				minWidth: 150,
				display: true
			},
			{
				prop: 'ExpiredDate',
				label: 'ExpiredDate',
				minWidth: 150,
				display: true
			},
			{
				prop: 'CreatedDate',
				label: 'CreatedDate',
				minWidth: 150,
				display: true
			},
			{
				prop: 'Note',
				label: 'Note',
				minWidth: 150,
				display: true
			},
		]
	}

	copyClipboard() {
		;(this.$refs.token as any).select()
		document.execCommand('copy')
	}
	async getData({ page, filter, sortParams,pageSize }) {
		// this.page = page;

		return await settingApi.GetPersonalAccessToken(filter,pageSize).then((res) => {
			const { data } = res as any
			const arrTemp = []

			return {
				data: data,
				total: 0,
			}
		})
	}
	submit() {
		;(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) {
				return
			} else {
				const data = Object.assign(
					this.ruleForm,
					{
						ExpiredDate: new Date(moment(this.ruleForm.ExpiredDate).format('YYYY-MM-DD')),
					},
					{}
				)
				await settingApi
					.AddPersonalAccessToken(this.ruleForm)
					.then((res) => {
						this.Cancel()
						this.showDialogToken = true
						this.tokenForm = Object.assign(
							{},
							{
								token: res.data,
							},
							{}
						)
					})
					.catch()
			}
		})
	}

	Insert() {
		this.showDialog = true
	}
	Edit() {}
	Delete() {
		const obj: PersonalAccessTokenModel[] = JSON.parse(JSON.stringify(this.rowsObj))
		if (obj.length < 1) {
			this.$alertSaveError(null, null, null, this.$t('ChooseRowForDelete').toString())
		} else if (obj.length > 1) {
			this.$alertSaveError(null, null, null, this.$t('MSG_SelectOnlyOneRow').toString())
		} else {
			this.$confirmDelete().then(async () => {
				await settingApi
					.RevokeAccessToken(obj[0].Index)
					.then((res) => {
						;(this.$refs.table as any).getTableData(this.page, null, null)
						if (!isNullOrUndefined(res.status) && res.status === 200) {
							this.$deleteSuccess()
						}
					})
					.catch(() => {})
			})
		}
	}
	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm
		ref.resetFields()
		this.initForm()
		this.showDialog = false
	}

	CancelViewToken() {
		this.showDialogToken = false
		this.tokenForm = Object.assign(
			{},
			{
				token: '',
			},
			{}
		)
	}

	focus(x) {
		var theField = eval('this.$refs.' + x)
		theField.focus()
	}
}
