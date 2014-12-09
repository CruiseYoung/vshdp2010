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
        private const string _BRANDING_PACKAGE_URL = @"http://packages.mtps.microsoft.com/brands/";

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
            _client.BaseAddress = @"http://services.mtps.microsoft.com/serviceapi/products/";

            string settingsFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                string.Format(CultureInfo.InvariantCulture, "{0}.xml", Assembly.GetEntryAssembly().GetName().Name));

            if (File.Exists(settingsFile))
                try
                {
                    var element = XDocument.Load(settingsFile).Root.Elements().Where(x => x.Name.LocalName == "proxy").Single();

                    _proxy = new WebProxy(element.Attributes().Where(x => x.Name.LocalName == "address").Single().Value);
                    if (element.Attributes().Any(x => x.Name.LocalName == "default" && (x.Value == "1" || x.Value == "true")))
					{
						_proxy.UseDefaultCredentials = true;
                        _proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
					}
					else
					{
						_proxy.Credentials = new NetworkCredential(
							element.Attributes().Where(x => x.Name.LocalName == "login").Single().Value,
							element.Attributes().Where(x => x.Name.LocalName == "password").Single().Value,
							element.Attributes().Where(x => x.Name.LocalName == "domain").Single().Value);
					}

                    _client.Proxy = _proxy;
                }
                catch (Exception ex)
                {
                    Program.LogException(ex);
                }
        }

        /// <summary>
        /// Download books information.
        /// </summary>
        /// <returns>List of available product groups.</returns>
        public Tuple<IEnumerable<ProductsGroup>, IList<Locale>> LoadBooksInformation()
        {
            Contract.Ensures(null != Contract.Result<Tuple<IEnumerable<ProductsGroup>, IList<Locale>>>());

            var productGroups = HelpIndexManager.LoadProductGroups(_client.DownloadData(""));
            var locales = new List<Locale>();

            foreach (var productGroup in productGroups)
                if (/*!string.IsNullOrWhiteSpace(productGroup.Code)*/
                    !string.IsNullOrWhiteSpace(productGroup.CodeLink))
                {
                    //productGroup.Products = HelpIndexManager.LoadProducts(
                    //    _client.DownloadData(string.Format(CultureInfo.InvariantCulture,
                    //        "{0}", productGroup.Code)));
                    productGroup.Products = HelpIndexManager.LoadProducts(
                        _client.DownloadData(productGroup.CodeLink));

                    foreach (var product in productGroup.Products)
                        if (/*!string.IsNullOrWhiteSpace(product.Code)*/
                            !string.IsNullOrWhiteSpace(product.CodeLink))
                        {
                            product.GroupCode = productGroup.Code;
                            product.GroupCodeLink = productGroup.CodeLink;
                            product.GroupCodeDescription = productGroup.CodeDescription;

                            //var tuple = HelpIndexManager.LoadBooks(
                            //    _client.DownloadData(string.Format(CultureInfo.InvariantCulture,
                            //        "{0}/{1}", product.GroupCode, product.Code)));

                            var tuple = HelpIndexManager.LoadBooks(
                                _client.DownloadData(product.CodeLink));

                            product.Books = tuple.Item1;

                            foreach (var locale in tuple.Item2)
                                if (!locales.Contains(locale))
                                    locales.Add(locale);
                        }
                }

            locales.Sort();

            return new Tuple<IEnumerable<ProductsGroup>, IList<Locale>>(productGroups, locales);
        }

        /// <summary>
        /// Download books.
        /// </summary>
        /// <param name="products">Products list.</param>
        /// <param name="directory">Target directory.</param>
        /// <param name="worker">Background worker instance.</param>
        public void DownloadBooks(IEnumerable<Product> products, string directory, string locLanguage, BackgroundWorker worker)
        {
            Contract.Requires(null != worker);

            if ((null == directory) || (null == products) || (null == locLanguage))
                return;

            _packagesCountTotal = 0;
            _packagesCountCurrent = 0;

            var lastModifiedTime = new DateTime(2000, 1, 1, 0, 0, 0);
            var packages = new List<Package>();

            // Add branding packages
            var brandingPackgeName1 = new Package
            {
                Name = HelpIndexManager.BRANDING_PACKAGE_NAME1,
                Deployed = @"true",
                LastModified = DateTime.Now,
                Tag = null,
                Link = string.Format(CultureInfo.InvariantCulture, "{0}{1}.cab", _BRANDING_PACKAGE_URL, HelpIndexManager.BRANDING_PACKAGE_NAME1),
                SizeBytes = 0,
                SizeBytesUncompressed = 0,
                ConstituentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}", @"../../serviceapi/packages/brands/", HelpIndexManager.BRANDING_PACKAGE_NAME1)
            };
            brandingPackgeName1.LastModified = FetchLastModified(brandingPackgeName1.Link);
            brandingPackgeName1.SizeBytes = FetchContentLength(brandingPackgeName1.Link);
            brandingPackgeName1.SizeBytesUncompressed = brandingPackgeName1.SizeBytes;
            packages.Add(brandingPackgeName1);
            if (brandingPackgeName1.LastModified > lastModifiedTime)
                lastModifiedTime = brandingPackgeName1.LastModified;

            var brandingPackgeName2 = new Package
            {
                Name = HelpIndexManager.BRANDING_PACKAGE_NAME2,
                Deployed = @"true",
                LastModified = DateTime.Now,
                Tag = null,
                Link = string.Format(CultureInfo.InvariantCulture, "{0}{1}.cab", _BRANDING_PACKAGE_URL, HelpIndexManager.BRANDING_PACKAGE_NAME2),
                SizeBytes = 0,
                SizeBytesUncompressed = 0,
                ConstituentLink = string.Format(CultureInfo.InvariantCulture, "{0}{1}", @"../../serviceapi/packages/brands/", HelpIndexManager.BRANDING_PACKAGE_NAME2)
            };
            brandingPackgeName2.LastModified = FetchLastModified(brandingPackgeName2.Link);
            brandingPackgeName2.SizeBytes = FetchContentLength(brandingPackgeName2.Link);
            brandingPackgeName2.SizeBytesUncompressed = brandingPackgeName2.SizeBytes;
            packages.Add(brandingPackgeName2);
            if (brandingPackgeName2.LastModified > lastModifiedTime)
                lastModifiedTime = brandingPackgeName2.LastModified;

            var locales = new List<Locale>();
            var LocaleModifieds = new Dictionary<string, DateTime>();
            // Fetching packages information
            foreach (var product in products)
                foreach (var book in product.Books)
                    if (!string.IsNullOrWhiteSpace(book.CodeLink))
                    {
                        book.Packages = HelpIndexManager.LoadPackages(
                            book,
                            _client.DownloadData(book.CodeLink));

                        //string packagePathDest = Path.Combine(directory, "packages", book.Locale.Code.ToLowerInvariant());

                        //if (!Directory.Exists(packagePathDest))
                        //    Directory.CreateDirectory(packagePathDest);
                        if (!locales.Contains(book.Locale))
                        {
                            locales.Add(book.Locale);

                            var LocaleModified = new DateTime(2000, 1, 1, 0, 0, 0);
                            LocaleModified = book.LastModified;
                            LocaleModifieds.Add(book.Locale.Code.ToLowerInvariant(), LocaleModified);
                        }
                        else
                        {
                            if (book.LastModified > LocaleModifieds[book.Locale.Code.ToLowerInvariant()])
                                LocaleModifieds[book.Locale.Code.ToLowerInvariant()] = book.LastModified;
                        }

                        foreach (var package in book.Packages)
                            //if ((package.Name != HelpIndexManager.BRANDING_PACKAGE_NAME1) &&
                            //    (package.Name != HelpIndexManager.BRANDING_PACKAGE_NAME2))
                            if (!packages.Contains(package))
                            {
                                packages.Add(package);
                                if (package.LastModified > lastModifiedTime)
                                    lastModifiedTime = package.LastModified;
                            }

                        if (worker.CancellationPending)
                            return;
                    }

            _packagesCountTotal = packages.Count;

            //if (locLanguage.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant())
                if (0 == locales.Count)
                    return;
                else if (1 == locales.Count)
                    locLanguage = locales[0].Name;
                else if (2 == locales.Count)
                {
                    bool bHasEN_US = false;
                    string strLoc = locLanguage;
                    foreach (var loc in locales)
                        if (loc.Name.ToLowerInvariant() == "en-us")
                            bHasEN_US = true;
                        else
                            strLoc = loc.Name;

                    if (bHasEN_US)
                        locLanguage = strLoc;
                    else
                        locLanguage = Locale.LocaleAll.Code;
                }
                else
                    locLanguage = Locale.LocaleAll.Code;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Generate Download File List
            try
            {
                string listFileName = string.Format(CultureInfo.InvariantCulture, "({0})PackageList.txt", locLanguage);
                string listFilePath = Path.Combine(directory, listFileName);

                if (File.Exists(listFilePath))
                    File.Delete(listFilePath);

                var writer = new StreamWriter(listFilePath);
                foreach (var package in packages)
                    writer.WriteLine(package.Link);
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

                    if (fileName == @"(" + locLanguage + @")HelpContentSetup")
                    {
                        File.Delete(file);
                        //break;
                    }
                }
            }
          
            Directory.GetFiles(directory, "*.xml").ForEach(x => File.Delete(x));
            //Directory.GetFiles(directory, "*.html").ForEach(x => File.Delete(x));

            string xmlname = string.Format(CultureInfo.InvariantCulture, "({0})HelpContentSetup.msha", locLanguage);
            xmlname = Path.Combine(directory, xmlname);
            // Creating setup indexes
            File.WriteAllText(
                //Path.Combine(directory, "HelpContentSetup.msha"),
                //Path.Combine(directory, xmlname),
                xmlname,
                HelpIndexManager.CreateSetupIndex(products, locLanguage), Encoding.UTF8);
            FileLastModifiedTime(xmlname, lastModifiedTime);

            foreach (var product in products)
            {
                var lastProductModified = new DateTime(2000, 1, 1, 0, 0, 0);
                bool includeLocLanguage = false;
                foreach (var book in product.Books)
                {
                    if (locLanguage.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant()
                        || (locLanguage.ToLowerInvariant() != "en-us"
                        && book.Locale.Name.ToLowerInvariant() == locLanguage.ToLowerInvariant()))
                        includeLocLanguage = true;

                    string bookFile = Path.Combine(directory, HelpIndexManager.CreateItemFileName(book, null));
                    if (File.Exists(bookFile))
                        File.Delete(bookFile);
                    File.WriteAllText(
                        bookFile,
                        HelpIndexManager.CreateBookPackagesIndex(product, book, locLanguage), Encoding.UTF8);

                    FileLastModifiedTime(bookFile, book.LastModified);

                    if (book.LastModified > lastProductModified)
                        lastProductModified = book.LastModified;
                }

                string productFile;
                if (!includeLocLanguage)
                    productFile = Path.Combine(directory, HelpIndexManager.CreateItemFileName(product, null));
                else
                    productFile = Path.Combine(directory, HelpIndexManager.CreateItemFileName(product, locLanguage));

                if (File.Exists(productFile))
                    File.Delete(productFile);
                File.WriteAllText(
                    productFile,
                    HelpIndexManager.CreateProductBooksIndex(product, locLanguage), Encoding.UTF8);

                FileLastModifiedTime(productFile, lastProductModified);
            }

            // Create directory
            string targetDirectory = Path.Combine(directory, "packages");

            // Cleaunup old packages
            CleanupOldPackages(packages, directory, directory, false, false);
            CleanupOldPackages(packages, directory, targetDirectory, false, true);
            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(directory, "packages", loc.Code.ToLowerInvariant());
                CleanupOldPackages(packages, directory, targetDirectoryLoc, true, true);
            }

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(targetDirectory, loc.Code.ToLowerInvariant());
                if (!Directory.Exists(targetDirectoryLoc))
                    Directory.CreateDirectory(targetDirectoryLoc);
            }

            // Downloading packages
            foreach (var package in packages)
            {
                string targetFileName = Path.Combine(targetDirectory, HelpIndexManager.CreatePackageFileName(package));
                if ((package.Name.ToLowerInvariant() != HelpIndexManager.BRANDING_PACKAGE_NAME1.ToLowerInvariant())
                    && (package.Name.ToLowerInvariant() != HelpIndexManager.BRANDING_PACKAGE_NAME2.ToLowerInvariant()))
                    targetFileName = Path.Combine(targetDirectory, package.Locale.Code.ToLowerInvariant(), HelpIndexManager.CreatePackageFileName(package));

                bool download = true;
                    
                // If file exist and file length is the same, skip it
                if (File.Exists(targetFileName))
                    download = FetchContentLength(package.Link) != new FileInfo(targetFileName).Length;

                if (download)
                    _client.DownloadFile(package.Link, targetFileName);

                FileLastModifiedTime(targetFileName, package.LastModified);

                OnChangeProgress(100 * ++_packagesCountCurrent / _packagesCountTotal);

                if (worker.CancellationPending)
                    return;
            }

            foreach (var loc in locales)
            {
                string targetDirectoryLoc = Path.Combine(targetDirectory, loc.Code.ToLowerInvariant());
                FileLastModifiedTime(targetDirectoryLoc, LocaleModifieds[loc.Code.ToLowerInvariant()], true);                
            }
            FileLastModifiedTime(targetDirectory, lastModifiedTime, true);
            FileLastModifiedTime(directory, lastModifiedTime, true);
        }

        private void CleanupOldPackages(List<Package> packages, string cachePath, string cabDirectory, bool bDestDirectory, bool bCleanOtherCab)
        {
            if (!Directory.Exists(cabDirectory))
                return;
                //Directory.CreateDirectory(cabDirectory);

            foreach (string file in Directory.GetFiles(cabDirectory, "*.cab"))
            {
                string fileName = Path.GetFileName(file);
                bool actual = false;

                foreach (var package in packages)
                {
                    if (string.Compare(fileName.ToLowerInvariant(), HelpIndexManager.CreatePackageFileName(package).ToLowerInvariant(), true) == 0)
                    {
                        actual = true;

						string directoryDest = cabDirectory;
						if ( !bDestDirectory )
						{
							directoryDest = Path.Combine(cachePath, "packages");
                            if (!Directory.Exists(directoryDest))
                                Directory.CreateDirectory(directoryDest);

							if ((package.Name.ToLowerInvariant() != HelpIndexManager.BRANDING_PACKAGE_NAME1.ToLowerInvariant())
								&& (package.Name.ToLowerInvariant() != HelpIndexManager.BRANDING_PACKAGE_NAME2.ToLowerInvariant()))
								directoryDest = Path.Combine(cachePath, "packages", package.Locale.Code.ToLowerInvariant());

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
                    else if (fileName.ToLowerInvariant().Contains(package.Name.ToLowerInvariant() + @"(")
                        || fileName.ToLowerInvariant().Contains(package.Name.ToLowerInvariant() + @"."))
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

            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            if (null != _proxy)
                request.Proxy = _proxy;

            var response = (HttpWebResponse)request.GetResponse();
            
            result = response.ContentLength;
            response.Close();

            return result;
        }

        private void FileLastModifiedTime(string FilePath, DateTime lastModifiedTime, bool bDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return;

			try
            {
                if (bDirectory)
				{
					Directory.SetCreationTime(FilePath, lastModifiedTime);
					Directory.SetLastAccessTime(FilePath, lastModifiedTime);
					Directory.SetLastWriteTime(FilePath, lastModifiedTime);
				}
				else
				{
					File.SetCreationTime(FilePath, lastModifiedTime);
					File.SetLastAccessTime(FilePath, lastModifiedTime);
					File.SetLastWriteTime(FilePath, lastModifiedTime);
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

            DateTime result = DateTime.Now;

            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            if (null != _proxy)
                request.Proxy = _proxy;

            var response = (HttpWebResponse)request.GetResponse();

            result = response.LastModified;
            response.Close();

            return result;
        }
        
        private void OnChangeProgress(int progressPercentage)
        {
            Contract.Requires(progressPercentage >= 0);

            if (null != ProgressChanged)
                ProgressChanged(this, new ProgressChangedEventArgs(progressPercentage, null));
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