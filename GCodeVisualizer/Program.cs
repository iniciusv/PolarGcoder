using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

namespace GCodeVisualizer
{
	class Program
	{
		static void Main(string[] args)
		{
			// Caminho do arquivo G-code transformado
			string gcodeFilePath = @"C:\Users\vinic\Downloads\3d\CFFFP_20mm-box_rotational.gcode";

			// Ler o arquivo G-code
			var gcodeLines = File.ReadAllLines(gcodeFilePath);

			// Inicializar a janela Raylib
			const int screenWidth = 1800;
			const int screenHeight = 1200;
			InitWindow(screenWidth, screenHeight, "G-code Visualizer 3D");
			SetTargetFPS(60); // Definir FPS máximo para 60

			// Definir a câmera 3D
			var camera = new Camera3D();
			camera.position = new Vector3(10.0f, 10.0f, 10.0f); // Posição da câmera
			camera.target = new Vector3(0.0f, 0.0f, 0.0f);      // Ponto para onde a câmera está olhando
			camera.up = new Vector3(0.0f, 1.0f, 0.0f);          // Vetor "para cima" da câmera
			camera.fovy = 45.0f;                               // Campo de visão
			camera.projection = 1;                             // Projeção em perspectiva

			// Lista para armazenar os pontos do G-code
			var points = new List<Vector3>();

			// Variáveis para rastrear a camada atual
			float currentZ = 0;

			// Processar o G-code e extrair os pontos
			foreach (var line in gcodeLines)
			{
				if (line.StartsWith("G0") || line.StartsWith("G1"))
				{
					var parts = line.Split(' ');
					float x = 0, y = 0, z = currentZ; // Usar o valor atual de Z por padrão

					foreach (var part in parts)
					{
						if (part.StartsWith("X"))
						{
							x = float.Parse(part.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
						}
						else if (part.StartsWith("Y"))
						{
							y = float.Parse(part.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
						}
						else if (part.StartsWith("Z"))
						{
							z = float.Parse(part.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
							currentZ = z; // Atualizar a altura Z atual
						}
					}

					// Adicionar o ponto à lista (convertendo Y para rotação em radianos)
					points.Add(new Vector3(x, currentZ, y * 0.36f)); // Z é a altura, Y é a rotação
				}
			}

			// Criar o renderizador de G-code
			var gcodeRenderer = new GCodeRenderer(points, RED); // Última camada em vermelho

			// Configurações de controle da câmera
			float cameraMoveSpeed = 0.4f;
			float cameraRotationSpeed = 0.2f;

			// Valor inicial de targetZ
			float targetZ = 0;

			// Loop principal da janela
			while (!WindowShouldClose())
			{
				// Controles da câmera com o mouse
				if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
				{
					// Rotacionar a câmera com o mouse
					Vector2 delta = GetMouseDelta();
					camera.position.X -= delta.X * cameraRotationSpeed;
					camera.position.Y += delta.Y * cameraRotationSpeed;
				}

				if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
				{
					// Mover a câmera com o mouse
					Vector2 delta = GetMouseDelta();
					camera.target.X -= delta.X * cameraMoveSpeed;
					camera.target.Y += delta.Y * cameraMoveSpeed;
				}

				// Zoom com a roda do mouse
				float wheel = GetMouseWheelMove();
				camera.position.Z += wheel * cameraMoveSpeed;

				// Controle do targetZ com as teclas + e -
				if (IsKeyPressed(KeyboardKey.KEY_EQUAL) || IsKeyPressed(KeyboardKey.KEY_KP_ADD))
				{
					// Aumentar targetZ
					targetZ += 0.15f; // Ajuste o incremento conforme necessário
					targetZ = Math.Min(targetZ, currentZ); // Não permitir que targetZ ultrapasse o valor máximo de Z
				}
				if (IsKeyPressed(KeyboardKey.KEY_MINUS) || IsKeyPressed(KeyboardKey.KEY_KP_SUBTRACT))
				{
					// Diminuir targetZ
					targetZ -= 0.15f; // Ajuste o decremento conforme necessário
					targetZ = Math.Max(targetZ, 0); // Não permitir que targetZ seja menor que 0
				}

				// Iniciar o desenho
				BeginDrawing();
				ClearBackground(RAYWHITE);

				// Renderizar o G-code até a altura targetZ
				gcodeRenderer.Render(camera, targetZ);

				// Desenhar instruções na tela
				DrawText("Botão direito: Rotacionar câmera", 10, 10, 20, DARKGRAY);
				DrawText("Botão esquerdo: Mover câmera", 10, 30, 20, DARKGRAY);
				DrawText("Roda do mouse: Zoom", 10, 50, 20, DARKGRAY);
				DrawText($"Teclas + e -: Ajustar altura (Z atual: {targetZ:F2})", 10, 70, 20, DARKGRAY);

				EndDrawing();
			}

			// Fechar a janela Raylib
			CloseWindow();
		}
	}
}