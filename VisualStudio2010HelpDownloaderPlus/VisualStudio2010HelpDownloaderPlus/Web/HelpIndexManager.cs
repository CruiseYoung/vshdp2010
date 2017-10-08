using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;


namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Helper class for parsing and creating documents with help indexes.
    /// </summary>
    internal static class HelpIndexManager
    {
        #region XElement extension

        private static string GetClassName(this XElement element)
        {
            return GetAttributeValue(element, "class");
        }

        private static string GetAttributeValue(this XElement element, string name)
        {
            var attribute = element.Attribute(XName.Get(name, string.Empty));

            return attribute?.Value;
        }

        private static string GetChildClassValue(this XElement element, string name)
        {
            /*
            var result = element.Elements()
                .Where(x => x.GetClassName() == name).Take(1).Single();

            return result?.Value;
            */
            string value = null;
            try
            {
                var result = element.Elements().Where(x => x.GetClassName()?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false).Take(1).Single();
                value = result?.Value;
            }
            catch (Exception)
            {
                // ignored
            }

            return value;

        }

        private static string GetChildClassAttributeValue(this XElement element, string name, string attribute)
        {
            /*
            var result = element.Elements()
                .Where(x => x.GetClassName() == name).Take(1).Single();

            return result?.GetAttributeValue(attribute);
            */

            string value = null;
            try
            {
                var result = element.Elements().Where(x => x.GetClassName()?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false).Take(1).Single();
                value = result?.GetAttributeValue(attribute);
            }
            catch (Exception)
            {
                // ignored
            }

            return value;

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
            string[] parts = url?.Split('/');

            return parts?.Length > 0 ? parts[parts.Length - 1] : null;
        }

        /// <summary>
        /// Create file name for some items.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="selLocale"></param>
        /// <returns>File name.</returns>
        public static string CreateItemFileName(ItemBase item, string selLocale)
        {
            if (null == item)
                return null;

            if (item is Product)
            {
                if (string.IsNullOrEmpty(selLocale))
                    return string.Format(CultureInfo.InvariantCulture, "product-{0}.html", item.Code);
                else
                    return string.Format(CultureInfo.InvariantCulture, "product-{0}({1}).html", item.Code, selLocale);
            }
            else if (item is Book)
            {
                if (item.Locale.ToLowerInvariant() == "en-us")
                    return string.Format(CultureInfo.InvariantCulture, "book-{0}.html", item.Code);
                else
                    return string.Format(CultureInfo.InvariantCulture, "book-{0}({1}).html", item.Code, item.Locale);
            }
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

            return package.PackageEtag != null ? string.Format(CultureInfo.InvariantCulture, "{0}({1}).cab", package.Name, package.PackageEtag) : string.Format(CultureInfo.InvariantCulture, "{0}.cab", package.Name);
        }

        /// <summary>
        /// Create a file name
        /// </summary>
        /// <returns>
        /// A string containing the file name
        /// </returns>
        public static string CreatePackageFileNameUri(Package package)
        {
            if (null == package)
                return null;

            return package.PackageEtag != null ? string.Format(CultureInfo.InvariantCulture, "{0}({1}).cab", System.Uri.EscapeDataString(package.Name), package.PackageEtag) : string.Format(CultureInfo.InvariantCulture, "{0}.cab", System.Uri.EscapeDataString(package.Name));
        }

        #region Load information methods

        /// <summary>
        /// Load products groups from document.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <returns>List of products groups.</returns>
        public static ICollection<ProductGroup> LoadProductGroups(byte[] data)
        {
            Contract.Ensures(null != Contract.Result<ICollection<ProductGroup>>());

            XDocument document;

            //string str_tmp = Encoding.UTF8.GetString(data).Trim();
            //data = Encoding.UTF8.GetBytes(str_tmp);
            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var result = new List<ProductGroup>();
            if (document.Root != null)
            {
                var query = document.Root.Elements()
                /*
                    .Where(x => x.GetClassName() == "product-groups").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product-group-list").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product-group");
                */
                    .Where(x => x.GetClassName()?.Equals("product-groups", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("product-group-list", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("product-group", StringComparison.OrdinalIgnoreCase) ?? false);

                foreach (var x in query)
                    result.Add(
                        new ProductGroup()
                        {
                            //Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                            Locale = x.GetChildClassValue("locale"),
                            Name = x.GetChildClassValue("name"),
                            Description = x.GetChildClassValue("description"),
                            //IconSrc = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
                            IconSrc = x.GetChildClassAttributeValue("icon", "src"),
                            IconAlt = x.GetChildClassAttributeValue("icon", "alt"),
                            Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-group-link", "href")),
                            Link = x.GetChildClassAttributeValue("product-group-link", "href"),
                            LinkDescription = x.GetChildClassValue("product-group-link")
                        }
                    );

                //result.Sort();
            }
            return result;
        }

        /// <summary>
        /// Load products from document.
        /// </summary>
        /// <param name="productGroup"></param>
        /// <param name="data">Document data.</param>
        /// <returns>List of products.</returns>
        public static ICollection<Product> LoadProducts(ProductGroup productGroup, byte[] data)
        {
            Contract.Ensures(null != Contract.Result<ICollection<Product>>());

            XDocument document;

            //string str_tmp = Encoding.UTF8.GetString(data).Trim();
            //data = Encoding.UTF8.GetBytes(str_tmp);
            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);


            if (document.Root != null)
            {
                var detailsElement = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "product-group").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "details").Take(1).Single();
                    */
                    .Where(x => x.GetClassName()?.Equals("product-group", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("details", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single();

                productGroup.ProductGroupsLink = detailsElement.GetChildClassAttributeValue("product-groups-link", "href");
                productGroup.ProductGroupsDescription = detailsElement.GetChildClassValue("product-groups-link");
            }

            var result = new List<Product>();
            if (document.Root != null)
            {
                var query = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "product-group").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product-list").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "product");
                    */
                    .Where(x => x.GetClassName()?.Equals("product-group", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("product-list", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("product", StringComparison.OrdinalIgnoreCase) ?? false);

                //foreach (var x in query)
                //    result.Add(
                //        new Product()
                //        {
                //            Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                //            Name = x.GetChildClassValue("name"),
                //            Description = x.GetChildClassValue("description"),
                //            IconSrc = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
                //            IconAlt = x.GetChildClassAttributeValue("icon", "alt"),
                //            Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-link", "href")),
                //            LinkDescription = x.GetChildClassValue("product-link")
                //        }
                //    );

                foreach (var x in query)
                {
                    var product = new Product()
                    {
                        Locale =  x.GetChildClassValue("locale"),

                        //Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                        Name = x.GetChildClassValue("name"),
                        Description = x.GetChildClassValue("description"),
                        //IconSrc = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
                        IconSrc = x.GetChildClassAttributeValue("icon", "src"),
                        IconAlt = x.GetChildClassAttributeValue("icon", "alt"),
                        Link = x.GetChildClassAttributeValue("product-link", "href"),
                        Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("product-link", "href")),
                        LinkDescription = x.GetChildClassValue("product-link"),

                        ProductGroupCode = productGroup.Code,
                        ProductGroupsLink = productGroup.ProductGroupsLink,
                        ProductGroupsDescription = productGroup.ProductGroupsDescription
                    };

                    var bAdd = true;
                    foreach (var productSrc in result)
                    {
                        if (productSrc.Code == product.Code)
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

                //result.Sort();
            }
            return result;
        }

        /// <summary>
        /// Load books from document.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="data">Document data.</param>
        /// <returns>List of books.</returns>
        public static Tuple<ICollection<Book>, ICollection<string>> LoadBooks(Product product, byte[] data)
        {
            Contract.Ensures(null != Contract.Result<Tuple<ICollection<Book>, ICollection<string>>>());

            XDocument document;

            //string str_tmp = Encoding.UTF8.GetString(data).Trim();
            //data = Encoding.UTF8.GetBytes(str_tmp);
            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var result = new List<Book>();
            var locales = new List<string>();
            if (document.Root != null)
            {
                var detailsElement = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "product").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "details").Take(1).Single();
                    */
                    .Where(x => x.GetClassName()?.Equals("product", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("details", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single();

                product.ProductGroupsLink = detailsElement.GetChildClassAttributeValue("product-groups-link", "href");
                product.ProductGroupsDescription = detailsElement.GetChildClassValue("product-groups-link");

                var productElement = document.Root.Elements()
                    .Where(x => x.GetClassName()?.Equals("product", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single();

                product.ProductGroupLink = productElement.GetChildClassAttributeValue("product-group-link", "href");
                product.ProductGroupDescription = productElement.GetChildClassValue("product-group-link");

                //string currentLink = document.Root.Elements()
                var booklistElement = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "product").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "book-list").Take(1).Single().GetChildClassAttributeValue("book-list-link", "href");
                    */
                    .Where(x => x.GetClassName()?.Equals("product", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("book-list", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single();
                    //.GetChildClassAttributeValue("book-list-link", "href");


                //if (!string.IsNullOrEmpty(currentLink))
                if (booklistElement != null)
                {
                    var query = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "product").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "book-list").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "book");
                    */
                    .Where(x => x.GetClassName()?.Equals("product", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("book-list", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("book", StringComparison.OrdinalIgnoreCase) ?? false);

                    foreach (var x in query)
                    {
                        string code = x.GetChildClassAttributeValue("book-link", "href").Replace(product.ProductGroupLink, null).TrimStart('/');
                        if (code.Contains('/'))
                            code = code.Substring(0, code.IndexOf('/'));

                        var book = new Book()
                        {
                            Name = x.GetChildClassValue("name"),
                            Locale = x.GetChildClassValue("locale"),
                            //Locale = new Locale() { Code = x.GetChildClassValue("locale") },
                            Description = x.GetChildClassValue("description"),
                            //IconSrc = CreateCodeFromUrl(x.GetChildClassAttributeValue("icon", "src")),
                            IconSrc = x.GetChildClassAttributeValue("icon", "src"),
                            IconAlt = x.GetChildClassAttributeValue("icon", "alt"),
                            Link = x.GetChildClassAttributeValue("book-link", "href"),
                            //Code = CreateCodeFromUrl(x.GetChildClassAttributeValue("book-link", "href")),
                            Code = code,
                            LinkDescription = x.GetChildClassValue("book-link"),

                            ProductCode = product.Code,
                            ProductGroupCode = product.ProductGroupCode
                        };
                    
                        result.Add(book);

                        if (!locales.Contains(book.Locale))
                            locales.Add(book.Locale);
                    }
                }

                //result.Sort();
                //locales.Sort();
            }

            return new Tuple<ICollection<Book>, ICollection<string>>(result, locales);
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

            XDocument document;

            //string str_tmp = Encoding.UTF8.GetString(data).Trim();
            //data = Encoding.UTF8.GetBytes(str_tmp);
            using (var stream = new MemoryStream(data))
                document = XDocument.Load(stream);

            var result = new List<Package>();
            if (document.Root != null)
            {
                var detailsElement = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "book").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "details").Take(1).Single();
                    */
                    .Where(x => x.GetClassName()?.Equals("book", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("details", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single();

                book.Vendor = detailsElement.GetChildClassValue("vendor");
                //book.Locale = detailsElement.GetChildClassValue("locale");
                ////book.Locale = new Locale() { Code = detailsElement.GetChildClassValue("locale") };
                //book.Name = detailsElement.GetChildClassValue("name");
                //book.Description = detailsElement.GetChildClassValue("description");
                book.LastModified = DateTime.Parse(detailsElement.GetChildClassValue("last-modified"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)/*.ToUniversalTime()*/;
                book.ProductLink = detailsElement.GetChildClassAttributeValue("product-link", "href");
                book.ProductDescription = detailsElement.GetChildClassValue("product-link");
                book.ProductGroupLink = detailsElement.GetChildClassAttributeValue("product-group-link", "href");
                book.ProductGroupDescription = detailsElement.GetChildClassValue("product-group-link");

                var query = document.Root.Elements()
                    /*
                    .Where(x => x.GetClassName() == "book").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "package-list").Take(1).Single().Elements()
                    .Where(x => x.GetClassName() == "package");
                    */
                    .Where(x => x.GetClassName()?.Equals("book", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("package-list", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Take(1)
                    .Single()
                    .Elements()
                    .Where(x => x.GetClassName()?.Equals("package", StringComparison.OrdinalIgnoreCase) ?? false);

                foreach (var x in query)
                {
                    result.Add(
                        new Package()
                        {
                            Name = x.GetChildClassValue("name"),
                            Deployed = x.GetChildClassValue("deployed"),
                            LastModified = DateTime.Parse(x.GetChildClassValue("last-modified"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)/*.ToUniversalTime()*/,
                            PackageEtag = x.GetChildClassValue("package-etag"),
                            CurrentLink = x.GetChildClassAttributeValue("current-link", "href"),
                            CurrentLinkDescription = x.GetChildClassValue("current-link"),
                            PackageSizeBytes = long.Parse(x.GetChildClassValue("package-size-bytes"), CultureInfo.InvariantCulture),
                            PackageSizeBytesUncompressed = long.Parse(x.GetChildClassValue("package-size-bytes-uncompressed"), CultureInfo.InvariantCulture),
                            //PackageConstituentLink = x.GetChildClassAttributeValue("package-constituent-link", "href"),
                            //PackageConstituentLinkDescription = x.GetChildClassValue("package-constituent-link"),
                            Locale = book.Locale // Locale = detailsElement.GetChildClassValue("locale") 
                        }
                    );
                }

                //result.Sort();
            }
            return result;
        }

        #endregion

        #region Create indexes methods

        private static XElement CreateElement(string name, string className, string value)
        {
            var element = new XElement(XName.Get(name, "http://www.w3.org/1999/xhtml"));
            
            if (!string.IsNullOrEmpty(className))
                element.SetAttributeValue(XName.Get("class", string.Empty), className);
            if (!string.IsNullOrEmpty(value))
                element.Value = value;

            return element;
        }

        /// <summary>
        /// Creates main help setup index.
        /// </summary>
        /// <param name="products">Products list.</param>
        /// <param name="selLoc"></param>
        /// <returns>Document data.</returns>
        public static string CreateSetupIndex(IEnumerable<Product> products, string selLoc)
        {
            Contract.Requires(null != products);

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                CreateElement("html", null, null));

            var bodyElement = CreateElement("body", "product-list", null);

            var iconElement = CreateElement("img", "icon", null);
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), "../../../serviceapi/content/image/ic298417");
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), "VS 100 Icon");

            var list = products as List<Product>;
            list?.Sort();
            foreach (var product in products)
            {
                var productElement = CreateElement("div", "product", null);

                var iconProductElement = CreateElement("img", "icon", null);
                iconProductElement.SetAttributeValue(XName.Get("src", string.Empty), product.IconSrc);
                iconProductElement.SetAttributeValue(XName.Get("alt", string.Empty), product.IconAlt);

                bool includeSelLoc = false;
                if (selLoc.ToLowerInvariant() == ItemBase.LocaleAll.ToLowerInvariant())
                {
                    includeSelLoc = true;
                }
                else if (selLoc.ToLowerInvariant() != "en-us")
                {
                    foreach (var book in product.Books)
                    {
                        if (book.Locale.ToLowerInvariant() == selLoc.ToLowerInvariant())
                        {
                            includeSelLoc = true;
                            break;
                        }
                    }
                }

                var productLinkElement = CreateElement("a", "product-link", product.LinkDescription);
                productLinkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    !includeSelLoc ? CreateItemFileName(product, null) : CreateItemFileName(product, selLoc));

                productElement.Add(
                    CreateElement("span", "locale", product.Locale),
                    CreateElement("span", "name", product.Name),
                    CreateElement("span", "description", product.Description),
                    iconProductElement,
                    productLinkElement);

                bodyElement.Add(productElement);
            }

            var divElement = CreateElement("div", null, null);
            divElement.SetAttributeValue(XName.Get("id", string.Empty), "GWDANG_HAS_BUILT");
            bodyElement.Add(divElement);

            document.Root?.Add(bodyElement);

            return document.ToStringWithDeclaration();
        }

        /// <summary>
        /// Create product books index.
        /// </summary>
        /// <param name="product">Product.</param>
        /// <param name="locLanguage"></param>
        /// <param name="lastModified"></param>
        /// <returns>Document data.</returns>
        public static string CreateProductBooksIndex(Product product, string locLanguage, DateTime lastModified)
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
                    product.ProductGroupCode, product.Code));

            var lastModifiedFmt = (lastModified.Millisecond % 10) == 0 ? "yyyy-MM-ddThh:mm:ss.ffZ" : "yyyy-MM-ddThh:mm:ss.fffZ";
            string lastModifiedStr = lastModified.ToUniversalTime()
                .ToString(lastModifiedFmt, CultureInfo.InvariantCulture);

            var metaDateElemet3 = CreateElement("meta", null, null);
            metaDateElemet3.SetAttributeValue(XName.Get("http-equiv", string.Empty), "Date");
            metaDateElemet3.SetAttributeValue(
                    XName.Get("content", string.Empty),
                    //lastModified.ToString("R")
                    lastModifiedStr
                    );

            var linkElement1 = CreateElement("link", null, null);
            linkElement1.SetAttributeValue(XName.Get("type", string.Empty), "text/css");
            linkElement1.SetAttributeValue(XName.Get("rel", string.Empty), "stylesheet");
            linkElement1.SetAttributeValue(XName.Get("href", string.Empty), "../../../serviceapi/styles/global.css");

            var strproduct = string.Format(CultureInfo.InvariantCulture, @"Constituents of Product {0}", product.Code);
            var titleElement = CreateElement("title", null, strproduct);

            headElement.Add(metaDateElemet1);
            headElement.Add(metaDateElemet2);
            headElement.Add(metaDateElemet3);
            headElement.Add(linkElement1);
            headElement.Add(titleElement);

            var iconElement = CreateElement("img", "icon", null);
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), product.IconSrc);
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), product.IconAlt);

            //string productGroupsLink = string.Format(CultureInfo.InvariantCulture, @"../../../serviceapi/products/{0}", product.ProductGroupsName);
            var productGroupLinkElement = CreateElement("a", "product-group-link", product.ProductGroupDescription);
            productGroupLinkElement.SetAttributeValue(XName.Get("href", string.Empty), product.ProductGroupLink);

            var productGroupsLinkElement = CreateElement("a", "product-groups-link", product.ProductGroupsDescription);
            productGroupsLinkElement.SetAttributeValue(XName.Get("href", string.Empty), product.ProductGroupsLink);


            var bodyElement = CreateElement("body", "product", null);
            var detailsElement = CreateElement("div", "details", null);
            detailsElement.Add(
                //new XText("Product Details:\r\n"),
                CreateElement("span", "locale", product.Locale),
                CreateElement("span", "name", product.Name),
                CreateElement("span", "description", product.Description),
                iconElement,
                productGroupsLinkElement
                //productGroupLinkElement
            );
            var bookListElement = CreateElement("div", "book-list", null);
            //bookListElement.Add(new XText("This Product contains the following:\r\n"));
            //This Product contains the following:\r\n 
            /*
            var bookListLinkElement = CreateElement("a", "book-list-link", "Books:");

            //bookListLinkElement.SetAttributeValue(
            //    XName.Get("href", string.Empty),
            //    string.Format(CultureInfo.InvariantCulture, @"../../../serviceapi/products/{0}/{1}/books",
            //        product.ProductGroupsName, product.Code));
            bookListLinkElement.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"{0}/books",
                    product.Link));
            bookListElement.Add(bookListLinkElement);
            */

            var books = product.Books as List<Book>;
            books?.Sort();
            foreach (var book in product.Books)
            {
                var bookElement = CreateElement("div", "book", null);

                var iconBookElement = CreateElement("img", "icon", null);
                iconBookElement.SetAttributeValue(XName.Get("src", string.Empty), book.IconSrc);
                iconBookElement.SetAttributeValue(XName.Get("alt", string.Empty), book.IconAlt);

                var linkElement = CreateElement("a", "book-link", book.LinkDescription);
                linkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    CreateItemFileName(book, null));

                bookElement.Add(
                    CreateElement("span", "name", book.Name),
                    CreateElement("span", "locale", book.Locale),
                    CreateElement("span", "description", book.Description),
                    //iconBookElement,
                    linkElement);

                bookListElement.Add(bookElement);
            }

            var divElement = CreateElement("div", null, null);
            divElement.SetAttributeValue(XName.Get("id", string.Empty), "GWDANG_HAS_BUILT");
            bookListElement.Add(divElement);

            bodyElement.Add(
                detailsElement,
                productGroupLinkElement,
                bookListElement);

            document.Root?.Add(
                headElement,
                bodyElement);

            return document.ToStringWithDeclaration();
        }

        /// <summary>
        /// Create book packages index.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="book">Book.</param>
        /// <param name="selLoc"></param>
        /// <returns>Document data.</returns>
        public static string CreateBookPackagesIndex(Product product, Book book, string selLoc)
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
                    book.ProductGroupCode, book.ProductCode, book.Code, book.Locale.ToLowerInvariant()));

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
            iconElement.SetAttributeValue(XName.Get("src", string.Empty), book.IconSrc);
            iconElement.SetAttributeValue(XName.Get("alt", string.Empty), book.IconAlt);

            //string productGroupsLink = string.Format(CultureInfo.InvariantCulture, @"../../../../../../serviceapi/products/{0}", product.ProductGroupsName);
            var productGroupsLinkElement = CreateElement("a", "product-groups-link", product.ProductGroupsDescription);
            productGroupsLinkElement.SetAttributeValue(XName.Get("href", string.Empty), product.ProductGroupsLink);

            var brandingPackageElement1 = CreateElement("a", "branding-package-link", Downloader.BrandingPackageName1);
            brandingPackageElement1.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"packages/{0}.cab", System.Uri.EscapeDataString(Downloader.BrandingPackageName1)));

            var brandingPackageElement2 = CreateElement("a", "branding-package-link", Downloader.BrandingPackageName2);
            brandingPackageElement2.SetAttributeValue(
                XName.Get("href", string.Empty),
                string.Format(CultureInfo.InvariantCulture, @"packages/{0}.cab", System.Uri.EscapeDataString(Downloader.BrandingPackageName2)));

            bool includeSelLoc = false;
            foreach (var bookTemp in product.Books)
            {
                if (selLoc.ToLowerInvariant() == ItemBase.LocaleAll.ToLowerInvariant()
                    || (selLoc.ToLowerInvariant() != "en-us"
                    && bookTemp.Locale.ToLowerInvariant() == selLoc.ToLowerInvariant()))
                {
                    includeSelLoc = true;
                    break;
                }
            }

            var productLinkElement = CreateElement("a", "product-link", book.ProductDescription);
            productLinkElement.SetAttributeValue(XName.Get("href", string.Empty),
                !includeSelLoc ? CreateItemFileName(product, null) : CreateItemFileName(product, selLoc));

            var productGroupLinkElement = CreateElement("a", "product-group-link", book.ProductGroupDescription);
            productGroupLinkElement.SetAttributeValue( XName.Get( "href", string.Empty), /*"HelpContentSetup.msha"*/book.ProductGroupLink);

            //var lastModifiedFmtBook = (book.LastModified.Millisecond % 10) == 0 ? "yyyy-MM-ddThh:mm:ss.ffZ" : "yyyy-MM-ddThh:mm:ss.fffZ";
            var lastModifiedFmtBook = "s";

            XElement bookLastModified = CreateElement("span", "last-modified", book.LastModified.ToUniversalTime().ToString(lastModifiedFmtBook, CultureInfo.InvariantCulture));

            detailsElement.Add(
                    CreateElement("span", "vendor", book.Vendor),
                    CreateElement("span", "locale", book.Locale),
                    CreateElement("span", "name", book.Name),
                    CreateElement("span", "description", book.Description),
                    bookLastModified,
                    //CreateElement("span", "last-modified", book.LastModified.ToUniversalTime().ToString("O")),
                    iconElement,
                    //productLinkElement,
                    productGroupLinkElement,
                    brandingPackageElement1,
                    brandingPackageElement2
                    );

            var packageListElement = CreateElement("div", "package-list", null);
            //packageListElement.Add(new XText("The following packages are available: in this book:\r\n"));
            //The following packages are available: in this book:

            var packages = book.Packages as List<Package>;
            packages?.Sort();
            foreach (var package in book.Packages)
            {
                var packageElement = CreateElement("div", "package", null);
                var currentLinkElement = CreateElement("a", "current-link", package.CurrentLinkDescription);
                
                currentLinkElement.SetAttributeValue(
                    XName.Get("href", string.Empty),
                    string.Format(CultureInfo.InvariantCulture, @"packages/{0}/{1}", book.Locale.ToLowerInvariant(), CreatePackageFileNameUri(package)));

                //var constituentLinkElement = CreateElement("a", "package-constituent-link", package.PackageConstituentLinkDescription);
                //constituentLinkElement.SetAttributeValue(
                //    XName.Get("href", string.Empty),
                //    string.Format(CultureInfo.InvariantCulture, @"packages/{0}/{1}", book.Locale.ToLowerInvariant(), package.Name));

                //var lastModifiedFmt = (package.LastModified.Millisecond % 10) == 0 ? "yyyy-MM-ddThh:mm:ss.ffZ" : "yyyy-MM-ddThh:mm:ss.fffZ";
                var lastModifiedFmt = "s";

                XElement lastModified = CreateElement("span", "last-modified", package.LastModified.ToUniversalTime().ToString(lastModifiedFmt, CultureInfo.InvariantCulture));

                packageElement.Add(
                    CreateElement("span", "name", package.Name),
                    //new XText("Deployed:\r\n"),
                    CreateElement("span", "deployed", package.Deployed),
                    lastModified,
                    CreateElement("span", "package-etag", package.PackageEtag),
                    currentLinkElement,
                    //CreateElement("span", "package-size-bytes", package.PackageSizeBytes),
                    //CreateElement("span", "package-size-bytes-uncompressed", package.PackageSizeBytesUncompressed)
                    //CreateElement("span", "package-constituent-link", package.PackageConstituentLink )
                    CreateElement( "span", "package-size-bytes", package.PackageSizeBytes.ToString() ),
                    CreateElement( "span", "package-size-bytes-uncompressed", package.PackageSizeBytesUncompressed.ToString() )//,
                    //new XText( @"(Package Constituents: " ),
                    //constituentLinkElement
                    //,
                    //new XText( @")" )
                    );
                packageListElement.Add(packageElement);
            }

            var divElement = CreateElement("div", null, null);
            divElement.SetAttributeValue(XName.Get("id", string.Empty), "GWDANG_HAS_BUILT");

            bodyElement.Add(
                detailsElement,
                packageListElement,
                divElement
                );
            document.Root?.Add(
                headElement,
                bodyElement);

            return document.ToStringWithDeclaration();
        }

        #endregion
    }
}