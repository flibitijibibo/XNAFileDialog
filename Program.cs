/* XNAFileDialog Example Program
 * Written by Ethan "flibitijibibo" Lee
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

class Program : Game
{
	private Program() : base()
	{
		GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
		gdm.PreparingDeviceSettings += PrepareDeviceSettings;
		gdm.PreferredBackBufferWidth = 1280;
		gdm.PreferredBackBufferHeight = 720;
	}

	protected override void LoadContent()
	{
		XNAFileDialog.LoadDialogAssets(this, "");
		XNAFileDialog.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
	}

	protected override void UnloadContent()
	{
		XNAFileDialog.UnloadDialogAssets();
	}

	protected override void Update(GameTime gameTime)
	{
		KeyboardState state = Keyboard.GetState();
		if (state.IsKeyDown(Keys.S))
		{
			XNAFileDialog.OnReceivedPath += SaveFile;
			XNAFileDialog.Open();
		}
		else if (state.IsKeyDown(Keys.L))
		{
			XNAFileDialog.OnReceivedPath += LoadFile;
			XNAFileDialog.Open();
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(ClearOptions.Target, Vector4.Zero, 1.0f, 0);
		base.Draw(gameTime);
	}

	private void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
		e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
	}

	private void SaveFile(string path)
	{
		XNAFileDialog.OnReceivedPath -= SaveFile;
		if (path == null)
		{
			Console.WriteLine("User cancelled save!");
			return;
		}
		Console.WriteLine("User saved to " + path);
	}

	private void LoadFile(string path)
	{
		XNAFileDialog.OnReceivedPath -= LoadFile;
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
