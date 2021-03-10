using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunctionExtend
{
    public static class ColorExtension
    {
        public static Color SetA(this Color color, float target)
        {
            return new Color(color.r, color.g, color.b, target);
        }
    }
}
