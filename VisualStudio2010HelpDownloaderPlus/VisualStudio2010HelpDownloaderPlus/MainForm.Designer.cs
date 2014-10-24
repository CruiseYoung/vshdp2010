namespace VisualStudio2010HelpDownloaderPlus
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _productsGroups = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._buttonLoadBooks = new System.Windows.Forms.Button();
            this._treeViewBooks = new System.Windows.Forms.TreeView();
            this._textBoxDirectory = new System.Windows.Forms.TextBox();
            this._buttonBrowseDirectory = new System.Windows.Forms.Button();
            this._progressBarAction = new System.Windows.Forms.ProgressBar();
            this._labelStartupTip = new System.Windows.Forms.Label();
            this._buttonDownloadBooks = new System.Windows.Forms.Button();
            this._labelDirectory = new System.Windows.Forms.Label();
            this._backgroundWorkerLoadBooks = new System.ComponentModel.BackgroundWorker();
            this._backgroundWorkerDownloadBooks = new System.ComponentModel.BackgroundWorker();
            this._labelLoadingBooks = new System.Windows.Forms.Label();
            this._labelFilter = new System.Windows.Forms.Label();
            this._comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _buttonLoadBooks
            // 
            this._buttonLoadBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._buttonLoadBooks.Location = new System.Drawing.Point(12, 348);
            this._buttonLoadBooks.Name = "_buttonLoadBooks";
            this._buttonLoadBooks.Size = new System.Drawing.Size(87, 27);
            this._buttonLoadBooks.TabIndex = 1;
            this._buttonLoadBooks.Text = "Load Books";
            this._buttonLoadBooks.UseVisualStyleBackColor = true;
            this._buttonLoadBooks.Click += new System.EventHandler(this.LoadBooksClick);
            // 
            // _treeViewBooks
            // 
            this._treeViewBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._treeViewBooks.CheckBoxes = true;
            this._treeViewBooks.HotTracking = true;
            this._treeViewBooks.Location = new System.Drawing.Point(12, 12);
            this._treeViewBooks.Name = "_treeViewBooks";
            this._treeViewBooks.ShowLines = false;
            this._treeViewBooks.Size = new System.Drawing.Size(492, 292);
            this._treeViewBooks.TabIndex = 3;
            this._treeViewBooks.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.NodesAfterCheck);
            // 
            // _textBoxDirectory
            // 
            this._textBoxDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxDirectory.Location = new System.Drawing.Point(103, 319);
            this._textBoxDirectory.Name = "_textBoxDirectory";
            this._textBoxDirectory.ReadOnly = true;
            this._textBoxDirectory.Size = new System.Drawing.Size(364, 23);
            this._textBoxDirectory.TabIndex = 8;
            // 
            // _buttonBrowseDirectory
            // 
            this._buttonBrowseDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonBrowseDirectory.Enabled = false;
            this._buttonBrowseDirectory.Image = ((System.Drawing.Image)(resources.GetObject("_buttonBrowseDirectory.Image")));
            this._buttonBrowseDirectory.Location = new System.Drawing.Point(473, 317);
            this._buttonBrowseDirectory.Name = "_buttonBrowseDirectory";
            this._buttonBrowseDirectory.Size = new System.Drawing.Size(31, 27);
            this._buttonBrowseDirectory.TabIndex = 0;
            this._buttonBrowseDirectory.UseVisualStyleBackColor = true;
            this._buttonBrowseDirectory.Click += new System.EventHandler(this.BrowseDirectoryClick);
            // 
            // _progressBarAction
            // 
            this._progressBarAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBarAction.Location = new System.Drawing.Point(12, 379);
            this._progressBarAction.MarqueeAnimationSpeed = 25;
            this._progressBarAction.Name = "_progressBarAction";
            this._progressBarAction.Size = new System.Drawing.Size(492, 11);
            this._progressBarAction.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBarAction.TabIndex = 10;
            // 
            // _labelStartupTip
            // 
            this._labelStartupTip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._labelStartupTip.AutoSize = true;
            this._labelStartupTip.BackColor = System.Drawing.SystemColors.Window;
            this._labelStartupTip.Location = new System.Drawing.Point(112, 136);
            this._labelStartupTip.Name = "_labelStartupTip";
            this._labelStartupTip.Size = new System.Drawing.Size(292, 15);
            this._labelStartupTip.TabIndex = 5;
            this._labelStartupTip.Text = "Press \"Load Books\" button for load books information";
            // 
            // _buttonDownloadBooks
            // 
            this._buttonDownloadBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._buttonDownloadBooks.Enabled = false;
            this._buttonDownloadBooks.Location = new System.Drawing.Point(417, 348);
            this._buttonDownloadBooks.Name = "_buttonDownloadBooks";
            this._buttonDownloadBooks.Size = new System.Drawing.Size(87, 27);
            this._buttonDownloadBooks.TabIndex = 4;
            this._buttonDownloadBooks.Text = "Download";
            this._buttonDownloadBooks.UseVisualStyleBackColor = true;
            this._buttonDownloadBooks.Click += new System.EventHandler(this.DownloadBooksClick);
            // 
            // _labelDirectory
            // 
            this._labelDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._labelDirectory.AutoSize = true;
            this._labelDirectory.Location = new System.Drawing.Point(12, 323);
            this._labelDirectory.Name = "_labelDirectory";
            this._labelDirectory.Size = new System.Drawing.Size(71, 15);
            this._labelDirectory.TabIndex = 7;
            this._labelDirectory.Text = "Store files in";
            // 
            // _backgroundWorkerLoadBooks
            // 
            this._backgroundWorkerLoadBooks.WorkerSupportsCancellation = true;
            this._backgroundWorkerLoadBooks.DoWork += new System.ComponentModel.DoWorkEventHandler(this.LoadBooksDoWork);
            this._backgroundWorkerLoadBooks.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.LoadBooksWorkCompleted);
            // 
            // _backgroundWorkerDownloadBooks
            // 
            this._backgroundWorkerDownloadBooks.WorkerReportsProgress = true;
            this._backgroundWorkerDownloadBooks.WorkerSupportsCancellation = true;
            this._backgroundWorkerDownloadBooks.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DownloadBooksDoWork);
            this._backgroundWorkerDownloadBooks.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.DownloadBooksProgressChanged);
            this._backgroundWorkerDownloadBooks.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DownloadBooksWorkCompleted);
            // 
            // _labelLoadingBooks
            // 
            this._labelLoadingBooks.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._labelLoadingBooks.AutoSize = true;
            this._labelLoadingBooks.BackColor = System.Drawing.SystemColors.Window;
            this._labelLoadingBooks.Location = new System.Drawing.Point(178, 136);
            this._labelLoadingBooks.Name = "_labelLoadingBooks";
            this._labelLoadingBooks.Size = new System.Drawing.Size(160, 15);
            this._labelLoadingBooks.TabIndex = 6;
            this._labelLoadingBooks.Text = "Loading books information...";
            this._labelLoadingBooks.Visible = false;
            // 
            // _labelFilter
            // 
            this._labelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._labelFilter.AutoSize = true;
            this._labelFilter.Enabled = false;
            this._labelFilter.Location = new System.Drawing.Point(154, 354);
            this._labelFilter.Name = "_labelFilter";
            this._labelFilter.Size = new System.Drawing.Size(84, 15);
            this._labelFilter.TabIndex = 9;
            this._labelFilter.Text = "Filter on locale";
            // 
            // _comboBoxFilter
            // 
            this._comboBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxFilter.Enabled = false;
            this._comboBoxFilter.FormattingEnabled = true;
            this._comboBoxFilter.Location = new System.Drawing.Point(245, 350);
            this._comboBoxFilter.Name = "_comboBoxFilter";
            this._comboBoxFilter.Size = new System.Drawing.Size(112, 23);
            this._comboBoxFilter.TabIndex = 2;
            this._comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.ComboBoxFilter_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 399);
            this.Controls.Add(this._comboBoxFilter);
            this.Controls.Add(this._labelFilter);
            this.Controls.Add(this._labelDirectory);
            this.Controls.Add(this._labelLoadingBooks);
            this.Controls.Add(this._labelStartupTip);
            this.Controls.Add(this._progressBarAction);
            this.Controls.Add(this._buttonBrowseDirectory);
            this.Controls.Add(this._textBoxDirectory);
            this.Controls.Add(this._treeViewBooks);
            this.Controls.Add(this._buttonDownloadBooks);
            this.Controls.Add(this._buttonLoadBooks);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(540, 430);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "%TITLE%";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _buttonLoadBooks;
        private System.Windows.Forms.TreeView _treeViewBooks;
        private System.Windows.Forms.TextBox _textBoxDirectory;
        private System.Windows.Forms.Button _buttonBrowseDirectory;
        private System.Windows.Forms.ProgressBar _progressBarAction;
        private System.Windows.Forms.Label _labelStartupTip;
        private System.Windows.Forms.Button _buttonDownloadBooks;
        private System.Windows.Forms.Label _labelDirectory;
        private System.ComponentModel.BackgroundWorker _backgroundWorkerLoadBooks;
        private System.ComponentModel.BackgroundWorker _backgroundWorkerDownloadBooks;
        private System.Windows.Forms.Label _labelLoadingBooks;
        private System.Windows.Forms.Label _labelFilter;
        private System.Windows.Forms.ComboBox _comboBoxFilter;
    }
}

