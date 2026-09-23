# Specification Quality Checklist: NPC 유형 5종 (NPC Types)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-09-23
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Constitution Gate (v1.1.0 Specify 게이트)

- [x] 기능 분류 명시 (원칙 I) — "게임 규칙 기능"
- [x] 각 요구사항에 SDD 조항 인용 (원칙 I) — §3, §3.1, §3.2, §2.4, §2.6
- [x] `[제안]` 의존 항목 확정 대기 표시 (원칙 I) — 조사 행동(§2.3), 밀고자 식별 단서(§9), 소음 값
- [x] 선행 SDD 인용 시 원문 발췌 (근거 문서 §) — §3.2에 발췌된 v0.4 §3.3 내용만 사용
- [x] 신규 에셋 목록과 예산 초과 여부 (원칙 II) — 0개
- [x] 싱글·코옵 동작 각각 기술 (원칙 IV)
- [x] 폐기 항목 미포함 (원칙 V)

## Notes

- 검증 1회차(2026-09-23): 전 항목 통과.
- 검토 메모: NPC 최소 이동("지정 지점으로 걸어가기")이 필요하다. 001은 NPC를 정지로 두었으므로 002 또는
  003 플랜에서 어느 쪽이 도입할지 결정해야 한다. `/speckit-plan` 시 확인.
- T040 헌장 최종 점검(2026-09-23): NpcTypes 코드에 유형 파라미터(지연·반경·전환 구간) 하드코딩 0건 — 전부 npc_types.json.
  숫자 리터럴은 라벨 오프셋·색만. 신규 에셋 0개. 브레이스 균형 통과. 002 수정은 가산 확장 + 억제 상태 Stop 1회화(동작 의미 불변).
- 구현 메모: 스펙 FR-010의 "002 FR-020 플래그로 구현"은 이중 +1 방지를 위해 003 InformerBehaviour 전담으로 바꿨다(research R-2). 외부 동작 동일.
- 구현 메모: 001 수정 2건 — RuntimeHud 사전 대화 검증을 대화 가능 NPC로 한정, 씬 빌더 `CreateNpc(..., talkable)` 오버로드.
- 미검증: Unity 없는 환경. 컴파일·EditMode 5묶음(+002 회귀)·Play(T039)는 집 PC.

