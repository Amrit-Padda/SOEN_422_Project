avr-gcc -Os -Wall -DF_CPU=16000000UL -mmcu=atmega328p  VMTest.c src/hal.c src/in.c src/vm.c src/_cout.c src/out.c src/_xtoa.c src/vmstack.c -o VMTest.o
pause
avr-objcopy -O ihex -j .text -j .data VMTest.o  VMTest.hex
avrdude -c arduino -p atmega328p -b 57600 -P COM3 -D -Uflash:w:VMTest.hex:i
pause
