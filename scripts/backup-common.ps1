#Requires -Version 7.2
# B's native PowerShell helpers. No password/hash is written to console output.
function Invoke-GqDocker {
    param([string[]]$DockerArguments)
    $StartInfo = [Diagnostics.ProcessStartInfo]::new('docker')
    $StartInfo.UseShellExecute = $false
    $StartInfo.RedirectStandardOutput = $true
    $StartInfo.RedirectStandardError = $true
    foreach ($Argument in $DockerArguments) { $StartInfo.ArgumentList.Add($Argument) }
    $NativeProcess = [Diagnostics.Process]::new()
    $NativeProcess.StartInfo = $StartInfo
    try {
        [void]$NativeProcess.Start()
        $OutputTask = $NativeProcess.StandardOutput.ReadToEndAsync()
        $ErrorTask = $NativeProcess.StandardError.ReadToEndAsync()
        $NativeProcess.WaitForExit()
        $NativeOutput = $OutputTask.GetAwaiter().GetResult()
        $NativeError = $ErrorTask.GetAwaiter().GetResult()
        if ($script:GqLog) { [IO.File]::AppendAllText($script:GqLog, $NativeError, [Text.UTF8Encoding]::new($false)) }
        # Explicit exit-code check for every native command; ErrorActionPreference alone is insufficient.
        if ($NativeProcess.ExitCode -ne 0) { throw "Docker thất bại (exit $($NativeProcess.ExitCode)); xem log riêng tư nếu có." }
        return $NativeOutput
    } finally { $NativeProcess.Dispose() }
}
function Get-GqContainer {
    param([string]$ContainerId)
    $ContainerData = Invoke-GqDocker -DockerArguments @('inspect', $ContainerId) | ConvertFrom-Json
    return @($ContainerData)[0]
}
function Assert-GqNoLink {
    param([string]$Path)
    $Item = Get-Item -LiteralPath $Path -Force
    while ($Item) {
        if ($Item.LinkType -or ($Item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Không dùng symlink/junction trong đường backup.' }
        $Item = if ($Item.PSIsContainer) { $Item.Parent } else { $Item.Directory }
    }
}
function Set-GqPrivate {
    param([string]$Path)
    $Item = Get-Item -LiteralPath $Path -Force
    if ($IsWindows) {
        $Identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $Acl = Get-Acl -LiteralPath $Path
        $Acl.SetAccessRuleProtection($true, $false)
        foreach ($Rule in @($Acl.Access)) { [void]$Acl.RemoveAccessRuleSpecific($Rule) }
        $Inheritance = if ($Item.PSIsContainer) { [Security.AccessControl.InheritanceFlags]'ContainerInherit,ObjectInherit' } else { [Security.AccessControl.InheritanceFlags]::None }
        $OwnerRule = [Security.AccessControl.FileSystemAccessRule]::new($Identity.User, 'FullControl', $Inheritance, 'None', 'Allow')
        $Acl.SetOwner($Identity.User)
        $Acl.AddAccessRule($OwnerRule)
        Set-Acl -LiteralPath $Path -AclObject $Acl
        if ((Get-Acl -LiteralPath $Path).Owner -ne $Identity.Name) { throw 'Không đặt được owner-only ACL.' }
    } else {
        $Mode = if ($Item.PSIsContainer) { '700' } else { '600' }
        & chmod $Mode $Path
        if ($LASTEXITCODE -ne 0) { throw 'Không đặt được quyền owner-only.' }
    }
}
function Write-GqText {
    param([string]$Path, [string]$Text)
    [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false))
    Set-GqPrivate $Path
}
function Wait-GqNeo4j {
    param([string]$ContainerId)
    for ($Attempt = 0; $Attempt -lt 90; $Attempt++) {
        $State = (Get-GqContainer $ContainerId).State
        if ($State.Health.Status -eq 'healthy') { return }
        if ($State.Status -in @('exited','dead') -or $State.Health.Status -eq 'unhealthy') { throw 'Neo4j không healthy.' }
        Start-Sleep -Seconds 2
    }
    throw 'Neo4j quá thời gian chờ healthy.'
}
function Invoke-GqQuery {
    param([string]$ContainerId, [string]$Cypher)
    return Invoke-GqDocker -DockerArguments @('exec','-e',"NEO4J_USERNAME=$script:GqUser",'-e',"NEO4J_PASSWORD=$script:GqPassword",
        $ContainerId,'cypher-shell','--access-mode','read','--format','plain','--history','in-memory','--fail-fast',$Cypher)
}
function Write-GqManifest {
    param([string]$ContainerId, [string]$Directory, [string]$Prefix)
    Invoke-GqQuery $ContainerId 'CALL db.awaitIndexes(60)' | Out-Null
    $Manifest = Invoke-GqQuery $ContainerId 'RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges,COUNT { (:TaiKhoan) } AS accounts,COUNT { ()-[:DA_LAM]->() } AS histories'
    $Manifest += Invoke-GqQuery $ContainerId 'SHOW CONSTRAINTS YIELD name,type,labelsOrTypes,properties RETURN name,type,labelsOrTypes,properties ORDER BY name'
    $Manifest += Invoke-GqQuery $ContainerId 'SHOW INDEXES YIELD name,type,labelsOrTypes,properties RETURN name,type,labelsOrTypes,properties ORDER BY name'
    $Manifest += Invoke-GqQuery $ContainerId 'MATCH (n) WHERE n:KhaiNiem OR n:DinhLy OR n:CongThuc OR n:DieuKien OR n:ChungMinh RETURN n.ma AS ma,labels(n) AS labels,properties(n) AS content ORDER BY ma'
    $HistoryPath = Join-Path $Directory "$Prefix-history.tmp"
    try {
        $HistoryLines = (Invoke-GqQuery $ContainerId 'MATCH ()-[r:DA_LAM]->() RETURN properties(r) AS history').Replace("`r`n","`n").TrimEnd("`n").Split("`n")
        [Array]::Sort($HistoryLines, [StringComparer]::Ordinal)
        Write-GqText $HistoryPath (($HistoryLines -join "`n") + "`n")
        $HistoryHash = (Get-FileHash -LiteralPath $HistoryPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $Manifest += "history-sha256=$HistoryHash`n"
        Write-GqText (Join-Path $Directory "$Prefix-manifest.txt") $Manifest
    } finally { if (Test-Path -LiteralPath $HistoryPath) { Remove-Item -LiteralPath $HistoryPath } }
}
