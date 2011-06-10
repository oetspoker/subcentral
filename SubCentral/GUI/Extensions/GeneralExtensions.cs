using System;
using NLog;

namespace SubCentral.GUI.Extensions {
    public static class GeneralExtensions {
          private static Logger logger = LogManager.GetCurrentClassLogger();

      #region String extensions
      public static bool IsNullOrWhiteSpace(this string value)
      {
          return (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim()));
      }
      #endregion
    }
}
