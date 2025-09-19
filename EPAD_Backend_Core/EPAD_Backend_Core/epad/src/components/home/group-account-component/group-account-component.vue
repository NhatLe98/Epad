<template>
	<div id="bgHome">
		<el-container>
			<el-header>
				<el-row>
					<el-col :span="12" class="left">
						<span id="FormName">{{ $t('GroupAccount') }}</span>
					</el-col>
					<el-col :span="12">
						<HeaderComponent />
					</el-col>
				</el-row>
			</el-header>
			<el-main class="bgHome">
				<div>
					<el-dialog :title="isEdit ? $t('EditGroupAccount') : $t('InsertGroupAccount')" 
					custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" 
					@close="closeForm" :close-on-click-modal="false"
					width="40%" top="5vh">
						<el-form :model="groupAccountForm" ref="groupAccountForm" :rules="rules" label-width="168px" label-position="top" @keyup.enter.native="Submit">
							<el-form-item @click.native="focus('GroupName')" :label="$t('GroupName')" prop="Name">
								<el-input ref="GroupName" v-model="groupAccountForm.Name"></el-input>
							</el-form-item>

							<el-form-item :label="$t('GroupDefault')">
								<el-checkbox ref="UseForDefault" v-model="groupAccountForm.UseForDefault"></el-checkbox>
							</el-form-item>
							<el-form-item :label="$t('GroupAdmin')">
								<el-checkbox ref="IsAdmin" v-model="groupAccountForm.IsAdmin"></el-checkbox>
							</el-form-item>
							<el-form-item @click.native="focus('Note')" :label="$t('Note')">
								<el-input ref="Note" type="textarea" :autosize="{ minRows: 3, maxRows: 6 }" :placeholder="$t('Note')" v-model="groupAccountForm.Note"></el-input>
							</el-form-item>
						</el-form>
						<span slot="footer" class="dialog-footer">
							<el-button class="btnCancel" @click="Cancel">{{ $t('Cancel') }}</el-button>
							<el-button class="btnOK" type="primary" @click="Submit">{{ $t('OK') }}</el-button>
						</span>
					</el-dialog>
				</div>
				<div>
					<data-table-function-component @insert="Insert" @edit="Edit" @delete="Delete" 
					v-bind:listExcelFunction="listExcelFunction" :showButtonColumConfig="true" :gridColumnConfig.sync="columns"> </data-table-function-component>
					<data-table-component ref="table" :get-data="getData" :columns="columns" :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
				</div>
				<!-- <div>
          <el-row>
            <el-col :span="12" class="left">
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                @click="Insert"
                round
              >{{ $t("Insert") }}</el-button>
              <el-button
                class="classLeft"
                id="btnFunction"
                type="primary"
                round
                @click="Edit"
              >{{ $t("Edit") }}</el-button>
            </el-col>
            <el-col :span="12">
              <el-button
                class="classRight"
                id="btnFunction"
                type="primary"
                round
                @click="Delete"
              >{{ $t("Delete") }}</el-button>
            </el-col>
          </el-row>
        </div> -->
			</el-main>
		</el-container>
	</div>
</template>
<script src="./group-account-component.ts"></script>
