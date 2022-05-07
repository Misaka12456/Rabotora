namespace Rabotora.Launcher.SubForms
{
	partial class Frm_About
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Frm_About));
			this.tbx_About = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tbx_About
			// 
			this.tbx_About.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbx_About.Enabled = false;
			this.tbx_About.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.tbx_About.Location = new System.Drawing.Point(0, 0);
			this.tbx_About.Multiline = true;
			this.tbx_About.Name = "tbx_About";
			this.tbx_About.ReadOnly = true;
			this.tbx_About.Size = new System.Drawing.Size(434, 211);
			this.tbx_About.TabIndex = 0;
			this.tbx_About.Text = resources.GetString("tbx_About.Text");
			// 
			// Frm_About
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 211);
			this.Controls.Add(this.tbx_About);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(450, 250);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(450, 250);
			this.Name = "Frm_About";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About Rabotora Launcher";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Frm_About_FormClosing);
			this.Load += new System.EventHandler(this.Frm_About_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tbx_About;
	}
}