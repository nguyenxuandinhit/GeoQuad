#Requires -Version 7.2
[CmdletBinding()]
param([Parameter(Mandatory)][string]$Archive, [string]$TargetProject = ('geoquad-b-restore-' + [Guid]::NewGuid().ToString('N').Substring(0,12)))
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'backup-common.ps1')
$Repository = Split-Path $PSScriptRoot -Parent
if ($TargetProject -notmatch '^geoquad-b-restore-[a-z0-9_-]{1,45}$') { throw 'Chỉ restore đích geoquad-b-restore-* mới; không overwrite dev.' }
if (-not (Test-Path -LiteralPath $Archive -PathType Leaf)) { throw 'Archive không tồn tại.' }
$ArchiveItem = Get-Item -LiteralPath $Archive -Force
Assert-GqNoLink $ArchiveItem.FullName
if ($ArchiveItem.LinkType -or $ArchiveItem.Name -ne 'neo4j.dump') { throw 'Archive phải là neo4j.dump, không symlink.' }
$Archive = $ArchiveItem.FullName
$Directory = Split-Path $Archive -Parent
$BackupBase = [IO.Path]::GetFullPath((Join-Path $Repository 'backups')) + [IO.Path]::DirectorySeparatorChar
if (-not $Directory.StartsWith($BackupBase, [StringComparison]::OrdinalIgnoreCase)) { throw 'Archive phải nằm dưới backups của repository.' }
foreach ($Required in @('sha256.txt','image.txt','source-manifest.txt')) {
    $Metadata = Join-Path $Directory $Required
    if (-not (Test-Path -LiteralPath $Metadata -PathType Leaf)) { throw "Thiếu metadata $Required" }
    Assert-GqNoLink $Metadata
}
$Expected = (Get-Content -LiteralPath (Join-Path $Directory 'sha256.txt') -Raw).Trim()
if ($Expected -notmatch '^[a-f0-9]{64}$' -or (Get-FileHash -LiteralPath $Archive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $Expected) { throw 'Checksum archive không hợp lệ.' }
$Image = (Get-Content -LiteralPath (Join-Path $Directory 'image.txt') -Raw).Trim()
if ($Image -notmatch '^sha256:[a-f0-9]{64}$') { throw 'Image ID không hợp lệ.' }
Invoke-GqDocker -DockerArguments @('image','inspect',$Image) | Out-Null
$Volume = "${TargetProject}_data"; $Container = "$TargetProject-neo4j"
$VolumeNames = (Invoke-GqDocker -DockerArguments @('volume','ls','--format','{{.Name}}')).Trim().Split("`n")
$ContainerNames = (Invoke-GqDocker -DockerArguments @('ps','-a','--format','{{.Names}}')).Trim().Split("`n")
if ($Volume -in $VolumeNames -or $Container -in $ContainerNames) { throw 'Đích đã tồn tại; không overwrite.' }
$script:GqUser = 'neo4j'; $script:GqPassword = if ($env:GQ_RESTORE_PASSWORD) { $env:GQ_RESTORE_PASSWORD } else { 'geoquad_restore_123' }
$script:GqLog = Join-Path $Directory "restore-$TargetProject.log"; Write-GqText $script:GqLog ''
Invoke-GqDocker -DockerArguments @('volume','create','--label',"geoquad.restore-run=$TargetProject",$Volume) | Out-Null
Invoke-GqDocker -DockerArguments @('run','--rm','--pull','never','--network','none','--user','0:0','-v',"${Volume}:/data",'-v',"${Directory}:/backup:ro",
    '--entrypoint','neo4j-admin',$Image,'database','load','neo4j','--from-path=/backup') | Out-Null
$HealthCommand = 'NEO4J_USERNAME=neo4j NEO4J_PASSWORD="${NEO4J_AUTH#neo4j/}" cypher-shell --format plain "RETURN 1" >/dev/null'
Invoke-GqDocker -DockerArguments @('run','-d','--pull','never','--name',$Container,'--label',"geoquad.restore-run=$TargetProject",'-v',"${Volume}:/data",
    '-p','127.0.0.1:27687:7687','-p','127.0.0.1:27474:7474','-e',"NEO4J_AUTH=neo4j/$script:GqPassword",'-e','NEO4J_server_memory_heap_initial__size=256m',
    '-e','NEO4J_server_memory_heap_max__size=512m','--health-cmd',$HealthCommand,'--health-interval','5s','--health-timeout','10s','--health-retries','30','--health-start-period','30s',$Image) | Out-Null
Wait-GqNeo4j $Container
Write-GqManifest $Container $Directory $TargetProject
$SourceManifest = (Get-Content -LiteralPath (Join-Path $Directory 'source-manifest.txt') -Raw).Replace("`r`n","`n")
$TargetManifest = (Get-Content -LiteralPath (Join-Path $Directory "$TargetProject-manifest.txt") -Raw).Replace("`r`n","`n")
if ($SourceManifest -cne $TargetManifest) { throw 'Manifest nguồn/đích khác nhau; giữ đích để kiểm tra.' }
Write-Output "RESTORED=$Container"
Write-Output 'BOLT=bolt://127.0.0.1:27687'
# User database only. Fresh system/auth; original source volume is never mounted here.
