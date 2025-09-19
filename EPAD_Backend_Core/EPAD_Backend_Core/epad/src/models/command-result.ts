export type CommandResult = {
    AppliedTime?: any;
    Command: string;
    ErrorCounter: number;
    ExcutingServiceIndex: number;
    ExternalData: string;
    FromTime: string;
    GroupIndex: string;
    /**
     * equivalent IC_SystemCommand.Index in database
     */
    ID: string;
    IPAddress: string;
    IsOverwriteData: boolean;
    ListUsers: any[]
    Port: number;
    SerialNumber: string;
    ToTime: string;
}