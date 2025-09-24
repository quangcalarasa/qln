export function convertUrl(str?: string) {
  if (!str) return undefined;
  str = str.toLowerCase();
  str = str.replace(/á|à|ả|ã|ạ|â|ấ|ầ|ẩ|ẫ|ậ|ă|ắ|ằ|ẳ|ẵ|ặ"/g, 'a');
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
  str = str.replace(/đ/g, 'd');
  str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g, ' ');
  str = str.replace(/[^a-zA-Z0-9 ]/g, '');
  str = str.replace(/ + /g, ' ');
  str = str.trim();
  str = str.replace(/ /g, '-');

  return str;
}
export function roundIfDecimal(number: number, decimalPlaces: number) {
  if (Number.isInteger(number)) {
    return number;
  } else {
    return formatNumber(parseFloat(number.toFixed(decimalPlaces)).toString());
  }
}
function formatNumber(input: string) {
  const parsedNumber = parseFloat(input.replace(',', '.')); // Chuyển chuỗi thành số
  if (isNaN(parsedNumber)) {
    return NaN; // Trả về NaN nếu không phải số hợp lệ
  }
  return parsedNumber;
}
export function currencyFormatter(currency: string, sign?: string) {
  if (currency) {
    var sansDec = parseInt(currency).toFixed(0);
    var formatted = sansDec.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return sign ? sign + `${formatted}` : `${formatted}`;
  }
  else return "";

  // return currency?.toLocaleString('vi-VN');

}

export function converStringToDate(str: string) {
  const [day, month, year] = str.split('/');
  return new Date(+year, +month - 1, +day);
}

