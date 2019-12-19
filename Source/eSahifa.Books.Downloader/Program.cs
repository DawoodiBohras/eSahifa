using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eSahifa.Books.Downloader
{
    class Program
    {
        private string[] _booksList = {
            "https://esahifa.com/books/bk00001",
        };

        static void Main(string[] args)
        {
            try
            {
                var directory = args.FirstOrDefault() ?? Environment.CurrentDirectory;
                Console.WriteLine("Output Directory: " + directory);
                int bookNumber = 1;
                while (true)
                {
                    bool completed = DownloadBook(directory, bookNumber).Result;
                    if (completed == false)
                        break;
                    bookNumber++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("APPLICATION FAILURE!");
                Console.WriteLine(ex.ToString());
                Console.Read();
            }
        }

        static async Task<bool> DownloadBook(string directory, int bookNumber)
        {
            var di = Directory.CreateDirectory(Path.Combine(directory, $"Book{bookNumber:D5}"));
            Console.WriteLine("Output Directory: " + di.FullName);
            Console.WriteLine($"Starting download of Book {bookNumber}...");

            using (HttpClient client = new HttpClient())
            {
                int pageNumber = 1;
                while (true)
                {
                    Console.Write($"Downloading book {bookNumber} page {pageNumber}...");

                    try
                    {
                        string filePath = di.FullName + $"\\{pageNumber:D3}.jpg";

                        if (!File.Exists(filePath))
                        {
                            string url = $"https://esahifa.com/books/bk{bookNumber:D5}/{pageNumber:D3}.jpg";
                            Console.Write(url);

                            // Send  request asynchronously
                            using (HttpResponseMessage response = await client.GetAsync(url))
                            {
                                // Check that response was successful or throw exception
                                response.EnsureSuccessStatusCode();

                                // Read response asynchronously and save asynchronously to file
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    // Copy the content from response to filestream
                                    await response.Content.CopyToAsync(fileStream);
                                }
                            }
                            Console.WriteLine("...DONE!");
                        }
                        else
                        {
                            Console.WriteLine("already exists.");
                        }

                        pageNumber++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("...FAILED!");
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                }
            }

            if (di.Exists && di.GetFiles().Length == 0)
            {
                Console.WriteLine($"Book {bookNumber} does not exist.");
                di.Delete(true);
                return false;
            }
            else
            {
                Console.WriteLine($"Book {bookNumber} has been downloaded!");
                return true;
            }
        }
    }
}