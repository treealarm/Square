#include "tracks.pb.h"
#include "tracks.grpc.pb.h"

#include <grpc/grpc.h>
#include <grpcpp/channel.h>
#include <grpcpp/client_context.h>
#include <grpcpp/create_channel.h>
#include <grpcpp/security/credentials.h>



using grpc::Channel;
using grpc::ClientContext;
using grpc::ClientReader;
using grpc::ClientReaderWriter;
using grpc::ClientWriter;
using grpc::Status;

class TracksGrpcServiceClient
{
  std::unique_ptr<tracks::TracksGrpcService::Stub> stub_;

public:
  TracksGrpcServiceClient(std::shared_ptr<Channel> channel)
    : stub_(tracks::TracksGrpcService::NewStub(channel))
  {
  }

  void UpdateFigures(double lat = 55.755864, double lon = 37.617698) {

    ClientContext context;
    tracks::ProtoFigures response;

    tracks::ProtoFigures figs;
    tracks::ProtoFig* fig =
      figs.add_figs();

    fig->set_id("6423e54d513bfe83e9d59792");
    fig->set_name("Test cpp");
    fig->set_radius(100);
    figs.set_add_tracks(true);

    tracks::ProtoGeometry* loc = fig->mutable_geometry();
    
    loc->set_type("Point");
    auto coord = loc->add_coord();
    coord->set_lat(lat);
    coord->set_lon(lon);
   

    auto extra_prop = fig->add_extra_props();
    extra_prop->set_prop_name("track_name");
    extra_prop->set_str_val("lisa_alert");

    extra_prop = fig->add_extra_props();
    extra_prop->set_prop_name("track_name");
    extra_prop->set_str_val("lisa_alert_cpp");

    auto stat = stub_->UpdateFigures(&context, figs, &response);
    std::string s;
    response.SerializeToString(&s);
    std::cout  << "replay:" << stat.error_message() << s << std::endl;
  }

};