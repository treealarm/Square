# CMakeList.txt : CMake project for GRPCTracksClient, include source and define
# project specific logic here.
#

set(GRPC_ROOT_FOLDER $ENV{GRPC_ROOT})
set(PROTO_INC "${GRPC_ROOT_FOLDER}\\third_party\\protobuf\\src")
#set(PROTO_LIB "${GRPC_ROOT_FOLDER}\\.build\\Release")
set(PROTO_LIB_PATH "${GRPC_ROOT_FOLDER}\\.build\\third_party\\protobuf\\Release")
set(PROTO_LIB "libprotobuf.lib")


message(STATUS "DEBUG: x=${PROTO_INC}")

include_directories(${PROTO_INC})
link_directories(${PROTO_LIB_PATH})


# Add source to this project's executable.
add_executable (GRPCTracksClient "GRPCTracksClient.cpp" "GRPCTracksClient.h" "tracks.pb.cc" "tracks.pb.h")

target_link_libraries(GRPCTracksClient ${PROTO_LIB})


if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET GRPCTracksClient PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
