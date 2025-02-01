using System.Collections.Generic;
using Xunit;

public class RotationalConverterTests
{
	[Theory]
	[InlineData("G1 X50 Y100 Z0", "G1 X50 Y360.000 Z0")] // Y = 100 mm → Y = 360.000 graus
	[InlineData("G0 X50 Y100 Z0", "G0 X50 Y360.000 Z0")] // Y = 100 mm → Y = 360.000 graus
	[InlineData("G1 X100 Y50 Z0", "G1 X100 Y180.000 Z0")] // Y = 50 mm → Y = 180.000 graus
	[InlineData("G1 X150 Y25 Z0", "G1 X150 Y90.000 Z0")]  // Y = 25 mm → Y = 90.000 graus
	[InlineData("G1 X50 Y-100 Z0", "G1 X50 Y-360.000 Z0")] // Y = -100 mm → Y = -360.000 graus
	[InlineData("G1 X100 Y-50 Z0", "G1 X100 Y-180.000 Z0")] // Y = -50 mm → Y = -180.000 graus
	[InlineData("G1 X50 Z0", "G1 X50 Z0")] // Sem coordenada Y → linha inalterada
	[InlineData("M104 S200", "M104 S200")] // Comando não relacionado a movimento → linha inalterada
	public void TestConvertToRotational_InlineData(string inputLine, string expectedLine)
	{
		// Arrange
		var gcodeLines = new List<string> { inputLine };

		// Act
		var result = RotationalConverter.ConvertToRotational(gcodeLines);

		// Assert
		Assert.Equal(expectedLine, result[0]);
	}

	[Fact]
	public void TestConvertToRotational_MultipleLines()
	{
		// Arrange
		var gcodeLines = new List<string>
		{
			"G1 X50 Y100 Z0",
			"M104 S200", // Comando não relacionado a movimento
            "G1 X100 Y25 Z0"
		};

		var expectedLines = new List<string>
		{
			"G1 X50 Y360.000 Z0",
			"M104 S200",
			"G1 X100 Y90.000 Z0"
		};

		// Act
		var result = RotationalConverter.ConvertToRotational(gcodeLines);

		// Assert
		Assert.Equal(expectedLines, result);
	}
}