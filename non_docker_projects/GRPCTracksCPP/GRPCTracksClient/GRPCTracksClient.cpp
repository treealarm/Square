// GRPCTracksClient.cpp : Defines the entry point for the application.
//

#include "GRPCTracksClient.h"
#include "TracksGrpcServiceClient.h"

#include <chrono>
#include <thread>

using namespace std::chrono_literals;
using namespace std;

int main()
{
	cout << "Hello CMake." << endl;

  TracksGrpcServiceClient client(
    grpc::CreateChannel(
      "127.0.0.1:5000",
      grpc::InsecureChannelCredentials()
    )
  );
  double lat = 55.755864;
  double lon = 37.617698;

  for (double i = 0; i < 1; i += 0.001)
  {
    client.UpdateFigures(lat + i, lon - i);
    std::this_thread::sleep_for(1000ms);
  }
	return 0;
}
