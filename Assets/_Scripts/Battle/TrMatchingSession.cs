/// <summary>
/// 로비 씬 매칭 완료 후 BattleHamburger 씬으로 데이터를 전달하는 static 홀더.
/// TrUI_MatchingPopup이 쓰고, TrBattleManager가 읽는다.
/// </summary>
public static class TrMatchingSession
{
    public static string RoomId;
    public static bool   IsPlayer1;
    public static bool   PendingRematch;
    public static int?   BurgerSeed;
    public static string MyNickname;
    public static string OppNickname;

    public static bool IsReady => !string.IsNullOrEmpty(RoomId);

    public static void Clear()
    {
        RoomId      = null;
        IsPlayer1   = false;
        BurgerSeed  = null;
        MyNickname  = null;
        OppNickname = null;
        // PendingRematch는 로비가 직접 소비 후 리셋
    }
}
