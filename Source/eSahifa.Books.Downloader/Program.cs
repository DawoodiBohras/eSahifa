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
                while (bookNumber <= 20)
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
            }
            Console.WriteLine(string.Empty);
            Console.WriteLine("BOOK DOWNLOAD IS COMPLETE!!!");
            Console.Read();
        }

        static async Task<bool> DownloadBook(string directory, int bookNumber)
        {
            var di = Directory.CreateDirectory(Path.Combine(directory, $"Book{bookNumber:D5}"));
            Console.WriteLine($"Starting download of Book {bookNumber} to {di.FullName}");

            using (HttpClient client = new HttpClient())
            {
                int pageNumber = 0;
                while (bookNumber <= 20)
                {
                    try
                    {
                        if (pageNumber == 0)
                        {
                            try
                            {
                                Console.Write($"Downloading book {bookNumber} thumbnail...");
                                string filePath = di.FullName + $"\\thumb.jpg";
                                if (!File.Exists(filePath))
                                {
                                    string url = $"https://esahifa.com/books/bk{bookNumber:D5}/thumb.jpg";
                                    Console.Write(url);
                                    await DownloadFile(client, filePath, url);
                                    Console.WriteLine("...DONE!");
                                }
                                else
                                {
                                    Console.WriteLine("already exists.");
                                }
                            }
                            catch(Exception)
                            {
                                Console.WriteLine("could not download.");
                            }
                        }
                        else
                        {
                            Console.Write($"Downloading book {bookNumber} page {pageNumber}...");
                            string filePath = di.FullName + $"\\{pageNumber:D3}.jpg";

                            if (!File.Exists(filePath))
                            {
                                string url = $"https://esahifa.com/books/bk{bookNumber:D5}/{pageNumber:D3}.jpg";
                                Console.Write(url);
                                await DownloadFile(client, filePath, url);
                                Console.WriteLine("...DONE!");
                            }
                            else
                            {
                                Console.WriteLine("already exists.");
                            }
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

        private static async Task DownloadFile(HttpClient client, string filePath, string url)
        {
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
        }
    }
}