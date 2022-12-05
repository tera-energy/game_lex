using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class TrInvertImage : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material result = base.materialForRendering;
            result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return result;
        }
    }
}
