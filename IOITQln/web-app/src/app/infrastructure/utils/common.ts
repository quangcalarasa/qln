export function convertDate(date: string) {
  if (date) {
    let _date = new Date(date);
    let month = _date.getMonth() + 1;
    let day = _date.getDate();

    return `${_date.getFullYear()}-${month < 10 ? '0' + month : month}-${day >= 10 ? day : '0' + day}`;
  }

  return undefined;
}

export function convertMoney(i: any) {
  if (i) {
    let number = i;
    let formattedNumber = number.toLocaleString('en-US');
    return formattedNumber;
  }
  return undefined;
}
export function CompareString(str1:string,str2:string)
{
  console.log(str1);
  
  const trimmedStr1 = ConvertUrl(str1);
  const trimmedStr2 = ConvertUrl(str2);
  return trimmedStr1===trimmedStr2;
}
export function ConvertUrl(str:string) {
  str = str.toLowerCase();
  str = str.replace(/á|à|ả|ã|ạ|â|ấ|ầ|ẩ|ẫ|ậ|ă|ắ|ằ|ẳ|ẵ|ặ"/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g, " ");
  str = str.replace(/[^a-zA-Z0-9 ]/g, "");
  str = str.replace(/ + /g, " ");
  str = str.trim();
  str = str.replace(/ /g, "-");

  return str;
}
export function convertExcelNumberToDate(excelNumber: any): any {
  let dateTime;
  if (typeof excelNumber === 'number') {
    const timestamp = (excelNumber - 2) * (1000 * 60 * 60 * 24) + new Date("1900-01-01").getTime();
    const dateObj = new Date(timestamp);
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    dateTime = convertDate(`${month}/${day}/${year}`);
  }
  else {
    // const [day, month, year] = excelNumber.split('/').reverse();
    // dateTime = convertDate(`${month}/${day}/${year}`);
    const parts = excelNumber.split('/');
    if (parts.length === 3) {
      const day = parseInt(parts[0], 10);
      const month = parseInt(parts[1], 10);
      const year = parseInt(parts[2], 10);
      dateTime = convertDate(`${month}/${day}/${year}`);
    }
  }
  return dateTime;
}