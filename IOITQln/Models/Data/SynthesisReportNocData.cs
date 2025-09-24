using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Data
{
    public class SynthesisReportNocData
    {
        public string LoaiBienBan { get; set; }
        public string SoTtCan { get; set; }
        public string SoTtHo { get; set; }
        public string MaDinhDanh { get; set; }
        public string SoNha { get; set; }
        public string Duong { get; set; }
        public string Phuong { get; set; }
        public string Quan { get; set; }
        public string LoaiNha { get; set; }
        public string CapNha { get; set; }
        public float? DienTichXayDung { get; set; }
        public float? KhuonVien { get; set; }
        public float? DienTichDatRieng { get; set; }
        public float? DienTichDatChung { get; set; }
        public float? DienTichDatChungPhanBo { get; set; }
        public float? DienTichSuDungRieng { get; set; }
        public float? DienTichSuDungChung { get; set; }
        public float? DienTichSuDungChungPhanBo { get; set; }
        public float? TongDienTichSuDungChung { get; set; }
        public float? TongDienTichSuDung { get; set; }
        public string ViTri { get; set; }
        public string HeSoHem { get; set; }
        public string HeSoPhanBo { get; set; }
        public string DonGiaDat { get; set; }
        public float? ThueSuat { get; set; }
        public decimal? TienThuePhaiNopTungNam { get; set; }
        public string NguoiDaiDien { get; set; }
        public string CanCuoc { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NoiCap { get; set; }
        public string DiaChiThuongTru { get; set; }
        public string SoDienThoai { get; set; }
        public string ThanhVien { get; set; }
        public string ThoiDiemGiaoNhanNha { get; set; }
        public string ThoiDiemBoTriSuDung { get; set; }
        public string SoQuyetDinh { get; set; }
        public DateTime? NgayKyQuyetDinh { get; set; }
        public string TenBanVe { get; set; }
        public DateTime? NgayLapBanVe { get; set; }
        public string TinhTrangNha { get; set; }
        public string GhiChu { get; set; }
        public string TrangThaiHopDong { get; set; }
        public string SoGiayXacNhan { get; set; }
        public float? NoCuCoVat { get; set; }
        public string SoHopDong { get; set; }
        public DateTime? NgayKyHopDong { get; set; }
        public decimal? GiaThueNhaTheoCongIch { get; set; }
        public decimal? GiaThueNhaHopDongTam { get; set; }
        public decimal? GiaThueNhaChinhThuc { get; set; }
        public decimal? GiaBanNha { get; set; }
        public float? DienTichNha { get; set; }
        public decimal? GiaBanDat { get; set; }
        public float? DienTichDat { get; set; }
        public decimal? SoTienMienGiamNha { get; set; }
        public decimal? SoTienMienGiamDat { get; set; }
        public decimal? ThanhTien { get; set; }
        public decimal? TrangThai { get; set; }
        public string SoQuyetDinhBanNha { get; set; }
        public DateTime? NgayQuyetDinhBanNha { get; set; }
        public string SoBienBanThanhLyHopDong { get; set; }
        public DateTime? NgayThanhLyHopDong { get; set; }
        public string GhiChuThongTinBan { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
        public bool? HasSaleInfo { get; set; } = false;
        public bool? HasRentInfo { get; set; } = false;
    }
}
