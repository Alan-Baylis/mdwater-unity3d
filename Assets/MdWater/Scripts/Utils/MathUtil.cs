using UnityEngine;
using System;
using System.Collections;

namespace MynjenDook
{
    public static class MathUtil
    {
        static public double modf(double x, out double integer)
        {
            integer = Math.Floor(x);
            double fraction = x - integer;

            #if DEBUG
            if (fraction < 0)
                fraction = 0; // 小数部分
            #endif

            return fraction;
        }
    }
}
