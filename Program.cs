/* XNAFileDialog Example Program
 * Written by Ethan "flibitijibibo" Lee
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

class Program : Game
{
	private const string startTest = "/home/flibitijibibo/.local/share/TowerFall/";

	private SpriteBatch spriteBatch;

	private Program() : base()
	{
		GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
		gdm.PreparingDeviceSettings += PrepareDeviceSettings;
		gdm.PreferredBackBufferWidth = 1280;
		gdm.PreferredBackBufferHeight = 720;
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		spriteBatch.Dispose();
		spriteBatch = null;
	}

	private bool wasDown = false;
	protected override void Update(GameTime gameTime)
	{
		KeyboardState state = Keyboard.GetState();
		if (state.IsKeyDown(Keys.S))
		{
			if (!wasDown)
			{
				wasDown = true;
				IsMouseVisible = true;
				XNAFileDialog.GraphicsDevice = GraphicsDevice;
				XNAFileDialog.StartDirectory = startTest;
				if (XNAFileDialog.ShowDialogSynchronous("Choose a .txt file!", "startFile.txt"))
				{
					System.Console.WriteLine(XNAFileDialog.Path);
				}
			}
		}
		else if (wasDown)
		{
			wasDown = false;
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(ClearOptions.Target, Vector4.Zero, 1.0f, 0);
		spriteBatch.Begin();
		// Some nonsense I'm sure...
		spriteBatch.End();
		base.Draw(gameTime);
	}

	private void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
		e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
	}

	private void SaveFile(string path)
	{
		if (path == null)
		{
			Console.WriteLine("User cancelled save!");
			return;
		}
		Console.WriteLine("User saved to " + path);
	}

	private void LoadFile(string path)
	{
		if (path == null)
		{
			Console.WriteLine("User cancelled load!");
			return;
		}
		Console.WriteLine("User loaded from " + path);
	}

	private static void Main(string[] args)
	{
		using (Program program = new Program())
		{
			program.Run();
		}
	}
}
