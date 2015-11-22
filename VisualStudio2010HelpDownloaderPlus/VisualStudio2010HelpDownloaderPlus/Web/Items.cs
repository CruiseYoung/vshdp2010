using System;
using System.Collections.Generic;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// product-group.
    /// </summary>
    internal sealed class ProductGroup : ItemBase
    {
        /// <summary>
        /// product-groups-link.
        /// </summary>
        public string ProductGroupsLink { get; set; }
        /// <summary>
        /// Group code Description(for Uri combination).
        /// </summary>
        public string ProductGroupsDescription { get; set; }

        /// <summary>
        /// Products.
        /// </summary>
        public ICollection<Product> Products { get; set; }
     }

    /// <summary>
    /// Product.
    /// </summary>
    internal sealed class Product : ItemBase
    {
        /// <summary>
        /// Group code (for Uri combination).
        /// </summary>
        public string ProductGroupCode { get; set; }
        /// <summary>
        /// product-group-link.
        /// </summary>
        public string ProductGroupLink { get; set; }
        /// <summary>
        /// Group code Description(for Uri combination).
        /// </summary>
        public string ProductGroupDescription { get; set; }

        /// <summary>
        /// product-groups-link.
        /// </summary>
        public string ProductGroupsLink { get; set; }
        /// <summary>
        /// Group code Description(for Uri combination).
        /// </summary>
        public string ProductGroupsDescription { get; set; }
        ///// <summary>
        ///// last-modified.
        ///// </summary>
        //public DateTime LastModified { get; set; }

        /// <summary>
        /// Books.
        /// </summary>
        public ICollection<Book> Books { get; set; }
    }

    /// <summary>
    /// Book.
    /// </summary>
    internal sealed class Book : ItemBase
    {
        /// <summary>
        /// vendor.
        /// </summary>
        public string Vendor { get; set; }
        /// <summary>
        /// last-modified.
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// product-link.
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// product-link.
        /// </summary>
        public string ProductLink { get; set; }
        /// <summary>
        /// product name.
        /// </summary>
        public string ProductDescription { get; set; }
        /// <summary>
        /// product-group-link.
        /// </summary>
        public string ProductGroupCode { get; set; }
        /// <summary>
        /// product-group-link.
        /// </summary>
        public string ProductGroupLink { get; set; }
        /// <summary>
        /// product group name.
        /// </summary>
        public string ProductGroupDescription { get; set; }

        /// <summary>
        /// Packages.
        /// </summary>
        public ICollection<Package> Packages { get; set; }
    }
}