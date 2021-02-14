namespace Merrow {
    partial class MerrowStandard {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MerrowStandard));
            this.rndGroupBox = new System.Windows.Forms.GroupBox();
            this.rndDropsDropdown = new System.Windows.Forms.ComboBox();
            this.rndDropsToggle = new System.Windows.Forms.CheckBox();
            this.rndTextContentDropdown = new System.Windows.Forms.ComboBox();
            this.rndTextContentToggle = new System.Windows.Forms.CheckBox();
            this.rndTextPaletteDropdown = new System.Windows.Forms.ComboBox();
            this.rndTextPaletteToggle = new System.Windows.Forms.CheckBox();
            this.rndChestDropdown = new System.Windows.Forms.ComboBox();
            this.rndChestToggle = new System.Windows.Forms.CheckBox();
            this.rndSpellDropdown = new System.Windows.Forms.ComboBox();
            this.rndSpellToggle = new System.Windows.Forms.CheckBox();
            this.quaGroupBox = new System.Windows.Forms.GroupBox();
            this.quaLevelToggle = new System.Windows.Forms.CheckBox();
            this.quaSoulToggle = new System.Windows.Forms.CheckBox();
            this.quaAccuracyDropdown = new System.Windows.Forms.ComboBox();
            this.quaAccuracyToggle = new System.Windows.Forms.CheckBox();
            this.quaZoomDropdown = new System.Windows.Forms.ComboBox();
            this.quaZoomToggle = new System.Windows.Forms.CheckBox();
            this.quaInvalidityToggle = new System.Windows.Forms.CheckBox();
            this.merrowName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.expGroupBox = new System.Windows.Forms.GroupBox();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.seedLabel = new System.Windows.Forms.Label();
            this.genButton = new System.Windows.Forms.Button();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.seedTextBox = new System.Windows.Forms.TextBox();
            this.creditLabel = new System.Windows.Forms.Label();
            this.rndGroupBox.SuspendLayout();
            this.quaGroupBox.SuspendLayout();
            this.expGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // rndGroupBox
            // 
            this.rndGroupBox.Controls.Add(this.rndDropsDropdown);
            this.rndGroupBox.Controls.Add(this.rndDropsToggle);
            this.rndGroupBox.Controls.Add(this.rndTextContentDropdown);
            this.rndGroupBox.Controls.Add(this.rndTextContentToggle);
            this.rndGroupBox.Controls.Add(this.rndTextPaletteDropdown);
            this.rndGroupBox.Controls.Add(this.rndTextPaletteToggle);
            this.rndGroupBox.Controls.Add(this.rndChestDropdown);
            this.rndGroupBox.Controls.Add(this.rndChestToggle);
            this.rndGroupBox.Controls.Add(this.rndSpellDropdown);
            this.rndGroupBox.Controls.Add(this.rndSpellToggle);
            this.rndGroupBox.Location = new System.Drawing.Point(13, 72);
            this.rndGroupBox.Name = "rndGroupBox";
            this.rndGroupBox.Size = new System.Drawing.Size(323, 187);
            this.rndGroupBox.TabIndex = 0;
            this.rndGroupBox.TabStop = false;
            this.rndGroupBox.Text = "RANDOMIZATION";
            // 
            // rndDropsDropdown
            // 
            this.rndDropsDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndDropsDropdown.FormattingEnabled = true;
            this.rndDropsDropdown.Items.AddRange(new object[] {
            "SHUFFLED",
            "RANDOM",
            "RANDOM + WINGS",
            "RANDOM + GEMS",
            "CHAOS"});
            this.rndDropsDropdown.Location = new System.Drawing.Point(160, 125);
            this.rndDropsDropdown.Name = "rndDropsDropdown";
            this.rndDropsDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndDropsDropdown.TabIndex = 13;
            // 
            // rndDropsToggle
            // 
            this.rndDropsToggle.AutoSize = true;
            this.rndDropsToggle.Location = new System.Drawing.Point(6, 127);
            this.rndDropsToggle.Name = "rndDropsToggle";
            this.rndDropsToggle.Size = new System.Drawing.Size(142, 17);
            this.rndDropsToggle.TabIndex = 12;
            this.rndDropsToggle.Text = "Randomize enemy drops";
            this.rndDropsToggle.UseVisualStyleBackColor = true;
            this.rndDropsToggle.CheckedChanged += new System.EventHandler(this.rndDropsToggle_CheckedChanged);
            // 
            // rndTextContentDropdown
            // 
            this.rndTextContentDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndTextContentDropdown.FormattingEnabled = true;
            this.rndTextContentDropdown.Items.AddRange(new object[] {
            "Shuffled"});
            this.rndTextContentDropdown.Location = new System.Drawing.Point(160, 98);
            this.rndTextContentDropdown.Name = "rndTextContentDropdown";
            this.rndTextContentDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndTextContentDropdown.TabIndex = 11;
            this.rndTextContentDropdown.SelectedIndexChanged += new System.EventHandler(this.rndTextContentDropdown_IndexChanged);
            // 
            // rndTextContentToggle
            // 
            this.rndTextContentToggle.AutoSize = true;
            this.rndTextContentToggle.Location = new System.Drawing.Point(6, 100);
            this.rndTextContentToggle.Name = "rndTextContentToggle";
            this.rndTextContentToggle.Size = new System.Drawing.Size(127, 17);
            this.rndTextContentToggle.TabIndex = 10;
            this.rndTextContentToggle.Text = "Randomize textboxes";
            this.rndTextContentToggle.UseVisualStyleBackColor = true;
            this.rndTextContentToggle.CheckedChanged += new System.EventHandler(this.rndTextContentToggle_CheckedChanged);
            // 
            // rndTextPaletteDropdown
            // 
            this.rndTextPaletteDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndTextPaletteDropdown.FormattingEnabled = true;
            this.rndTextPaletteDropdown.Items.AddRange(new object[] {
            "Black (default)",
            "Red",
            "Blue",
            "White",
            "SHUFFLED (any of above)"});
            this.rndTextPaletteDropdown.Location = new System.Drawing.Point(160, 71);
            this.rndTextPaletteDropdown.Name = "rndTextPaletteDropdown";
            this.rndTextPaletteDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndTextPaletteDropdown.TabIndex = 9;
            this.rndTextPaletteDropdown.SelectedIndexChanged += new System.EventHandler(this.rndTextPaletteDropdown_IndexChanged);
            // 
            // rndTextPaletteToggle
            // 
            this.rndTextPaletteToggle.AutoSize = true;
            this.rndTextPaletteToggle.Location = new System.Drawing.Point(6, 73);
            this.rndTextPaletteToggle.Name = "rndTextPaletteToggle";
            this.rndTextPaletteToggle.Size = new System.Drawing.Size(97, 17);
            this.rndTextPaletteToggle.TabIndex = 8;
            this.rndTextPaletteToggle.Text = "Set text palette";
            this.rndTextPaletteToggle.UseVisualStyleBackColor = true;
            this.rndTextPaletteToggle.CheckedChanged += new System.EventHandler(this.rndTextPaletteToggle_CheckedChanged);
            // 
            // rndChestDropdown
            // 
            this.rndChestDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndChestDropdown.FormattingEnabled = true;
            this.rndChestDropdown.Items.AddRange(new object[] {
            "SHUFFLED",
            "RANDOM",
            "RANDOM + WINGS",
            "RANDOM + GEMS",
            "CHAOS"});
            this.rndChestDropdown.Location = new System.Drawing.Point(160, 44);
            this.rndChestDropdown.Name = "rndChestDropdown";
            this.rndChestDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndChestDropdown.TabIndex = 7;
            this.rndChestDropdown.SelectedIndexChanged += new System.EventHandler(this.rndChestDropdown_IndexChanged);
            // 
            // rndChestToggle
            // 
            this.rndChestToggle.AutoSize = true;
            this.rndChestToggle.Location = new System.Drawing.Point(6, 46);
            this.rndChestToggle.Name = "rndChestToggle";
            this.rndChestToggle.Size = new System.Drawing.Size(152, 17);
            this.rndChestToggle.TabIndex = 5;
            this.rndChestToggle.Text = "Randomize chest contents";
            this.rndChestToggle.UseVisualStyleBackColor = true;
            this.rndChestToggle.CheckedChanged += new System.EventHandler(this.rndChestToggle_CheckedChanged);
            // 
            // rndSpellDropdown
            // 
            this.rndSpellDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndSpellDropdown.FormattingEnabled = true;
            this.rndSpellDropdown.Items.AddRange(new object[] {
            "Shuffled",
            "Item1",
            "Item2"});
            this.rndSpellDropdown.Location = new System.Drawing.Point(160, 17);
            this.rndSpellDropdown.Name = "rndSpellDropdown";
            this.rndSpellDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndSpellDropdown.TabIndex = 4;
            this.rndSpellDropdown.SelectedIndexChanged += new System.EventHandler(this.rndSpellDropdown_IndexChanged);
            // 
            // rndSpellToggle
            // 
            this.rndSpellToggle.AutoSize = true;
            this.rndSpellToggle.Cursor = System.Windows.Forms.Cursors.Default;
            this.rndSpellToggle.Location = new System.Drawing.Point(6, 19);
            this.rndSpellToggle.Name = "rndSpellToggle";
            this.rndSpellToggle.Size = new System.Drawing.Size(139, 17);
            this.rndSpellToggle.TabIndex = 1;
            this.rndSpellToggle.Text = "Override spell properties";
            this.rndSpellToggle.UseVisualStyleBackColor = true;
            this.rndSpellToggle.CheckedChanged += new System.EventHandler(this.rndSpellToggle_CheckedChanged);
            // 
            // quaGroupBox
            // 
            this.quaGroupBox.Controls.Add(this.quaLevelToggle);
            this.quaGroupBox.Controls.Add(this.quaSoulToggle);
            this.quaGroupBox.Controls.Add(this.quaAccuracyDropdown);
            this.quaGroupBox.Controls.Add(this.quaAccuracyToggle);
            this.quaGroupBox.Controls.Add(this.quaZoomDropdown);
            this.quaGroupBox.Controls.Add(this.quaZoomToggle);
            this.quaGroupBox.Controls.Add(this.quaInvalidityToggle);
            this.quaGroupBox.Location = new System.Drawing.Point(344, 72);
            this.quaGroupBox.Name = "quaGroupBox";
            this.quaGroupBox.Size = new System.Drawing.Size(217, 187);
            this.quaGroupBox.TabIndex = 1;
            this.quaGroupBox.TabStop = false;
            this.quaGroupBox.Text = "QUALITY";
            // 
            // quaLevelToggle
            // 
            this.quaLevelToggle.AutoSize = true;
            this.quaLevelToggle.Location = new System.Drawing.Point(6, 127);
            this.quaLevelToggle.Name = "quaLevelToggle";
            this.quaLevelToggle.Size = new System.Drawing.Size(163, 17);
            this.quaLevelToggle.TabIndex = 11;
            this.quaLevelToggle.Text = "All spells unlocked at Level 1";
            this.quaLevelToggle.UseVisualStyleBackColor = true;
            this.quaLevelToggle.CheckedChanged += new System.EventHandler(this.quaLevelToggle_CheckedChanged);
            // 
            // quaSoulToggle
            // 
            this.quaSoulToggle.AutoSize = true;
            this.quaSoulToggle.Location = new System.Drawing.Point(6, 100);
            this.quaSoulToggle.Name = "quaSoulToggle";
            this.quaSoulToggle.Size = new System.Drawing.Size(127, 17);
            this.quaSoulToggle.TabIndex = 10;
            this.quaSoulToggle.Text = "All spells Soul Search";
            this.quaSoulToggle.UseVisualStyleBackColor = true;
            this.quaSoulToggle.CheckedChanged += new System.EventHandler(this.quaSoulToggle_CheckedChanged);
            // 
            // quaAccuracyDropdown
            // 
            this.quaAccuracyDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.quaAccuracyDropdown.FormattingEnabled = true;
            this.quaAccuracyDropdown.Items.AddRange(new object[] {
            "Status spells",
            "All spells"});
            this.quaAccuracyDropdown.Location = new System.Drawing.Point(105, 71);
            this.quaAccuracyDropdown.Name = "quaAccuracyDropdown";
            this.quaAccuracyDropdown.Size = new System.Drawing.Size(102, 21);
            this.quaAccuracyDropdown.TabIndex = 9;
            this.quaAccuracyDropdown.SelectedIndexChanged += new System.EventHandler(this.quaAccuracyDropdown_IndexChanged);
            // 
            // quaAccuracyToggle
            // 
            this.quaAccuracyToggle.AutoSize = true;
            this.quaAccuracyToggle.Location = new System.Drawing.Point(6, 73);
            this.quaAccuracyToggle.Name = "quaAccuracyToggle";
            this.quaAccuracyToggle.Size = new System.Drawing.Size(93, 17);
            this.quaAccuracyToggle.TabIndex = 8;
            this.quaAccuracyToggle.Text = "Max accuracy";
            this.quaAccuracyToggle.UseVisualStyleBackColor = true;
            this.quaAccuracyToggle.CheckedChanged += new System.EventHandler(this.quaAccuracyToggle_CheckedChanged);
            // 
            // quaZoomDropdown
            // 
            this.quaZoomDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.quaZoomDropdown.FormattingEnabled = true;
            this.quaZoomDropdown.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.quaZoomDropdown.Location = new System.Drawing.Point(137, 44);
            this.quaZoomDropdown.MaxDropDownItems = 11;
            this.quaZoomDropdown.Name = "quaZoomDropdown";
            this.quaZoomDropdown.Size = new System.Drawing.Size(70, 21);
            this.quaZoomDropdown.TabIndex = 7;
            this.quaZoomDropdown.SelectedIndexChanged += new System.EventHandler(this.quaZoomDropdown_IndexChanged);
            // 
            // quaZoomToggle
            // 
            this.quaZoomToggle.AutoSize = true;
            this.quaZoomToggle.Location = new System.Drawing.Point(6, 46);
            this.quaZoomToggle.Name = "quaZoomToggle";
            this.quaZoomToggle.Size = new System.Drawing.Size(125, 17);
            this.quaZoomToggle.TabIndex = 5;
            this.quaZoomToggle.Text = "Increase zoom range";
            this.quaZoomToggle.UseVisualStyleBackColor = true;
            this.quaZoomToggle.CheckedChanged += new System.EventHandler(this.quaZoomToggle_CheckedChanged);
            // 
            // quaInvalidityToggle
            // 
            this.quaInvalidityToggle.AutoSize = true;
            this.quaInvalidityToggle.Cursor = System.Windows.Forms.Cursors.Default;
            this.quaInvalidityToggle.Location = new System.Drawing.Point(6, 19);
            this.quaInvalidityToggle.Name = "quaInvalidityToggle";
            this.quaInvalidityToggle.Size = new System.Drawing.Size(149, 17);
            this.quaInvalidityToggle.TabIndex = 1;
            this.quaInvalidityToggle.Text = "Remove Boss spell debuff";
            this.quaInvalidityToggle.UseVisualStyleBackColor = true;
            this.quaInvalidityToggle.CheckedChanged += new System.EventHandler(this.quaInvalidityToggle_CheckedChanged);
            // 
            // merrowName
            // 
            this.merrowName.AutoSize = true;
            this.merrowName.Font = new System.Drawing.Font("Georgia", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.merrowName.Location = new System.Drawing.Point(3, 9);
            this.merrowName.Name = "merrowName";
            this.merrowName.Size = new System.Drawing.Size(277, 56);
            this.merrowName.TabIndex = 2;
            this.merrowName.Text = "MERROW";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Bold);
            this.labelVersion.Location = new System.Drawing.Point(265, 23);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(73, 38);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "v18";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // expGroupBox
            // 
            this.expGroupBox.Controls.Add(this.filenameLabel);
            this.expGroupBox.Controls.Add(this.seedLabel);
            this.expGroupBox.Controls.Add(this.genButton);
            this.expGroupBox.Controls.Add(this.filenameTextBox);
            this.expGroupBox.Controls.Add(this.seedTextBox);
            this.expGroupBox.Location = new System.Drawing.Point(13, 265);
            this.expGroupBox.Name = "expGroupBox";
            this.expGroupBox.Size = new System.Drawing.Size(548, 55);
            this.expGroupBox.TabIndex = 4;
            this.expGroupBox.TabStop = false;
            this.expGroupBox.Text = "EXPORT";
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(207, 24);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(60, 13);
            this.filenameLabel.TabIndex = 13;
            this.filenameLabel.Text = "FILENAME";
            // 
            // seedLabel
            // 
            this.seedLabel.AutoSize = true;
            this.seedLabel.Location = new System.Drawing.Point(27, 24);
            this.seedLabel.Name = "seedLabel";
            this.seedLabel.Size = new System.Drawing.Size(36, 13);
            this.seedLabel.TabIndex = 12;
            this.seedLabel.Text = "SEED";
            // 
            // genButton
            // 
            this.genButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.genButton.Location = new System.Drawing.Point(436, 13);
            this.genButton.Name = "genButton";
            this.genButton.Size = new System.Drawing.Size(102, 35);
            this.genButton.TabIndex = 2;
            this.genButton.Text = "GENERATE";
            this.genButton.UseVisualStyleBackColor = true;
            this.genButton.Click += new System.EventHandler(this.genButton_Click);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(273, 21);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(130, 20);
            this.filenameTextBox.TabIndex = 1;
            this.filenameTextBox.TextChanged += new System.EventHandler(this.filenameTextBox_TextChanged);
            // 
            // seedTextBox
            // 
            this.seedTextBox.Location = new System.Drawing.Point(69, 21);
            this.seedTextBox.Name = "seedTextBox";
            this.seedTextBox.Size = new System.Drawing.Size(100, 20);
            this.seedTextBox.TabIndex = 0;
            this.seedTextBox.TextChanged += new System.EventHandler(this.seedTextBox_TextChanged);
            // 
            // creditLabel
            // 
            this.creditLabel.AutoSize = true;
            this.creditLabel.Location = new System.Drawing.Point(362, 9);
            this.creditLabel.Name = "creditLabel";
            this.creditLabel.Size = new System.Drawing.Size(199, 52);
            this.creditLabel.TabIndex = 5;
            this.creditLabel.Text = "Developed by Hangedman.\r\nGreetz: Landmine36, Irenepunmaster,\r\nBingchang, Jeville," +
    " Mallos31, Usedpizza,\r\nall at Rosemary and Rectangles.";
            this.creditLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MerrowStandard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 332);
            this.Controls.Add(this.creditLabel);
            this.Controls.Add(this.expGroupBox);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.merrowName);
            this.Controls.Add(this.quaGroupBox);
            this.Controls.Add(this.rndGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MerrowStandard";
            this.Text = "MERROW";
            this.rndGroupBox.ResumeLayout(false);
            this.rndGroupBox.PerformLayout();
            this.quaGroupBox.ResumeLayout(false);
            this.quaGroupBox.PerformLayout();
            this.expGroupBox.ResumeLayout(false);
            this.expGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox rndGroupBox;
        private System.Windows.Forms.CheckBox rndSpellToggle;
        private System.Windows.Forms.ComboBox rndSpellDropdown;
        private System.Windows.Forms.ComboBox rndChestDropdown;
        private System.Windows.Forms.CheckBox rndChestToggle;
        private System.Windows.Forms.ComboBox rndTextContentDropdown;
        private System.Windows.Forms.CheckBox rndTextContentToggle;
        private System.Windows.Forms.ComboBox rndTextPaletteDropdown;
        private System.Windows.Forms.CheckBox rndTextPaletteToggle;
        private System.Windows.Forms.GroupBox quaGroupBox;
        private System.Windows.Forms.CheckBox quaSoulToggle;
        private System.Windows.Forms.ComboBox quaAccuracyDropdown;
        private System.Windows.Forms.CheckBox quaAccuracyToggle;
        private System.Windows.Forms.ComboBox quaZoomDropdown;
        private System.Windows.Forms.CheckBox quaZoomToggle;
        private System.Windows.Forms.CheckBox quaInvalidityToggle;
        private System.Windows.Forms.Label merrowName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.CheckBox quaLevelToggle;
        private System.Windows.Forms.GroupBox expGroupBox;
        private System.Windows.Forms.Label creditLabel;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.Label seedLabel;
        private System.Windows.Forms.Button genButton;
        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.TextBox seedTextBox;
        private System.Windows.Forms.ComboBox rndDropsDropdown;
        private System.Windows.Forms.CheckBox rndDropsToggle;
    }
}

