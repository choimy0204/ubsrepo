<#
.SYNOPSIS
  D:\UbisamPlatform\bin을 비우고, 플랫폼(Core/Shell/Launcher)을 Release로 다시 빌드해 깨끗하게 채운다.

.DESCRIPTION
  평소에는 이 스크립트가 필요 없다. 플랫폼을 빌드하면(Debug/Release 모두) 저장소 루트의
  Directory.Build.targets가 결과물을 D:\UbisamPlatform\bin에 자동으로 반영한다.

  다만 자동 반영은 덮어쓰기만 하므로, 의존 패키지를 빼는 등으로 더 이상 쓰지 않게 된 옛 dll이 그
  폴더에 남는다. 그런 것까지 정리하고 Release 결과물만으로 깨끗한 배포본을 만들고 싶을 때
  (다른 PC에 넘기기 직전 등) 이 스크립트를 쓴다.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts\publish-platform.ps1
#>

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $RepoRoot "UbisamBase.Platform.slnx"
$PublishDir = "D:\UbisamPlatform\bin"

Write-Host "== 1) $PublishDir 비우기 ==" -ForegroundColor Cyan
if (Test-Path $PublishDir) {
    Get-ChildItem $PublishDir -Force | Remove-Item -Recurse -Force -Confirm:$false
}

Write-Host "== 2) 플랫폼 Release 빌드 (빌드가 끝나면 Directory.Build.targets가 $PublishDir 을 채운다) ==" -ForegroundColor Cyan
dotnet build $Solution -c Release
if ($LASTEXITCODE -ne 0) { throw "플랫폼 빌드 실패" }

Write-Host "== 3) 확인 ==" -ForegroundColor Cyan
foreach ($name in "UbisamBase.Core.dll", "UbisamBase.Shell.exe", "UbisamBase.Launcher.exe", "UbisamBase.Launcher.exe.config", "README.txt") {
    if (-not (Test-Path (Join-Path $PublishDir $name))) { throw "$name 이(가) 배포 폴더에 없음" }
}
if (-not (Select-String -Path (Join-Path $PublishDir "UbisamBase.Launcher.exe.config") -Pattern "assemblyBinding" -Quiet)) {
    throw "Launcher 설정에 바인딩 리디렉트가 없음 - 실행 직후 FileLoadException으로 죽는다"
}
$leftover = Get-ChildItem $PublishDir -Recurse -File -Include *.pdb, *.cs
if ($leftover) { throw "배포 폴더에 .pdb 또는 .cs가 남아 있음 - 확인 필요" }

Write-Host "== 4) 깃에 올릴 업데이트 파일(dist) 갱신 ==" -ForegroundColor Cyan
# 저장소의 dist 폴더 = D:\UbisamPlatform\bin 과 완전히 같은 내용.
# 실행하는 PC의 Launcher가 이 폴더를 받아가 배포 폴더를 덮어쓴다(설정: update-settings.json).
$DistDir = Join-Path $RepoRoot "dist"
robocopy $PublishDir $DistDir /MIR /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw "dist 갱신 실패 (robocopy $LASTEXITCODE)" }
$global:LASTEXITCODE = 0
Write-Host "   $DistDir 갱신 완료 - git add/commit/push 하면 다른 PC가 이 버전으로 업데이트한다" -ForegroundColor DarkGray

Write-Host "== 완료: $PublishDir ==" -ForegroundColor Green
