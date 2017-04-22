using System;
using System.Collections;

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
            int val;
            if (null == other)
            {
                val = 1;
                return val;
            }

            int idxThis = Name.LastIndexOf('_');
            int idxOther = other.Name.LastIndexOf('_');

            if (idxThis == -1 || idxOther == -1)
                val = String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
                //val = Name.CompareTo(other.Name);
            else
            {
                int pkgNoThis;
                int pkgNoOther;
                bool resultThis = Int32.TryParse(Name.Substring(idxThis + 1), out pkgNoThis);
                bool resultOther = Int32.TryParse(other.Name.Substring(idxOther + 1), out pkgNoOther);

                if (!resultThis || !resultOther)
                    val = String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
                    //val = Name.CompareTo(other.Name);
                else if ((val = String.Compare(Name.Substring(0, idxThis), other.Name.Substring(0, idxOther), StringComparison.OrdinalIgnoreCase)) == 0
                    && (val = Comparer.Default.Compare(pkgNoThis, pkgNoOther)) == 0)
                { }
            }

            return val;
        }
    }
}