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

#include "XNAFileDialog.h"

#include "imgui.h"
#include "imguifilesystem.h"

#include <stdlib.h>

/* FIXME: XNA4...? -flibit */
#define PLATFORM_FNA

/* BEGIN INTERNAL FUNCTION/VARIABLE DECLARATIONS */

XNAFileDialog_ReceivePath receive;
XNAFileDialog_BufferData buffer;
XNAFileDialog_DrawPrimitives render;
ImDrawVert *vertexArray = NULL;
ImDrawIdx *indexArray = NULL;
size_t vertexArrayLen = 0;
size_t indexArrayLen = 0;
void XNAFileDialog_RenderDrawLists(ImDrawData *draw_data);
void XNAFileDialog_SetClipboardText(const char *text);
const char *XNAFileDialog_GetClipboardText();

/* END INTERNAL FUNCTION DECLARATIONS */

/* BEGIN PLATFORM HEADERS/DECLARATIONS */

#ifdef PLATFORM_FNA
#include <SDL2/SDL.h>
int mouseWheel = 0;
int XNAFileDialog_EventFilter(void *whatever, SDL_Event *event)
{
	ImGuiIO& io = ImGui::GetIO();
	if (event->type == SDL_MOUSEWHEEL)
	{
		if (event->wheel.y > 0)
		{
			mouseWheel = 1;
		}
		else if (event->wheel.y < 0)
		{
			mouseWheel = -1;
		}
		return 0;
	}
	if (event->type == SDL_TEXTINPUT)
	{
		io.AddInputCharactersUTF8(event->text.text);
		return 0;
	}
	if (event->type == SDL_KEYDOWN || event->type == SDL_KEYUP)
	{
		int key = event->key.keysym.sym & ~SDLK_SCANCODE_MASK;
		io.KeysDown[key] = (event->type == SDL_KEYDOWN);
		io.KeyShift = (SDL_GetModState() & KMOD_SHIFT) != 0;
		io.KeyCtrl = (SDL_GetModState() & KMOD_CTRL) != 0;
		io.KeyAlt = (SDL_GetModState() & KMOD_ALT) != 0;
		return 0;
	}
	return 1;
}
#else
#error Win32/XNA4 backend? -flibit
#endif

/* BEGIN PLATFORM HEADERS/DECLARATIONS */

/* BEGIN PUBLIC API IMPLEMENTATION */

void XNAFileDialog_Init(
	XNAFileDialog_CreateTexture createTexture,
	XNAFileDialog_BufferData bufferData,
	XNAFileDialog_DrawPrimitives drawPrimitives,
	XNAFileDialog_ReceivePath receivePath
) {
	/* Font Texture */
	ImGuiIO& io = ImGui::GetIO();
	unsigned char *pixels;
	int width, height;
	io.Fonts->GetTexDataAsAlpha8(&pixels, &width, &height);
	io.Fonts->TexID = createTexture(pixels, width, height);
	io.Fonts->ClearInputData();
	io.Fonts->ClearTexData();

	/* Dialog Callbacks */
	receive = receivePath;

	/* Render Callbacks */
	buffer = bufferData;
	render = drawPrimitives;
	io.RenderDrawListsFn = XNAFileDialog_RenderDrawLists;

	/* Clipboard Callbacks */
	io.SetClipboardTextFn = XNAFileDialog_SetClipboardText;
	io.GetClipboardTextFn = XNAFileDialog_GetClipboardText;

#ifdef PLATFORM_FNA
	/* Key Symbol Mappings */
	io.KeyMap[ImGuiKey_Tab] = SDLK_TAB;
	io.KeyMap[ImGuiKey_LeftArrow] = SDL_SCANCODE_LEFT;
	io.KeyMap[ImGuiKey_RightArrow] = SDL_SCANCODE_RIGHT;
	io.KeyMap[ImGuiKey_UpArrow] = SDL_SCANCODE_UP;
	io.KeyMap[ImGuiKey_DownArrow] = SDL_SCANCODE_DOWN;
	io.KeyMap[ImGuiKey_PageUp] = SDL_SCANCODE_PAGEUP;
	io.KeyMap[ImGuiKey_PageDown] = SDL_SCANCODE_PAGEDOWN;
	io.KeyMap[ImGuiKey_Home] = SDL_SCANCODE_HOME;
	io.KeyMap[ImGuiKey_End] = SDL_SCANCODE_END;
	io.KeyMap[ImGuiKey_Delete] = SDLK_DELETE;
	io.KeyMap[ImGuiKey_Backspace] = SDLK_BACKSPACE;
	io.KeyMap[ImGuiKey_Enter] = SDLK_RETURN;
	io.KeyMap[ImGuiKey_Escape] = SDLK_ESCAPE;
	io.KeyMap[ImGuiKey_A] = SDLK_a;
	io.KeyMap[ImGuiKey_C] = SDLK_c;
	io.KeyMap[ImGuiKey_V] = SDLK_v;
	io.KeyMap[ImGuiKey_X] = SDLK_x;
	io.KeyMap[ImGuiKey_Y] = SDLK_y;
	io.KeyMap[ImGuiKey_Z] = SDLK_z;

	/* SDL Events */
	SDL_SetEventFilter(XNAFileDialog_EventFilter, NULL);
#else
#error Win32/XNA4 backend? -flibit
#endif
}

void XNAFileDialog_Shutdown()
{
#ifdef PLATFORM_FNA
	SDL_SetEventFilter(NULL, NULL);
#else
#error Win32/XNA4 backend? -flibit
#endif
	ImGui::Shutdown();
}

void XNAFileDialog_Update()
{
	ImGuiIO& io = ImGui::GetIO();

#ifdef PLATFORM_FNA
	/* Update Timestep
	 * FIXME: Use GameTime instead? -flibit
	 */
	static double lastTimeS = 0.0f;
	Uint32 ticks = SDL_GetTicks();
	double timeS = ticks / 1000.0;
	io.DeltaTime = (lastTimeS > 0.0) ?
		(float) (timeS - lastTimeS) :
		(1.0f / 60.0f);
	lastTimeS = timeS;

	/* Update Mouse State
	 * FIXME: Faux-backbuffer scaling? -flibit
	 */
	int mx, my;
	Uint32 state = SDL_GetMouseState(&mx, &my);
	io.MousePos = ImVec2((float) mx, (float) my);
	io.MouseDown[0] = (state & SDL_BUTTON(SDL_BUTTON_LEFT)) != 0;
	io.MouseDown[1] = (state & SDL_BUTTON(SDL_BUTTON_RIGHT)) != 0;
	io.MouseDown[2] = (state & SDL_BUTTON(SDL_BUTTON_MIDDLE)) != 0;
	io.MouseWheel = (float) mouseWheel;
	mouseWheel = 0;
#else
#error Win32/XNA4 backend? -flibit
#endif

	/* TODO: FileSystemDialog */
	static ImGuiFs::Dialog dialog;
	dialog.chooseFileDialog(true);
	if (strlen(dialog.getChosenPath()) > 0)
	{
		receive(dialog.getChosenPath());
	}

	ImGui::NewFrame();
}

void XNAFileDialog_Render()
{
	ImGui::Render();
}

/* END PUBLIC API IMPLEMENTATION */

/* BEGIN INTERNAL FUNCTIONS */

void XNAFileDialog_RenderDrawLists(ImDrawData *draw_data)
{
	ImDrawVert *curVertDest;
	ImDrawIdx *curIndexDest;
	int curVertOffset;
	int curIndexOffset;
	ImDrawList *cmdList;
	ImDrawCmd *cmd;
	int i, j;

	/* Check vertex/index array sizes, alloc if needed */
	if (vertexArrayLen < draw_data->TotalVtxCount)
	{
		vertexArrayLen = draw_data->TotalVtxCount;
		if (vertexArray != NULL)
		{
			free(vertexArray);
		}
		vertexArray = (ImDrawVert*) malloc(sizeof(ImDrawVert) * vertexArrayLen);
	}
	if (indexArrayLen < draw_data->TotalIdxCount)
	{
		indexArrayLen = draw_data->TotalIdxCount;
		if (indexArray != NULL)
		{
			free(indexArray);
		}
		indexArray = (ImDrawIdx*) malloc(sizeof(ImDrawIdx) * indexArrayLen);
	}

	/* Copy the command lists into a single vertex/index buffer pair */
	curVertDest = vertexArray;
	curIndexDest = indexArray;
	for (i = 0; i < draw_data->CmdListsCount; i += 1)
	{
		cmdList = draw_data->CmdLists[i];
		memcpy(
			curVertDest,
			&cmdList->VtxBuffer[0],
			cmdList->VtxBuffer.size() * sizeof(ImDrawVert)
		);
		memcpy(
			curIndexDest,
			&cmdList->IdxBuffer[0],
			cmdList->IdxBuffer.size() * sizeof(ImDrawIdx)
		);
		curVertDest += cmdList->VtxBuffer.size();
		curIndexDest += cmdList->IdxBuffer.size();
	}
	buffer(
		vertexArray,
		sizeof(ImDrawVert) * vertexArrayLen,
		indexArray,
		sizeof(ImDrawIdx) * indexArrayLen
	);

	/* Draw each command list */
	curVertOffset = 0;
	curIndexOffset = 0;
	for (i = 0; i < draw_data->CmdListsCount; i += 1)
	{
		cmdList = draw_data->CmdLists[i];
		for (j = 0; j < cmdList->CmdBuffer.size(); j += 1)
		{
			cmd = &cmdList->CmdBuffer[j];
			render(
				cmd->ClipRect.x,
				cmd->ClipRect.y,
				cmd->ClipRect.z,
				cmd->ClipRect.w,
				cmd->ElemCount,
				curIndexOffset,
				curVertOffset
			);
			curIndexOffset += cmd->ElemCount;
		}
		curVertOffset += cmdList->VtxBuffer.size();
	}
}

const char *XNAFileDialog_GetClipboardText()
{
#ifdef PLATFORM_FNA
	return SDL_GetClipboardText();
#else
#error Win32/XNA4 backend? -flibit
#endif
}

void XNAFileDialog_SetClipboardText(const char *text)
{
#ifdef PLATFORM_FNA
	SDL_SetClipboardText(text);
#else
#error Win32/XNA4 backend? -flibit
#endif
}

/* END INTERNAL FUNCTIONS */
