using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

		private readonly Key[] allowedKeys = { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.Back, Key.Delete, Key.Left, Key.Up, Key.Down, Key.Right };

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
			if (!allowedKeys.Contains(e.Key))
			{
				e.Handled = true;
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
