using System.Globalization;

public static class GCodeFileHandler
{
	// Ler o arquivo G-code e converter para a entidade GcodeLines
	public static GcodeLines ParseGCodeFile(string filePath)
	{
		var gcodeLines = new GcodeLines();
		using (var reader = new StreamReader(filePath))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				var command = ParseGCodeLine(line);
				if (command != null)
				{
					gcodeLines.AddCommand(command);
				}
			}
		}
		return gcodeLines;
	}

	// Escrever o arquivo G-code a partir da entidade GcodeLines
	public static void WriteGCodeFile(string filePath, GcodeLines gcodeLines)
	{
		using (var writer = new StreamWriter(filePath))
		{
			foreach (var command in gcodeLines.Commands)
			{
				writer.WriteLine(command.ToString());
			}
		}
	}

	public static GcodeLineCommand ParseGCodeLine(string line)
	{
		// Remove comentários da linha
		var commentIndex = line.IndexOf(';');
		string comments = "";
		if (commentIndex >= 0)
		{
			comments = line.Substring(commentIndex + 1).Trim();
			line = line.Substring(0, commentIndex).Trim();
		}

		// Extrai tags da linha (ex: [tag1, tag2])
		var tags = new List<string>();
		var tagStartIndex = line.IndexOf('[');
		var tagEndIndex = line.IndexOf(']');
		if (tagStartIndex >= 0 && tagEndIndex > tagStartIndex)
		{
			var tagContent = line.Substring(tagStartIndex + 1, tagEndIndex - tagStartIndex - 1);
			tags.AddRange(tagContent.Split(',').Select(t => t.Trim()));
			line = line.Substring(0, tagStartIndex).Trim();
		}

		// Divide a linha em partes (comando e átomos)
		var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
		{
			return null; // Linha vazia ou apenas comentários/tags
		}

		// Cria o comando
		var command = new GcodeLineCommand(parts[0]);
		command.Comments = comments;
		command.Tags = tags;

		// Processa os átomos (ex: X10, Y20)
		for (int i = 1; i < parts.Length; i++)
		{
			var part = parts[i];
			if (part.Length > 1 && char.IsLetter(part[0]))
			{
				char letter = part[0];
				if (double.TryParse(part.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
				{
					command.AddAtom(letter, value);
				}
			}
		}

		return command;
	}
}