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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MerrowStandard));
            this.rndGroupBox = new System.Windows.Forms.GroupBox();
            this.rndSpellNames = new System.Windows.Forms.CheckBox();
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
            this.labelVersion = new System.Windows.Forms.Label();
            this.expGroupBox = new System.Windows.Forms.GroupBox();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.seedLabel = new System.Windows.Forms.Label();
            this.genButton = new System.Windows.Forms.Button();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.seedTextBox = new System.Windows.Forms.TextBox();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.tabsControl = new System.Windows.Forms.TabControl();
            this.CreditsTab = new System.Windows.Forms.TabPage();
            this.creditsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.RandomizerTab = new System.Windows.Forms.TabPage();
            this.crcWarningLabel = new System.Windows.Forms.Label();
            this.CustomTab = new System.Windows.Forms.TabPage();
            this.ReaderTab = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.rndToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.rndWeightedChest = new System.Windows.Forms.CheckBox();
            this.quaRestlessToggle = new System.Windows.Forms.CheckBox();
            this.verboseCheckBox = new System.Windows.Forms.CheckBox();
            this.advFilenameLabel = new System.Windows.Forms.Label();
            this.advGenerateButton = new System.Windows.Forms.Button();
            this.advFilenameText = new System.Windows.Forms.TextBox();
            this.advAddressLabel = new System.Windows.Forms.Label();
            this.advAddressText = new System.Windows.Forms.TextBox();
            this.advContentLabel = new System.Windows.Forms.Label();
            this.advContentText = new System.Windows.Forms.TextBox();
            this.advHelpLabel = new System.Windows.Forms.Label();
            this.rndGroupBox.SuspendLayout();
            this.quaGroupBox.SuspendLayout();
            this.expGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            this.tabsControl.SuspendLayout();
            this.CreditsTab.SuspendLayout();
            this.RandomizerTab.SuspendLayout();
            this.CustomTab.SuspendLayout();
            this.ReaderTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // rndGroupBox
            // 
            this.rndGroupBox.Controls.Add(this.rndWeightedChest);
            this.rndGroupBox.Controls.Add(this.rndSpellNames);
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
            this.rndGroupBox.Location = new System.Drawing.Point(8, 6);
            this.rndGroupBox.Name = "rndGroupBox";
            this.rndGroupBox.Size = new System.Drawing.Size(323, 314);
            this.rndGroupBox.TabIndex = 0;
            this.rndGroupBox.TabStop = false;
            this.rndGroupBox.Text = "RANDOMIZATION";
            // 
            // rndSpellNames
            // 
            this.rndSpellNames.AutoSize = true;
            this.rndSpellNames.Checked = true;
            this.rndSpellNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rndSpellNames.Cursor = System.Windows.Forms.Cursors.Default;
            this.rndSpellNames.Location = new System.Drawing.Point(160, 50);
            this.rndSpellNames.Name = "rndSpellNames";
            this.rndSpellNames.Size = new System.Drawing.Size(103, 17);
            this.rndSpellNames.TabIndex = 14;
            this.rndSpellNames.Text = "Hint spell names";
            this.rndToolTip.SetToolTip(this.rndSpellNames, "Spell names are updated to be a randomized prefix/suffix combination based on the" +
        " original and modifier spell.");
            this.rndSpellNames.UseVisualStyleBackColor = true;
            this.rndSpellNames.Visible = false;
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
            this.rndDropsDropdown.Location = new System.Drawing.Point(160, 168);
            this.rndDropsDropdown.Name = "rndDropsDropdown";
            this.rndDropsDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndDropsDropdown.TabIndex = 13;
            this.rndDropsDropdown.SelectedIndexChanged += new System.EventHandler(this.rndDropsDropdown_SelectedIndexChanged);
            // 
            // rndDropsToggle
            // 
            this.rndDropsToggle.AutoSize = true;
            this.rndDropsToggle.Location = new System.Drawing.Point(6, 170);
            this.rndDropsToggle.Name = "rndDropsToggle";
            this.rndDropsToggle.Size = new System.Drawing.Size(142, 17);
            this.rndDropsToggle.TabIndex = 12;
            this.rndDropsToggle.Text = "Randomize enemy drops";
            this.rndToolTip.SetToolTip(this.rndDropsToggle, resources.GetString("rndDropsToggle.ToolTip"));
            this.rndDropsToggle.UseVisualStyleBackColor = true;
            this.rndDropsToggle.CheckedChanged += new System.EventHandler(this.rndDropsToggle_CheckedChanged);
            // 
            // rndTextContentDropdown
            // 
            this.rndTextContentDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rndTextContentDropdown.FormattingEnabled = true;
            this.rndTextContentDropdown.Items.AddRange(new object[] {
            "Shuffled"});
            this.rndTextContentDropdown.Location = new System.Drawing.Point(160, 138);
            this.rndTextContentDropdown.Name = "rndTextContentDropdown";
            this.rndTextContentDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndTextContentDropdown.TabIndex = 11;
            this.rndTextContentDropdown.SelectedIndexChanged += new System.EventHandler(this.rndTextContentDropdown_IndexChanged);
            // 
            // rndTextContentToggle
            // 
            this.rndTextContentToggle.AutoSize = true;
            this.rndTextContentToggle.Location = new System.Drawing.Point(6, 140);
            this.rndTextContentToggle.Name = "rndTextContentToggle";
            this.rndTextContentToggle.Size = new System.Drawing.Size(127, 17);
            this.rndTextContentToggle.TabIndex = 10;
            this.rndTextContentToggle.Text = "Randomize textboxes";
            this.rndToolTip.SetToolTip(this.rndTextContentToggle, "Redistribute all textbox content.\r\nSHUFFLED: All non-inn textboxes are shuffled t" +
        "ogether, and all inn textboxes are shuffled together.");
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
            this.rndTextPaletteDropdown.Location = new System.Drawing.Point(160, 198);
            this.rndTextPaletteDropdown.Name = "rndTextPaletteDropdown";
            this.rndTextPaletteDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndTextPaletteDropdown.TabIndex = 9;
            this.rndTextPaletteDropdown.SelectedIndexChanged += new System.EventHandler(this.rndTextPaletteDropdown_IndexChanged);
            // 
            // rndTextPaletteToggle
            // 
            this.rndTextPaletteToggle.AutoSize = true;
            this.rndTextPaletteToggle.Location = new System.Drawing.Point(6, 200);
            this.rndTextPaletteToggle.Name = "rndTextPaletteToggle";
            this.rndTextPaletteToggle.Size = new System.Drawing.Size(97, 17);
            this.rndTextPaletteToggle.TabIndex = 8;
            this.rndTextPaletteToggle.Text = "Set text palette";
            this.rndToolTip.SetToolTip(this.rndTextPaletteToggle, "Changes the text colour palette.\r\n<COLOUR>: Use one of four default included colo" +
        "urs.\r\nSHUFFLED: Select one of those four included colours at random.");
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
            this.rndChestDropdown.Location = new System.Drawing.Point(160, 78);
            this.rndChestDropdown.Name = "rndChestDropdown";
            this.rndChestDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndChestDropdown.TabIndex = 7;
            this.rndChestDropdown.SelectedIndexChanged += new System.EventHandler(this.rndChestDropdown_IndexChanged);
            // 
            // rndChestToggle
            // 
            this.rndChestToggle.AutoSize = true;
            this.rndChestToggle.Location = new System.Drawing.Point(6, 80);
            this.rndChestToggle.Name = "rndChestToggle";
            this.rndChestToggle.Size = new System.Drawing.Size(152, 17);
            this.rndChestToggle.TabIndex = 5;
            this.rndChestToggle.Text = "Randomize chest contents";
            this.rndToolTip.SetToolTip(this.rndChestToggle, resources.GetString("rndChestToggle.ToolTip"));
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
            this.rndSpellDropdown.Location = new System.Drawing.Point(160, 18);
            this.rndSpellDropdown.Name = "rndSpellDropdown";
            this.rndSpellDropdown.Size = new System.Drawing.Size(157, 21);
            this.rndSpellDropdown.TabIndex = 4;
            this.rndSpellDropdown.SelectedIndexChanged += new System.EventHandler(this.rndSpellDropdown_IndexChanged);
            // 
            // rndSpellToggle
            // 
            this.rndSpellToggle.AutoSize = true;
            this.rndSpellToggle.Cursor = System.Windows.Forms.Cursors.Default;
            this.rndSpellToggle.Location = new System.Drawing.Point(6, 20);
            this.rndSpellToggle.Name = "rndSpellToggle";
            this.rndSpellToggle.Size = new System.Drawing.Size(139, 17);
            this.rndSpellToggle.TabIndex = 1;
            this.rndSpellToggle.Text = "Override spell properties";
            this.rndToolTip.SetToolTip(this.rndSpellToggle, resources.GetString("rndSpellToggle.ToolTip"));
            this.rndSpellToggle.UseVisualStyleBackColor = true;
            this.rndSpellToggle.CheckedChanged += new System.EventHandler(this.rndSpellToggle_CheckedChanged);
            // 
            // quaGroupBox
            // 
            this.quaGroupBox.Controls.Add(this.quaRestlessToggle);
            this.quaGroupBox.Controls.Add(this.quaLevelToggle);
            this.quaGroupBox.Controls.Add(this.quaSoulToggle);
            this.quaGroupBox.Controls.Add(this.quaAccuracyDropdown);
            this.quaGroupBox.Controls.Add(this.quaAccuracyToggle);
            this.quaGroupBox.Controls.Add(this.quaZoomDropdown);
            this.quaGroupBox.Controls.Add(this.quaZoomToggle);
            this.quaGroupBox.Controls.Add(this.quaInvalidityToggle);
            this.quaGroupBox.Location = new System.Drawing.Point(339, 6);
            this.quaGroupBox.Name = "quaGroupBox";
            this.quaGroupBox.Size = new System.Drawing.Size(217, 314);
            this.quaGroupBox.TabIndex = 1;
            this.quaGroupBox.TabStop = false;
            this.quaGroupBox.Text = "QUALITY / FUN";
            // 
            // quaLevelToggle
            // 
            this.quaLevelToggle.AutoSize = true;
            this.quaLevelToggle.Location = new System.Drawing.Point(6, 140);
            this.quaLevelToggle.Name = "quaLevelToggle";
            this.quaLevelToggle.Size = new System.Drawing.Size(163, 17);
            this.quaLevelToggle.TabIndex = 11;
            this.quaLevelToggle.Text = "All spells unlocked at Level 1";
            this.rndToolTip.SetToolTip(this.quaLevelToggle, "Set all spell unlock levels to 1. ");
            this.quaLevelToggle.UseVisualStyleBackColor = true;
            this.quaLevelToggle.CheckedChanged += new System.EventHandler(this.quaLevelToggle_CheckedChanged);
            // 
            // quaSoulToggle
            // 
            this.quaSoulToggle.AutoSize = true;
            this.quaSoulToggle.Location = new System.Drawing.Point(6, 110);
            this.quaSoulToggle.Name = "quaSoulToggle";
            this.quaSoulToggle.Size = new System.Drawing.Size(127, 17);
            this.quaSoulToggle.TabIndex = 10;
            this.quaSoulToggle.Text = "All spells Soul Search";
            this.rndToolTip.SetToolTip(this.quaSoulToggle, resources.GetString("quaSoulToggle.ToolTip"));
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
            this.quaAccuracyDropdown.Location = new System.Drawing.Point(105, 78);
            this.quaAccuracyDropdown.Name = "quaAccuracyDropdown";
            this.quaAccuracyDropdown.Size = new System.Drawing.Size(102, 21);
            this.quaAccuracyDropdown.TabIndex = 9;
            this.quaAccuracyDropdown.SelectedIndexChanged += new System.EventHandler(this.quaAccuracyDropdown_IndexChanged);
            // 
            // quaAccuracyToggle
            // 
            this.quaAccuracyToggle.AutoSize = true;
            this.quaAccuracyToggle.Location = new System.Drawing.Point(6, 80);
            this.quaAccuracyToggle.Name = "quaAccuracyToggle";
            this.quaAccuracyToggle.Size = new System.Drawing.Size(93, 17);
            this.quaAccuracyToggle.TabIndex = 8;
            this.quaAccuracyToggle.Text = "Max accuracy";
            this.rndToolTip.SetToolTip(this.quaAccuracyToggle, resources.GetString("quaAccuracyToggle.ToolTip"));
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
            this.quaZoomDropdown.Location = new System.Drawing.Point(137, 48);
            this.quaZoomDropdown.MaxDropDownItems = 11;
            this.quaZoomDropdown.Name = "quaZoomDropdown";
            this.quaZoomDropdown.Size = new System.Drawing.Size(70, 21);
            this.quaZoomDropdown.TabIndex = 7;
            this.quaZoomDropdown.SelectedIndexChanged += new System.EventHandler(this.quaZoomDropdown_IndexChanged);
            // 
            // quaZoomToggle
            // 
            this.quaZoomToggle.AutoSize = true;
            this.quaZoomToggle.Location = new System.Drawing.Point(6, 50);
            this.quaZoomToggle.Name = "quaZoomToggle";
            this.quaZoomToggle.Size = new System.Drawing.Size(125, 17);
            this.quaZoomToggle.TabIndex = 5;
            this.quaZoomToggle.Text = "Increase zoom range";
            this.rndToolTip.SetToolTip(this.quaZoomToggle, "Changes maximum zoom distance (2-12). 1 is default.\r\nValue is not a multiple, jus" +
        "t a range of stable values available. \r\n[WARNING - Will cause checksum error loo" +
        "p on any PJ64 derivative (e.g. RAP64).]");
            this.quaZoomToggle.UseVisualStyleBackColor = true;
            this.quaZoomToggle.CheckedChanged += new System.EventHandler(this.quaZoomToggle_CheckedChanged);
            // 
            // quaInvalidityToggle
            // 
            this.quaInvalidityToggle.AutoSize = true;
            this.quaInvalidityToggle.Cursor = System.Windows.Forms.Cursors.Default;
            this.quaInvalidityToggle.Location = new System.Drawing.Point(6, 20);
            this.quaInvalidityToggle.Name = "quaInvalidityToggle";
            this.quaInvalidityToggle.Size = new System.Drawing.Size(149, 17);
            this.quaInvalidityToggle.TabIndex = 1;
            this.quaInvalidityToggle.Text = "Remove Boss spell debuff";
            this.rndToolTip.SetToolTip(this.quaInvalidityToggle, "Boss spells no longer debuff the player with passive Invalidity effect. \r\nInvalid" +
        "ity removes buffs/debuffs, so default boss spells make them mostly useless.");
            this.quaInvalidityToggle.UseVisualStyleBackColor = true;
            this.quaInvalidityToggle.CheckedChanged += new System.EventHandler(this.quaInvalidityToggle_CheckedChanged);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(459, 68);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(73, 45);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "v20";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // expGroupBox
            // 
            this.expGroupBox.Controls.Add(this.verboseCheckBox);
            this.expGroupBox.Controls.Add(this.filenameLabel);
            this.expGroupBox.Controls.Add(this.seedLabel);
            this.expGroupBox.Controls.Add(this.genButton);
            this.expGroupBox.Controls.Add(this.filenameTextBox);
            this.expGroupBox.Controls.Add(this.seedTextBox);
            this.expGroupBox.Location = new System.Drawing.Point(8, 357);
            this.expGroupBox.Name = "expGroupBox";
            this.expGroupBox.Size = new System.Drawing.Size(548, 55);
            this.expGroupBox.TabIndex = 4;
            this.expGroupBox.TabStop = false;
            this.expGroupBox.Text = "EXPORT";
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.filenameLabel.Location = new System.Drawing.Point(153, 25);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(62, 13);
            this.filenameLabel.TabIndex = 13;
            this.filenameLabel.Text = "FILENAME:";
            this.rndToolTip.SetToolTip(this.filenameLabel, resources.GetString("filenameLabel.ToolTip"));
            // 
            // seedLabel
            // 
            this.seedLabel.AutoSize = true;
            this.seedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.seedLabel.Location = new System.Drawing.Point(6, 25);
            this.seedLabel.Name = "seedLabel";
            this.seedLabel.Size = new System.Drawing.Size(39, 13);
            this.seedLabel.TabIndex = 12;
            this.seedLabel.Text = "SEED:";
            this.rndToolTip.SetToolTip(this.seedLabel, "Auto-generated pseudorandom seed. \r\nDefaults to a random 9-digit number but can b" +
        "e edited freely.");
            // 
            // genButton
            // 
            this.genButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.genButton.Location = new System.Drawing.Point(437, 14);
            this.genButton.Name = "genButton";
            this.genButton.Size = new System.Drawing.Size(102, 35);
            this.genButton.TabIndex = 2;
            this.genButton.Text = "GENERATE";
            this.genButton.UseVisualStyleBackColor = true;
            this.genButton.Click += new System.EventHandler(this.genButton_Click);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(218, 22);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(130, 20);
            this.filenameTextBox.TabIndex = 1;
            this.filenameTextBox.TextChanged += new System.EventHandler(this.filenameTextBox_TextChanged);
            // 
            // seedTextBox
            // 
            this.seedTextBox.Location = new System.Drawing.Point(45, 21);
            this.seedTextBox.MaxLength = 9;
            this.seedTextBox.Name = "seedTextBox";
            this.seedTextBox.Size = new System.Drawing.Size(100, 20);
            this.seedTextBox.TabIndex = 0;
            this.seedTextBox.TextChanged += new System.EventHandler(this.seedTextBox_TextChanged);
            // 
            // logoBox
            // 
            this.logoBox.Image = global::Merrow.Properties.Resources.merrowbar;
            this.logoBox.InitialImage = global::Merrow.Properties.Resources.merrowbar;
            this.logoBox.Location = new System.Drawing.Point(41, 16);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(412, 154);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoBox.TabIndex = 6;
            this.logoBox.TabStop = false;
            // 
            // tabsControl
            // 
            this.tabsControl.Controls.Add(this.CreditsTab);
            this.tabsControl.Controls.Add(this.RandomizerTab);
            this.tabsControl.Controls.Add(this.CustomTab);
            this.tabsControl.Controls.Add(this.ReaderTab);
            this.tabsControl.Location = new System.Drawing.Point(12, 12);
            this.tabsControl.Name = "tabsControl";
            this.tabsControl.SelectedIndex = 0;
            this.tabsControl.Size = new System.Drawing.Size(573, 444);
            this.tabsControl.TabIndex = 7;
            // 
            // CreditsTab
            // 
            this.CreditsTab.BackColor = System.Drawing.Color.Transparent;
            this.CreditsTab.Controls.Add(this.creditsLinkLabel);
            this.CreditsTab.Controls.Add(this.logoBox);
            this.CreditsTab.Controls.Add(this.labelVersion);
            this.CreditsTab.Location = new System.Drawing.Point(4, 22);
            this.CreditsTab.Name = "CreditsTab";
            this.CreditsTab.Padding = new System.Windows.Forms.Padding(3);
            this.CreditsTab.Size = new System.Drawing.Size(565, 418);
            this.CreditsTab.TabIndex = 0;
            this.CreditsTab.Text = "Credits";
            // 
            // creditsLinkLabel
            // 
            this.creditsLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.creditsLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(411, 41);
            this.creditsLinkLabel.Location = new System.Drawing.Point(6, 173);
            this.creditsLinkLabel.Name = "creditsLinkLabel";
            this.creditsLinkLabel.Size = new System.Drawing.Size(553, 239);
            this.creditsLinkLabel.TabIndex = 7;
            this.creditsLinkLabel.TabStop = true;
            this.creditsLinkLabel.Text = resources.GetString("creditsLinkLabel.Text");
            this.creditsLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.creditsLinkLabel.UseCompatibleTextRendering = true;
            this.creditsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // RandomizerTab
            // 
            this.RandomizerTab.BackColor = System.Drawing.Color.Transparent;
            this.RandomizerTab.Controls.Add(this.crcWarningLabel);
            this.RandomizerTab.Controls.Add(this.rndGroupBox);
            this.RandomizerTab.Controls.Add(this.quaGroupBox);
            this.RandomizerTab.Controls.Add(this.expGroupBox);
            this.RandomizerTab.Location = new System.Drawing.Point(4, 22);
            this.RandomizerTab.Name = "RandomizerTab";
            this.RandomizerTab.Padding = new System.Windows.Forms.Padding(3);
            this.RandomizerTab.Size = new System.Drawing.Size(565, 418);
            this.RandomizerTab.TabIndex = 1;
            this.RandomizerTab.Text = "Quest 64 Randomizer";
            // 
            // crcWarningLabel
            // 
            this.crcWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.crcWarningLabel.ForeColor = System.Drawing.Color.Firebrick;
            this.crcWarningLabel.Location = new System.Drawing.Point(6, 323);
            this.crcWarningLabel.Name = "crcWarningLabel";
            this.crcWarningLabel.Size = new System.Drawing.Size(550, 31);
            this.crcWarningLabel.TabIndex = 5;
            this.crcWarningLabel.Text = "WARNING: Checksum error. Patched file will not work in PJ64 derivatives, others m" +
    "ay see error messages.";
            this.crcWarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CustomTab
            // 
            this.CustomTab.BackColor = System.Drawing.Color.Transparent;
            this.CustomTab.Controls.Add(this.advHelpLabel);
            this.CustomTab.Controls.Add(this.advFilenameLabel);
            this.CustomTab.Controls.Add(this.advContentLabel);
            this.CustomTab.Controls.Add(this.advGenerateButton);
            this.CustomTab.Controls.Add(this.advFilenameText);
            this.CustomTab.Controls.Add(this.advContentText);
            this.CustomTab.Controls.Add(this.advAddressLabel);
            this.CustomTab.Controls.Add(this.advAddressText);
            this.CustomTab.Location = new System.Drawing.Point(4, 22);
            this.CustomTab.Name = "CustomTab";
            this.CustomTab.Size = new System.Drawing.Size(565, 418);
            this.CustomTab.TabIndex = 2;
            this.CustomTab.Text = "Generic Patch Generator";
            // 
            // ReaderTab
            // 
            this.ReaderTab.BackColor = System.Drawing.Color.Transparent;
            this.ReaderTab.Controls.Add(this.label2);
            this.ReaderTab.Location = new System.Drawing.Point(4, 22);
            this.ReaderTab.Name = "ReaderTab";
            this.ReaderTab.Size = new System.Drawing.Size(565, 418);
            this.ReaderTab.TabIndex = 3;
            this.ReaderTab.Text = "Binary File Reader";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(559, 418);
            this.label2.TabIndex = 1;
            this.label2.Text = "Currently incomplete.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rndToolTip
            // 
            this.rndToolTip.AutomaticDelay = 400;
            // 
            // rndWeightedChest
            // 
            this.rndWeightedChest.AutoSize = true;
            this.rndWeightedChest.Checked = true;
            this.rndWeightedChest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rndWeightedChest.Cursor = System.Windows.Forms.Cursors.Default;
            this.rndWeightedChest.Location = new System.Drawing.Point(160, 110);
            this.rndWeightedChest.Name = "rndWeightedChest";
            this.rndWeightedChest.Size = new System.Drawing.Size(116, 17);
            this.rndWeightedChest.TabIndex = 15;
            this.rndWeightedChest.Text = "Weighted contents";
            this.rndToolTip.SetToolTip(this.rndWeightedChest, "Chest contents are guaranteed to contain at least one of each item in the selecte" +
        "d category.");
            this.rndWeightedChest.UseVisualStyleBackColor = true;
            this.rndWeightedChest.Visible = false;
            // 
            // quaRestlessToggle
            // 
            this.quaRestlessToggle.AutoSize = true;
            this.quaRestlessToggle.Location = new System.Drawing.Point(6, 170);
            this.quaRestlessToggle.Name = "quaRestlessToggle";
            this.quaRestlessToggle.Size = new System.Drawing.Size(96, 17);
            this.quaRestlessToggle.TabIndex = 12;
            this.quaRestlessToggle.Text = "Restless NPCs";
            this.rndToolTip.SetToolTip(this.quaRestlessToggle, "All NPCs will wander.\r\n[WARNING: May cause unknown issues.]\r\n");
            this.quaRestlessToggle.UseVisualStyleBackColor = true;
            // 
            // verboseCheckBox
            // 
            this.verboseCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.verboseCheckBox.Checked = true;
            this.verboseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.verboseCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.verboseCheckBox.Location = new System.Drawing.Point(360, 16);
            this.verboseCheckBox.Name = "verboseCheckBox";
            this.verboseCheckBox.Size = new System.Drawing.Size(71, 34);
            this.verboseCheckBox.TabIndex = 13;
            this.verboseCheckBox.Text = "Verbose Log";
            this.rndToolTip.SetToolTip(this.verboseCheckBox, "If enabled, spoiler log will contain a complete listing of modified spells, chest" +
        "s, and/or drops.");
            this.verboseCheckBox.UseVisualStyleBackColor = true;
            this.verboseCheckBox.CheckedChanged += new System.EventHandler(this.verboseCheckBox_CheckedChanged);
            // 
            // advFilenameLabel
            // 
            this.advFilenameLabel.AutoSize = true;
            this.advFilenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.advFilenameLabel.Location = new System.Drawing.Point(220, 379);
            this.advFilenameLabel.Name = "advFilenameLabel";
            this.advFilenameLabel.Size = new System.Drawing.Size(63, 13);
            this.advFilenameLabel.TabIndex = 13;
            this.advFilenameLabel.Text = "FILENAME:";
            this.rndToolTip.SetToolTip(this.advFilenameLabel, "IPS patch filename. Default is \'merrowgenericpatch.ips\' if nothing is typed.\r\nExp" +
        "ort will automatically overwrite any existing file with the same name, and will " +
        "open a window to the save location.");
            // 
            // advGenerateButton
            // 
            this.advGenerateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.advGenerateButton.Location = new System.Drawing.Point(446, 368);
            this.advGenerateButton.Name = "advGenerateButton";
            this.advGenerateButton.Size = new System.Drawing.Size(102, 35);
            this.advGenerateButton.TabIndex = 2;
            this.advGenerateButton.Text = "GENERATE";
            this.advGenerateButton.UseVisualStyleBackColor = true;
            this.advGenerateButton.Click += new System.EventHandler(this.advGenerateButton_Click);
            // 
            // advFilenameText
            // 
            this.advFilenameText.Location = new System.Drawing.Point(285, 376);
            this.advFilenameText.MaxLength = 48;
            this.advFilenameText.Name = "advFilenameText";
            this.advFilenameText.Size = new System.Drawing.Size(130, 20);
            this.advFilenameText.TabIndex = 1;
            this.advFilenameText.TextChanged += new System.EventHandler(this.advFilenameText_TextChanged);
            // 
            // advAddressLabel
            // 
            this.advAddressLabel.AutoSize = true;
            this.advAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.advAddressLabel.Location = new System.Drawing.Point(10, 379);
            this.advAddressLabel.Name = "advAddressLabel";
            this.advAddressLabel.Size = new System.Drawing.Size(62, 13);
            this.advAddressLabel.TabIndex = 15;
            this.advAddressLabel.Text = "ADDRESS:";
            this.rndToolTip.SetToolTip(this.advAddressLabel, "Address to write above content to in patch.\r\nMust be hexadecimal, 6 characters.");
            // 
            // advAddressText
            // 
            this.advAddressText.Location = new System.Drawing.Point(75, 376);
            this.advAddressText.MaxLength = 6;
            this.advAddressText.Name = "advAddressText";
            this.advAddressText.Size = new System.Drawing.Size(111, 20);
            this.advAddressText.TabIndex = 14;
            this.advAddressText.TextChanged += new System.EventHandler(this.advAddressText_TextChanged);
            // 
            // advContentLabel
            // 
            this.advContentLabel.AutoSize = true;
            this.advContentLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.advContentLabel.Location = new System.Drawing.Point(10, 14);
            this.advContentLabel.Name = "advContentLabel";
            this.advContentLabel.Size = new System.Drawing.Size(101, 13);
            this.advContentLabel.TabIndex = 17;
            this.advContentLabel.Text = "PATCH CONTENT:";
            this.rndToolTip.SetToolTip(this.advContentLabel, "Patch content to be written to the address supplied below.\r\nMust be hexadecimal. " +
        "Must be an even number of characters (to assemble hex bytes).");
            // 
            // advContentText
            // 
            this.advContentText.Location = new System.Drawing.Point(13, 36);
            this.advContentText.MaxLength = 32768;
            this.advContentText.Multiline = true;
            this.advContentText.Name = "advContentText";
            this.advContentText.Size = new System.Drawing.Size(535, 318);
            this.advContentText.TabIndex = 16;
            this.advContentText.TextChanged += new System.EventHandler(this.advContentText_TextChanged);
            // 
            // advHelpLabel
            // 
            this.advHelpLabel.AutoSize = true;
            this.advHelpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.advHelpLabel.Location = new System.Drawing.Point(537, 5);
            this.advHelpLabel.Name = "advHelpLabel";
            this.advHelpLabel.Size = new System.Drawing.Size(25, 26);
            this.advHelpLabel.TabIndex = 18;
            this.advHelpLabel.Text = "?";
            this.rndToolTip.SetToolTip(this.advHelpLabel, "Explanation to be written.");
            // 
            // MerrowStandard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(594, 467);
            this.Controls.Add(this.tabsControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MerrowStandard";
            this.Text = "MERROW";
            this.Load += new System.EventHandler(this.MerrowForm_Load);
            this.rndGroupBox.ResumeLayout(false);
            this.rndGroupBox.PerformLayout();
            this.quaGroupBox.ResumeLayout(false);
            this.quaGroupBox.PerformLayout();
            this.expGroupBox.ResumeLayout(false);
            this.expGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            this.tabsControl.ResumeLayout(false);
            this.CreditsTab.ResumeLayout(false);
            this.CreditsTab.PerformLayout();
            this.RandomizerTab.ResumeLayout(false);
            this.CustomTab.ResumeLayout(false);
            this.CustomTab.PerformLayout();
            this.ReaderTab.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.CheckBox quaLevelToggle;
        private System.Windows.Forms.GroupBox expGroupBox;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.Label seedLabel;
        private System.Windows.Forms.Button genButton;
        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.TextBox seedTextBox;
        private System.Windows.Forms.ComboBox rndDropsDropdown;
        private System.Windows.Forms.CheckBox rndDropsToggle;
        private System.Windows.Forms.CheckBox rndSpellNames;
        private System.Windows.Forms.PictureBox logoBox;
        private System.Windows.Forms.TabControl tabsControl;
        private System.Windows.Forms.TabPage CreditsTab;
        private System.Windows.Forms.TabPage RandomizerTab;
        private System.Windows.Forms.TabPage CustomTab;
        private System.Windows.Forms.TabPage ReaderTab;
        private System.Windows.Forms.LinkLabel creditsLinkLabel;
        private System.Windows.Forms.Label crcWarningLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolTip rndToolTip;
        private System.Windows.Forms.CheckBox rndWeightedChest;
        private System.Windows.Forms.CheckBox quaRestlessToggle;
        private System.Windows.Forms.CheckBox verboseCheckBox;
        private System.Windows.Forms.Label advFilenameLabel;
        private System.Windows.Forms.Button advGenerateButton;
        private System.Windows.Forms.TextBox advFilenameText;
        private System.Windows.Forms.Label advContentLabel;
        private System.Windows.Forms.TextBox advContentText;
        private System.Windows.Forms.Label advAddressLabel;
        private System.Windows.Forms.TextBox advAddressText;
        private System.Windows.Forms.Label advHelpLabel;
    }
}

