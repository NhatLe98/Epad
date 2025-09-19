export const compareString = (str1: string, str2: string) => {
    if (str1 > str2) {
        return 1;
    }
    if (str1 < str2) {
        return -1;
    }
    return 0;
}