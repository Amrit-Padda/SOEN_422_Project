//
// Created by Roman on 20/11/2020.
//

#include "src/_cout.c"


int main(void) {
    Out_GetFactory("");

    COut_PutS("Test Out:<2\n");
    COut_PutS("Bools: [true|false|true|true]\n");
    COut_PutS("Ints:  [-1|-2147483648|2147483647|4294967295|FFFFFFFF]\n");

    COut_PutS("Bools: [");
    COut_PutB(-1); COut_PutC('|');
    COut_PutB(0);  COut_PutC('|');
    COut_PutB(1);  COut_PutC('|');
    COut_PutB(2);  COut_PutS("]\n");

    COut_PutS("Ints:  [");
    COut_PutI(0xFFFFFFFFL); COut_PutC('|');
    COut_PutI(0x80000000L); COut_PutC('|');
    COut_PutI(0x7FFFFFFFL); COut_PutC('|');
    COut_PutU(0xFFFFFFFFL); COut_PutC('|');
    COut_PutX(0xFFFFFFFFL);
    COut_PutS("]\n");
    return 0;
}