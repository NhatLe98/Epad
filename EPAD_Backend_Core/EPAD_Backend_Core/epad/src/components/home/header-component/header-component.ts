import { Component, Vue, Mixins, Prop } from 'vue-property-decorator';
import ComponentBase from '@/mixins/application/component-mixins';
import NotifyPopupComponent from "@/components/app-component/notify-popup-component/notify-popup-component.vue";
import { userAccountApi, IPasswordInfo } from '@/$api/user-account-api';
import { Mutation, Getter } from 'vuex-class';
import { isNullOrUndefined } from 'util';
import { Form as ElForm } from 'element-ui';
import { licenseApi } from '@/$api/license-api';
import { employeeInfoApi } from "@/$api/employee-info-api";
import { map } from 'jquery';

@Component({
	name: 'header-component',
	components: { NotifyPopupComponent },
})
export default class HeaderComponent extends Mixins(ComponentBase) {
	@Mutation('setUser', { namespace: 'User' }) $setUser;
	@Getter('getUser', { namespace: 'User' }) $getUser;
	@Prop({default: false}) showMasterEmployeeFilter: Boolean;

	enableMasterEmployeeFilter = false;

	loading_button = false;
	Username = 'User';
	showDialog = false;
	showDialogNotify = false;
	value = '';
	user = {};
	isUploading = false;
	numberOfNotify = null;
	usingSSO = false;
	options = [
		{
			value: 'vi',
			label: 'Tiếng Việt',
		},
		{
			value: 'en',
			label: 'English',
		},
	];

	ruleForm: IPasswordInfo = {
		UserName: '',
		Password: '',
		Email: '',
		NewPassword: '',
		ConfirmPassword: '',
		ServiceId: 0,
	};

	rules: any = {};

	expandedKey = [-1];
	loadingTree = false;
	treeData: any = [];
	filterTree = "";
	selectedLabel = [];
	key: any = [];
	arrData: any;
	defaultChecked = [];

	async loadDepertmentTree() {
		this.loadingTree = true;
		//Don't get info employee in department driver
		return await employeeInfoApi
			.GetEmployeeAsTree(8)
			.then((res) => {
				this.loadingTree = false;
				const data = res.data as any;
				this.treeData = data;
				if(this.treeData && this.showMasterEmployeeFilter){
					this.arrData = this.flattenArray(this.treeData);
					const jsonSessionMasterEmployeeFilter = localStorage.getItem("masterEmployeeFilter");
					if(jsonSessionMasterEmployeeFilter && jsonSessionMasterEmployeeFilter.trim().length > 0){
						const sessionMasterEmployeeFilter = JSON.parse(jsonSessionMasterEmployeeFilter);
						this.key = sessionMasterEmployeeFilter;
						this.defaultChecked = this.arrData.filter(x => sessionMasterEmployeeFilter.includes(x.EmployeeATID))
							?.map(x => x.ID) ?? [];
						if(this.defaultChecked && this.defaultChecked.length > 0){
							const result = this.findParentID(this.defaultChecked);
							if(result && result.length > 0){
								setTimeout(() => {
									const tree = (this.$refs.masterEmployeeTree as any);
									result.forEach(element => {
										// tree.store.nodesMap[element].checked = true;
										tree.store.nodesMap[element].expanded = true;
										tree.store.nodesMap[element].loaded = true;
									});
								}, 500);
							}
						}
					}
				}
				// console.log(this.treeData)
				//this.treeData[0] = this.GetListChildrent(data[0]);
			})
			.catch(() => {
				this.loadingTree = false;
			});
	}

	flattenArray(data, parentId = null, result = []) {
        const cloneData = Misc.cloneData(data);
        cloneData.forEach(item => {
            // Create a copy of the item to avoid mutating the original data
            // const newItem = { ...item };
            const newItem = Misc.cloneData(item);

            // Set the parentIndex property
            newItem.ParentID = parentId;

            // Remove the children property from the new item
            delete newItem.ListChildrent;

            // Add the new item to the result array
            result.push(newItem);

            // Get the current item's index in the result array
            const currentIndex = item.ID;

            // If the item has a children array, recursively flatten it
            if (Array.isArray(item.ListChildrent) && item.ListChildrent.length > 0) {
                this.flattenArray(item.ListChildrent, currentIndex, result);
            }

            // delete item.ListChildrent;
        });

        return result;
    }

	findParentID(arrID){
		let result = [];
		const parentIDs = this.arrData.filter(x => arrID.includes(x.ID))?.map(x => x.ParentID) ?? [];
		if(parentIDs && parentIDs.length > 0){
			result = result.concat(parentIDs);
			if(this.arrData.some(x => parentIDs.includes(x.ID) && x.ParentID)){
				const nestParentIDs = this.findParentID(parentIDs);
				if(nestParentIDs && nestParentIDs.length > 0){
					result = result.concat(nestParentIDs);
				}
			}
		}
		result = [...new Set(result)];
		return result;
	}

	loadedMasterFilter = false;
	updatePadding() {
		this.$nextTick(() => {
			const textSpan = document.getElementById('FormName');
			const childDiv = document.getElementById('masterEmployeeFilter');
			
			// Get the width of the text span
			let textWidth = textSpan.clientWidth;
			if(textWidth){
				textWidth += 20;
				if(childDiv){
					childDiv.style.left = `${textWidth}px`;
				}
			}
			this.loadedMasterFilter = true;
		})
	}
	clear(){
		this.key = [];
		this.defaultChecked = [];
		localStorage.removeItem("masterEmployeeFilter");
		(this.$refs.masterEmployeeTree as any).setCheckedKeys([]);
	}

	getIconClass(type, gender) {
        switch (type) {
            case "Company":
                return "el-icon-office-building";
                break;
            case "Department":
                return "el-icon-s-home";
                break;
            case "Employee":
                if (isNullOrUndefined(gender) || gender === "Other") {
                    return "el-icon-s-custom employee-other";
                } else if (gender === "Male") {
                    return "el-icon-s-custom employee-male";
                } else {
                    return "el-icon-s-custom employee-female";
                }
        }
    }

	handleScroll() {
        this.$refs.scrollbar && (this.$refs.scrollbar as any).handleScroll();
    }

	filterNode(value, data) {
        if (!value) return true;
        return (data.Name.indexOf(value) !== -1 || (!isNullOrUndefined(data.EmployeeATID) && data.EmployeeATID.indexOf(value) !== -1));

    }
    async filterTreeData() {
        (this.$refs.masterEmployeeTree as any).filter(this.filterTree);
    }
    // TODO
    async loadNode(node, resolve) {
        //console.log(node)
        //console.log(this.treeData[0])
        if (node.level === 1) {
            return resolve(this.treeData[0].ListChildrent);
        }
        //let data = this.treeData.filter(e => e.Level === node.level);
        //return resolve(data);
        //resolve(this.treeData[0]);
        //if (node.level > 10) return resolve([]);
        //console.log(this.treeData[0].ListChildrent);
        //setTimeout(() => {
        //    //const data = [{
        //    //    name: 'leaf',
        //    //    leaf: true
        //    //}, {
        //    //    name: 'zone'
        //    //}];
        //    const index = node.level;

        //    if (this.treeData[index].ListChildrent.length > 0) {
        //       // resolve(this.treeData[0].ListChildrent);
        //    }
        //}, 500);
    }

	loadingEffect(x) {
        const loading = this.$loading({
            lock: true,
            text: "Loading",
            spinner: "el-icon-loading",
            background: "rgba(0, 0, 0, 0.7)",
        });
        setTimeout(() => {
            loading.close();
        }, x);
    }

	async nodeCheck(e) {
        this.loadingEffect(500);
        if (!this.filterTree) {
            this.key = (this.$refs.masterEmployeeTree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee')
                .map((e) => e.EmployeeATID);
        }
        else {
            this.key = (this.$refs.masterEmployeeTree as any)
                .getCheckedNodes()
                .filter((e) => e.Type == 'Employee' && (e.Name.indexOf(this.filterTree) !== -1) || (!isNullOrUndefined(e.EmployeeATID) && e.EmployeeATID.indexOf(this.filterTree) !== -1))
                .map((e) => e.EmployeeATID);
        }
		if(this.key && this.key.length > 0){
			localStorage.setItem("masterEmployeeFilter", JSON.stringify(this.key));
		}else{
			localStorage.removeItem("masterEmployeeFilter");
		}
    }

	beforeMount() {
		// console.log(this.showMasterEmployeeFilter)
		Misc.readFileAsync('static/variables/app.host.json').then(x => {
			this.usingSSO = x.UsingSSO;
		});
		Misc.readFileAsync('static/variables/common-utils.json').then(x => {
			console.log(x)
            this.enableMasterEmployeeFilter = x?.EnableMasterEmployeeFilter ?? false;
			if(!this.enableMasterEmployeeFilter){
				localStorage.removeItem("masterEmployeeFilter");
			}
			console.log(this.enableMasterEmployeeFilter)
			if(this.enableMasterEmployeeFilter && this.showMasterEmployeeFilter){
				setTimeout(() => {
					console.log("a")
					this.updatePadding();
				}, 500);
			}
        });
		this.initRule();
		this.loadDepertmentTree();
	}

	initRule() {
		this.rules = {
			Password: [
				{
					required: true,
					message: this.$t('PleaseInputOldPassword'),
					trigger: 'blur',
				},
			],
			NewPassword: [
				{
					required: true,
					message: this.$t('PleaseInputNewPassword'),
					trigger: 'blur',
				},
				{
					message: this.$t('NewPasswordMustNotMatchOldPassword'),
					validator: (rule, value: string, callback) => {
						if (value == this.ruleForm.Password && isNullOrUndefined(value) === false) {
							callback(new Error());
						}
						callback();
					},
				},
				{
					message: this.$t('PasswordBinding'),
					validator: (rule, value: string, callback) => {
						const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
						if (isNullOrUndefined(value) === false && (value.length < 8 || regex.test(value) === false)) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
			ConfirmPassword: [
				{
					required: true,
					message: this.$t('PleaseInputPasswordConfirm'),
					trigger: 'change',
				},
				{
					message: this.$t('PasswordConfirmNotMatch'),
					validator: (rule, value: string, callback) => {
						if (value != this.ruleForm.NewPassword && isNullOrUndefined(value) === false) {
							callback(new Error());
						} else {
							callback();
						}
					},
				},
			],
		};
	}

	Cancel() {
		var ref = <ElForm>this.$refs.ruleForm;
		ref.resetFields();
		this.showDialog = false;
	}
	async submit() {
		var theField = this.$refs.ConfirmPassword as any;
		theField.blur();
		(this.$refs.ruleForm as any).validate(async (valid) => {
			if (!valid) {
				return;
			} else {
				if (this.isUploading === false) {
					this.isUploading = true;
					this.ruleForm.UserName = this.$getUser;
					this.loading_button = true;

					await userAccountApi
						.ChangePassword(this.ruleForm)
						.then((res) => {
							this.isUploading = false;
							this.showDialog = false;
							if (!isNullOrUndefined(res.status) && res.status === 200) {
								this.$saveSuccess();
							}
						})
						.catch((err) => {
							this.$alertSaveError(null, err);
							this.isUploading = false;
						});
				}
			}
		});
	}

	mounted() {
		var lang = this.$i18n.locale;
		this.value = lang;
		this.loadInFo();
	}

	async loadInFo() {
		return await userAccountApi.GetUserAccountInfo().then((res: any) => {
			this.Username = res.data.Name;
			this.$setUser(res.data.UserName);
		});
	}

	change() {
		var lang = this.$i18n.locale;
		if (lang != this.value) {
			this.$i18n.locale = this.value;
			localStorage.setItem('lang', this.value);
			this.$emit('changeLang', this.value);
		}
	}

	handleCommand(command) {
		if (command == 'Logout') {
			localStorage.removeItem("masterEmployeeFilter");
			if (this.usingSSO) {
			  window.location.replace(window.location.origin + "/signout-oidc");
			} 
			else {
				self.localStorage.removeItem('access_token');
				this.$router.push({
					name: 'login',
					query: { redirect: this.$router.currentRoute.path },
				});
			}
		} else if (command === 'ChangePassword') {
			this.showDialog = true;
		} else if( command === 'Version'){
			licenseApi.GetVersion().then(res => {
				if((res.data as any).includes('License: ')){
					let content = (res.data as any).split('License: ')[0].toString() + 'License: ';
					if(parseInt((res.data as any).split('License: ')[1].split('/')[2]) >= 2099){
						content += this.$t('ForeverLicense').toString();
					}else{
						content += (res.data as any).split('License: ')[1];
					}
					this.$alert(`${content}`, this.$t('Version').toString(), {
						dangerouslyUseHTMLString: true
					});
				}else{
					this.$alert(`${res.data}`, this.$t('Version').toString(), {
						dangerouslyUseHTMLString: true
					});
				}
			})
		}
	}

	focus(x) {
		var theField = eval('this.$refs.' + x);
		theField.focus();
	}

	setNumberOfNotify(value) {
		this.numberOfNotify = value;
	}



	showNotifyDialog() {
		const notifyDialog = this.$refs.notifyPopup as any;
		notifyDialog.showHideDialog(true);

	}
}
