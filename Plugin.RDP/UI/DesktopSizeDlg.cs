using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Plugin.RDP.UI
{
	internal partial class DesktopSizeDlg : Form
	{
		public String CustomSize
		{
			get	=> String.Format("{0}x{1}", txtWidth.Text, txtHeight.Text);
			set
			{
				if(!String.IsNullOrEmpty(value))
				{
					String[] wh = value.Split('x');
					txtWidth.Text = wh[0];
					txtHeight.Text = wh[1];
				}
			}
		}

		public DesktopSizeDlg(Object customSize)
		{
			InitializeComponent();
			if(customSize != null)
				this.CustomSize = customSize.ToString();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(base.DialogResult == DialogResult.OK)
			{
				Boolean cancel = false;
				if(!Int32.TryParse(txtWidth.Text, out Int32 dummy))
				{
					error.SetError(txtWidth, "Invalid width");
					cancel = true;
				}
				if(!Int32.TryParse(txtHeight.Text, out dummy))
				{
					error.SetError(txtHeight, "Invalid height");
					cancel = true;
				}
				e.Cancel = cancel;
			}
			base.OnClosing(e);
		}
	}
}