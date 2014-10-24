using System.Globalization;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Base class for container items.
    /// </summary>
    internal abstract class ItemBase
    {
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} [{1}]", Name ?? "NULL", Locale.Name ?? "NULL");
        }

        /// <summary>
        /// Item locale.
        /// </summary>
        public Locale Locale { get; set; }
        /// <summary>
        /// Item name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Item description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Item code (for Uri combination).
        /// </summary>
        public string Code { get; set; }
		/// <summary>
        /// Item URL.
        /// </summary>
        public string CodeLink { get; set; }
        /// <summary>
        /// Item code Description (for Uri combination).
        /// </summary>
        public string CodeDescription { get; set; }
        /// <summary>
        /// icon src.
        /// </summary>
        public string IconLink { get; set; }
        /// <summary>
        /// icon alt.
        /// </summary>
        public string IconLinkDescription { get; set; }
    }
}