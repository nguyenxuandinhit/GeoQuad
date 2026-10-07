<#
.SYNOPSIS
    Nạp dữ liệu seed vào Neo4j (US-01).
.DESCRIPTION
    Chạy lần lượt mọi file neo4j/seed/*.cypher theo thứ tự tên bằng cypher-shell
    trong container neo4j. Seed dùng MERGE nên chạy lại nhiều lần không sinh trùng (NFR-08).
.PARAMETER Dev
    Chạy thêm mọi file neo4j/dev/*.cypher (dữ liệu giả để thử tiến độ).
.EXAMPLE
    ./scripts/seed.ps1
    ./scripts/seed.ps1 -Dev
#>
[CmdletBinding()]
param(
    [switch]$Dev
)

$ErrorActionPreference = 'Stop'

$goc = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $goc

$nguoiDung = 'neo4j'
$matKhau   = 'geoquad123'

function Invoke-ThuMuc {
    param(
        [string]$ThuMucMay,      # đường dẫn trên máy, để liệt kê file
        [string]$ThuMucContainer # đường dẫn đã mount trong container
    )

    if (-not (Test-Path $ThuMucMay)) {
        Write-Host "  (bo qua) Khong tim thay thu muc $ThuMucMay" -ForegroundColor Yellow
        return
    }

    $files = Get-ChildItem -Path $ThuMucMay -Filter '*.cypher' -File | Sort-Object Name
    if ($files.Count -eq 0) {
        Write-Host "  (bo qua) Khong co file .cypher trong $ThuMucMay" -ForegroundColor Yellow
        return
    }

    foreach ($f in $files) {
        Write-Host "==> $($f.Name)" -ForegroundColor Cyan
        docker compose exec -T neo4j cypher-shell -u $nguoiDung -p $matKhau --format plain -f "$ThuMucContainer/$($f.Name)"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "LOI khi chay $($f.Name) (ma thoat $LASTEXITCODE). Dung lai." -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
}

Write-Host 'Kiem tra Neo4j da san sang...' -ForegroundColor Cyan
docker compose exec -T neo4j cypher-shell -u $nguoiDung -p $matKhau --format plain 'RETURN 1 AS ok' | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host 'Khong ket noi duoc Neo4j. Chay truoc: docker compose up -d neo4j' -ForegroundColor Red
    exit 1
}

Invoke-ThuMuc -ThuMucMay 'neo4j/seed' -ThuMucContainer '/seed'

if ($Dev) {
    Write-Host 'Nap them du lieu dev...' -ForegroundColor Cyan
    Invoke-ThuMuc -ThuMucMay 'neo4j/dev' -ThuMucContainer '/dev-seed'
}

Write-Host 'Seed xong.' -ForegroundColor Green
