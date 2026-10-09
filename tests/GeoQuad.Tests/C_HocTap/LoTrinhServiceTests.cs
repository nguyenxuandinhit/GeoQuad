using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.C_HocTap;

public sealed class LoTrinhServiceTests
{
    private sealed class Repo : ILoTrinhRepository
    {
        public string? MarkedTarget { get; private set; }
        public string? MarkedConcept { get; private set; }
        public Task<int?> LopHienTaiAsync(string id) => Task.FromResult<int?>(8);
        public Task<IReadOnlyList<KhaiNiemLoTrinh>> MucTieuHopLeAsync(int lop) => Task.FromResult<IReadOnlyList<KhaiNiemLoTrinh>>(
        [new("HINH_VUONG", "Hình vuông", 8, "DA_RA_SOAT"), new("HINH_THOI", "Hình thoi", 6, "DA_RA_SOAT")]);
        public Task<LoTrinhDoThi?> DoThiAsync(string id, string target) => Task.FromResult<LoTrinhDoThi?>(
            target == "HINH_VUONG" ? new(
                [new("TU_GIAC", "Tứ giác", 6, "DA_RA_SOAT"), new("HINH_VUONG", "Hình vuông", 8, "DA_RA_SOAT")],
                new Dictionary<string, IReadOnlyCollection<string>> { ["HINH_VUONG"] = ["TU_GIAC"], ["TU_GIAC"] = [] },
                new HashSet<string> { "TU_GIAC" }) :
            target == "HINH_THOI" ? new(
                [new("HINH_THOI", "Hình thoi", 6, "DA_RA_SOAT")],
                new Dictionary<string, IReadOnlyCollection<string>> { ["HINH_THOI"] = [] },
                new HashSet<string>()) : null);
        public Task<bool> DanhDauDaHocAsync(string id, string target, string ma, IReadOnlyCollection<string> closure)
        {
            MarkedTarget = target; MarkedConcept = ma; return Task.FromResult(true);
        }
    }

    private sealed class User : ICurrentUser
    {
        public bool DaDangNhap => TaiKhoanId is not null;
        public string? TaiKhoanId => "student-1";
        public string? BietDanh => "Mai";
        public bool LaQuanTri => false;
        public int Lop => 8;
        public bool XemNangCao => false;
        public int LopHienThi => 8;
    }

    [Fact]
    public async Task XemMacDinh_ChonMucTieuCaoNhatTrongLopVaTinhTienDoDistinct()
    {
        var service = new LoTrinhService(new Repo(), new User());

        var model = await service.XemAsync(null);

        Assert.Equal("HINH_VUONG", model.MucTieu);
        Assert.Equal(1, model.SoDaHoc);
        Assert.Equal(2, model.TongSo);
        Assert.Equal(50, model.PhanTram);
        Assert.Equal("TU_GIAC", model.CacBuoc[0].Ma);
        Assert.False(model.CoTheHocNgay);
    }

    [Fact]
    public async Task MucTieuKhongTienQuyet_CoTheHocNgay()
    {
        var service = new LoTrinhService(new Repo(), new User());

        var model = await service.XemAsync("HINH_THOI");

        Assert.Null(model.Loi);
        Assert.Single(model.CacBuoc);
        Assert.True(model.CoTheHocNgay);
    }

    [Fact]
    public async Task EmDaHieu_RejectsConceptOutsideSelectedRoadmap()
    {
        var repo = new Repo();
        var service = new LoTrinhService(repo, new User());

        var result = await service.EmDaHieuAsync("HINH_VUONG", "UNRELATED");

        Assert.False(result);
        Assert.Null(repo.MarkedConcept);
    }

    [Fact]
    public async Task EmDaHieu_PassesSelectedTargetAndRoadmapMembershipToRepository()
    {
        var repo = new Repo();
        var service = new LoTrinhService(repo, new User());

        var result = await service.EmDaHieuAsync("HINH_VUONG", "TU_GIAC");

        Assert.True(result);
        Assert.Equal("HINH_VUONG", repo.MarkedTarget);
        Assert.Equal("TU_GIAC", repo.MarkedConcept);
    }
}
