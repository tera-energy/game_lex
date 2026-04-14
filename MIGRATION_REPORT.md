# Firebase → Supabase 마이그레이션 보고서

**프로젝트:** PerplexBrainbow (`com.blazar.lex`)
**작업 기간:** 2026-03-19 ~ 2026-03-27 (약 2주)
**대상 브랜치:** `supabase-migration`

---

## 1. 왜 바꿨나

**비용 구조**
Firebase Realtime Database는 무료 티어에서 동시 접속 수와 저장 용량 제한이 있다. 사용자가 늘어날수록 Blaze Plan 전환이 불가피한데, 모바일 게임 특성상 트래픽 예측이 어렵다. Supabase는 PostgreSQL 기반으로 비용 구조가 예측 가능하고 무료 티어의 실질적 한도가 더 넓다.

**SDK 의존성 크기**
Firebase SDK는 Unity 환경에서 AAR 파일만 80개 이상을 요구한다. 빌드 크기, 빌드 시간, 의존성 충돌 위험이 모두 높다. 이 프로젝트에서 실제로 쓰는 Firebase 기능은 인증과 단순 데이터 저장 두 가지뿐이었고, Realtime Database의 실시간 동기화는 사용하지 않았다. 기능 범위 대비 유지보수 비용이 과도하다고 판단했다.

---

## 2. 무엇을 어떻게 바꿨나

### 인증 (AuthManager.cs)

| 항목 | 변경 전 (Firebase) | 변경 후 (Supabase) |
|------|-------------------|-------------------|
| 초기화 | `CheckAndFixDependenciesAsync()` 비동기 대기 | 초기화 없음 — REST 방식이므로 즉시 `IsReady = true` |
| 로그인 | Firebase Auth SDK Task 콜백 | `POST /auth/v1/token` REST 호출 (코루틴) |
| 세션 유지 | Firebase SDK 자동 관리 | refresh_token → PlayerPrefs 저장, 앱 재시작 시 수동 갱신 |
| 메서드 이름 | `zSetFirebase()`, `IsFirebaseReady` | `zInitialize()`, `IsReady` |

초기화 대기 로직이 사라지면서 "Firebase 준비 전 호출" 타이밍 이슈가 원천 차단됐다. Google Sign-In 플러그인은 그대로 유지하고, 플러그인이 반환한 idToken만 Supabase로 전달하는 방식을 택했다.

### 데이터베이스 (DatabaseManager.cs)

| 항목 | 변경 전 (Firebase) | 변경 후 (Supabase) |
|------|-------------------|-------------------|
| DB 방식 | Realtime Database (JSON 트리) | PostgreSQL (테이블 구조) |
| 호출 방식 | SDK `SetValueAsync()` / `GetValueAsync()` Task | `UnityWebRequest` 코루틴 |
| JSON 처리 | Firebase SDK가 자동 직렬화 | `JsonUtility.FromJson<T>()` 수동 파싱 |

### HTTP 클라이언트 (SupabaseClient.cs, 신규)

외부 Supabase Unity 패키지를 쓰지 않고 `UnityWebRequest`만으로 GET / POST / PATCH / UPSERT / DELETE 래퍼를 직접 구현했다. 서드파티 Supabase 패키지는 아직 성숙도가 낮고, 외부 의존성이 추가될수록 Unity 버전 업그레이드와 플랫폼 빌드 설정 리스크가 커지기 때문이다.

### Android 의존성 정리

Firebase 관련 AAR 80개 전량 제거, Google Sign-In에 필요한 패키지 10개만 유지. `google-services.json`도 제거했다 (webClientId는 코드에 직접 설정).

---

## 3. 잘 처리된 부분

- **에디터 더미 유저 분기**: `#if UNITY_EDITOR` 블록에서 DB 호출 없이 로컬 더미 유저를 자동 생성한다. 네트워크 없이 게임 로직을 반복 검증할 수 있어 개발 생산성이 높다.
- **코루틴 통일**: Firebase의 Task/콜백 패턴을 Unity 코루틴 + `WaitUntil(() => isDone)` 패턴으로 통일했다. UnityWebRequest가 코루틴과 함께 동작하도록 설계된 만큼, Unity 코드와의 정합성이 더 높다.
- **인증 체계 실질화**: Firebase 익명 Auth에 의존하던 구조에서 Google/Apple 소셜 로그인 기반의 실질적인 인증 체계로 전환됐다.

---

## 4. 더 손봐야 할 것

### P0 — 스토어 제출 전 필수

**Supabase AnonKey 하드코딩 제거**
`SupabaseClient.cs` 상단에 AnonKey가 상수로 하드코딩되어 있다. APK 리버싱으로 노출 가능한 구조다. ScriptableObject나 빌드 시 환경 변수 주입 방식으로 분리해야 한다.

**RLS(Row Level Security) 정책 설정**
현재 anon key로 모든 테이블에 접근 가능한 상태다. 최소한 "자신의 데이터만 읽기/쓰기" 수준의 RLS 정책을 Supabase 대시보드에서 설정해야 한다. 랭킹 조회는 전체 읽기를 허용하되, 점수 쓰기는 인증된 본인만 가능해야 한다.

**refresh_token 평문 저장 문제**
PlayerPrefs는 모바일에서 평문으로 저장된다. refresh_token이 탈취되면 계정 탈취로 이어질 수 있다. Android Keystore / iOS Keychain 래퍼 적용, 또는 최소한 AES 암호화 적용이 필요하다.

### P1 — 통합 테스트

실기기에서 전체 플로우 검증이 필요하다. Google 로그인 → Supabase 세션 발급 → 점수 저장 → 랭킹 조회 전체 흐름, 그리고 랭킹 화면(상단 리스트 + 하단 고정 박스)을 실기기에서 확인해야 한다. 에뮬레이터에서 안 잡히는 네트워크 타임아웃, UI 이슈가 실기기에서 나올 수 있다.

### P2 — 코드 품질 (낮은 우선순위, 기능에 영향 없음)

**SupabaseClient Upsert 헤더 중복**
`SetHeaders()` 공통 메서드가 있음에도 Upsert에서 헤더를 직접 기술하고 있다. 향후 헤더 스펙 변경 시 누락 실수가 생길 수 있다. Upsert도 `SetHeaders()`를 사용하도록 통일하면 된다. 수정 난이도가 낮다.

**JsonUtility 한계**
`JsonUtility`는 중첩 객체, nullable 필드를 지원하지 않아 현재 래퍼 클래스로 우회 중이다. 응답 스펙이 복잡해지면 파싱 코드가 비대해진다. `Newtonsoft.Json (Json.NET for Unity)`을 도입하면 해결되며, Unity 2021+ 패키지 매니저에서 공식 지원한다.

### P3 — 기술 부채 (스토어 제출 이후)

**user_scores 테이블 리팩토링**
현재 `score1~score5` 고정 컬럼 구조다. 게임 모드가 늘어나거나 기록 수를 바꾸려면 스키마 변경이 필요하다. `game_sessions(user_id, game_mode, score, played_at)` 방식으로 전환하면 유연성과 통계 가능성이 모두 높아진다. 스키마 마이그레이션과 클라이언트 파싱 변경이 동반되므로 별도 Phase로 진행한다.

---

## 요약

| 구분 | 항목 | 시점 |
|------|------|------|
| P0 | AnonKey 분리, RLS 설정, refresh_token 암호화 | 스토어 제출 전 |
| P1 | 실기기 통합 테스트 | 스토어 제출 전 |
| P2 | Upsert 헤더 통일, Json.NET 도입 | 여유 시 |
| P3 | user_scores → game_sessions 리팩토링 | 스토어 제출 이후 |
