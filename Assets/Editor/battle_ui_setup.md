# 대결 모드 UI 설계 문서

## 왜 줄다리기 단일 게이지 방식인가?

기존 설계는 `player1_hp` / `player2_hp` 두 개의 HP 바였음.
→ 사실상 "솔로 게임 2개를 동시에 보는 것"과 같아 긴장감이 낮음.

**줄다리기 방식으로 변경한 이유:**
- 상대 행동이 내 게이지에 즉각 반영 → 내 차례가 아닐 때도 긴장 유지
- 화면 공간 절약 → 퍼즐 영역 확보
- 역전 드라마 가능성 → 끝까지 포기 안 함

---

## DB 구조

```
gauge_position SMALLINT NOT NULL DEFAULT 0
-- 범위: -100 ~ +100
-- 0    = 중앙 (시작)
-- +100 = player1 완전 점령 → player1 승리
-- -100 = player2 완전 점령 → player2 승리
```

**게이지 이동 방향:**
```
player2 점령         중앙        player1 점령
  -100  ← ─ ─ ─ ─ ─  0  ─ ─ ─ ─ ─ → +100

player1 completed: +delta (오른쪽)
player1 failed:    -delta (왼쪽)
player2 completed: -delta (왼쪽)
player2 failed:    +delta (오른쪽)
```

**데미지 계산:**
- completed (햄버거 완성): 15 × 재료수 배율
- failed (실수): 8 × 재료수 배율
- 재료 배율: 3→1.0 / 4→1.05 / 5→1.1 / 6→1.15 / 7→1.2 / 8→1.3

---

## C# 이벤트 구조

```
TrBattleManager
  └─ OnGaugeChanged(float myRatio)
       myRatio: 0~1
       0.5 = 중앙
       1.0 = 내가 완전 점령
       0.0 = 상대가 완전 점령

  변환 공식:
  - player1이면: myRatio = (gaugePos + 100) / 200f
  - player2이면: myRatio = (100 - gaugePos) / 200f
```

---

## 씬 Hierarchy 구조 (BattleHamburger)

```
BattleHamburger (씬)
├── [기존 PuzzleHamburger 오브젝트 전부]
├── BattleManager                    ← TrBattleManager (씬 루트, Canvas 밖)
└── UI Canvas
    └── BattleHPBar                  ← TrUI_BattleHPBar
        ├── ImgBarBackground         ← 상대 색 배경 (stretch)
        ├── ImgMyBar                 ← 내 색 fill (Filled/Horizontal/Left)
        ├── CollisionPoint           ← 경계 이동 오브젝트 (RectTransform만)
        │   ├── ImgMyFace            ← 내 캐릭터 얼굴 원
        │   └── ImgOppFace           ← 상대 캐릭터 얼굴 원
        ├── TxtMyName                ← 내 닉네임
        ├── TxtOppName               ← 상대 닉네임
        ├── TxtOppProgress           ← "상대 3/6층" 진행 상황
        └── MySideFlash              ← "→ MY SIDE" 플래시 (기본 비활성)
    └── BurgerProjectile             ← TrUI_BurgerProjectile
```

---

## TrUI_BattleHPBar SerializeField 연결표

| 필드 | 연결 오브젝트 | 설명 |
|------|-------------|------|
| `_myColor` | - | 내 색 (기본 빨강) |
| `_oppColor` | - | 상대 색 (기본 파랑) |
| `_imgBarBackground` | ImgBarBackground | 배경 바 |
| `_imgMyBar` | ImgMyBar | 내 fill 바 |
| `_tfCollisionPoint` | CollisionPoint | 경계 이동 RectTransform |
| `_imgMyFace` | ImgMyFace | 내 얼굴 원 |
| `_imgOppFace` | ImgOppFace | 상대 얼굴 원 |
| `_barWidth` | - | 바 너비 픽셀값 (기본 600) |
| `_txtMyName` | TxtMyName | 내 닉네임 텍스트 |
| `_txtOppName` | TxtOppName | 상대 닉네임 텍스트 |
| `_txtOppProgress` | TxtOppProgress | 상대 진행 상황 |
| `_goMySideFlash` | MySideFlash | 게임 시작 플래시 |

---

## CollisionPoint 이동 원리

바 왼쪽 끝 = anchoredPosition.x 기준 -barWidth/2
바 오른쪽 끝 = +barWidth/2

```csharp
float x = (myRatio - 0.5f) * barWidth;
// myRatio=0.5 → x=0 (중앙)
// myRatio=1.0 → x=+300 (오른쪽 끝)
// myRatio=0.0 → x=-300 (왼쪽 끝)
```

---

## 앵커(Anchor) 설명

Unity UI에서 앵커는 부모 기준 위치를 정의함.
- **고정 앵커** (Min=Max): 부모 크기 변해도 내 위치/크기 유지
- **stretch 앵커** (Min=0, Max=1): 부모 크기에 맞게 자동으로 늘어남

ImgBarBackground/ImgMyBar는 stretch 사용 → 화면 해상도 달라져도 항상 전체 너비 유지.

---

## 관련 파일

| 파일 | 역할 |
|------|------|
| `Assets/Editor/battle_schema.sql` | DB 스키마 + Functions |
| `Assets/_Scripts/Battle/TrBattleManager.cs` | 매칭, WebSocket, 게이지 업데이트 |
| `Assets/_Scripts/Battle/TrUI_BattleHPBar.cs` | 줄다리기 UI |
| `Assets/_Scripts/Battle/TrUI_BurgerProjectile.cs` | 버거 투사체 애니메이션 |
| `Assets/_Scenes/Puzzles/BattleHamburger/BattleHamburger.unity` | 대결 씬 |
