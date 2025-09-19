<template>
  <div id="bgHome">
    <el-container>
      <el-header>
        <el-row>
          <el-col :span="12" class="left">
            <span id="FormName">{{ $t("CameraControl") }}</span>
          </el-col>
          <el-col :span="12">
            <HeaderComponent />
          </el-col>
        </el-row>
      </el-header>
       <el-main class="bgHome">
           <div>
				<data-table-function-component
					@insert="Insert" @edit="Edit" @delete="Delete"
					@add-excel="AddOrDeleteFromExcel('add')" @delete-excel="AddOrDeleteFromExcel('delete')" @export-excel="ExportToExcel"
                    @check-image="CheckImageClicked"
					v-bind:listExcelFunction="listExcelFunction"
                    :showButtonColumConfig="true" :gridColumnConfig.sync="columns">
                </data-table-function-component>
				<data-table-component :get-data="getData" ref="table" :columns="columns" :selectedRows.sync="rowsObj" :isShowPageSize="true"></data-table-component>
			</div>
            <!-- dialog insert -->
            <div>
                <el-dialog :title="isEdit ? $t('Edit') : $t('Insert')" style="margin-top:20px !important;"
                    custom-class="customdialog" :visible.sync="showDialog" :before-close="Cancel" :close-on-click-modal="false">
					<el-form class="formScroll" :model="cameraModel" :rules="rules" ref="form" label-width="168px" label-position="top" @keyup.enter.native="Submit">
						<el-form-item :label="$t('CameraName')" prop="Name">
							<el-input v-model="cameraModel.Name"></el-input>
						</el-form-item>
						<el-form-item :label="$t('IPAddress')" prop="IpAddress">
							<el-input v-model="cameraModel.IpAddress"></el-input>
						</el-form-item>
                        <el-form-item :label="$t('Port')" prop="Port">
							<el-input v-model="cameraModel.Port"></el-input>
						</el-form-item>
                        <el-form-item :label="$t('SerialNumber')" prop="Serial">
							<el-input v-model="cameraModel.Serial"></el-input>
						</el-form-item>
                        <el-form-item :label="$t('CameraType')" prop="Serial">
							<el-select v-model="cameraModel.Type" placeholder="Select" style="">
                                <el-option v-for="item in cameraTypeList" :key="item.Index"
                                    :label="$t(item.Dislay)" :value="item.Value">
                                </el-option>
                            </el-select>
						</el-form-item>
						<el-form-item :label="$t('UserLogin')" prop="UserName">
							<el-input v-model="cameraModel.UserName"></el-input>
						</el-form-item>
                        <el-form-item :label="$t('Password')" prop="Password">
							<el-input type="password" v-model="cameraModel.Password" show-password></el-input>
						</el-form-item>
                        <el-form-item :label="$t('Note')" prop="Description">
							<el-input type="textarea" :rows="4" v-model="cameraModel.Description"></el-input>
						</el-form-item>
					</el-form>
					<span slot="footer" class="dialog-footer">
						<el-button class="btnCancel" @click="Cancel">
							{{ $t('Cancel') }}
						</el-button>
						<el-button class="btnOK" type="primary" @click="ConfirmClick">
							{{ $t('OK') }}
						</el-button>
					</span>
                   
                    
				</el-dialog>
            </div>
             <!-- dialog check -->
            <div>
                <el-dialog :title="$t('CheckImageFromCamera')" style="margin-top:20px !important;"
                    :visible.sync="showCheckImageDialog" :close-on-click-modal="false">
                    <div>
                        <span>Chọn camera</span>
                        <el-select v-model="selectedCamera" placeholder="Select" style="margin-left:10px;">
                            <el-option v-for="item in arrCameraInfo" :key="item.Index"
                                :label="item.Name" :value="item.Index">
                            </el-option>
                        </el-select>
                        <span style="margin-left:10px;">Chọn kênh</span>
                        <el-input v-model="selectedChannel" maxlength="100" style="width:100px;margin-left:10px;"></el-input>
                        <el-button class="btnOK" type="primary" @click="ViewClick">
							{{ $t('View') }}
						</el-button>
                        <div style="margin-top:10px;">
                            <el-input type="textarea" :rows="2" v-model="messageCheck" style="width:100%;"></el-input>
                        </div>
                        <div style="margin-top:20px;">
                            <el-image style="width: auto; height: auto" :src="cameraCheckImageLink" fit="fill">
                                
                            </el-image>
                        </div>
                    </div>
                </el-dialog>
            </div>
       </el-main>
    </el-container>
  </div>
</template>
<script src="./camera-control.ts"></script>
<style lang="scss">
    .formScroll{
        height: 55vh;
        overflow-y: auto;
    }
    .el-dialog{
        margin-top: 20px !important;
    }
</style>