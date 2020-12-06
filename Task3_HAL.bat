avr-gcc -Os -Wall -DF_CPU=16000000UL -mmcu=atmega328p  Task3_HAL.c src/hal.c src/in.c src/vm.c src/_cout.c src/out.c src/_xtoa.c src/vmstack.c -o Task3_HAL.o
pause
avr-objcopy -O ihex -j .text -j .data Task3_HAL.o  Task3_HAL.hex
avrdude -c arduino -p atmega328p -b 57600 -P COM3 -D -Uflash:w:Task3_HAL.hex:i
pause
