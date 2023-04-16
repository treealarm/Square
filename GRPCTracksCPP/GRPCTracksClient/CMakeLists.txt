# CMakeList.txt : CMake project for GRPCTracksClient, include source and define
# project specific logic here.
#


include(FetchContent)
FetchContent_Declare(
  gRPC
  GIT_REPOSITORY https://github.com/grpc/grpc
  GIT_TAG        v1.53.0
)
set(FETCHCONTENT_QUIET OFF)
FetchContent_MakeAvailable(gRPC)

# Add source to this project's executable.
add_executable (GRPCTracksClient 
"GRPCTracksClient.cpp" 
"GRPCTracksClient.h" 
"tracks.pb.cc" 
"tracks.pb.h" 
"tracks.grpc.pb.h"
"tracks.grpc.pb.cc"
"TracksGrpcServiceClient.cpp"
"TracksGrpcServiceClient.h")

target_link_libraries(GRPCTracksClient grpc++)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET GRPCTracksClient PROPERTY CXX_STANDARD 20)
endif()

# TODO: Add tests and install targets if needed.
