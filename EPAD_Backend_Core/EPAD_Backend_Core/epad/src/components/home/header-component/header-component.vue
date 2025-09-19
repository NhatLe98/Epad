<template>
	<div>
		<el-row style="position: unset !important;">
			<el-col :span="12" style="position: absolute;" id="masterEmployeeFilter" 
				v-if="enableMasterEmployeeFilter && showMasterEmployeeFilter">
				<el-popover ref="elPopover" transition="el-zoom-in-top" 
					popper-class="el-select-department-tree__popover" trigger="click"
					@after-enter="handleScroll()"
					:style="loadedMasterFilter ? 'display: unset;' : 'display: none;'"
					>

					<el-input class="master-employee-filter__input-tree"
						style="
							padding: 5px;
							position: absolute;
							width: 95%;
							height: 30px;
							z-index: 1000;
						"
						:placeholder="$t('SearchData')"
						v-model="filterTree"
						@keyup.enter.native="filterTreeData()"
						>
						<i slot="suffix" class="el-icon-search" @click="filterTreeData()"></i>
					</el-input>
					<el-tree class="master-employee-filter table-text-color" style="margin-top: 50px;" :data="treeData"
						:props="{ label: 'Name', children: 'ListChildrent' }" :filter-node-method="filterNode"
						:default-expanded-keys="expandedKey" :default-checked-keys="defaultChecked" node-key="ID" 
						@check="nodeCheck" ref="masterEmployeeTree" show-checkbox
						highlight-current v-loading="loadingTree">
						<template slot-scope="scoped">
							<div>
								<i :class="getIconClass(scoped.data.Type, scoped.data.Gender)" />
								<span class="ml-5">{{ scoped.data.Name }}</span>
							</div>
						</template>
					</el-tree>

					<el-input
						v-model="key"
						:placeholder="$t('SelectEmployee')"
						ref="reference"
						slot="reference"
						:validate-event="false"
						style="width: 300px; z-index: 999; height: 31px; margin-top: 10px;"
						readonly
						class="master-employee-filter__view-input"
					>
						<!-- <span v-if="key && key.length > 0" slot="prefix">
							{{ $t('SelectEmployee') }}
						</span> -->
						<i
						v-if="key && key.length > 0"
						@click.stop="clear()"
						slot="suffix"
						style="font-size: 14px !important; color: red;"
						:title="'Chọn nhân viên'"
						class="el-input__icon el-input__icon-close el-icon-circle-close"
						></i>
						<i v-else
						slot="suffix"
						class="el-input__icon el-input__icon-arrow-down el-icon-arrow-down"
						></i>
					</el-input>
				</el-popover>
				<span :style="loadedMasterFilter ? 'display: unset;' : 'display: none;'"
					style="display: inline-block; margin-left: 5px;font-size: 1vw;">
					{{ $t('SelectEmployee') }}
				</span>
			</el-col>
			<el-col :span="12" style="position: absolute; right: 0 !important;">
				<div class="header-wrapper">
					<el-select v-model="value" @change="change" placeholder="Tiếng Việt" style="margin-right: 24px;">
						<el-option v-for="item in options" :key="item.value" :label="item.label"
							:value="item.value"></el-option>
					</el-select>
					<span @click="showNotifyDialog" style="margin-right:20px; margin-left:-10px">
						<el-badge :value="numberOfNotify" style="margin-top:5px; cursor:pointer">
							<i class="el-icon-message-solid" :title="$t('DialogHeaderNotifyDialog')"
								style="font-size: 28px;color: #606266; margin-right:-10px; cursor:pointer"></i>
						</el-badge>
					</span>
					<el-dropdown trigger="click" @command="handleCommand">
						<span class="el-dropdown-link">
							<img class="user-avatar" src="../../../assets/images/favicon.png" alt="ảnh đại diện" />
						</span>
						<el-dropdown-menu slot="dropdown" class="dropdown-user">
							<el-dropdown-item class="fullname">
								{{ Username }}
							</el-dropdown-item>

							<el-dropdown-item command="ChangePassword">
								<svg width="21px" height="23px">
									<path stroke="none" fill="gray"
										d="M12 1l9 4v6c0 5.5-3.8 10.7-9 12-5.2-1.3-9-6.5-9-12V5l9-4zM7.7 15.1A5.3 5.3 0 0 1 12 6.7a5.3 5.3 0 0 1 4.3 8.4c-.6-1.1-3-1.7-4.3-1.7-1.3 0-3.7.6-4.3 1.7zM12 8.3a2.2 2.2 0 0 0-2.2 2.2c0 1.2 1 2.2 2.2 2.2a2.2 2.2 0 0 0 2.2-2.2c0-1.2-1-2.2-2.2-2.2zm0-2.6A6.3 6.3 0 0 0 5.7 12a6.3 6.3 0 0 0 6.3 6.3 6.3 6.3 0 0 0 6.3-6.3A6.3 6.3 0 0 0 12 5.7z"
										class="style-scope yt-icon"></path>
								</svg>
								{{ $t('ChangePassword') }}
							</el-dropdown-item>
							<el-dropdown-item command="Version">
								<i class="el-icon-tickets yt-icon"
									style="width:21px; height:21px; font-size: 1.5rem; margin-right: 16px;"></i>{{
								$t('Version') }}
							</el-dropdown-item>
							<el-dropdown-item command="Logout">
								<svg width="21px" height="21px">
									<!-- <svg viewBox="0 0 24 24" style="display: block;"> -->
									<path stroke="none" fill="gray"
										d="M10.09 15.59L11.5 17l5-5-5-5-1.41 1.41L12.67 11H3v2h9.67l-2.58 2.59zM19 3H5c-1.11 0-2 .9-2 2v4h2V5h14v14H5v-4H3v4c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2z"
										class="style-scope yt-icon"></path>
								</svg>{{ $t('Logout') }}
							</el-dropdown-item>

						</el-dropdown-menu>
					</el-dropdown>
				</div>
			</el-col>
		</el-row>

		<div class="changepw-dialog-wrapper">
			<el-dialog :title="$t('ChangePassword')" :visible.sync="showDialog" :before-close="Cancel"
				custom-class="customdialog">
				<el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top"
					@keyup.enter.native="submit">
					<el-form-item @click.native="focus('PasswordOld')" :label="$t('PasswordOld')" prop="Password">
						<el-input ref="PasswordOld" id="Input" v-model="ruleForm.Password" type="password"></el-input>
					</el-form-item>
					<el-form-item @click.native="focus('PasswordNew')" :label="$t('PasswordNew')" prop="NewPassword">
						<el-input ref="PasswordNew" id="Input" v-model="ruleForm.NewPassword"
							type="password"></el-input>
					</el-form-item>
					<el-form-item @keyup.enter.native="submit" @click.native="focus('ConfirmPassword')"
						:label="$t('ConfirmPassword')" prop="ConfirmPassword">
						<el-input ref="ConfirmPassword" id="Input" v-model="ruleForm.ConfirmPassword"
							type="password"></el-input>
					</el-form-item>
				</el-form>

				<span slot="footer" class="dialog-footer">
					<el-button class="btnCancel" @click="Cancel">
						{{ $t('Cancel') }}
					</el-button>
					<el-button class="btnOK" type="primary" @click="submit">
						{{ $t('OK') }}
					</el-button>
				</span>
			</el-dialog>
		</div>
		<div>
			<notify-popup-component :title="$t('DialogHeaderNotifyDialog')"
				@getNumberOfNotifyComponent="setNumberOfNotify" ref="notifyPopup"></notify-popup-component>
		</div>
	</div>
</template>
<script src="./header-component.ts"></script>
<style lang="scss">
.master-employee-filter__view-input{
	input{
		text-overflow: ellipsis;
	}
}

.master-employee-filter{
	max-height: 80vh !important;	
	.el-tree-node.is-expanded{
		max-height: inherit !important;
		overflow-y: auto;
	}
}

.user-avatar {
	border-radius: 50%;
}

.changepw-dialog-wrapper {

	.btnCancel,
	.btnOK {
		span {
			font-size: 15px;
			font-weight: 500;
		}
	}
}

.header-wrapper {
	width: 100%;
	padding-right: 24px;
	height: 51px;
	display: flex;
	justify-content: flex-end;
	align-items: center;
}

.el-select-dropdown.el-popper {
	/* width: 200px !important; */
	max-width: 200px !important;
	min-width: none !important;
}

/* .user-function {
  margin-left: 5px;
} */
.user-function a#Admin {
	width: 36px;
	height: 36px;
	display: block;
}

header {
	height: 51px !important;
	box-shadow: 0px 4px 4px rgba(123, 123, 123, 0.25);
	padding-right: 0 !important;
}

.user-name {
	width: 120px;
	padding: 0 15px;
	position: relative;
	border-bottom: 0.5px solid #767676;
}

.dropdown-user {
	margin-top: 0 !important;
	padding-top: 0;
	left: unset !important;
	right: 5px;
	min-width: 190px;
	width: fit-content;

	.el-dropdown-menu__item {
		font-size: 15px;
		display: flex;
		justify-content: flex-start;
		align-items: center;
		padding: 0 20px;
		height: 45px;
		line-height: 22px;

		&:not(.is-disabled, .fullname):hover {
			background-color: #f2f2f2;
			font-weight: 600;
			color: #004282;
		}

		&.fullname {
			background-color: #004282;
			color: #fff;
			font-weight: 600;
			font-size: 16px;
			padding: 0 15px;
			width: max-content;
			min-width: 190px;

			// text-align: center;
			&:hover {
				background-color: #004282;
				color: #fff;
			}
		}

		svg {
			margin-right: 16px;
		}
	}

	.popper__arrow {
		display: none;
	}
}

.master-employee-filter__input-tree {
	.el-input__suffix{
		top: unset !important;
		margin-right: 5px;
		height: 32px !important;
		.el-input__suffix-inner{
			line-height: 32px;
		}
	}
}
</style>
