using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;

namespace test.Controllers
{
    // Trang quản trị: nhân viên thấy trang chủ phục vụ, quản lý thấy bảng điều khiển
    [Authorize(Roles = VaiTro.QuanLy + "," + VaiTro.NhanVien)]
    public class QuanTriController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public QuanTriController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // GET: /QuanTri
        public async Task<IActionResult> Index()
        {
            var homNay = DateOnly.FromDateTime(DateTime.Today);
            var dauNgay = homNay.ToDateTime(TimeOnly.MinValue);
            var cuoiNgay = dauNgay.AddDays(1);

            var model = new TrangQuanTriViewModel
            {
                SoHoaDonChuaThanhToan = await _context.HoaDons
                    .CountAsync(h => h.TrangThai == TrangThaiHoaDon.ChuaThanhToan),
                SoDatBanChoXacNhan = await _context.DatBans
                    .CountAsync(d => d.TrangThai == TrangThaiDatBan.ChoXacNhan && d.NgayGio >= dauNgay),
                SoBanDangDung = await _context.Bans.CountAsync(b => b.TrangThai == TrangThaiBan.DangDung),
                SoBanDangHoatDong = await _context.Bans.CountAsync(b => b.TrangThai != TrangThaiBan.NgungSuDung),
                SoLoCanhBao = await _context.TonKhos.CountAsync(t => t.SoLuongTon > 0 && t.NgayHetHan != null
                    && t.NgayHetHan <= homNay.AddDays(QuyDinhKho.SapHetHanNgay))
            };

            if (User.IsInRole(VaiTro.QuanLy))
            {
                await NapBangDieuKhienQuanLy(model, homNay, dauNgay, cuoiNgay);
            }
            else
            {
                await NapTrangChuNhanVien(model, dauNgay);
            }

            return View(model);
        }

        // ===== Bảng điều khiển quản lý: doanh thu, biểu đồ, nhân sự, yêu cầu cần duyệt =====
        private async Task NapBangDieuKhienQuanLy(TrangQuanTriViewModel model, DateOnly homNay, DateTime dauNgay, DateTime cuoiNgay)
        {
            // Doanh thu là thông tin nhạy cảm nên chỉ quản lý thấy
            var thuHomNay = _context.ThanhToans
                .Where(t => t.TrangThai == TrangThaiThanhToan.ThanhCong && t.NgayTT >= dauNgay && t.NgayTT < cuoiNgay);

            model.ThuHomNay = await thuHomNay.SumAsync(t => (decimal?)t.SoTien) ?? 0;

            // Doanh thu theo giờ: gom trong SQL rồi điền vào mảng 24 ô
            var theoGio = await thuHomNay
                .GroupBy(t => t.NgayTT.Hour)
                .Select(g => new { Gio = g.Key, Tien = g.Sum(x => x.SoTien) })
                .ToListAsync();
            foreach (var g in theoGio) model.DoanhThuTheoGio[g.Gio] = g.Tien;

            model.SoHoaDonHomNay = await _context.HoaDons
                .CountAsync(h => h.NgayLap >= dauNgay && h.NgayLap < cuoiNgay && h.TrangThai != TrangThaiHoaDon.DaHuy);

            model.SoChuyenKhoanChoXacNhan = await _context.ThanhToans
                .CountAsync(t => t.TrangThai == TrangThaiThanhToan.ChoXacNhan);

            // Món bán chạy 7 ngày gần nhất (tính trên hóa đơn đã thanh toán)
            var tu = dauNgay.AddDays(-6);
            var chiTiet = _context.ChiTietHoaDons
                .Where(c => c.HoaDon.TrangThai == TrangThaiHoaDon.DaThanhToan && c.HoaDon.NgayLap >= tu && c.HoaDon.NgayLap < cuoiNgay);

            model.TopMon = await chiTiet
                .GroupBy(c => c.MonAn.TenMon)
                .Select(g => new DiemBieuDo { Nhan = g.Key, GiaTri = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.GiaTri)
                .Take(5)
                .ToListAsync();

            var theoNgay = await chiTiet
                .GroupBy(c => c.HoaDon.NgayLap.Date)
                .Select(g => new { Ngay = g.Key, SoPhan = g.Sum(x => x.SoLuong) })
                .ToListAsync();
            for (var i = 0; i < 7; i++)
            {
                var ngay = tu.AddDays(i);
                model.SoPhanBan7Ngay.Add(new DiemBieuDo
                {
                    Nhan = ngay.ToString("dd/MM"),
                    GiaTri = theoNgay.FirstOrDefault(x => x.Ngay == ngay)?.SoPhan ?? 0
                });
            }

            // Nhân sự: ai đang có công việc được phân công (chưa kết thúc)
            var phanCongHienTai = await _context.PhanCongs
                .Where(p => p.NgayBatDau <= homNay && (p.NgayKetThuc == null || p.NgayKetThuc >= homNay))
                .Select(p => new { p.MaNV, p.CongViec.TenCV })
                .ToListAsync();

            var nhanViens = await _context.NhanViens.OrderBy(n => n.HoTen).ToListAsync();
            model.SoNhanVienDangPhanCong = phanCongHienTai.Select(p => p.MaNV).Distinct().Count();
            model.NhanSu = nhanViens.Take(6).Select(n =>
            {
                var cv = phanCongHienTai.Where(p => p.MaNV == n.MaNV).Select(p => p.TenCV).ToList();
                return new NhanSuDong
                {
                    HoTen = n.HoTen,
                    ChucVu = n.ChucVu,
                    CongViec = cv.Count > 0 ? string.Join(", ", cv) : "—",
                    DangPhanCong = cv.Count > 0
                };
            }).ToList();
        }

        // ===== Trang chủ nhân viên: đơn đang xử lý + yêu cầu cần làm =====
        private async Task NapTrangChuNhanVien(TrangQuanTriViewModel model, DateTime dauNgay)
        {
            var hoaDons = await _context.HoaDons
                .Where(h => h.TrangThai == TrangThaiHoaDon.ChuaThanhToan)
                .OrderBy(h => h.NgayLap)
                .Take(6)
                .Select(h => new
                {
                    h.MaHD,
                    h.Ban.SoBan,
                    h.NgayLap,
                    h.TongTien,
                    Mon = h.ChiTietHoaDons.Select(c => new { c.MonAn.TenMon, c.SoLuong }).ToList()
                })
                .ToListAsync();

            var maChoXacNhan = await _context.ThanhToans
                .Where(t => t.TrangThai == TrangThaiThanhToan.ChoXacNhan)
                .Select(t => t.MaHD).Distinct().ToListAsync();

            model.DonDangXuLy = hoaDons.Select(h => new DonDangXuLyDong
            {
                MaHD = h.MaHD,
                SoBan = h.SoBan,
                NgayLap = h.NgayLap,
                TongTien = h.TongTien,
                CoChoXacNhan = maChoXacNhan.Contains(h.MaHD),
                Mon = h.Mon.Select(m => (m.TenMon, m.SoLuong)).ToList()
            }).ToList();

            model.DatBanChoXacNhan = await (
                from d in _context.DatBans
                join b in _context.Bans on d.MaBan equals b.MaBan
                join k in _context.KhachHangs on d.MaKH equals k.MaKH
                where d.TrangThai == TrangThaiDatBan.ChoXacNhan && d.NgayGio >= dauNgay
                orderby d.NgayGio
                select new DatBanQuanLyDong
                {
                    MaDatBan = d.MaDatBan,
                    NgayGio = d.NgayGio,
                    SoBan = b.SoBan,
                    SoNguoi = d.SoNguoi,
                    TenKhach = k.HoTen,
                    SDTKhach = k.SDT ?? "",
                    TrangThai = d.TrangThai
                }).Take(4).ToListAsync();
        }
    }
}
