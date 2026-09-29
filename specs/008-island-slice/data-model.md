# Data Model: 염전섬 그레이박스 수직 슬라이스

## SliceRules (JSON) — contracts/slice-rules-schema.json
| 블록 | 필드 | 상태 |
|---|---|---|
| clock | nightAtSeconds > 0, departureAtSeconds > nightAt, nextBoatDelaySeconds > 0 | 확정대기 (v0.6 §2 원문 없음) |
| evidence | investigateSeconds > 0, twoHanded(bool), seenCooldownSeconds > 0, oneHandEventId, twoHandEventId | 확정대기 (§2.3) |
| restricted | cooldownSeconds ≥ 0, eventId | 확정대기 (§2.3) |
| dock | boardRadius > 0, requireEvidenceForSuccess(bool) | 확정 (§2.5 봉쇄), 성공 조건은 해석 |
| vehicle | allowEvidenceAboard(bool) | 가정 |
| objectives[] | id, text, markerFacilityId, condition ∈ {evidence_picked, at_dock_with_evidence} | 기반 |
| hints[] | 문자열 ≥ 1, hintSeconds > 0 | 기반 |
| feedback | heartbeat {intervalsByStage[4] > 0, zoneMultiplier[4] > 0, volume 0~1}, pulse {seconds, alpha 0~1}, tint {alphaByZone[4] 0~1, nightAlpha}, hitstop {seconds ≥ 0}, shake {amplitude ≥ 0, seconds ≥ 0}, bigText {seconds > 0} | 기반 |

## IslandLayout (JSON) — contracts/island-layout-schema.json
- facilities[]: `{ id, kind ∈ {dock, boat, road, warehouse, office, house, saltfield, phone, evidence, vehicle, trigger, spawn}, position{x,y,z}, size{x,y,z}, rotationY, label }`
- npcs[]: `{ npcId(003 배정 존재), displayName, position, facing }`
- restrictedZones[]: `{ id, center, size }`
- dockFacilityId, warehouseFacilityId(마커 대상), evidenceFacilityId

## 순수 로직
- `RunClock`: `Elapsed`, `Phase {Day, Night, DepartureOpen, Lockdown, Ended}`, `Tick(dt, bool blocked) → RunPhase? transition`, `IsNight`, `IsDepartureOpen`, `NextBoatAt`, `RemainingToDeparture`, `RemainingToNextBoat`, `End()`.
- `ObjectiveTracker(steps)`: `Current`, `Index`, `IsComplete`, `Advance()`, `Satisfy(condition) → bool advanced`.
- `DepartureRule.Evaluate(elapsed, departureAt, nextBoatAt, blocked, hasEvidence, allAtDock, requireEvidence) → DepartureVerdict { bool CanBoard; string Reason; RunOutcome Outcome }`; `static RunOutcome? Forced(elapsed, nextBoatAt, blocked)`.
- `HeartbeatRule.IntervalFor(maxStage 0..3, zone 0..3, intervalsByStage, zoneMultiplier) → seconds`.
- `enum RunOutcome { None, Success, LeftWithoutEvidence, ForcedDeparture }`

## 런타임
- `RunDirector`: `Rules`, `Clock`, `Objectives`, `Outcome`, `Stats`(신고 완료·제압·인카운터·선택·플래그), `EndRun(outcome)`, `Restart()`.
- `DockBoat`(IInteractable "배 타기"): 부두 반경, 확인 단계(증거 없이 출도 시 2회 E).
- `RestrictedZone`: 중심·크기·플레이어별 쿨다운.
- `EvidenceItem`(IInteractable "조사 (E 길게)"): `CutProgress`, 완료 시 `CarriableObject.Locked=false`, 자신 비활성.
- `EvidenceWatcher`(플레이어): 쿨다운 타이머.
- `FeedbackDirector`: 이벤트 → `ScreenFx`/`ProceduralAudio`/`OrbitCamera.Shake`/HUD.
- `ScreenFx`: `Pulse(color, seconds, alpha)`, `SetTint(alpha)`, `BigText(text, seconds)`, `HitStop(seconds)`.
- `ProceduralAudio`: `Tone(freq, seconds, wave, volume)`, `Noise(seconds, volume)`, `SetHeartbeat(intervalSeconds or 0)`, `SetDrone(bool)`.
- `EyeIndicator`: P1을 보는 NPC 수.
- `ResultScreen`: 결과·통계·R.

## 이벤트 (SliceEvents)
- `OnObjectiveAdvanced(step)`, `OnRunPhaseChanged(RunPhase)`, `OnDepartureDenied(string reason)`, `OnRunEnded(RunOutcome, RunStats)`, `OnEvidencePicked(PlayerEntity)`, `OnRestrictedEntered(PlayerEntity, string zoneId)`
