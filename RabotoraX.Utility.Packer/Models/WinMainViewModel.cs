using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RabotoraX.Utility.Packer.Models;

public partial class WinMainViewModel : ObservableObject
{
	private readonly IStorageProvider _storageProvider;

	[ObservableProperty] private ObservableCollection<PackItem> _files = [];
	[ObservableProperty] private bool _useCompression = true;
	[ObservableProperty] private string _encryptionkey = string.Empty;
	[ObservableProperty] private ComboBoxItem? _blockSize;
	[ObservableProperty] private string _statusText = "Ready";
	[ObservableProperty] private float _progressValue = 0;
	[ObservableProperty] private bool _isPacking = false;

	public WinMainViewModel(IStorageProvider provider)
	{
		_storageProvider = provider;
	}

	[RelayCommand]
	private async Task AddFilesAsync()
	{
		var result = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select files to pack",
			AllowMultiple = true
		});
		foreach (var file in result)
		{
			Files.Add(new PackItem()
			{
				LogicalPath = file.Name,
				PhysicalPath = file.Path.LocalPath
			});
		}
		StatusText = $"Added {result.Count} files.";
	}

	[RelayCommand]
	private async Task AddFolderAsync()
	{
		var result = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
		if (result.Count > 0)
		{
			var folderPath = result[0].Path.LocalPath;
			string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                
			foreach (var file in allFiles)
			{
				var relativePath = Path.GetRelativePath(folderPath, file).Replace('\\', '/');
				Files.Add(new PackItem { LogicalPath = relativePath, PhysicalPath = file });
			}
			StatusText = $"Added {allFiles.Length} files from folder.";
		}
	}

	[RelayCommand]
	private void Clear()
	{
		Files.Clear();
		StatusText = "Cleared all files from list.";
	}

	[RelayCommand]
	private async Task PackAsync()
	{
		if (Files.Count == 0) 
		{
			StatusText = "No files to pack.";
			return;
		}
		
		var saveFile = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Save packed file",
			SuggestedFileName = "Resources.pkg",
			DefaultExtension = "pkg",
			FileTypeChoices = [ new FilePickerFileType("Rabotora Data Package") { Patterns = [ "*.pkg" ] } ]
		});
		
		if (saveFile == null) return;

		IsPacking = true;
		ProgressValue = 0;
		StatusText = "Packing files...";
		
		try
		{
			int bSize = 65536;
			if (BlockSize?.Content is string s && int.TryParse(s, out int parsed)) bSize = parsed;

			byte[]? keyBytes = null;
			if (!string.IsNullOrWhiteSpace(Encryptionkey))
			{
				keyBytes = Encoding.UTF8.GetBytes(Encryptionkey);
				if (keyBytes.Length != 16 && keyBytes.Length != 24 && keyBytes.Length != 32)
				{
					StatusText = "Pack failed: Encryption key must be 16, 24, or 32 bytes long.";
					return;
				}
			}

			var progress = new Progress<float>(p => ProgressValue = p * 100);

			await Task.Run(() => RDataPacker.PackAsync(saveFile.Path.LocalPath, Files, bSize, UseCompression, keyBytes, progress));

			StatusText = "Pack successful!";
		}
		catch (Exception ex)
		{
			StatusText = $"Pack failed: {ex.Message}";
		}
		finally
		{
			IsPacking = false;
		}
	}

	private bool CanPack() => !IsPacking;
}