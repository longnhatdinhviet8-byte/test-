using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;

namespace test.Controllers
{
    // Trang "Gọi món tại quán" (mở từ menu "Đặt bàn"): khách chọn bàn của mình, trang hiện mã QR,
    // khách quét bằng điện thoại để vào thực đơn của đúng bàn đó (/ban/{token}).
    public class GoiMonTaiQuanController : Controller
    {
        // true : ai cũng chọn bàn và xem QR được (tiện cho demo / màn hình đặt ở quầy).
        // false: chỉ nhân viên / quản lý thấy danh sách bàn; khách phải quét QR đã in dán trên bàn.
        // Lưu ý: mã QR chính là "chìa khóa" của bàn. Để true thì ai mở trang này cũng gọi món được cho bất kỳ bàn nào,
        // kể cả người không ngồi tại quán.
        private const bool KhachTuChonBan = true;

        private readonly QuanLyNhaHangContext _context;

        public GoiMonTaiQuanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var laNhanVien = User.IsInRole(VaiTro.QuanLy) || User.IsInRole(VaiTro.NhanVien);
            var thayDuocBan = KhachTuChonBan || laNhanVien;

            // Bàn ngừng sử dụng thì không ai gọi món được nên không đưa vào danh sách chọn
            var bans = thayDuocBan
                ? await _context.Bans
                    .Where(b => b.TrangThai != TrangThaiBan.NgungSuDung)
                    .OrderBy(b => b.SoBan)
                    .ToListAsync()
                : new List<Ban>();

            ViewBag.ThayDuocBan = thayDuocBan;
            return View(bans);
        }
    }
}
