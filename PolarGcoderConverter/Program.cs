using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
	static void Main(string[] args)
	{
		// Caminho da pasta de entrada
		string inputPath = @"C:\Users\vinic\Downloads\3d";

		// Nome do arquivo de entrada
		string inputFileName = "CFFFP_20mm-box.gcode";

		// Caminho completo do arquivo de entrada
		string inputFilePath = Path.Combine(inputPath, inputFileName);

		// Nome do arquivo de saída (adiciona o sufixo "_rotational")
		string outputFileName = Path.GetFileNameWithoutExtension(inputFileName) + "_rotational.gcode";

		// Caminho completo do arquivo de saída
		string outputFilePath = Path.Combine(inputPath, outputFileName);

		try
		{
			// Ler o arquivo G-code
			var gcodeLines = GCodeFileHandler.ReadGCodeFile(inputFilePath);

			// Converter coordenadas Y para rotação da mesa (100 mm = 360 graus)
			var rotationalGcodeLines = RotationalConverter.ConvertToRotational(gcodeLines);

			// Escrever o arquivo G-code com coordenadas ajustadas
			GCodeFileHandler.WriteGCodeFile(outputFilePath, rotationalGcodeLines);

			Console.WriteLine("Conversão concluída com sucesso!");
			Console.WriteLine($"Arquivo de saída: {outputFilePath}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ocorreu um erro: {ex.Message}");
		}
	}
}

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

// Classe para conversão de coordenadas Y para rotação da mesa
public static class RotationalConverter
{
	public static List<string> ConvertToRotational(List<string> gcodeLines)
	{
		var rotationalLines = new List<string>();

		foreach (var line in gcodeLines)
		{
			if (line.StartsWith("G0") || line.StartsWith("G1"))
			{
				var parts = line.Split(' ');
				double x = 0, y = 0, z = 0;
				bool hasX = false, hasY = false, hasZ = false;

				foreach (var part in parts)
				{
					if (part.StartsWith("X"))
					{
						x = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
						hasX = true;
					}
					else if (part.StartsWith("Y"))
					{
						y = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
						hasY = true;
					}
					else if (part.StartsWith("Z"))
					{
						z = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
						hasZ = true;
					}
				}

				if (hasY)
				{
					// Converte Y para rotação da mesa (100 mm = 360 graus)
					double rotationalY = (y / 100.0) * 360.0;

					// Substitui Y pelo valor convertido, usando cultura invariável
					var newParts = parts.Select(p =>
					{
						if (p.StartsWith("Y")) return $"Y{rotationalY.ToString("F3", CultureInfo.InvariantCulture)}";
						return p;
					}).ToArray();

					rotationalLines.Add(string.Join(" ", newParts));
				}
				else
				{
					rotationalLines.Add(line); // Mantém a linha inalterada se não tiver Y
				}
			}
			else
			{
				rotationalLines.Add(line); // Mantém linhas que não são G0 ou G1
			}
		}

		return rotationalLines;
	}
	public static List<string> DetectLongRotations(List<string> gcodeLines)
	{
		var longRotationLines = new List<string>();
		double previousY = 0;

		foreach (var line in gcodeLines)
		{
			if (line.StartsWith("G0"))
			{
				var parts = line.Split(' ');
				double y = 0;
				bool hasY = false;

				foreach (var part in parts)
				{
					if (part.StartsWith("Y"))
					{
						y = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
						hasY = true;
					}
				}

				if (hasY)
				{
					// Calcula a diferença em Y (rotação da mesa)
					double deltaY = Math.Abs(y - previousY);

					// Se a diferença for maior que 180 graus, é uma volta pelo lado mais longo
					if (deltaY > 180)
					{
						longRotationLines.Add(line);
					}

					// Atualiza o valor anterior de Y
					previousY = y;
				}
			}
		}

		return longRotationLines;
	}
}