using System;
using System.Collections.Generic;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Products group.
    /// </summary>
    internal sealed class ProductsGroup : ItemBase
    {
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
        public string GroupCode { get; set; }
        /// <summary>
        /// Group URL.
        /// </summary>
        public string GroupCodeLink { get; set; }
        /// <summary>
        /// Group code Description(for Uri combination).
        /// </summary>
        public string GroupCodeDescription { get; set; }
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
        /// Vendor name.
        /// </summary>
        public string Vendor { get; set; }
        /// <summary>
        /// Last modified time.
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Packages.
        /// </summary>
        public ICollection<Package> Packages { get; set; }
        /// <summary>
        /// Item description.
        /// </summary>
        //public string Description { get; set; }
        /// <summary>
        /// product-link.
        /// </summary>
        public string ProductLink { get; set; }
        /// <summary>
        /// product-name.
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// product-group-link.
        /// </summary>
        public string ProductGroupLink { get; set; }
        /// <summary>
        /// product-group-name.
        /// </summary>
        public string ProductGroupName { get; set; }
    }
}