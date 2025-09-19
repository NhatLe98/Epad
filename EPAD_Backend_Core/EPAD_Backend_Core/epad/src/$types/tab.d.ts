declare interface ITab {
    id?: number;
    tabName: string;
    title: string,
    active?: boolean;
    componentName: string;
    userType?: number;
    showAdd?: boolean,
    showEdit?: boolean,
    showDelete?: boolean,
    showMore?: boolean,
    bind?: unknown;
    modified?: boolean;
    filterModel?: any;
    selectedRowKeys?: any[];
    moreFunctions?: Array<>;
    showIntegrate?: boolean;    
    iconClass?: string;
    iconImage?: string;
    listEmployeeATID?: any[];
    departmentData?: any[];
    showReadGoogleSheet?: boolean;    

}

declare interface ITabCollection extends Array<ITab> { }