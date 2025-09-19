declare interface IMenu {
    id: string;
    name: string;
    roles: Array<IStateRole>;
    level?: number;
    view?: boolean;
    isLeaf?: boolean;
    parentId?: string;
    menuId?: string;
}


declare interface IStateRole {
    group_id: number;
    role: string;
}

declare interface IGroup {
    id: any;
    name: string;
}

declare interface IMenuDevice {
    privilegeindex: string;
    name: string;
    roles: Array<IStateRole>;
}
declare interface IMatrixList extends Array<IMenu> { }

declare interface IMatrixColumn extends Array<IGroup> { }

declare interface IMatrixListDevice extends Array<IMenuDevice> { }




