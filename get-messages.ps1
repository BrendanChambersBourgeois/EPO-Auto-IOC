#connect-ExchangeOnline -ShowProgress $true

function Get-Messages
{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )
    $m = {}
    $callm = {}
    #QuarantineMessage
    $m = Get-QuarantineMessage -StartReceivedDate (Get-Date).AddDays(-1) -PageSize 100 -QuarantineTypes malware
    #QuarantineMessage with html/htm - likly call phishing
    $callm = $m | Preview-QuarantineMessage | where {$_.attachment -like '*.html' -or $_.attachment -like '*.htm'}
    $Array = @()
    $urls = @()
    foreach($call in $callm)
    {
        $call_identity = $call | select -ExpandProperty Identity
        $call_file_path = $call_identity -replace "\\", "-"
        $tmp = Export-QuarantineMessage -Identity $call_identity
        $tmp.eml | ."$PSScriptRoot\EPO-Auto-IOC\EPO-Auto-IOC.exe"
        
    }
return $Array
}

