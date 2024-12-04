using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using C1.C1Preview;

using IcsComCtrl;
using IcsComPrint;
using IcsComDb;
using IcsSRacDlg.Dialog;



namespace SMTORI
{
    public partial class dlgPrnSetting : DialogEx
    {
        public dlgPrnSetting(int pnMode)
        {
            InitializeComponent();
            nMode = pnMode;
            // v120201 印刷関連エラー回避のためコメント化
            //PrnInit();
        }

        /// <summary>
        /// Printクラスのインスタンス
        /// </summary>
        private prnSMTORI cPrnSMTORI;
        private int nMode = 0;
        //internal DialogManager DlgMng = new DialogManager(Global.cConKaisya);
        //internal DialogManager DlgMng = new DialogManager(1, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
        internal static IcsSRacDlg.Dialog.DialogManager DlgMng;

        
        /// <summary>
        /// ダイアログload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dlgPrnSetting_Load(object sender, EventArgs e)
        {
            #region
            Rdo_T.Checked = true;
            Rdo_All.Checked = true;
            Radio_SortTRCD.Checked = true;
            #endregion
            Txt_HJCD_Sta.ReadOnlyEx = true;
            Txt_HJCD_End.ReadOnlyEx = true;
            
            Txt_TRCD_Sta.ExTextBoxType = eTextBoxType.Code;
            Txt_TRCD_End.ExTextBoxType = eTextBoxType.Code;
            Txt_TRCD_Sta.ExCodeType = Global.nTRCD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
            Txt_TRCD_End.ExCodeType = Global.nTRCD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
            Txt_TRCD_Sta.ExCodeLength = Global.nTRCD_Len;
            Txt_TRCD_End.ExCodeLength = Global.nTRCD_Len;

            // 一見取引先は出力しない
            Global.Prn_PType = 1;

            DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 3, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);

            //if ( Global.nExpPrn == 2 )
            //{
            //    //印刷
            //    Rdo_D.Enabled = true;
            //    Grp_PrtItem.Visible = true;
            //    BtnOK.Top = 450;
            //    BtnCancel.Top = 450;
            //    this.Height = 530;
            //}
            //else
            //{ 
            //    //エクスポート
            //    Rdo_D.Enabled = false;
            //    Grp_PrtItem.Visible = false;
            //    BtnOK.Top = 360;
            //    BtnCancel.Top = 360;
            //    this.Height = 440;
            //}

            if (Global.nSAIKEN_F == 0)
            {
                rdo_Tori_Saiken.Enabled = false;
            }
            if (Global.nSAIMU_F == 0)
            {
                rdo_Tori_Saimu.Enabled = false;
            }
            if (Global.nKIJITU_F == 0)
            {
                Chk_Kijitsu_Only.Enabled = false;
            }

            OutputItemControl();
        }


        /// <summary>
        /// 取引先CD(先頭)50音検索ボタンを押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_TRCD_Sta_Click(object sender, EventArgs e)
        {
            bool bHJCD = (Global.nTRCD_HJ == 1 ? true : false);

            DialogManager.SToriData toriData = null;
            if (rdo_Tori_All.Checked == true)
            {
                 toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
            }
            else if (rdo_Tori_Saiken.Checked == true)
            {
                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
            }
            else
            {
                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
            }

            if (toriData != null)
            {
                Txt_TRCD_Sta.ExCodeDB = toriData.COD;
                if (bHJCD == true)
                {
                    Txt_HJCD_Sta.ReadOnlyEx = false;
                    Txt_HJCD_Sta.Text = toriData.HOJO.ToString().PadLeft(6, '0');
                    TxtToriStaNam.Text = toriData.RYAKU;

                }
                else
                {
                    Txt_HJCD_Sta.ReadOnlyEx = true;
                    Txt_HJCD_Sta.Text = "";
                    TxtToriStaNam.Text = toriData.RYAKU;
                }
                SendKeys.Send("{TAB}");
            }

            //if (Rdo_Z.Checked)
            //{
            //    DialogManager.ZToriData srcZToriData = DlgMng.DispZTORI("");
            //    if(srcZToriData != null)
            //    {
            //        Txt_TRCD_Sta.ExCodeDB = srcZToriData.COD;
            //        TxtToriStaNam.Text = srcZToriData.RYAKU;
            //        SendKeys.Send("{TAB}");
            //    }
            //}
            //else if (Rdo_SS.Checked)
            //{
            //    DialogManager.SToriData srcToriData = DlgMng.DispTTORI("", bHJCD);
            //    if (srcToriData != null)
            //    {
            //        Txt_TRCD_Sta.ExCodeDB = srcToriData.COD;
            //        if (bHJCD == true)
            //        {
            //            Txt_HJCD_Sta.ReadOnlyEx = false;
            //            Txt_HJCD_Sta.Text = srcToriData.HOJO.ToString().PadLeft(6, '0');
            //            TxtToriStaNam.Text = srcToriData.RYAKU;

            //        }
            //        else
            //        {
            //            Txt_HJCD_Sta.ReadOnlyEx = true;
            //            Txt_HJCD_Sta.Text = "";
            //            TxtToriStaNam.Text = srcToriData.RYAKU;
            //        }
            //        SendKeys.Send("{TAB}");
            //    }
            //}
            //else if(Rdo_All.Checked)
            //{
            //    DialogManager.ALLToriData srcToriData = DlgMng.DispTALLTORI("", bHJCD);
            //    if (srcToriData != null)
            //    {
            //        Txt_TRCD_Sta.ExCodeDB = srcToriData.COD;
            //        if (bHJCD == true)
            //        {
            //            Txt_HJCD_Sta.ReadOnlyEx = false;
            //            if (srcToriData.HOJO.ToString() != null && srcToriData.HOJO.ToString() != "")
            //            {
            //                Txt_HJCD_Sta.Text = srcToriData.HOJO.ToString().PadLeft(6, '0');
            //            }
            //            else
            //            {
            //                Txt_HJCD_Sta.Text = "";
            //            }
            //            TxtToriStaNam.Text = srcToriData.RYAKU;

            //        }
            //        else
            //        {
            //            Txt_HJCD_Sta.ReadOnlyEx = true;
            //            Txt_HJCD_Sta.Text = "";
            //            TxtToriStaNam.Text = srcToriData.RYAKU;
            //        }
            //        SendKeys.Send("{TAB}");
            //    }
            //}
        }


        /// <summary>
        /// 取引先CD(末尾)50音検索ボタンを押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_TRCD_End_Click(object sender, EventArgs e)
        {
            bool bHJCD = (Global.nTRCD_HJ == 1 ? true : false);

            DialogManager.SToriData toriData = null;
            if (rdo_Tori_All.Checked == true)
            {
                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
            }
            else if (rdo_Tori_Saiken.Checked == true)
            {
//-- <2016/02/18 S_WORD.NASHIへ>
//                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NYUKIN);
                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
//-- <2016/02/18>
            }
            else
            {
//-- <2016/02/18 S_WORD.NASHIへ>
//                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.SHIHARAI);
                toriData = DlgMng.DispTORI("", bHJCD, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
//-- <2016/02/18>
            }

            if (toriData != null)
            {
                Txt_TRCD_End.ExCodeDB = toriData.COD;
                if (bHJCD == true)
                {
                    Txt_HJCD_End.ReadOnlyEx = false;
                    Txt_HJCD_End.Text = toriData.HOJO.ToString().PadLeft(6, '0');
                    TxtToriEndNam.Text = toriData.RYAKU;

                }
                else
                {
                    Txt_HJCD_End.ReadOnlyEx = true;
                    Txt_HJCD_End.Text = "";
                    TxtToriEndNam.Text = toriData.RYAKU;
                }
                SendKeys.Send("{TAB}");
            }

            //if (Rdo_Z.Checked)
            //{
            //    DialogManager.ZToriData srcZToriData = DlgMng.DispZTORI("");
            //    if (srcZToriData != null)
            //    {
            //        Txt_TRCD_End.ExCodeDB = srcZToriData.COD;
            //        TxtToriEndNam.Text = srcZToriData.RYAKU;
            //        SendKeys.Send("{TAB}");
            //    }
            //}
            //else if (Rdo_SS.Checked)
            //{
            //    DialogManager.SToriData srcToriData = DlgMng.DispTTORI("", bHJCD);
            //    if (srcToriData != null)
            //    {
            //        Txt_TRCD_End.ExCodeDB = srcToriData.COD;
            //        if (bHJCD == true)
            //        {
            //            Txt_HJCD_End.ReadOnlyEx = false;
            //            Txt_HJCD_End.Text = srcToriData.HOJO.ToString().PadLeft(6, '0');
            //            TxtToriEndNam.Text = srcToriData.RYAKU;
            //        }
            //        else
            //        {
            //            Txt_HJCD_End.ReadOnlyEx = true;
            //            Txt_HJCD_End.Text = "";
            //            TxtToriEndNam.Text = srcToriData.RYAKU;
            //        }
            //        SendKeys.Send("{TAB}");
            //    }
            //}
            //else if (Rdo_All.Checked)
            //{
            //    DialogManager.ALLToriData srcToriData = DlgMng.DispTALLTORI("", bHJCD);
            //    if (srcToriData != null)
            //    {
            //        Txt_TRCD_End.ExCodeDB = srcToriData.COD;
            //        if (bHJCD == true)
            //        {
            //            Txt_HJCD_End.ReadOnlyEx = false;
            //            if (srcToriData.HOJO.ToString() != null && srcToriData.HOJO.ToString() != "")
            //            {
            //                Txt_HJCD_End.Text = srcToriData.HOJO.ToString().PadLeft(6, '0');
            //            }
            //            else
            //            {
            //                Txt_HJCD_End.Text = "";
            //            }
            //            TxtToriEndNam.Text = srcToriData.RYAKU;
            //        }
            //        else
            //        {
            //            Txt_HJCD_End.ReadOnlyEx = true;
            //            Txt_HJCD_End.Text = "";
            //            TxtToriEndNam.Text = srcToriData.RYAKU;
            //        }
            //        SendKeys.Send("{TAB}");
            //    }
            //}
        }


        /// <summary>
        /// OKボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            ValidateChildren();

            int iPrnKind = 0;
            bool bDCheck = false;

            //Prj312
            // 帳表選択
            if (Rdo_D.Checked == true & Global.nExpPrn == 2)
            {
                //台帳
                iPrnKind = 1;
            }
            else
            {
                //取引先一覧
                iPrnKind = 0;
            }

            if (rdo_Tori_All.Checked == true)
            {
                Global.PrnTarget = 0;
            }
            else if (rdo_Tori_Saiken.Checked == true)
            {
                Global.PrnTarget = 1;
            }
            else if (rdo_Tori_Saimu.Checked == true)
            {
                Global.PrnTarget = 2;
            }
            else if (Chk_Kijitsu_Only.Checked == true)
            {
                Global.PrnTarget = 3;
            }

            //入力チェック&選択値を格納
            // ソート順
            Global.Prn_SortKEY = (Radio_SortTRCD.Checked == true ? 0 : 1);
            
            // 改頁設定
            Global.Prn_PagingTRCD = (Chk_PagingTRCD.Checked == true ? 0 : 1);

            // 出力ﾏｽﾀｰ設定(財務・債務)
            //if (Rdo_Z.Checked)
            //{
            //    Global.Prn_PKind = 1;
            //}
            //else if(Rdo_SS.Checked)
            //{
                Global.Prn_PKind = 2;
            //}
            //else
            //{
            //    Global.Prn_PKind = 0;
            //}

            // 取引先(ｺｰﾄﾞ有り)
            if (Global.Prn_PType != 2)
            {
                Global.Prn_TRCD_Sta = (Txt_TRCD_Sta.Text != "" ? Txt_TRCD_Sta.ExCodeDB : "");
                Global.Prn_TRCD_End = (Txt_TRCD_End.Text != "" ? Txt_TRCD_End.ExCodeDB : "");
                if (Global.Prn_PKind != 1 && Global.nTRCD_HJ == 1)
                {
                    Global.Prn_HJCD_Sta = (Txt_HJCD_Sta.Text != "" ? Txt_HJCD_Sta.Text : "");
                    Global.Prn_HJCD_End = (Txt_HJCD_End.Text != "" ? Txt_HJCD_End.Text : "");
                }
                else if (Global.Prn_PKind != 1 && Global.nTRCD_HJ == 0)
                {
                    Global.Prn_HJCD_Sta = "000000";
                    Global.Prn_HJCD_End = "000000";
                }
                else
                {
                    Global.Prn_HJCD_Sta = "";
                    Global.Prn_HJCD_End = "";
                }
            }
            else
            {
                Global.Prn_TRCD_Sta = "";
                Global.Prn_TRCD_End = "";
                Global.Prn_HJCD_Sta = "";
                Global.Prn_HJCD_End = "";
            }

            // 入力可能期間
            Global.Prn_ZSTYMD_Null = Chk_M1.Checked;
            if (!Global.Prn_ZSTYMD_Null)
            {
                Global.Prn_ZSTYMD_Sta = Dat_InStaS.Value;
                Global.Prn_ZSTYMD_End = Dat_InStaE.Value;
            }
            Global.Prn_ZEDYMD_Null = Chk_M2.Checked;
            if (!Global.Prn_ZEDYMD_Null)
            {
                Global.Prn_ZEDYMD_Sta = Dat_InEndS.Value;
                Global.Prn_ZEDYMD_End = Dat_InEndE.Value;
            }

            // 使用可能期間
            Global.Prn_STYMD_Null = Chk_M3.Checked;
            if (!Global.Prn_STYMD_Null)
            {
                Global.Prn_STYMD_Sta = Dat_UseStaS.Value;
                Global.Prn_STYMD_End = Dat_UseStaE.Value;
            }
            Global.Prn_EDYMD_Null = Chk_M4.Checked;
            if (!Global.Prn_EDYMD_Null)
            {
                Global.Prn_EDYMD_Sta = Dat_UseEndS.Value;
                Global.Prn_EDYMD_End = Dat_UseEndE.Value;
            }

            // 出力項目指定(台帳)
            if (iPrnKind == 1)
            {
                if (Chk_Kihon.Checked == true)
                {
                    Global.Prn_Address = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Address = 1;
                }
                if (Chk_Kaisyu.Checked == true)
                {
                    Global.Prn_Kaisyu = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Kaisyu = 1;
                }
                if (Chk_Frigin.Checked == true)
                {
                    Global.Prn_Frigin = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Frigin = 1;
                }
                if (Chk_Shiharai.Checked == true)
                {
                    Global.Prn_Shiharai = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Shiharai = 1;
                }
                if (Chk_Others.Checked == true)
                {
                    Global.Prn_Others = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Others = 1;
                }
                if (Chk_Gaika.Checked == true)
                {
                    Global.Prn_Gaika = 0;
                    bDCheck = true;
                }
                else
                {
                    Global.Prn_Gaika = 1;
                }
//-- <2016/03/15 マスタ情報は画面表示から外し常に印刷>
//                if (Chk_Master.Checked == true)
//                {
                    Global.Prn_Master = 0;
//                    bDCheck = true;
//                }
//                else
//                {
//                    Global.Prn_Master = 1;
//                }
//-- <2016/03/15>
            }

            // チェック処置
            if (Global.Prn_TRCD_Sta != "" && Global.Prn_TRCD_End != "")
            {
                //if (int.Parse(Global.Prn_TRCD_Sta) > int.Parse(Global.Prn_TRCD_End))
                if (Global.Prn_TRCD_Sta.CompareTo(Global.Prn_TRCD_End) > 0)
                {
                    Txt_TRCD_Sta.Focus();
                    MessageBox.Show(
                        "開始　＞　終了になっています。",
//-- <2016/03/22>
//                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    return;
                }
                //else if (int.Parse(Global.Prn_TRCD_Sta) == int.Parse(Global.Prn_TRCD_End))
                else if (Global.Prn_TRCD_Sta.CompareTo(Global.Prn_TRCD_End) == 0)
                {
                    if (Global.Prn_HJCD_Sta != "" && Global.Prn_HJCD_End != "")
                    {
                        if (int.Parse(Global.Prn_HJCD_Sta) > int.Parse(Global.Prn_HJCD_End))
                        {
                            Txt_HJCD_Sta.Focus();
                            MessageBox.Show(
                                "開始　＞　終了になっています。",
//-- <2016/03/22>
//                                "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            return;
                        }
                    }
                }
            }
            if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
            {
                if(Global.Prn_ZSTYMD_Sta > Global.Prn_ZSTYMD_End)
                {
                    Dat_InStaS.Focus();
                    MessageBox.Show(
                        "開始　＞　終了になっています。",
//-- <2016/03/22>
//                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    return;
                }
            }
            if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
            {
                if (Global.Prn_ZEDYMD_Sta > Global.Prn_ZEDYMD_End)
                {
                    Dat_InEndS.Focus();
                    MessageBox.Show(
                        "開始　＞　終了になっています。",
//-- <2016/03/22>
//                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    return;
                }
            }
            if (Global.Prn_STYMD_Sta != 0 && Global.Prn_STYMD_End != 0)
            {
                if (Global.Prn_STYMD_Sta > Global.Prn_STYMD_End)
                {
                    Dat_UseStaS.Focus();
                    MessageBox.Show(
                        "開始　＞　終了になっています。",
//-- <2016/03/22>
//                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    return;
                }
            }
            if (Global.Prn_EDYMD_Sta != 0 && Global.Prn_EDYMD_End != 0)
            {
                if (Global.Prn_EDYMD_Sta > Global.Prn_EDYMD_End)
                {
                    Dat_UseEndS.Focus();
                    MessageBox.Show(
                        "開始　＞　終了になっています。",
//-- <2016/03/22>
//                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    return;
                }
            }
//-- <2016/03/15>
//            if (iPrnKind == 1 && !bDCheck)
            if (iPrnKind == 1 && !bDCheck && !Chk_Kijitsu_Only.Checked)
            {
                Chk_Kihon.Focus();
                MessageBox.Show(
                    "出力指定項目が指定されていません。",
//-- <2016/03/22>
//                    "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    "範囲指定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                return;
            }

            Global.Prn_Kind = iPrnKind;

            // v120201 印刷関連エラー回避
            PrnInit();

            string sPrintTitleH = "";
            string sPrintTitleT = "";

            if (Global.PrnTarget == 0)
            {
                sPrintTitleH = "取引先";
            }
            else if (Global.PrnTarget == 1)
            {
                sPrintTitleH = "得意先";
            }
            else if (Global.PrnTarget == 2)
            {
                sPrintTitleH = "仕入先";
            }
            else
            {
                sPrintTitleH = "取引先";
                sPrintTitleT = "（期日管理のみ使用）";
            }

            if (Sel_DataExist() == true)
            {
                if (Global.nExpPrn == 1)
                {
                    //エクスポート
                    using (IcsSRacDlg.EXPIMP.dlgExpImp dlgExp = new IcsSRacDlg.EXPIMP.dlgExpImp(Global.sCcod, Global.nUcod, Global.sPrgId, 2))
                    {
                        dlgExp.InitFileName = Global.sPrgId + ".csv";
                        if (dlgExp.ShowDialog() == DialogResult.OK)
                        {
                            Global.nComTitle = dlgExp.IsOutputCNAM ? 0 : 1;
                            Global.nComCD = dlgExp.IsOutputCCOD ? 0 : 1;
                            Global.sComment = dlgExp.IsOutputComment ? dlgExp.Comment : String.Empty;

                            string sPrintTitle = sPrintTitleH + "一覧表" + sPrintTitleT;

                           ExportReport(dlgExp, sPrintTitle);

                            // V12.03.02
                            Dispose();
                        }
                    }
                }
                else
                {
                    if (cPrnSMTORI.PrnSetDlg() == true)
                    {
                        if (iPrnKind == 0)
                        {
                            cPrnSMTORI.Print(sPrintTitleH + "一覧表" + sPrintTitleT);
                        }
                        else
                        {
                            cPrnSMTORI.Print(sPrintTitleH + "台帳" + sPrintTitleT);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "対象となるデータが存在しません。",
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK,
//                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    "対象データがありません。",
                    Global.sPrgName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
//-- <2016/03/22>
            }

            // v120201 印刷関連エラー回避
            cPrnSMTORI.Dispose();
        }


        /// <summary>
        /// キャンセルボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //--->V01.12.01 ATT ADD ▼ (7626)
            DialogManager DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 0, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
            //--->V01.12.01 ATT ADD ▲ (7626)
            Dispose();
        }


        /// <summary>
        /// 印刷初期設定
        /// </summary>
        private void PrnInit()
        {
            //印刷設定情報初期化（PrnValのインスタンス作成）
            PrnVal cPrnVal = new PrnVal();
            //印刷動作
            cPrnVal.nTitlSw = 1;
            cPrnVal.nCocdSw = 1;
            cPrnVal.nDateSw = 2;
            cPrnVal.nPaperSize[0] = ePaperSize.A4;
            if (Global.nGengo == 0)
            {
                cPrnVal.nReki = 1;
            }
            else
            {
                cPrnVal.nReki = 0;
            }
            if (Global.Prn_Kind == 1)
            {
                cPrnVal.nOrient = ePaperOrient.Tate;
            }
            else
            {
                cPrnVal.nOrient = ePaperOrient.Yoko;
            }
            cPrnVal.nPageMax = 9999;
            cPrnVal.sSaveFile = Global.sMMDir + @"\AP150\CO" + Global.sCcod + @"\" + Global.sPrgId +
                                Global.nUcod.ToString("0000") + ".dat";
            cPrnVal.nPrevBtn = 1;
            //初期設定
            cPrnVal.nRTitlSw = 1;
            cPrnVal.nRCocdSw = 1;
            cPrnVal.nRDateSw = 2;
            cPrnVal.nRStPage = 1;
            cPrnVal.nREdPage = 9999;
            cPrnVal.nRPaperSize = ePaperSize.A4;
            if (Global.Prn_Kind == 1)
            {
                cPrnVal.nOrient = ePaperOrient.Tate;
            }
            else
            {
                cPrnVal.nOrient = ePaperOrient.Yoko;
            }
            cPrnVal.nRCopyCnt = 1;
            cPrnVal.nRPropSw = 1;
            //印刷出力クラスのインスタンス化
            cPrnSMTORI = new prnSMTORI(this, Global.sPrgId, Global.nUcod, Global.sCcod,
                        Global.cKaisya.nKESN, 0, ref cPrnVal, Global.sPrgName);

            //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
            cPrnSMTORI.bDoPdf = true;
            cPrnSMTORI.bDoDivPrev = true;
            cPrnSMTORI.nUsrSec = Global.cUsrSec.nFFLG;
            cPrnSMTORI.sDefaultFileName = Global.sPrgId + ".pdf";
            //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

        }

        #region データ存在チェック
        /// <summary>
        /// データ存在チェック
        /// </summary>
        public bool Sel_DataExist()
        {
            try
            {
                ValidateChildren();

                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //string sFuncLength = DbCls.DbType == DbCls.eDbType.Oracle ? "LENGTH" : "LEN";
                string sFuncLength = IcsComUtil.ComUtil.IsPostgreSQL() ? "LENGTH" : "LEN";
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

                if (Global.Prn_PKind == 0 || Global.Prn_PKind == 2)
                {
                    #region 債務支払
                    //SS_TORIにレコードが存在するか否かのチェック
                    if (Global.gcDataReader != null)
                    {
                        Global.gcDataReader.Close();
                        Global.gcDataReader.Dispose();
                    }

                    string sWhere1 = "";
                    string sWhere2 = "";

                    string sSubwhere = " AND T.TRCD = COALESCE(ST.TRCD, ' ') ";

                    Global.cCmdSel.CommandText = "SELECT ST.TRCD, ST.HJCD FROM SS_TORI ST WHERE ";

                    #region 通常の取引先
                    if (Global.Prn_PType == 1 || Global.Prn_PType == 0)
                    {

                        if ((Txt_TRCD_Sta.Text != "") &&
                            (Txt_HJCD_Sta.Text != "") &&
                            (Txt_TRCD_End.Text != "") &&
                            (Txt_HJCD_End.Text != ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Txt_TRCD_Sta.ExCodeDB
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Txt_TRCD_Sta.ExCodeDB + "' AND ST.HJCD >= '" + Txt_HJCD_Sta.Text
                                                       + "')) AND (COALESCE(ST.TRCD, ' ') < '" + Txt_TRCD_End.ExCodeDB + "' OR (COALESCE(ST.TRCD, ' ') = '"
                                                       + Txt_TRCD_End.ExCodeDB + "' AND ST.HJCD <= '" + Txt_HJCD_End.Text + "')) "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text != "") &&
                                 (Txt_HJCD_Sta.Text != "") &&
                                 (Txt_TRCD_End.Text != "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Txt_TRCD_Sta.ExCodeDB
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Txt_TRCD_Sta.ExCodeDB + "' AND ST.HJCD >= '" + Txt_HJCD_Sta.Text
                                                       + "')) AND COALESCE(ST.TRCD, ' ') <= '" + Txt_TRCD_End.ExCodeDB + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text != "") &&
                                 (Txt_HJCD_Sta.Text != "") &&
                                 (Txt_TRCD_End.Text == "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') > '" + Txt_TRCD_Sta.ExCodeDB
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Txt_TRCD_Sta.ExCodeDB + "' AND ST.HJCD >= '" + Txt_HJCD_Sta.Text + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text != "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text != "") &&
                                 (Txt_HJCD_End.Text != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Txt_TRCD_Sta.ExCodeDB + "' AND ("
                                                       + "COALESCE(ST.TRCD, ' ') < '" + Txt_TRCD_End.ExCodeDB + "' OR (COALESCE(ST.TRCD, ' ') = '" + Txt_TRCD_End.ExCodeDB
                                                       + "' AND ST.HJCD <= '" + Txt_HJCD_End.Text + "')) AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text != "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text != "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') BETWEEN '" + Txt_TRCD_Sta.ExCodeDB + "' AND '"
                                                       + Txt_TRCD_End.ExCodeDB + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text != "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text == "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Txt_TRCD_Sta.ExCodeDB + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text == "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text != "") &&
                                 (Txt_HJCD_End.Text != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') < '" + Txt_TRCD_End.ExCodeDB
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Txt_TRCD_End.ExCodeDB + "' AND ST.HJCD <= '" + Txt_HJCD_End.Text + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text == "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text != "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') <= '" + Txt_TRCD_End.ExCodeDB + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Txt_TRCD_Sta.Text == "") &&
                                 (Txt_HJCD_Sta.Text == "") &&
                                 (Txt_TRCD_End.Text == "") &&
                                 (Txt_HJCD_End.Text == ""))
                        {
                            sWhere1 = " (" + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                    }
                    #endregion

                    #region 一見取引先
                    if (Global.Prn_PType == 2 || Global.Prn_PType == 0)
                    {
                        if (Global.Prn_TRCD_Once != "")
                        {
                            sWhere2 = string.Format("( ST.TORI_NAM like '%{0}%' AND ", Global.Prn_TRCD_Once);
                        }
                        else
                        {
                            sWhere2 = " ( ";
                        }
                        sWhere2 += sFuncLength + "(COALESCE(ST.TRCD, ' ')) = 13 ) ";

                    }
                    #endregion

                    if (Global.Prn_PType == 1)
                    {
                        Global.cCmdSel.CommandText += sWhere1;
                    }
                    else if (Global.Prn_PType == 2)
                    {
                        Global.cCmdSel.CommandText += sWhere2;
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " ( " + sWhere1 + " OR " + sWhere2 + " ) ";
                    }

                    #region 期間範囲指定
                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + " )";
                        }
                        else if (Global.Prn_ZSTYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + " )";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                    }
                    if (!Global.Prn_ZEDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                        }
                        else if (Global.Prn_ZEDYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + " )";
                        }
                        else if (Global.Prn_ZEDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + " )";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD = 0  " + sSubwhere + ") ";
                    }
                    if (!Global.Prn_STYMD_Null)
                    {
                        if (Global.Prn_STYMD_Sta != 0 && Global.Prn_STYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND ST.STYMD BETWEEN " + Global.Prn_STYMD_Sta + " AND " + Global.Prn_STYMD_End;
                        }
                        else if (Global.Prn_STYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ST.STYMD >= " + Global.Prn_STYMD_Sta + " AND ST.STYMD <> 0)";
                        }
                        else if (Global.Prn_STYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ST.STYMD <= " + Global.Prn_STYMD_End + " OR ST.STYMD = 0)";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND ST.STYMD = 0 ";
                    }
                    if (!Global.Prn_EDYMD_Null)
                    {
                        if (Global.Prn_EDYMD_Sta != 0 && Global.Prn_EDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND ST.EDYMD BETWEEN " + Global.Prn_EDYMD_Sta + " AND " + Global.Prn_EDYMD_End;
                        }
                        else if (Global.Prn_EDYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ST.EDYMD >= " + Global.Prn_EDYMD_Sta + " OR ST.EDYMD = 0)";
                        }
                        else if (Global.Prn_EDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ST.EDYMD <= " + Global.Prn_EDYMD_End + " AND ST.EDYMD <> 0)";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND ST.EDYMD = 0 ";
                    }
                    #endregion
                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                    if (Global.gcDataReader.HasRows == true)
                    {
                        return true;
                    }
                    #endregion
                }
                if ((Global.Prn_PKind == 0 || Global.Prn_PKind == 1) && Global.Prn_PType != 2)
                {
                    #region 財務
                    //TRNAMにレコードが存在するか否かのチェック
                    if (Global.gcDataReader != null)
                    {
                        Global.gcDataReader.Close();
                        Global.gcDataReader.Dispose();
                    }

                    if ((Txt_TRCD_Sta.Text != "") &&
                        (Txt_TRCD_End.Text != ""))
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM " + Global.sZJoin + "TRNAM T WHERE T.TRCD >= " + Global.Prn_TRCD_Sta + " AND T.TRCD <= " + Global.Prn_TRCD_End + " ";
                    }
                    else if ((Txt_TRCD_Sta.Text != "") &&
                             (Txt_TRCD_End.Text == ""))
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM " + Global.sZJoin + "TRNAM T WHERE T.TRCD >= " + Global.Prn_TRCD_Sta + " ";
                    }
                    else if ((Txt_TRCD_Sta.Text == "") &&
                             (Txt_TRCD_End.Text != ""))
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM " + Global.sZJoin + "TRNAM T WHERE T.TRCD <= " + Global.Prn_TRCD_End + " ";
                    }
                    else if ((Txt_TRCD_Sta.Text == "") &&
                             (Txt_TRCD_End.Text == ""))
                    {
                        Global.cCmdSel.CommandText = "SELECT TRCD FROM " + Global.sZJoin + "TRNAM T WHERE T.TRCD is not NULL ";
                    }
                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End;
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND ISTAYMD <> 0) ";
                        }
                        else if (Global.Prn_ZSTYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR ISTAYMD = 0) ";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND ISTAYMD = 0 ";
                    }
                    if (!Global.Prn_ZEDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End;
                        }
                        if (Global.Prn_ZEDYMD_Sta != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR IENDYMD = 0) ";
                        }
                        if (Global.Prn_ZEDYMD_End != 0)
                        {
                            Global.cCmdSel.CommandText += " AND (IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND IENDYMD <> 0) ";
                        }
                    }
                    else
                    {
                        Global.cCmdSel.CommandText += " AND IENDYMD = 0 ";
                    }
                    if (Global.Prn_PKind == 0)
                    {
                        Global.cCmdSel.CommandText += " AND NOT EXISTS(SELECT T.TRCD FROM SS_TORI SS WHERE T.TRCD = SS.TRCD) ";
                    }

                    DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                    if (Global.gcDataReader.HasRows == true)
                    {
                        return true;
                    }
                    #endregion
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_DataExist　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return false;
            }
            finally
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
            }
        }
        #endregion

        //補助CDの0埋め
        private void Txt_HJCD_Sta_Validating(object sender, CancelEventArgs e)
        {
            if (Txt_HJCD_Sta.Text != "")
            {
                Txt_HJCD_Sta.Text = Txt_HJCD_Sta.Text.PadLeft(6, '0');
            }
            TxtToriStaNam.Text = GetTrName(Txt_TRCD_Sta.Text, Txt_HJCD_Sta.Text, 1);
        }


        //補助CDの0埋め
        private void Txt_HJCD_End_Validating(object sender, CancelEventArgs e)
        {
            if (Txt_HJCD_End.Text != "")
            {
                Txt_HJCD_End.Text = Txt_HJCD_End.Text.PadLeft(6, '0');
            }
            TxtToriEndNam.Text = GetTrName(Txt_TRCD_End.Text, Txt_HJCD_End.Text, 1);
        }


        //取引先CDが入力され、補助CD使用が可なら補助コードを有効化、それ以外は補助CDをClear
        private void Txt_TRCD_Sta_Validating(object sender, CancelEventArgs e)
        {
            if ((Global.nTRCD_Type == 0) && (Txt_TRCD_Sta.Text != ""))
            {
                Txt_TRCD_Sta.Text = Txt_TRCD_Sta.Text.PadLeft(Global.nTRCD_Len, '0');
            }
            // --->V01.13.01 HWY DELETE ▼(7627）
            //else if ((Global.nTRCD_Type == 1) && (Txt_TRCD_Sta.Text != ""))
            //{
            //    Txt_TRCD_Sta.Text = Txt_TRCD_Sta.Text.PadRight(Global.nTRCD_Len, ' ').ToUpper();
            //}
            // <---V01.13.01 HWY DELETE ▲(7627)
            if ((Txt_TRCD_Sta.Text != "") &&
                (Global.nTRCD_HJ == 1) && !Rdo_Z.Checked)
            {
                Txt_HJCD_Sta.ReadOnlyEx = false;
            }
            else
            {
                Txt_HJCD_Sta.ReadOnlyEx = true;
                Txt_HJCD_Sta.ClearValue();
            }
            TxtToriStaNam.Text = GetTrName(Txt_TRCD_Sta.Text, Txt_HJCD_Sta.Text, 1);
        }


        //取引先CDが入力され、補助CD使用が可なら補助コードを有効化、それ以外は補助CDをClear
        private void Txt_TRCD_End_Validating(object sender, CancelEventArgs e)
        {
            if ((Global.nTRCD_Type == 0) && (Txt_TRCD_End.Text != ""))
            {
                Txt_TRCD_End.Text = Txt_TRCD_End.Text.PadLeft(Global.nTRCD_Len, '0');
            }
            //2013/07/16 ICS.居軒 ▼開始側の取引先が入力されているとき、終了側の取引先をクリアしても終了側の補助コードがクリアされない不具合修正
            //else if ((Global.nTRCD_Type == 1) && (Txt_TRCD_Sta.Text != ""))
            // --->V01.13.01 HWY DELETE ▼(7627)
            //else if ((Global.nTRCD_Type == 1) && (Txt_TRCD_End.Text != ""))
            ////2013/07/16 ICS.居軒 ▲開始側の取引先が入力されているとき、終了側の取引先をクリアしても終了側の補助コードがクリアされない不具合修正
            //{
            //    Txt_TRCD_End.Text = Txt_TRCD_End.Text.PadRight(Global.nTRCD_Len, ' ').ToUpper();
            //}
            // <---V01.13.01 HWY DELETE ▲(7627)

            if ((Txt_TRCD_End.Text != "") &&
                (Global.nTRCD_HJ == 1) && !Rdo_Z.Checked)
            {
                Txt_HJCD_End.ReadOnlyEx = false;
            }
            else
            {
                Txt_HJCD_End.ReadOnlyEx = true;
                Txt_HJCD_End.ClearValue();
            }
            TxtToriEndNam.Text = GetTrName(Txt_TRCD_End.Text, Txt_HJCD_End.Text, 1);
        }


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 取引先CDの入力制御
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Txt_TRCD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (Global.nTRCD_Type == 0)
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    else
        //    {
        //-- <2016/03/10 禁則設定がたりない>
        //                if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b')
        //       if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '-')
        //-- <2016/03/10>                
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)

        #region エクスポート
        #region Prj312【エクスポート】



        /// <summary>
        /// 文字列を引用符で囲みます
        /// 【引数】
        /// 文字列データ
        /// 【戻り値】
        /// 空文字以外なら二重引用符で囲んだ結果を返します
        /// </summary>
        /// <param name="strP_Data"></param>
        /// <returns></returns>
        public static string Lap_Data(string strP_Data)
        {
            #region Actions
            if (strP_Data == "")
            {
                return "";
            }
            return "\"" + strP_Data + "\"";
            #endregion
        }

        /// <summary>
        /// 取引先名称打ち出し
        /// 取引先登録のみで使用。
        /// 現在は　ExpTrnm0　→　取引先登録
        /// 現在は　ExpTrnm　 →　それ以外
        /// </summary>
        /// <param name="nMode">0:取引先名称1:科目取引先残高各月2:科目取引先予算</param>
        /// <param name="strCode_Dsp"></param>
        /// <param name="strName_Dsp"></param>
        /// <returns></returns>
        internal void ExportReport(IcsSRacDlg.EXPIMP.dlgExpImp dlg, string PrintExportTitle)
        {
            #region Actions
            string exportPath = Path.Combine(dlg.ExpImpPath, dlg.ExpImpFileName);

            //CSVファイルに書き込むときに使うEncoding
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("Shift_JIS");

            //開く
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(exportPath, false, enc))
            try
            {
                Sel_TRCD_List_E();

                // ﾍﾟｰｼﾞ印刷
                if (Global.gcDataReader.HasRows == false)
                {
                    MessageBox.Show("エクスポート件数が0件です", "エクスポート処理", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
                }
                else
                {
                    #region ヘッダ

                    #region ヘッダデータ
                    string sComNam = "";
                    string sComnt = "";
                    string sCompanyTitle = "";
                    if (Global.nComTitle == 0)
                    {
                        sCompanyTitle = "会社名 ";
                    }
                    string sCompanyCode = "";
                    if (Global.nComCD == 0)
                    {
                        sCompanyCode = string.Format("[{0}]", Global.sCcod);
                    }
                    // Ver.01.02.05 Toda -->
                    //sComNam = sCompanyTitle + sCompanyCode + Global.cKaisya.sCNAM;
                    sComNam = sCompanyTitle + sCompanyCode + IcsSSSInfo.SSSInfo.sCNAM;
                    // Ver.01.02.05 <--
                    sComnt = Global.sComment;
                    #endregion

                    string[] sHead = new string[13];
                    sHead[0] = ",";
                    sHead[1] = ",";
                    sHead[2] = ",";
                    sHead[3] = ",";
                    sHead[4] = ",";
                    sHead[5] = ",";
                    sHead[6] = "\"" + PrintExportTitle + "\"";
                    sHead[7] = ",";
                    sHead[8] = ",";
                    sHead[9] = ",";
                    sHead[10] = ",";
                    sHead[11] = ",";
                    sHead[12] = "";
                    sw.WriteLine(gSet(sHead));
                    sHead[0] = sComNam + ",";
                    sHead[1] = ",";
                    sHead[2] = ",";
                    sHead[3] = ",";
                    sHead[4] = ",";
                    sHead[5] = ",";
                    sHead[6] = ",";
                    sHead[7] = ",";
                    sHead[8] = ",";
                    sHead[9] = ",";
                    sHead[10] = ",";
                    sHead[11] = ",";
                    sHead[12] = "\"1頁\"";
                    sw.WriteLine(gSet(sHead));
                    sHead[0] = ",";
                    sHead[1] = ",";
                    sHead[2] = ",";
                    sHead[3] = ",";
                    sHead[4] = ",";
                    sHead[5] = ",";
                    sHead[6] = ",";
                    sHead[7] = ",";
                    sHead[8] = ",";
                    sHead[9] = ",";
                    sHead[10] = ",";
                    sHead[11] = ",";
                    sHead[12] = sComnt + ",";
                    sw.WriteLine(gSet(sHead));
                    sHead[0] = "\"取引先コード\",";
                    sHead[1] = "\"補助コード\",";
                    sHead[2] = "\"取引先名称\",";
                    sHead[3] = "\"取引先(正式)名称\",";
                    sHead[4] = "\"カナ\",";
                    sHead[5] = "\"財務\",";
                    sHead[6] = "\"債務\",";
                    sHead[7] = "\"入力開始日\",";
                    sHead[8] = "\"入力終了日\",";
                    sHead[9] = "\"使用開始日\",";
                    sHead[10] = "\"使用終了日\",";
                    sHead[11] = "\"科目\",";
                    sHead[12] = "\"手形\"";
                    sw.WriteLine(gSet(sHead));
                    #endregion

                    #region エクスポートデータ
                    string[] sDate = new string[13];
                    while (Global.gcDataReader.Read())
                    {
                        // 財務と債務共に存在する場合、財務のデータ行は出力しない
                        if (Global.Prn_PKind == 0 && Global.gcDataReader["TYP"].ToString() == "Z" && Global.gcDataReader["SSFLG"].ToString() == "2")
                        {
                            continue;
                        }
                        string sTRCD = "";
                        if ((Global.nTRCD_Type == 0) &&
                            (Global.nTRCD_ZE == 1))
                        {
                            sTRCD = Global.gcDataReader["TRCD"].ToString().TrimStart('0');
                        }
                        else if (Global.nTRCD_Type == 1)
                        {
                            sTRCD = "\"" + Global.gcDataReader["TRCD"].ToString() + "\"";
                        }
                        else
                        {
                            sTRCD = Global.gcDataReader["TRCD"].ToString();
                        }
                        if (sTRCD.Length == 13)
                        {
                            sDate[0] = ",";
                            sDate[1] = ",";
                        }
                        else
                        {
                            sDate[0] = sTRCD + ",";
                            if (Global.nTRCD_HJ == 1)
                            {
                                if (Global.gcDataReader["HJCD"] == null || Global.gcDataReader["HJCD"] == DBNull.Value)
                                {
                                    sDate[1] = ",";
                                }
                                else
                                {
                                    sDate[1] = Global.gcDataReader["HJCD"].ToString().PadLeft(6, '0') + ",";
                                }
                            }
                            else
                            {
                                sDate[1] = ",";
                            }
                        }
                        sDate[2] = "\"" + Global.gcDataReader["RYAKU"].ToString() + "\",";
                        sDate[3] = "\"" + Global.gcDataReader["TORI_NAM"].ToString() + "\",";
                        sDate[4] = "\"" + Global.gcDataReader["KNLD"].ToString() + "\",";
                        sDate[5] = "\"" + (Global.gcDataReader["ZFLG"].ToString() == "1" ? "○" : "×") + "\",";
                        sDate[6] = "\"" + (Global.gcDataReader["SSFLG"].ToString() == "1" || Global.gcDataReader["SSFLG"].ToString() == "2" ? "○" : "×") + "\",";
                        if (Global.gcDataReader["ISTAYMD"].ToString() != "0" && Global.gcDataReader["ISTAYMD"].ToString() != "")
                        {
                            sDate[7] = "\"" + Global.gcDataReader["ISTAYMD"].ToString().Insert(6, "/").Insert(4, "/") + "\",";
                        }
                        else
                        {
                            sDate[7] = ",";
                        }
                        if (Global.gcDataReader["IENDYMD"].ToString() != "0" && Global.gcDataReader["IENDYMD"].ToString() != "")
                        {
                            sDate[8] = "\"" + Global.gcDataReader["IENDYMD"].ToString().Insert(6, "/").Insert(4, "/") + "\",";
                        }
                        else
                        {
                            sDate[8] = ",";
                        }
                        if (Global.gcDataReader["STYMD"].ToString() != "0" && Global.gcDataReader["STYMD"].ToString() != "")
                        {
                            sDate[9] = "\"" + Global.gcDataReader["STYMD"].ToString().Insert(6, "/").Insert(4, "/") + "\",";
                        }
                        else
                        {
                            sDate[9] = ",";
                        }
                        if (Global.gcDataReader["EDYMD"].ToString() != "0" && Global.gcDataReader["EDYMD"].ToString() != "")
                        {
                            sDate[10] = "\"" + Global.gcDataReader["EDYMD"].ToString().Insert(6, "/").Insert(4, "/") + "\",";
                        }
                        else
                        {
                            sDate[10] = ",";
                        }
                        sDate[11] = "\"" + (Global.gcDataReader["KMK"].ToString() == "1" ? "○" : "×") + "\",";
                        sDate[12] = "\"" + (Global.gcDataReader["TGASW"].ToString() == "1" ? "○" : "×") + "\"";
                        sw.WriteLine(gSet(sDate));
                    }
                    #endregion
                    sw.Close();
                }

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
                //}
            
            #endregion
            
        }

        /// <summary>
        /// 取引先リストに出力するデータを取得(エクスポート用)
        /// </summary>
        internal static void Sel_TRCD_List_E()
        {
            #region 取引先一覧表のデータ取得SQL作成
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }

                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //string sFuncLength = DbCls.DbType == DbCls.eDbType.Oracle ? "LENGTH" : "LEN";
                string sFuncLength = IcsComUtil.ComUtil.IsPostgreSQL() ? "LENGTH" : "LEN";
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】

                string sCmd1 = "";
                string sCmd2 = "";

                //where句の生成
                string sSqlWhere = "";
                string sWhere1 = "";
                string sWhere2 = "";

                sSqlWhere = " WHERE ";

                string sSubwhere = " AND T.TRCD = COALESCE(ST.TRCD, ' ') ";

                if (Global.Prn_PKind == 2 || Global.Prn_PKind == 0)
                {
                    if (Global.Prn_PType == 1 || Global.Prn_PType == 0)
                    {
                        if ((Global.Prn_TRCD_Sta != "") &&
                            (Global.Prn_HJCD_Sta != "") &&
                            (Global.Prn_TRCD_End != "") &&
                            (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta
                                                       + "')) AND (COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End + "' OR (COALESCE(ST.TRCD, ' ') = '"
                                                       + Global.Prn_TRCD_End + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "')) "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta != "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " ((COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta
                                                       + "')) AND COALESCE(ST.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta != "") &&
                                 (Global.Prn_TRCD_End == "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') > '" + Global.Prn_TRCD_Sta
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_Sta + "' AND ST.HJCD >= '" + Global.Prn_HJCD_Sta + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND ("
                                                       + "COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_End
                                                       + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "')) AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') BETWEEN '" + Global.Prn_TRCD_Sta + "' AND '"
                                                       + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta != "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End == "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta == "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End != ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') < '" + Global.Prn_TRCD_End
                                                       + "' OR (COALESCE(ST.TRCD, ' ') = '" + Global.Prn_TRCD_End + "' AND ST.HJCD <= '" + Global.Prn_HJCD_End + "') "
                                                       + "AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else if ((Global.Prn_TRCD_Sta == "") &&
                                 (Global.Prn_HJCD_Sta == "") &&
                                 (Global.Prn_TRCD_End != "") &&
                                 (Global.Prn_HJCD_End == ""))
                        {
                            sWhere1 = " (COALESCE(ST.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' AND " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                        else
                        {
                            sWhere1 = " (" + sFuncLength + "(COALESCE(ST.TRCD, ' ')) < 13) ";
                        }
                    }
                    if (Global.Prn_PType == 2 || Global.Prn_PType == 0)
                    {
                        if (Global.Prn_TRCD_Once != "")
                        {
                            sWhere2 = string.Format("( ST.TORI_NAM like '%{0}%' AND ", Global.Prn_TRCD_Once);
                        }
                        else
                        {
                            sWhere2 = " ( ";
                        }
                        sWhere2 += sFuncLength + "(COALESCE(ST.TRCD, ' ')) = 13 ) ";

                    }
                    if (Global.Prn_PType == 1)
                    {
                        sSqlWhere += sWhere1;
                    }
                    else if (Global.Prn_PType == 2)
                    {
                        sSqlWhere += sWhere2;
                    }
                    else
                    {
                        sSqlWhere += " ( " + sWhere1 + " OR " + sWhere2 + " ) ";
                    }

                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End + sSubwhere + ")";
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND T.ISTAYMD <> 0) " + sSubwhere + " )";
                        }
                        else if (Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR T.ISTAYMD = 0) " + sSubwhere + " )";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND EXISTS(SELECT T.ISTAYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.ISTAYMD = 0  " + sSubwhere + ") ";
                    }
                    if (!Global.Prn_EDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End + sSubwhere + ")";
                        }
                        else if (Global.Prn_ZEDYMD_Sta != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR T.IENDYMD = 0) " + sSubwhere + " )";
                        }
                        else if (Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE (T.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND T.IENDYMD <> 0) " + sSubwhere + " )";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND EXISTS(SELECT T.IENDYMD FROM " + Global.sZJoin + "TRNAM T WHERE T.IENDYMD = 0 " + sSubwhere + ") ";
                    }
                    if (!Global.Prn_STYMD_Null)
                    {
                        if (Global.Prn_STYMD_Sta != 0 && Global.Prn_STYMD_End != 0)
                        {
                            sSqlWhere += " AND ST.STYMD BETWEEN " + Global.Prn_STYMD_Sta + " AND " + Global.Prn_STYMD_End;
                        }
                        else if (Global.Prn_STYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ST.STYMD >= " + Global.Prn_STYMD_Sta + " AND ST.STYMD <> 0)";
                        }
                        else if (Global.Prn_STYMD_End != 0)
                        {
                            sSqlWhere += " AND (ST.STYMD <= " + Global.Prn_STYMD_End + " OR ST.STYMD = 0)";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ST.STYMD = 0 ";
                    }
                    if (!Global.Prn_EDYMD_Null)
                    {
                        if (Global.Prn_EDYMD_Sta != 0 && Global.Prn_EDYMD_End != 0)
                        {
                            sSqlWhere += " AND ST.EDYMD BETWEEN " + Global.Prn_EDYMD_Sta + " AND " + Global.Prn_EDYMD_End;
                        }
                        else if (Global.Prn_EDYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ST.EDYMD >= " + Global.Prn_EDYMD_Sta + " OR ST.EDYMD = 0)";
                        }
                        else if (Global.Prn_EDYMD_End != 0)
                        {
                            sSqlWhere += " AND (ST.EDYMD <= " + Global.Prn_EDYMD_End + " AND ST.EDYMD <> 0)";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ST.EDYMD = 0 ";
                    }

                    //取引先の検索SQL生成&実行
                    sCmd1 = "SELECT ST.TRCD TRCD, ST.HJCD HJCD, ST.RYAKU RYAKU, ST.TORI_NAM TORI_NAM, ST.KNLD KNLD, ";
                    sCmd1 += " ST.TGASW TGASW, CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRNAM Z WHERE COALESCE(Z.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > '0' THEN '1' ELSE 0 END ZFLG, '1' SSFLG, ";
                    sCmd1 += " ZT2.ISTAYMD ISTAYMD, ZT2.IENDYMD IENDYMD, ST.STYMD STYMD, ST.EDYMD EDYMD, ";
                    sCmd1 += " CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ST.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'SS' TYP ";
                    //sCmd1 += " ,CASE WHEN " + sFuncLength + "(COALESCE(ST.TRCD, ' ')) = 13 THEN '1' ELSE '0' END VFLG FROM SS_TORI ST ";
                    sCmd1 += " FROM SS_TORI ST ";
                    sCmd1 += " LEFT JOIN " + Global.sZJoin + "TRNAM ZT2 ON COALESCE(ST.TRCD, ' ') = COALESCE(ZT2.TRCD, ' ') " + sSqlWhere + " ";
                }
                if (Global.Prn_PKind == 1 || Global.Prn_PKind == 0)
                {
                    if ((Global.Prn_TRCD_Sta != "") &&
                        (Global.Prn_TRCD_End != ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' AND COALESCE(ZT.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta != "") &&
                        (Global.Prn_TRCD_End == ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') >= '" + Global.Prn_TRCD_Sta + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta == "") &&
                        (Global.Prn_TRCD_End != ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') <= '" + Global.Prn_TRCD_End + "' ";
                    }
                    else if ((Global.Prn_TRCD_Sta == "") &&
                        (Global.Prn_TRCD_End == ""))
                    {
                        sSqlWhere = " COALESCE(ZT.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ";
                    }
                    if (!Global.Prn_ZSTYMD_Null)
                    {
                        if (Global.Prn_ZSTYMD_Sta != 0 && Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND ZT.ISTAYMD BETWEEN " + Global.Prn_ZSTYMD_Sta + " AND " + Global.Prn_ZSTYMD_End;
                        }
                        else if (Global.Prn_ZSTYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ZT.ISTAYMD >= " + Global.Prn_ZSTYMD_Sta + " AND ZT.ISTAYMD <> 0) ";
                        }
                        else if (Global.Prn_ZSTYMD_End != 0)
                        {
                            sSqlWhere += " AND (ZT.ISTAYMD <= " + Global.Prn_ZSTYMD_End + " OR ZT.ISTAYMD = 0) ";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ZT.ISTAYMD = 0 ";
                    }
                    if (!Global.Prn_EDYMD_Null)
                    {
                        if (Global.Prn_ZEDYMD_Sta != 0 && Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND ZT.IENDYMD BETWEEN " + Global.Prn_ZEDYMD_Sta + " AND " + Global.Prn_ZEDYMD_End;
                        }
                        else if (Global.Prn_ZEDYMD_Sta != 0)
                        {
                            sSqlWhere += " AND (ZT.IENDYMD >= " + Global.Prn_ZEDYMD_Sta + " OR ZT.IENDYMD = 0) ";
                        }
                        else if (Global.Prn_ZEDYMD_End != 0)
                        {
                            sSqlWhere += " AND (ZT.IENDYMD <= " + Global.Prn_ZEDYMD_End + " AND ZT.IENDYMD <> 0) ";
                        }
                    }
                    else
                    {
                        sSqlWhere += " AND ZT.IENDYMD = 0 ";
                    }
                    sCmd2 = "SELECT ZT.TRCD TRCD, null HJCD, ZT.TRMX RYAKU, ZT.TRNAM TORI_NAM, ZT.RNLD KNLD, ";
                    sCmd2 += " '0' TGASW, '1' ZFLG, CASE WHEN ( SELECT COUNT(*) FROM SS_TORI SS WHERE COALESCE(SS.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') AND SS.HJCD = 0 ) > 0 THEN '2' ";
                    sCmd2 += " WHEN ( SELECT COUNT(*) FROM SS_TORI SS WHERE COALESCE(SS.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END SSFLG, ";
                    sCmd2 += " ZT.ISTAYMD ISTAYMD, ZT.IENDYMD IENDYMD, '0' STYMD, '0' EDYMD, ";
                    //sCmd2 += " ,CASE WHEN ( SELECT COUNT(*) FROM TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'Z' TYP, '0' VFLG FROM TRNAM ZT ";
                    sCmd2 += " CASE WHEN ( SELECT COUNT(*) FROM " + Global.sZJoin + "TRZAN ZN WHERE COALESCE(ZN.TRCD, ' ') = COALESCE(ZT.TRCD, ' ') ) > 0 THEN '1' ELSE '0' END KMK, 'Z' TYP FROM " + Global.sZJoin + "TRNAM ZT ";
                    sCmd2 += "WHERE " + sSqlWhere + " ";
                }
                if (Global.Prn_PKind == 2 || Global.Prn_PType == 2)
                {
                    Global.cCmdSel.CommandText = sCmd1;
                }
                else if (Global.Prn_PKind == 1)
                {
                    Global.cCmdSel.CommandText = sCmd2;
                }
                else
                {
                    Global.cCmdSel.CommandText = sCmd1 + " UNION ALL " + sCmd2;
                }

                if (Global.Prn_SortKEY == 0)
                {
                    //Global.cCmdSel.CommandText += " ORDER BY VFLG, TRCD, HJCD ";
                    Global.cCmdSel.CommandText += " ORDER BY TRCD, HJCD ";
                }
                else
                {
                    //Global.cCmdSel.CommandText += " ORDER BY KNLD, VFLG, TRCD, HJCD ";
//-- <2016/03/10 カナ順ではなくフリガナ順>
//                    Global.cCmdSel.CommandText += " ORDER BY KNLD, TRCD, HJCD ";
                    //---> V02.01.01 HWPO ADD ▼【PostgreSQL対応】
                    if (IcsComUtil.ComUtil.IsPostgreSQL())
                    {
                        Global.cCmdSel.CommandText += " ORDER BY TRFURI NULLS FIRST,TRCD,HJCD";
                    }
                    else
                    {
                    //<--- V02.01.01 HWPO ADD ▲【PostgreSQL対応】
                        Global.cCmdSel.CommandText += " ORDER BY TRFURI, TRCD, HJCD ";
                    }
//-- <2016/03/10>
                }

                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_TRCD_List　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            #endregion
        }


        /// <summary>
        /// タイトル名称取得
        /// </summary>
        public static string Get_TTNM()
        {
            string sRet = "";
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                Global.cCmdSel.CommandText = "SELECT TTNM FROM VOLUM WHERE KESN = :p  ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KESN", Global.cKaisya.nKESN);
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                if (Global.gcDataReader.HasRows == true)
                {
                    Global.gcDataReader.Read();
                    sRet = Global.gcDataReader["TTNM"].ToString(); ;
                }

                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                return sRet;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_TTNM　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                return sRet = "";
            }
            finally
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
            }
        }



        //
        public static Boolean OutputHJCD()
        {
            return Global.bSaimuTori_Dsp & Global.bTRCD_HJ;
        }

        public static Boolean OutputUYMD()
        {
            return Global.bUseKikan_Dsp & Global.bChkSaimuUse;
        }

        //

        #endregion
        #endregion



        private void Rdo_sheet_CheckedChanged(object sender, EventArgs e)
        {
            if (Rdo_T.Checked)
            {
                Chk_PagingTRCD.Enabled = false;
                Chk_PagingTRCD.Checked = false;
            //    panel2.Enabled = true;
            //    //Grp_PrtItem.Enabled = false;
            }
            else
            {
                Chk_PagingTRCD.Enabled = true;
            //    Grp_PrtItem.Enabled = true;
            //    panel2.Enabled = false;
            //    Rdo_SS.Checked = true;
            }
//-- <>
            OutputItemControl();
//-- <>
        }

        private void Rdo_kind_CheckedChanged(object sender, EventArgs e)
        {
            if(Rdo_Z.Checked)
            {
                //Txt_HJCD_Sta.Clear();
                Txt_HJCD_Sta.ClearValue();
                //Txt_HJCD_End.Clear();
                Txt_HJCD_End.ClearValue();
                Dat_UseStaS.Clear();
                Dat_UseStaE.Clear();
                Chk_M3.Checked = false;
                Dat_UseEndS.Clear();
                Dat_UseEndE.Clear();
                Chk_M4.Checked = false;
                Txt_HJCD_Sta.ReadOnlyEx = true;
                Txt_HJCD_End.ReadOnlyEx = true;
                panel8.Enabled = false;

                Global.SearchMode = 0;
            }
            else
            {
                if ((Txt_TRCD_Sta.Text != "") &&
                    (Global.nTRCD_HJ == 1))
                {
                    Txt_HJCD_Sta.ReadOnlyEx = false;
                }
                else
                {
                    Txt_HJCD_Sta.ReadOnlyEx = true;
                    //Txt_HJCD_Sta.Clear();
                    Txt_HJCD_Sta.ClearValue();
                }
                if ((Txt_TRCD_End.Text != "") &&
                    (Global.nTRCD_HJ == 1))
                {
                    Txt_HJCD_End.ReadOnlyEx = false;
                }
                else
                {
                    Txt_HJCD_End.ReadOnlyEx = true;
                    //Txt_HJCD_End.Clear();
                    Txt_HJCD_End.ClearValue();
                }
                panel8.Enabled = true;

                if(Rdo_SS.Checked)
                {
                    Global.SearchMode = 1;
                }
                else
                {
                    Global.SearchMode = 2;
                }
            }
            GetTrName();
        }

        private void Chk_M1_CheckedChanged(object sender, EventArgs e)
        {
            if(Chk_M1.Checked)
            {
                Dat_InStaS.Enabled = false;
                Dat_InStaE.Enabled = false;
            }
            else
            {
                Dat_InStaS.Enabled = true;
                Dat_InStaE.Enabled = true;
            }
        }

        private void Chk_M2_CheckedChanged(object sender, EventArgs e)
        {
            if (Chk_M2.Checked)
            {
                Dat_InEndS.Enabled = false;
                Dat_InEndE.Enabled = false;
            }
            else
            {
                Dat_InEndS.Enabled = true;
                Dat_InEndE.Enabled = true;
            }
        }

        private void Chk_M3_CheckedChanged(object sender, EventArgs e)
        {
            if (Chk_M3.Checked)
            {
                Dat_UseStaS.Enabled = false;
                Dat_UseStaE.Enabled = false;
            }
            else
            {
                Dat_UseStaS.Enabled = true;
                Dat_UseStaE.Enabled = true;
            }
        }

        private void Chk_M4_CheckedChanged(object sender, EventArgs e)
        {
            if (Chk_M4.Checked)
            {
                Dat_UseEndS.Enabled = false;
                Dat_UseEndE.Enabled = false;
            }
            else
            {
                Dat_UseEndS.Enabled = true;
                Dat_UseEndE.Enabled = true;
            }
        }

        private void panel5_EnabledChanged(object sender, EventArgs e)
        {
            if (Txt_TRCD_Sta.ExCodeValue == "")
            {
                Txt_HJCD_Sta.ReadOnlyEx = true;
            }
            if (Txt_TRCD_End.ExCodeValue == "")
            {
                Txt_HJCD_End.ReadOnlyEx = true;
            }
        }

        internal static string ReadExPath()
        {
            try
            {
                string sExPath = "";

                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                Global.cCmdSel.CommandText = "SELECT CDATA FROM SS_OPTION1 WHERE PRGID = :p AND USNO = :p AND KEYNM1 = :p AND KEYNM2 = :p ";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@PRGID", "dlgExpImp");
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@USNO", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEYNM1", Global.sPrgId);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEYNM2", "EFILE");
                //if (IcsComDb.DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    // ※パラメータにコマンドテキストとパラメータを追加済みのDbCommandを渡す
                    IcsComDb.DbCls.ReplacePlaceHolder(Global.cCmdSel);
                }
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                if (Global.gcDataReader.HasRows)
                {
                    Global.gcDataReader.Read();

                    sExPath = Global.gcDataReader["CDATA"].ToString();
                }

                return sExPath;
            }
            catch
            {
                return "";
            }
            finally
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
            }
        }

        internal static void WhiteExPath(string sExPath)
        {
            try
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                Global.cCmdSel.CommandText = "SELECT KEYNO FROM SS_OPTION1 WHERE PRGID = :p AND USNO = :p AND KEYNM1 = :p AND KEYNM2 = :p ORDER BY KEYNO DESC";
                Global.cCmdSel.Parameters.Clear();
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@PRGID", "dlgExpImp");
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@USNO", Global.nUcod);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEYNM1", Global.sPrgId);
                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@KEYNM2", "EFILE");
                //if (IcsComDb.DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                {
                    // ※パラメータにコマンドテキストとパラメータを追加済みのDbCommandを渡す
                    IcsComDb.DbCls.ReplacePlaceHolder(Global.cCmdSel);
                }
                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                if (!Global.gcDataReader.HasRows)
                {
                    Global.cCmdIns.CommandText = "INSERT INTO SS_OPTION1 ( PRGID, USNO, KEYNM1, KEYNM2, KEYNO, DTYP, IDATA, CDATA )";
                    Global.cCmdIns.CommandText += " VALUES ( :p, :p, :p, :p, 0, 2, :p, :p )";
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@PRGID", "dlgExpImp");
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@USNO", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEYNM1", Global.sPrgId);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEYNM2", "EFILE");
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@IDATA", null);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDATA", sExPath);
                    //if (IcsComDb.DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        // ※パラメータにコマンドテキストとパラメータを追加済みのDbCommandを渡す
                        IcsComDb.DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                else
                {
                    Global.cCmdIns.CommandText = "UPDATE SS_OPTION1 SET CDATA = :p WHERE PRGID = :p AND USNO = :p AND KEYNM1 = :p AND KEYNM2 = :p AND KEYNO = 0 AND DTYP = 2 ";
                    Global.cCmdIns.Parameters.Clear();
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@CDATA", sExPath);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@PRGID", "dlgExpImp");
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@USNO", Global.nUcod);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEYNM1", Global.sPrgId);
                    DbCls.AddParamaterByValue(ref Global.cCmdIns, "@KEYNM2", "EFILE");
                    //if (IcsComDb.DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                    {
                        // ※パラメータにコマンドテキストとパラメータを追加済みのDbCommandを渡す
                        IcsComDb.DbCls.ReplacePlaceHolder(Global.cCmdIns);
                    }
                    DbCls.ConvStrParaEmptyToNull(Global.cCmdIns);
                    Global.cCmdIns.ExecuteNonQuery();
                }
                return;
            }
            finally
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
            }
        }

        private void GetTrName()
        {
            TxtToriStaNam.Text = GetTrName(Txt_TRCD_Sta.Text, Txt_HJCD_Sta.Text, 1);
            TxtToriEndNam.Text = GetTrName(Txt_TRCD_End.Text, Txt_HJCD_End.Text, 1);
        }
        private string GetTrName(string sTRCD, string sHJCD, int nMode)
        {
            try
            {
                int nHJCD = 0;

                int.TryParse(sHJCD, out nHJCD);

                string sTrName = "";

                if(Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
                switch(nMode)
                {
                    case 0:
                        // 財務の取引先名称の取得
                        Global.cCmdSel.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";
                        Global.cCmdSel.Parameters.Clear();
                        DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                        DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                        if (Global.gcDataReader.HasRows == true)
                        {
                            // <マルチDB対応>Readが必須なので追加
                            Global.gcDataReader.Read();
                            sTrName = Global.gcDataReader["TRMX"].ToString();
                        }
                        break;
                    case 1:
                        // 債務の取引先名称の取得
                        string sAnd = "";
                        if (rdo_Tori_Saiken.Checked == true)
                        {
                            sAnd = "AND SAIKEN = 1 ";
                        }
                        else if (rdo_Tori_Saimu.Checked == true)
                        {
                            sAnd = "AND SAIMU = 1 ";
                        }

                        if (Global.nTRCD_HJ == 1)
                        {
                            if (sHJCD != "" && sHJCD != null)
                            {
                                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                //if (DbCls.DbType == DbCls.eDbType.Oracle)
                                if(IcsComUtil.ComUtil.IsPostgreSQL())
                                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                {
                                    Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p AND LENGTH(TRCD) < 13 " + sAnd;
                                }
                                else
                                {
                                    Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p AND LEN(TRCD) < 13 " + sAnd;
                                }
                                Global.cCmdSel.Parameters.Clear();
                                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", nHJCD);
                                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                            //if (DbCls.DbType == DbCls.eDbType.Oracle)
                            if(IcsComUtil.ComUtil.IsPostgreSQL())
                            //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                            {
                                Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = 0 AND LENGTH(TRCD) < 13 " + sAnd;
                            }
                            else
                            {
                                Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = 0 AND LEN(TRCD) < 13 " + sAnd;
                            }
                            Global.cCmdSel.Parameters.Clear();
                            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                            DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
                        }

                        if (Global.gcDataReader.HasRows == true)
                        {
                            while (Global.gcDataReader.Read())
                            {
                                sTrName = Global.gcDataReader["RYAKU"].ToString();
                            }
                        }
                        break;
                    case 2:
                        if (Global.nTRCD_HJ == 1 && sHJCD != "000000")
                        {
                            if (sHJCD != "" && sHJCD != null)
                            {
                                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                                //if (DbCls.DbType == DbCls.eDbType.Oracle)
                                if(IcsComUtil.ComUtil.IsPostgreSQL())
                                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                                {
                                    Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p AND LENGTH(TRCD) < 13 ";
                                }
                                else
                                {
                                    Global.cCmdSel.CommandText = "SELECT RYAKU, TORI_NAM FROM SS_TORI WHERE RTRIM(TRCD) = :p AND HJCD = :p AND LEN(TRCD) < 13 ";
                                }
                                Global.cCmdSel.Parameters.Clear();
                                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@HJCD", nHJCD);
                                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);
                                if (Global.gcDataReader.HasRows == true)
                                {
                                    while (Global.gcDataReader.Read())
                                    {
                                        sTrName = Global.gcDataReader["RYAKU"].ToString();
                                    }
                                }
                            }
                            else
                            {
                                Global.cCmdSel.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";
                                Global.cCmdSel.Parameters.Clear();
                                DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                                DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                                if (Global.gcDataReader.HasRows == true)
                                {
                                    // <マルチDB対応>Readが必須なので追加
                                    Global.gcDataReader.Read();
                                    sTrName = Global.gcDataReader["TRMX"].ToString();
                                }
                            }
                        }
                        else
                        {
                            Global.cCmdSel.CommandText = "SELECT * FROM TRNAM WHERE RTRIM(TRCD) = :p ";
                            Global.cCmdSel.Parameters.Clear();
                            DbCls.AddParamaterByValue(ref Global.cCmdSel, "@TRCD", sTRCD);
                            DbCls.ExecuteQuery(ref Global.cCmdSel, ref Global.gcDataReader);

                            if (Global.gcDataReader.HasRows == true)
                            {
                                // <マルチDB対応>Readが必須なので追加
                                Global.gcDataReader.Read();
                                sTrName = Global.gcDataReader["TRMX"].ToString();
                            }
                        }
                        break;
                }

                return sTrName;
            }
            catch
            {
                return "";
            }
            finally
            {
                if (Global.gcDataReader != null)
                {
                    Global.gcDataReader.Close();
                    Global.gcDataReader.Dispose();
                }
            }
        }
        internal static string gSet(string[] s)
        {
            string ss = "";
            foreach(string s1 in s)
            {
                ss += s1;
            }
            return ss;
        }

        private void Select_OutputItem_CheckedChanged(object sender, EventArgs e)
        {
//-- <2016/03/15>
//            if (Chk_Kihon.Checked || Chk_Kaisyu.Checked || Chk_Shiharai.Checked || Chk_Frigin.Checked || Chk_Others.Checked || Chk_Gaika.Checked || Chk_Master.Checked)
            if (Chk_Kihon.Checked || Chk_Kaisyu.Checked || Chk_Shiharai.Checked || Chk_Frigin.Checked || Chk_Others.Checked || Chk_Gaika.Checked)
//-- <>
            {
                Chk_PagingTRCD.Enabled = true;
                Rdo_D.Checked = true;
            }
            else
            {
                Chk_PagingTRCD.Enabled = false;
//-- <9999>
//                Rdo_T.Checked = true;
//-- <9999>
            }
        }

        private void Output_Kind_CheckedChanged(object sender, EventArgs e)
        {
            if (DlgMng != null)
            {
                DlgMng = null;
            }
            if (rdo_Tori_All.Checked == true)
            {
                //Chk_Kijitsu_Only.Enabled = true;
                DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 3, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
                OutputItemControl();
            }
            else if (rdo_Tori_Saiken.Checked == true)
            {
                //Chk_Kijitsu_Only.Enabled = false;
                //Chk_Kijitsu_Only.Checked = false;
                DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 1, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
                OutputItemControl();
            }
            else if (rdo_Tori_Saimu.Checked == true)
            {
                //Chk_Kijitsu_Only.Enabled = false;
                //Chk_Kijitsu_Only.Checked = false;
                DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 2, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
                OutputItemControl();
            }

            Txt_TRCD_Sta.ClearValue();
            Txt_HJCD_Sta.ClearValue();
            Txt_HJCD_Sta.ReadOnlyEx = true;
            TxtToriStaNam.ClearValue();

            Txt_TRCD_End.ClearValue();
            Txt_HJCD_End.ClearValue();
            Txt_HJCD_End.ReadOnlyEx = true;
            TxtToriEndNam.ClearValue();
        }

        private void dlgPrnSetting_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.F9:
                    if (Txt_TRCD_Sta.ContainsFocus)
                    {
                        ValidateChildren();
                        Btn_TRCD_Sta.PerformClick();
                    }
                    if (Txt_TRCD_End.ContainsFocus)
                    {
                        ValidateChildren();
                        Btn_TRCD_End.PerformClick();
                    }
                    if (Txt_HJCD_Sta.ContainsFocus)
                    {
                        DialogManager.SToriData toriData = null;
                        toriData = DlgMng.DispTORI(Txt_TRCD_Sta.ExCodeDB, true, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
                        if (toriData != null)
                        {
                            Txt_HJCD_Sta.Text = toriData.HOJO.ToString();
                            SendKeys.Send("{TAB}");
                        }
                    }
                    if (Txt_HJCD_End.ContainsFocus)
                    {
                        DialogManager.SToriData toriData = null;
                        toriData = DlgMng.DispTORI(Txt_TRCD_End.ExCodeDB, true, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
                        if (toriData != null)
                        {
                            Txt_HJCD_End.Text = toriData.HOJO.ToString();
                            SendKeys.Send("{TAB}");
                        }
                    }
                    break;
            }
        }

        private void F9_ON(object sender, EventArgs e)
        {
            Lbl_F9_Search.Enabled = true;
        }

        private void F9_OFF(object sender, EventArgs e)
        {
            Lbl_F9_Search.Enabled = false;
        }

        private void Chk_Kijitsu_Only_CheckedChanged(object sender, EventArgs e)
        {
            if (Chk_Kijitsu_Only.Checked == true)
            {
                rdo_Tori_All.Enabled = false;
                rdo_Tori_All.Checked = false;

                rdo_Tori_Saiken.Enabled = false;
                rdo_Tori_Saiken.Checked = false;

                rdo_Tori_Saimu.Enabled = false;
                rdo_Tori_Saimu.Checked = false;

                if (DlgMng != null)
                {
                    DlgMng = null;
                }
                DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 3, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
                OutputItemControl();
            }
            else
            {
                rdo_Tori_All.Enabled = true;
                rdo_Tori_All.Checked = true;

                if (Global.nSAIKEN_F == 1)
                {
                    rdo_Tori_Saiken.Enabled = true;
                }
                if (Global.nSAIMU_F == 1)
                {
                    rdo_Tori_Saimu.Enabled = true;
                }
            }
        }

        private void OutputItemControl()
        {
            Chk_Kihon.Checked = false;
            Chk_Kaisyu.Checked = false;
            Chk_Shiharai.Checked = false;
            Chk_Frigin.Checked = false;
            Chk_Others.Checked = false;
            Chk_Gaika.Checked = false;

//-- < 取引先台帳のみの制御になっている>
            if (Rdo_D.Checked)
            {
//-- <>
                if ((Global.nSAIKEN_F == 1 && Global.nSAIMU_F == 1 && Global.nKIJITU_F == 0) ||
                    (Global.nSAIKEN_F == 1 && Global.nSAIMU_F == 0 && Global.nKIJITU_F == 1) ||
                    (Global.nSAIKEN_F == 1 && Global.nSAIMU_F == 1 && Global.nKIJITU_F == 1))
                {
                    if (rdo_Tori_All.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = true;
                        Chk_Shiharai.Enabled = true;
                        Chk_Frigin.Enabled = true;
                        Chk_Others.Enabled = true;
                        // --->V01.15.01 HWY UPDATE ▼(6442)
                        //Chk_Gaika.Enabled = true;
                        Chk_Gaika.Enabled = Global.nGAIKA_F == 1 ? true : false;
                        // <---V01.15.01 HWY UPDATE ▲(6442)
                    }
                    if (rdo_Tori_Saiken.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = true;
                        Chk_Shiharai.Enabled = false;
                        Chk_Frigin.Enabled = false;
                        // ▼#111516　竹内　2022/02/18
                        //Chk_Others.Enabled = false;
                        Chk_Others.Enabled = true;
                        // ▲#111516　竹内　2022/02/18
                        Chk_Gaika.Enabled = false;
                    }
                    if (rdo_Tori_Saimu.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = true;
                        Chk_Frigin.Enabled = true;
                        Chk_Others.Enabled = true;
                        // --->V01.15.01 HWY UPDATE ▼(6442)
                        //Chk_Gaika.Enabled = true;
                        Chk_Gaika.Enabled = Global.nGAIKA_F == 1 ? true : false;
                        // <---V01.15.01 HWY UPDATE ▲(6442)
                    }
                    if (Chk_Kijitsu_Only.Checked == true)
                    {
                        // ▼#111516　竹内　2022/02/18
                        //Chk_Kihon.Enabled = false;
                        Chk_Kihon.Enabled = true;
                        // ▲#111516　竹内　2022/02/18
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = false;
                        Chk_Frigin.Enabled = false;
                        // ▼#111516　竹内　2022/02/18
                        //Chk_Others.Enabled = false;
                        Chk_Others.Enabled = true;
                        // ▲#111516　竹内　2022/02/18
                        Chk_Gaika.Enabled = false;
                    }
                }
                else if (Global.nSAIKEN_F == 0 && Global.nSAIMU_F == 1 && Global.nKIJITU_F == 1)
                {
                    if (rdo_Tori_All.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = true;
                        Chk_Frigin.Enabled = true;
                        Chk_Others.Enabled = true;
                        // --->V01.15.01 HWY UPDATE ▼(6442)
                        //Chk_Gaika.Enabled = true;
                        Chk_Gaika.Enabled = Global.nGAIKA_F == 1 ? true : false;
                        // <---V01.15.01 HWY UPDATE ▲(6442)
                    }
                    if (rdo_Tori_Saimu.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = true;
                        Chk_Frigin.Enabled = true;
                        Chk_Others.Enabled = true;
                        // --->V01.15.01 HWY UPDATE ▼(6442)
                        //Chk_Gaika.Enabled = true;
                        Chk_Gaika.Enabled = Global.nGAIKA_F == 1 ? true : false;
                        // <---V01.15.01 HWY UPDATE ▲(6442)
                    }
                    if (Chk_Kijitsu_Only.Checked == true)
                    {
                        Chk_Kihon.Enabled = false;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = false;
                        Chk_Frigin.Enabled = false;
                        Chk_Others.Enabled = false;
                        Chk_Gaika.Enabled = false;
                    }
                }
                else if (Global.nSAIKEN_F == 1 && Global.nSAIMU_F == 0 && Global.nKIJITU_F == 0)
                {
                    if (rdo_Tori_All.Checked == true || rdo_Tori_Saiken.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = true;
                        Chk_Shiharai.Enabled = false;
                        Chk_Frigin.Enabled = false;
                        Chk_Others.Enabled = false;
                        Chk_Gaika.Enabled = false;
                    }
                }
                else if (Global.nSAIKEN_F == 0 && Global.nSAIMU_F == 1 && Global.nKIJITU_F == 0)
                {
                    if (rdo_Tori_All.Checked == true || rdo_Tori_Saimu.Checked == true)
                    {
                        Chk_Kihon.Enabled = true;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = true;
                        Chk_Frigin.Enabled = true;
                        Chk_Others.Enabled = true;
                        // --->V01.15.01 HWY UPDATE ▼(6442)
                        //Chk_Gaika.Enabled = true;
                        Chk_Gaika.Enabled = Global.nGAIKA_F == 1 ? true : false;
                        // <---V01.15.01 HWY UPDATE ▲(6442)
                    }
                }
                else if (Global.nSAIKEN_F == 0 && Global.nSAIMU_F == 0 && Global.nKIJITU_F == 1)
                {
                    if (rdo_Tori_All.Checked == true || Chk_Kijitsu_Only.Checked == true)
                    {
                        Chk_Kihon.Enabled = false;
                        Chk_Kaisyu.Enabled = false;
                        Chk_Shiharai.Enabled = false;
                        Chk_Frigin.Enabled = false;
                        Chk_Others.Enabled = false;
                        Chk_Gaika.Enabled = false;
                    }
                }
//-- <>
            }
            else
            {
                Chk_Kihon.Enabled = false;
                Chk_Kaisyu.Enabled = false;
                Chk_Shiharai.Enabled = false;
                Chk_Frigin.Enabled = false;
                Chk_Others.Enabled = false;
                Chk_Gaika.Enabled = false;
            }
//-- <>
        }
    }
}
