using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class GCodeTranslator
{
	// Método para transladar as coordenadas X e Y do G-code
	public GcodeLines Translate(GcodeLines gcodeLines, double offsetX, double offsetY)
	{
		var translatedLines = new GcodeLines();

		foreach (var command in gcodeLines.Commands)
		{
			var translatedCommand = new GcodeLineCommand(command.Command)
			{
				Comments = command.Comments,
				Tags = command.Tags
			};

			// Translada os átomos X e Y
			foreach (var atom in command.Atoms)
			{
				if (atom.Letter == 'X')
				{
					translatedCommand.AddAtom('X', atom.Value + offsetX);
				}
				else if (atom.Letter == 'Y')
				{
					translatedCommand.AddAtom('Y', atom.Value + offsetY);
				}
				else
				{
					translatedCommand.AddAtom(atom.Letter, atom.Value);
				}
			}

			translatedLines.AddCommand(translatedCommand);
		}

		return translatedLines;
	}

	// Método para calcular o baricentro (centro geométrico) da geometria
	public (double centroidX, double centroidY) FindCentroid(GcodeLines gcodeLines)
	{
		double sumX = 0;
		double sumY = 0;
		int count = 0;

		foreach (var command in gcodeLines.Commands)
		{
			if (command.Command.StartsWith("G0") || command.Command.StartsWith("G1"))
			{
				double x = 0;
				double y = 0;
				bool hasX = false;
				bool hasY = false;

				// Extrai os valores de X e Y dos átomos
				foreach (var atom in command.Atoms)
				{
					if (atom.Letter == 'X')
					{
						x = atom.Value;
						hasX = true;
					}
					else if (atom.Letter == 'Y')
					{
						y = atom.Value;
						hasY = true;
					}
				}

				if (hasX && hasY)
				{
					sumX += x;
					sumY += y;
					count++;
				}
			}
		}

		if (count == 0)
		{
			throw new InvalidOperationException("Nenhuma coordenada X e Y encontrada para calcular o baricentro.");
		}

		double centroidX = sumX / count;
		double centroidY = sumY / count;

		return (centroidX, centroidY);
	}
}