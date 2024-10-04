using Renci.SshNet.Common;
using Renci.SshNet;
using System.Net.Sockets;

namespace VacationApi.Services
{
    /// <summary>
    /// Service that will define methods for files.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Will save a file and return it's path
        /// </summary>
        /// <param name="file">File to save</param>
        /// <param name="name">Name to give to the file</param>
        /// <param name="directory">Directory where to save the file</param>
        /// <returns>
        /// The path of where the file is saved on the server.
        /// </returns>
        public string SaveFile(IFormFile file, string name, string directory)
        {
            var ftpServerUri = "192.168.132.203";
            var ftpUsername = SecretConfig.MATRICULE;
            var ftpPassword = SecretConfig.PASSWORD;

            var extention = file.FileName.Substring(file.FileName.LastIndexOf("."));
            var fileName = $"{ftpServerUri}/~e200106/{directory}/{name}{extention}";
            using SftpClient client = new(ftpServerUri, 22, ftpUsername, ftpPassword);
            try
            {
                client.Connect();
                if (client.IsConnected)
                {
                    client.UploadFile(file.OpenReadStream(), $"public_html/{directory}/{name}{extention}");
                    client.Disconnect();
                }
            }
            catch (Exception e) when (e is SshConnectionException || e is SocketException || e is ProxyException)
            {
                Console.WriteLine($"Error connecting to server: {e.Message}");
            }
            catch (SshAuthenticationException e)
            {
                Console.WriteLine($"Failed to authenticate: {e.Message}");
            }
            catch (SftpPermissionDeniedException e)
            {
                Console.WriteLine($"Operation denied by the server: {e.Message}");
            }
            catch (SshException e)
            {
                Console.WriteLine($"Sftp Error: {e.Message}");
            }
            return "https://" + fileName;
        }
    }
}
