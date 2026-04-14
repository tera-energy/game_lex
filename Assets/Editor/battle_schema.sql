-- ============================================================
-- 대결 모드 DB 스키마 (Supabase SQL Editor에서 실행)
-- 수정 이력:
--   - INSERT RLS 정책 추가 (직접 INSERT 차단)
--   - UPDATE WITH CHECK 절 추가
--   - fn_battle_hp_update: 불필요한 재조회 제거, HP+승패 단일 UPDATE,
--                          잘못된 action 유효성 검사 추가
--   - fn_battle_start: BOOLEAN 반환으로 변경 (성공 여부 클라이언트 전달)
--   - fn_battle_heartbeat: 방 소속 검증 추가
--   - fn_cleanup_abandoned_battles: ready 상태 방치 처리 추가
--   - 인덱스: 매칭 쿼리 + 하트비트 조회용 인덱스 추가
--   - [2026-04-01] player1_hp/player2_hp 두 컬럼 방식 → gauge_position 줄다리기 방식으로 변경
--                  gauge_position: -100(player2 완전 점령) ~ 0(중앙) ~ +100(player1 완전 점령)
--                  fn_battle_hp_update → fn_battle_gauge_update로 함수명 변경 및 로직 전면 수정
-- ============================================================

-- ── battle_rooms 테이블 ──────────────────────────────────────
CREATE TABLE IF NOT EXISTS battle_rooms (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    status               TEXT NOT NULL DEFAULT 'waiting',
                         -- waiting | ready | in_progress | finished | abandoned
    player1_id           UUID REFERENCES users(id),
    player2_id           UUID REFERENCES users(id),
    -- [변경] player1_hp/player2_hp 제거 → gauge_position 줄다리기 방식
    -- 0 = 중앙(시작), +100 = player1 완전 점령(player1 승리), -100 = player2 완전 점령(player2 승리)
    gauge_position       SMALLINT NOT NULL DEFAULT 0,
    burger_seed          INT NOT NULL DEFAULT floor(random() * 2147483647)::INT,
    player1_correct      INT NOT NULL DEFAULT 0,
    player2_correct      INT NOT NULL DEFAULT 0,
    player1_wrong        INT NOT NULL DEFAULT 0,
    player2_wrong        INT NOT NULL DEFAULT 0,
    player1_heartbeat_at TIMESTAMPTZ DEFAULT now(),
    player2_heartbeat_at TIMESTAMPTZ DEFAULT now(),
    result               TEXT,   -- player1_win | player2_win | draw
    winner_id            UUID REFERENCES users(id),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    started_at           TIMESTAMPTZ,
    finished_at          TIMESTAMPTZ,
    last_updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    -- [2-Phase Handshake] 양쪽 WebSocket 구독 완료 신호 플래그
    player1_ws_ready     BOOLEAN NOT NULL DEFAULT false,
    player2_ws_ready     BOOLEAN NOT NULL DEFAULT false
);

-- ── 인덱스 ──────────────────────────────────────────────────
-- [수정] 매칭 쿼리: WHERE status='waiting' AND player2_id IS NULL AND created_at > ...
-- 복합 인덱스로 Index Scan + 정렬 모두 커버
CREATE INDEX IF NOT EXISTS idx_battle_rooms_matching
    ON battle_rooms (status, player2_id, created_at ASC)
    WHERE status = 'waiting' AND player2_id IS NULL;

-- [수정] 클린업 쿼리: WHERE status IN (...) AND heartbeat_at < ...
CREATE INDEX IF NOT EXISTS idx_battle_rooms_cleanup
    ON battle_rooms (status, player1_heartbeat_at, player2_heartbeat_at)
    WHERE status IN ('waiting', 'ready', 'in_progress');

-- 플레이어별 참여 방 조회 (RLS SELECT 정책 + Realtime 필터링)
CREATE INDEX IF NOT EXISTS idx_battle_rooms_player1 ON battle_rooms (player1_id);
CREATE INDEX IF NOT EXISTS idx_battle_rooms_player2 ON battle_rooms (player2_id);

-- ── Realtime 활성화 ──────────────────────────────────────────
ALTER PUBLICATION supabase_realtime ADD TABLE battle_rooms;

-- ── RLS ─────────────────────────────────────────────────────
ALTER TABLE battle_rooms ENABLE ROW LEVEL SECURITY;

-- 참가자만 조회 가능
CREATE POLICY "battle_rooms_select" ON battle_rooms
    FOR SELECT USING (
        auth.uid() = player1_id OR auth.uid() = player2_id
    );

-- [수정] 직접 INSERT 전면 차단 — 반드시 fn_join_or_create_battle(SECURITY DEFINER) 경유
-- 악의적 클라이언트가 REST API로 임의의 방을 생성하는 것을 방지
CREATE POLICY "battle_rooms_insert_deny" ON battle_rooms
    FOR INSERT WITH CHECK (false);

-- [수정] UPDATE WITH CHECK 추가
-- USING: 대상 행을 찾을 때 (자신이 참가한 방만)
-- WITH CHECK: UPDATE 후 새 값 검증 (player_id 교체 등 위조 방지)
-- 주의: 게임 로직 UPDATE는 모두 SECURITY DEFINER 함수가 담당하므로
--        이 정책은 클라이언트 직접 호출 방어용
CREATE POLICY "battle_rooms_update" ON battle_rooms
    FOR UPDATE
    USING (auth.uid() = player1_id OR auth.uid() = player2_id)
    WITH CHECK (auth.uid() = player1_id OR auth.uid() = player2_id);

-- ── fn_join_or_create_battle ─────────────────────────────────
-- 빈 방 찾기 or 새 방 생성
-- SELECT FOR UPDATE SKIP LOCKED: 같은 트랜잭션 내에서 잠금 → UPDATE 순서 보장
-- SKIP LOCKED: 이미 다른 트랜잭션이 잠근 방은 건너뜀 (동시 매칭 안전)
CREATE OR REPLACE FUNCTION fn_join_or_create_battle(p_player_id UUID)
RETURNS UUID
LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
    v_room_id UUID;
BEGIN
    -- 대기 중인 방 찾기 (자신이 만든 방 제외)
    -- FOR UPDATE SKIP LOCKED: 동시에 두 플레이어가 같은 방에 참가하는 것을 방지
    SELECT id INTO v_room_id
    FROM battle_rooms
    WHERE status = 'waiting'
      AND player1_id != p_player_id
      AND player2_id IS NULL
      AND created_at > now() - INTERVAL '30 seconds'
    ORDER BY created_at ASC
    LIMIT 1
    FOR UPDATE SKIP LOCKED;

    IF v_room_id IS NOT NULL THEN
        -- 방에 player2로 참가
        UPDATE battle_rooms
        SET player2_id      = p_player_id,
            status          = 'ready',
            last_updated_at = now()
        WHERE id = v_room_id;
    ELSE
        -- 새 방 생성 (player1)
        INSERT INTO battle_rooms (player1_id)
        VALUES (p_player_id)
        RETURNING id INTO v_room_id;
    END IF;

    RETURN v_room_id;
END;
$$;

-- ── fn_battle_gauge_update ───────────────────────────────────
-- [변경] 기존 fn_battle_hp_update에서 fn_battle_gauge_update로 전면 교체
-- gauge_position 줄다리기 방식으로 변경:
--   completed (공격 성공):
--     player1이면 gauge_position += delta (최대 +100)
--     player2이면 gauge_position -= delta (최소 -100)
--   failed (실수):
--     player1이면 gauge_position -= delta (최소 -100)
--     player2이면 gauge_position += delta (최대 +100)
--   delta: completed=15×multiplier, failed=8×multiplier
-- 승리 조건:
--   gauge_position >= +100 → player1 승리
--   gauge_position <= -100 → player2 승리
-- p_action: 'completed' | 'failed'
-- p_ingredients: 재료 수 (3~8), 배율 계산용
CREATE OR REPLACE FUNCTION fn_battle_gauge_update(
    p_room_id     UUID,
    p_player_id   UUID,
    p_action      TEXT,
    p_ingredients INT
)
RETURNS VOID
LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
    v_is_player1      BOOLEAN;
    v_p1_id           UUID;
    v_p2_id           UUID;
    v_gauge           SMALLINT;
    v_multiplier      NUMERIC;
    v_delta           SMALLINT;
    v_new_gauge       SMALLINT;
    v_result          TEXT;
    v_winner_id       UUID;
BEGIN
    -- 잘못된 action 값 조기 차단
    IF p_action NOT IN ('completed', 'failed') THEN
        RAISE EXCEPTION 'Invalid p_action: %. Must be completed or failed.', p_action;
    END IF;

    -- 행 잠금: 같은 방에 대한 동시 gauge 업데이트를 직렬화
    SELECT player1_id = p_player_id,
           player1_id, player2_id,
           gauge_position
    INTO v_is_player1, v_p1_id, v_p2_id, v_gauge
    FROM battle_rooms
    WHERE id = p_room_id AND status = 'in_progress'
    FOR UPDATE;

    IF NOT FOUND THEN RETURN; END IF;

    -- 배율: 재료 수 3→1.0, 4→1.05, 5→1.1, 6→1.15, 7→1.2, 8→1.3
    v_multiplier := CASE p_ingredients
        WHEN 3 THEN 1.0
        WHEN 4 THEN 1.05
        WHEN 5 THEN 1.1
        WHEN 6 THEN 1.15
        WHEN 7 THEN 1.2
        WHEN 8 THEN 1.3
        ELSE    1.0
    END;

    -- delta 계산 후 gauge 이동 방향 결정
    -- completed: 내 방향으로 이동 (player1=+, player2=-)
    -- failed:    상대 방향으로 이동 (player1=-, player2=+)
    IF p_action = 'completed' THEN
        v_delta := GREATEST(1, floor(15 * v_multiplier)::SMALLINT);
        IF v_is_player1 THEN
            v_new_gauge := LEAST(100, v_gauge + v_delta);
        ELSE
            v_new_gauge := GREATEST(-100, v_gauge - v_delta);
        END IF;

    ELSE -- 'failed'
        v_delta := GREATEST(1, floor(8 * v_multiplier)::SMALLINT);
        IF v_is_player1 THEN
            v_new_gauge := GREATEST(-100, v_gauge - v_delta);
        ELSE
            v_new_gauge := LEAST(100, v_gauge + v_delta);
        END IF;
    END IF;

    -- 승패 판정: 경계값 도달 여부 확인
    IF v_new_gauge >= 100 THEN
        v_result    := 'player1_win';
        v_winner_id := v_p1_id;
    ELSIF v_new_gauge <= -100 THEN
        v_result    := 'player2_win';
        v_winner_id := v_p2_id;
    END IF;
    -- draw 없음: gauge는 항상 한쪽으로 이동하므로 동시 경계 도달 불가

    -- gauge + 카운터 + 승패 정보를 단일 UPDATE로 원자적 처리
    UPDATE battle_rooms
    SET gauge_position  = v_new_gauge,
        player1_correct = player1_correct + CASE WHEN v_is_player1      AND p_action = 'completed' THEN 1 ELSE 0 END,
        player2_correct = player2_correct + CASE WHEN NOT v_is_player1  AND p_action = 'completed' THEN 1 ELSE 0 END,
        player1_wrong   = player1_wrong   + CASE WHEN v_is_player1      AND p_action = 'failed'    THEN 1 ELSE 0 END,
        player2_wrong   = player2_wrong   + CASE WHEN NOT v_is_player1  AND p_action = 'failed'    THEN 1 ELSE 0 END,
        status          = CASE WHEN v_result IS NOT NULL THEN 'finished'   ELSE status      END,
        result          = CASE WHEN v_result IS NOT NULL THEN v_result     ELSE result      END,
        winner_id       = CASE WHEN v_result IS NOT NULL THEN v_winner_id  ELSE winner_id   END,
        finished_at     = CASE WHEN v_result IS NOT NULL THEN now()        ELSE finished_at END,
        last_updated_at = now()
    WHERE id = p_room_id;
END;
$$;

-- ── 기존 테이블에 컬럼 추가 (이미 테이블이 있는 경우 실행) ────────
ALTER TABLE battle_rooms
    ADD COLUMN IF NOT EXISTS player1_ws_ready BOOLEAN NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS player2_ws_ready BOOLEAN NOT NULL DEFAULT false;

-- ── fn_battle_ws_ready ───────────────────────────────────────
-- [2-Phase Handshake] WebSocket 구독 완료 신호 전송.
-- 클라이언트가 Realtime 구독 완료 직후 호출.
-- 양쪽 모두 신호를 보낸 시점에 서버가 in_progress 전환 + started_at 설정.
-- 반환값:
--   TRUE  = 이 호출로 양쪽 모두 ready → in_progress 전환 발생
--   FALSE = 아직 상대 대기 중 (클라이언트는 Realtime으로 in_progress 이벤트 수신 대기)
CREATE OR REPLACE FUNCTION fn_battle_ws_ready(p_room_id UUID, p_player_id UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
    v_is_player1 BOOLEAN;
    v_both_ready BOOLEAN;
BEGIN
    -- ready 상태 방에서만 동작 (행 잠금 — 동시 호출 직렬화)
    SELECT player1_id = p_player_id
    INTO v_is_player1
    FROM battle_rooms
    WHERE id = p_room_id AND status = 'ready'
    FOR UPDATE;

    IF NOT FOUND THEN RETURN FALSE; END IF;

    -- 내 WS ready 플래그 설정
    IF v_is_player1 THEN
        UPDATE battle_rooms
        SET player1_ws_ready = true, last_updated_at = now()
        WHERE id = p_room_id;
    ELSE
        UPDATE battle_rooms
        SET player2_ws_ready = true, last_updated_at = now()
        WHERE id = p_room_id;
    END IF;

    -- 양쪽 모두 ready인지 확인
    SELECT (player1_ws_ready AND player2_ws_ready)
    INTO v_both_ready
    FROM battle_rooms
    WHERE id = p_room_id;

    -- 양쪽 모두 ready면 in_progress 전환 + 2초 후 게임 시작 시각 설정
    -- 2초: 양쪽이 이 Realtime 이벤트를 수신 후 카운트다운을 표시할 여유 시간
    IF v_both_ready THEN
        UPDATE battle_rooms
        SET status          = 'in_progress',
            started_at      = now() + INTERVAL '2 seconds',
            last_updated_at = now()
        WHERE id = p_room_id AND status = 'ready';
        RETURN TRUE;
    END IF;

    RETURN FALSE;
END;
$$;

-- ── fn_battle_start ──────────────────────────────────────────
-- ready → in_progress 전환 (양쪽 모두 준비 확인 후 호출)
-- [수정] RETURNS BOOLEAN: 실제로 전환이 일어났는지 Unity 클라이언트에 알림
--        TRUE  = 내가 게임을 시작시킨 플레이어 (선착순)
--        FALSE = 상대방이 이미 시작시킴 (이 호출은 no-op)
--        클라이언트는 둘 다 Realtime으로 status 변화를 감지하므로
--        결과값 자체보다는 방어적 처리 / 로그 용도로 활용
CREATE OR REPLACE FUNCTION fn_battle_start(p_room_id UUID, p_player_id UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql SECURITY DEFINER
AS $$
DECLARE
    v_rows_updated INT;
BEGIN
    -- status = 'ready' 조건이 두 번째 호출을 자동 차단 (이미 in_progress이므로)
    UPDATE battle_rooms
    SET status          = 'in_progress',
        started_at      = now() + INTERVAL '3 seconds',
        last_updated_at = now()
    WHERE id            = p_room_id
      AND status        = 'ready'
      AND (player1_id   = p_player_id OR player2_id = p_player_id);

    GET DIAGNOSTICS v_rows_updated = ROW_COUNT;
    RETURN v_rows_updated > 0;
END;
$$;

-- ── fn_battle_heartbeat ──────────────────────────────────────
-- [수정] 방 소속 검증 추가: 해당 방의 player1/player2만 heartbeat 갱신 가능
--        미소속 플레이어가 heartbeat를 보내 방을 좀비 상태로 유지하는 것을 방지
CREATE OR REPLACE FUNCTION fn_battle_heartbeat(p_room_id UUID, p_player_id UUID)
RETURNS VOID
LANGUAGE plpgsql SECURITY DEFINER
AS $$
BEGIN
    UPDATE battle_rooms
    SET player1_heartbeat_at = CASE WHEN player1_id = p_player_id THEN now() ELSE player1_heartbeat_at END,
        player2_heartbeat_at = CASE WHEN player2_id = p_player_id THEN now() ELSE player2_heartbeat_at END,
        last_updated_at      = now()
    WHERE id = p_room_id
      -- [수정] player1_id 또는 player2_id가 일치해야만 갱신
      AND (player1_id = p_player_id OR player2_id = p_player_id)
      -- 종료된 방에는 heartbeat 불필요
      AND status IN ('waiting', 'ready', 'in_progress');
END;
$$;

-- ── fn_cleanup_abandoned_battles ────────────────────────────
-- pg_cron으로 매 1분 실행: 방치된 방 abandoned 처리
-- [수정] ready 상태 방치 처리 추가 (기존에는 ready가 영원히 남아있는 버그)
--        ready 방이 60초 이상 in_progress로 전환되지 않으면 abandoned 처리
CREATE OR REPLACE FUNCTION fn_cleanup_abandoned_battles()
RETURNS VOID
LANGUAGE plpgsql SECURITY DEFINER
AS $$
BEGIN
    UPDATE battle_rooms
    SET status          = 'abandoned',
        last_updated_at = now()
    WHERE status IN ('waiting', 'ready', 'in_progress')
      AND (
          -- waiting: player2가 없이 35초 이상 경과
          (status = 'waiting'
              AND player2_id IS NULL
              AND created_at < now() - INTERVAL '35 seconds')

          -- [수정] ready: 매칭은 됐지만 fn_battle_start 미호출로 60초 이상 경과
          OR (status = 'ready'
              AND last_updated_at < now() - INTERVAL '60 seconds')

          -- in_progress: 어느 한쪽이라도 60초 이상 heartbeat 없음
          OR (status = 'in_progress'
              AND (
                  player1_heartbeat_at < now() - INTERVAL '60 seconds'
                  OR player2_heartbeat_at < now() - INTERVAL '60 seconds'
              ))
      );
END;
$$;

-- ── pg_cron 등록 ─────────────────────────────────────────────
-- Supabase Dashboard → Database → Extensions에서 pg_cron 활성화 필요
-- SELECT cron.schedule('cleanup-abandoned-battles', '* * * * *', 'SELECT fn_cleanup_abandoned_battles()');
