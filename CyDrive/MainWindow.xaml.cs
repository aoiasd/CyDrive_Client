using CyDrive.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xaml;

namespace CyDrive
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public ObservableCollection<FileInfo> localFileList,remoteFileList;

		private CyDriveClient cdc;
		private DispatcherTimer timer = new DispatcherTimer();

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cdc = new CyDriveClient();
			var isLogin = await cdc.LoginAsync();
			remoteFileList = new ObservableCollection<FileInfo>(await cdc.ListRemoteDirAsync());
			localFileList = new ObservableCollection<FileInfo>(await Utils.Utils.ListLocalDirAsync(cdc.User));
			RemoteFileListBox.ItemsSource = remoteFileList;
			LocalFileListBox.ItemsSource = localFileList;

			timer.Tick += RefreshRemoteFileListTick;
			timer.Interval = new TimeSpan(0, 0, 2);
			timer.Start();
		}

		private async void RefreshRemoteFileListTick(object sender, EventArgs e)
		{
			RefreshList(remoteFileList, await cdc.ListRemoteDirAsync());
		}

		private async void OpenFileWithDefaultProgram(object sender, MouseButtonEventArgs e)
		{
			if (LocalFileListBox.SelectedItem == null) return;
			FileInfo fileInfo = LocalFileListBox.SelectedItem as FileInfo;
			var path = System.IO.Path.Combine(cdc.User.WorkDir, fileInfo.FilePath);
			if (!fileInfo.IsDir)
			{
				new Process
				{
					StartInfo = new ProcessStartInfo(path)
					{
						UseShellExecute = true
					}
				}.Start();
			}
			else
			{
				cdc.User.WorkDir = path;
				RefreshList(localFileList, await Utils.Utils.ListLocalDirAsync(cdc.User));
			}
			
		}

		private async void Download(object sender, EventArgs e)
		{
			foreach (var Item in RemoteFileListBox.SelectedItems)
			{
				var fileInfo = Item as FileInfo;
				await cdc.DownloadAsync(fileInfo.FilePath);
			}
			RefreshList(localFileList, await Utils.Utils.ListLocalDirAsync(cdc.User));
		}

		private async void Upload(object sender, EventArgs e)
		{
			foreach (var Item in LocalFileListBox.SelectedItems)
			{
				var fileInfo = Item as FileInfo;
				await cdc.UploadAsync(fileInfo.FilePath);
			}
			RefreshList(remoteFileList, await cdc.ListRemoteDirAsync());
		}

		private async void Delete_Remote(object sender, EventArgs e)
		{
			foreach (var Item in RemoteFileListBox.SelectedItems)
			{
				var fileInfo = Item as FileInfo;
				await cdc.DeleteFileAsync(fileInfo.FilePath);
			}
			RefreshList(remoteFileList, await cdc.ListRemoteDirAsync());
		}

		private async void Delete_Local(object sender, EventArgs e)
		{
			foreach (var Item in LocalFileListBox.SelectedItems)
			{
				var fileInfo = Item as FileInfo;
				System.IO.File.Delete(
			 System.IO.Path.Combine(
				 cdc.User.WorkDir, fileInfo.FilePath));
			}
			
			RefreshList(localFileList, await Utils.Utils.ListLocalDirAsync(cdc.User));
		}

		private void LocalFileListBox_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;
			var fileNameList = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach(var fileName in fileNameList)
			{
				var destPath = System.IO.Path.Combine(cdc.User.WorkDir,
					System.IO.Path.GetFileName(fileName));
				System.IO.File.Copy(fileName,
					destPath);

				var fileInfo = new FileInfo(cdc.User, System.IO.Path.GetFileName(fileName));
				localFileList.Add(fileInfo);
			}
		}

		private void RefreshList(ObservableCollection<FileInfo> list,FileInfo[] newList)
		{
			list.Clear();
			foreach (var fileInfo in newList)
			{
				list.Add(fileInfo);
			}
		}
	}
}
