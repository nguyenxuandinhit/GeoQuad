#Requires -Version 7.2
[CmdletBinding()]
param([string]$Project = 'geoquad', [string]$Compose = '', [string]$Output = '', [switch]$Quiesced)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'backup-common.ps1')
$Repository = Split-Path $PSScriptRoot -Parent
if (-not $Quiesced) { throw 'Cần -Quiesced: đã ngăn writer ngoài web (Browser/seed/job) đến khi backup kết thúc.' }
if ($Project -notmatch '^[a-z0-9][a-z0-9_-]{0,50}$') { throw 'Project không hợp lệ.' }
if (-not $Compose) { $Compose = Join-Path $Repository 'docker-compose.yml' }
if (-not (Test-Path -LiteralPath $Compose -PathType Leaf)) { throw 'Không có Compose.' }
$ComposeArguments = @('compose','-p',$Project,'-f',$Compose)
$ContainerId = (Invoke-GqDocker -DockerArguments ($ComposeArguments + @('ps','-a','-q','neo4j'))).Trim()
if (-not $ContainerId -or $ContainerId.Contains("`n")) { throw 'Cần đúng một Neo4j nguồn.' }
$Source = Get-GqContainer $ContainerId
if ($Source.Config.Labels.'com.docker.compose.project' -ne $Project) { throw 'Sai project nguồn.' }
$Image = $Source.Image
$DataMount = @($Source.Mounts | Where-Object { $_.Destination -eq '/data' -and $_.Type -eq 'volume' })
if ($DataMount.Count -ne 1) { throw 'Nguồn phải dùng named volume /data.' }
$DataVolume = $DataMount[0].Name
if ($Project -eq 'geoquad-b-tests' -and ($DataVolume -ne 'geoquad-b-tests_b-test-data' -or $Source.Config.Labels.'geoquad.test-isolation' -ne 'B-only')) { throw 'Sai isolation B.' }
$script:GqUser = if ($env:GQ_BACKUP_USER) { $env:GQ_BACKUP_USER } else { 'neo4j' }
$script:GqPassword = $env:GQ_BACKUP_PASSWORD
if (-not $script:GqPassword) {
    $AuthEntry = @($Source.Config.Env | Where-Object { $_.StartsWith('NEO4J_AUTH=') })
    if ($AuthEntry.Count -ne 1) { throw 'Cần GQ_BACKUP_PASSWORD.' }
    $Authentication = $AuthEntry[0].Substring(11).Split('/',2)
    if ($Authentication.Count -ne 2) { throw 'Cần GQ_BACKUP_PASSWORD.' }
    $script:GqUser = $Authentication[0]; $script:GqPassword = $Authentication[1]
}
$BackupBase = [IO.Path]::GetFullPath((Join-Path $Repository 'backups'))
Assert-GqNoLink $BackupBase
$RunId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,12)
if (-not $Output) { $Output = Join-Path $BackupBase $RunId }
$Output = [IO.Path]::GetFullPath($Output)
if ([IO.Path]::GetDirectoryName($Output) -ne $BackupBase -or [IO.Path]::GetFileName($Output) -notmatch '^[A-Za-z0-9_-]+$' -or (Test-Path -LiteralPath $Output)) { throw 'Output phải là một thư mục mới ngay dưới backups, không traversal.' }
[void][IO.Directory]::CreateDirectory($Output); Set-GqPrivate $Output
$script:GqLog = Join-Path $Output 'backup.log'; Write-GqText $script:GqLog ''
Invoke-GqDocker -DockerArguments @('run','--rm','--pull','never','--network','none','--user','0:0','-v',"${Output}:/backup",'--entrypoint','sh',$Image,'-c','test -w /backup') | Out-Null
$NeoWasRunning = [bool]$Source.State.Running
$WebId = ''; $WebWasRunning = $false
$Services = (Invoke-GqDocker -DockerArguments ($ComposeArguments + @('config','--services'))).Trim().Split("`n").Trim()
if ('web' -in $Services) {
    $WebId = (Invoke-GqDocker -DockerArguments ($ComposeArguments + @('ps','-a','-q','web'))).Trim()
    if ($WebId.Contains("`n")) { throw 'Chỉ hỗ trợ một web container.' }
    if ($WebId) { $WebWasRunning = [bool](Get-GqContainer $WebId).State.Running }
}
$Helper = "$Project-backup-$RunId"
try {
    if ($WebWasRunning) { Invoke-GqDocker -DockerArguments @('stop',$WebId) | Out-Null }
    if (-not $NeoWasRunning) { Invoke-GqDocker -DockerArguments @('start',$ContainerId) | Out-Null }
    Wait-GqNeo4j $ContainerId
    $Drained = $false
    for ($Attempt=0; $Attempt -lt 30; $Attempt++) {
        $CountText = (Invoke-GqQuery $ContainerId "SHOW TRANSACTIONS YIELD database WHERE database='neo4j' RETURN count(*) AS n").Trim().Split("`n")[-1].Trim()
        if ($CountText -notmatch '^\d+$') { throw 'Không đọc được transaction nguồn.' }
        if ([int]$CountText -le 1) { $Drained=$true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $Drained) { throw 'Nguồn vẫn có transaction ngoài maintenance.' }
    Write-GqManifest $ContainerId $Output 'source'
    Write-GqText (Join-Path $Output 'version.txt') (Invoke-GqQuery $ContainerId 'CALL dbms.components() YIELD versions RETURN versions[0] AS version')
    Write-GqText (Join-Path $Output 'image.txt') "$Image`n"
    Invoke-GqDocker -DockerArguments @('stop',$ContainerId) | Out-Null
    if ((Get-GqContainer $ContainerId).State.Running) { throw 'Không dump DB online.' }
    Invoke-GqDocker -DockerArguments @('run','--rm','--pull','never','--name',$Helper,'--label',"geoquad.backup-run=$RunId",'--network','none','--user','0:0',
        '-v',"${DataVolume}:/data",'-v',"${Output}:/backup",'--entrypoint','neo4j-admin',$Image,'database','dump','neo4j','--to-path=/backup') | Out-Null
    $Archive = Join-Path $Output 'neo4j.dump'
    if (-not (Test-Path -LiteralPath $Archive) -or (Get-Item -LiteralPath $Archive).Length -eq 0) { throw 'Không có archive sau dump.' }
    if (-not $IsWindows) {
        $OwnerUid = (& id -u).Trim(); if ($LASTEXITCODE -ne 0) { throw 'Không đọc được uid.' }
        $OwnerGid = (& id -g).Trim(); if ($LASTEXITCODE -ne 0) { throw 'Không đọc được gid.' }
        Invoke-GqDocker -DockerArguments @('run','--rm','--pull','never','--network','none','--user','0:0','-v',"${Output}:/backup",'--entrypoint','chown',$Image,"${OwnerUid}:$OwnerGid",'/backup/neo4j.dump') | Out-Null
    }
    Get-ChildItem -LiteralPath $Output -File | ForEach-Object { Set-GqPrivate $_.FullName }
    Write-GqText (Join-Path $Output 'sha256.txt') ((Get-FileHash -LiteralPath $Archive -Algorithm SHA256).Hash.ToLowerInvariant() + "`n")
} finally {
    $Helpers = (Invoke-GqDocker -DockerArguments @('ps','-a','--filter',"label=geoquad.backup-run=$RunId",'--format','{{.Names}}')).Trim().Split("`n")
    if ($Helper -in $Helpers) { Invoke-GqDocker -DockerArguments @('rm','-f',$Helper) | Out-Null }
    if ($NeoWasRunning) { Invoke-GqDocker -DockerArguments @('start',$ContainerId) | Out-Null; Wait-GqNeo4j $ContainerId }
    else { Invoke-GqDocker -DockerArguments @('stop',$ContainerId) | Out-Null }
    if ($WebWasRunning) { Invoke-GqDocker -DockerArguments @('start',$WebId) | Out-Null }
}
Write-Output "ARCHIVE=$Archive"
