using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


class Program
{
    static async Task Main()
    {
        string apiEndpoint = "https://archivebooksapi.badryouth.org/Admin/api/UploadBooksImagesToCart";
        string folderPath = @"C:\images"; // Replace with your folder path
        int scanIntervalSeconds = 1; // Set the interval for scanning the folder (in seconds)

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            Console.ReadLine();
            return;
        }

        while (true)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif" }; // Add more file extensions if needed
            List<string> filesToUpload = new List<string>();

            foreach (string extension in supportedExtensions)
            {
                string[] files = Directory.GetFiles(folderPath, $"*{extension}");

                filesToUpload.AddRange(files);
            }

            if (filesToUpload.Count > 0)
            {
                using (HttpClient client = new HttpClient())
                {
                    foreach (string file in filesToUpload)
                    {
                        using (MultipartFormDataContent formData = new MultipartFormDataContent())
                        {
                            byte[] fileBytes = File.ReadAllBytes(file);
                            ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            formData.Add(fileContent, "Images", Path.GetFileName(file));

                            // Upload the file with progress
                            HttpResponseMessage response = await client.PostAsync(apiEndpoint, formData);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"File uploaded successfully: {file}");

                                // Delete the file after upload
                                File.Delete(file);
                                Console.WriteLine($"File deleted: {file}");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to upload file: {file}. Error: {response.StatusCode}");
                            }
                        }
                    }

                    Console.WriteLine("All files uploaded and deleted successfully.");
                }
            }
            else
            {
                Console.WriteLine("No supported files found in the specified folder.");
            }

            Thread.Sleep(scanIntervalSeconds * 1000); // Sleep for the specified interval before the next scan
        }
    }
}

