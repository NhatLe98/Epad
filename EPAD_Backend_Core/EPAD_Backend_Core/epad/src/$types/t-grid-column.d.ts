declare interface IColumnConfig {
    name: string,
    dataField: string,
    dataType?: 'lookup' | 'date' | 'translate' | 'image' | 'imageTooltip' | 'viewDetailPopup',
    fixed: boolean,
    width: number,
    show: boolean,
    lookup?: ILookup,
    format?: string
}

declare interface ILookup {
    dataSource: any,
    displayMember: string,
    valueMember: string
}