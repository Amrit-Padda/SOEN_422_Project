/* hal.c -- Hardware Abstraction Layer implementation
//
// Copyright (C) 1985-2020 by Michel de Champlain
//
*/

#include "hal.h"
#include "out.h"
#include "in.h"

void
Hal_Init(void) {
    VMOut_Init(Out_GetFactory("")); // "" to save space, later should be "console".
    VMIn_Init();
#ifdef FullVersion
    Add other init subsystems here.
#endif
}
