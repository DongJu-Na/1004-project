# Specification Quality Checklist: 차량 이동 (Vehicle Drive)

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

- [x] 기능 분류 명시 (원칙 I) — "기반 기능", 게임 규칙 미포함 선언
- [x] 각 요구사항에 근거 인용 (원칙 I) — `[PP-2]` `[PP-완료2]`
- [x] PrototypePlan 범위 밖 항목 미포함 — 복수 차량·래그돌 제외
- [x] 신규 에셋 목록과 예산 초과 여부 (원칙 II) — 0개
- [x] 싱글·코옵 동작 각각 기술 (원칙 IV) — 운전석·동승석
- [x] 폐기 항목 미포함 (원칙 V) — 차량 NPC 무해

## Notes

- 검증 1회차(2026-09-23): 전 항목 통과.
- 검토 메모: 동승석은 PrototypePlan에 없는 추가다. 헌장 원칙 IV 충족을 위한 것이며 Assumptions에 기록했다.
- T031 헌장 최종 점검(2026-09-23): `Vehicle` asmdef 참조 = PlayFoundation·InputSystem·UI만(규칙 어셈블리 없음). 코드에 의심·소음·인카운터 식별자 0.
  튠 값은 vehicle_params.json. 신규 에셋 0(Cube 차체 + Cylinder 바퀴). 브레이스 균형 통과. Unity 6 API(linearVelocity·linearDamping) 사용.
- 구현 메모: 001 가산 4건(IsInVehicle, OrbitCamera.SetTarget, FallRespawn/모터의 탑승 중 건너뜀, 탐지기 안내 억제). NPC는 Rigidbody 없는 정적 콜라이더.
- 미검증: Unity 없는 환경. 컴파일·EditMode 4묶음·Play(T030)는 집 PC. 주행 감각(서스펜션·조향 계수)은 JSON 튠 필요 가능.

