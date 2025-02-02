using System.Globalization;
// Entidade GcodeAtom: Representa um átomo de G-code (letra e valor)
public class GcodeAtom
{
	public char Letter { get; set; } // Letra do átomo (ex: 'X', 'Y', 'F')
	public double Value { get; set; } // Valor associado à letra

	// Construtor para facilitar a criação de átomos
	public GcodeAtom(char letter, double value)
	{
		Letter = letter;
		Value = value;
	}

	// Sobrescreve o método ToString para exibir o átomo no formato G-code
	public override string ToString()
	{
		return $"{Letter}{Value.ToString(CultureInfo.InvariantCulture)}";
	}
}
