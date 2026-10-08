# Kiểm thử GeoQuad

## Quân/B — kết quả ngày 08/10/2026

Người thực hiện: agent theo yêu cầu `ak-cook --auto` của Quân; chưa có chữ ký review độc lập của A/C. Branch `feature/B-ap-dung`. Chạy .NET SDK8.0.425/runtime8.0.31, Neo4j5.26.31 Community trên Docker Desktop/macOS; chỉ project test `geoquad-b-tests`, không mutation database dev.

### Chạy lại

```sh
docker compose -p geoquad-b-tests -f tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml up -d --wait
dotnet build GeoQuad.sln --nologo --disable-build-servers -m:1
dotnet test GeoQuad.sln --no-build --filter 'Category!=Integration' --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --no-build --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration' --settings tests/GeoQuad.Tests/B_ApDung/Integration/neo4j.runsettings --nologo
bash -n scripts/backup.sh scripts/restore.sh scripts/backup-common.sh
```

Máy này dùng host `/private/tmp/geoquad-dotnet8/dotnet` vì SDK8 được cài riêng trong phiên. Máy khác có .NET8 dùng `dotnet`. Runsettings chứa credential test disposable và opt-in rõ; fixture kiểm tra URI, project/volume marker, loopback ports và seed mount read-only trước reset. B integration dùng xUnit collection serial. Không chạy hai integration process cùng lúc.

### Kết quả B

| Gate | Kết quả và evidence |
|---|---|
| Build net8 | Đạt, 0 warning/0 error |
| Unit regression toàn solution | 415 đạt, 0 fail/skip sau tích hợp origin/main@95a8519 và bổ sung11case service B |
| Integration/HTTP/load/drill full | 14 đạt sau tích hợp95a8519, 0 fail/skip, 3m03s; link căn cứ B→trang hình A HTTP200; 4/4case quản trị/restore đã chạy lại sau sửa fixture lịch sử theo SRS |
| Seed/schema thật | Đúng 14 điều kiện/20 dấu hiệu/6 định lý nền/4 chứng minh; 13 unique constraints/fulltext, core3/12/7/6/7; A→B→A→B và B→A giữ snapshot |
| US-13 | Bốn AC README; direct/inheritance; QPP2–3 bước tối đa3; condition missingCount/mã ổn định; draft/null/class gate và HTTP400 |
| US-14 | 4 mẫu, bước liên tục/căn cứ; thiếu/draft/căn cứ A chưa hoàn thiện không có link và URL404; advanced/class qua HTTP |
| US-17 | 3 bối cảnh/9 tình huống trong graph test đã rà soát; TH-01 đúng dấu hiệu/lời giải; list/URL cùng gate; một AP_DUNG thiếu lớp loại cả tình huống |
| US-18 | 23 unit cases: comma/dot, positive finite, đơn vị, tolerance biên đối xứng, tam giác, hai trace vuông, extreme finite; HTTPCSRF/class/practice, input còn nguyên; không tăng node/edge |
| US-25 | Ba loại, số0, clearing field loại cũ, mã bất biến; mọi route admin từ chối guest/học sinh; role/login/token thật; admin lớp8 quản lý lớp12/NHAP/ẩn; preview encode/không write; transaction rollback thật, ID sai/thiếu, duplicate concurrent một winner; DA_LAM giữ7 quan hệ lượt làm riêng và các property luc/dung/đáp án/thời gian |
| Backup shell | Dump offline và restore mới cùng imageID; manifest/constraints/catalog/digest khớp; account password ứng dụng và DA_LAM đúng; source không đổi; fresh system/auth |
| Flow lỗi shell | 4 cases: dumpfail phục hồi trạng thái; unhealthy không startweb; nguồn stopped được trả stopped; checksum hỏng từ chối; không success giả |
| Cypher B | Mọi repository query chạy thật, params/output/counters trong [docs B](cypher/B-ap-dung.md); mutation audit rollback |
| Tải gợi ý | HTTP thật, 10 warmup +1000 requests/100 workers; 0 lỗi; p95=427.4867ms≤2s. Giá trị/hardware/raw timings mới nhất tại [performance-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/performance-B.json) |
| Review mã | Root đọc guard/class/query/mutation/scripts và kết quả test; code-simplifier làm gọn3file trong B, không đổi publiccontract/query/transaction; không có independent A review |

Số integration cuối và output tổng hợp được ghi tại [execution-B.md](../plans/261008-0132-quan-b-ap-dung-tdd/reports/execution-B.md). Test integration thường bao nhiều assertion/case trong một Fact; không lấy test count làm thay thế độ phủ AC. Báo cáo [backup-drill-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/backup-drill-B.json) là drill mới nhất, chỉ giữ counts/run path/flags, không password/hash.

Seed phát hành B vẫn `NHAP`: graph test mô phỏng `DA_RA_SOAT` bằng marker `reviewTestOnly`. Kết quả public trong test không phải chứng cứ C đã duyệt toán học/SGK. Không đưa marker này vào seed dev.

### Quân/B kiểm thử C — chưa nghiệm thu

| Story C | Expected | Actual / lý do |
|---|---|---|
| US-08 | Hồ sơ đổi lớp/biệt danh/password/xóa tài khoản | Chưa kiểm thử: `HocTap/Controllers/HoSoController.cs` vẫn DangXayDung |
| US-19 | List/filter bài reviewed/visible theo lớp | Chưa kiểm thử: BaiTap.Index controller khung |
| US-20 | Làm/chấm ba loại | Chưa kiểm thử: BaiTap.Lam controller khung |
| US-21 | Ghi DA_LAM đúng lượt/đúng sai | Chưa kiểm thử: chưa có reader/writer C hoàn thiện; history B fixture không thay acceptance C |
| US-22 | Lộ trình prerequisite | Chưa kiểm thử: LoTrinh.Index controller khung |
| US-23 | Tiến độ và thống kê lịch sử | Chưa kiểm thử: TienDo.Index controller khung |
| US-24 | Gợi ý bài kế tiếp | Chưa kiểm thử: BaiTap.KeTiep/TienDo.GoiY controller khung |
| ≥2 Cypher C | Query C thật + giải thích/output | Chưa chạy: `docs/cypher/C-hoc-tap.md` chưa có query; không lấy query B để đóng gate này |
| Hide B → C | Bài ẩn không có trong list/Lam/gợi ý, lịch sử vẫn được tính | Chưa kiểm thử C. B đã chứng minh hide không xóa node/DA_LAM; reader C phải lọc hiển thị/review/class riêng |

### A kiểm thử B và các gate còn lại

- [ ] A ghi reviewer/ngày/browser và chạy AC US-13/14/17/18/25, lỗi B có regression trước sửa.
- [ ] C ký phiếu [content-review-B.md](../plans/261008-0132-quan-b-ap-dung-tdd/reports/content-review-B.md) trước publish seed.
- [ ] Browser360px/desktop, keyboard/focus, KaTeX, network4G≤3s/payload≤2MB: chưa chạy vì CUA báo không có browser khả dụng; scoped CSS16px/target48px và SVGtitle mới chỉ được đọc mã.
- [ ] PowerShell7.2/Windows drill, ACL owner-only và flow finally: chưa chạy vì máy không có pwsh/Windows. Scripts .ps1 đã rà soát native ExitCode, path/reparse guards, recovery order; chưa claim hỗ trợ đã nghiệm thu.
- [ ] A cập nhật README và kiểm tra setup máy sạch≤15phút; C hoàn thiện demo/recovery; chưa đóng US-26/27.

Không sửa file/sharedcontract thuộc A/C để né dependency. URL B tới trang hình/Bản đồ/ngân hàng bài tập giữ đúng README; các đích còn khung phải được owner hoàn thiện. Archive/log backup nằm trong `backups/` gitignored, owner-only; không chia sẻ raw archive như báo cáo test.

### CSV và nhánh bàn giao

Theo yêu cầu “CSV trước”, thêm Status/Comment cho 27 dòng B trong `docs/GeoQuad_Jira_2ngay.csv`: 5 Done (ID53/57/68/72/100), 21 In Progress, 1 To Do (ID104). Giữ nguyên 16 cột cũ/119 bản ghi, 4 header Labels và mọi giá trị nguồn; A/C không nhận trạng thái/comment mới. Báo cáo [jira-csv-update-B.json](../plans/261008-0132-quan-b-ap-dung-tdd/reports/jira-csv-update-B.json) có mapping từng dòng. Đây là update CSV repository; Jira online chưa cập nhật.

Nhánh duy nhất `feature/B-ap-dung` được fast-forward theo `origin/main@95a8519` trước kiểm thử; không tạo nhánh mới hoặc gộp vào main. Bản SRS Markdown/ảnh được đóng gói nguyên trạng để các link tài liệu có đủ nguồn khi review. Mọi gate chưa có evidence giữ mở, PR để draft do còn các gate review/browser/Windows.
