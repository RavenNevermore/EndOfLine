#ifndef	_UDP_BROADCAST_
#define	_UDP_BROADCAST_

#define _UDP_DEBUG_

#define FALSE 0
#define TRUE 1

#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/time.h>

static int beacon_active = FALSE;

int bind_socket(u_int32_t ip, int port);

void udp_callout_from_socket(int socket,
                             int broadcast_port,
                             const char* call_token){



    struct sockaddr_in broadcast_addr;
    broadcast_addr.sin_family = AF_INET;
    broadcast_addr.sin_addr.s_addr = htonl(INADDR_BROADCAST);
    broadcast_addr.sin_port = htons(broadcast_port);
    socklen_t addrlen = sizeof(broadcast_addr);

    //loop: 1. broadcast callout. 2. listen for response for 3 sec. 3. repeat
#ifdef _UDP_DEBUG_
    printf("Calling all beacons with %s.\n", call_token);
#endif
    sendto(socket, call_token, strlen(call_token), 0,
           (struct sockaddr *)&broadcast_addr, addrlen);
#ifdef _UDP_DEBUG_
    printf("Callout complete.\n");
    fflush(stdout);
#endif
}

/**
 * @return the socket to listen for callbacks
 *         or -1 if there was an error, creating the socket
 */
int udp_create_sailor_socket(int port/*, int broadcast_port, const char* call_token*/){
    int s = bind_socket(INADDR_ANY, port);
    if (0 > s) return -1;

    int val = 1;
    struct timeval timeout;
    timeout.tv_sec = 1;
    timeout.tv_usec = 0;
    setsockopt(s, SOL_SOCKET, SO_BROADCAST, &val, sizeof(val));
    setsockopt(s, SOL_SOCKET, SO_RCVTIMEO, (char *)&timeout, sizeof(timeout));

    //udp_callout_from_socket(s, broadcast_port, call_token);
    return s;
}

int udp_next_response(int socket, const char* beacon_token, char* responder_ip, int buffer_size){
    struct sockaddr_in caller_address;
    socklen_t caller_addrlen = sizeof(caller_address);

    char buffer[ 255 ];
    long msgSize = recvfrom(socket,
                            buffer,
                            255,//strlen(beacon_token),
                            0,
                            (struct sockaddr*)&caller_address,
                            &caller_addrlen);

    if (0 < msgSize){
        char caller_ip[INET_ADDRSTRLEN];
        inet_ntop(AF_INET,
                  &(caller_address.sin_addr),
                  caller_ip,
                  INET_ADDRSTRLEN);

        if (strncmp(beacon_token, buffer, strlen(beacon_token))){
#ifdef _UDP_DEBUG_
            printf("Ignoring responding server %s.\n", caller_ip);
            printf("Response was '%s' but expected '%s'\n", buffer, beacon_token);
            fflush(stdout);
#endif
            return 0;
        } else {
            int token_length = strlen(beacon_token);
            if (msgSize > token_length){
#ifdef _UDP_DEBUG_
                printf("Found additional message.\n");
                fflush(stdout);
#endif
                int add_length = msgSize - token_length;
                char* call_msg = malloc(sizeof(char)*add_length+1);
                strncpy(call_msg, buffer + token_length, add_length);
                call_msg[add_length] = '\0';
#ifdef _UDP_DEBUG_
                printf("Psssst: '%s'\n", call_msg);
                fflush(stdout);
#endif
                char answer[INET_ADDRSTRLEN+strlen(call_msg)];
                strcpy(answer, caller_ip);
                strcat(answer, call_msg);

                strcpy(responder_ip, answer);
            } else {
                strcpy(responder_ip, caller_ip);
            }
            buffer_size = strlen(responder_ip);
#ifdef _UDP_DEBUG_
            printf("Returning: '%s'\n", responder_ip);
            fflush(stdout);
#endif
            return 1;
        }
    } else {
        return 0;
    }
}


void close_sailors_ears(int socket){
    shutdown(socket, 2);
}

/**
 * starts a UDP beacon, that can answer incomming broadcast calls.
 * Note: There can ever only be one beacon active, per machine.
 */
void udp_create_beacon(int port, const char* listen_for_token,
                       const char* answer_with_token){
    if (beacon_active){
#ifdef _UDP_DEBUG_
        printf("Beacon is already active!\n");
        fflush(stdout);
#endif
        return;
    }

    beacon_active = TRUE;

    int s = bind_socket(INADDR_ANY, port);
    if (0 > s) return;

    struct sockaddr_in callerAddress;
    socklen_t addrlen = sizeof(callerAddress);


    while (beacon_active){
        unsigned char buffer[ strlen(listen_for_token) ];
#ifdef _UDP_DEBUG_
        printf("Listening on Port %d\n", port);
        fflush(stdout);
#endif
        long msgSize = recvfrom(s,
                                buffer,
                                strlen(listen_for_token),
                                0,
                                (struct sockaddr*)&callerAddress,
                                &addrlen);
#ifdef _UDP_DEBUG_
        printf("received %d bytes\n", (int)msgSize);
        fflush(stdout);
#endif
        if (msgSize > 0) {
            buffer[msgSize] = 0;

            char callerAddrStr[INET_ADDRSTRLEN];
            inet_ntop(AF_INET,
                      &(callerAddress.sin_addr),
                      callerAddrStr,
                      INET_ADDRSTRLEN);
#ifdef _UDP_DEBUG_
            printf("received message from %s: \"%s\"\n",
                   callerAddrStr,
                   buffer);
            printf("Will answer: '%s'\n", answer_with_token);
            fflush(stdout);
#endif

            sendto(s, answer_with_token, strlen(answer_with_token), 0,
                   (struct sockaddr *)&callerAddress, addrlen);
            
        }
    }

    shutdown(s, 2);

#ifdef _UDP_DEBUG_
    printf("Beacon shut down.\n");
    fflush(stdout);
#endif
}

void udp_destroy_beacon(){
    beacon_active = FALSE;

}

/* * * * * * * * * * * * * * * * * */

int bind_socket(u_int32_t ip, int port){
    int s = socket(PF_INET, SOCK_DGRAM, 0);

    struct sockaddr_in address;
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = htonl(ip);
    address.sin_port = htons(port);

    if (bind(s, (struct sockaddr*)&address, sizeof(address))){
#ifdef _UDP_DEBUG_
        printf("\nERROR (%d: %s) WHILE BINDING SOCKET!\n",
               errno, strerror(errno));
#endif
        return -1;
    }
    return s;
}


#endif
