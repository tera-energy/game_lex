# PerplexBrainbow (game_lex)

Unity 기반 모바일 단어 퍼즐 게임. Android / iOS 지원.

- Package name: `com.blazar.lex`
- Engine: Unity (2021.x 이상 권장)

---

## 기술 스택

| 분류 | 내용 |
|------|------|
| Engine | Unity (Android / iOS) |
| Backend | [Supabase](https://supabase.com) — Auth REST API + PostgREST |
| HTTP 클라이언트 | `UnityWebRequest` 기반 자체 구현 (`SupabaseClient.cs`) |
| 소셜 로그인 — Google | Google Sign-In Unity Plugin (ID Token → Supabase) |
| 소셜 로그인 — Apple | Apple Auth Unity Plugin (iOS 전용) |
| UI | TextMeshPro, DOTween |

> **Note** Firebase SDK는 완전히 제거되었습니다. `Assets/Firebase/` 폴더와 Firebase 관련 AAR이 없는 것이 정상입니다.

---

## 프로젝트 구조 (주요 스크립트)

```
Assets/_Scripts/Mananger/
├── SupabaseClient.cs     # Supabase URL/AnonKey 상수 + HTTP 래퍼 (GET/POST/PATCH/UPSERT/DELETE)
├── AuthManager.cs        # 로그인·로그아웃·자동로그인·게스트→플랫폼 연동
├── DatabaseManager.cs    # 유저 데이터 CRUD, 점수 관리, 앱 버전 체크
├── StaminaManager.cs     # 스태미나 충전 타이머, DB 동기화
└── TrLobbyManager.cs     # 로비 초기화, 랭킹 보드 렌더링
```

---

## DB 스키마 (Supabase)

```sql
-- 유저 기본 정보
users (
  id                uuid PRIMARY KEY,  -- Supabase Auth UID
  email             text,
  nickname          text UNIQUE,
  stamina           int,
  stamina_updated_at text,
  max_score         int,
  platform_type     int                -- 0:NONE 1:GUEST 2:GOOGLE 3:APPLE
)

-- 유저별 상위 5개 점수
user_scores (
  user_id  uuid REFERENCES users(id),
  score1   int,
  score2   int,
  score3   int,
  score4   int,
  score5   int
)

-- 앱 버전 관리
app_versions (
  character  text,
  platform   text,
  version    text
)
```

---

## 인증 플로우

```
[Guest]   POST /auth/v1/token?grant_type=anonymous
[Google]  Google Sign-In Plugin → IdToken → POST /auth/v1/token?grant_type=id_token  (provider=google)
[Apple]   Apple Auth Plugin    → IdToken → POST /auth/v1/token?grant_type=id_token  (provider=apple)

자동로그인: PlayerPrefs에 저장된 refresh_token으로
          POST /auth/v1/token?grant_type=refresh_token → access_token 갱신
```

세션 토큰은 `PlayerPrefs`에 저장됩니다 (`SupabaseRefreshToken` 키).

---

## 마이그레이션 이력: Firebase → Supabase

이 프로젝트는 초기 Firebase 기반에서 Supabase로 전면 마이그레이션되었습니다.

### 제거된 것
| 항목 | 비고 |
|------|------|
| Firebase Auth SDK v9.1.0 | DLL + Android AAR ~80개 전체 삭제 |
| Firebase Realtime Database SDK | |
| `Assets/Firebase/` 폴더 | Editor DLL, m2repository 포함 |
| `google-services.json` | Firebase 프로젝트 설정 파일 |
| `FirebaseApp.CheckAndFixDependenciesAsync()` 초기화 패턴 | |

### 추가된 것
| 항목 | 비고 |
|------|------|
| `SupabaseClient.cs` | static HTTP 래퍼. 외부 패키지 없이 `UnityWebRequest`만 사용 |
| Supabase Auth REST 연동 | anonymous / id_token(Google·Apple) / refresh_token grant |
| Supabase PostgREST 연동 | users, user_scores, app_versions 테이블 |

### 이름 변경 (하위 호환 주의)
| 변경 전 | 변경 후 |
|---------|---------|
| `zSetFirebase()` | `zInitialize()` |
| `IsFirebaseReady` | `IsReady` |

---

## 환경 설정 주의사항

### 1. Supabase AnonKey 하드코딩
`SupabaseClient.cs` 상단에 `ProjectUrl`과 `AnonKey`가 상수로 선언되어 있습니다.
AnonKey는 공개 클라이언트 키이지만, 프로덕션 환경에서는 RLS(Row Level Security) 정책으로 접근을 제어해야 합니다.
키를 교체할 경우 이 두 상수를 수정하세요.

```csharp
// SupabaseClient.cs
public const string ProjectUrl = "https://<your-project>.supabase.co";
public const string AnonKey    = "<your-anon-key>";
```

### 2. Google Sign-In 설정
Google Sign-In Unity 플러그인(`Assets/Plugins/`)을 그대로 사용합니다.
`AuthManager.cs`의 `_webClientId`를 본인의 Google Cloud 프로젝트 Web Client ID로 교체해야 합니다.

```csharp
// AuthManager.cs
string _webClientId = "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com";
```

Supabase 대시보드 → Authentication → Providers → Google에서도 동일한 Client ID/Secret을 등록해야 합니다.

### 3. Android 의존성
`AndroidResolverDependencies.xml`에는 Firebase 패키지가 없습니다. Google Sign-In 관련 패키지만 포함됩니다.

```
com.google.android.gms:play-services-auth:16+
com.google.android.play:core:1.10.3
com.google.signin:google-signin-support:1.0.4
```

### 4. 에디터 실행 (개발 편의)
Unity Editor에서 Play 시 `AuthManager.yCreateDummyUser()`가 자동 호출되어 DB 네트워크 요청 없이 로컬 더미 유저로 동작합니다.

- 더미 userId: `00000000-0000-0000-0000-000000000001`
- 더미 nickname: `Editor`

### 5. DEVELOPMENT_BUILD 심볼
`DEVELOPMENT_BUILD` 스크립팅 심볼이 활성화된 빌드에서는 `StaminaManager`의 스태미나 소모 로직이 바이패스되어 무제한 플레이가 가능합니다.

### 6. 키스토어
Android 빌드용 키스토어 정보는 **절대 소스에 커밋하지 마세요.** 로컬 `ProjectSettings`에만 보관하거나 CI/CD 환경 변수로 주입하세요.

---

## 알려진 기술 부채

- `user_scores` 테이블의 `score1~score5` 고정 컬럼 구조 → 추후 `game_sessions` 방식(게임 세션별 행)으로 리팩토링 예정
