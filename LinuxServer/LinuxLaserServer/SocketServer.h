#pragma once
#include <vector>
#include <mutex>
#include <iostream>
#include <pthread.h>
//#include <unistd.h>
#include <memory>

#include"socket.h"
using namespace std;


#define DATA_LEN 16

class LaserServer
{
public:
	LaserServer(shared_ptr<int> x, shared_ptr<int> y, shared_ptr<mutex> mtx);
	~LaserServer();
	const int port = 1986;

	void start();
private:
	Socket listen_socket;
	Socket communicate_socket;
	pthread_t listen_thread;
	pthread_t communicate_thread;
	shared_ptr<mutex> _mtx;
	shared_ptr<int> _x, _y;

	static void *ListenThread(void *ptr);
	static void *CommunicateThread(void *ptr);

	static void  intToByte(int i, char *bytes, int size = 4);


};
