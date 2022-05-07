#pragma warning disable CA2211
using System;
using System.Reflection;
using System.Windows.Forms;

namespace Rabotora.Launcher.SubForms
{
	public partial class Frm_About : Form
	{
		public static bool IsShown = false;
		public Frm_About()
		{
			InitializeComponent();
			IsShown = true;
		}

		private void Frm_About_Load(object sender, EventArgs e)
		{
			tbx_About.Text = tbx_About.Text.Replace("{Version}", Assembly.GetExecutingAssembly().GetName().Version!.ToString(3));
		}

		private void Frm_About_FormClosing(object sender, FormClosingEventArgs e)
		{
			IsShown = false;
		}
	}
}
