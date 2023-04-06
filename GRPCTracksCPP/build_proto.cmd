SET PATH=D:\TESTS\GRPC_CPP\grpc\.build\Release

protoc -I D:\TESTS\Leaflet\LeafletAlarms\Protos --grpc_out=D:\TESTS\Leaflet\GRPCTracksCPP\GRPCTracksClient --plugin=protoc-gen-grpc=grpc_cpp_plugin D:\TESTS\Leaflet\LeafletAlarms\Protos\tracks.proto

protoc -I D:\TESTS\Leaflet\LeafletAlarms\Protos --cpp_out=D:\TESTS\Leaflet\GRPCTracksCPP\GRPCTracksClient D:\TESTS\Leaflet\LeafletAlarms\Protos\tracks.proto