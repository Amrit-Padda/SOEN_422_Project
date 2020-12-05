
#include "hal.h"
#include <avr/io.h>
#include <avr/interrupt.h>



void hal_Interrupt_Disable(void){
    cli();
}
void hal_Interrupt_Enable(void){
    DDRD &= ~(1 << DDD2);     // Clear PD2 

    PORTD |= (1 << PORTD2);    // set PD2 as Pull-up
    
    EICRA |= (1 << ISC00);    // set INT0 to trigger on ANY logic change
    EIMSK |= (1 << INT0);     // Turns on INT0

    sei(); 

}
u16 hal_Interrupt_SaveAndDisable(void){

}
void hal_Interrupt_Restore(u16 flags){

}

