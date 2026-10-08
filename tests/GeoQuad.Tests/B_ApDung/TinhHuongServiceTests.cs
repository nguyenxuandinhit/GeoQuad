using GeoQuad.Tests.B_ApDung.Doubles;
using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Tests.B_ApDung;

public sealed class TinhHuongServiceTests
{
    [Fact]
    public async Task UnknownContextIsRejectedBeforeScenarioRead()
    {
        var repo=new ScenarioFake();
        await Assert.ThrowsAsync<ArgumentException>(() => new TinhHuongService(repo,new CurrentUserGia()).DanhSachAsync("UNKNOWN"));
        Assert.Equal(0,repo.Reads);
    }

    [Fact]
    public async Task EmptyListKeepsAllContextsAndUsesDisplayedGrade()
    {
        var repo=new ScenarioFake();
        var model=await new TinhHuongService(repo,new CurrentUserGia(3)).DanhSachAsync("BC_NHA_CUA");
        Assert.Equal(3,model.BoiCanh.Count);Assert.Empty(model.TinhHuong);Assert.Equal("BC_NHA_CUA",model.Loc);
        Assert.Equal(3,repo.Grade);
    }

    [Fact]
    public async Task InvalidDetailDoesNotQueryAndAdvancedBadgeUsesActualGrade()
    {
        var repo=new ScenarioFake { Detail=new("TH-01","Tên","BC_NHA_CUA","Nhà cửa","Mô tả","Lời giải",true,8,[]) };
        var service=new TinhHuongService(repo,new CurrentUserGia(3,true));
        Assert.Null(await service.XemAsync(" "));Assert.Equal(0,repo.Reads);
        var model=await service.XemAsync("TH-01");
        Assert.True(model!.NangCao);Assert.Equal(12,repo.Grade);Assert.Equal("TH-01",repo.Code);
    }

    private sealed class ScenarioFake : ITinhHuongRepository
    {
        public int Reads { get; private set; }
        public int Grade { get; private set; }
        public string? Code { get; private set; }
        public TinhHuongItem? Detail { get; init; }
        public Task<IReadOnlyList<BoiCanhItem>> BoiCanhAsync() => Task.FromResult<IReadOnlyList<BoiCanhItem>>([
            new("BC_NHA_CUA","Nhà cửa"),new("BC_DO_DUNG","Đồ dùng"),new("BC_MANH_VUON","Mảnh vườn")]);
        public Task<IReadOnlyList<TinhHuongItem>> DocAsync(int lop,string? ma=null)
        {
            Reads++;Grade=lop;Code=ma;
            return Task.FromResult<IReadOnlyList<TinhHuongItem>>(Detail == null ? [] : [Detail]);
        }
    }
}
