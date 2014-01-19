#include <stdio.h>
#include <string.h>

char* string_copy (const char* string) {
    if (string == NULL) return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

void testFunction() {
    printf("Hello Out There!");
    UnitySendMessage("Cube", "setMyTexty", "I come from C");
}

void other_test(char* text){
    printf("I got text: %s\n", string_copy(text));

}

