param([string]$file)

# Define the directory to check
$directoryToCheck = "Assets/Resources/CVRFury/nvhpmm/AppComponents/"

# Get the directory of the script
$scriptDirectory = Split-Path $PSCommandPath

# concatenate the script directory with the file
$file = Join-Path $scriptDirectory $file

# concatenate the script directory with the directory to check
$directoryToCheck = Join-Path $scriptDirectory $directoryToCheck

# Check if the file is the specified directory
if ($file.StartsWith($directoryToCheck)) {

  $fileName = [System.IO.Path]::GetFileName($file)

  # print the file name to the console
  Write-Host "File Name: $fileName"

  # get the path of the $file excluding the file name+extension
  $filePath = [System.IO.Path]::GetDirectoryName($file)

  # print the file path to the console
  Write-Host "File Path: $filePath"

  # check if the fileName ends with .cs.source
  if ($fileName -match "\.cs\.source$")
  {
    # remove the .source extension
    $newFileName = $fileName -replace '\.source$', ''

    # concatenate the file path with the new file name
    $newFileName = Join-Path $filePath $newFileName

    # rename the file to .cs
    Rename-Item -Path $file -NewName $newFileName

    # trigger the csharpier command
    Write-Host "Running csharpier on $newFileName"
    dotnet csharpier $newFileName

    # rename the file back to .cs.source
    Rename-Item -Path $newFileName -NewName $file
  }
}
else {
  # not in the specified directory, so will need to handle normal csharpier command
  Write-Host "Running csharpier on $file"
  dotnet csharpier $file
}
