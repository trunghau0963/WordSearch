using UnityEngine;

/// <summary>
/// Singleton that persists across scenes, carrying the selected level data
/// from the menu to the gameplay scene.
/// </summary>
public class LevelPlayDataHolder : MonoBehaviour
{
    public static LevelPlayDataHolder Instance { get; private set; }

    public LevelPlayData CurrentData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetData(LevelPlayData data)
    {
        CurrentData = data;
    }

    /// <summary>
    /// Convenience: create or get the singleton at any time.
    /// </summary>
    public static LevelPlayDataHolder GetOrCreate()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("LevelPlayDataHolder");
        return go.AddComponent<LevelPlayDataHolder>();
    }
}
