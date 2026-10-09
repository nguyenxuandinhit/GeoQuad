# Cypher phần C — Học tập cá nhân

Tài liệu này lưu các truy vấn phần C để chạy trên Neo4j 5, cùng ý nghĩa ký hiệu và kết quả dự kiến. Tham số người dùng luôn truyền riêng qua driver, không ghép chuỗi. Mọi bài tập phải được nối tới ít nhất một `KhaiNiem` bằng `LIEN_QUAN_DEN`.

Đối chiếu SRS: FR-50/51/60 là Must; FR-04/52/61/62 là Should. BR-05 yêu cầu bài số nằm trong sai số (mặc định 0,01) **và đơn vị khớp đề**; BR-06 xét tối đa 5 lượt gần nhất, tối thiểu 3, ngưỡng dưới 60%; BR-08 yêu cầu gắn ít nhất một khái niệm; BR-09 cấm lưu tiến độ cho khách. Phụ lục D.10 xác nhận BT-014 (đáp án 96, lớp 8) và BT-027 (lời giải mẫu CM-01); các chi tiết này đã phản ánh trong seed/truy vấn.

SRS nói ngân hàng khoảng 30 bài được soạn trong Sprint 2, còn README/Jira đặt seed BT-011..030 vào ngày 07/10 (Sprint 1). Nội dung C hiện theo phạm vi README/Jira; lịch giữa các tài liệu chưa thống nhất.

SRS FR-04 mô tả đổi biệt danh/lớp/mật khẩu; màn hình SCR-17 còn liệt kê chức năng xóa tài khoản. README cũng có truy vấn xóa, nên phần C giữ chức năng này và yêu cầu xác nhận mật khẩu trước khi xóa.

## Rà soát US-03

README phân công C chạy seed của A (`10-kienthuc-A.cypher`) và B (`20-dinhly-B.cypher`), đối chiếu SRS Phụ lục B/README Phụ lục D rồi ghi kết quả ở đây. Ngày 08/10/2026, C đã chạy trên hai container Neo4j 5.26 Community tạm, không gắn volume/cổng host, từ checkout `feature/C-hoc-tap@a1d9837`. Mỗi DB chạy `00-schema` → `01-khung` → A→B hoặc B→A, sau đó lặp lại hai seed cùng thứ tự. Sau khi đồng bộ `origin/main` vào nhánh C tại `ba99d9e`, C chạy lại đối chiếu từng mã và các truy vấn AC trên DB test A→B. Sau review và sửa seed B, C còn chạy `docker compose up -d neo4j` và `./scripts/seed.ps1` trên DB Compose của dự án. Kết quả từng giai đoạn được ghi rõ bên dưới.

| Kiểm tra | A→B lần 1 | A→B lần 2 | B→A lần 1 | B→A lần 2 |
|---|---:|---:|---:|---:|
| Tổng nút | 92 | 92 | 92 | 92 |
| Tổng quan hệ | 193 | 193 | 193 | 193 |

Đếm theo nhãn trên DB test: `DieuKien=14`, `DauHieu=20`, `TinhChat=12`, `CongThuc=12`, định lý nền `DinhLy` không nhãn phụ `=6`. Truy vấn chẩn đoán quan hệ của dấu hiệu và truy vấn thiếu `nguon`/`trangThai` bên dưới đều trả **0 dòng**. Trước review C: 14 điều kiện, 20 dấu hiệu, 6 định lý nền là `NHAP`; 12 tính chất và 12 công thức của A đã là `DA_RA_SOAT`.

`python tests/GeoQuad.Tests/C_HocTap/us03_review.py <container-test>` (đặt `GEOQUAD_US03_PASSWORD` trong môi trường) đối chiếu **từng mã** giữa README D.3–D.7, SRS B.3–B.7 và graph: 14/20/12/12/6 mục đều khớp mã, nội dung hoặc tên, lớp, hình và các quan hệ dấu hiệu; chạy trên cả hai thứ tự đều `PASS`. Biểu thức công thức ở phụ lục dùng ký hiệu in, seed dùng LaTeX, nên bài kiểm tra tự động chỉ so tên/lớp/hình của công thức; cần đọc biểu thức và biến số thủ công trước khi phê duyệt nội dung.

**Quyết định nguồn của nhóm:** Thành viên C xác nhận ngày 08/10/2026: nội dung GeoQuad do nhóm tự biên soạn, không chọn bộ SGK riêng; website mang tính giáo dục, không thay thế chương trình giáo dục chính thức. Theo phương án tạm tại SRS OI-02, C dùng chương trình khung GDPT 2018 để đối chiếu chủ đề/lớp, rồi kiểm tra độc lập tính đúng toán học của từng mục với định nghĩa và lập luận trong Phụ lục B. Chương trình khung không nêu nguyên văn tất cả phát biểu; `nguon` trong seed B ghi rõ nội dung tự biên soạn và phạm vi đối chiếu, không mạo nhận là trích dẫn SGK. A đã đặt 24 tính chất/công thức `DA_RA_SOAT`; C cũng kiểm tra độc lập các mục này, nhưng nguồn A vẫn ghi chung theo lớp nên cần A bổ sung tham chiếu chủ đề/trang nếu nhóm muốn cùng mức truy vết như B.

**Kiểm tra lại lỗi A đã báo:** `origin/feature/A-tra-cuu@3d9ac19` ghi rõ xung đột giữa README mục 2 điểm 6 (coi nội dung seed đã `DA_RA_SOAT`) và SRS mục 4/NFR-09 (ban đầu `NHAP`, chỉ duyệt sau nguồn và người thứ hai rà soát). Đối chiếu hai văn bản xác nhận xung đột này có thật; việc B giữ `NHAP` trước review là tuân theo SRS, không phải lỗi mã B. Sau review C ở đây, C sửa **file seed B** rồi chạy lại DB test; không đổi trạng thái thủ công trên Neo4j Browser vì lần seed tiếp theo sẽ ghi đè. README nên được nhóm sửa để mô tả rõ quy trình `NHAP` → review → `DA_RA_SOAT`.

**Nguồn chương trình đối chiếu:** [Chương trình GDPT môn Toán 2018 của Bộ GDĐT, bản PDF do Trường THCS An Đồng lưu](https://admintruong.haiphong.shieldix.app/data/haiphong/thcsandong/2023_6/1/3-cttoan_16202319.pdf), trang in 59–60 (lớp 7: góc so le trong, tổng góc tam giác, tam giác bằng nhau), 65–67 (lớp 8: Pythagore, tứ giác đặc biệt và ví dụ dấu hiệu, đường trung bình), 82–83 (lớp 10: vectơ). Tài liệu xác nhận **chủ đề/lớp và một số ví dụ**, còn phần phát biểu chi tiết là sản phẩm tự biên soạn do C rà soát toán học. Với các khái niệm lớp thấp và công thức A, C đối chiếu thêm các trang in 30–31, 35–37, 41–42 của cùng tài liệu; lớp 8 cho công thức đường chéo dùng Pythagore. Không coi tài liệu này là SGK hay bảo đảm website thay thế chương trình chính thức.

**Biên bản nội dung của C (08/10/2026):** Các mã dưới đây đã được đọc từng phát biểu trong README D.3–D.7, so với seed và SRS B.3–B.7. Với dấu hiệu, C kiểm tra cả hình nền, điều kiện và kết luận; các suy luận dùng tứ giác đơn không suy biến theo cách hiểu hình học phổ thông. `nguon` trong seed trỏ SRS tương ứng và trang chương trình để truy vết. Đây là rà soát nội dung tự biên soạn, không phải chứng nhận của Bộ GDĐT.

| Mã đã rà soát | Đối chiếu toán học | Kết quả |
|---|---|---|
| `DK_MOT_CAP_CANH_DOI_SONG`, `DK_CANH_DOI_SONG`, `DK_CANH_DOI_BANG`, `DK_MOT_CAP_SONG_SONG_BANG`, `DK_GOC_DOI_BANG`, `DK_CHEO_CAT_TRUNG_DIEM`, `DK_BA_GOC_VUONG` | Bảy điều kiện đầu mô tả đúng dữ kiện dùng ở các dấu hiệu hình thang, bình hành, chữ nhật. “Một cặp” được hiểu là ít nhất một cặp, phù hợp định nghĩa hình thang bao hàm trong `01-khung`. | Đạt |
| `DK_MOT_GOC_VUONG`, `DK_CHEO_BANG_NHAU`, `DK_BON_CANH_BANG`, `DK_HAI_CANH_KE_BANG`, `DK_CHEO_VUONG_GOC`, `DK_CHEO_PHAN_GIAC`, `DK_HAI_GOC_KE_DAY_BANG` | Bảy điều kiện còn lại khớp dữ kiện dùng cho chữ nhật, thoi, vuông, thang cân; điều kiện đường phân giác luôn kèm hình nền, không áp độc lập cho mọi tứ giác. | Đạt |
| `DH_HT_1`, `DH_HTC_1`, `DH_HTC_2` | Hai mã đầu theo định nghĩa; trong hình thang, hai đường chéo bằng nhau suy ra hai góc kề đáy bằng nhau. | Đạt |
| `DH_HBH_1`, `DH_HBH_2`, `DH_HBH_3`, `DH_HBH_4`, `DH_HBH_5` | Song song hai cặp là định nghĩa; cặp cạnh đối bằng nhau, một cặp đối vừa song song vừa bằng, cặp góc đối bằng, hoặc hai đường chéo phân đôi nhau đều suy ra hình bình hành bằng tam giác bằng nhau/quan hệ song song. | Đạt |
| `DH_HCN_1`, `DH_HCN_2`, `DH_HCN_3` | Ba góc vuông kéo theo góc thứ tư vuông; hình bình hành có một góc vuông hoặc hai đường chéo bằng nhau là hình chữ nhật. | Đạt |
| `DH_THOI_1`, `DH_THOI_2`, `DH_THOI_3`, `DH_THOI_4` | Bốn cạnh bằng nhau là định nghĩa; trong hình bình hành, hai cạnh kề bằng nhau, đường chéo vuông góc, hoặc một đường chéo phân giác một góc đều kéo theo bốn cạnh bằng nhau. | Đạt |
| `DH_HV_1`, `DH_HV_2`, `DH_HV_3`, `DH_HV_4`, `DH_HV_5` | Hình chữ nhật thêm cạnh kề bằng, đường chéo vuông góc hoặc phân giác góc đều là hình thoi; hình thoi thêm góc vuông hoặc đường chéo bằng nhau đều là hình chữ nhật. Giao của hai hình là hình vuông. | Đạt |
| `DL_NEN_1`, `DL_NEN_2`, `DL_NEN_5` | Tổng góc tam giác, ba trường hợp bằng nhau và góc so le trong của đường thẳng song song đúng toán học; chương trình lớp 7 ở trang 59–60 có các chủ đề này. | Đạt |
| `DL_NEN_3`, `DL_NEN_4` | Pythagore và đường trung bình tam giác đúng toán học; chương trình lớp 8 ở trang 65–67 có hai nội dung này. | Đạt |
| `DL_NEN_6` | Với thứ tự đỉnh ABCD, `vectơ AB = vectơ DC` tương đương một cặp cạnh đối song song và bằng nhau, đúng dấu hiệu hình bình hành; chương trình lớp 10 ở trang 82–83 có vectơ bằng nhau và ứng dụng hình học. | Đạt |
| `TC_*` (12 mã D.5), `CT_*` (12 mã D.6) của A | C đọc lại từng tính chất, biểu thức LaTeX, `bienSo`, đại lượng và lớp; công thức chu vi/diện tích, đường chéo theo Pythagore đúng phép tính và khớp D.5/D.6. Tuy nhiên chương trình khung chỉ nêu nhận biết hình bình hành/hình thoi ở lớp 4 (tr. 37), còn việc tính chu vi/diện tích các hình đặc biệt được nêu rõ ở lớp 6 (tr. 51); `CT_HBH_DT` và `CT_THOI_DT` gắn lớp 4 là ánh xạ đề xuất cần A/nhóm quyết định. | Đạt toán học; mở finding lớp/nguồn A |

> **Cập nhật 09/10/2026 — bàn giao cho B (phương án a):** theo README quy ước 2.1, nhánh C **không** giữ thay đổi trong file của B. Toàn bộ chỉnh sửa dưới đây (seed `20-dinhly-B.cypher`, `docs/cypher/B-ap-dung.md`, `content-review-B.md`, hai đoạn của B trong `docs/kiem-thu.md`) được gửi B dưới dạng patch `C-gui-B-ra-soat-US03.patch` để B tự áp dụng trên nhánh B. Cho tới khi B áp dụng, seed trên nhánh C vẫn để 40 mục B ở `NHAP`; các kết quả "sau review" bên dưới là kết quả chạy thử trên DB test với bản seed đã sửa.

Bản đề xuất cho `20-dinhly-B.cypher` đổi `NHAP` → `DA_RA_SOAT` cho đúng 14+20+6 mã vừa rà soát, đồng thời thay dòng “chờ C đối chiếu SGK” bằng nguồn tự biên soạn, mục SRS và trang chương trình phù hợp. **Không** đổi `21-chungminh-B.cypher`: bốn chứng minh mẫu và các bước thuộc US-04, phiếu B còn yêu cầu rà soát căn cứ riêng, nhất là CM-04.

**Output chạy lại sau merge main:** `us03_review.py` trả `PASS` với README/SRS/Neo4j lần lượt `14/14/14`, `20/20/20`, `12/12/12`, `12/12/12`, `6/6/6`. Truy vấn đếm tất cả trạng thái trả `DieuKien=14, DauHieu=20, TinhChat=12, CongThuc=12, DinhLy nền=6, NHAP=40, DA_RA_SOAT=24`; dấu hiệu sai cấu trúc quan hệ `0`, nút thiếu/rỗng `nguon` hoặc thiếu `trangThai` `0`. Hai số 0 chỉ chứng minh cấu trúc/trường dữ liệu, không chứng minh chất lượng nguồn.

**Output sau khi C duyệt và nạp lại seed B:** C chép `20-dinhly-B.cypher` đã sửa vào hai DB test, chạy hai lần trên mỗi DB. `us03_review.py` tiếp tục `PASS` ở cả hai. Truy vấn AC lọc `DA_RA_SOAT` trả `DieuKien=14`, `DauHieu=20`, `TinhChat=12`, `CongThuc=12`, định lý nền `=6`; trong 64 mục US-03, `NHAP=0`. Truy vấn dấu hiệu sai quan hệ `0`, thiếu/rỗng nguồn hoặc thiếu trạng thái `0`, nguồn còn chuỗi “chờ C” `0`. DB B→A giữ `92` nút, `193` quan hệ sau hai lần nạp B; DB A→B đã có thêm seed US-04/dev nên có `154` nút, `372` quan hệ, không dùng số này để kiểm tra riêng US-03.

**Output trên Neo4j Compose của dự án:** `docker compose up -d neo4j` thành công; lần gọi seed ngay khi container vừa khởi động gặp `Connection refused`, sau khi healthcheck báo `healthy` thì `./scripts/seed.ps1` chạy đủ `00,01,10,11,12,20,21,30` và kết thúc `Seed xong.`. Chạy seed lần hai vẫn `153` nút, `365` quan hệ. `us03_review.py geoquad-neo4j` trả `PASS` cho từng phụ lục D.3–D.7; truy vấn AC trên DB Compose trả `14/20/12/12/6` mục `DA_RA_SOAT`, `NHAP=0` trong 64 mục US-03, `saiQuanHe=0`, `thieuNguonHoacTrangThai=0`, `conChoC=0`. Việc thiếu nguồn/lớp A nêu trong bảng review là finding nghiệp vụ chưa được giải quyết chỉ bằng các truy vấn đếm.

### Rà soát 10 mục A bổ sung ngoài Phụ lục B (09/10/2026)

Sau khi merge `origin/main@612fc01`, seed `10-kienthuc-A.cypher` có thêm 8 tính chất và 2 công thức không có trong README D.5/D.6 và SRS B.5/B.6. `us03_review.py` đã sửa để phụ lục phải có đủ và khớp, còn mục thêm thì liệt kê để rà tay (vẫn kiểm `nguon`, `trangThai`). Script: PASS. C đọc từng phát biểu và biểu thức trong seed:

| Mã | Nội dung (tóm tắt) | Đúng toán học | Ghi chú cho A |
|---|---|---|---|
| `TC_TG_2` | Tổng bốn góc ngoài (mỗi đỉnh một góc) bằng 360° | Đạt, với tứ giác **lồi** | Nên thêm chữ "lồi": với tứ giác lõm, "góc ngoài" tại đỉnh lõm không xác định theo nghĩa thông thường |
| `TC_HT_2` | Đường trung bình hình thang song song hai đáy, bằng nửa tổng hai đáy | Đạt | `nguon` ghi "CT GDPT 2018 – Toán 8", nhưng C chỉ thấy *đường trung bình của tam giác* ở trang in 65–67 của chương trình đã dẫn. Nếu không có trang cụ thể, nên ghi là nội dung mở rộng |
| `TC_HTC_3` | Hình thang cân có trục đối xứng qua trung điểm hai đáy | Đạt | Tính đối xứng của hình được học từ lớp 6 (hình học trực quan); gắn lớp 8 vẫn chấp nhận được |
| `TC_HBH_4` | Giao điểm hai đường chéo là tâm đối xứng | Đạt | — |
| `TC_HCN_3` | Hai trục đối xứng qua trung điểm hai cặp cạnh đối, tâm là giao điểm hai đường chéo | Đạt | "Có hai trục" đúng (hình vuông có thêm hai trục, đã ghi ở `TC_HV_2`) |
| `TC_THOI_3` | Hai trục đối xứng là hai đường chéo, tâm là giao điểm | Đạt | — |
| `TC_HV_2` | Bốn trục đối xứng, tâm là giao điểm hai đường chéo | Đạt | — |
| `TC_HV_3` | Hai đường chéo bằng nhau, vuông góc, là phân giác các góc | Đạt | — |
| `CT_HT_CV` | `P = a + b + c + d`, lớp 5 | Đạt | — |
| `CT_THOI_CHEO` | `d_2 = 2\sqrt{a^2 - \frac{d_1^2}{4}}`, lớp 8 | Đạt (Pythagore trên nửa hai đường chéo; BT-023: 13, 10 → 24) | Điều kiện `d₁ < 2a` đã được `MayTinhHinhHoc` chặn. **Lỗi hiển thị:** `MayTinhHinhHoc.cs:147` viết `@"d_2 = 2\\sqrt{…\\frac…}"` (verbatim, nên là hai dấu `\`). Dòng "Công thức:" trong "Các bước làm" của máy tính hiện thành chữ vụn "2 sqrt … frac …"; kiểm tra trên trình duyệt với a = 13, d₁ = 10. Sửa: dùng một dấu `\` như các công thức khác trong file |

Kết luận: 10/10 mục đúng toán học. Ba việc gửi A: thêm "lồi" cho `TC_TG_2`, bổ sung trang nguồn hoặc ghi "mở rộng" cho `TC_HT_2`, sửa dấu `\` ở `MayTinhHinhHoc.cs:147`.

### Đếm các nhóm nội dung US-03

```cypher
MATCH (n:DauHieu {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soDauHieu;
MATCH (n:TinhChat {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soTinhChat;
MATCH (n:CongThuc {trangThai:'DA_RA_SOAT'}) RETURN count(n) AS soCongThuc;
MATCH (n:DinhLy {trangThai:'DA_RA_SOAT'})
WHERE NOT n:DauHieu AND NOT n:TinhChat
RETURN count(n) AS soDieuKienDinhLyNen;
MATCH (n:DieuKien) RETURN count(n) AS soDieuKien;
```

Kết quả **sau review C và nạp lại seed B** khi lọc `DA_RA_SOAT`: `DauHieu=20`, `TinhChat=12`, `CongThuc=12`, định lý nền `=6`; riêng `DieuKien` (truy vấn không lọc trạng thái) là `14`, và cả 14 đều đã `DA_RA_SOAT`. Sáu định lý nền là mục D.7; chúng không thay thế 14 nút `DieuKien`.

### Mỗi dấu hiệu phải đủ các quan hệ

```cypher
MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})
OPTIONAL MATCH (d)-[la:YEU_CAU_LA]->(:KhaiNiem)
WITH d, count(DISTINCT la) AS soNen
OPTIONAL MATCH (d)-[co:YEU_CAU_CO]->(:DieuKien)
WITH d, soNen, count(DISTINCT co) AS soDieuKien
OPTIONAL MATCH (d)-[k:KHANG_DINH]->(:KhaiNiem)
WITH d, soNen, soDieuKien, count(DISTINCT k) AS soKetLuan
WHERE soNen <> 1 OR soDieuKien < 1 OR soKetLuan <> 1
RETURN d.ma AS ma, soNen, soDieuKien, soKetLuan;
```

Kết quả sau review là 0 dòng trên đủ 20 dấu hiệu `DA_RA_SOAT`; kiểm tra độc lập trên **mọi** nút `DauHieu` cũng trả 0 dòng. Truy vấn kiểm tra từng nút dấu hiệu có đúng một hình nền, ít nhất một điều kiện và đúng một kết luận. Đối chiếu thêm đủ `nguon` và `trangThai`:

```cypher
MATCH (n)
WHERE (n:DauHieu OR n:TinhChat OR n:CongThuc OR n:DieuKien)
  AND (n.nguon IS NULL OR n.trangThai IS NULL)
RETURN labels(n) AS nhan, n.ma AS ma, n.nguon AS nguon, n.trangThai AS trangThai;
```

Kết quả chạy DB test là 0 dòng. Chất lượng nguồn và nội dung được ghi riêng trong biên bản review phía trên; truy vấn này chỉ xác nhận các trường có giá trị. Số nút/quan hệ trước/sau lần chạy lặp và thứ tự đảo đã ghi ở bảng trên.

## US-04 — Kiểm tra seed bài tập C

Seed C có 20 bài: 7 trắc nghiệm, 10 đáp án số, 3 bài chứng minh có lời giải; 6 bài nối Hình thoi và có BT-014/BT-027 theo D.10. Bốn nút `ChungMinh` mẫu của US-04 cha thuộc seed B (`21-chungminh-B.cypher`). Kiểm tra tĩnh ngày 08/10/2026 đã đối chiếu mã, cấp/lớp, loại, công thức tính, đáp án, liên kết khái niệm và dữ liệu làm bài mẫu; `python tests/GeoQuad.Tests/C_HocTap/seed_contract.py` trả `PASS`. BT-026 đã được sửa để chỉ có một phương án đúng.

Ngày 08/10/2026, trên container Neo4j 5.26 test cô lập của mục US-03, C đã chạy tiếp các seed `11-baitap-A`, `12-tinhhuong-A`, `21-chungminh-B`, `30-baitap-C` (sau schema, khung, seed A/B US-03). **Output thật:** toàn nhóm `30 BaiTap`, `9 TinhHuong`, `4 ChungMinh`; cấp `CAP_1=10`, `CAP_2=15`, `CAP_3=5`; riêng C `20/7/10/3`, `6` bài Hình thoi. Bài tập và tình huống thiếu `LIEN_QUAN_DEN`: `0`; bốn chứng minh đều nối tới ít nhất một khái niệm qua dấu hiệu, thứ tự bước là `[1,2,3,4]` cho CM-01..03 và `[1,2,3]` cho CM-04. BT-014 trả đáp án `96`, sai số `0.01`, đơn vị `cm²`, công thức `CT_THOI_DT`; BT-027 có `loiGiaiMau` và nối `CM-01` qua `DH_HCN_2`, nhưng CM-01 vẫn `NHAP` nên UI public lọc `DA_RA_SOAT` chưa hiển thị link. Trước/sau chạy lại seed C: `153` nút, `365` quan hệ, không đổi.

Để kiểm tra dev seed, C tạo một nút `TaiKhoan` giả `hocsinh8` chỉ trong DB test (id ngẫu nhiên, không phải tài khoản thật), rồi chạy `31-lam-bai-mau-C.cypher` hai lần: cả hai lần có `7` lượt `DEV-C-*`; 5 lượt Hình thoi gần nhất có `2` lượt đúng. Chưa dùng kết quả này để khẳng định đã chạy luồng giao diện của US-23.

Seed C cần chạy **sau `01-khung.cypher`** vì `Lop`, `CapHoc` và `KhaiNiem` được lấy bằng `MATCH`, không tự tạo nút nền. Tham chiếu tới `DinhLy`/`CongThuc` của A/B dùng `MERGE` theo mã, nên có thể chạy trước hoặc sau seed A/B. Bốn chứng minh B vẫn `NHAP` và cần review nguồn/lời giải trước khi công bố theo NFR-09.

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN count(b) AS soBai,
       count(CASE WHEN b.loai = 'TRAC_NGHIEM' THEN 1 END) AS tracNghiem,
       count(CASE WHEN b.loai = 'DAP_AN_SO' THEN 1 END) AS dapAnSo,
       count(CASE WHEN b.loai = 'CHUNG_MINH' THEN 1 END) AS chungMinh;
```

Kết quả dự kiến cho riêng C: `20`, `7`, `10`, `3`. Sau khi seed A chạy, toàn nhóm dự kiến có 30 bài và đủ ba loại.

Kiểm tra phân bố cấp và cấu trúc từng bài:

```cypher
MATCH (b:BaiTap)-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN cap.ma AS cap, count(DISTINCT b) AS soBai
ORDER BY cap;

MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
OPTIONAL MATCH (b)-[:SU_DUNG]->(x)
WITH b, count(DISTINCT l) AS soLop, count(DISTINCT k) AS soKhaiNiem,
     count(DISTINCT x) AS soKienThuc
WHERE soLop <> 1 OR soKhaiNiem < 1 OR soKienThuc < 1
   OR b.nguon IS NULL OR b.trangThai <> 'DA_RA_SOAT'
   OR b.hienThi <> true OR b.doKho < 1 OR b.doKho > 3
RETURN b.ma AS ma, soLop, soKhaiNiem, soKienThuc,
       b.nguon AS nguon, b.trangThai AS trangThai;
```

Kết quả dự kiến: `CAP_2 = 15`, `CAP_3 = 5`; truy vấn chẩn đoán trả 0 dòng. Ba bài chứng minh phải có `loiGiaiMau`; bảy bài trắc nghiệm phải có bốn phương án và một đáp án A–D; mười bài số phải có `dapAnSo`, `saiSo = 0.01`, `donVi` (BT-028 không có đơn vị).

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WITH b, collect(DISTINCT k.ma) AS khaiNiem
WHERE size(khaiNiem) = 0
RETURN b.ma AS ma;
```

Kết quả dự kiến là 0 dòng. Kiểm tra ít nhất năm bài gắn Hình thoi:

```cypher
MATCH (b:BaiTap)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:'HINH_THOI'})
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
RETURN count(DISTINCT b) AS soBaiHinhThoi;
```

Kết quả dự kiến ít nhất `5` (seed C hiện gắn sáu bài). Kiểm tra bài số/proof đặc tả:

```cypher
MATCH (b:BaiTap {ma:'BT-014'})-[:SU_DUNG]->(c:CongThuc {ma:'CT_THOI_DT'})
RETURN b.dapAnSo AS dapAn, b.saiSo AS saiSo, b.donVi AS donVi, c.ma AS congThuc;

MATCH (b:BaiTap {ma:'BT-027'})-[:SU_DUNG]->(d:DinhLy {ma:'DH_HCN_2'})
OPTIONAL MATCH (cm:ChungMinh {ma:'CM-01'})-[:CHUNG_MINH_CHO]->(d)
RETURN b.loai AS loai, b.loiGiaiMau IS NOT NULL AS coLoiGiai,
       d.ma AS dauHieu, cm.ma AS chungMinhMau;
```

Kết quả dự kiến: BT-014 có `96`, sai số `0.01`, `cm²`, công thức `CT_THOI_DT`; BT-027 là `CHUNG_MINH`, có lời giải và liên kết tới `CM-01` sau khi seed B được nạp.

Sau khi chạy seed C lần thứ nhất và thứ hai, dùng cùng truy vấn đếm dưới đây. Hai lần phải trả cùng số nút và số quan hệ xuất phát từ các bài C:

```cypher
MATCH (b:BaiTap)
WHERE b.ma >= 'BT-011' AND b.ma <= 'BT-030'
OPTIONAL MATCH (b)-[r]->()
RETURN count(DISTINCT b) AS soBai, count(DISTINCT r) AS soQuanHe;
```

Chạy dữ liệu dev **sau khi ứng dụng Development tạo tài khoản demo** `hocsinh8` (`tenDangNhap`; `id` của tài khoản là UUID). Nên kiểm tra trên tài khoản demo chưa có lượt làm khác, vì các lượt làm mới có thể đổi tập 5 lần gần nhất. Seed dev dùng `MERGE` theo `maLan`, chạy hai lần vẫn có 7 lượt mẫu, trong đó 5 lượt liên quan Hình thoi và 2 lượt đúng:

```cypher
MATCH (:TaiKhoan {tenDangNhap:'hocsinh8'})-[d:DA_LAM]->(:BaiTap)
WHERE d.maLan STARTS WITH 'DEV-C-'
RETURN count(d) AS soLanMau;

MATCH (:TaiKhoan {tenDangNhap:'hocsinh8'})-[d:DA_LAM]->
      (:BaiTap)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:'HINH_THOI'})
WHERE d.maLan STARTS WITH 'DEV-C-'
WITH d ORDER BY d.luc DESC
WITH collect(d)[0..5] AS gan
RETURN size(gan) AS soLanGanNhat,
       size([x IN gan WHERE x.dung]) AS soLanDung;
```

Kết quả dự kiến: `soLanMau = 7`, `soLanGanNhat = 5`, `soLanDung = 2`. Bài BT-017 có hai lượt mẫu (đúng với `84`, sai với `80`) để kiểm tra khái niệm mới có dưới ba lượt không bị xếp vào danh sách cần ôn.

## US-08 — Cập nhật hồ sơ

Đổi biệt danh/lớp; `$tk`, `$lop`, `$bietDanh` là tham số, không lấy bằng nối chuỗi. Ứng dụng phải gọi `IAuthHelper.DangNhapAsync` sau khi lưu để cấp lại cookie theo lớp mới. Ngày 08/10/2026 đã chạy `dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter FullyQualifiedName~HoSoServiceTests --nologo`: **15 đạt, 0 lỗi, 0 bỏ qua** (.NET 8 runtime). Đây là unit test service với repository/auth giả; chưa kiểm tra AC qua HTTP, cookie và Neo4j thật.

```cypher
MATCH (tk:TaiKhoan {id:$tk}), (l:Lop {so:$lop})
OPTIONAL MATCH (tk)-[r:HOC_LOP]->()
DELETE r
MERGE (tk)-[:HOC_LOP]->(l)
SET tk.bietDanh = $bietDanh
RETURN tk.id AS id, tk.bietDanh AS bietDanh, l.so AS lop;
```

Đổi mật khẩu: trước truy vấn cần xác thực mật khẩu cũ bằng `IAuthHelper.KiemTraMatKhau`, sau đó băm mật khẩu mới bằng `BamMatKhau`.

```cypher
MATCH (tk:TaiKhoan {id:$tk})
SET tk.matKhauBam = $bam
RETURN tk.id AS id;
```

README còn mở rộng US-08 thành xóa tài khoản; câu sau xóa các quan hệ học tập cùng nút tài khoản và phải chỉ chạy sau xác nhận mật khẩu.

```cypher
MATCH (tk:TaiKhoan {id:$tk})
DETACH DELETE tk;
```

Kiểm tra: sau đổi lớp 7 → 8, `HOC_LOP` chỉ còn trỏ tới lớp 8; mật khẩu sai không được đổi; sau xóa, tài khoản và các quan hệ `DA_LAM`, `DA_HOC`, `HOC_LOP` không còn.

## US-19 — Lọc ngân hàng bài tập

Kiểm tra bổ sung: URL `lop=abc` và `doKho=abc` báo “Bộ lọc không hợp lệ” qua HTTP, không mở rộng danh sách. Smoke sau sửa tiếp tục `PASS`; build/unit C vẫn 21/21. Kiểm kê CUA trả `apps=[]`, `browsers=[]`, nên phần kiểm tra trực quan 360px/keyboard còn mở, không được coi HTTP smoke là bằng chứng giao diện đã đạt.

Ngày 08/10/2026 đã triển khai repository/service/controller/view. `dotnet test ... --filter FullyQualifiedName~C_HocTap` đạt **21/21** (gồm 5 case dịch vụ bài tập). `python tests/GeoQuad.Tests/C_HocTap/us19_http_smoke.py <container-test> http://localhost:5080` chạy HTTP thật trên Neo4j test cô lập, dùng fixtures mã ngẫu nhiên và dọn trong `finally`: lọc cấp 2 + lớp 8 + đáp án số + mức 2 + Hình thoi có BT-014 và đúng một thẻ fixture; bài lớp 9, ẩn và NHAP không xuất hiện; guest không thấy lịch sử; tài khoản lớp 8 có hai lượt làm/một đúng hiện “Đã làm: 2 lần · Đã đúng”; bật nâng cao thấy bài lớp 9 và nhãn Nâng cao; cấp/lớp mâu thuẫn hiện empty state. Kết quả `PASS`. Controller còn chặn ModelState cho số lớp/độ khó không hợp lệ, regression unit đạt. Chưa kiểm tra trực quan 360px/keyboard nên chưa đóng phase UI.

Hằng `BaiTapRepository.CypherLoc` (nguyên văn):

```cypher
MATCH (bt:BaiTap {hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
WHERE l.so <= $lop
  AND ($lopLoc IS NULL OR l.so = $lopLoc)
  AND ($capLoc IS NULL OR cap.ma = $capLoc)
  AND ($loai IS NULL OR bt.loai = $loai)
  AND ($doKho IS NULL OR bt.doKho = $doKho)
  AND EXISTS { MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
  AND ($khaiNiem IS NULL OR EXISTS {
      MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:$khaiNiem})
  })
OPTIONAL MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(bt)
RETURN bt.ma AS ma, left(bt.de,120) AS de, bt.loai AS loai,
       bt.doKho AS doKho, l.so AS lop, cap.ma AS cap,
       count(DISTINCT d) AS soLan,
       max(CASE WHEN d.dung THEN 1 ELSE 0 END) > 0 AS daDung
ORDER BY lop, doKho, ma
```

Khác README: thêm `trangThai:'DA_RA_SOAT'` (NFR-09) và bắt buộc có `LIEN_QUAN_DEN` (BR-08). Truyền `null` cho bộ lọc không chọn; `$tk` là `null` với khách nên cột lịch sử bằng 0.

Output thật trên fixture (09/10/2026), `$lop = 8, $lopLoc = 8, $loai = 'DAP_AN_SO', $khaiNiem = 'HINH_THOI'`: `BT-020` (mức 1), `BT-013` (2), `BT-014` (2), `BT-023` (3). Đúng AC "lớp 8 + đáp án số + Hình thoi".

## US-20 — Lấy đề, chấm và lưu lần làm

> Cập nhật 09/10/2026: các truy vấn dưới đây chép nguyên văn hằng Cypher trong `Areas/HocTap/Repositories/LamBaiRepository.cs`. Chỗ nào khác README đều có lý do ghi kèm.

**Lấy đề (`CypherChiTiet`).** Một truy vấn cho cả GET làm bài và chấm ở server. Controller chỉ đưa ra view các trường công khai (`LamBaiViewModel` không có đáp án, giải thích, lời giải).

```cypher
MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND EXISTS { MATCH (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
OPTIONAL MATCH (b)-[:SU_DUNG]->(:DinhLy)<-[:CHUNG_MINH_CHO]-(cm:ChungMinh {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lc:Lop)
WHERE lc IS NULL OR lc.so <= $lop
RETURN b.ma AS ma,b.de AS de,b.loai AS loai,l.so AS lop,b.doKho AS doKho,
       coalesce(b.phuongAn,[]) AS phuongAn,b.dapAnDung AS dapAnDung,
       b.dapAnSo AS dapAnSo,b.saiSo AS saiSo,coalesce(b.donVi,'') AS donVi,
       coalesce(b.giaiThich,'') AS giaiThich,b.loiGiaiMau AS loiGiaiMau,
       cm.ma AS maChungMinh LIMIT 1
```

Khác README: thêm `trangThai:'DA_RA_SOAT'`, lọc lớp `$lop = LopHienThi` (BR-04) và bắt buộc có `LIEN_QUAN_DEN` (BR-08), để URL gõ tay không mở được bài nháp hay bài lớp cao hơn.

**Kiến thức liên quan (`CypherKienThucLienQuan`, mới).** Chỉ gọi khi dựng **trang kết quả**, không bao giờ ở GET làm bài, vì tên công thức gợi ý đáp án.

```cypher
MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})
OPTIONAL MATCH (b)-[:SU_DUNG]->(x {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lx:Lop)
WHERE x:DinhLy OR x:CongThuc
WITH b, x, min(lx.so) AS lopX
WITH b, [i IN collect(CASE WHEN x IS NOT NULL AND lopX <= $lop
          THEN x {.ma, ten: coalesce(x.ten, x.noiDung), bieuThuc: x.bieuThuc, lop: lopX} END)
         WHERE i IS NOT NULL] AS kienThuc
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lk:Lop)
WITH kienThuc, k, min(lk.so) AS lopK
RETURN kienThuc,
       [i IN collect(CASE WHEN k IS NOT NULL AND lopK <= $lop
          THEN k {.ma, .ten, lop: lopK} END) WHERE i IS NOT NULL] AS khaiNiem
```

Ký hiệu: `x {.ma, ten: …}` là *map projection*, lấy một số thuộc tính của nút thành map; `min(lx.so)` lấy lớp xuất hiện đầu tiên khi nút có nhiều `THUOC_LOP`; `[i IN collect(…) WHERE i IS NOT NULL]` là list comprehension bỏ phần tử rỗng do `OPTIONAL MATCH` sinh ra. Mục có `lop > Lop của học sinh` hiện nhãn "Nâng cao" (quy ước 2.7).

Output thật trên fixture C (09/10/2026):

| `$ma`, `$lop` | `kienThuc` | `khaiNiem` |
|---|---|---|
| BT-014, 8 | `CT_THOI_DT` "Diện tích hình thoi", `S = \frac{d_1 \cdot d_2}{2}`, lớp 4 | `HINH_THOI` lớp 4 |
| BT-018, 8 | `CT_HCN_CHEO` lớp 8; `DL_NEN_3` (Pythagore) lớp 8 | `HINH_CHU_NHAT` lớp 1 |
| BT-018, 7 | `[]` (cả hai lớp 8 > 7) | `HINH_CHU_NHAT` lớp 1 |
| BT-027, 8 | `DH_HCN_2` lớp 8 | `HINH_BINH_HANH` lớp 4, `HINH_CHU_NHAT` lớp 1 |

**Ghi lượt làm (`CypherLockAccount` + `CypherGhiLan`, một giao dịch).** Khác README (`CREATE`): mỗi lượt có mã `maLan` nằm trong token đã ký, nên nộp lại cùng token chỉ có một quan hệ, và kết quả lần đầu không bị ghi đè (nộp lại khác đáp án → HTTP 409).

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
SET tk._hocTapLock = $nonce
REMOVE tk._hocTapLock
RETURN l.so AS lop
```

`SET` rồi `REMOVE` ngay một thuộc tính giả để giữ khóa ghi trên nút tài khoản tới hết giao dịch, buộc hai lần nộp song song chạy lần lượt. Sau đó service so lớp trong DB với lớp trong cookie; khác thì không ghi.

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(la:Lop)
MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
WHERE la.so=$lopThuc AND lb.so <= la.so AND b.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
  AND EXISTS { MATCH (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
MERGE (tk)-[d:DA_LAM {maLan:$maLan}]->(b)
ON CREATE SET d.payloadHash=$hash, d.dapAnDaChon=$dapAn, d.donViDaChon=$donVi,
              d.dung=$dung,
              d.luc=datetime(), d.thoiGianGiay=$seconds
RETURN d.maLan AS maLan,b.ma AS maBaiTap,d.payloadHash AS payloadHash,d.dapAnDaChon AS dapAn,
       d.dung AS dung,d.thoiGianGiay AS seconds
```

`ON CREATE SET` chỉ chạy khi `MERGE` vừa tạo quan hệ; nộp lại cùng `maLan` trả bản ghi cũ để service so `payloadHash`. Khách không chạy truy vấn này (BR-09).

**Chấm** là logic thuần `ChamBai` (không phải Cypher): trắc nghiệm so chữ cái; bài số chấp nhận một dấu `,` hoặc `.`, đơn vị phải khớp, đúng khi `|nhập − dapAnSo| ≤ saiSo`. Unit test 20 ca.

**Kiểm chứng:** `us20_us21_http_smoke.py` PASS: GET không lộ đáp án hay gợi ý, 4 nút A–D, thiếu CSRF bị 400, khách được chấm nhưng không lưu và thấy lời mời đăng nhập, kết quả có kiến thức liên quan, nộp lại khác đáp án bị 409.

## US-21 — Xem lời giải mẫu

Nút "Xem lời giải mẫu" chỉ hiện khi bài `CHUNG_MINH` có `loiGiaiMau` **và** chứng minh mẫu liên quan đã công khai (`CypherProofPublic`):

```cypher
MATCH (cm:ChungMinh {ma:$ma, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND EXISTS { MATCH (cm)-[:CHUNG_MINH_CHO]->(:DinhLy {trangThai:'DA_RA_SOAT'}) }
RETURN count(cm)>0 AS ok
```

Hiện trạng (09/10/2026, sau khi B duyệt CM-01…CM-04 trên `main`): BT-027 hiện nút "Xem lời giải mẫu"; trang lời giải có link `/ApDung/ChungMinh/Xem/CM-01` trả HTTP 200. Lỗi đã sửa: link trước đó tạo bằng `asp-route-ma` nên ra `/ApDung/ChungMinh/Xem?ma=CM-01` (404), vì action của B nhận `id` theo hợp đồng URL README 3.2. `us20_us21_http_smoke.py` kiểm tra cả hai chiều: chứng minh đã duyệt thì có link mở được; tạm đặt `NHAP` thì nút ẩn và POST trả 404 (khôi phục trạng thái trong `finally`).

## US-22 — Lộ trình và đánh dấu đã học

Khác README: README dùng một truy vấn `max(length(p))` để xếp. Code lấy đồ thị con rồi sắp xếp tô pô (Kahn) trong `LoTrinhThuTu`, vì độ sâu lớn nhất không bảo đảm tiên quyết đứng trước khi các nhánh dài khác nhau. Chu trình hoặc quá 12 bước thì báo lỗi, không trả lộ trình nửa vời.

Danh sách mục tiêu (`CypherMucTieu`), `$lop` là lớp thật trong DB:

```cypher
MATCH (k:KhaiNiem {loai:'HINH',trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WITH k,min(l.so) AS lop
WHERE lop <= $lop
RETURN k.ma AS ma,k.ten AS ten,lop,k.trangThai AS trangThai
ORDER BY lop DESC,ma
```

Output `$lop = 4` (hocsinh4): `HINH_BINH_HANH 4`, `HINH_THOI 4`, `TU_GIAC 3`, `HINH_CHU_NHAT 1`, `HINH_VUONG 1`. Không có Hình thang cân, đúng AC.

Đồ thị tiên quyết (`CypherDoThi`):

```cypher
MATCH (target:KhaiNiem {ma:$ma,trangThai:'DA_RA_SOAT'})
OPTIONAL MATCH (target)-[:CAN_BIET_TRUOC*0..12]->(n:KhaiNiem)
WITH n WHERE n IS NOT NULL
OPTIONAL MATCH (n)-[:THUOC_LOP]->(l:Lop)
WITH n,min(l.so) AS lop
OPTIONAL MATCH (n)-[:CAN_BIET_TRUOC]->(p:KhaiNiem)
RETURN n.ma AS ma,n.ten AS ten,coalesce(n.trangThai,'') AS trangThai,coalesce(lop,-1) AS lop,
       collect(DISTINCT p.ma) AS tienQuyet
ORDER BY ma
```

`*0..12`: đường đi dài 0 tới 12 bước, nên có cả chính mục tiêu. Output `HINH_VUONG`: `HINH_VUONG → [HINH_CHU_NHAT, HINH_THOI]`, `HINH_CHU_NHAT → [GOC_VUONG, CANH_DOI]`, `HINH_THOI → [HAI_DT_VUONG_GOC, CANH_KE]`, bốn yếu tố nền không có tiên quyết. Sau sắp xếp: yếu tố nền → Hình chữ nhật, Hình thoi → Hình vuông (cuối). `TU_GIAC` là mục tiêu duy nhất không có tiên quyết; trang hiện "Em có thể học ngay khái niệm này".

"Em đã hiểu" (`CypherKhoaTaiKhoan` + `CypherGhiDaHoc`) khóa tài khoản như US-20, rồi chỉ ghi `DA_HOC` nếu khái niệm nằm trong lộ trình đang xem và lớp không vượt lớp học sinh:

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
MATCH (target:KhaiNiem {ma:$muc,trangThai:'DA_RA_SOAT'})
MATCH (target)-[:CAN_BIET_TRUOC*0..12]->(k:KhaiNiem {ma:$ma,trangThai:'DA_RA_SOAT'})
MATCH (k)-[:THUOC_LOP]->(kl:Lop)
WHERE k.ma IN $maLoTrinh AND kl.so <= l.so
MERGE (tk)-[r:DA_HOC]->(k)
ON CREATE SET r.luc=datetime()
RETURN count(r)>0 AS ok
```

**Kiểm chứng:** `us22_http_smoke.py` PASS: khách bị chuyển tới đăng nhập, CSRF sai trả 400, `DA_HOC` chỉ một quan hệ và giữ `luc` khi bấm lại, ✓ sau khi tải lại, câu "học ngay" với `TU_GIAC`.

## US-23 — Tiến độ và phần cần ôn

Khác README: README tính "cần ôn" bằng Cypher. Code chỉ lấy lịch sử thô (`TienDoRepository.CypherLichSu`) rồi tính trong `TienDoQuyTac` (C# thuần, có unit test). Lý do: một lượt làm có thể nối nhiều khái niệm (BT-027 nối hai hình), và cần khử trùng theo `maLan` trước khi lấy 5 lượt gần nhất; trong C# dễ kiểm thử hơn.

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[r:DA_LAM]->(b:BaiTap)
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
WITH r,b,k WHERE k IS NOT NULL
RETURN coalesce(r.maLan,elementId(r)) AS maLan,b.ma AS maBai,
       coalesce(r.dung,false) AS dung,coalesce(toString(r.luc),'') AS luc,
       k.ma AS maKhaiNiem,k.ten AS tenKhaiNiem
ORDER BY luc DESC,maLan DESC
```

Lịch sử lấy cả bài hiện đã bị ẩn (quản trị ẩn bài không xóa tiến độ). Quy tắc BR-06 trong `TienDoQuyTac.PhanTich`: tổng quan trên toàn lịch sử; "cần ôn" khi khái niệm có ≥ 3 lượt và tỉ lệ đúng trong tối đa 5 lượt gần nhất `< HocTap:NguongCanOn` (0,6).

Output (tổng hợp từ cùng mẫu) cho `hocsinh8` sau dev seed: `HINH_THOI` 5 lượt / 2 đúng → cần ôn (40%); `HINH_BINH_HANH` 2 lượt → chưa đủ dữ liệu, không xếp vào.

Trang Tiến độ có mục "Cần ôn lại", thanh ngang HTML/CSS (`role="img"`, `aria-label`, số % bằng chữ), lời chúc mừng chỉ khi có ít nhất một khái niệm ≥ 3 lượt và không khái niệm nào cần ôn.

**Kiểm chứng:** `us23_us24_http_smoke.py` PASS, gồm AC README với dev seed (`hocsinh8` thấy Hình thoi, không thấy Hình bình hành).

## US-24 — Gợi ý bài tiếp theo

Ứng viên (`CypherUngVien`), `$lop` lớp thật trong DB:

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(lop:Lop)
MATCH (b:BaiTap {hienThi:true,trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lk:Lop)
WHERE lb.so <= lop.so AND lk.so <= lop.so
  AND b.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
  AND NOT EXISTS { MATCH (tk)-[d:DA_LAM]->(b) WHERE d.dung=true }
RETURN DISTINCT b.ma AS ma,b.de AS de,k.ma AS maKhaiNiem,k.ten AS tenKhaiNiem,lb.so AS lop,b.doKho AS doKho
ORDER BY maKhaiNiem,doKho,ma
```

`GoiYService` chọn tối đa 5 bài không trùng theo bốn nhánh (`LoaiGoiY`, SRS UC-13):

| Nhánh | Khi nào | Thứ tự | Thông điệp |
|---|---|---|---|
| `ChuaCoLichSu` | Chưa có lượt nào (1a) | Dễ trước | Mời làm bài đầu tiên |
| `ChuaDuDuLieu` | Có lượt nhưng không khái niệm nào ≥ 3 lượt | Dễ trước | Làm thêm vài bài |
| `CanOn` | Có khái niệm cần ôn | Chỉ bài của khái niệm đó, độ khó tăng dần (FR-62) | Bài nên ôn |
| `KhongCanOn` | Đủ dữ liệu, không cần ôn (3a) | Độ khó **giảm dần** | Chúc mừng + link Lộ trình |

Nhánh `ChuaDuDuLieu` có để học sinh sai 2/2 không bị chúc mừng và đẩy sang bài khó nhất. "Bài kế tiếp" của học sinh là bài gợi ý đầu tiên khác bài vừa làm; của khách là bài kế tiếp cùng lớp theo mã (`CypherBaiTiepTheo`).

**Kiểm chứng:** `us23_us24_http_smoke.py` (bài từng đúng không được gợi ý, ≤ 5, tăng dần khi cần ôn, giảm dần ở 3a, không chúc mừng khi chưa đủ dữ liệu) và `c_roundtrip_http_smoke.py` (3 lần nộp sai BT-013 qua form → Hình thoi vào "Cần ôn lại" → 5 gợi ý Hình thoi → làm đúng bài đầu thì bài đó rời danh sách) đều PASS.

## Giải thích ký hiệu Cypher

| Ký hiệu | Ý nghĩa trong truy vấn phần C |
|---|---|
| `(b:BaiTap)` | Nút tên biến `b`, nhãn `BaiTap`; `{ma:$ma}` là điều kiện thuộc tính. |
| `-[r:DA_LAM]->` | Quan hệ có hướng, biến `r`, loại `DA_LAM`; dấu mũi tên chỉ chiều tài khoản → bài tập. |
| `-[:LIEN_QUAN_DEN]->` | Quan hệ không cần lấy biến, nối bài tới khái niệm. |
| `{ma: row.ma}` | Map thuộc tính dùng để tìm/tạo theo khóa `ma`. |
| `$tk`, `$lop`, `$ma` | Tham số bind riêng; không phải nội suy chuỗi. |
| `MERGE` | Tìm mẫu đã có hoặc tạo mẫu còn thiếu; dùng để seed có thể chạy lại. |
| `OPTIONAL MATCH` | Ghép nếu có; không làm mất bài khi học sinh chưa có lịch sử. |
| `EXISTS { MATCH ... }` | Kiểm tra có tồn tại đường/mẫu thỏa điều kiện. |
| `*0..` | Đường đi có độ dài từ 0 trở lên, nên gồm cả chính khái niệm mục tiêu. |
| `[]` trong `collect(...)[0..5]` | Danh sách kết quả; `[0..5]` lấy tối đa năm phần tử đầu. |
| `|` trong `[x IN gan WHERE ...]` | Phân cách biểu thức đầu ra của list comprehension với biến/điều kiện. |
| `WITH` | Chuyển tiếp, nhóm hoặc tính toán các giá trị cho phần kế tiếp của truy vấn. |
| `CASE WHEN` | Điều kiện để cộng số lần đúng mà vẫn giữ các lần sai trong tổng số. |
| `FOREACH (... IN ... | ...)` | Lặp thao tác cập nhật seed cho từng mã liên quan trong danh sách. |
| `DISTINCT` | Loại trùng khi nhiều đường quan hệ cùng đưa về một nút. |
| `IS NULL` / `IS NOT NULL` | Kiểm tra bộ lọc chưa chọn hoặc dữ liệu có tồn tại. |

## Đề xuất thay đổi chung

- **[Nghiêm trọng, PHẦN 0] KaTeX biến mọi cặp ngoặc thành công thức** (phát hiện 09/10/2026 khi chụp giao diện C ở 360px). `src/GeoQuad.Web/wwwroot/js/site.js` dòng 13–14 viết `{ left: '\[', right: '\]' }` và `{ left: '\(', right: '\)' }`. Trong chuỗi JavaScript nháy đơn, `'\('` chỉ là `'('`, nên auto-render coi mọi `( … )` và `[ … ]` là công thức. Hậu quả thấy được: đề BT-014 "(đơn vị cm²)" hiện thành chữ nghiêng dính liền "đơnvịcm²"; "(40%)" trên trang Tiến độ chỉ còn "40" vì `%` là chú thích trong TeX. Lỗi này ảnh hưởng mọi trang có ngoặc của A, B, C. Sửa: đổi thành `'\\['`, `'\\]'`, `'\\('`, `'\\)'`. **Đã sửa ngày 09/10/2026** theo đồng ý của người dùng (file PHẦN 0). Kiểm tra trên trình duyệt: đề BT-014 hiện "(đơn vị cm²)", trang Tiến độ hiện "(40%)" và "(cần ôn)"; công thức `$…$` vẫn render (trang kết quả 1, trang Hình chữ nhật của A 3).
- **[PHẦN 0 / A, B] `<main>` lồng nhau.** `_Layout.cshtml` đã có `<main id="noi-dung">`; view nào bọc thêm `<main>` sẽ tạo hai vùng chính lồng nhau, sai HTML và gây rối trình đọc màn hình. C đã đổi 7 view `HocTap` sang `<div>`. Còn ở B: `ApDung/Views/ChungMinh/Xem.cshtml`, `ApDung/Views/GoiY/Index.cshtml`, `ApDung/Views/TinhHuong/Index.cshtml`, `ApDung/Views/TinhHuong/Xem.cshtml`, `QuanTri/Views/BaiTap/Index.cshtml`, `QuanTri/Views/BaiTap/Soan.cshtml`.

- Neo4j không bảo đảm thứ tự bên trong `collect()` nếu không sắp xếp trước; truy vấn US-23 sắp xếp các lượt theo `luc` trước khi lấy 5 lượt gần nhất.
- Seed mẫu dùng thuộc tính phụ `DA_LAM.maLan` để `MERGE` idempotent. Thuộc tính này chỉ định danh dữ liệu dev; lượt làm thật vẫn là quan hệ mới như BR-09.
- Cần seed nền và seed A/B trước khi kiểm tra các AC có liên quan đến cấp/lớp, tính chất, công thức, dấu hiệu và CM-01.
- README mục 2 điểm 6 đang khẳng định mọi seed đều `DA_RA_SOAT`, trái SRS mục 4/NFR-09 và thực tế `21-chungminh-B.cypher` còn `NHAP`. Nhóm sửa README thành quy trình `NHAP` → có nguồn + người thứ hai rà soát → `DA_RA_SOAT`; tham khảo biên bản US-03 phía trên.
- A/nhóm đối chiếu lại `CT_HBH_DT`, `CT_THOI_DT` đang gắn lớp 4: chương trình Toán 2018 nêu nhận biết hai hình ở lớp 4 (tr. 37), nhưng ghi rõ tính chu vi/diện tích các hình đặc biệt ở lớp 6 (tr. 51). Nếu giữ lớp 4 theo mục tiêu website, ghi đó là nội dung mở rộng thay vì khẳng định yêu cầu chương trình chính thức; cập nhật `nguon` cụ thể cho seed A.

## Evidence triển khai C — 08/10/2026

Kiểm tra mới trên fixture riêng `geoquad-c-tests-neo4j` (Bolt `127.0.0.1:27688`, HTTP `127.0.0.1:27474`; không mount seed, nạp lần lượt bằng `cypher-shell`):

- Seed fixture nạp đủ 8 file hai lượt liên tiếp; tất cả lệnh seed PASS. US-03 review đối chiếu README/SRS/Neo4j: 14 điều kiện, 20 dấu hiệu, 12 tính chất, 12 công thức, 6 định lý nền; US-03 appendix and graph contract PASS.
- dotnet build GeoQuad.sln: 0 warning, 0 error. Toàn bộ 412 test ngoài B_ApDung và integration PASS; lượt chạy rộng hơn đạt 468/472, 4 test B_ApDung cần chmod/WSL không có trong Windows hiện tại.
- US-20/21 HTTP + Neo4j: PASS — GET không lộ khóa đáp án, POST thiếu CSRF bị từ chối, guest không ghi `DA_LAM`, chấm đáp án ở server, replay khác payload nhận 409 và không đổi lượt đã lưu. Chứng minh `BT-027`/`CM-01` còn `NHAP`: không có nút và POST xem lời giải trả 404.
- US-22 HTTP + Neo4j: PASS — guest tới đăng nhập; roadmap lớp 8 gồm mục tiêu `HINH_VUONG` và tiên quyết `GOC_VUONG`; CSRF sai trả 400; `DA_HOC` được ghi duy nhất, gửi lặp giữ nguyên `luc`.
- US-23/24 HTTP + Neo4j: PASS — fixture 10 lượt cho 70% toàn lịch sử và 40% 5 lượt mới nhất; bài từng làm đúng không được gợi ý; tối đa 5 bài duy nhất; `KeTiep` học sinh đi tới bài được gợi ý.
- Unit C sau US-22/23/24: 58/58 PASS; US-23 dùng ngưỡng <60% với tối thiểu 3 lượt trên cửa sổ 5 lượt, và gợi ý được sắp độ khó tăng dần. UI 360 px/desktop và cross-review chưa có evidence.

Truy vấn cốt lõi và ý nghĩa:

```cypher
MATCH (target:KhaiNiem {ma:$ma})-[:CAN_BIET_TRUOC*0..12]->(node:KhaiNiem)
RETURN DISTINCT node.ma AS ma
```

`*0..12` lấy mục tiêu và tối đa 12 bước tiên quyết; `DISTINCT` khử trùng các nhánh hội tụ. Service dựng lại chiều tiên quyết → mục tiêu rồi topo-sort; chu trình/thiếu node/vượt giới hạn trả lỗi an toàn, không trả lộ trình một phần.

```cypher
MATCH (tk:TaiKhoan {id:$tk})-[r:DA_LAM]->(b:BaiTap)
RETURN coalesce(r.maLan,elementId(r)) AS maLan, b.ma AS maBaiTap,
       coalesce(r.dung,false) AS dung, coalesce(toString(r.luc),'') AS luc
```

Lịch sử cá nhân lấy toàn bộ lượt đã ghi, kể cả bài hiện bị ẩn; tổng quan tính trên toàn lịch sử, còn nhãn “nên ôn” chỉ xét 5 lượt mới nhất của từng khái niệm sau khi khử lượt trùng.

Fixture C là dữ liệu kiểm thử dùng một lần, không phải bằng chứng nguồn nội dung/peer review. `BT-027`/`CM-01` vẫn chờ B hoàn tất trạng thái rà soát trước khi nghiệm thu đường xem lời giải công khai.