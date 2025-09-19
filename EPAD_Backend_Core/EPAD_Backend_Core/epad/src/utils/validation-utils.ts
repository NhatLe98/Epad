const isIPAddress = (str: string) => {
    const pattern = /^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)(\.(?!$)|$)){4}$/;
    return pattern.test(str);
}

const isPort = (str: string) => {
    return /^[0-9]+/.test(str) && +str >= 0 && +str <= 65535;
}

export {
    isIPAddress,
    isPort,
}