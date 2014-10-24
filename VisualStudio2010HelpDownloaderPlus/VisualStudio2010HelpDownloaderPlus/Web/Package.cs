using System;
using System.Collections.Generic;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Package.
    /// </summary>
    internal sealed class Package : IEquatable<Package>
    {
        public override string ToString()
        {
            return Name ?? "NULL";
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Deployed.
        /// </summary>
        public string Deployed { get; set; }
        /// <summary>
        /// Last modified time.
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Tag.
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// Package URL for downloading.
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// package-size-bytes.
        /// </summary>
        public long SizeBytes { get; set; }
        /// <summary>
        /// package-size-bytes-uncompressed.
        /// </summary>
        public long SizeBytesUncompressed { get; set; }
        /// <summary>
        /// package-constituent-link.
        /// </summary>
        public string ConstituentLink { get; set; }
        /// <summary>
        /// Item locale.
        /// </summary>
        public Locale Locale { get; set; }

        public bool Equals(Package other)
        {
            if (this.Link == other.Link )
                return true;
            else
                return false;
        }

    }
}