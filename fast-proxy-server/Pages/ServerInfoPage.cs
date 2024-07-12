using Logging;
using System.Text;

namespace Pages
{
	class ServerInfoPage : IPage
	{
		public bool HighLevel { get; set; } = true;
		List<LogEntryStruct> logEntries;
		async Task<byte[]> IPage.CreateHtml(string url, byte[] request)
		{
			StringBuilder li_list = new();
			int i = 0;
			StringBuilder javascript = new();
			foreach (LogEntryStruct Entry in logEntries)
			{
				if (Entry.url == "/info")
				{
					continue;
				}
				li_list.AppendLine($"<li><a onclick=\"l{i}();\" href=\"#\">Request {Entry.type}, {Entry.url}</a></li>");
				javascript.AppendLine($"function l{i}() {{" +
					$"document.getElementById('r').innerText = '{Entry.ToString().Replace("\n", "\\n").Replace("\r", "\\r")}';" +
					$"}}");
				i++;
			}									   
			return Utils.StringToBytes(								   										
				"<html>" +						   										
				"	<head>" +					   										
				"		<style>" +														
				"			body {" +													
				"				background: black;" +	
				"				color: #ffffff;" +										
				"			}" +														
				"			#e {" +
				"				background-color: #2F2F2F;" +														
				"			}" +														
				"			#view {" +													
				"				width: 100%;" +											
				"				height: 100%;" +
				"				background-color: #000000;" +							
				"				color: #abffab;" +										
				"			}" +														
				"		</style>" +														
				"		<script>" +														
			   $"			{javascript}" +
				"		</script>" +
				"	</head>" +
				"	<body>" +
				"		<ul id=\"select\">" +
			   $"			{li_list}" +
				"		</ul>" +
				"		<div id=\"view\">" +
				"			<hr />" +
				"			<div id=\"e\">" +
				"				<code id=\"r\" ></code>" +
				"			</div>" +
				"			<hr />" +
				"		</div>" +
				"	</body>" +
				"</html>");
		}

		public ServerInfoPage(ref List<LogEntryStruct> logEntries) {
			this.logEntries = logEntries;
		}

		bool IPage.Test(string url)
		{
			if (url == "/info")
			{
				return true;
			}
			return false;
		}
	}
}
