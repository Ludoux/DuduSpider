﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ludoux.DuduSpider
{
    class HomeTimeline
    {
        const int API = 7;
        /// <summary>
        /// 抓取并保存（.html）最新的首页消息
        /// </summary>
        /// <param name="authorization">知乎校验用户身份</param>
        /// <param name="clist">之前抓取时保存的 List Cell（以不抓取这部分），若初始化则抓取保存全部</param>
        /// /// <returns>仅包括这次抓取的内容{ List Story, List Cell }</returns>
        public static object[] FetchHomeTimeline(string authorization, List<Cell> clist, List<Uri> allowedUrlHost)
        {
            
            List<Story> newSList = new List<Story>();//这次需要抓取的
            List<Cell> newCList = fetchCellListOnly(authorization);
            newCList = newCList.Except(clist).ToList();//在抓取到的首页 cell 中删去抓取过的内容，余下的就是这次需要抓取的
            return new object[]{fetch(authorization, newCList, allowedUrlHost), newCList};
        }
        /// <summary>
        ///抓取并保存（.html）提供的 List 列表的消息，内部方法
        /// </summary>
        /// <param name="authorization">知乎校验用户身份</param>
        /// <param name="clist">抓取提供的 List</param>
        /// <returns>仅包括这次抓取的内容</returns>
        private static List<Story> fetch(string authorization, List<Cell> clist, List<Uri> allowedUrlHost)
        {
            List<Cell> cellList = clist; ;
            List<Story> storyList = new List<Story>();

            int i = 0;
            foreach (Cell c in cellList)
            {
                LogWriter.WriteLine("[" + i++.ToString() + "]" + c.Title);
                storyList.Add(new Story(HttpRequest.DownloadString(string.Format(@"https://news-at.zhihu.com/api/{0}/story/{1}", API, c.Id)), allowedUrlHost, "首页流"));
            }

            List<Story> notEmptyStoryList = new List<Story>();
            foreach (Story s in storyList)
            {
                if (s.Manifest[0].Count > 0 && s.Manifest[0][0] != "")//为空的就是前面获取时直接 return 的文章，不进入制作电子书的环节
                    notEmptyStoryList.Add(s);
            }
            return notEmptyStoryList;
        }
        /// <summary>
        /// 仅抓取首页的 Cell List，内部方法
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        private static List<Cell> fetchCellListOnly(string authorization)
        {
            List<Cell> cellList = new List<Cell>();
            string webSource = HttpRequest.DownloadString(string.Format(@"https://news-at.zhihu.com/api/{0}/home_timeline", API), authorization: authorization);
            Regex r = new Regex(@"""title"":""(?<title>.*?)"",.*?""time"":(?<time>\d*?),[^{]*?id"":(?<id>\d*?)(}|,[^{]*?)");
            MatchCollection collection = r.Matches(webSource);
            foreach (Match m in collection)
            {
                cellList.Add(new Cell(Convert.ToInt32(m.Groups["time"].Value), Convert.ToInt32(m.Groups["id"].Value), m.Groups["title"].Value));
            }
            return cellList;
        }
        
    }
}
