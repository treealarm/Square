$client = new-object System.Net.WebClient
$currentDir = (Get-Item -Path ".\" -Verbose).FullName + "\"
$wsdlsDir = $currentDir + "wsdl\"

New-Item -ItemType Directory -Force -Path $wsdlsDir

$wsdl_urls = @()
$wsdl_urls += ,@("http://www.onvif.org/onvif/ver10/deviceio.wsdl", "wsdl/wsdl/wsdl/deviceio.wsdl")
$wsdl_urls += ,@("http://www.onvif.org/ver10/schema/common.xsd", "ver10/schema/common.xsd")
$wsdl_urls += ,@("http://www.onvif.org/ver10/schema/metadatastream.xsd", "ver10/schema/metadatastream.xsd")
$wsdl_urls += ,@("http://www.onvif.org/ver20/analytics/humanbody.xsd", "ver20/analytics/humanbody.xsd")
$wsdl_urls += ,@("http://www.onvif.org/ver20/analytics/humanface.xsd", "ver20/analytics/humanface.xsd")
$wsdl_urls += ,@("https://www.onvif.org/ver10/pacs/types.xsd", "wsdl/wsdl/wsdl/wsdl/types.xsd")  # Добавлен этот файл

foreach($url in $wsdl_urls) {
	Write-Host $("Start download: " + $url[0])
	New-Item $($wsdlsDir + $url[1]) -type file -force
	$client.DownloadFile( $url[0],  $($wsdlsDir + $url[1]))
}
