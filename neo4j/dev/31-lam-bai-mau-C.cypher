// Dữ liệu thử US-23 cho hocsinh8: 5 lần gần nhất về Hình thoi có đúng 2 lần.
// Hai lần về Hình bình hành minh họa khái niệm có dưới 3 lượt không vào danh sách ôn.
// MERGE theo maLan làm cho dữ liệu dev chạy lặp lại không sinh lượt làm trùng.

// Chạy sau khi ứng dụng Development tạo tài khoản demo hocsinh8.
// id là UUID; hocsinh8 là tenDangNhap, không được tạo TaiKhoan id='hocsinh8'.

UNWIND [
  {maLan:'DEV-C-HT-001', baiTap:'BT-023', luc:'2026-10-07T15:00:00Z', dung:false, dapAn:'20'},
  {maLan:'DEV-C-HT-002', baiTap:'BT-014', luc:'2026-10-07T15:05:00Z', dung:true, dapAn:'96'},
  {maLan:'DEV-C-HT-003', baiTap:'BT-020', luc:'2026-10-07T15:10:00Z', dung:false, dapAn:'20'},
  {maLan:'DEV-C-HT-004', baiTap:'BT-012', luc:'2026-10-07T15:15:00Z', dung:true, dapAn:'B'},
  {maLan:'DEV-C-HT-005', baiTap:'BT-023', luc:'2026-10-07T15:20:00Z', dung:false, dapAn:'22'},
  {maLan:'DEV-C-HBH-001', baiTap:'BT-017', luc:'2026-10-07T14:40:00Z', dung:true, dapAn:'84'},
  {maLan:'DEV-C-HBH-002', baiTap:'BT-017', luc:'2026-10-07T14:50:00Z', dung:false, dapAn:'80'}
] AS row
MATCH (tk:TaiKhoan {tenDangNhap:'hocsinh8'})
MATCH (b:BaiTap {ma:row.baiTap})
MERGE (tk)-[d:DA_LAM {maLan:row.maLan}]->(b)
SET d.luc = datetime(row.luc),
    d.dung = row.dung,
    d.dapAnDaChon = row.dapAn,
    d.thoiGianGiay = 45;
