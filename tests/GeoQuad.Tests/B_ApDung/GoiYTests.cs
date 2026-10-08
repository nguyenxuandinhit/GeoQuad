using GeoQuad.Tests.B_ApDung.Doubles;
using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Tests.B_ApDung;

public sealed class GoiYTests
{
    private readonly GoiYRepositoryGia _repo = new();

    [Fact]
    public async Task DauHieuDuDieuKienDungDauVaKhongNhanBan()
    {
        var result = await new GoiYService(_repo, new CurrentUserGia()).TimAsync(
            new GoiYRequest("HINH_BINH_HANH", "HINH_CHU_NHAT", ["DK_MOT_GOC_VUONG", "DK_MOT_GOC_VUONG"]));
        Assert.Equal("DH_HCN_2", result.The.First().DauHieu.Ma);
        Assert.Empty(result.The.First().ConThieu);
        Assert.Single(result.The.First().DaCo);
        Assert.Null(result.The.First().DauHieu.MaChungMinh);
    }

    [Fact]
    public async Task SuyRaTrucTiepKhongCanTimChuoi()
    {
        _repo.SuyRa = true;
        var result = await new GoiYService(_repo, new CurrentUserGia()).TimAsync(
            new GoiYRequest("HINH_VUONG", "HINH_CHU_NHAT", []));
        Assert.Contains("Mọi", result.ThongBao);
        Assert.Empty(result.The);
        Assert.Equal(0, _repo.SoLanTimChuoi);
    }

    [Fact]
    public async Task ChuoiHienThiThieuTungBuocVaGioiHanBa()
    {
        _repo.Direct = [];
        _repo.Chains = [["DH_HCN_1", "DH_HV_1"]];
        var result = await new GoiYService(_repo, new CurrentUserGia()).TimAsync(new GoiYRequest("TU_GIAC", "HINH_VUONG", []));
        Assert.Single(result.Chuoi);
        Assert.Equal(2, result.Chuoi[0].Count);
        Assert.All(result.Chuoi[0], card => Assert.Single(card.ConThieu));
    }

    [Fact]
    public async Task MaLaVaDieuKienNgoaiCatalogBiTuChoi()
    {
        var service = new GoiYService(_repo, new CurrentUserGia());
        await Assert.ThrowsAsync<ArgumentException>(() => service.TimAsync(new GoiYRequest("HINH_LA", "HINH_CHU_NHAT", [])));
        await Assert.ThrowsAsync<ArgumentException>(() => service.TimAsync(new GoiYRequest("TU_GIAC", "HINH_CHU_NHAT", ["DK_LA"])));
    }

    [Fact]
    public async Task LopBaNhanThongBaoLopTam()
    {
        var result = await new GoiYService(_repo, new CurrentUserGia(3)).TimAsync(new GoiYRequest("TU_GIAC", "HINH_CHU_NHAT", []));
        Assert.Contains("lớp 8", result.ThongBao);
        Assert.Empty(result.The);
    }

    private sealed class GoiYRepositoryGia : IGoiYRepository
    {
        public bool SuyRa;
        public int SoLanTimChuoi;
        public IReadOnlyList<string> Direct = ["DH_HCN_3", "DH_HCN_2"];
        public IReadOnlyList<IReadOnlyList<string>> Chains = [];
        private static readonly DieuKienItem Right = new("DK_MOT_GOC_VUONG", "Có một góc vuông", 8);
        private static readonly DieuKienItem Diagonal = new("DK_CHEO_BANG_NHAU", "Hai đường chéo bằng nhau", 8);
        public Task<GoiYCatalog> CatalogAsync(int lop) => Task.FromResult(new GoiYCatalog(
            new[] { "TU_GIAC", "HINH_BINH_HANH", "HINH_CHU_NHAT", "HINH_VUONG" }.Select(ma => new HinhItem(ma, ma, 1)).ToArray(),
            [Right, Diagonal],
            [new("DH_HCN_2", "Góc vuông", "HINH_BINH_HANH", "HINH_CHU_NHAT", 8, [Right], null),
             new("DH_HCN_3", "Chéo bằng", "HINH_BINH_HANH", "HINH_CHU_NHAT", 8, [Diagonal], null),
             new("DH_HCN_1", "Ba góc vuông", "TU_GIAC", "HINH_CHU_NHAT", 8, [Right], null),
             new("DH_HV_1", "Hai cạnh kề bằng", "HINH_CHU_NHAT", "HINH_VUONG", 8, [Diagonal], null)]));
        public Task<bool> SuyRaAsync(string nen, string dich, int lop) => Task.FromResult(SuyRa);
        public Task<IReadOnlyList<string>> TrucTiepAsync(string nen, string dich, int lop) => Task.FromResult(Direct);
        public Task<IReadOnlyList<IReadOnlyList<string>>> ChuoiAsync(string nen, string dich, int lop)
        { SoLanTimChuoi++; return Task.FromResult(Chains); }
    }
}
