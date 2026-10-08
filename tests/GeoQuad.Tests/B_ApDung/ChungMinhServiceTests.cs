using GeoQuad.Tests.B_ApDung.Doubles;
using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Tests.B_ApDung;

public sealed class ChungMinhServiceTests
{
    [Theory]
    [InlineData("1,2",false)]
    [InlineData("1,2,3",true)]
    [InlineData("3,1,2",true)]
    [InlineData("1,1,3",false)]
    [InlineData("1,2,4",false)]
    [InlineData("1,2,3,4,5,6,7",false)]
    public async Task IncompleteOrAmbiguousStepSequenceCannotBePresented(string orders,bool ready)
    {
        var repo=new ProofFake(orders.Split(',').Select(int.Parse).Select(i => new BuocItem(i,"Bước",[new("DL_NEN_1","Căn cứ",null)])).ToArray());
        var model=await new ChungMinhService(repo,new CurrentUserGia()).XemAsync("CM-01");
        Assert.Equal(ready,model != null);
        if(model != null) Assert.Equal(Enumerable.Range(1,model.Buoc.Count),model.Buoc.Select(b => b.ThuTu));
    }

    [Fact]
    public async Task EmptyGroundAndBlankStepCannotBePresented()
    {
        var steps=new[] {new BuocItem(1,"Bước",[]),new BuocItem(2,"Bước",[new("DL_NEN_1","Căn cứ",null)]),new BuocItem(3,"Bước",[new("DL_NEN_1","Căn cứ",null)])};
        Assert.Null(await new ChungMinhService(new ProofFake(steps),new CurrentUserGia()).XemAsync("CM-01"));
        steps[0]=new(1," ",[new("DL_NEN_1","Căn cứ",null)]);
        Assert.Null(await new ChungMinhService(new ProofFake(steps),new CurrentUserGia()).XemAsync("CM-01"));
    }

    [Fact]
    public async Task AdvancedGradeIsForwardedAndInvalidCodeDoesNotReadRepository()
    {
        var repo=new ProofFake(Enumerable.Range(1,3).Select(i => new BuocItem(i,"Bước",[new("DL_NEN_1","Căn cứ",null)])).ToArray());
        var service=new ChungMinhService(repo,new CurrentUserGia(3,true));
        Assert.Null(await service.XemAsync(new string('A',65))); Assert.Equal(0,repo.Reads);
        var model=await service.XemAsync("CM-01");
        Assert.True(model!.NangCao); Assert.Equal(12,repo.Grade);
    }

    private sealed class ProofFake(IReadOnlyList<BuocItem> steps) : IChungMinhRepository
    {
        public int Reads { get; private set; }
        public int Grade { get; private set; }
        public Task<ChungMinhViewModel?> XemAsync(string ma,int lop)
        {
            Reads++;Grade=lop;
            return Task.FromResult<ChungMinhViewModel?>(new(ma,"Mẫu","Giả thiết","Kết luận","Dấu hiệu",steps,8));
        }
    }
}
