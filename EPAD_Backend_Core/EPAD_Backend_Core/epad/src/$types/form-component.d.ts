declare interface IFormEntry {
    id?: number;
    formName: string;
    componentName: string;
    active: boolean;
    isShow: boolean;
    default?: boolean;
  }
  
  declare interface IFormCollection extends Array<IFormEntry> {}
  