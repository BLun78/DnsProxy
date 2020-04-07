using System;

namespace ARSoft.Tools.Net.Dns
{

    internal static class RecordTypeHelper
    {
        public static string ToShortString(this RecordType recordType)
        {
            string res;
            if (!EnumHelper<RecordType>.Names.TryGetValue(recordType, out res))
            {
                return "TYPE" + (int)recordType;
            }
            return res.ToUpper();
        }

        public static bool TryParseShortString(string s, out RecordType recordType)
        {
            if (String.IsNullOrEmpty(s))
            {
                recordType = RecordType.Invalid;
                return false;
            }

            if (EnumHelper<RecordType>.TryParse(s, true, out recordType))
                return true;

            if (s.StartsWith("TYPE", StringComparison.InvariantCultureIgnoreCase))
            {
                ushort classValue;
                if (UInt16.TryParse(s.Substring(4), out classValue))
                {
                    recordType = (RecordType)classValue;
                    return true;
                }
            }
            recordType = RecordType.Invalid;
            return false;
        }

        public static RecordType ParseShortString(string s)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentOutOfRangeException(nameof(s));

            RecordType recordType;
            if (EnumHelper<RecordType>.TryParse(s, true, out recordType))
                return recordType;

            if (s.StartsWith("TYPE", StringComparison.InvariantCultureIgnoreCase))
            {
                ushort classValue;
                if (UInt16.TryParse(s.Substring(4), out classValue))
                    return (RecordType)classValue;
            }

            throw new ArgumentOutOfRangeException(nameof(s));
        }
    }
}
