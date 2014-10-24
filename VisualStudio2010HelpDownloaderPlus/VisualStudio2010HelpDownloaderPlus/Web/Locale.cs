using System;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    internal sealed class Locale : IComparable<Locale>
    {
        private readonly static Locale _localeAll = new Locale() { Code = "all" };

        private static string NormalizeLocale(string value)
        {
            // "en-us" -> "en-US"

            if (null == value)
                return null;

            string[] parts = value.Split('-');

            if (parts.Length != 2)
                return value;

            return string.Join("-", parts[0], parts[1]/*.ToUpperInvariant()*/);
        }

        public int CompareTo(Locale other)
        {
            return null == other ? 1 : string.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (null != obj) && (obj is Locale) ? Code.Equals(((Locale)obj).Code) : false;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Locale for all languages.
        /// </summary>
        public static Locale LocaleAll
        {
            get { return _localeAll; }
        }
        
        /// <summary>
        /// Normalized locale name isplay.
        /// </summary>
        public string Name
        {
            get
            {
                return NormalizeLocale(Code);
            }
        }
        /// <summary>
        /// Locale code.
        /// </summary>
        public string Code { get; set; }
    }
}