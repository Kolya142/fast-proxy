using SettingsCode;
using System.Net.Sockets;

namespace Pages
{
	class Proxy : IPage
	{
		private Root settings;
		public bool HighLevel { get; set; } = false;
		public Proxy(Root settings) { this.settings = settings; }
		async Task<byte[]> IPage.CreateHtml(string url, byte[] request)
		{
			foreach (Endpoint server in settings.servers)
			{
				foreach (SettingsCode.Path path in server.paths)
				{
					Console.WriteLine($"{path.rq}, {url}, {url.StartsWith(path.rq)}, {server.host}, {server.port}");
					if (url.StartsWith(path.rq))
					{
						request = Utils.StringToBytes(Utils.BytesToString(request).Replace(path.rq, path.re));
						Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						client.Connect(Utils.CreateEndPoint(server.host, server.port));
						client.Send(request);
						await Task.Delay(50);
						byte[] bytes = Utils.RequestAll(client);
						return bytes;
					}
				}
			}
			return new byte[] { 0x34, 0x30, 0x34 };
		}

		bool IPage.Test(string url)
		{
            foreach (Endpoint server in settings.servers)
            {
                foreach (SettingsCode.Path path in server.paths)
                {
					if (url.StartsWith(path.rq))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
