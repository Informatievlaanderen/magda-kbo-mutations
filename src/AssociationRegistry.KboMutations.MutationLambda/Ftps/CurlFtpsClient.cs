using System.Diagnostics;
using Amazon.Lambda.Core;
using AssociationRegistry.KboMutations.MutationLambda.Configuration;

namespace AssociationRegistry.KboMutations.MutationLambda.Ftps;

public class CurlFtpsClient : IFtpsClient
{
    private readonly KboMutationsConfiguration _kboMutationsConfiguration;
    private readonly ILambdaLogger _logger;

    public CurlFtpsClient(
        ILambdaLogger logger,
        KboMutationsConfiguration kboMutationsConfiguration)
    {
        _logger = logger;
        _kboMutationsConfiguration = kboMutationsConfiguration;
    }

    public string GetListing(string sourceDirectory)
    {
        _logger.LogInformation($"Fetching mutation files from folder {sourceDirectory}");

        using (var process = new Process
               {
                   StartInfo = new ProcessStartInfo
                   {
                       FileName = _kboMutationsConfiguration.CurlLocation,
                       Arguments = $"--ssl-reqd " +
                                   $"-k " +
                                   $"--user {_kboMutationsConfiguration.Username}:{_kboMutationsConfiguration.Password} " +
                                   $"--cert {_kboMutationsConfiguration.CertPath} " +
                                   $"--key {_kboMutationsConfiguration.KeyPath} --key-type {_kboMutationsConfiguration.KeyType} " +
                                   (!string.IsNullOrEmpty(_kboMutationsConfiguration.CaCertPath) ? $"--cacert {_kboMutationsConfiguration.CaCertPath} " : "") +
                                   $"{sourceDirectory} --fail --silent --show-error",
                       RedirectStandardOutput = true,
                       RedirectStandardError = true,
                       UseShellExecute = false,
                       CreateNoWindow = true
                   }
               })
        {
            process.Start();
            process.WaitForExit();
            var result = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
                throw new Exception($"Could not list files at {sourceDirectory}:\n{error}");

            return result;
        }
    }

    // public string GetListing(string sourceDirectory)
    // {
    //     _logger.LogInformation($"Fetching mutation files from folder {sourceDirectory}");
    //
    //     using (var process = new Process
    //            {
    //                StartInfo = new ProcessStartInfo
    //                {
    //                    FileName = _kboMutationsConfiguration.CurlLocation,
    //                    Arguments = $"--ftp-ssl " +
    //                                $"--user {_kboMutationsConfiguration.Username}:{_kboMutationsConfiguration.Password} " +
    //                                $"--cert {_kboMutationsConfiguration.CertPath} " +
    //                                $"--key {_kboMutationsConfiguration.KeyPath} --key-type {_kboMutationsConfiguration.KeyType} " +
    //                                $"--cacert {_kboMutationsConfiguration.CaCertPath} " +
    //                                $"{sourceDirectory} --fail --silent --show-error",
    //                    RedirectStandardOutput = true,
    //                    RedirectStandardError = true,
    //                    UseShellExecute = false,
    //                    CreateNoWindow = true
    //                }
    //            })
    //     {
    //         process.Start();
    //         process.WaitForExit();
    //         var result = process.StandardOutput.ReadToEnd();
    //         var error = process.StandardError.ReadToEnd();
    //
    //         if (process.ExitCode != 0)
    //             throw new Exception($"Could not list files at {sourceDirectory}:\n{error}");
    //
    //         return result;
    //     }
    // }

    public bool Download(Stream stream, string sourceFilePath)
    {
        var fileName = Guid.NewGuid().ToString();
        using (var process = new Process
               {
                   StartInfo = new ProcessStartInfo
                   {
                       FileName = _kboMutationsConfiguration.CurlLocation,
                       Arguments = $"--ssl-reqd " +
                                   $"-k " +
                                   $"--user {_kboMutationsConfiguration.Username}:{_kboMutationsConfiguration.Password} " +
                                   $"--cert {_kboMutationsConfiguration.CertPath} " +
                                   $"--key {_kboMutationsConfiguration.KeyPath} --key-type {_kboMutationsConfiguration.KeyType} " +
                                   (!string.IsNullOrEmpty(_kboMutationsConfiguration.CaCertPath) ? $"--cacert {_kboMutationsConfiguration.CaCertPath} " : "") +
                                   $"{sourceFilePath} " +
                                   $"-o {fileName} --fail --silent --show-error",
                       RedirectStandardOutput = true,
                       RedirectStandardError = true,
                       UseShellExecute = false,
                       CreateNoWindow = true
                   }
               })
        {
            process.Start();
            process.WaitForExit();
            var error = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Could not download file {sourceFilePath}:\n{error}");
                return false;
            }

            var readAllBytes = File.ReadAllBytes(fileName);
            stream.Write(readAllBytes);
            File.Delete(fileName);

            return true;
        }
    }

    public void MoveFile(string baseUri, string sourceFilePath, string destinationFilePath)
    {
        using (var process = new Process
               {
                   StartInfo = new ProcessStartInfo
                   {
                       FileName = _kboMutationsConfiguration.CurlLocation,
                       Arguments = $"--ssl-reqd " +
                                   $"-k " +
                                   $"--user {_kboMutationsConfiguration.Username}:{_kboMutationsConfiguration.Password} " +
                                   $"--cert {_kboMutationsConfiguration.CertPath} " +
                                   $"--key {_kboMutationsConfiguration.KeyPath} --key-type {_kboMutationsConfiguration.KeyType} " +
                                   (!string.IsNullOrEmpty(_kboMutationsConfiguration.CaCertPath) ? $"--cacert {_kboMutationsConfiguration.CaCertPath} " : "") +
                                   $"{baseUri} " +
                                   $"-Q \"-RNFR {sourceFilePath.TrimStart('/')}\" " +
                                   $"-Q \"-RNTO {destinationFilePath.TrimStart('/')}\" --fail --silent --show-error",
                       RedirectStandardOutput = true,
                       RedirectStandardError = true,
                       UseShellExecute = false,
                       CreateNoWindow = true
                   }
               })
        {
            process.Start();
            process.WaitForExit();

            var error = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
                _logger.LogError($"Could not move file {sourceFilePath} to {destinationFilePath}:\n{error}");
        }
    }
}