using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperCrawler
{
    class Program
    {
        private static string downloadPath = @"D:\Temp\";
        private static string site = "http://sj.zol.com.cn";
        private static HttpClient httpClient = new HttpClient();
        private static string startPage = "/bizhi/dongman/1.html";

        static void Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            startCrawlerasync();

            Console.ReadKey();
        }

        private static async Task startCrawlerasync()
        {
            try
            {
                var html = await httpClient.GetStringAsync(site + startPage);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var dongmans = htmlDocument.DocumentNode.SelectNodes("//li[@class=\"photo-list-padding\"]//a");
                foreach (var dongman in dongmans)
                {
                    var url = site + dongman.Attributes["href"].Value;
                    await getLinksFromTitleAsync(url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 尋找同分類的頁面
        /// </summary>
        /// <returns></returns>
        private static async Task getLinksFromTitleAsync(string url)
        {
            var links = new List<string>();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var title = htmlDocument.DocumentNode.SelectNodes("//div[@class=\"wrapper photo-tit clearfix\"]//h1//a").FirstOrDefault().InnerText;
            Console.WriteLine("作品名稱: " + title);

            //在下載資料夾中建立作品資料夾
            string pathString = System.IO.Path.Combine(downloadPath, title);
            System.IO.Directory.CreateDirectory(pathString);

            //
            var pages = htmlDocument.DocumentNode.SelectNodes("//ul[@id=\"showImg\"]//li//a");
            int count = pages.Count;
            Console.WriteLine("共" + count + "筆");
            for (int i = 0; i < count; i++)
            {
                Console.Write(string.Format("{0}: ", i + 1));
                var link = pages[i].Attributes["href"].Value;
                links.Add(link);
                await findImageUrl(title, site + link);
            }
        }

        /// <summary>
        /// 從頁面中尋找圖片網址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task findImageUrl(string title, string url)
        {
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var img = htmlDocument.DocumentNode.SelectNodes("//img[@id=\"bigImg\"]").FirstOrDefault();
            var imgSrc = img.Attributes["src"].Value;
            downlodImage(title, imgSrc);
        }

        /// <summary>
        /// 從圖片網址下載圖片
        /// </summary>
        /// <param name="title">資料夾名稱</param>
        /// <param name="imgSrc"></param>
        private static void downlodImage(string title, string imgSrc)
        {
            string fileName = Path.GetFileName(imgSrc);
            string localFilename = string.Format("{0}{1}\\{2}", downloadPath, title, fileName);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(imgSrc, localFilename);
            }
            Console.Write(string.Format("下載 {0} 完成\n", imgSrc));
        }
    }
}
