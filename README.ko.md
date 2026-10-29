# 현추사결

[中文](README.md) | [English](README.en.md) | [한국어](README.ko.md)

**The Scroll of Taiwu**의 무공 세트 MOD **현추사결** 소스 프로젝트입니다. 이 저장소는 게임 개발사나 배급사와 무관합니다.

## 현재 버전

- MOD 버전: `1.1.0.0`
- 지원 게임 버전: `1.1.23`
- Steam Workshop FileId: `3747599301`
- 제작자: `Umbrella`

## 주요 기능

- 네 가지 기본 무공을 최상위 핵심 무공으로 재구성합니다.
- 네 결을 모두 운공하면 세트 효과를 제공합니다.
- 캐릭터마다 정순, 윤회, 나이와 수명 비율, 습득 무공에 따라 성장 배율을 계산합니다.
- 태오와 NPC가 같은 무공 효과와 성장 알고리즘을 사용합니다.
- NPC 혈극 및 일반 공격 별도 설정과 선택적 디버그 로그를 제공합니다.
- 게임 내 텍스트는 `CN/CNH/EN/KO` 및 실행 중 언어 전환을 지원하며 알 수 없는 언어는 중국어로 표시됩니다.

## 설정

게임 MOD 관리에서 「자세한 설정」을 열어 각 무공과 세트 강도를 조정할 수 있습니다. 기본 강도는 모두 100이며 변경 후 게임을 완전히 다시 시작해야 합니다.

성장 모드의 실제 강도는 「강도」 페이지 값에 성장 배율을 곱한 값입니다. 나이는 수명 곡선을 따륾니다. 수명의 중점에서 최고점에 도달한 후 점차 감소합니다.

## 디렉터리

```text
src/          C# 프로젝트와 소스
tools/        빌드, 검증 및 배포 스크립트
docs/         개발 및 유지 문서
deploy/       배포 설정, 기본 설정 템플릿과 리소스
.tmpbuild/    로컬 빌드 출력
.tmpdeploy/   배포 후보 임시 디렉터리
.decompiled/  로컬 API 스냅샷
```

## 빌드

게임과 .NET SDK 8.0 이상이 필요합니다. 게임 설치 경로는 `GameDir`로 지정합니다.

```powershell
dotnet build src/CoreSkillGrowth.Backend/CoreSkillGrowth.Backend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
dotnet build src/CoreSkillGrowth.Frontend/CoreSkillGrowth.Frontend.csproj -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\The Scroll Of Taiwu"
```

출력은 `.tmpbuild/<Configuration>/Plugins/`에 생성됩니다. Debug 결과는 게임에 배포하지 않습니다.

## 배포

`deploy/Config.Lua`를 갱신하고 Release를 빌드한 후, `.tmpbuild/Release/Plugins`와 `deploy` 리소스를 `.tmpdeploy`에 조립합니다. 플레이어가 수정한 설정 대신 기본 `deploy/Settings.Lua`만 포함합니다. 후보 패키지의 내용, 구조와 SHA-256을 확인한 후 업로드합니다.

## 로컬 개발 및 온라인 디렉터리

- 이 작업 사본만 소스, 변경 사항과 커밋의 기준입니다.
- `SteamLibrary\steamapps\workshop\content\838350`은 게시된 MOD를 받는 용도이며 테스트 배포나 소스 관리에는 사용하지 않습니다.
- 로컬 테스트는 빌드 후 `The Scroll Of Taiwu\Mod\XuanShuFourArts`에 배포합니다. 해당 디렉터리를 소스로 사용하지 않습니다.
- 로컬 개발 버전과 Workshop 구독 버전을 동시에 활성화하지 않습니다.
- 이전 ZIP은 `E:\Work\VibeCoding\TaiWuDev\Archives`에 보관하며 덮어쓰지 않습니다.

## 라이선스

소스 코드는 MIT License를 따릅니다. 자세한 내용은 [LICENSE](LICENSE)와 [ASSET_LICENSE.md](ASSET_LICENSE.md)를 참고하세요.
