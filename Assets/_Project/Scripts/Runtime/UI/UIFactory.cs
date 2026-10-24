using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace RescueSim.UI
{
    /// <summary>Builds world-space VR UI (panels, text, buttons, sliders, toggles) from code so layouts are reproducible.</summary>
    public static class UIFactory
    {
        public static readonly Color Panel = new Color(0.07f, 0.09f, 0.12f, 0.94f);
        public static readonly Color Accent = new Color(1f, 0.55f, 0.1f, 1f);
        public static readonly Color AccentDim = new Color(0.55f, 0.3f, 0.08f, 1f);
        public static readonly Color Button = new Color(0.16f, 0.2f, 0.27f, 1f);
        public static readonly Color ButtonHover = new Color(0.25f, 0.33f, 0.45f, 1f);
        public static readonly Color TextMain = new Color(0.93f, 0.95f, 0.97f, 1f);
        public static readonly Color TextDim = new Color(0.62f, 0.68f, 0.76f, 1f);
        public static readonly Color Good = new Color(0.3f, 0.9f, 0.5f, 1f);
        public static readonly Color Bad = new Color(1f, 0.35f, 0.3f, 1f);

        /// <summary>World-space canvas at 1 px = 1.5 mm. Size is given in pixels.</summary>
        public static Canvas CreateWorldCanvas(string name, Vector2 pixelSize, Transform parent, float metersPerPixel = 0.0015f, bool interactive = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = pixelSize;
            rt.localScale = Vector3.one * metersPerPixel;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            if (interactive) go.AddComponent<TrackedDeviceGraphicRaycaster>();
            return canvas;
        }

        public static Image AddBackground(Transform parent, Color color, string name = "Background")
        {
            var img = NewUI<Image>(name, parent);
            img.color = color;
            Stretch(img.rectTransform);
            img.raycastTarget = false;
            return img;
        }

        public static TMP_Text AddText(Transform parent, string text, float size, Vector2 anchoredPos, Vector2 sizeDelta,
            TextAlignmentOptions align = TextAlignmentOptions.Left, Color? color = null, FontStyles style = FontStyles.Normal, string name = null)
        {
            var t = NewUI<TextMeshProUGUI>(name ?? "Text", parent);
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color ?? TextMain;
            t.fontStyle = style;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return t;
        }

        public static Button AddButton(Transform parent, string label, Vector2 anchoredPos, Vector2 sizeDelta, UnityAction onClick,
            Color? fill = null, float fontSize = 34f)
        {
            var img = NewUI<Image>("Button_" + label, parent);
            img.color = Color.white;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var baseColor = fill ?? Button;
            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.25f);
            colors.selectedColor = baseColor;
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            colors.fadeDuration = 0.05f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var t = AddText(img.transform, label, fontSize, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, TextMain, FontStyles.Bold, "Label");
            Stretch(t.rectTransform);
            return btn;
        }

        public static Slider AddSlider(Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, float min, float max, float value, UnityAction<float> onChanged)
        {
            var root = NewUI<RectTransform>("Slider", parent);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = anchoredPos;
            root.sizeDelta = sizeDelta;

            var bg = NewUI<Image>("Background", root);
            bg.color = new Color(0.15f, 0.18f, 0.24f, 1f);
            var bgRt = bg.rectTransform;
            bgRt.anchorMin = new Vector2(0f, 0.3f); bgRt.anchorMax = new Vector2(1f, 0.7f);
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

            var fillArea = NewUI<RectTransform>("Fill Area", root);
            fillArea.anchorMin = new Vector2(0f, 0.3f); fillArea.anchorMax = new Vector2(1f, 0.7f);
            fillArea.offsetMin = new Vector2(10f, 0f); fillArea.offsetMax = new Vector2(-30f, 0f);
            var fill = NewUI<Image>("Fill", fillArea);
            fill.color = Accent;
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

            var handleArea = NewUI<RectTransform>("Handle Slide Area", root);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(20f, 0f); handleArea.offsetMax = new Vector2(-20f, 0f);
            var handle = NewUI<Image>("Handle", handleArea);
            handle.color = Color.white;
            handle.rectTransform.sizeDelta = new Vector2(40f, 0f);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min; slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        public static Toggle AddToggle(Transform parent, string label, Vector2 anchoredPos, Vector2 sizeDelta, bool value, UnityAction<bool> onChanged)
        {
            var root = NewUI<Image>("Toggle_" + label, parent);
            root.color = Button;
            var rt = root.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var box = NewUI<Image>("Box", rt);
            box.color = new Color(0.05f, 0.06f, 0.08f, 1f);
            var boxRt = box.rectTransform;
            boxRt.anchorMin = boxRt.anchorMax = new Vector2(0f, 0.5f);
            boxRt.pivot = new Vector2(0f, 0.5f);
            boxRt.anchoredPosition = new Vector2(16f, 0f);
            boxRt.sizeDelta = new Vector2(44f, 44f);

            var check = NewUI<Image>("Checkmark", boxRt);
            check.color = Accent;
            var cRt = check.rectTransform;
            cRt.anchorMin = Vector2.zero; cRt.anchorMax = Vector2.one;
            cRt.offsetMin = new Vector2(8f, 8f); cRt.offsetMax = new Vector2(-8f, -8f);

            var t = AddText(rt, label, 32f, Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, TextMain, FontStyles.Normal, "Label");
            Stretch(t.rectTransform);
            t.rectTransform.offsetMin = new Vector2(76f, 0f);

            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = root;
            toggle.graphic = check;
            var colors = toggle.colors;
            colors.normalColor = Button; colors.highlightedColor = ButtonHover; colors.pressedColor = Color.Lerp(Button, Color.black, 0.25f);
            toggle.colors = colors;
            toggle.SetIsOnWithoutNotify(value);
            if (onChanged != null) toggle.onValueChanged.AddListener(onChanged);
            return toggle;
        }

        static Sprite _white;
        /// <summary>1x1 white sprite; Image.Filled needs a sprite to honour fillAmount.</summary>
        public static Sprite WhiteSprite => _white != null ? _white : (_white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f));

        public static Image AddBar(Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, Color fillColor, string name = "Bar")
        {
            var back = NewUI<Image>(name + "_Back", parent);
            back.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            var rt = back.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            back.raycastTarget = false;

            var fill = NewUI<Image>(name + "_Fill", rt);
            fill.sprite = WhiteSprite;
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            Stretch(fill.rectTransform);
            fill.raycastTarget = false;
            return fill;
        }

        public static T NewUI<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return typeof(T) == typeof(RectTransform) ? (T)(Component)go.transform : go.AddComponent<T>();
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
