using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmicCoreApi.Helper
{
    public static class MathHelper
    {
        public static decimal TruncateDecimal(decimal? value)
        {
            if (value == null) return 0;

            var editGstTotal = Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
            return editGstTotal;
        }
    }
}