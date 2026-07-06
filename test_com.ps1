$ErrorActionPreference = 'Stop'
try {
    $cad = New-Object -ComObject GstarCAD.Application
    Write-Host "COM Created"
    
    $templatePath = "C:\Users\PARTH\source\repos\MegaEngineeringSuite\Templates\BAFFLE_Flange_template.dwg"
    $doc = $cad.Documents.Open($templatePath)
    Write-Host "Doc Opened"
    
    $outputPath = "C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings\test_COM.dwg"
    $doc.SaveAs($outputPath)
    Write-Host "Doc Saved"
    
    $doc.Close($false)
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($doc) | Out-Null
    
    $cad.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($cad) | Out-Null
    Write-Host "Done"
}
catch {
    Write-Error $_.Exception.Message
}
