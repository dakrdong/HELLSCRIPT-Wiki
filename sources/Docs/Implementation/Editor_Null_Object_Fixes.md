# HELLSCRIPT 편집기 자리표시 객체 오류 수정

작성일: 2026-09-15

## 배경

Unity 편집기에서 `GetComponent` 계열 호출이 없는 부품을 찾으면 진짜 `null`이 아니라 오류 문구를 담은 자리표시 객체를 돌려준다. 이 객체는 Unity가 재정의한 `==`·`!=`에서만 `null`로 취급되고, C#의 `??`·`?.`·`is null`에서는 살아 있는 참조로 보인다. 그래서 `GetComponent<T>() ?? AddComponent<T>()`는 편집기에서 `AddComponent`로 넘어가지 않고, 다음 멤버 접근에서 `MissingComponentException`이 난다. 플레이어 빌드에서는 `GetComponent`가 진짜 `null`을 돌려주므로 같은 코드가 정상 동작한다. 이 차이 때문에 개발 빌드 스모크가 모두 통과한 화면이 편집기 Play 모드에서만 예외를 낸다.

첫 사례는 커스텀 그래픽의 `CanvasRenderer` 누락이었고 [플레이 화면 콘텐츠 독 기록](Play_Content_Dock.md)의 "편집기에서 드러난 기존 문제와 수정" 절에 정리했다. 이 문서는 두 번째 사례와 재발 방지 검사를 다룬다.

## 증상

플레이 화면 오른쪽 위의 설정 버튼을 누르면 다음 예외가 나고 설정 창이 열리지 않았다.

```
MissingComponentException: There is no 'CanvasGroup' attached to the "HELLSCRIPT UI" game object, but a script is trying to access it.
  Hellscript.GameUI.ShowCommonPanel (System.Boolean help) (at Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ScreenSettings.cs:33)
```

## 원인과 수정

| 위치 | 이전 | 수정 |
|---|---|---|
| `GameUI.ScreenSettings.cs:33` 설정 창 열기 | 모든 캔버스에 `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()`로 입력 차단용 그룹을 얻고 바로 `interactable`을 읽었다. 편집기에서는 자리표시 객체가 반환되어 첫 번째 캔버스에서 예외가 났다. | `TryGetComponent(out CanvasGroup group)`가 거짓이면 `AddComponent`한다. |
| `GameUI.Presets.cs:73` 프리셋 이름 입력 | 입력 모듈의 `inputOverride`를 `GetComponent<HellscriptTextInput>() ?? AddComponent<HellscriptTextInput>()`로 채웠다. 편집기에서는 자리표시 객체가 대입되어 실제 입력 대체가 적용되지 않았다. 예외는 없었다. | 같은 방식으로 `TryGetComponent` 뒤 `AddComponent`한다. |

`TryGetComponent`는 없는 부품에 대해 거짓과 진짜 `null`을 돌려주므로 편집기와 플레이어에서 같은 경로를 탄다.

## 재발 방지 검사

Edit Mode 검사 `EditorNullPatternTests`는 `Assets/HELLSCRIPT/Runtime`과 `Assets/HELLSCRIPT/Editor`의 C# 소스에서 `GetComponent…(…)` 뒤에 `??` 또는 `?.`가 오는 줄을 찾아 실패시킨다. 파일 이름에 `Smoke`가 든 런타임 스모크는 플레이어에서만 실행되므로 제외한다. `GraphicComponentTests`는 모든 `Graphic` 파생 클래스가 `CanvasRenderer`를 함께 만드는지 확인한다.

## 검증

사용자 편집기가 프로젝트를 열고 있어 원본 프로젝트에서는 배치 실행을 하지 않았다. 검증은 프로젝트 복제본에서 했고, 결과는 [증거 폴더](EditorNullObjectEvidence/)에 있다.

| 항목 | 결과 |
|---|---|
| 수정 전 사용자 편집기 로그 | 설정 버튼을 누를 때마다 `CanvasGroup` 예외가 기록됐다. [표본 4회](EditorNullObjectEvidence/editor-exception-sample.txt) |
| 수정 전 복제본 `EditorNullPatternTests` | [실패](EditorNullObjectEvidence/nullpattern-test-before.xml). `GameUI.Presets.cs:73`과 `GameUI.ScreenSettings.cs:33` 두 줄을 나열했다. |
| 수정 후 복제본 Edit Mode 검사 | `EditorNullPatternTests`, `GraphicComponentTests`, 설정 개편·프리셋 저장·화면 설정 검사 [64개 모두 통과](EditorNullObjectEvidence/nullpattern-test-after.xml) |
| 수정 후 복제본 편집기 재현 | 사용자와 같은 Enter Play Mode 설정으로 Play 모드에 들어가 제목 화면에서 설정 창을 두 번 열고 닫고, 마을에서 다시 열고 닫았다. 예외 없이 `CommonPanelOpen`이 참이 됐다. 프리셋 이름 대화상자를 열면 입력 모듈의 `inputOverride`가 실제 `HellscriptTextInput` 부품이 됐다. [기록](EditorNullObjectEvidence/editor-repro-settings-after.txt) |

수정 전 상태의 설정 창 재현은 복제본에서 따로 돌리지 않았고 사용자 로그를 근거로 삼았다. 열려 있는 편집기는 파일 변경을 감지해 다시 컴파일하므로, 컴파일이 끝난 뒤 Play 모드에서 설정 버튼을 누르면 설정 창이 열려야 한다. 실기기에서는 확인하지 않았다.
