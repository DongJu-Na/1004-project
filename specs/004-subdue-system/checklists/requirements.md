# Specification Quality Checklist: 제압 시스템 (Subdue System)

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
- [x] 각 요구사항에 SDD 조항 인용 (원칙 I) — §4.1~§4.5, §3.2, §8.3, §1
- [x] `[제안]` 의존 항목 확정 대기 표시 (원칙 I) — 실패 확률, 소음 값, 기절 발견 +3
- [x] 선행 SDD 인용 시 원문 발췌 (근거 문서 §) — §4.2 발췌 손 규칙만 사용
- [x] 신규 에셋 목록과 예산 초과 여부 (원칙 II) — 0개
- [x] 싱글·코옵 동작 각각 기술 (원칙 IV) — §8.3 두 명 운반
- [x] 폐기 항목 미포함 (원칙 V) — 무기 없음, 비살상만, 물건은 무기 속성 없음

## Notes

- 검증 1회차(2026-09-23): 전 항목 통과.
- 검토 메모: "정면 밀치기 실패 시 개인 의심 3"은 SDD에 없는 합리적 해석이다. `/speckit-clarify`에서 확인 권장.
- T045 헌장 최종 점검(2026-09-23): Subdue 코드에 무기·체력·살상 식별자 0건(금지 선언 주석만). 수치는 subdue_rules.json·002 사건 표.
  코드 숫자는 손 위치 오프셋·눕힘 각도 등 표현 파라미터만. 신규 에셋 0. 브레이스 균형 통과.
- 구현 메모: 001 가산 4건(모터 배율·달리기 허용, HandsBusy, 탐지기 비활성 건너뜀, HUD 보조 안내), 002 가산 3건(global scope, ForceReportAttempt, 사건 7개), 003 데이터에 sub_* 배정 4건.
- 미검증: Unity 없는 환경. 컴파일·EditMode 6묶음·Play(T044)는 집 PC.

