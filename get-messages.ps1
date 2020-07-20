#connect-ExchangeOnline -ShowProgress $true
#Install-Package MimeKit

function Get-Messages{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )
$m = {}
$callm = {}
#QuarantineMessage
$m = Get-QuarantineMessage -StartReceivedDate (Get-Date).AddDays(-30) -PageSize 100 -QuarantineTypes malware
#QuarantineMessage with html/htm - likly call phishing
$callm = $m | Preview-QuarantineMessage | where {$_.attachment -like '*.html' -or $_.attachment -like '*.htm'}
foreach($call in $callm){
$call_identity = $call | select -ExpandProperty Identity
$call_file_path = $call_identity -replace "\\", "-"
$pwd = Get-Location
$tmp = Export-QuarantineMessage -Identity $call_identity
$attachment = $tmp.eml | .".\EPO-Auto-IOC\EPO-Auto-IOC.exe"
$attachment | Out-File -FilePath .\attachments\$call_file_path
write-host $call_file_path 'saved'
}
}