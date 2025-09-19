import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class RealTimeApi extends BaseApi {

  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }
  public ConnectToServer() {
    return this.get('ConnectToServer');
  }
  public GetSystemDateTime() {
    return this.get('GetSystemDateTime');
  }
  public GetGCSRealTimeServer() {
    return this.get('GetGCSRealTimeServer');
  }
  public GetCustomerMonitoringHistoryList(size: number) {
    return this.get('GetCustomerMonitoringHistoryList', { params: { size } });
  }

  public GetWalkerMonitoringHistoryListByLineIndex(lineIndex: number, size: number) {
    return this.get('GetWalkerMonitoringHistoryListByLineIndex', { params: { lineIndex, size } });
  }
  public GetWalkerMonitoringHistoryByLogIndex(logIndex: number) {
    return this.get('GetWalkerMonitoringHistoryByLogIndex', { params: { logIndex } });
  }
  public GetCameraLiveViewLinkByLineIndex(lineIndex: number) {
    return this.get('GetCameraLiveViewLinkByLineIndex', { params: { lineIndex } });
  }
  public SaveImageFromBase64(data: string, imageType: string, imageName: string) {
    return this.post('SaveImageFromBase64', { base64: data, type: imageType, name: imageName });
  }
  public UpdateLogStatus(data: LogParam) {
    return this.post('UpdateLogStatus', data);
  }
  public UpdateLogStatusAuto(data: LogParam) {
    return this.post('UpdateLogStatusAuto', data);
  }
  public UpdateParkingLogStatus(data: ParkingLogParam) {
    return this.post('UpdateParkingLogStatus', data);
  }
  public UpdateParkingLogStatusAuto(data: ParkingLogParam) {
    return this.post('UpdateParkingLogStatusAuto', data);
  }
  public CallControllerByParam(id: number, timeLogIndex: number){
    const data: CallControllerParam = {
      Id: id,
      TimeLogIndex: timeLogIndex
    }

    return this.post('CallControllerByParam', data);
  }
  public GetGCSCurrentRealTimeServer() {
    return this.get('GetGCSCurrentRealTimeServer');
  }
  public CheckParkingLogDayLimit(logIndex: number, timeOut: Date, error: string) {
    return this.get('CheckParkingLogDayLimit', { params: { logIndex, timeOut, error } });
  }
  public GetImageAsBase64Url(streamLink: string) {
    return this.get('GetImageAsBase64Url', { params: { streamLink } });
  }

}
export interface CallControllerParam {
  Id: number;
  TimeLogIndex: number;
}
export interface LogParam {
  Index: number;
  InOut: number;
  OpenController: boolean;
  LineIndex: number;
  Note: string;
  UserName: string;
  IsException?: boolean;
  ExceptionReason?: string;
  EmployeeATID?: string;
  CardNumber?: string;
}
export interface CustomerMonitoringData {
  Index: number;
  EmployeeATID: string;
  CheckTime: Date | string;
  Success: boolean;
  InOut: number;
  Error: string;
  CompanyIndex: number;
  CustomerName: string;
  CompanyName: string;
  CustomerPhone: string;
  CardNumber: string;
  WorkContent: string;
  FromTime: Date | string | null;
  ToTime: Date | string | null;
  RegisterTime: Date | string | null;
  WorkingTime: string;
  ContactPerson: string;
  RegisterImage: string;
  VerifyImage: string;
}
export interface WalkerInfo extends MonitoringInfo {
  ObjectType: string;
  ListInfo: InfoDetail[];
  RegisterImage: string;
  VerifyImage: string;
  CardNumber?: string;
}
export interface InfoDetail {
  Title: string;
  Data: string;
}
export interface WalkerHistoryInfo {
  EmployeeATID: string;
  CheckTime: Date | string;
  Success: boolean;
  InOut: number;
  Error: string;
  LineIndex: number;
  CardNumber: string;
  ObjectType: string;
  FullName: string;
  Department: string;
  Position: string;
  CompanyName: string;
  WorkContent: string;
  ContactPerson: string;
  StudentCode: string;
  Class: string;
  ClassTeacher: string;
  RegisterImage: string;
  VerifyImage: string;
  ApproveStatus: string;
  LogIndex: number;
}

export interface MonitoringInfo {
  Index: number;
  EmployeeATID: string;
  CheckTime: Date | string;
  Success: boolean;
  InOut: number;
  Error: string;
  CompanyIndex: number;
  LineIndex: number;
}
export interface ParkingMonitoringInfo {
  Index: number;
  EmployeeATID: string;
  CheckTime: Date | string;
  Success: boolean;
  InOut: number;
  Error: string;
  CompanyIndex: number;
  LineIndex: number;

  EmployeeName: string;
  EmployeeCode: string;
  DepartmentName: string;
  DepartmentIndex: number;
  BikeModels: Array<string>;
  BikePlateIn: string;
  BikePlateOut: string;
  BikePlatesRegister: Array<string>;
  CardNumber: string;
  AccessObject: string;
  NumberOfBikeInParking: number;

  TimeIn: Date | string;
  TimeOut: Date | string | null;

  ImagePlateIn: string;
  ImageFaceIn: string;
  ImagePlateOut: string;
  ImageFaceOut: string;

  CustomerIndex: number;
}

export interface ParkingMonitoringResult {
  LineIndex: number;
  ListCameraInLine: CameraInfo[];
  ListCameraOutLine: CameraInfo[];
}
export interface ParkingLogParam {
  Index: number;
  OpenController: boolean;
  InOut: number;
  Note: string;
  LineIndex: number;
  PlateNumber: string;
  UserName: string;
  CurrentLogIndex: number;
  IsAutoRecognition: boolean;
}

export interface CameraInfo {
  CameraIndex: number;
  StreamLink: string;
  CameraType: string;
}
export interface CameraLiveViewParking {
  CameraPlate_LineIn_Index: number;
  CameraFace_LineIn_Index: number;
  CameraPlate_LineOut_Index: number;
  CameraFace_LineOut_Index: number;
  CameraPlate_LineIn_Link: string;
  CameraFace_LineIn_Link: string;
  CameraPlate_LineOut_Link: string;
  CameraFace_LineOut_Link: string;
}

export const realtimeApi = new RealTimeApi('RealTime');
