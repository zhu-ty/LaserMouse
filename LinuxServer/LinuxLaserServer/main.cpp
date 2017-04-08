#define WIN32_LEAN_AND_MEAN
#include <time.h>    
#include <iostream>
#include <fstream>
#include <cstdlib>
#include <cstdio>
#include <string>
#include <mutex>
#include "opencv2/opencv.hpp"
#include "SocketServer.h"
#include <memory>
#include <cmath>
using namespace cv;
using namespace std;

//1.原始参数设定
int camwidth=640;
int camheight=480;//画幅
int scrwidth=camwidth;
int scrheight=camheight;//画面中看到的投影屏幕的大小
int pcwidth = 1920; 
int pcheight= 1080; 
double minVal,maxVal;
Point maxloc,minloc,mouse;
Mat src_frame,frame;
vector<Mat> bgr_frame;
VideoCapture cap(0);
int x = 0, y = 0;

//void setscreen(int &w, int &h);

int main(int argc, char** argv)
{
	mutex mtx;
	shared_ptr<int> xp, yp;
	shared_ptr<mutex> mtxp;

	xp.reset(&x);
	yp.reset(&y);
	mtxp.reset(&mtx);

	LaserServer ls(xp, yp, mtxp);
	ls.start();


	//2.检查摄像头
	if(!cap.isOpened())
	{
		cerr << "Can not open a camera or file." << endl;
		return -1;
	}

	//3.初始化窗口
	namedWindow("frame",1); 
	int x0=pcwidth/2,y0=pcheight/2;
	//setscreen(scrwidth,scrheight);初始化标定

	//4.进入主循环
	while(1)
	{
		//5.传入一帧
		cap >> src_frame;
		if(src_frame.empty())	break;
        cv::flip(src_frame, src_frame, 1);
		split(src_frame, bgr_frame);
		minMaxLoc(bgr_frame[2],&minVal,&maxVal,&minloc,&maxloc);		

		//8.给鼠标传参
		/*
		mouse.x=maxloc.x*pcwidth/scrwidth;
		mouse.y=maxloc.y*pcheight/scrheight;
		*/
		mouse.x=maxloc.x*10000/scrwidth;
		mouse.y=maxloc.y*10000/scrheight;


		//可靠性检查
		mtx.lock();
		if (abs(mouse.x-x0)<1500 && abs(mouse.y-y0)<1500){
			//SetCursorPos(mouse.x,mouse.y);
			cout<<"set mouse x="<<mouse.x<<" y="<<mouse.y<<endl;
			cout<<"width:"<<bgr_frame[0].rows<<"height:"<<bgr_frame[0].cols<<endl;
			x0=mouse.x,y0=mouse.y;
			x=10000-mouse.x,y=mouse.y;
		}
		else
		{
			x=-1,y=-1;
		}
		mtx.unlock();		
		imshow("frame", bgr_frame[2]);
		if(waitKey(1)>= 0)	break;
		

	}	
	return 0;
}

void setpoint(Point &p){

	int x0=1920/2,y0=1080/2;
	while(1)
	{
		cap >> frame;
		if(frame.empty())	break;
		cvtColor(frame,frame,COLOR_BGR2GRAY);
		minMaxLoc(frame,&minVal,&maxVal,&minloc,&maxloc);		
		mouse.x=maxloc.x*pcwidth/scrwidth;
		mouse.y=maxloc.y*pcheight/scrheight;
		if (mouse.x-x0<300 && mouse.y-y0<300){
			//SetCursorPos(mouse.x,mouse.y);
			x0=mouse.x,y0=mouse.y;		
		}
		imshow("frame", frame);
		if(waitKey(1)>= 0){p.x=maxloc.x;p.y=maxloc.y;break;}
	}
}

void setscreen(int &w, int &h){	
	Point A,B,C,D;
	cout<<"point A :"<<endl;
	setpoint(A);
	cout<<A.x <<"      "<<A.y<<endl;

	cout<<"point B :"<<endl;
	setpoint(B);
	cout<<B.x <<"      "<<B.y<<endl;

	cout<<"point C :"<<endl;
	setpoint(C);
	cout<<C.x <<"      "<<C.y<<endl;

	cout<<"point D :"<<endl;
	setpoint(D);
	cout<<D.x <<"      "<<D.y<<endl;
}
