using Avalonia.Controls;
using RabotoraX.Utility.Packer.Models;

namespace RabotoraX.Utility.Packer;

public partial class WinMain : Window
{
	public WinMain()
	{
		InitializeComponent();
		DataContext = new WinMainViewModel(StorageProvider);
	}
}