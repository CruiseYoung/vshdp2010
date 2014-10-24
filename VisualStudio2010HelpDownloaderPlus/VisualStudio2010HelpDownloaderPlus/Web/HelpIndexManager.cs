using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Helper class for parsing and creating documents with help indexes.
    /// </summary>
    internal static class HelpIndexManager
    {
        public const string BRANDING_PACKAGE_NAME1 = "dev10";
        public const string BRANDING_PACKAGE_NAME2 = "dev10-ie6";

        #region XElement extension

        private static string GetClassName(this XElement element)
        {
            return GetAttributeValue(element, "class");
        }

        private static string GetAttributeValue(this XElement element, string name)
        {
            var attribute = element.Attribute(XName.Get(name, string.Empty));

            return attribute == null ? null : attribute.Value;
        }

        private static string GetChildClassValue(this XElement element, string name)
        {
            var result = element.Elements()
                .Where(x => x.GetClassName() == name).Take(1).Single();

            return null != result ? result.Value : null;
        }

        private static string GetChildClassAttributeValue(this XElement element, string name, string attribute)
        {
            var result = element.Elements()
                .Where(x => x.GetClassName() == name).Take(1).Single();

            return null != result ? result.GetAttributeValue(attribute) : null;
        }

        #endregion

        private static string ToStringWithDeclaration(this XDocument document)
        {
            return null == document.Declaration ?
                document.ToString() :
                string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}", Environment.NewLine, document.Declaration.ToString(), document.ToString());
        }

        private static string CreateCodeFromUrl(string url)
        {
            if (null == url)
                return null;

            string[] parts = url.Split('/');

            return parts.Length > 0 ? parts[parts.Length - 1] : null;
        }

        /// <summary>
        /// Create file name for some items.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <returns>File name.</returns>
        public static string CreateItemFileName(ItemBase item, string locLanguage)
        {
            if (null == item)
                return null;

            if (item is Product)
            {
                if (null == locLanguage)
                    return string.Format(CultureInfo.InvariantCulture, "product-{0}.html", item.Code);
                else
                    return string.Format(CultureInfo.InvariantCulture, "product-{0}({1}).html", item.Code, locLanguage);
            }
            else if (item is Book)
                return string.Format(CultureInfo.InvariantCulture, "book-{0}-{1}.html", item.Code, item.Locale.Name);
            else
                return null;
        }

        /// <summary>
        /// Create file name for package.
        /// </summary>
        /// <param name="package">Package.</param>
        /// <returns>File name.</returns>
        public static string CreatePackageFileName(Package package)
        {
            if (null == package)
                return null;

            if ((package.Name != BRANDING_PACKAGE_NAME1) &&
                (package.Name != BRANDING_PACKAGE_NAME2))
                return string.Format(CultureInfo.InvariantCulture, "{0}({1}).cab", package.Name/*.ToLowerInvariant()*/, package.Tag);
            else
                return string.Format(CultureInfo.InvariantCulture, "{0}.cab", package.Name/*.ToLowerInvariant()*/);
        }

        #region Load information methods

        /// <summary>
        /// Load products groups from document.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <returns>List of products groups.</returns>
        public static ICollection<ProductsGroup> LoadProductGroups(byte[] data)
        {
            Contract.Ensures(null != Contract.Result<ICollection<ProductsGroup>>());

            XDocument document = null;

            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var query = document.Root.Elements()
                .Where(x => x.GetClassName() == "product-groups").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product-group-list").Take(1).Single().Elements()
                        .Where(x => x.GetClassName() == "product-group");

            var result = new List<ProductsGroup>();

            foreach (var x in query)
                result.Add(
                    new ProductsGroup()
                    {
                        Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                        Name = x.GetChildClassValue("name"),
                        Description = x.GetChildClassValue("description"),
                        //IconLink = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
						IconLink = x.GetChildClassAttributeValue("icon", "src"),
                        IconLinkDescription = x.GetChildClassAttributeValue("icon", "alt"),
                        Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-group-link", "href")),
						CodeLink = x.GetChildClassAttributeValue("product-group-link", "href"),
                        CodeDescription = x.GetChildClassValue("product-group-link")
                    }
                );

            return result;
        }

        /// <summary>
        /// Load products from document.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <returns>List of products.</returns>
        public static ICollection<Product> LoadProducts(byte[] data)
        {
            Contract.Ensures(null != Contract.Result<ICollection<Product>>());

            XDocument document = null;

            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var query = document.Root.Elements()
                .Where(x => x.GetClassName() == "product-group").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product-list").Take(1).Single().Elements()
                        .Where(x => x.GetClassName() == "product");

            var result = new List<Product>();

            //foreach (var x in query)
            //    result.Add(
            //        new Product()
            //        {
            //            Locale = new Locale() { Code = x.GetChildClassValue("locale") },
            //            Name = x.GetChildClassValue("name"),
            //            Description = x.GetChildClassValue("description"),
            //            IconLink = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
            //            IconLinkDescription = x.GetChildClassAttributeValue("icon", "alt"),
            //            Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-link", "href")),
            //            CodeDescription = x.GetChildClassValue("product-link")
            //        }
            //    );

            foreach (var x in query)
            {
                var product = new Product()
                    {
                        Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                        Name = x.GetChildClassValue("name"),
                        Description = x.GetChildClassValue("description"),
                        //IconLink = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
						IconLink = x.GetChildClassAttributeValue("icon", "src"),
                        IconLinkDescription = x.GetChildClassAttributeValue("icon", "alt"),
                        Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-link", "href")),
						CodeLink = x.GetChildClassAttributeValue("product-link", "href"),
                        CodeDescription = x.GetChildClassValue("product-link")
                    };

                var bAdd = true;
                foreach (var y in result)
                {
                    if (y.Code == product.Code)
                    {
                        bAdd = false;
                        break;
                    }
                }

                //if (product.Name == "Microsoft BizTalk Server 2010"
                //    || product.Name == "SharePoint Products and Technologies"
                //    || product.Name == "SQL Server Denali"
                //    )
                //{
                //    continue;
                //}

                if (bAdd)
                    result.Add(product);
            }

            return result;
        }

        /// <summary>
        /// Load books from document.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <returns>List of books.</returns>
        public static Tuple<ICollection<Book>, IList<Locale>> LoadBooks(byte[] data)
        {
            Contract.Ensures(null != Contract.Result<Tuple<ICollection<Book>, IList<Locale>>>());

            XDocument document = null;

            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            string currentLink = document.Root.Elements()
                .Where(x => x.GetClassName() == "product").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "book-list").Take(1).Single().GetChildClassAttributeValue("book-list-link", "href");

            var result = new List<Book>();
            var locales = new List<Locale>();

            if (null != currentLink)
            {
                var query = document.Root.Elements()
                .Where(x => x.GetClassName() == "product").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "book-list").Take(1).Single().Elements()
                        .Where(x => x.GetClassName() == "book");

                foreach (var x in query)
                {
                    string code = x.GetChildClassAttributeValue("book-link", "href").Replace(currentLink, null).TrimStart('/');

                    if (code.Contains('/'))
                        code = code.Substring(0, code.IndexOf('/'));

                    var book = new Book()
                        {
                            Name = x.GetChildClassValue("name"),
                            Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                            Description = x.GetChildClassValue("description"),
                            //IconLink = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
							IconLink = x.GetChildClassAttributeValue("icon", "src"),
                            IconLinkDescription = x.GetChildClassAttributeValue("icon", "alt"),
                            Code = code,
							CodeLink = x.GetChildClassAttributeValue("book-link", "href"),
                            CodeDescription = x.GetChildClassValue("book-link")
                        };
                    
                    result.Add(book);

                    if (!locales.Contains(book.Locale))
                        locales.Add(book.Locale);
                }
            }

            return new Tuple<ICollection<Book>,IList<Locale>>(result, locales);
        }
        
        /// <summary>
        /// Load packages from document.
        /// </summary>
        /// <param name="book">Packages book.</param>
        /// <param name="data">Document data.</param>
        /// <returns>List of packages.</returns>
        public static ICollection<Package> LoadPackages(Book book, byte[] data)
        {
            Contract.Requires(null != book);
            Contract.Ensures(null != Contract.Result<ICollection<Package>>());

            XDocument document = null;

            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var detailsElement = document.Root.Elements()
                .Where(x => x.GetClassName() == "book").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "details").Take(1).Single();

            book.Vendor = detailsElement.GetChildClassValue("vendor");
            book.Locale = new Locale() { Code = detailsElement.GetChildClassValue("locale") };
            book.Name = detailsElement.GetChildClassValue("name");
            book.Description = detailsElement.GetChildClassValue("description");
            book.LastModified = DateTime.Parse(detailsElement.GetChildClassValue("last-modified"));
            book.ProductLink = detailsElement.GetChildClassAttributeValue("product-link", "href");
            book.ProductName = detailsElement.GetChildClassValue("product-link");
            book.ProductGroupLink = detailsElement.GetChildClassAttributeValue("product-group-link", "href");
            book.ProductGroupName = detailsElement.GetChildClassValue("product-group-link");
            
            var query = document.Root.Elements()
                .Where(x => x.GetClassName() == "book").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "package-list").Take(1).Single().Elements()
                        .Where(x => x.GetClassName() == "package");

            var result = new List<Package>();

            foreach (var x in query)
            {
                string name = x.GetChildClassValue("name");

                result.Add(
                    new Package()
                    {
                        Name = name,
                        Deployed = x.GetChildClassValue("deployed"),
                        LastModified = DateTime.Parse(x.GetChildClassValue("last-modified")),
                        Tag = x.GetChildClassValue("package-etag"),
                        Link = x.GetChildClassAttributeValue("current-link", "href"),
                        SizeBytes = long.Parse(x.GetChildClassValue("package-size-bytes"), CultureInfo.InvariantCulture),
                        SizeBytesUncompressed = long.Parse(x.GetChildClassValue("package-size-bytes-uncompressed"), CultureInfo.InvariantCulture),
                        ConstituentLink = x.GetChildClassAttributeValue("package-constituent-link", "href"),
                        Locale = book.Locale //new Locale() { Code = detailsElement.GetChildClassValue("locale") }
                    }
                );
            }

            return result;
        }

        #endregion

        #region Create indexes methods

        private static XElement CreateElement(string name, string className, string value)
        {
            var element = new XElement(XName.Get(name, "http://www.w3.org/1999/xhtml"));
            
            if (null != className)
                element.SetAttributeValue(XName.Get("class", string.Empty), className);
            if (null != value)
                element.Value = value;

            return element;
        }

        /// <summary>
        /// Creates main help setup index.
        /// </summary>
        /// <param name="products">Products list.</param>
        /// <returns>Document data.</returns>
        public static string CreateSetupIndex(IEnumerable<Product> products, string locLanguage)
        {
            Contract.Requires(null != products);

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                CreateElement("html", null, null));

            var bodyElement = CreateElement("body", "product-list", null);

            var iconElement = CreateElement("img", "icon", null);
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), "../../../serviceapi/content/image/ic298417");
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), "VS 100 Icon");

            foreach (var product in products)
            {
                var productElement = CreateElement("div", "product", null);

                var iconProductElement = CreateElement("img", "icon", null);
                iconProductElement.SetAttributeValue(XName.Get("src", string.Empty), product.IconLink);
                iconProductElement.SetAttributeValue(XName.Get("alt", string.Empty), product.IconLinkDescription);

                bool includeLocLanguage = false;
                if (locLanguage.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant())
                {
                    includeLocLanguage = true;
                }
                else if (locLanguage.ToLowerInvariant() != "en-us")
                {
                    foreach (var book in product.Books)
                    {
                        if (book.Locale.Name.ToLowerInvariant() == locLanguage.ToLowerInvariant())
                        {
                            includeLocLanguage = true;
                            break;
                        }
                    }
                }

                var linkElement = CreateElement("a", "product-link", product.CodeDescription);
                if (!includeLocLanguage)
                    linkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    CreateItemFileName(product, null));
                else
                    linkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    CreateItemFileName(product, locLanguage));
                
                productElement.Add(
                    CreateElement("span", "locale", product.Locale.Code),
                    CreateElement("span", "name", product.Name),
                    CreateElement("span", "description", product.Description),
                    iconProductElement,
                    linkElement);

                bodyElement.Add(productElement);
            }

            document.Root.Add(bodyElement);

            return document.ToStringWithDeclaration();
        }

        /// <summary>
        /// Create product books index.
        /// </summary>
        /// <param name="product">Product.</param>
        /// <returns>Document data.</returns>
        public static string CreateProductBooksIndex(Product product, string locLanguage)
        {
            Contract.Requires(null != product);
            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                CreateElement("html", null, null));

            var headElement = CreateElement("head", null, null);

            var metaDateElemet1 = CreateElement("meta", null, null);
            metaDateElemet1.SetAttributeValue(XName.Get("name", string.Empty), "ROBOTS");
            metaDateElemet1.SetAttributeValue(XName.Get("content", string.Empty), "NOINDEX, NOFOLLOW");

            var metaDateElemet2 = CreateElement("meta", null, null);
            metaDateElemet2.SetAttributeValue(XName.Get("http-equiv", string.Empty), "Content-Location");
            metaDateElemet2.SetAttributeValue(
                    XName.Get("content", string.Empty),
                    string.Format(CultureInfo.InvariantCulture, @"http://services.mtps.microsoft.com/ServiceAPI/products/{0}/{1}",
                    product.GroupCode, product.Code));

            var metaDateElemet3 = CreateElement("meta", null, null);
            metaDateElemet3.SetAttributeValue(XName.Get("http-equiv", string.Empty), "Date");
            metaDateElemet3.SetAttributeValue(
                    XName.Get("content", string.Empty),
                    DateTime.Now.ToString("R"));

            var linkElement1 = CreateElement("link", null, null);
            linkElement1.SetAttributeValue(XName.Get("type", string.Empty), "text/css");
            linkElement1.SetAttributeValue(XName.Get("rel", string.Empty), "stylesheet");
            linkElement1.SetAttributeValue(XName.Get("href", string.Empty), "../../../serviceapi/styles/global.css");

            var strproduct = string.Format(CultureInfo.InvariantCulture, @"Constituents of Product {0}", product.Code);
            var titleElement = CreateElement("title", null, strproduct);

            headElement.Add(metaDateElemet1);
            headElement.Add(metaDateElemet2);
            //headElement.Add(metaDateElemet3);
            headElement.Add(linkElement1);
            headElement.Add(titleElement);

            var iconElement = CreateElement("img", "icon", null);
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), product.IconLink);
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), product.IconLinkDescription);

            //string productGroupsLink = string.Format(CultureInfo.InvariantCulture, @"../../../serviceapi/products/{0}", product.GroupCode);
            string productGroupsLink = product.GroupCodeLink;
            var productGroupsLinkElement = CreateElement("a", "product-groups-link", product.GroupCodeDescription);
            productGroupsLinkElement.SetAttributeValue(XName.Get("href", string.Empty), productGroupsLink);

            var bodyElement = CreateElement("body", "product", null);
            var detailsElement = CreateElement("div", "details", null);
            detailsElement.Add(
                    //Product Group Details:
                    CreateElement("span", "locale", product.Locale.Code),
                    CreateElement("span", "name", product.Name),
                    CreateElement("span", "description", product.Description),
                    iconElement,
                    productGroupsLinkElement
                    );
            var bookListElement = CreateElement("div", "book-list", null);
            var bookListLinkElement = CreateElement("a", "book-list-link", "Books:");
            //This Product contains the following 

            //bookListLinkElement.SetAttributeValue(
            //    XName.Get("href", string.Empty),
            //    string.Format(CultureInfo.InvariantCulture, @"../../../serviceapi/products/{0}/{1}/books",
            //        product.GroupCode, product.Code));
            bookListLinkElement.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"{0}/books",
                    product.CodeLink));
            bookListElement.Add(bookListLinkElement);

            foreach (var book in product.Books)
            {
                var bookElement = CreateElement("div", "book", null);

                var iconBookElement = CreateElement("img", "icon", null);
                iconBookElement.SetAttributeValue(XName.Get("src", string.Empty), book.IconLink);
                iconBookElement.SetAttributeValue(XName.Get("alt", string.Empty), book.IconLinkDescription);

                var linkElement = CreateElement("a", "book-link", book.CodeDescription);
                linkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    CreateItemFileName(book, null));

                bookElement.Add(
                    CreateElement("span", "name", book.Name),
                    CreateElement("span", "locale", book.Locale.Code),
                    CreateElement("span", "description", book.Description),
                    iconBookElement,
                    linkElement);

                bookListElement.Add(bookElement);
            }

            bodyElement.Add(
                detailsElement,
                bookListElement);
            document.Root.Add(
                headElement,
                bodyElement);

            return document.ToStringWithDeclaration();
        }

        /// <summary>
        /// Create book packages index.
        /// </summary>
        /// <param name="book">Book.</param>
        /// <returns>Document data.</returns>
        public static string CreateBookPackagesIndex(Product product, Book book, string locLanguage)
        {
            Contract.Requires(null != product);
            Contract.Requires(null != book);

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                CreateElement("html", null, null));

            var headElement = CreateElement("head", null, null);
            var metaDateElemet1 = CreateElement("meta", null, null);
            metaDateElemet1.SetAttributeValue(XName.Get("name", string.Empty), "ROBOTS");
            metaDateElemet1.SetAttributeValue(XName.Get("content", string.Empty), "NOINDEX, NOFOLLOW");

            var metaDateElemet2 = CreateElement("meta", null, null);
            metaDateElemet2.SetAttributeValue(XName.Get("http-equiv", string.Empty), "Content-Location");
            metaDateElemet2.SetAttributeValue(
                    XName.Get("content", string.Empty),
                    string.Format(CultureInfo.InvariantCulture, @"http://services.mtps.microsoft.com/ServiceAPI/products/{0}/{1}/books/{2}/{3}",
                    product.GroupCode, product.Code, book.Code, book.Locale.Code.ToLowerInvariant()));

            var linkElement1 = CreateElement("link", null, null);
            linkElement1.SetAttributeValue(XName.Get("type", string.Empty), "text/css");
            linkElement1.SetAttributeValue(XName.Get("rel", string.Empty), "stylesheet");
            linkElement1.SetAttributeValue(XName.Get("href", string.Empty), "../../../../../../serviceapi/styles/global.css");

            var strproduct = string.Format(CultureInfo.InvariantCulture, @"Package Listing for book {0}", book.Code);
            var titleElement = CreateElement("title", null, strproduct);

            headElement.Add(metaDateElemet1);
            headElement.Add(metaDateElemet2);
            headElement.Add(linkElement1);
            headElement.Add(titleElement);

            var bodyElement = CreateElement("body", "book", null);
            var detailsElement = CreateElement("div", "details", null);

            var iconElement = CreateElement("img", "icon", null);
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), book.IconLink);
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), book.IconLinkDescription);

            //string productGroupsLink = string.Format(CultureInfo.InvariantCulture, @"../../../../../../serviceapi/products/{0}", product.GroupCode);
            string productGroupsLink = product.GroupCodeLink;
            var productGroupsLinkElement = CreateElement("a", "product-groups-link", product.GroupCodeDescription);
            productGroupsLinkElement.SetAttributeValue(XName.Get("href", string.Empty), productGroupsLink);

            var brandingPackageElement1 = CreateElement("a", "branding-package-link", BRANDING_PACKAGE_NAME1);
            brandingPackageElement1.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"packages\{0}.cab", BRANDING_PACKAGE_NAME1));

            var brandingPackageElement2 = CreateElement("a", "branding-package-link", BRANDING_PACKAGE_NAME2);
            brandingPackageElement2.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"packages\{0}.cab", BRANDING_PACKAGE_NAME2));

            bool includeLocLanguage = false;
            foreach (var bookTemp in product.Books)
            {
                if (locLanguage.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant()
                    || (locLanguage.ToLowerInvariant() != "en-us"
                    && bookTemp.Locale.Name.ToLowerInvariant() == locLanguage.ToLowerInvariant()))
                {
                    includeLocLanguage = true;
                    break;
                }
            }

            var productLinkElement = CreateElement("a", "product-link", book.ProductName);
            if (!includeLocLanguage)
            {
                productLinkElement.SetAttributeValue(XName.Get("href", string.Empty), CreateItemFileName(product, null));
            }
            else
                productLinkElement.SetAttributeValue(XName.Get("href", string.Empty), CreateItemFileName(product, locLanguage));

            var productGroupLinkElement = CreateElement("a", "product-group-link", book.ProductGroupName);
			productGroupLinkElement.SetAttributeValue( XName.Get( "href", string.Empty), /*"HelpContentSetup.msha"*/book.ProductGroupLink);

            XElement bookLastModified;
            if ((book.LastModified.Millisecond % 10) == 0)
                bookLastModified = CreateElement("span", "last-modified", book.LastModified.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.ffZ", CultureInfo.InvariantCulture));
            else
                bookLastModified = CreateElement("span", "last-modified", book.LastModified.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffZ", CultureInfo.InvariantCulture)); 

            detailsElement.Add(
                    CreateElement("span", "vendor", book.Vendor),
                    CreateElement("span", "locale", book.Locale.Code),
                    CreateElement("span", "name", book.Name),
                    CreateElement("span", "description", book.Description),
                    bookLastModified,
                    //CreateElement("span", "last-modified", book.LastModified.ToUniversalTime().ToString("O")),
                    iconElement,
                    productLinkElement,
                    productGroupLinkElement,
                    brandingPackageElement1,
                    brandingPackageElement2
                    );

            var packageListElement = CreateElement("div", "package-list", null);
            //The following packages are available: in this book:

            foreach (var package in book.Packages)
            {
                var packageElement = CreateElement("div", "package", null);
                var linkElement = CreateElement("a", "current-link", CreatePackageFileName(package));
                
                linkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    string.Format(CultureInfo.InvariantCulture, @"packages\{0}\{1}", package.Locale.Code.ToLowerInvariant(), CreatePackageFileName(package)));

                var constituentLinkElement = CreateElement("a", "package-constituent-link", package.Name);
                constituentLinkElement.SetAttributeValue(XName.Get("href", string.Empty), package.ConstituentLink);

                XElement lastModified;
                if ((package.LastModified.Millisecond % 10) == 0)
                    lastModified = CreateElement("span", "last-modified", package.LastModified.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.ffZ", CultureInfo.InvariantCulture));
                else
                    lastModified = CreateElement("span", "last-modified", package.LastModified.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffZ", CultureInfo.InvariantCulture)); 


                packageElement.Add(
                    CreateElement("span", "name", package.Name),
                    CreateElement("span", "deployed", package.Deployed),
                    //Deployed:
                    lastModified,
                    //CreateElement("span", "last-modified", package.LastModified.ToUniversalTime().ToString("O")),
                    CreateElement("span", "package-etag", package.Tag),
                    linkElement,
                    //CreateElement("span", "package-size-bytes", package.SizeBytes),
                    //CreateElement("span", "package-size-bytes-uncompressed", package.SizeBytesUncompressed)
                    //CreateElement("span", "package-constituent-link", package.ConstituentLink )
                    CreateElement( "span", "package-size-bytes", package.SizeBytes.ToString() ),
					CreateElement( "span", "package-size-bytes-uncompressed", package.SizeBytesUncompressed.ToString() ),
					//new XText( @"(Package Constituents: " ),
					constituentLinkElement
					//new XText( @")" )	
                    );

                packageListElement.Add(packageElement);
            }

            bodyElement.Add(
                detailsElement,
                packageListElement);
            document.Root.Add(
                headElement,
                bodyElement);

            return document.ToStringWithDeclaration();
        }

        #endregion
    }
}