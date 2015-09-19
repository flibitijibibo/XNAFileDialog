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

#ifndef XNAFILEDIALOG_H
#define XNAFILEDIALOG_H

#ifdef __cplusplus
extern "C" {
#endif

#ifdef _WIN32
#define EXPORTFN 
#define DELEGATECALL 
#else
#define EXPORTFN
#define DELEGATECALL
#endif

typedef void* (DELEGATECALL *XNAFileDialog_CreateTexture)(
	unsigned char*, int, int
);

typedef void (DELEGATECALL *XNAFileDialog_BufferData)(void*, int, void*, int);

typedef void (DELEGATECALL *XNAFileDialog_DrawPrimitives)(
	int, int, int, int, int, int, int, int
);

typedef void (DELEGATECALL *XNAFileDialog_ReceivePath)(const char*);

EXPORTFN void XNAFileDialog_Init(
	XNAFileDialog_CreateTexture createTexture,
	XNAFileDialog_BufferData bufferData,
	XNAFileDialog_DrawPrimitives drawPrimitives,
	XNAFileDialog_ReceivePath receivePath
);

EXPORTFN void XNAFileDialog_Shutdown();

EXPORTFN void XNAFileDialog_Update();

EXPORTFN void XNAFileDialog_Render();

#undef EXPORTFN
#undef DELEGATECALL

#ifdef __cplusplus
}
#endif

#endif /* XNAFILEDIALOG_H */
