
SET LEAFLET_PROTO_SRC=D:\TESTS\Leaflet\LeafletAlarms\Protos
SET PROTO_PATH=D:\TESTS\GRPC_CPP\grpc\third_party\protobuf\src
SET OUT_PATH=D:\TESTS\Leaflet\GRPCTracksCPP\GRPCTracksClient
SET OUT_PATH_GRPC=D:\TESTS\Leaflet\GRPCTracksCPP\GRPCTracksClient\GRPC
SET PROTO_FILE=D:\TESTS\Leaflet\LeafletAlarms\Protos\tracks.proto
SET RELEASE_BIN=D:\TESTS\GRPC_CPP\grpc\.build\Release
SET PATH=%RELEASE_BIN%

protoc -I %LEAFLET_PROTO_SRC% --proto_path %PROTO_PATH% --grpc_out=%OUT_PATH% --plugin=protoc-gen-grpc=%RELEASE_BIN%\grpc_cpp_plugin.exe %PROTO_FILE%

protoc -I %LEAFLET_PROTO_SRC% --proto_path %PROTO_PATH%  --cpp_out=%OUT_PATH% %PROTO_FILE%