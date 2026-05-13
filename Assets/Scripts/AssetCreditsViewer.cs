using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the contents of Resources/Management/AssetsDocumentation.txt
/// in a runtime-generated scrollable overlay panel with fade animation.
/// Attach to any persistent GameObject in the main menu scene.
/// </summary>
public class AssetCreditsViewer : MonoBehaviour
{
    [Header("Optional")]
    [Tooltip("TMP Font to use for the documentation text. Falls back to TMP default if unset.")]
    public TMP_FontAsset font;

    // Runtime UI references
    private GameObject _overlayRoot;
    private CanvasGroup _canvasGroup;
    private bool _isVisible;
    private Coroutine _fadeCoroutine;

    // Design constants
    private const float FadeInDuration = 0.3f;
    private const float FadeOutDuration = 0.25f;
    private const float OverlayAlpha = 0.7f;
    private const float PanelWidthPercent = 0.80f;
    private const float PanelHeightPercent = 0.80f;
    private const int FontSize = 16;
    private const float LineSpacing = 20f;
    private const int ScrollbarWidth = 20;
    private const int HeaderHeight = 50;
    private const int ContentPadding = 20;

    /// <summary>
    /// Call this from a button's OnClick to show the asset documentation overlay.
    /// </summary>
    public void ShowDocumentation()
    {
        if (_isVisible) return;

        // Load the text asset
        TextAsset textAsset = Resources.Load<TextAsset>("Management/AssetsDocumentation");
        if (textAsset == null)
        {
            Debug.LogError("[AssetCreditsViewer] Could not load Resources/Management/AssetsDocumentation.txt");
            return;
        }

        // Build the UI if this is the first time
        if (_overlayRoot == null)
        {
            BuildOverlayUI(textAsset.text);
        }
        else
        {
            // Update text in case it changed
            var contentText = _overlayRoot.GetComponentInChildren<TextMeshProUGUI>();
            if (contentText != null && contentText.name == "DocText")
                contentText.text = textAsset.text;
        }

        _overlayRoot.SetActive(true);
        _isVisible = true;

        // Reset scroll position to top
        var scrollRect = _overlayRoot.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        // Fade in
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, FadeInDuration));
    }

    /// <summary>
    /// Hides the documentation overlay with a fade-out animation.
    /// </summary>
    public void HideDocumentation()
    {
        if (!_isVisible || _overlayRoot == null) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, FadeOutDuration, () =>
        {
            _overlayRoot.SetActive(false);
            _isVisible = false;
        }));
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration, System.Action onComplete = null)
    {
        if (_canvasGroup == null) yield break;

        _canvasGroup.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
        onComplete?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────
    //  Runtime UI Construction
    // ─────────────────────────────────────────────────────────────────

    private void BuildOverlayUI(string documentationText)
    {
        // ── Root Canvas ──
        _overlayRoot = new GameObject("AssetCreditsOverlay");
        _overlayRoot.transform.SetParent(transform, false);

        Canvas canvas = _overlayRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // On top of everything

        CanvasScaler scaler = _overlayRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _overlayRoot.AddComponent<GraphicRaycaster>();

        _canvasGroup = _overlayRoot.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        // ── Dark Backdrop (clickable to close) ──
        GameObject backdrop = CreateUIElement("Backdrop", _overlayRoot.transform);
        RectTransform backdropRT = backdrop.GetComponent<RectTransform>();
        StretchFull(backdropRT);

        Image backdropImg = backdrop.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, OverlayAlpha);

        Button backdropBtn = backdrop.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(HideDocumentation);

        // ── Content Panel (centered box) ──
        GameObject panel = CreateUIElement("ContentPanel", backdrop.transform);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(1920 * PanelWidthPercent, 1080 * PanelHeightPercent);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.165f, 0.165f, 0.165f, 1f); // #2A2A2A

        // Border effect via Outline component
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        panelOutline.effectDistance = new Vector2(2, -2);

        // Panel Image with raycastTarget=true already blocks clicks from reaching backdrop
        // (Do NOT add a Button here — it would intercept scroll wheel events)

        // ── Header Bar ──
        GameObject header = CreateUIElement("Header", panel.transform);
        RectTransform headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 1);
        headerRT.anchorMax = new Vector2(1, 1);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.sizeDelta = new Vector2(0, HeaderHeight);
        headerRT.anchoredPosition = Vector2.zero;

        Image headerImg = header.AddComponent<Image>();
        headerImg.color = new Color(0.22f, 0.22f, 0.22f, 1f);

        // Title text
        GameObject titleObj = CreateUIElement("TitleText", header.transform);
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.offsetMin = new Vector2(ContentPadding, 0);
        titleRT.offsetMax = new Vector2(-HeaderHeight, 0); // Leave room for close button

        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Asset Documentation";
        titleTMP.fontSize = 22;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) titleTMP.font = font;

        // Close button (✕)
        GameObject closeBtn = CreateUIElement("CloseButton", header.transform);
        RectTransform closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1, 0);
        closeBtnRT.anchorMax = new Vector2(1, 1);
        closeBtnRT.pivot = new Vector2(1f, 0.5f);
        closeBtnRT.sizeDelta = new Vector2(HeaderHeight, 0);
        closeBtnRT.anchoredPosition = Vector2.zero;

        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red-ish

        Button closeBtnComponent = closeBtn.AddComponent<Button>();
        closeBtnComponent.targetGraphic = closeBtnImg;
        ColorBlock closeColors = closeBtnComponent.colors;
        closeColors.highlightedColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        closeColors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        closeBtnComponent.colors = closeColors;
        closeBtnComponent.onClick.AddListener(HideDocumentation);

        // Close button "✕" text
        GameObject closeText = CreateUIElement("CloseText", closeBtn.transform);
        RectTransform closeTextRT = closeText.GetComponent<RectTransform>();
        StretchFull(closeTextRT);

        TextMeshProUGUI closeTMP = closeText.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "✕";
        closeTMP.fontSize = 24;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.color = Color.white;
        closeTMP.alignment = TextAlignmentOptions.Center;
        if (font != null) closeTMP.font = font;

        // ── Scroll Area ──
        // Viewport (masks content)
        GameObject viewport = CreateUIElement("Viewport", panel.transform);
        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(ContentPadding, ContentPadding); // left, bottom padding
        viewportRT.offsetMax = new Vector2(-ContentPadding - ScrollbarWidth, -HeaderHeight); // right (scrollbar), top (header)

        viewport.AddComponent<Image>().color = new Color(0.165f, 0.165f, 0.165f, 1f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content (the actual scrollable text container)
        GameObject content = CreateUIElement("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0); // Height driven by ContentSizeFitter

        // VerticalLayoutGroup properly queries children for preferred sizes
        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(ContentPadding, ContentPadding, ContentPadding, ContentPadding);
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Documentation text — sizing is driven by the VerticalLayoutGroup above
        GameObject textObj = CreateUIElement("DocText", content.transform);

        TextMeshProUGUI docTMP = textObj.AddComponent<TextMeshProUGUI>();
        docTMP.text = documentationText;
        docTMP.fontSize = FontSize;
        docTMP.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        docTMP.alignment = TextAlignmentOptions.TopLeft;
        docTMP.lineSpacing = LineSpacing;
        docTMP.textWrappingMode = TextWrappingModes.Normal;
        docTMP.overflowMode = TextOverflowModes.Overflow;
        docTMP.richText = false;
        if (font != null) docTMP.font = font;

        // ── Vertical Scrollbar ──
        GameObject scrollbarObj = CreateUIElement("Scrollbar", panel.transform);
        RectTransform scrollbarRT = scrollbarObj.GetComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(1, 0);
        scrollbarRT.anchorMax = new Vector2(1, 1);
        scrollbarRT.pivot = new Vector2(1, 0.5f);
        scrollbarRT.sizeDelta = new Vector2(ScrollbarWidth, 0);
        scrollbarRT.offsetMin = new Vector2(-ScrollbarWidth - ContentPadding, ContentPadding);
        scrollbarRT.offsetMax = new Vector2(-ContentPadding, -HeaderHeight);

        Image scrollbarBg = scrollbarObj.AddComponent<Image>();
        scrollbarBg.color = new Color(0.12f, 0.12f, 0.12f, 1f); // Dark track

        // Scrollbar sliding area
        GameObject slidingArea = CreateUIElement("SlidingArea", scrollbarObj.transform);
        RectTransform slidingRT = slidingArea.GetComponent<RectTransform>();
        StretchFull(slidingRT);
        slidingRT.offsetMin = new Vector2(0, 0);
        slidingRT.offsetMax = new Vector2(0, 0);

        // Scrollbar handle
        GameObject handle = CreateUIElement("Handle", slidingArea.transform);
        RectTransform handleRT = handle.GetComponent<RectTransform>();
        StretchFull(handleRT);

        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Visible gray thumb

        // Configure Scrollbar component
        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.handleRect = handleRT;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handleImg;
        ColorBlock scrollColors = scrollbar.colors;
        scrollColors.highlightedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        scrollColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        scrollbar.colors = scrollColors;

        // ── ScrollRect ──
        ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        _overlayRoot.SetActive(false);
    }

    // ── Helpers ──

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = 5; // UI layer
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
