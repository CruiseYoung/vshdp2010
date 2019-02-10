using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace VisualStudio2010HelpDownloaderPlus.Web
{
    /// <summary>
    /// Books information and packages download manager.
    /// </summary>
    internal sealed class Downloader : IDisposable
    {
        private const string BrandingPackageUrl = @"http://packages.mtps.microsoft.com/brands/";
        public const string BrandingPackageName1 = "dev10";
        public const string BrandingPackageName2 = "dev10-ie6";

        private int _packagesCountTotal;
        private int _packagesCountCurrent;

        private WebClient _client = new WebClient();
        private WebProxy _proxy;

        /// <summary>
        /// Raises when the downloader indicates that some progress has been made.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        public Downloader()
        {
            _client.Encoding = Encoding.UTF8;
            _client.BaseAddress = @"http://services.mtps.microsoft.com/serviceapi/products/";

            string settingsFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                string.Format(CultureInfo.InvariantCulture, "{0}.xml", Assembly.GetEntryAssembly().GetName().Name));

            if (File.Exists(settingsFile))
            {
                try
                {
                    XElement element = XDocument.Load( settingsFile ).Root;
                    if (element != null)
                    {
                        /*
                        //var element = XDocument.Load(settingsFile).Root.Elements().Single(x => x.Name.LocalName == "proxy");
                        element = element.Elements().Single(x => x.Name.LocalName == "proxy");

                        _proxy = new WebProxy(element.Attributes().Single(x => x.Name.LocalName == "address").Value);
                        if (element.Attributes().Any(x => x.Name.LocalName == "default" && (x.Value == "1" || x.Value == "true")))
                        {
                            _proxy.UseDefaultCredentials = true;
                            _proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                        }
                        else
                        {
                            _proxy.Credentials = new NetworkCredential(
                                element.Attributes().Single(x => x.Name.LocalName == "login").Value,
                                element.Attributes().Single(x => x.Name.LocalName == "password").Value,
                                element.Attributes().Single(x => x.Name.LocalName == "domain").Value);
                        }
                        */

                        element = element.Elements().Single(x => x.Name.LocalName?.Equals("proxy", StringComparison.OrdinalIgnoreCase) ?? false);
                        _proxy = new WebProxy(element.Attributes().Single(x => x.Name.LocalName?.Equals("address", StringComparison.OrdinalIgnoreCase) ?? false).Value)
                        {
                            Credentials =
                                new NetworkCredential(
                                element.Attributes().Single(x => x.Name.LocalName?.Equals("login", StringComparison.OrdinalIgnoreCase) ?? false).Value,
                                element.Attributes().Single(x => x.Name.LocalName?.Equals("password", StringComparison.OrdinalIgnoreCase) ?? false).Value,
                                element.Attributes().Single(x => x.Name.LocalName?.Equals("domain", StringComparison.OrdinalIgnoreCase) ?? false).Value)
                        };

                        _client.Proxy = _proxy;
                    }
                }
                catch (Exception ex)
                {
                    Program.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Download books information.
        /// </summary>
        /// <returns>List of available product groups.</returns>
        public Tuple<ICollection<ProductGroup>, ICollection<string>> LoadBooksInformation()
        {
            Contract.Ensures(null != Contract.Result<Tuple<ICollection<ProductGroup>, ICollection<string>>>());

            var productGroups = HelpIndexManager.LoadProductGroups(_client.DownloadData(""));
            var locales = new List<string>();

            foreach (var productGroup in productGroups)
            {
                if (/*!string.IsNullOrWhiteSpace(productGroup.Code)*/
                    !string.IsNullOrWhiteSpace(productGroup.Link))
                {
                    //productGroup.Products = HelpIndexManager.LoadProducts(
                    //    _client.DownloadData(string.Format(CultureInfo.InvariantCulture,
                    //        "{0}", productGroup.Code)));
                    productGroup.Products = HelpIndexManager.LoadProducts(
                        productGroup, _client.DownloadData(productGroup.Link));

                    foreach (var product in productGroup.Products)
                        if (/*!string.IsNullOrWhiteSpace(product.Code)*/
                            !string.IsNullOrWhiteSpace(product.Link))
                        {
                            //product.ProductGroupsName = productGroup.Code;
                            //product.ProductGroupsLink = productGroup.Link;
                            //product.ProductGroupsDescription = productGroup.LinkDescription;

                            //var tuple = HelpIndexManager.LoadBooks(
                            //    _client.DownloadData(string.Format(CultureInfo.InvariantCulture,
                            //        "{0}/{1}", product.ProductGroupsName, product.Code)));

                            var tuple = HelpIndexManager.LoadBooks(
                                product, _client.DownloadData(product.Link));

                            product.Books = tuple.Item1;

                            foreach (var locale in tuple.Item2)
                                if (!locales.Contains(locale))
                                    locales.Add(locale);
                        }
                }
            }

            locales.Sort();

            return new Tuple<ICollection<ProductGroup>, ICollection<string>>(productGroups, locales);
        }

        /// <summary>
        /// Download books.
        /// </summary>
        /// <param name="products">Products list.</param>
        /// <param name="directory">Target directory.</param>
        /// <param name="selLoc"></param>
        /// <param name="worker">Background worker instance.</param>
        public void DownloadBooks(IEnumerable<Product> products, string directory, string selLoc, BackgroundWorker worker)
        {
            Contract.Requires(null != worker);

            if (string.IsNullOrEmpty(directory) || (null == products) || string.IsNullOrEmpty(selLoc))
                return;

            _packagesCountTotal = 0;
            _packagesCountCurrent = 0;

            var lastModifiedTime = new DateTime(2000, 1, 1, 0, 0, 0);
            //var packages = new List<Package>();
            var packages = new Dictionary<string, Package>();
            
            // Add branding packages
            var brandingPackgeName1 = new Package
            {
                Name = BrandingPackageName1,
                Deployed = @"true",
                LastModified = DateTime.Now,
                PackageEtag = null,
                CurrentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}.cab", BrandingPackageUrl, BrandingPackageName1),
                PackageSizeBytes = 0,
                PackageSizeBytesUncompressed = 0,
                PackageConstituentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}", @"../../serviceapi/packages/brands/", BrandingPackageName1)
            };
            brandingPackgeName1.LastModified = FetchLastModified(brandingPackgeName1.CurrentLink);
            brandingPackgeName1.PackageSizeBytes = FetchContentLength(brandingPackgeName1.CurrentLink);
            brandingPackgeName1.PackageSizeBytesUncompressed = brandingPackgeName1.PackageSizeBytes;
            packages.Add(brandingPackgeName1.Name, brandingPackgeName1);
            if (brandingPackgeName1.LastModified > lastModifiedTime)
                lastModifiedTime = brandingPackgeName1.LastModified;

            var brandingPackgeName2 = new Package
            {
                Name = BrandingPackageName2,
                Deployed = @"true",
                LastModified = DateTime.Now,
                PackageEtag = null,
                CurrentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}.cab", BrandingPackageUrl, BrandingPackageName2),
                PackageSizeBytes = 0,
                PackageSizeBytesUncompressed = 0,
                PackageConstituentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}", @"../../serviceapi/packages/brands/", BrandingPackageName2)
            };
            brandingPackgeName2.LastModified = FetchLastModified(brandingPackgeName2.CurrentLink);
            brandingPackgeName2.PackageSizeBytes = FetchContentLength(brandingPackgeName2.CurrentLink);
            brandingPackgeName2.PackageSizeBytesUncompressed = brandingPackgeName2.PackageSizeBytes;
            packages.Add(brandingPackgeName2.Name, brandingPackgeName2);
            if (brandingPackgeName2.LastModified > lastModifiedTime)
                lastModifiedTime = brandingPackgeName2.LastModified;

            var locales = new List<string>();
            var localeModifieds = new Dictionary<string, DateTime>();
            // Fetching packages information
            var enumerable = products as Product[] ?? products.ToArray();
            foreach (var product in enumerable)
                foreach (var book in product.Books)
                    if (!string.IsNullOrWhiteSpace(book.Link))
                    {
                        book.Packages = HelpIndexManager.LoadPackages(
                            book,
                            _client.DownloadData(book.Link));

                        //string packagePathDest = Path.Combine(directory, "packages", book.Locale.Code.ToLowerInvariant());

                        //if (!Directory.Exists(packagePathDest))
                        //    Directory.CreateDirectory(packagePathDest);
                        if (!locales.Contains(book.Locale))
                        {
                            locales.Add(book.Locale);

                            //var LocaleModified = new DateTime(2000, 1, 1, 0, 0, 0);
                            //LocaleModified = book.LastModified;
                            localeModifieds.Add(book.Locale.ToLowerInvariant(), book.LastModified);
                        }
                        else
                        {
                            if (book.LastModified > localeModifieds[book.Locale.ToLowerInvariant()])
                                localeModifieds[book.Locale.ToLowerInvariant()] = book.LastModified;
                        }

                        foreach (var package in book.Packages)
                        {
                            string name = package.Name.ToLowerInvariant();
                            if (!packages.ContainsKey(name))
                            {
                                packages.Add(name, package);
                            }
                            /*
                            //if ((package.Name != HelpIndexManager.BRANDING_PACKAGE_NAME1) &&
                            //    (package.Name != HelpIndexManager.BRANDING_PACKAGE_NAME2))
                            if (!packages.Contains(package))
                            {
                                packages.Add(package);
                            }
                            */
                            if (package.LastModified > lastModifiedTime)
                                lastModifiedTime = package.LastModified;
                        }

                        if (worker.CancellationPending)
                            return;
                    }

            _packagesCountTotal = packages.Count;

            //if (selLoc.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant())
                if (0 == locales.Count)
                    return;
                else if (1 == locales.Count)
                    selLoc = locales[0];
                else if (2 == locales.Count)
                {
                    bool bHasEnUs = false;
                    string strLoc = selLoc;
                    foreach (var loc in locales)
                        if (loc.ToLowerInvariant() == "en-us")
                            bHasEnUs = true;
                        else
                            strLoc = loc;

                    selLoc = bHasEnUs ? strLoc : ItemBase.LocaleAll;
                }
                else
                    selLoc = ItemBase.LocaleAll;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Generate Download File List
            try
            {
                string listFileName = string.Format(CultureInfo.InvariantCulture, "({0})PackageList.txt", selLoc);
                string listFilePath = Path.Combine(directory, listFileName);

                if (File.Exists(listFilePath))
                    File.Delete(listFilePath);

                StreamWriter writer = new StreamWriter(listFilePath);
                //IEnumerable<Package> query = packages.OrderBy(package => package.CurrentLink);
                //foreach (Package package in query)
                //    writer.WriteLine(package.CurrentLink);
                var query = packages.Values.ToList();
                query.Sort();
                foreach (Package package in query)
                    writer.WriteLine(package.CurrentLink);
                /*
                packages.Sort();
                foreach (Package package in packages)
                    writer.WriteLine(package.CurrentLink);
                */
                writer.Close();

                FileLastModifiedTime(listFilePath, lastModifiedTime);
            }
            catch (Exception e)
            {
                Program.LogException(e);
            }
            
            foreach (string file in Directory.GetFiles(directory, "*.msha"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!fileName.Contains(@"HelpContentSetup")
                        || fileName.Contains(@"HelpContentSetup("))
                    {
                        File.Delete(file);
                    }

                    if (fileName == @"(" + selLoc + @")HelpContentSetup")
                    {
                        File.Delete(file);
                        //break;
                    }
                }
            }
          
            Directory.GetFiles(directory, "*.xml").ForEach(File.Delete);
            //Directory.GetFiles(directory, "*.html").ForEach(x => File.Delete(x));

            string xmlname = string.Format(CultureInfo.InvariantCulture, "({0})HelpContentSetup.msha", selLoc);
            xmlname = Path.Combine(directory, xmlname);
            // Creating setup indexes
            File.WriteAllText(
                //Path.Combine(directory, "HelpContentSetup.msha"),
                //Path.Combine(directory, xmlname),
                xmlname,
                HelpIndexManager.CreateSetupIndex(enumerable, selLoc), Encoding.UTF8);
            FileLastModifiedTime(xmlname, lastModifiedTime);

            foreach (var product in enumerable)
            {
                var lastProductModified = new DateTime(2000, 1, 1, 0, 0, 0);
                bool includeLocLanguage = false;
                foreach (var book in product.Books)
                {
                    if (selLoc.ToLowerInvariant() == ItemBase.LocaleAll.ToLowerInvariant()
                        || (selLoc.ToLowerInvariant() != "en-us"
                        && book.Locale.ToLowerInvariant() == selLoc.ToLowerInvariant()))
                        includeLocLanguage = true;

                    string bookFile = Path.Combine(directory, HelpIndexManager.CreateItemFileName(book, null));
                    if (File.Exists(bookFile))
                        File.Delete(bookFile);
                    File.WriteAllText(
                        bookFile,
                        HelpIndexManager.CreateBookPackagesIndex(product, book, selLoc), Encoding.UTF8);

                    FileLastModifiedTime(bookFile, book.LastModified);

                    if (book.LastModified > lastProductModified)
                        lastProductModified = book.LastModified;
                }

                var productFile = Path.Combine(directory, !includeLocLanguage ? HelpIndexManager.CreateItemFileName(product, null) : HelpIndexManager.CreateItemFileName(product, selLoc));

                if (File.Exists(productFile))
                    File.Delete(productFile);
                File.WriteAllText(
                    productFile,
                    HelpIndexManager.CreateProductBooksIndex(product, selLoc, lastProductModified), Encoding.UTF8);

                FileLastModifiedTime(productFile, lastProductModified);
            }

            string helpLibDir = directory.Substring(0, directory.LastIndexOf(MainForm.VsDirName, StringComparison.Ordinal));
            string oldDirectory = helpLibDir + MainForm.OldVsDirName;
            if (oldDirectory != directory)
            {
                // Cleaunup old packages
                CleanupOldPackages(packages, directory, oldDirectory, false, false);
                CleanupOldPackages(packages, directory, Path.Combine(oldDirectory, "packages"), false, true);
                foreach (var loc in locales)
                {
                    string targetDirectoryLoc = Path.Combine(oldDirectory, "packages", loc.ToLowerInvariant());
                    CleanupOldPackages(packages, directory, targetDirectoryLoc, false, true);
                }
            }

            // Create directory
            string targetDirectory = Path.Combine(directory, "packages");
            CleanupOldPackages(packages, directory, directory, false, false);
            CleanupOldPackages(packages, directory, targetDirectory, false, true);
            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(directory, "packages", loc.ToLowerInvariant());
                CleanupOldPackages(packages, directory, targetDirectoryLoc, true, true);
            }

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(targetDirectory, loc.ToLowerInvariant());
                if (!Directory.Exists(targetDirectoryLoc))
                    Directory.CreateDirectory(targetDirectoryLoc);
            }

            // Downloading packages
            foreach (Package package in packages.Values)
            //foreach (var package in packages)
            {
                string targetFileName = Path.Combine(targetDirectory, HelpIndexManager.CreatePackageFileName(package));
                if ((package.Name.ToLowerInvariant() != BrandingPackageName1.ToLowerInvariant())
                    && (package.Name.ToLowerInvariant() != BrandingPackageName2.ToLowerInvariant()))
                    targetFileName = Path.Combine(targetDirectory, package.Locale.ToLowerInvariant(), HelpIndexManager.CreatePackageFileName(package));

                bool download = false;
                    
                // If file exist and file length is the same, skip it
                if (File.Exists(targetFileName))
                {
                    FileInfo curFileInfo = new FileInfo(targetFileName);
                    if (package.LastModified != curFileInfo.LastWriteTime)
                    {
                        package.LastModified = FetchLastModified(package.CurrentLink);
                    }
                    //if (package.LastModified != curFileInfo.LastWriteTime)
                    //{
                    //    download = true;
                    //    File.Delete(targetFileName);
                    //}

                    if (package.PackageSizeBytes != curFileInfo.Length)
                    {
                        package.PackageSizeBytes = FetchContentLength(package.CurrentLink);
                    }
                    if (package.PackageSizeBytes != curFileInfo.Length)
                    {
                        download = true;
                        File.Delete(targetFileName);
                    }

                    //download = FetchContentLength(package.CurrentLink) != curFileInfo.Length;
                }
                else
                {
                    download = true;
                    package.LastModified = FetchLastModified(package.CurrentLink);
                }

                if (download)
                    _client.DownloadFile(package.CurrentLink, targetFileName);

                FileLastModifiedTime(targetFileName, package.LastModified);

                OnChangeProgress(100 * ++_packagesCountCurrent / _packagesCountTotal);

                if (worker.CancellationPending)
                    return;
            }

            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(targetDirectory, loc.ToLowerInvariant());
                FileLastModifiedTime(targetDirectoryLoc, localeModifieds[loc.ToLowerInvariant()], true);
            }
            FileLastModifiedTime(targetDirectory, lastModifiedTime, true);
            FileLastModifiedTime(directory, lastModifiedTime, true);
            FileLastModifiedTime(helpLibDir, lastModifiedTime, true);
        }

        private void CleanupOldPackages(/*List<Package> packages*/Dictionary<string, Package> packages, string cachePath, string cabDirectory, bool bDestDirectory, bool bCleanOtherCab)
        {
            if (!Directory.Exists(cabDirectory))
                return;
                //Directory.CreateDirectory(cabDirectory);

            foreach (string file in Directory.GetFiles(cabDirectory, "*.cab"))
            {
                string fileName = Path.GetFileName(file);
                bool actual = false;

                foreach (Package package in packages.Values)
                //foreach (var package in packages)
                {
                    if (String.Compare(fileName, HelpIndexManager.CreatePackageFileName(package), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        actual = true;

                        string directoryDest = cabDirectory;
                        if ( !bDestDirectory )
                        {
                            directoryDest = Path.Combine(cachePath, "packages");
                            if (!Directory.Exists(directoryDest))
                                Directory.CreateDirectory(directoryDest);

                            if ((package.Name.ToLowerInvariant() != BrandingPackageName1.ToLowerInvariant())
                                && (package.Name.ToLowerInvariant() != BrandingPackageName2.ToLowerInvariant()))
                                directoryDest = Path.Combine(cachePath, "packages", package.Locale.ToLowerInvariant());

                            if (!Directory.Exists(directoryDest))
                                Directory.CreateDirectory(directoryDest);
                        }

                        string packagePathDest = Path.Combine(directoryDest, HelpIndexManager.CreatePackageFileName(package));

                        if (file != packagePathDest)
                        {
                            if (bDestDirectory || !File.Exists(packagePathDest))
                            {
                                FileInfo oldFile = new FileInfo(file);
                                oldFile.MoveTo(Path.Combine(packagePathDest));
                            }
                            else
                            {
                                //actual = false;
                                File.Delete(file);
                            }
                        }

                        break;
                    }

                    //Compare(String strA, int indexA, String strB, int indexB, int length, bool ignoreCase);
                    else if (fileName != null && (string.Compare(fileName, 0, package.Name.ToLowerInvariant() + @"(", 0, package.Name.Length + 1, true) == 0
                        || string.Compare(fileName, 0, package.Name.ToLowerInvariant() + @".", 0, package.Name.Length + 1, true) == 0))
                        
                        //(fileName.ToLowerInvariant().Contains(package.Name.ToLowerInvariant() + @"(")
                        //                          || fileName.ToLowerInvariant().Contains(package.Name.ToLowerInvariant() + @".")))
                    {
                        //actual = false;
                        File.Delete(file);
                        break;
                    }
                }

                if (bCleanOtherCab && !actual)
                    File.Delete(file);
            }
        }

        private long FetchContentLength(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return 0;

            long result = 0;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                if (null != _proxy)
                    request.Proxy = _proxy;

                var response = (HttpWebResponse)request.GetResponse();
            
                result = response.ContentLength;
                response.Close();
            }
            catch (Exception e)
            {
                Program.LogException(e);
            }

            return result;
        }

        private void FileLastModifiedTime(string filePath, DateTime lastModifiedTime, bool bDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            //DateTime lastModifiedTimeUtc = lastModifiedTime.ToUniversalTime();
            try
            {
                if (bDirectory)
                {
                    Directory.SetCreationTime(filePath, lastModifiedTime);
                    Directory.SetLastAccessTime(filePath, lastModifiedTime);
                    Directory.SetLastWriteTime(filePath, lastModifiedTime);
                }
                else
                {
                    File.SetCreationTime(filePath, lastModifiedTime);
                    File.SetLastAccessTime(filePath, lastModifiedTime);
                    File.SetLastWriteTime(filePath, lastModifiedTime);
                }
            }
            catch (Exception e)
            {
                Program.LogException(e);
            }
        }

        private DateTime FetchLastModified(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return DateTime.Now;

            var result = DateTime.Now;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                if (null != _proxy)
                    request.Proxy = _proxy;

                var response = (HttpWebResponse)request.GetResponse();

                result = response.LastModified/*.ToUniversalTime()*/;
                response.Close();
            }
            catch (Exception e)
            {
                Program.LogException(e);
            }

            return result;
        }
        private void OnChangeProgress(int progressPercentage)
        {
            Contract.Requires(progressPercentage >= 0);

            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progressPercentage, null));
        }

        public void Dispose()
        {
            if (null != _client)
            {
                if (null != _proxy)
                    _proxy = null;

                _client.Dispose();
                _client = null;

                GC.SuppressFinalize(this);
            }
        }
    }
}