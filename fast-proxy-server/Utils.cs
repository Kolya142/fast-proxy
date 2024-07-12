using System.Net;
using System.Net.Sockets;
using System.Text;
static class Utils
{
	public static EndPoint CreateEndPoint(string ip, int port)
	{
		IPAddress ipAddr = IPAddress.Parse(ip);
		IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
		return ipEndPoint;
	}
	public static string BytesToString(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}
	public static byte[] RequestAll(Socket client)
	{
		List<byte> bytes = new List<byte>();
		for (int i = 0; i < 5; i++)
		{
			if (client.Available > 0)
				break;
			Task.Delay(10);
		}
		while (client.Available > 0)
		{
			byte[] data = new byte[2048];
			int dataSize = client.Receive(data);
			data = data.Take(dataSize).ToArray();
			bytes.AddRange(data);
		}
		return bytes.ToArray();
	}
	public static byte[] StringToBytes(string strings)
	{
		return Encoding.UTF8.GetBytes(strings);
	}
	public static byte[] MakeHeader(byte[] html)
	{
		string header = "HTTP/1.0 200 OK\r" +
						"Server: SimpleHTTP/0.1 .NET/9.0\r" +
						"Date: Unknown\r" +
						"Content-type: text/html; charset=utf-8\r" +
						"Content-Length: " + html.Length.ToString() + "\n\n";
		return StringToBytes(header);
	}
}