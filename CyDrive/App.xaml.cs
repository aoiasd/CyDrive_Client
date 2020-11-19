using CyDrive.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CyDrive
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public ObservableCollection<FileInfo> LocalFileList { get; set; } = new ObservableCollection<FileInfo>();
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			
		}
	}
}
