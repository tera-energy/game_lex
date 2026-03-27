using UnityEngine;

[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "Config/Supabase Config")]
public class TrSupabaseConfig : ScriptableObject
{
    public string projectUrl;
    public string anonKey;
}
