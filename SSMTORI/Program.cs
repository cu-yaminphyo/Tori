using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.Data.Common;
using IcsComCtrl;
using IcsComUtil;
using IcsComDb;
using IcsComInfo;
using IcsComProxy;

namespace SMTORI
{
    static class Program
    {
        internal const int mnSaimuID = 260;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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
                        //-- <2016/02/17 文言等>
                        //                        "ＩＣＳ財務メニューから起動してください。",
                        //                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "ＩＣＳ財務メニューから起動してください。" + "\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //-- <2016/02/17>
                    Application.Exit();
                    return;
                }

                //開始メッセージを表示する。
                WaitMsg.ICS_MessageWndOpen(ComUtil.sPrgName, WaitMsg.eMsgCode.MSG_START, null);

                //インストール情報取得
                if (!ComInfo.GetInstallMDDir(out Global.sMMDir))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
                        //-- <2016/02/17 文言等>
                        //                        "インストール情報の取得に失敗しました。\n業務を終了します。",
                        //                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "インストール情報の取得に失敗しました。\n業務を終了します。" + "\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //-- <2016/02/17>
                    Application.Exit();
                    return;
                }

                string sErrMsg;
                // 基本情報を取得し、引数で指定した変数に格納する。
                if (!ComInfo.IcsGetCommonInfo(
                    Global.cUsrTbl, Global.cUsrSec, Global.cKaisya, Global.cGengo, Global.cKankyo,
                    Global.nUcod, Global.sCcod, out sErrMsg))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
                        //-- <2016/02/17 文言等>
                        //                        sErrMsg + "\n業務を終了します。",
                        //                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        sErrMsg + "\n業務を終了します。" + "\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //-- <2016/02/17>
                    Application.Exit();
                    return;
                }

                //----------多重起動チェック----------
                // Mutexクラスの作成
                // ---> V02.23.02 KSM UPDATE ▼(mutexでの多重起動チェックの実装対応)
                //System.Threading.Mutex cMutex = new System.Threading.Mutex(false, Global.sPrgId);
                // ミューテックスの所有権を要求する
                //if (cMutex.WaitOne(0, false) == false)
                Global.cMutex = new System.Threading.Mutex(false, "MutexSSMTORI");
                if (Global.cMutex.WaitOne(0, false) == false)
                // <--- V02.23.02 KSM UPDATE ▲(mutexでの多重起動チェックの実装対応)
                {
                    WaitMsg.ICS_MessageWndClose();

                    // すでに起動していると判断して終了
                    MessageBox.Show(
                        //-- <2016/02/17 文言等>
                        //                        "この業務は既に起動されています。",
                        //                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "この業務は既に起動されています。" + "\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //-- <2016/02/17>
                    Application.Exit();
                    return;
                }

                Global.sZJoin = (ComUtil.IsPostgreSQL()) ? "" :"ICSP_312Z" + Global.sCcod + "..";//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】
                // データベース関係オブジェクト生成

                //-- <2016/02/17 メッセージを追加>
                try
                {
                    //-- <2016/02/17>
                    // 接続
                    // 共通DB接続
                    Global.cConCommon = DbCls.CreateConnectionC(Global.nUcod);
                    // ---> V02.26.01 AKYM DELETE ▼(No.111969)
                    // 会社DB接続
                    //Global.cConKaisya = DbCls.CreateConnectionZ(Global.nUcod, Global.sCcod);
                    // 会社DB接続(SS)
                    //Global.cConSaikenSaimu = DbCls.CreateConnectionRVT(Global.nUcod, Global.sCcod);

                    //Global.cCmdCommonSel = DbCls.CreateCommandObject(ref Global.cConCommon);
                    //Global.cCmdSelZ = DbCls.CreateCommandObject(ref Global.cConKaisya);
                    //Global.cCmdSel = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    //Global.cCmdInsZ = DbCls.CreateCommandObject(ref Global.cConKaisya);
                    //Global.cCmdIns = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    //Global.cCmdDel = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                    // <--- V02.26.01 AKYM DELETE ▲(No.111969)
                    //-- <2016/02/17 メッセージを追加>
                }
                catch
                {
                    WaitMsg.ICS_MessageWndClose();
                    MessageBox.Show(
                        "会社情報(共通)の取得に失敗しました。\n業務を終了します。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Application.Exit();
                    return;
                }

                // ---> V02.26.01 AKYM ADD ▼(No.111969)
                // 会社DB接続
                Global.cConKaisya = DbCls.CreateConnectionZ(Global.nUcod, Global.sCcod);
                if (IcsSSSInfo.SSSInfo.ChkKsubsysSS(Global.sCcod, Global.cConCommon, Global.sPrgName, true, true, true, true, true) == false)
                {
                    Application.Exit();
                    return;
                }
                // 会社DB接続(SS)
                Global.cConSaikenSaimu = DbCls.CreateConnectionRVT(Global.nUcod, Global.sCcod);

                Global.cCmdCommonSel = DbCls.CreateCommandObject(ref Global.cConCommon);
                Global.cCmdSelZ = DbCls.CreateCommandObject(ref Global.cConKaisya);
                Global.cCmdSel = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                Global.cCmdInsZ = DbCls.CreateCommandObject(ref Global.cConKaisya);
                Global.cCmdIns = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                Global.cCmdDel = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
                // <--- V02.26.01 AKYM ADD ▲(No.111969)
                //-- <2016/02/17>
                // DB接続文字列の設定。
                //string sConnectionString =
                //    "dsn=ICSP_Z" + Global.sCcod + ";" +
                //    "UID=IUSER" + String.Format("{0:0000}", Global.nUcod) + ";" +
                //    "PWD=de2_IO@" + String.Format("{0:0000}", Global.nUcod) + ";";

                // サブシステム使用チェック
                //string sResult = SubSystemUseCheck();
                //if (sResult.Equals("NO_EXIST"))
                //-- <2016/02/17 見直し>
                #region 見直し
                //if (IcsSSSInfo.SSSInfo.ChkKsubsysSS(Global.sCcod, Global.cConCommon, Global.sPrgName, false, true, true, true, true) == false)
                //{
                //    WaitMsg.ICS_MessageWndClose();

                //    MessageBox.Show(
                //        "債務支払管理システムを\n使用しない設定になっています。",
                //        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    Application.Exit();
                //    return;
                //}

                //// 基本情報を取得し、引数で指定した変数に格納する。
                //if (!ComInfo.IcsGetCommonInfo(
                //    Global.cUsrTbl, Global.cUsrSec, Global.cKaisya, Global.cGengo, Global.cKankyo,
                //    Global.nUcod, Global.sCcod, out sErrMsg))
                //{
                //    WaitMsg.ICS_MessageWndClose();

                //    MessageBox.Show(
                //        sErrMsg + "\n業務を終了します。",
                //        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //    Application.Exit();
                //    return;
                //}

                //// 会社登録チェック
                //string sResult = KaisyaTourokuCheck();
                //if (sResult.Equals("NO_EXIST"))
                //{
                //    WaitMsg.ICS_MessageWndClose();

                //    MessageBox.Show(
                //        "債権債務の会社登録が行われていません。",
                //        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    Application.Exit();
                //    return;
                //}

                //if (Global.nSAIKEN_F == 1)
                //{
                //    // 管理マスター登録チェック
                //    sResult = KanriTourokuCheck_Saiken();
                //    if (sResult.Equals("NO_EXIST"))
                //    {
                //        WaitMsg.ICS_MessageWndClose();

                //        MessageBox.Show(
                //            "管理テーブル（債権）の登録が行われていません。",
                //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        Application.Exit();
                //        return;
                //    }
                //}

                //Global.nSAIKEN_F_USec = 0;
                //Global.nSAIMU_F_USec = 0;
                //Global.nKIJITU_F_USec = 0;

                //Global.nGroup = 0;

                //if (Global.nSAIMU_F == 1)
                //{
                //    // 管理マスター登録チェック
                //    sResult = KanriTourokuCheck_Saimu();
                //    if (sResult.Equals("NO_EXIST"))
                //    {
                //        WaitMsg.ICS_MessageWndClose();

                //        MessageBox.Show(
                //            "管理テーブル（債務）の登録が行われていません。",
                //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        Application.Exit();
                //        return;
                //    }

                //    // 債務管理マスターよりセキュリティ使用情報を取得する
                //    int nIdata1;
                //    decimal nIdata2;
                //    string sCdata;
                //    Get_SS_KANRI(1, "F_SECU", out nIdata1, out nIdata2, out sCdata);
                //    if (nIdata1 == 1)
                //    {
                //        sResult = SecTourokuCheck();
                //        if (sResult.Equals("NO_EXIST"))
                //        {
                //            WaitMsg.ICS_MessageWndClose();

                //            MessageBox.Show(
                //                "取引先詳細情報登録の使用が許可されていません。",
                //                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //            Application.Exit();
                //            return;
                //        }
                //        if (Global.nSAIMU_F_USec == 1)
                //        {
                //            Get_SS_KANRI(1, "TRGRP_F", out nIdata1, out nIdata2, out sCdata);
                //            if (nIdata1 == 1)
                //            {
                //                Global.nGroup = 1;
                //            }
                //        }
                //    }
                //}


                ////if(!IcsComProxy.ProxyCOMMONLIBV.UserSecForPrg(Global.nUcod, Global.sCcod, "260", "TRMAKE", Global.cUsrTbl.nMSEC))
                ////{
                ////    MessageBox.Show(
                ////        "使用権限がありません。\n業務を終了します。",
                ////        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                ////    Application.Exit();
                ////    return;
                ////}

                //Global.bZaimBoot = IcsComProxy.ProxyCOMMONLIBV.UserSecForPrg(Global.nUcod, Global.sCcod, "050", "TRMAKE", Global.cUsrTbl.nMSEC);
                #endregion
                // ---> V02.26.01 AKYM DELETE ▼(No.111969)
                //if (IcsSSSInfo.SSSInfo.ChkKsubsysSS(Global.sCcod, Global.cConCommon, Global.sPrgName, true, true, true, true, true) == false)
                //{
                //Application.Exit();
                //return;
                //}
                // <--- V02.26.01 AKYM DELETE ▲(No.111969)
                IcsSSSInfo.SSSInfo.GetKsubsysSS(Global.sCcod, Global.cConCommon, out Global.bSub801, out Global.bSub802, out Global.bSub803, out Global.bSub804);

                // 基本情報を取得し、引数で指定した変数に格納する。
                if (!ComInfo.IcsGetCommonInfo(
                    Global.cUsrTbl, Global.cUsrSec, Global.cKaisya, Global.cGengo, Global.cKankyo,
                    Global.nUcod, Global.sCcod, out sErrMsg))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
                        sErrMsg + "\n業務を終了します。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Application.Exit();
                    return;
                }

                // 会社登録チェック  取りあえずこのメッセージは共通ライブラリで感知しているので無用なのですがフラグを読み取っているので残します。
                string sResult = KaisyaTourokuCheck();
                if (sResult.Equals("NO_EXIST"))
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
                        "債権債務の会社登録が行われていません。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                if (Global.nSAIKEN_F == 1)
                {
                    // 管理マスター登録チェック
                    sResult = KanriTourokuCheck_Saiken();
                    if (sResult.Equals("NO_EXIST"))
                    {
                        // Ver.01.02.01 [SS_4091]対応 Toda -->
                        //WaitMsg.ICS_MessageWndClose();

                        //MessageBox.Show(
                        //    "管理テーブル（債権）の登録が行われていません。\n\nVer" + Global.sPrgVer,
                        //    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //Application.Exit();
                        //return;
                        Global.nSAIKEN_F = 0;
                        // Ver.01.02.01 <--
                    }
                    //--->V01.12.01 ATT ADD ▼ (8084)
                    else
                    {
                        Get_TBLKanriIdata();
                    }
                    //--->V01.12.01 ATT ADD ▲ (8084)
                }

                Global.nSAIKEN_F_USec = 0;
                Global.nSAIMU_F_USec = 0;
                Global.nKIJITU_F_USec = 0;

                Global.nGroup = 0;

                if (Global.nSAIMU_F == 1)
                {
                    // 管理マスター登録チェック
                    sResult = KanriTourokuCheck_Saimu(1);
                    if (sResult.Equals("NO_EXIST"))
                    {
                        // Ver.01.02.01 [SS_4091]対応 Toda -->
                        //WaitMsg.ICS_MessageWndClose();

                        //MessageBox.Show(
                        //    "管理マスター(債務)の登録がされていません。\n\nVer" + Global.sPrgVer,
                        //    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //Application.Exit();
                        //return;
                        Global.nSAIMU_F = 0;
                        // Ver.01.02.01 <--
                    }

                    //// 債務管理マスターよりセキュリティ使用情報を取得する
                    //int nIdata1;
                    //decimal nIdata2;
                    //string sCdata;
                    //Get_SS_KANRI(1, "F_SECU", out nIdata1, out nIdata2, out sCdata);
                    //if (nIdata1 == 1)
                    //{
                    //    sResult = SecTourokuCheck();
                    //    if (sResult.Equals("NO_EXIST"))
                    //    {
                    //        WaitMsg.ICS_MessageWndClose();

                    //        MessageBox.Show(
                    //            "取引先詳細情報登録の使用が許可されていません。",
                    //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //        Application.Exit();
                    //        return;
                    //    }
                    //    if (Global.nSAIMU_F_USec == 1)
                    //    {
                    //        Get_SS_KANRI(1, "TRGRP_F", out nIdata1, out nIdata2, out sCdata);
                    //        if (nIdata1 == 1)
                    //        {
                    //            Global.nGroup = 1;
                    //        }
                    //    }
                    //}
                }
                if (Global.nKIJITU_F == 1)
                {
                    // 管理マスター登録チェック
                    sResult = KanriTourokuCheck_Saimu(2);
                    if (sResult.Equals("NO_EXIST"))
                    {
                        // Ver.01.02.01 [SS_4091]対応 Toda -->
                        //WaitMsg.ICS_MessageWndClose();

                        //MessageBox.Show(
                        //    "管理マスター(期日)の登録がされていません。\n\nVer" + Global.sPrgVer,
                        //    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //Application.Exit();
                        //return;
                        Global.nKIJITU_F = 0;
                        // Ver.01.02.01 <--
                    }
                }
                // Ver.01.02.01 [SS_4091]対応 Toda -->
                if (Global.nSAIKEN_F == 0 && Global.nSAIMU_F == 0 && Global.nKIJITU_F == 0)
                {
                    WaitMsg.ICS_MessageWndClose();

                    MessageBox.Show(
                        "管理マスターが登録されていません。\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
                // Ver.01.02.01 <--

                if (!IcsComProxy.ProxyCOMMONLIBV.UserSecForPrg(Global.nUcod, Global.sCcod, "050", "TRMAKE", Global.cUsrTbl.nMSEC))
                {
                    // Ver.00.01.11 [SS_4056] Toda -->
                    //WaitMsg.ICS_MessageWndClose();

                    //MessageBox.Show(
                    //    "使用権限がありません。\n業務を終了します。\n\nVer" + Global.sPrgVer,
                    //    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //Application.Exit();
                    //return;
                    Global.bZaimBoot = false;
                    // Ver.00.01.11 <--
                }
                else { Global.bZaimBoot = true; }

                //-- <2016/03/08>
                // 債務管理マスターよりセキュリティ使用情報を取得する
                int nIdata1;
                decimal nIdata2;
                string sCdata;
                Get_SS_KANRI(1, "F_SECU", out nIdata1, out nIdata2, out sCdata);
                if (nIdata1 == 1)
                {
                    sResult = SecTourokuCheck();

                    Get_SS_KANRI(1, "TRGRP_F", out nIdata1, out nIdata2, out sCdata);
                    if (nIdata1 == 1)
                    {
                        Global.nGroup = 1;
                    }
                }


                //-- <2016/02/17>
                #region

                //ICSLIBV.Var.gnUser = Global.nUcod;
                //ICSLIBV.Var.gstrCCOD = Global.sCcod;
                //ICSLIBV.Var.gstrPgmID = Global.sPrgId;
                //ICSLIBV.Var.gstrPgmName = Global.sPrgName;

                #endregion


                //債権債務DBの更新
                if (IcsComProxy.ProxyDBACCLIBSSV.DB_TableAddSS(Global.nUcod, Global.sCcod) == false)
                {
                    MessageBox.Show(
                            "債権債務管理DBの更新に失敗しました。\n\nVer" + Global.sPrgVer,
                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }


                Application.Run(new frmSMTORI());
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
                    //-- <2016/03/22>
                    //                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
                    //                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //-- <2016/03/22>
            }
            finally
            {
                //メッセージを閉じる
                WaitMsg.ICS_MessageWndClose();
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


        /// <summary>
        /// サブシステム使用チェック
        /// </summary>
        /// <returns>
        /// 結果
        /// "EXIST" --- レコードが存在する
        /// "NO_EXIST" --- レコードが存在しない
        /// "ERROR" --- エラーが発生
        /// </returns>
        static string SubSystemUseCheck()
        {
            string sResult = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                Global.cCmdCommonSel.CommandText = "SELECT * FROM KSUBSYS";
                Global.cCmdCommonSel.CommandText += " WHERE CCOD = :p AND SUBID = :p ";
                // パラメータクリア
                Global.cCmdCommonSel.Parameters.Clear();
                // パラメータセット
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@CCOD", Global.sCcod);
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@SUBID", mnSaimuID);
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdCommonSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == false)
                {
                    sResult = "NO_EXIST";
                }
                else
                {
                    sResult = "EXIST";
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                sResult = "ERROR";
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
            return sResult;
        }

        /// <summary>
        /// 会社登録チェック
        /// </summary>
        /// <returns>
        /// 結果
        /// "EXIST" --- レコードが存在する
        /// "NO_EXIST" --- レコードが存在しない
        /// "ERROR" --- エラーが発生
        /// </returns>
        static string KaisyaTourokuCheck()
        {
            string sResult = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                Global.cCmdSel.CommandText = "SELECT * FROM SS_VOLUM ";
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == false)
                {
                    sResult = "NO_EXIST";

                    Global.nTRCD_HJ = 0;
                    Global.nKIJITU_F = 0;
                }
                else
                {
                    sResult = "EXIST";
                    Global.gcDataReader.Read();

//-- <2016/02/17 見直し>
//                    Global.nTRCD_HJ = DbCls.GetNumNullZero<int>(Global.gcDataReader["TRCD_HJ"]);    // SS_VOLUM(補助コード使用)
//                    Global.nSAIKEN_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIKEN"]);  // SS_VOLUM(債権管理）
//                    Global.nSAIMU_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIMU"]);    // SS_VOLUM(債務管理）
//                    Global.nKIJITU_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_KJTKAN"]);  // SS_VOLUM(期日管理）
////-- <2016/02/14 相殺使用と外貨使用を取得しておく>
//                    Global.nSOSAI_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SOSAI"]);    // SS_VOLUM(相殺管理）
//                    Global.nGAIKA_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["GAIKA_F"]);    // SS_VOLUM(外貨使用）
////-- <2016/02/14>
                    Global.nSAIKEN_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIKEN"]);  // SS_VOLUM(債権管理）
                    if (Global.nSAIKEN_F == 1 && !Global.bSub801)
                    { Global.nSAIKEN_F = 0; }
                    Global.nSAIMU_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIMU"]);    // SS_VOLUM(債務管理）
                    if (Global.nSAIMU_F == 1 && !Global.bSub802)
                    { Global.nSAIMU_F = 0; }
                    Global.nKIJITU_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_KJTKAN"]);  // SS_VOLUM(期日管理）
                    if (Global.nKIJITU_F == 1 && !Global.bSub804)
                    { Global.nKIJITU_F = 0; }
                    Global.nSOSAI_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SOSAI"]);    // SS_VOLUM(相殺管理）
                    if (Global.nSOSAI_F == 1 && !Global.bSub803)
                    { Global.nSOSAI_F = 0; }
                    Global.nTRCD_HJ = DbCls.GetNumNullZero<int>(Global.gcDataReader["TRCD_HJ"]);    // SS_VOLUM(補助コード使用)
                    Global.nGAIKA_F = DbCls.GetNumNullZero<int>(Global.gcDataReader["GAIKA_F"]);    // SS_VOLUM(外貨使用）
//-- <2016/02/17>
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                sResult = "ERROR";
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
            return sResult;
        }

        internal static string KanriTourokuCheck_Saiken()
        {
            string sResult = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                Global.cCmdSel.CommandText = "SELECT * FROM TBLKANRI ";
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == false)
                {
                    sResult = "NO_EXIST";
                }
                else
                {
                    sResult = "EXIST";
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                sResult = "ERROR";
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
            return sResult;
        }
        //--->V01.12.01 ATT ADD ▼ (8084)
        internal static void Get_TBLKanriIdata()
        {
            try
            {
                Global.cCmdSel.CommandText = "SELECT KANRIDATA FROM TBLKANRI WHERE KANRICD = '専用入金口座を利用する'";

                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
                if (Global.gcDataReader.HasRows == true)
                {
                    Global.gcDataReader.Read();
                    Global.nKanri_IDATA = DbCls.GetNumNullZero<int>(Global.gcDataReader["KANRIDATA"]);
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
        }
        //<---V01.12.01 ATT ADD ▲ (8084)

//-- <2016/02/17 見直し>
        //internal static string KanriTourokuCheck_Saimu()
        //{
        //    string sResult = "";
        //    try
        //    {
        //        // コマンドオブジェクトにSQL文セット
        //        Global.cCmdSel.CommandText = "SELECT * FROM SS_KANRI ";
        //        // データリーダーでデータを取得する。
        //        DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

        //        // データリーダーに１行以上の行が格納されているかチェック。
        //        if (Global.gcDataReader.HasRows == false)
        //        {
        //            sResult = "NO_EXIST";
        //        }
        //        else
        //        {
        //            sResult = "EXIST";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //エラーメッセージ
        //        MessageBox.Show(
        //            "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
        //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        sResult = "ERROR";
        //    }
        //    finally
        //    {
        //        // リソースの開放。
        //        Global.gcDataReader.Close();
        //        Global.gcDataReader.Dispose();
        //    }
        //    return sResult;
        //}

        internal static string KanriTourokuCheck_Saimu(short nKUBN)
        {
            string sResult = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                if (nKUBN == 1)
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_KANRI WHERE KANRIID = 1 ";
                }
                else
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_KANRI WHERE KANRIID = 2 ";
                }
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == false)
                {
                    sResult = "NO_EXIST";
                }
                else
                {
                    sResult = "EXIST";
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                sResult = "ERROR";
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
            return sResult;
        }
//-- <2016/02/17>
        internal static void Get_SS_KANRI(int nId, string sKeyNm, out int nIdata1, out decimal nIdata2, out string sCdata)
        {
            nIdata1 = -1;
            nIdata2 = -1;
            sCdata = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                Global.cCmdSel.CommandText = "SELECT * FROM SS_KANRI WHERE KANRIID = " + nId + " AND KEYNM = '" + sKeyNm + "'";//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】「.ToString()」のみ削除
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == true)
                {
                    Global.gcDataReader.Read();

                    nIdata1 = DbCls.GetNumNullZero<int>(Global.gcDataReader["IDATA1"]);
                    nIdata2 = DbCls.GetNumNullZero<decimal>(Global.gcDataReader["IDATA2"]);
                    sCdata = Global.gcDataReader["CDATA"].ToString();
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/02/05 文言修正等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/02/05>
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
        }

        internal static string SecTourokuCheck()
        {
            string sResult = "";
            try
            {
                // コマンドオブジェクトにSQL文セット
                Global.cCmdSel.CommandText = "SELECT * FROM SS_SECVOL WHERE USRNO = " + Global.nUcod;//<--- V02.01.01 HWPO DELETE ◀「.ToString()」のみ削除
                // データリーダーでデータを取得する。
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                // データリーダーに１行以上の行が格納されているかチェック。
                if (Global.gcDataReader.HasRows == false)
                {
                    sResult = "NO_EXIST";
                }
                else
                {
                    Global.gcDataReader.Read();

                    sResult = "EXIST";
                    Global.nSAIKEN_F_USec = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIKEN"]);
                    Global.nSAIMU_F_USec = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_SAIMU"]);
                    Global.nKIJITU_F_USec = DbCls.GetNumNullZero<int>(Global.gcDataReader["F_KJTKAN"]);
                }
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                sResult = "ERROR";
            }
            finally
            {
                // リソースの開放。
                Global.gcDataReader.Close();
                Global.gcDataReader.Dispose();
            }
            return sResult;
                   
        }
    }
}
