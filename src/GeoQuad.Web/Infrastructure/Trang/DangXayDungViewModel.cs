namespace GeoQuad.Web.Infrastructure.Trang;

/// <summary>
/// Dữ liệu cho trang "Đang xây dựng" (PHẦN 0 tạo sẵn cho mọi URL ở README mục 3.2,
/// để trang chưa làm xong không trả về lỗi 404).
/// </summary>
public sealed record DangXayDungViewModel(string TieuDe, string Story, string Phan, string MoTa);
