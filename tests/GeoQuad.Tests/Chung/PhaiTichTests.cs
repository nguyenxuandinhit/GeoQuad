using System.ComponentModel.DataAnnotations;
using GeoQuad.Web.Models;

namespace GeoQuad.Tests.Chung;

/// <summary>
/// Ô "đã đọc thông báo quyền riêng tư" của biểu mẫu đăng ký (US-05, NFR-06).
/// Trước đây dùng <c>[Range(typeof(bool), "true", "true")]</c> nên luật `range` của
/// jQuery Validate so sánh chuỗi và trang luôn báo chưa tích dù đã tích.
/// </summary>
public class PhaiTichTests
{
    private static DangKyViewModel HopLe(bool dongY) => new()
    {
        TenDangNhap = "hocsinh_8a",
        MatKhau = "Matkhau@123",
        NhapLaiMatKhau = "Matkhau@123",
        BietDanh = "Bạn lớp 8",
        Lop = 8,
        DongYQuyenRiengTu = dongY
    };

    private static List<ValidationResult> KiemTra(DangKyViewModel form)
    {
        var loi = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), loi, validateAllProperties: true);
        return loi;
    }

    [Fact]
    public void DaTichThiKhongConLoi()
        => Assert.Empty(KiemTra(HopLe(true)));

    [Fact]
    public void ChuaTichThiBaoDungMotLoi()
    {
        var loi = Assert.Single(KiemTra(HopLe(false)));

        Assert.Equal([nameof(DangKyViewModel.DongYQuyenRiengTu)], loi.MemberNames);
        Assert.Equal("Em cần tích vào ô xác nhận đã đọc thông báo quyền riêng tư "
                     + "và được cha mẹ/người giám hộ đồng ý.", loi.ErrorMessage);
    }
}
