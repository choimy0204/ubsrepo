UbisamBase 플랫폼 배포 폴더 (빌드할 때 자동 생성 - 직접 고치지 마세요)
갱신 시각 : 2026-09-21 22:44:28
빌드 구성 : Release
원본 소스 : D:\업무\03_개발\Source\용접기\08_UbisamPlatform\
----
이 폴더는 원본 소스에서 플랫폼(Core/Shell/Launcher)을 빌드할 때마다 자동으로 갱신됩니다(Debug/Release 모두).
소스(.cs)와 디버그 심볼(.pdb)은 들어 있지 않습니다.
----
이 폴더를 참조하는 프로그램은 UbisamBase.Launcher.exe로 실행됩니다.
Launcher는 실행할 때마다 이 폴더 내용을 자기 폴더로 복사한 뒤 Shell을 띄우므로,
플랫폼이 갱신되면 프로그램을 다시 빌드하지 않아도 다음 실행부터 새 버전이 적용됩니다.
----
다른 PC에서 쓰려면 이 폴더를 통째로 그 PC의 D:\UbisamPlatform\bin 에 복사하면 됩니다.
쓰지 않게 된 옛 파일까지 정리하려면 원본 소스 폴더에서 scripts\publish-platform.ps1 을 실행하세요.
