using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Ext;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum UniTextLayoutType
{
    Horizontal,
    RomanVertical,
    Vertical
}

public enum UniTextAlign
{
    LeftOrTop,
    Center,
    RightOrBottom,
}

[ExecuteInEditMode]
public class UniText : MonoBehaviour
{
    // protected Transform uniTextPoolRoot;
    protected Stack<Character> pools = new Stack<Character>();

    // Return Character disactive
    protected Character GetCharacterComponent()
    {
        // if (!uniTextPoolRoot)
        // {
        //     var tmp = new GameObject("uniTextPoolRoot");
        //     uniTextPoolRoot = tmp.transform;
        //     uniTextPoolRoot.gameObject.SetActive(true);
        //     pools = new Queue<Character>();
        // }

        if (pools.Count > 0)
        {
            return pools.Pop();
        }

        // left top point
        var text = Resources.Load<Character>("Character");

        var obj = GameObject.Instantiate(text);
        obj.transform.SetParent(this.transform);
        obj.transform.localScale = Vector3.one;
        return obj;
    }


    protected void SendCharacterToPool(Character c)
    {
        // if (!uniTextPoolRoot)
        // {
        //     var tmp = new GameObject("uniTextPoolRoot");
        //     uniTextPoolRoot = tmp.transform;
        //     uniTextPoolRoot.gameObject.SetActive(true);
        //     pools = new Queue<Character>();
        // }

        if (c == null)
        {
            return;
        }

        c.SetActive(false);
        // c.transform.SetParent(uniTextPoolRoot);
        pools.Push(c);
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    protected static void Reset()
    {
        // uniTextPoolRoot = null;
        // pools?.Clear();
        //
        // var objs = GameObject.FindObjectsOfType<GameObject>(true);
        // foreach (var obj in objs)
        // {
        //     if (obj.name != "uniTextPoolRoot")
        //     {
        //         continue;
        //     }
        //
        //     GameObject.DestroyImmediate(obj);
        // }
    }
#endif

    public TMP_FontAsset fontAsset;
    private TMP_FontAsset o_fontAsset;

    public float fontSize = 30;
    private float o_fontSize = -1;
    [TextArea] public string text;
    private string o_text;
    public Color color = Color.black;
    private Color o_color = default;
    public FontStyles style = FontStyles.Normal;
    private FontStyles o_style = default;
    public Vector2 size { get; private set; }

    public UniTextLayoutType layoutType = UniTextLayoutType.Horizontal;
    private UniTextLayoutType o_layoutType = default;


    // 100 = 1em
    public float lineSpace;
    private float o_lineSpace;
    public float characterSpace;
    private float o_characterSpace;
    public UniTextAlign align = UniTextAlign.LeftOrTop;
    private UniTextAlign o_align = UniTextAlign.LeftOrTop;

    protected RectTransform m_rectTransform;
    protected List<Character> m_characters = new List<Character>();

    private void Awake()
    {
        m_rectTransform = this.GetComponent<RectTransform>();
        m_rectTransform.pivot = new Vector2(0, 1);
    }

    protected void Clean()
    {
        for (int i = m_characters.Count - 1; i >= 0; i--)
        {
            var c = m_characters[i];
            SendCharacterToPool(c);
        }

        m_characters.Clear();
    }

    [ContextMenu("Build")]
    public void Build()
    {
        Clean();
        if (text == null)
        {
            text = "";
        }

        // fontAsset.TryAddCharacters(text);
        m_characters.Capacity = text.Length;

        bool enableNonRoman = layoutType == UniTextLayoutType.Vertical;
        foreach (char character in text)
        {
            if (character == '\n')
            {
                m_characters.Add(null);
                continue;
            }

            var charComp = GetCharacterComponent();
            charComp.SetActive(true);
            // charComp.transform.SetParent(this.transform);
            ;
            charComp.character = character;
            charComp.enableNonRoman = enableNonRoman;
            charComp.style = style;
            charComp.fontSize = fontSize;
            charComp.color = color;
            charComp.fontAsset = fontAsset;

            charComp.UpdateContent();

            m_characters.Add(charComp);
        }

        if (layoutType == UniTextLayoutType.Horizontal)
        {
            BuildHorizontal();
        }
        else if (layoutType == UniTextLayoutType.Vertical)
        {
            BuildVertical();
        }
        else if (layoutType == UniTextLayoutType.RomanVertical)
        {
            BuildVertical();
        }
    }

    protected void BuildHorizontal()
    {
        float x_point = 0;
        float y_point = 0;

        float lineHeight = fontSize * (lineSpace + 100f) / 100f;
        List<float> widths = new List<float>();
        float maxWidth = 0;
        float height = 0;
        int line = 0;
        widths.Add(0);
        foreach (var character in m_characters)
        {
            if (character == null)
            {
                x_point = 0;
                y_point -= lineHeight;
                line++;
                widths.Add(0);
                continue;
            }

            character.SetAnchoredPosition(x_point, y_point);
            height = y_point;
            var c_size = character.size;

            // right edge
            x_point += c_size.x;

            widths[line] = x_point;
            maxWidth = Mathf.Max(maxWidth, x_point);
            // next x point
            x_point += (characterSpace * fontSize / 100f);
        }

        height -= fontSize;
        height *= -1;

        if (align != UniTextAlign.LeftOrTop)
        {
            line = 0;
            foreach (var character in m_characters)
            {
                if (character == null)
                {
                    line++;
                    continue;
                }

                var offset = (maxWidth - widths[line]);
                if (align == UniTextAlign.Center)
                {
                    offset /= 2;
                }

                var pos = character.anchoredPosition;

                character.SetAnchoredPosition(pos.x + offset, pos.y);
            }
        }

        this.size = new Vector2(maxWidth, height);
        this.m_rectTransform.sizeDelta = this.size;
    }

    protected void BuildVertical()
    {
        float gap = fontSize * lineSpace / 100f;

        bool nonRoman = layoutType == UniTextLayoutType.Vertical;

        //prepare
        float cur_max_width = fontSize;
        List<float> maxWidth = new List<float>();
        for (int i = 0; i < m_characters.Count; i++)
        {
            var character = m_characters[i];
            if (character == null)
            {
                maxWidth.Add(cur_max_width);
                cur_max_width = fontSize;
                continue;
            }

            var c_size = character.size;
            cur_max_width = Mathf.Max(c_size.x, cur_max_width);

            if (i == m_characters.Count - 1)
            {
                maxWidth.Add(cur_max_width);
            }
        }

        // 当前一竖
        float x_point = 0;
        float y_point = 0;
        float left_x_point = 0;
        int cur_y = 0;
        List<float> heights = new List<float>();
        heights.Add(0);
        float minHeight = 0;
        for (int i = 0; i < m_characters.Count; i++)
        {
            var character = m_characters[i];
            if (character == null)
            {
                y_point = 0;
                var cur_v_width = maxWidth[cur_y];
                x_point -= cur_v_width;
                x_point -= (characterSpace * fontSize / 100f);
                cur_y++;
                heights.Add(0);
                continue;
            }

            left_x_point = x_point;
            float offset = 0;
            var size = character.size;
            if (nonRoman)
            {
                offset = cur_max_width - size.x;
            }
            else
            {
                offset = (cur_max_width - size.x) / 2;
            }

            character.SetAnchoredPosition(x_point + offset, y_point);
            var c_size = character.size;

            // bottom edge
            y_point -= (c_size.y);

            heights[cur_y] = y_point;
            minHeight = Mathf.Min(y_point, minHeight);
            // next y point
            y_point -= gap;
        }

        cur_y = 0;
        for (int i = 0; i < m_characters.Count; i++)
        {
            var character = m_characters[i];
            if (character == null)
            {
                cur_y++;
                continue;
            }

            var pos = character.anchoredPosition;

            pos.x -= left_x_point;
            if (align != UniTextAlign.LeftOrTop)
            {
                var offset = minHeight - heights[cur_y];
                if (align == UniTextAlign.Center)
                {
                    offset /= 2;
                }

                pos.y += offset;
            }

            character.SetAnchoredPosition(pos.x, pos.y);
        }

        if (maxWidth.Count > 0)
        {
            float width = maxWidth[0] - left_x_point;
            float height = -minHeight;
            size = new Vector2(width, height);
            this.m_rectTransform.sizeDelta = size;
        }
        else
        {
            float width = 0;
            float height = 0;
            size = Vector2.zero;
            this.m_rectTransform.sizeDelta = new Vector2(0, 0);
        }
    }

// #if UNITY_EDITOR
    protected void LateUpdate()
    {
        bool dirty = false;
        if (fontSize != o_fontSize)
        {
            dirty = true;
        }
        else if (fontAsset != o_fontAsset)
        {
            dirty = true;
        }
        else if (layoutType != o_layoutType)
        {
            dirty = true;
        }
        else if (text != o_text)
        {
            dirty = true;
        }
        else if (color != o_color)
        {
            dirty = true;
        }
        else if (style != o_style)
        {
            dirty = true;
        }
        else if (lineSpace != o_lineSpace)
        {
            dirty = true;
        }
        else if (characterSpace != o_characterSpace)
        {
            dirty = true;
        }
        else if (align != o_align)
        {
            dirty = true;
        }

        o_fontSize = fontSize;
        o_fontAsset = fontAsset;
        o_layoutType = layoutType;
        o_text = text;
        o_color = color;
        o_style = style;
        o_lineSpace = lineSpace;
        o_characterSpace = characterSpace;
        o_align = align;

        if (dirty)
        {
            Build();
        }
    }
}