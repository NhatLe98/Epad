declare interface IState {
    name: string,
    title: string,
    icon: string,
    type: string
}

declare interface IStateCollection extends Array<IState> {}