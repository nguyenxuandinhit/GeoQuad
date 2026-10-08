# Cypher – Phần B · Áp dụng kiến thức

> Khung do PHẦN 0 tạo (US-01). Thành viên B ghi vào đây **mọi** truy vấn Cypher của phần mình.
> Story của phần B: US-13, US-14, US-17, US-18, US-25.

Quy ước bắt buộc (README mục 2, điểm 11): mỗi truy vấn có **ba** mục —
câu Cypher, **giải thích từng ký hiệu** xuất hiện trong câu đó, và **kết quả mong đợi**
trên dữ liệu seed. Tài liệu này dùng để cả nhóm học Neo4j và trình bày khi demo.

## Bảng ký hiệu Cypher (tham khảo chung)

| Ký hiệu | Nghĩa |
|---|---|
| `()` | một nút, ví dụ `(k:KhaiNiem)` là nút có nhãn `KhaiNiem`, đặt tên tạm là `k` |
| `:Nhan` | nhãn của nút (`KhaiNiem`, `DinhLy`…) hoặc kiểu của quan hệ |
| `[]` | một quan hệ, ví dụ `-[:THUOC_LOP]->` |
| `->` | chiều của quan hệ (từ nút phụ thuộc tới nút được tham chiếu) |
| `{}` | bộ thuộc tính / bản đồ khóa-giá trị, ví dụ `{ma: $ma}` |
| `$ten` | **tham số** — dữ liệu người dùng luôn truyền qua đây, không nối chuỗi (NFR-05) |
| `*1..` | đi theo quan hệ từ 1 bước trở lên (đường đi độ dài thay đổi) |
| `\|` | "hoặc", ví dụ `-[:CO_TINH_CHAT\|CO_CONG_THUC]->` |
| `MATCH` / `OPTIONAL MATCH` | tìm mẫu; `OPTIONAL` thì không có cũng vẫn trả dòng (giá trị `null`) |
| `MERGE` | tìm theo khóa, chưa có thì tạo — nhờ vậy seed chạy lại không trùng (NFR-08) |
| `WITH` | chuyển kết quả sang bước tiếp theo (như một "đường ống") |
| `UNWIND` | trải một danh sách thành từng dòng |

## Ký hiệu bổ sung cho query B

| Ký hiệu | Giải thích |
|---|---|
| `WHERE`, `AND`, `OR`, `NOT`, `IS NULL` | Lọc và kết hợp điều kiện; kiểm tra null riêng để chặn dữ liệu thiếu |
| `EXISTS {…}`, `NOT EXISTS {…}`, `COUNT {…}` | Kiểm tra/loại/đếm mẫu con mà không nhân các dòng query ngoài |
| `p=…`, `nodes(p)`, `*0..6` | Gán đường đi, lấy nút trên đường, giới hạn độ dài gồm đường0 |
| `(…){2,3}` | Quantified path lặp một đoạn gồm hai cạnh dấu hiệu từ2 đến3lần; biến trong đoạn trả danh sách |
| `all(x IN list WHERE …)`, `IN` | Mọi phần tử phải đạt predicate; kiểm tra mã thuộc danh sách |
| `[x IN list \| x.ma]`, `[...][0]`, `size`, `range(1,n)` | Map danh sách, truy cập phần tử đầu, lấy độ dài và dãy1..n |
| `min`, `max`, `collect(DISTINCT …)` | Aggregation theo nhóm còn trong WITH/RETURN, lấy lớp/gom các mục không trùng |
| `coalesce`, `trim`, `toLower`, `CONTAINS` | Giá trị đầu không null, bỏ khoảng trắng, chữ thường, tìm chuỗi con |
| `SET b += $props`, `DELETE r` | Merge map properties vào nút (null xóa property); chỉ xóa cạnh đã chọn, không xóa nút |
| `CREATE`, `MERGE`, `LIMIT`, `ORDER BY` | Tạo mới, tìm-hoặc-tạo, giới hạn số dòng, sắp kết quả ổn định |
| `labels`, `properties`, `CALL`, `YIELD` | Lấy nhãn/map thuộc tính; gọi procedure và chọn cột trả về |

## Truy vấn theo story

<!-- Mẫu cho mỗi truy vấn:

### US-xx · <tên màn hình> (FR-xx, UC-xx)

```cypher
...
```

**Giải thích ký hiệu:** …

**Kết quả mong đợi trên dữ liệu seed:** …
-->

### US-02/03/04 · Seed B và trạng thái review

Seed B dùng `20-dinhly-B.cypher` (14 điều kiện, 20 dấu hiệu, 6 định lý nền)
và `21-chungminh-B.cypher` (4 chứng minh, bước/căn cứ). Danh mục được chép từ
SRS B.3/B.4/B.7/B.9. Đó là nguồn soạn thảo; nguồn SGK độc lập và chữ ký C vẫn
chưa có. Tất cả nội dung B giữ `NHAP`. Xem
[phiếu review C](../../plans/261008-0132-quan-b-ap-dung-tdd/reports/content-review-B.md).

`MERGE (d:DinhLy {ma:row.ma})` khớp placeholder A trước khi `SET d:DauHieu`;
không MERGE đồng thời cả nhãn phụ vì có thể không khớp nút gốc. `YEU_CAU_LA`
trỏ hình nền, `KHANG_DINH` trỏ kết luận, `YEU_CAU_CO` trỏ điều kiện,
`THUOC_LOP` trỏ lớp xuất hiện đầu tiên. Định lý nền không có nhãn phụ.
TC_* do A sở hữu chỉ được `MERGE` nhãn gốc/mã, không bị B gán nội dung.

Seed CM dùng mã bước `CM-01-B1`… và `CO_BUOC {thuTu}` từ 1. `CAN_CU` trỏ
định lý nền/tính chất. BR-08 đi qua `CHUNG_MINH_CHO` → `KHANG_DINH` tới
khái niệm; không thêm quan hệ ngoài schema.

### US-02 · Kiểm kê schema và khung

```cypher
SHOW CONSTRAINTS YIELD name, type RETURN name, type ORDER BY name;
SHOW INDEXES YIELD name, type RETURN name, type ORDER BY name;
RETURN COUNT { (:CapHoc) } AS cap, COUNT { (:Lop) } AS lop,
       COUNT { (:KhaiNiem {loai:'HINH'}) } AS hinh,
       COUNT { (:KhaiNiem {loai:'YEU_TO'}) } AS yeuto,
       COUNT { ()-[:LA_TRUONG_HOP_DAC_BIET_CUA]->() } AS dacbiet;
```

**Giải thích:** `SHOW` đọc metadata; `YIELD` chọn cột; `RETURN` xuất dữ liệu;
`ORDER BY` sắp xếp. `COUNT { pattern }` đếm mẫu graph; `:Nhan` giới hạn nhãn;
`{loai:'HINH'}` khớp thuộc tính; `AS` đặt tên cột. Mũi tên theo chiều hình đặc
biệt → hình tổng quát, không được đảo chiều khi tra gợi ý.

**Kết quả mong đợi:** 13 constraints `UNIQUENESS`, index fulltext
`kienThucTimKiem`; counts `3/12/7/6/7`. Kết quả thực tế ghi trong báo cáo chạy
phase 1; build đơn thuần không chứng minh các counts này.

### US-03 · Catalog và lớp

```cypher
MATCH (d:DauHieu)-[:YEU_CAU_LA]->(nen:KhaiNiem),
      (d)-[:KHANG_DINH]->(dich:KhaiNiem),
      (d)-[:YEU_CAU_CO]->(dk:DieuKien), (d)-[:THUOC_LOP]->(l:Lop)
RETURN d.ma AS ma, nen.ma AS nen, dich.ma AS dich,
       dk.ma AS dieuKien, l.so AS lop, d.trangThai AS trangThai
ORDER BY ma;
```

**Giải thích:** `MATCH` bắt buộc đủ mọi quan hệ; dấu phẩy kết hợp mẫu sử dụng
cùng `d`; `d.ma` đọc thuộc tính; các alias là cột output. Đây là truy vấn audit
admin/test, cố ý bao gồm bản nháp; public queries phải dùng
`trangThai='DA_RA_SOAT'` và lớp hiển thị, kể cả URL chi tiết.

**Kết quả mong đợi:** 20 dấu hiệu lớp 8; `DH_HCN_2` có nền
`HINH_BINH_HANH`, đích `HINH_CHU_NHAT`, điều kiện `DK_MOT_GOC_VUONG`.
Mọi bản nháp B có `trangThai=NHAP`. DB tests đối chiếu toàn bộ 20 bộ mã, không
chỉ số lượng. Seed lặp A→B→A→B và B→A phải giữ nguyên labels/properties/edges.

### US-04 · Thứ tự bước và căn cứ

```cypher
MATCH (cm:ChungMinh {ma:$ma})-[r:CO_BUOC]->(b:Buoc)-[:CAN_CU]->(d:DinhLy)
WITH cm,r,b,d ORDER BY r.thuTu,d.ma
RETURN cm.giaThiet AS giaThiet, cm.ketLuan AS ketLuan,
       r.thuTu AS thuTu, b.noiDung AS noiDung, collect(DISTINCT d.ma) AS canCu
ORDER BY thuTu;
```

**Giải thích:** `$ma` là tham số driver, không nối chuỗi từ URL. `r` giữ quan
hệ để đọc `thuTu`. `WITH` giữ scope/sắp xếp trước aggregation;
`collect(DISTINCT ...)` gom căn cứ không lặp; `ORDER BY thuTu` xác định thứ tự
kết quả. Không dùng thứ tự Neo4j tự trả về hoặc thứ tự từ mã bước.

**Kết quả mong đợi:** `CM-01` có 4 bước, `CM-02` 4, `CM-03` 4, `CM-04` 3;
mỗi bước có căn cứ và thứ tự liên tục 1..n. Mẫu thiếu căn cứ không được public.
Cần C review cả chiều đảo của quan hệ góc ở CM-04 trước công bố.

### Khung test DB/HTTP cô lập

Compose authority: `tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml`.
Project `geoquad-b-tests`, volume `geoquad-b-tests_b-test-data`, cổng Bolt
`127.0.0.1:17687`, HTTP Neo4j `127.0.0.1:17474`; pin dòng 5.26 Community.
Fixture xác minh marker trên container/volume và port mapping trước mutation.
Không trỏ test vào Neo4j dev; thiếu opt-in/config thì FAIL rõ ràng.

```sh
docker compose -p geoquad-b-tests -f tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml up -d --wait
export GQ_B_INTEGRATION=1
export GQ_B_TEST_URI=bolt://127.0.0.1:17687
export GQ_B_TEST_USER=neo4j
export GQ_B_TEST_PASSWORD=geoquad_test_123
dotnet build GeoQuad.sln --nologo --disable-build-servers -m:1
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'Category!=Integration'
dotnet test tests/GeoQuad.Tests/GeoQuad.Tests.csproj --filter 'FullyQualifiedName~GeoQuad.Tests.B_ApDung&Category=Integration'
```

Mật khẩu trên chỉ cho container test riêng. Chạy bằng SDK/runtime .NET 8;
không đổi target hoặc roll-forward để coi test là đạt. HTTP helper chạy Program
Production trên loopback 15080; tài khoản test dùng `AuthHelper` thật, cookie/CSRF
thật, không gắn role giả. Dừng process sau test; không chạy demo seeder.

Fixture reset dùng `MATCH (n) DETACH DELETE n` **chỉ sau isolation guard**:
`MATCH (n)` chọn graph test, `DETACH` xóa cả các quan hệ của nút. Seed thực tế
được `cypher-shell --fail-fast -f` đọc từ mount `/seed:ro`, không split dấu `;`
hoặc tái tạo dữ liệu giả. Query diagnostic/invariants và snapshot đầy đủ nằm
trong `SeedBTests.cs`; kết quả chạy là bằng chứng counts, hướng/thuộc tính edges.

### US-07 · CurrentUser và guest

Tái sử dụng `ICurrentUser`: guest mặc định lớp 8, lớp lấy từ cookie `gq_lop`,
nâng cao từ `gq_nangcao`; `LopHienThi=12` khi bật nâng cao. Public B truyền lớp
hiển thị vào query; `TaiKhoanId=null` của guest không được dùng để ghi học tập.
Test CurrentUser chung vẫn là bằng chứng middleware/helper gốc; double B chỉ
cung cấp seam cho service tests. Chưa có buổi kickoff/trình bày nhóm được ghi.

## Đề xuất thay đổi chung

> README mục 2, điểm 1: nếu thấy cần sửa file của PHẦN 0 hoặc của phần khác thì **không sửa**,
> mà ghi đề xuất vào đây và làm cách tạm trong phạm vi phần B.

| Ngày | Đề xuất | Lý do | Cách tạm đang dùng | Trạng thái |
|---|---|---|---|---|
| 08/10/2026 | C hoàn thiện bài tập/tiến độ và reader bài ẩn | C controllers còn khung; A đã tích hợp main95a8519 | B giữ URL hợp đồng, thêm HTTP test liên kết căn cứ sang trang hình A | Chờ C và review nhóm |

## Truy vấn chạy thật của US-13/14/17/18/25

Chạy ngày 08/10/2026 bằng .NET 8.0.31 / Neo4j 5.26.31 trong `geoquad-b-tests`. Graph dùng fixture `SeedReviewedForTestsAsync`: trạng thái rà soát chỉ được mô phỏng trong graph test; seed phát hành vẫn `NHAP`. Các truy vấn mutation dưới đây chạy trong một transaction rồi **ROLLBACK**, không tạo bài thật.

Ký hiệu chung: `(x:Nhan)` là nút; `[r:QUAN_HE]` là cạnh có hướng; `$ten` là tham số Driver, không ghép chuỗi đầu vào; `EXISTS {…}` kiểm tra một mẫu con; `COUNT {…}` đếm theo từng nút; `collect(DISTINCT …)` gom các mục không trùng. `THUOC_LOP` giới hạn lớp ở public; admin chọn bộ lọc riêng. `NOT EXISTS` với nhánh `IS NULL` chặn placeholder chưa có lớp/nội dung/trạng thái.

### GoiYRepository.CypherHinh

Danh mục hình đã rà soát trong lớp hiện tại. `min(l.so)` lấy lớp đầu tiên của khái niệm.

```cypher
MATCH (h:KhaiNiem {loai:'HINH',trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND h.ten IS NOT NULL
RETURN h.ma AS ma,h.ten AS ten,min(l.so) AS lop ORDER BY ma
```

Tham số thực nghiệm: `{"lop":8}`.

Kết quả thật: 7 dòng; mã `HINH_BINH_HANH, HINH_CHU_NHAT, HINH_THANG, HINH_THANG_CAN, HINH_THOI, HINH_VUONG, TU_GIAC`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### GoiYRepository.CypherDieuKien

Danh mục điều kiện hợp lệ để nhận đầu vào; mã ngoài danh mục bị từ chối ở service.

```cypher
MATCH (d:DieuKien {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND d.noiDung IS NOT NULL
RETURN d.ma AS ma,d.noiDung AS noiDung,min(l.so) AS lop ORDER BY ma
```

Tham số thực nghiệm: `{"lop":8}`.

Kết quả thật: 14 dòng; mã `DK_BA_GOC_VUONG, DK_BON_CANH_BANG, DK_CANH_DOI_BANG, DK_CANH_DOI_SONG, DK_CHEO_BANG_NHAU, DK_CHEO_CAT_TRUNG_DIEM, DK_CHEO_PHAN_GIAC, DK_CHEO_VUONG_GOC, DK_GOC_DOI_BANG, DK_HAI_CANH_KE_BANG, DK_HAI_GOC_KE_DAY_BANG, DK_MOT_CAP_CANH_DOI_SONG, DK_MOT_CAP_SONG_SONG_BANG, DK_MOT_GOC_VUONG`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### GoiYRepository.CypherDauHieu

Đọc dấu hiệu và toàn bộ điều kiện cùng nền/kết luận đã rà soát. Nếu một điều kiện không hợp lệ thì loại cả dấu hiệu, tránh thu thập thiếu giả thiết.

```cypher
MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:YEU_CAU_LA]->(nen:KhaiNiem {trangThai:'DA_RA_SOAT'}),
  (d)-[:KHANG_DINH]->(dich:KhaiNiem {trangThai:'DA_RA_SOAT'}), (d)-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND d.noiDung IS NOT NULL
  AND all(h IN [nen,dich] WHERE EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop })
  AND COUNT { (d)-[:YEU_CAU_CO]->(:DieuKien) } > 0
  AND NOT EXISTS { (d)-[:YEU_CAU_CO]->(bad:DieuKien)
    WHERE bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR bad.noiDung IS NULL
      OR NOT EXISTS { (bad)-[:THUOC_LOP]->(bl:Lop) WHERE bl.so <= $lop } }
MATCH (d)-[:YEU_CAU_CO]->(dk:DieuKien)-[:THUOC_LOP]->(kl:Lop)
WHERE kl.so <= $lop
WITH d,nen,dich,l,dk,min(kl.so) AS dkLop ORDER BY dk.ma
RETURN d.ma AS ma,d.noiDung AS noiDung,nen.ma AS nen,dich.ma AS dich,l.so AS lop,
  collect(DISTINCT {ma:dk.ma,noiDung:dk.noiDung,lop:dkLop}) AS dks
ORDER BY ma
```

Tham số thực nghiệm: `{"lop":8}`.

Kết quả thật: 20 dòng; mã `DH_HBH_1, DH_HBH_2, DH_HBH_3, DH_HBH_4, DH_HBH_5, DH_HCN_1, DH_HCN_2, DH_HCN_3, DH_HTC_1, DH_HTC_2, DH_HT_1, DH_HV_1, DH_HV_2, DH_HV_3, DH_HV_4, DH_HV_5, DH_THOI_1, DH_THOI_2, DH_THOI_3, DH_THOI_4`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### GoiYRepository.CypherSuyRa

`LA_TRUONG_HOP_DAC_BIET_CUA*0..6` đi từ hình đặc biệt đến hình tổng quát, gồm đường dài 0. Mọi khái niệm trên đường phải qua gate lớp/review.

```cypher
RETURN EXISTS {
  MATCH p=(nen:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(dich:KhaiNiem {ma:$dich})
  WHERE all(h IN nodes(p) WHERE h.trangThai='DA_RA_SOAT' AND
    EXISTS { (h)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop })
} AS yes
```

Tham số thực nghiệm: `{"nen":"HINH_BINH_HANH","dich":"HINH_CHU_NHAT","lop":8}`.

Kết quả thật: 1 dòng; yes=False. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### GoiYRepository.CypherTrucTiep

Tìm dấu hiệu từ nền đã biết hoặc nền tổng quát kế thừa; service đối chiếu catalog rồi sắp số điều kiện thiếu và mã.

```cypher
MATCH p=(input:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(nen:KhaiNiem)
MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:YEU_CAU_LA]->(nen),
  (d)-[:KHANG_DINH]->(:KhaiNiem {ma:$dich}), (d)-[:THUOC_LOP]->(l:Lop)
WHERE l.so <= $lop AND all(h IN nodes(p) WHERE h.trangThai='DA_RA_SOAT' AND
  EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop })
RETURN DISTINCT d.ma AS ma ORDER BY ma
```

Tham số thực nghiệm: `{"nen":"HINH_BINH_HANH","dich":"HINH_CHU_NHAT","lop":8}`.

Kết quả thật: 3 dòng; mã `DH_HCN_1, DH_HCN_2, DH_HCN_3`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### GoiYRepository.CypherChuoi

Quantified path `{2,3}` tìm 2–3 dấu hiệu trung gian; `all` áp gate từng bước/điều kiện/hình và loại đường lặp. Sắp độ dài/mã, tối đa 3 chuỗi.

```cypher
MATCH inheritance=(input:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(start:KhaiNiem)
MATCH p=(start)
  ((:KhaiNiem)<-[:YEU_CAU_LA]-(d:DauHieu)-[:KHANG_DINH]->(:KhaiNiem)){2,3}
  (:KhaiNiem {ma:$dich})
WHERE all(x IN d WHERE x.trangThai='DA_RA_SOAT' AND x.noiDung IS NOT NULL AND
  EXISTS { (x)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
  COUNT { (x)-[:YEU_CAU_CO]->(:DieuKien) } > 0 AND
  NOT EXISTS { (x)-[:YEU_CAU_CO]->(bad:DieuKien)
    WHERE bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR bad.noiDung IS NULL OR
    NOT EXISTS { (bad)-[:THUOC_LOP]->(bl:Lop) WHERE bl.so <= $lop } })
  AND all(h IN nodes(p)+nodes(inheritance) WHERE NOT h:KhaiNiem OR
    (h.trangThai='DA_RA_SOAT' AND EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop }))
  AND all(n IN nodes(p) WHERE size([other IN nodes(p) WHERE other=n])=1)
WITH DISTINCT [x IN d | x.ma] AS codes
RETURN codes ORDER BY size(codes),codes LIMIT 3
```

Tham số thực nghiệm: `{"nen":"TU_GIAC","dich":"HINH_VUONG","lop":8}`.

Kết quả thật: 3 dòng; chuỗi DH_HCN_1 → DH_HV_1; DH_HCN_1 → DH_HV_2; DH_HCN_1 → DH_HV_3. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### ChungMinhRepository.CypherCatalog

Chỉ quảng bá link chứng minh khi đủ 3–6 bước liên tục, giả thiết/kết luận và mọi căn cứ qua gate; dùng cùng điều kiện với trang chi tiết.

```cypher
MATCH (cm:ChungMinh)-[:CHUNG_MINH_CHO]->(dh:DauHieu) WHERE cm.trangThai='DA_RA_SOAT' AND cm.giaThiet IS NOT NULL AND trim(cm.giaThiet)<>'' AND
cm.ketLuan IS NOT NULL AND trim(cm.ketLuan)<>'' AND
EXISTS { (cm)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
EXISTS { (cm)-[:CHUNG_MINH_CHO]->(dh:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(h:KhaiNiem {trangThai:'DA_RA_SOAT'})
  WHERE dh.noiDung IS NOT NULL AND EXISTS { (dh)-[:THUOC_LOP]->(dl:Lop) WHERE dl.so <= $lop }
  AND EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop } } AND
COUNT { (cm)-[:CO_BUOC]->(:Buoc) } >= 3 AND COUNT { (cm)-[:CO_BUOC]->(:Buoc) } <= 6 AND
all(i IN range(1,COUNT { (cm)-[:CO_BUOC]->(:Buoc) }) WHERE
  COUNT { (cm)-[r:CO_BUOC]->(:Buoc) WHERE r.thuTu=i }=1) AND
NOT EXISTS { (cm)-[:CO_BUOC]->(b:Buoc)
  WHERE b.trangThai IS NULL OR b.trangThai <> 'DA_RA_SOAT' OR b.noiDung IS NULL OR trim(b.noiDung)='' OR
  COUNT { (b)-[:CAN_CU]->(:DinhLy) }=0 OR
  EXISTS { (b)-[:CAN_CU]->(cc:DinhLy)
    WHERE cc.trangThai IS NULL OR cc.trangThai <> 'DA_RA_SOAT' OR cc.noiDung IS NULL OR trim(cc.noiDung)='' OR
    NOT EXISTS { (cc)-[:THUOC_LOP]->(cl:Lop) WHERE cl.so <= $lop } OR
    (cc:TinhChat AND NOT EXISTS { (owner:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:CO_TINH_CHAT]->(cc)
      WHERE EXISTS { (owner)-[:THUOC_LOP]->(ol:Lop) WHERE ol.so <= $lop } }) } } RETURN cm.ma AS ma,dh.ma AS dauHieu ORDER BY ma
```

Tham số thực nghiệm: `{"lop":8}`.

Kết quả thật: 4 dòng; mã `CM-01, CM-02, CM-03, CM-04`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### ChungMinhRepository.CypherXem

Đọc chứng minh và gom căn cứ theo bước đã sắp `r.thuTu`. Tính chất có chủ sở hữu để tạo liên kết trang hình; căn cứ nền hiển thị chi tiết tại chỗ.

```cypher
MATCH (cm:ChungMinh {ma:$ma}) WHERE cm.trangThai='DA_RA_SOAT' AND cm.giaThiet IS NOT NULL AND trim(cm.giaThiet)<>'' AND
cm.ketLuan IS NOT NULL AND trim(cm.ketLuan)<>'' AND
EXISTS { (cm)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
EXISTS { (cm)-[:CHUNG_MINH_CHO]->(dh:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(h:KhaiNiem {trangThai:'DA_RA_SOAT'})
  WHERE dh.noiDung IS NOT NULL AND EXISTS { (dh)-[:THUOC_LOP]->(dl:Lop) WHERE dl.so <= $lop }
  AND EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop } } AND
COUNT { (cm)-[:CO_BUOC]->(:Buoc) } >= 3 AND COUNT { (cm)-[:CO_BUOC]->(:Buoc) } <= 6 AND
all(i IN range(1,COUNT { (cm)-[:CO_BUOC]->(:Buoc) }) WHERE
  COUNT { (cm)-[r:CO_BUOC]->(:Buoc) WHERE r.thuTu=i }=1) AND
NOT EXISTS { (cm)-[:CO_BUOC]->(b:Buoc)
  WHERE b.trangThai IS NULL OR b.trangThai <> 'DA_RA_SOAT' OR b.noiDung IS NULL OR trim(b.noiDung)='' OR
  COUNT { (b)-[:CAN_CU]->(:DinhLy) }=0 OR
  EXISTS { (b)-[:CAN_CU]->(cc:DinhLy)
    WHERE cc.trangThai IS NULL OR cc.trangThai <> 'DA_RA_SOAT' OR cc.noiDung IS NULL OR trim(cc.noiDung)='' OR
    NOT EXISTS { (cc)-[:THUOC_LOP]->(cl:Lop) WHERE cl.so <= $lop } OR
    (cc:TinhChat AND NOT EXISTS { (owner:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:CO_TINH_CHAT]->(cc)
      WHERE EXISTS { (owner)-[:THUOC_LOP]->(ol:Lop) WHERE ol.so <= $lop } }) } }
MATCH (cm)-[:CHUNG_MINH_CHO]->(dh:DauHieu), (cm)-[:THUOC_LOP]->(l:Lop), (cm)-[r:CO_BUOC]->(b:Buoc)
MATCH (b)-[:CAN_CU]->(cc:DinhLy)
OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT]->(cc)
WITH cm,dh,l,r,b,collect(DISTINCT {ma:cc.ma,noiDung:cc.noiDung,maHinh:h.ma}) AS canCu
ORDER BY r.thuTu
RETURN cm.ma AS ma,cm.ten AS ten,cm.giaThiet AS giaThiet,cm.ketLuan AS ketLuan,
  dh.noiDung AS dinhLy,l.so AS lop,collect({thuTu:r.thuTu,noiDung:b.noiDung,canCu:canCu}) AS buoc
```

Tham số thực nghiệm: `{"ma":"CM-01","lop":8}`.

Kết quả thật: 1 dòng; mã `CM-01`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### TinhHuongRepository.CypherBoiCanh

Đọc đầy đủ 3 bối cảnh, kể cả bối cảnh chưa có tình huống phù hợp lớp.

```cypher
MATCH (b:BoiCanh) RETURN b.ma AS ma,b.ten AS ten ORDER BY ma
```

Tham số thực nghiệm: `{}`.

Kết quả thật: 3 dòng; mã `BC_DO_DUNG, BC_MANH_VUON, BC_NHA_CUA`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### TinhHuongRepository.CypherDoc

Danh sách và chi tiết dùng cùng query. Tính `max(l.so)` trên toàn bộ AP_DUNG; loại toàn tình huống nếu một kiến thức thiếu review/lớp/nội dung. `$ma=null` đọc danh sách, `$ma` cụ thể đọc chi tiết.

```cypher
MATCH (th:TinhHuong {trangThai:'DA_RA_SOAT'})-[:TRONG_BOI_CANH]->(bc:BoiCanh)
WHERE ($ma IS NULL OR th.ma=$ma) AND th.ten IS NOT NULL AND th.moTa IS NOT NULL AND
  th.loiGiai IS NOT NULL AND trim(th.loiGiai) <> '' AND
  COUNT { (th)-[:AP_DUNG]->() } > 0 AND COUNT { (th)-[:LIEN_QUAN_DEN]->(:KhaiNiem) } > 0 AND
  NOT EXISTS { (th)-[:AP_DUNG]->(bad) WHERE NOT (bad:DinhLy OR bad:CongThuc) OR
    bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR
    coalesce(bad.noiDung,bad.ten) IS NULL OR trim(coalesce(bad.noiDung,bad.ten))='' OR
    NOT EXISTS { (bad)-[:THUOC_LOP]->(:Lop) } OR
    EXISTS { (bad)-[:THUOC_LOP]->(high:Lop) WHERE high.so > $lop } } AND
  NOT EXISTS { (th)-[:LIEN_QUAN_DEN]->(h) WHERE NOT h:KhaiNiem OR h.trangThai IS NULL OR
    h.trangThai <> 'DA_RA_SOAT' OR NOT EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop } }
MATCH (th)-[:AP_DUNG]->(x)-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (owner:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(x)
OPTIONAL MATCH (x)-[:KHANG_DINH]->(dich:KhaiNiem)
WITH th,bc,max(l.so) AS lopCan,collect(DISTINCT {ma:x.ma,noiDung:coalesce(x.noiDung,x.ten),
  bieuThuc:x.bieuThuc,maHinh:coalesce(owner.ma,dich.ma)}) AS knowledge
WHERE lopCan <= $lop
RETURN th.ma AS ma,th.ten AS ten,bc.ma AS bc,bc.ten AS boiCanh,th.moTa AS moTa,th.loiGiai AS loiGiai,
  coalesce(th.thucHanh,false) AS thucHanh,lopCan AS lop,knowledge ORDER BY ma
```

Tham số thực nghiệm: `{"ma":null,"lop":8}`.

Kết quả thật: 9 dòng; mã `TH-01, TH-02, TH-03, TH-04, TH-05, TH-06, TH-07, TH-08, TH-09`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### DoDacRepository.Cypher

Đọc đúng dấu hiệu đã rà soát cho trace đo đạc; không tạo DA_LAM. Controller yêu cầu tập trả về khớp toàn bộ mã classifier cần.

```cypher
MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(h:KhaiNiem {trangThai:'DA_RA_SOAT'})
WHERE d.ma IN $mas AND d.noiDung IS NOT NULL AND
  EXISTS { (d)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
  EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop }
RETURN DISTINCT d.ma AS ma,d.noiDung AS noiDung,h.ma AS hinh ORDER BY ma
```

Tham số thực nghiệm: `{"mas":["DH_HCN_2"],"lop":8}`.

Kết quả thật: 1 dòng; mã `DH_HCN_2`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherKhaiNiem

Catalog khái niệm admin mọi lớp; `$ids` dùng cùng query để kiểm tra toàn bộ ID trong transaction lưu.

```cypher
MATCH (n:KhaiNiem {trangThai:'DA_RA_SOAT'})
WHERE ($ids IS NULL OR n.ma IN $ids) AND n.ten IS NOT NULL AND
  EXISTS { (n)-[:THUOC_LOP]->(:Lop) }
RETURN n.ma AS ma,n.ten AS ten ORDER BY ma
```

Tham số thực nghiệm: `{"ids":null}`.

Kết quả thật: 13 dòng; mã `CANH_DOI, CANH_KE, DUONG_CHEO, GOC_VUONG, HAI_DT_SONG_SONG, HAI_DT_VUONG_GOC, HINH_BINH_HANH, HINH_CHU_NHAT, HINH_THANG, HINH_THANG_CAN, HINH_THOI, HINH_VUONG, TU_GIAC`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherSuDung

Catalog định lý/công thức admin mọi lớp, xác minh đúng nhãn, có lớp/review trước gắn quan hệ.

```cypher
MATCH (n) WHERE (n:DinhLy OR n:CongThuc) AND n.trangThai='DA_RA_SOAT' AND
  ($ids IS NULL OR n.ma IN $ids) AND coalesce(n.noiDung,n.ten) IS NOT NULL AND
  EXISTS { (n)-[:THUOC_LOP]->(:Lop) }
RETURN n.ma AS ma,coalesce(n.noiDung,n.ten) AS ten ORDER BY ma
```

Tham số thực nghiệm: `{"ids":null}`.

Kết quả thật: 50 dòng; mã `CT_HBH_CV, CT_HBH_DT, CT_HCN_CHEO, CT_HCN_CV, CT_HCN_DT, CT_HT_DT, CT_HV_CHEO, CT_HV_CV, CT_HV_DT, CT_TG_CHUVI, CT_THOI_CV, CT_THOI_DT, DH_HBH_1, DH_HBH_2, DH_HBH_3, DH_HBH_4, DH_HBH_5, DH_HCN_1, DH_HCN_2, DH_HCN_3, DH_HTC_1, DH_HTC_2, DH_HT_1, DH_HV_1, DH_HV_2, DH_HV_3, DH_HV_4, DH_HV_5, DH_THOI_1, DH_THOI_2, DH_THOI_3, DH_THOI_4, DL_NEN_1, DL_NEN_2, DL_NEN_3, DL_NEN_4, DL_NEN_5, DL_NEN_6, TC_HBH_1, TC_HBH_2, TC_HBH_3, TC_HCN_1, TC_HCN_2, TC_HTC_1, TC_HTC_2, TC_HT_1, TC_HV_1, TC_TG_1, TC_THOI_1, TC_THOI_2`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherDanhSach

Admin lọc lớp/loại/hiển thị/trạng thái/từ khóa. `COUNT { DA_LAM }` riêng tránh nhân lượt bởi join quan hệ nội dung.

```cypher
MATCH (b:BaiTap)
WHERE ($lopLoc IS NULL OR EXISTS { (b)-[:THUOC_LOP]->(:Lop {so:$lopLoc}) }) AND
  ($loai IS NULL OR b.loai=$loai) AND ($hienThi IS NULL OR coalesce(b.hienThi,false)=$hienThi) AND
  ($trangThai IS NULL OR b.trangThai=$trangThai) AND
  ($tuKhoa IS NULL OR toLower(b.ma) CONTAINS toLower($tuKhoa) OR toLower(b.de) CONTAINS toLower($tuKhoa))
OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
WITH b,min(l.so) AS lop
RETURN b.ma AS ma,coalesce(b.de,'') AS de,b.loai AS loai,lop,coalesce(b.doKho,1) AS doKho,
  coalesce(b.hienThi,false) AS hienThi,coalesce(b.trangThai,'NHAP') AS trangThai,
  COUNT { (:TaiKhoan)-[:DA_LAM]->(b) } AS soLanLam ORDER BY ma
```

Tham số thực nghiệm: `{"lopLoc":null,"loai":null,"hienThi":null,"trangThai":null,"tuKhoa":null}`.

Kết quả thật: 10 dòng; mã `BT-001, BT-002, BT-003, BT-004, BT-005, BT-006, BT-007, BT-008, BT-009, BT-010`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherXem

Nạp bài để sửa cùng mọi quan hệ nội dung; không dùng lớp trong claim admin để loại bài.

```cypher
MATCH (b:BaiTap {ma:$ma})
OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
OPTIONAL MATCH (b)-[:SU_DUNG]->(s)
RETURN properties(b) AS props,min(l.so) AS lop,collect(DISTINCT k.ma) AS khaiNiem,collect(DISTINCT s.ma) AS suDung
```

Tham số thực nghiệm: `{"ma":"BT-001"}`.

Kết quả thật: 1 dòng. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherTao

CREATE giữ unique constraint làm trọng tài khi hai yêu cầu cùng mã. `$props` do validator whitelist; không lấy property tùy ý từ HTTP.

```cypher
CREATE (b:BaiTap {ma:$ma}) SET b += $props RETURN b.ma AS ma
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","props":{"loai":"TRAC_NGHIEM","de":"B\u00E0i d\u00F9ng ri\u00EAng cho audit query","giaiThich":"Gi\u1EA3i th\u00EDch audit","doKho":1,"hienThi":true,"nguon":"Nh\u00F3m GeoQuad","trangThai":"NHAP","phuongAn":["A","B","C","D"],"dapAnDung":"A","dapAnSo":null,"saiSo":null,"donVi":null,"loiGiaiMau":null}}`.

Kết quả thật: 1 dòng; mã `BT_DOC_B`. Counters: nodesCreated=1, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=14.

### BaiTapQuanTriRepository.CypherSua

Giữ nguyên nút và mã, cập nhật whitelist; các field loại cũ nhận null để Neo4j xóa property. Không thay nút đã có DA_LAM.

```cypher
MATCH (b:BaiTap {ma:$ma}) SET b += $props RETURN b.ma AS ma
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","props":{"loai":"TRAC_NGHIEM","de":"B\u00E0i d\u00F9ng ri\u00EAng cho audit query","giaiThich":"Gi\u1EA3i th\u00EDch audit","doKho":1,"hienThi":true,"nguon":"Nh\u00F3m GeoQuad","trangThai":"NHAP","phuongAn":["A","B","C","D"],"dapAnDung":"A","dapAnSo":null,"saiSo":null,"donVi":null,"loiGiaiMau":null}}`.

Kết quả thật: 1 dòng; mã `BT_DOC_B`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=13.

### BaiTapQuanTriRepository.CypherXoaQuanHe

Chỉ xóa ba loại quan hệ nội dung trước dựng lại trong cùng transaction; không chạm DA_LAM.

```cypher
MATCH (b:BaiTap {ma:$ma}) OPTIONAL MATCH (b)-[r:THUOC_LOP|LIEN_QUAN_DEN|SU_DUNG]->() DELETE r
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B"}`.

Kết quả thật: 0 dòng. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherLop

Gắn đúng lớp đã được xác minh trong transaction.

```cypher
MATCH (b:BaiTap {ma:$ma}),(l:Lop {so:$lop}) MERGE (b)-[:THUOC_LOP]->(l)
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","lop":8}`.

Kết quả thật: 0 dòng. Counters: nodesCreated=0, relationshipsCreated=1, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherGanKhaiNiem

UNWIND danh sách ID đã xác minh đầy đủ và MERGE cạnh. Không để MATCH âm thầm bỏ ID thiếu rồi commit một phần.

```cypher
MATCH (b:BaiTap {ma:$ma}) UNWIND $ids AS id MATCH (k:KhaiNiem {ma:id}) MERGE (b)-[:LIEN_QUAN_DEN]->(k)
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","ids":["HINH_CHU_NHAT"]}`.

Kết quả thật: 0 dòng. Counters: nodesCreated=0, relationshipsCreated=1, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherGanSuDung

Gắn định lý/công thức đã xác minh nhãn và ID; cùng transaction với thuộc tính/lớp/khái niệm.

```cypher
MATCH (b:BaiTap {ma:$ma}) UNWIND $ids AS id MATCH (n) WHERE n.ma=id AND (n:DinhLy OR n:CongThuc)
MERGE (b)-[:SU_DUNG]->(n)
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","ids":["DH_HCN_2"]}`.

Kết quả thật: 0 dòng. Counters: nodesCreated=0, relationshipsCreated=1, relationshipsDeleted=0, propertiesSet=0.

### BaiTapQuanTriRepository.CypherHienThi

Chỉ SET cờ hiển thị và trả mã để báo 404 nếu thiếu bài; giữ nguyên lịch sử và node.

```cypher
MATCH (b:BaiTap {ma:$ma}) SET b.hienThi=$hienThi RETURN b.ma AS ma
```

Tham số thực nghiệm: `{"ma":"BT_DOC_B","hienThi":false}`.

Kết quả thật: 1 dòng; mã `BT_DOC_B`. Counters: nodesCreated=0, relationshipsCreated=0, relationshipsDeleted=0, propertiesSet=1.

Query xác minh lớp trong transaction: `MATCH (l:Lop {so:$lop}) RETURN l.so AS so`; `$lop=8` trả đúng một dòng `8`. Các bước lưu chỉ chạy sau khi tập ID/lớp đủ; test rollback thật và concurrent create được ghi tại docs/kiem-thu.md.

## Backup/restore và bàn giao US-26/27

Scripts B gồm `backup.sh`, `restore.sh` và helper `backup-common.sh`; PowerShell gồm ba file tương ứng. Helper mới dùng chung logic manifest, native process và quyền file, không sửa seed scripts chung.

Trước backup, người vận hành ngăn mọi writer ngoài web: Neo4j Browser, cypher-shell seed, job và các app khác. `--quiesced` / `-Quiesced` xác nhận maintenance này; script không tự chặn mọi client Bolt. Script dừng web nếu có, chờ transaction còn lại, capture manifest, dừng Neo4j rồi dump bằng **image ID của nguồn**. Trap/finally phục hồi đúng dịch vụ vốn chạy; chờ Neo4j healthy trước web. Nguồn vốn stopped được trả lại stopped. Không báo ARCHIVE thành công khi dump hoặc recovery lỗi.

```sh
# Chỉ project test riêng; không dừng database dev trong bài test.
./scripts/backup.sh --project geoquad-b-tests --compose tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml --quiesced
# Lấy đúng ARCHIVE=... mà backup vừa trả về.
./scripts/restore.sh --archive backups/<run-id>/neo4j.dump --target-project geoquad-b-restore-<run-id>
```

```powershell
# PowerShell 7.2+, Docker Desktop; ví dụ project test riêng.
./scripts/backup.ps1 -Project geoquad-b-tests -Compose tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml -Quiesced
./scripts/restore.ps1 -Archive backups/<run-id>/neo4j.dump -TargetProject geoquad-b-restore-<run-id>
```

`GQ_BACKUP_USER/PASSWORD` có thể cấp credential maintenance qua environment; nếu bỏ trống, script đọc `NEO4J_AUTH` của container nguồn trong bộ nhớ. Không ghi password/hash lên console. `GQ_RESTORE_PASSWORD` đặt auth DBMS mới; mặc định credential chỉ dành drill local. Backup chứa dữ liệu tài khoản ứng dụng và băm mật khẩu: shell đặt directory0700/file0600, PowerShell Windows đặt owner-only ACL. Không commit `backups/`. Archive, checksum, source-manifest, version, image ID và log riêng tư nằm cùng run directory.

Restore chỉ tạo đích **mới** `geoquad-b-restore-*`, volume riêng, Bolt27687/Browser27474 bind loopback. Từ chối target dev/đích tồn tại, archive thiếu/symlink, đường ngoài backups và checksum sai trước load. Không có chế độ overwrite trong phiên bản này. Muốn thay thế production phải có protocol riêng được owner phê duyệt, giữ backup trước và cửa sổ maintenance.

Manifest kiểm tra số nút/cạnh/tài khoản/DA_LAM, constraints/indexes, nội dung catalog và digest properties lịch sử; không giữ username/hash tài khoản trong manifest. Dump `neo4j` phục hồi **application database**, không phục hồi `system` users Neo4j. Đích khởi tạo DBMS auth mới; tài khoản GeoQuad và băm mật khẩu trong application graph được giữ. Script không tự xóa đích khi load/manifest fail, để người vận hành kiểm tra. Bài test cleanup chỉ tài nguyên có marker đúng run do chính test tạo.

**Kết quả thật:** drill shell trên Neo4j5.26.31 phục hồi graph có 134 nút/299 cạnh ở lượt đầu, đúng DA_LAM và xác minh đăng nhập ứng dụng bằng AuthHelper; manifest nguồn/đích khớp, graph nguồn không đổi. Báo cáo mới nhất tại [backup-drill-B.json](../../plans/261008-0132-quan-b-ap-dung-tdd/reports/backup-drill-B.json). Flow tests chứng minh dump fail, recovery không healthy, nguồn vốn stopped và checksum hỏng không success giả. PowerShell đã đọc rà soát nhưng **chưa chạy trên PowerShell/Windows**, chưa nghiệm thu ACL Windows.

### Các query manifest

```cypher
CALL db.awaitIndexes(60);
RETURN COUNT { () } AS nodes, COUNT { ()-[]->() } AS edges,
  COUNT { (:TaiKhoan) } AS accounts, COUNT { ()-[:DA_LAM]->() } AS histories;
SHOW CONSTRAINTS YIELD name,type,labelsOrTypes,properties
RETURN name,type,labelsOrTypes,properties ORDER BY name;
SHOW INDEXES YIELD name,type,labelsOrTypes,properties
RETURN name,type,labelsOrTypes,properties ORDER BY name;
MATCH (n) WHERE n:KhaiNiem OR n:DinhLy OR n:CongThuc OR n:DieuKien OR n:ChungMinh
RETURN n.ma AS ma,labels(n) AS labels,properties(n) AS content ORDER BY ma;
MATCH ()-[r:DA_LAM]->() RETURN properties(r) AS history;
CALL dbms.components() YIELD versions RETURN versions[0] AS version;
SHOW TRANSACTIONS YIELD database WHERE database='neo4j' RETURN count(*) AS n;
```

**Ký hiệu:** `CALL` gọi procedure; `YIELD` chọn cột procedure/metadata; `labels` lấy nhãn, `properties` lấy map thuộc tính; `COUNT {}` đếm mẫu; `ORDER BY name/ma` giữ manifest có thứ tự. Properties DA_LAM được sort Ordinal ở command runner, SHA256 rồi xóa file tạm; không dùng `toString(map)` vì Neo4j không hỗ trợ. `SHOW TRANSACTIONS` đếm cả query maintenance đang chạy; script đợi còn tối đa một transaction trước snapshot. `db.awaitIndexes` chờ index online, không so trạng thái POPULATING thoáng qua.

**Kết quả:** version5.26.31, 13 unique constraints và index fulltext; counts/catalog/digest sau load bằng trước dump. Lượt lỗi `toString(map)` khi phát triển đã trả nonzero và phục hồi source, được sửa bằng hash trên output properties đã sort; drill sau sửa đạt. Những query này chạy thật trong BackupRestoreTests, không chỉ shell lint.

### Checklist bàn giao B

1. C review độc lập nguồn/toán học của seed (đặc biệt căn cứ góc ở CM-04); ghi người/ngày/kết luận rồi mới đổi NHAP thành DA_RA_SOAT. Không copy trạng thái `reviewTestOnly` từ fixture sang dev.
2. A kiểm tra các URL `/ApDung/GoiY`, `/ApDung/ChungMinh/Xem/CM-01`, `/ApDung/TinhHuong/Xem/TH-01`, `/QuanTri/BaiTap`; dùng cả guest lớp3/8, nâng cao và admin.
3. C hoàn thiện reader bài tập/tiến độ trước kiểm thử bài ẩn và ≥2 Cypher C. Bản đồ/trang hình A đã tích hợp từ main95a8519; ngân hàng bài tập và tiến độ C còn controller khung. Link căn cứ B sang trang hình A có test HTTP trên trạng thái tích hợp.
4. Chạy kiểm tra 360px/desktop, keyboard, KaTeX và network4G; phiên tool hiện không có browser khả dụng nên chưa có evidence visual.
5. A đưa lệnh backup/restore đã nghiệm thu vào README và kiểm tra máy sạch≤15phút; C đưa bước demo B/recovery vào kịch bản chung. Không đóng US-26/27 chỉ vì test B đạt.
