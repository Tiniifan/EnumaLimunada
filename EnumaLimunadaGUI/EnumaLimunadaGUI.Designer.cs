namespace EnumaLimunadaGUI
{
    partial class EnumaLimunadaGUI
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.selectedFileListBox = new System.Windows.Forms.ListBox();
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.selectedFolderTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.openButton = new System.Windows.Forms.Button();
            this.addIEGOFlagCheckBox = new System.Windows.Forms.CheckBox();
            this.runButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // selectedFileListBox
            // 
            this.selectedFileListBox.FormattingEnabled = true;
            this.selectedFileListBox.Location = new System.Drawing.Point(16, 41);
            this.selectedFileListBox.Name = "selectedFileListBox";
            this.selectedFileListBox.Size = new System.Drawing.Size(552, 368);
            this.selectedFileListBox.TabIndex = 0;
            this.selectedFileListBox.SelectedIndexChanged += new System.EventHandler(this.SelectedFileListBox_SelectedIndexChanged);
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(574, 12);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(102, 23);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Enabled = false;
            this.removeButton.Location = new System.Drawing.Point(574, 41);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(102, 23);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // selectedFolderTextBox
            // 
            this.selectedFolderTextBox.Location = new System.Drawing.Point(61, 13);
            this.selectedFolderTextBox.Name = "selectedFolderTextBox";
            this.selectedFolderTextBox.ReadOnly = true;
            this.selectedFolderTextBox.Size = new System.Drawing.Size(399, 20);
            this.selectedFolderTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Output";
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(466, 12);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(102, 23);
            this.openButton.TabIndex = 5;
            this.openButton.Text = "...";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // addIEGOFlagCheckBox
            // 
            this.addIEGOFlagCheckBox.AutoSize = true;
            this.addIEGOFlagCheckBox.Location = new System.Drawing.Point(574, 99);
            this.addIEGOFlagCheckBox.Name = "addIEGOFlagCheckBox";
            this.addIEGOFlagCheckBox.Size = new System.Drawing.Size(102, 17);
            this.addIEGOFlagCheckBox.TabIndex = 6;
            this.addIEGOFlagCheckBox.Text = "Add IEGO Flags";
            this.addIEGOFlagCheckBox.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Enabled = false;
            this.runButton.Location = new System.Drawing.Point(574, 70);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(102, 23);
            this.runButton.TabIndex = 8;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(16, 41);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(552, 368);
            this.logTextBox.TabIndex = 9;
            this.logTextBox.Visible = false;
            // 
            // EnumaLimunadaGUI
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 421);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.addIEGOFlagCheckBox);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectedFolderTextBox);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.selectedFileListBox);
            this.Name = "EnumaLimunadaGUI";
            this.Text = "EnumaLimunadaGUI";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.EnumaLimunadaGUI_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.EnumaLimunadaGUI_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox selectedFileListBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TextBox selectedFolderTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.CheckBox addIEGOFlagCheckBox;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox logTextBox;
    }
}

