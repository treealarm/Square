# Определяем корневую директорию WSDL и XSD-файлов
$wsdlRoot = (Get-Item -Path ".\" -Verbose).FullName + "\wsdl\"

# Указываем WSDL-файлы, которые нужно обработать
$wsdlFiles = @(
   
    "ver10/device/wsdl/devicemgmt.wsdl",
    "ver10/media/wsdl/media.wsdl",
    "ver10/events/wsdl/event.wsdl",
    "ver10/replay.wsdl",
    "ver10/search.wsdl",
    "ver10/pacs/accesscontrol.wsdl",
    "ver10/deviceio.wsdl",
    "ver20/imaging/wsdl/imaging.wsdl",
    "ver20/ptz/wsdl/ptz.wsdl"
    
)

# Указываем XSD-файлы, которые могут потребоваться
$xsdFiles = @(
    "ver10/schema/onvif.xsd"
    ,"ver10/schema/common.xsd"
    ,"ver10/schema/metadatastream.xsd"
    ,"ver10/pacs/types.xsd"
    ,"ver20/analytics/rules.xsd",
    ,"ver20/analytics/humanbody.xsd",
    ,"ver20/analytics/humanface.xsd"
)

# Формируем абсолютные пути и проверяем существование файлов
$wsdlPaths = @()
foreach ($file in $wsdlFiles) {
    $fullPath = "$wsdlRoot$file"
    if (Test-Path $fullPath) {
        $wsdlPaths += $fullPath
    } else {
        Write-Host "Ошибка: Файл не найден -> $fullPath" -ForegroundColor Red
    }
}

$xsdPaths = @()
foreach ($file in $xsdFiles) {
    $fullPath = "$wsdlRoot$file"
    if (Test-Path $fullPath) {
        $xsdPaths += $fullPath
    } else {
        Write-Host "Ошибка: Файл не найден -> $fullPath" -ForegroundColor Red
    }
}

# Проверяем, что список WSDL не пуст
if ($wsdlPaths.Count -eq 0) {
    Write-Host "Ошибка: Нет доступных WSDL-файлов!" -ForegroundColor Red
    exit 1
}

# Вывод списка используемых файлов
Write-Host "Используемые WSDL-файлы:" -ForegroundColor Green
$wsdlPaths | ForEach-Object { Write-Host $_ }

Write-Host "Используемые XSD-файлы:" -ForegroundColor Cyan
$xsdPaths | ForEach-Object { Write-Host $_ }

# Генерация классов
#dotnet-svcutil @($wsdlPaths + $xsdPaths) --namespace "*","ONVIF.AC" --targetFramework "net8.0" --verbosity "Debug"

# Формируем строку с аргументами
$wsdlFiles = $wsdlPaths -join " "
$xsdFiles = $xsdPaths -join " "

$arguments = "$wsdlFiles $xsdFiles --targetFramework `"net8.0`" --verbosity `"Debug`""

# Выводим команду перед выполнением
Write-Host "Executing: dotnet-svcutil $arguments"

# Выполняем команду
Invoke-Expression "dotnet-svcutil $arguments"


