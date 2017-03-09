set(name main)

cmake_minimum_required(VERSION 2.8)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -std=gnu++11 -pthread")
project(${name})
find_package(OpenCV REQUIRED)
add_executable(${name} ${name}.cpp SocketServer.cpp socket.cc)
target_link_libraries(${name} ${OpenCV_LIBS})
