# 제작 규격 조정

제작을 위임한 원본 작업 `01a0c6d5-75e8-7a32-9993-499d9b2001c9`가 2026-09-22에 전달한 후속 지시를 적용합니다. 최초 README와 JSON은 인계 근거로 수정하지 않고 보존합니다.

- 1024px 고정은 제작을 위임한 작업에서 정한 목표였습니다. 내장 도구가 반환한 **1254×1254px 네이티브 RGBA PNG도 정식 인계 원본 규격으로 허용**합니다.
- 중앙 76%는 가독성과 여백을 판단하는 지침입니다. 실제 잘림, 형태 누락, 불투명 배경은 실패로 처리합니다.
- 알파를 그대로 보존하며 크로마키, 배경 제거, 프로그램을 통한 원본 크기 조정을 수행하지 않습니다.
- 실제 반환 크기와 원본 해시를 기록합니다. 도구가 모델 근거를 반환하지 않았으므로 모델은 `unknown`, 자산 승인 상태는 `candidate`입니다.
- Unity 표시 크기와 최종 임포트 설정은 후속 UI 통합 작업에서 결정합니다.

기존 비동기 확인 질문은 크기 조정을 진행하기 위한 것이었습니다. 이 조정으로 원본 크기가 허용되었으므로 그 답을 기다리지 않고 원본 제작을 계속합니다. 도구 외 편집을 허용받았다는 의미는 아닙니다.

## English

Apply the follow-up instruction issued on 2026-09-22 by the source task `01a0c6d5-75e8-7a32-9993-499d9b2001c9`. Preserve the original handoff README and JSON unchanged as evidence.

- The initial 1024px target was chosen by the delegating task. **Native 1254×1254 RGBA PNG output is also an accepted handoff master.**
- The central 76% is a readability and padding guideline. Actual clipping, missing forms, and opaque backgrounds fail review.
- Preserve native alpha. Do not chroma-key, remove backgrounds, or programmatically resize the originals.
- Record actual dimensions and source hashes. Since the tool returns no verifiable model identity, keep `model: unknown` and `approval: candidate`.
- The later UI integration task determines Unity display sizing and final importer choices.

The pending question concerned permission for resizing. This revision accepts the original output size, so production continues without waiting for that answer. It does not authorize non-tool image editing.
