//
// Created by Roman on 25/11/2020.
//
#include <stdio.h>  /* for FILE   */
#include <string.h> /* for strtok */
#include <stdlib.h>

#include "src/hal.h"
#include "src/out.h"
#include "src/vm.h"
#include "src/_stdtype.h"
#include "src/in.h"

#define CommandReturn_Success 0x40
#define CommandReturn_UnknownCmd 0x41
#define CommandReturn_InvalidCmd 0x42
#define CommandReturn_InvalidAddr 0x43
#define CommandReturn_MemoryFail 0x44


volatile u8* memory;
volatile int memLen = 0;

volatile u8 status = 0x40;

void sendAck(){
    VMOut_PutC(0xCC);
    VMOut_PutC(0);
}
void sendNak(){
    VMOut_PutC(0x33);
    VMOut_PutC(0);
}
void sendStatus(u8 status){
    VMOut_PutC(0x3);
    VMOut_PutC(status);
    VMOut_PutC(0);
}
void Download(u8 a, u8 b, u8 c, u8 d){
    int size = d + (c << 8) + (b << 16) + (a << 24);
    memory = (u8*)realloc(memory, size);        //todo: consider, is a larger than "needed" memory size detrimental? maybe use static memory size?
}
void run(){
    VM_Init(memory);
    VM_execute(memory);
}
void reset(){
    memory = realloc(memory, 0);        //todo: consider, is simply zeroing or voiding all the elements in the memory not enough?
    memLen = 0;
}


void receiveData(u8* data){
    int i;
    for(i = 3; i < data[0] && memLen < sizeof(memory);i++){
        memory[memLen++] = data[i];
    }
}

char * loadPacket(u8 * packet){
    while(true) {
        packet[0] = VMIn_GetC();
        int length = packet[0];
        int checksum = packet[0];
        packet = (u8 *) realloc(packet, length);
        int i;
        for (i = 1; i < length; i++) {
            packet[i] = VMIn_GetC();
            if (i > 1) {
                checksum += packet[i];
            }
        }
        if (checksum == packet[1]) {
            sendAck();
            return packet;
        } else {
            sendNak();
        }
    }
}

void LoaderLoop(){
    while(true) {
        u8 *packet = (u8 *) realloc(packet, 3);
        packet = loadPacket(packet);                //todo test the packet loader (receive a packet, and then echo it back, so that format can be asserted)
        u8 command = packet[2];
        switch (command) {
            case 0x20://Ping                        //todo test ping
                break;
            case 0x23://GetStatus                   //todo test GetStatus
                sendStatus(status);
                break;
            case 0x21://Download                    //todo test Download
                Download(packet[3], packet[4], packet[5], packet[6]);//todo change Download to take the packet or an int as an argument, not indecies that's retarded
                break;
            case 0x24://SendData                    //todo test SendData
                receiveData(packet);
                break;
            case 0x22://Run                         //todo test Run
                run();
                break;
            case 0x25://reset                       // todo test reset
                reset();
                break;
        }
    }
}

int main(void){
    Hal_Init();

    while(true){
        char x = VMIn_GetC();

        VMOut_PutC(x);
        VMOut_PutC('\n');
    }
    //todo test loaderloop
    LoaderLoop();

    return 0;
}
