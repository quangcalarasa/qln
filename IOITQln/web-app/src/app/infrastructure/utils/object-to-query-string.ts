export function objectToQueryString(obj: any) {
    let str = '?' + Object.keys(obj).reduce(function (a: any, k) {
        a.push(k + '=' + encodeURIComponent(obj[k]));
        return a;
    }, []).join('&');
    return str;
}