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

        //キーワード毎の流速閾値
        private int[] thresholdAr = { FlowCheckTest.Properties.Settings.Default.Threshold1,
                                      FlowCheckTest.Properties.Settings.Default.Threshold2,
                                      FlowCheckTest.Properties.Settings.Default.Threshold3,
                                      FlowCheckTest.Properties.Settings.Default.Threshold4,
                                      FlowCheckTest.Properties.Settings.Default.Threshold5};

        //検索キーワード
        private string keyword;

        //流速閾値
        private int threshold;

        //計測直後の流速
        private int flowSpeed;

        //前回計測時の流速の差
        private int previousFlow;

        //ツイート取得率(取得件数 / Twitterにリクエストした件数)
        private double percentage;

        //コンストラクタの引数で設定ファイルのキーワードと閾値が代入される
        public PrepareState(int num)
        {
            this.keyword = keywordAr[num];
            this.threshold = thresholdAr[num];
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

        //設定ファイルから検索キーワードを返す
        public string getKeyword()
        {
            return keyword;
        }

        //設定ファイルからツイート取得件数を返す
        public double getTweetNum()
        {
            return FlowCheckTest.Properties.Settings.Default.getTweetNum;
        }

        //設定ファイルから流速の計測時間(秒数)を返す
        public int getPeriod()
        {
            return FlowCheckTest.Properties.Settings.Default.period;
        }

        //設定ファイルからsleepTimeを返す
        public int getSleepTime()
        {
            return FlowCheckTest.Properties.Settings.Default.SleepTime;
        }

        //キーワードに紐付く閾値を返す
        public int getThreshold()
        {
            return threshold;
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

        //流速をセットする
        public void setFlowSpeed(int flowSpeed) {
            this.flowSpeed = flowSpeed;
        }

        //流速を返す
        public int getFlowSpeed() {
            return flowSpeed;
        }

        //前回計測時の流速との差分を返す
        public int getDifference(int flowSpeed) {
            return flowSpeed - previousFlow;
        }

        //過去の流速を代入する
        public void setPreviousFlow(int flowSpeed) {
            previousFlow = flowSpeed;
        }
    }
}
