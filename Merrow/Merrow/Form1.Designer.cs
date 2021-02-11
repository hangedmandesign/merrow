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
            this.rndGroupBox = new System.Windows.Forms.GroupBox();
            this.rndTextContentDropdown = new System.Windows.Forms.ComboBox();
            this.rndTextContentToggle = new System.Windows.Forms.CheckBox();
            this.rndTextPaletteDropdown = new System.Windows.Forms.ComboBox();
            this.rndTextPaletteToggle = new System.Windows.Forms.CheckBox();
            this.rndChestDropdown = new System.Windows.Forms.ComboBox();
            this.rndChestToggle = new System.Windows.Forms.CheckBox();
            this.rndSpellDropdown = new System.Windows.Forms.ComboBox();
            this.rndSpellToggle = new System.Windows.Forms.CheckBox();
            this.quaGroupBox = new System.Windows.Forms.GroupBox();
            this.quaSoulToggle = new System.Windows.Forms.CheckBox();
            this.quaAccuracyDropdown = new System.Windows.Forms.ComboBox();
            this.quaAccuracyToggle = new System.Windows.Forms.CheckBox();
            this.quaZoomDropdown = new System.Windows.Forms.ComboBox();
            this.quaZoomToggle = new System.Windows.Forms.CheckBox();
            this.quaInvalidityToggle = new System.Windows.Forms.CheckBox();
            this.merrowName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.rndGroupBox.SuspendLayout();
            this.quaGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // rndGroupBox
            // 
            this.rndGroupBox.Controls.Add(this.rndTextContentDropdown);
            this.rndGroupBox.Controls.Add(this.rndTextContentToggle);
            this.rndGroupBox.Controls.Add(this.rndTextPaletteDropdown);
            this.rndGroupBox.Controls.Add(this.rndTextPaletteToggle);
            this.rndGroupBox.Controls.Add(this.rndChestDropdown);
            this.rndGroupBox.Controls.Add(this.rndChestToggle);
            this.rndGroupBox.Controls.Add(this.rndSpellDropdown);
            this.rndGroupBox.Controls.Add(this.rndSpellToggle);
            this.rndGroupBox.Location = new System.Drawing.Point(13, 89);
            this.rndGroupBox.Name = "rndGroupBox";
            this.rndGroupBox.Size = new System.Drawing.Size(323, 126);
            this.rndGroupBox.TabIndex = 0;
            this.rndGroupBox.TabStop = false;
            this.rndGroupBox.Text = "RANDOMIZATION";
            this.rndGroupBox.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // rndTextContentDropdown
            // 
            this.rndTextContentDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndTextContentDropdown.FormattingEnabled = true;
            this.rndTextContentDropdown.Items.AddRange(new object[] {
            "Shuffled",
            "Shortened"});
            this.rndTextContentDropdown.Location = new System.Drawing.Point(162, 98);
            this.rndTextContentDropdown.Name = "rndTextContentDropdown";
            this.rndTextContentDropdown.Size = new System.Drawing.Size(155, 21);
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
            "Colour1",
            "Colour2",
            "Colour3",
            "Shuffled (any of above)"});
            this.rndTextPaletteDropdown.Location = new System.Drawing.Point(162, 71);
            this.rndTextPaletteDropdown.Name = "rndTextPaletteDropdown";
            this.rndTextPaletteDropdown.Size = new System.Drawing.Size(155, 21);
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
            "Shuffled",
            "Random",
            "Random & Wings",
            "Random & Gems",
            "Chaos"});
            this.rndChestDropdown.Location = new System.Drawing.Point(162, 44);
            this.rndChestDropdown.Name = "rndChestDropdown";
            this.rndChestDropdown.Size = new System.Drawing.Size(155, 21);
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
            this.rndSpellDropdown.Location = new System.Drawing.Point(162, 17);
            this.rndSpellDropdown.Name = "rndSpellDropdown";
            this.rndSpellDropdown.Size = new System.Drawing.Size(155, 21);
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
            this.quaGroupBox.Controls.Add(this.quaSoulToggle);
            this.quaGroupBox.Controls.Add(this.quaAccuracyDropdown);
            this.quaGroupBox.Controls.Add(this.quaAccuracyToggle);
            this.quaGroupBox.Controls.Add(this.quaZoomDropdown);
            this.quaGroupBox.Controls.Add(this.quaZoomToggle);
            this.quaGroupBox.Controls.Add(this.quaInvalidityToggle);
            this.quaGroupBox.Location = new System.Drawing.Point(344, 89);
            this.quaGroupBox.Name = "quaGroupBox";
            this.quaGroupBox.Size = new System.Drawing.Size(217, 126);
            this.quaGroupBox.TabIndex = 1;
            this.quaGroupBox.TabStop = false;
            this.quaGroupBox.Text = "QUALITY";
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
            this.merrowName.Location = new System.Drawing.Point(13, 13);
            this.merrowName.Name = "merrowName";
            this.merrowName.Size = new System.Drawing.Size(277, 56);
            this.merrowName.TabIndex = 2;
            this.merrowName.Text = "MERROW";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Bold);
            this.labelVersion.Location = new System.Drawing.Point(277, 27);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(80, 38);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "v1.7";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // MerrowStandard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 302);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.merrowName);
            this.Controls.Add(this.quaGroupBox);
            this.Controls.Add(this.rndGroupBox);
            this.Name = "MerrowStandard";
            this.Text = "MERROW";
            this.rndGroupBox.ResumeLayout(false);
            this.rndGroupBox.PerformLayout();
            this.quaGroupBox.ResumeLayout(false);
            this.quaGroupBox.PerformLayout();
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
    }
}

