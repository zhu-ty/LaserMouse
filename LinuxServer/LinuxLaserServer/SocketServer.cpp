#include "SocketServer.h"



LaserServer::LaserServer(shared_ptr<int> x, shared_ptr<int> y, shared_ptr<mutex> mtx)
{
	_x = x;
	_y = y;
	_mtx = mtx;
}

LaserServer::~LaserServer()
{
}

void LaserServer::start()
{

	if(!listen_socket.create())
    {
        cout<<"failed create socket"<<endl;
        return;
    }
	if(!listen_socket.bind(port))
    {
        cout<<"failed bind port"<<endl;
        return;
    }
	cout << "Server Created!" << endl << "Now,Listening." << endl;
	//listen_thread = new thread(&LaserServer::ListenThread, this);
	//listen_thread->detach();
	pthread_create(&listen_thread,NULL,ListenThread,this);
	return;
}

void *LaserServer::ListenThread(void *ptr)
{
    LaserServer *ls = (LaserServer*)ptr;
    ls->listen_socket.listen();
	while (1)
	{
        ls->listen_socket.accept(ls->communicate_socket);
		pthread_create(&(ls->communicate_thread),NULL,CommunicateThread,ls);
	}
	return 0;
}

void *LaserServer::CommunicateThread(void *ptr)
{
	LaserServer *s = (LaserServer *)ptr;
//	SOCKADDR_IN dest_add;
//	int nAddrLen = sizeof(dest_add);
	//Íš¹ýRCVSocket»ñµÃ¶Ô·œµÄµØÖ·
//	if (::getpeername(*s, (SOCKADDR*)&dest_add, &nAddrLen) != 0)
//	{
//		std::cout << "Get IP address by socket failed!" << endl;
//		return;
//	}
//	cout << "IP: " << ::inet_ntoa(dest_add.sin_addr) <<
//		"  PORT: " << ntohs(dest_add.sin_port) << "connected!" << endl;
    cout<<"Somebody connected!"<<endl;
	while (1)
	{
		int byte_rev;
		char buffer[DATA_LEN] = { 0 };
		byte_rev = s->communicate_socket.recv(buffer, DATA_LEN);
		if (buffer[0] == 'G' && buffer[1] == 'E' && buffer[2] == 'T')
		{
			s->_mtx->lock();
			char buffer_send[DATA_LEN] = { 0 };
			buffer_send[0] = 'R';
			buffer_send[1] = 'E';
			buffer_send[2] = 'T';
			buffer_send[3] = 'X';
			char buffer_tmp[4];
			intToByte(*(s->_x), buffer_tmp);
			memcpy(buffer_send + 4, buffer_tmp, 4);
			intToByte(*(s->_y), buffer_tmp);
			memcpy(buffer_send + 8, buffer_tmp, 4);
			if(*(s->_x) == -1 || *(s->_y) == -1)
				intToByte(0, buffer_tmp);
			else
				intToByte(1, buffer_tmp);
			memcpy(buffer_send + 12, buffer_tmp, 4);
			s->_mtx->unlock();
			s->communicate_socket.send(buffer_send, DATA_LEN);
		}
	}
	return 0;
}

void LaserServer::intToByte(int i, char * bytes, int size)
{
	//byte[] bytes = new byte[4];
	memset(bytes, 0, sizeof(char) *  size);
	bytes[0] = (char)(0x000000ff & i);
	bytes[1] = (char)((0x0000ff00 & i) >> 8);
	bytes[2] = (char)((0x00ff0000 & i) >> 16);
	bytes[3] = (char)((0xff000000 & i) >> 24);
	return;
}
