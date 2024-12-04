using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

using System.Data.Common;
using IcsComUtil;
using IcsComDb;

//-- <2016/03/21>
using VB = Microsoft.VisualBasic;

namespace SMTORI
{
    /// <summary>
    /// 業務ロジック・クラス
    /// </summary>
    class blgSMTORI
    {
        /// <summary>
        /// メイン・フォームへの参照
        /// </summary>
        private frmSMTORI mcMainForm;

        private DbDataReader reader;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="frmMainForm">メイン・フォームへの参照</param>
        public blgSMTORI(frmSMTORI frmMainForm)
        {
            // メイン・フォームへの参照の保存。
            mcMainForm = frmMainForm;
        }
        public blgSMTORI()
        {
        }


        /// <summary>
        /// 印刷設定ダイアログボックス表示処理
        /// </summary>
        public void PrnSettingView()
        {
            using (dlgPrnSetting cDlgPrnSetting = new dlgPrnSetting(1))
            {
                try
                {
                    cDlgPrnSetting.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
//-- <2016/03/22>
//                        "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                }
                //**
                cDlgPrnSetting.Dispose();
            }
        }


        /// <summary>
        /// 財務取引先の検索
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iCnt"></param>
        public void Sel_TRNAM(string sTRCD, out int iCnt)
        {
            iCnt = 0;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★TRNAM.Select(財務取引先の取得)
                // ---> V02.30.01 WMH UPDATE ▼(No.116415)
                //Global.cCmdSelZ.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";
                Global.cCmdSelZ.CommandText = "SELECT * FROM TRNAM WHERE TRCD = :p ";
                // <--- V02.30.01 WMH UPDATE ▲(No.116415)
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@TRCD", sTRCD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.TRCD = sTRCD;
                    Global.RYAKU = reader["TRMX"].ToString();
                    Global.TORI_NAM = reader["TRNAM"].ToString();
                    Global.KNLD = reader["RNLD"].ToString();
                    iCnt++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 財務取引先の新規登録
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iCnt"></param>
        public void Ins_TRNAM(string sTRCD, string sRNLD, string sTRMX, string sTRNAM)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                Global.cCmdIns.CommandText = "INSERT INTO TRNAM (TRCD, RNLD, TRMX, TRNAM, FUSR, FMOD, FTIM ,LUSR, LMOD, LTIM) ";
                Global.cCmdIns.CommandText += "VALUES (:p, :p, :p, :p, :p, :p, :p ,:p, :p, :p) ";
                Global.cCmdIns.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RNLD", sRNLD);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMX", sTRMX);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRNAM", sTRNAM);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmss")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmss")));
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                Global.cCmdIns.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "　Ver" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 財務取引先の更新
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iCnt"></param>
        public void Upd_TRNAM(string sTRCD, string sRNLD, string sTRMX, string sTRNAM)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★TRNAM.Select(財務取引先の取得)
                Global.cCmdSel.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.TRCD_R = sTRCD;
                    Global.RYAKU_R = reader["TRMX"].ToString();
                    Global.TORI_NAM_R = reader["TRNAM"].ToString();
                    Global.KNLD_R = reader["RNLD"].ToString();
                    Global.FUSR_R = reader["FUSR"].ToString();
                    Global.FMOD_R = reader["FMOD"].ToString();
                    Global.FTIM_R = reader["FTIM"].ToString();
                    Global.LUSR_R = reader["LUSR"].ToString();
                    Global.LMOD_R = reader["LMOD"].ToString();
                    Global.LTIM_R = reader["LTIM"].ToString();
                }

                Global.cCmdIns.CommandText = "UPDATE TRNAM SET RNLD = :p, TRMX = :p, TRNAM = :p, LUSR = :p, LMOD = :p, LTIM = :p  ";
                Global.cCmdIns.CommandText += "WHERE TRCD = :p ";
                Global.cCmdIns.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RNLD", sRNLD);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMX", sTRMX);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRNAM", sTRNAM);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmss")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", sTRCD);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                Global.cCmdIns.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 財務会社情報の検索
        /// </summary>
        public void Get_VOLUM()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★VOLUM.Select(財務会社情報の取得)
                Global.cCmdSelZ.CommandText = "SELECT KMAN,KOUT FROM VOLUM WHERE KESN = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.nKMAN = DbCls.GetNumNullZero<int>(reader["KMAN"]);
                    Global.nVolKJUN = DbCls.GetNumNullZero<int>(reader["KOUT"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 取引先連動処理用の財務会社情報の取得
        /// </summary>
        public void Get_VOLUM_SSTori()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★VOLUM.Select(財務会社情報の取得)
                Global.cCmdSelZ.CommandText = "SELECT KESN, TRFLG FROM VOLUM WHERE KESN = :p or KESN = :p ORDER BY KESN";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN) + 1);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    // <マルチDB対応>Readが必須なので追加
                    while (reader.Read())
                    {
                        Global.nTRFLG[i] = DbCls.GetNumNullZero<int>(reader["TRFLG"]);
                        i++;
                    }
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★VOLUM2.Select(財務会社情報２の取得)
                Global.cCmdSelZ.CommandText = "SELECT KESN, CKEI, SYMD, EYMD FROM VOLUM2 WHERE (KESN = :p or KESN = :p) and (CKEI = :p or CKEI = :p) ORDER BY KESN, CKEI";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN) + 1);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@CKEI1", 10);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@CKEI2", 120);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    while (reader.Read())
                    {
                        if (DbCls.GetNumNullZero<int>(reader["KESN"]) == DbCls.GetNumNullZero<int>(Global.sKESN))
                        {
                            if (DbCls.GetNumNullZero<int>(reader["CKEI"]) == 10)
                            {
                                Global.nSYMD[0] = DbCls.GetNumNullZero<int>(reader["SYMD"]);
                            }
                            else if (DbCls.GetNumNullZero<int>(reader["CKEI"]) == 120)
                            {
                                Global.nEYMD[0] = DbCls.GetNumNullZero<int>(reader["EYMD"]);
                            }
                        }
                        else if (DbCls.GetNumNullZero<int>(reader["KESN"]) == DbCls.GetNumNullZero<int>(Global.sKESN) + 1)
                        {
                            if (DbCls.GetNumNullZero<int>(reader["CKEI"]) == 10)
                            {
                                Global.nSYMD[1] = DbCls.GetNumNullZero<int>(reader["SYMD"]);
                            }
                            else if (DbCls.GetNumNullZero<int>(reader["CKEI"]) == 120)
                            {
                                Global.nEYMD[1] = DbCls.GetNumNullZero<int>(reader["EYMD"]);
                            }
                        }
                    }
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★GAIKACTL.Select(外貨設定の取得)
                Global.cCmdSelZ.CommandText = "SELECT KESN, F_USE FROM GAIKACTL WHERE KESN = :p or KESN = :p ORDER BY KESN";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN) + 1);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    // <マルチDB対応>Readが必須なので追加
                    while (reader.Read())
                    {
                        Global.nGCFLG[i] = DbCls.GetNumNullZero<int>(reader["F_USE"]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 消費税情報の検索
        /// </summary>
        public void Get_SVOLUM()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★VOLUM.Select(財務会社情報の取得)
                Global.cCmdSelZ.CommandText = "SELECT HSSW FROM SVOLUM WHERE KESN = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.nHSSW = DbCls.GetNumNullZero<int>(reader["HSSW"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\n\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 債務取引先の検索
        /// </summary>
        /// <param name="iCnt"></param>
        public void Cnt_TRCD(out int iCnt)
        {
            iCnt = 0;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_TORI.Select(債務取引先の取得)
                if (Global.nTRCD_HJ == 1)
                {
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if (ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM SS_TORI WHERE LENGTH(TRCD) < 13  ORDER BY TRCD, HJCD ";
                    }
                    else
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM SS_TORI WHERE LEN(TRCD) < 13 ORDER BY TRCD, HJCD ";
                    }
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }
                else
                {
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM SS_TORI WHERE HJCD = 0 AND LENGTH(TRCD) < 13  ORDER BY TRCD ";
                    }
                    else
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM SS_TORI WHERE HJCD = 0 AND LEN(TRCD) < 13 ORDER BY TRCD ";
                    }
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        iCnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nCnt_TRCD　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        public void Cnt_TRCD_Pos(string TRCD, int HJCD, out int iCnt)
        {
            iCnt = 0;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_TORI.Select(債務取引先の取得)
                if (Global.nTRCD_HJ == 1)
                {
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD, HJCD FROM SS_TORI WHERE LENGTH(TRCD) < 13  ORDER BY TRCD, HJCD ";
                    }
                    else
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD, HJCD FROM SS_TORI WHERE LEN(TRCD) < 13 ORDER BY TRCD, HJCD ";
                    }
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }
                else
                {
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD, HJCD FROM SS_TORI WHERE HJCD = 0 AND LENGTH(TRCD) < 13  ORDER BY TRCD ";
                    }
                    else
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD, HJCD FROM SS_TORI WHERE HJCD = 0 AND LEN(TRCD) < 13 ORDER BY TRCD ";
                    }
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        iCnt++;
                        if (TRCD == reader["TRCD"].ToString() && HJCD == Convert.ToInt32(reader["HJCD"].ToString()))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nCnt_TRCD_Pos　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        //履歴出力要否の取得
        /// </summary>
        public void Get_RIREKI_SW()
        {
            Global.nRirekiSW = 0;
            int nTosei = 0;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SERRKI.Select(履歴設定テーブルの検索)
                Global.cCmdSelZ.CommandText = "SELECT IDNO, RSEQ, FLG FROM SETSTK WHERE IDNO = 1 AND RSEQ >= 1 ORDER BY RSEQ DESC ";
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    if (reader["FLG"].ToString() == "1")
                    {
                        nTosei = 1;
                    }
                }

                if (nTosei == 0)
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    var now = Get_DBTime();
                    //★SS_SERRKI.Select(履歴設定テーブルの検索)
                    Global.cCmdSel.CommandText = "SELECT RKSET, RKSDATE, RKDATE, RKTIM, RKUSR FROM SS_SETRKI WHERE RKID = 1 AND RKSDATE <= :p AND RKSET = 1 ";
                    Global.cCmdSel.Parameters.Clear();
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKSDATE", IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("yyyyMMdd"));
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKSDATE", now.ToString("yyyyMMdd"));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKSDATE",DbCls.GetNumNullZero<int>(now.ToString("yyyyMMdd")));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                    if (reader.HasRows == true)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        Global.nRirekiSW = 1;
                        Global.sRirekiDate = reader["RKSDATE"].ToString();
                        Global.sRirekiStartDate = reader["RKDATE"].ToString();
                        Global.sRirekiStartTime = reader["RKTIM"].ToString();
                        Global.sRirekiStartUser = reader["RKUSR"].ToString();
                    }
                }
                else
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    //★SS_SERRKI.Select(履歴設定テーブルの検索)
                    Global.cCmdSel.CommandText = "SELECT RKSET, RKSDATE, RKDATE, RKTIM, RKUSR FROM SS_SETRKI WHERE RKID = 1 AND RKSET = 1 ";
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                    if (reader.HasRows == true)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        Global.nRirekiSW = 1;
                        Global.sRirekiDate = reader["RKSDATE"].ToString();
                        Global.sRirekiStartDate = reader["RKDATE"].ToString();
                        Global.sRirekiStartTime = reader["RKTIM"].ToString();
                        Global.sRirekiStartUser = reader["RKUSR"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_RIREKI_SW　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 敬称マスタの値を敬称コンボボックスに設定
        /// </summary>
        public void Get_KeiNM(out string[] sArray)
        {
            sArray = null;
            try
            {
                int iCnt = 0;
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_KEISYO.Select(敬称の取得)
                Global.cCmdSel.CommandText = "SELECT * FROM SS_KEISYO ";
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        iCnt++;
                    }
                    sArray = new String[iCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_KEISYO.Select(敬称の取得)
                Global.cCmdSel.CommandText = " SELECT * FROM SS_KEISYO ";
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i] = reader["KEICD"].ToString() + ":" +
                                    reader["KEISNM"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KeiNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 源泉税テーブルの値を源泉区分コンボボックスに設定
        /// </summary>
        public void Get_GensenNM(int iCALKBN, int iGOU, out string[,] sArray)
        {
            sArray = null;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_GENTBL.Select(源泉区分データの取得)
                if (iCALKBN == 1 || iCALKBN == 2)
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_GENTBL WHERE CALKBN = :p AND GOU = :p ";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@CALKBN", iCALKBN);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GOU", iGOU);
                }
                else
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_GENTBL WHERE GOU = :p ORDER BY KBN";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GOU", iGOU);
                }
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int iCmbCnt = 0;
                    while (reader.Read())
                    {
                        iCmbCnt += 1;
                    }
                    sArray = new String[iCmbCnt, 2];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_GENTBL.Select(源泉区分データの取得)
                if (iCALKBN == 1 || iCALKBN == 2)
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_GENTBL WHERE CALKBN = :p AND GOU = :p ";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@CALKBN", iCALKBN);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GOU", iGOU);
                }
                else
                {
                    Global.cCmdSel.CommandText = "SELECT * FROM SS_GENTBL WHERE GOU = :p ORDER BY KBN";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GOU", iGOU);
                }
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i, 0] = reader["KBN"].ToString();
                        sArray[i, 1] = reader["KBNNM"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_GensenNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 支払区分テーブルの値を支払区分コンボボックスに設定
        /// </summary>
        public void Get_SKBNM(int iSKBKIND, out string[] sArray)
        {
            sArray = null;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSel.CommandText = "SELECT * FROM SS_SKUBN WHERE SKKBN = 11 AND SKBSW = 1 AND SKBKIND = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SKBKIND", iSKBKIND);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int iCmbCnt = 0;
                    while (reader.Read())
                    {
                        iCmbCnt += 1;
                    }
                    sArray = new String[iCmbCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSel.CommandText = "SELECT * FROM SS_SKUBN WHERE SKKBN = 11 AND SKBSW = 1 AND SKBKIND = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SKBKIND", iSKBKIND);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i] = reader["SKBNCOD"].ToString() + ":" +
                                    reader["SKBNM"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SKBNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 入金区分テーブルの値を約定コンボボックスに設定
        /// </summary>
        public void Get_NKBNM(out string[] sArray)
        {
            sArray = null;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
//-- <2016/02/15 98:でんさいも追加>
//                Global.cCmdSel.CommandText = "SELECT * FROM TBLKUBUN WHERE SIKIBETU = 2 AND SKBSW = 1 AND (SKBKIND BETWEEN 1 AND 9 OR SKBKIND BETWEEN 21 AND 29) ORDER BY KUBUNCD";
                Global.cCmdSel.CommandText = "SELECT * FROM TBLKUBUN WHERE SIKIBETU = '2' AND SKBSW = 1 AND (SKBKIND BETWEEN 1 AND 9 OR SKBKIND BETWEEN 21 AND 29 OR SKBKIND = 98) ORDER BY KUBUNCD";//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】「''」のみ追加
//-- <2016/02/15>
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int iCmbCnt = 0;
                    while (reader.Read())
                    {
                        iCmbCnt += 1;
                    }
                    sArray = new String[iCmbCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
//-- <2016/02/15 98:でんさいも追加>
//                Global.cCmdSel.CommandText = "SELECT * FROM TBLKUBUN WHERE SIKIBETU = 2 AND SKBSW = 1 AND (SKBKIND BETWEEN 1 AND 9 OR SKBKIND BETWEEN 21 AND 29) ORDER BY CAST(KUBUNCD AS int)";
                Global.cCmdSel.CommandText = "SELECT * FROM TBLKUBUN WHERE SIKIBETU = '2' AND SKBSW = 1 AND (SKBKIND BETWEEN 1 AND 9 OR SKBKIND BETWEEN 21 AND 29 OR SKBKIND = 98) ORDER BY CAST(KUBUNCD AS int)";//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】「''」のみ追加
//-- <2016/02/15>
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i] = reader["KUBUNCD"].ToString() + ":" +
                                    reader["KUBUNMEI"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_NKBNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 回収設定で使用しない入金区分の種別のリストを取得する
        /// </summary>
        /// <returns></returns>
        internal int[] GetNotUseNyukinKbn()
        {
            var list = new List<int>();
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                string query = "";
                query += "SELECT DISTINCT SKBKIND ";
                query += "  FROM TBLKUBUN ";
                query += " WHERE SIKIBETU = '2' ";//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】「''」のみ追加
                query += "   AND SKBSW = 1 ";
                query += "   AND SKBKIND NOT BETWEEN 1 AND 9 ";
                query += "   AND SKBKIND NOT BETWEEN 21 AND 29 ";
                query += "   AND SKBKIND <> 98 ";
                query += "ORDER BY SKBKIND";
                Global.cCmdSel.CommandText = query;
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        list.Add(DbCls.GetNumNullZero<int>(reader["SKBKIND"]));
                    }
                }

                return list.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGetNotUseNyukinKbn　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return list.ToArray();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 幣種テーブルの値を取引通貨コンボボックスに設定
        /// </summary>
        public void Get_HEI_CD(out string[] sArray)
        {
            sArray = null;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSelZ.CommandText = "SELECT DISTINCT HEI_CD FROM USEHEI ORDER BY HEI_CD";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    int iCmbCnt = 0;
                    while (reader.Read())
                    {
                        iCmbCnt += 1;
                    }
                    sArray = new String[iCmbCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSelZ.CommandText = "SELECT DISTINCT HEI_CD FROM USEHEI ORDER BY HEI_CD";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i] = reader["HEI_CD"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_HEI_CD　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 手数料IDテーブルの値を手数料IDコンボボックスに設定
        /// </summary>
        public void Get_TESUUID(string sOwnBkCod, out string[] sArray)
        {
            sArray = null;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSel.CommandText = @"SELECT DISTINCT
	                                               T.TESUID
	                                              ,T.TESUNAM
                                               FROM
	                                               SS_TESUID T
	                                               INNER JOIN SS_FRGEN F ON F.OWNBKCOD = :p AND F.TESUID = T.TESUID
                                               ORDER BY T.TESUID";
                                                
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@OWNBKCOD", sOwnBkCod);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int iCmbCnt = 0;
                    while (reader.Read())
                    {
                        iCmbCnt += 1;
                    }
                    sArray = new String[iCmbCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分データの取得)
                Global.cCmdSel.CommandText = @"SELECT DISTINCT
	                                               T.TESUID
	                                              ,T.TESUNAM
                                               FROM
	                                               SS_TESUID T
	                                               INNER JOIN SS_FRGEN F ON F.OWNBKCOD = :p AND F.TESUID = T.TESUID
                                               ORDER BY T.TESUID";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@OWNBKCOD", sOwnBkCod);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sArray[i] = reader["TESUID"].ToString() + ":" +
                                    reader["TESUNAM"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TESUUID　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 期(Global.sKESN)の取得
        /// </summary>
        public void Get_KI()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★A_KI.Select(期の取得)
                //Global.cCmdSel.CommandText = "SELECT KESN  FROM ( SELECT " +
                //" V.KESN AS KESN, (( SELECT MAX( V2.KESN )  FROM VOLUM V2 ) - V.KESN) AS KE " +
                //" FROM VOLUM V ) AS A_KI WHERE A_KI.KE = 1";
                Global.cCmdSelZ.CommandText = "SELECT KESN FROM " + DbCls.GetViewSchemaString(Global.sCcod) + ".A_KI WHERE KI = 1 ";
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.sKESN = reader["KESN"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 入力項目の入力タイプ・入力桁の取得
        /// </summary>
        //public void Get_SS_VOLUM()
        public void Get_Env()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_VOLUM.Select(会社設定項目の情報取得)
                //Global.cCmdSel.CommandText = " SELECT TRCD_HJ, TRCD_TYP, TRCD_LEN, TRCD_ZE, KCOD_TYP, KCOD_LEN, KCOD_ZE, BCD_ZMAK, "
                //                           + " F_BCOD, BCOD_TYP, BCOD_LEN, BCOD_ZE, F_EDCOD, ECOD_TYP, ECOD_LEN, ECOD_ZE, GENGO, KANRI_F, F_ICHI "
                //                           + " FROM SS_VOLUM WHERE SEQ = 1 ";
                Global.cCmdSelZ.CommandText = @"Select COALESCE(VOLUM.TRTY, 0) As TRCD_TYP, COALESCE(VOLUM.TRLN, 0) As TRCD_LEN,
                                                       COALESCE(VOLUM.KMTY, 0) As KCOD_TYP, COALESCE(VOLUM.KMLN, 0) As KCOD_LEN,
                                                       COALESCE(VOLUM.BMTY, 0) As BCOD_TYP, COALESCE(VOLUM.BMLN, 0) As BCOD_LEN,
                                                       COALESCE(VOLUM.EDTY, 0) As ECOD_TYP, COALESCE(VOLUM.EDLN, 0) As ECOD_LEN,
                                                       COALESCE(VOLUM.KJTY, 0) As KJCD_TYP, COALESCE(VOLUM.KJLN, 0) As KJCD_LEN,
                                                       COALESCE(VOLUM.BMFLG, 0) As F_BCOD,
                                                       COALESCE(VOLUM.KJFLG, 0) As F_KJCD,
                                                       COALESCE(VOLUM.EDFLG, 0) As F_EDCOD,
                                                       COALESCE(VOLUM.GNNO, 0) As GENGO
                                                FROM VOLUM
                                                WHERE KESN = " + Global.cKaisya.nKESN.ToString();

                //DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.nTRCD_Type = DbCls.GetNumNullZero<int>(reader["TRCD_TYP"]) == 1 ? 0 : 1;
                    Global.nTRCD_Len = DbCls.GetNumNullZero<int>(reader["TRCD_LEN"]);
                    //Global.nTRCD_ZE = DbCls.GetNumNullZero<int>(reader["TRCD_ZE"]);
                    Global.nTRCD_ZE = 0;

                    Global.nKCOD_Type = DbCls.GetNumNullZero<int>(reader["KCOD_TYP"]) == 1 ? 0 : 1;
                    Global.nKCOD_Len = DbCls.GetNumNullZero<int>(reader["KCOD_LEN"]);
                    //Global.nKCOD_ZE = DbCls.GetNumNullZero<int>(reader["KCOD_ZE"]);
                    Global.nKCOD_ZE = 0;

                    Global.nBCOD_F = DbCls.GetNumNullZero<int>(reader["F_BCOD"]);
                    Global.nBCOD_Type = DbCls.GetNumNullZero<int>(reader["BCOD_TYP"]) == 1 ? 0 : 1;
                    Global.nBCOD_Len = DbCls.GetNumNullZero<int>(reader["BCOD_LEN"]);
                    //Global.nBCOD_ZE = DbCls.GetNumNullZero<int>(reader["BCOD_ZE"]);
                    Global.nBCOD_ZE = 0;

                    Global.nEDCOD_F = DbCls.GetNumNullZero<int>(reader["F_EDCOD"]);
                    Global.nEDCOD_Type = DbCls.GetNumNullZero<int>(reader["ECOD_TYP"]) == 1 ? 0 : 1;
                    Global.nEDCOD_Len = DbCls.GetNumNullZero<int>(reader["ECOD_LEN"]);
                    //Global.nEDCOD_ZE = DbCls.GetNumNullZero<int>(reader["ECOD_ZE"]);
                    Global.nEDCOD_ZE = 0;

                    Global.nGengo = DbCls.GetNumNullZero<int>(reader["GENGO"]);

                    int nIData1;
                    decimal nIData2;
                    string sCData;
                    Get_SS_KANRI(1, "BCD_ZMAK", out nIData1, out nIData2, out sCData);
                    Global.nBCD_ZMAK = nIData1;
                    //Global.nBCD_ZMAK = DbCls.GetNumNullZero<int>(reader["BCD_ZMAK"]);  //管理テーブル

                    Get_SS_KANRI(1, "F_ICHI", out nIData1, out nIData2, out sCData);
                    Global.nF_ICHI = nIData1;
                    //Global.nF_ICHI = DbCls.GetNumNullZero<int>(reader["F_ICHI"]);      //管理テーブル

                    //Global.nTRCD_HJ = DbCls.GetNumNullZero<int>(reader["TRCD_HJ"]);    //SS_VOLUM
                    //Global.nKANRI_F = DbCls.GetNumNullZero<int>(reader["KANRI_F"]);    //SS_VOLUM(期日管理？）

                    if (Global.nSAIKEN_F == 1)
                    {
                        Global.nETAN_Len = Convert.ToInt32(GetKanriData("債権担当者コード桁数"));
                        Global.nETAN_Type = Convert.ToInt32(GetKanriData("債権担当者コードタイプ"));
                        Global.nF_SENYOU = Convert.ToInt32(GetKanriData("専用入金口座を利用する"));
                    }
                    else
                    {
                        Global.nETAN_Len = 0;
                        Global.nETAN_Type = 0;
                        Global.nF_SENYOU = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Env　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        public string GetKanriData(string sKeyString)
        {
            string sResult = "";
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendLine("SELECT");
            sbSql.AppendLine("    KANRIDATA");
            sbSql.AppendLine("FROM");
            sbSql.AppendLine("    TBLKANRI");
            sbSql.AppendLine("WHERE");
            sbSql.AppendFormat("    KANRICD = '{0}'\r\n", sKeyString);

            DbCommand cmd = (Global.cConSaikenSaimu).CreateCommand();
            cmd.CommandText = sbSql.ToString();
            /*if (DbCls.DbType == DbCls.eDbType.SQLServer)*/ { DbCls.ReplacePlaceHolder(cmd); }//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】「if条件のみ削除」

            using (DbDataReader dReader = cmd.ExecuteReader())
            {
                try
                {
                    if (dReader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        dReader.Read();

                        sResult = dReader[0].ToString();
                    }
                }
                finally
                {
                    dReader.Close();
                }
            }
            return sResult;
        }

        public bool Get_SS_KANRI(int nId, string sKey, out int onIdata1, out decimal onIdata2, out string osCdata)
        {
            bool bRet = false;
            onIdata1 = 0;
            onIdata2 = 0;
            osCdata = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SELECT IDATA1, IDATA2, CDATA FROM SS_KANRI");
                sb.AppendFormat("WHERE KANRIID = {0} AND KEYNM = '{1}'", nId, sKey);

                Global.cCmdSel.CommandText = sb.ToString();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    onIdata1 = DbCls.GetNumNullZero<int>(reader["IDATA1"]);
                    onIdata2 = DbCls.GetNumNullZero<decimal>(reader["IDATA2"]);
                    osCdata = reader["CDATA"].ToString();
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SS_KANRI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return bRet;
        }

        /// <summary>
        /// 自社銀行マスタの値をコンボボックスに設定
        /// </summary>
        /// <param name="sBKNAMArray"></param>
        /// <param name="sBRNAMArray"></param>
        /// <param name="sYKNKINDArray"></param>
        /// <param name="sKOZANOArray"></param>
        /// <param name="sIRAININArray"></param>
        /// <param name="sFACNAMArray"></param>
        public void Get_OWNBK(out string[] sOWNIDArray, out string[] sFACIDArray,
                              out string[,] sBKNAMArray, out string[,] sBRNAMArray,
                              out string[,] sYKNKINDArray, out string[] sKOZANOArray,
                              out string[] sIRAININArray, out string[] sFACNAMArray)
        {
            sOWNIDArray = null;
            sFACIDArray = null;
            sBKNAMArray = null;
            sBRNAMArray = null;
            sYKNKINDArray = null;
            sKOZANOArray = null;
            sIRAININArray = null;
            sFACNAMArray = null;
            try
            {
                int iCnt = 0;
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_OWNBK.Select(自社銀行情報の取得)
//-- <2016/02/09 外貨を除く、口座IDソート>
//                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, f.FACID, f.FACNAM, "
//                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
//                                           + "FROM SS_OWNBK a "
//                                           + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
//                                           + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
//                                           + "LEFT JOIN SS_FACTER f ON f.OWNID = a.OWNID "
//                                           + "ORDER BY a.OWNBKCOD, a.OWNBRCOD, a.YOKNKIND, a.KOZANO ";
                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, f.FACID, f.FACNAM, "
                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
                                           + "FROM SS_OWNBK a "
                                           //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BANK b ON a.OWNBKCOD = b.BKCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                           + "LEFT JOIN SS_FACTER f ON f.OWNID = a.OWNID "
                                           + "WHERE GAIKA <> 1 ORDER BY a.OWNID ";
//-- <2016/02/09>
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        iCnt++;
                    }
                    sOWNIDArray = new string[iCnt];
                    sFACIDArray = new string[iCnt];
                    sBKNAMArray = new String[iCnt, 2];
                    sBRNAMArray = new String[iCnt, 2];
                    sYKNKINDArray = new String[iCnt, 2];
                    sKOZANOArray = new String[iCnt];
                    //sIRAININArray = new String[iCnt];
                    sFACNAMArray = new String[iCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_OWNBK.Select(自社銀行情報の取得)
//-- <2016/02/09 外貨を除く、口座IDソート>
//                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, f.FACID, f.FACNAM, "
//                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
//                                           + "FROM SS_OWNBK a "
//                                           + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
//                                           + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
//                                           + "LEFT JOIN SS_FACTER f ON f.OWNID = a.OWNID "
//                                           + "ORDER BY a.OWNBKCOD, a.OWNBRCOD, a.YOKNKIND, a.KOZANO ";
                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, f.FACID, f.FACNAM, "
                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
                                           + "FROM SS_OWNBK a "
                                           //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           +"LEFT JOIN " + Global.sZJoin + "BANK b ON a.OWNBKCOD = b.BKCOD "
                                           +"LEFT JOIN " + Global.sZJoin + "BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                           + "LEFT JOIN SS_FACTER f ON f.OWNID = a.OWNID "
                                           + "WHERE GAIKA <> 1 ORDER BY a.OWNID ";
//-- <2016/02/09>
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sOWNIDArray[i] = reader["OWNID"].ToString();
                        sFACIDArray[i] = reader["FACID"].ToString();
                        sBKNAMArray[i, 0] = reader["OWNBKCOD"].ToString();
                        sBKNAMArray[i, 1] = reader["BKNAM"].ToString();
                        sBRNAMArray[i, 0] = reader["OWNBRCOD"].ToString();
                        sBRNAMArray[i, 1] = reader["BRNAM"].ToString();
                        sYKNKINDArray[i, 0] = reader["YOKNKIND"].ToString();
                        sYKNKINDArray[i, 1] = reader["YKNKIND"].ToString();
                        sKOZANOArray[i] = reader["KOZANO"].ToString();
                        //sIRAININArray[i] = reader["IRAININ"].ToString();
                        sFACNAMArray[i] = reader["FACNAM"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_OWNBK　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #region　コメントアウト
        ///// <summary>
        ///// 自社銀行（外貨出金口座）マスタの値をコンボボックスに設定
        ///// </summary>
        ///// <param name="sBKNAMArray"></param>
        ///// <param name="sBRNAMArray"></param>
        ///// <param name="sYKNKINDArray"></param>
        ///// <param name="sKOZANOArray"></param>
        ///// <param name="sIRAININArray"></param>
        ///// <param name="sFACNAMArray"></param>
        //public void Get_OWNBK_Gaika(out string[] sOWNIDArray, out string[,] sBKNAMArray, out string[,] sBRNAMArray,
        //                            out string[,] sYKNKINDArray, out string[] sKOZANOArray)
        //{
        //    sOWNIDArray = null;
        //    sBKNAMArray = null;
        //    sBRNAMArray = null;
        //    sYKNKINDArray = null;
        //    sKOZANOArray = null;
        //    try
        //    {
        //        int iCnt = 0;
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //        //★SS_OWNBK.Select(自社銀行情報の取得)
        //        Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, "
        //                                   + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
        //                                   + "FROM SS_OWNBK a "
        //                                   + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
        //                                   + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
        //                                   + "WHERE GAIKA = 1"
        //                                   + "ORDER BY a.OWNBKCOD, a.OWNBRCOD, a.YOKNKIND, a.KOZANO ";
        //        DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

        //        if (reader.HasRows == true)
        //        {
        //            while (reader.Read())
        //            {
        //                iCnt++;
        //            }
        //            sOWNIDArray = new string[iCnt];
        //            sBKNAMArray = new String[iCnt, 2];
        //            sBRNAMArray = new String[iCnt, 2];
        //            sYKNKINDArray = new String[iCnt, 2];
        //            sKOZANOArray = new String[iCnt];
        //        }

        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //        //★SS_OWNBK.Select(自社銀行情報の取得)
        //        Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, "
        //                                   + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND "
        //                                   + "FROM SS_OWNBK a "
        //                                   + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
        //                                   + "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
        //                                   + "WHERE GAIKA = 1"
        //                                   + "ORDER BY a.OWNBKCOD, a.OWNBRCOD, a.YOKNKIND, a.KOZANO ";
        //        DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

        //        if (reader.HasRows == true)
        //        {
        //            int i = 0;
        //            while (reader.Read())
        //            {
        //                sOWNIDArray[i] = reader["OWNID"].ToString();
        //                sBKNAMArray[i, 0] = reader["OWNBKCOD"].ToString();
        //                sBKNAMArray[i, 1] = reader["BKNAM"].ToString();
        //                sBRNAMArray[i, 0] = reader["OWNBRCOD"].ToString();
        //                sBRNAMArray[i, 1] = reader["BRNAM"].ToString();
        //                sYKNKINDArray[i, 0] = reader["YOKNKIND"].ToString();
        //                sYKNKINDArray[i, 1] = reader["YKNKIND"].ToString();
        //                sKOZANOArray[i] = reader["KOZANO"].ToString();
        //                i += 1;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(
        //            "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
        //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //    }
        //}
        #endregion
        /// <summary>
        /// 自社銀行（外貨出金口座）マスタの値をコンボボックスに設定
        /// </summary>
        /// <param name="sBKNAMArray"></param>
        /// <param name="sBRNAMArray"></param>
        /// <param name="sYKNKINDArray"></param>
        /// <param name="sKOZANOArray"></param>
        /// <param name="sIRAININArray"></param>
        /// <param name="sFACNAMArray"></param>
        public void Get_OWNBK_Gaika(out string[] sOWNIDArray, out string[,] sBKNAMArray, out string[,] sBRNAMArray,
                                    out string[,] sYKNKINDArray, out string[] sKOZANOArray, out string[] sHEICDArray)
        {
            sOWNIDArray = null;
            sBKNAMArray = null;
            sBRNAMArray = null;
            sYKNKINDArray = null;
            sKOZANOArray = null;
            sHEICDArray = null;
            try
            {
                int iCnt = 0;
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_OWNBK.Select(自社銀行情報の取得)
                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, "
                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND, HEI_CD "
                                           + "FROM SS_OWNBK a "
                                           //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BANK b ON a.OWNBKCOD = b.BKCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                           // 2024/09/10 Postgres16対応 垣内▼
                                           //+ "WHERE GAIKA = 1"
                                           + "WHERE GAIKA = 1 "
                                           // 2024/09/10 Postgres16対応 垣内▲
                                           + "ORDER BY a.OWNID ";
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        iCnt++;
                    }
                    sOWNIDArray = new string[iCnt];
                    sBKNAMArray = new String[iCnt, 2];
                    sBRNAMArray = new String[iCnt, 2];
                    sYKNKINDArray = new String[iCnt, 2];
                    sKOZANOArray = new String[iCnt];
                    sHEICDArray = new String[iCnt];
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_OWNBK.Select(自社銀行情報の取得)
                Global.cCmdSel.CommandText = "SELECT a.OWNID, a.OWNBKCOD, b.BKNAM, a.OWNBRCOD, c.BRNAM, a.YOKNKIND, a.KOZANO, "
                                           + "case a.YOKNKIND when '1' then '普通預金' when '2' then '当座預金' when '4' then '貯蓄' when '9' then 'その他' end as YKNKIND, HEI_CD "
                                           + "FROM SS_OWNBK a "
                                           //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BANK b ON a.OWNBKCOD = b.BKCOD "
                                           //+ "LEFT JOIN ICSP_312Z" + Global.sCcod + "..BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BANK b ON a.OWNBKCOD = b.BKCOD "
                                           + "LEFT JOIN " + Global.sZJoin + "BRANCH c ON a.OWNBKCOD = c.BKCOD  AND a.OWNBRCOD = c.BRCOD "
                                           //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                           // 2024/09/10 Postgres16対応 垣内▼
                                           //+ "WHERE GAIKA = 1"
                                           + "WHERE GAIKA = 1 "
                                           // 2024/09/10 Postgres16対応 垣内▲
                                           + "ORDER BY a.OWNID ";
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        sOWNIDArray[i] = reader["OWNID"].ToString();
                        sBKNAMArray[i, 0] = reader["OWNBKCOD"].ToString();
                        sBKNAMArray[i, 1] = reader["BKNAM"].ToString();
                        sBRNAMArray[i, 0] = reader["OWNBRCOD"].ToString();
                        sBRNAMArray[i, 1] = reader["BRNAM"].ToString();
                        sYKNKINDArray[i, 0] = reader["YOKNKIND"].ToString();
                        sYKNKINDArray[i, 1] = reader["YKNKIND"].ToString();
                        sKOZANOArray[i] = reader["KOZANO"].ToString();
                        sHEICDArray[i] = reader["HEI_CD"].ToString();
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_OWNBK_Gaika　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/09>
        #region 50音検索関連
        //敬称の取得
        public string Get_KeiNM(string sKEICD)
        {
            string sKEINM = "";
            try
            {
                if (sKEICD == "")
                {
                    return sKEINM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_KEISYOU.Select(担当者名称の取得)
                Global.cCmdSel.CommandText = "SELECT KEISNM FROM SS_KEISYO WHERE KEICD = :p ";
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEICD", sKEICD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEICD", DbCls.GetNumNullZero<int>(sKEICD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKEINM = reader["KEISNM"].ToString();
                }
                return sKEINM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KeiNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sKEINM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 担当名称の取得
        /// </summary>
        /// <param name="sTCOD"></param>
        public string Get_TNAM(string sTCOD)
        {
            string sTNAM = "";
            try
            {
                if (sTCOD == "")
                {
                    return sTNAM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★TANTOU.Select(担当者名称の取得)
                Global.cCmdSelZ.CommandText = "SELECT TNAM FROM TANTOU WHERE TCOD = :p AND TFLG = 0 ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@TCOD", sTCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sTNAM = reader["TNAM"].ToString();
                }
                return sTNAM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TNAM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
                return sTNAM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 担当名称の取得
        /// </summary>
        /// <param name="sTCOD"></param>
        public string Get_TBMN(string sTCOD)
        {
            string sBSCOD = "";
            try
            {
                if (sTCOD == "")
                {
                    return sBSCOD;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★TANTOU.Select(担当者に紐付く部署コードの取得)
                Global.cCmdSelZ.CommandText = "SELECT BSCOD FROM TANTOU WHERE TCOD = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@TCOD", sTCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sBSCOD = reader["BSCOD"].ToString();
                }
                return sBSCOD;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TBMN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sBSCOD;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        #region コメントアウト
        ///// <summary>
        ///// 部門名称の取得
        ///// </summary>
        ///// <param name="sBCOD"></param>
        //public string Get_BNAM(string sBCOD)
        //{
        //    string sBNAM = "";
        //    try
        //    {
        //        if (sBCOD == "")
        //        {
        //            return sBNAM;
        //        }
        //        if (Global.nBCOD_Type == 1)
        //        {
        //            sBCOD = sBCOD.PadRight(Global.nBCOD_Len, ' ');
        //        }

        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //        //★BNAME.Select(部門名称の取得)
        //        Global.cCmdSelZ.CommandText = "SELECT BNAM FROM BNAME WHERE KESN = :p AND BCOD = :p AND BFLG = 0 ";
        //        Global.cCmdSelZ.Parameters.Clear();
        //        DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
        //        DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BCOD", sBCOD);
        //        DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

        //        if (reader.HasRows == true)
        //        {
        //            // <マルチDB対応>Readが必須なので追加
        //            reader.Read();

        //            sBNAM = reader["BNAM"].ToString();
        //        }
        //        return sBNAM;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(
        //            "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
        //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return sBNAM;
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //    }
        //}
        #endregion

        /// <summary>
        /// 部門名称の取得
        /// </summary>
        /// <param name="sBCOD"></param>
        public string Get_BNAM(string sBCOD)
        {
            string sBNAM = "";
            try
            {
                if (sBCOD == "")
                {
                    return sBNAM;
                }
                if (Global.nBCOD_Type == 1)
                {
                    sBCOD = sBCOD.PadRight(Global.nBCOD_Len, ' ');
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★BNAME.Select(部門名称の取得)
                Global.cCmdSelZ.CommandText = "SELECT BNAME.BNAM FROM BNAME,(SELECT MAX(KESN) AS KSN, BCOD FROM BNAME WHERE KESN IN (:p, :p) ";
                Global.cCmdSelZ.CommandText += " AND BFLG = 0 GROUP BY BCOD) B WHERE B.BCOD = BNAME.BCOD AND B.KSN = BNAME.KESN AND BNAME.BCOD = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN1", DbCls.GetNumNullZero<int>(Global.sKESN));
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN2", DbCls.GetNumNullZero<int>(Global.sKESN) + 1);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BCOD", sBCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sBNAM = reader["BNAM"].ToString();
                }
                return sBNAM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_BNAM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sBNAM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 科目名称の取得
        /// </summary>
        /// <param name="sKICD"></param>
        public string Get_KNAM(string sKCOD)
        {
            string sKNAM = "";
            try
            {
                if (sKCOD == "")
                {
                    return sKNAM;
                }
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★KNAME.Select(科目名称の取得)
                //--->V01.12.01 ATT UPDATE ▼ (7063)
                //Global.cCmdSelZ.CommandText = "SELECT KNAM FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 ";
                Global.cCmdSelZ.CommandText = "SELECT KNAM FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 " + ((Convert.ToInt32(Global.GAI_F) == 0) ? "AND GAFLG = 0" : "");
                //<---V01.12.01 ATT UPDATE ▲ (7063)
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KCOD", sKCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKNAM = reader["KNAM"].ToString();
                }
                return sKNAM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KNAM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sKNAM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 科目略称の取得
        /// </summary>
        /// <param name="sKICD"></param>
        public string Get_KNMX(string sKCOD)
        {
            string sKNMX = "";
            try
            {
                if (sKCOD == "")
                {
                    return sKNMX;
                }
                if (Global.nKCOD_Type == 1)
                {
                    sKCOD = sKCOD.PadRight(Global.nKCOD_Len, ' ');
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★KNAME.Select(科目名称の取得)
                //--->V01.12.01 ATT UPDATE ▼ (7063)
                //Global.cCmdSelZ.CommandText = "SELECT KNMX FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 ";
                Global.cCmdSelZ.CommandText = "SELECT KNMX FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 " + ((Convert.ToInt32(Global.GAI_F) == 0) ? "AND GAFLG = 0" : "");
                //<---V01.12.01 ATT UPDATE ▲ (7063)
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KCOD", sKCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKNMX = reader["KNMX"].ToString();
                }
                return sKNMX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KNMX　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sKNMX;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/22>
        /// <summary>
        /// 科目略称の取得(外貨の場合)
        /// </summary>
        /// <param name="sKICD"></param>
        public string Get_KNMXKAIGAI(string sKCOD)
        {
            string sKNMX = "";
            try
            {
                if (sKCOD == "")
                {
                    return sKNMX;
                }
                if (Global.nKCOD_Type == 1)
                {
                    sKCOD = sKCOD.PadRight(Global.nKCOD_Len, ' ');
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★KNAME.Select(科目名称の取得)
                Global.cCmdSelZ.CommandText = "SELECT KNMX FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 AND GAFLG = 1 ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KCOD", sKCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKNMX = reader["KNMX"].ToString();
                }
                return sKNMX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_KNMXKAIGAI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sKNMX;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/22>

        /// <summary>
        /// 科目コードの変換(KCOD ⇒ KICD)
        /// </summary>
        /// <param name="sKCOD"></param>
        public string Conv_KCODtoKICD(string sKCOD)
        {
            string sKICD = "";
            if (sKCOD == "")
            {
                return sKICD;
            }
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★KNAME.Select(科目CDの取得)
                Global.cCmdSelZ.CommandText = "SELECT KICD FROM KNAME WHERE KESN = :p AND KCOD = :p AND BKBN = 5 ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KCOD", sKCOD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKICD = reader["KICD"].ToString();
                }
                return sKICD;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nConv_KCODtoKICD　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sKICD;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 科目コードの変換(KCOD ⇒ KICD)
        /// </summary>
        /// <param name="sKICD"></param>
        public string Conv_KICDtoKCOD(string sKICD)
        {
            string sKCOD = "";
            try
            {
                if (sKICD == "")
                {
                    return sKCOD;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★KNAME.Select(科目内部CDの取得)
                Global.cCmdSelZ.CommandText = "SELECT KCOD FROM KNAME WHERE KESN = :p AND KICD = :p AND BKBN = 5 ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KCOD", sKICD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sKCOD = reader["KCOD"].ToString();
                }
                return sKCOD;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nConv_KICDtoKCOD　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sKCOD;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 指定された部門CD/科目CDの組み合わせがBKZANに存在するかチェック
        /// </summary>
        /// <param name="sBCOD"></param>
        /// <param name="sKCOD"></param>
        /// <returns></returns>
        public bool Chk_BKZAN(string sBCOD, string sKCOD)
        {
            try
            {
                string sKICD = Conv_KCODtoKICD(sKCOD);
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★BKZAN.Select(部門科目残高の存在チェック)
                Global.cCmdSelZ.CommandText = "SELECT BCOD FROM BKZAN WHERE KESN = :p AND BCOD = :p AND KICD = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", Global.sKESN);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KESN", DbCls.GetNumNullZero<int>(Global.sKESN));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BCOD", sBCOD);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@KICD", sKICD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_BKZAN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_GinFuriSKBN(string sTRCD, string sHJCD, string sSHINO, string sSHOID)
        {
            bool bRet = false;

            try
            {
                //**>>ICS-S 2013/05/21
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //**<<ICS-E
                //★SS_TSHOH.Select()
                //Global.cCmdSel.CommandText = "SELECT IsNull(SI_KUBN1, 0) AS SI_KUBN1, "
                //                           + "IsNull(SI_KUBN2, 0) AS SI_KUBN2, "
                //                           + "IsNull(SI_KUBN3, 0) AS SI_KUBN3, "
                //                           + "IsNUll(SI_KUBN4, 0) AS SI_KUBN4 "
                //                           + "FROM SS_SHOHO "
                //                           + "WHERE SHINO = :p";
                Global.cCmdSel.CommandText = "SELECT COALESCE(SI_KUBN1, 0) AS SI_KUBN1, "
                                           + "COALESCE(SI_KUBN2, 0) AS SI_KUBN2, "
                                           + "COALESCE(SI_KUBN3, 0) AS SI_KUBN3, "
                                           + "COALESCE(SI_KUBN4, 0) AS SI_KUBN4 "
                                           + "FROM SS_SHOHO "
                                           + "WHERE SHINO = :p";
                Global.cCmdSel.Parameters.Clear();
                int itry;
                if (int.TryParse(sSHINO, out itry) == false) itry = -1;
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHINO", int.Parse(itry == -1 ? itry.ToString() : sSHINO));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    reader.Read();
                    string sSKubn1 = reader["SI_KUBN1"].ToString();
                    string sSKubn2 = reader["SI_KUBN2"].ToString();
                    string sSKubn3 = reader["SI_KUBN3"].ToString();
                    string sSKubn4 = reader["SI_KUBN4"].ToString();
                    string sSKind = "";

                    sSKind = Get_SKBKIND(sSKubn1);
                    if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                    {
                        bRet = true;
                        return bRet;
                    }
                    sSKind = Get_SKBKIND(sSKubn2);
                    if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                    {
                        bRet = true;
                        return bRet;
                    }
                    sSKind = Get_SKBKIND(sSKubn3);
                    if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                    {
                        bRet = true;
                        return bRet;
                    }
                    sSKind = Get_SKBKIND(sSKubn4);
                    if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                    {
                        bRet = true;
                        return bRet;
                    }
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                string[,] sTSHOHArray;
                Get_SS_TSHOH_All(sTRCD, sHJCD, out sTSHOHArray);

                if (sTSHOHArray != null && sTSHOHArray.Length > 0)
                {
                    for (int i = 0; i < sTSHOHArray.Length / 30; i++)
                    {
                        if (sTSHOHArray[i, 0] == sSHOID)
                        {
                            continue;
                        }

                        string sSKubn = "";
                        string sSKind = "";
                        sSKubn = DbCls.GetStrNullKara(sTSHOHArray[i, 6]);
                        sSKind = Get_SKBKIND(sSKubn);
                        if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                        {
                            bRet = true;
                            break;
                        }
                        sSKubn = DbCls.GetStrNullKara(sTSHOHArray[i, 12]);
                        sSKind = Get_SKBKIND(sSKubn);
                        if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                        {
                            bRet = true;
                            break;
                        }
                        sSKubn = DbCls.GetStrNullKara(sTSHOHArray[i, 18]);
                        sSKind = Get_SKBKIND(sSKubn);
                        if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                        {
                            bRet = true;
                            break;
                        }
                        sSKubn = DbCls.GetStrNullKara(sTSHOHArray[i, 24]);
                        sSKind = Get_SKBKIND(sSKubn);
                        if (sSKind == "6" || sSKind == "7" || sSKind == "8" || sSKind == "12")
                        {
                            bRet = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_GinFuriSKBN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return bRet;
        }
//-- <2016/03/22>
        /// <summary>
        /// 自社銀行に初期値があるかチェック
        /// </summary>
        /// <param name="sBCOD"></param>
        /// <param name="sKCOD"></param>
        /// <returns></returns>
        public bool Chk_FRIGINFDEF(string sTRCD, string sHJCD, string sGINID)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_FRIGIN(振込先銀行の初期値存在チェック)
                Global.cCmdSel.CommandText = "SELECT GIN_ID FROM SS_FRIGIN WHERE RTRIM(TRCD) = :p AND HJCD = :p AND FDEF = 1 AND GIN_ID <> :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】(RTRIM)のみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GIN_ID", int.Parse(sGINID));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_FRIGINFDEF　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/22>


        /// <summary>
        /// 対象取引先を使用した債権データが存在するか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：存在する、false：存在しない</returns>
        public bool Exists_Saiken_Data(string sTRCD, string sHJCD)
        {
            bool result = false;
            DbCommand cmd;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            string sql = "SELECT 1 FROM TBLSEIKYU WHERE RTRIM(TOKUCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】(RTRIM)のみ追加
            sql += "      UNION ALL ";
            sql += "      SELECT 1 FROM TBLNYUKIN WHERE RTRIM(TOKUCD) = :p AND HJCD = :p";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】(RTRIM)のみ追加
            Global.cCmdSel.CommandText = sql;
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TOKUCD1", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD1", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD1", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TOKUCD2", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD2", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD2", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
            /*if (DbCls.DbType == DbCls.eDbType.SQLServer)*/ { DbCls.ReplacePlaceHolder(Global.cCmdSel); }//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】「if条件のみ削除」

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        /// <summary>
        /// 対象取引先を使用した債務データが存在するか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：存在する、false：存在しない</returns>
        public bool Exists_Saimu_Data(string sTRCD, string sHJCD)
        {
            bool result = false;
            DbCommand cmd;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            Global.cCmdSel.CommandText = "SELECT * FROM SS_SHDATA WHERE RTRIM(TRCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            //if (DbCls.DbType == DbCls.eDbType.SQLServer) { DbCls.ReplacePlaceHolder(Global.cCmdSel); }
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            DbCls.ReplacePlaceHolder(Global.cCmdSel);
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        /// <summary>
        /// 対象取引先を使用した入金（期日）データが存在するか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：存在する、false：存在しない</returns>
        public bool Exists_Nyukin_Data(string sTRCD, string sHJCD)
        {
            bool result = false;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            Global.cCmdSel.CommandText = "SELECT * FROM SS_SJDATA WHERE RTRIM(TRCD) = :p AND HJCD = :p AND DKINDKBN = 0";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            //if (DbCls.DbType == DbCls.eDbType.SQLServer) { DbCls.ReplacePlaceHolder(Global.cCmdSel); }
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            DbCls.ReplacePlaceHolder(Global.cCmdSel);
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        /// <summary>
        /// 対象取引先を使用した支払（期日）データが存在するか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：存在する、false：存在しない</returns>
        public bool Exists_Siharai_Data(string sTRCD, string sHJCD)
        {
            bool result = false;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            Global.cCmdSel.CommandText = "SELECT * FROM SS_SJDATA WHERE RTRIM(TRCD) = :p AND HJCD = :p AND DKINDKBN = 1";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            //if (DbCls.DbType == DbCls.eDbType.SQLServer) { DbCls.ReplacePlaceHolder(Global.cCmdSel); }
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            DbCls.ReplacePlaceHolder(Global.cCmdSel);
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        /// <summary>
        /// 対象取引先が、相殺連結マスターの仕入先に登録済みかどうか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：登録済み、false：未登録</returns>
        public bool Exists_Sousai_Siire(string sTRCD, string sHJCD)
        {
            bool result = false;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            Global.cCmdSel.CommandText = "SELECT * FROM SS_SOUSAIM WHERE RTRIM(SICD) = :p AND SIHJCD = :p";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SICD", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIHJCD", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            //if (DbCls.DbType == DbCls.eDbType.SQLServer) { DbCls.ReplacePlaceHolder(Global.cCmdSel); }
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIHJCD", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            DbCls.ReplacePlaceHolder(Global.cCmdSel);
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        /// <summary>
        /// 対象取引先が、相殺連結マスターの得意先に登録済みかどうか
        /// </summary>
        /// <param name="sTRCD">取引先コード</param>
        /// <param name="sHJCD">補助コード</param>
        /// <returns>true：登録済み、false：未登録</returns>
        public bool Exists_Sousai_Tokui(string sTRCD, string sHJCD)
        {
            bool result = false;

            Global.cCmdSel.CommandTimeout = DbCls.CmdTimeOut;
            Global.cCmdSel.CommandText = "SELECT * FROM SS_SOUSAIM WHERE RTRIM(TKCD) = :p AND TKHJCD = :p";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
            Global.cCmdSel.Parameters.Clear();
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TKCD", sTRCD.TrimEnd());
            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
            //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TKHJCD", string.IsNullOrEmpty(sHJCD) ? "0" : sHJCD);
            //if (DbCls.DbType == DbCls.eDbType.SQLServer) { DbCls.ReplacePlaceHolder(Global.cCmdSel); }
            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TKHJCD", string.IsNullOrEmpty(sHJCD) ? 0 : int.Parse(sHJCD));
            DbCls.ReplacePlaceHolder(Global.cCmdSel);
            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

            using (DbDataReader reader = Global.cCmdSel.ExecuteReader())
            {
                result = reader.HasRows;
            }

            return result;
        }

        public bool Chk_SS_SHDATA(string sTRCD, string sHJCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                //取引先の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SHDATA "
                                           + " WHERE RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND HJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SS_SHDATA　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_SS_SJDATA(string sTRCD, string sHJCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                //取引先の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SJDATA "
                                           + " WHERE (RTRIM(TRCD) = :p AND HJCD = :p)"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "    OR (RTRIM(SJ_JTSAKI) = :p AND JTHJCD = :p)"
                                           + "    OR (RTRIM(SJ_SIHARAI) = :p AND SIHARAIHJCD = :p)";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SJ_JTSAKI", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@JTHJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SJ_SIHARAI", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIHARAIHJCD", int.Parse(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SS_SJDATA　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_SS_SHDATA(string sTRCD, string sHJCD, string sSHOID)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先情報の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SHDATA "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND HJCD = :p"
                                           + "   AND SH_TORIBNK = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SH_TORIBNK", int.Parse(sSHOID));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SS_SHDATA　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_SS_SHDATA(string sTRCD, string sHJCD, string sBCOD, string sKCOD, string sSKBN)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //**>>ICS-S 2103/05/17
                if (sBCOD == "" && sKCOD == "")
                {
                    //支払方法の存在チェック
                    Global.cCmdSel.CommandText = " SELECT * "
                                               + " FROM SS_SHDATA "
                                               + " WHERE 1=1"
                                               + "   AND RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                               + "   AND HJCD = :p"
                                               + "   AND SH_KUBN = :p";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SH_KUBN", int.Parse(sSKBN));
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }
                else if (sBCOD == "" && sKCOD != "")
                {
                    //支払方法の存在チェック
                    Global.cCmdSel.CommandText = " SELECT * "
                                               + " FROM SS_SHDATA "
                                               + " WHERE 1=1"
                                               + "   AND RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                               + "   AND HJCD = :p"
                                               + "   AND KICD = :p"
                                               + "   AND SH_KUBN = :p";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KICD", sKCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SH_KUBN", int.Parse(sSKBN));
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }
                else if (sBCOD != "" && sKCOD == "")
                {
                    //支払方法の存在チェック
                    Global.cCmdSel.CommandText = " SELECT * "
                                               + " FROM SS_SHDATA "
                                               + " WHERE 1=1"
                                               + "   AND RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                               + "   AND HJCD = :p"
                                               + "   AND BMNCD = :p"
                                               + "   AND SH_KUBN = :p";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@BMNCD", sBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KICD", sKCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SH_KUBN", int.Parse(sSKBN));
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                }
                //**<<ICS-E
                else
                {
                    //支払方法の存在チェック
                    Global.cCmdSel.CommandText = " SELECT * "
                                               + " FROM SS_SHDATA "
                                               + " WHERE 1=1"
                                               + "   AND RTRIM(TRCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                               + "   AND HJCD = :p"
                                               + "   AND BMNCD = :p"
                                               + "   AND KICD = :p"
                                               + "   AND SH_KUBN = :p";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@BMNCD", sBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KICD", sKCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SH_KUBN", int.Parse(sSKBN));
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
                    //**>>ICS-S 2013/05/17
                }
                //**<<ICS-E

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SS_SHDATA　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_SaikenDaihyo(string sTRCD, string sHJCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先情報の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM TBLSAIKEN "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(NYDAICD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND NYDAIHJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@NYDAICD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@NYDAIHJCD", int.Parse(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/21>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SaikenDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Chk_SaimuDaihyo(string sTRCD, string sHJCD)
        {
            if (string.IsNullOrEmpty(sTRCD) || string.IsNullOrEmpty(sHJCD))
            {
                return false;
            }

            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先情報の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SDAIHYO "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(SICD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND SIHJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SICD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIHJCD", int.Parse(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_SaimuDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public bool Get_SaikenDaihyo(string sTRCD, string sHJCD, out string sNYDAICD, out string sNYDAIHJCD)
        {
            sNYDAICD = "";
            sNYDAIHJCD = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先情報の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM TBLSAIKEN "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(TOKUCD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND HJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TOKUCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", Global.ObjectToInt(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sNYDAICD = reader["NYDAICD"].ToString();
                    sNYDAIHJCD = reader["NYDAIHJCD"].ToString();
                }

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SaikenDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 自分が債権代表者のレコードにあるか
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>        
        public bool Get_MySaikenDaihyo(string sNYDAICD, string sNYDAIHJCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //レコードの存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM TBLSAIKEN "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(NYDAICD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND NYDAIHJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@NYDAICD", sNYDAICD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@NYDAIHJCD", Global.ObjectToInt(sNYDAIHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_MySaikenDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/15>
        public bool Get_SaimuDaihyo(string sTRCD, string sHJCD, out string sSIDAICD, out string sSIDAIHJCD)
        {
            sSIDAICD = "";
            sSIDAIHJCD = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先情報の存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SDAIHYO "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(SICD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND SIHJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SICD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIHJCD", Global.ObjectToInt(sHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSIDAICD = reader["SIDAICD"].ToString();
                    sSIDAIHJCD = reader["SIDAIHJCD"].ToString();
                }

                return reader.HasRows;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SaimuDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        /// <summary>
        /// 自分が債務代表者のレコードにあるか
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>        
        public bool Get_MySaimuDaihyo(string sSIDAICD, string sSIDAIHJCD)
        {
            if (string.IsNullOrEmpty(sSIDAICD) || string.IsNullOrEmpty(sSIDAIHJCD))
            {
                return false;
            }

            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //レコードの存在チェック
                Global.cCmdSel.CommandText = " SELECT * "
                                           + " FROM SS_SDAIHYO "
                                           + " WHERE 1=1"
                                           + "   AND RTRIM(SIDAICD) = :p"//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                                           + "   AND SIDAIHJCD = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIDAICD", sSIDAICD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SIDAIHJCD", Global.ObjectToInt(sSIDAIHJCD));
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_MySaimuDaihyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/15>
        /// <summary>
        /// 支払方法名称の取得
        /// </summary>
        /// <param name="sSHINO"></param>
        public string Get_SHINM(string sSHINO)
        {
            string sSHINM = "";
            try
            {
                if (sSHINO == "")
                {
                    return sSHINM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SHOHO.Select(支払名称の取得)
                Global.cCmdSel.CommandText = "SELECT SICOMENT FROM SS_SHOHO WHERE SHINO = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHINO", int.Parse(sSHINO));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSHINM = reader["SICOMENT"].ToString();
                }
                return sSHINM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SHINM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sSHINM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

//        /// <summary>
//        /// 自社銀行情報の取得
//        /// </summary>
//        /// <param name="nOwnId"></param>
//        public DataTable GetOwnBankInfo(decimal nOwnId)
//        {
//            StringBuilder sbSql = new StringBuilder();
//            sbSql.AppendLine("SELECT");
//            sbSql.AppendLine("     *");
//            sbSql.AppendLine("FROM");
//            sbSql.AppendLine("    SS_OWNBK");
//            sbSql.AppendLine("WHERE");
//            sbSql.AppendFormat("    OWNID = '{0}'", nOwnId);

//            DataTable dtResult = new DataTable();

//            DbDataAdapter cdaBank = DbCls.CreateDataAdapterObject();
//            DbCommand cocSqlSel = DbCls.CreateCommandObject(sbSql.ToString(), Global.cConSaikenSaimu);
//            try
//            {
//                cdaBank.SelectCommand = cocSqlSel;
//                cdaBank.Fill(dtResult);
//            }
//            catch (Exception e)
//            {
//                //エラーメッセージ
//                MessageBox.Show(
////-- <2016/03/11 文言等>
////                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
////                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "　Ver" + Global.sPrgVer,
//                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
////-- <2016/03/11>
//            }
//            finally
//            {
//                //リソース開放
//                if (cocSqlSel != null)
//                {
//                    cocSqlSel.Dispose();
//                }
//                if (cdaBank != null)
//                {
//                    cdaBank.Dispose();
//                }
//            }
//            return dtResult;
//        }

        public DataTable GetOwnBankInfo(int nOwnId)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendLine("SELECT");
                sbSql.AppendLine("     *");
                sbSql.AppendLine("FROM");
                sbSql.AppendLine("    SS_OWNBK");
                sbSql.AppendLine("WHERE");
                sbSql.AppendFormat("    OWNID = '{0}'", nOwnId);

                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("OWNBKCOD", typeof(string));
                dtResult.Columns.Add("OWNBRCOD", typeof(string));
                dtResult.Columns.Add("YOKNKIND", typeof(string));
                dtResult.Columns.Add("KOZANO", typeof(string));

                Global.cCmdSel.CommandText = sbSql.ToString();
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        DataRow dr = dtResult.NewRow();

                        dr["OWNBKCOD"] = reader["OWNBKCOD"].ToString();
                        dr["OWNBRCOD"] = reader["OWNBRCOD"].ToString();
                        dr["YOKNKIND"] = reader["YOKNKIND"].ToString();
                        dr["KOZANO"] = reader["KOZANO"].ToString();
                        
                        dtResult.Rows.Add(dr);
                    }
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGetOwnBankInfo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <2016/03/11>

        public DataTable GetTesuuIdInfo_OLD(string sOwnBkCod)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendLine("SELECT DISTINCT");
            sbSql.AppendLine("     TI.TESUID,");
            sbSql.AppendLine("     TI.TESUNAM");
            sbSql.AppendLine("FROM");
            sbSql.AppendLine("    SS_TESUID TI");
            sbSql.AppendFormat("    INNER JOIN SS_FRGEN FG ON FG.TESUID = TI.TESUID AND FG.OWNBKCOD = '{0}'\r\n", sOwnBkCod);
            sbSql.AppendLine("ORDER BY TI.TESUID");

            DataTable dtResult = new DataTable();

            DbDataAdapter cdaBank = DbCls.CreateDataAdapterObject();
            DbCommand cocSqlSel = DbCls.CreateCommandObject(sbSql.ToString(), Global.cConSaikenSaimu);
            try
            {
                cdaBank.SelectCommand = cocSqlSel;
                cdaBank.Fill(dtResult);
            }
            catch (Exception e)
            {
                //エラーメッセージ
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + e.Message + "\r\nGetTesuuIdInfo_OLD　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                //リソース開放
                if (cocSqlSel != null)
                {
                    cocSqlSel.Dispose();
                }
                if (cdaBank != null)
                {
                    cdaBank.Dispose();
                }
            }
            return dtResult;
        }
        public DataTable GetTesuuIdInfo(string sOwnBkCod)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendLine("SELECT DISTINCT");
                sbSql.AppendLine("     TI.TESUID,");
                sbSql.AppendLine("     TI.TESUNAM");
                sbSql.AppendLine("FROM");
                sbSql.AppendLine("    SS_TESUID TI");
//-- <>
//                sbSql.AppendFormat("    INNER JOIN SS_FRGEN FG ON FG.TESUID = TI.TESUID AND FG.OWNBKCOD = '{0}'\r\n", sOwnBkCod);
                sbSql.AppendLine("    INNER JOIN SS_FRGEN FG ON FG.TESUID = TI.TESUID\r\n");
//-- <>       
                sbSql.AppendLine("ORDER BY TI.TESUID");

                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("TESUID", typeof(int));
                dtResult.Columns.Add("TESUNAM", typeof(string));

                Global.cCmdSel.CommandText = sbSql.ToString();
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        DataRow dr = dtResult.NewRow();

                        dr["TESUID"] = Convert.ToInt32(reader["TESUID"].ToString());
                        dr["TESUNAM"] = reader["TESUNAM"].ToString();

                        dtResult.Rows.Add(dr);
                    }
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGetTesuuIdInfo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return null;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public string Get_TesuuIdNm(string sTesuuID)
        {
            string sResult = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendLine("SELECT");
                sbSql.AppendLine("     TESUNAM");
                sbSql.AppendLine("FROM");
                sbSql.AppendLine("    SS_TESUID");
                sbSql.AppendLine("WHERE");
                sbSql.AppendFormat("    TESUID = {0}\r\n", sTesuuID);


                Global.cCmdSel.CommandText = sbSql.ToString();
                Global.cCmdSel.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        sResult = reader["TESUNAM"].ToString();
                    }
                }

                return sResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sResult;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 支払方法名称の取得
        /// </summary>
        /// <param name="sSHINO"></param>
        public void Sel_SHINM(string sSHINO, out string sSHINM)
        {
            sSHINM = "";
            try
            {
                if (sSHINO == "")
                {
                    return;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SHOHO.Select(支払名称の取得)
                //Global.cCmdSel.CommandText = "SELECT A.SICOMENT, "
                //                           + "A.SI_BANK1, BK1.BKNAM AS BKNAM1, A.SI_SITEN1, BR1.BRNAM AS BRNAM1, A.SI_KOZA1, A.SI_KOZANO1, A.SI_IRAININ1, OBK1.FACNAM AS FACNAM1, "
                //                           + "A.SI_BANK2, BK2.BKNAM AS BKNAM2, A.SI_SITEN2, BR2.BRNAM AS BRNAM2, A.SI_KOZA2, A.SI_KOZANO2, A.SI_IRAININ2, OBK2.FACNAM AS FACNAM2, "
                //                           + "A.SI_BANK3, BK3.BKNAM AS BKNAM3, A.SI_SITEN3, BR3.BRNAM AS BRNAM3, A.SI_KOZA3, A.SI_KOZANO3, A.SI_IRAININ3, OBK3.FACNAM AS FACNAM3, "
                //                           + "A.SI_BANK4, BK4.BKNAM AS BKNAM4, A.SI_SITEN4, BR4.BRNAM AS BRNAM4, A.SI_KOZA4, A.SI_KOZANO4, A.SI_IRAININ4, OBK4.FACNAM AS FACNAM4 "
                //                           + "FROM SS_SHOHO A "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BANK BK1 ON BK1.BKCOD = A.SI_BANK1 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BANK BK2 ON BK2.BKCOD = A.SI_BANK2 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BANK BK3 ON BK3.BKCOD = A.SI_BANK3 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BANK BK4 ON BK4.BKCOD = A.SI_BANK4 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BRANCH BR1 ON BR1.BKCOD = A.SI_BANK1 AND BR1.BRCOD = A.SI_SITEN1 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BRANCH BR2 ON BR2.BKCOD = A.SI_BANK1 AND BR2.BRCOD = A.SI_SITEN2 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BRANCH BR3 ON BR3.BKCOD = A.SI_BANK1 AND BR3.BRCOD = A.SI_SITEN3 "
                //                           + "LEFT JOIN ICSP_312Z" + Global.cKaisya.sCCOD + "..BRANCH BR4 ON BR4.BKCOD = A.SI_BANK1 AND BR4.BRCOD = A.SI_SITEN4 "
                //                           + "LEFT JOIN SS_OWNBK OBK1 ON OBK1.OWNBKCOD = A.SI_BANK1 AND OBK1.OWNBRCOD = A.SI_SITEN1 AND OBK1.YOKNKIND = A.SI_KOZA1 AND OBK1.KOZANO = A.SI_KOZANO1 AND OBK1.IRAININ = A.SI_IRAININ1 "
                //                           + "LEFT JOIN SS_OWNBK OBK2 ON OBK2.OWNBKCOD = A.SI_BANK2 AND OBK2.OWNBRCOD = A.SI_SITEN2 AND OBK2.YOKNKIND = A.SI_KOZA2 AND OBK2.KOZANO = A.SI_KOZANO2 AND OBK2.IRAININ = A.SI_IRAININ2 "
                //                           + "LEFT JOIN SS_OWNBK OBK3 ON OBK3.OWNBKCOD = A.SI_BANK3 AND OBK3.OWNBRCOD = A.SI_SITEN3 AND OBK3.YOKNKIND = A.SI_KOZA3 AND OBK3.KOZANO = A.SI_KOZANO3 AND OBK3.IRAININ = A.SI_IRAININ3 "
                //                           + "LEFT JOIN SS_OWNBK OBK4 ON OBK4.OWNBKCOD = A.SI_BANK4 AND OBK4.OWNBRCOD = A.SI_SITEN4 AND OBK4.YOKNKIND = A.SI_KOZA4 AND OBK4.KOZANO = A.SI_KOZANO4 AND OBK4.IRAININ = A.SI_IRAININ4 "
                //                           + "WHERE SHINO = :p ";
                Global.cCmdSel.CommandText = "SELECT SICOMENT FROM SS_SHOHO WHERE SHINO = :p";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHINO", int.Parse(sSHINO));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSHINM = reader["SICOMENT"].ToString();
                    //Global.SI_BANK1_tb3 = reader["SI_BANK1"].ToString();
                    //Global.SI_BANKNM1_tb3 = reader["BKNAM1"].ToString();
                    //Global.SI_SITEN1_tb3 = reader["SI_SITEN1"].ToString();
                    //Global.SI_SITENNM1_tb3 = reader["BRNAM1"].ToString();
                    //Global.SI_KOZA1_tb3 = reader["SI_KOZA1"].ToString();
                    //Global.SI_KOZANO1_tb3 = reader["SI_KOZANO1"].ToString();
                    //Global.SI_IRAININ1_tb3 = reader["SI_IRAININ1"].ToString();
                    //Global.FACNAM1_tb3 = reader["FACNAM1"].ToString();
                    //Global.SI_BANK2_tb3 = reader["SI_BANK2"].ToString();
                    //Global.SI_BANKNM2_tb3 = reader["BKNAM2"].ToString();
                    //Global.SI_SITEN2_tb3 = reader["SI_SITEN2"].ToString();
                    //Global.SI_SITENNM2_tb3 = reader["BRNAM2"].ToString();
                    //Global.SI_KOZA2_tb3 = reader["SI_KOZA2"].ToString();
                    //Global.SI_KOZANO2_tb3 = reader["SI_KOZANO2"].ToString();
                    //Global.SI_IRAININ2_tb3 = reader["SI_IRAININ2"].ToString();
                    //Global.FACNAM2_tb3 = reader["FACNAM2"].ToString();
                    //Global.SI_BANK3_tb3 = reader["SI_BANK3"].ToString();
                    //Global.SI_BANKNM3_tb3 = reader["BKNAM3"].ToString();
                    //Global.SI_SITEN3_tb3 = reader["SI_SITEN3"].ToString();
                    //Global.SI_SITENNM3_tb3 = reader["BRNAM3"].ToString();
                    //Global.SI_KOZA3_tb3 = reader["SI_KOZA3"].ToString();
                    //Global.SI_KOZANO3_tb3 = reader["SI_KOZANO3"].ToString();
                    //Global.SI_IRAININ3_tb3 = reader["SI_IRAININ3"].ToString();
                    //Global.FACNAM3_tb3 = reader["FACNAM3"].ToString();
                    //Global.SI_BANK4_tb3 = reader["SI_BANK4"].ToString();
                    //Global.SI_BANKNM4_tb3 = reader["BKNAM4"].ToString();
                    //Global.SI_SITEN4_tb3 = reader["SI_SITEN4"].ToString();
                    //Global.SI_SITENNM4_tb3 = reader["BRNAM4"].ToString();
                    //Global.SI_KOZA4_tb3 = reader["SI_KOZA4"].ToString();
                    //Global.SI_KOZANO4_tb3 = reader["SI_KOZANO4"].ToString();
                    //Global.SI_IRAININ4_tb3 = reader["SI_IRAININ4"].ToString();
                    //Global.FACNAM4_tb3 = reader["FACNAM4"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SHINM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 支払区分名称の取得
        /// </summary>
        /// <param name="sSKBNCOD"></param>
        public string Get_SKUBN(string sSKBNCOD)
        {
            string sSKBNM = "";
            try
            {
                if ((sSKBNCOD == "") ||
                    (sSKBNCOD == null))
                {
                    return sSKBNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分名の取得)
                Global.cCmdSel.CommandText = "SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SKBNCOD", int.Parse(sSKBNCOD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSKBNM = reader["SKBNM"].ToString();
                }
                return sSKBNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SKUBN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sSKBNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- < 改良>
        public string Get_NKUBN(string sKUBUNCD)
        {
            string sSKBNM = "";
            try
            {
                if ((sKUBUNCD == "") ||
                    (sKUBUNCD == null))
                {
                    return sSKBNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分名の取得)
                Global.cCmdSel.CommandText = "SELECT KUBUNMEI FROM TBLKUBUN WHERE SIKIBETU = '2' AND KUBUNCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KUBUNCD", sKUBUNCD);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSKBNM = reader["KUBUNMEI"].ToString();
                }
                return sSKBNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_NKUBN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
                return sSKBNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        public string Get_NKUBN(string sKUBUNCD, short nSW)
        {
            string sSKBNM = "";
            try
            {
                if ((sKUBUNCD == "") ||
                    (sKUBUNCD == null))
                {
                    return sSKBNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分名の取得)
                if (nSW == 0)
                {
                    Global.cCmdSel.CommandText = "SELECT KUBUNMEI FROM TBLKUBUN WHERE SIKIBETU = '2' AND KUBUNCD = :p AND SKBSW = 1 ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                }
                else
                {
                    Global.cCmdSel.CommandText = "SELECT SKBKIND FROM TBLKUBUN WHERE SIKIBETU = '2' AND KUBUNCD = :p AND SKBSW = 1 ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                }
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KUBUNCD", sKUBUNCD);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();
                    if (nSW == 0)
                    {
                        sSKBNM = reader["KUBUNMEI"].ToString();
                    }
                    else
                    {
                        sSKBNM = reader["SKBKIND"].ToString();
                        switch (sSKBNM)
                        {
                            case "2":
                            case "21":
                            case "22":
                            case "98":
                                sSKBNM = "1";
                                break;
                            default:
                                sSKBNM = "0";
                                break;
                        }
                    }
                }
                return sSKBNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_NKUBN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
                return sSKBNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
//-- <>

        public string Get_NKUBN_Display(string sKUBUNCD)
        {
            string sSKBNM = "";
            try
            {
                if ((sKUBUNCD == "") ||
                    (sKUBUNCD == null))
                {
                    return sSKBNM;
                }

                if (sKUBUNCD == "0")
                {
                    return " 0:なし";
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                Global.cCmdSel.CommandText = "SELECT KUBUNMEI FROM TBLKUBUN WHERE SIKIBETU = '2' AND KUBUNCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「''」のみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KUBUNCD", sKUBUNCD);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    reader.Read();

                    sSKBNM = sKUBUNCD.PadLeft(2) + ":" + reader["KUBUNMEI"].ToString();
                }
                return sSKBNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_NKUBN_Display　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sSKBNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public string Get_UserName(string sUsr)
        {
            string sUNam = "";
            try
            {
                if ((sUsr == "") || (sUsr == null))
                {
                    return sUNam;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                //★SS_SKUBN.Select(支払区分名の取得)
                Global.cCmdCommonSel.CommandText = "SELECT UNAM FROM USRTBL WHERE UCOD = :p ";
                Global.cCmdCommonSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@UCOD", int.Parse(sUsr));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdCommonSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sUNam = reader["UNAM"].ToString();
                }
                return sUNam;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_UserName　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
                return sUNam;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 銀行名称の取得
        /// </summary>
        /// <param name="sBANK_CD"></param>
        public string Get_BANKNM(string sBANK_CD)
        {
            string sBANKNM = "";
            try
            {
                if ((sBANK_CD == "") ||
                    (sBANK_CD == null))
                {
                    return sBANKNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★BANK.Select(銀行名の取得)
                Global.cCmdSelZ.CommandText = "SELECT BKNAM FROM BANK WHERE BKCOD = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BKCOD", sBANK_CD);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sBANKNM = reader["BKNAM"].ToString();
                }
                return sBANKNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_BANKNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
                return sBANKNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 支店名称の取得
        /// </summary>
        /// <param name="sBANK_CD"></param>
        /// <param name="sSITEN_ID"></param>
        public string Get_SITENNM(string sBANK_CD, string sSITEN_ID)
        {
            string sSITENNM = "";
            try
            {
                if ((sBANK_CD == "") ||
                    (sSITEN_ID == "") ||
                    (sBANK_CD == null) ||
                    (sSITEN_ID == null))
                {
                    return sSITENNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★BRANCH.Select(支店名の取得)
                Global.cCmdSelZ.CommandText = "SELECT BRNAM FROM BRANCH WHERE BKCOD = :p AND BRCOD = :p ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BKCOD", sBANK_CD);
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@BRCOD", sSITEN_ID);
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSITENNM = reader["BRNAM"].ToString();
                }
                return sSITENNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SITENNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sSITENNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 支払区分ｺｰﾄﾞの支払種別を返却
        /// </summary>
        /// <param name="sSKBNCOD"></param>
        /// <returns></returns>
        public string Get_SKBKIND(string sSKBNCOD)
        {
            string sSKBKIND = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払種別の取得)
                Global.cCmdSel.CommandText = "SELECT SKBKIND FROM SS_SKUBN WHERE SKKBN = 11 AND SKBNCOD = :p ";
                Global.cCmdSel.Parameters.Clear();
                //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                if (!string.IsNullOrEmpty(sSKBNCOD))
                {
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SKBNCOD", int.Parse(sSKBNCOD));
                }
                else
                {
                    sSKBNCOD = null;
                //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SKBNCOD", sSKBNCOD);
                }
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSKBKIND = reader["SKBKIND"].ToString();
                }
                return sSKBKIND;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SKBKIND　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sSKBKIND;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        //源泉区分の取得
        public string Get_GGKBNNM(string sGENSEN, string sGOU, string sGGKBN)
        {
            string GGKBNNM = "";
            try
            {
                if ((sGENSEN == "") ||
                    (sGOU == "") ||
                    (sGGKBN == ""))
                {
                    return GGKBNNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_GENTBL.Select(源泉区分の取得)
                Global.cCmdSel.CommandText = "SELECT KBNNM FROM SS_GENTBL WHERE CALKBN = :p AND GOU = :p AND KBN = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@CALKBN", sGENSEN);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GOU", sGOU);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KBN", sGGKBN);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    GGKBNNM = reader["SKBKIND"].ToString();
                }
                return GGKBNNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_GGKBNNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <>
                return GGKBNNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        //取引先CD・補助CD・支払IDに紐付くSS_TSHOHの期日補正及び休日補正の取得
        public void Get_HoseiDT(string sTRCD, string sHJCD, string sSHO_ID, out string sHaraiDT, out string sKijituDT)
        {
            sTRCD = GetTrcdDB(sTRCD);
            sHaraiDT = "";
            sKijituDT = "";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_TSHOH.Select(支払日補正/支払期日補正の取得
                Global.cCmdSel.CommandText = "SELECT HARAI_H, KIJITU_H FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHO_ID", sSHO_ID);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHO_ID", int.Parse(sSHO_ID));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sHaraiDT = reader["HARAI_H"].ToString();
                    sKijituDT = reader["KIJITU_H"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_HoseiDT　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        //取引先CD・補助CD・支払IDに紐付くSS_TSHOHの依頼先情報の取得
        public void Get_TRCD_Tb4(string sTRCD, string sHJCD, string sSHO_ID, out string[,] sTRCD_Tb3Array)
        {
            sTRCD = GetTrcdDB(sTRCD);
            sTRCD_Tb3Array = new string[4, 6];
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_TSHOH.Select(支払日補正/支払期日補正の取得
                //Global.cCmdSel.CommandText = "SELECT SI_KUBN1, SI_BANK1, SI_SITEN1, SI_KOZA1, SI_KOZANO1, SI_IRAININ1, "
                //                           + "SI_KUBN2, SI_BANK2, SI_SITEN2, SI_KOZA2, SI_KOZANO2, SI_IRAININ2, "
                //                           + "SI_KUBN3, SI_BANK3, SI_SITEN3, SI_KOZA3, SI_KOZANO3, SI_IRAININ3, "
                //                           + "SI_KUBN4, SI_BANK4, SI_SITEN4, SI_KOZA4, SI_KOZANO4, SI_IRAININ4 "
                //                           + "FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p ";
                Global.cCmdSel.CommandText = "SELECT SI_KUBN1, OWNID1, SI_KUBN2, OWNID2, SI_KUBN3, OWNID3, SI_KUBN4, OWNID4 "
                                           + "FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p ";
//Global.cCmdSel.CommandText = @"SELECT 
//	TS.SHO_ID, TS.BCOD, TS.KICD, TS.SHINO, TS.HARAI_H, TS.KIJITU_H, 
//	KB1.SKBKIND KIND1, TS.SI_KUBN1, OB1.OWNBKCOD SI_BANK1, OB1.OWNBRCOD SI_SITEN1, OB1.YOKNKIND SI_KOZA1, OB1.KOZANO SI_KOZANO1,
//	KB2.SKBKIND KIND2, TS.SI_KUBN2, OB2.OWNBKCOD SI_BANK2, OB2.OWNBRCOD SI_SITEN2, OB2.YOKNKIND SI_KOZA2, OB2.KOZANO SI_KOZANO2,
//	KB3.SKBKIND KIND3, TS.SI_KUBN3, OB3.OWNBKCOD SI_BANK3, OB3.OWNBRCOD SI_SITEN3, OB3.YOKNKIND SI_KOZA3, OB3.KOZANO SI_KOZANO3,
//	KB4.SKBKIND KIND4, TS.SI_KUBN4, OB4.OWNBKCOD SI_BANK4, OB4.OWNBRCOD SI_SITEN4, OB4.YOKNKIND SI_KOZA4, OB4.KOZANO SI_KOZANO4,
//	OBF1.OWNBKCOD SI_BANKF1, OBF1.OWNBRCOD SI_SITENF1, OBF1.YOKNKIND SI_KOZAF1, OBF1.KOZANO SI_KOZANOF1,
//	OBF2.OWNBKCOD SI_BANKF2, OBF2.OWNBRCOD SI_SITENF2, OBF2.YOKNKIND SI_KOZAF2, OBF2.KOZANO SI_KOZANOF2,
//	OBF3.OWNBKCOD SI_BANKF3, OBF3.OWNBRCOD SI_SITENF3, OBF3.YOKNKIND SI_KOZAF3, OBF3.KOZANO SI_KOZANOF3,
//	OBF4.OWNBKCOD SI_BANKF4, OBF4.OWNBRCOD SI_SITENF4, OBF4.YOKNKIND SI_KOZAF4, OBF4.KOZANO SI_KOZANOF4
//FROM SS_TSHOH TS
//    LEFT JOIN SS_SKUBN KB1 ON KB1.SKKBN = 11 AND KB1.SKBNCOD = TS.SI_KUBN1
//    LEFT JOIN SS_SKUBN KB2 ON KB2.SKKBN = 11 AND KB2.SKBNCOD = TS.SI_KUBN2
//    LEFT JOIN SS_SKUBN KB3 ON KB3.SKKBN = 11 AND KB3.SKBNCOD = TS.SI_KUBN3
//    LEFT JOIN SS_SKUBN KB4 ON KB4.SKKBN = 11 AND KB4.SKBNCOD = TS.SI_KUBN4
//	LEFT JOIN SS_OWNBK OB1 ON OB1.OWNID = TS.OWNID1
//	LEFT JOIN SS_OWNBK OB2 ON OB2.OWNID = TS.OWNID2
//	LEFT JOIN SS_OWNBK OB3 ON OB3.OWNID = TS.OWNID3
//	LEFT JOIN SS_OWNBK OB4 ON OB4.OWNID = TS.OWNID4
//	LEFT JOIN SS_FACTER FC1 ON FC1.FACID = TS.OWNID1
//	LEFT JOIN SS_FACTER FC2 ON FC2.FACID = TS.OWNID2
//	LEFT JOIN SS_FACTER FC3 ON FC3.FACID = TS.OWNID3
//	LEFT JOIN SS_FACTER FC4 ON FC4.FACID = TS.OWNID4
//	LEFT JOIN SS_OWNBK OBF1 ON OBF1.OWNID = FC1.OWNID
//	LEFT JOIN SS_OWNBK OBF2 ON OBF2.OWNID = FC2.OWNID
//	LEFT JOIN SS_OWNBK OBF3 ON OBF3.OWNID = FC3.OWNID
//	LEFT JOIN SS_OWNBK OBF4 ON OBF4.OWNID = FC4.OWNID
//WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p 
//ORDER BY TS.SHO_ID 
//";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHO_ID", sSHO_ID);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHO_ID", int.Parse(sSHO_ID));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sTRCD_Tb3Array[0, 0] = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
                    sTRCD_Tb3Array[0, 1] = DbCls.GetStrNullKara(reader["OWNID1"].ToString());

                    sTRCD_Tb3Array[1, 0] = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
                    sTRCD_Tb3Array[1, 1] = DbCls.GetStrNullKara(reader["OWNID2"].ToString());

                    sTRCD_Tb3Array[2, 0] = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
                    sTRCD_Tb3Array[2, 1] = DbCls.GetStrNullKara(reader["OWNID3"].ToString());

                    sTRCD_Tb3Array[3, 0] = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
                    sTRCD_Tb3Array[3, 1] = DbCls.GetStrNullKara(reader["OWNID4"].ToString());
                }
                else
                {
                    {
                        sTRCD_Tb3Array[0, 0] = "";
                        sTRCD_Tb3Array[0, 1] = "";
                        sTRCD_Tb3Array[0, 2] = "";
                        sTRCD_Tb3Array[0, 3] = "";
                        sTRCD_Tb3Array[0, 4] = "";
                        sTRCD_Tb3Array[0, 5] = "";

                        sTRCD_Tb3Array[1, 0] = "";
                        sTRCD_Tb3Array[1, 1] = "";
                        sTRCD_Tb3Array[1, 2] = "";
                        sTRCD_Tb3Array[1, 3] = "";
                        sTRCD_Tb3Array[1, 4] = "";
                        sTRCD_Tb3Array[1, 5] = "";

                        sTRCD_Tb3Array[2, 0] = "";
                        sTRCD_Tb3Array[2, 1] = "";
                        sTRCD_Tb3Array[2, 2] = "";
                        sTRCD_Tb3Array[2, 3] = "";
                        sTRCD_Tb3Array[2, 4] = "";
                        sTRCD_Tb3Array[2, 5] = "";

                        sTRCD_Tb3Array[3, 0] = "";
                        sTRCD_Tb3Array[3, 1] = "";
                        sTRCD_Tb3Array[3, 2] = "";
                        sTRCD_Tb3Array[3, 3] = "";
                        sTRCD_Tb3Array[3, 4] = "";
                        sTRCD_Tb3Array[3, 5] = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TRCD_Tb4　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// グループ名称の取得
        /// </summary>
        /// <param name="sSKBNCOD"></param>
        public string Get_GrpNm(string sGRPID)
        {
            string sGRPNM = "";
            try
            {
                if ((sGRPID == "") ||
                    (sGRPID == null))
                {
                    return sGRPNM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分名の取得)
                Global.cCmdSel.CommandText = "SELECT GRPNM FROM SS_GROUP WHERE GRPID = :p ";
                Global.cCmdSel.Parameters.Clear();
                // --->V02.23.01 KKL UPDATE ▼(109109)
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GRPID", sGRPID);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@GRPID", int.Parse(sGRPID));
                // <---V02.23.01 KKL UPDATE ▲(109109)
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sGRPNM = reader["GRPNM"].ToString();
                }
                return sGRPNM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_GrpNm　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sGRPNM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 営業担当者名の取得
        /// </summary>
        /// <param name="sTANTOCD"></param>
        public string Get_ETanNm(string sTANTOCD)
        {
            string sTANTONM = "";
            try
            {
                if ((sTANTOCD == "") ||
                    (sTANTOCD == null))
                {
                    return sTANTONM;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_SKUBN.Select(支払区分名の取得)
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //Global.cCmdSel.CommandText = "SELECT TANTOMEI FROM TBLTANTO WHERE TANTOCD = :p ";
                //Global.cCmdSel.Parameters.Clear();
                //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TANTOCD", sTANTOCD);
                Global.cCmdSel.CommandText = "SELECT TANTOMEI FROM TBLTANTO WHERE RTRIM(TANTOCD) = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TANTOCD", sTANTOCD.Trim());
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sTANTONM = reader["TANTOMEI"].ToString();
                }
                return sTANTONM;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_ETanNm　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sTANTONM;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #endregion


        #region 取引先マスタの検索(通常検索/前行・次行検索/先頭・最終行検索)
        /// <summary>
        /// 指定された取引先CDで取引先マスタを検索
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="sHJCD"></param>
        /// <param name="iCnt"></param>
        /// <param name="iCntHigh"></param>
        /// <param name="iCntLow"></param>
        public void Sel_SS_TORI(string sTRCD, string sHJCD, out int iCnt, out bool bHighDataExist, out bool bLowDataExist)
        {
            iCnt = 0;
            bHighDataExist = false;
            bLowDataExist = false;
            try
            {
                #region read higher data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where length(tr.TRCD) < 13");
                    }
                    else
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where len(tr.TRCD) < 13");
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD > '{0}' or (tr.TRCD = '{0}' and tr.HJCD > {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD > '{0}'", GetTrcdDB(sTRCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        bHighDataExist = (DbCls.GetNumNullZero<int>(reader["nCnt"]) >= 1);
                    }
                }
                #endregion

                #region read lower data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where length(tr.TRCD) < 13");
                    }
                    else
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where len(tr.TRCD) < 13");
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD < '{0}' or (tr.TRCD = '{0}' and tr.HJCD < {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD < '{0}'", GetTrcdDB(sTRCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        bLowDataExist = (DbCls.GetNumNullZero<int>(reader["nCnt"]) >= 1);
                    }
                }
                #endregion

                #region read data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                    //sb.AppendFormat("select * from SS_TORI tr left outer join ICSP_312Z{2}..TRNAM ZT ON tr.trcd = ZT.trcd where tr.TRCD = '{0}' and tr.HJCD = {1}"
                    //    , GetTrcdDB(sTRCD), GetHjcdDB(sHJCD), Global.cKaisya.sCCOD);
                    sb.AppendFormat("select * from SS_TORI tr left outer join {0}TRNAM ZT ON tr.trcd = ZT.trcd where tr.TRCD = '{1}' and tr.HJCD = {2}"
                        , Global.sZJoin, GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)), Global.cKaisya.sCCOD);
                    //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                    //sb.AppendFormat("select * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where tr.TRCD = '{0}' and tr.HJCD = {1}"
                    //    , GetTrcdDB(sTRCD), GetHjcdDB(sHJCD));
                    //sb.AppendFormat("select * from SS_TORI tr where tr.TRCD = '{0}' and tr.HJCD = {1}"
                    //    , GetTrcdDB(sTRCD), GetHjcdDB(sHJCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        Set_SS_TRCD();
                        iCnt++;
                        if (Global.E_TANTOCD != "")
                        {
                            Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
                        }
                        if (Global.SAIMU == "1" && Global.GRPID != "0")
                        {
                            Global.GRPIDNM = Get_GrpNm(Global.GRPID);
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TORI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 取引先(前レコード)の取得
        /// 取引先ｺｰﾄﾞは呼び出される前段階でTrimされている
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="sHJCD"></param>
        /// <param name="iCntHigh"></param>
        /// <param name="iCntLow"></param>
        public void Sel_SS_TORI_Prev(string sTRCD, string sHJCD, out bool bHighDataExist, out bool bLowDataExist)
        {
            bHighDataExist = true;
            bLowDataExist = false;
            try
            {
                #region read lower data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where length(tr.TRCD) < 13");
                    }
                    else
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where len(tr.TRCD) < 13");
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD < '{0}' or (tr.TRCD = '{0}' and tr.HJCD < {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD < '{0}'", GetTrcdDB(sTRCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        bLowDataExist = (DbCls.GetNumNullZero<int>(reader["nCnt"]) > 1);
                    }
                }
                #endregion

                #region read previous data
                {
                    var sb = new StringBuilder();

                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)                    
                    //{
                    //    sb.AppendLine("select * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where ROWNUM = 1 AND length(tr.TRCD) < 13");
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        sb.AppendLine("select * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where length(tr.TRCD) < 13");
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        //sb.AppendLine("select top 1 * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13");
                        sb.AppendFormat("select top 1 * from SS_TORI tr left outer join ICSP_312Z{0}..TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13", Global.sCcod);
                    }
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    //{
                    //    sb.AppendLine("select * from SS_TORI tr where ROWNUM = 1 AND length(tr.TRCD) < 13");
                    //}
                    //else
                    //{
                    //    sb.AppendLine("select top 1 * from SS_TORI tr where len(tr.TRCD) < 13");
                    //}
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD < '{0}' or (tr.TRCD = '{0}' and tr.HJCD < {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD < '{0}'", GetTrcdDB(sTRCD));
                    sb.AppendLine(" order by tr.TRCD desc, tr.HJCD desc");
                    if (ComUtil.IsPostgreSQL()) sb.AppendLine(" limit 1 offset 0");//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        Set_SS_TRCD();
                        if (Global.E_TANTOCD != "")
                        {
                            Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
                        }
                        if (Global.SAIMU == "1" && Global.GRPID != "0")
                        {
                            Global.GRPIDNM = Get_GrpNm(Global.GRPID);
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TORI_Prev　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 取引先(次レコード)の取得
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="sHJCD"></param>
        /// <param name="iCntHigh"></param>
        /// <param name="iCntLow"></param>
        public bool Sel_SS_TORI_Next(string sTRCD, string sHJCD, out bool bHighDataExist, out bool bLowDataExist)
        {
            bool exists = false;
            bHighDataExist = false;
            bLowDataExist = false;
            try
            {
                #region upperdata
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where length(tr.TRCD) < 13");
                    }
                    else
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where len(tr.TRCD) < 13");
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD > '{0}' or (tr.TRCD = '{0}' and tr.HJCD > {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD > '{0}'", GetTrcdDB(sTRCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        bHighDataExist = (DbCls.GetNumNullZero<int>(reader["nCnt"]) > 1);
                    }
                }
                #endregion

                #region lowerdata
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    if(ComUtil.IsPostgreSQL())
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where length(tr.TRCD) < 13");
                    }
                    else
                    {
                        sb.AppendLine("select count(*) nCnt from SS_TORI tr where len(tr.TRCD) < 13");
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD < '{0}' or (tr.TRCD = '{0}' and tr.HJCD <= {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD <= '{0}'", GetTrcdDB(sTRCD));
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        bLowDataExist = (DbCls.GetNumNullZero<int>(reader["nCnt"]) > 0);
                    }
                }
                #endregion

                #region ReadNextData
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    //{
                    //    sb.AppendLine("select * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd where ROWNUM = 1 AND length(tr.TRCD) < 13");
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        sb.AppendLine("select * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd where length(tr.TRCD) < 13");
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        //sb.AppendLine("select top 1 * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13");
                        sb.AppendFormat("select top 1 * from SS_TORI tr  left outer join ICSP_312Z{0}..TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13", Global.sCcod);
                    }
                    if (Global.nTRCD_HJ == 1)
                        sb.AppendFormat(" and (tr.TRCD > '{0}' or (tr.TRCD = '{0}' and tr.HJCD > {1}))"
                            , GetTrcdDB(sTRCD), int.Parse(GetHjcdDB(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    else
                        sb.AppendFormat(" and tr.TRCD > '{0}'", GetTrcdDB(sTRCD));
                    sb.AppendLine(" order by tr.TRCD, tr.HJCD");
                    if (ComUtil.IsPostgreSQL()) sb.AppendLine(" limit 1 offset 0");//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        exists = true;
                        Set_SS_TRCD();
                        if (Global.E_TANTOCD != "")
                        {
                            Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
                        }
                        if (Global.SAIMU == "1" && Global.GRPID != "0")
                        {
                            Global.GRPIDNM = Get_GrpNm(Global.GRPID);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TORI_Next　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
            finally
            {
                DisposeDataReader();
            }
            return exists;
        }


        /// <summary>
        /// 取引先(先頭レコード)の取得
        /// </summary>
        /// <param name="iCnt"></param>
        public void Sel_SS_TORI_1st(out int iCnt)
        {
            iCnt = 0;
            try
            {
                #region read 1st data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    //{
                    //    sb.AppendLine("select * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd where ROWNUM <= 2 length(tr.TRCD) < 13");
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        sb.AppendLine("select * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where length(tr.TRCD) < 13");
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        //sb.AppendLine("select top 2 * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13");
                        sb.AppendFormat("select top 2 * from SS_TORI tr left outer join ICSP_312Z{0}..TRNAM ZT ON tr.trcd = ZT.trcd where len(tr.TRCD) < 13", Global.sCcod);
                    }
                    sb.AppendLine("order by tr.TRCD, tr.HJCD");
                    if (ComUtil.IsPostgreSQL()) sb.AppendLine(" limit 2 offset 0");//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        iCnt = 1;
                        Set_SS_TRCD();
                        while (reader.Read())
                        {
                            iCnt++;
                        }
                        if (Global.E_TANTOCD != "")
                        {
                            Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
                        }
                        if (Global.SAIMU == "1" && Global.GRPID != "0")
                        {
                            Global.GRPIDNM = Get_GrpNm(Global.GRPID);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TORI_1st　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 取引先(最終レコード)の取得
        /// </summary>
        /// <param name="iCnt"></param>
        public void Sel_SS_TORI_Last(out int iCnt)
        {
            iCnt = 0;
            try
            {
                #region read last data
                {
                    var sb = new StringBuilder();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    //{
                    //    sb.AppendLine("select * from SS_TORI tr left outer join TRNAM ZT ON tr.trcd = ZT.trcd  where ROWNUM <= 2 AND length(tr.TRCD) < 13");
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        sb.AppendLine("select * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd  where length(tr.TRCD) < 13");
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        //sb.AppendLine("select top 2 * from SS_TORI tr  left outer join TRNAM ZT ON tr.trcd = ZT.trcd  where len(tr.TRCD) < 13");
                        sb.AppendFormat("select top 2 * from SS_TORI tr  left outer join ICSP_312Z{0}..TRNAM ZT ON tr.trcd = ZT.trcd  where len(tr.TRCD) < 13", Global.sCcod);
                    }
                    sb.AppendLine("order by tr.TRCD desc, tr.HJCD desc");
                    if (ComUtil.IsPostgreSQL()) sb.AppendLine(" limit 2 offset 0");
                    ExecuteQuery(sb.ToString());
                    if (reader.HasRows)
                    {
                        iCnt = 1;
                        Set_SS_TRCD();
                        while (reader.Read())
                        {
                            iCnt++;
                        }
                        if (Global.E_TANTOCD != "")
                        {
                            Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
                        }
                        if (Global.SAIMU == "1" && Global.GRPID != "0")
                        {
                            Global.GRPIDNM = Get_GrpNm(Global.GRPID);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TORI_Last　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// DataReaderの内容をGlobalに設定
        /// </summary>
        private void Set_SS_TRCD()
        {
            Init_DispVal();

            // <マルチDB対応>Readが必須なので追加
            reader.Read();

            Global.TRCD = DbCls.GetStrNullKara(reader["TRCD"].ToString());
            Global.HJCD = DbCls.GetStrNullKara(reader["HJCD"].ToString()).PadLeft(6, '0');
            Global.TRKBN = DbCls.GetStrNullKara(reader["TRKBN"].ToString());
            Global.RYAKU = DbCls.GetStrNullKara(reader["RYAKU"].ToString());
            Global.TORI_NAM = DbCls.GetStrNullKara(reader["TORI_NAM"].ToString());
            Global.KNLD = DbCls.GetStrNullKara(reader["KNLD"].ToString());
            Global.TGASW = DbCls.GetStrNullKara(reader["TGASW"].ToString());
            Global.ZIP = DbCls.GetStrNullKara(reader["ZIP"].ToString());
            Global.ADDR1 = DbCls.GetStrNullKara(reader["ADDR1"].ToString());
            Global.ADDR2 = DbCls.GetStrNullKara(reader["ADDR2"].ToString());
            Global.TEL = DbCls.GetStrNullKara(reader["TEL"].ToString());
            Global.FAX = DbCls.GetStrNullKara(reader["FAX"].ToString());
            Global.SBUSYO = DbCls.GetStrNullKara(reader["SBUSYO"].ToString());
            Global.STANTO = DbCls.GetStrNullKara(reader["STANTO"].ToString());
            Global.KEICD = DbCls.GetStrNullKara(reader["KEICD"].ToString());
            Global.STAN = DbCls.GetStrNullKara(reader["STAN"].ToString());
            Global.SJBCD = DbCls.GetStrNullKara(reader["SJBCD"].ToString());
            Global.SBCOD = DbCls.GetStrNullKara(reader["SBCOD"].ToString());
            Global.SKICD = DbCls.GetStrNullKara(reader["SKICD"].ToString());
            Global.NAYOSE = DbCls.GetStrNullKara(reader["NAYOSE"].ToString());
            Global.F_SETUIN = DbCls.GetStrNullKara(reader["F_SETUIN"].ToString());
            Global.F_SHITU = DbCls.GetStrNullKara(reader["F_SHITU"].ToString());
            Global.F_ZAN = DbCls.GetStrNullKara(reader["F_ZAN"].ToString());
            Global.F_SOUFU = DbCls.GetStrNullKara(reader["F_SOUFU"].ToString());
            Global.ANNAI = DbCls.GetStrNullKara(reader["ANNAI"].ToString());
            Global.TSOKBN = DbCls.GetStrNullKara(reader["TSOKBN"].ToString());
            Global.HORYU = DbCls.GetStrNullKara(reader["HORYU"].ToString());
//--<2016/03/14 小数点第３位まで>
//            Global.HOVAL = Convert.ToDecimal(DbCls.GetStrNullKara(reader["HOVAL"].ToString())).ToString("#0.0");
            Global.HOVAL = Convert.ToDecimal(DbCls.GetStrNullKara(reader["HOVAL"].ToString())).ToString("#0.##0");
//-- <2016/03/14>
            //Global.HOKKBN = DbCls.GetStrNullKara(reader["HOKKBN"].ToString());
            //Global.HODM1 = DbCls.GetStrNullKara(reader["HODM1"].ToString());
            //Global.KAIIN = DbCls.GetStrNullKara(reader["KAIIN"].ToString());
            //Global.KYKAI = DbCls.GetStrNullKara(reader["KYKAI"].ToString());
            //Global.KYVAL = DbCls.GetStrNullKara(reader["KYVAL"].ToString());
            //Global.KYCAL = DbCls.GetStrNullKara(reader["KYCAL"].ToString());
            //Global.KYZAF = DbCls.GetStrNullKara(reader["KYZAF"].ToString());
            //Global.KYZVL = DbCls.GetStrNullKara(reader["KYZVL"].ToString());
            //Global.KYZRT = DbCls.GetStrNullKara(reader["KYZRT"].ToString());
            //Global.KYZAH = DbCls.GetStrNullKara(reader["KYZAH"].ToString());
            //Global.KYZAS = DbCls.GetStrNullKara(reader["KYZAS"].ToString());
            //Global.KYROF = DbCls.GetStrNullKara(reader["KYROF"].ToString());
            //Global.KYRVL = DbCls.GetStrNullKara(reader["KYRVL"].ToString());
            //Global.KYRRT = DbCls.GetStrNullKara(reader["KYRRT"].ToString());
            //Global.KYROH = DbCls.GetStrNullKara(reader["KYROH"].ToString());
            //Global.KYROS = DbCls.GetStrNullKara(reader["KYROS"].ToString());
            //Global.KYGAF = DbCls.GetStrNullKara(reader["KYGAF"].ToString());
            //Global.KYGVL = DbCls.GetStrNullKara(reader["KYGVL"].ToString());
            //Global.KYGRT = DbCls.GetStrNullKara(reader["KYGRT"].ToString());
            //Global.KYGAH = DbCls.GetStrNullKara(reader["KYGAH"].ToString());
            //Global.KYGAS = DbCls.GetStrNullKara(reader["KYGAS"].ToString());
            //Global.KYKEF = DbCls.GetStrNullKara(reader["KYKEF"].ToString());
            //Global.KYKVL = DbCls.GetStrNullKara(reader["KYKVL"].ToString());
            //Global.KYKRT = DbCls.GetStrNullKara(reader["KYKRT"].ToString());
            //Global.KYKEH = DbCls.GetStrNullKara(reader["KYKEH"].ToString());
            //Global.KYKES = DbCls.GetStrNullKara(reader["KYKES"].ToString());
            Global.GENSEN = DbCls.GetStrNullKara(reader["GENSEN"].ToString());
            Global.GOU = DbCls.GetStrNullKara(reader["GOU"].ToString());
            Global.GGKBN = DbCls.GetStrNullKara(reader["GGKBN"].ToString());
            Global.GGKBNM = DbCls.GetStrNullKara(reader["GGKBNM"].ToString());
            Global.GSKUBN = DbCls.GetStrNullKara(reader["GSKUBN"].ToString());
            //Global.GSSKBN = DbCls.GetStrNullKara(reader["GSSKBN"].ToString());
            //Global.SZEI = DbCls.GetStrNullKara(reader["SZEI"].ToString());
            Global.SOSAI = DbCls.GetStrNullKara(reader["SOSAI"].ToString());
            Global.SOKICD = DbCls.GetStrNullKara(reader["SOKICD"].ToString());
            Global.GAIKA = DbCls.GetStrNullKara(reader["GAIKA"].ToString());
            Global.HEI_CD = DbCls.GetStrNullKara(reader["HEI_CD"].ToString());
            Global.DM1 = DbCls.GetStrNullKara(reader["DM1"].ToString());
            Global.DM2 = DbCls.GetStrNullKara(reader["DM2"].ToString());
            Global.DM3 = DbCls.GetStrNullKara(reader["DM3"].ToString());
            Global.STYMD = DbCls.GetStrNullKara(reader["STYMD"].ToString());
            Global.EDYMD = DbCls.GetStrNullKara(reader["EDYMD"].ToString());
            Global.ZSTYMD = DbCls.GetStrNullKara(reader["ISTAYMD"].ToString());
            Global.ZEDYMD = DbCls.GetStrNullKara(reader["IENDYMD"].ToString());
            Global.STFLG = DbCls.GetStrNullKara(reader["STFLG"].ToString());
            Global.CDM1 = DbCls.GetStrNullKara(reader["CDM1"].ToString());
            Global.LUSR = DbCls.GetStrNullKara(reader["LUSR"].ToString());
            Global.LMOD = DbCls.GetStrNullKara(reader["LMOD"].ToString());

            //Global.KYZSKBN = DbCls.GetStrNullKara(reader["KYZSKBN"].ToString());
            //Global.KYRSKBN = DbCls.GetStrNullKara(reader["KYRSKBN"].ToString());
            //Global.KYGSKBN = DbCls.GetStrNullKara(reader["KYGSKBN"].ToString());
            //Global.KYKSKBN = DbCls.GetStrNullKara(reader["KYKSKBN"].ToString());

            //**>>
            Global.CDM2 = DbCls.GetStrNullKara(reader["CDM2"].ToString());
            Global.CD03 = DbCls.GetStrNullKara(reader["CD03"].ToString());
            Global.IDM1 = DbCls.GetStrNullKara(reader["IDM1"].ToString());
            //**<<

            Global.SAIKEN = DbCls.GetStrNullKara(reader["SAIKEN"].ToString());
            Global.SAIKEN_FLG = DbCls.GetStrNullKara(reader["SAIKEN_FLG"].ToString());
            Global.SAIMU = DbCls.GetStrNullKara(reader["SAIMU"].ToString());
            Global.SAIMU_FLG = DbCls.GetStrNullKara(reader["SAIMU_FLG"].ToString());

            Global.TRFURI = DbCls.GetStrNullKara(reader["TRFURI"].ToString());
            Global.TRMAIL = DbCls.GetStrNullKara(reader["TRMAIL"].ToString());
            Global.TRURL = DbCls.GetStrNullKara(reader["TRURL"].ToString());
            Global.BIKO = DbCls.GetStrNullKara(reader["BIKO"].ToString());
            Global.E_TANTOCD = DbCls.GetStrNullKara(reader["E_TANTOCD"].ToString());
            Global.MYNO_AITE = DbCls.GetStrNullKara(reader["MYNO_AITE"].ToString());
            Global.SRYOU_F = DbCls.GetStrNullKara(reader["SRYOU_F"].ToString());
            Global.GRPID = DbCls.GetStrNullKara(reader["GRPID"].ToString());

            Global.TEGVAL = DbCls.GetStrNullKara(reader["TEGVAL"].ToString());
            Global.GSSKBN = DbCls.GetStrNullKara(reader["GSSKBN"].ToString());

            Global.HR_KIJYUN = DbCls.GetStrNullKara(reader["HR_KIJYUN"].ToString());
            Global.HORYU_F = DbCls.GetStrNullKara(reader["HORYU_F"].ToString());
            Global.HRORYUGAKU = DbCls.GetStrNullKara(reader["HRORYUGAKU"].ToString());
            Global.HRKBN = DbCls.GetStrNullKara(reader["HRKBN"].ToString());

            Global.GAI_F = DbCls.GetStrNullKara(reader["GAI_F"].ToString());
            Global.GAI_SF = DbCls.GetStrNullKara(reader["GAI_SF"].ToString());
            Global.GAI_SH = DbCls.GetStrNullKara(reader["GAI_SH"].ToString());
            Global.GAI_KZID = DbCls.GetStrNullKara(reader["GAI_KZID"].ToString());
            Global.GAI_TF = DbCls.GetStrNullKara(reader["GAI_TF"].ToString());
            Global.ENG_NAME = DbCls.GetStrNullKara(reader["ENG_NAME"].ToString());
            Global.ENG_ADDR = DbCls.GetStrNullKara(reader["ENG_ADDR"].ToString());
            Global.ENG_KZNO = DbCls.GetStrNullKara(reader["ENG_KZNO"].ToString());
            Global.ENG_SWIF = DbCls.GetStrNullKara(reader["ENG_SWIF"].ToString());
            Global.ENG_BNKNAM = DbCls.GetStrNullKara(reader["ENG_BNKNAM"].ToString());
            Global.ENG_BRNNAM = DbCls.GetStrNullKara(reader["ENG_BRNNAM"].ToString());
            Global.ENG_BNKADDR = DbCls.GetStrNullKara(reader["ENG_BNKADDR"].ToString());

            Global.TOKUKANA = DbCls.GetStrNullKara(reader["TOKUKANA"].ToString());
            Global.FUTAN = DbCls.GetStrNullKara(reader["FUTAN"].ToString());
            Global.KAISYU = DbCls.GetStrNullKara(reader["KAISYU"].ToString());
            Global.YAKUJYO = DbCls.GetStrNullKara(reader["YAKUJO"].ToString());
            Global.SHIME = DbCls.GetStrNullKara(reader["SHIME"].ToString());
            Global.KAISYUHI = DbCls.GetStrNullKara(reader["KAISYUHI"].ToString());
            Global.KAISYUSIGHT = DbCls.GetStrNullKara(reader["KAISYUSIGHT"].ToString());
            Global.Y_KINGAKU = DbCls.GetStrNullKara(reader["Y_KINGAKU"].ToString());
            Global.HOLIDAY = DbCls.GetStrNullKara(reader["HOLIDAY"].ToString());
            Global.MIMAN = DbCls.GetStrNullKara(reader["MIMAN"].ToString());
            Global.IJOU_1 = DbCls.GetStrNullKara(reader["IJOU_1"].ToString());
            Global.BUNKATSU_1 = DbCls.GetStrNullKara(reader["BUNKATSU_1"].ToString());
            Global.HASU_1 = DbCls.GetStrNullKara(reader["HASU_1"].ToString());
            Global.SIGHT_1 = DbCls.GetStrNullKara(reader["SIGHT_1"].ToString());
            Global.IJOU_2 = DbCls.GetStrNullKara(reader["IJOU_2"].ToString());
            Global.BUNKATSU_2 = DbCls.GetStrNullKara(reader["BUNKATSU_2"].ToString());
            Global.HASU_2 = DbCls.GetStrNullKara(reader["HASU_2"].ToString());
            Global.SIGHT_2 = DbCls.GetStrNullKara(reader["SIGHT_2"].ToString());
            Global.IJOU_3 = DbCls.GetStrNullKara(reader["IJOU_3"].ToString());
            Global.BUNKATSU_3 = DbCls.GetStrNullKara(reader["BUNKATSU_3"].ToString());
            Global.HASU_3 = DbCls.GetStrNullKara(reader["HASU_3"].ToString());
            Global.SIGHT_3 = DbCls.GetStrNullKara(reader["SIGHT_3"].ToString());
            Global.SEN_GINKOCD = DbCls.GetStrNullKara(reader["SEN_GINKOCD"].ToString());
            Global.SEN_SITENCD = DbCls.GetStrNullKara(reader["SEN_SITENCD"].ToString());
            Global.YOKINSYU = DbCls.GetStrNullKara(reader["YOKINSYU"].ToString());
            Global.SEN_KOZANO = DbCls.GetStrNullKara(reader["SEN_KOZANO"].ToString());
            Global.KASO_SITENNM = DbCls.GetStrNullKara(reader["SEN_SHITENMEI"].ToString());
            // Ver.01.02.03 [SS_4666]対応 Toda -->
            //Global.JIDOU_GAKUSYU = DbCls.GetStrNullKara(reader["JIDOU_GAKUSYU"].ToString());
            Global.JIDOU_GAKUSYU = DbCls.GetNumNullZero<Int16>(reader["JIDOU_GAKUSYU"]).ToString();
            // Ver.01.02.03 <--
            Global.NYUKIN_YOTEI = DbCls.GetStrNullKara(reader["NYUKIN_YOTEI"].ToString());
            Global.TESURYO_GAKUSYU = DbCls.GetStrNullKara(reader["TESURYO_GAKUSYU"].ToString());
            // Ver.01.02.03 [SS_4666]対応 Toda -->
            //Global.TESURYO_GOSA = DbCls.GetStrNullKara(reader["TESURYO_GOSA"].ToString());
            Global.TESURYO_GOSA = DbCls.GetNumNullZero<Int16>(reader["TESURYO_GOSA"]).ToString();
            // Ver.01.02.03 <--
            Global.RYOSYUSYO = DbCls.GetStrNullKara(reader["RYOSYUSYO"].ToString());
            Global.SHIN_KAISYACD = DbCls.GetStrNullKara(reader["SHIN_KAISYACD"].ToString());
            Global.YOSIN = DbCls.GetStrNullKara(reader["YOSIN"].ToString());
            Global.YOSHINRANK = DbCls.GetStrNullKara(reader["YOSHINRANK"].ToString());
            Global.GAIKA = DbCls.GetStrNullKara(reader["GAIKA"].ToString());
            Global.TSUKA = DbCls.GetStrNullKara(reader["TSUKA"].ToString());
            Global.GAIKA_KEY_F = DbCls.GetStrNullKara(reader["GAIKA_KEY_F"].ToString());
            Global.GAIKA_KEY_B = DbCls.GetStrNullKara(reader["GAIKA_KEY_B"].ToString());
            Global.HIFURIKOZA_1 = DbCls.GetStrNullKara(reader["HIFURIKOZA_1"].ToString());
            Global.HIFURIKOZA_2 = DbCls.GetStrNullKara(reader["HIFURIKOZA_2"].ToString());
            Global.HIFURIKOZA_3 = DbCls.GetStrNullKara(reader["HIFURIKOZA_3"].ToString());

            //if (Global.E_TANTOCD != "")
            //{
            //    Global.E_TANTONM = Get_ETanNm(Global.E_TANTOCD);
            //}
            //if (Global.SAIMU == "1" && Global.GRPID != "0")
            //{
            //    Global.GRPIDNM = Get_GrpNm(Global.GRPID);
            //}
        }
        #endregion


        #region 取引先支払方法・自社支払方法の検索(通常検索/前行・次行検索)
        /// <summary>
        /// 取引先支払方法から支払方法の設定値を取得
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="sHJCD"></param>
        /// <param name="nID"></param>
        public void Get_TSHOH(string sTRCD, string sHJCD, int nID)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read data
                string query =
                #region query
                    // @"SELECT SI_KUBN1, SKBN1.SKBNM AS SKBNM1, TSH.SI_BANK1, BNK1.BKNAM AS BNK1, TSH.SI_SITEN1, 
                    //BRN1.BRNAM AS BRN1, TSH.SI_KOZA1, TSH.SI_KOZANO1, TSH.SI_IRAININ1, 
                    //SI_KUBN2, SKBN2.SKBNM AS SKBNM2, TSH.SI_BANK2, BNK2.BKNAM AS BNK2, TSH.SI_SITEN2, 
                    //BRN2.BRNAM AS BRN2, TSH.SI_KOZA2, TSH.SI_KOZANO2, TSH.SI_IRAININ2, 
                    //SI_KUBN3, SKBN3.SKBNM AS SKBNM3, TSH.SI_BANK3, BNK3.BKNAM AS BNK3, TSH.SI_SITEN3, 
                    //BRN3.BRNAM AS BRN3, TSH.SI_KOZA3, TSH.SI_KOZANO3, TSH.SI_IRAININ3, 
                    //SI_KUBN4, SKBN4.SKBNM AS SKBNM4, TSH.SI_BANK4, BNK4.BKNAM AS BNK4, TSH.SI_SITEN4, 
                    //BRN4.BRNAM AS BRN4, TSH.SI_KOZA4, TSH.SI_KOZANO4, TSH.SI_IRAININ4 
                    //FROM SS_TSHOH TSH 
                    //LEFT JOIN SS_SKUBN SKBN1 ON SKBN1.SKKBN = 11 AND TSH.SI_KUBN1 = SKBN1.SKBNCOD 
                    //LEFT JOIN BANK BNK1 ON TSH.SI_BANK1 = BNK1.BKCOD 
                    //LEFT JOIN BRANCH BRN1 ON TSH.SI_BANK1 = BRN1.BKCOD AND TSH.SI_SITEN1 = BRN1.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN2 ON SKBN2.SKKBN = 11 AND TSH.SI_KUBN2 = SKBN2.SKBNCOD 
                    //LEFT JOIN BANK BNK2 ON TSH.SI_BANK2 = BNK2.BKCOD 
                    //LEFT JOIN BRANCH BRN2 ON TSH.SI_BANK2 = BRN2.BKCOD AND TSH.SI_SITEN2 = BRN2.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN3 ON SKBN3.SKKBN = 11 AND TSH.SI_KUBN3 = SKBN3.SKBNCOD 
                    //LEFT JOIN BANK BNK3 ON TSH.SI_BANK3 = BNK3.BKCOD 
                    //LEFT JOIN BRANCH BRN3 ON TSH.SI_BANK3 = BRN3.BKCOD AND TSH.SI_SITEN3 = BRN3.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN4 ON SKBN4.SKKBN = 11 AND TSH.SI_KUBN4 = SKBN4.SKBNCOD 
                    //LEFT JOIN BANK BNK4 ON TSH.SI_BANK4 = BNK4.BKCOD 
                    //LEFT JOIN BRANCH BRN4 ON TSH.SI_BANK4 = BRN4.BKCOD AND TSH.SI_SITEN4 = BRN4.BRCOD 
                    //WHERE TSH.TRCD = :p AND TSH.HJCD = :p AND TSH.SHO_ID = :p ";
                    String.Format(
                    @"SELECT a.SI_KUBN1,
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_BANK1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                 LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BNK1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_SITEN1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                 LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BRN1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZA1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZANO1, 
						a.SI_KUBN2,
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_BANK2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BNK2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_SITEN2, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2	
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BRN2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZA2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZANO2, 
						a.SI_KUBN3,
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_BANK3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BNK3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_SITEN3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BRN3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZA3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZANO3, 
						a.SI_KUBN4,
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_BANK4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BNK4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_SITEN4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BRN4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZA4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZANO4
                     FROM SS_TSHOH a
                    WHERE a.TRCD = :p AND a.HJCD = :p AND a.SHO_ID = :p "
                    , ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + "..");
                #endregion

                ExecuteQuery(query,
                    new DBParameter("@TRCD", sTRCD),
                    new DBParameter("@HJCD", int.Parse(sHJCD)),//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    new DBParameter("@SHID", nID));
                #endregion

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.KUBN1_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
                    Global.KUBNNM1_tb3 = DbCls.GetStrNullKara(reader["SKBNM1"].ToString());
                    Global.BANK1_tb3 = DbCls.GetStrNullKara(reader["SI_BANK1"].ToString());
                    Global.BANKNM1_tb3 = DbCls.GetStrNullKara(reader["BNK1"].ToString());
                    Global.SITEN1_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN1"].ToString());
                    Global.SITENNM1_tb3 = DbCls.GetStrNullKara(reader["BRN1"].ToString());
                    Global.KOZA1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA1"].ToString());
                    Global.KOZANO1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO1"].ToString());
                    //Global.IRAININ1_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ1"].ToString());
                    Global.KUBN2_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
                    Global.KUBNNM2_tb3 = DbCls.GetStrNullKara(reader["SKBNM2"].ToString());
                    Global.BANK2_tb3 = DbCls.GetStrNullKara(reader["SI_BANK2"].ToString());
                    Global.BANKNM2_tb3 = DbCls.GetStrNullKara(reader["BNK2"].ToString());
                    Global.SITEN2_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN2"].ToString());
                    Global.SITENNM2_tb3 = DbCls.GetStrNullKara(reader["BRN2"].ToString());
                    Global.KOZA2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA2"].ToString());
                    Global.KOZANO2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO2"].ToString());
                    //Global.IRAININ2_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ2"].ToString());
                    Global.KUBN3_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
                    Global.KUBNNM3_tb3 = DbCls.GetStrNullKara(reader["SKBNM3"].ToString());
                    Global.BANK3_tb3 = DbCls.GetStrNullKara(reader["SI_BANK3"].ToString());
                    Global.BANKNM3_tb3 = DbCls.GetStrNullKara(reader["BNK3"].ToString());
                    Global.SITEN3_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN3"].ToString());
                    Global.SITENNM3_tb3 = DbCls.GetStrNullKara(reader["BRN3"].ToString());
                    Global.KOZA3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA3"].ToString());
                    Global.KOZANO3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO3"].ToString());
                    //Global.IRAININ3_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ3"].ToString());
                    Global.KUBN4_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
                    Global.KUBNNM4_tb3 = DbCls.GetStrNullKara(reader["SKBNM4"].ToString());
                    Global.BANK4_tb3 = DbCls.GetStrNullKara(reader["SI_BANK4"].ToString());
                    Global.BANKNM4_tb3 = DbCls.GetStrNullKara(reader["BNK4"].ToString());
                    Global.SITEN4_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN4"].ToString());
                    Global.SITENNM4_tb3 = DbCls.GetStrNullKara(reader["BRN4"].ToString());
                    Global.KOZA4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA4"].ToString());
                    Global.KOZANO4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO4"].ToString());
                    //Global.IRAININ4_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ4"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 自社支払方法から支払方法の設定値を取得
        /// </summary>
        /// <param name="sSHINO"></param>
        public void Get_SHOHO(string sSHINO)
        {
            try
            {
                #region read data
                string query =
                #region query
                    // @"SELECT SI_KUBN1, SKBN1.SKBNM AS SKBNM1, SHO.SI_BANK1, BNK1.BKNAM AS BNK1, SHO.SI_SITEN1, 
                    //BRN1.BRNAM AS BRN1, SHO.SI_KOZA1, SHO.SI_KOZANO1, SHO.SI_IRAININ1, 
                    //SI_KUBN2, SKBN2.SKBNM AS SKBNM2, SHO.SI_BANK2, BNK2.BKNAM AS BNK2, 
                    //SHO.SI_SITEN2, BRN2.BRNAM AS BRN2, SHO.SI_KOZA2, SHO.SI_KOZANO2, SHO.SI_IRAININ2, 
                    //SI_KUBN3, SKBN3.SKBNM AS SKBNM3, SHO.SI_BANK3, BNK3.BKNAM AS BNK3, 
                    //SHO.SI_SITEN3, BRN3.BRNAM AS BRN3, SHO.SI_KOZA3, SHO.SI_KOZANO3, SHO.SI_IRAININ3, 
                    //SI_KUBN4, SKBN4.SKBNM AS SKBNM4, SHO.SI_BANK4, BNK4.BKNAM AS BNK4, 
                    //SHO.SI_SITEN4, BRN4.BRNAM AS BRN4, SHO.SI_KOZA4, SHO.SI_KOZANO4, SHO.SI_IRAININ4 
                    //FROM SS_SHOHO SHO  
                    //LEFT JOIN SS_SKUBN SKBN1 ON SKBN1.SKKBN = 11 AND SHO.SI_KUBN1 = SKBN1.SKBNCOD 
                    //LEFT JOIN BANK BNK1 ON SHO.SI_BANK1 = BNK1.BKCOD 
                    //LEFT JOIN BRANCH BRN1 ON SHO.SI_BANK1 = BRN1.BKCOD AND SHO.SI_SITEN1 = BRN1.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN2 ON SKBN2.SKKBN = 11 AND SHO.SI_KUBN2 = SKBN2.SKBNCOD 
                    //LEFT JOIN BANK BNK2 ON SHO.SI_BANK2 = BNK2.BKCOD 
                    //LEFT JOIN BRANCH BRN2 ON SHO.SI_BANK2 = BRN2.BKCOD AND SHO.SI_SITEN2 = BRN2.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN3 ON SKBN3.SKKBN = 11 AND SHO.SI_KUBN3 = SKBN3.SKBNCOD 
                    //LEFT JOIN BANK BNK3 ON SHO.SI_BANK3 = BNK3.BKCOD 
                    //LEFT JOIN BRANCH BRN3 ON SHO.SI_BANK3 = BRN3.BKCOD AND SHO.SI_SITEN3 = BRN3.BRCOD 
                    //LEFT JOIN SS_SKUBN SKBN4 ON SKBN4.SKKBN = 11 AND SHO.SI_KUBN4 = SKBN4.SKBNCOD 
                    //LEFT JOIN BANK BNK4 ON SHO.SI_BANK4 = BNK4.BKCOD 
                    //LEFT JOIN BRANCH BRN4 ON SHO.SI_BANK4 = BRN4.BKCOD AND SHO.SI_SITEN4 = BRN4.BRCOD 
                    //WHERE SHINO = :p ";
String.Format(
@"SELECT
	 A.SI_KOUZAID1, SI_KUBN1, SKBN1.SKBNM AS SKBNM1, OBK1.OWNBKCOD AS SI_BANK1, BK1.BKNAM AS BNK1, OBK1.OWNBRCOD AS SI_SITEN1, BR1.BRNAM AS BRN1, OBK1.YOKNKIND AS SI_KOZA1, OBK1.KOZANO AS SI_KOZANO1
	,A.SI_KOUZAID2, SI_KUBN2, SKBN2.SKBNM AS SKBNM2, OBK2.OWNBKCOD AS SI_BANK2, BK2.BKNAM AS BNK2, OBK2.OWNBRCOD AS SI_SITEN2, BR2.BRNAM AS BRN2, OBK2.YOKNKIND AS SI_KOZA2, OBK2.KOZANO AS SI_KOZANO2
	,A.SI_KOUZAID3, SI_KUBN3, SKBN3.SKBNM AS SKBNM3, OBK3.OWNBKCOD AS SI_BANK3, BK3.BKNAM AS BNK3, OBK3.OWNBRCOD AS SI_SITEN3, BR3.BRNAM AS BRN3, OBK3.YOKNKIND AS SI_KOZA3, OBK3.KOZANO AS SI_KOZANO3
	,A.SI_KOUZAID4, SI_KUBN4, SKBN4.SKBNM AS SKBNM4, OBK4.OWNBKCOD AS SI_BANK4, BK4.BKNAM AS BNK4, OBK4.OWNBRCOD AS SI_SITEN4, BR4.BRNAM AS BRN4, OBK4.YOKNKIND AS SI_KOZA4, OBK4.KOZANO AS SI_KOZANO4
FROM 
	SS_SHOHO A 
	LEFT JOIN SS_SKUBN SKBN1 ON SKBN1.SKKBN = 11 AND A.SI_KUBN1 = SKBN1.SKBNCOD
	LEFT JOIN SS_SKUBN SKBN2 ON SKBN2.SKKBN = 11 AND A.SI_KUBN2 = SKBN2.SKBNCOD
	LEFT JOIN SS_SKUBN SKBN3 ON SKBN3.SKKBN = 11 AND A.SI_KUBN3 = SKBN3.SKBNCOD
	LEFT JOIN SS_SKUBN SKBN4 ON SKBN4.SKKBN = 11 AND A.SI_KUBN4 = SKBN4.SKBNCOD
	LEFT JOIN SS_OWNBK OBK1 ON OBK1.OWNID = A.SI_KOUZAID1
	LEFT JOIN SS_OWNBK OBK2 ON OBK2.OWNID = A.SI_KOUZAID2
	LEFT JOIN SS_OWNBK OBK3 ON OBK3.OWNID = A.SI_KOUZAID3
	LEFT JOIN SS_OWNBK OBK4 ON OBK4.OWNID = A.SI_KOUZAID4
	LEFT JOIN {0}BANK BK1 ON BK1.BKCOD = OBK1.OWNBKCOD
	LEFT JOIN {0}BANK BK2 ON BK2.BKCOD = OBK2.OWNBKCOD
	LEFT JOIN {0}BANK BK3 ON BK3.BKCOD = OBK3.OWNBKCOD
	LEFT JOIN {0}BANK BK4 ON BK4.BKCOD = OBK4.OWNBKCOD
	LEFT JOIN {0}BRANCH BR1 ON BR1.BKCOD = OBK1.OWNBKCOD AND BR1.BRCOD = OBK1.OWNBRCOD
	LEFT JOIN {0}BRANCH BR2 ON BR2.BKCOD = OBK2.OWNBKCOD AND BR2.BRCOD = OBK2.OWNBRCOD
	LEFT JOIN {0}BRANCH BR3 ON BR3.BKCOD = OBK3.OWNBKCOD AND BR3.BRCOD = OBK3.OWNBRCOD
	LEFT JOIN {0}BRANCH BR4 ON BR4.BKCOD = OBK4.OWNBKCOD AND BR4.BRCOD = OBK4.OWNBRCOD
WHERE 
	A.SHINO = :p"
, ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + "..");
                #endregion
                //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                ExecuteQuery(query, new DBParameter("@SHINO", int.Parse(sSHINO)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                #endregion

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.OWNID1 = DbCls.GetStrNullKara(reader["SI_KOUZAID1"].ToString());
                    Global.KUBN1_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
                    Global.KUBNNM1_tb3 = DbCls.GetStrNullKara(reader["SKBNM1"].ToString());
                    Global.BANK1_tb3 = DbCls.GetStrNullKara(reader["SI_BANK1"].ToString());
                    Global.BANKNM1_tb3 = DbCls.GetStrNullKara(reader["BNK1"].ToString());
                    Global.SITEN1_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN1"].ToString());
                    Global.SITENNM1_tb3 = DbCls.GetStrNullKara(reader["BRN1"].ToString());
                    Global.KOZA1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA1"].ToString());
                    Global.KOZANO1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO1"].ToString());
                    //Global.IRAININ1_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ1"].ToString());

                    Global.OWNID2 = DbCls.GetStrNullKara(reader["SI_KOUZAID2"].ToString());
                    Global.KUBN2_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
                    Global.KUBNNM2_tb3 = DbCls.GetStrNullKara(reader["SKBNM2"].ToString());
                    Global.BANK2_tb3 = DbCls.GetStrNullKara(reader["SI_BANK2"].ToString());
                    Global.BANKNM2_tb3 = DbCls.GetStrNullKara(reader["BNK2"].ToString());
                    Global.SITEN2_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN2"].ToString());
                    Global.SITENNM2_tb3 = DbCls.GetStrNullKara(reader["BRN2"].ToString());
                    Global.KOZA2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA2"].ToString());
                    Global.KOZANO2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO2"].ToString());
                    //Global.IRAININ2_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ2"].ToString());

                    Global.OWNID3 = DbCls.GetStrNullKara(reader["SI_KOUZAID3"].ToString());
                    Global.KUBN3_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
                    Global.KUBNNM3_tb3 = DbCls.GetStrNullKara(reader["SKBNM3"].ToString());
                    Global.BANK3_tb3 = DbCls.GetStrNullKara(reader["SI_BANK3"].ToString());
                    Global.BANKNM3_tb3 = DbCls.GetStrNullKara(reader["BNK3"].ToString());
                    Global.SITEN3_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN3"].ToString());
                    Global.SITENNM3_tb3 = DbCls.GetStrNullKara(reader["BRN3"].ToString());
                    Global.KOZA3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA3"].ToString());
                    Global.KOZANO3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO3"].ToString());
                    //Global.IRAININ3_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ3"].ToString());

                    Global.OWNID4 = DbCls.GetStrNullKara(reader["SI_KOUZAID4"].ToString());
                    Global.KUBN4_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
                    Global.KUBNNM4_tb3 = DbCls.GetStrNullKara(reader["SKBNM4"].ToString());
                    Global.BANK4_tb3 = DbCls.GetStrNullKara(reader["SI_BANK4"].ToString());
                    Global.BANKNM4_tb3 = DbCls.GetStrNullKara(reader["BNK4"].ToString());
                    Global.SITEN4_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN4"].ToString());
                    Global.SITENNM4_tb3 = DbCls.GetStrNullKara(reader["BRN4"].ToString());
                    Global.KOZA4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA4"].ToString());
                    Global.KOZANO4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO4"].ToString());
                    //Global.IRAININ4_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ4"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SHOHO　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public void Get_Facid_To_Ownbk(int nNum, string sFacID)
        {
            try
            {
                string query = String.Format(@"SELECT
	                                 OB.OWNBKCOD, OB.OWNBRCOD, OB.YOKNKIND, OB.KOZANO, BK.BKNAM, BR.BRNAM
                                 FROM
	                                 SS_FACTER FA
	                                 INNER JOIN SS_OWNBK OB ON OB.OWNID = FA.OWNID
	                                 LEFT JOIN {0}BANK BK ON BK.BKCOD = OB.OWNBKCOD
	                                 LEFT JOIN {0}BRANCH BR ON BR.BKCOD = OB.OWNBKCOD AND BR.BRCOD = OB.OWNBRCOD
                                 WHERE
	                                 FA.FACID = :p"
                    , ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + "..");

                ExecuteQuery(query, new DBParameter("@FACID", int.Parse(sFacID)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加

                if (reader.HasRows == true)
                {
                    reader.Read();

                    if (nNum == 1)
                    {
                        Global.BANK1_tb3 = DbCls.GetStrNullKara(reader["OWNBKCOD"].ToString());
                        Global.BANKNM1_tb3 = DbCls.GetStrNullKara(reader["BKNAM"].ToString());
                        Global.SITEN1_tb3 = DbCls.GetStrNullKara(reader["OWNBRCOD"].ToString());
                        Global.SITENNM1_tb3 = DbCls.GetStrNullKara(reader["BRNAM"].ToString());
                        Global.KOZA1_tb3 = DbCls.GetStrNullKara(reader["YOKNKIND"].ToString());
                        Global.KOZANO1_tb3 = DbCls.GetStrNullKara(reader["KOZANO"].ToString());
                    }
                    else if (nNum == 2)
                    {
                        Global.BANK2_tb3 = DbCls.GetStrNullKara(reader["OWNBKCOD"].ToString());
                        Global.BANKNM2_tb3 = DbCls.GetStrNullKara(reader["BKNAM"].ToString());
                        Global.SITEN2_tb3 = DbCls.GetStrNullKara(reader["OWNBRCOD"].ToString());
                        Global.SITENNM2_tb3 = DbCls.GetStrNullKara(reader["BRNAM"].ToString());
                        Global.KOZA2_tb3 = DbCls.GetStrNullKara(reader["YOKNKIND"].ToString());
                        Global.KOZANO2_tb3 = DbCls.GetStrNullKara(reader["KOZANO"].ToString());
                    }
                    else if (nNum == 3)
                    {
                        Global.BANK3_tb3 = DbCls.GetStrNullKara(reader["OWNBKCOD"].ToString());
                        Global.BANKNM3_tb3 = DbCls.GetStrNullKara(reader["BKNAM"].ToString());
                        Global.SITEN3_tb3 = DbCls.GetStrNullKara(reader["OWNBRCOD"].ToString());
                        Global.SITENNM3_tb3 = DbCls.GetStrNullKara(reader["BRNAM"].ToString());
                        Global.KOZA3_tb3 = DbCls.GetStrNullKara(reader["YOKNKIND"].ToString());
                        Global.KOZANO3_tb3 = DbCls.GetStrNullKara(reader["KOZANO"].ToString());
                    }
                    else if (nNum == 4)
                    {
                        Global.BANK4_tb3 = DbCls.GetStrNullKara(reader["OWNBKCOD"].ToString());
                        Global.BANKNM4_tb3 = DbCls.GetStrNullKara(reader["BKNAM"].ToString());
                        Global.SITEN4_tb3 = DbCls.GetStrNullKara(reader["OWNBRCOD"].ToString());
                        Global.SITENNM4_tb3 = DbCls.GetStrNullKara(reader["BRNAM"].ToString());
                        Global.KOZA4_tb3 = DbCls.GetStrNullKara(reader["YOKNKIND"].ToString());
                        Global.KOZANO4_tb3 = DbCls.GetStrNullKara(reader["KOZANO"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Facid_To_Ownbk　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 支払条件タブに設定する値の検索
        /// 対象テーブル:取引先支払方法
        /// </summary>
        /// <param name="sTRCD">現在表示中の取引先コード</param>
        /// <param name="iSS_TSHOH_cnt">sTRCDに紐付く取引先支払方法のデータ件数</param>
        public void Sel_SS_TSHOH(string sTRCD, string sHJCD, out int iSS_TSHOH_cnt)
        {
            iSS_TSHOH_cnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read data
                string query;
                //---> V02.01.01 HWPO DELETE ▼【PostgreSQL対応】
                //if (DbCls.DbType == DbCls.eDbType.Oracle)                
//                {
//                    query =
//                    @"SELECT a.*, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                    FROM SS_TSHOH a 
//                    WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p ORDER BY SHO_ID ";
//                }
//                else
                //<--- V02.01.01 HWPO DELETE ▲【PostgreSQL対応】
                {
//                    query =
//                    @"SELECT top 1 a.*, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                    (SELECT SKBNM FROM SS_SKUBN WHERE a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                    (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                    (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                    FROM SS_TSHOH a 
//                    WHERE TRCD = :p AND HJCD = :p ORDER BY SHO_ID ";

                    query =
                    String.Format(
                    @"SELECT {1} a.*, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_BANK1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                 LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BKNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_SITEN1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                 LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BRNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZA1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZANO1, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_BANK2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BKNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_SITEN2, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2	
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BRNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZA2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZANO2, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_BANK3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BKNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_SITEN3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BRNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZA3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZANO3, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_BANK4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BKNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_SITEN4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BRNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZA4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZANO4, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_BANK5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BKNAM5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_SITEN5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BRNAM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZA5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZANO5 
                     FROM SS_TSHOH a 
                    WHERE TRCD = :p AND HJCD = :p ORDER BY SHO_ID {2}"
                    , ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + ".."
                    , ComUtil.IsPostgreSQL() ? "" : "top 1"
                    , ComUtil.IsPostgreSQL() ? "LIMIT 1 OFFSET 0" : "");
                
                }
                ExecuteQuery(query
                    , new DBParameter("@TRCD", sTRCD)
                    , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                if (reader.HasRows)
                {
                    set_SS_TSHOH(); //検索結果の格納
                    //while (reader.Read())
                    //{
                    //    iSS_TSHOH_cnt += 1;
                    //}
                    iSS_TSHOH_cnt = Get_SS_TSHOH_Count(sTRCD, sHJCD);
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        internal int Get_SS_TSHOH_Count(string trcd, string hjcd)
        {
            int count = 0;
            string query =
            #region query
 @"
select count(*) cnt
from SS_TSHOH
where
    TRCD = :p
and HJCD = :p
";
            #endregion
            trcd = GetTrcdDB(trcd);
            try
            {
                ExecuteQuery(query
                    , new DBParameter("@trcd", trcd)
                    , new DBParameter("@hjcd", int.Parse(hjcd)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                if (reader.HasRows)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    count = DbCls.GetNumNullZero<int>(reader[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return count;
        }

        /// <summary>
        /// 支払条件タブに設定する値の検索
        /// 対象テーブル:取引先支払方法
        /// </summary>
        /// <param name="sTRCD">現在表示中の取引先コード</param>
        /// <param name="iSS_TSHOH_cnt">sTRCDに紐付く取引先支払方法のデータ件数</param>
        public void Sel_SS_TSHOH_Prev(string sTRCD, string sHJCD, string sSHO_ID, out int iCnt)
        {
            iCnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                {
                    string query =
                    #region query
 @"SELECT count(*) cnt
FROM SS_TSHOH a 
WHERE TRCD = :p AND HJCD = :p ";
                    #endregion
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        iCnt = (DbCls.GetNumNullZero<int>(reader["cnt"]));
                    }
                }
                #endregion
                #region read data
                {
                    string query;
                    //---> V02.01.01 HWPO DELETE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)                    
//                    {
//                        query =
//                        @"SELECT a.*, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                        FROM SS_TSHOH a 
//                        WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p AND SHO_ID < :p ORDER BY SHO_ID DESC 
//                        ";
//                    }
//                    else
                    //<--- V02.01.01 HWPO DELETE ▲【PostgreSQL対応】
                    {
//                        query =
//                        @"SELECT top 1 a.*, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                        FROM SS_TSHOH a 
//                        WHERE TRCD = :p AND HJCD = :p AND SHO_ID < :p ORDER BY SHO_ID DESC 
//                        ";
                        query =
                        String.Format(
                        @"SELECT {1} a.*, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_BANK1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                 LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BKNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_SITEN1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                 LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BRNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZA1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZANO1, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_BANK2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BKNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_SITEN2, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2	
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BRNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZA2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZANO2, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_BANK3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BKNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_SITEN3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BRNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZA3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZANO3, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_BANK4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BKNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_SITEN4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BRNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZA4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZANO4, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_BANK5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BKNAM5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_SITEN5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BRNAM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZA5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZANO5 
                     FROM SS_TSHOH a 
                    WHERE TRCD = :p AND HJCD = :p AND SHO_ID < :p ORDER BY SHO_ID DESC {2}"
                    , ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + ".."
                    , ComUtil.IsPostgreSQL() ? "" : "top 1"
                    , ComUtil.IsPostgreSQL() ? "LIMIT 1 OFFSET 0" : "");

                    }
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                        //, new DBParameter("@HJCD", sHJCD)
                        //, new DBParameter("@SHO_ID", sSHO_ID));
                        , new DBParameter("@HJCD", int.Parse(sHJCD))
                        , new DBParameter("@SHO_ID", int.Parse(sSHO_ID)));
                        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        set_SS_TSHOH();
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TSHOH_Prev　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 支払条件タブに設定する値の検索
        /// 対象テーブル:取引先支払方法
        /// </summary>
        /// <param name="sTRCD">現在表示中の取引先コード</param>
        /// <param name="iSS_TSHOH_cnt">sTRCDに紐付く取引先支払方法のデータ件数</param>
        public void Sel_SS_TSHOH_Next(string sTRCD, string sHJCD, string sSHO_ID, out int iCnt)
        {
            iCnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                {
                    string query =
                    #region query
 @"SELECT count(*) cnt
FROM SS_TSHOH a 
WHERE TRCD = :p AND HJCD = :p ";
                    #endregion
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        iCnt = DbCls.GetNumNullZero<int>(reader["cnt"]);
                    }
                }
                #endregion

                #region read data
                {
                    string query;
                    //---> V02.01.01 HWPO DELETE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)  
//                    {
//                        query =
//                        @"SELECT a.*, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                        FROM SS_TSHOH a 
//                        WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p AND SHO_ID > :p ORDER BY SHO_ID ";
//                    }
//                    else
                    //<--- V02.01.01 HWPO DELETE ▲【PostgreSQL対応】
                    {
//                        query =
//                        @"SELECT top 1 a.*, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK1 = BKCOD) AS BKNAM1, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK1 = BKCOD AND a.SI_SITEN1 = BRCOD) AS BRNAM1, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK2 = BKCOD) AS BKNAM2, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK2 = BKCOD AND a.SI_SITEN2 = BRCOD) AS BRNAM2, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK3 = BKCOD) AS BKNAM3, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK3 = BKCOD AND a.SI_SITEN3 = BRCOD) AS BRNAM3, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK4 = BKCOD) AS BKNAM4, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK4 = BKCOD AND a.SI_SITEN4 = BRCOD) AS BRNAM4, 
//                        (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5, 
//                        (SELECT BKNAM FROM BANK WHERE a.SI_BANK5 = BKCOD) AS BKNAM5, 
//                        (SELECT BRNAM FROM BRANCH WHERE a.SI_BANK5 = BKCOD AND a.SI_SITEN5 = BRCOD) AS BRNAM5 
//                        FROM SS_TSHOH a 
//                        WHERE TRCD = :p AND HJCD = :p AND SHO_ID > :p ORDER BY SHO_ID ";
                        query =
                        String.Format(
                        @"SELECT {1} a.*, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN1 = SKBNCOD) AS SKBNM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_BANK1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                 LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BKNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_SITEN1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                 LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                 LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS BRNAM1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZA1, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID1 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID1
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN1) AS SI_KOZANO1, 
                     (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN2 = SKBNCOD) AS SKBNM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_BANK2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BKNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_SITEN2, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2	
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS BRNAM2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZA2, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID2 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID2
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN2) AS SI_KOZANO2, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN3 = SKBNCOD) AS SKBNM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_BANK3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BKNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_SITEN3, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS BRNAM3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZA3, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID3 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID3
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN3) AS SI_KOZANO3, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN4 = SKBNCOD) AS SKBNM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_BANK4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BKNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_SITEN4, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS BRNAM4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZA4, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID4 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID4
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN4) AS SI_KOZANO4, 
                    (SELECT SKBNM FROM SS_SKUBN WHERE SKKBN = 11 AND a.SI_KUBN5 = SKBNCOD) AS SKBNM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBKCOD ELSE OB.OWNBKCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_BANK5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BK_F.BKNAM ELSE BK_O.BKNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BANK BK_O ON BK_O.BKCOD = OB.OWNBKCOD
			                LEFT JOIN {0}BANK BK_F ON BK_F.BKCOD = OB_F.OWNBKCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BKNAM5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.OWNBRCOD ELSE OB.OWNBRCOD END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_SITEN5, 
                    (SELECT CASE WHEN S.SKBKIND = 8 THEN BR_F.BRNAM ELSE BR_O.BRNAM END
		               FROM SS_SKUBN S
		                    LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                    LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
			                LEFT JOIN {0}BRANCH BR_O ON BR_O.BKCOD = OB.OWNBKCOD AND BR_O.BRCOD = OB.OWNBRCOD
			                LEFT JOIN {0}BRANCH BR_F ON BR_F.BKCOD = OB_F.OWNBKCOD AND BR_F.BRCOD = OB_F.OWNBRCOD
	                  WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS BRNAM5,
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.YOKNKIND ELSE OB.YOKNKIND END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZA5, 
                     (SELECT CASE WHEN S.SKBKIND = 8 THEN OB_F.KOZANO ELSE OB.KOZANO END
		                FROM SS_SKUBN S
		                     LEFT JOIN SS_OWNBK OB ON OB.OWNID = a.OWNID5 
			                 LEFT JOIN SS_FACTER FC ON FC.FACID = a.OWNID5
		                     LEFT JOIN SS_OWNBK OB_F ON OB_F.OWNID = FC.OWNID
	                   WHERE S.SKKBN = 11 AND S.SKBNCOD = a.SI_KUBN5) AS SI_KOZANO5 
                     FROM SS_TSHOH a 
                    WHERE TRCD = :p AND HJCD = :p AND SHO_ID > :p ORDER BY SHO_ID {2}"
                    , ComUtil.IsPostgreSQL() ? "" : "ICSP_312Z" + Global.sCcod + ".."
                    , ComUtil.IsPostgreSQL() ? "" : "top 1"
                    , ComUtil.IsPostgreSQL() ? "LIMIT 1 OFFSET 0" : "");

                    }
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //ExecuteQuery(query
                    //    , new DBParameter("@TRCD", sTRCD)
                    //    , new DBParameter("@HJCD", sHJCD)
                    //    , new DBParameter("@SHO_ID", sSHO_ID));
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD))
                        , new DBParameter("@SHO_ID",int.Parse(sSHO_ID)));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        set_SS_TSHOH();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_TSHOH_Next　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// DataReaderの内容をGlobalに設定
        /// </summary>
        private void set_SS_TSHOH()
        {
            // <マルチDB対応>Readが必須なので追加
            reader.Read();

            //タブ1+共通分
            Global.SHO_ID_tb1 = DbCls.GetStrNullKara(reader["SHO_ID"].ToString());
            Global.BCOD_tb1 = DbCls.GetStrNullKara(reader["BCOD"].ToString());
            Global.KICD_tb1 = DbCls.GetStrNullKara(reader["KICD"].ToString());
            Global.SHINO_tb1 = DbCls.GetStrNullKara(reader["SHINO"].ToString());
            Global.HARAI_H_tb1 = DbCls.GetStrNullKara(reader["HARAI_H"].ToString());
            Global.KIJITU_H_tb1 = DbCls.GetStrNullKara(reader["KIJITU_H"].ToString());
            //タブ3固有分
            Global.SHO_ID_tb3 = DbCls.GetStrNullKara(reader["SHO_ID"].ToString());
            Global.BCOD_tb3 = DbCls.GetStrNullKara(reader["BCOD"].ToString());
            Global.KICD_tb3 = DbCls.GetStrNullKara(reader["KICD"].ToString());
            Global.SHINO_tb3 = DbCls.GetStrNullKara(reader["SHINO"].ToString());
            Global.KUBN1_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
            Global.KUBNNM1_tb3 = DbCls.GetStrNullKara(reader["SKBNM1"].ToString());
            Global.BANK1_tb3 = DbCls.GetStrNullKara(reader["SI_BANK1"].ToString());
            Global.BANKNM1_tb3 = DbCls.GetStrNullKara(reader["BKNAM1"].ToString());
            Global.SITEN1_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN1"].ToString());
            Global.SITENNM1_tb3 = DbCls.GetStrNullKara(reader["BRNAM1"].ToString());
            Global.KOZA1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA1"].ToString());
            Global.KOZANO1_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO1"].ToString());
            //Global.IRAININ1_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ1"].ToString());
            Global.KUBN2_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
            Global.KUBNNM2_tb3 = DbCls.GetStrNullKara(reader["SKBNM2"].ToString());
            Global.BANK2_tb3 = DbCls.GetStrNullKara(reader["SI_BANK2"].ToString());
            Global.BANKNM2_tb3 = DbCls.GetStrNullKara(reader["BKNAM2"].ToString());
            Global.SITEN2_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN2"].ToString());
            Global.SITENNM2_tb3 = DbCls.GetStrNullKara(reader["BRNAM2"].ToString());
            Global.KOZA2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA2"].ToString());
            Global.KOZANO2_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO2"].ToString());
            //Global.IRAININ2_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ2"].ToString());
            Global.KUBN3_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
            Global.KUBNNM3_tb3 = DbCls.GetStrNullKara(reader["SKBNM3"].ToString());
            Global.BANK3_tb3 = DbCls.GetStrNullKara(reader["SI_BANK3"].ToString());
            Global.BANKNM3_tb3 = DbCls.GetStrNullKara(reader["BKNAM3"].ToString());
            Global.SITEN3_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN3"].ToString());
            Global.SITENNM3_tb3 = DbCls.GetStrNullKara(reader["BRNAM3"].ToString());
            Global.KOZA3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA3"].ToString());
            Global.KOZANO3_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO3"].ToString());
            //Global.IRAININ3_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ3"].ToString());
            Global.KUBN4_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
            Global.KUBNNM4_tb3 = DbCls.GetStrNullKara(reader["SKBNM4"].ToString());
            Global.BANK4_tb3 = DbCls.GetStrNullKara(reader["SI_BANK4"].ToString());
            Global.BANKNM4_tb3 = DbCls.GetStrNullKara(reader["BKNAM4"].ToString());
            Global.SITEN4_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN4"].ToString());
            Global.SITENNM4_tb3 = DbCls.GetStrNullKara(reader["BRNAM4"].ToString());
            Global.KOZA4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA4"].ToString());
            Global.KOZANO4_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO4"].ToString());
            //Global.IRAININ4_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ4"].ToString());

            //Global.SI_KUBN5_tb3 = DbCls.GetStrNullKara(reader["SI_KUBN5"].ToString());
            //Global.SI_KUBNNM5_tb3 = DbCls.GetStrNullKara(reader["SKBNM5"].ToString());
            //Global.SI_BANK5_tb3 = DbCls.GetStrNullKara(reader["SI_BANK5"].ToString());
            //Global.SI_BANKNM5_tb3 = DbCls.GetStrNullKara(reader["BKNAM5"].ToString());
            //Global.SI_SITEN5_tb3 = DbCls.GetStrNullKara(reader["SI_SITEN5"].ToString());
            //Global.SI_SITENNM5_tb3 = DbCls.GetStrNullKara(reader["BRNAM5"].ToString());
            //Global.SI_KOZA5_tb3 = DbCls.GetStrNullKara(reader["SI_KOZA5"].ToString());
            //Global.SI_KOZANO5_tb3 = DbCls.GetStrNullKara(reader["SI_KOZANO5"].ToString());
            //Global.SI_IRAININ5_tb3 = DbCls.GetStrNullKara(reader["SI_IRAININ5"].ToString());
        }


        /// <summary>
        /// 支払方法コードで自社支払方法を検索
        /// </summary>
        /// <param name="sSHINO"></param>
        public void Sel_SS_SHOHO(string sSHINO)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //自社支払方法の検索
                Global.cCmdSel.CommandText = "SELECT a.*, b.HARAI_H, b.KIJITU_H FROM SS_SHOHO a "
                                           + "LEFT JOIN SS_SKUBN b ON b.SKKBN = 11 AND a.SKBNCOD = b.SKBNCOD "
                                           + "WHERE SHINO = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHINO", int.Parse(sSHINO));//<<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.SIMEBI_tb1 = DbCls.GetStrNullKara(reader["SIMEBI"].ToString());
                    Global.SHIHARAIMM_tb1 = DbCls.GetStrNullKara(reader["SHIHARAIMM"].ToString());
                    Global.SIHARAIDD_tb1 = DbCls.GetStrNullKara(reader["SIHARAIDD"].ToString());
                    Global.SKIJITUMM_tb1 = DbCls.GetStrNullKara(reader["SKIJITUMM"].ToString());
                    Global.SKIJITUDD_tb1 = DbCls.GetStrNullKara(reader["SKIJITUDD"].ToString());
                    Global.SKBNCOD_tb1 = DbCls.GetStrNullKara(reader["SKBNCOD"].ToString());
                    Global.V_YAKUJO_tb1 = DbCls.GetStrNullKara(reader["V_YAKUJO"].ToString());
                    Global.YAKUJOA_L_tb1 = DbCls.GetStrNullKara(reader["YAKUJOA_L"].ToString());
                    Global.YAKUJOA_M_tb1 = DbCls.GetStrNullKara(reader["YAKUJOA_M"].ToString());
                    Global.YAKUJOB_LH_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_LH"].ToString());
                    Global.YAKUJOB_H1_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_H1"].ToString());
                    Global.YAKUJOB_R1_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_R1"].ToString());
                    Global.YAKUJOB_U1_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_U1"].ToString());
                    //Global.YAKUJOB_S1_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_S1"].ToString());
                    Global.YAKUJOB_H2_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_H2"].ToString());
                    Global.YAKUJOB_R2_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_R2"].ToString());
                    Global.YAKUJOB_U2_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_U2"].ToString());
                    //Global.YAKUJOB_S2_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_S2"].ToString());
                    Global.YAKUJOB_H3_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_H3"].ToString());
                    Global.YAKUJOB_R3_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_R3"].ToString());
                    Global.YAKUJOB_U3_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_U3"].ToString());
                    //Global.YAKUJOB_S3_tb1 = DbCls.GetStrNullKara(reader["YAKUJOB_S3"].ToString());
                    Global.HARAI_H_tb1 = DbCls.GetStrNullKara(reader["HARAI_H"].ToString());
                    Global.KIJITU_H_tb1 = DbCls.GetStrNullKara(reader["KIJITU_H"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_SHOHO　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 新規採番(処理ID)
        /// </summary>
        /// <param name="sTRCD">現在表示中の取引先コード</param>
        /// <param name="iSHO_ID">採番する処理id</param>
        public void Sel_MaxSHO_ID(string sTRCD, string sHJCD, out int iSHO_ID, out int iCnt)
        {
            iSHO_ID = 1;
            iCnt = 1;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //取引先支払方法の検索SQL生成&実行
                Global.cCmdSel.CommandText = "SELECT SHO_ID FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p "
                                           + "ORDER BY SHO_ID DESC ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();
                    iCnt++;
                    iSHO_ID = iSHO_ID + DbCls.GetNumNullZero<int>(reader["SHO_ID"]);
                    while (reader.Read())
                    {
                        iCnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_MaxSHO_ID　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        //取引先CD・補助CDに紐付く全TSHOHの取得
        public void Get_SS_TSHOH_All(string sTRCD, string sHJCD, out string[,] sTSHOHArray)
        {
            sTSHOHArray = null;
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                int nCnt = 0;
                #region read count
                {
                    string query =
                    #region query
 @"SELECT count(*) cnt
FROM SS_TSHOH 
WHERE TRCD = :p AND HJCD = :p ";
                    #endregion
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        nCnt = DbCls.GetNumNullZero<int>(reader[0]);
                        sTSHOHArray = new string[nCnt, 30];
                    }

                }
                #endregion

                #region read all data
                {
                    string query =
                    #region query
                        // @"SELECT SHO_ID, BCOD, KICD, SHINO, HARAI_H, KIJITU_H, 
                        //SI_KUBN1, SI_BANK1, SI_SITEN1, SI_KOZA1, SI_KOZANO1, SI_IRAININ1, 
                        //SI_KUBN2, SI_BANK2, SI_SITEN2, SI_KOZA2, SI_KOZANO2, SI_IRAININ2, 
                        //SI_KUBN3, SI_BANK3, SI_SITEN3, SI_KOZA3, SI_KOZANO3, SI_IRAININ3, 
                        //SI_KUBN4, SI_BANK4, SI_SITEN4, SI_KOZA4, SI_KOZANO4, SI_IRAININ4 
                        //FROM SS_TSHOH 
                        //WHERE TRCD = :p AND HJCD = :p ORDER BY SHO_ID ";
@"SELECT 
	TS.SHO_ID, TS.BCOD, TS.KICD, TS.SHINO, TS.HARAI_H, TS.KIJITU_H, 
	KB1.SKBKIND KIND1, TS.SI_KUBN1, OB1.OWNBKCOD SI_BANK1, OB1.OWNBRCOD SI_SITEN1, OB1.YOKNKIND SI_KOZA1, OB1.KOZANO SI_KOZANO1,
	KB2.SKBKIND KIND2, TS.SI_KUBN2, OB2.OWNBKCOD SI_BANK2, OB2.OWNBRCOD SI_SITEN2, OB2.YOKNKIND SI_KOZA2, OB2.KOZANO SI_KOZANO2,
	KB3.SKBKIND KIND3, TS.SI_KUBN3, OB3.OWNBKCOD SI_BANK3, OB3.OWNBRCOD SI_SITEN3, OB3.YOKNKIND SI_KOZA3, OB3.KOZANO SI_KOZANO3,
	KB4.SKBKIND KIND4, TS.SI_KUBN4, OB4.OWNBKCOD SI_BANK4, OB4.OWNBRCOD SI_SITEN4, OB4.YOKNKIND SI_KOZA4, OB4.KOZANO SI_KOZANO4,
	OBF1.OWNBKCOD SI_BANKF1, OBF1.OWNBRCOD SI_SITENF1, OBF1.YOKNKIND SI_KOZAF1, OBF1.KOZANO SI_KOZANOF1,
	OBF2.OWNBKCOD SI_BANKF2, OBF2.OWNBRCOD SI_SITENF2, OBF2.YOKNKIND SI_KOZAF2, OBF2.KOZANO SI_KOZANOF2,
	OBF3.OWNBKCOD SI_BANKF3, OBF3.OWNBRCOD SI_SITENF3, OBF3.YOKNKIND SI_KOZAF3, OBF3.KOZANO SI_KOZANOF3,
	OBF4.OWNBKCOD SI_BANKF4, OBF4.OWNBRCOD SI_SITENF4, OBF4.YOKNKIND SI_KOZAF4, OBF4.KOZANO SI_KOZANOF4
FROM SS_TSHOH TS
    LEFT JOIN SS_SKUBN KB1 ON KB1.SKKBN = 11 AND KB1.SKBNCOD = TS.SI_KUBN1
    LEFT JOIN SS_SKUBN KB2 ON KB2.SKKBN = 11 AND KB2.SKBNCOD = TS.SI_KUBN2
    LEFT JOIN SS_SKUBN KB3 ON KB3.SKKBN = 11 AND KB3.SKBNCOD = TS.SI_KUBN3
    LEFT JOIN SS_SKUBN KB4 ON KB4.SKKBN = 11 AND KB4.SKBNCOD = TS.SI_KUBN4
	LEFT JOIN SS_OWNBK OB1 ON OB1.OWNID = TS.OWNID1
	LEFT JOIN SS_OWNBK OB2 ON OB2.OWNID = TS.OWNID2
	LEFT JOIN SS_OWNBK OB3 ON OB3.OWNID = TS.OWNID3
	LEFT JOIN SS_OWNBK OB4 ON OB4.OWNID = TS.OWNID4
	LEFT JOIN SS_FACTER FC1 ON FC1.FACID = TS.OWNID1
	LEFT JOIN SS_FACTER FC2 ON FC2.FACID = TS.OWNID2
	LEFT JOIN SS_FACTER FC3 ON FC3.FACID = TS.OWNID3
	LEFT JOIN SS_FACTER FC4 ON FC4.FACID = TS.OWNID4
	LEFT JOIN SS_OWNBK OBF1 ON OBF1.OWNID = FC1.OWNID
	LEFT JOIN SS_OWNBK OBF2 ON OBF2.OWNID = FC2.OWNID
	LEFT JOIN SS_OWNBK OBF3 ON OBF3.OWNID = FC3.OWNID
	LEFT JOIN SS_OWNBK OBF4 ON OBF4.OWNID = FC4.OWNID
WHERE TRCD = :p AND HJCD = :p 
ORDER BY TS.SHO_ID 
";
                    #endregion
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    nCnt = 0;
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sTSHOHArray[nCnt, 0] = DbCls.GetStrNullKara(reader["SHO_ID"].ToString());
                            sTSHOHArray[nCnt, 1] = DbCls.GetStrNullKara(reader["BCOD"].ToString());
                            sTSHOHArray[nCnt, 2] = DbCls.GetStrNullKara(reader["KICD"].ToString());
                            sTSHOHArray[nCnt, 3] = DbCls.GetStrNullKara(reader["SHINO"].ToString());
                            sTSHOHArray[nCnt, 4] = DbCls.GetStrNullKara(reader["HARAI_H"].ToString());
                            sTSHOHArray[nCnt, 5] = DbCls.GetStrNullKara(reader["KIJITU_H"].ToString());
                            if (DbCls.GetStrNullKara(reader["KIND1"].ToString()) != "8")
                            {
                                sTSHOHArray[nCnt, 6] = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
                                sTSHOHArray[nCnt, 7] = DbCls.GetStrNullKara(reader["SI_BANK1"].ToString());
                                sTSHOHArray[nCnt, 8] = DbCls.GetStrNullKara(reader["SI_SITEN1"].ToString());
                                sTSHOHArray[nCnt, 9] = DbCls.GetStrNullKara(reader["SI_KOZA1"].ToString());
                                sTSHOHArray[nCnt, 10] = DbCls.GetStrNullKara(reader["SI_KOZANO1"].ToString());
                            }
                            else
                            {
                                sTSHOHArray[nCnt, 6] = DbCls.GetStrNullKara(reader["SI_KUBN1"].ToString());
                                sTSHOHArray[nCnt, 7] = DbCls.GetStrNullKara(reader["SI_BANKF1"].ToString());
                                sTSHOHArray[nCnt, 8] = DbCls.GetStrNullKara(reader["SI_SITENF1"].ToString());
                                sTSHOHArray[nCnt, 9] = DbCls.GetStrNullKara(reader["SI_KOZAF1"].ToString());
                                sTSHOHArray[nCnt, 10] = DbCls.GetStrNullKara(reader["SI_KOZANOF1"].ToString());
                            }
                            //sTSHOHArray[nCnt, 11] = DbCls.GetStrNullKara(reader["SI_IRAININ1"].ToString());

                            if (DbCls.GetStrNullKara(reader["KIND2"].ToString()) != "8")
                            {
                                sTSHOHArray[nCnt, 12] = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
                                sTSHOHArray[nCnt, 13] = DbCls.GetStrNullKara(reader["SI_BANK2"].ToString());
                                sTSHOHArray[nCnt, 14] = DbCls.GetStrNullKara(reader["SI_SITEN2"].ToString());
                                sTSHOHArray[nCnt, 15] = DbCls.GetStrNullKara(reader["SI_KOZA2"].ToString());
                                sTSHOHArray[nCnt, 16] = DbCls.GetStrNullKara(reader["SI_KOZANO2"].ToString());
                            }
                            else
                            {
                                sTSHOHArray[nCnt, 12] = DbCls.GetStrNullKara(reader["SI_KUBN2"].ToString());
                                sTSHOHArray[nCnt, 13] = DbCls.GetStrNullKara(reader["SI_BANKF2"].ToString());
                                sTSHOHArray[nCnt, 14] = DbCls.GetStrNullKara(reader["SI_SITENF2"].ToString());
                                sTSHOHArray[nCnt, 15] = DbCls.GetStrNullKara(reader["SI_KOZAF2"].ToString());
                                sTSHOHArray[nCnt, 16] = DbCls.GetStrNullKara(reader["SI_KOZANOF2"].ToString());
                            }
                            //sTSHOHArray[nCnt, 17] = DbCls.GetStrNullKara(reader["SI_IRAININ2"].ToString());

                            if (DbCls.GetStrNullKara(reader["KIND3"].ToString()) != "8")
                            {
                                sTSHOHArray[nCnt, 18] = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
                                sTSHOHArray[nCnt, 19] = DbCls.GetStrNullKara(reader["SI_BANK3"].ToString());
                                sTSHOHArray[nCnt, 20] = DbCls.GetStrNullKara(reader["SI_SITEN3"].ToString());
                                sTSHOHArray[nCnt, 21] = DbCls.GetStrNullKara(reader["SI_KOZA3"].ToString());
                                sTSHOHArray[nCnt, 22] = DbCls.GetStrNullKara(reader["SI_KOZANO3"].ToString());
                            }
                            else
                            {
                                sTSHOHArray[nCnt, 18] = DbCls.GetStrNullKara(reader["SI_KUBN3"].ToString());
                                sTSHOHArray[nCnt, 19] = DbCls.GetStrNullKara(reader["SI_BANKF3"].ToString());
                                sTSHOHArray[nCnt, 20] = DbCls.GetStrNullKara(reader["SI_SITENF3"].ToString());
                                sTSHOHArray[nCnt, 21] = DbCls.GetStrNullKara(reader["SI_KOZAF3"].ToString());
                                sTSHOHArray[nCnt, 22] = DbCls.GetStrNullKara(reader["SI_KOZANOF3"].ToString());
                            }
                            //sTSHOHArray[nCnt, 23] = DbCls.GetStrNullKara(reader["SI_IRAININ3"].ToString());

                            if (DbCls.GetStrNullKara(reader["KIND3"].ToString()) != "8")
                            {
                                sTSHOHArray[nCnt, 24] = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
                                sTSHOHArray[nCnt, 25] = DbCls.GetStrNullKara(reader["SI_BANK4"].ToString());
                                sTSHOHArray[nCnt, 26] = DbCls.GetStrNullKara(reader["SI_SITEN4"].ToString());
                                sTSHOHArray[nCnt, 27] = DbCls.GetStrNullKara(reader["SI_KOZA4"].ToString());
                                sTSHOHArray[nCnt, 28] = DbCls.GetStrNullKara(reader["SI_KOZANO4"].ToString());
                            }
                            else
                            {
                                sTSHOHArray[nCnt, 24] = DbCls.GetStrNullKara(reader["SI_KUBN4"].ToString());
                                sTSHOHArray[nCnt, 25] = DbCls.GetStrNullKara(reader["SI_BANKF4"].ToString());
                                sTSHOHArray[nCnt, 26] = DbCls.GetStrNullKara(reader["SI_SITENF4"].ToString());
                                sTSHOHArray[nCnt, 27] = DbCls.GetStrNullKara(reader["SI_KOZAF4"].ToString());
                                sTSHOHArray[nCnt, 28] = DbCls.GetStrNullKara(reader["SI_KOZANOF4"].ToString());
                            }
                            //sTSHOHArray[nCnt, 29] = DbCls.GetStrNullKara(reader["SI_IRAININ4"].ToString());
                            nCnt++;
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SS_TSHOH_All　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        public int Get_SelectedKey(string sBKCOD, string sBRCOD, string sYKNKIND, string sKOZANO, string sIRAININ, string sSKBKIND)
        {
            int nSelectedCnt = 0;
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //振込先銀行の検索SQL生成&実行
                string sSql = "";
                if (sSKBKIND != "8")
                {
//-- <2016/02/09 外貨口座を除く口座IDでソート>
//                    sSql = "SELECT OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO " //, IRAININ "
//                         + "FROM SS_OWNBK "
//                         //+ "WHERE FACNAM is null "
//                         + "ORDER BY OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO "; //, IRAININ ";
                    sSql = "SELECT OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO "
                         + "FROM SS_OWNBK WHERE GAIKA <> 1 "
                         + "ORDER BY OWNID ";
//-- <2016/02/09>
                }
                else
                {
//-- <2016/02/09>
//                    sSql = "SELECT OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO " //, IRAININ "
//                         + "FROM SS_OWNBK "
//                         + "INNER JOIN SS_FACTER f ON f.OWNID = SS_OWNBK.OWNID "
//                         //+ "WHERE FACNAM is not null "
//                         + "ORDER BY OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO "; //, IRAININ ";
                    sSql = "SELECT OWNBKCOD, OWNBRCOD, YOKNKIND, KOZANO "
                         + "FROM SS_OWNBK "
                         + "INNER JOIN SS_FACTER f ON f.OWNID = SS_OWNBK.OWNID "
                         + "WHERE GAIKA <> 1  "
                         + "ORDER BY f.FACID "; 
//-- <2016/02/09>
                }
                Global.cCmdSel.CommandText = sSql;
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    nSelectedCnt = 0;
                    while (reader.Read())
                    {
                        if ((DbCls.GetStrNullKara(reader["OWNBKCOD"].ToString()) == sBKCOD) &&
                            (DbCls.GetStrNullKara(reader["OWNBRCOD"].ToString()) == sBRCOD) &&
                            (DbCls.GetStrNullKara(reader["YOKNKIND"].ToString()) == sYKNKIND) &&
                            (DbCls.GetStrNullKara(reader["KOZANO"].ToString()) == sKOZANO)) // &&
                            //(DbCls.GetStrNullKara(reader["IRAININ"].ToString()) == sIRAININ))
                        {
                            return nSelectedCnt;
                        }
                        nSelectedCnt++;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SelectedKey　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return 0;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 対象取引先の支払条件に「部門：全て」「科目：全て」の条件が複数存在しているかどうか確認する
        /// </summary>
        /// <returns>true：複数あり、false：1レコード以下</returns>
        internal bool Exists_Plural_SS_TSHOH_BK_All(string sTRCD, string sHJCD)
        {
            if (string.IsNullOrEmpty(sTRCD) || string.IsNullOrEmpty(sHJCD))
            {
                return false;
            }

            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                string query;
                query = "  SELECT COUNT(*) ";
                query += "   FROM SS_TSHOH ";
                query += "  WHERE RTRIM(TRCD) = :p AND HJCD = :p AND BCOD = '0' AND KICD = '0' ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                query += " HAVING COUNT(*) >= 2 ";
                ExecuteQuery(query
                    , new DBParameter("@TRCD", sTRCD.TrimEnd())
                    , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加

                return reader.HasRows;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SelectedKey　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        #endregion


        #region 振込先銀行の検索(通常検索/前行・次行検索)
        //---> V01.14.01 HWPO UPDATE ▼(8510)
        //private int GetSS_FRIGIN_Count(string trcd, string hjcd)
        internal int GetSS_FRIGIN_Count(string trcd, string hjcd)
        //<--- V01.14.01 HWPO UPDATE ▲(8510)
        {
            int result = 0;
            trcd = GetTrcdDB(trcd);
            string query =
            #region query
 @"SELECT count(*) cnt FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p";
            #endregion
            //---> V01.14.01 HWPO UPDATE ▼(8510)
            //ExecuteQuery(query
            //    , new DBParameter("@TRCD", trcd)
            //    , new DBParameter("@HJCD", hjcd));
            //if (reader.HasRows)
            //{
            //    // <マルチDB対応>Readが必須なので追加
            //    reader.Read();

            //    result = DbCls.GetNumNullZero<int>(reader[0]);
            //}
            try
            {
                ExecuteQuery(query
                , new DBParameter("@TRCD", trcd)
                , new DBParameter("@HJCD", int.Parse(hjcd)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                if (reader.HasRows)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    result = DbCls.GetNumNullZero<int>(reader[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }            
            //<--- V01.14.01 HWPO UPDATE ▲(8510)
            return result;
        }

        /// <summary>
        /// 取引先コードで振込先銀行を検索
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iSS_FRIGIN_cnt"></param>
        public void Sel_SS_FRIGIN(string sTRCD, string sHJCD, out int iSS_FRIGIN_cnt)
        {
            iSS_FRIGIN_cnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                {
                    iSS_FRIGIN_cnt = GetSS_FRIGIN_Count(sTRCD, sHJCD);
                }
                #endregion
                #region read data
                {
                    string query;
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle) 
                    //{
                    //    query = @"SELECT * FROM SS_FRIGIN WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p ORDER BY GIN_ID ";
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        query = @"SELECT * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p ORDER BY GIN_ID LIMIT 1 OFFSET 0"; 
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        query = @"SELECT top 1 * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p ORDER BY GIN_ID ";
                    }
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    if (reader.HasRows)
                    {
                        set_SS_FRIGIN();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_FRIGIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 取引先コードで振込先銀行を検索
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iSS_FRIGIN_cnt"></param>
        public void Sel_SS_FRIGIN_Prev(string sTRCD, string sHJCD, string sGIN_ID, out int iCnt, out int iSS_FRIGIN_cnt)
        {
            iSS_FRIGIN_cnt = 0;
            iCnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                {
                    string query =
                    #region query
 @"select count(*) cnt, sum(case when GIN_ID < :p then 1 else 0 end) remains 
from SS_FRIGIN where TRCD = :p and HJCD = :p";
                    #endregion
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //ExecuteQuery(query
                    //    , new DBParameter("@GNID", sGIN_ID)
                    //    , new DBParameter("@TRCD", sTRCD)
                    //    , new DBParameter("@HJCD", sHJCD));
                    ExecuteQuery(query
                        , new DBParameter("@GNID", int.Parse(sGIN_ID))
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        iCnt = DbCls.GetNumNullZero<int>(reader["cnt"]);
                        iSS_FRIGIN_cnt = DbCls.GetNumNullZero<int>(reader["remains"]);
                    }
                }
                #endregion
                #region read data
                {
                    string query;
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)
                    //{
                    //    query = @"SELECT * FROM SS_FRIGIN WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p AND GIN_ID < :p ORDER BY GIN_ID DESC ";
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        query = @"SELECT * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p AND GIN_ID < :p ORDER BY GIN_ID DESC LIMIT 1 OFFSET 0";
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        query = @"SELECT top 1 * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p AND GIN_ID < :p ORDER BY GIN_ID DESC ";
                    }
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //ExecuteQuery(query
                    //    , new DBParameter("@TRCD", sTRCD)
                    //    , new DBParameter("@HJCD", sHJCD)
                    //    , new DBParameter("@GNID", sGIN_ID));
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD))
                        , new DBParameter("@GNID", int.Parse(sGIN_ID)));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        set_SS_FRIGIN();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_FRIGIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// 取引先コードで振込先銀行を検索
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="iSS_FRIGIN_cnt"></param>
        public void Sel_SS_FRIGIN_Next(string sTRCD, string sHJCD, string sGIN_ID, out int iCnt, out int iSS_FRIGIN_cnt)
        {
            iSS_FRIGIN_cnt = 0;
            iCnt = 0;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                {
                    string query =
                    #region query
 @"select count(*) cnt, sum(case when GIN_ID > :p then 1 else 0 end) remains
from SS_FRIGIN where TRCD = :p and HJCD = :p";
                    #endregion
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //ExecuteQuery(query
                    //    , new DBParameter("@GNID", sGIN_ID)
                    //    , new DBParameter("@TRCD", sTRCD)
                    //    , new DBParameter("@HJCD", sHJCD));
                    ExecuteQuery(query
                        , new DBParameter("@GNID", int.Parse(sGIN_ID))
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD)));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();

                        iCnt = DbCls.GetNumNullZero<int>(reader["cnt"]);
                        iSS_FRIGIN_cnt = DbCls.GetNumNullZero<int>(reader["remains"]);
                    }
                }
                #endregion
                #region read data
                {
                    string query;
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.Oracle)                    
                    //{
                    //    query = @"SELECT * FROM SS_FRIGIN WHERE ROWNUM = 1 AND TRCD = :p AND HJCD = :p AND GIN_ID > :p order by GIN_ID";
                    //}
                    if(ComUtil.IsPostgreSQL())
                    {
                        query = @"SELECT * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p AND GIN_ID > :p order by GIN_ID LIMIT 1 OFFSET 0";
                    }
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    else
                    {
                        query = @"SELECT top 1 * FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p AND GIN_ID > :p order by GIN_ID";
                    }
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //ExecuteQuery(query
                    //    , new DBParameter("@TRCD", sTRCD)
                    //    , new DBParameter("@HJCD", sHJCD)
                    //    , new DBParameter("@GNID", sGIN_ID));
                    ExecuteQuery(query
                        , new DBParameter("@TRCD", sTRCD)
                        , new DBParameter("@HJCD", int.Parse(sHJCD))
                        , new DBParameter("@GNID", int.Parse(sGIN_ID)));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (reader.HasRows)
                    {
                        set_SS_FRIGIN();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SS_FRIGIN_Next　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// DataReaderの内容をGlobalに設定
        /// </summary>
        private void set_SS_FRIGIN()
        {
            // <マルチDB対応>Readが必須なので追加
            reader.Read();

            Global.GIN_ID_tb2 = DbCls.GetStrNullKara(reader["GIN_ID"].ToString());
            Global.BANK_CD_tb2 = DbCls.GetStrNullKara(reader["BANK_CD"].ToString());
            Global.SITEN_ID_tb2 = DbCls.GetStrNullKara(reader["SITEN_ID"].ToString());
            Global.YOKIN_TYP_tb2 = DbCls.GetStrNullKara(reader["YOKIN_TYP"].ToString());
            Global.KOUZA_tb2 = DbCls.GetStrNullKara(reader["KOUZA"].ToString());
            Global.MEIGI_tb2 = DbCls.GetStrNullKara(reader["MEIGI"].ToString());
            Global.MEIGIK_tb2 = DbCls.GetStrNullKara(reader["MEIGIK"].ToString());
            Global.TESUU_tb2 = DbCls.GetStrNullKara(reader["TESUU"].ToString());
            Global.SOUKIN_tb2 = DbCls.GetStrNullKara(reader["SOUKIN"].ToString());
            Global.GENDO_tb2 = DbCls.GetStrNullKara(reader["GENDO"].ToString());

            Global.FDEF = DbCls.GetStrNullKara(reader["FDEF"].ToString());
            Global.DDEF = DbCls.GetStrNullKara(reader["DDEF"].ToString());
            Global.FTESUID = DbCls.GetStrNullKara(reader["FTESUID"].ToString());
            Global.DTESUSW = DbCls.GetStrNullKara(reader["DTESUSW"].ToString());
            Global.DTESU = DbCls.GetStrNullKara(reader["DTESU"].ToString());
        }


        /// <summary>
        /// 新規採番(銀行ID)
        /// </summary>
        /// <param name="sTRCD">現在表示中の取引先コード</param>
        /// <param name="sHJCD">現在表示中の取引先コード</param>
        /// <param name="iSHO_ID">採番する銀行id</param>
        public void Sel_MaxGIN_ID(string sTRCD, string sHJCD, out int iGIN_ID, out int iCnt)
        {
            iGIN_ID = 1;
            iCnt = 1;

            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                #region read count
                string query =
                #region query
 @"select count(*) cnt, max(GIN_ID) max_id from SS_FRIGIN where TRCD = :p and HJCD = :p";
                #endregion
                ExecuteQuery(query
                    , new DBParameter("@TRCD", sTRCD)
                    , new DBParameter("@HJCD", int.Parse(sHJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                if (reader.HasRows)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    iGIN_ID += DbCls.GetNumNullZero<int>(reader["max_id"]);
                    iCnt += DbCls.GetNumNullZero<int>(reader["cnt"]);
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_MaxGIN_ID　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #endregion

        #region 登録
        /// <summary>
        /// データ登録：取引先
        /// </summary>
        public void Insert_SS_TORI_MIN(int iInsUpdFlg)
        {
            try
            {
                if (iInsUpdFlg == 0)
                {
                    //取引先マスタの登録SQL生成&実行(手形管理のみ使用チェックON用)
                    // Ver.00.01.09 [SS_1312]対応 -->
                    //Global.cCmdIns.CommandText = "INSERT INTO SS_TORI  "
                    //                           + "(TRCD, HJCD, TRKBN, RYAKU, TORI_NAM, KNLD, TRFURI, TGASW, SAIKEN, SAIKEN_FLG, SAIMU, SAIMU_FLG, GRPID, HJSW, "
                    //                           + "STYMD, EDYMD, STFLG, FMOD, FTIM, FUSR, FWAY, LMOD, LTIM, LUSR, LWAY) "
                    //                           + "VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                    Global.cCmdIns.CommandText = "INSERT INTO SS_TORI  "
                           + "(TRCD, HJCD, TRKBN, RYAKU, TORI_NAM, KNLD, TRFURI, TGASW, SAIKEN, SAIKEN_FLG, SAIMU, SAIMU_FLG, GRPID, HJSW, SBCOD, SKICD, "
                           // ▼#111516　竹内　2022/03/09
                           + "ZIP, ADDR1, ADDR2, TEL, FAX, SBUSYO, STANTO, KEICD, STAN, TRMAIL, TRURL, BIKO, E_TANTOCD, CDM1, IDM1, MYNO_AITE, SOSAI, SRYOU_F, "
                           // ▲#111516　竹内　2022/03/09 
                           + "STYMD, EDYMD, STFLG, FMOD, FTIM, FUSR, FWAY, LMOD, LTIM, LUSR, LWAY) "
                           // ▼#111516　竹内　2022/03/09
                           //+ "VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                           + "VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                           // ▲#111516　竹内　2022/03/09
                    // Ver.00.01.09 <--
                    Global.cCmdIns.Parameters.Clear();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", int.Parse(Global.TRKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", int.Parse(Global.TGASW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", int.Parse(Global.SAIKEN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", int.Parse(Global.SAIKEN_FLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", int.Parse(Global.SAIMU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", int.Parse(Global.SAIMU_FLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", int.Parse(Global.GRPID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    // ▼#111516　竹内　2022/03/09
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    if (String.IsNullOrEmpty(Global.KEICD) || Global.KEICD == "0")
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", DBNull.Value);
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", int.Parse(Global.KEICD));
                    }
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", int.Parse(Global.IDM1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", int.Parse(Global.SOSAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", int.Parse(Global.SRYOU_F));
                    // ▲#111516　竹内　2022/03/09
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", int.Parse(Global.STYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", int.Parse(Global.EDYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", int.Parse(Global.STFLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    #region OLD
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", Global.TRKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", Global.TGASW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", Global.SAIKEN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", Global.SAIKEN_FLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", Global.SAIMU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", Global.SAIMU_FLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", Global.GRPID);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    //// Ver.00.01.09 [SS_1312]対応 -->
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    //// Ver.00.01.09 <--
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", Global.STYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", Global.EDYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", Global.STFLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    #endregion                    
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", Global.KEICD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GOU", Global.GOU);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);

                    ////**>>ICS-S 2013/05/20
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", Global.CDM2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CD03", Global.CD03);
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else
                {
                    //取引先マスタの更新SQL生成&実行(手形管理のみ使用チェックON用)
                    //Global.cCmdIns.CommandText = "UPDATE SS_TORI SET "
                    //                           + "RYAKU = :p, TORI_NAM = :p, KNLD = :p, TGASW = :p, ZIP = :p, ADDR1 = :p, ADDR2 = :p, TEL = :p, FAX = :p, "
                    //                           + "SBUSYO = :p, STANTO = :p, KEICD = :p, STYMD = :p, EDYMD = :p, STFLG = :p, LMOD = :p, LTIM = :p, LUSR = :p, LWAY = :p "
                    //                           + "WHERE TRCD = :p AND HJCD = :p ";
                    //Global.cCmdIns.Parameters.Clear();
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", Global.TGASW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", Global.KEICD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", Global.STYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", Global.EDYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", Global.STFLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);

                    Global.cCmdIns.CommandText = "UPDATE SS_TORI SET "
                                               + "RYAKU = :p, TORI_NAM = :p, KNLD = :p, TGASW = :p, SAIKEN = 0, SAIKEN_FLG = 0, SAIMU = 0, SAIMU_FLG = 0, TRFURI = :p, "
                                               // Ver.00.01.09 [SS_1312]対応 -->
                                               + "SBCOD = :p, SKICD = :p, "
                                               // ▼#111516　竹内　2022/03/09
                                               + "ZIP = :p, ADDR1 = :p, ADDR2 = :p, TEL = :p, FAX = :p, SBUSYO = :p, STANTO = :p, KEICD = :p, STAN = :p, "
                                               + "TRMAIL = :p, TRURL = :p, BIKO = :p, E_TANTOCD = :p, CDM1 = :p, IDM1 = :p, MYNO_AITE = :p, SOSAI = :p, SRYOU_F = :p, "
                                               // ▲#111516　竹内　2022/03/09
                                               // Ver.00.01.09 <--
                                               + "STYMD = :p, EDYMD = :p, STFLG = :p, LMOD = :p, LTIM = :p, LUSR = :p, LWAY = :p "
                                               + "WHERE TRCD = :p AND HJCD = :p ";
                    Global.cCmdIns.Parameters.Clear();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", int.Parse(Global.TGASW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    // ▼#111516　竹内　2022/03/09
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    if (String.IsNullOrEmpty(Global.KEICD) || Global.KEICD == "0")
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", DBNull.Value);
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", int.Parse(Global.KEICD));
                    }
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", int.Parse(Global.IDM1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", int.Parse(Global.SOSAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", int.Parse(Global.SRYOU_F));
                    // ▲#111516　竹内　2022/03/09
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", int.Parse(Global.STYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", int.Parse(Global.EDYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", int.Parse(Global.STFLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    #region OLD
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", Global.TGASW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    //// Ver.00.01.09 [SS_1312]対応 -->
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    //// Ver.00.01.09 <--
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", Global.STYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", Global.EDYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", Global.STFLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    #endregion
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nInsert_SS_TORI_MIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// データ登録：取引先
        /// </summary>
        public void Insert_SS_TORI_Full(int iInsUpdFlg)
        {
            try
            {
                if (iInsUpdFlg == 0)
                {
                    //取引先マスタの登録SQL生成&実行(手形管理のみ使用チェックOFF用)
                    Global.cCmdIns.CommandText = "INSERT INTO SS_TORI "
                                               + "(TRCD, HJCD, TRKBN, RYAKU, TORI_NAM, KNLD, TGASW, HJSW, ZIP, ADDR1, "
                                               + "ADDR2, TEL, FAX, SBUSYO, STANTO, KEICD, STAN, SJBCD, SBCOD, SKICD, "
                        //+ "SI_KUBN1, SI_BANK1, SI_SITEN1, SI_KOZA1, SI_KOZANO1, SI_IRAININ1, "
                        //+ "SI_KUBN2, SI_BANK2, SI_SITEN2, SI_KOZA2, SI_KOZANO2, SI_IRAININ2, "
                        //+ "SI_KUBN3, SI_BANK3, SI_SITEN3, SI_KOZA3, SI_KOZANO3, SI_IRAININ3, "
                        //+ "SI_KUBN4, SI_BANK4, SI_SITEN4, SI_KOZA4, SI_KOZANO4, SI_IRAININ4, "
                        //+ "SI_KUBN5, SI_BANK5, SI_SITEN5, SI_KOZA5, SI_KOZANO5, SI_IRAININ5, "
                                               + "NAYOSE, F_SETUIN, F_SHITU, F_ZAN, F_SOUFU, ANNAI, TSOKBN, HORYU, HOVAL, "
                        //+ "KAIIN, KYKAI, KYVAL, KYCAL, KYZAF, KYZVL, KYZRT, KYZAH, KYZAS, "
                        //+ "KYROF, KYRVL, KYRRT, KYROH, KYROS, KYGAF, KYGVL, KYGRT, KYGAH, KYGAS, "
                        //+ "KYKEF, KYKVL, KYKRT, KYKEH, KYKES, GENSEN, GOU, GGKBN, GGKBNM, GSKUBN, "
                                               + "GENSEN, GOU, GGKBN, GGKBNM, GSKUBN, "  //5
                                               + "SZEI, SOSAI, SOKICD, HEI_CD, DM1, DM2 , DM3, STYMD, EDYMD, STFLG, "
                                               + "CDM1, CDM2, CDM3, CDM4, IDM1, IDM2, IDM3, IDM4, FMOD, FTIM, FUSR, FWAY, LMOD, "  //7
                        //+ "LTIM, LUSR, LWAY, KYZSKBN, KYRSKBN, KYGSKBN, KYKSKBN, CD03, "
                                               + "LTIM, LUSR, LWAY, CD03, "

                                               + "TRFURI, GRPID, SAIKEN, SAIKEN_FLG, SAIMU, SAIMU_FLG, "
                                               + "TRMAIL, TRURL, BIKO, E_TANTOCD, MYNO_AITE, SRYOU_F, "

                                               + "TOKUKANA, FUTAN, KAISYU, YAKUJO, SHIME, KAISYUHI, KAISYUSIGHT, "
                                               + "Y_KINGAKU, HOLIDAY, MIMAN, "
                                               + "IJOU_1, BUNKATSU_1, HASU_1, SIGHT_1, "
                                               + "IJOU_2, BUNKATSU_2, HASU_2, SIGHT_2, "
                                               + "IJOU_3, BUNKATSU_3, HASU_3, SIGHT_3, "
                                               + "SEN_GINKOCD, SEN_SITENCD, SEN_SHITENMEI, YOKINSYU, SEN_KOZANO, "
                                               + "JIDOU_GAKUSYU, NYUKIN_YOTEI, TESURYO_GAKUSYU, TESURYO_GOSA, RYOSYUSYO, "
                                               + "SHIN_KAISYACD, YOSIN, YOSHINRANK, "
                                               + "GAIKA, TSUKA, "
                                               + "GAIKA_KEY_F, GAIKA_KEY_B, "
                                               + "HIFURIKOZA_1, HIFURIKOZA_2, HIFURIKOZA_3, "

                                               + "GAI_F, TEGVAL, GSSKBN, HR_KIJYUN, HORYU_F, HRORYUGAKU, HRKBN, "
                                               + "GAI_SF, GAI_SH, GAI_KZID, GAI_TF, ENG_NAME, ENG_ADDR, "
                                               + "ENG_KZNO, ENG_SWIF, ENG_BNKNAM, ENG_BRNNAM, ENG_BNKADDR) "

                                               + "VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, " //5
                                               + ":p, :p, :p, :p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, "  //7
                                               + ":p, :p, :p, :p, "

                                               + ":p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, :p, "

                                               + ":p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, "
                                               + ":p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, "
                                               + ":p, :p, "
                                               + ":p, :p, "
                                               + ":p, :p, :p, "

                                               + ":p, :p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p, :p, "
                                               + ":p, :p, :p, :p, :p) ";
                    //**<<
                    Global.cCmdIns.Parameters.Clear();
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", int.Parse(Global.TRKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", int.Parse(Global.TGASW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    if (String.IsNullOrEmpty(Global.KEICD) || Global.KEICD == "0")
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", DBNull.Value);
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", int.Parse(Global.KEICD));
                    }
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SJBCD", Global.SJBCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NAYOSE", int.Parse(Global.NAYOSE));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SETUIN", int.Parse(Global.F_SETUIN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SHITU", int.Parse(Global.F_SHITU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_ZAN", int.Parse(Global.F_ZAN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SOUFU", int.Parse(Global.F_SOUFU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ANNAI", int.Parse(Global.ANNAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSOKBN", int.Parse(Global.TSOKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU", int.Parse(Global.HORYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOVAL", Str2NullAndDec(Global.HOVAL));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENSEN", int.Parse(Global.GENSEN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GOU", GetNullableInt(Global.GOU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBN", GetNullableInt(Global.GGKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBNM", GetNullableString(Global.GGKBNM));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSKUBN", GetNullableInt(Global.GSKUBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SZEI", int.Parse(Global.SZEI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", int.Parse(Global.SOSAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOKICD", Global.SOKICD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HEI_CD", Global.HEI_CD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM1", Global.DM1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM2", int.Parse(Global.DM2));//<--- V02.01.02 HWPO UPDATE ◀【PostgreSQL対応】スペースのみ削除
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM3", int.Parse(Global.DM3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", int.Parse(Global.STYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", int.Parse(Global.EDYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", int.Parse(Global.STFLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", Global.CDM2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM3", "");
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM4", "");
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", int.Parse(Global.IDM1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM2", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM3", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM4", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CD03", Global.CD03);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", int.Parse(Global.GRPID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", int.Parse(Global.SAIKEN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", int.Parse(Global.SAIKEN_FLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", int.Parse(Global.SAIMU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", int.Parse(Global.SAIMU_FLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", int.Parse(Global.SRYOU_F));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TOKUKANA", Global.TOKUKANA);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUTAN", GetNullableInt(Global.FUTAN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYU", Global.KAISYU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YAKUJO", int.Parse(Global.YAKUJYO));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIME", Global.SHIME);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUHI", Global.KAISYUHI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUSIGHT", Global.KAISYUSIGHT);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_KINGAKU", Str2NullAndDec(Global.Y_KINGAKU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOLIDAY", GetNullableInt(Global.HOLIDAY));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MIMAN", Global.MIMAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_1", Global.IJOU_1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_1", Str2NullAndDec(Global.BUNKATSU_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_1", GetNullableInt(Global.HASU_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_1", Global.SIGHT_1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_2", Global.IJOU_2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_2", Str2NullAndDec(Global.BUNKATSU_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_2", GetNullableInt(Global.HASU_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_2", Global.SIGHT_2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_3", Global.IJOU_3);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_3", Str2NullAndDec(Global.BUNKATSU_3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_3", GetNullableInt(Global.HASU_3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_3", Global.SIGHT_3);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_GINKOCD", Global.SEN_GINKOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SITENCD", Global.SEN_SITENCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SHITENMEI", Global.KASO_SITENNM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKINSYU", Global.YOKINSYU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_KOZANO", Global.SEN_KOZANO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@JIDOU_GAKUSYU", GetNullableInt(Global.JIDOU_GAKUSYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NYUKIN_YOTEI", GetNullableInt(Global.NYUKIN_YOTEI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GAKUSYU", GetNullableInt(Global.TESURYO_GAKUSYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GOSA", GetNullableInt(Global.TESURYO_GOSA));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYOSYUSYO", GetNullableInt(Global.RYOSYUSYO));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIN_KAISYACD", Global.SHIN_KAISYACD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSIN", Str2NullAndDec(Global.YOSIN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSHINRANK", Global.YOSHINRANK);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", GetNullableInt(Global.GAIKA));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSUKA", Global.TSUKA);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_F", Global.GAIKA_KEY_F);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_B", Global.GAIKA_KEY_B);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_1", GetNullableInt(Global.HIFURIKOZA_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_2", GetNullableInt(Global.HIFURIKOZA_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_3", GetNullableInt(Global.HIFURIKOZA_3));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_F", int.Parse(Global.GAI_F));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEGVAL", GetNullableInt(Global.TEGVAL));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", int.Parse(Global.GSSKBN));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HR_KIJYUN", Str2NullAndDec(Global.HR_KIJYUN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU_F", GetNullableInt(Global.HORYU_F));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRORYUGAKU", Str2NullAndDec(Global.HRORYUGAKU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRKBN", GetNullableInt(Global.HRKBN));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SF", GetNullableInt(Global.GAI_SF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SH", GetNullableInt(Global.GAI_SH));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_KZID", GetNullableInt(Global.GAI_KZID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_TF", GetNullableInt(Global.GAI_TF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_NAME", Global.ENG_NAME);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_ADDR", Global.ENG_ADDR);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_KZNO", Global.ENG_KZNO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_SWIF", Global.ENG_SWIF);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKNAM", Global.ENG_BNKNAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BRNNAM", Global.ENG_BRNNAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKADDR", Global.ENG_BNKADDR);
                    #region OLD
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", Global.TRKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", Global.TGASW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", Global.KEICD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SJBCD", Global.SJBCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_BANK1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_SITEN1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZA1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZANO1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_IRAININ1", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_BANK2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_SITEN2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZA2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZANO2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_IRAININ2", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_BANK3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_SITEN3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZA3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZANO3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_IRAININ3", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_BANK4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_SITEN4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZA4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZANO4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_IRAININ4", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN5", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_BANK5", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_SITEN5", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZA5", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KOZANO5", DBNull.Value); //null);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_IRAININ5", DBNull.Value); //null);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NAYOSE", Global.NAYOSE);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SETUIN", Global.F_SETUIN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SHITU", Global.F_SHITU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_ZAN", Global.F_ZAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SOUFU", Global.F_SOUFU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ANNAI", Global.ANNAI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSOKBN", Global.TSOKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU", Global.HORYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOVAL", Global.HOVAL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOKKBN", Global.HOKKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HODM1", Global.HODM1);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAIIN", Global.KAIIN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKAI", Global.KYKAI);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYVAL", Global.KYVAL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYCAL", Global.KYCAL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZAF", Global.KYZAF);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZVL", Global.KYZVL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZRT", Global.KYZRT);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZAH", Global.KYZAH);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZAS", Global.KYZAS);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYROF", Global.KYROF);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYRVL", Global.KYRVL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYRRT", Global.KYRRT);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYROH", Global.KYROH);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYROS", Global.KYROS);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGAF", Global.KYGAF);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGVL", Global.KYGVL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGRT", Global.KYGRT);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGAH", Global.KYGAH);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGAS", Global.KYGAS);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKEF", Global.KYKEF);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKVL", Global.KYKVL);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKRT", Global.KYKRT);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKEH", Global.KYKEH);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKES", Global.KYKES);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENSEN", Global.GENSEN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GOU", GetNullableInt(Global.GOU));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBN", GetNullableInt(Global.GGKBN));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBNM", GetNullableString(Global.GGKBNM));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSKUBN", GetNullableInt(Global.GSKUBN));
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", Global.GSSKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SZEI", Global.SZEI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", Global.SOSAI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOKICD", Global.SOKICD);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", Global.GAIKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HEI_CD", Global.HEI_CD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM1", Global.DM1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM2 ", Global.DM2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM3", Global.DM3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", Global.STYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", Global.EDYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", Global.STFLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);
                    ////**>>ICS-S 2013/05/20
                    ////**DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", "");
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", Global.CDM2);
                    ////**<<ICS-E
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM3", "");
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM4", "");
                    ////**>>ICS-S 2013/05/20
                    ////**DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", Global.IDM1);
                    ////**<<ICS-E
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM2", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM3", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM4", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZSKBN", DBNull.Value); // null); //★暫定
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYRSKBN", DBNull.Value); // null); //★暫定
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGSKBN", DBNull.Value); // null); //★暫定
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKSKBN", DBNull.Value); // null); //★暫定
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZSKBN", Global.KYZSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYRSKBN", Global.KYRSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGSKBN", Global.KYGSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKSKBN", Global.KYKSKBN);
                    ////**>>
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CD03", Global.CD03);
                    ////**<<

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", Global.GRPID);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", Global.SAIKEN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", Global.SAIKEN_FLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", Global.SAIMU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", Global.SAIMU_FLG);


                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", Global.SRYOU_F);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TOKUKANA", Global.TOKUKANA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUTAN", Global.FUTAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYU", Global.KAISYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YAKUJO", Global.YAKUJYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIME", Global.SHIME);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUHI", Global.KAISYUHI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUSIGHT", Global.KAISYUSIGHT);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_KINGAKU", Global.Y_KINGAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOLIDAY", Global.HOLIDAY);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MIMAN", Global.MIMAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_1", Global.IJOU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_1", Global.BUNKATSU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_1", Global.HASU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_1", Global.SIGHT_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_2", Global.IJOU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_2", Global.BUNKATSU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_2", Global.HASU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_2", Global.SIGHT_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_3", Global.IJOU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_3", Global.BUNKATSU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_3", Global.HASU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_3", Global.SIGHT_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_GINKOCD", Global.SEN_GINKOCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SITENCD", Global.SEN_SITENCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SHITENMEI", Global.KASO_SITENNM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKINSYU", Global.YOKINSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_KOZANO", Global.SEN_KOZANO);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@JIDOU_GAKUSYU", Global.JIDOU_GAKUSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NYUKIN_YOTEI", Global.NYUKIN_YOTEI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GAKUSYU", Global.TESURYO_GAKUSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GOSA", Global.TESURYO_GOSA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYOSYUSYO", Global.RYOSYUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIN_KAISYACD", Global.SHIN_KAISYACD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSIN", Global.YOSIN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSHINRANK", Global.YOSHINRANK);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", Global.GAIKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSUKA", Global.TSUKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_F", Global.GAIKA_KEY_F);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_B", Global.GAIKA_KEY_B);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_1", Global.HIFURIKOZA_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_2", Global.HIFURIKOZA_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_3", Global.HIFURIKOZA_3);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_F", Global.GAI_F);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEGVAL", Global.TEGVAL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", Global.GSSKBN);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HR_KIJYUN", Global.HR_KIJYUN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU_F", Global.HORYU_F);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRORYUGAKU", Global.HRORYUGAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRKBN", Global.HRKBN);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SF", Global.GAI_SF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SH", Global.GAI_SH);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_KZID", Global.GAI_KZID);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_TF", Global.GAI_TF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_NAME", Global.ENG_NAME);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_ADDR", Global.ENG_ADDR);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_KZNO", Global.ENG_KZNO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_SWIF", Global.ENG_SWIF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKNAM", Global.ENG_BNKNAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BRNNAM", Global.ENG_BRNNAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKADDR", Global.ENG_BNKADDR);
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    #endregion                    
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】                    
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else
                {
                    Global.cCmdIns.CommandText = "UPDATE SS_TORI SET "
                                               + "TRKBN = :p, RYAKU = :p, TORI_NAM = :p, KNLD = :p, TGASW = :p, HJSW = :p, ZIP = :p, ADDR1 = :p, ADDR2 = :p, "
                                               + "TEL = :p, FAX = :p, SBUSYO = :p, STANTO = :p, KEICD = :p, STAN = :p, SJBCD = :p, SBCOD = :p, SKICD = :p, "
                                               + "NAYOSE = :p, F_SETUIN = :p, F_SHITU = :p, F_ZAN = :p, F_SOUFU = :p, ANNAI = :p, TSOKBN = :p, HORYU = :p, "
                                               + "HOVAL = :p, GENSEN = :p, GOU = :p, GGKBN = :p, GGKBNM = :p, GSKUBN = :p, "
                                               + "SZEI = :p, SOSAI = :p, SOKICD = :p, HEI_CD = :p, DM1 = :p, DM2  = :p, DM3 = :p, "
                                               + "STYMD = :p, EDYMD = :p, STFLG = :p, CDM1 = :p, LMOD = :p, LTIM = :p, LUSR = :p, LWAY = :p, "
                                               + "CDM2 = :p, CD03 = :p, IDM1 = :p, "

                                               + ""
                                               + "TRFURI = :p, GRPID = :p, SAIKEN = :p, SAIKEN_FLG =:p, SAIMU = :p, SAIMU_FLG = :p, "
                                               + "TRMAIL = :p, TRURL = :p, BIKO = :p, E_TANTOCD = :p, MYNO_AITE = :p, SRYOU_F = :p, "
                                               
                                               + "TOKUKANA = :p, FUTAN = :p, KAISYU = :p, YAKUJO = :p, SHIME = :p, KAISYUHI = :p, KAISYUSIGHT = :p, "
                                               + "Y_KINGAKU = :p, HOLIDAY = :p, MIMAN = :p, "
                                               + "IJOU_1 = :p, BUNKATSU_1 = :p, HASU_1 = :p, SIGHT_1 = :p, "
                                               + "IJOU_2 = :p, BUNKATSU_2 = :p, HASU_2 = :p, SIGHT_2 = :p, "
                                               + "IJOU_3 = :p, BUNKATSU_3 = :p, HASU_3 = :p, SIGHT_3 = :p, "
                                               + "SEN_GINKOCD = :p, SEN_SITENCD = :p, SEN_SHITENMEI = :p, YOKINSYU = :p, SEN_KOZANO = :p, "
                                               + "JIDOU_GAKUSYU = :p, NYUKIN_YOTEI = :p, TESURYO_GAKUSYU = :p, TESURYO_GOSA = :p, RYOSYUSYO = :p, "
                                               + "SHIN_KAISYACD = :p, YOSIN = :p, YOSHINRANK = :p, "
                                               + "GAIKA = :p, TSUKA = :p, "
                                               + "GAIKA_KEY_F = :p, GAIKA_KEY_B = :p, "
                                               + "HIFURIKOZA_1 = :p, HIFURIKOZA_2 = :p, HIFURIKOZA_3 = :p, "

                                               + "GAI_F = :p, TEGVAL = :p, GSSKBN = :p, HR_KIJYUN = :p, HORYU_F = :p, HRORYUGAKU = :p, HRKBN = :p, "
                                               + "GAI_SF = :p, GAI_SH = :p, GAI_KZID = :p, GAI_TF = :p, ENG_NAME = :p, ENG_ADDR = :p, "
                                               + "ENG_KZNO = :p, ENG_SWIF = :p, ENG_BNKNAM = :p, ENG_BRNNAM = :p, ENG_BNKADDR = :p "

                                               + "WHERE TRCD = :p AND HJCD = :p ";
                    //+ "KYKEH = :p, KYKES = :p, GENSEN = :p, GOU = :p, GGKBN = :p, GGKBNM = :p, GSKUBN = :p, GSSKBN = :p, "
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", int.Parse(Global.TRKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", int.Parse(Global.TGASW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    if (String.IsNullOrEmpty(Global.KEICD) || Global.KEICD == "0")
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", DBNull.Value);
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", int.Parse(Global.KEICD));
                    }
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SJBCD", Global.SJBCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NAYOSE", int.Parse(Global.NAYOSE));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SETUIN", int.Parse(Global.F_SETUIN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SHITU", int.Parse(Global.F_SHITU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_ZAN", int.Parse(Global.F_ZAN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SOUFU", int.Parse(Global.F_SOUFU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ANNAI", int.Parse(Global.ANNAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSOKBN", int.Parse(Global.TSOKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU", int.Parse(Global.HORYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOVAL", Str2NullAndDec(Global.HOVAL));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENSEN", int.Parse(Global.GENSEN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GOU", GetNullableInt(Global.GOU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBN", GetNullableInt(Global.GGKBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBNM", GetNullableString(Global.GGKBNM));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSKUBN", GetNullableInt(Global.GSKUBN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SZEI", int.Parse(Global.SZEI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", int.Parse(Global.SOSAI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOKICD", Global.SOKICD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HEI_CD", Global.HEI_CD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM1", Global.DM1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM2", int.Parse(Global.DM2));//<--- V02.01.02 HWPO UPDATE ◀【PostgreSQL対応】スペースのみ削除
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM3", int.Parse(Global.DM3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", int.Parse(Global.STYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", int.Parse(Global.EDYMD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", int.Parse(Global.STFLG));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", Global.CDM2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CD03", Global.CD03);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", int.Parse(Global.IDM1));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", int.Parse(Global.GRPID));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", int.Parse(Global.SAIKEN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", int.Parse(Global.SAIKEN_FLG));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", int.Parse(Global.SAIMU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", int.Parse(Global.SAIMU_FLG));


                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", int.Parse(Global.SRYOU_F));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TOKUKANA", Global.TOKUKANA);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUTAN", GetNullableInt(Global.FUTAN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYU", Global.KAISYU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YAKUJO", int.Parse(Global.YAKUJYO));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIME", Global.SHIME);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUHI", Global.KAISYUHI);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUSIGHT", Global.KAISYUSIGHT);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_KINGAKU", Str2NullAndDec(Global.Y_KINGAKU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOLIDAY", GetNullableInt(Global.HOLIDAY));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MIMAN", Global.MIMAN);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_1", Global.IJOU_1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_1", Str2NullAndDec(Global.BUNKATSU_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_1", GetNullableInt(Global.HASU_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_1", Global.SIGHT_1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_2", Global.IJOU_2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_2", Str2NullAndDec(Global.BUNKATSU_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_2", GetNullableInt(Global.HASU_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_2", Global.SIGHT_2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_3", Global.IJOU_3);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_3", Str2NullAndDec(Global.BUNKATSU_3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_3", GetNullableInt(Global.HASU_3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_3", Global.SIGHT_3);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_GINKOCD", Global.SEN_GINKOCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SITENCD", Global.SEN_SITENCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SHITENMEI", Global.KASO_SITENNM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKINSYU", Global.YOKINSYU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_KOZANO", Global.SEN_KOZANO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@JIDOU_GAKUSYU", GetNullableInt(Global.JIDOU_GAKUSYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NYUKIN_YOTEI", GetNullableInt(Global.NYUKIN_YOTEI));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GAKUSYU", GetNullableInt(Global.TESURYO_GAKUSYU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GOSA", GetNullableInt(Global.TESURYO_GOSA));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYOSYUSYO", GetNullableInt(Global.RYOSYUSYO));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIN_KAISYACD", Global.SHIN_KAISYACD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSIN", Str2NullAndDec(Global.YOSIN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSHINRANK", Global.YOSHINRANK);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", GetNullableInt(Global.GAIKA));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSUKA", Global.TSUKA);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_F", Global.GAIKA_KEY_F);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_B", Global.GAIKA_KEY_B);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_1", GetNullableInt(Global.HIFURIKOZA_1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_2", GetNullableInt(Global.HIFURIKOZA_2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_3", GetNullableInt(Global.HIFURIKOZA_3));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_F", int.Parse(Global.GAI_F));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEGVAL", GetNullableInt(Global.TEGVAL));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", int.Parse(Global.GSSKBN));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HR_KIJYUN", Str2NullAndDec(Global.HR_KIJYUN));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU_F", GetNullableInt(Global.HORYU_F));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRORYUGAKU", Str2NullAndDec(Global.HRORYUGAKU));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRKBN", GetNullableInt(Global.HRKBN));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SF", GetNullableInt(Global.GAI_SF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SH", GetNullableInt(Global.GAI_SH));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_KZID", GetNullableInt(Global.GAI_KZID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_TF", GetNullableInt(Global.GAI_TF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_NAME", Global.ENG_NAME);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_ADDR", Global.ENG_ADDR);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_KZNO", Global.ENG_KZNO);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_SWIF", Global.ENG_SWIF);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKNAM", Global.ENG_BNKNAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BRNNAM", Global.ENG_BRNNAM);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKADDR", Global.ENG_BNKADDR);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));

                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    #region OLD
                    //Global.cCmdIns.Parameters.Clear();
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRKBN", Global.TRKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYAKU", Global.RYAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TORI_NAM", Global.TORI_NAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KNLD", Global.KNLD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TGASW", Global.TGASW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJSW", 0);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ZIP", Global.ZIP);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR1", Global.ADDR1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ADDR2", Global.ADDR2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEL", Global.TEL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FAX", Global.FAX);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBUSYO", Global.SBUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STANTO", Global.STANTO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEICD", Global.KEICD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STAN", Global.STAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SJBCD", Global.SJBCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SBCOD", Global.SBCOD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SKICD", Global.SKICD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NAYOSE", Global.NAYOSE);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SETUIN", Global.F_SETUIN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SHITU", Global.F_SHITU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_ZAN", Global.F_ZAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@F_SOUFU", Global.F_SOUFU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ANNAI", Global.ANNAI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSOKBN", Global.TSOKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU", Global.HORYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOVAL", Global.HOVAL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENSEN", Global.GENSEN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GOU", GetNullableInt(Global.GOU));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBN", GetNullableInt(Global.GGKBN));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GGKBNM", GetNullableString(Global.GGKBNM));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSKUBN", GetNullableInt(Global.GSKUBN));
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", Global.GSSKBN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SZEI", Global.SZEI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOSAI", Global.SOSAI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOKICD", Global.SOKICD);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", Global.GAIKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HEI_CD", Global.HEI_CD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM1", Global.DM1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM2 ", Global.DM2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DM3", Global.DM3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STYMD", Global.STYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@EDYMD", Global.EDYMD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@STFLG", Global.STFLG);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", Global.CDM1);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYZSKBN", Global.KYZSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYRSKBN", Global.KYRSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYGSKBN", Global.KYGSKBN);
                    ////DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KYKSKBN", Global.KYKSKBN);
                    ////**>>
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", Global.CDM2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CD03", Global.CD03);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", Global.IDM1);
                    ////**<<

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRFURI", Global.TRFURI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GRPID", Global.GRPID);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN", Global.SAIKEN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIKEN_FLG", Global.SAIKEN_FLG);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU", Global.SAIMU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SAIMU_FLG", Global.SAIMU_FLG);


                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMAIL", Global.TRMAIL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRURL", Global.TRURL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BIKO", Global.BIKO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@E_TANTOCD", Global.E_TANTOCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MYNO_AITE", Global.MYNO_AITE);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SRYOU_F", Global.SRYOU_F);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TOKUKANA", Global.TOKUKANA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUTAN", Global.FUTAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYU", Global.KAISYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YAKUJO", Global.YAKUJYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIME", Global.SHIME);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUHI", Global.KAISYUHI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KAISYUSIGHT", Global.KAISYUSIGHT);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_KINGAKU", Global.Y_KINGAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HOLIDAY", Global.HOLIDAY);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MIMAN", Global.MIMAN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_1", Global.IJOU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_1", Global.BUNKATSU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_1", Global.HASU_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_1", Global.SIGHT_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_2", Global.IJOU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_2", Global.BUNKATSU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_2", Global.HASU_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_2", Global.SIGHT_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IJOU_3", Global.IJOU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BUNKATSU_3", Global.BUNKATSU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HASU_3", Global.HASU_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIGHT_3", Global.SIGHT_3);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_GINKOCD", Global.SEN_GINKOCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SITENCD", Global.SEN_SITENCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_SHITENMEI", Global.KASO_SITENNM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKINSYU", Global.YOKINSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SEN_KOZANO", Global.SEN_KOZANO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@JIDOU_GAKUSYU", Global.JIDOU_GAKUSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@NYUKIN_YOTEI", Global.NYUKIN_YOTEI);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GAKUSYU", Global.TESURYO_GAKUSYU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESURYO_GOSA", Global.TESURYO_GOSA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RYOSYUSYO", Global.RYOSYUSYO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHIN_KAISYACD", Global.SHIN_KAISYACD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSIN", Global.YOSIN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOSHINRANK", Global.YOSHINRANK);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA", Global.GAIKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TSUKA", Global.TSUKA);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_F", Global.GAIKA_KEY_F);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAIKA_KEY_B", Global.GAIKA_KEY_B);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_1", Global.HIFURIKOZA_1);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_2", Global.HIFURIKOZA_2);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HIFURIKOZA_3", Global.HIFURIKOZA_3);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_F", Global.GAI_F);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TEGVAL", Global.TEGVAL);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GSSKBN", Global.GSSKBN);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HR_KIJYUN", Global.HR_KIJYUN);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HORYU_F", Global.HORYU_F);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRORYUGAKU", Global.HRORYUGAKU);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HRKBN", Global.HRKBN);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SF", Global.GAI_SF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_SH", Global.GAI_SH);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_KZID", Global.GAI_KZID);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GAI_TF", Global.GAI_TF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_NAME", Global.ENG_NAME);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_ADDR", Global.ENG_ADDR);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_KZNO", Global.ENG_KZNO);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_SWIF", Global.ENG_SWIF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKNAM", Global.ENG_BNKNAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BRNNAM", Global.ENG_BRNNAM);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@ENG_BNKADDR", Global.ENG_BNKADDR);

                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    ////if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    //{
                    //    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    //}
                    #endregion                    
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nInsert_SS_TORI_Full　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        private object GetNullableInt(string value)
        {
            int result = 0;
            if (!int.TryParse(value, out result)) { return DBNull.Value; }
            return result;
        }
        private object GetNullableString(string value)
        {
            if (string.IsNullOrEmpty(value)) { return DBNull.Value; }
            return value;
        }
        //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
        private object Str2NullAndDec(string str)
        {
            decimal result = 0;
            if (string.IsNullOrEmpty(str))
            {
                return DBNull.Value;
            }
            else
            {
                decimal oDec;
                return decimal.TryParse(str, out oDec) ? oDec : 0;
            }
            return result;
        }
        //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

        /// <summary>
        /// データ登録：取引先支払方法
        /// </summary>
        public void Insert_SS_TSHOH(int iInsUpdFlg)
        {
            try
            {
                if (iInsUpdFlg == 0)
                {
                    //取引先支払方法の登録SQL生成&実行
                    Global.cCmdIns.CommandText = "INSERT INTO SS_TSHOH "
                                               + "(TRCD, HJCD, SHO_ID, BCOD, JBCD, KICD, SJBCD, SHINO, HARAI_H, KIJITU_H, "
                                               + "SI_KUBN1, OWNID1, SI_KUBN2, OWNID2, SI_KUBN3, OWNID3, SI_KUBN4, OWNID4, SI_KUBN5, OWNID5, "
                                               + "Y_SHIHARAIMM, Y_SIHARAIDD, Y_SKIJITUMM, Y_SKIJITUDD, "
                                               + "CDM1, CDM2, CDM3, CDM4, IDM1, IDM2, IDM3, IDM4, FMOD, FTIM, FUSR, FWAY, LMOD, LTIM, LUSR, LWAY) "
                                               //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                                               + "VALUES (:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p,"
                                               + " :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p)";
                                               //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHO_ID", int.Parse(Global.SHO_ID_tb1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BCOD", Global.BCOD_tb1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@JBCD", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KICD", Global.KICD_tb1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SJBCD", Global.SJBCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHINO", GetNullableInt(Global.SHINO_tb1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HARAI_H", int.Parse(Global.HARAI_H_tb1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KIJITU_H", int.Parse(Global.KIJITU_H_tb1));

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN1", GetNullableInt(Global.KUBN1_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID1", GetNullableInt(Global.OWNID1_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN2", GetNullableInt(Global.KUBN2_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWINID2", GetNullableInt(Global.OWNID2_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN3", GetNullableInt(Global.KUBN3_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID3", (Global.OWNID2_tb3 == null) ? DBNull.Value : GetNullableInt(Global.OWNID3_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN4", GetNullableInt(Global.KUBN4_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID4", (Global.OWNID2_tb3 == null) ? DBNull.Value : GetNullableInt(Global.OWNID4_tb3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SIKUBN5", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID5", DBNull.Value);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SHIHARAIMM", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SIHARAIDD", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SKIJITUMM", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SKIJITUDD", 0);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM3", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM4", DBNull.Value);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM2", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM3", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM4", 0);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                    #region OLD
                    //StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("VALUES (");
                    //sb.AppendLine("'" + Global.TRCD + "'" + ",");
                    //sb.AppendLine("'" + Global.HJCD + "'" + ",");
                    //sb.AppendLine("'" + Global.SHO_ID_tb1 + "'" + ",");
                    //sb.AppendLine("'" + Global.BCOD_tb1 + "'" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("'" + Global.KICD_tb1 + "'" + ",");
                    //sb.AppendLine("'" + Global.SJBCD + "'" + ",");
                    //sb.AppendLine("'" + Global.SHINO_tb1 + "'" + ",");
                    //sb.AppendLine("'" + Global.HARAI_H_tb1 + "'" + ",");
                    //sb.AppendLine("'" + Global.KIJITU_H_tb1 + "'" + ",");

                    //sb.AppendLine((Global.KUBN1_tb3 == null ? "null" : "'" + Global.KUBN1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.OWNID1_tb3 == null ? "null" : "'" + Global.OWNID1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN2_tb3 == null ? "null" : "'" + Global.KUBN2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.OWNID2_tb3 == null ? "null" : "'" + Global.OWNID2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN3_tb3 == null ? "null" : "'" + Global.KUBN3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.OWNID2_tb3 == null ? "null" : "'" + Global.OWNID3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN4_tb3 == null ? "null" : "'" + Global.KUBN4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.OWNID2_tb3 == null ? "null" : "'" + Global.OWNID4_tb3 + "'") + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");

                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    ////sb.AppendLine((Global.SHIHARAIMM_tb1 == null ? "null" : Global.SHIHARAIMM_tb1) + ",");
                    ////sb.AppendLine((Global.SIHARAIDD_tb1 == null ? "null" : Global.SIHARAIDD_tb1) + ",");
                    ////sb.AppendLine((Global.SKIJITUMM_tb1 == null ? "null" : Global.SKIJITUMM_tb1) + ",");
                    ////sb.AppendLine((Global.SKIJITUDD_tb1 == null ? "null" : Global.SKIJITUDD_tb1) + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("'" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")) + "'" + ",");
                    //sb.AppendLine("'" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")) + "'" + ",");
                    //sb.AppendLine("'" + Convert.ToString(Global.nUcod) + "'" + ",");
                    //sb.AppendLine("0" + ",");
                    //sb.AppendLine("'" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")) + "'" + ",");
                    //sb.AppendLine("'" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")) + "'" + ",");
                    //sb.AppendLine("'" + Convert.ToString(Global.nUcod) + "'" + ",");
                    //sb.AppendLine("0");
                    //sb.AppendLine((Global.KUBN1_tb3 == null ? "null" : "'" + Global.KUBN1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.BANK1_tb3 == null ? "null" : "'" + Global.BANK1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.SITEN1_tb3 == null ? "null" : "'" + Global.SITEN1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZA1_tb3 == null ? "null" : "'" + Global.KOZA1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZANO1_tb3 == null ? "null" : "'" + Global.KOZANO1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.IRAININ1_tb3 == null ? "null" : "'" + Global.IRAININ1_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN2_tb3 == null ? "null" : "'" + Global.KUBN2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.BANK2_tb3 == null ? "null" : "'" + Global.BANK2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.SITEN2_tb3 == null ? "null" : "'" + Global.SITEN2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZA2_tb3 == null ? "null" : "'" + Global.KOZA2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZANO2_tb3 == null ? "null" : "'" + Global.KOZANO2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.IRAININ2_tb3 == null ? "null" : "'" + Global.IRAININ2_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN3_tb3 == null ? "null" : "'" + Global.KUBN3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.BANK3_tb3 == null ? "null" : "'" + Global.BANK3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.SITEN3_tb3 == null ? "null" : "'" + Global.SITEN3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZA3_tb3 == null ? "null" : "'" + Global.KOZA3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZANO3_tb3 == null ? "null" : "'" + Global.KOZANO3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.IRAININ3_tb3 == null ? "null" : "'" + Global.IRAININ3_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KUBN4_tb3 == null ? "null" : "'" + Global.KUBN4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.BANK4_tb3 == null ? "null" : "'" + Global.BANK4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.SITEN4_tb3 == null ? "null" : "'" + Global.SITEN4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZA4_tb3 == null ? "null" : "'" + Global.KOZA4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.KOZANO4_tb3 == null ? "null" : "'" + Global.KOZANO4_tb3 + "'") + ",");
                    //sb.AppendLine((Global.IRAININ4_tb3 == null ? "null" : "'" + Global.IRAININ4_tb3 + "'") + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null" + ",");
                    //sb.AppendLine("null");

                    //sb.AppendLine(")");
                    //Global.cCmdIns.CommandText += sb.ToString();
                    //// 空⇒null変換
                    //Global.cCmdIns.CommandText = Global.cCmdIns.CommandText.Replace("''", "null");
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    #endregion                                        
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】                    
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else if (iInsUpdFlg == 1)
                {
                    //取引先支払方法の更新SQL生成&実行(パラメータ渡しでエラーになってしまう為直結してます。)
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    #region OLD
                    //string sSI_KUBN1 = (Global.KUBN1_tb3 == null ? "null" : "'" + Global.KUBN1_tb3 + "'");
                    //string sOWNID1 = (Global.OWNID1_tb3 == null ? "null" : "'" + Global.OWNID1_tb3 + "'");
                    //string sSI_KUBN2 = (Global.KUBN2_tb3 == null ? "null" : "'" + Global.KUBN2_tb3 + "'");
                    //string sOWNID2 = (Global.OWNID2_tb3 == null ? "null" : "'" + Global.OWNID2_tb3 + "'");
                    //string sSI_KUBN3 = (Global.KUBN3_tb3 == null ? "null" : "'" + Global.KUBN3_tb3 + "'");
                    //string sOWNID3 = (Global.OWNID3_tb3 == null ? "null" : "'" + Global.OWNID3_tb3 + "'");
                    //string sSI_KUBN4 = (Global.KUBN4_tb3 == null ? "null" : "'" + Global.KUBN4_tb3 + "'");
                    //string sOWNID4 = (Global.OWNID4_tb3 == null ? "null" : "'" + Global.OWNID4_tb3 + "'");

                    //string sSHIHARAIMM = (Global.SHIHARAIMM_tb1 == null ? "null" : Global.SHIHARAIMM_tb1);
                    //string sSIHARAIDD = (Global.SIHARAIDD_tb1 == null ? "null" : Global.SIHARAIDD_tb1);
                    //string sSKIJITUMM = (Global.SKIJITUMM_tb1 == null ? "null" : Global.SKIJITUMM_tb1);
                    //string sSKIJITUDD = (Global.SKIJITUDD_tb1 == null ? "null" : Global.SKIJITUDD_tb1);

                    //Global.cCmdIns.CommandText = "UPDATE SS_TSHOH SET BCOD = '" + Global.BCOD_tb1
                    //                           + "', KICD = '" + Global.KICD_tb1
                    //                           + "', SHINO = '" + Global.SHINO_tb1
                    //                           + "', HARAI_H = " + Global.HARAI_H_tb1
                    //                           + ", KIJITU_H = " + Global.KIJITU_H_tb1
                    //                           + ", LMOD = '" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd"))
                    //                           + "', LTIM = '" + DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff"))
                    //                           + "', LUSR = '" + Global.nUcod + "', LWAY = 0, "
                    //                           + "SI_KUBN1 = " + sSI_KUBN1
                    //                           + ", OWNID1 = " + sOWNID1
                    //                           + ", SI_KUBN2 = " + sSI_KUBN2
                    //                           + ", OWNID2 = " + sOWNID2
                    //                           + ", SI_KUBN3 = " + sSI_KUBN3
                    //                           + ", OWNID3 = " + sOWNID3
                    //                           + ", SI_KUBN4 = " + sSI_KUBN4
                    //                           + ", OWNID4 = " + sOWNID4
                    //                           + ", Y_SHIHARAIMM = 0"
                    //                           + ", Y_SIHARAIDD = 0"
                    //                           + ", Y_SKIJITUMM = 0"
                    //                           + ", Y_SKIJITUDD = 0"
                    //               //+ ", Y_SHIHARAIMM = " + sSHIHARAIMM
                    //               //+ ", Y_SIHARAIDD = " + sSIHARAIDD
                    //               //+ ", Y_SKIJITUMM = " + sSKIJITUMM
                    //               //+ ", Y_SKIJITUDD = " + sSKIJITUDD
                    //                           + " WHERE TRCD = '" + Global.TRCD + "' AND HJCD = '" + Global.HJCD + "' AND SHO_ID = '" + Global.SHO_ID_tb1 + "' ";
                    //// 空⇒null変換
                    //Global.cCmdIns.CommandText = Global.cCmdIns.CommandText.Replace("''", "null");
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    #endregion

                    string sSI_KUBN1 = (Global.KUBN1_tb3 == null ? "null" : Global.KUBN1_tb3);
                    string sOWNID1 = (Global.OWNID1_tb3 == null ? "null" : Global.OWNID1_tb3);
                    string sSI_KUBN2 = (Global.KUBN2_tb3 == null ? "null" : Global.KUBN2_tb3);
                    string sOWNID2 = (Global.OWNID2_tb3 == null ? "null" : Global.OWNID2_tb3);
                    string sSI_KUBN3 = (Global.KUBN3_tb3 == null ? "null" : Global.KUBN3_tb3);
                    string sOWNID3 = (Global.OWNID3_tb3 == null ? "null" : Global.OWNID3_tb3);
                    string sSI_KUBN4 = (Global.KUBN4_tb3 == null ? "null" : Global.KUBN4_tb3);
                    string sOWNID4 = (Global.OWNID4_tb3 == null ? "null" : Global.OWNID4_tb3);

                    string sSHIHARAIMM = (Global.SHIHARAIMM_tb1 == null ? "null" : Global.SHIHARAIMM_tb1);
                    string sSIHARAIDD = (Global.SIHARAIDD_tb1 == null ? "null" : Global.SIHARAIDD_tb1);
                    string sSKIJITUMM = (Global.SKIJITUMM_tb1 == null ? "null" : Global.SKIJITUMM_tb1);
                    string sSKIJITUDD = (Global.SKIJITUDD_tb1 == null ? "null" : Global.SKIJITUDD_tb1);

                    Global.cCmdIns.CommandText = @"UPDATE SS_TSHOH SET BCOD = :p , KICD = :p , SHINO = :p, HARAI_H = :p , KIJITU_H = :p, LMOD = :p, LTIM = :p, LUSR = :p, LWAY = :p
                                               ,SI_KUBN1 = :p , OWNID1 = :p, SI_KUBN2 = :p , OWNID2 = :p , SI_KUBN3 = :p , OWNID3 = :p , SI_KUBN4 = :p , OWNID4 = :p 
                                               , Y_SHIHARAIMM = :p, Y_SIHARAIDD = :p, Y_SKIJITUMM = :p, Y_SKIJITUDD = :p
                                                WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p";

                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BCOD", Global.BCOD_tb1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KICD", Global.KICD_tb1);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHINO", GetNullableInt(Global.SHINO_tb1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HARAI_H", int.Parse(Global.HARAI_H_tb1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KIJITU_H", int.Parse(Global.KIJITU_H_tb1));
                    
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN1", GetNullableInt(sSI_KUBN1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID1", GetNullableInt(sOWNID1));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN2", GetNullableInt(sSI_KUBN2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID2", GetNullableInt(sOWNID2));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN3", GetNullableInt(sSI_KUBN3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID3", GetNullableInt(sOWNID3));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SI_KUBN4", GetNullableInt(sSI_KUBN4));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@OWNID4", GetNullableInt(sOWNID4));
                    
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SHIHARAIMM", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SIHARAIDD", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SKIJITUMM", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@Y_SKIJITUDD", 0);
                    
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SHO_ID", int.Parse(Global.SHO_ID_tb1));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else
                {
                    Global.cCmdDel.CommandText = "DELETE FROM SS_TSHOH WHERE TRCD = '" + Global.TRCD + "' AND HJCD = " + int.Parse(Global.HJCD);//Global.HJCD;//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】】
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdDel);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdDel);
                    Global.cCmdDel.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nInsert_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// データ登録：振込先銀行
        /// </summary>
        public void Insert_SS_FRIGIN(int iInsUpdFlg)
        {
            try
            {
                if (iInsUpdFlg == 0)
                {
                    //振込先銀行の登録SQL生成&実行
                    Global.cCmdIns.CommandText = "INSERT INTO SS_FRIGIN "
                                               + "(TRCD, HJCD, GIN_ID, BANK_CD, SITEN_ID, YOKIN_TYP, KOUZA, MEIGI, MEIGIK, TESUU, SOUKIN, GENDO, KVAL, "
                                               + "CDM1, CDM2, CDM3, CDM4, IDM1, IDM2, IDM3, IDM4, FMOD, FTIM, FUSR, FWAY, LMOD, LTIM, LUSR, LWAY, FDEF, FTESUID, DDEF, DTESUSW, DTESU) "
                                               //---> V02.01.01 HWPO UDPATE ▼【PostgreSQL対応】
                                               //+ "VALUES (:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, null, null, null, null, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                                               + "VALUES (:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                                               //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GIN_ID", Global.GIN_ID_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GIN_ID", int.Parse(Global.GIN_ID_tb2));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BANK_CD", Global.BANK_CD_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SITEN_ID", Global.SITEN_ID_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKIN_TYP", Global.YOKIN_TYP_tb2);
                    // ▼#115527 2022/06/16 竹内
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KOUZA", Global.KOUZA_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KOUZA", Global.KOUZA_tb2.PadLeft(7, '0'));
                    // ▲#115527 2022/06/16 竹内
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MEIGI", Global.MEIGI_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MEIGIK", Global.MEIGIK_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESUU", int.Parse(Global.TESUU_tb2));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOUKIN", Global.SOUKIN_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENDO", Convert.ToInt64(Global.GENDO_tb2));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KVAL", 0);
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】コメントを外す
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM1", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM2", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM3", DBNull.Value);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDM4", DBNull.Value);
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM1", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM2", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM3", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDM4", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);

                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FDEF", Global.FDEF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTESUID", Global.FTESUID);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DDEF", Global.DDEF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESUSW", Global.DTESUSW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESU", Global.DTESU);
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FDEF", int.Parse(Global.FDEF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTESUID", int.Parse(Global.FTESUID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DDEF", int.Parse(Global.DDEF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESUSW", int.Parse(Global.DTESUSW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESU", GetNullableInt(Global.DTESU));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】                    
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else if (iInsUpdFlg == 1)
                {
                    //振込先銀行の更新SQL生成&実行
                    Global.cCmdIns.CommandText = "UPDATE SS_FRIGIN SET BANK_CD = :p, SITEN_ID = :p, YOKIN_TYP = :p, KOUZA = :p, MEIGI = :p, MEIGIK= :p, "
                                               + "TESUU = :p, SOUKIN = :p, GENDO = :p, LMOD = :p, LTIM = :p, LUSR = :p, LWAY = :p, "
                                               + "FDEF = :p, FTESUID = :p, DDEF = :p, DTESUSW = :p, DTESU = :p "
                                               + "WHERE TRCD = :p AND HJCD = :p AND GIN_ID = :p ";
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@BANK_CD", Global.BANK_CD_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SITEN_ID", Global.SITEN_ID_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@YOKIN_TYP", Global.YOKIN_TYP_tb2);
                    // ▼#115527 2022/06/16 竹内
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KOUZA", Global.KOUZA_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KOUZA", Global.KOUZA_tb2.PadLeft(7, '0'));
                    // ▲#115527 2022/06/16 竹内
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MEIGI", Global.MEIGI_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@MEIGIK", Global.MEIGIK_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TESUU", int.Parse(Global.TESUU_tb2));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@SOUKIN", Global.SOUKIN_tb2);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GENDO", Convert.ToInt64(Global.GENDO_tb2));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", DbCls.GetNumNullZero<int>(Global.dNow.ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", DbCls.GetNumNullZero<int>(Global.dNow.ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);

                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FDEF", Global.FDEF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTESUID", Global.FTESUID);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DDEF", Global.DDEF);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESUSW", Global.DTESUSW);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESU", Global.DTESU);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FDEF", int.Parse(Global.FDEF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTESUID", int.Parse(Global.FTESUID));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DDEF", int.Parse(Global.DDEF));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESUSW", int.Parse(Global.DTESUSW));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@DTESU", GetNullableInt(Global.DTESU));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GIN_ID", Global.GIN_ID_tb2);
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@GIN_ID", int.Parse(Global.GIN_ID_tb2));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】                    
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else
                {
                    Global.cCmdDel.CommandText = "DELETE FROM SS_FRIGIN WHERE TRCD = '" + Global.TRCD + "' AND HJCD = " + int.Parse(Global.HJCD);//Global.HJCD;//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdDel);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdDel);
                    Global.cCmdDel.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nInsert_SS_FRIGIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        public void Update_SS_FRIGIN_FDEF()
        {
            try
            {
                // 2024/09/10 Postgres16対応 垣内▼
                //Global.cCmdIns.CommandText = "UPDATE SS_FRIGIN SET FDEF = 0"
                Global.cCmdIns.CommandText = "UPDATE SS_FRIGIN SET FDEF = 0 "
                // 2024/09/10 Postgres16対応 垣内▲
                                           + "WHERE TRCD = :p AND HJCD = :p AND FDEF = 1";
                Global.cCmdIns.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                Global.cCmdIns.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nUpdate_SS_FRGIN_FDEF　Ver" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        public void Update_SS_FRIGIN_DDEF()
        {
            try
            {
                // 2024/09/10 Postgres16対応 垣内▼
                //Global.cCmdIns.CommandText = "UPDATE SS_FRIGIN SET DDEF = 0"
                Global.cCmdIns.CommandText = "UPDATE SS_FRIGIN SET DDEF = 0 "
                // 2024/09/10 Postgres16対応 垣内▲
                                           + "WHERE TRCD = :p AND HJCD = :p AND DDEF = 1";
                Global.cCmdIns.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", Global.HJCD);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@HJCD", int.Parse(Global.HJCD));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                Global.cCmdIns.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nUpdate_SS_FRIGIN_DDEF　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        #endregion



        #region 削除
        /// <summary>
        /// データ削除：取引先
        /// </summary>
        public void Del_SS_TORI(string sTRCD, string sHJCD)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                //取引先マスタの削除SQL生成&実行
                Global.cCmdDel.CommandText = "DELETE FROM SS_TORI WHERE TRCD = :p AND HJCD = :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                Global.cCmdDel.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nDel_SS_TORI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
        }


        /// <summary>
        /// データ削除：取引先支払方法
        /// </summary>
        public void Del_SS_TSHOH_ALL(string sTRCD, string sHJCD)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                //取引先支払方法の削除SQL生成&実行
                Global.cCmdDel.CommandText = "DELETE FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                Global.cCmdDel.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nDel_SS_TSHOH_ALL　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
        }


        /// <summary>
        /// データ削除：振込先銀行
        /// </summary>
        public void Del_SS_FRIGIN_ALL(string sTRCD, string sHJCD)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                //振込先銀行の削除SQL生成&実行
                Global.cCmdDel.CommandText = "DELETE FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                Global.cCmdDel.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/11 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nDel_SS_FRIGIN_ALL　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/11>
            }
        }


        /// <summary>
        /// データ削除：取引先支払方法
        /// </summary>
        public void Del_SS_TSHOH(string sTRCD, string sHJCD, string sSHOID)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                //取引先支払方法の削除SQL生成&実行
                Global.cCmdDel.CommandText = "DELETE FROM SS_TSHOH WHERE TRCD = :p AND HJCD = :p AND SHO_ID = :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@SHO_ID", sSHOID);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@SHO_ID", int.Parse(sSHOID));
                //<--- V02.01.01 HWPO UDPATE ▲【PostgreSQL対応】               
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                Global.cCmdDel.ExecuteNonQuery();

                Global.cCmdDel.CommandText = "UPDATE SS_TSHOH SET SHO_ID = (SHO_ID - 1) WHERE TRCD = :p AND HJCD = :p AND SHO_ID > :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@SHO_ID", sSHOID);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@SHO_ID", int.Parse(sSHOID));                
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdDel);
                Global.cCmdDel.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nDel_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// データ削除：振込先銀行
        /// </summary>
        public void Del_SS_FRIGIN(string sTRCD, string sHJCD, string sGINID)
        {
            try
            {
                sTRCD = GetTrcdDB(sTRCD);
                //振込先銀行の削除SQL生成&実行
                Global.cCmdDel.CommandText = "DELETE FROM SS_FRIGIN WHERE TRCD = :p AND HJCD = :p AND GIN_ID = :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@GIN_ID", sGINID);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@GIN_ID", int.Parse(sGINID));                
                //<--- V02.01.01 HWPO DELETE ▲【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                Global.cCmdDel.ExecuteNonQuery();

                Global.cCmdDel.CommandText = "UPDATE SS_FRIGIN SET GIN_ID = (GIN_ID - 1) WHERE TRCD = :p AND HJCD = :p AND GIN_ID > :p ";
                Global.cCmdDel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@TRCD", sTRCD);
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", sHJCD);
                //DbCls.AddParamaterByValue(ref Global.cCmdDel, "@GIN_ID", sGINID);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@HJCD", int.Parse(sHJCD));
                DbCls.AddParamaterByValue(ref Global.cCmdDel, "@GIN_ID", int.Parse(sGINID));
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdDel);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdDel);
                Global.cCmdDel.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nDel_SS_FRIGIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        #endregion



        #region 初期化関連
        /// <summary>
        /// Globalの初期化
        /// </summary>
        public void Init_DispVal()
        {
            Global.TRCD = "";
            Global.HJCD = "";
            Global.TRKBN = "0";
            Global.RYAKU = "";
            Global.TORI_NAM = "";
            Global.KNLD = "";
            Global.TGASW = "0";
            Global.ZIP = "";
            Global.ADDR1 = "";
            Global.ADDR2 = "";
            Global.TEL = "";
            Global.FAX = "";
            Global.SBUSYO = "";
            Global.STANTO = "";
            Global.KEICD = "1";
            Global.STAN = "";
            Global.SJBCD = "";
            Global.SBCOD = "";
            Global.SKICD = "";
            Global.NAYOSE = "1";
            //Global.F_SETUIN = "0";
            Global.F_SETUIN = "1";
            //**>>ICS-S 2013/03/04
            //**Global.F_SHITU = "0";
            Global.F_SHITU = "1";
            //**<<ICS-E
            //Global.F_ZAN = "";
            Global.F_ZAN = "0";
            Global.F_SOUFU = "0";
            Global.ANNAI = "1";
            Global.TSOKBN = "1";
            Global.HORYU = "0";
//-- <2016/03/14>
//            Global.HOVAL = "100.0";
            Global.HOVAL = "100.000M";
//-- <2016/03/14>
            //Global.HOKKBN = "";
            Global.HOKKBN = "0";
            Global.HODM1 = "";
            Global.KAIIN = "0";
            Global.KYKAI = "0";
            Global.KYVAL = "0";
            Global.KYCAL = "0";
            Global.KYZAF = "0";
            Global.KYZVL = "0";
            Global.KYZRT = "0.0";
            Global.KYZAH = "9";
            //Global.KYZAS = "";
            Global.KYZAS = "0";
            Global.KYROF = "0";
            Global.KYRVL = "0";
            Global.KYRRT = "0.0";
            Global.KYROH = "9";
            //Global.KYROS = "";
            Global.KYROS = "0";
            Global.KYGAF = "0";
            Global.KYGVL = "0";
            Global.KYGRT = "0.0";
            Global.KYGAH = "9";
            //Global.KYGAS = "";
            Global.KYGAS = "0";
            Global.KYKEF = "0";
            Global.KYKVL = "0";
            Global.KYKRT = "0.0";
            Global.KYKEH = "9";
            //Global.KYKES = "";
            Global.KYKES = "0";
            Global.GENSEN = "0";
            Global.GOU = "";
            Global.GGKBN = "";
            Global.GGKBNM = "";
//-- <2016/03/14 初期値変更>
//            Global.GSKUBN = "29";
            Global.GSKUBN = "";
//-- <2016/03/14>
            //Global.GSSKBN = "";
            switch (Global.nHSSW)
            {
                case 0:
                    Global.SZEI = "0";
                    break;
                case 1:
                    Global.SZEI = "2";
                    break;
                case 2:
                    Global.SZEI = "1";
                    break;
            }
            //Global.SOSAI = "";
            Global.SOSAI = "0";
            Global.SOKICD = "";
            //Global.GAIKA = "";
            Global.GAIKA = "0";
            Global.HEI_CD = "";
            Global.DM1 = "";
            Global.DM2 = "";
            Global.DM3 = "";
            Global.STYMD = "";
            Global.EDYMD = "";
            Global.ZSTYMD = "____/__/__";
            Global.ZEDYMD = "____/__/__";
            Global.STFLG = "0";
            Global.CDM1 = "";
//-- < 更新者のクリア>
            Global.LUSR = "";
//-- <>
            Global.LMOD = "";
            Global.KYZSKBN = "";
            Global.KYRSKBN = "";
            Global.KYGSKBN = "";
            Global.KYKSKBN = "";
            Global.SHO_ID_tb1 = "";
            Global.BCOD_tb1 = "";
            Global.KICD_tb1 = "";
            Global.SHINO_tb1 = "";
            Global.SIMEBI_tb1 = "";
            Global.SHIHARAIMM_tb1 = "";
            Global.SIHARAIDD_tb1 = "";
            Global.HARAI_H_tb1 = "0";
            Global.SKIJITUMM_tb1 = "";
            Global.SKIJITUDD_tb1 = "";
            Global.KIJITU_H_tb1 = "0";
            Global.SKBNCOD_tb1 = "";
            Global.V_YAKUJO_tb1 = "";
            Global.YAKUJOA_L_tb1 = "";
            Global.YAKUJOA_M_tb1 = "";
            Global.YAKUJOB_LH_tb1 = "";
            Global.YAKUJOB_H1_tb1 = "";
            Global.YAKUJOB_R1_tb1 = "";
            Global.YAKUJOB_U1_tb1 = "";
            Global.YAKUJOB_S1_tb1 = "";
            Global.YAKUJOB_H2_tb1 = "";
            Global.YAKUJOB_R2_tb1 = "";
            Global.YAKUJOB_U2_tb1 = "";
            Global.YAKUJOB_S2_tb1 = "";
            Global.YAKUJOB_H3_tb1 = "";
            Global.YAKUJOB_R3_tb1 = "";
            Global.YAKUJOB_U3_tb1 = "";
            Global.YAKUJOB_S3_tb1 = "";
            Global.GIN_ID_tb2 = "";
            Global.BANK_CD_tb2 = "";
            Global.SITEN_ID_tb2 = "";
            Global.YOKIN_TYP_tb2 = "1";
            Global.KOUZA_tb2 = ""; // "0";
            Global.MEIGI_tb2 = "";
            Global.MEIGIK_tb2 = "";
            Global.TESUU_tb2 = "1";
            Global.SOUKIN_tb2 = "7";
            Global.GENDO_tb2 = "";
            Global.SHO_ID_tb3 = "";
            Global.BCOD_tb3 = "";
            Global.KICD_tb3 = "";
            Global.SHINO_tb3 = "";
            Global.KUBN1_tb3 = "";
            Global.BANK1_tb3 = "";
            Global.SITEN1_tb3 = "";
            Global.KOZA1_tb3 = "";
            Global.KOZANO1_tb3 = "";
            Global.IRAININ1_tb3 = "";
            Global.KUBN2_tb3 = "";
            Global.BANK2_tb3 = "";
            Global.SITEN2_tb3 = "";
            Global.KOZA2_tb3 = "";
            Global.KOZANO2_tb3 = "";
            Global.IRAININ2_tb3 = "";
            Global.KUBN3_tb3 = "";
            Global.BANK3_tb3 = "";
            Global.SITEN3_tb3 = "";
            Global.KOZA3_tb3 = "";
            Global.KOZANO3_tb3 = "";
            Global.IRAININ3_tb3 = "";
            Global.KUBN4_tb3 = "";
            Global.BANK4_tb3 = "";
            Global.SITEN4_tb3 = "";
            Global.KOZA4_tb3 = "";
            Global.KOZANO4_tb3 = "";
            Global.IRAININ4_tb3 = "";
            //Global.KUBN5_tb3 = "";
            //Global.BANK5_tb3 = "";
            //Global.SITEN5_tb3 = "";
            //Global.KOZA5_tb3 = "";
            //Global.KOZANO5_tb3 = "";
            //Global.IRAININ5_tb3 = "";
            //**>>ICS-S 2013/05/20
            Global.IDM1 = "0";
            Global.CDM2 = "";
            Global.CD03 = "";
            //**<<ICS-E

            Global.TRFURI = "";                  // 共通：フリガナ
            Global.SAIKEN = "0";                  // 共通：□得意先
            Global.SAIKEN_FLG = "0";              // 共通：□入金代表者
            Global.SAIMU = "0";                   // 共通：□仕入先
            Global.SAIMU_FLG = "0";               // 共通：□支払代表者
            Global.GRPID = "";                   // 共通：取引先グループ
            Global.GRPIDNM = "";                 // 共通：取引先グループ名
            Global.TRMAIL = "";                  // 基本情報：メールアドレス
            Global.TRURL = "";                   // 基本情報：ホームページ
            Global.BIKO = "";                    // 基本情報：備考
            Global.E_TANTOCD = "";               // 基本情報：営業担当者コード
            Global.E_TANTONM = "";               // 基本情報：営業担当者名
            Global.MYNO_AITE = "";               // 基本情報：マイナンバー　法人番号
            Global.SRYOU_F = "0";                 // 基本情報：相殺処理　□相殺領収書を発行する
            Global.TOKUKANA = "";                // 回収設定：入金消込設定　照合用フリガナ
//-- <2016/02/17 "">
//            Global.FUTAN = "0";                   // 回収設定：入金消込設定　手数料負担区分
//            Global.KAISYU = "0";                  // 回収設定：回収予定設定　回収方法
            Global.FUTAN = "";                   // 回収設定：入金消込設定　手数料負担区分
            Global.KAISYU = "";                  // 回収設定：回収予定設定　回収方法
//-- <2016/02/17>            
//-- <2016/02/08 初期値は0>
//            Global.YAKUJYO = "";                 // 回収設定：回収予定設定　約定を指定
            Global.YAKUJYO = "0";                 // 回収設定：回収予定設定　約定を指定
//--<2016/02/08>
            Global.SHIME = "";                   // 回収設定：回収予定設定　締日
            Global.KAISYUHI = "";                // 回収設定：回収予定設定　回収予定（MDD）
            Global.KAISYUSIGHT = "";             // 回収設定：回収予定設定　回収期日（MDD）
            Global.Y_KINGAKU = "";               // 回収設定：回収予定設定　約定金額
            Global.HOLIDAY = "";                 // 回収設定：回収予定設定　休業日設定
            Global.MIMAN = "";                   // 回収設定：回収予定設定　約定金額未満
            Global.IJOU_1 = "";                  // 回収設定：回収予定設定　約定金額以上①
            Global.BUNKATSU_1 = "";              // 回収設定：回収予定設定　分割①
            Global.HASU_1 = "";                  // 回収設定：回収予定設定　端数単位①
            Global.SIGHT_1 = "";                 // 回収設定：回収予定設定　回収サイト①
            Global.IJOU_2 = "";                  // 回収設定：回収予定設定　約定金額以上②
            Global.BUNKATSU_2 = "";              // 回収設定：回収予定設定　分割②
            Global.HASU_2 = "";                  // 回収設定：回収予定設定　端数単位②
            Global.SIGHT_2 = "";                 // 回収設定：回収予定設定　回収サイト②
            Global.IJOU_3 = "";                  // 回収設定：回収予定設定　約定金額以上③
            Global.BUNKATSU_3 = "";              // 回収設定：回収予定設定　分割③
            Global.HASU_3 = "";                  // 回収設定：回収予定設定　端数単位③
            Global.SIGHT_3 = "";                 // 回収設定：回収予定設定　回収サイト③
            Global.SEN_GINKOCD = "";             // 回収設定：専用入金口座　銀行コード
            Global.SEN_GINKONM = "";             // 回収設定：専用入金口座　銀行名
            Global.SEN_SITENCD = "";             // 回収設定：専用入金口座　支店コード
            Global.KASO_SITENCD = "";            // 回収設定：専用入金口座　仮想支店コード
            Global.KASO_SITENNM = "";            // 回収設定：専用入金口座　仮想支店名
            Global.YOKINSYU = "";                // 回収設定：専用入金口座　預金種別
            Global.SEN_KOZANO = "";              // 回収設定：専用入金口座　口座番号
//-- <2016/03/14 初期値変更>
//            Global.JIDOU_GAKUSYU = "0";           // 回収設定：各設定　□カナ自動学習
            Global.JIDOU_GAKUSYU = "1";           // 回収設定：各設定　□カナ自動学習
//-- <2016/03/14>
            Global.NYUKIN_YOTEI = "0";            // 回収設定：各設定　□入金予定利用
            Global.TESURYO_GAKUSYU = "0";         // 回収設定：各設定　□手数料自動学習する
//-- <2016/03/14 初期値変更>
//            Global.TESURYO_GOSA = "0";            // 回収設定：各設定　□手数料誤差利用する
            Global.TESURYO_GOSA = "1";            // 回収設定：各設定　□手数料誤差利用する
//-- <2016/03/14>
            Global.RYOSYUSYO = "0";               // 回収設定：各設定　□領収書発行する
            Global.SHIN_KAISYACD = "";           // 回収設定：各設定　信用調査用企業コード
            Global.YOSIN = "";                   // 回収設定：各設定　与信限度額
            Global.YOSHINRANK = "";              // 回収設定：各設定　与信ランク
            Global.TSUKA = "";                   // 回収設定：外貨関連　取引通貨
            Global.GAIKA_KEY_F = "";             // 回収設定：外貨関連　照合ｷｰ（前）
            Global.GAIKA_KEY_B = "";             // 回収設定：外貨関連　照合ｷｰ（後）
            Global.HIFURIKOZA_1 = "";            // 回収設定：被振込口座設定　被振込口座１（自社銀行キー）
            Global.HIBKCD_1 = "";                // 回収設定：被振込口座設定　被振込口座１（銀行コード）
            Global.HIBKNM_1 = "";                // 回収設定：被振込口座設定　被振込口座１（銀行名）
            Global.HIBRCD_1 = "";                // 回収設定：被振込口座設定　被振込口座１（支店コード）
            Global.HIBRNM_1 = "";                // 回収設定：被振込口座設定　被振込口座１（支店名）
            Global.HIYOKN_1 = "";                // 回収設定：被振込口座設定　被振込口座１（預金種別）
            Global.HIKOZA_1 = "";                // 回収設定：被振込口座設定　被振込口座１（口座番号）
            Global.HIFURIKOZA_2 = "";            // 回収設定：被振込口座設定　被振込口座２（自社銀行キー）
            Global.HIBKCD_2 = "";                // 回収設定：被振込口座設定　被振込口座２（銀行コード）
            Global.HIBKNM_2 = "";                // 回収設定：被振込口座設定　被振込口座２（銀行名）
            Global.HIBRCD_2 = "";                // 回収設定：被振込口座設定　被振込口座２（支店コード）
            Global.HIBRNM_2 = "";                // 回収設定：被振込口座設定　被振込口座２（支店名）
            Global.HIYOKN_2 = "";                // 回収設定：被振込口座設定　被振込口座２（預金種別）
            Global.HIKOZA_2 = "";                // 回収設定：被振込口座設定　被振込口座２（口座番号）
            Global.HIFURIKOZA_3 = "";            // 回収設定：被振込口座設定　被振込口座３（自社銀行キー）
            Global.HIBKCD_3 = "";                // 回収設定：被振込口座設定　被振込口座３（銀行コード）
            Global.HIBKNM_3 = "";                // 回収設定：被振込口座設定　被振込口座３（銀行名）
            Global.HIBRCD_3 = "";                // 回収設定：被振込口座設定　被振込口座３（支店コード）
            Global.HIBRNM_3 = "";                // 回収設定：被振込口座設定　被振込口座３（支店名）
            Global.HIYOKN_3 = "";                // 回収設定：被振込口座設定　被振込口座３（預金種別）
            Global.HIKOZA_3 = "";                // 回収設定：被振込口座設定　被振込口座３（口座番号）
            Global.GAI_F = "0";                  // 支払条件：◎取引区分
            Global.OWNID1_tb3 = "";              // 支払条件：口座ID1
            Global.OWNID2_tb3 = "";              // 支払条件：口座ID2
            Global.OWNID3_tb3 = "";              // 支払条件：口座ID3
            Global.OWNID4_tb3 = "";              // 支払条件：口座ID4
            Global.FDEF = "1";                    // 振込先情報：□初期値
            Global.DDEF = "0";                    // 振込先情報：□でんさい代表口座
            Global.FTESUID = "";                 // 振込先情報：銀行振込　手数料ID
            Global.DTESUSW = "0";                 // 振込先情報：全銀電子債権ネットワーク　□手数料設定を使用する
            Global.DTESU = "";                   // 振込先情報：全銀電子債権ネットワーク　手数料負担
            Global.TEGVAL = "";                  // その他情報：手形関連　送料
            Global.GSSKBN = "1";                  // その他情報：源泉税関連　計算基準
            Global.HR_KIJYUN = "";               // その他情報：控除関連　計算摘要基準額
            Global.HORYU_F = "";                 // その他情報：控除関連　計算区分フラグ
            Global.HRORYUGAKU = "";              // その他情報：控除関連　定額
            Global.HRKBN = "";                   // その他情報：控除関連　作成区分
            Global.GAI_SF = "0";                  // 外貨設定：◎送金種類
            Global.GAI_SH = "0";                  // 外貨設定：◎送金支払方法
            Global.GAI_KZID = "";                // 外貨設定：出金口座
//-- <2016/03/09 非選択項目>
//            Global.GAI_TF = "";                  // 外貨設定：手数料負担
            Global.GAI_TF = "1";                  // 外貨設定：手数料負担
//-- <2016/03/09>
            Global.ENG_NAME = "";                // 外貨設定：英語表記　受取人名
            Global.ENG_ADDR = "";                // 外貨設定：英語表記　住所
            Global.ENG_KZNO = "";                // 外貨設定：外国向け送金設定　口座番号
            Global.ENG_SWIF = "";                // 外貨設定：外国向け送金設定　SWIFTコード
            Global.ENG_BNKNAM = "";              // 外貨設定：外国向け送金設定　被仕向銀行名
            Global.ENG_BRNNAM = "";              // 外貨設定：外国向け送金設定　被仕向支店名
            Global.ENG_BNKADDR = "";             // 外貨設定：外国向け送金設定　被仕向銀行住所
        }
        #endregion



        #region 文字列変換関連
        //端数単位の変換
        public string Get_HasuUnit_NM(int pUnit)
        {
            //switch (pUnit)
            //{
            //    case 1:
            //        return "十万";
            //    case 2:
            //        return "万";
            //    case 3:
            //        return "千";
            //    case 4:
            //        return "百";
            //    case 5:
            //        return "十";
            //    case 6:
            //        return "一";
            //    case 9:
            //        return "端数";
            //}
            switch (pUnit)
            {
                case 0:
                    return "端数";
                case 1:
                    return "一";
                case 2:
                    return "十";
                case 3:
                    return "百";
                case 4:
                    return "千";
                case 5:
                    return "万";
                case 6:
                    return "十万";
            }
            return "";
        }
        public string Get_HasuUnit_NM_Saiken(int pUnit)
        {
            switch (pUnit)
            {
                case 0:
                    return "端数";
                case 1:
                    return "一";
                case 2:
                    return "十";
                case 3:
                    return "百";
                case 4:
                    return "千";
                case 5:
                    return "万";
                case 6:
                    return "十万";
            }
            return "";
        }

        //端数処理の変換
        public string Get_HasuShori_NM(int pShori)
        {
            switch (pShori)
            {
                case 0:
                    return "切り捨て";
                case 1:
                    return "四捨五入";
                case 2:
                    return "切り上げ";
            }
            return "";
        }

        //端数処理の変換
        public string Get_HasuShori_NM2(int pShori)
        {
            switch (pShori)
            {
                case 1:
                    return "切り捨て";
                case 2:
                    return "四捨五入";
                case 3:
                    return "切り上げ";
            }
            return "";
        }

        //支払通知発行区分の変換
        public string Get_HaraiTuuti_NM(int pShori)
        {
            switch (pShori)
            {
                case 0:
                    return "印刷しない";
                case 1:
                    return "印刷する";
            }
            return "";
        }

        //休日補正の変換
        public string Get_Hosei_NM(int pHosei)
        {
            switch (pHosei)
            {
                case 0:
                    return "前営業日";
                case 1:
                    return "当日";
                case 2:
                    return "後営業日";
            }
            return "";
        }

        //号の変換
        public string Get_Gou_NM(int pGou)
        {
            switch (pGou)
            {
                case 1:
                    return "原稿料・作曲料等";
                case 2:
                    return "弁護士・税理士等";
            }
            return "";
        }

        //協力会費計算区分の変換
        public string Get_Kycal_NM(int pKycal)
        {
            switch (pKycal)
            {
                case 0:
                    return "比率";
                case 1:
                    return "実額";
            }
            return "";
        }

        //預金種別の変換
        public string Get_YokinType_NM(int pYokinType)
        {
            switch (pYokinType)
            {
                case 1:
                    return "普通預金";
                case 2:
                    return "当座預金";
                case 4:
                    return "貯蓄";
                case 9:
                    return "その他";
            }
            return "";
        }

        public string Get_Sen_YokinType_NM(int pYokinType)
        {
            switch (pYokinType)
            {
                case 1:
                    return "普通預金";
                case 2:
                    return "当座預金";
                case 4:
                    return "貯蓄預金";
                case 5:
                    return "通知預金";
            }
            return "";
        }

        //手数料負担の変換
        public string Get_Tesuu_NM(int pTesuu)
        {
            switch (pTesuu)
            {
                case 1:
                    return "自社負担";
                case 2:
                    return "客先負担";
                case 3:
                    return "折半";
                case 4:
                    return "一律";
            }
            return "";
        }

        //送金区分の変換
        public string Get_Soukin_NM(int pSoukin)
        {
            switch (pSoukin)
            {
                case 7:
                    return "電信";
                case 8:
                    return "文書";
            }
            return "";
        }

        //送付案内出力有無の変換
        public string Get_FSoufu_NM(int pFSoufu)
        {
            switch (pFSoufu)
            {
                case 0:
                    return "送付しない";
                case 1:
                    return "書留";
                case 2:
                    return "簡易書留";
                case 3:
                    return "空白";
            }
            return "";
        }

        //案内文パターンの変換
        public string Get_Annai_NM(int pAnnai)
        {
            switch (pAnnai)
            {
                case 1:
                    return "パターン１";
                case 2:
                    return "パターン２";
            }
            return "";
        }

        //送料負担区分の変換
        public string Get_Tsokbn_NM(int pTsokbn)
        {
            switch (pTsokbn)
            {
                case 0:
                    return "来社";
                case 1:
                    return "自社負担";
                case 2:
                    return "客先負担";
            }
            return "";
        }

        //端数単位の変換
        public string Get_HasuUnit_CD(string pUnit)
        {
            //switch (pUnit)
            //{
            //    case "十万":
            //        return "1";
            //    case "万":
            //        return "2";
            //    case "千":
            //        return "3";
            //    case "百":
            //        return "4";
            //    case "十":
            //        return "5";
            //    case "一":
            //        return "6";
            //    case "端数":
            //        return "9";
            //}
            switch (pUnit)
            {
                case "十万":
                    return "6";
                case "万":
                    return "5";
                case "千":
                    return "4";
                case "百":
                    return "3";
                case "十":
                    return "2";
                case "一":
                    return "1";
                case "端数":
                    return "0";
            }
            return "";
        }

        //端数単位の変換
        public string Get_HasuUnit_CD2(string pUnit)
        {
            switch (pUnit)
            {
                case "1:十万":
                    return "1";
                case "2:万":
                    return "2";
                case "3:千":
                    return "3";
                case "4:百":
                    return "4";
                case "5:十":
                    return "5";
                case "6:一":
                    return "6";
                case "9:無し":
                    return "9";
            }
            return "";
        }

        //端数処理の変換
        public string Get_HasuShori_CD(string pShori)
        {
            switch (pShori)
            {
                case "0:切り捨て":
                    return "0";
                case "1:四捨五入":
                    return "1";
                case "2:切り上げ":
                    return "2";
            }
            return "";
        }

        //端数処理の変換
        public string Get_HasuShori_CD2(string pShori)
        {
            switch (pShori)
            {
                case "1:切り捨て":
                    return "1";
                case "2:四捨五入":
                    return "2";
                case "3:切り上げ":
                    return "3";
            }
            return "";
        }

        //支払通知発行区分の変換
        public string Get_HaraiTuuti_CD(string pShori)
        {
            switch (pShori)
            {
                case "0:印刷しない":
                    return "0";
                case "1:印刷する":
                    return "1";
            }
            return "";
        }

        //休日補正の変換
        public string Get_Hosei_CD(string pHosei)
        {
            switch (pHosei)
            {
                case "0:前営業日":
                    return "0";
                case "1:当日":
                    return "1";
                case "2:後営業日":
                    return "2";
            }
            return "";
        }

        //号の変換
        public string Get_Gou_CD(string pGou)
        {
            switch (pGou)
            {
                case "1:原稿料・作曲料等":
                    return "1";
                case "2:弁護士・税理士等":
                    return "2";
            }
            return "0";
        }

        //協力会費計算区分の変換
        public string Get_Kycal_CD(string pKycal)
        {
            switch (pKycal)
            {
                case "0:比率":
                    return "0";
                case "1:実額":
                    return "1";
            }
            return "";
        }

        //預金種別の変換
        public string Get_YokinType_CD(string pYokinType)
        {
            switch (pYokinType)
            {
                case "1:普通預金":
                    return "1";
                case "2:当座預金":
                    return "2";
                case "4:貯蓄":
                    return "4";
                case "9:その他":
                    return "9";
            }
            return "";
        }
        public string Get_Sen_YokinType_CD(string pYokinType)
        {
            switch (pYokinType)
            {
                case "1:普通預金":
                    return "1";
                case "2:当座預金":
                    return "2";
                case "4:貯蓄預金":
                    return "4";
                case "5:通知預金":
                    return "5";
            }
            return "";
        }

        //手数料負担の変換
        public string Get_Tesuu_CD(string pTesuu)
        {
            switch (pTesuu)
            {
                case "1:自社負担":
                    return "1";
                case "2:客先負担":
                    return "2";
                case "3:折半":
                    return "3";
                case "4:一律":
                    return "4";
            }
            return "";
        }

        //送金区分の変換
        public string Get_Soukin_CD(string pSoukin)
        {
            switch (pSoukin)
            {
                case "7:電信":
                    return "7";
                case "8:文書":
                    return "8";
            }
            return "";
        }

        //送付案内出力有無の変換
        public string Get_FSoufu_CD(string pFSoufu)
        {
            switch (pFSoufu)
            {
                case "0:送付しない":
                    return "0";
                case "1:書留":
                    return "1";
                case "2:簡易書留":
                    return "2";
                case "3:空白":
                    return "3";
            }
            return "";
        }

        //案内文パターンの変換
        public string Get_Annai_CD(string pAnnai)
        {
            switch (pAnnai)
            {
                case "1:パターン１":
                    return "1";
                case "2:パターン２":
                    return "2";
            }
            return "";
        }

        //送料負担区分の変換
        public string Get_Tsokbn_CD(string pTsokbn)
        {
            switch (pTsokbn)
            {
                case "0:来社":
                    return "0";
                case "1:自社負担":
                    return "1";
                case "2:客先負担":
                    return "2";
            }
            return "";
        }

        //名寄せの変換
        public string Get_NayoseNM(string pNayoseCD)
        {
            switch (pNayoseCD)
            {
                case "0":
                    return "名寄せしない";
                case "1":
                    return "名寄せする";
            }
            return pNayoseCD;
        }

        //節印の変換
        public string Get_SetuinNM(string pSetuinCD)
        {
            switch (pSetuinCD)
            {
                case "0":
                    return "節印しない";
                case "1":
                    return "節印する";
            }
            return pSetuinCD;
        }

        //**>>ICS-S 2013/05/20
        //譲渡制限の変換
        public string Get_JyotoNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "制限しない";
                case "1":
                    return "金融機関のみに制限する";
            }
            return pJyotoCD;
        }
        //**<<ICS-E

        //相殺許可SW
        public string Get_SosaiNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "許可しない";
                case "1":
                    return "許可する";
            }
            return pJyotoCD;
        }

        //相殺領収書発行SW
        public string Get_SosaiRyouNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "発行しない";
                case "1":
                    return "発行する";
            }
            return pJyotoCD;
        }

        //取引区分
        public string Get_TorihikiNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "国内";
                case "1":
                    return "海外";
            }
            return pJyotoCD;
        }

        //送金種類
        public string Get_GaiSFNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "電信送金";
                case "1":
                    return "送信小切手";
            }
            return pJyotoCD;
        }

        //送金支払方法
        public string Get_GaiSHNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "通知払";
                case "1":
                    return "請求払";
            }
            return pJyotoCD;
        }

        //源泉税元金額税区分
        public string Get_GSSKBN_NM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "1":
                    return "支払金額";
                case "2":
                    return "税抜金額";
            }
            return pJyotoCD;
        }

        //約定指定
        public string Get_YakujyoNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "指定しない";
                case "1":
                    return "指定する";
            }
            return pJyotoCD;
        }

        //初期値
        public string Get_ShokichiNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "通常";
                case "1":
                    return "初期値";
            }
            return pJyotoCD;
        }

        //でんさい代表口座
        public string Get_DensaiNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "通常";
                case "1":
                    return "代表口座";
            }
            return pJyotoCD;
        }

        //でんさい手数料設定
        public string Get_DensaiTesuuNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "使用する";
            }
            return pJyotoCD;
        }

        //外貨を使用
        public string Get_GaikaNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "使用する";
            }
            return pJyotoCD;
        }

        public string Get_SetteiNM(string pJyotoCD)
        {
            switch (pJyotoCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "使用する";
            }
            return pJyotoCD;
        }

        //源泉税計算区分
        public string Get_GENSENNM(string pGENSENCD)
        {
            switch (pGENSENCD)
            {
                case "0":
                    return "計算しない";
                case "1":
                    //**>>ICS-S 2013/03/04
                    //**return "報酬・金額の額×10％(但し、100万超の部分20%)";
                    return "支払金額×10.21％(但し、100万超の部分20.42%)";
                //**<<ICS-E
                case "2":
                    //**>>ICS-S 2013/03/04
                    //**return "(報酬・金額の額－1万)×10％";
                    return "(支払金額－1万)×10.21％";
                case "3":
                    return "計算なしで支払調書を出力する";
                //**<<ICS-E
            }
            return pGENSENCD;
        }

        //保留
        public string Get_HoryuNM(string pHoryuCD)
        {
            switch (pHoryuCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "支払保留を使用する";
                case "2":
                    return "自動控除を使用する";
            }
            return pHoryuCD;
        }

        //チェックボックスのチェック有無変換(コード⇒チェック有無)
        public bool Get_Cmb_CD1(int pBool)
        {
            switch (pBool)
            {
                case 0:
                    return false;
                case 1:
                    return true;
            }
            return false;
        }

        //チェックボックスのチェック有無変換(コード⇒チェック有無)
        public bool Get_Cmb_NM2(int pBool)
        {
            switch (pBool)
            {
                case 0:
                    return true;
                case 1:
                    return false;
            }
            return false;
        }

        //チェックボックスのチェック有無変換(チェック有無⇒コード)
        public string Get_Cmb_CD1(bool pBool)
        {
            switch (pBool)
            {
                case false:
                    return "0";
                case true:
                    return "1";
            }
            return "0";
        }

        //チェックボックスのチェック有無変換(チェック有無⇒コード)
        public string Get_Cmb_CD2(bool pBool)
        {
            switch (pBool)
            {
                case true:
                    return "0";
                case false:
                    return "1";
            }
            return "0";
        }

        //手形管理のみSWのリファレンス取得
        public string Get_TGASW_NM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "債権債務で使用";
                case "1":
                    return "期日管理のみで使用(入金＆支払)";
                case "2":
                    return "期日管理のみで使用(入金)";
                case "3":
                    return "期日管理のみで使用(支払)";
            }
            return "";
        }

        //得意先SWのリファレンス取得
        public string Get_SAIKEN_NM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "使用する";
            }
            return "";
        }

        //入金代表者SWのリファレンス取得
        public string Get_SAIKEN_FLG_NM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "オフ(通常)";
                case "1":
                    return "オン(入金代表者)";
            }
            return "";
        }

        //仕入先SWのリファレンス取得
        public string Get_SAIMU_NM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "使用しない";
                case "1":
                    return "使用する";
            }
            return "";
        }

        //支払代表者SWのリファレンス取得
        public string Get_SAIMU_FLG_NM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "オフ(通常)";
                case "1":
                    return "オン(債務代表者)";
            }
            return "";
        }

        //取引停止のリファレンス値取得
        public string Get_STFLGNM(string pCD)
        {
            switch (pCD)
            {
                case "0":
                    return "継続";
                case "1":
                    return "取引停止";
            }
            return "";
        }
        #endregion



        #region 一見先対応
        /// <summary>
        /// 一見先取引先CDの取得
        /// </summary>
        public string Get_TRCD_ICHI()
        {
            //string sTRCD_ICHI = "1000000000000";
            string sTRCD_ICHI = "0";
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★SS_TRSEQ.Select(一見先取引先CDの最大値取得)
                Global.cCmdSel.CommandText = "SELECT MAX(TRSEQ) AS MaxTRCD FROM SS_TRSEQ ";
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    if (DbCls.GetStrNullKara(reader["MaxTRCD"].ToString()) != "")
                    {
                        sTRCD_ICHI = (Convert.ToInt64(reader["MaxTRCD"].ToString()) + 1).ToString();
                    }
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                var now = Get_DBTime();
                //★SS_TRSEQ.Insert(採番下TRCDの登録)
                Global.cCmdIns.CommandText = "INSERT INTO SS_TRSEQ "
                                           + "(TRSEQ, FMOD, FTIM, FUSR, FWAY, LMOD, LTIM, LUSR, LWAY ) "
                                           + "VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p) ";
                Global.cCmdIns.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRSEQ", Convert.ToInt64(sTRCD_ICHI));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD",
                //DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("yyyyMMdd")));
                DbCls.GetNumNullZero<int>(now.ToString("yyyyMMdd")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM",
                //DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("HHmmssff")));
                DbCls.GetNumNullZero<int>(now.ToString("HHmmssff")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FWAY", 0);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD",
                //DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("yyyyMMdd")));
                DbCls.GetNumNullZero<int>(now.ToString("yyyyMMdd")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM",
                //DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("HHmmssff")));
                DbCls.GetNumNullZero<int>(now.ToString("HHmmssff")));
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LWAY", 0);
                //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    DbCls.ReplacePlaceHolder(Global.cCmdIns);
                }
                DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                Global.cCmdIns.ExecuteNonQuery();

                sTRCD_ICHI = (1000000000000 + Convert.ToInt64(sTRCD_ICHI)).ToString();
                return sTRCD_ICHI;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TRCD_ICHI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return "";
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #endregion



        #region 履歴関連
        //DtRIREKI_New
        public void New_dtRIREKI()
        {
            Global.dtRIREKI = new DataTable();
            Global.dtRIREKI.Columns.Add("RKTRCD", typeof(string));
            Global.dtRIREKI.Columns.Add("RKHJCD", typeof(string));
            Global.dtRIREKI.Columns.Add("RKBNID", typeof(int));
            Global.dtRIREKI.Columns.Add("RKID", typeof(int));
            Global.dtRIREKI.Columns.Add("RKKKM", typeof(string));
            Global.dtRIREKI.Columns.Add("RKKBN", typeof(int));
            Global.dtRIREKI.Columns.Add("RKNM", typeof(string));
            Global.dtRIREKI.Columns.Add("RKBITM", typeof(string));
            Global.dtRIREKI.Columns.Add("RKAITEM", typeof(string));
        }


        //変更履歴の登録
        public void Insert_SS_RKITORI()
        {
            if (Global.nRirekiSW == 0)
            {
                return;
            }

            Int64 nSEQ = 1;
            var dat = Global.dNow;
            int mod = dat.Year * 10000 + dat.Month * 100 + dat.Day;
            int tim = (dat.Hour * 10000000 + dat.Minute * 100000 + dat.Second * 1000 + dat.Millisecond) / 10;
            try
            {
                for (int n = 0; n < Global.dtRIREKI.Rows.Count; n++)
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    //★SS_RKITORI.Select(最大SEQの取得)
                    Global.cCmdSel.CommandText = "SELECT MAX(RKSEQ) AS RKSEQ FROM SS_RKITORI "
                                               + "WHERE RKTRCD = :p AND RKHJCD = :p ";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKTRCD", Global.dtRIREKI.Rows[n]["RKTRCD"].ToString());
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKHJCD", Global.dtRIREKI.Rows[n]["RKHJCD"].ToString());
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@RKHJCD", int.Parse(Global.dtRIREKI.Rows[n]["RKHJCD"].ToString()));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                    // <マルチDB対応>Readが必須なので変更
                    if (reader.HasRows == true)
                    {
                        reader.Read();
                        if (DbCls.GetStrNullKara(reader["RKSEQ"].ToString()) != "")
                        {
                            nSEQ = Convert.ToInt64(reader["RKSEQ"].ToString()) + 1;
                        }
                    }
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    //★SS_RKITORI.Select(既に登録済)
                    Global.cCmdSel.CommandText = "SELECT FMOD FROM SS_TORI "
                        //+ "WHERE RTRIM(TRCD) = :p AND HJCD = :p AND FMOD <= :p ";
                                                + "WHERE TRCD = :p AND HJCD = :p AND FMOD < :p ";
                    Global.cCmdSel.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", Global.dtRIREKI.Rows[n]["RKTRCD"].ToString());
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", Global.dtRIREKI.Rows[n]["RKHJCD"].ToString());
                    //DbCls.AddParamaterByValue(ref Global.cCmdSel, "@FMOD", Global.sRirekiDate);
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(Global.dtRIREKI.Rows[n]["RKHJCD"].ToString()));
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, "@FMOD", int.Parse(Global.sRirekiDate));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                    if ((reader.HasRows == true) &&
                        (nSEQ == 1))
                    {
                        // <マルチDB対応>Readが必須なので追加
                        reader.Read();
                        //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                        reader.Close();
                        reader.Dispose();
                        //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】

                        //★SS_RKITORI.Insert(【開始】レコードの登録)
                        Global.cCmdIns.CommandText = "INSERT INTO SS_RKITORI "
                            //+ "(RKTRCD, RKHJCD, RKBNID, RKID, RKSEQ, RKKKM, RKKBN, RKWAY, RKNM, RKBITM, RKAITEM, RKDATE, RKTIM, RKUSR) "
                                                   + "(RKTRCD, RKHJCD, RKBNID, RKID, RKSEQ, RKKKM, RKKBN, RKWAY, RKNM, RKBITM, RKAITEM, RKDATE, RKTIM, RKUSR, RKRYAKU, RKTORINAM) "
                            //+ "VALUES(:p, :p, :p, :p, :p, :p, :p, 0, :p, :p, :p, :p, :p, :p) ";
                                                   + "VALUES(:p, :p, :p, :p, :p, :p, :p, 0, :p, :p, :p, :p, :p, :p, :p, :p) ";
                        Global.cCmdIns.Parameters.Clear();
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTRCD", Global.dtRIREKI.Rows[n]["RKTRCD"].ToString());
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKHJCD", int.Parse(Global.dtRIREKI.Rows[n]["RKHJCD"].ToString()));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBNID", 0);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKID", 0);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKSEQ", nSEQ);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKKM", " ");
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKBN", 0);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKNM", DBNull.Value);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBITM", DBNull.Value);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKAITEM", DBNull.Value);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKDATE", mod); //Global.sRirekiStartDate);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTIM", tim); //Global.sRirekiStartTime);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKUSR", Global.nUcod); // Global.sRirekiStartUser);
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKRYAKU", LeftB(Global.RYAKU, 20)); // 20byte
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTORINAM", LeftB(Global.TORI_NAM, 44));
                        //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                        {
                            DbCls.ReplacePlaceHolder(Global.cCmdIns);
                        }
                        DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                        Global.cCmdIns.ExecuteNonQuery();

                        //変更履歴登録用にseqをカウントアップ
                        nSEQ++;
                    }

                    //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】

                    //★SS_RKITORI.Insert(変更履歴の登録)
                    Global.cCmdIns.CommandText = "INSERT INTO SS_RKITORI "
                        //+ "(RKTRCD, RKHJCD, RKBNID, RKID, RKSEQ, RKKKM, RKKBN, RKWAY, RKNM, RKBITM, RKAITEM, RKDATE, RKTIM, RKUSR) "
                                               + "(RKTRCD, RKHJCD, RKBNID, RKID, RKSEQ, RKKKM, RKKBN, RKWAY, RKNM, RKBITM, RKAITEM, RKDATE, RKTIM, RKUSR, RKRYAKU, RKTORINAM) "
                        //+ "VALUES(:p, :p, :p, :p, :p, :p, :p, 0, :p, :p, :p, :p, :p, :p) ";
                                               + "VALUES(:p, :p, :p, :p, :p, :p, :p, 0, :p, :p, :p, :p, :p, :p, :p, :p) ";
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTRCD", Global.dtRIREKI.Rows[n]["RKTRCD"].ToString());
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKHJCD", Global.dtRIREKI.Rows[n]["RKHJCD"].ToString());
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBNID", Global.dtRIREKI.Rows[n]["RKBNID"].ToString());
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKID", Global.dtRIREKI.Rows[n]["RKID"].ToString());
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKHJCD", int.Parse(Global.dtRIREKI.Rows[n]["RKHJCD"].ToString()));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBNID", int.Parse(Global.dtRIREKI.Rows[n]["RKBNID"].ToString()));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKID", int.Parse(Global.dtRIREKI.Rows[n]["RKID"].ToString()));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKSEQ", nSEQ);
                    if (DbCls.GetStrNullKara(Global.dtRIREKI.Rows[n]["RKKKM"]) == "")
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKKM", " ");
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKKM", Global.dtRIREKI.Rows[n]["RKKKM"].ToString());
                    }
                    //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                    //DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKBN", Global.dtRIREKI.Rows[n]["RKKBN"].ToString());
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKKBN", int.Parse(Global.dtRIREKI.Rows[n]["RKKBN"].ToString()));
                    //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                    if (Global.dtRIREKI.Rows[n]["RKNM"] != DBNull.Value)
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKNM", Global.dtRIREKI.Rows[n]["RKNM"].ToString());
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKNM", DBNull.Value);
                    }
                    if (Global.dtRIREKI.Rows[n]["RKBITM"] != DBNull.Value)
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBITM", Global.dtRIREKI.Rows[n]["RKBITM"].ToString());
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKBITM", DBNull.Value);
                    }
                    if (Global.dtRIREKI.Rows[n]["RKAITEM"] != DBNull.Value)
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKAITEM", Global.dtRIREKI.Rows[n]["RKAITEM"].ToString());
                    }
                    else
                    {
                        DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKAITEM", DBNull.Value);
                    }
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKDATE", mod);
                    // DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("yyyyMMdd")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTIM", tim);
                    // DbCls.GetNumNullZero<int>(IcsSSUtil.IDate.GetDBNow(Global.cConCommon).ToString("HHmmssff")));
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKUSR", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKRYAKU", LeftB(Global.RYAKU, 20)); // 20bytes
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RKTORINAM", LeftB(Global.TORI_NAM, 44));
                    //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nInsert_SS_RKITORI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        //public void Insert_TRMRKI()
        //{
        //    Int64 nSEQ = 1;
        //    var dat = Global.dNow;
        //    int mod = dat.Year * 10000 + dat.Month * 100 + dat.Day;
        //    int tim = (dat.Hour * 100000 + dat.Minute * 1000 + dat.Second * 10) / 10;
        //    try
        //    {
        //        // 財務で履歴を作成する設定か確認
        //        if (!RirekiChk())
        //        {
        //            return;
        //        }
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //        Global.cCmdSel.CommandText = "SELECT MAX(RSEQ) AS RSEQ FROM TRMRKI WHERE TRCD = :p";
        //        Global.cCmdSel.Parameters.Clear();
        //        DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", Global.TRCD_R);
        //        DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

        //        // <マルチDB対応>Readが必須なので変更
        //        if (reader.HasRows == true)
        //        {
        //            reader.Read();
        //            if (DbCls.GetStrNullKara(reader["RSEQ"].ToString()) != "")
        //            {
        //                nSEQ = Convert.ToInt64(reader["RSEQ"].ToString()) + 1;
        //            }
        //        }

        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //        Global.cCmdSel.CommandText = "SELECT * FROM TRNAM WHERE TRCD = :p";
        //        Global.cCmdSel.Parameters.Clear();
        //        DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", Global.TRCD_R);
        //        DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

        //        if (reader.HasRows == true)
        //        {
        //            Global.cCmdIns.CommandText = "INSERT INTO TRMRKI (TRCD, RSEQ, RNLD, TRMX, TRNM, FUSR, FMOD, FTIM, LUSR, LMOD, LTIM, RUSR, RMOD, RTIM, RKBN) ";
        //            Global.cCmdIns.CommandText += " VALUES(:p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, :p, 1) ";
        //            Global.cCmdIns.Parameters.Clear();
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRCD", Global.TRCD_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RSEQ", nSEQ);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RNLD", Global.KNLD_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRMX", Global.RYAKU_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@TRNM", Global.TORI_NAM_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FUSR", Global.FUSR_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FMOD", Global.FMOD_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@FTIM", Global.FTIM_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LUSR", Global.LUSR_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LMOD", Global.LMOD_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@LTIM", Global.LTIM_R);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RUSR", Global.nUcod);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RMOD", mod);
        //            DbCls.AddParamaterByValue(ref Global.cCmdIns, "@RTIM", tim);
        //            if (DbCls.DbType == DbCls.eDbType.SQLServer)
        //            {
        //                DbCls.ReplacePlaceHolder(Global.cCmdIns);
        //            }
        //            DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
        //            Global.cCmdIns.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(
        //            "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
        //            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //            reader.Dispose();
        //        }
        //    }
        //}
        #endregion



        #region 入力チェック
        /// <summary>
        /// 支払条件タブのユニークチェック
        /// </summary>
        /// <param name="sTRCD"></param>
        /// <param name="sHJCD"></param>
        /// <param name="sBCOD"></param>
        /// <param name="sKCOD"></param>
        /// <param name="nSHOID"></param>
        /// <returns></returns>
        public bool Chk_UniqKey(string sTRCD, string sHJCD, string sBCOD, string sKCOD, ref string sSHOID)
        {
            try
            {
                string sTRCD_wk = sTRCD;
                string sHJCD_wk = sHJCD;
                string sBCOD_wk = sBCOD;
                string sKCOD_wk = sKCOD;

                if (Global.nTRCD_Type == 1)
                {
                    sTRCD_wk = sTRCD.PadRight(Global.nTRCD_Len, ' ');
                }
                if (Global.nBCOD_Type == 1 && sBCOD != "0")
                {
                    sBCOD_wk = sBCOD.PadRight(Global.nBCOD_Len, ' ');
                }
                if (Global.nKCOD_Type == 1 && sKCOD != "0")
                {
                    sKCOD_wk = sKCOD.PadRight(Global.nKCOD_Len, ' ');
                }
                if (sHJCD != "")
                {
                    //sHJCD_wk = Convert.ToInt16(sHJCD).ToString();
                    sHJCD_wk = Convert.ToInt32(sHJCD).ToString();
                }
                else
                {
                    sHJCD_wk = "0";
                }

                if (sKCOD != "0")
                {
                    sKCOD_wk = Conv_KCODtoKICD(sKCOD_wk);
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★取引先支払方法の検索(キー重複チェック)
                Global.cCmdSel.CommandText = "SELECT SHO_ID FROM SS_TSHOH "
                                           + "WHERE TRCD = :p AND HJCD = :p AND BCOD = :p AND JBCD = '0' AND KICD = :p AND SJBCD = '0' AND SHO_ID != :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「'')」のみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD_wk);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", int.Parse(sHJCD_wk));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@BCOD", sBCOD_wk);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KICD", sKCOD_wk);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@SHO_ID", int.Parse(sSHOID));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    sSHOID = DbCls.GetStrNullKara(reader["SHO_ID"].ToString());
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_UniqKey　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public int Get_TRNAM(string sTRCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★財務取引先の検索
                Global.cCmdSelZ.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSelZ, "@TRCD", sTRCD.TrimEnd());
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    return 0;
                }
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TRNAM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <>
                return 9;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        #endregion


        //文字列カット
        public string StringCut(string pStr, int pLen)
        {
            byte[] bData;
            byte[] boData = new byte[pLen];
            string sData;

            bData = Encoding.GetEncoding(932).GetBytes(pStr);
            if (bData.Length <= pLen)
            {
                return pStr;
            }
            for (int i = 0; i < pLen; i++)
            {
                boData[i] = bData[i];
            }
            sData = Encoding.GetEncoding(932).GetString(boData);
            if (Encoding.GetEncoding(932).GetByteCount(sData) != pLen)
            {
                boData[pLen - 1] = 0x20;
                sData = System.Text.Encoding.GetEncoding(932).GetString(boData);
            }

            return sData;
        }

        internal string GetTrcdDB(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : Global.nTRCD_Type == 0
                    ? value.PadLeft(Global.nTRCD_Len, '0')
                    : value.PadRight(Global.nTRCD_Len);
        }

        internal string GetHjcdDB(string value)
        {
            int hjcd = 0;
            int.TryParse(value, out hjcd);
            return hjcd.ToString();
        }

        private void ExecuteQuery(string query, params DBParameter[] args)
        {
            DisposeDataReader();
            Global.cCmdSel.CommandText = query;
            Global.cCmdSel.Parameters.Clear();
            if (args != null && args.Length > 0)
            {
                foreach (DBParameter param in args)
                {
                    DbCls.AddParamaterByValue(ref Global.cCmdSel, param.Name, param.Value);
                }
            }
            DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);
        }
        private void ExecuteQueryZ(string query, params DBParameter[] args)
        {
            DisposeDataReader();
            Global.cCmdSelZ.CommandText = query;
            Global.cCmdSelZ.Parameters.Clear();
            if (args != null && args.Length > 0)
            {
                foreach (DBParameter param in args)
                {
                    DbCls.AddParamaterByValue(ref Global.cCmdSelZ, param.Name, param.Value);
                }
            }
            DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);
        }

        private void DisposeDataReader()
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        private string LeftB(string value, int length)
        {
            string s = string.Empty;
            if (string.IsNullOrEmpty(value)) { return s; }
            var enc = Encoding.GetEncoding("shift_jis");
            for (int i = 0; i < value.Length; i++)
            {
                s = value.Substring(0, i + 1);
                if (enc.GetByteCount(s) > length) { return value.Substring(0, i); }
            }
            return s;
        }

        // V12.03.01 SS_TORI,SS_TSHOH,SS_FRIGINの中から最新の更新日時を取得
        internal void Get_UPDATE()
        {
            // Ver.01.02.01 [SS_4523]対応 Toda -->
            #region 
            //int[] update = new int[3];
            //update[0] = 0;
            //update[1] = 0;
            //update[2] = 0;

            //#region SS_TSHOHの更新日時を取得
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendFormat("select MAX(LMOD) max1 from SS_TSHOH tr where tr.TRCD = '{0}' and tr.HJCD = {1}"
            //        , GetTrcdDB(Global.TRCD), GetHjcdDB(Global.HJCD));
            //    ExecuteQuery(sb.ToString());
            //    if (reader.HasRows)
            //    {
            //        // <マルチDB対応>Readが必須なので追加
            //        reader.Read();

            //        update[0] = DbCls.GetNumNullZero<int>(reader["max1"]);
            //    }
            //}
            //#endregion

            //#region SS_FRIGINの更新日時を取得
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendFormat("select MAX(LMOD) max2 from SS_FRIGIN tr where tr.TRCD = '{0}' and tr.HJCD = {1}"
            //        , GetTrcdDB(Global.TRCD), GetHjcdDB(Global.HJCD));
            //    ExecuteQuery(sb.ToString());
            //    if (reader.HasRows)
            //    {
            //        // <マルチDB対応>Readが必須なので追加
            //        reader.Read();

            //        update[1] = DbCls.GetNumNullZero<int>(reader["max2"]);
            //    }
            //}
            //#endregion

            ////SS_TORIの更新日時を取得
            //if (Global.LMOD != "" || Global.LMOD == null)
            //{
            //    update[2] = Convert.ToInt32(Global.LMOD);
            //}

            //// 比較して最新の日時を取得
            //Global.LMOD = Convert.ToString(update.Max());
            #endregion
            int[] nlusr = new int[3];
            int[] nlmod = new int[3];
            decimal[] update = new decimal[3];

            #region SS_TSHOHの更新ユーザー、日時を取得
            {
                var sb = new StringBuilder();
                sb.AppendFormat("select distinct t1.LUSR, t1.LMOD, (t1.LMOD * 10000000000 + t1.LTIM) dt from SS_TSHOH t1 where t1.TRCD = '{0}' and t1.HJCD = {1} "
                    + "and not exists(select 1 from SS_TSHOH t2 where t1.TRCD = t2.TRCD and t1.HJCD = t2.HJCD and t1.LMOD * 10000000000 + t1.LTIM < t2.LMOD * 10000000000 + t2.LTIM)"
                    , GetTrcdDB(Global.TRCD), int.Parse(GetHjcdDB(Global.HJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.Parse()」のみ追加
                ExecuteQuery(sb.ToString());
                if (reader.HasRows)
                {
                    reader.Read();

                    nlusr[0] = DbCls.GetNumNullZero<int>(reader["LUSR"]);
                    nlmod[0] = DbCls.GetNumNullZero<int>(reader["LMOD"]);
                    update[0] = DbCls.GetNumNullZero<decimal>(reader["dt"]);
                }
            }
            #endregion

            #region SS_FRIGINの更新ユーザー、日時を取得
            {
                var sb = new StringBuilder();
                sb.AppendFormat("select distinct t1.LUSR, t1.LMOD, (t1.LMOD * 10000000000 + t1.LTIM) dt from SS_FRIGIN t1 where t1.TRCD = '{0}' and t1.HJCD = {1} "
                    + "and not exists(select 1 from SS_FRIGIN t2 where t1.TRCD = t2.TRCD and t1.HJCD = t2.HJCD and t1.LMOD * 10000000000 + t1.LTIM < t2.LMOD * 10000000000 + t2.LTIM)"
                    , GetTrcdDB(Global.TRCD), int.Parse(GetHjcdDB(Global.HJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.parse()」のみ追加
                ExecuteQuery(sb.ToString());
                if (reader.HasRows)
                {
                    reader.Read();

                    nlusr[1] = DbCls.GetNumNullZero<int>(reader["LUSR"]);
                    nlmod[1] = DbCls.GetNumNullZero<int>(reader["LMOD"]);
                    update[1] = DbCls.GetNumNullZero<decimal>(reader["dt"]);
                }
            }
            #endregion

            #region SS_TORIの更新ユーザー、日時を取得
            {
                var sb = new StringBuilder();
                sb.AppendFormat("select LUSR, LMOD, (LMOD * 10000000000 + LTIM) dt from SS_TORI where TRCD = '{0}' and HJCD = {1}"
                    , GetTrcdDB(Global.TRCD), int.Parse(GetHjcdDB(Global.HJCD)));//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】「int.parse()」のみ追加
                ExecuteQuery(sb.ToString());
                if (reader.HasRows)
                {
                    reader.Read();

                    nlusr[2] = DbCls.GetNumNullZero<int>(reader["LUSR"]);
                    nlmod[2] = DbCls.GetNumNullZero<int>(reader["LMOD"]);
                    update[2] = DbCls.GetNumNullZero<decimal>(reader["dt"]);
                }
            }
            #endregion

            // 比較して最新の日時を取得
            for (int i = 0; i < 3; i++)
            {
                if (update[i] == update.Max())
                {
                    Global.LUSR = Convert.ToString(nlusr[i]);
                    Global.LMOD = Convert.ToString(nlmod[i]);
                    break;
                }
            }
            // Ver.01.02.01 <--
        }

        internal void Get_ZYMD()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendFormat("select ISTAYMD, IENDYMD from TRNAM tr where tr.TRCD = '{0}' "
                    , GetTrcdDB(Global.TRCD));
                ExecuteQueryZ(sb.ToString());
                if (reader.HasRows)
                {
                    // <マルチDB対応>Readが必須なので追加
                    reader.Read();

                    Global.ZSTYMD = DbCls.GetNumNullZero<int>(reader["ISTAYMD"]).ToString();
                    Global.ZEDYMD = DbCls.GetNumNullZero<int>(reader["IENDYMD"]).ToString();
                }
                else
                {
                    Global.ZSTYMD = "";
                    Global.ZEDYMD = "";
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        // 債務取引先の存在チェック
        public bool SS_TORI_Exist(string sTRCD, int nHJCD)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★債務取引先の検索
                Global.cCmdSel.CommandText = "SELECT * FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p ";//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】RTRIMのみ追加
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD.TrimEnd());
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", nHJCD);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref reader);

                if (reader.HasRows == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSS_TORI_Exist　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public void RirekiChk()
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                //★内部統制のチェック
                Global.cCmdSelZ.CommandText = "SELECT FLG, RYMD, RTIM, RUSR FROM  SETSTK WHERE IDNO = 1 AND RSEQ >= 1 ORDER BY RSEQ DESC ";
                Global.cCmdSelZ.Parameters.Clear();
                DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                if (reader.HasRows == true)
                {
                    Global.bRKFLG = true;
                    return;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                //★履歴のチェック

                Global.cCmdCommonSel.CommandText = "select c.SUBNM from SUBSYS c, KSUBSYS k "
                                           + " where k.CCOD = :p and k.SUBID = :p and c.SUBID = :p ";
                Global.cCmdCommonSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@CCOD", Global.sCcod);
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@SUBID1", 20);
                DbCls.AddParamaterByValue(ref Global.cCmdCommonSel, "@SUBID2", 20);
                DbCls.ExecuteQuery(ref Global.cCmdCommonSel, ref reader);

                if (reader.HasRows == true)
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    Global.cCmdSelZ.CommandText = "SELECT IDNO FROM SETRKIK WHERE FLG = 1 ";
                    Global.cCmdSelZ.Parameters.Clear();
                    DbCls.ExecuteQuery(ref Global.cCmdSelZ, ref reader);

                    if (reader.HasRows == true)
                    {
                        Global.bRKFLG = true;
                        return;
                    }
                }
                Global.bRKFLG = false;
                return;
            }
            catch
            {
                Global.bRKFLG = false;
                return;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        internal DateTime Get_DBTime()
        {
            //Global.cCmdSel.CommandText = "SELECT GETDATE() ";
            //return (DateTime)Global.cCmdSel.ExecuteScalar();

            IDate idt = new IDate();
            DateTime date;
            int nYyyymmdd = 0;
            int nHhmmss = 0;
            idt.GetDBNow(Global.cConSaikenSaimu, out nYyyymmdd, out nHhmmss);

            string sYyyymmdd = nYyyymmdd.ToString().Insert(4, "/").Insert(7, "/");
            string sHhmmss = nHhmmss.ToString().PadLeft(6, '0').Insert(2, ":").Insert(5, ":");
            date = DateTime.ParseExact(sYyyymmdd + " " + sHhmmss, "yyyy/MM/dd HH:mm:ss", null);

            return date;
        }
        internal DateTime Get_DBTime(DbTransaction trn)
        {
            //Global.cCmdSel.CommandText = "SELECT GETDATE() ";
            //return (DateTime)Global.cCmdSel.ExecuteScalar();

            IDate idt = new IDate();
            DateTime date;
            int nYyyymmdd = 0;
            int nHhmmss = 0;
            idt.GetDBNow(Global.cConSaikenSaimu, trn, out nYyyymmdd, out nHhmmss);

            string sYyyymmdd = nYyyymmdd.ToString().Insert(4, "/").Insert(7, "/");
            string sHhmmss = nHhmmss.ToString().PadLeft(6, '0').Insert(2, ":").Insert(5, ":");
            date = DateTime.ParseExact(sYyyymmdd + " " + sHhmmss, "yyyy/MM/dd HH:mm:ss", null);

            return date;
        }
    }

    class DBParameter
    {
        internal string Name { get; set; }
        internal object Value { get; set; }
        public DBParameter() : this(string.Empty, null) { }
        public DBParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
    namespace StringUtil
    {
        public static class StrinUtil
        {
            public const int LocalIDJpn = 0x0411;
            #region 変換

            public static string ToNotNull(string value)
            {
                return string.IsNullOrEmpty(value) ? string.Empty : value;
            }

            public static string ToHankaku(string value)
            {
                return VB.Strings.StrConv(
                    value, VB.VbStrConv.Narrow, LocalIDJpn);
            }

            public static string ToKatakana(string value)
            {
                return VB.Strings.StrConv(
                    value, VB.VbStrConv.Katakana, LocalIDJpn);
            }

            public static string ToHankakuKatakana(string value)
            {
                return
                    ToHankaku(
                    ToKatakana(value));
            }

            public static string ToZenkaku(string value)
            {
                return VB.Strings.StrConv(
                    value, VB.VbStrConv.Wide, LocalIDJpn);
            }

            public static string ToHiragana(string value)
            {
                return VB.Strings.StrConv(
                    value, VB.VbStrConv.Hiragana, LocalIDJpn);
            }

            public static string ToZenkakuHiragana(string value)
            {
                return
                    ToHiragana(
                    ToZenkaku(value));
            }

            public static string DelPadLeft(string v, char pad = '0')
            {
                if (string.IsNullOrEmpty(v)) return v;
                int iStart = 0;
                foreach (char c in v)
                {
                    if (c != pad) break;
                    iStart++;
                }
                return v.Substring(iStart);
            }

            #endregion 変換
            #region 比較

            public static bool SafetyIsEqual(string v1, string v2)
            {
                return
                    ToNotNull(v1).Equals(
                    ToNotNull(v2));
            }

            #endregion 比較
            #region 折り返し

            public static string[] FoldString(
                string input,
                int bytesPerLine,
                int maxLineCount = int.MaxValue
                )
            {
                return
                    FoldString(
                    input,
                    ShiftJis,//SJIS
                    bytesPerLine,
                    maxLineCount);
            }

            public static string[] FoldString(
                string input,
                Encoding enc,
                int bytesPerLine,
                int maxLineCount = int.MaxValue
                )
            {
                List<string> lines = new List<string>();
                FoldString(
                    lines,
                    input,
                    enc,
                    bytesPerLine,
                    maxLineCount
                    );
                return lines.ToArray();
            }

            public static int FoldString(
                List<string> lines,
                string input,
                Encoding enc,
                int bytesPerLine,
                int maxLineCount = int.MaxValue
                )
            {
                if (maxLineCount <= lines.Count + 1)
                {
                    lines.Add(input);
                    return lines.Count;
                }
                int len = input.Length;
                for (int iChar = 0; iChar < len; iChar++)
                {
                    char[] chars = input.ToCharArray(0, iChar + 1);
                    byte[] bytes = enc.GetBytes(chars);
                    if (bytesPerLine < bytes.Length)
                    {
                        if (0 == iChar)
                        {
                            //NG
                            lines.Add(input);
                            return lines.Count;
                        }
                        else
                        {
                            lines.Add(input.Substring(0, iChar));
                            return
                                FoldString(
                                lines,
                                input.Substring(iChar),
                                enc,
                                bytesPerLine,
                                maxLineCount
                                );
                        }
                    }
                }
                lines.Add(input);
                return lines.Count;
            }

            #endregion 折り返し
            #region shift-jis 関連 バイト数での指定

            public static Encoding _sjis = null;
            public static Encoding ShiftJis
            {
                get
                {
                    if (_sjis == null)
                    {
                        _sjis = Encoding.GetEncoding(932); //SJIS
                    }
                    return _sjis;
                }
            }

            /// <summary>Shift-JISでのバイト数取得</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static int GetByteCount(string s)
            {
                return GetByteCount(s, ShiftJis);
            }

            /// <summary>指定したエンコードでのバイト数取得</summary>
            /// <param name="s"></param>
            /// <param name="enc"></param>
            /// <returns></returns>
            public static int GetByteCount(string s, Encoding enc)
            {
                if (string.IsNullOrEmpty(s)) { return 0; }
                if (enc == null) { enc = ShiftJis; }
                return enc.GetByteCount(s);
            }

            /// <summary>文字列を指定したバイト数で返す</summary>
            /// <param name="s"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public static string LeftB(string s, int length)
            {
                return CutString(s, length);
            }

            /// <summary>文字列の末尾を指定したバイト数で返す</summary>
            /// <param name="s"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public static string RightB(string s, int length)
            {
                return Reverse(LeftB(Reverse(s), length));
            }

            /// <summary>文字列を指定したバイト数で返す</summary>
            /// <param name="input"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public static string CutString(
                string input, int length)
            {
                return CutString(input, length, ShiftJis);
            }

            public static string[] Fold(
                this string v,
                int bytesPerLine,
                int maxLineCount = int.MaxValue
                )
            {
                return FoldString(v, bytesPerLine, maxLineCount);
            }
            /// <summary>文字列を指定したバイト数で返す</summary>
            /// <param name="input"></param>
            /// <param name="length"></param>
            /// <param name="enc"></param>
            /// <returns></returns>
            public static string CutString(
                string input, int length, Encoding enc)
            {
                string[] s = Fold(input, length, 2);
                return s.Length == 0 ? string.Empty : s[0];
            }

            /// <summary>反転文字列の取得</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string Reverse(string s)
            {
                if (string.IsNullOrEmpty(s)) { return string.Empty; }
                var charArray = s.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            /// <summary>文字列を指定した開始位置(バイト数)から、指定したバイト数で切り取り、文字を返す</summary>
            /// <param name="s"></param>
            /// <param name="startIndex"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public static string SubstringB(string s, int startIndex, int length)
            {
                if (string.IsNullOrEmpty(s)) { return string.Empty; }
                var data = ShiftJis.GetBytes(s);
                if (length < 0
                    || startIndex < 0
                    || data.Length <= startIndex) { return string.Empty; }
                return
                    ShiftJis.GetString(
                    data,
                    startIndex,
                    Math.Min(data.Length - startIndex, length));
            }

            /// <summary>文字列を指定した開始位置(バイト数)より先の文字を返す</summary>
            /// <param name="s"></param>
            /// <param name="startIndex"></param>
            /// <returns></returns>
            public static string SubstringB(string s, int startIndex)
            {
                if (string.IsNullOrEmpty(s)) { return string.Empty; }
                if (startIndex < 0) { return string.Empty; }
                var length = ShiftJis.GetByteCount(s) - startIndex;
                if (length < 0) { return string.Empty; }
                return SubstringB(s, startIndex, length);
            }

            /// <summary>PadRight バイト数版</summary>
            /// <param name="s"></param>
            /// <param name="length"></param>
            /// <param name="padding">' '半角スペースがdefault</param>
            /// <returns></returns>
            public static string PadRightB(string s, int length, char padding = ' ')
            {
                var data = ShiftJis.GetBytes(s);
                var result = new byte[length];
                if (ShiftJis.GetByteCount(padding.ToString()) > 1) { padding = ' '; }
                for (var i = 0; i < length; i++)
                {
                    result[i] = (i < data.Length) ? data[i] : Convert.ToByte(padding);
                }
                return ShiftJis.GetString(result);
            }

            #endregion
            #region エスケープ処理

            /// <summary>シングルクォートのエスケープ処理</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string EscapeQuote(string s)
            {
                return string.IsNullOrEmpty(s)
                    ? string.Empty
                    : s.Replace("'", "''");
            }

            /// <summary>DataTable Selectなどのlike で利用するエスケープ処理</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            /// <remarks>エスケープ対象文字：*,%,[,]</remarks>
            public static string EscapeLike(string s)
            {
                return string.IsNullOrEmpty(s)
                    ? string.Empty
                    : System.Text.RegularExpressions.Regex.Replace
                    (s, @"([\*%\[\]])", "[$1]");
            }

            /// <summary>SQL Serverにおけるlike 句のエスケープ処理</summary>
            /// <param name="s"></param>
            /// <returns>プレースホルダを利用する場合でも、エスケープ処理が必要
            /// エスケープ対象文字：_,%,[</returns>
            public static string EscapeSqlLike(string s)
            {
                return string.IsNullOrEmpty(s)
                    ? string.Empty
                    : System.Text.RegularExpressions.Regex.Replace
                    (s, @"([_%\[])", "[$1]");
            }

            /// <summary>バックスラッシュ(\)のエスケープ処理</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string EscapeBackslash(string s)
            {
                return string.IsNullOrEmpty(s)
                    ? string.Empty
                    : s.Replace("\\", "\\\\");
            }

            /// <summary>アンパサンド(&)のエスケープ処理</summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string EscapeAmpersand(string s)
            {
                return string.IsNullOrEmpty(s)
                    ? string.Empty
                    : s.Replace("&", "&&");
            }

            #endregion
            #region EB用文字変換

            /// <summary>EBデータ使用不可文字</summary>
            public static readonly List<char> invalidEBChars = new List<char>()
        {
            '､',
            ':',
            ';',
            '<',
            '>',
            '&',
            '^',
            '%',
            '#',
            '?',
            '@',
            '$',
            '|',
            '_',
            '\'',
            '[',
            ']',
            '+',
            '*',
            '=',
            '!',
            '"',
        };

            /// <summary>EB使用不可文字のうち、変換を行うもの</summary>
            public static readonly Dictionary<char, char> replaceEBChars = new Dictionary<char, char>()
        {
            {'｡', '.'},
            {'･', '.'},
            {'ｰ', '-'},
            {'ｧ', 'ｱ'},
            {'ｨ', 'ｲ'},
            {'ｩ', 'ｳ'},
            {'ｪ', 'ｴ'},
            {'ｫ', 'ｵ'},
            {'ｬ', 'ﾔ'},
            {'ｭ', 'ﾕ'},
            {'ｮ', 'ﾖ'},
            {'ｯ', 'ﾂ'},
        };

            public static string ToValidEBKana(string value)
            {
                return string.IsNullOrEmpty(value)
                    ? string.Empty
                    : new string(ToHankakuKatakana(value)
                    .Where(c => !invalidEBChars.Contains(c))
                    .Select(c => replaceEBChars.ContainsKey(c) ? replaceEBChars[c] : c)
                    .ToArray()).ToUpper().Trim();
            }
            public static string RemoveNotUseChar(string value)
            {
                return ToValidEBKana(value);
            }

            public static string ToValidEBKanaWithoutAbbreviation(string value, List<string> abbreviations)
            {
                if (string.IsNullOrEmpty(value)) { return string.Empty; }
                value = ToValidEBKana(value);
                var patterns = new string[] { "({0})", "{0})", "({0}" };
                foreach (var symbols in abbreviations)
                    foreach (var pattern in patterns)
                    {
                        var target = string.Format(pattern, symbols);
                        if (!value.Contains(target)) continue;
                        value = value.Replace(target, "");
                    }
                return value.Trim();
            }

            public static string RemoveHojinKaku(string value, List<string> abbreviations)
            {
                return ToValidEBKanaWithoutAbbreviation(value, abbreviations);
            }
            public static string RemoveHojinkaku(string value)
            {
                return ToValidEBKanaWithoutAbbreviation(value, HojinkakuMasterList);
                //            return value;
            }
            public static List<string> HojinkakuMasterList = new List<string> {
            "ｼﾖｸﾊﾝｷﾖｳ",
            "ｺｳｷﾖｳﾚﾝ",
            "ｺｸｷﾖｳﾚﾝ",
            "ﾉｳｷﾖｳﾚﾝ",
            "ｹｲｻﾞｲﾚﾝ",
            "ｷﾖｳｻｲﾚﾝ",
            "ｶｲｼﾞﾖｳ",
            "ｼﾞﾕｳｸﾐ",
            "ｷﾞﾖｷﾖｳ",
            "ｾｲｷﾖｳ",
            "ｷﾞﾖﾚﾝ",
            "ｼﾔｷﾖｳ",
            "ｼﾖｸｱﾝ",
            "ｷﾖｳｻｲ",
            "ｷﾖｳｸﾐ",
            "ｺｸﾎﾚﾝ",
            "ﾛｳｸﾐ",
            "ｺｳﾈﾝ",
            "ｾｲﾒｲ",
            "ﾄｸﾖｳ",
            "ﾕｳｸﾐ",
            "ｹﾝﾎﾟ",
            "ｺｸﾎ",
            "ｼﾔﾎ",
            "ｶｻｲ",
            "ﾄｸﾋ",
            "ｶﾝﾘ",
            "ﾛｳﾑ",
            "ｶﾌﾞ",
            "ｻﾞｲ",
            "ｼﾕﾂ",
            "ｼﾕｳ",
            "ﾁﾕｳ",
            "ｶﾞｸ",
            "ﾎｺﾞ",
            "ﾕｳ",
            "ｼﾔ",
            "ﾌｸ",
            "ｴｲ",
            "ﾚﾝ",
            "ﾄﾞｸ",
            "ﾍﾞﾝ",
            "ｷﾞﾖ",
            "ｼﾎｳ",
            "ｾﾞｲ",
            "ﾀﾞｲ",
            "ﾉｳ",
            "ﾄﾞ",
            "ﾕ",
            "ﾒ",
            "ｿ",
            "ｼ",
            "ｶ",
            "ｲ",
        };
            public static bool IsValidEBChars(string value)
            {
                return System.Text.RegularExpressions.Regex.IsMatch(value,
                    @"^[ 0-9A-Z\-\uFF62-\uFF9F\.\(\)\\/]+$");
            }

            #endregion
            #region その他

            /// <summary>SQL 文字列組立で、空文字の場合に NULL でデータを登録する際に利用するメソッド</summary>
            /// <param name="value"></param>
            /// <returns>空文字の場合 NULL, なんらかの値が入っている場合 'value' として返す</returns>
            /// <remarks>今後、クエリのパラメータ化を進めるにあたり、使用を減らしていく方向</remarks>
            public static string GetNullableText(string value)
            {
                return string.IsNullOrEmpty(value)
                    ? "NULL"
                    : string.Format("'{0}'", EscapeQuote(value));
            }

            public static string GetNullableText(object value)
            {
                var s = Convert.ToString(value);
                return string.IsNullOrEmpty(s)
                    ? "NULL"
                    : string.Format("'{0}'", EscapeQuote(s));
            }

            public static string GetNullableValue(object value)
            {
                var s = Convert.ToString(value);
                return string.IsNullOrEmpty(s)
                    ? "NULL"
                    : s;
            }

            public static string GetNullableDate(object value)
            {
                DateTime dat;
                return DateTime.TryParse(Convert.ToString(value), out dat)
                    ? string.Format("'{0:yyyy/MM/dd}'", dat)
                    : "NULL";
            }

            public static void Clear(StringBuilder v)
            {
                if (v == null) return;
                v.Remove(0, v.Length);
            }

            #endregion その他
        }

    }

}
