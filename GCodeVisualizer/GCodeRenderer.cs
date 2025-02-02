using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;
using Color = ZeroElectric.Vinculum.Color;

namespace GCodeVisualizer
{
	public class GCodeRenderer
	{
		// Lista de pontos do G-code
		private List<Vector3> points;

		// Cor da última camada
		private Color lastLayerColor;

		public GCodeRenderer(List<Vector3> points, Color lastLayerColor)
		{
			this.points = points;
			this.lastLayerColor = lastLayerColor;
		}

		// Renderiza as camadas do G-code até a altura Z especificada
		public void Render(Camera3D camera, float targetZ)
		{
			BeginMode3D(camera);

			// Variável para armazenar a última camada renderizada
			float lastZ = -1;

			int i = 0;
			Vector3 lastPoint = points.FirstOrDefault();



			foreach (var point in points)
			{
				i++;
				if (point.Y < targetZ - 0.1f)
				{
					DrawLine3D(lastPoint, point, GRAY);
					lastPoint = point;

				}
				else if (point.Y < targetZ)
				{ 
					DrawLine3D(lastPoint, point, RED);
					lastPoint = point;
				}






			}



			EndMode3D();
		}
	}
}