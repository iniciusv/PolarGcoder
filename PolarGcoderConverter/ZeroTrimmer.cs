using System;

using System.Collections.Generic;

using System.Globalization;


public static class ZeroTrimmer

{

	/// <summary>

	/// Remove zeros não significativos de números em uma lista de linhas G-code.

	/// </summary>

	/// <param name="gcodeLines">Lista de linhas do arquivo G-code.</param>

	/// <returns>Lista de linhas com zeros não significativos removidos.</returns>

	public static List<string> TrimZeros(List<string> gcodeLines)

	{

		var trimmedLines = new List<string>();


		foreach (var line in gcodeLines)

		{

			if (string.IsNullOrWhiteSpace(line))

			{

				// Mantém linhas em branco inalteradas

				trimmedLines.Add(line);

				continue;

			}


			// Divide a linha em partes separadas por espaços

			var parts = line.Split(' ');

			var newParts = new List<string>();


			foreach (var part in parts)

			{

				if (IsNumericPart(part))

				{

					// Remove zeros não significativos da parte numérica

					string trimmedPart = TrimNumericPart(part);

					newParts.Add(trimmedPart);

				}

				else

				{

					// Mantém partes não numéricas inalteradas

					newParts.Add(part);

				}

			}


			// Reconstrói a linha com as partes ajustadas

			trimmedLines.Add(string.Join(" ", newParts));

		}


		return trimmedLines;

	}


	/// <summary>

	/// Verifica se uma parte da linha contém um valor numérico (ex.: X1.234, Y-5.67).

	/// </summary>

	/// <param name="part">Parte da linha.</param>

	/// <returns>True se for numérico; caso contrário, false.</returns>

	private static bool IsNumericPart(string part)

	{

		// Verifica se a parte começa com uma letra seguida de dígitos ou ponto decimal

		return char.IsLetter(part[0]) && double.TryParse(part.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out _);

	}


	/// <summary>

	/// Remove zeros não significativos de uma parte numérica.

	/// </summary>

	/// <param name="part">Parte numérica (ex.: X1.200, Y-5.670).</param>

	/// <returns>Parte numérica com zeros não significativos removidos.</returns>

	private static string TrimNumericPart(string part)

	{

		// Separa a letra inicial (ex.: 'X', 'Y', 'Z') do valor numérico

		char prefix = part[0];

		string numericValue = part.Substring(1);


		// Converte o valor numérico para double e depois para string sem zeros extras

		if (double.TryParse(numericValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))

		{

			// Formata o número removendo zeros não significativos

			string trimmedValue = value.ToString("G15", CultureInfo.InvariantCulture);


			// Retorna a parte original com o valor ajustado

			return $"{prefix}{trimmedValue}";

		}


		// Se falhar na conversão, retorna a parte original inalterada

		return part;

	}

}