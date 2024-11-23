using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPWrap : TextMeshProUGUI
{
    public bool isDirty => m_isPreferredHeightDirty || m_isPreferredWidthDirty || m_isLayoutDirty;
}
