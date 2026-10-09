# Traceability và scout — 08/10/2026

## Phạm vi C đầy đủ

Jira CSV gán C34rows: 7stories +27subtasks; 21 subtasks là Cypher/giải thích, repo/service/xUnit, MVC/UI cho US08,19–24;6subtasks là reviewUS03,seedUS04,kickoffUS05,cross-testA,fixC,demo. C còn điều phối US01/05/06 theo README:56. Không nhận sở hữu toàn file chung.

| Story / việc | Nguồn file:line | Phase / evidence phải có |
|---|---|---|
| US01/05/06 điều phối | README.md:56,239,264,282 | 1,7: owner evidence compose/homequery,consent/hash/duplicate,lock5/5min/admin |
| KickoffUS05 | docs/GeoQuad_Jira_2ngay.csv:68; README.md:217 | 1: trình bày TaiKhoan/HOC_LOP/constraint |
| US03 C | README.md:309; CSV:44 | 1: counts/IDs/nguồn/rà toán học/repeat |
| US04 C | README.md:324; CSV:57 | 2: BT011..030 15cấp2/5cấp3,7/≥7/3,≥5thoi,dev |
| US08 | README.md:620; CSV:88 | 2: Cypher+repo/unit+HTTP/UI/cookie/deletehistory |
| US19 | README.md:635; CSV:217 | 3:5filters/history/preview,360px |
| US20 | README.md:652; SRS:517; CSV:228 | 4: ≥6ChamBai,units/tolerance,DA_LAM/guest,POST |
| US21 | README.md:670; CSV:240 | 4: proofbutton/no-grade/BT027→CM01 reviewed |
| US22 | README.md:679; CSV:251 | 5: targets/topo/DA_HOC/realgrade/reload |
| US23 | README.md:699; CSV:264 | 6: allhistorycharts/recent5/min3/40%/threshold |
| US24 | README.md:725,661; CSV:275 | 6:≤5/notevercorrect/ASCnormal/DESCfallback/KeTiep |
| US26 | README.md:744; CSV:309,312 | 7: C→A6stories,2Browserqueries;B→C;Cfix |
| US27 | README.md:752,770; CSV:324 | 7:docs/demo.md+rehearsal;Asetup/Bbackupcoordination |

SRS ở đây là docs/GeoQuad_SRS.md; CSV là docs/GeoQuad_Jira_2ngay.csv. Các reference phải scout lại nếu tài liệu được sửa. Source-of-truth: README ownership/API, SRS businessrules, CSV subtasks; mâu thuẫn cần ghi owner, không im lặng bỏ rule.

## Code scout bằng chứng

- Infrastructure/Neo4j/IGraphDb.cs:12–18 cung cấp transaction API; GraphDb.cs:31 dùng managed transaction retry, attemptid phải ổn định ngoài callback.
- Infrastructure/Auth/ICurrentUser.cs:24 LopHienThi=12 khi nângcao; README:680/726 yêu cầu lớp thực cho lộ trình/gợiý.
- Areas/HocTap/HocTapModule.cs:13 DI C; BaiTapController.cs:27/30 Lam/KeTiep stub; LoTrinhController.cs:11 và TienDoController.cs:11/15 stub.
- Areas/HocTap/Repositories/BaiTapRepository.cs:11 đã có public gates; Index/service/models/view là WIP hiện tại, không viết lại vì overview cũ.
- Areas/ApDung/Services/ChungMinhService.cs:12–17 reviewgate B; Areas/KienThuc/Controllers/ThuVienController.cs:35–48 có POST “Em đã hiểu”, C dùng cùng semantic idempotence qua repo riêng.
- tests/GeoQuad.Tests/GeoQuad.Tests.csproj:12 xUnit2.5.3, chưa MVC.Testing; dùng fake repos + HTTP thật hiện có.
- tests/GeoQuad.Tests/C_HocTap/us19_http_smoke.py:22 guard chỉ prefix, cần label/ports/mount/marker trước reset; fixtureB hardcodes B-only, không dùng chung DB.

## Quyết định và gate

1. Giữ đủ scope C,7phase, không thêm UI library/chartframework; service/repository typed theo kiến trúc hiện có.
2. Người dùng xác nhận AI tự soạn không bộSGK; nguồn nội bộ chính xác + rationale toán học + khung chương trình, không claim textbookreview. Disclaimer không thay nguồn.
3. B20review hoàn thành thật nhưng nguồn/lớp A còn finding; B21proof riêng chưa reviewed. CM04 cần rà chiều suy luận; A/B sửa đúng ownership.
4. Histories giữ khi adminẩn; candidate/detail/submit không mở bàiẩn. Publicpreview khác personalized realgrade.
5. Tokenattemptid + transaction lock giải quyết retries; mỗi lượt mới vẫn quan hệ mới. Không MERGE history chỉ theo cặp account/bài.
6. Deep mỗi phase scout lại; plan mở đủ các bước để thực thi nhưng giữ gate chưa được nghiệm thu. Estimatedremaining33h.
