param([long]$TotalAmount = 100000, [string]$BaseUrl = "http://localhost:5108")

$now = [DateTime]::UtcNow
$password = "RkFLRVBBU1NXT1JEMTIzNA=="
$raw = $now.ToString("yyyyMMddHHmmss") + "FAKEGOOGLE" + "FG-00001" + $TotalAmount + $password
$hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($raw))
$hex = -join ($hash | ForEach-Object { $_.ToString("x2") })
$sig = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($hex))

$body = @{
    partnerkey      = "FAKEGOOGLE"
    partnerrefno    = "FG-00001"
    partnerpassword = $password
    totalamount     = $TotalAmount
    timestamp       = $now.ToString("o")
    sig             = $sig
} | ConvertTo-Json

Invoke-RestMethod -Uri "$BaseUrl/api/submittrxmessage" -Method Post -ContentType "application/json" -Body $body | ConvertTo-Json -Compress
