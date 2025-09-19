<template>
    <div id="bgHome">
        <el-container>
            <el-header>
                <el-row>
                    <el-col :span="12" class="left">
                        <span id="FormName">{{ $t("Configuration") }}</span>
                    </el-col>
                    <el-col :span="12">
                        <HeaderComponent />
                    </el-col>
                </el-row>
            </el-header>
            <el-main id="bgHome">

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoDownloadLogFromMachine')" name="1">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_LOG.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="12">
                                                <el-form-item :label="$t('DownloadFromPreviousDay')" label-width="250px">
                                                    <el-input v-model="configCollection.DOWNLOAD_LOG.PreviousDays"></el-input>
                                                </el-form-item>
                                            </el-col>
                                            <el-col :span="12">
                                                <span>{{ $t('DayToCurrentDay') }}</span>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_LOG.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_LOG.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.DOWNLOAD_LOG.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.DOWNLOAD_LOG.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_LOG.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.DOWNLOAD_LOG.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.DOWNLOAD_LOG.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="24">
                                                <el-form-item :label="$t('DeleteLogAfterDownloadSuccess')"
                                                              label-width="250px">
                                                    <el-checkbox style="margin-left: 0px;"
                                                                 v-model="configCollection.DOWNLOAD_LOG.DeleteLogAfterSuccess" />
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoDeleteLogFromMachine')"
                                                      name="2"
                                                      style="font-weight:bolder;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.DELETE_LOG.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="12">
                                                <el-form-item label="Xóa log cũ từ trước đó" label-width="250px">
                                                    <el-input v-model="configCollection.DELETE_LOG.PreviousDays"></el-input>
                                                </el-form-item>
                                            </el-col>
                                            <el-col :span="12">
                                                <span>{{ $t('DayToCurrentDay') }}</span>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.DELETE_LOG.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DELETE_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DELETE_LOG.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.DELETE_LOG.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.DELETE_LOG.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DELETE_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DELETE_LOG.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.DELETE_LOG.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.DELETE_LOG.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoStartMachine')"
                                                      name="3"
                                                      style="font-weight:600px;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.START_MACHINE.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.START_MACHINE.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.START_MACHINE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.START_MACHINE.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.START_MACHINE.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.START_MACHINE.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.START_MACHINE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.START_MACHINE.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.START_MACHINE.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.START_MACHINE.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('ReProcessingRegisterCard')"
                                                      name="3"
                                                      style="font-weight:600px;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.RE_PROCESSING_REGISTERCARD.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.RE_PROCESSING_REGISTERCARD.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.RE_PROCESSING_REGISTERCARD.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.RE_PROCESSING_REGISTERCARD.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.RE_PROCESSING_REGISTERCARD.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.RE_PROCESSING_REGISTERCARD.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.RE_PROCESSING_REGISTERCARD.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.RE_PROCESSING_REGISTERCARD.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.RE_PROCESSING_REGISTERCARD.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.RE_PROCESSING_REGISTERCARD.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>
                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('DownloadParkingLog')"
                                                      name="3"
                                                      style="font-weight:600px;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_PARKING_LOG.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_PARKING_LOG.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_PARKING_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_PARKING_LOG.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.DOWNLOAD_PARKING_LOG.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.DOWNLOAD_PARKING_LOG.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_PARKING_LOG.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_PARKING_LOG.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.DOWNLOAD_PARKING_LOG.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.DOWNLOAD_PARKING_LOG.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>
                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('IntegrateInfoToOffline')"
                                                      name="3"
                                                      style="font-weight:600px;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.INTEGRATE_INFO_TO_OFFLINE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.INTEGRATE_INFO_TO_OFFLINE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.INTEGRATE_INFO_TO_OFFLINE.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoDeleteBlacklist')"
                                                      name="3"
                                                      style="font-weight:600px;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.AUTO_DELETE_BLACKLIST.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.AUTO_DELETE_BLACKLIST.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.AUTO_DELETE_BLACKLIST.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.AUTO_DELETE_BLACKLIST.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.AUTO_DELETE_BLACKLIST.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.AUTO_DELETE_BLACKLIST.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.AUTO_DELETE_BLACKLIST.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.AUTO_DELETE_BLACKLIST.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.AUTO_DELETE_BLACKLIST.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.AUTO_DELETE_BLACKLIST.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>
                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoDownloadUserFromMachine')"
                                                      name="4"
                                                      style="font-weight:bolder;">
                                        <el-row :gutter="20" style="padding-top:10px;">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_USER.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.DOWNLOAD_USER.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_USER.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_USER.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.DOWNLOAD_USER.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.DOWNLOAD_USER.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.DOWNLOAD_USER.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.DOWNLOAD_USER.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.DOWNLOAD_USER.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.DOWNLOAD_USER.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('AutoAddOrDeleteUserFromMachine')"
                                                      name="5"
                                                      style="font-weight:bolder;">
                                        <el-row :gutter="20">
                                            <el-col :span="20" style="padding-top:10px;">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.ADD_OR_DELETE_USER.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.ADD_OR_DELETE_USER.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.ADD_OR_DELETE_USER.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.ADD_OR_DELETE_USER.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.ADD_OR_DELETE_USER.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.ADD_OR_DELETE_USER.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.ADD_OR_DELETE_USER.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.ADD_OR_DELETE_USER.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.ADD_OR_DELETE_USER.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.ADD_OR_DELETE_USER.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row :gutter="20">
                    <el-form>
                        <el-col :span="24">
                            <el-form-item>
                                <el-collapse>
                                    <el-collapse-item :title="$t('EmployeeIntegrate')"
                                                      name="6"
                                                      style="font-weight:bolder;">
                                        <el-row :gutter="20">
                                            <el-col :span="20" style="padding-top:10px;">
                                                <el-form-item :label="$t('Time')" label-width="250px">
                                                    <el-select v-model="configCollection.EMPLOYEE_INTEGRATE.TimePos"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in timePosOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20">
                                            <el-col :span="20">
                                                <el-form-item :label="$t('SendResultToEMail')" label-width="250px">
                                                    <el-select v-model="configCollection.EMPLOYEE_INTEGRATE.Email"
                                                               multiple
                                                               filterable
                                                               clearable
                                                               reserve-keyword
                                                               allow-create
                                                               class="w-100">
                                                        <el-option v-for="item in emailOption"
                                                                   :key="item"
                                                                   :label="item"
                                                                   :value="item"></el-option>
                                                    </el-select>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.EMPLOYEE_INTEGRATE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.EMPLOYEE_INTEGRATE.SendMailWhenError">{{ $t('SendMailWhenError')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailSuccess')" label-width="250px">
                                                    <el-input placeholder="Success title" v-model="configCollection.EMPLOYEE_INTEGRATE.TitleEmailSuccess"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailSuccess')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Success body" v-model="configCollection.EMPLOYEE_INTEGRATE.BodyEmailSuccess"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                        <el-row :gutter="20" v-if="configCollection.EMPLOYEE_INTEGRATE.Email.length != 0">
                                            <el-col :span="20">
                                                <el-form-item>
                                                    <el-checkbox v-model="configCollection.EMPLOYEE_INTEGRATE.AlwaysSend">{{ $t('AlwaysSendMailWhenDownloadLogDone')}}</el-checkbox>
                                                </el-form-item>
                                                <el-form-item :label="$t('TitleEmailError')" label-width="250px">
                                                    <el-input placeholder="Error title" v-model="configCollection.EMPLOYEE_INTEGRATE.TitleEmailError"></el-input>
                                                </el-form-item>
                                                <el-form-item :label="$t('BodyEmailError')" label-width="250px">
                                                    <el-input type="textarea" placeholder="Error body" v-model="configCollection.EMPLOYEE_INTEGRATE.BodyEmailError"></el-input>
                                                </el-form-item>
                                            </el-col>
                                        </el-row>
                                    </el-collapse-item>
                                </el-collapse>
                            </el-form-item>
                        </el-col>
                    </el-form>
                </el-row>

                <el-row>
                    <el-col :span="24" class="left">
                        <el-button class="classLeft"
                                   id="btnSave"
                                   type="primary"
                                   @click="SaveConfig"
                                   round>{{ $t("Save") }}</el-button>
                    </el-col>
                </el-row>
            </el-main>
        </el-container>
    </div>
</template>
<script src="./system-config-component.ts"></script>

<style scoped>
    .el-checkbox {
        margin-left: 250px;
    }
</style>
