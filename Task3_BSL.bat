avr-gcc -Os -Wall -DF_CPU=16000000UL -mmcu=atmega328p  Task3_BSL.c src/_xtoa.c  -o Task3_BSL.o
pause
avr-objcopy -O ihex -j .text -j .data Task3_BSL.o  Task3_BSL.hex
avrdude -c arduino -p atmega328p -b 57600 -P COM3 -D -Uflash:w:Task3_BSL.hex:i
pause
