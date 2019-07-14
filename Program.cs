using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CoreTweet;
using System.Text;

namespace FlowCheck
{
    static class FlowCheck
    {

        [STAThread]
        static void Main()
        {
            new FCT();
            Application.Run();
        }

        class FCT : Form
        {
            //スレッド、タスクトレイアイコン、インスタンス配列の準備
            private Thread thread;
            private NotifyIcon icon;
            

            //スレッドを立ち上げる
            public FCT()
            {
                this.ShowInTaskbar = false;
                this.setComponents();

                thread = new Thread(intervalCheck);
                thread.Start();

                //初期開始メッセージ
                icon.BalloonTipTitle = "FlowChecker";
                icon.BalloonTipText = "処理を開始します";

                //メッセージ表示時間
                icon.ShowBalloonTip(5 * 1000);
            }

            //右クリックから終了を選んだ時の処理
            private void Close_Click(object sender, EventArgs e)
            {
                thread.Abort();
                Application.Exit();
            }

            //常駐時のSleepと流速計測および出力を行う
            private void intervalCheck()
            {
                //キーワード別のインスタンス配列を作成
                PrepareState[] psAr = new PrepareState[5];
                for (int i = 0; i < 5; i++)
                {
                    psAr[i] = new PrepareState(i);
                }

                //手動終了されるまで繰り返し
                while (true)
                {
                    //sleepはミリ秒指定なので1000で乗算
                    System.Threading.Thread.Sleep(FlowCheckTest.Properties.Settings.Default.SleepTime * 1000);

                    //インスタンス4つを順番に渡して実行する
                    for (int i = 0; i < psAr.Length; i++)
                    {
                        //流速を取得
                        int flowSpeed = getFlowSpeed(psAr[i]);

                        //流速が閾値を超えたらタスクトレイから出力
                        if (flowSpeed >= psAr[i].getThreshold())
                        {
                            dispNotify(createDispMsg(psAr[i], flowSpeed));
                        }
                        //流速が-1ならエラー表示
                        else if (flowSpeed == -1)
                        {
                            dispError();
                        }

                        //1ループごとに1分間のSleepを行う
                        System.Threading.Thread.Sleep(60 * 1000);
                    }
                }
            }

            //タスクトレイに追加するコンポーネントの設定
            private void setComponents()
            {
                icon = new NotifyIcon();
                icon.Icon = new Icon("app.ico");
                icon.Visible = true;
                icon.Text = "FlowChecker";
                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem menuItem = new ToolStripMenuItem();
                menuItem.Text = "&終了";
                menuItem.Click += new EventHandler(Close_Click);
                menu.Items.Add(menuItem);

                icon.ContextMenuStrip = menu;
            }

            //表示するメッセージ内容の作成処理
            private StringBuilder createDispMsg(PrepareState ps, int flowSpeed)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                sb.Append(ps.getKeyword());
                sb.Append("\"の流速が閾値を超過。\n流速: ");
                sb.Append(flowSpeed);
                sb.Append(" tweet/hour\n");
                sb.Append("取得率: ");
                sb.Append(ps.getPercentage());
                sb.Append("%");

                return sb;
            }

            //閾値超過時のメッセージ出力処理
            private void dispNotify(StringBuilder sb)
            {
                icon.BalloonTipTitle = "FlowChecker";
                icon.BalloonTipText = sb.ToString();

                //メッセージ表示時間
                icon.ShowBalloonTip(8 * 1000);
            }

            //Twitterの都合でツイートを1件も取得できなかった場合の処理
            private void dispError()
            {
                icon.BalloonTipTitle = "FlowChecker : Exception";
                icon.BalloonTipText = "ツイートを取得できませんでした";

                //メッセージ表示時間
                icon.ShowBalloonTip(8 * 1000);

                //アプリケーションの終了
                thread.Abort();
                Application.Exit();
            }

            //流速を求める処理
            private int getFlowSpeed(PrepareState ps)
            {
                //初期化
                SearchResult result = null;
                DateTimeOffset oldTimeStamp = new DateTimeOffset();

                try
                {
                    //検索実行
                    result = ps.getTokens().Search.Tweets(count => (int)ps.getTweetNum(), q => ps.getKeyword());

                    //末尾ツイートの時刻を取得
                    oldTimeStamp = getTimeStamp(result, ps);
                }
                catch (Exception)
                {
                    //取得件数が0件の場合は-1を返す
                    return -1;
                }

                //末尾ツイートのUNIXタイムを取得
                long unixTimeEnd = getUnixTimeEnd(oldTimeStamp);

                //現在のUNIXタイムを取得
                long unixTimeNow = getUnixTimeNow();

                //現在時刻と古いツイートの時刻の差分を求める
                long sec = unixTimeNow - unixTimeEnd;

                //流速計算(30分または1時間あたり）
                double flow = (ps.getTweetNum() / (double)sec) * ps.getPeriod();

                //流速を返す
                return (int)flow;

            }

            DateTimeOffset getTimeStamp(SearchResult result, PrepareState ps)
            {

                //謎の初期化
                DateTimeOffset oldTweetTimeStamp = new DateTimeOffset();
                int tweetCnt = 0;

                //ツイートだけ時刻を取得(取得漏れを考慮して最後尾は取得しない）
                foreach (var tweet in result)
                {
                    oldTweetTimeStamp = tweet.CreatedAt;
                    tweetCnt++;
                }

                //ツイート取得率算出のため、ツイート数をインスタンスへセット
                ps.calcPercentage(tweetCnt, (int)ps.getTweetNum());

                //1件もツイートが取得できなかった場合、Exceptionを投げる
                if (tweetCnt == 0)
                {
                    throw new Exception();
                }

                return oldTweetTimeStamp;
            }

            long getUnixTimeNow()
            {
                //UNIXエポック時刻
                DateTime dtUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                //現在時刻のDateTimeオブジェクト
                DateTime dt = DateTime.Now;

                //UTC時刻に変更
                dt = dt.ToUniversalTime();

                //現在時刻のDateTimeからUNIXエポック時刻のDateTimeを引いて、その結果を秒数で表す
                return (long)dt.Subtract(dtUnixEpoch).TotalSeconds;
            }
            long getUnixTimeEnd(DateTimeOffset old)
            {
                //UNIXエポック時刻
                DateTimeOffset dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

                //oldのDateTimeOffsetからUNIXエポック時刻のDateTimeOffsetを引いて、その結果を秒数で表す
                return (long)old.Subtract(dto).TotalSeconds;
            }

        }
    }
}
