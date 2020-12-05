//
// Created by Roman on 02/12/2020.
//


#include "in.h"
#include <avr/io.h>


void VMIn_Init(void){
    UBRR0H = (103 >> 8) ;
    UBRR0L = 103 ;

    UCSR0B |= ( 1 << RXEN0 ); // Enable receive

    UCSR0C |= (1 << UCSZ01) | (1 << UCSZ00);//Set Frame


};

char VMIn_GetC() {
    while(!(UCSR0A & (1 << RXC0)));
    char input = UDR0;
    return input;
};