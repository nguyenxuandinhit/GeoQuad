# Red-team — 4 lenses, 08/10/2026

Assumptions, Failure, Security và Scope/Complexity được review bởi agent chỉ đọc; Scope là lượt độc lập sau Failure do giới hạn concurrency. Không agent nào sửa code hoặc chạy test. Findings trùng được gộp, có evidence file:line, root đối chiếu source và sửa plan trong phạm vi người dùng yêu cầu hoàn thiện. Đây là chỉnh sửa kỹ thuật của plan, không thêm quyền triển khai/push hay xác nhận nội dung thay thành viên.

| # | Severity / lens | Evidence lúc draft | Disposition / sửa |
|---|---|---|---|
| 1 | High Failure; Medium Security/Assumptions | phase-04-us20-us21-lam-bai.md:20,40 | Accepted: persisted result immutable, replay payload khác409, concurrent tests |
| 2 | High Failure | phase-05-us22-lo-trinh.md:19,48 | Accepted: lockaccount trước DA_HOC MERGE, recheck+concurrent oneedge |
| 3 | High Failure; Medium Assumptions | phase-06-us23-us24-tien-do-goi-y.md:45 | Accepted: thay impossible9/10+2/5 thành7/10=70%+2/5=40% |
| 4 | Medium Failure/Security | research/execution-contract.md:20,22; phase4 draft | Accepted: bootstrap guard trước marker; preserve marker; ownedapp+sentinelHTTP handshake |
| 5 | Medium Failure | phase-01-start.md:52; us03_review.py:104 | Accepted: script không help, dùng docstring+containerargv thật |
| 6 | Medium Security | Infrastructure/Auth/CurrentUser.cs:26; phase5:13 | Accepted: personalized reads/writes lấy HOC_LOP DB, stale2sessions test |
| 7 | Medium Assumptions/Scope | phase4:19,21; Program.cs không Session | Accepted: protected guestcookie; CookieTempData nhỏ, no explanationpayload; 2tab/lostcookie tests |
| 8 | Medium Scope | phase5:6,32; execution-contract:9 | Accepted: fixture taskF prerequisite DB4/5/6, independent unit5 |
| 9 | Low Scope | phase6:6; plan.md:53 | Accepted: phase6 dependency chỉ4; linkroadmap nghiệm thu phase7 |

Không findings bị reject,9unique sau dedup; không có Critical. Citation line ở draft reviewer đọc; các dòng mới đã thay đổi sau sửa. Không coi unresolved source A/proofB/UI/crossreview là lỗi đã được sửa trong code.

## Decision delta / Whole-plan sweep

Rà toàn plan: no9/10fake fixture, no session dependency, no helpcommand sai, no staleDockerblocker, no allUS19stub, no allphase6 phụ thuộc5. Chốt gate lớp DB/replay/lock/marker/app handshake thống nhất giữa phase và execution contract. Fixture guard và locking phải được kiểm chứng bằng test thật khi thực thi; kế hoạch không tự chứng minh tính đúng của implementation.
