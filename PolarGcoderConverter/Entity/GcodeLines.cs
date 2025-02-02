// Entidade que representa um arquivo G-code
public class GcodeLines
{
	public List<GcodeLineCommand> Commands { get; set; } // Lista de comandos

	public GcodeLines()
	{
		Commands = new List<GcodeLineCommand>();
	}

	// Adiciona um comando à lista
	public void AddCommand(GcodeLineCommand command)
	{
		Commands.Add(command);
	}

	// Sobrescreve o método ToString para exibir o arquivo G-code
	public override string ToString()
	{
		return string.Join(Environment.NewLine, Commands);
	}
}