using Microsoft.AspNetCore.Identity;

namespace test.Models;

public static class KhoiTaoDuLieu
{
    // Hàm khởi tạo toàn bộ CSDL và dữ liệu mẫu nếu chưa có
    public static void KhoiTao(QuanLyNhaHangContext db)
    {
        // 1. Tự động tạo CSDL và bảng nếu chưa tồn tại
        db.Database.EnsureCreated();

        var hasher = new PasswordHasher<NhanVien>();

        // 2. Tự động thêm tài khoản nhân viên / quản lý nếu bảng đang trống
        if (!db.NhanViens.Any())
        {
            var admin = new NhanVien
            {
                HoTen = "Quản Lý Hệ Thống",
                ChucVu = "Quản lý",
                TaiKhoan = "admin",
                SDT = "0901234567",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today),
                MatKhau = ""
            };
            admin.MatKhau = hasher.HashPassword(admin, "123456");

            var quanly2 = new NhanVien
            {
                HoTen = "Nguyễn Văn Quản Lý",
                ChucVu = "Quản lý",
                TaiKhoan = "quanly",
                SDT = "0901234568",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today),
                MatKhau = ""
            };
            quanly2.MatKhau = hasher.HashPassword(quanly2, "123456");

            var thungan = new NhanVien
            {
                HoTen = "Trần Thị Thu Ngân",
                ChucVu = "Thu ngân",
                TaiKhoan = "thungan",
                SDT = "0902345678",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today),
                MatKhau = ""
            };
            thungan.MatKhau = hasher.HashPassword(thungan, "123456");

            var daubep = new NhanVien
            {
                HoTen = "Lê Văn Bếp Trưởng",
                ChucVu = "Đầu bếp",
                TaiKhoan = "daubep",
                SDT = "0903456789",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today),
                MatKhau = ""
            };
            daubep.MatKhau = hasher.HashPassword(daubep, "123456");

            var phucvu = new NhanVien
            {
                HoTen = "Phạm Văn Phục Vụ",
                ChucVu = "Phục vụ",
                TaiKhoan = "phucvu",
                SDT = "0904567890",
                NgayVaoLam = DateOnly.FromDateTime(DateTime.Today),
                MatKhau = ""
            };
            phucvu.MatKhau = hasher.HashPassword(phucvu, "123456");

            db.NhanViens.AddRange(admin, quanly2, thungan, daubep, phucvu);
            db.SaveChanges();
        }

        // 3. Tự động thêm Danh mục và Món ăn mẫu nếu chưa có
        if (!db.DanhMucs.Any())
        {
            var dm1 = new DanhMuc { MaDanhMuc = 1, TenDanhMuc = "Món Khai Vị", TrangThai = "Hoạt động" };
            var dm2 = new DanhMuc { MaDanhMuc = 2, TenDanhMuc = "Món Chính", TrangThai = "Hoạt động" };
            var dm3 = new DanhMuc { MaDanhMuc = 3, TenDanhMuc = "Đồ Uống", TrangThai = "Hoạt động" };
            db.DanhMucs.AddRange(dm1, dm2, dm3);
            db.SaveChanges();

            if (!db.MonAns.Any())
            {
                db.MonAns.AddRange(
                    new MonAn { TenMon = "Gỏi Ngó Sen Tôm Thịt", Gia = 85000, MaDanhMuc = 1 },
                    new MonAn { TenMon = "Bò Lúc Lắc Khoai Tây", Gia = 150000, MaDanhMuc = 2 },
                    new MonAn { TenMon = "Lẩu Thái Hải Sản", Gia = 280000, MaDanhMuc = 2 },
                    new MonAn { TenMon = "Trà Đào Cam Sả", Gia = 35000, MaDanhMuc = 3 }
                );
                db.SaveChanges();
            }
        }

        // 4. Tự động thêm Bàn ăn mẫu nếu chưa có
        if (!db.Bans.Any())
        {
            db.Bans.AddRange(
                new Ban { SoBan = "Bàn 01", SoChoNgoi = 4, TrangThai = "Trống" },
                new Ban { SoBan = "Bàn 02", SoChoNgoi = 4, TrangThai = "Trống" },
                new Ban { SoBan = "Bàn 03", SoChoNgoi = 6, TrangThai = "Trống" },
                new Ban { SoBan = "Bàn VIP 01", SoChoNgoi = 10, TrangThai = "Trống" }
            );
            db.SaveChanges();
        }

        // 5. Mã hóa các mật khẩu chữ thường nếu còn sót
        MaHoaMatKhauCu(db);
    }

    public static void MaHoaMatKhauCu(QuanLyNhaHangContext db)
    {
        var hasher = new PasswordHasher<NhanVien>();
        var daDoi = false;

        foreach (var nv in db.NhanViens.ToList())
        {
            if (!string.IsNullOrEmpty(nv.MatKhau) && !nv.MatKhau.StartsWith("AQAAAA"))
            {
                nv.MatKhau = hasher.HashPassword(nv, nv.MatKhau);
                daDoi = true;
            }
        }

        if (daDoi) db.SaveChanges();
    }
}
