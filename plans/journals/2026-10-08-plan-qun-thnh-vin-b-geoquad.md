---
title: Plan Quân thành viên B GeoQuad
date: 2026-10-08
summary: "Lập deep/TDD plan đầy đủ phần B theo Jira/SRS, 7 phase và phản biện contract."
---

# Plan Quân thành viên B GeoQuad

## Công việc

Đối chiếu docs/GeoQuad_Jira_2ngay.csv (multiline), docs/GeoQuad_SRS.md, README và source main@f3f6172. Tạo plan tại plans/261008-0132-quan-b-ap-dung-tdd/plan.md và7phase; không sửa code ứng dụng hoặc tài liệu nguồn user.

## Quyết định

Giữ đủ5story24điểm chức năng và nhiệm vụ chung của Quân/B (34điểm đứng tên). Ước lượng26h tập trung, chưa tính chờ A/C. TDD logic thuần + Neo4j/HTTP thật trong môi trường cô lập; không sửa csproj/chung. Khóa quy ước sai số tương đối, mã bài bất biến và restore userDB vào volume mới.

## Review và kiểm chứng

Review4lenses, xử lý7findings về snapshot backup, archive permissions, fixture opt-in/tênfile, public/admin grade policy, typed fake seams, Bash syntax gate và template thừa. CLI validate đạt;195 structural/link checks đạt. Đây là validation plan, chưa chạy chức năng mới, .NET8/DB/backup hoặc performance.

## Tiếp theo

Bắt đầu phase1 khi Quân yêu cầu triển khai; kiểm tra .NET8/Docker, seed B và peer-review C trước DA_RA_SOAT; mỗi phase sau scout lại source. Artifacts là authority, task index cục bộ chỉ projection. Không publish bên ngoài.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.
