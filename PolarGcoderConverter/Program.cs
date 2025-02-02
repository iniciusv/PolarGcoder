using System;
using System.Collections.Generic;
using System.IO;

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
			// Ler o arquivo G-code e converter para a entidade GcodeLines
			var gcodeLines = GCodeFileHandler.ParseGCodeFile(inputFilePath);

			// Instanciar o tradutor de G-code
			var gcodeTranslator = new GCodeTranslator();

			// Encontrar o baricentro da geometria
			var (centroidX, centroidY) = gcodeTranslator.FindCentroid(gcodeLines);
			Console.WriteLine($"Baricentro encontrado: X = {centroidX}, Y = {centroidY}");

			// Transladar o G-code para que o baricentro fique na origem (0, 0)
			var translatedGcodeLines = gcodeTranslator.Translate(gcodeLines, -centroidX, -centroidY);

			// Instanciar o conversor rotacional
			var rotationalConverter = new RotationalConverter();

			// Converter coordenadas Y para rotação da mesa (100 mm = 360 graus)
			var rotationalGcodeLines = rotationalConverter.ConvertToRotational(translatedGcodeLines);

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