using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.C_HocTap;

public sealed class GoiYServiceTests
{
    private sealed class Repo(IReadOnlyList<LuotLamTomTat> history, IReadOnlyList<BaiTapGoiY> candidates) : ITienDoRepository
    {
        public Task<int?> LopHienTaiAsync(string id) => Task.FromResult<int?>(8);
        public Task<IReadOnlyList<LuotLamTomTat>> LichSuAsync(string id) => Task.FromResult(history);
        public Task<IReadOnlyList<BaiTapGoiY>> UngVienAsync(string id, int lop) => Task.FromResult(candidates);
    }

    private sealed class User : ICurrentUser
    {
        public bool DaDangNhap => true;
        public string? TaiKhoanId => "student";
        public string? BietDanh => "An";
        public bool LaQuanTri => false;
        public int Lop => 8;
        public bool XemNangCao => false;
        public int LopHienThi => 8;
    }

    [Fact]
    public async Task WeakCandidates_AreUniqueAndOrderedByIncreasingDifficulty()
    {
        var history = Enumerable.Range(0, 3).Select(i => new LuotLamTomTat($"A{i}", $"OLD{i}", false,
            $"2026-10-08T00:0{i}:00Z", "HINH_THOI", "Hình thoi")).ToArray();
        var candidates = new[]
        {
            new BaiTapGoiY("BT-3", "hard", "HINH_THOI", "Hình thoi", 8, 3),
            new BaiTapGoiY("BT-1", "easy", "HINH_THOI", "Hình thoi", 8, 1),
            new BaiTapGoiY("BT-2", "medium", "HINH_THOI", "Hình thoi", 8, 2),
            new BaiTapGoiY("BT-1", "easy duplicate", "HINH_THOI", "Hình thoi", 8, 1)
        };
        var service = new GoiYService(new Repo(history, candidates), new User(), new HocTapOptions { NguongCanOn = .6m });

        var result = await service.XemAsync();

        Assert.Equal(["BT-1", "BT-2", "BT-3"], result!.BaiTap.Select(x => x.Ma));
        Assert.Equal(new[] { 1, 2, 3 }, result.BaiTap.Select(x => x.DoKho));
    }

    [Fact]
    public async Task EmptyHistory_UsesSafeFallbackAndEmptyProgressMessage()
    {
        var candidates = new[]
        {
            new BaiTapGoiY("BT-2", "hard", "HINH_THOI", "Hình thoi", 8, 3),
            new BaiTapGoiY("BT-1", "easy", "HINH_THOI", "Hình thoi", 8, 1)
        };
        var service = new GoiYService(new Repo([], candidates), new User(), new HocTapOptions());

        var result = await service.XemAsync();

        Assert.False(result!.CoLichSu);
        Assert.Equal(["BT-1", "BT-2"], result.BaiTap.Select(x => x.Ma));
        Assert.Contains("đầu tiên", result.ThongBao);
        Assert.Equal(LoaiGoiY.ChuaCoLichSu, result.Loai);
    }

    // UC-13 luồng 3a: đủ dữ liệu, không khái niệm nào cần ôn → chúc mừng, bài khó trước.
    [Fact]
    public async Task CoLichSuKhongCanOn_XepKhoTruocVaChucMung()
    {
        var history = Enumerable.Range(0, 3).Select(i => new LuotLamTomTat($"A{i}", $"OLD{i}", true,
            $"2026-10-08T00:0{i}:00Z", "HINH_THOI", "Hình thoi")).ToArray();
        var candidates = new[]
        {
            new BaiTapGoiY("BT-1", "easy", "HINH_VUONG", "Hình vuông", 8, 1),
            new BaiTapGoiY("BT-3", "hard", "HINH_VUONG", "Hình vuông", 8, 3),
            new BaiTapGoiY("BT-2", "medium", "HINH_VUONG", "Hình vuông", 8, 2)
        };
        var service = new GoiYService(new Repo(history, candidates), new User(), new HocTapOptions { NguongCanOn = .6m });

        var result = await service.XemAsync();

        Assert.Equal(LoaiGoiY.KhongCanOn, result!.Loai);
        Assert.Equal(new[] { 3, 2, 1 }, result.BaiTap.Select(x => x.DoKho));
        Assert.Contains("Chúc mừng", result.ThongBao);
    }

    [Fact]
    public async Task CoLichSuKhongCanOn_ToiDaNamBaiKhongTrung()
    {
        var history = Enumerable.Range(0, 3).Select(i => new LuotLamTomTat($"A{i}", $"OLD{i}", true,
            $"2026-10-08T00:0{i}:00Z", "HINH_THOI", "Hình thoi")).ToArray();
        var candidates = Enumerable.Range(1, 7)
            .Select(i => new BaiTapGoiY($"BT-{i}", "de", "HINH_VUONG", "Hình vuông", 8, 1 + i % 3))
            .Append(new BaiTapGoiY("BT-1", "trung", "HINH_CHU_NHAT", "Hình chữ nhật", 8, 2))
            .ToArray();
        var service = new GoiYService(new Repo(history, candidates), new User(), new HocTapOptions { NguongCanOn = .6m });

        var result = await service.XemAsync();

        Assert.Equal(5, result!.BaiTap.Count);
        Assert.Equal(5, result.BaiTap.Select(x => x.Ma).Distinct().Count());
        Assert.True(result.BaiTap.Zip(result.BaiTap.Skip(1)).All(p => p.First.DoKho >= p.Second.DoKho));
    }

    // Có lịch sử nhưng chưa khái niệm nào đủ 3 lượt: không được chúc mừng hay đẩy bài khó.
    [Fact]
    public async Task HaiLuotSai_ChuaDuDuLieu_KhongChucMungVaDeTruoc()
    {
        var history = Enumerable.Range(0, 2).Select(i => new LuotLamTomTat($"A{i}", $"OLD{i}", false,
            $"2026-10-08T00:0{i}:00Z", "HINH_THOI", "Hình thoi")).ToArray();
        var candidates = new[]
        {
            new BaiTapGoiY("BT-3", "hard", "HINH_THOI", "Hình thoi", 8, 3),
            new BaiTapGoiY("BT-1", "easy", "HINH_THOI", "Hình thoi", 8, 1)
        };
        var service = new GoiYService(new Repo(history, candidates), new User(), new HocTapOptions { NguongCanOn = .6m });

        var result = await service.XemAsync();

        Assert.Equal(LoaiGoiY.ChuaDuDuLieu, result!.Loai);
        Assert.Equal(["BT-1", "BT-3"], result.BaiTap.Select(x => x.Ma));
        Assert.DoesNotContain("Chúc mừng", result.ThongBao);
    }
}
