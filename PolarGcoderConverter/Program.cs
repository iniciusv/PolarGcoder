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

			var RotationalConverter = new RotationalConverter();

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

// Classe para conversão de coordenadas Y para rotação da mesa
public class RotationalConverter
{
	private readonly int XPrecision;
	private readonly int YPrecision;
	private readonly double ThetaConversionFactor;

	// Construtor da classe
	public RotationalConverter(int xPrecision = 3, int yPrecision = 2, double thetaConversionFactor = 360.0)
	{
		XPrecision = xPrecision;
		YPrecision = yPrecision;
		ThetaConversionFactor = thetaConversionFactor;
	}

	// Método para converter coordenadas cartesianas em polares
	public List<string> ConvertToRotational(List<string> gcodeLines)
	{
		var rotationalLines = new List<string>();

		foreach (var line in gcodeLines)
		{
			if (line.StartsWith("G0") || line.StartsWith("G1"))
			{
				var parts = line.Split(' ');
				double x = 0, y = 0;

				foreach (var part in parts)
				{
					if (part.StartsWith("X"))
					{
						x = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
					}
					else if (part.StartsWith("Y"))
					{
						y = double.Parse(part.Substring(1), CultureInfo.InvariantCulture);
					}
				}

				// Calcula as novas coordenadas polares
				var (r, theta) = CartesianToPolar(x, y);
				double newX = theta;  // Ângulo em graus
				double newY = r;      // Raio

				// Reconstroi a linha com as novas coordenadas
				ReconstructLine(rotationalLines, parts, newX, newY);
			}
			else
			{
				rotationalLines.Add(line); // Mantém linhas que não são G0 ou G1
			}
		}

		return rotationalLines;
	}

	// Método para reconstruir a linha com as novas coordenadas
	private void ReconstructLine(List<string> rotationalLines, string[] parts, double newX, double newY)
	{
		var newLine = new List<string>();

		bool hasX = false;
		bool hasY = false;

		// Percorre as partes da linha original
		foreach (var part in parts)
		{
			if (part.StartsWith("X"))
			{
				// Substitui o valor de X pelo novo valor, aplicando o arredondamento
				newLine.Add(ReplaceOrAddCoordinate(part, newX, "X", XPrecision));
				hasX = true;
			}
			else if (part.StartsWith("Y"))
			{
				// Substitui o valor de Y pelo novo valor, aplicando o arredondamento
				newLine.Add(ReplaceOrAddCoordinate(part, newY, "Y", YPrecision));
				hasY = true;
			}
			else
			{
				// Mantém as outras partes da linha
				newLine.Add(part);
			}
		}

		// Insere X antes de Y, se necessário
		if (!hasX)
		{
			InsertCoordinate(newLine, newX, "X", "Y", XPrecision);
		}

		// Insere Y depois de X, se necessário
		if (!hasY)
		{
			InsertCoordinate(newLine, newY, "Y", "X", YPrecision);
		}

		// Junta as partes para formar a nova linha e adiciona à lista de linhas rotacionais
		rotationalLines.Add(string.Join(" ", newLine));
	}

	// Função auxiliar para substituir ou adicionar uma coordenada com arredondamento
	private string ReplaceOrAddCoordinate(string part, double value, string prefix, int precision)
	{
		double roundedValue = Math.Round(value, precision);
		return $"{prefix}{roundedValue.ToString($"F{precision}", CultureInfo.InvariantCulture)}";
	}

	// Função auxiliar para inserir uma coordenada na posição correta com arredondamento
	private void InsertCoordinate(List<string> line, double value, string prefix, string referencePrefix, int precision)
	{
		double roundedValue = Math.Round(value, precision);
		int referenceIndex = line.FindIndex(p => p.StartsWith(referencePrefix));
		if (referenceIndex == -1)
		{
			// Se a referência não existir, insere no final
			line.Add($"{prefix}{roundedValue.ToString($"F{precision}", CultureInfo.InvariantCulture)}");
		}
		else
		{
			// Insere antes ou depois da referência, dependendo do prefixo
			if (prefix == "X" && referencePrefix == "Y")
			{
				line.Insert(referenceIndex, $"{prefix}{roundedValue.ToString($"F{precision}", CultureInfo.InvariantCulture)}");
			}
			else if (prefix == "Y" && referencePrefix == "X")
			{
				line.Insert(referenceIndex + 1, $"{prefix}{roundedValue.ToString($"F{precision}", CultureInfo.InvariantCulture)}");
			}
		}
	}

	// Método para converter coordenadas cartesianas em polares
	public (double, double) CartesianToPolar(double x, double y)
	{
		double r = Math.Sqrt(x * x + y * y); // Calcula o raio (hipotenusa)
		double theta = Math.Atan2(y, x) * (ThetaConversionFactor / (2 * Math.PI)); // Calcula o ângulo com o fator de conversão

		return (r, theta); // Retorna o raio e o ângulo
	}

public  List<string> DetectLongRotations(List<string> gcodeLines)
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