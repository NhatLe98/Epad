<template>
	<div id="bgHome">
		<el-container>
			<el-header>
				<el-row>
					<el-col :span="12" class="left">
						<span id="FormName">{{ $t('PersonalAccessToken') }}</span>
					</el-col>
					<el-col :span="12">
						<HeaderComponent />
					</el-col>
				</el-row>
			</el-header>
			<el-main class="bgHome">
				<div>
					<el-dialog :title="$t('AddLicense')" custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel">
						<el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="168px" label-position="top" @keyup.enter.native="submit">
							<el-form-item :label="$t('Name')" prop="Name" @click.native="focus('Name')">
								<el-input ref="Name" v-model="ruleForm.Name"></el-input>
							</el-form-item>
							<!-- <el-form-item :label="$t('Scopes')" @click.native="focus('Scopes')" prop="Scopes">
								<el-input ref="Scopes" v-model="ruleForm.Scopes"></el-input>
							</el-form-item> -->
							<el-form-item :label="$t('Note')" prop="Note" @click.native="focus('Note')">
								<el-input ref="Note" v-model="ruleForm.Note"></el-input>
							</el-form-item>
							<el-form-item :label="$t('ExpiredDate')" prop="ExpiredDate">
								<el-date-picker
									class="timepicker-noborder"
									style="width: 100%;"
									format="dd/MM/yyyy"
									v-model="ruleForm.ExpiredDate"
									type="date"
									placeholder
									prefix-icon="el-icon-caret-bottom"
									clear-icon="none"
								>
								</el-date-picker>
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
					<data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" :isHiddenEdit="true" v-bind:listExcelFunction="listExcelFunction"></data-table-function-component>
					<data-table-component :isHiddenPaging="false" :get-data="getData" ref="table" :columns="columns" :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
				</div>
				<div class="dialog-token">
					<el-dialog :title="$t('Token')" custom-class="customdialog" :visible.sync="showDialogToken" :before-close="CancelViewToken" :close-on-click-modal="false">
						<el-form :model="tokenForm" class="form-token-copy" ref="tokenForm" label-width="168px" label-position="top">
							<el-form-item :label="$t('PersonalAccessToken')" prop="Name" @click.native="focus('Name')">
								<el-input readonly ref="token" v-model="tokenForm.token"></el-input>
								<span @click="copyClipboard" class="icon-copy el-icon-copy-document"></span>
							</el-form-item>
						</el-form>

						<div class="warning-token">
							<div style="margin-bottom: 5px;">
								<span class="el-icon-warning-outline"></span><span>{{ $t('KeepTokenSecret') }}</span>
							</div>

							<div>
								<span class="el-icon-warning-outline"></span><span>{{ $t('TokenShowOneTime') }}</span>
							</div>
						</div>

						<!-- <span slot="footer" class="dialog-footer">
							<el-button class="btnCancel" @click="Cancel">
								{{ $t('Cancel') }}
							</el-button>
							<el-button class="btnOK" type="primary" @click="submit">
								{{ $t('OK') }}
							</el-button>
						</span> -->
					</el-dialog>
				</div>
			</el-main>
		</el-container>
	</div>
</template>

<script src="./personal-access-token-component.ts" />

<style lang="scss">
	.warning-token {
		border-radius: 8px;
		width: 100%;
		height: fit-content;
		padding: 5px 10px;
		word-break: break-word;
		text-align: justify;
		background-color: #f56c6c;

		color: #fff;
		span.el-icon-warning-outline {
			margin-right: 5px;
		}
	}
	.form-token-copy {
		position: relative;
		.icon-copy {
			cursor: pointer;
			position: absolute;
			right: 15px;
			bottom: 17px;
		}
	}
</style>
