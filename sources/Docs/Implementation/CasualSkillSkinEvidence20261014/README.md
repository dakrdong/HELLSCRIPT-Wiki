# 스킬 창 스킨 검증 근거

2026-10-14 · [English](README.en.md) · [파일럿 결과](../Casual_Skill_Window_Skin_20261014.md)

실제 Mac 게임의 before/after, 실제 Chrome 시안 캡처, 관련 회귀 결과를 보존한다. 이미지 합성판은 검토용 축소본이며 `before/`와 `after/`의 PNG 원본은 수정하지 않았다.

- `PASSED.txt`, `RESUME_PASSED.txt`, `native-receipt.json`: 최종 실제 플레이어, 별도 프로세스 재진입, 생성 바이너리와 소스 근거.
- `screenshots.json`: 직업 3개 × KO/EN × 7크기 × active/passive, 총 84장의 크기·SHA256. 원자료는 작업의 `Builds/Validation/CasualSkillSkin/native`에 있다. 대표 원본 10장은 `after/`에 포함했다.
- `before-after.png`, `viewport-contact-sheet.png`: 실제 캡처 비교판. before는 기존 검증된 빌드의 화면이며 `before-provenance.json`에 기준과 동일한 Assets/Packages/ProjectSettings tree를 기록했다.
- `claude-*.jpg`, `design-decisions.json`: Chrome에서 시안의 탭·패시브·스크롤·회수/재투자를 확인한 근거. 본문 400을 우선하라는 사용자 결정과 실제 투명 배경 CSS 차이를 기록한다.
- `editmode-provenance.json`, XML 두 개: 마지막 관련 64개와 직전 실행에서 재사용한 38개, 서로 다른 관련 검사 총 102개 통과. `disabled-shield-editmode.xml`도 중간 수정 근거로 남긴다.
- `final-*.log`, `font-validation.json`: golden 41개 생성 일치, JS/C# 각각 55,803개 검사, look/font/UI 정적 검사. 전체 프로젝트 EditMode를 실행했다는 뜻이 아니다.
- `raw-layout-differences.json`, `final-golden-diff.json`, `geometry-diff.json`: 새 색·선·모서리로 인해 layout/scenario raw 차이가 3,600개다. **raw diff=0 / styles-only 조건은 충족하지 않았다.** 기능 문서·viewmodel·interaction은 기준과 byte identical이다. 승인된 색·선·모서리 기준으로 raw 외형 차이를 수용했으며, 자식 텍스트/장식 치수 예외는 `acceptance-review.json`에 별도로 보고한다.
- `native-attempts.json`, `failures/`: 고친 실패 6회의 기록. 이전 바이너리를 복제하지 않고 마지막 플레이어 하나를 유지했다.
- `native-binary-manifest.json`: 앱 내부 323파일의 SHA256과 bytes. 앱 자체는 Git에 포함하지 않았다.
- `final-scope-isolation.json`, `restored-editor-side-effects-final.json`: 보호 경로 차이 0, 원본 미커밋 아트 문서 해시 유지, 작업 체크아웃의 Editor 부수 설정 복원.
- `artifact-lifecycle.json`: 작업 공간·단일 캐시·플레이어·원자료 실측, 보존 사유와 다음 검토일. APFS 독점 점유/회수량으로 해석하지 않는다. 미통합 소스와 검토 의존 산출물은 보존 중이며, 영구 삭제를 실행하지 않았다.

재현은 프로젝트의 Unity 6000.6.0f1과 기존 CLI/빌더를 사용한다. 정확한 빌드·검사·실행 명령은 lifecycle에 있다. 네이티브 검사는 반드시 `-force-sound-off`와 작업 전용 `-hellscriptSavePath` **디렉터리**를 사용한다. 재진입은 같은 전용 세이브·출력에 `-hellscriptSkillTreeResume`를 더해 실행한다. 웹 빌드, 원격 push, 다른 UI 확장, 실사용자 세이브 검사는 이 결과에 포함되지 않는다.

남은 차이: 실제 라이브 캔버스의 더 큰 글자, Chrome에서 무효인 패널 배경 CSS, 일부 uGUI 모서리/페이드/글리프/안티앨리어싱. 새 캐주얼 잠금판의 별도 24개 브라우저 기준 렌더는 생성하지 않았다. CPU/GC는 미측정이며 실기기 접근성·WebGL·main 통합 후 전체 검사는 후속 범위다.
