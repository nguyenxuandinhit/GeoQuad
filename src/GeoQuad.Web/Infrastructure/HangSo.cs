namespace GeoQuad.Web.Infrastructure;

/// <summary>Tên cookie dùng chung (khách chọn lớp — US-07).</summary>
public static class GqCookie
{
    public const string Lop = "gq_lop";
}

/// <summary>Quy ước nghiệp vụ dùng chung.</summary>
public static class QuyUoc
{
    /// <summary>Lớp mặc định của khách khi chưa chọn (US-07).</summary>
    public const int LopMacDinh = 8;

    public const int LopNhoNhat = 1;
    public const int LopLonNhat = 12;
}
