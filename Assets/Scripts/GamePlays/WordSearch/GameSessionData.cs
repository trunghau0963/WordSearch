/// <summary>
/// Static runtime holder for the current WordSearch game session data.
/// Populated by GameDataSelector on scene load. All gameplay scripts
/// read BoardData from here instead of a ScriptableObject reference.
/// </summary>
public static class GameSessionData
{
    public static BoardData CurrentBoard { get; set; }
}
