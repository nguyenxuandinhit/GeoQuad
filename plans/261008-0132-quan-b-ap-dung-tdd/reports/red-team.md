# Red Team Review — Quân/B

Ngày 2026-10-08. Bốn lenses: Security/Fact Checker, Failure/Flow Tracer, Assumption/Scope Auditor, Complexity/Contract Verifier. Review plan và chứng cứ source; không chạy code chưa triển khai, không sửa app.

## Findings và adjudication

Bốn lenses hoàn tất qua ba reviewer và một lượt review riêng của reviewer đã rảnh (runtime tối đa4 agent gồm root). 9 observations → deduplicate thành **7 findings**; **7 Accept / 0 Reject**, 1 High và 6 Medium. Các sửa dưới đây sửa tính nhất quán của draft trong phạm vi lập plan đã được yêu cầu, không thay phạm vi sản phẩm hoặc bắt đầu implementation. Không có quyết định sản phẩm cần hỏi thêm; Quân có thể review artifacts trước cook.

| # | Finding / evidence | Severity | Disposition và sửa |
|---|---|---|---|
| 1 | Manifest lấy trước stop writer; source có `TaiKhoanRepository.cs:47` tạo node, phase07 draft Architecture lấy snapshot quá sớm | High | Accept: quiesce/drain web và writer khác, lấy manifest khi Neo4j online rồi stop/dump; thêm boundary-write case |
| 2 | Archive có hash/account/history (`TaiKhoanRepository.cs:47`) nhưng draft phase07 chỉ gitignore | Medium | Accept: owner-only dir/file/ACL, kiểm tra Docker ownership, archive/manifest/log; failure không báo success |
| 3 | Research fixture cần opt-in và tên compose khác phase01; `research/tdd-architecture-B.md:46–47` | Medium | Accept: authority `Integration/compose.neo4j.yml`; export GQ_B_INTEGRATION=1; align research |
| 4 | Public grade/review policy trong plan draft áp cả QuanTri, trái SRS UC15 `GeoQuad_SRS.md:484–495` và `BaiTapController.cs:13` role gate | Medium | Accept: ApDung public gate riêng; admin mọi lớp/trạng thái theo filter; thêm claim8/manage12+NHAP/ẩn test |
| 5 | Hai phase01/07 còn template TaskA/Step1 ở cuối draft | Medium | Accept: xóa toàn bộ scaffold thừa; validation/sweep chống template |
| 6 | FakeGraphDb không cùng DTO interface seam; `IGraphDb.cs:12` trả IRecord | Medium | Accept: phase01 chỉ CurrentUser/fixture; interfaces typed colocate repo, fake Doubles riêng phase2/3/4/6; service mapping và rollback decorator inventory rõ |
| 7 | `bash -n backup.sh restore.sh` chỉ parse file đầu; Bash manual `/usr/share/man/man1/bash.1:219` ARGUMENTS | Medium | Accept: hai lệnh syntax gate độc lập |

Line references của findings tới draft mô tả trạng thái trước sửa; paths source tương đối root repo. Kết quả là review **thiết kế**, không có lời hứa source mới đã qua DB/HTTP/backup tests.

## Whole-Plan Consistency Sweep

Đã đọc lại plan.md + cả7phase +2research sau thay đổi; kiểm tra fixture/file names, opt-in, typed seam, grade-public/admin, backup manifest/permissions, source ownership, test gates và task state. Search không còn các tên fixture/Fakes/GraphDbGia hoặc template cũ trong execution contracts. Bảy decision deltas được truyền tới mọi bản trùng có liên quan. Không còn contradiction được reviewer xác minh chưa xử lý. Chi tiết structural checks ở [validation-checks.json](validation-checks.json), validation ở [validation.md](validation.md).
