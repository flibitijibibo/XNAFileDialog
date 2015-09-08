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
#endregion

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
 * Lastly, call Open and a dialog window will show up on the screen. It will be
 * using the Game.Update and Game.Draw methods, so unlike the FileDialog we are
 * NOT halting your program! The dialog essentially runs like a standalone
 * window running inside your program, so both your game AND the dialog can
 * receive input and draw to the screen. Prepare your interface accordingly!
 *
 * The receivedPath parameter is a callback for us to send the selected path.
 * For example, you can have a Save(string) and Load(string) depending on
 * whether or not you want the given dialog to save to or load from the result.
 * Note that the result can be NULL if the user closes the window instead of
 * providing a path!
 *
 * Once the user has provided a file path or closed the window, the dialog will
 * dispose itself and call receivedPath either with the given path or with NULL.
 * Aside from the DialogAssets, no content/component management is needed.
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

	#endregion

	#region Private Static Variables

	private static Game game;
	private static SpriteBatch spriteBatch;
	private static Texture2D spriteSheet;
	private static Rectangle[] spriteRects;
	private static SpriteFont spriteFont;
	private static XNAFileDialog instance;

	private static readonly string[] properties = new string[]
	{
		"OuterColor",
		"OuterBorderCorner",
		"OuterBorder",
		"InnerColor",
		"InnerBorderCorner",
		"InnerBorder",
		"Home",
		"File",
		"Folder",
		"Cancel",
		"Select"
	};

	#endregion

	#region Public Static Methods

	public static void LoadDialogAssets(Game game, string path)
	{
		XNAFileDialog.game = game;
		spriteBatch = new SpriteBatch(game.GraphicsDevice);
		spriteSheet = game.Content.Load<Texture2D>(Path.Combine(path, "FileDialogTexture"));
		spriteFont = game.Content.Load<SpriteFont>(Path.Combine(path, "FileDialogFont"));
		spriteRects = new Rectangle[properties.Length];
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
				string[] value = line.Substring(separator + 1).Split(',');
				Rectangle result = new Rectangle(
					int.Parse(value[0]),
					int.Parse(value[1]),
					int.Parse(value[2]),
					int.Parse(value[3])
				);
				for (int i = 0; i < properties.Length; i += 1)
				{
					if (parameter == properties[i])
					{
						spriteRects[i] = result;
						break;
					}
				}
			}
		}
	}

	public static void UnloadDialogAssets()
	{
		spriteBatch.Dispose();
		spriteSheet.Dispose();
		game = null;
	}

	public static void Open(Action<string> receivedPath)
	{
		if (instance != null)
		{
			throw new InvalidOperationException("One dialog at a time!");
		}
		if (CurrentDirectory == null)
		{
			throw new ArgumentNullException("Need a starting directory!");
		}
		if (receivedPath == null)
		{
			throw new ArgumentNullException("Need a callback for file paths!");
		}
		instance = new XNAFileDialog(game, receivedPath);
		game.Components.Add(instance);
	}

	#endregion

	#region FileDialog Instance

	private Action<string> fileAction;
	private string[] currentDirectories;
	private string[] currentFiles;
	private string[] currentPath;

	private XNAFileDialog(Game game, Action<string> action) : base(game)
	{
		fileAction = action;
		GenerateDirectoryInfo();
	}

	private void GenerateDirectoryInfo()
	{
		currentDirectories = Directory.GetDirectories(CurrentDirectory);
		currentFiles = Directory.GetFiles(CurrentDirectory);
		currentPath = CurrentDirectory.Split(Path.DirectorySeparatorChar);
	}

	public override void Update(GameTime gameTime)
	{
		/* TODO: Input handling.
		 * OnReceivedPath is called when OK or X is selected.
		 * OK can only be called when a file is selected!
		 */
		string finalPath = Environment.GetEnvironmentVariable("Placeholder~");
		if (finalPath != null)
		{
			fileAction(finalPath);
			Dispose();
			game.Components.Remove(this);
			instance = null;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		// TODO: Make these variables not dumb
		int totalWidth = 800;
		int totalHeight = 600;
		int startPosX = 0;
		int startPosY = 0;

		// IT BEGINS
		spriteBatch.Begin(
			SpriteSortMode.Deferred,
			BlendState.AlphaBlend,
			SamplerState.PointClamp,
			DepthStencilState.None,
			RasterizerState.CullNone
		);

		// Window Color
		Rectangle borderRect = new Rectangle(
			startPosX + spriteRects[2].Width,
			startPosY + spriteRects[2].Height,
			totalWidth - (spriteRects[2].Width * 2),
			totalHeight - (spriteRects[2].Height * 2)
		);
		spriteBatch.Draw(
			spriteSheet,
			borderRect,
			spriteRects[0],
			Color.White,
			0.0f,
			Vector2.Zero,
			SpriteEffects.None,
			0.0f
		);
		// Top Left Border Corner
		Vector2 curPos = new Vector2(startPosX, startPosY);
		spriteBatch.Draw(
			spriteSheet,
			curPos,
			spriteRects[1],
			Color.White,
			0.0f,
			Vector2.Zero,
			Vector2.One,
			SpriteEffects.None,
			0.0f
		);
		// Top Right Border Corner
		curPos.X = totalWidth - spriteRects[1].Width;
		spriteBatch.Draw(
			spriteSheet,
			curPos,
			spriteRects[1],
			Color.White,
			0.0f,
			Vector2.Zero,
			Vector2.One,
			SpriteEffects.FlipHorizontally,
			0.0f
		);
		// Bottom Right Border Corner
		curPos.Y = totalHeight - spriteRects[1].Height;
		spriteBatch.Draw(
			spriteSheet,
			curPos,
			spriteRects[1],
			Color.White,
			0.0f,
			Vector2.Zero,
			Vector2.One,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically,
			0.0f
		);
		// Bottom Left Border Corner
		curPos.X -= totalWidth - spriteRects[1].Width;
		spriteBatch.Draw(
			spriteSheet,
			curPos,
			spriteRects[1],
			Color.White,
			0.0f,
			Vector2.Zero,
			Vector2.One,
			SpriteEffects.FlipVertically,
			0.0f
		);
		// Top Border
		borderRect.X = startPosX + spriteRects[1].Width;
		borderRect.Y = startPosY;
		borderRect.Width = totalWidth - (spriteRects[1].Width * 2);
		borderRect.Height = spriteRects[2].Height;
		spriteBatch.Draw(
			spriteSheet,
			borderRect,
			spriteRects[2],
			Color.White,
			0.0f,
			Vector2.Zero,
			SpriteEffects.None,
			0.0f
		);
		// Bottom Border
		borderRect.Y = totalHeight - spriteRects[2].Height;
		spriteBatch.Draw(
			spriteSheet,
			borderRect,
			spriteRects[2],
			Color.White,
			0.0f,
			Vector2.Zero,
			SpriteEffects.None,
			0.0f
		);
		// Left Border
		borderRect.X = startPosY + spriteRects[1].Height;
		borderRect.Y = startPosX + spriteRects[1].Width;
		borderRect.Width = totalHeight - (spriteRects[1].Height * 2);
		borderRect.Height = spriteRects[2].Width;
		spriteBatch.Draw(
			spriteSheet,
			borderRect,
			spriteRects[2],
			Color.White,
			MathHelper.PiOver2,
			Vector2.Zero,
			SpriteEffects.None,
			0.0f
		);
		// Right Border
		borderRect.X = totalWidth;
		spriteBatch.Draw(
			spriteSheet,
			borderRect,
			spriteRects[2],
			Color.White,
			MathHelper.PiOver2,
			Vector2.Zero,
			SpriteEffects.None,
			0.0f
		);

		// HELLO PLACEHOLDER HOW ARE YOU TODAY
		curPos.X = 0;
		curPos.Y = spriteBatch.GraphicsDevice.PresentationParameters.BackBufferHeight - 64;
		spriteBatch.DrawString(
			spriteFont,
			currentDirectories.Length + " " + currentFiles[0] + " " + currentPath[1],
			curPos,
			Color.White
		);

		// We out.
		spriteBatch.End();
	}

	#endregion
}
