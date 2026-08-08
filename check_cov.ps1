$xml = Select-Xml -Path "C:\Users\Asus\source\repos\Application de vente\TestResults\2185e2b6-1646-4907-808d-5526b7ee2f40\coverage.cobertura.xml" -XPath "//class[contains(@name, 'Controller') and not(contains(@name, 'Tests'))]"
$xml | ForEach-Object {
    [PSCustomObject]@{
        Name = $_.Node.name
        LineRate = $_.Node.'line-rate'
    }
} | Format-Table -AutoSize
