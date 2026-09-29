# Research: 염전섬 그레이박스 수직 슬라이스

## R-1. "뭘 해야 할지 모르겠다"에 대한 설계 답
- **Decision**: 목표 단계를 데이터로 두고(문구·마커 시설·완료 조건), HUD 상단에 항상 문구·거리·시계를 보여준다. 마커는 001 `DestinationMarker` 재사용. 첫 30초 조작 안내.
- **Rationale**: 목표가 화면에 없으면 규칙이 아무리 많아도 심부름조차 성립하지 않는다. PrototypePlan 4단계와 같은 것.

## R-2. "쥬시가 없다"에 대한 설계 답
- **Decision**: `FeedbackDirector`가 기존 이벤트 20종을 구독해 시각 1개 + 청각 1개 이상으로 매핑한다. 강도·색·간격은 `feedback` JSON. 반응 기본 4종:
  화면 가장자리 붉은 맥동(uGUI 이미지 4장 알파), 전체 색조(구간별), 중앙 큰 문구(0.8초 페이드), 카메라 흔들림(001 `OrbitCamera.Shake` 가산)·히트스톱(`Time.timeScale` 0을 0.1초 실시간).
  소리는 `ProceduralAudio`가 `AudioClip.Create`로 합성(사인·사각·삼각·노이즈, 짧은 감쇠). 심박은 간격 루프, 밤은 55Hz 저음 루프.
- **Rationale**: 규칙이 "보이지 않으면" 플레이어에게는 없는 것이다. 에셋 없이 되는 것부터.
- **Alternatives**: 오디오 에셋 — 원칙 II·현재 단계에 맞지 않음. 후속.

## R-3. 시계·출항·봉쇄
- **Decision**: 순수 `RunClock`: `Elapsed`, `Phase {Day, Night, DepartureOpen, Lockdown, Ended}`, `Tick(dt, blocked)`; 밤 전환·출항 시각·다음 배 시각은 JSON. 순수 `DepartureRule.Evaluate(elapsed, rules, blocked, hasEvidence, allAtDock)` → `{CanBoard, Reason, Outcome(Success|LeftWithoutEvidence|Forced|None)}`. 봉쇄 상태로 다음 배 시각 도달 = `Forced`(강제 출도, 해석).
- **Rationale**: §2.5 "시계를 보며 의심을 관리". 값은 확정대기.

## R-4. 증거·조사·제한 구역
- **Decision**: 증거 = 004 `CarriableObject`(IsEvidence, 두 손, Locked=true) + 008 `EvidenceItem`(IInteractable "조사", 005 `CutProgress` 재사용으로 E 길게 진행; 시작 시 003 `Investigate` 1회; 완료 시 Locked=false). `EvidenceWatcher`(플레이어)가 증거를 든 상태에서 `NpcVision.IsSeeing`이면 쿨다운마다 `evidence_seen_one_hand/two_hands`. `RestrictedZone`(박스 범위 폴링)이 진입 시 `enter_restricted_area`(002 표의 시야 조건이 감시자 앞에서만 적용되게 함).
- **Rationale**: 규칙을 새로 쓰지 않고 002 표의 기존 사건 3개를 발생시키는 주체만 만든다. §4.2 두 손 규칙은 004가 그대로 적용.

## R-5. 섬 배치 데이터
- **Decision**: `island_layout.json`에 시설(kind·위치·크기·회전)·NPC(npcId·표시명·위치·방향)·제한 구역·트리거·증거·스폰을 둔다. 에디터 빌더가 읽어 프리미티브로 생성. 001~007 빌더의 정적 메서드(플레이어·NPC·트럭·전화기·관리소·트리거)를 재사용하고, 새 시설(부두·배·창고·집·염전)만 이 기능이 만든다.
- **Rationale**: FR-019. 레벨 조정을 JSON으로.

## R-6. 결과·재시작
- **Decision**: `ResultScreen`이 중앙 패널에 결과 종류·통계(002 섬 값·구간, 005 완료 수, 004 제압 수, 007 발생 수·선택·플래그)를 보여주고 R로 `SceneManager.LoadScene(활성 씬)`. 002 이월 값·007 이력은 표시만.

## R-7. 코옵 출항
- **Decision**: `allAtDock` = 모든 `PlayerEntity`가 부두 반경 안. 테스트 씬은 P2를 부두에 세운다.

## R-8. 증거를 든 채 탑승
- **Decision**: 006 `VehicleSeats.AllowBusyHands` 정적 플래그(가산)를 `slice_rules.vehicle.allowEvidenceAboard`로 켠다. 탑승 중 손 상태 유지. 스펙 Assumptions.

## R-9. 재시작과 정적 상태
- **Decision**: 정적 이벤트(각 `*Events`)에 구독한 컴포넌트는 `OnDisable`에서 반드시 해제(씬 리로드 누수 방지). `TimeOfDay`·시스템 싱글톤은 씬 오브젝트라 리로드로 초기화된다. 001 `PlayerEntity.All` 등 정적 리스트는 OnDisable로 비워진다.
