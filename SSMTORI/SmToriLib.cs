using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using IcsComCtrl;
using IcsComUtil;
using IcsComDb;
using IcsComInfo;
using System.Threading.Tasks;

namespace SMTORI
{
    public class SmToriLib
    {
        public bool Show(IWin32Window cOwner, string sCcod, int nUcod)
        {
            //  
            string messageTitle = "";

            try
            {
                Global.sCcod = sCcod;
                Global.nUcod = nUcod;

                #region

                // ApplicationExitイベントハンドラを追加
                Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

                // アセンブリからプログラムID、プログラム名称、製品バージョンを取得して、
                // Globalクラスのメンバ変数に設定する。
                ComUtil.GetProgramInfo(ref Global.sPrgId, ref Global.sPrgName, ref Global.sPrgVer);

                //開始メッセージを表示する。
                WaitMsg.ICS_MessageWndOpen(ComUtil.sPrgName, WaitMsg.eMsgCode.MSG_START, null);

                //----------起動チェック----------
                // コマンドライン引数から会社コード、ユーザIDを取得して、Globalクラスのメンバ変数に設定する。
                if (!Global.SetArgArray(System.Environment.GetCommandLineArgs()))
                {
                    WaitMsg.ICS_MessageWndClose();

                    // エラーの場合
                    MessageBox.Show(
//-- <2016/03/22>
//                        "ＩＣＳ財務メニューから起動してください。",
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "ＩＣＳ財務メニューから起動してください。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                    Application.Exit();
                    return false;
                }

                //開始メッセージを表示する。
                WaitMsg.ICS_MessageWndOpen(ComUtil.sPrgName, WaitMsg.eMsgCode.MSG_START, null);

                //インストール情報取得
                if (!ComInfo.GetInstallMDDir(out Global.sMMDir))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
//-- <2016/03/22>
//                        "インストール情報の取得に失敗しました。\n業務を終了します。",
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "インストール情報の取得に失敗しました。\n業務を終了します。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                    Application.Exit();
                    return false;
                }

                string sErrMsg;
                // 基本情報を取得し、引数で指定した変数に格納する。
                if (!ComInfo.IcsGetCommonInfo(
                    Global.cUsrTbl, Global.cUsrSec, Global.cKaisya, Global.cGengo, Global.cKankyo,
                    Global.nUcod, Global.sCcod, out sErrMsg))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
//-- <2016/03/22>
//                        sErrMsg + "\n業務を終了します。",
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        sErrMsg + "\n業務を終了します。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                    Application.Exit();
                    return false;
                }

                //----------多重起動チェック----------
                // Mutexクラスの作成
                System.Threading.Mutex cMutex = new System.Threading.Mutex(false, Global.sPrgId);
                // ミューテックスの所有権を要求する
                if (cMutex.WaitOne(0, false) == false)
                {
                    WaitMsg.ICS_MessageWndClose();

                    // すでに起動していると判断して終了
                    MessageBox.Show(
//-- <2016/03/22>
//                        "この業務は既に起動されています。",
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "この業務は既に起動されています。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                    Application.Exit();
                    return false;
                }

                // データベース関係オブジェクト生成

                // 接続
                // 共通DB接続
                Global.cConCommon = DbCls.CreateConnectionC(Global.nUcod);
                // 会社DB接続
                Global.cConKaisya = DbCls.CreateConnectionZ(Global.nUcod, Global.sCcod);

                Global.cCmdCommonSel = DbCls.CreateCommandObject(ref Global.cConCommon);
                Global.cCmdSel = DbCls.CreateCommandObject(ref Global.cConKaisya);
                Global.cCmdIns = DbCls.CreateCommandObject(ref Global.cConKaisya);
                Global.cCmdDel = DbCls.CreateCommandObject(ref Global.cConKaisya);

                // DB接続文字列の設定。
//-- <2016/03/22>
//                string sConnectionString =
//                    "dsn=ICSP_Z" + Global.sCcod + ";" +
//                    "UID=IUSER" + String.Format("{0:0000}", Global.nUcod) + ";" +
//                    "PWD=de2_IO@" + String.Format("{0:0000}", Global.nUcod) + ";";
//-- <2016/03/22>
                // 基本情報を取得し、引数で指定した変数に格納する。
                if (!ComInfo.IcsGetCommonInfo(
                    Global.cUsrTbl, Global.cUsrSec, Global.cKaisya, Global.cGengo, Global.cKankyo,
                    Global.nUcod, Global.sCcod, out sErrMsg))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
//-- <2016/03/22>
//                        sErrMsg + "\n業務を終了します。",
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        sErrMsg + "\n業務を終了します。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
//-- <2016/03/22>
                    Application.Exit();
                    return false;
                }
                #endregion



                // メインフォーム起動
                Global.bUpdated = false;
                using (frmSMTORI frm = new frmSMTORI())
                {
                    //frm.ShowDialog();
                    frm.ShowDialog(cOwner);
                }
                
                return Global.bUpdated;
            }

            catch (Exception ex)
            {
                // 例外発生時は、エラーメッセージを表示してプログラム終了
                MessageBox.Show(ex.Message + "業務を終了します。", messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Global.bUpdated;
            }

            finally
            {
            }
        }

        /// <summary>
        /// アプリケーション終了時の処理を記述するイベントハンドラメソッドです。 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // データベース関係オブジェクト破棄
            if (Global.cConKaisya != null)
            {
                Global.cConKaisya.Dispose();
            }
            if (Global.cConCommon != null)
            {
                Global.cConCommon.Dispose();
            }
        }
    }
}
