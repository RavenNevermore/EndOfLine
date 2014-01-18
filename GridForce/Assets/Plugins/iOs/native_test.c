
#include <stdio.h>

void testFunction() {
    printf("Hello Out There!");
    UnitySendMessage("Cube", "setMyTexty", "I come from C");
}

