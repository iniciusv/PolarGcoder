using System.Collections.Generic;
using Xunit;

public class RotationalConverterTests
{
	[Theory]
	[InlineData(0, 0, 0, 0)] // Teste com a origem
	[InlineData(10, 10, 14.142, 45)]
	[InlineData(0, 10, 10, 90)]
	[InlineData(10, 0, 10, 0)]
	public void CartesianToPolar_ShouldConvertCorrectly(double x, double y, double expectedR, double expectedTheta)
	{
		// Arrange
		var tolerance = 0.0001; // Tolerância para diferenças devido a erros de ponto flutuante

		// Act
		var RotationalConverter = new RotationalConverter();
		var (actualR, actualTheta) = RotationalConverter.CartesianToPolar(x, y);

		// Assert
		Assert.True(Math.Abs(actualR - expectedR) < tolerance, $"Expected radius: {expectedR}, but got: {actualR}");
		Assert.True(Math.Abs(actualTheta - expectedTheta) < tolerance, $"Expected angle: {expectedTheta} degrees, but got: {actualTheta} degrees");
	}




	[Theory]
	[InlineData("G1 X0 Y0 Z0", "G1 X0 Y0 Z0", 360)]
	[InlineData("G1 X10 Y10 Z0", "G1 X45 Y14.14 Z0", 360)]
	[InlineData("G1 X10 Y10 Z0", "G1 X12.5 Y14.14 Z0", 100)]

	[InlineData("M104 S200", "M104 S200", 360)] // Comando não relacionado a movimento: inalterado.
	public void TestConvertToRotational_SingleLine(string inputLine, string expectedLine, int thetaConversionFactor)
	{
		// Arrange
		var gcodeLines = new List<string> { inputLine };

		// Act
		var RotationalConverter = new RotationalConverter(3, 2, thetaConversionFactor);

		var result = RotationalConverter.ConvertToRotational(gcodeLines);
		var trimmedResult = ZeroTrimmer.TrimZeros(result);

		// Assert
		Assert.Equal(expectedLine, trimmedResult[0]);
	}



[Fact]
	public void TestConvertToRotational_MultipleLines()
	{
		// Arrange
		var gcodeLines = new List<string>
		{
			"G1 X50 Y100 Z0",  // Movimento 1
            "M104 S200",       // Comando não de movimento
            "G1 X100 Y25 Z0"   // Movimento 2
        };

		/* 
         * Cálculo do baricentro:
         *   Considerando os comandos de movimento: (50,100) e (100,25)
         *   centerX = (50 + 100) / 2 = 75
         *   centerY = (100 + 25) / 2 = 62.5
         * 
         * Linha 1:
         *   Y' = 100 - 62.5 = 37.5
         *   Ângulo = (37.5 / 60) * 360 = 225°
         *   Resultado esperado: "G1 X225 Y37.5 Z0"
         * 
         * Linha 3:
         *   Y' = 25 - 62.5 = -37.5
         *   Ângulo = (-37.5 / 60) * 360 = -225° → normalizado para 135°
         *   Resultado esperado: "G1 X135 Y-37.5 Z0"
         */
		var expectedLines = new List<string>
		{
			"G1 X225 Y37.5 Z0",
			"M104 S200",
			"G1 X135 Y-37.5 Z0"
		};

		// Act
		var RotationalConverter = new RotationalConverter();

		var result = RotationalConverter.ConvertToRotational(gcodeLines);
		var trimmedResult = ZeroTrimmer.TrimZeros(result);

		// Assert
		Assert.Equal(expectedLines, trimmedResult);
	}

	[Fact]
	public void TestConvertToRotational_TwoMovementCommands()
	{
		// Arrange
		var gcodeLines = new List<string>
		{
			"G1 X0 Y60 Z0",  // Movimento 1
            "G1 X0 Y0 Z0"    // Movimento 2
        };

		/* 
         * Cálculo:
         *   centerY = (60 + 0) / 2 = 30.
         * 
         * Linha 1:
         *   Y' = 60 - 30 = 30 → Ângulo = (30 / 60) * 360 = 180°
         * Linha 2:
         *   Y' = 0 - 30 = -30 → Ângulo = (-30 / 60) * 360 = -180° → normalizado para 180°
         */
		var expectedLines = new List<string>
		{
			"G1 X180 Y30 Z0",
			"G1 X180 Y-30 Z0"
		};

		// Act
		var RotationalConverter = new RotationalConverter();

		var result = RotationalConverter.ConvertToRotational(gcodeLines);
		var trimmedResult = ZeroTrimmer.TrimZeros(result);

		// Assert
		Assert.Equal(expectedLines, trimmedResult);
	}
}
