using System;
using System.Net;
using System.Net.Sockets;

namespace SocketSenderServerWPF
{
	class Server
	{
		private IProgress<string> progress_str;
		private IProgress<Boolean> progress_hmi;

		private UdpClient receiver;

		public Server(IProgress<string> prg_str, IProgress<Boolean> prg_hmi)
		{
			progress_str = prg_str;
			progress_hmi = prg_hmi;
		}

		public void openSocket(int port)
		{
			receiver = new UdpClient(port);
			receiver.BeginReceive(new AsyncCallback(DataReceived), receiver);

			progress_hmi.Report(true);
		}

		public void closeSocket()
		{
			if (receiver != null)
			{
				receiver.Close();
				receiver = null;
			}
		}

		private void DataReceived(IAsyncResult ar)
		{
			try
			{
				UdpClient u = (UdpClient)ar.AsyncState;
				IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
				Byte[] receivedBytes = u.EndReceive(ar, ref receivedIpEndPoint);

				// Convert data to ASCII and print in console
				string receivedText = BitConverter.ToString(receivedBytes).Replace("-", string.Empty);
				progress_str.Report(receivedIpEndPoint + ": " + receivedText + Environment.NewLine);

				// Restart listening for udp data packages
				u.BeginReceive(new AsyncCallback(DataReceived), u);
			}
			catch (ObjectDisposedException)
			{
				progress_str.Report("DataReceived: Socket has been closed");
				progress_hmi.Report(false);
			}
			catch (SocketException se)
			{
				progress_str.Report(se.Message);
				progress_hmi.Report(false);
			}
		}
	}
}
