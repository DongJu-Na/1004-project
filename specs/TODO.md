# PROJECT 1028 — 잔여 업무 To-Do (2026-09-23 기준)

001~007 스펙·플랜·태스크·구현 문서 작업은 끝났다. 남은 것은 (A) Unity에서 실제로 돌려보는 일, (B) 스펙에서 "해석"으로 남긴 결정,
(C) 헌장이 요구하는 문서 보강, (D) 수치 튠, (E) 아직 스펙이 없는 기능, (F) v0.9 콘텐츠, (G) 프로젝트 관리다. 위에서 아래로 순서대로 하면 된다.

---

## A. 즉시 — Unity에서 검증 (집 PC, Unity 6000.6.2f1)

- [ ] A1. 프로젝트 열기 → 콘솔 컴파일 오류 해결. 약 160개 C# 파일이 첫 컴파일. 오류는 001(PlayFoundation)부터 순서대로. 첫 오류 메시지를 그대로 Claude Code에 붙이면 수정.
- [ ] A2. `Window > General > Test Runner` → EditMode → Run All. 어셈블리 7개(PlayFoundation·Suspicion·NpcTypes·Subdue·Report·Vehicle·Encounter) 테스트 35파일 녹색 확인.
- [ ] A3. 001 `Tools > PROJECT 1028 > Build Play Foundation Test Scene (2 Players)` → `specs/001-play-foundation/quickstart.md` §3 시나리오 (이동·카메라 / 대화·목적지 / 시야 / 코옵). → tasks T038 체크.
- [ ] A4. 002 `Build Suspicion Test Scene` → `specs/002-suspicion-system/quickstart.md` (F1~F7 사건, ] 섬 의심도, N 낮/밤, R 이월, T 배속). → T042.
- [ ] A5. 003 `Build NPC Types Test Scene` → `specs/003-npc-types/quickstart.md` (I 조사, L 라벨 숨김, 밀고자 전달, 동조자 전환). → T039.
- [ ] A6. 004 `Build Subdue Test Scene` → `specs/004-subdue-system/quickstart.md` (F 제압, E 들기/옮기기, G 내려놓기, 목격/발견 +40/+30). → T044.
- [ ] A7. 005 `Build Report Test Scene` → `specs/005-report-scenario/quickstart.md` (신고 이동, E 끊기, K 도구, 제압 중단). → T037.
- [ ] A8. 006 `Build Vehicle Test Scene` → `specs/006-vehicle-drive/quickstart.md` (탑승·주행·하차·뒤집힘 복구·NPC 무해). 주행 감각이 나쁘면 `StreamingAssets/Vehicle/vehicle_params.json` 튠. → T030.
- [ ] A9. 007 `Build Encounter Test Scene` → `specs/007-encounter-framework/quickstart.md` (X 강제, 1/2 선택, Z 100런 시뮬). → T039.
- [ ] A10. 각 quickstart §4 성공 기준 체크박스에 결과 기록. 실패 항목은 "현상 + 재현 순서"로 적어 두기.

## B. 스펙에서 "해석"으로 남긴 결정 — `/speckit-clarify` 또는 직접 확정

각 스펙 `checklists/requirements.md` Notes와 Assumptions에 적혀 있다. SDD에 없어서 내가 정한 것들이다. 맞으면 그대로, 아니면 데이터/스펙 수정.

- [ ] B1. 004: 정면 밀치기 **실패 시 개인 의심 3**(`shove_failed`, 확정대기). 실패해도 목격 대가(+40) 없음.
- [ ] B2. 004: "인지하지 못한 상태" = 001 "보고 있는가 = 거짓" (개인 의심 값과 무관).
- [ ] B3. 005: **관리소는 끊을 수 없다**, 전화기만 끊기 가능. 사용 가능한 지점이 없으면 NPC가 **신고를 포기**한다(§5.2 "아무 일도 일어나지 않는다" 해석).
- [ ] B4. 005: 신고 동작(도착 후) 중 전화기가 끊기면 재타깃(끊긴 전화로는 신고를 마칠 수 없다).
- [ ] B5. 003: 밀고자 전달을 002 전파 플래그가 아니라 003 `InformerBehaviour`가 전담(이중 +1 방지). 밀고자 도착 시 관리자 개인 의심 +1만, 섬 의심도 변화 없음.
- [ ] B6. 002: 시간대 배율은 개인 의심에만, 섬 의심도에는 미적용. 감시자 +2 등 "즉시" 규칙은 배율 무시(`ignoresTimeMultiplier`).
- [ ] B7. 002: "매우 느림" 감소 = 1분 안에 구간이 안 바뀌는 속도(개인 60초/1, 섬 0.5/분). 이월 = 절반, 섬 최소 5.
- [ ] B8. 001: 점프 유지 여부(기존 모터에 있음, SDD 무언급). 대화 진행 키 = 상호작용 키(E) 겸용.
- [ ] B9. 006: 동승석 1개 추가(PrototypePlan에 없음, 헌장 원칙 IV 충족용).
- [ ] B10. 007: C 유형 선택은 타임아웃(12초) 시 기본 선택. 선택 중 도보 이탈은 잠금 때문에 불가.

## C. 헌장이 요구하는 문서 보강 (`.specify/memory/constitution.md` TODO)

- [ ] C1. **에셋 예산 v0.1** 문서를 `Docs/`에 추가(원칙 II 판정 기준). 없으면 헌장 "근거 문서" 표에 위치를 기재.
- [ ] C2. **SDD v0.4~v0.7**을 `Docs/`에 추가(선행 조항 인용용). 현재 003(§3.2 진영), 004(§4.2 손 규칙)는 v0.8에 발췌된 문장만 근거로 씀.
- [ ] C3. 헌장 상단 Sync Impact Report 주석은 검토용 임시 내용 → git 붙일 때 삭제.

## D. 확정 대기 수치 튠 (SDD §9 "구현 후 튜닝") — 전부 JSON, 코드 수정 없음

- [ ] D1. `StreamingAssets/Suspicion/suspicion_rules.json`: §2.3 상승 7종(조사 +2, 제한구역 +2, 증거 목격 +1/+2, 밤 이동 +1, 달리기 +1, 소음 +1), 감소 주기, 전파 거리 3m/지연 4s, 밤 배율 0.7, 이월 계수 0.5/최소 5, 소음 반경 4/8/14, `shove_failed` 3, 발견 개인 +3, 인카운터 샘플 사건 2개.
- [ ] D2. `NpcTypes/npc_types.json`: 밀고자 전달 지연 8s·도착 거리 1.5, 경계자 감지 5m/소리 10m/쿨다운 5s.
- [ ] D3. `Subdue/subdue_rules.json`: 밀치기 실패 확률 0.35, 운반 속도 배율 0.45, 사거리 1.8/2.0, 후면 부채꼴 120°.
- [ ] D4. `Report/report_rules.json`: 최소 도보 6s, 신고 동작 3s, 끊기 4s, 도구 요구 false, 사거리 2.0.
- [ ] D5. `Vehicle/vehicle_params.json`: 속도·가속·조향·서스펜션(주행 감각).
- [ ] D6. `Encounter/encounters.json`: 유형 비율 30/25/20/25(§6.3), 런당 2~4, 간격 20s, 도보 0.35/차량 0.7 확률, 선택 타임아웃 12s.
- [ ] D7. 확정되면 해당 항목 `status`를 `"확정대기"` → `"확정"`으로 바꾸고 SDD에도 반영(헌장 원칙 I).

## E. 아직 스펙이 없는 기능 — `/speckit-specify`부터

### E-1. 기반 기능 (PrototypePlan 근거)
- [ ] E1. **저장/불러오기** (PrototypePlan 5단계): 002 이월 값(`CarryOverSnapshot`), 005 끊긴 전화기, 007 플래그·이력(`Ledger.Export`)을 받아 보존. "재시작 후 진행 복구" 완료 기준.
- [ ] E2. **미션 목표·HUD** (PrototypePlan 4단계): 001 목적지 마커를 미션 목표로 확장, 도착 시 보상·완료 표시.
- [ ] E3. **Timeline 컷신** (PrototypePlan 5단계).
- [ ] E4. **정식 입력 바인딩**: 지금 테스트 콘솔 키(F 제압, G 내려놓기, K 도구, I 조사, L 라벨, X/1/2/S/Z, V, N/R/T/[ ])는 `Keyboard.current` 직접 읽기. `InputSystem_Actions.inputactions`에 액션 추가(에디터 수동) 후 코드 교체. `Interact`의 Hold 인터랙션 재검토.
- [ ] E5. **UI 정리**: 레거시 Text → TextMeshPro 전환, C 유형 선택지 위젯을 001 대화 UI에 통합, 디버그 패널과 게임 HUD 분리.
- [ ] E6. **NPC 경로 탐색**: 003·005 NPC 이동이 직선(`NpcMover`, 장애물 회피 없음). NavMesh(`com.unity.ai.navigation` 이미 설치) 도입, 씬 베이크 절차 포함.
- [ ] E7. **타이틀·옵션 메뉴**: Vertical Slice 이후.

### E-2. 게임 규칙 기능 (SDD 확정 조항 근거)
- [ ] E8. **조사 상호작용**("E 길게", §2.3): 지금은 003 `NpcTypeSystem.Investigate(actor)` 외부 호출만. 증거·조사 대상 시스템(v0.4~v0.7 근거 필요 → C2 선행).
- [ ] E9. **증거 소지·손 규칙 본체**(v0.4 §6.3): 004는 검증용 상자만. 증거 물건, 한 손/두 손 증거, 목격 사건(`evidence_seen_*`) 발생 주체.
- [ ] E10. **도구 5종**(v0.6 §3.1, `[제안]`→확정 대기): 005 `PlayerToolkit.HasCuttingTool` 불리언을 실제 도구로.
- [ ] E11. **위장(작업복)**: 002 `PlayerDisguise.IsDisguised` 불리언을 착용 시스템·효과량으로.
- [ ] E12. **시계·출항 시각·봉쇄 연출**(§2.5, v0.6 §2): 002 `DepartureBlocked` 상태를 배·시계 UI로. 낮/밤 자동 전환.
- [ ] E13. **§2.4 구간 효과**: 순찰 증가, 구역 폐쇄, 감시자 따라붙음, 야간 통행 불가 — 각 구간 전환 이벤트를 소비하는 맵·NPC 스케줄 기능.
- [ ] E14. **차량 소음→의심**(§2.3 차량 소음): 006 `VehicleState.EngineOn/IsMoving`을 읽어 002 `noise` 사건 발생. 검문(§6.2 B)의 차량 정지 강제.
- [ ] E15. **코옵 동기화**(§8): 네트워크 전송. PrototypePlan이 Vertical Slice 이후로 둔 항목. 001~007은 개체 단위 독립성만 확보됨.
- [ ] E16. **문서 시스템**(장부·취소선, §6.5 회수 대상 `document`): 007 `CallbacksFor` 조회 결과를 실제 문서 변화로.

## F. v0.9 콘텐츠 (SDD v0.9 나온 뒤)

- [ ] F1. 004 염전섬 분 단위 대본 → 첫 서사 콘텐츠 스펙(001 대화 JSON + 007 인카운터 + 005 신고 지점 배치).
- [ ] F2. 인카운터 개별 20~30개 → `encounters.json`에 `sample:false`로 추가(코드 변경 없음). 단가 합계 HUD 확인.
- [ ] F3. 회수 테이블 전체 → `callbacks[]`.
- [ ] F4. 밀고자 식별 단서(§9) → 003 확장.
- [ ] F5. 인카운터 샘플 9개(`sample:true`) 제거 또는 콘텐츠로 승격.

## G. 프로젝트 관리

- [ ] G1. **git 초기화** + Unity용 `.gitignore`(Library/, Temp/, Logs/, obj/, *.csproj, *.sln, UserSettings/). `.claude/settings.local.json` 제외. 헌장 Sync Impact Report 주석 제거 후 첫 커밋.
- [ ] G2. `Docs/ThirdPartyAssets.md`는 비어 있음 — 외부 에셋 도입 시 기록(헌장 원칙 II).
- [ ] G3. 빈 폴더 `Assets/Scripts/Mission`, `Save` — E1·E2 구현 시 사용.
- [ ] G4. 대소문자 폴더 `Assets/Scripts/NPC`(기존)와 계획서의 `Npc` 표기 통일(문서는 이미 `NPC`로 갱신됨).
- [ ] G5. Unity 프로젝트 이름 불일치: 폴더 `1004-project-master`, 솔루션 `Island Project.slnx`, SDD `PROJECT 1028`, README `1004-project`. 하나로 정리.

---

## 실행 순서 요약
A(검증) → B(해석 확정) → G1(git) → D(튠) → E1·E4·E6(저장·입력·경로) → E8~E14(규칙 확장) → F(v0.9 콘텐츠) → E15(코옵)
