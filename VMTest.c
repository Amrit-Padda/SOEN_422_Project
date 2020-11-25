//
// Created by Roman on 25/11/2020.
//
#include <stdio.h>  /* for FILE   */
#include <string.h> /* for strtok */

#include "src/hal.h"
#include "src/out.h"
#include "src/vm.h"
#include "src/_stdtype.h"
int main(void){

    Hal_Init();

    u8 memory[] = { 0x91, 0xFF, 0x82, 0x00 };

    VMOut_PutS("Attempt\n");


    VM_Init(memory);
    VM_execute(memory);


    return 0;
}
