using System;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Base class for container items.
    /// </summary>
    internal abstract class ItemBase : IEquatable<ItemBase>, IComparable<ItemBase>
    {
        /// <summary>
        /// Locale for all languages.
        /// </summary>
        public static string LocaleAll { get; } = "all";

        public override string ToString()
        {
            //return string.Format(CultureInfo.InvariantCulture, "{0}", Name ?? "NULL");
            return Name;
        }

        public int CompareTo(ItemBase other)
        {
            int val;
            if (null == other)
            {
                val = 1;
                return val;
            }

            if ((val = String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase/*StringComparison.OrdinalIgnoreCase*/)) == 0
                && (val = String.Compare(Locale, other.Locale, StringComparison.OrdinalIgnoreCase/*StringComparison.OrdinalIgnoreCase*/)) == 0
                && (val = String.Compare(Link, other.Link, StringComparison.OrdinalIgnoreCase/*StringComparison.OrdinalIgnoreCase*/)) == 0)
            {}

            return val;
        }

        //public override int GetHashCode()
        //{
        //    return Code.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj is ItemBase)
        //        return Equals((ItemBase)obj);
            
        //    return false;

        //    //return (null != obj) && (obj is ItemBase) ? Code.Equals(((ItemBase)obj).Code) : false;
        //}

        public bool Equals(ItemBase other)
        {
            if (other == null)
                return false;

            return Code.ToLowerInvariant().Equals(other.Code.ToLowerInvariant());
        }

        ///// <summary>
        ///// Item locale.
        ///// </summary>
        //public Locale Locale { get; set; }
        /// <summary>
        /// locale.
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// icon src.
        /// </summary>
        public string IconSrc { get; set; }
        /// <summary>
        /// icon alt.
        /// </summary>
        public string IconAlt { get; set; }
        /// <summary>
        /// link.
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Item code (for Uri combination).
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Item Link Description (for Uri combination).
        /// </summary>
        public string LinkDescription { get; set; }
    }
}