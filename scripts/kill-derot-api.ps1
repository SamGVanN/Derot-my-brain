$procs = Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.CommandLine -match 'DerotMyBrain.API' }
if ($procs) {
  foreach ($p in $procs) {
    Write-Output "Killing PID=$($p.ProcessId) Name=$($p.Name)"
    try { Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue } catch {}
  }
} else {
  Write-Output 'No DerotMyBrain.API processes found'
}
