
// Classe para manipulação de arquivos G-code
public static class GCodeFileHandler
{
	public static List<string> ReadGCodeFile(string filePath)
	{
		var lines = new List<string>();
		using (var reader = new StreamReader(filePath))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				lines.Add(line);
			}
		}
		return lines;
	}

	public static void WriteGCodeFile(string filePath, List<string> lines)
	{
		using (var writer = new StreamWriter(filePath))
		{
			foreach (var line in lines)
			{
				writer.WriteLine(line);
			}
		}
	}
}
