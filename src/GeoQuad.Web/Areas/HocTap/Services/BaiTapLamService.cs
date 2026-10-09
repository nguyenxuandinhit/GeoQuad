using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class BaiTapLamService(ILamBaiRepository repo, ICurrentUser user,
    LanLamTokenService tokens, TimeProvider clock)
{
    public async Task<(BaiTapChiTiet? Bai, LanLamToken? Token, bool CoLoiGiaiMau)> MoAsync(string ma, string? guestNonce)
    {
        var bai = await repo.ChiTietAsync(ma, user.LopHienThi);
        if (bai is null || bai.Loai is not ("TRAC_NGHIEM" or "DAP_AN_SO" or "CHUNG_MINH")) return (null, null, false);
        var proof = bai.MaChungMinh is not null && await repo.LaChungMinhCongKhaiAsync(bai.MaChungMinh, user.LopHienThi);
        return (bai, tokens.Tao(ma, user.TaiKhoanId, guestNonce), proof && !string.IsNullOrWhiteSpace(bai.LoiGiaiMau));
    }

    public async Task<KetQuaNopBai> NopAsync(string ma, BaiTapNopInput input, string? guestNonce)
    {
        var owner = user.TaiKhoanId is { } id ? "tk:" + id : "guest:" + (guestNonce ?? "");
        if (string.IsNullOrEmpty(guestNonce) && !user.DaDangNhap ||
            !tokens.ThuGiaiMa(input.Token, ma, owner, out var claim))
            return Loi("Phiên làm bài đã hết hạn. Em mở lại đề nhé.");

        var (bai, _, _) = await MoBaiKhongTaoTokenAsync(ma);
        if (bai is null) return Loi("Bài tập không còn hiển thị hoặc không phù hợp với lớp của em.");
        if (bai.Loai == "CHUNG_MINH") return Loi("Bài chứng minh không chấm tự động.");

        var answer = (input.DapAn ?? "").Trim();
        var grade = bai.Loai == "TRAC_NGHIEM"
            ? ChamBai.TracNghiem(answer, bai.DapAnDung ?? "")
            : ChamBai.So(answer, input.DonVi, bai.DapAnSo ?? decimal.MinValue, bai.SaiSo, bai.DonVi);
        if (!grade.HopLe) return Loi(grade.Loi ?? "Đáp án không hợp lệ.");
        var key = ChamBai.DapAnDungDangChuoi(bai.Loai, bai.DapAnDung, bai.DapAnSo);
        var elapsed = (int)Math.Clamp((clock.GetUtcNow() - claim.MoLuc).TotalSeconds, 0, 86400);
        var payloadHash = LanLamTokenService.Hash(answer, input.DonVi);
        if (user.TaiKhoanId is null)
            return new(true, true, false, grade.Dung, claim.MaLan, answer, key, bai.DonVi, "",
                await TaoKetQuaAsync(bai, answer, key, grade.Dung, laKhach: true));

        var saved = await repo.GhiNhanAsync(user.TaiKhoanId, user.Lop, bai, claim.MaLan,
            payloadHash, answer, (input.DonVi ?? "").Trim(), grade.Dung, elapsed);
        if (saved is null) return Loi("Tài khoản hoặc lớp học đã thay đổi. Em đăng nhập lại nhé.");
        if (saved.XungDot) return new(false, true, true, saved.Dung, saved.MaLan, saved.DapAn,
            saved.DapAnDung, saved.DonVi, "Lượt này đã được nộp với đáp án khác.");
        return new(true, true, false, saved.Dung, saved.MaLan, saved.DapAn, saved.DapAnDung,
            saved.DonVi, "", await TaoKetQuaAsync(bai, saved.DapAn, saved.DapAnDung, saved.Dung, laKhach: false));
    }

    public async Task<KetQuaLamBaiViewModel?> KetQuaTaiKhoanAsync(string maLan)
    {
        if (user.TaiKhoanId is null) return null;
        var saved = await repo.KetQuaAsync(user.TaiKhoanId, maLan);
        if (saved is null) return null;
        var bai = await repo.ChiTietAsync(saved.MaBaiTap, user.LopHienThi);
        return bai is null ? null : await TaoKetQuaAsync(bai, saved.DapAn, saved.DapAnDung, saved.Dung, laKhach: false);
    }

    public async Task<KetQuaLamBaiViewModel?> KetQuaKhachAsync(string ma, string token, string? nonce,
        string answer, bool dung)
    {
        if (string.IsNullOrEmpty(nonce) || !tokens.ThuGiaiMa(token, ma, "guest:" + nonce, out _)) return null;
        var bai = await repo.ChiTietAsync(ma, user.LopHienThi);
        if (bai is null || bai.Loai == "CHUNG_MINH") return null;
        var key = ChamBai.DapAnDungDangChuoi(bai.Loai, bai.DapAnDung, bai.DapAnSo);
        return await TaoKetQuaAsync(bai, answer, key, dung, laKhach: true);
    }

    public Task<string?> BaiTiepTheoKhachAsync(string ma) => repo.BaiTiepTheoKhachAsync(ma, user.LopHienThi);

    public bool TryReadToken(string token, string? guestNonce, out LanLamClaim claim)
    {
        var owner = user.TaiKhoanId is { } id ? "tk:" + id : "guest:" + (guestNonce ?? "");
        return tokens.TryRead(token, owner, out claim);
    }

    public string MaLan(string token)
    {
        var owner = user.TaiKhoanId is { } id ? "tk:" + id : "guest:";
        return tokens.TryRead(token, owner, out var claim) ? claim.MaLan : "";
    }

    public async Task<KetQuaLamBaiViewModel?> LoiGiaiMauAsync(string ma)
    {
        var (bai, _, _) = await MoBaiKhongTaoTokenAsync(ma);
        if (bai is null || bai.Loai != "CHUNG_MINH" || string.IsNullOrWhiteSpace(bai.LoiGiaiMau) || bai.MaChungMinh is null) return null;
        if (!await repo.LaChungMinhCongKhaiAsync(bai.MaChungMinh, user.LopHienThi)) return null;
        return new(bai.Ma, bai.De, "", "", "", false, bai.LoiGiaiMau, bai.MaChungMinh);
    }

    private Task<(BaiTapChiTiet? Bai, LanLamToken? Token, bool CoLoiGiaiMau)> MoBaiKhongTaoTokenAsync(string ma) =>
        ma.Length == 0 ? Task.FromResult<(BaiTapChiTiet?, LanLamToken?, bool)>((null, null, false)) : MoAsync(ma, null);

    // Kiến thức liên quan chỉ nạp ở trang kết quả, không bao giờ ở GET làm bài (tránh gợi ý đáp án).
    private async Task<KetQuaLamBaiViewModel> TaoKetQuaAsync(BaiTapChiTiet bai, string answer, string key,
        bool correct, bool laKhach)
    {
        var lienQuan = await repo.KienThucLienQuanAsync(bai.Ma, user.LopHienThi);
        return new(bai.Ma, bai.De, answer, key, bai.DonVi, correct, bai.GiaiThich, bai.MaChungMinh,
            LaKhach: laKhach, LopHocSinh: user.Lop)
        {
            KienThuc = lienQuan.KienThuc,
            KhaiNiem = lienQuan.KhaiNiem
        };
    }

    private static KetQuaNopBai Loi(string text) => new(false, false, false, false, "", "", "", "", text);
}
