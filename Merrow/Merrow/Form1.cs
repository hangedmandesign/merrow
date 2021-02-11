using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Merrow {
    public partial class MerrowStandard : Form {
        public MerrowStandard() {
            InitializeComponent();
            PrepareDropdowns();
            
        }

        private void PrepareDropdowns() {
            rndSpellDropdown.SelectedIndex = 0;
            rndChestDropdown.SelectedIndex = 0;
            rndTextPaletteDropdown.SelectedIndex = 0;
            rndTextContentDropdown.SelectedIndex = 0;
            quaAccuracyDropdown.SelectedIndex = 0;
            quaZoomDropdown.SelectedIndex = 0;
            rndSpellDropdown.Visible = false;
            rndChestDropdown.Visible = false;
            rndTextPaletteDropdown.Visible = false;
            rndTextContentDropdown.Visible = false;
            quaAccuracyDropdown.Visible = false;
            quaZoomDropdown.Visible = false;
        }

        private void groupBox1_Enter(object sender, EventArgs e) {

        }

        private void rndSpellToggle_CheckedChanged(object sender, EventArgs e) {
            if(rndSpellToggle.Checked) { rndSpellDropdown.Visible = true; } else { rndSpellDropdown.Visible = false; }
        }

        private void rndChestToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndChestToggle.Checked) { rndChestDropdown.Visible = true; } else { rndChestDropdown.Visible = false; }
        }

        private void rndTextPaletteToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextPaletteToggle.Checked) { rndTextPaletteDropdown.Visible = true; } else { rndTextPaletteDropdown.Visible = false; }
        }

        private void rndTextContentToggle_CheckedChanged(object sender, EventArgs e) {
            if (rndTextContentToggle.Checked) { rndTextContentDropdown.Visible = true; } else { rndTextContentDropdown.Visible = false; }
        }

        private void rndSpellDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void rndChestDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void rndTextPaletteDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void rndTextContentDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void quaZoomDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void quaAccuracyDropdown_IndexChanged(object sender, EventArgs e) {

        }

        private void quaInvalidityToggle_CheckedChanged(object sender, EventArgs e) {

        }

        private void quaZoomToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaZoomToggle.Checked) { quaZoomDropdown.Visible = true; } else { quaZoomDropdown.Visible = false; }
        }

        private void quaAccuracyToggle_CheckedChanged(object sender, EventArgs e) {
            if (quaAccuracyToggle.Checked) { quaAccuracyDropdown.Visible = true; } else { quaAccuracyDropdown.Visible = false; }
        }

        private void quaSoulToggle_CheckedChanged(object sender, EventArgs e) {

        }
    }
}
