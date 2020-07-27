# connect-ExchangeOnline -ShowProgress $true
function Get-Messages
{ 
      Param (
            [Parameter(Mandatory=$false)]
            # Date 'MM/dd/yyyy'
            [String]$StartReceivedDate = (Get-Date).addMonths(-1),
            $MaxThreads = 60,
            $SleepTimer = 500,
            $MaxWaitAtEnd = 600,
            $OutputType = "Text")

    $messagecount = 1
    $i = 1
    while($messagecount -gt 0)
    { 
        Write-Host "[*] Collecting Quarantine Messages - Page: $i"
        $message = Get-QuarantineMessage -StartExpiresDate (Get-Date).Addhours(-24) -PageSize 1000 -page $i -QuarantineTypes malware, spam, phish
        $messageS += $message
        $messagecount = $message | measure | select -ExpandProperty count
        $i++
    }
    $messagecount = $messageS | measure | select -ExpandProperty count
    Write-Host "[+] Collected Quarantine Messages: $messagecount"
    # QuarantineMessage with html/htm - likly call phishing
    # Write-Host "[*] Filtering Collected Messages"
    # $callm = $message | Preview-QuarantineMessage | where {$_.attachment -like '*.html' -or $_.attachment -like '*.htm'}
    # $callmcount = $callm | measure.count | select -ExpandProperty count
    # Write-Host "[+] Filtering Collected Messages: $callmcount"
    Get-Job | Remove-Job -Force
    foreach($call in $messageS)
    {
        While ($(Get-Job -state running).count -ge $MaxThreads)
        {
            Write-Progress -Activity "Creating Message List"  -Status "Waiting for threads to close"  -CurrentOperation "$i threads created - $($(Get-Job -state running).count) threads open"  -PercentComplete ($i / $Computers.count * 100)
            Start-Sleep -Milliseconds $SleepTimer
        }
        $call_identity = $call | select -ExpandProperty Identity
        Write-Host "[*] Processing: $call_identity"
        $tmp = Export-QuarantineMessage -Identity $call_identity
        # $tmp.eml | Start-Job ."$PSScriptRoot\bin\Debug\netcoreapp3.1\EPO-Auto-IOC.exe"
        
        #"Starting job - $Computer"
        $i++
        Start-Job -ScriptBlock { $tmp.eml | ."$PSScriptRoot\bin\Debug\netcoreapp3.1\EPO-Auto-IOC.exe" }
        Write-Progress  -Activity "Creating Server List" -Status "Starting Threads" -CurrentOperation "$i threads created - $($(Get-Job -state running).count) threads open" -PercentComplete ($i / $messageS.count * 100)
    

    }
    $Complete = Get-date

    While ($(Get-Job -State Running).count -gt 0)
    {
        $messagesStillRunning = ""
        ForEach ($System  in $(Get-Job -state running)){$messagesStillRunning += ", $($System.name)"}
        $messagesStillRunning = $messagesStillRunning.Substring(2)
        Write-Progress -Activity "Creating Message List" -Status "$($(Get-Job -State Running).count) threads remaining" -CurrentOperation "$messagesStillRunning" -PercentComplete ($(Get-Job -State Completed).count / $(Get-Job).count * 100)
        If ($(New-TimeSpan $Complete $(Get-Date)).totalseconds -ge $MaxWaitAtEnd){"Killing all jobs still running . . .";Get-Job -State Running | Remove-Job -Force}
        Start-Sleep -Milliseconds $SleepTimer
    }

    "Reading all jobs"

    If ($OutputType -eq "Text")
    {
        ForEach($Job in Get-Job)
        {
            "$($Job.Name)"
            "****************************************"
            Receive-Job $Job
            " "
        }
    }
    ElseIf($OutputType -eq "GridView"){
        Get-Job | Receive-Job | Select-Object * -ExcludeProperty RunspaceId | out-gridview
    
    }

}
