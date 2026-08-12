using BLL;

var result = DatabaseBackupBLL.CreateVerifiedBackup();
Console.WriteLine(result.Success ? "OK" : "FAIL");
Console.WriteLine("Success=" + result.Success);
Console.WriteLine("DatabaseName=" + result.DatabaseName);
Console.WriteLine("BackupPath=" + (result.BackupPath ?? ""));
Console.WriteLine("CreatedAt=" + result.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
Console.WriteLine("SizeBytes=" + result.SizeBytes);
Console.WriteLine("Verified=" + result.Verified);
if (!string.IsNullOrEmpty(result.ErrorMessage))
    Console.WriteLine("ErrorMessage=" + result.ErrorMessage);

Environment.Exit(result.Success ? 0 : 1);
