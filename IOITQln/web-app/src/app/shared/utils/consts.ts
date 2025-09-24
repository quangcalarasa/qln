import { STColumn, STColumnTag, STColumnTagValue } from '@delon/abc/st';
import { CurrencyMaskInputMode } from 'ngx-currency';
import { DecreeEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';

//Pattern
export const Only_Text_Pattern = '^[a-zA-Z ]*$';
export const Only_Number_Pattern = '^(0|[1-9][0-9]*)$';
export const rgxPassword = new RegExp('^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{7,}$');
export const errMsgRgxPassword = "Mật khẩu tối thiểu 8 kí tự, phải có 1 chữ thường, 1 chữ hoa, 1 số và 1 ký tự đặc biệt!";

//Hệ só hẻm trải đất nghị định 61
export const IsAlley_61_Coefficient = 0.8;

// Hệ số
export const Coefficient = {
  1: 'Hệ số lương',
  2: 'Hệ số thời điểm bố trí',
  3: 'Hệ số VAT'
};
export const DecreeMd167 = [
  { Name: '99/2015/NĐ-CP', Id: 99 },
  { Name: '34/2013/NĐ-CP', Id: 34 },
  { Name: '61/NĐ-CP', Id: 61 }
];
export const KiosStatus = [
  { Name: 'Đã cho thuê', Id: 1 },
  { Name: 'Chưa cho thuê', Id: 2 }
];
export const LandType = [
  { Name: 'Đất ở', Id: 1 },
  { Name: 'Đất thương mại, dịch vụ', Id: 2 },
  { Name: 'Đất sản xuất, kinh doanh phi nông nghiệp không phải là đất thương mại, dịch vụ', Id: 3 },
  { Name: 'Đất sử dụng vào các mục đích công cộng có mục đích kinh doanh', Id: 4 },
  { Name: 'Đất xây dựng trụ sở cơ quan, đất công trình sự nghiệp', Id: 5 }
];
export const PlanContent = [
  { Name: 'Quản lý cho thuê', Id: 1 },
  { Name: 'Tạm quản lý chờ bố trí', Id: 2 },
  { Name: 'Bán', Id: 3 },
  { Name: 'Khác', Id: 4 }
];
export const PurposeUsing = [
  { Name: 'Quản lý cho thuê', Id: 1 },
  { Name: 'Tạm quản lý chờ bố trí', Id: 2 },
  { Name: 'Đề xuất phương án xử lý, sắp xếp', Id: 3 },
  { Name: 'Bán', Id: 4 },
  { Name: 'Khác', Id: 5 }
];
//Cấu hình format currency
export const customCurrencyMaskConfig = {
  align: 'left',
  allowNegative: true,
  allowZero: true,
  decimal: ',',
  precision: 0,
  prefix: '',
  suffix: '',
  thousands: '.',
  nullable: true,
  min: undefined,
  max: undefined,
  inputMode: CurrencyMaskInputMode.NATURAL
};

export const TypeLogAction = {
  // 0: 'Xem',
  1: 'Thêm mới',
  2: 'Cập nhật',
  3: 'Xóa'
  // 4: '',
  // 5: '',
  // 6: '',
  // 7: '',
  // 8: ''
};

export const TypeReportApply = {
  //Loại biên bản áp dụng
  1: 'Nhà hộ chung',
  2: 'Nhà riêng lẻ',
  3: 'Nhà chung cư',
  4: 'Bán phần diện tích sử dụng chung',
  5: 'Bán phần diện tích đất liền kề'
};

export const LevelBlock = {
  1: 'Cấp 1',
  2: 'Cấp 2',
  3: 'Cấp 3',
  4: 'Cấp 4'
};

export const LevelAlley = {
  1: 'Hẻm cấp 1',
  2: 'Hẻm cấp 2',
  3: 'Hẻm cấp còn lại'
};

export const LocationResidentialLand = {
  1: 'Vị trí 1',
  2: 'Vị trí 2',
  3: 'Vị trí 3',
  4: 'Vị trí 4'
};

export const TypeMainTexTure = {
  1: 'Móng',
  2: 'Khung cột',
  3: 'Tường',
  4: 'Nền, sàn',
  5: 'Khung cột đỡ mái',
  6: 'Mái'
};

export const NumberFloor = {
  2: '2 tầng',
  3: '3 tầng',
  4: '4 tầng',
  5: '5 tầng trở lên'
};

export const TypeSex = {
  1: 'Nam',
  2: 'Nữ',
  3: 'Khác'
};

export const TypeLandSpecial = {
  1: 'Đoạn đường nằm hai bên dạ cầu (song song cầu)',
  2: 'Đoạn đường nằm hai bên cầu vượt (song song cầu)',
  3: 'Nằm trong hành lang bảo vệ của đường điện cao thế',
  4: 'Đường nhánh dẫn lên cầu vượt',
  5: 'Cách lề đường bằng một con kênh, rạch không được san lấp'
};

//Điều khoản áp dụng
export const TermApply = {
  134: 'Điều 27, khoản 1 điều 34',
  234: 'Khoản 2, Điều 34',
  35: 'Điều 35',
  65: 'Điều 65',
  70: 'Điều 70',
  71: 'Điều 71',
  61: 'Điều 7'
};

//Phân hệ chức năng
export const SubSystem = {
  1: 'Chung',
  2: 'Nhà ở cũ',
  3: 'Nhà tái định cư'
};

//Nghị định
export const Decree = {
  99: '99/2015/NĐ-CP',
  34: '34/2013/NĐ-CP',
  61: '61/NĐ-CP'
};

//Hiện trạng sử dụng
export const UsageStatus = {
  1: 'Đang cho thuê',
  2: 'Nhà trống',
  3: 'Bị chiếm dụng',
  4: 'Các trường hợp khác'
};

//Loại hẻm
export const TypeAlley = {
  1: 'Đất nằm mặt tiền hẻm chính',
  2: 'Đất nằm ở hẻm phụ'
};

export const FileStatus = {
  //Trạng thái hồ sơ
  1: 'Tiếp nhận',
  2: 'Kiểm tra hồ sơ',
  3: 'Đã xử lý'
};
export const TypeCoefficient = {
  1: 'Bảng hệ số tỉnh thành',
  2: 'Bảng hệ tầng cao',
  3: 'Bảng hệ số K5',
  5: 'Bảng hệ số khu vực',
  6: 'Bảng hệ số kỹ thuật',
  7: 'Bảng hệ số giá thuê'
};
export const TypeQD = {
  1753: 'QĐ 1753',
  9: 'QĐ 09',
  22: 'QĐ 22-2'
};

export const TypeSupportReq = {
  1: 'Đã tiếp nhận',
  2: 'Đang xử lý',
  3: 'Hoàn thành',
}

export const TypePersonSupportName = {
  1: 'Nhân viên',
  2: 'Quản lý',
}

export const TypeHouse = {
  1: 'Đủ điều kiện',
  2: 'Chưa đủ điều kiện',
  3: 'Không đủ điều kiện'
};

export const TypeReception = {
  1: 'Đã tiếp nhận',
  2: 'Chưa tiếp nhận',
  3: 'Không tiếp nhận'
};

export const TypeUsageStatus = {
  1: 'Đã tiếp nhận',
  2: 'Chưa tiếp nhận',
  3: 'Không tiếp nhận'
};

export const TypeOverYear = {
  1: 'Trả góp',
  2: 'Tạm cư',
  3: 'Thuê',
  4: 'Bán 1 lần'
}

export const TypeNotificationForm ={
  1: 'Bm001',
  2: 'Bm002',
  3: 'Bm003',
  4: 'Bm004',
  5: 'Bm005',
}

//Loại giá của hợp đồng 167
export const TypePriceContract167 = {
  1: 'Giá niêm yết',
  2: 'Đấu giá'
};

//Thơi gian thuê - hợp đồng 167
export const RentalPeriodContract167 = {
  1: 'Tạm bố trí',
  2: 'Thuê 1 năm',
  3: 'Thuê 5 năm',
  4: 'Khác'
};

//Mục đích thuê - hợp đồng 167
export const RentalPurposeContract167 = {
  1: 'Kinh doanh, dịch vụ',
  2: 'Cơ sở sản xuất',
  3: 'Kho bãi',
  4: 'Khác'
};

//Kỳ thanh toán - hợp đồng 167
export const PaymentPeriodContract167 = {
  1: 'Tháng',
  2: 'Quý'
};

//Trạng thái hợp đồng 167
export const ContractStatus167 = {
  1: 'Còn hiệu lực',
  2: 'Hết hiệu lực'
};

//Loại hợp đồng 167
export const Contract167Type = {
  1: 'Hợp đồng',
  2: 'Phụ lục hợp đồng'
};

export const ModuleSystem = {
  1: 'Nhà ở cũ',
  2: 'Nhà tái định cư',
  3: 'Nhà ở 167'
}