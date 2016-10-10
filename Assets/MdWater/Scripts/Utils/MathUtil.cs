using UnityEngine;
using System;
using System.Collections;

namespace MynjenDook
{
    public static class MathUtil
    {
        static public double modf(double x, out double integer)
        {
            integer = Math.Round(x);
            return x - integer;
        }
    }
}
