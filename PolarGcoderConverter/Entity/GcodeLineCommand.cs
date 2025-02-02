public class GcodeLineCommand
{
	public string Command { get; set; } // Comando principal (ex: "G0", "G1")
	public List<GcodeAtom> Atoms { get; set; } // Lista de átomos (ex: X10, Y20)
	public string Comments { get; set; } // Comentários associados ao comando
	public List<string> Tags { get; set; } // Tags associadas ao comando

	// Construtor para facilitar a criação de comandos
	public GcodeLineCommand(string command)
	{
		Command = command;
		Atoms = new List<GcodeAtom>();
		Comments = string.Empty;
		Tags = new List<string>();
	}

	// Adiciona um átomo ao comando
	public void AddAtom(char letter, double value)
	{
		Atoms.Add(new GcodeAtom(letter, value));
	}

	// Sobrescreve o método ToString para exibir o comando no formato G-code
	public override string ToString()
	{
		var atomsString = string.Join(" ", Atoms);
		var tagsString = Tags.Count > 0 ? $" [{string.Join(", ", Tags)}]" : "";
		var commentsString = string.IsNullOrEmpty(Comments) ? "" : $" ;{Comments}";
		return $"{Command} {atomsString}{tagsString}{commentsString}";
	}
}
