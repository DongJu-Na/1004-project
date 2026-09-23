# Research: 제압 시스템

## R-1. 대가 사건을 섬 값에만 적용하는 scope
- **Decision**: 002 `SuspicionEventRule.scope`에 `global` 추가(가산). 대상 NPC 없이 `islandDelta`만 적용. `subdue_witnessed`(40)·
  `unconscious_found_island`(30)이 이 scope. 검증기는 `global`을 허용하고 personalDelta>0이면 경고.
- **Rationale**: FR-015 "목격자 수와 무관하게 제압 1회당 1회", FR-016 "발견자마다 1회" 모두 섬 값만 바뀐다. target scope로 우회하면
  개인 값 0 적용이라는 부자연스러운 경로가 생긴다.

## R-2. 깨어남 → 개인 의심 3 + 신고 시도
- **Decision**: 002 사건 `unconscious_wake`(personal 3, target=깨어난 NPC, actor=제압자, ignoresTimeMultiplier, 확정). 이미 3이라
  단계 전환이 없으면 `SuspicionSystem.ForceReportAttempt(npc, player)`(가산, `SuppressReportAttempt` 존중)로 신고 시도 사건을 낸다.
- **Rationale**: FR-011 "즉시 신고 시도". 값 상승은 표를 통해(FR-018), 이벤트는 002가 소유.

## R-3. 손 상태와 다른 상호작용 차단
- **Decision**: `PlayerEntity.HandsBusy`(가산 bool) — 004 `PlayerHands`가 두 손 점유 시 참. 001 `NpcTalkInteractable.CanInteract`가 이를
  거부 조건에 추가(Edge case "기절자 들고 대화 → 거부"). 이동 잠금(`Lock`)과 별개.

## R-4. 운반 속도·달리기 불가
- **Decision**: 001 `ThirdPersonMotor`에 `SpeedMultiplier`(기본 1)·`SprintAllowed`(기본 true) 가산. `PlayerHands`가 기절자 운반 중
  `carry.speedMultiplier`(JSON, 확정대기)와 `SprintAllowed=false` 적용. 물건(한 손·두 손)은 속도 영향 없음(SDD 미언급).

## R-5. 동작 선택 입력
- **Decision**: 한 키 `F`(테스트 콘솔 매핑, `Keyboard.current`)로 문맥 동작: 물건을 들고 있으면 물건으로 기절, 빈손이면 대상이 나를
  보고 있지 않고 후면 부채꼴 안이면 뒤에서 제압, 아니면 정면 밀치기. 안내 문구에 결정된 동작을 표시("뒤에서 제압 [F]"). 대상은
  카메라 전방 최근접 NPC(`InteractableSelector` 재사용, 사거리 JSON). 정식 입력 액션 바인딩(inputactions 편집)은 에디터 수동 작업이라
  후속.
- **Alternatives**: 동작별 키 3개 — 손 제약과 겹쳐 UX 혼란. 기각.

## R-6. 들기·내려놓기
- **Decision**: 물건과 기절자는 `IInteractable`("들기"/"들어 옮기기", E). 내려놓기는 `G`(콘솔 매핑). 든 것은 플레이어 자식으로
  부착(물건: 오른손 위치, 기절자: 어깨 위치, 콜라이더 비활성). 내려놓으면 플레이어 앞 바닥에 놓는다.

## R-7. 기절자 발견 스캔
- **Decision**: 001 `NpcVision`은 플레이어만 본다. 004 `SubdueSystem`이 각 기절자에 대해 다른 NPC의 `NpcVision`(fov·range·눈 위치)
  파라미터와 `VisionEvaluator` + `Physics.Linecast`로 "보고 있는가"를 직접 계산한다(플레이어 판정과 같은 규칙). 순수 `CostLedger`가
  관찰자별 1회·목격자 제외를 보장한다.

## R-8. 목격 판정 시점
- **Decision**: 제압 **성공 순간**에 `NpcVision.IsSeeing(actor)`가 참인 NPC(대상 제외, 기절 중 제외) 집합 = 목격자. 하나 이상이면
  `subdue_witnessed` 1회, 목격자를 원장에 기록(발견 +30 중복 방지, FR-016). 실패한 밀치기는 목격 대가 없음(SDD 미언급 → 소음만).

## R-9. 기절 시간과 배속
- **Decision**: 순수 `UnconsciousTimer`(min·max에서 주입 난수로 추출, `Tick(dt)`, `IsAwake`). `Time.time` 기반이므로 002 콘솔의
  `Time.timeScale` 배속으로 검증 가능. 운반 중에도 틱(FR-014).

## R-10. 기절 중 비활성화
- **Decision**: `UnconsciousState` 부착 시 `NpcVision`·`NpcMover`·`NpcSuspicionBehaviour`·`InformerBehaviour`·`SentinelBehaviour`를
  `enabled=false`, 캡슐을 90° 눕힘, `NpcTalkInteractable` 비활성. 깨어나면 복구. `NpcVision`이 꺼지므로 002 시야 조건과 003 조사가
  자연히 제외되고, 002 전파는 `PropagatesSuspicion`을 임시 false로 두어 차단(원래 값 저장·복구).

## R-11. 제압 불가 대상
- **Decision**: `SubdueJudgement.Evaluate(npc).CanSubdue == false`면 동작·물리 반응 없이 HUD 안내만. 목격·소음도 없다(아무 일도
  일어나지 않음).
