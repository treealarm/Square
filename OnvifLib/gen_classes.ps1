$wsdlFiles = @(
    "wsdl/onvif/ver10/device/wsdl/devicemgmt.wsdl",
    "wsdl/onvif/ver10/media/wsdl/media.wsdl",
    "wsdl/onvif/ver20/ptz/wsdl/ptz.wsdl",
    "wsdl/onvif/ver20/imaging/wsdl/imaging.wsdl"
)

# Преобразуем относительные пути в абсолютные
$wsdlPaths = $wsdlFiles | ForEach-Object { Resolve-Path $_ }

# Генерация классов
dotnet-svcutil @wsdlPaths --namespace "*","ONVIF.AC"
