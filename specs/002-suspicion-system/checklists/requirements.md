# Specification Quality Checklist: 의심 시스템과 시간대 위협 (Suspicion System)

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
- [x] 각 요구사항에 SDD 조항 인용 (원칙 I) — §2.1~§2.7, §7, §8.1
- [x] `[제안]` 의존 항목 확정 대기 표시 (원칙 I) — §2.3 상승 수치, v0.6 §3.1 위장
- [x] 신규 에셋 목록과 예산 초과 여부 (원칙 II) — 0개
- [x] 싱글·코옵 동작 각각 기술 (원칙 IV) — 개인 의심 개체별, 섬 의심도 공유
- [x] 독립 검증 가능 (원칙 III) — 테스트 씬 + 임의 사건 발생

## Notes

- 검증 1회차(2026-09-23): 전 항목 통과.
- 검토 메모: 상승량·감소 속도·전파 거리 등 수치는 모두 데이터 파일로 분리되어 스펙에 값이 없다. 이는
  §2.3 `[제안]` 처리 원칙에 따른 의도적 결정이다. `/speckit-clarify`에서 초깃값 확인을 권장한다.
- T043 헌장 최종 점검(2026-09-23): Suspicion 코드의 숫자 리터럴은 범위 상수(0~3, 0~100, 26/51/76)와 표현 파라미터
  (따라가기 거리 2.5, 이동 속도 1.6, 라벨 높이, 말 걸기 쿨다운)만 존재. 의심 상승량·감소·전파·배율·이월 계수는 전부
  suspicion_rules.json. 신규 에셋 0개. PlayFoundation asmdef는 Suspicion을 참조하지 않음(회귀 안전). 브레이스 균형 통과.
- 구현 메모: 001 파일 2개 수정 — RuntimeHud.CreateFixedText 추가, PlayFoundationSceneBuilder 메서드 public 전환·GameObject 반환.
  001 동작 변화 없음.
- 미검증: Unity가 없는 환경에서 작성. 컴파일·EditMode 6묶음·Play 검증(T042)은 집 PC에서 수행.

