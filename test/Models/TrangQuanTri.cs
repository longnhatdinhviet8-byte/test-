namespace test.Models;

// Số liệu hiển thị trên trang quản trị (bảng điều khiển quản lý + trang chủ nhân viên)
public class TrangQuanTriViewModel
{
    // ----- Dùng chung -----
    public int SoHoaDonChuaThanhToan { get; set; }
    public int SoDatBanChoXacNhan { get; set; }
    public int SoBanDangDung { get; set; }
    public int SoBanDangHoatDong { get; set; }
    public int SoLoCanhBao { get; set; }          // lô kho sắp hết hạn hoặc đã hết hạn
    public decimal? ThuHomNay { get; set; }       // chỉ quản lý được xem

    // ----- Bảng điều khiển quản lý -----
    public int SoHoaDonHomNay { get; set; }                  // "Tổng số đơn"
    public int SoNhanVienDangPhanCong { get; set; }          // nhân viên đang có công việc được phân công
    public int SoChuyenKhoanChoXacNhan { get; set; }         // khách báo đã chuyển khoản, chờ đối soát
    public decimal[] DoanhThuTheoGio { get; set; } = new decimal[24];   // doanh thu hôm nay theo từng giờ
    public List<DiemBieuDo> SoPhanBan7Ngay { get; set; } = new();       // số phần bán mỗi ngày (7 ngày)
    public List<DiemBieuDo> TopMon { get; set; } = new();               // 5 món bán chạy (7 ngày)
    public List<NhanSuDong> NhanSu { get; set; } = new();

    // ----- Trang chủ nhân viên -----
    public List<DonDangXuLyDong> DonDangXuLy { get; set; } = new();
    public List<DatBanQuanLyDong> DatBanChoXacNhan { get; set; } = new();
}

public class DiemBieuDo
{
    public string Nhan { get; set; } = "";
    public decimal GiaTri { get; set; }
}

public class NhanSuDong
{
    public string HoTen { get; set; } = "";
    public string ChucVu { get; set; } = "";
    public string CongViec { get; set; } = "";
    public bool DangPhanCong { get; set; }
}

// Một thẻ "đơn hàng đang xử lý" (hóa đơn chưa thanh toán của một bàn)
public class DonDangXuLyDong
{
    public int MaHD { get; set; }
    public string SoBan { get; set; } = "";
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }
    public bool CoChoXacNhan { get; set; }
    public List<(string TenMon, int SoLuong)> Mon { get; set; } = new();
}
