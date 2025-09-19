import axios from 'axios';
import { Component, Ref, Vue } from 'vue-property-decorator';
import { Form } from 'element-ui';
import HeaderComponent from '@/components/home/header-component/header-component.vue';
import DepartmentTree from '@/components/app-component/department-tree-component/department-tree-component.vue';
import { hrUserApi } from '@/$api/hr-user-api';

@Component({
	name: 'report',
	components: { HeaderComponent, DepartmentTree },
})
export default class ReportPage extends Vue {
	@Ref('param-form')
	paramsForm!: Form;
	reportHost: any = {};
    optionReport: any = {};
    
	typeReport: any = '';

	listTypeReport = [];
	listReport = [];
	selectedReport: any = '';
	lookupReport = {};

	nameReport = '';
	reportId = '';
	clientName= '';
	listNote = [];
	listNoteLookup = [];

	reportParamsValue: any[] = [];
	reportParams: any[] = [];
	
	loadingParam: boolean = false;
    
	listDepartmentSelected = [];
	listEmployeeSelected = [];

	masterEmployeeFilter = [];

	async beforeMount() {
		this.loadNotePSV();
		Misc.readFileAsync('static/variables/common-utils.json').then(x => {
            this.clientName = x.ClientName;
        });
        this.reportHost = await Misc.readFileAsync('static/variables/ssrs.json');
        this.reportHost = Object.assign(this.reportHost, { Auth: { username: 'admin', password: 'admin' } });
        this.loadReportListAdvance('');
		// this.loadReportList();
	}

	get getReportLink() {
		const { ReportDomain, Port, View, Path, Folder } = this.reportHost;
		const rp = this.lookupReport[this.selectedReport];
		if(rp != null && rp != undefined) {
			const url = `${ReportDomain}:${Port}/${View}?/${Folder}/${this.typeReport}/${rp.Name}`;
			return url;
		}
	}
	
	// get getReportLink() {
	// 	const { ReportDomain, Port, View, Path, Folder } = this.reportHost;
	// 	const url = `${ReportDomain}:${Port}/${View}?/${Folder}/${this.typeReport.label}/${this.report.label}`;
	// 	return url;
	// }

	changeLocation(ev){
		if(!ev || ev.length == 0){
			this.listNote = [...this.listNoteLookup];
		}else{
			this.listNote = this.listNoteLookup.filter(x => ev.includes(x.Area));
		}
	}

	get reportApi() {
		const { ReportDomain, Port, Api } = this.reportHost;
		return `${ReportDomain}:${Port}/${Api}`;
	}

	async loadReportList() {
		//Lấy danh sách loại báo báo và báo cáo
		//Nếu folderName === '' thì là lấy loại báo cáo
		const { ReportDomain, Port, Api, Folder, Auth } = this.reportHost;
        const apiUrl = `${ReportDomain}:${Port}/${Api}//Folders(path='/${Folder}')/CatalogItems`;

		await axios
			.get(apiUrl, { auth: Auth })
			.then((res: any) => {
				const listTemp = res.data.value.map((item) => {
					this.lookupReport[item.Id] = item;
					return {
						value: item.Id,
						label: item.Description,
						Path: item.Path,
						Name: item.Name
					}
				});
				this.listReport = listTemp;
			})
			.catch((e) => {});
	}

	async loadReportListAdvance(folderName) {
		console.log("ReportPage ~ loadReportListAdvance ~ folderName:", folderName)
		//Lấy danh sách loại báo báo và báo cáo
		//Nếu folderName === '' thì là lấy loại báo cáo
		const { ReportDomain, Port, Api, Folder, Auth } = this.reportHost;
        // const apiUrl = `${ReportDomain}:${Port}/${Api}//Folders(path='/${Folder}')/CatalogItems`;

		let apiUrl = '';
		if (folderName === '') {
			apiUrl = `${ReportDomain}:${Port}/${Api}//Folders(path='/${Folder}')/CatalogItems`;
		} else {
			apiUrl = `${ReportDomain}:${Port}/${Api}//Folders(path='/${Folder}/${folderName}')/CatalogItems`;
		}

		await axios
			.get(apiUrl, { auth: Auth }) 
			.then((res: any) => {
				const listTemp = res.data.value.map((item) => {
					this.lookupReport[item.Id] = item;
					return {
						value: item.Id,
						label: item.Description,
						Path: item.Path,
						Name: item.Name
					}
				});
				console.log('listTemp', listTemp)
				if (folderName === '') {
					this.listTypeReport = listTemp;
				} else {
					this.listReport = listTemp;
				}
			})
			.catch((e) => {});
	}



	selectTypeReport() {
		this.loadReportListAdvance(this.typeReport);
	}

	async _handleReportChange() {
		if (Misc.isEmpty(this.selectedReport)) return;
		this.loadingParam = true;
		this.reportParamsValue = [];
		this.reportParams = [];
		const { ReportDomain, Port, Api, Auth } = this.reportHost;
		const apiUrl = `${ReportDomain}:${Port}/${Api}/Reports(${this.selectedReport})/ParameterDefinitions`;
		await axios.get(apiUrl, { auth: Auth }).then((rep: any) => {

			rep.data.value.forEach((item: any) => {
				let a: any = '';
				if (item.Name.toLowerCase().indexOf('date') > -1) {
					a = moment(new Date()).format('YYYY-MM-DD');
				} else if (item.Name.toLowerCase().indexOf('month') > -1) {
					a = moment(new Date()).format('YYYY-MM');
				} else if (item.Name.toLowerCase().indexOf('year') > -1) {
					a = moment(new Date()).format('YYYY');
				} else if (item.MultiValue === true) {
					a = [];
				}
				this.reportParamsValue.push(a);
				const b = item;
				if (item.ValidValues.length > 0) {
					b.ValidValues.forEach((it) => {
						it['value'] = it['Value'];
						it['label'] = it['Label'];
					});
				}

				// if(item.Name === 'SpecialParamDepartmentNoteSelected' && this.clientName == 'PSV'){
				// 	this.listNote = b.ValidValues;
				// 	this.listNoteLookup = b.ValidValues;
				// }

				this.reportParams.push(b);
			});
		})
		.finally(() => {
			this.loadingParam = false;
		})
    }

	async loadNotePSV() {
		return await hrUserApi.GetAllNote().then((rep: any) => {
			const data = rep.data;
				this.listNote = data;
				this.listNoteLookup = data;
			
			//console.log(this.listDepartmentAEON)
		})
	}

	async submit(e) {
		e.preventDefault();
		setTimeout(() => {
			const form = this.paramsForm.$el as any;
			form.submit();
		}, 100);
	}

	get hasDepartmentSelect() {
		return this.reportParams.some((item) => item.Name === 'Department');
	}
	get hasEmployeeSelect() {
		return this.reportParams.some((item) => item.Name === 'SpecialParamEmployeeSelected');
    }
    
    _selectDepartmentVisibleChange(val) {
        if(val === true) return;
        const deptTree = this.$refs.treeDepartment as any;
        this.listDepartmentSelected = deptTree.getSelectedDepartment();
    }

    _selectEmployeeVisibleChange(val) {
        if(val === true) return;
        const empTree = this.$refs.treeEmployee as any;
        this.listEmployeeSelected = empTree.getSelectedEmployee();
    }

	getDateTimeFor(stringDateTime) {
		return moment(stringDateTime).format('YYYY-MM-DD');
    }
    
    selectAll(index) {
        if(this.reportParams[index].MultiValue === false) return;
        this.reportParamsValue[index].length = 0;
        this.reportParams[index].ValidValues.forEach(e => {
            this.reportParamsValue[index].push(e.Value);
        });
    }

	selectAllNote(index) {
        if(this.reportParams[index].MultiValue === false) return;
        this.reportParamsValue[index].length = 0;
        this.listNote.forEach(e => {
            this.reportParamsValue[index].push(e.Note);
        });
    }
}
