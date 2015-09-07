#region License
/* XNAFileDialog - Portable File Dialog for XNA Games
 *
 * Copyright (c) 2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Asset Documentation
/* The dialog spritesheet and spritesheet atlas can be wherever you like in your
 * Content/ folder - you will specify the path when loading the assets.
 *
 * Note that the filenames must be the following for the assets:
 *
 * FileDialogTexture - Texture2D
 * FileDialogFont.xnb - SpriteFont
 * FileDialogSprites.txt - Raw text file
 *
 * - The texture can be any format supported by XNA/FNA.
 * - The font should be small enough to handle long path names, but large enough
 *   to be readable in a living room environment (i.e. Big Picture certified).
 * - The spritesheet info is exactly what you'd expect. Odds are, you can figure
 *   out how this works with one quick look inside.
 *
 * Feel free to modify the example assets to work with your game's art style.
 */

#region API Documentation
/* The XNAFileDialog API is entirely static:
 *
 * Call XNAFileDialog.LoadDialogAssets() in your Game.LoadContent method.
 * Call XNAFileDialog.UnloadDialogAssets() in your Game.UnloadContent method.
 *
 * Set CurrentDirectory to your game's save path, then DO NOT TOUCH IT AGAIN.
 * The dialog will be updating this to the user's current directory, so that we
 * can remember where the user actually wants to save/load their data.
 *
 * Set OnReceivedPath to whatever method you want to use the given path.
 * For example, you can have a Save(string) and Load(string) depending on
 * whether or not you want the given dialog to save to or load from the result.
 * Note that OnReceivedPath can be NULL if the user closes the window instead of
 * providing a path!
 *
 * Lastly, call Open and a dialog window will show up on the screen. It will be
 * using the Game.Update and Game.Draw methods, so unlike the FileDialog we are
 * NOT halting your program! The dialog essentially runs like a standalone
 * window running inside your program, so both your game AND the dialog can
 * receive input and draw to the screen. Prepare your interface accordingly!
 *
 * Once the user has provided a file path or closed the window, the dialog will
 * dispose itself and call OnReceivedPath, either with the given path or with
 * NULL. Aside from the DialogAssets, no content/component management is needed.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

public sealed class XNAFileDialog : DrawableGameComponent
{
	#region Public Static Variables

	public static string CurrentDirectory;
	public static event Action<string> OnReceivedPath;

	#endregion

	#region Private Static Variables

	private static Game game;
	private static SpriteBatch spriteBatch;
	private static Texture2D spriteSheet;
	private static SpriteFont spriteFont;
	private static XNAFileDialog instance;

	#endregion

	public static void LoadDialogAssets(Game game, string path)
	{
		XNAFileDialog.game = game;
		spriteBatch = new SpriteBatch(game.GraphicsDevice);
		spriteSheet = game.Content.Load<Texture2D>(Path.Combine(path, "FileDialogTexture"));
		spriteFont = game.Content.Load<SpriteFont>(Path.Combine(path, "FileDialogFont"));
		using (FileStream fileIn = File.OpenRead(Path.Combine(path, "FileDialogSprites.txt")))
		using (StreamReader reader = new StreamReader(fileIn))
		{
			for (
				string line = reader.ReadLine();
				line != null;
				line = reader.ReadLine()
			) {
				int separator = line.IndexOf(':');
				string parameter = line.Substring(0, separator);
				string value = line.Substring(separator + 1);
				System.Console.WriteLine(parameter + " " + value);
				// TODO: Map rectangles to widget components
			}
		}
	}

	public static void UnloadDialogAssets()
	{
		spriteBatch.Dispose();
		spriteSheet.Dispose();
		game = null;
	}

	public static void Open()
	{
		if (instance != null)
		{
			throw new InvalidOperationException("One dialog at a time!");
		}
		if (CurrentDirectory == null)
		{
			throw new ArgumentNullException("Need a starting directory!");
		}
		if (OnReceivedPath == null)
		{
			throw new ArgumentNullException("Need a callback for file paths!");
		}
		instance = new XNAFileDialog(game);
		game.Components.Add(instance);
	}

	#endregion

	#region FileDialog Instance

	private XNAFileDialog(Game game) : base(game) { }

	public override void Update(GameTime gameTime)
	{
		/* TODO: Input handling.
		 * OnReceivedPath is called when OK or X is selected.
		 * OK can only be called when a file is selected!
		 */
		string finalPath = Environment.GetEnvironmentVariable("Placeholder~");
		if (finalPath != null)
		{
			OnReceivedPath(finalPath);
			Dispose();
			game.Components.Remove(this);
			instance = null;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		spriteBatch.Begin();
		// TODO: Draw all widgets based on spritesheet info
		spriteBatch.Draw(spriteSheet, Vector2.Zero, Color.White);
		spriteBatch.DrawString(spriteFont, "Placeholder~", Vector2.Zero, Color.White);
		spriteBatch.End();
	}

	#endregion
}
