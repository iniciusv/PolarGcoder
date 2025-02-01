using System;
using System.Collections.Generic;
using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace GCodeVisualizer
{
	public class GCodeRenderer
	{
		// Lista de pontos do G-code
		private List<Vector3> points;

		// Valor máximo de Z a ser exibido
		private float maxZ;

		// Cor da última camada
		private Color lastLayerColor;

		public GCodeRenderer(List<Vector3> points, float maxZ, Color lastLayerColor)
		{
			this.points = points;
			this.maxZ = maxZ;
			this.lastLayerColor = lastLayerColor;
		}

		// Define o valor máximo de Z a ser exibido
		public void SetMaxZ(float maxZ)
		{
			this.maxZ = maxZ;
		}

		// Renderiza as camadas do G-code
		public void Render(Camera3D camera)
		{
			BeginMode3D(camera);

			// Variável para armazenar a última camada renderizada
			float lastZ = -1;

			// Desenhar as linhas do G-code
			for (int i = 1; i < points.Count; i++)
			{
				var start = points[i - 1];
				var end = points[i];

				// Verificar se os pontos estão dentro do limite de Z
				if (start.Y <= maxZ && end.Y <= maxZ)
				{
					// Verificar se é a última camada
					bool isLastLayer = (start.Y == maxZ || end.Y == maxZ);

					// Definir a cor da linha
					Color lineColor = isLastLayer ? lastLayerColor : BLACK;

					// Desenhar a linha
					DrawLine3D(start, end, lineColor);

					// Atualizar a última camada renderizada
					lastZ = Math.Max(start.Y, end.Y);
				}
			}

			EndMode3D();
		}
	}
}