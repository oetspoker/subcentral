using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubCentral.Utils {
    public static class Extensions {
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static double CorrectNaN(this double possibleNaN) {
            if (double.IsNaN(possibleNaN))
                possibleNaN = 0;
            return possibleNaN;
        }
    }
}
