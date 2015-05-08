using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SocketSenderServerWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private IProgress<string> progress_str;
		private IProgress<Boolean> progress_srv;

		private Server server;

		public MainWindow()
		{
			InitializeComponent();

			progress_str = new Progress<string>(status =>
			{
				OutputLog.Items.Add(status);
				OutputLog.UpdateLayout();
				OutputLog.ScrollIntoView(OutputLog.Items[OutputLog.Items.Count - 1]);
			});

			progress_srv = new Progress<Boolean>(status =>
			{
				PortNoBox.IsEnabled = !status;
				if (!status)
				{
					UpdateOpenBtnStatus();
					ServerStatusText.Content = "Server is stopped";
				}
				else
				{
					StartMenuItem.IsEnabled = !status;
					ServerStatusText.Content = "Server is running";
				}
				StopMenuItem.IsEnabled = status;
			});

			ServerIpBox.Text = GetIP();
			PortNoBox.Text = "5050";

			progress_srv.Report(false);

			server = new Server(progress_str, progress_srv);
		}

		private string GetIP()
		{
			String strHostName = Dns.GetHostName();

			// Find host by name
			IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

			// Grab the first IP addresses
			String IPStr = "";
			foreach (IPAddress ipaddress in iphostentry.AddressList)
			{
				if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
				{
					IPStr = ipaddress.ToString();
					break;
				}
			}
			return IPStr;
		}

		private IPAddress parseIpAddr()
		{
			try
			{
				IPAddress ip = IPAddress.Parse(ServerIpBox.Text);
				return ip;
			}
			catch (Exception e)
			{
				progress_str.Report("ERROR: Could not parse server IP!");
				progress_str.Report(e.ToString());
			}

			return null;
		}

		private int parsePortNo()
		{
			int port = -1;

			try
			{
				port = System.Convert.ToInt16(PortNoBox.Text);
			}
			catch (Exception e)
			{
				progress_str.Report("ERROR: Could not parse port number!");
				progress_str.Report(e.ToString());
			}

			return port;
		}

		private void UpdateOpenBtnStatus()
		{
			Match matchIp = Regex.Match(ServerIpBox.Text, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
			Match matchPort = Regex.Match(PortNoBox.Text, @"\d");

			if (String.IsNullOrEmpty(ServerIpBox.Text) || String.IsNullOrEmpty(PortNoBox.Text) || !matchIp.Success || !matchPort.Success)
			{
				StartMenuItem.IsEnabled = false;
			}
			else
			{
				StartMenuItem.IsEnabled = true;
			}
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void StopMenuItem_Click(object sender, RoutedEventArgs e)
		{
			server.closeSocket();
		}

		private void StartMenuItem_Click(object sender, RoutedEventArgs e)
		{
			StartMenuItem.IsEnabled = false;

			// See if we have text on the IP and Port text fields
			if (ServerIpBox.Text == "" || PortNoBox.Text == "")
			{
				progress_str.Report("IP Address and Port Number are required to start the Server");
				server.closeSocket();
				return;
			}

			server.openSocket(System.Convert.ToInt32(PortNoBox.Text));
		}

		private void PortNoBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateOpenBtnStatus();
		}

		private void PortNoBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch(e.Key)
			{
				case Key.D0:
				case Key.D1:
				case Key.D2:
				case Key.D3:
				case Key.D4:
				case Key.D5:
				case Key.D6:
				case Key.D7:
				case Key.D8:
				case Key.D9:
				case Key.Back:
				case Key.Delete:
				case Key.Left:
				case Key.Up:
				case Key.Down:
				case Key.Right:
					break;
				default:
					e.Handled = true;
					break;
			}
		}

		private void PortNoBox_PastingHandler(object sender, DataObjectPastingEventArgs e)
		{
			e.CancelCommand();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			server.closeSocket();
		}
	}
}
