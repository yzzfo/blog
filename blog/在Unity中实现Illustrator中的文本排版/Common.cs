using TMPro;
using UnityEngine;

namespace Common
{
    public static class MathUtils
    {
        public static Vector3 Match2(this Vector3 v, Vector3 target)
        {
            var scale = Mathf.Min(target.x / v.x, target.y / v.y, target.z / v.z);
            return scale * v;
        }

        public static Vector2 Match2(this Vector2 v, Vector2 target)
        {
            var scale = Mathf.Min(target.x / v.x, target.y / v.y);
            return scale * v;
        }

        public static float Scale2(this Vector3 v, Vector3 target)
        {
            var scale = Mathf.Min(target.x / v.x, target.y / v.y, target.z / v.z);
            return scale;
        }

        public static float Scale2(this Vector2 v, Vector2 target)
        {
            var scale = Mathf.Min(target.x / v.x, target.y / v.y);
            return scale;
        }

        public static float CaculateTMPLinePixelPerEm(float fontSize)
        {
            return fontSize / 100f;
        }

        public static float Pixel2EmInTMP(float pixel, float fontsize)
        {
            var pixelPerEm = CaculateTMPLinePixelPerEm(fontsize);
            return pixel / pixelPerEm;
        }

        // 计算文字大小倍率，例如FontSize等于12px时，实际上他还要乘多少才能到真实高度
        public static float FontScale(TMP_FontAsset asset)
        {
            var fontHeight = asset.faceInfo.ascentLine - asset.faceInfo.descentLine;
            var sample = asset.faceInfo.pointSize;
            return fontHeight / sample;
        }

        public static Vector2 AI2UnityOffset(string fontName, float fontSize, Vector2 size, Vector3 localScale,
            bool isV)
        {
            return -Unity2AIOffset(fontName, fontSize, size, localScale, isV);
        }

        public static float TMPSpaceEm(TMP_FontAsset asset)
        {
            var glyph = asset.glyphTable[0];
            var w = glyph.metrics.horizontalAdvance;
            return w / asset.faceInfo.pointSize;
        }
    }
}