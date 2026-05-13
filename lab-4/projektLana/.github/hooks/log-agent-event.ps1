param(
    [Parameter(Mandatory = $true)]
    [string]$EventName
)

$ErrorActionPreference = 'Stop'

$logPath = Join-Path $PSScriptRoot 'agent_log.txt'
$rawInput = [Console]::In.ReadToEnd()

switch ($EventName) {
    'UserPromptSubmit' {
        try {
            $payload = $rawInput | ConvertFrom-Json
            $message = $payload.prompt
        }
        catch {
            $message = $rawInput.Trim()
        }

        Add-Content -Path $logPath -Value ("--- UserPromptSubmit ---`n{0}`n" -f $message)
    }
    'PreToolUse' {
        Add-Content -Path $logPath -Value ("--- PreToolUse ---`n{0}`n" -f $rawInput.Trim())
    }
    'PostToolUse' {
        Add-Content -Path $logPath -Value ("--- PostToolUse ---`n{0}`n" -f $rawInput.Trim())
    }
    'Stop' {
        Add-Content -Path $logPath -Value ("--- Stop ---`n{0}`n" -f $rawInput.Trim())
    }
    default {
        throw "Unsupported event: $EventName"
    }
}