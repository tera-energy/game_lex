# Supabase RLS(Row Level Security) 설정 가이드

Supabase 대시보드 → SQL Editor에서 아래 SQL을 순서대로 실행한다.

---

## 1. users 테이블

```sql
-- RLS 활성화
ALTER TABLE users ENABLE ROW LEVEL SECURITY;

-- 전체 읽기 허용 (랭킹 조회용 — 인증 여부 무관)
CREATE POLICY "users_select_all"
  ON users FOR SELECT
  USING (true);

-- 본인 데이터만 INSERT (Supabase Auth UID = users.id)
CREATE POLICY "users_insert_own"
  ON users FOR INSERT
  WITH CHECK (auth.uid() = id);

-- 본인 데이터만 UPDATE
CREATE POLICY "users_update_own"
  ON users FOR UPDATE
  USING (auth.uid() = id);

-- 본인 데이터만 DELETE
CREATE POLICY "users_delete_own"
  ON users FOR DELETE
  USING (auth.uid() = id);
```

---

## 2. user_scores 테이블

```sql
-- RLS 활성화
ALTER TABLE user_scores ENABLE ROW LEVEL SECURITY;

-- 본인 점수만 읽기
CREATE POLICY "scores_select_own"
  ON user_scores FOR SELECT
  USING (auth.uid() = user_id);

-- 본인 점수만 INSERT
CREATE POLICY "scores_insert_own"
  ON user_scores FOR INSERT
  WITH CHECK (auth.uid() = user_id);

-- 본인 점수만 UPDATE
CREATE POLICY "scores_update_own"
  ON user_scores FOR UPDATE
  USING (auth.uid() = user_id);

-- 본인 점수만 DELETE
CREATE POLICY "scores_delete_own"
  ON user_scores FOR DELETE
  USING (auth.uid() = user_id);
```

---

## 3. app_versions 테이블

```sql
-- RLS 활성화
ALTER TABLE app_versions ENABLE ROW LEVEL SECURITY;

-- 전체 읽기 허용 (버전 체크용)
CREATE POLICY "versions_select_all"
  ON app_versions FOR SELECT
  USING (true);

-- 쓰기는 service_role만 (클라이언트에서 직접 수정 불가)
-- INSERT / UPDATE / DELETE 정책 없음 = anon/authenticated 모두 차단
```

---

## 적용 후 확인

```sql
-- 각 테이블의 RLS 활성화 여부 확인
SELECT tablename, rowsecurity
FROM pg_tables
WHERE schemaname = 'public'
  AND tablename IN ('users', 'user_scores', 'app_versions');

-- 설정된 정책 목록 확인
SELECT tablename, policyname, cmd, qual
FROM pg_policies
WHERE schemaname = 'public'
ORDER BY tablename, cmd;
```

---

## 주의사항

- `auth.uid()`는 Supabase Auth로 로그인한 유저의 UUID를 반환한다. 익명 로그인(`anonymous grant`)도 UID가 발급되므로 정책이 정상 적용된다.
- `users` 테이블의 SELECT를 `true`로 열어둔 이유: 랭킹 조회 시 전체 유저 닉네임·점수를 읽어야 하기 때문이다.
- RLS를 활성화하면 service_role 키를 사용하는 서버 사이드 관리 작업(예: 어드민 스크립트)에는 영향이 없다.
