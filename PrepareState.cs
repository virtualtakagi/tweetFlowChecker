using CoreTweet;
using System;

namespace FlowCheck
{
    public class PrepareState
    {
        //設定ファイルの検索キーワード
        private string[] keywordAr = {FlowCheckTest.Properties.Settings.Default.keyword1,
                                      FlowCheckTest.Properties.Settings.Default.keyword2,
                                      FlowCheckTest.Properties.Settings.Default.keyword3,
                                      FlowCheckTest.Properties.Settings.Default.keyword4,
                                      FlowCheckTest.Properties.Settings.Default.keyword5};

        //検索キーワード
        private string keyword;

        //ツイート取得率(取得件数 / Twitterにリクエストした件数)
        private double percentage;

        //0～4の引数に設定ファイルのキーワードが代入される
        public PrepareState(int num)
        {
            this.keyword = keywordAr[num];
        }

        //設定ファイルからAPIキーを設定しアクセストークンの取得
        public Tokens getTokens()
        {
            Tokens tokens = CoreTweet.Tokens.Create(FlowCheckTest.Properties.Settings.Default.API_KEY,
                FlowCheckTest.Properties.Settings.Default.API_SECRET,
                FlowCheckTest.Properties.Settings.Default.CONSUMER_KEY,
                FlowCheckTest.Properties.Settings.Default.CONSUMER_SECRET);

            return tokens;
        }

        //設定ファイルから検索キーワードを設定
        public string getKeyword()
        {
            return keyword;
        }

        //設定ファイルからツイート取得件数を設定
        public double getTweetNum()
        {
            return FlowCheckTest.Properties.Settings.Default.getTweetNum;
        }

        //設定ファイルから流速の計測時間(秒数)を設定
        public int getPeriod()
        {
            return FlowCheckTest.Properties.Settings.Default.SearchIntervalTime;
        }

        //設定ファイルからsleepTimeを設定
        public int getSleepTime()
        {
            return FlowCheckTest.Properties.Settings.Default.SleepTime;
        }

        //設定ファイルから閾値を設定
        public int getThreshold()
        {
            return FlowCheckTest.Properties.Settings.Default.Threshold;
        }

        //ツイート取得率を計算する
        public void calcPercentage(int cnt, int num)
        {
            percentage = Math.Floor((double)cnt / (double)num * 100);
        }

        //ツイート取得率を返す
        public int getPercentage()
        {
            return (int)percentage;
        }
    }
}
