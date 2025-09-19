declare interface IGroupMenu {
    group_id: string;
    group_key: string;
    group_name: string;
    group_path: string;
    group_icon: string;
    group_show: boolean;
    list_menu: Array<IMainMenu>;
}

declare interface IMainMenu {
    id: string;
    key: string;
    name: string;
    path: string;
    icon: string;
}