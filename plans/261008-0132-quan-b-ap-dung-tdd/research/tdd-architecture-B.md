# Quân B: kiến trúc và TDD có thể thực thi

Ngày nghiên cứu: 2026-10-08. Phạm vi: chỉ đọc source/SRS/README và tài liệu chính thức; chưa chạy DB, chưa sửa ứng dụng. Đây là đầu vào cho plan, không phải kết quả kiểm thử.

## Khuyến nghị và bằng chứng source

**Xếp hạng 1:** giữ MVC/.NET 8 + Neo4j.Driver 5.28.4; Repository nhận `IGraphDb`, Service nhận interface Repository và `ICurrentUser`; logic thuần tách lớp. Dùng fake viết tay cho unit/service/controller test, Neo4j thật riêng cho Cypher/transaction. Đây là phương án ít thay đổi nhất trong thời gian hai ngày và đúng phạm vi B.

| Phương án | Hiệu năng | Độ phức tạp/bảo trì | Chi phí/rủi ro tiếp nhận | Phù hợp |
|---|---|---|---|---|
| 1. Interface trong Area B + fake viết tay + DB fixture riêng | Không đổi đường chạy production | Thấp; fake theo DTO, không giả lập Cypher | Không thêm package; thấp | Cao, không sửa csproj/chung |
| 2. Moq/Testcontainers/WebApplicationFactory | Tương đương production; test startup tốn thời gian | Thuận tiện khi đã có hạ tầng | Phải thêm package/sửa csproj của phần 0; không được B tự làm | Chỉ dùng sau phối hợp chủ phần 0 |
| 3. Fake `IRecord`/query runner cho mọi repository test | Không chứng minh Cypher/rollback thật | Cao, dễ test khớp implementation | Không package nhưng độ tin cậy thấp | Không chọn cho kiểm chứng DB |

- B sở hữu `Areas/ApDung/**`, `Areas/QuanTri/**`, hai seed `20-dinhly-B.cypher`, `21-chungminh-B.cypher`, `tests/GeoQuad.Tests/B_ApDung/**`, `docs/cypher/B-ap-dung.md`. Script backup/restore thuộc TH nhưng CSV giao Quân; ghi rõ quyền sở hữu mở rộng cho đúng các script đó.
- `ApDungModule.cs`, `QuanTriModule.cs` đã được Program gọi; đăng ký DI tại hai module này, không cần sửa Program.
- `IGraphDb.WriteTransactionAsync(Func<IAsyncQueryRunner,Task>)` có sẵn; GraphDb triển khai bằng managed `ExecuteWriteAsync`.
- `ICurrentUser.LopHienThi` đã xử lý chế độ nâng cao; mọi truy vấn B lấy giá trị này thay vì đọc cookie lại.
- `B_ApDung/KhungTests.cs` hiện chỉ `Assert.True(true)`; chưa có test nghiệp vụ B.
- Test csproj hiện có xUnit 2.5.3, Microsoft.NET.Test.Sdk, coverlet và web project reference; không có mocking/TestHost/Testcontainers. File `.cs` dưới B tự compile, không cần sửa csproj. Controller gọi trực tiếp chỉ chứng minh kết quả action, **không** chứng minh authorization/anti-forgery middleware.
- `dotnet --list-runtimes` hiện có runtime 9.0.19 và 10.0.11, thiếu Microsoft.NETCore.App/Microsoft.AspNetCore.App 8.x. Cổng nghiệm thu cần runtime .NET 8 trên host hoặc container SDK/runtime 8; roll-forward chỉ là kiểm tra phụ, không tuyên bố đúng runtime đích.

## Thiết kế kiểm thử và cổng RED/GREEN

Mỗi nhóm test mới chạy trước implementation phải đỏ vì thiếu/hành vi sai, không dùng `Assert.True(true)` hoặc skip để ghi RED. Sau code chạy GREEN, rồi refactor cùng test. Gắn namespace `GeoQuad.Tests.B_ApDung`, trait `Category=Integration` cho DB tests.

```sh
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~B_ApDung&Category!=Integration' --nologo
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~B_ApDung&Category=Integration' --nologo
dotnet build GeoQuad.sln --nologo
```

| Story | Unit/service/controller (RED trước code) | DB/HTTP nghiệm thu thực |
|---|---|---|
| Seed US03/04 | Kiểm tra catalog mã mong đợi từ README/SRS; không so sánh file với chính nó | Seed B hai lần: 14 ĐiềuKien, 20 dấu hiệu, 6 định lý nền, 4 chứng minh, 3–6 bước liên tục; đúng label gốc; mỗi dấu hiệu 1 nền/1 đích/≥1 điều kiện; căn cứ tồn tại; số nút/quan hệ ổn định; đảo thứ tự A/B vẫn nối đúng placeholders |
| US13 | Sắp xếp thiếu ít trước rồi mã; ✓/✗; cùng nền/đích; suy ra trực tiếp; không kết quả; fallback | HBH+góc vuông→HCN đầu DH_HCN_2 thiếu0; HBH→Thoi có4 dấu hiệu; TG+cạnh đối→HBH thiếu0; HV→HCN direct; fallback đúng 2–3 bước; mỗi bước lọc lớp/trạng thái; dấu hiệu chưa rà soát bị bỏ; grade3 direct URL không lộ nội dung8 |
| US14 | DTO bước đúng thứ tự; căn cứ tính chất dẫn hình, dấu hiệu/nền dẫn hộp nội dung; thiếu chứng minh không có nút | CM01 bước đúng và có căn cứ; null OPTIONAL MATCH không sinh căn cứ rỗng; ID không tồn tại→404; direct grade thấp bị chặn |
| US17 | Ba bối cảnh vẫn có thẻ khi chưa có tình huống phù hợp; lớp/nâng cao; thực hành theo cờ | TH01 liên kết DH_HBH_2 và DH_HCN_3; grade3 list/detail không lộ TH01; placeholder AP_DUNG chưa có lớp/trạng thái không được coi là kiến thức hợp lệ |
| US18 | Ít nhất8 case: HCN chuẩn2/.9/2.19; HV nhánh thoi; HV nhánh HCN; Thoi; HBH; không xác định; âm/0; tam giác suy biến; biên sai số. Thêm parse dấu phẩy/chấm, null/rỗng, NaN/Infinity, sai số không hợp lệ, scale cùng đơn vị | POST TH01 lấy dấu hiệu thật; giả mạo ID/thucHanh=false/lớp không đủ bị từ chối; chống CSRF qua request thật; không ghi DA_HOC/DA_LAM |
| US25 | Validator ba loại; 4 phương án không rỗng/đúng1 A-D; numeric0 hợp lệ nếu đề cho phép, saiSo>0; chứng minh có lời giải; ≥1 concept; lớp1–12/khó1–3; mã/de bắt buộc; route/body mã khác bị chặn | Unknown lớp/concept/dinhly/congthuc→rollback; duplicate code→lỗi tiếng Việt bằng constraint; edit không tồn tại→404; chuyển loại xóa field cũ; empty SU_DUNG có chủ ý→0 cạnh; ẩn/hiện giữ DA_LAM/id/properties; failure sau bước SET/delete relation phải rollback toàn bộ; HTTP khách/học sinh không vào admin, CSRF invalid không ghi |

## Seam test không chạm common/csproj

- B tạo `IGoiYRepository`, `IChungMinhRepository`, `ITinhHuongRepository`, `IBaiTapQuanTriRepository` colocate trong file repository của mỗi phase/Area; dùng DTO typed, không đẩy `IRecord` lên Service/View. Fake viết tay nằm `B_ApDung/Doubles/` (đồng bộ inventory phase2/3/4/6); fake `ICurrentUser` có lớp3/8/nângcao12.
- Unit controller dùng `DefaultHttpContext` + `ControllerContext`, `ModelState`; kiểm tra attribute bằng reflection chỉ là kiểm tra khai báo. Test chống CSRF/phân quyền cần mở app thật và gửi HTTP hoặc kiểm thử browser, không coi gọi action trực tiếp là đủ.
- Fixture integration `IAsyncLifetime` dùng driver thật + `new GraphDb(driver, Options.Create(...))`; file Compose **độc lập** đặt `tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml` (đã đồng bộ authority phase01), không extends file root có container_name/cổng/volume dev. Dùng container/volume B riêng, localhost Bolt17687/Browser17474, verify image runtime ≥5.9; collection integration không chạy song song.
- Không fallback URI về 7687/dev. Fixture đòi opt-in `GQ_B_INTEGRATION=1`, URI17687 và sentinel DB riêng; thiếu cấu hình khi chạy suite Integration phải lỗi rõ ràng. Default unit command lọc Integration. Nếu dùng custom `FactAttribute` skip integration khi opt-in thiếu thì báo số skipped, không ghi full green.
- Chuẩn bị seed fixture từ schema/chung/A/B; repository-root lookup theo `GeoQuad.sln`; chạy `.cypher` bằng cypher-shell `-f` hoặc bộ tách câu có hiểu quote/comment, **không** `Split(';')`. Seed suite và mutation suite phân tách; mutation tạo mã `BTEST_<run-id>` và chỉ cleanup phạm vi fixture riêng. Không `MATCH(n) DETACH DELETE n` trên DB dev.

## US25: giao dịch quản trị

Trong **một** `WriteTransactionAsync`: xác minh lớp tồn tại, tập mã concept và SU_DUNG khớp đầy đủ tập đã dedup, kiểm tra edit target tồn tại, rồi cập nhật properties và dựng lại quan hệ. `MATCH` mất dòng không phải exception; phải kiểm tra số record hoặc throw validation exception để rollback. Không MERGE một reference do người dùng nhập; không silently bỏ unknown IDs.

- Code `ma` làm định danh ổn định: edit giữ mã route, loại `ma` khỏi map cập nhật; từ chối body khác route. Đây là quyết định an toàn của plan, SRS chưa quy định đổi mã.
- `SET bt += $props` với **whitelist** fields; gửi null cho fields loại cũ cần xóa. Không bỏ null khỏi map vì thuộc tính cũ sẽ còn. Không cho client sửa `hienThi/nguon/trangThai` qua form property map.
- Numeric `dapAnSo=0` khác thiếu giá trị. Dùng nullable parsed number cho missing; đáp án số không bắt buộc dương trong SRS, chỉ saiSo dương. Driver hỗ trợ FLOAT là double, không gửi .NET decimal trực tiếp.
- Xóa đúng cạnh outgoing THUOC_LOP/LIEN_QUAN_DEN/SU_DUNG; không xóa `BaiTap` hoặc DA_LAM. UI ẩn/hiện là POST anti-forgery.
- Constraint `BaiTap.ma` chống race duplicate; precheck chỉ trợ giúp UX. Managed callback có thể retry: không thực hiện HTTP/file/event side effects trong callback; consume cursor trong transaction trước return.
- Test rollback mạnh: decorator `IGraphDb` chuyển callback thật vào GraphDb, query runner delegation chèn exception sau statement mutation/relation; sau catch đọc DB mới, node/quan hệ/DA_LAM giống trước. Không cần sửa production common.
- SRS yêu cầu giữ lịch sử làm bài khi ẩn, không yêu cầu snapshot đề/đáp án cũ khi sửa; không tự thêm versioning/schema.

## US18: định nghĩa dung sai cần khóa trong plan

Đề xuất mặc định rõ ràng: `rel(a,b)=abs(a-b)/max(a,b)` với a,b>0; chấp nhận `rel <= tolerancePct/100`; so 4 cạnh bằng nhau bằng `(max-min)/max` để tránh so sánh gần bằng không có tính bắc cầu. Không làm tròn số trước phân loại; chỉ làm tròn phần hiển thị.

- Nhánh4cạnh bằng→Thoi DH_THOI_1; thêm đường chéo bằng→HV DH_HV_5. Nếu chưa thỏa4cạnh: cạnh đối bằng→HBH DH_HBH_2; thêm chéo bằng→HCN DH_HCN_3; thêm AB≈BC→HV DH_HV_1.
- Square nhánh HCN khả thi với số đo nhiễu: AB=1, BC=1.009, CD=1.009, DA=1.018, AC=BD=1.43, tol1%; từng cặp đối và cặp kề đạt nhưng range4cạnh vượt1%. Chốt thứ tự/công thức rồi dùng vector này chống nhánh không bao giờ tới.
- Test biên đơn giản 99 và100 tại1% (max-denominator); 98.999 vượt1%; test hàm so sánh thuần không lẫn hình học.
- Các tam giác cần kiểm tra: (AB,BC,AC), (AC,CD,DA), (AB,BD,DA), (BC,CD,BD). Bất đẳng thức nghiêm; scale theo max input trước cộng nếu double có thể overflow.
- Parser form cục bộ nhận string, trim, chấp nhận **một** dấu chấm hoặc phẩy thập phân, không thousands separator/chuỗi hai loại dấu. Parse invariant sau chuẩn hóa, `double.IsFinite`, báo tiếng Việt cạnh field; `2,00`, `0,90`, `2,19` đúng trên culture host bất kỳ. Không đổi middleware culture/common model binder.
- SRS/README không định nghĩa denominator, range tolerance hoặc tập cặp khi báo lệch lớn nhất. Ghi quyết định mặc định: >0 và<100%; tập AB/CD, BC/DA, AC/BD và các cặp cạnh phục vụ kiểm tra 4cạnh; hiển thị chính cặp có rel lớn nhất, tie theo tên ổn định. Đây là giả định product cần công bố, không phải fact SRS.
- Bốn tam giác hợp lệ chưa chứng minh sáu khoảng cách tạo cùng tứ giác phẳng lồi. Implement đúng mức BR10/README hiện có; UI nêu giả định lồi và kết luận theo số đo/dung sai. Không tự thêm Cayley–Menger/solver làm đổi AC bộ số đo làm tròn.

## US13: QPP cần hoàn thiện query README

Neo4j5 QPP ≥5.9 và biến `d` ngoài pattern là danh sách, nên `all(x IN d ...)` và `[x IN d|...]` hợp lệ. README fallback chỉ lọc reviewed, thiếu eligibility từng bước. B bổ sung với predicate EXISTS THUOC_LOP <=$lop cho mọi `x`, trả mã/đích/nền/điều kiện thiếu từng bước, sort số bước + chuỗi mã ổn định, tối đa3 chuỗi; không suy diễn fallback là chứng minh đã hoàn tất khi còn điều kiện thiếu. Query bound `{2,3}` đúng AC tối đa3 bước. Test Neo4j thật syntax/ordering, không fake string.

## US27: Community backup/restore offline và diễn tập cô lập

Khuyến nghị dùng dump/load, không online backup Enterprise. Cần kiểm tra version server thật bằng `CALL dbms.components()` và `neo4j-admin database dump/load --help`; tag `neo4j:5-community` trôi, không mặc định đúng latest/giống ảnh đã chạy. Ghi image ID/digest của container nguồn, dùng đúng ảnh cho dump/load/restore drill.

Luồng script (tham số thư mục timestamp, validation exitcode; shell trap/PowerShell finally chỉ khởi động lại dịch vụ vốn đang chạy):

```sh
docker compose stop web neo4j
docker compose run --rm --no-deps --pull never --entrypoint neo4j-admin neo4j database dump neo4j --to-path=/backups/<timestamp>
docker compose start neo4j
# Đợi health OK trước start web, khôi phục trạng thái chạy ban đầu.
docker compose start web
```

Thư mục host `backups/<timestamp>` phải tạo và writable trước dừng DB. Dump chỉ chạy khi nguồn đã offline; không `compose exec` vào DB đang chạy. Dùng --help5 để xác minh filename/flags, checksum và kiểm tra archive tồn tại. Khi script lỗi phải trả nonzero, không tuyên bố backup thành công.

**Drill mặc định:** tạo volume mới `geoquad-b-restore-<run-id>`; không load vào volume dev. `docker run --rm --entrypoint neo4j-admin --volume <new-volume>:/data --volume <absolute-backup-dir>:/backups:ro <captured-image-id> database load neo4j --from-path=/backups`. Destination rỗng nên không cần overwrite. Sau đó start container ảnh đó với volume mới, cổng loopback riêng, auth đúng DB hệ thống đích; đối chiếu số nút/quan hệ/constraints/catalog và một DA_LAM mẫu với manifest nguồn. Nếu restore neo4j user DB vào installation mới thì system users không tự đi cùng; plan giải thích recreate auth hoặc dump system riêng nếu cần restore cả DBMS.

`restore.sh/.ps1` mặc định tạo đích riêng; ghi đè volume hiện tại phải tham số explicit và xác nhận tên đích, có backup trước. Không dùng `down -v` với root Compose. Cleanup chỉ tên container/volume đã tạo cho drill.

## Nguồn, độ tin cậy và giới hạn

Nguồn chính thức ưu tiên, đối chiếu ba hệ tài liệu Neo4j/Microsoft/Docker. Neo4j5 manual là authority cho QPP; current docs dùng cho hành vi driver/SET và nguyên lý offline, nhưng không lấy tính năng2025/2026 áp vào Neo4j5. Các commands cụ thể phải xác minh `--help` của image nguồn trong implementation. Không dùng tutorial/community post để quyết định API.

1. [Neo4j5 quantified path patterns](https://neo4j.com/docs/cypher-manual/5/patterns/variable-length-paths/) — ≥5.9, group variables và bounded traversal.
2. [Neo4j .NET managed transactions](https://neo4j.com/docs/dotnet-manual/current/transactions/) — ACID/rollback, retry callback và consume kết quả trong callback; chỉ dùng API ExecuteRead/ExecuteWrite vốn đã có trong source5.28.4.
3. [Cypher SET map/null](https://neo4j.com/docs/cypher-manual/current/clauses/set/) — += giữ field không trong map, null xóa field. Không dùng dynamic label/key mới.
4. [Neo4j .NET type mappings](https://neo4j.com/docs/dotnet-manual/current/data-types/) — INTEGER long, FLOAT double; không dùng UUID/vector mới6.x.
5. [Docker Compose run](https://docs.docker.com/reference/cli/docker/compose/run/) — one-off thừa hưởng volume, --no-deps/--rm/--entrypoint.
6. [Neo4j offline Docker dump/load](https://neo4j.com/docs/operations-manual/current/docker/dump-load/) — DB dừng, container admin riêng, load có thể vào data volume mới. Không copy current2026 image vào kế hoạch5.
7. [Offline backup](https://neo4j.com/docs/operations-manual/current/backup-restore/offline-backup/) và [restore dump](https://neo4j.com/docs/operations-manual/current/backup-restore/restore-dump/) — contract offline, archive/path flags; archived `/5` URLs không truy cập được trong công cụ nghiên cứu.
8. [Microsoft Double.TryParse](https://learn.microsoft.com/en-us/dotnet/api/system.double.tryparse?view=netframework-4.7.2) — tài liệu remarks .NET Core3+ giải thích overflow có thể ra Infinity và parse NaN/Infinity; do đó TryParse alone không đủ cho validation.

Chưa kiểm chứng performance/NFR100 users, chưa chạy dump/load, chưa xác nhận image version/digest, chưa chạy .NET8 đúng runtime, chưa kiểm thử workflow web. Không bổ sung lib vì csproj ngoài quyền B. Adoption risk thấp với stack sẵn có; QPP có rủi ro version image và backup có rủi ro permission/version nên đặt gate runtime/help rõ.

Quyết định còn cần công bố trong plan: denominator/tập lệch/tolerance của đo đạc; mã bài bất biến khi sửa; fallback diễn đạt chuỗi còn thiếu điều kiện; giữ DA_LAM khi ẩn không đồng nghĩa lưu snapshot khi sửa; restore user DB hay toàn DBMS. Có thể dùng mặc định đề xuất ở trên để không chặn lập plan.
