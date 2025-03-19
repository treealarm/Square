$wsdlFiles = @(
    "wsdl/onvif/ver10/device/wsdl/devicemgmt.wsdl",
    "wsdl/onvif/ver10/media/wsdl/media.wsdl",
    "wsdl/onvif/ver20/ptz/wsdl/ptz.wsdl",
    "wsdl/onvif/ver20/imaging/wsdl/imaging.wsdl"
)

# ����������� ������������� ���� � ����������
$wsdlPaths = $wsdlFiles | ForEach-Object { Resolve-Path $_ }

# ��������� �������
dotnet-svcutil @wsdlPaths --namespace "*","ONVIF.AC"
