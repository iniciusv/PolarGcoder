using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class RotationalConverter
{
	private readonly int _xPrecision;
	private readonly int _yPrecision;
	private readonly double _thetaConversionFactor;

	// Construtor da classe
	public RotationalConverter(int xPrecision = 3, int yPrecision = 2, double thetaConversionFactor = 360.0)
	{
		_xPrecision = xPrecision;
		_yPrecision = yPrecision;
		_thetaConversionFactor = thetaConversionFactor;
	}

	// Método para converter coordenadas cartesianas em polares
	public GcodeLines ConvertToRotational(GcodeLines gcodeLines)
	{
		var rotationalLines = new GcodeLines();

		foreach (var command in gcodeLines.Commands)
		{
			if (command.Command.StartsWith("G0") || command.Command.StartsWith("G1"))
			{
				double x = 0, y = 0;

				// Extrai os valores de X e Y dos átomos
				foreach (var atom in command.Atoms)
				{
					if (atom.Letter == 'X')
					{
						x = atom.Value;
					}
					else if (atom.Letter == 'Y')
					{
						y = atom.Value;
					}
				}

				// Calcula as novas coordenadas polares
				var (r, theta) = CartesianToPolar(x, y);
				double newX = theta;  // Ângulo em graus
				double newY = r;      // Raio

				// Reconstroi o comando com as novas coordenadas
				var newCommand = ReconstructCommand(command, newX, newY);
				rotationalLines.AddCommand(newCommand);
			}
			else
			{
				// Mantém comandos que não são G0 ou G1
				rotationalLines.AddCommand(command);
			}
		}

		return rotationalLines;
	}

	// Método para reconstruir um comando com as novas coordenadas
	private GcodeLineCommand ReconstructCommand(GcodeLineCommand command, double newX, double newY)
	{
		var newCommand = new GcodeLineCommand(command.Command)
		{
			Comments = command.Comments,
			Tags = command.Tags
		};

		bool hasX = false;
		bool hasY = false;

		// Percorre os átomos do comando original
		foreach (var atom in command.Atoms)
		{
			if (atom.Letter == 'X')
			{
				// Substitui o valor de X pelo novo valor, aplicando o arredondamento
				newCommand.AddAtom('X', Math.Round(newX, _xPrecision));
				hasX = true;
			}
			else if (atom.Letter == 'Y')
			{
				// Substitui o valor de Y pelo novo valor, aplicando o arredondamento
				newCommand.AddAtom('Y', Math.Round(newY, _yPrecision));
				hasY = true;
			}
			else
			{
				// Mantém os outros átomos
				newCommand.AddAtom(atom.Letter, atom.Value);
			}
		}

		// Insere X antes de Y, se necessário
		if (!hasX)
		{
			InsertAtom(newCommand, 'X', Math.Round(newX, _xPrecision), 'Y');
		}

		// Insere Y depois de X, se necessário
		if (!hasY)
		{
			InsertAtom(newCommand, 'Y', Math.Round(newY, _yPrecision), 'X');
		}

		return newCommand;
	}

	// Função auxiliar para inserir um átomo na posição correta
	private void InsertAtom(GcodeLineCommand command, char letter, double value, char referenceLetter)
	{
		int referenceIndex = command.Atoms.FindIndex(a => a.Letter == referenceLetter);
		if (referenceIndex == -1)
		{
			// Se a referência não existir, insere no final
			command.AddAtom(letter, value);
		}
		else
		{
			// Insere antes ou depois da referência, dependendo da letra
			if (letter == 'X' && referenceLetter == 'Y')
			{
				command.Atoms.Insert(referenceIndex, new GcodeAtom(letter, value));
			}
			else if (letter == 'Y' && referenceLetter == 'X')
			{
				command.Atoms.Insert(referenceIndex + 1, new GcodeAtom(letter, value));
			}
		}
	}

	// Método para converter coordenadas cartesianas em polares
	public (double, double) CartesianToPolar(double x, double y)
	{
		double r = Math.Sqrt(x * x + y * y); // Calcula o raio (hipotenusa)
		double theta = Math.Atan2(y, x) * (_thetaConversionFactor / (2 * Math.PI)); // Calcula o ângulo com o fator de conversão

		return (r, theta); // Retorna o raio e o ângulo
	}

	// Método para detectar rotações longas
	public GcodeLines DetectLongRotations(GcodeLines gcodeLines)
	{
		var longRotationLines = new GcodeLines();
		double previousY = 0;

		foreach (var command in gcodeLines.Commands)
		{
			if (command.Command.StartsWith("G0"))
			{
				double y = 0;
				bool hasY = false;

				// Extrai o valor de Y dos átomos
				foreach (var atom in command.Atoms)
				{
					if (atom.Letter == 'Y')
					{
						y = atom.Value;
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
						longRotationLines.AddCommand(command);
					}

					// Atualiza o valor anterior de Y
					previousY = y;
				}
			}
		}

		return longRotationLines;
	}
}