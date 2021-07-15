
using System;

namespace Rabotora.Launcher
{
	partial class FormBase
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
			this.SuspendLayout();
			// 
			// FormBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1264, 681);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.Name = "FormBase";
			this.Text = "Rabotora Game Launcher Main";
			this.Shown += new System.EventHandler(this.FormBase_Shown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormBase_KeyUp);
			this.Resize += new System.EventHandler(this.FormBase_Resize);
			this.ResumeLayout(false);

		}



		#endregion
	}
}