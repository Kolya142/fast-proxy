using System.Net;
using System.Net.Sockets;
using Logging;
using Newtonsoft.Json;
using Pages;
using SettingsCode;

namespace Program;
class Program
{
	List<IPage> pagesList = new();
	public List<LogEntryStruct> logEntries = new();
	private void CreatePagesList(Root settings)
	{
		pagesList.Clear();
		// pagesList.Add(new MakingProxyServerPage());
		pagesList.Add(new ServerInfoPage(ref logEntries));
		pagesList.Add(new Proxy(settings));
	}
	static async Task ClientProccess(Program program, Task<Socket> task_client, int request_index)
	{
		Socket client = await task_client;
		string html =
			"<head>" +
			"</head>" +
			"<body>" +
			"	<h1>FPS</h1>" +
			"	<br />" +
			"	<a href=\"info\">Info Page</a>" +
			"</body>";
		byte[] htmlbytes = Utils.StringToBytes(html);
		string header =
			"HTTP/1.0 200 OK\r" +
			"Server: SimpleHTTP/0.1 .NET/9.0\r" +
			"Date: Unknown\r" +
			"Content-type: text/html; charset=utf-8\r" +
			"Content-Length: " + htmlbytes.Length.ToString() + "\n\n";
		byte[] headerbytes = Utils.StringToBytes(header);
		byte[] buffer = Utils.RequestAll(client);
		File.WriteAllBytes($"C:\\Users\\sony\\Documents\\FastProxyLogs\\{request_index}_rq.bin", buffer);
		string bufferText = Utils.BytesToString(buffer);
		string[] bufferSplit = bufferText.Split('\n')[0].Split(" ");
		byte[] response = { 0 };
		bool is_page = false;
		if (bufferSplit.Length >= 3)
		{
			foreach (IPage page in program.pagesList)
			{
				if (page.Test(bufferSplit[1]))
				{
					if (page.HighLevel)
					{
						byte[] paged = await page.CreateHtml(bufferSplit[1], buffer);
						response = Utils.MakeHeader(paged).Concat(paged).ToArray();
						client.Send(response);
						is_page = true;
					}
					else
					{
						byte[] paged = await page.CreateHtml(bufferSplit[1], buffer);
						response = paged;
						int i = 0;
						while (paged.Length > 0)
						{
							i = client.Send(paged);
							paged = paged.TakeLast(paged.Length - i).ToArray();
						}
						is_page = true;
					}
				}
			}
			if (bufferSplit[1] == "/")
			{
				is_page = true;
				response = headerbytes.Concat(htmlbytes).ToArray();
				client.Send(response);
			}
		}
		if (!is_page)
		{
			response = new byte[]{ 0x48, 0x54, 0x54, 0x50, 0x2F, 0x31, 0x2E, 0x30, 0x20, 0x34, 0x30, 0x34, 0x20, 0x45, 0x52, 0x52, 0x0D, 0x0A, 0x0D, 0x0A, 0x3C, 0x68, 0x31, 0x3E, 0x34, 0x30, 0x34, 0x20, 0x70, 0x61, 0x67, 0x65, 0x3C, 0x2F, 0x68, 0x31, 0x3E};
			client.Send(response);
		}

		File.WriteAllBytes($"C:\\Users\\sony\\Documents\\FastProxyLogs\\{request_index}_rs.bin", response);
		LogEntryStruct log = new LogEntryStruct()
		{
			ClientAddr = (client.RemoteEndPoint as IPEndPoint)?.Address?.ToString(),
			Request = buffer,
			Response = response,
			url = bufferSplit[1],
			type = bufferSplit[0],
		};
		program.logEntries.Add(log);
		client.Shutdown(SocketShutdown.Both);
		client.Close();
	}
	static async Task Main(string[] args)
	{
		string json = File.ReadAllText("example.json");
		Root settings = JsonConvert.DeserializeObject<Root>(json);
		
		Program program = new();
		program.CreatePagesList(settings);
		Socket sender = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		sender.Bind(Utils.CreateEndPoint(settings.options.Addr, settings.options.Port));
		sender.Listen(10);
		int request_index = 0;
		while (true)
		{
			try
			{
				Task<Socket> client = sender.AcceptAsync();
				_ = ClientProccess(program, client, request_index);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			request_index++;
		}
	}
}