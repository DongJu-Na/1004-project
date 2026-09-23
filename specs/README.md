# PROJECT 1028 — 스펙 실행 안내 (2026-09-23)

헌장: `.specify/memory/constitution.md` v1.1.0
스펙 7개 모두 plan → tasks → analyze → implement까지 완료(2026-09-23). 남은 것은 Unity에서의 컴파일·테스트·Play 수동 검증이다.

## 순서와 의존

| 순번 | 폴더 | 분류 | 근거 | 선행 |
|---|---|---|---|---|
| 001 | 001-play-foundation | 기반 | PrototypePlan 1·3단계 | 없음 |
| 002 | 002-suspicion-system | 게임 규칙 | SDD §2, §7 | 001 |
| 003 | 003-npc-types | 게임 규칙 | SDD §3 | 001, 002 |
| 004 | 004-subdue-system | 게임 규칙 | SDD §4 | 001~003 |
| 005 | 005-report-scenario | 게임 규칙(통합) | SDD §5 | 001~004 |
| 006 | 006-vehicle-drive | 기반 | PrototypePlan 2단계 | 001 (002~005와 독립) |
| 007 | 007-encounter-framework | 게임 규칙 | SDD §6 | 001, 002, 006 |

## 현재 상태 (2026-09-23)

| 기능 | 어셈블리 | 데이터 | 테스트 씬 메뉴 | 수동 검증 |
|---|---|---|---|---|
| 001 | PlayFoundation | StreamingAssets/Dialogue | Build Play Foundation Test Scene | T038 |
| 002 | Suspicion | StreamingAssets/Suspicion | Build Suspicion Test Scene | T042 |
| 003 | NpcTypes | StreamingAssets/NpcTypes | Build NPC Types Test Scene | T039 |
| 004 | Subdue | StreamingAssets/Subdue | Build Subdue Test Scene | T044 |
| 005 | Report | StreamingAssets/Report | Build Report Test Scene | T037 |
| 006 | Vehicle | StreamingAssets/Vehicle | Build Vehicle Test Scene | T030 |
| 007 | Encounter | StreamingAssets/Encounter | Build Encounter Test Scene | T039 |

의존: Encounter → Subdue/Vehicle → NpcTypes → Suspicion → PlayFoundation. Vehicle은 PlayFoundation만 참조.
이 코드는 Unity 없는 환경에서 작성되어 **첫 컴파일이 아직 없다**. Unity 6000.6.2f1로 열고 콘솔 오류를 순서대로 해결한 뒤
각 기능의 quickstart.md를 001부터 따라간다.

## 한 기능을 끝까지 돌리는 방법

이 폴더는 git이 아니므로 현재 작업 기능은 `.specify/feature.json` 하나로 정해진다.

1. `.specify/feature.json`의 `feature_directory`를 작업할 폴더로 바꾼다. 지금은 `specs/001-play-foundation`.
2. 순서대로 입력한다.

```
/speckit-clarify
/speckit-plan
/speckit-tasks
/speckit-analyze
/speckit-implement
```

- `/speckit-clarify`는 선택이다. 각 스펙의 `checklists/requirements.md` Notes에 확인을 권장한 항목이 적혀 있다.
- `/speckit-plan`은 헌장 Constitution Check에서 원칙 I~V를 판정한다. 기반 기능(001, 006)은 원칙 I의 "기반 기능"
  갈래로 판정된다.
- `/speckit-tasks`는 헌장 Tasks 게이트에 따라 독립 테스트 태스크를 구현 태스크 앞에 배치해야 한다.
- 다음 기능으로 넘어갈 때 `feature.json`을 바꾸고 1부터 반복한다.

## 생성하지 않은 것과 이유

- **코옵 동기화(§8)**: 네트워크 전송은 PrototypePlan이 "온라인: Vertical Slice 이후"로 둔 기반 기능이므로 지금
  스펙을 쓰지 않았다. 대신 001~007 모두 "싱글·코옵 동작" 절을 갖고 플레이어 개체 단위 독립성을 요구한다.
- **004 염전섬 대본, 인카운터 20~30개 목록, 회수 테이블 내용**: SDD v0.9 예정(`[제안]`)이므로 헌장 원칙 I에 따라
  스펙 대상이 아니다. v0.9가 나오면 007 위에 콘텐츠 스펙으로 추가한다.
- **저장·타이틀·옵션 메뉴**: PrototypePlan 5단계. 002(이월 값)·005(끊긴 전화기)·007(플래그)이 "외부로 넘길 수 있게
  노출"까지 해두었으므로 저장 스펙은 이것들을 받기만 하면 된다.

## 스펙 공통 가정

- 에셋 폴더가 비어 있어 모든 테스트 씬은 엔진 기본 프리미티브로 구성한다. 신규 아트 에셋 0개.
- `[제안]` 수치는 전부 데이터 파일로 분리하고 `확정 대기`로 표시했다. SDD 제안값은 초깃값으로만 쓴다.
- 저장소에 없는 문서: 에셋 예산 v0.1, SDD v0.4~v0.7. 헌장 TODO에 기록되어 있다.
