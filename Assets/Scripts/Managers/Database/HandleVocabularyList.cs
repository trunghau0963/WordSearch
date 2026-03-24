using UnityEngine;

/// <summary>
/// Ensures VocabularyList and PlayHistory singletons exist in the scene.
/// All CRUD operations are handled globally by the singletons themselves.
/// </summary>
public class HandleVocabularyList : MonoBehaviour
{
    void Awake()
    {
        // Ensure the global VocabularyList singleton exists
        if (VocabularyList.Instance == null)
        {
            var go = new GameObject("VocabularyList");
            go.AddComponent<VocabularyList>();
        }

        // Ensure the global PlayHistory singleton exists
        if (PlayHistory.Instance == null)
        {
            var go = new GameObject("PlayHistory");
            go.AddComponent<PlayHistory>();
        }
    }
}
