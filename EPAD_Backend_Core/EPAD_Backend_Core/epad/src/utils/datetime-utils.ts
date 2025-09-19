export const fHMS = (dateStart: Date, dateEnd: Date) => {
    const start = moment(dateStart);
    const end = moment(dateEnd);
    if(start > end){
        return "00:00:00";
    }else{
        return moment.utc(moment.duration(end.diff(start)).asMilliseconds()).format("HH:mm:ss");
    }
}

export const fDateTime = (dateStr: string) => {
    return dateStr ? moment(dateStr).format('YYYY-MM-DD HH:mm:ss') : "";
};

/**
 * Get end time of DateTime
 * @returns Date with time 23:59:59
 */
 export const getEndTimeOfDate = (dateTime: Date) => {
    return new Date(dateTime.setHours(23, 59, 59, 999));
}
/**
 * Get start time of DateTime
 * @returns Date with time 00:00:00
 */
export const getStartTimeOfDate = (dateTime: Date) => {
    return new Date(dateTime.setHours(0, 0, 0, 0));
}

export const addHours = (date: Date, hours: number) => {
    return new Date(date.setHours(date.getHours() + hours));
}