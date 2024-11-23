using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Character : MonoBehaviour
{
    private TMPWrap textMeshPro;
    private RectTransform m_RectTransform;

    public TMP_FontAsset fontAsset;
    public FontStyles style;

    public char character;
    public float fontSize;

    public Color color;

    public bool enableNonRoman;

    // TODO
    public Vector2 size { get; set; }

    public Vector2 anchoredPosition => m_RectTransform.anchoredPosition;

    private void Awake()
    {
        m_RectTransform = this.GetComponent<RectTransform>();
        textMeshPro = this.GetComponentInChildren<TMPWrap>();
        textMeshPro.raycastTarget = false;
    }

    public void UpdateContent()
    {
        this.textMeshPro.rectTransform.localScale = Vector3.one;
        textMeshPro.fontStyle = style;
        textMeshPro.text = character.ToString();
        textMeshPro.fontSize = fontSize;
        textMeshPro.color = color;
        if (fontAsset)
        {
            textMeshPro.font = fontAsset;
        }
        else
        {
            textMeshPro.font = FontManager.Instance.GetDefaultFontAsset();
            fontAsset = textMeshPro.font;
        }

        if (!textMeshPro.isDirty)
        {
            return;
        }

        this.size = new Vector2(textMeshPro.preferredWidth, fontSize);
        if (character == ' ')
        {
            this.size = new Vector2(this.size.x, MathUtils.TMPSpaceEm(fontAsset) * fontSize);
        }

        if (enableNonRoman)
        {
            if (fontAsset.HasCharacter(character))
            {
                var glyph = fontAsset.characterLookupTable[character].glyph;
                var baseSize = fontAsset.faceInfo.pointSize;
                var ascent = fontAsset.faceInfo.ascentLine;
                var decent = fontAsset.faceInfo.descentLine;
                var offset_base_top = (ascent / baseSize) * fontSize;
                var span_bottom = fontSize - offset_base_top;


                var y = glyph.metrics.horizontalBearingY;
                var h = glyph.metrics.height;
                var b = y - h;

                var need = (-b / baseSize) * fontSize;
                var delta = need - span_bottom;
                if (delta > 0)
                {
                    this.size = new Vector2(this.size.x, this.size.y + delta);
                }
            }

            textMeshPro.rectTransform.sizeDelta = this.size;

            if (character < 0x7f && character != ' ')
            {
                // var glyph = fontAsset.glyphLookupTable[character];
                // glyph.metrics
                // textMeshPro.alignment = TextAlignmentOptions.Center;
                textMeshPro.transform.localEulerAngles = new Vector3(0, 0, -90);
                this.size = new Vector2(this.size.y, this.size.x);
            }
            else
            {
                // textMeshPro.alignment = TextAlignmentOptions.Top;
                textMeshPro.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
        else
        {
            textMeshPro.rectTransform.sizeDelta = this.size;
            // textMeshPro.alignment = TextAlignmentOptions.Top;
            textMeshPro.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        this.m_RectTransform.sizeDelta = this.size;
        textMeshPro.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetAnchoredPosition(float x, float y)
    {
        m_RectTransform.anchoredPosition = new Vector2(x, y);
    }

    public void SetActive(bool active)
    {
        if (!active)
        {
            this.textMeshPro.rectTransform.localScale = Vector3.zero;
        }
        else
        {
            this.textMeshPro.rectTransform.localScale = Vector3.one;
        }
    }
}