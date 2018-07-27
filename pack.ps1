ls -Recurse -Filter *.nuspec | foreach{
    C:\tools\nuget.exe pack $_.FullName -OutputDirectory ./
}