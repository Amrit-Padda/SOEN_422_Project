
#include "hal.h"
#include <avr/io.h>



u32 hal_IOReg_Read (u32 ioreg){
    volatile uint8_t * pin = ioreg;
    return *pin ;
}


void hal_IOReg_Write(u32 ioreg, u32 value){
    volatile uint8_t * pin = ioreg;
    *pin = value ;
}

    
   