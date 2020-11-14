using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ImgDownloader
{
    class Program
    {
        static void Main(string[] args)
        {

            #region downloader
            //Dictionary<string,string> urlList = new Dictionary<string, string>();

            ////urlList.Add("Cardinals",@"https://static.www.nfl.com/image/private/f_auto/league/u9fltoslqdsyao8cpm0k");
            ////urlList.Add("Panthers",@"https://static.www.nfl.com/image/private/f_auto/league/ervfzgrqdpnc7lh5gqwq");
            ////urlList.Add("Bears",@"https://static.www.nfl.com/image/private/f_auto/league/ra0poq2ivwyahbaq86d2");
            ////urlList.Add("Cowboys",@"https://static.www.nfl.com/image/private/f_auto/league/ieid8hoygzdlmzo0tnf6");
            ////urlList.Add("Lions",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/ocvxwnapdvwevupe4tpr");
            ////urlList.Add("Packers",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/gppfvr7n8gljgjaqux2x");
            ////urlList.Add("Rams",@"https://static.www.nfl.com/image/private/f_auto/league/ayvwcmluj2ohkdlbiegi");
            ////urlList.Add("Vikings",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/teguylrnqqmfcwxvcmmz");
            ////urlList.Add("Saints",@"https://static.www.nfl.com/image/private/f_auto/league/grhjkahghjkk17v43hdx");
            ////urlList.Add("Giants",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/t6mhdmgizi6qhndh8b9p");
            ////urlList.Add("Eagles",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/puhrqgj71gobgdkdo6uq");
            ////urlList.Add("49ers",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/dxibuyxbk0b9ua5ih9hn");
            ////urlList.Add("Buccaneers",@"https://static.www.nfl.com/image/private/f_auto/league/v8uqiualryypwqgvwcih");
            ////urlList.Add("Washington",@"https://static.www.nfl.com/image/private/f_auto/league/ywoi3t4jja8fokqpyegk");
            ////urlList.Add("Ravens",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/ucsdijmddsqcj1i9tddd");
            ////urlList.Add("Bills",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/giphcy6ie9mxbnldntsf");
            ////urlList.Add("Bengals",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/okxpteoliyayufypqalq");
            ////urlList.Add("Browns",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/grxy59mqoflnksp2kocc");
            ////urlList.Add("Broncos",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/t0p7m5cjdjy18rnzzqbx");
            ////urlList.Add("Texans",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/bpx88i8nw4nnabuq0oob");
            ////urlList.Add("Colts",@"https://static.www.nfl.com/image/private/f_auto/league/ketwqeuschqzjsllbid5");
            ////urlList.Add("Jaguars",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/qycbib6ivrm9dqaexryk");
            ////urlList.Add("Chiefs",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/ujshjqvmnxce8m4obmvs");
            ////urlList.Add("Raiders",@"https://static.www.nfl.com/image/private/f_auto/league/gzcojbzcyjgubgyb6xf2");
            ////urlList.Add("Chargers",@"https://static.www.nfl.com/image/private/f_auto/league/dhfidtn8jrumakbogeu4");
            ////urlList.Add("Dolphins",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/lits6p8ycthy9to70bnt");
            ////urlList.Add("Patriots",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/moyfxx3dq5pio4aiftnc");
            ////urlList.Add("Jets",@"https://static.www.nfl.com/image/private/f_auto/league/ekijosiae96gektbo4iw");
            ////urlList.Add("Steelers",@"https://res.cloudinary.com/nflleague/image/private/f_auto/league/xujg9t3t4u5nmjgr54wx");
            ////urlList.Add("Titans",@"https://static.www.nfl.com/image/private/f_auto/league/pln44vuzugjgipyidsre");
            ////urlList.Add("Falcons", @"https://static.www.nfl.com/image/private/f_auto/league/d8m7hzpsbrl6pnqht8op");
            ////urlList.Add("Seahawks", @"https://res.cloudinary.com/nflleague/image/private/f_auto/league/gcytzwpjdzbpwnwxincg");



            //foreach (var url in urlList)
            //{
            //    using (var client = new WebClient())
            //    {
            //        client.DownloadFile(url.Value, $"{url.Key}.png");
            //    }
            //}
            #endregion

            #region
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.png"))
            {
                //var tempName =file.ToLowerInvariant();
                //File.Move(file, tempName);
                File.Move(file, file.ToLowerInvariant());
            }
            #endregion
        }
    }
}
