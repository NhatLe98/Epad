declare interface CommandRequest {
    Action: string;
    ListSerial: Array<string>;
    ListUser: Array<string>;
    FromTime?: any;
    ToTime?: any;
}