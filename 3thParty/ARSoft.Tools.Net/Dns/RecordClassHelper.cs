using System;

namespace ARSoft.Tools.Net.Dns
{
    internal static class RecordClassHelper
    {
        public static string ToShortString(this RecordClass recordClass)
        {
            switch (recordClass)
            {
                case RecordClass.INet:
                    return "IN";
                case RecordClass.Chaos:
                    return "CH";
                case RecordClass.Hesiod:
                    return "HS";
                case RecordClass.None:
                    return "NONE";
                case RecordClass.Any:
                    return "*";
                default:
                    return "CLASS" + (int)recordClass;
            }
        }

        public static bool TryParseShortString(string s, out RecordClass recordClass, bool allowAny = true)
        {
            if (String.IsNullOrEmpty(s))
            {
                recordClass = RecordClass.Invalid;
                return false;
            }

            switch (s.ToUpperInvariant())
            {
                case "IN":
                    recordClass = RecordClass.INet;
                    return true;

                case "CH":
                    recordClass = RecordClass.Chaos;
                    return true;

                case "HS":
                    recordClass = RecordClass.Hesiod;
                    return true;

                case "NONE":
                    recordClass = RecordClass.None;
                    return true;

                case "*":
                    if (allowAny)
                    {
                        recordClass = RecordClass.Any;
                        return true;
                    }
                    else
                    {
                        recordClass = RecordClass.Invalid;
                        return false;
                    }

                default:
                    if (s.StartsWith("CLASS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ushort classValue;
                        if (UInt16.TryParse(s.Substring(5), out classValue))
                        {
                            recordClass = (RecordClass)classValue;
                            return true;
                        }
                    }
                    recordClass = RecordClass.Invalid;
                    return false;
            }
        }
    }
}
