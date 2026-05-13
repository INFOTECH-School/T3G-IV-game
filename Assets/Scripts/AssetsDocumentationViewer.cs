using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AssetsDocumentationViewer : MonoBehaviour
{
    [Tooltip("The scroll rect to reset when the panel opens.")]
    public ScrollRect scrollRect;

    private void OnEnable()
    {
        StartCoroutine(ResetScrollNextFrame());
    }

    // Wait one frame so the ContentSizeFitter can recalculate the height first
    private IEnumerator ResetScrollNextFrame()
    {
        yield return null;

        if (scrollRect == null || scrollRect.content == null) yield break;

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
