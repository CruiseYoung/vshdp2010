using System;
using System.Collections;
using System.Collections.Generic;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Package.
    /// </summary>
    internal sealed class Package : IEquatable<Package>, IComparable<Package>
    {
        public override string ToString()
        {
            return Name /*?? "NULL"*/;
        }

        /// <summary>
        /// name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// deployed.
        /// </summary>
        public string Deployed { get; set; }
        /// <summary>
        /// last-modified.
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// package-etag.
        /// </summary>
        public string PackageEtag { get; set; }
        /// <summary>
        /// current-link.
        /// </summary>
        public string CurrentLink { get; set; }
        /// <summary>
        /// product group name.
        /// </summary>
        public string CurrentLinkDescription { get; set; }
        /// <summary>
        /// package-size-bytes.
        /// </summary>
        public long PackageSizeBytes { get; set; }
        /// <summary>
        /// package-size-bytes-uncompressed.
        /// </summary>
        public long PackageSizeBytesUncompressed { get; set; }
        /// <summary>
        /// package-constituent-link.
        /// </summary>
        public string PackageConstituentLink { get; set; }
        /// <summary>
        /// package-constituent-link.
        /// </summary>
        public string PackageConstituentLinkDescription { get; set; }
        /// <summary>
        /// Item locale.
        /// </summary>
        public string Locale { get; set; }

        public bool Equals(Package other)
        {
            if (other == null)
                return false;

            return CurrentLink.ToLowerInvariant().Equals(other.CurrentLink.ToLowerInvariant());
        }

        public int CompareTo(Package other)
        {
            int val = 0;
            if (null == other)
            {
                val = 1;
                return val;
            }

            int idx_this = Name.LastIndexOf('_');
            int idx_other = other.Name.LastIndexOf('_');

            if (idx_this == -1 || idx_other == -1)
                val = string.Compare(Name, other.Name, true);
                //val = Name.CompareTo(other.Name);
            else
            {
                int pkgNo_this = 0;
                int pkgNo_other = 0;
                bool result_this = Int32.TryParse(Name.Substring(idx_this + 1), out pkgNo_this);
                bool result_other = Int32.TryParse(other.Name.Substring(idx_other + 1), out pkgNo_other);

                if (!result_this || !result_other)
                    val = string.Compare(Name, other.Name, true);
                    //val = Name.CompareTo(other.Name);
                else if ((val = string.Compare(Name.Substring(0, idx_this), other.Name.Substring(0, idx_other), true)) == 0
                    && (val = Comparer.Default.Compare(pkgNo_this, pkgNo_other)) == 0)
                { }
            }

            return val;
        }
    }
}