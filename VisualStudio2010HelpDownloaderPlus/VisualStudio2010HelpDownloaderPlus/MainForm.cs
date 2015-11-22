using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VisualStudio2010HelpDownloaderPlus.Web;

namespace VisualStudio2010HelpDownloaderPlus
{
    /// <summary>
    /// Main application form.
    /// </summary>
    internal sealed partial class MainForm : Form
    {
        private IEnumerable<ProductGroup> _productsGroups;
        private List<string> _locales;
        private string selLocale;
        private bool _expanded = true;
        public static string VsDirName = @"\Visual Studio 2010";
        public static string OldVsDirName = @"\VisualStudio10";

        public MainForm()
        {
            InitializeComponent();

            Text = Application.ProductName;

            ContextMenu = new ContextMenu();
            ContextMenu.Popup += ContextMenuPopup;
            ContextMenu.MenuItems.Add("Expand All", ExpandAllNodesClick);
            ContextMenu.MenuItems.Add("Collapse All", CollapseAllNodesClick);
            ContextMenu.MenuItems.Add("-");
            ContextMenu.MenuItems.Add("Check All", CheckAllNodesClick);
            ContextMenu.MenuItems.Add("Uncheck All", UncheckAllNodesClick);

            _comboBoxFilter.DisplayMember = "NameNormalized";
            _textBoxDirectory.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "HelpLibrary");
            //_textBoxDirectory.Text = Path.Combine(@"F:\Visual Studio\HelpLibrary");
        }

        #region UI methods

        private void ContextMenuPopup(object sender, EventArgs e)
        {
            ContextMenu.MenuItems.ForEach<MenuItem>(x => x.Enabled = _treeViewBooks.Nodes.Count > 0);
        }

        private void ExpandAllNodes()
        {
            _treeViewBooks.BeginUpdate();
            _treeViewBooks.Nodes.ForEach<TreeNode>(x => x.ExpandAll());
            _treeViewBooks.EndUpdate();
        }

        private void ExpandAllNodesClick(object sender, EventArgs e)
        {
            _expanded = true;
            ExpandAllNodes();
        }

        private void CollapseAllNodesClick(object sender, EventArgs e)
        {
            _expanded = false;

            _treeViewBooks.BeginUpdate();
            _treeViewBooks.Nodes.ForEach<TreeNode>(x => x.Collapse());
            _treeViewBooks.EndUpdate();
        }

        private void CheckAllNodesClick(object sender, EventArgs e)
        {
            foreach (TreeNode x in _treeViewBooks.Nodes)
            {
                x.Checked = true;
                ChangeChildNodesState(x, true);
            }
        }

        private void UncheckAllNodesClick(object sender, EventArgs e)
        {
            foreach (TreeNode x in _treeViewBooks.Nodes)
            {
                x.Checked = false;
                ChangeChildNodesState(x, false);
            }
        }

        private void ChangeChildNodesState(TreeNode node, bool state)
        {
            foreach (TreeNode x in node.Nodes)
            {
                x.Checked = state;

                ChangeChildNodesState(x, state);
            }
        }

        private bool AtLeastOneBookSelected()
        {
            foreach (TreeNode groupNode in _treeViewBooks.Nodes)
                foreach (TreeNode productNode in groupNode.Nodes)
                    foreach (TreeNode bookNode in productNode.Nodes)
                        if (bookNode.Checked)
                            return true;

            return false;
        }

        private void NodesAfterCheck(object sender, TreeViewEventArgs e)
        {
            ChangeChildNodesState(e.Node, e.Node.Checked);

            _buttonDownloadBooks.Enabled = AtLeastOneBookSelected();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            NativeMethods.SetWindowTheme(new HandleRef(this, _treeViewBooks.Handle), "Explorer", null);
        }

        #endregion

        #region Logic methods

        private void LoadBooksClick(object sender, EventArgs e)
        {
            _progressBarAction.Style = ProgressBarStyle.Marquee;
            _buttonDownloadBooks.Enabled = false;
            _labelFilter.Enabled = false;
            _comboBoxFilter.Enabled = false;
            _buttonLoadBooks.Enabled = false;
            _buttonBrowseDirectory.Enabled = false;
            _labelStartupTip.Visible = false;
            _labelLoadingBooks.Visible = true;
            _treeViewBooks.Nodes.Clear();

            _backgroundWorkerLoadBooks.RunWorkerAsync();
        }

        private void LoadBooksDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var downloader = new Downloader())
                    e.Result = downloader.LoadBooksInformation();
            }
            catch (WebException ex)
            {
                Program.LogException(ex);
            }
        }

        private void LoadBooksWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (null == e.Result)
            {
                MessageBox.Show(this, "Error occured while loading books information. See event log for details.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var tupleTotal = (Tuple<ICollection<ProductGroup>, ICollection<string>>)e.Result;

                _productsGroups = tupleTotal.Item1;

                _locales = tupleTotal.Item2 as List<string>;
                (_locales as List<string>).Insert(0, ItemBase.LocaleAll);

                //_comboBoxFilter.DataSource = _locales;
            }

            _labelFilter.Enabled = true;
            _comboBoxFilter.Enabled = true;
            _buttonDownloadBooks.Enabled = true;

            _buttonLoadBooks.Enabled = true;
            _buttonBrowseDirectory.Enabled = true;
            _labelLoadingBooks.Visible = false;
            _progressBarAction.Style = ProgressBarStyle.Continuous;

            if ((_locales as List<string>).Count > 0)
            {
                int i = 0;
                int selIndex = 0;
                //_comboBoxFilter.SelectedIndex = 0;
                foreach (string locale in _locales)
                {
                    if ("en-us" == locale)
                    {
                        selIndex = i;
                        //_comboBoxFilter.SelectedIndex = i;
                        //break; // DEBUG
                    }

                    //if ("zh-cn" == locale) // DEBUG
                    //{
                    //    selIndex = i;
                    //    //_comboBoxFilter.SelectedIndex = i;
                    //    //break;
                    //}
                    ++i;
                }

                _comboBoxFilter.DataSource = _locales;
                _comboBoxFilter.SelectedIndex = selIndex;
            }
            else
            {
                _labelFilter.Enabled = false;
                _comboBoxFilter.Enabled = false;
                _buttonDownloadBooks.Enabled = false;
                _labelStartupTip.Visible = true;
            }
        }

        private void DownloadBooksClick(object sender, EventArgs e)
        {
            var products = new List<Product>();

            foreach (TreeNode productGroupNode in _treeViewBooks.Nodes)
                foreach (TreeNode productNode in productGroupNode.Nodes)
                {
                    //var product = (Product)productNode.PackageEtag;
                    var books = new List<Book>();

                    foreach (TreeNode bookNode in productNode.Nodes)
                        if (bookNode.Checked)
                            books.Add((Book)bookNode.Tag);

                    var productSrc = productNode.Tag as Product;
                    if (books.Count > 0 && productSrc != null)
                    {
                        Product product = null;
                        //foreach (var p in products)
                        //{
                        //    if (p.Code == productSrc.Code)
                        //    {
                        //        product = p;
                        //        break;
                        //    }
                        //}

                        //if (product != null)
                        //{
                        //    foreach (var b in books)
                        //        product.Books.Add(b);
                        //}
                        //else
                        //{
                            product = new Product();

                            product.Locale = productSrc.Locale;
                            product.Name = productSrc.Name;
                            product.Description = productSrc.Description;
                            product.IconSrc = productSrc.IconSrc;
                            product.IconAlt = productSrc.IconAlt;
                            product.Link = productSrc.Link;
                            product.Code = productSrc.Code;
                            product.LinkDescription = productSrc.LinkDescription;
                            product.ProductGroupCode = productSrc.ProductGroupCode;
                            product.ProductGroupLink = productSrc.ProductGroupLink;
                            product.ProductGroupDescription = productSrc.ProductGroupDescription;
                            product.Books = books;

                            products.Add(product);
                        //}
                    }
                }

            _buttonDownloadBooks.Enabled = false;
            _buttonLoadBooks.Enabled = false;
            _buttonBrowseDirectory.Enabled = false;
            _labelFilter.Enabled = false;
            _comboBoxFilter.Enabled = false;

            _backgroundWorkerDownloadBooks.RunWorkerAsync(new Tuple<IEnumerable<Product>, string>(products, _textBoxDirectory.Text + VsDirName));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void DownloadBooksDoWork(object sender, DoWorkEventArgs e)
        {
            Downloader downloader = null;

            _buttonLoadBooks.Enabled = false;
            _buttonDownloadBooks.Enabled = false;
            _buttonBrowseDirectory.Enabled = false;
            _labelFilter.Enabled = false;
            _comboBoxFilter.Enabled = false;

            try
            {
                var tuple = (Tuple<IEnumerable<Product>, string>)e.Argument;

                downloader = new Downloader();
                downloader.ProgressChanged += DownloaderProgressChanged;
                downloader.DownloadBooks(tuple.Item1, tuple.Item2, selLocale, (BackgroundWorker)sender);

                e.Result = 0;
            }
            catch (Exception ex)
            {
                Program.LogException(ex);
            }
            finally
            {
                if (null != downloader)
                {
                    downloader.ProgressChanged -= DownloaderProgressChanged;
                    downloader.Dispose();
                    downloader = null;
                }
            }
        }

        private void DownloadBooksWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (null == e.Result)
                MessageBox.Show(this, "Error occured while downloading books. See event log for details.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(this, "All selected books downloaded successfully", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            _buttonLoadBooks.Enabled = true;
            _buttonDownloadBooks.Enabled = true;
            _buttonBrowseDirectory.Enabled = true;
            _labelFilter.Enabled = true;
            _comboBoxFilter.Enabled = true;

            _progressBarAction.Value = 0;
        }

        private void DisplayBooks()
        {
            _buttonDownloadBooks.Enabled = false;

            var selLoc = _comboBoxFilter.SelectedItem as string;
            _treeViewBooks.Nodes.Clear();

            foreach (var productGroups in _productsGroups)
            {
                var productGroupsNode = _treeViewBooks.Nodes.Add(productGroups.Name);

                int countProduct = 0;
                foreach (var product in productGroups.Products)
                {
                    var productNode = productGroupsNode.Nodes.Add(product.Name);
                    productNode.Tag = product;

                    int countBook = 0;
                    foreach (var book in product.Books)
                    {
                        bool add = true;
                        if (selLoc.ToLowerInvariant() != ItemBase.LocaleAll.ToLowerInvariant())
                        {
                            if ("en-us" != book.Locale.ToLowerInvariant()
                                && selLoc.ToLowerInvariant() != book.Locale.ToLowerInvariant())
                            {
                                add = false;
                            }
                        }

                        if (add)
                        {
                            var node = productNode.Nodes.Add(book.Name);
                            node.Tag = book;

                            bool exists = true;
                            string bookFile = Path.Combine(_textBoxDirectory.Text + VsDirName, HelpIndexManager.CreateItemFileName(book, null));
                            if (!File.Exists(bookFile))
                            {
                                bookFile = Path.Combine(_textBoxDirectory.Text + OldVsDirName, HelpIndexManager.CreateItemFileName(book, null));
                                if (!File.Exists(bookFile))
                                    exists = false;
                            }

                            if (exists)
                            {
                                _buttonDownloadBooks.Enabled = true;

                                node.Checked = true;
                                ++countBook;
                            }
                        }

                        //if (selLocale.ToLowerInvariant() == ItemBase.LocaleAll.ToLowerInvariant()
                        //    || selLocale.ToLowerInvariant() == book.Locale.ToLowerInvariant()
                        //     || "en-us" == book.Locale.ToLowerInvariant()
                        //    )
                        //{
                        //    var node = productNode.Nodes.Add(
                        //        (book.Locale.ToLowerInvariant() == "en-us") || (selLocale.ToLowerInvariant() != ItemBase.LocaleAll.ToLowerInvariant()) ? book.Name : book.ToString());
                        //    node.Tag = book;

                        //    string bookFile = Path.Combine(_textBoxDirectory.Text + "\\VisualStudio10", HelpIndexManager.CreateItemFileName(book, null));
                        //    if (File.Exists(bookFile))
                        //    {
                        //        _buttonDownloadBooks.Enabled = true;

                        //        node.Checked = true;
                        //        ++countBook;
                        //    }
                        //}
                    }

                    if (productNode.Nodes.Count > 0 && productNode.Nodes.Count == countBook)
                    {
                        productNode.Checked = true;
                        ++countProduct;
                    }

                    if (productNode.Nodes.Count == 0)
                        productNode.Remove();
                }

                if (productGroupsNode.Nodes.Count > 0 && productGroupsNode.Nodes.Count == countProduct)
                {
                    productGroupsNode.Checked = true;
                    ++countProduct;
                }

                if (productGroupsNode.Nodes.Count == 0)
                    productGroupsNode.Remove();
            }

            if (_expanded)
                ExpandAllNodes();
       }

        private void DownloaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _backgroundWorkerDownloadBooks.ReportProgress(e.ProgressPercentage);
        }

        private void DownloadBooksProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressBarAction.Value = e.ProgressPercentage;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //_buttonLoadBooks.PerformClick();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_backgroundWorkerLoadBooks.IsBusy && !_backgroundWorkerLoadBooks.CancellationPending)
                _backgroundWorkerLoadBooks.CancelAsync();

            if (_backgroundWorkerDownloadBooks.IsBusy && !_backgroundWorkerDownloadBooks.CancellationPending)
                _backgroundWorkerDownloadBooks.CancelAsync();

            base.OnClosing(e);
        }

        private void BrowseDirectoryClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                dialog.SelectedPath = _textBoxDirectory.Text;
                dialog.Description = "Select folder to store selected MSDN Library books";

                if (DialogResult.OK == dialog.ShowDialog(this))
                    _textBoxDirectory.Text = dialog.SelectedPath;
            }
        }

        private void ComboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            selLocale = _comboBoxFilter.SelectedItem as string;
            _treeViewBooks.Nodes.Clear();
            DisplayBooks();
        }

        #endregion
    }
}