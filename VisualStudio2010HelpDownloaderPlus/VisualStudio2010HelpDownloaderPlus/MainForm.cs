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
        private IEnumerable<ProductsGroup> _productsGroups;
        private Locale locale;
        private bool _expanded = true;

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
                var tupleTotal = (Tuple<IEnumerable<ProductsGroup>, IList<Locale>>)e.Result;
               
                _productsGroups = tupleTotal.Item1;

                var locales = tupleTotal.Item2;
                locales.Insert(0, Locale.LocaleAll);

                _comboBoxFilter.DataSource = locales;
            }

            _buttonDownloadBooks.Enabled = true;
            _labelFilter.Enabled = true;
            _comboBoxFilter.Enabled = true;
            _buttonLoadBooks.Enabled = true;
            _buttonBrowseDirectory.Enabled = true;
            _labelLoadingBooks.Visible = false;
            _progressBarAction.Style = ProgressBarStyle.Continuous;

            if (_comboBoxFilter.Items.Count>0)
            {
                _comboBoxFilter.SelectedIndex = 3;
                //_comboBoxFilter.SelectedIndex = 13;
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

            foreach (TreeNode groupNode in _treeViewBooks.Nodes)
                foreach (TreeNode productNode in groupNode.Nodes)
                {
                    //var product = (Product)productNode.Tag;
                    var books = new List<Book>();

                    foreach (TreeNode bookNode in productNode.Nodes)
                        if (bookNode.Checked)
                            books.Add((Book)bookNode.Tag);

                    if (books.Count > 0)
                    {
                        var product = new Product();

                        product.Locale               = ((Product)productNode.Tag).Locale;
                        product.Name                 = ((Product)productNode.Tag).Name;
                        product.Description          = ((Product)productNode.Tag).Description;
                        product.Code                 = ((Product)productNode.Tag).Code;
                        product.CodeLink             = ((Product)productNode.Tag).CodeLink;
                        product.CodeDescription      = ((Product)productNode.Tag).CodeDescription;
                        product.IconLink             = ((Product)productNode.Tag).IconLink;
                        product.IconLinkDescription  = ((Product)productNode.Tag).IconLinkDescription;
                        product.GroupCode            = ((Product)productNode.Tag).GroupCode;
                        product.GroupCodeLink        = ((Product)productNode.Tag).GroupCodeLink;
                        product.GroupCodeDescription = ((Product)productNode.Tag).GroupCodeDescription;
                        product.Books                = books;

                        products.Add(product);
                    }
                }

            _buttonDownloadBooks.Enabled = false;
            _buttonLoadBooks.Enabled = false;
            _buttonBrowseDirectory.Enabled = false;
            _labelFilter.Enabled = false;
            _comboBoxFilter.Enabled = false;

            _backgroundWorkerDownloadBooks.RunWorkerAsync(new Tuple<IEnumerable<Product>, string>(products, _textBoxDirectory.Text + "\\VisualStudio10"));
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
                downloader.DownloadBooks(tuple.Item1, tuple.Item2, locale.Name, (BackgroundWorker)sender);

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

            /*var*/ locale = (Locale)_comboBoxFilter.SelectedItem;

            foreach (var group in _productsGroups)
            {
                var groupNode = _treeViewBooks.Nodes.Add(group.Name);

                int countProducts = 0;
                foreach (var product in group.Products)
                {
                    var productNode = groupNode.Nodes.Add(product.Name);
                    productNode.Tag = product;

                    int countBooks = 0;
                    foreach (var book in product.Books)
                    {
                        if ((locale.Code.ToLowerInvariant() == Locale.LocaleAll.Code.ToLowerInvariant()) ||
                            (locale.Code.ToLowerInvariant() == book.Locale.Code.ToLowerInvariant())
                             || ("en-us" == book.Locale.Code.ToLowerInvariant())
                            )
                        {
                            var node = productNode.Nodes.Add(
                                (book.Locale.Code.ToLowerInvariant() == "en-us") || (locale.Code.ToLowerInvariant() != Locale.LocaleAll.Code.ToLowerInvariant()) ? book.Name : book.ToString());
                            node.Tag = book;

                            string bookFile = Path.Combine(_textBoxDirectory.Text + "\\VisualStudio10", HelpIndexManager.CreateItemFileName(book, null));
                            if (File.Exists(bookFile))
                            {
                                _buttonDownloadBooks.Enabled = true;

                                node.Checked = true;
                                ++countBooks;
                            }
                        }
                    }

                    if (productNode.Nodes.Count > 0 && productNode.Nodes.Count == countBooks)
                    {
                        productNode.Checked = true;
                        ++countProducts;
                    }

                    if (productNode.Nodes.Count == 0)
                        productNode.Remove();
                }

                if (groupNode.Nodes.Count > 0 && groupNode.Nodes.Count == countProducts)
                {
                    groupNode.Checked = true;
                    ++countProducts;
                }

                if (groupNode.Nodes.Count == 0)
                    groupNode.Remove();
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

            _buttonLoadBooks.PerformClick();
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
            _treeViewBooks.Nodes.Clear();
            DisplayBooks();
        }

        #endregion
    }
}