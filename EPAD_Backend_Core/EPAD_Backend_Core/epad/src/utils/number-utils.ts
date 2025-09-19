export function getDigit(str: string) {
    return +str.replace(/\D/g,'');
}
