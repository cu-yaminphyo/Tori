using System;
//--
//using System.Collections;
//--
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using IcsComCtrl;
using IcsComInfo;
using IcsComDb;
using IcsComPrint;
using IcsSRacDlg.Dialog;
using IcsSSSInfo;
using IcsSSSTori;

using System.Data.Common;
//--
using Microsoft.VisualBasic;
using System.Collections;


namespace SMTORI
{
    public partial class frmSMTORI : FormEx
    {
        public frmSMTORI()
        {
            InitializeComponent();
            IcsComCtrl.SaveFormLib.SetSaveFormInfo(Global.cConCommon, this, Global.sPrgId, Global.nUcod);
            RemoveTRCDEventHandler();
            AddTRCDEventHandler();
        }

        private void AddTRCDEventHandler()
        {
            this.Txt_TRCD.TextChanged += this.Txt_TRCD_TextChanged;
        }

        private void RemoveTRCDEventHandler()
        {
            this.Txt_TRCD.TextChanged -= this.Txt_TRCD_TextChanged;
        }

        private void SetTRCDText(string value)
        {
            value = string.IsNullOrEmpty(value)
                ? value
                : Global.nTRCD_Type == 0
                    ? value.PadLeft(Global.nTRCD_Len, '0')
                    : value.Trim().ToUpper();
            RemoveTRCDEventHandler();
            Txt_TRCD.ExCodeDB = value;
            AddTRCDEventHandler();
        }


        /// <summary>
        /// 業務ロジック・クラスのインスタンス
        /// </summary>
        private blgSMTORI mcBsLogic;
        private SSTori mcSSTori;
        //internal DialogManager DlgMng = new DialogManager(Global.cConKaisya); 
//-- <2016/02/16 システム区分0で呼び出しを行う>
//        internal DialogManager DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 2 , Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
        internal DialogManager DlgMng = new IcsSRacDlg.Dialog.DialogManager(Global.sCcod, 0, Global.cConSaikenSaimu, Global.cConKaisya, Global.cConCommon);
//-- <2016/02/16>

        static string sNotUse = "0";
        static string sUse = "1";
        static string sDueOnly = "2";

        #region 変数
        DialogResult res;
        int nInsertFlg = 0;
        string sTNAM;                 //担当名格納用
        string sBNAM;                 //部門名格納用
        string sKNAM;                 //科目名格納用
        string sSHINM;                //支払条件名
        string sSKBNM;                //支払区分名称格納用
        string sBANKNM;               //銀行名格納用
        string sSITENNM;              //支店名格納用
        string[] sOWNIDArray;
        string[] sFACIDArray;
        string[,] sBKNAMArray;
        string[,] sBRNAMArray;
        string[,] sYKNKINDArray;
        string[] sKOZANOArray;
        string[] sIRAININArray;
        string[] sFACNAMArray;
        int nTRCDflg = 0;             //取引先CD変更フラグ
        int nTRCD_ChgFlg;             //支払方法変更が取引先CD変更に伴うものか単独変更か(単独時は休日補正書換有)の判別用
        int nShinoChgFlg = 0;
        string sTmpHrai_H;
        string sTmpKijitu_H;
        int nDispChgFlg_Main;         //画面項目変更フラグ
        int nDispChgFlg_TSHOH;        //画面項目変更フラグ
        int nDispChgFlg_FRIGIN;       //画面項目変更フラグ
        int nTabBindNavi = 0;         //TABバインドナビ操作中フラグ
        int nIchiUpdFlg = 0;          //一見更新処理済みフラグ
//-- < 国内⇔海外変更フラグ>
        bool bKOKUKAI = false;
        bool bGENNull = false;
        bool bHORYUNull = false;
        bool bKOUJYONull = false;
        bool bHenkou = false;
//-- <>

        bool bEventCancel = false;
        bool Flg_Tsh_Fri = false; //<--- V01.14.01 HWPO ADD ◀(8510)

//-- < ナビゲーション>
//------------        bool bNaviNew = false;
//-- <>

        // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
        int nBindNavi = 0;            //共通画面バインドナビ操作中フラグ

        string sZIP_Before;           //郵便番号変更チェック用
        decimal nHOVAL_Before;        //支払率変更チェック用
        int nErrFlg = 0;
        System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");

        private prnSMTORI mcPrnSMTORI;
        private bool fa;
        private bool fKeyClick = false;// <--- V02.28.01 KKL ADD◂(No.115107)
        // Ver.00.01.09 [SS_1312]対応 -->
        private bool bN = false;    // 編集チェックにかからないようにするための「その場しのぎ」のフラグ
        // Ver.00.01.09 <--

        #endregion


        #region フォームロード
        /// <summary>
        /// フォームロードイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSMTORI_Load(object sender, EventArgs e)
        {
            try
            {
                Global.bZUpdFlg = false;

//--
                // カナ名変換用
                Global.htChange = new Hashtable();
                Global.MakeHash();
//--



                // セキュリティ対応(ﾏｽﾀ権限：処理不可の場合、業務起動不可）※財務からの呼出時は考慮しない。そもそも財務取引先登録が起動不可の筈
                if (Global.cUsrSec.nMFLG < 1)
                {
//-- <2016/03/22>
//                    MessageBox.Show("ﾕｰｻﾞｰ" + Global.nUcod.ToString().PadLeft(4, '0') + "：" + Global.cUsrTbl.sUNAM + "は\nﾏｽﾀ権限が処理不可の為、このﾌﾟﾛｸﾞﾗﾑを使用できません。",
//                                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("ﾕｰｻﾞｰ" + Global.nUcod.ToString().PadLeft(4, '0') + "：" + Global.cUsrTbl.sUNAM + "は\nﾏｽﾀ権限が処理不可の為、このﾌﾟﾛｸﾞﾗﾑを使用できません。\n\nVer" + Global.sPrgVer,
                                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                    WaitMsg.ICS_MessageWndClose();
                    Environment.Exit(0);
                }

                Cbo_SAIKEN.SelectedIndexChanged -= Cbo_SAIKEN_SelectedIndexChanged;
                Cbo_SAIKEN.DataSource = Create_Cbo_SAIKEN_List(true);
                Cbo_SAIKEN.DisplayMember = "Name";
                Cbo_SAIKEN.ValueMember = "Code";
                Cbo_SAIKEN.SelectedIndexChanged += Cbo_SAIKEN_SelectedIndexChanged;

                Cbo_SAIMU.SelectedIndexChanged -= Cbo_SAIMU_SelectedIndexChanged;
                Cbo_SAIMU.DataSource = Create_Cbo_SAIMU_List(true);
                Cbo_SAIMU.DisplayMember = "Name";
                Cbo_SAIMU.ValueMember = "Code";
                Cbo_SAIMU.SelectedIndexChanged += Cbo_SAIMU_SelectedIndexChanged;

                Cbo_SAIKEN.Enabled = false;
                Cbo_SAIMU.Enabled = false;

                // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
                Global.bEnabledState = (Global.cUsrSec.nMFLG < 2 ? false : true);
                Global.bReadOnlyState = (Global.cUsrSec.nMFLG < 2 ? true : false);

                // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
                if (Global.cUsrSec.nMFLG < 2 || (Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                {
                    Txt_RYAKU.ReadOnlyEx = true;
                    Txt_TORI_NAM.ReadOnlyEx = true;
                    Txt_TRFURI.ReadOnlyEx = true;
                    Txt_KNLD.ReadOnlyEx = true;
                    Txt_TRFURI.ReadOnlyEx = true;
                    Lbl_ZIP.Enabled = true;
                    Tb1_Txt_ZIP.ReadOnlyEx = true;
                    Lbl_ADDR1.Enabled = true;
                    Tb1_Txt_ADDR1.ReadOnlyEx = true;
                    Lbl_ADDR2.Enabled = true;
                    Tb1_Txt_ADDR2.ReadOnlyEx = true;

                    Tb1_Txt_TRMAIL.ReadOnlyEx = true;
                    Tb1_Txt_TRURL.ReadOnlyEx = true;
                    Tb1_Txt_BIKO.ReadOnlyEx = true;
                    Tb1_Txt_E_TANTOCD.ReadOnlyEx = true;
                    Tb1_Txt_MYNO_AITE.ReadOnlyEx = true;

                    Lbl_SBUSYO.Enabled = true;
                    Tb1_Txt_SBUSYO.ReadOnlyEx = true;
                    Lbl_STANTO.Enabled = true;
                    Tb1_Txt_STANTO.ReadOnlyEx = true;
                    Lbl_KEICD.Enabled = true;
                    Tb1_Cmb_KEICD.Enabled = false;
                    Lbl_TEL.Enabled = true;
                    Tb1_Txt_TEL.ReadOnlyEx = true;
                    Lbl_FAX.Enabled = true;
                    Tb1_Txt_FAX.ReadOnlyEx = true;
                    Lbl_STAN.Enabled = true;
                    Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                    Lbl_SBCOD.Enabled = true;
                    Tb5_Txt_SBCOD.ReadOnlyEx = true;
                    Lbl_SKCOD.Enabled = true;
                    Tb5_Txt_SKCOD.ReadOnlyEx = true;
                    Tb5_Chk_NAYOSE.Enabled = false;
                    Tb5_Chk_F_SETUIN.Enabled = false;
                    Chk_STFLG.Enabled = false;
                    cDfTitl1.Enabled = true;
                    Lbl_STYMD.Enabled = true;
                    Txt_STYMD.Enabled = false;
                    Lbl_EDYMD.Enabled = true;
                    Txt_EDYMD.Enabled = false;
                    cDfTitl2.Enabled = true;
                    Lbl_ZSTYMD.Enabled = true;
                    Txt_ZSTYMD.Enabled = false;
                    Lbl_ZEDYMD.Enabled = true;
                    Txt_ZEDYMD.Enabled = false;
                    Lbl_LUSR.Enabled = true;
                    Txt_LUSR.Enabled = false;
                    Lbl_LMOD.Enabled = true;
                    Txt_LMOD.Enabled = false;
                }
                else
                {
                    Txt_RYAKU.ReadOnlyEx = false;
                    Txt_TORI_NAM.ReadOnlyEx = false;
                    Txt_KNLD.ReadOnlyEx = false;
                    Txt_TRFURI.ReadOnlyEx = false;
                    Lbl_ZIP.Enabled = true;
                    Tb1_Txt_ZIP.ReadOnlyEx = false;
                    Lbl_ADDR1.Enabled = true;
                    Tb1_Txt_ADDR1.ReadOnlyEx = false;
                    Lbl_ADDR2.Enabled = true;
                    Tb1_Txt_ADDR2.ReadOnlyEx = false;

                    Tb1_Txt_TRMAIL.ReadOnlyEx = false;
                    Tb1_Txt_TRURL.ReadOnlyEx = false;
                    Tb1_Txt_BIKO.ReadOnlyEx = false;
                    Tb1_Txt_E_TANTOCD.ReadOnlyEx = Global.nSAIKEN_F != 1;
                    Tb1_Txt_MYNO_AITE.ReadOnlyEx = false;

                    Lbl_SBUSYO.Enabled = true;
                    Tb1_Txt_SBUSYO.ReadOnlyEx = false;
                    Lbl_STANTO.Enabled = true;
                    Tb1_Txt_STANTO.ReadOnlyEx = false;
                    Lbl_KEICD.Enabled = true;
                    Tb1_Cmb_KEICD.Enabled = true;
                    Lbl_TEL.Enabled = true;
                    Tb1_Txt_TEL.ReadOnlyEx = false;
                    Lbl_FAX.Enabled = true;
                    Tb1_Txt_FAX.ReadOnlyEx = false;
                    Lbl_STAN.Enabled = (Global.nKMAN == 0 ? false : true);
                    Tb5_Txt_STAN_CD.ReadOnlyEx = (Global.nKMAN == 0 ? true : false);
                    Lbl_SBCOD.Enabled = (Global.nBCOD_F == 0 ? false : true);
                    Tb5_Txt_SBCOD.ReadOnlyEx = (Global.nBCOD_F == 0 ? true : false);
                    Lbl_SKCOD.Enabled = true;
                    Tb5_Txt_SKCOD.ReadOnlyEx = false;
                    if (Tb5_Chk_OUTPUT.Enabled == true)
                    {
                        Tb5_Chk_NAYOSE.Checked = false;
                        Tb5_Chk_NAYOSE.Enabled = false;
                        Tb5_Chk_F_SETUIN.Checked = false;
                        Tb5_Chk_F_SETUIN.Enabled = false;
                        Tb5_Chk_OUTPUT.Checked = true;
                        Tb5_Chk_GENSEN.Enabled = true;
                    }
                    else
                    {
                        Tb5_Chk_NAYOSE.Enabled = true;
                        Tb5_Chk_F_SETUIN.Enabled = true;
                        Tb5_Chk_GENSEN.Enabled = false;
                    }
                    Chk_STFLG.Enabled = true;
                    cDfTitl1.Enabled = true;
                    Lbl_STYMD.Enabled = true;
                    Txt_STYMD.Enabled = true;
                    Lbl_EDYMD.Enabled = true;
                    Txt_EDYMD.Enabled = true;
                    cDfTitl2.Enabled = true;
                    Lbl_ZSTYMD.Enabled = true;
                    Txt_ZSTYMD.Enabled = true;
                    Lbl_ZEDYMD.Enabled = true;
                    Txt_ZEDYMD.Enabled = true;
                    Lbl_LUSR.Enabled = true;
                    Txt_LUSR.Enabled = true;
                    Lbl_LMOD.Enabled = true;
                    Txt_LMOD.Enabled = true;
                }



                // メインフォームのタイトルバーの文字列（業務名称、バージョン、会社コード、会社名称）を取得し
                // タイトルバーに表示する。
                this.Text = Global.GetMainFormCaption(Global.cKaisya);

                //TLBにユーザー名と機能IDを設定
                TLB.SetToolStripLabel(Global.cUsrTbl.sUNAM, null, Global.sPrgId + "-1");

                // 業務ロジック・クラスのインスタンスの生成。
                mcBsLogic = new blgSMTORI(this);
                mcSSTori = new SSTori();

                //各種制御用情報の取得
                mcBsLogic.Get_KI();
                mcBsLogic.Get_VOLUM();
                mcBsLogic.Get_SVOLUM();
                //mcBsLogic.Get_SS_VOLUM();
                mcBsLogic.Get_Env();
                Set_DispControl();
                mcBsLogic.Get_RIREKI_SW();
                mcBsLogic.New_dtRIREKI();

                //初期画面設定
                Set_InitVal();

                //起動時点では登録ボタンは押下不可
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;

                // ---> V02.26.01 KSM ADD ▼(No.113951)
                this.ActiveControl = Txt_TRCD;
                Txt_TRCD.Focus();
                Txt_TRCD.SelectAll();
                Txt_TRCD.ExTextBoxType = eTextBoxType.Code;
                Txt_TRCD.ExCodeType = Global.nTRCD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
                Txt_TRCD.ExCodeLength = Global.nTRCD_Len;
                Tb3_Txt_BCOD.ExTextBoxType = eTextBoxType.Code;
                Tb3_Txt_BCOD.ExCodeType = Global.nBCOD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
                Tb3_Txt_BCOD.ExCodeLength = Global.nBCOD_Len;
                Tb3_Txt_KCOD.ExTextBoxType = eTextBoxType.Code;
                Tb3_Txt_KCOD.ExCodeType = Global.nKCOD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
                Tb3_Txt_KCOD.ExCodeLength = Global.nKCOD_Len;
                Tb5_Txt_SBCOD.ExTextBoxType = eTextBoxType.Code;
                Tb5_Txt_SBCOD.ExCodeType = Global.nBCOD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
                Tb5_Txt_SBCOD.ExCodeLength = Global.nBCOD_Len;
                Tb5_Txt_SKCOD.ExTextBoxType = eTextBoxType.Code;
                Tb5_Txt_SKCOD.ExCodeType = Global.nKCOD_Type == 0 ? eCodeType.Suuji : eCodeType.Eisuu;
                Tb5_Txt_SKCOD.ExCodeLength = Global.nKCOD_Len;
                // <--- V02.26.01 KSM ADD ▲(No.113951)
                // 財務取引先登録から呼出
                if (Global.bZMode == true)
                {
                    Txt_TRCD.ExCodeDB = Global.sTRCD_R;
                    Txt_HJCD.Text = Global.sHJCD_R;

                    Sel_SSTORI();

                    MNU_QUIT.Text = "戻る(&X)";
                    FKB.F11_Text = "戻る";
                }
                else
                {
                    //他APからの呼出対応
                    if (Global.sTRCD_R == "" && Global.sTRNAM_R != "")
                    {
                        Txt_TRCD.Visible = false;
                        Txt_HJCD.Visible = false;

                        Global.nIchigenCode = mcBsLogic.Get_TRCD_ICHI();
                        Txt_HJCD.Text = "0";
                        if (System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(Global.sTRNAM_R) <= 20)
                        {
                            Txt_RYAKU.Text = Global.sTRNAM_R;
                        }
                        else
                        {
                            // 一見名称が２０バイトを超える場合の処理（略称のレングスオーバー）
                            System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");
                            byte[] btBytes = hEncoding.GetBytes(Global.sTRNAM_R);

                            Txt_RYAKU.Text = hEncoding.GetString(btBytes, 0, 20);

                            // ２０バイト目が２バイトコードの場合、そこを文字列の終端(0x00)とする
                            if (btBytes[19] == 0x82)
                            {
                                btBytes[19] = 0x00;
                                Txt_RYAKU.Text = hEncoding.GetString(btBytes, 0, 20);
                            }
                        }
                        Txt_TORI_NAM.Text = Global.sTRNAM_R;
                        Global.nDispMode = 0;

                        Chg_DispControl();

                        //一見登録時固有処理
                        BindNavi1.Enabled = false;
                        Txt_TRCD.ReadOnlyEx = true;
                        Txt_HJCD.ReadOnlyEx = true;
                        Txt_RYAKU.ReadOnlyEx = false;
                        Txt_TORI_NAM.ReadOnlyEx = false;
                        Txt_TRFURI.ReadOnlyEx = false;
                        Txt_KNLD.ReadOnlyEx = true;

                        if (Global.nShTgSW == 0)
                        {
                            // 債務業務から呼び出し
                            Cbo_SAIKEN.SelectedValue = sNotUse;
                            Cbo_SAIMU.SelectedValue = sUse;
                        }
                        else
                        {
                            // 期日業務から呼び出し
                            Cbo_SAIKEN.SelectedValue = sDueOnly;
                            Cbo_SAIMU.SelectedValue = sDueOnly;
                        }
                        Cbo_SAIKEN.Enabled = false;
                        Cbo_SAIMU.Enabled = false;

                        //支払方法の変更時に取引先CD変更モードではなく支払方法ID変更モードとするためnTRCDをリセット
                        //登録ボタン押下時に登録処理をさせる為に【新規】ラベルを設定
                        nTRCDflg = 0;
                        Lbl_Old_New1.Text = "【　新規　】";
                        Tb3_Lbl_Old_New2.Text = "【　新規　】";
                        Tb4_Lbl_Old_New3.Text = "【　新規　】";

                        BindNavi2_Selected.Text = "1";
                        BindNavi2_Cnt.Text = "/ 1";
                        Tb1_Lbl_SHO_ID_V.Text = "1";

                        Tb4_BindNavi_Selected.Text = "1";
                        Tb4_BindNavi_Cnt.Text = "/ 1";
                        Tb4_Lbl_GIN_ID_V.Text = "1";
//-- <2016/03/24>
                        Tb4_Chk_FDEF.Checked = true;
//-- <2016/03/24>

                        // 2013.3.6 修正 S
                        FKB.F02_Enabled = false;
                        FKB.F06_Enabled = false;
                        MNU_PRNT.Enabled = false;
                        MNU_DELETE.Enabled = false;
                        MNU_SEARCH.Enabled = false;
                        MNU_Z_SACH.Enabled = false;
                        Tb1_BindNavi2.Enabled = false;
                        Tb3_BindNavi_Add.Enabled = false;
                        Tb3_BindNavi_DEL.Enabled = false;

                        Tb4_BindNavi_First.Enabled = false;
                        Tb4_BindNavi_Prev.Enabled = false;
                        Tb4_BindNavi_Next.Enabled = false;
                        Tb4_BindNavi_End.Enabled = false;
                        Tb4_BindNavi_Add.Enabled = false;
                        Tb4_BindNavi_DEL.Enabled = false;

                        Tb3_Lbl_HARAI_KBN1.Text = "";
                        Tb3_Lbl_HARAI_KBN2.Text = "";
                        Tb3_Lbl_HARAI_KBN3.Text = "";
                        Tb3_Lbl_HARAI_KBN4.Text = "";
                        // 2013.3.6 修正 E

                        FKB.F08_Enabled = false;
                        FKB.F09_Enabled = false;

                        Tb_Main.SelectedIndex = 0;
                    }
                    else if (Global.sTRCD_R != "" && Global.sTRNAM_R != "")
                    {
                        Global.nIchigenCode = Global.sTRCD_R;

                        Txt_TRCD.ExCodeDB = Global.sTRCD_R;
                        Txt_HJCD.Text = "0";
                        Global.nDispMode = 0;

                        Chg_DispControl();
                        nTRCDflg = 1;
                        Sel_SSTORI();

                        Cbo_SAIKEN.Enabled = false;
                        Cbo_SAIMU.Enabled = false;

                        //一見登録時固有処理
                        Txt_TRCD.Visible = false;
                        Txt_HJCD.Visible = false;
                        BindNavi1.Enabled = false;
                        Txt_TRCD.ReadOnlyEx = true;
                        Txt_HJCD.ReadOnlyEx = true;
                        Txt_RYAKU.ReadOnlyEx = false;
                        Txt_TORI_NAM.ReadOnlyEx = false;
                        Txt_TRFURI.ReadOnlyEx = false;
                        Txt_KNLD.ReadOnlyEx = true;

                        FKB.F02_Enabled = false;
                        FKB.F06_Enabled = false;
                        MNU_PRNT.Enabled = false;
                        MNU_DELETE.Enabled = false;
                        MNU_SEARCH.Enabled = false;
                        MNU_Z_SACH.Enabled = false;

                        FKB.F08_Enabled = false;
                        FKB.F09_Enabled = false;
                    }
                }
                //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
                IcsSSSPrint.PrintOption.Initialize(Global.cConSaikenSaimu, Global.sPrgId);
                // 管理者のみ[頁数設定]ダイアログを有効とする
                MNU_OPT.Visible = (Global.cUsrTbl.nKANF == 2);
                MNU_OPT_PAGE.Visible = (Global.cUsrTbl.nKANF == 2);
                WaitMsg.ICS_MessageWndClose();
                //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応
                Global.nVolKJUN = 0;// ICSLIBV.Var.cCompany.Volum[ICSLIBV.Var.cCompany.Mc_GetKesn(13)].nKOUT;

                mcBsLogic.RirekiChk();
                mcBsLogic.Get_VOLUM_SSTori();
//-- <2016/02/09 初期タブ制御>
                TAB_Enable_Control();
//-- <2016/02/09>

                // Ver.01.02.04 [SS_4778] Toda -->
                // ---> V02.26.01 KSM DELETE ▼(No.113951)
                //this.ActiveControl = Txt_TRCD;
                //Txt_TRCD.Focus();
                //Txt_TRCD.SelectAll();
                // <--- V02.26.01 KSM DELETE ▲(No.113951)
                // Ver.01.02.04 <--
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nfrmSMTORI_Load　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                //メッセージを閉じる
                WaitMsg.ICS_MessageWndClose();
                //俺も知らない謎のスリープ100ミリ秒
                System.Threading.Thread.Sleep(100);
            }
        }
        #endregion


        #region メイン処理
        /// <summary>
        /// 登録ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_REG_Click(object sender, EventArgs e)
        {
            Ins_SSTORI();
            if (nErrFlg == 0)
            {
                Txt_TRCD.Focus();
            }
        }

        private void Ins_SSTORI()
        {
            // トランザクション処理
            DbTransaction trn = (Global.cConSaikenSaimu).BeginTransaction(IsolationLevel.ReadCommitted);
            Global.cCmdSel.Transaction = trn;
            Global.cCmdIns.Transaction = trn;
            Global.cCmdDel.Transaction = trn;
            
            Global.iZCheck = 0;

            try
            {
                //コードエラーがある場合はエラー項目へフォーカスを返す
                // Ver.01.09.03 [SIAS_7220] Toda -->
                // 現状では、編集チェックから登録処理を行った場合に、Validateレベルのチェックが行われていない
                // その為、エラーとなる値が入力されていても登録可となっている。
                // とりあえず「グループID」のみ、ここでチェックをかける。
                if (Txt_GRPID.Text != "" && mcBsLogic.Get_GrpNm(Txt_GRPID.Text) == "")
                {
                    Txt_GRPNM.ClearValue();
                    Txt_GRPID.IsError = true;
                    Txt_GRPID.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                // Ver.01.09.03 <--
                if (Tb1_Txt_ZIP.IsError == true)                                                            // 郵便番号
                {
                    Tb1_Txt_ZIP.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb5_Txt_STAN_CD.IsError == true)                                                   // 主担当者
                {
                    Tb_Main.SelectedIndex = 4;
                    Tb5_Txt_STAN_CD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb5_Txt_SBCOD.IsError == true)                                                     // 初期部門コード
                {
                    Tb_Main.SelectedIndex = 4;
                    Tb5_Txt_SBCOD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb5_Txt_SKCOD.IsError == true)                                                     // 初期科目コード
                {
                    Tb_Main.SelectedIndex = 4;
                    Tb5_Txt_SKCOD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb3_Txt_BCOD.IsError == true)                                                      // 支払条件　部門コード
                {
                    Tb_Main.SelectedIndex = 2;
                    Tb3_Txt_BCOD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb3_Txt_KCOD.IsError == true)                                                      // 支払条件　科目コード
                {
                    Tb_Main.SelectedIndex = 2;
                    Tb3_Txt_KCOD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb3_Txt_SHINO.IsError == true)                                                     // 支払条件　支払方法ID
                {
                    Tb_Main.SelectedIndex = 2;
                    Tb3_Txt_SHINO.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb4_Txt_BANK_CD.IsError == true)                                                   // 振込先情報　銀行コード
                {
                    Tb_Main.SelectedIndex = 3;
                    Tb4_Txt_BANK_CD.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                else if (Tb4_Txt_SITEN_ID.IsError == true)                                                  // 振込先情報　銀行支店コード
                {
                    Tb_Main.SelectedIndex = 3;
                    Tb4_Txt_SITEN_ID.Focus();
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                //else if (Txt_ZSTYMD.Text != "" & (Txt_ZSTYMD.Text == "" ? 0 : int.Parse(Txt_ZSTYMD.Text)) > Txt_STYMD.Value)
                //{
                //    Tb_Main.SelectedIndex = 1;
                //    Txt_STYMD.Focus();
                //    nErrFlg = 1;
                //    return;
                //}
                //else if (Txt_ZEDYMD.Text != "" & (Txt_ZEDYMD.Text == "" ? 0 : int.Parse(Txt_ZEDYMD.Text)) > Txt_EDYMD.Value)
                //{
                //    Tb_Main.SelectedIndex = 1;
                //    Txt_EDYMD.Focus();
                //    nErrFlg = 1;
                //    return;
                //}
                else
                {
                    //Btn_REG.Focus();
                }

                // 使用可能期間チェック
                int nZSTYMD = int.TryParse(Txt_ZSTYMD.Text.Replace("/", ""), out nZSTYMD) ? nZSTYMD : 0;
                int nZEDYMD = int.TryParse(Txt_ZEDYMD.Text.Replace("/", ""), out nZEDYMD) ? nZEDYMD : 99999999;
                int nSTYMD = Txt_STYMD.Value;
                int nEDYMD = Txt_EDYMD.Value == 0 ? 99999999 : Txt_EDYMD.Value;

                if (!(nZSTYMD <= nSTYMD && nEDYMD <= nZEDYMD))
                {
                    res = MessageBox.Show(
                        "使用可能期間が財務の入力可能期間の範囲外になっています。\n使用可能期間に財務の入力可能期間を複写して登録しますか。",
                        "入力可能期間・使用可能期間チェック", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (res == DialogResult.Cancel)
                    {
                        Txt_STYMD.Focus();
                        nErrFlg = 1;
                        trn.Rollback();
                        return;
                    }
                    Txt_STYMD.Value = nZSTYMD;
                    Txt_EDYMD.Value = nZEDYMD == 99999999 ? 0 : nZEDYMD;
                }

                string sDaiCd = "";
                string sDHjCd = "";
                bool bSaikenDaihyo = mcBsLogic.Get_MySaikenDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                bool bSaikenChild = mcBsLogic.Get_SaikenDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd);
                bool bSaimuDaihyo = mcBsLogic.Get_MySaimuDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                bool bSaimuChild = mcBsLogic.Get_SaimuDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd);

                if ((bSaikenDaihyo || bSaikenChild) && Cbo_SAIKEN.SelectedValue.ToString() != sUse)
                {
                    res = MessageBox.Show("代表者マスター登録に登録されている為、変更できません。", "登録確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cbo_SAIKEN.Focus();
                    Cbo_SAIKEN.IsError = true;
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                if ((bSaikenDaihyo && !Chk_SAIKEN_FLG.Checked) || (bSaikenChild && Chk_SAIKEN_FLG.Checked))
                {
                    res = MessageBox.Show("代表者マスター登録に登録されている為、変更できません。", "登録確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (Chk_SAIKEN_FLG.Enabled)
                    {
                        Chk_SAIKEN_FLG.Focus();
                        Chk_SAIKEN_FLG.IsError = true;
                    }
                    else
                    {
                        Cbo_SAIKEN.Focus();
                        Cbo_SAIKEN.IsError = true;
                    }
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }

                if ((bSaimuDaihyo || bSaimuChild) && Cbo_SAIMU.SelectedValue.ToString() != sUse)
                {
                    res = MessageBox.Show("代表者マスター登録に登録されている為、変更できません。", "登録確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cbo_SAIMU.Focus();
                    Cbo_SAIMU.IsError = true;
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }
                if ((bSaimuDaihyo && !Chk_SAIMU_FLG.Checked) || (bSaimuChild && Chk_SAIMU_FLG.Checked))
                {
                    res = MessageBox.Show("代表者マスター登録に登録されている為、変更できません。", "登録確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (Chk_SAIMU_FLG.Enabled)
                    {
                        Chk_SAIMU_FLG.Focus();
                        Chk_SAIMU_FLG.IsError = true;
                    }
                    else
                    {
                        Cbo_SAIMU.Focus();
                        Cbo_SAIMU.IsError = true;
                    }
                    nErrFlg = 1;
                    trn.Rollback();
                    return;
                }

                // 登録・更新日時を同期させるために取得
                //Global.dNow = IcsSSUtil.IDate.GetDBNow(Global.cConCommon);
                Global.dNow = mcBsLogic.Get_DBTime(trn);

                int iInsUpdFlg = 0;
                Global.iDispChangeFlg = 0;

                //手形管理のみ使用フラグがonの場合、取引先CD/略称/50音/正式名称のみで登録可能
                // ▼#111516　竹内　2022/02/18
                if (Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly && Cbo_SAIMU.SelectedValue.ToString() == sDueOnly
                    || Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly && Cbo_SAIMU.SelectedValue.ToString() == sNotUse
                    || Cbo_SAIKEN.SelectedValue.ToString() == sNotUse && Cbo_SAIMU.SelectedValue.ToString() == sDueOnly)
                //if (Cbo_SAIKEN.SelectedValue.ToString() == sNotUse && Cbo_SAIMU.SelectedValue.ToString() == sNotUse)
                // ▲#111516　竹内　2022/02/18
                {
                    //手形管理のみ使用フラグON用の入力チェック
                    Chk_DispVal_TGASW_ON();
                    if (nErrFlg == 1)
                    {
                        trn.Rollback();
                        return;
                    }

                    if (nErrFlg == 0)
                    {
                        //画面の情報を格納(SS_TORIの登録に必要な項目のみ)
                        Get_Main_Data();
                        Get_Tb1_Data();  // #111516　竹内　2022/03/08


                        //登録or更新
                        if (Lbl_Old_New1.Text == "【　新規　】")
                        {
                            iInsUpdFlg = 0;
                            if (nInsertFlg == 0)
                            {
                                int nIsZaimuExists = mcBsLogic.Get_TRNAM(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB); //0:財務登録済　1:財務未登録

                                if (nIsZaimuExists == 0 || nIsZaimuExists == 1)
                                {
                                    nInsertFlg = nIsZaimuExists;
                                }
                                else
                                {
                                    MessageBox.Show(
//-- <2016/03/22>
//                                        "エラーが発生しました。",
//                                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                    trn.Rollback();
                                    return;
                                }
                            }
                            nDispChgFlg_Main = 1;

                            //履歴関連 ＠2011/07 履歴対応
                            //新規の場合、dtRIREKIに積んだ変更情報をクリアし、新規登録用の情報へ差し替える
                            Global.dtRIREKI.Clear();
                            Set_dtRIREKI(0, 0, "", 1, null, null, null);
                        }
                        else
                        {
                            iInsUpdFlg = 1;
                        }

                        //メイン部の項目に何かしら変更が発生していた場合に登録を呼び出す
                        if (nDispChgFlg_Main == 1)
                        {
                            int nIsZaimuExists = mcBsLogic.Get_TRNAM(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB); //0:財務登録済　1:財務未登録

                            //// 財務の同期処理
                            //if (nIsZaimuExists == 1)
                            //{
                            //    // 財務の取引先を新規登録
                            //    mcBsLogic.Ins_TRNAM(Txt_TRCD.ExCodeDB, Txt_KNLD.Text, Txt_RYAKU.Text, Txt_TORI_NAM.Text);
                            //}
                            //else
                            //{
                            //    if (Txt_HJCD.Text == "000000" || Txt_HJCD.Text == "")
                            //    {
                            //        // 財務の取引先を更新
                            //        mcBsLogic.Upd_TRNAM(Txt_TRCD.ExCodeDB, Txt_KNLD.Text, Txt_RYAKU.Text, Txt_TORI_NAM.Text);
                            //        bZUpdFlg = true;
                            //    }
                            //}

                            if(mcSSTori.RegTrnam(Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), Global.RYAKU, Global.TORI_NAM, Global.KNLD, Global.bRKFLG, Global.nTRFLG,
                                false, 0, 0, Global.dNow, Global.cCmdSelZ, Global.cCmdInsZ) == -1)
                            {
                                MessageBox.Show(
//-- <2016/03/22>
//                                    "エラーが発生しました。",
//                                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                trn.Rollback();
                                return;
                            }

                            // Ver.00.01.09 [SS_1312]対応 -->
                            //財務側の取引先登録が処理可能の場合のみ、連動処理を行う
                            if (Global.bZaimBoot)
                            {
                                if (mcSSTori.RegTrzan(DbCls.GetNumNullZero<int>(Global.sKESN), Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), DbCls.GetNumNullZero<int>(Global.STFLG), Global.nTRFLG,
                                    Global.nGCFLG, DbCls.GetNumNullZero<int>(Global.STYMD), DbCls.GetNumNullZero<int>(Global.EDYMD), Global.nSYMD, Global.nEYMD, Global.SKICD, Global.SBCOD, Global.cCmdSelZ, Global.cCmdInsZ) == -1)
                                {
                                    MessageBox.Show(
                                        "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    trn.Rollback();
                                    return;
                                }
                            }
                            // Ver.00.01.09 <--

                            // ▼#111516　竹内　2022/03/08
                            mcBsLogic.Insert_SS_TORI_MIN(iInsUpdFlg);
                            //mcBsLogic.Insert_SS_TORI_Full(iInsUpdFlg);
                            // ▲#111516　竹内　2022/03/08

                            //履歴関連 ＠2011/07 履歴対応
                            mcBsLogic.Insert_SS_RKITORI();
                        }
                        Global.dtRIREKI.Clear();

                        Lbl_Old_New1.Text = "【　変更　】";

                        trn.Commit();

                        //一見時は取引先終了、通常時は現在のキーで再検索
                        if (!Global.bIchigen)
                        {
                            nTRCDflg = 1;
                            //Chk_TGASW.Checked = false;
                            Sel_SSTORI();
                        }
                        else
                        {
                            nDispChgFlg_Main = 0;
                            nDispChgFlg_TSHOH = 0;
                            nDispChgFlg_FRIGIN = 0;
                            nIchiUpdFlg = 1;
                            this.Close();
                        }
                    }
                }
                else
                {
                    //手形管理のみ使用フラグOFF用の入力チェック
                    Chk_DispVal_TGASW_OFF();
                    if (nErrFlg == 1)
                    {
                        trn.Rollback();
                        return;
                    }

                    string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                    string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                    string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                    if (!(Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") && mcBsLogic.Chk_UniqKey(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == false)
                    {
                        string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                        string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                        trn.Rollback();
                        MessageBox.Show(
                            "既に"
                            + "\nID：" + sSHOID
                            + "\n部門：" + sBMNNM
                            + "\n科目：" + sKMKNM
//-- <2016/03/22>
//                            + "\nは登録済です。",
//                            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            + "\nは登録済です。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                            Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                        Tb3_Txt_BCOD.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    if (nErrFlg == 0)
                    {
                        //画面の情報を格納
//-- <2016/03/11 再度復活>                        
//-- <2016/03/09 条件で通るものが違う>
                        Get_Main_Data();
                        Get_Tb1_Data();
                        Get_Tb2_Data();
                        Get_Tb3_Data();
                        Get_Tb4_Data();
                        Get_Tb5_Data_Koujyo();
                        Get_Tb5_Data();
                        Get_Tb6_Data();
//-- <2016/03/11>


//-- <2016/03/11 再度閉鎖>
//                        // 共通の内容　全てに共通
//                        Get_Main_Data();
//
//                        // 基本情報　期日管理のみ以外
//                        if (Chk_TGASW.Checked)
//                        { Get_Tb1_Data(); }
//
//                        // 回収情報　債権のみ
//                        if (Chk_SAIKEN.Checked)
//                        { Get_Tb2_Data(); }
//
//                        // 支払条件　債務の国内のみ　だけど通す
//                        if (Chk_SAIMU.Checked)
//                        { Get_Tb3_Data(); }
//
//                        // 振込条件　債務の国内のみ
//                        if (Chk_SAIMU.Checked && Tb3_Rdo_GAI_F0.Checked)
//                        { Get_Tb4_Data(); }
//
//                        // その他情報(源泉・控除)　債務の国内のみ　だけど通す
//                        if (Chk_SAIMU.Checked && Tb3_Rdo_GAI_F0.Checked)
//                        { Get_Tb5_Data_Koujyo(); }
//
//                        // その他情報(上記以外)　債務のみ
//                        if (Chk_SAIMU.Checked)
//                        { Get_Tb5_Data(); }
//
//                        // 外貨設定　債務の海外のみ だけど通す
//                        if (Chk_SAIMU.Checked)
//                        { Get_Tb6_Data(); }
//-- <2016/03/11>



                        //登録or更新
                        if (Lbl_Old_New1.Text == "【　新規　】")
                        {
                            iInsUpdFlg = 0;
                            if (nInsertFlg == 0)
                            {
                                int nIsZaimuExists = mcBsLogic.Get_TRNAM(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB); //0:財務登録済　1:財務未登録

                                if (nIsZaimuExists == 0 || nIsZaimuExists == 1)
                                {
                                    nInsertFlg = nIsZaimuExists;
                                }
                                else
                                {
                                    trn.Rollback();
                                    MessageBox.Show(
//-- <2016/03/22>
//                                        "エラーが発生しました。",
//                                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                    return;
                                }
                            }
                            nDispChgFlg_Main = 1;

                            //履歴関連 ＠2011/07 履歴対応
                            //新規の場合、dtRIREKIに積んだ変更情報をクリアし、新規登録用の情報へ差し替える
                            Global.dtRIREKI.Clear();
                            Set_dtRIREKI(0, 0, "", 1, null, null, null);

                        }
                        else
                        {
                            iInsUpdFlg = 1;
                        }

                        //メイン部の項目に何かしらの変更が入っていた場合のみ更新
                        if (nDispChgFlg_Main == 1)
                        {
                            int nIsZaimuExists = mcBsLogic.Get_TRNAM(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB); //0:財務登録済　1:財務未登録

                            //// 財務の同期処理
                            //if (nIsZaimuExists == 1)
                            //{
                            //    // 財務の取引先を新規登録
                            //    mcBsLogic.Ins_TRNAM(Txt_TRCD.ExCodeDB, Txt_KNLD.Text, Txt_RYAKU.Text, Txt_TORI_NAM.Text);
                            //}
                            //else
                            //{
                            //    if (Txt_HJCD.Text == "000000" || Txt_HJCD.Text == "")
                            //    {
                            //        // 財務の取引先を更新
                            //        mcBsLogic.Upd_TRNAM(Txt_TRCD.ExCodeDB, Txt_KNLD.Text, Txt_RYAKU.Text, Txt_TORI_NAM.Text);
                            //        bZUpdFlg = true;
                            //    }
                            //}

                            if(mcSSTori.RegTrnam(Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), Global.RYAKU, Global.TORI_NAM, Global.KNLD, Global.bRKFLG, Global.nTRFLG,
                                false, 0, 0, Global.dNow, Global.cCmdSelZ, Global.cCmdInsZ) == -1)
                            {
                                MessageBox.Show(
//-- <2016/03/22>
//                                    "エラーが発生しました。",
//                                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                trn.Rollback();
                                return;
                            }

                            if (Cbo_SAIMU.SelectedValue.ToString() != sUse && Global.bZaimBoot)
                            {
                                if (mcSSTori.RegTrzan(DbCls.GetNumNullZero<int>(Global.sKESN), Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), DbCls.GetNumNullZero<int>(Global.STFLG), Global.nTRFLG,
                                    Global.nGCFLG, DbCls.GetNumNullZero<int>(Global.STYMD), DbCls.GetNumNullZero<int>(Global.EDYMD), Global.nSYMD, Global.nEYMD, Global.SKICD, Global.SBCOD, Global.cCmdSelZ, Global.cCmdInsZ) == -1)
                                {
                                    MessageBox.Show(
                                        "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    trn.Rollback();
                                    return;
                                }
                            }

                            mcBsLogic.Insert_SS_TORI_Full(iInsUpdFlg);
                        }

                        //**>>
                        //**Lbl_Old_New1.Text = "【　変更　】";
                        //**<<

                        if (Tb3_Lbl_Old_New2.Text == "【　新規　】")
                        {
                            iInsUpdFlg = 0;
                        }
                        else if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                        {
                            iInsUpdFlg = 1;
                        }
                        
                        if (Global.GAI_F == "1")
                        {
                            iInsUpdFlg = 3;     // 全削除
                        }

                        //---> V01.14.01 HWPO ADD ▼(8510)
                        if (Flg_Tsh_Fri && Tb3_Rdo_GAI_F1.Checked)
                        {
                            string TRCD = Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB;
                            string HJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                            mcBsLogic.Del_SS_TSHOH_ALL(TRCD, HJCD);
                            mcBsLogic.Del_SS_FRIGIN_ALL(TRCD, HJCD);
                            nDispChgFlg_TSHOH = 0;
                            nDispChgFlg_FRIGIN = 0;
                            Flg_Tsh_Fri = false;
                        }
                        //<--- V01.14.01 HWPO ADD ▲(8510)
                        //支払条件の項目に何かしらの変更が入っていた場合のみ更新
                        if (nDispChgFlg_TSHOH == 1)
                        {
                            //**                
                            if (iInsUpdFlg == 0)
                            {
                                if (Lbl_Old_New1.Text == "【　変更　】")
                                {
                                    //**Global.dtRIREKI.Clear();
                                    Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "", 3, null, null, null);
                                }
                            }
                            //**                    

                            //財務側の取引先登録が処理可能の場合のみ、連動処理を行う
                            if (Global.bZaimBoot)
                            {
                                if (mcSSTori.RegTrzan(DbCls.GetNumNullZero<int>(Global.sKESN), Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), DbCls.GetNumNullZero<int>(Global.STFLG), Global.nTRFLG,
                                    Global.nGCFLG, DbCls.GetNumNullZero<int>(Global.STYMD), DbCls.GetNumNullZero<int>(Global.EDYMD), Global.nSYMD, Global.nEYMD, Global.KICD_tb1, Global.BCOD_tb1, Global.cCmdSelZ, Global.cCmdInsZ) == -1)
                                {
                                    MessageBox.Show(
//-- <2016/03/22>
//                                        "エラーが発生しました。",
//                                        Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                    trn.Rollback();
                                    return;
                                }
                            }

                            //---> V01.14.01 HWPO UPDATE ▼(8510)
                            //mcBsLogic.Insert_SS_TSHOH(iInsUpdFlg);
                            if(Flg_Tsh_Fri)
                            {
                                if (mcBsLogic.Get_SS_TSHOH_Count(Global.TRCD, Global.HJCD) > 0)
                                {
                                    mcBsLogic.Insert_SS_TSHOH(1);
                                }
                                else
                                {
                                    mcBsLogic.Insert_SS_TSHOH(0);
                                }  
                            }
                            else
                            {
                                mcBsLogic.Insert_SS_TSHOH(iInsUpdFlg);
                            }                             
                            //<--- V01.14.01 HWPO UPDATE ▲(8510)
                        }
                        //**>>
                        //**Tb1_Lbl_Old_New2.Text = "【　変更　】";
                        //**<<

                        if (Tb4_Lbl_Old_New3.Text == "【　新規　】")
                        {
                            iInsUpdFlg = 0;
                        }
                        else if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                        {
                            iInsUpdFlg = 1;
                        }

                        if (Global.GAI_F == "1")
                        {
                            iInsUpdFlg = 3;     // 全削除
                        }
                        //振込先情報の項目に何かしらの変更が入っていた場合のみ更新
                        if (nDispChgFlg_FRIGIN == 1)
                        {
                            //**                
                            if (iInsUpdFlg == 0)
                            {
                                if (Lbl_Old_New1.Text == "【　変更　】")
                                {
                                    //**Global.dtRIREKI.Clear();
                                    Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "", 3, null, null, null);
                                }
                            }
                            //**                    
                            if (Tb4_BindNavi_Cnt.Text.Replace("/", "").Replace(" ", "") != "1")
                            {
                                if (Global.FDEF == "1")
                                {
                                    mcBsLogic.Update_SS_FRIGIN_FDEF();
                                }
                                if (Global.DDEF == "1")
                                {
                                    mcBsLogic.Update_SS_FRIGIN_DDEF();
                                }
                            }
                            //---> V01.14.01 HWPO UPDATE ▼(8510)
                            //mcBsLogic.Insert_SS_FRIGIN(iInsUpdFlg);
                            if(Flg_Tsh_Fri)
                            {
                                if (mcBsLogic.GetSS_FRIGIN_Count(Global.TRCD, Global.HJCD) > 0)
                                {
                                    mcBsLogic.Insert_SS_FRIGIN(1);
                                }
                                else
                                {
                                    mcBsLogic.Insert_SS_FRIGIN(0);
                                }
                            }
                            else
                            {
                                mcBsLogic.Insert_SS_FRIGIN(iInsUpdFlg);
                            }                            
                            Flg_Tsh_Fri = false;
                            //<--- V01.14.01 HWPO UPDATE ▲(8510)
                        }

                        //履歴関連 ＠2011/07 履歴対応
                        mcBsLogic.Insert_SS_RKITORI();

                        Global.dtRIREKI.Clear();

                        //**>>
                        Lbl_Old_New1.Text = "【　変更　】";
                        Tb3_Lbl_Old_New2.Text = "【　変更　】";                    
                        //**<<

                        Tb4_Lbl_Old_New3.Text = "【　変更　】";


                        if (Global.iZCheck == 1 && Global.HJCD == "000000" && Global.STFLG == "0")
                        {
                            if (Global.gcDataReader != null)
                            {
                                Global.gcDataReader.Close();
                                Global.gcDataReader.Dispose();
                            }
                            Global.cCmdSel.CommandText = "select ST.TRCD, ST.HJCD, TH.KICD, TH.BCOD from SS_TORI ST ";
                            Global.cCmdSel.CommandText += " left outer join SS_TSHOH TH on ST.TRCD = TH.TRCD and ST.HJCD = TH.HJCD ";
//                            Global.cCmdSel.CommandText += " where ST.TRCD = :p and ST.HJCD = :p";
                            Global.cCmdSel.CommandText += " where ST.TRCD = ':p' and ST.HJCD = :p";

                            Global.cCmdSel.Parameters.Clear();
                            DbCls.AddParamaterByValue(Global.cCmdSel, "@TRCD", Global.TRCD);
                            DbCls.AddParamaterByValue(Global.cCmdSel, "@HJCD", int.Parse(Global.HJCD));//<--- V02.01.01 HWPO ADD ◀【PostgreSQL対応】
                            //if (DbCls.DbType == DbCls.eDbType.SQLServer)//<--- V02.01.01 HWPO DELETE ◀【PostgreSQL対応】
                            {
                                DbCls.ReplacePlaceHolder(Global.cCmdSel);
                            }
                            DbCls.ExecuteQuery(ref Global.cCmdSel, out Global.dtTSHOH);
                            for(int i = 0; i < Global.dtTSHOH.Rows.Count; i++)
                            {
                                //財務側の取引先登録が処理可能の場合のみ、連動処理を行う
                                if (Global.bZaimBoot)
                                {
                                    string ZKICD = DbCls.GetStrNullKara(Global.dtTSHOH.Rows[i]["KICD"]);
                                    string ZBCOD = DbCls.GetStrNullKara(Global.dtTSHOH.Rows[i]["BCOD"]);
                                    if (mcSSTori.RegTrzan(DbCls.GetNumNullZero<int>(Global.sKESN), Global.nUcod, Global.TRCD, DbCls.GetNumNullZero<int>(Global.HJCD), DbCls.GetNumNullZero<int>(Global.STFLG), Global.nTRFLG,
                                        Global.nGCFLG, DbCls.GetNumNullZero<int>(Global.STYMD), DbCls.GetNumNullZero<int>(Global.EDYMD), Global.nSYMD, Global.nEYMD, ZKICD, ZBCOD, Global.cCmdSel, Global.cCmdIns) == -1)
                                    {
                                        MessageBox.Show(
//-- <2016/03/22>
//                                            "エラーが発生しました。",
//                                            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            "エラーが発生しました。\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                                            Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                                        trn.Rollback();
                                        return;
                                    }
                                }
                            }
                        }

                        trn.Commit();

                        //一見時は取引先終了、通常時は現在のキーで再検索
                        if (!Global.bIchigen)
                        {
                            nTRCDflg = 1;
                            //Chk_TGASW.Checked = false;
                            Sel_SSTORI();
                        }
                        else
                        {
                            nDispChgFlg_Main = 0;
                            nDispChgFlg_TSHOH = 0;
                            nDispChgFlg_FRIGIN = 0;
                            nIchiUpdFlg = 1;
                            this.Close();
                        }
                    }
                }



                if (iInsUpdFlg == 0 || (iInsUpdFlg == 1 && Global.iDispChangeFlg == 1))
                {
                    Global.bZUpdFlg = true;
                }

                //選択タブ・登録件数のリセット
                // 不具合表-0041によりコメント
                //Tb_Main.SelectedIndex = 0;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;
                Global.bUpdated = true;
                Refresh_DataCnt();
            }
            catch (Exception ex)
            {
                trn.Rollback();
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nIns_SSTORI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// エクスポート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_EXPT_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F1}");
        }

        /// <summary>
        /// 印刷
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_PRNT_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F2}");
        }


        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_DELETE_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F6}");
        }


        /// <summary>
        /// メニューで[バージョン]-[バージョン情報]が選択された場合の処理 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_VERINFO_Click(object sender, EventArgs e)
        {
            DlgMng.DispVersion();
        }


        /// <summary>
        /// メニューで[メニュー]-[終了]が選択された場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_QUIT_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F11}");
        }


        /// <summary>
        /// メニューで[操作]-[検索]が選択された場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_SEARCH_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F9}");
        }

        /// <summary>
        /// メニューで[操作]-[財務検索]が選択された場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_Z_SACH_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{F8}");
        }

        /* SIAS_3154,2226 差分 --> */
        /// <summary>
        /// 取引先マスター不整合チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MNU_TR_CHK_Click(object sender, EventArgs e)
        {
            if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
            {
                res = MessageBox.Show(
                    "変更されています、確定しますか。", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                // res = MessageBoxTourokuKakunin();    登録確認メッセージを統一する場合に使用（現状未使用）
                if (res == DialogResult.Cancel)
                {
                    return;
                }
                else if (res == DialogResult.No)
                {
                    nTRCDflg = 1;
                    Sel_SSTORI();
                }
                else if (res == DialogResult.Yes)
                {
                    nErrFlg = 0;
                    Ins_SSTORI();
                    if (nErrFlg == 1)
                    {
                        return;
                    }
                    else
                    {
                        nDispChgFlg_Main = 0;
                        nDispChgFlg_TSHOH = 0;
                        nDispChgFlg_FRIGIN = 0;
                        Btn_REG.Enabled = false;
                    }
                }
            }
            //
            bool bRet = false;
            DbCommand cmdChk = DbCls.CreateCommandObject(ref Global.cConSaikenSaimu);
            IcsSSSTori.SSTori oSSTori = new IcsSSSTori.SSTori();
            DialogResult eDlgResult = DialogResult.None;
            bRet = oSSTori.ChkMasterConsistency(Global.sCcod, 2, cmdChk, out eDlgResult);
            if (bRet == false)
            {
                return;
            }
            else
            {
                res = MessageBox.Show(
                    "財務の取引先マスターと債務支払の取引先マスターとの間で不整合は検知されませんでした。", Global.sPrgName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        /* SIAS_3154,2226 差分 <-- */
        #endregion


        #region サブ処理
        /// <summary>
        /// 取引先ｺｰﾄﾞ テキストChg時には再検索される為、登録ボタンのenabled=trueは不要
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TRCD_TextChanged(object sender, EventArgs e)
        {
            nTRCDflg = 1;
            Lbl_Haifun.Enabled = false;
            Txt_HJCD.Text = "";
            Txt_HJCD.ReadOnlyEx = true;

            nDispChgFlg_Main = 0;
            nDispChgFlg_TSHOH = 0;
            nDispChgFlg_FRIGIN = 0;
        }


        /// <summary>
        /// 取引先補助ｺｰﾄﾞ テキストChg時には再検索される為、登録ボタンのenabled=trueは不要
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_HJCD_TextChanged(object sender, EventArgs e)
        {
            nTRCDflg = 1;

            //---> V01.14.01 HWPO UPDATE ▼(8510)
            //nDispChgFlg_Main = 0;
            //nDispChgFlg_TSHOH = 0;
            //nDispChgFlg_FRIGIN = 0;
            if (nDispChgFlg_Main != 1 && nDispChgFlg_TSHOH != 1 && nDispChgFlg_FRIGIN != 1)
            {
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
            }            
            //<--- V01.14.01 HWPO UPDATE ▲(8510)
                        
        }


        /// <summary>
        /// 画面初期値の設定
        /// </summary>
        private void Set_InitVal()
        {
            try
            {
                //Globalの初期化
                mcBsLogic.Init_DispVal();

                //敬称コンボボックス
                string[] sArray = null;
                mcBsLogic.Get_KeiNM(out sArray);

                if (sArray != null)
                {
                    //敬称の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                    );
                    for (int i = 0; i < sArray.Length; )
                    {
                        int iCombo = Convert.ToInt32(sArray[i].ToString().Substring(0, sArray[i].ToString().IndexOf(':')));
                        string sCombo = sArray[i].ToString().Substring(sArray[i].ToString().IndexOf(':') + 1);

                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                        i++;
                    }
                    Tb1_Cmb_KEICD.DisplayMember = "Value";
                    Tb1_Cmb_KEICD.ValueMember = "Key";
                    Tb1_Cmb_KEICD.DataSource = list;
                }

                //源泉区分コンボボックス
                sArray = null;
                mcBsLogic.Get_SKBNM(29, out sArray);

                if (sArray != null)
                {
                    //源泉区分の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                    );
                    for (int i = 0; i < sArray.Length; )
                    {
                        int iCombo = Convert.ToInt32(sArray[i].ToString().Substring(0, sArray[i].ToString().IndexOf(':')));
                        string sCombo = sArray[i].ToString();

                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                        i++;
                    }
                    Tb5_Cmb_GSKUBN.DisplayMember = "Value";
                    Tb5_Cmb_GSKUBN.ValueMember = "Key";
                    Tb5_Cmb_GSKUBN.DataSource = list;
//-- <2016/03/23>
                    bGENNull = false;
//-- <2016/03/23>
                }
//-- <2016/03/23>
                else { bGENNull = true; }
//-- <2016/03/23>

                //源泉区分コンボボックス
                sArray = null;
                mcBsLogic.Get_SKBNM(23, out sArray);

                if (sArray != null)
                {
                    //源泉区分の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                    );

                    for (int i = 0; i < sArray.Length; )
                    {
                        int iCombo = Convert.ToInt32(sArray[i].ToString().Substring(0, sArray[i].ToString().IndexOf(':')));
                        string sCombo = sArray[i].ToString();

                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                        i++;
                    }
                }

                // 回収設定　約定コンボボックス
                SetKaisyuComboList(Tb2_Chk_GAIKA.Checked);

                // 回収設定　取引通貨コンボボックス
                sArray = null;
                mcBsLogic.Get_HEI_CD(out sArray);

                if (sArray != null)
                {
                    //幣種の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>()
                    );

                    for (int i = 0; i < sArray.Length; )
                    {
                        string sCombo = sArray[i].ToString();

                        list.Add(new System.Collections.Generic.KeyValuePair<string, string>(sCombo, sCombo));
                        i++;
                    }
                    Tb2_Cmb_TSUKA.DisplayMember = "Value";
                    Tb2_Cmb_TSUKA.ValueMember = "Key";
                    Tb2_Cmb_TSUKA.DataSource = list;
                    Tb2_Cmb_TSUKA.SelectedIndex = -1;
                }

//-- <2016/03/22>
                // 控除関連　自動控除リスト
                sArray = null;
                mcBsLogic.Get_SKBNM(25, out sArray);

                if (sArray == null)
                {
                    bHORYUNull = true;
                }
                else { bHORYUNull = false; }
//-- <2016/03/22>

                //その他情報　控除関連　作成区分コンボボックス
                sArray = null;
                mcBsLogic.Get_SKBNM(23, out sArray);

                if (sArray != null)
                {
                    //控除＋の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                    );
                    for (int i = 0; i < sArray.Length; )
                    {
                        int iCombo = Convert.ToInt32(sArray[i].ToString().Substring(0, sArray[i].ToString().IndexOf(':')));
                        string sCombo = sArray[i].ToString();

                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                        i++;
                    }
                    Tb5_Cmb_HRKBN.DisplayMember = "Value";
                    Tb5_Cmb_HRKBN.ValueMember = "Key";
                    Tb5_Cmb_HRKBN.DataSource = list;
//-- <2016/03/23>
                    bKOUJYONull = false;
//-- <2016/03/23>
                }
                else
                {
//-- <2016/03/23>
//                    Tb4_Grp_KJ.Enabled = false;
                    bKOUJYONull = true;
//-- <2016/03/23>
                }

                // 外貨設定　取引通貨コンボボックス
                sArray = null;
                mcBsLogic.Get_HEI_CD(out sArray);

                if (sArray != null)
                {
                    //幣種の数だけLOOP
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>()
                    );

                    for (int i = 0; i < sArray.Length; )
                    {
                        string sCombo = sArray[i].ToString();

                        list.Add(new System.Collections.Generic.KeyValuePair<string, string>(sCombo, sCombo));
                        i++;
                    }
                    Tb6_Cmb_HEI_CD.DisplayMember = "Value";
                    Tb6_Cmb_HEI_CD.ValueMember = "Key";
                    Tb6_Cmb_HEI_CD.DataSource = list;
                    Tb6_Cmb_HEI_CD.SelectedIndex = -1;
                }

//-- <2016/03/09 >
//                // 外貨設定　出金口座コンボボックス
//                string[] sOWNID_G_Array;
//                string[,] sBKNAM_G_Array;
//                string[,] sBRNAM_G_Array;
//                string[,] sYKNKIND_G_Array;
//                string[] sKOZANO_G_Array;
//                mcBsLogic.Get_OWNBK_Gaika(out sOWNID_G_Array, out sBKNAM_G_Array, out sBRNAM_G_Array, out sYKNKIND_G_Array, out sKOZANO_G_Array);
//                if (sOWNID_G_Array != null)
//                {
//                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
//                    list = (
//                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
//                    );

//                    for (int i = 0; i < sOWNID_G_Array.GetLength(0); i++)
//                    {
//                        int nOwnId = Convert.ToInt32(sOWNID_G_Array[i].ToString());
//                        string sGaikaBk = string.Format("{0}：{1}：{2}：{3}",
//                                mcBsLogic.StringCut(string.Format("{0,-30}", sBKNAM_G_Array[i, 1].ToString()), 30),
////-- <2016/02/08 支店名称が銀行名になっている>
////                                mcBsLogic.StringCut(string.Format("{0,-30}", sBKNAM_G_Array[i, 1].ToString()), 30),
//                                mcBsLogic.StringCut(string.Format("{0,-30}", sBRNAM_G_Array[i, 1].ToString()), 30),
////-- <2016/02/08>
//                                mcBsLogic.StringCut(string.Format("{0,-8}", sYKNKIND_G_Array[i, 1]), 8),
//                                mcBsLogic.StringCut(string.Format("{0,-14}", sKOZANO_G_Array[i].ToString()), 14));

//                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(nOwnId, sGaikaBk));
//                    }
//                    Tb6_Cmb_GAI_KZID.DisplayMember = "Value";
//                    Tb6_Cmb_GAI_KZID.ValueMember = "Key";
//                    Tb6_Cmb_GAI_KZID.DataSource = list;
//                    Tb6_Cmb_GAI_KZID.SelectedIndex = -1;
//                }
//-- <2016/03/09>
                // 外貨設定　出金口座コンボボックス
                string[] sOWNID_G_Array;
                string[,] sBKNAM_G_Array;
                string[,] sBRNAM_G_Array;
                string[,] sYKNKIND_G_Array;
                string[] sKOZANO_G_Array;
                string[] sHEICD_G_Array;

                mcBsLogic.Get_OWNBK_Gaika(out sOWNID_G_Array, out sBKNAM_G_Array, out sBRNAM_G_Array, out sYKNKIND_G_Array, out sKOZANO_G_Array, out sHEICD_G_Array);
                if (sOWNID_G_Array != null)
                {
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                    list = (
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                    );

                    for (int i = 0; i < sOWNID_G_Array.GetLength(0); i++)
                    {
                        int nOwnId = Convert.ToInt32(sOWNID_G_Array[i].ToString());
                        string sGaikaBk = string.Format("{0}：{1}：{2}：{3}：{4}",
                                mcBsLogic.StringCut(string.Format("{0,-30}", sBKNAM_G_Array[i, 1].ToString()), 30),
                                mcBsLogic.StringCut(string.Format("{0,-30}", sBRNAM_G_Array[i, 1].ToString()), 30),
                                mcBsLogic.StringCut(string.Format("{0,-8}", sYKNKIND_G_Array[i, 1]), 8),
                                mcBsLogic.StringCut(string.Format("{0,-7}", sKOZANO_G_Array[i].ToString()), 7),   
                                mcBsLogic.StringCut(string.Format("{0,-4}", sHEICD_G_Array[i].ToString()), 4));

                        list.Add(new System.Collections.Generic.KeyValuePair<int, string>(nOwnId, sGaikaBk));
                    }
                    Tb6_Cmb_GAI_KZID.DisplayMember = "Value";
                    Tb6_Cmb_GAI_KZID.ValueMember = "Key";
                    Tb6_Cmb_GAI_KZID.DataSource = list;
                    Tb6_Cmb_GAI_KZID.SelectedIndex = -1;
                }

                //画面描画
                SetDispVal_Init();

                //取引先登録件数をカウント
                Refresh_DataCnt();

                //各種画面コントロールの制御
                Chg_DispControl();

                Generate_Tb4_Cmb_GOU();



                Generate_Tb5_Cmb_HORYU();


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSet_InitVal　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 回収設定の入金区分コンボボックスのリストをセットする
        /// </summary>
        /// <param name="isGaika"></param>
        private void SetKaisyuComboList(bool isGaika)
        {
            var notUseKind = mcBsLogic.GetNotUseNyukinKbn();

            var list1 = SSSInfo.SelectTblKubunList(Global.cConSaikenSaimu, Global.sCcod, eKubunSikibetu.Nyukin, isGaika, eKubunListFirstLine.Nothing, notUseKind);
            var list2 = new List<TBLKUBUN>(list1);
            var list3 = new List<TBLKUBUN>(list1);
            var list4 = SSSInfo.SelectTblKubunList(Global.cConSaikenSaimu, Global.sCcod, eKubunSikibetu.Nyukin, isGaika, eKubunListFirstLine.Zero_Nashi, notUseKind);
            var list5 = new List<TBLKUBUN>(list4);

            Tb2_Cmb_KAISYU.DisplayMember = "Value";
            Tb2_Cmb_KAISYU.ValueMember = "Key";
            Tb2_Cmb_KAISYU.DataSource = list1;
            Tb2_Cmb_KAISYU.SelectedIndex = -1;

            Tb2_Cmb_MIMAN.DisplayMember = "Value";
            Tb2_Cmb_MIMAN.ValueMember = "Key";
            Tb2_Cmb_MIMAN.DataSource = list2;
            Tb2_Cmb_MIMAN.SelectedIndex = -1;

            Tb2_Cmb_IJOU_1.DisplayMember = "Value";
            Tb2_Cmb_IJOU_1.ValueMember = "Key";
            Tb2_Cmb_IJOU_1.DataSource = list3;
            Tb2_Cmb_IJOU_1.SelectedIndex = -1;

            Tb2_Cmb_IJOU_2.DisplayMember = "Value";
            Tb2_Cmb_IJOU_2.ValueMember = "Key";
            Tb2_Cmb_IJOU_2.DataSource = list4;
            Tb2_Cmb_IJOU_2.SelectedIndex = -1;

            Tb2_Cmb_IJOU_3.DisplayMember = "Value";
            Tb2_Cmb_IJOU_3.ValueMember = "Key";
            Tb2_Cmb_IJOU_3.DataSource = list5;
            Tb2_Cmb_IJOU_3.SelectedIndex = -1;
        }

        /// <summary>
        /// 取引先データカウントのリフレッシュ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_DataCnt()
        {
            int iTRCDCnt;
            mcBsLogic.Cnt_TRCD(out iTRCDCnt);

            MaxCnt.Text = "/" + iTRCDCnt.ToString("#,##0");
            //Txt_DataCnt.Text = iTRCDCnt.ToString();
            //this.BindNavi1_Prev.Enabled = (iTRCDCnt > 0);
            //this.BindNavi1_First.Enabled = (iTRCDCnt > 0);
        }


        /// <summary>
        /// 各タブのデータを検索
        /// </summary>
        private void Sel_TabData()
        {
            // ---> V02.37.01 YMP UPDATE ▼(122172)
            //Sel_SS_TSHOH();     //支払条件タブの検索・データ設定
            //Sel_SS_FRIGIN();    //振込先情報タブの検索・データ設定
            Sel_SS_TSHOH(nDispChgFlg_TSHOH == 0 ? false : true);     //支払条件タブの検索・データ設定
            Sel_SS_FRIGIN(nDispChgFlg_FRIGIN == 0 ? false : true);    //振込先情報タブの検索・データ設定
            // <--- V02.37.01 YMP UPDATE ▲(122172)
        }


        /// <summary>
        /// 号を変更⇒変更された号に合致する源泉区分リストを生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Cmb_GOU_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Tb5_Cmb_GOU.Text != "")
                {
                    //源泉区分コンボボックス
                    string[,] sArray = null;
                    int iCALKBN;
                    if (!Tb5_Chk_GENSEN.Checked && Tb5_Chk_OUTPUT.Checked)
                    {
                        iCALKBN = 3;
                    }
                    else
                    {
                        iCALKBN = (Tb5_Radio_GENSEN1.Checked == true ? 1 : 2);
                    }
                    int iGOU = Convert.ToInt32(mcBsLogic.Get_Gou_CD(Tb5_Cmb_GOU.Text));
                    mcBsLogic.Get_GensenNM(iCALKBN, iGOU, out sArray);

                    //データがあった場合は源泉区分のリストを生成
                    if (sArray != null)
                    {
                        //源泉区分の数だけLOOP
                        System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                        list = (
                            new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                        );
                        for (int i = 0; i < sArray.Length / 2; )
                        {
                            int iCombo = Convert.ToInt32(sArray[i, 0]);
                            string sCombo = "";
                            if (sArray[i, 1].ToString() == "")
                            {
                                sCombo = sArray[i, 1].ToString();
                            }
                            else
                            {
                                sCombo = Convert.ToInt32(sArray[i, 0]) + ":" + sArray[i, 1].ToString();
                            }

                            list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                            i++;
                        }
                        Tb5_Cmb_GGKBN.DisplayMember = "Value";
                        Tb5_Cmb_GGKBN.ValueMember = "Key";
                        Tb5_Cmb_GGKBN.DataSource = list;
                    }
                    else
                    {
                        Tb5_Cmb_GGKBN.DataSource = null;
                    }
                }
                else
                {
                    Tb5_Cmb_GGKBN.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb4_Cmb_GOU_TextChanged　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        #endregion
//--
        #region 
        /// <summary>
        /// 控除関連の指定を変更⇒変更された控除に合致する作成区分リストを生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generate_Tb5_Cmb_HORYU()
        {
            try
            {
//-- <2016/03/23>
                if (bHORYUNull && bKOUJYONull)
                {
                    Tb4_Grp_GENSEN.Enabled = false;
                    return;
                }
                if (bKOUJYONull)
                {
                    Tb5_Rdo_HORYU2.Enabled = false;
                }
                else if (bHORYUNull) { Tb5_Rdo_HORYU1.Enabled = false; }

                // 控除関連の使用しないにチェックが無い時
                if (!Tb5_Rdo_HORYU0.Checked)
                {
                    // 作成区分コンボに値がある
//                    if (Tb5_Cmb_HRKBN.Text != "")
//                    {
                        //　作成区分コンボボックス
                        //string[,] sArray = null;
                        string[] sArray = null;

                        // 支払保留なら1、自動控除なら2
                        int iHORYUKBN = (Tb5_Rdo_HORYU1.Checked == true ? 1 : 2);

                        if (iHORYUKBN == 1)
                        {
                            sArray = null;
                            mcBsLogic.Get_SKBNM(25, out sArray);
                        }
                        else
                        {
                            sArray = null;
                            mcBsLogic.Get_SKBNM(23, out sArray);
                        }

                        //データがあった場合は作成区分のリストを生成
                        if (sArray != null)
                        {
                            //控除＋の数だけLOOP
                            System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                            list = (
                                new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                            );
                            for (int i = 0; i < sArray.Length; )
                            {
                                int iCombo = Convert.ToInt32(sArray[i].ToString().Substring(0, sArray[i].ToString().IndexOf(':')));
                                string sCombo = sArray[i].ToString();

                                list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                                i++;
                            }
                            Tb5_Cmb_HRKBN.DisplayMember = "Value";
                            Tb5_Cmb_HRKBN.ValueMember = "Key";
                            Tb5_Cmb_HRKBN.DataSource = list;
                        }
                        else
                        {
//-- <>
                            if (iHORYUKBN == 1)
                            {
                                Tb5_Rdo_HORYU1.Enabled = false;
                            }
                            else { Tb5_Rdo_HORYU2.Enabled = false; }

                            //Tb5_Cmb_HRKBN.DataSource = null;
////                            Tb5_Rdo_HORYU0.Checked = true;
////                            Tb4_Grp_KJ.Enabled = false;
                        }
//                    }
//                    else
//                    {
//                        //Tb5_Cmb_HRKBN.DataSource = null;
//                    }

                    Tb5_Txt_HR_KIJYUN.ClearValue();
                    Tb5_Cmb_HORYU_F.SelectedIndex = -1;
                    if (Tb5_Rdo_HORYU1.Checked)
                    {
                        Tb5_Txt_HOVAL.Text = "100.000";
                        Tb5_Txt_HOVAL.ExNumValue = 100.000M;
                    }
                    else if (Tb5_Rdo_HORYU2.Checked)
                    {
                        Tb5_Txt_HOVAL.Text = "0.000";
                        Tb5_Txt_HOVAL.ExNumValue = 0.000M;
                    }
                    Tb5_Txt_HRORYUGAKU.Text = "";

                }
//-- <2016/03/22>
                else
                {
                    Tb5_Cmb_HRKBN.SelectedIndex = -1;
                    Tb5_Txt_HR_KIJYUN.ClearValue();
                    Tb5_Cmb_HORYU_F.SelectedIndex = -1;
                    Tb5_Txt_HOVAL.Text = "100.000";
                    Tb5_Txt_HOVAL.ExNumValue = 100.000M;
                    Tb5_Txt_HRORYUGAKU.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTB5_Cmb_HORYU_TextChange　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
//--

        #region 画面とのデータI/O

        #region 見直し　元コメントアウト　2016/02/22
        //--　<9999>
//        /// <summary>
//        /// 画面へのデータセット(初期用)
//        /// </summary>
//        public void SetDispVal_Init_OLD()
//        {
//            Txt_TRFURI.ClearValue();                                                        // 共通　フリガナ

//            Txt_GRPID.ClearValue();                                                         // 共通　取引先グループコード
//            Txt_GRPNM.ClearValue();                                                         // 共通　取引先グループ名称

//            Txt_SaikenDaihyoCd.ClearValue();                                                // 共通　入金代表者取引先コード
//            Txt_SaikenDaihyoHj.ClearValue();                                                // 共通　入金代表者取引先補助コード
//            Txt_SaimuDaihyoCd.ClearValue();                                                 // 共通　支払代表者取引先コード
//            Txt_SaimuDaihyoHj.ClearValue();                                                 // 共通　支払代表者取引先補助コード

//            Tb1_Txt_ZIP.Text = "";                                                          // 基本設定　郵便番号
//            Tb1_Txt_ADDR1.Text = "";                                                        // 基本設定　住所（上段）
//            Tb1_Txt_ADDR2.Text = "";                                                        // 基本設定　住所（下段）
//            Tb1_Txt_SBUSYO.Text = "";                                                       // 基本設定　部署
//            Tb1_Txt_STANTO.Text = "";                                                       // 基本設定　相手先担当者名
//            Tb1_Txt_TEL.Text = "";                                                          // 基本設定　電話番号
//            Tb1_Txt_FAX.Text = "";                                                          // 基本設定　ＦＡＸ番号
//            Tb1_Txt_TRMAIL.ClearValue();                                                    // 基本設定　メールアドレス
//            Tb1_Txt_TRURL.ClearValue();                                                     // 基本設定　ホームページURL
//            Tb1_Txt_BIKO.ClearValue();                                                      // 基本設定　備考
//            Tb1_Txt_E_TANTOCD.ClearValue();                                                 // 基本設定　営業担当者
//            Tb1_Txt_E_TANTONM.ClearValue();                                                 // 基本設定　営業担当者名称
//            Tb1_Txt_UsrNo.ClearValue();                                                     // 基本設定　全銀電子債権ネットワーク　利用者番号
//            Tb1_Chk_Jyoto.Checked = false;                                                  // 基本設定　全銀電子債権ネットワーク　債務者請求時、譲渡を制限する
//            Tb1_Txt_MYNO_AITE.ClearValue();                                                 // 基本設定　マイナンバー精度　法人番号
//            Tb1_Chk_SOSAI.Checked = false;                                                  // 基本設定　相殺処理　相殺を許可する
////-- <2016/02/14 会社登録で相殺使用しないを追加>
//            if (Global.nSOSAI_F == 0)
//            { Tb1_Chk_SOSAI.Enabled = false; }
////-- <2016/02/14>
//            Tb1_Chk_SRYOU_F.Checked = false;                                                // 基本設定　相殺処理　相殺領収書を発行する

//            Tb2_Txt_TOKUKANA.ClearValue();                                                  // 回収設定　照合用フリガナ
//            Tb2_Txt_SHIME.ClearValue();                                                     // 回収設定　回収予定設定　締日
//            Tb2_Txt_KAISYUHI_M.ClearValue();                                                // 回収設定　回収予定設定　回収予定　月
//            Tb2_Txt_KAISYUHI_D.ClearValue();                                                // 回収設定　回収予定設定　回収予定　日
//            Tb2_Txt_KAISYUSIGHT_M.ClearValue();                                             // 回収設定　回収予定設定　回収期日　月
//            Tb2_Txt_KAISYUSIGHT_D.ClearValue();                                             // 回収設定　回収予定設定　回収期日　日

//            Tb2_Txt_Y_KINGAKU.ClearValue();                                                 // 回収設定　回収予定設定　約定金額
//            Tb2_Txt_BUNKATSU_1.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上①　分割率
//            Tb2_Txt_SIGHT_M_1.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上①　月
//            Tb2_Txt_SIGHT_D_1.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上①　日
//            Tb2_Txt_BUNKATSU_2.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上②　分割率
//            Tb2_Txt_SIGHT_M_2.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上②　月
//            Tb2_Txt_SIGHT_D_2.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上②　日
//            Tb2_Txt_BUNKATSU_3.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上③　分割率
//            Tb2_Txt_SIGHT_M_3.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上③　月
//            Tb2_Txt_SIGHT_D_3.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上③　日

//            Tb2_Txt_Y_KINGAKU.Enabled = false;                                              // 回収設定　回収予定設定　約定金額
//            Tb2_Txt_Y_KINGAKU_EN.Enabled = false;                                           // 回収設定　回収予定設定　約定金額　円表示
//            Tb2_Cmb_MIMAN.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額未満　入金区分コンボ
//            Tb2_Cmb_IJOU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①　入金区分コンボ
//            Tb2_Txt_BUNKATSU_1.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上①　分割率
//            Tb2_Cmb_HASU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①　端数設定コンボ
//            Tb2_Txt_SIGHT_M_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　月
//            Tb2_Txt_SIGHT_D_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　日
//            Tb2_Cmb_IJOU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②　入金区分コンボ
//            Tb2_Txt_BUNKATSU_2.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上②　分割率
//            Tb2_Cmb_HASU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②　端数設定コンボ
//            Tb2_Txt_SIGHT_M_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　月
//            Tb2_Txt_SIGHT_D_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　日
//            Tb2_Cmb_IJOU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③　入金区分コンボ
//            Tb2_Txt_BUNKATSU_3.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上③　分割率
//            Tb2_Cmb_HASU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③　端数設定コンボ
//            Tb2_Txt_SIGHT_M_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　月
//            Tb2_Txt_SIGHT_D_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　日　

//            Tb2_Lbl_Y_KINGAKU.Enabled = false;                                              // 回収設定　回収予定設定　約定金額ラベル
//            Tb2_Lbl_MIMAN.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額未満ラベル
//            Tb2_Lbl_IJOU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①ラベル
//            Tb2_Lbl_IJOU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②ラベル
//            Tb2_Lbl_IJOU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③ラベル
//            Tb2_Lbl_BUNKATSU.Enabled = false;                                               // 回収設定　回収予定設定　約定金額以上　分割ラベル
//            Tb2_Lbl_BUNKATSU_1.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上①　％ラベル
//            Tb2_Lbl_BUNKATSU_2.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上②　％ラベル
//            Tb2_Lbl_BUNKATSU_3.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上③　％ラベル
//            Tb2_Cmb_HASU.Enabled = false;                                                   // 回収設定　回収予定設定　約定金額以上　端数ラベル
//            Tb2_Lbl_SIGHT.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額以上　回収期日ラベル
//            Tb2_Lbl_SIGHT_M_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　ヶ月後ラベル
//            Tb2_Lbl_SIGHT_M_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　ヶ月後ラベル
//            Tb2_Lbl_SIGHT_M_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　ヶ月後ラベル
//            Tb2_Lbl_SIGHT_D_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　日ラベル
//            Tb2_Lbl_SIGHT_D_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　日ラベル
//            Tb2_Lbl_SIGHT_D_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　日ラベル

//            Tb2_Txt_SEN_GINKOCD.ClearValue();                                               // 回収設定　専用入金口座　銀行コード
//            Tb2_Txt_SEN_GINKONM.ClearValue();                                               // 回収設定　専用入金口座　銀行名称
//            Tb2_Txt_SEN_SITENCD.ClearValue();                                               // 回収設定　専用入金口座　支店コード
//            Tb2_Txt_SEN_KSITENCD.ClearValue();                                              // 回収設定　専用入金口座　仮想支店コード
//            Tb2_Txt_SEN_KSITENNM.ClearValue();                                              // 回収設定　専用入金口座　仮想支店名称
//            Tb2_Txt_SEN_KOZANO.ClearValue();                                                // 回収設定　専用入金口座　口座番号
//            Tb2_Txt_SHIN_KAISYACD.ClearValue();                                             // 回収設定　各設定　信用調査用企業コード
//            Tb2_Txt_YOSIN.ClearValue();                                                     // 回収設定　各設定　与信限度額
//            Tb2_Txt_YOSHINRANK.ClearValue();                                                // 回収設定　各設定　与信ランク
//            Tb2_Txt_GAIKA_KEY_F.ClearValue();                                               // 回収設定　外貨関連　照合キー（前）
//            Tb2_Txt_GAIKA_KEY_B.ClearValue();                                               // 回収設定　外貨関連　照合キー（後）
//            Tb2_Txt_HIFURIKOZA_1.ClearValue();                                              // 回収設定　被振込口座設定　口座ID①
//            Tb2_Txt_HIBKCD_1.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード①
//            Tb2_Txt_HIBKNM_1.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称①
//            Tb2_Txt_HIBRCD_1.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード①
//            Tb2_Txt_HIBRNM_1.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称①
//            Tb2_Txt_HIYOKN_1.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別①
//            Tb2_Txt_HIKOZANO_1.ClearValue();                                                // 回収設定　被振込口座設定　口座番号①
//            Tb2_Txt_HIFURIKOZA_2.ClearValue();                                              // 回収設定　被振込口座設定　口座ID②
//            Tb2_Txt_HIBKCD_2.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード②
//            Tb2_Txt_HIBKNM_2.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称②
//            Tb2_Txt_HIBRCD_2.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード②
//            Tb2_Txt_HIBRNM_2.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称②
//            Tb2_Txt_HIYOKN_2.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別②
//            Tb2_Txt_HIKOZANO_2.ClearValue();                                                // 回収設定　被振込口座設定　口座番号②
//            Tb2_Txt_HIFURIKOZA_3.ClearValue();                                              // 回収設定　被振込口座設定　口座ID③
//            Tb2_Txt_HIBKCD_3.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード③
//            Tb2_Txt_HIBKNM_3.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称③
//            Tb2_Txt_HIBRCD_3.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード③
//            Tb2_Txt_HIBRNM_3.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称③
//            Tb2_Txt_HIYOKN_3.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別③
//            Tb2_Txt_HIKOZANO_3.ClearValue();                                                // 回収設定　被振込口座設定　口座番号③

//            Tb2_Txt_SEN_SITENCD.Enabled = false;                                            // 回収設定　専用入金口座　支店コード
//            Tb2_Txt_SEN_KSITENCD.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店コード
//            Tb2_Txt_SEN_KSITENNM.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店名称
//            Tb2_Lbl_SEN_SITENCD.Enabled = false;                                            // 回収設定　専用入金口座　支店ラベル
//            Tb2_Lbl_SEN_KSITENCD.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店ラベル

//            Tb2_Cmb_TSUKA.Enabled = false;                                                  // 回収設定　外貨関連　取引通貨コンボ
//            Tb2_Txt_GAIKA_KEY_F.Enabled = false;                                            // 回収設定　外貨関連　照合キー（前）
//            Tb2_Txt_GAIKA_KEY_B.Enabled = false;                                            // 回収設定　外貨関連　照合キー（後）
//            Tb2_Lbl_TSUKA.Enabled = false;                                                  // 回収設定　外貨関連　取引通貨ラベル
//            Tb2_Lbl_GAIKA_KEY_F.Enabled = false;                                            // 回収設定　外貨関連　照合キー（前）ラベル
//            Tb2_Lbl_GAIKA_KEY_T.Enabled = false;                                            // 回収設定　外貨関連　照合キー（後）ラベル

//            Tb3_Txt_ShimeNm.ClearValue();

//            //**ICS-S 2012/05/22
//            //**Cmb_KEICD.SelectedIndex = 1;
//            //Cmb_KEICD.SelectedIndex = 0;
//            if (Tb1_Cmb_KEICD.Items.Count > 0) { Tb1_Cmb_KEICD.SelectedIndex = 0; }
//            //**ICS-E
//            Tb5_Txt_STAN_CD.Text = "";
//            Tb5_Txt_STAN_NM.Text = "";
//            Tb5_Txt_SBCOD.ExCodeDB = "";
//            Tb5_Txt_SBCOD_NM.Text = "";
//            Tb5_Txt_SKCOD.ExCodeDB = "";
//            Tb5_Txt_SKINM.Text = "";
//            Tb5_Chk_NAYOSE.Checked = true;
//            //Chk_F_SETUIN.Checked = false;
//            Tb5_Chk_F_SETUIN.Checked = true;
//            Tb3_Txt_BCOD.ExCodeDB = "";
//            Tb3_Txt_BNAM.Text = "";
//            Tb3_Txt_KCOD.ExCodeDB = "";
//            Tb3_Txt_KINM.Text = "";
//            Tb3_Txt_SHINO.Text = "";
//            Tb3_Txt_SHINM.Text = "";
//            Tb3_Txt_SHIMEBI.Text = "";
//            Tb3_Txt_SHIHARAIMM.Text = "";
//            Tb3_Txt_SIHARAIDD.Text = "";
//            Tb3_Txt_SKIJITUMM.Text = "";
//            Tb3_Txt_SKIJITUDD.Text = "";
//            Tb3_Cmb_HARAI_H.Text = "0:前営業日";
//            Tb3_Cmb_KIJITU_H.Text = "0:前営業日";
//            Lbl_Old_New1.Text = "";
//            Tb1_Lbl_SHO_ID_V.Text = "";
//            Tb3_Txt_SKBNCOD.Text = "";
//            Tb3_Txt_V_YAKUJO.Text = "";
//            Tb3_Txt_YAKUJOA_L.Text = "";
//            Tb3_Txt_YAKUJOA_M.Text = "";
//            Tb3_Txt_YAKUJOB_LH.Text = "";
//            Tb3_Txt_YAKUJOB_H1.Text = "";
//            Tb3_Txt_YAKUJOB_R1.Text = "";
//            Tb3_Txt_YAKUJOB_U1.Text = "";
//            Tb3_Txt_YAKUJOB_H2.Text = "";
//            Tb3_Txt_YAKUJOB_R2.Text = "";
//            Tb3_Txt_YAKUJOB_U2.Text = "";
//            Tb3_Txt_YAKUJOB_H3.Text = "";
//            Tb3_Txt_YAKUJOB_R3.Text = "";
//            Tb3_Txt_YAKUJOB_U3.Text = "";
//            Txt_STYMD.Value = 0;
//            Txt_EDYMD.Value = 0;
//            Txt_LUSR.Text = "";
//            Txt_LMOD.Text = "";
//            Tb_Main.SelectedIndex = 0;
//            Tb_Main.Enabled = false;

//            Tb3_Lbl_Old_New2.Text = "";
//            Tb4_Lbl_GIN_ID_V.Text = "";
//            Tb4_Lbl_Old_New3.Text = "";
//            BindNavi1.Enabled = true;
//            {
//                int count;
//                mcBsLogic.Cnt_TRCD(out count);
//                BindNavi1_First.Enabled = (count > 0);
//                BindNavi1_Prev.Enabled = (count > 0);
//            }
//            //BindNavi1_First.Enabled = false; //true;
//            //BindNavi1_Prev.Enabled = false; // true;
//            BindNavi1_Next.Enabled = false;
//            BindNavi1_End.Enabled = false;
//            Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";
//            Tb4_Cmb_TESUU.Text = "1:自社負担";
//            Tb4_Cmb_SOUKIN.Text = "7:電信";
//            //Tb4_Radio_KAIIN0.Checked = true;
//            //Tb5_Cmb_HORYU_F.Text = "0:比率";
//            Tb5_Cmb_F_SOUFU.Text = "0:送付しない";
//            Tb5_Cmb_ANNAI.Text = "1:パターン１";
//            Tb5_Cmb_TSOKBN.Text = "1:自社負担";
//            //**>>ICS-S 2013/03/04
//            //**Tb5_Cmb_SHITU.Text = "0:印刷しない";
//            Tb5_Cmb_SHITU.Text = "1:印刷する";
//            //**<<ICS-E

//            Tb5_Txt_HOVAL.ExNumValue = 100;
//            Tb5_Txt_HRORYUGAKU.ClearValue();
//            Tb5_Cmb_HORYU_F.SelectedIndex = -1;
//            Tb5_Cmb_HRKBN.SelectedIndex = -1;

//            Tb6_Txt_TEGVAL.ClearValue();
//            Tb6_Txt_ENG_NAME.ClearValue();
//            Tb6_Txt_ENG_ADDR.ClearValue();
//            Tb6_Txt_ENG_KZNO.ClearValue();
//            Tb6_Txt_ENG_SWIF.ClearValue();
//            Tb6_Txt_ENG_BNKNAM.ClearValue();
//            Tb6_Txt_ENG_BRNNAM.ClearValue();
//            Tb6_Txt_ENG_BNKADDR.ClearValue();
//        }
        //-- <9999>
        #endregion

        /// <summary>
        /// 画面へのデータセット(初期用)
        /// </summary>
        public void SetDispVal_Init()
        {
            //　【　共　通　】
            // 値
            Txt_TRFURI.ClearValue();                                                        // 共通　フリガナ
            Txt_GRPID.ClearValue();                                                         // 共通　取引先グループコード
            Txt_GRPNM.ClearValue();                                                         // 共通　取引先グループ名称
            Txt_SaikenDaihyoCd.ClearValue();                                                // 共通　入金代表者取引先コード
            Txt_SaikenDaihyoHj.ClearValue();                                                // 共通　入金代表者取引先補助コード
            Txt_SaimuDaihyoCd.ClearValue();
            Txt_SaimuDaihyoHj.ClearValue();

            //　【　基本設定タブ　】
            // 値
            Tb1_Txt_ZIP.Text = "";                                                          // 基本設定　郵便番号
            Tb1_Txt_ADDR1.Text = "";                                                        // 基本設定　住所（上段）
            Tb1_Txt_ADDR2.Text = "";                                                        // 基本設定　住所（下段）
            Tb1_Txt_SBUSYO.Text = "";                                                       // 基本設定　部署
            Tb1_Txt_STANTO.Text = "";                                                       // 基本設定　相手先担当者名
            Tb1_Txt_TEL.Text = "";                                                          // 基本設定　電話番号
            Tb1_Txt_FAX.Text = "";                                                          // 基本設定　ＦＡＸ番号
            Tb1_Txt_TRMAIL.ClearValue();                                                    // 基本設定　メールアドレス
            Tb1_Txt_TRURL.ClearValue();                                                     // 基本設定　ホームページURL
            Tb1_Txt_BIKO.ClearValue();                                                      // 基本設定　備考
            Tb1_Txt_E_TANTOCD.ClearValue();                                                 // 基本設定　営業担当者
            Tb1_Txt_E_TANTONM.ClearValue();                                                 // 基本設定　営業担当者名称
            Tb1_Txt_UsrNo.ClearValue();                                                     // 基本設定　全銀電子債権ネットワーク　利用者番号
            Tb1_Chk_Jyoto.Checked = false;                                                  // 基本設定　全銀電子債権ネットワーク　債務者請求時、譲渡を制限する
            Tb1_Txt_MYNO_AITE.ClearValue();                                                 // 基本設定　マイナンバー制度　法人番号
            Tb1_Chk_SOSAI.Checked = false;                                                  // 基本設定　相殺処理　相殺を許可する
            Tb1_Chk_SRYOU_F.Checked = false;                                                // 基本設定　相殺処理　相殺領収書を発行する

            // 状態
            if (Global.nSOSAI_F == 0)                                                       // 基本設定　債権債務会社マスター　相殺を使用しない場合
            { Tb1_Chk_SOSAI.Enabled = false; }                                                  // 相殺処理　相殺を許可するを非活性化

            //　【　回収設定タブ　】
            // 入金消込設定グループ
            // 値
            Tb2_Txt_TOKUKANA.ClearValue();                                                  // 回収設定　照合用フリガナ

            // 回収予定設定グループ
            // 値
            Tb2_Txt_SHIME.ClearValue();                                                     // 回収設定　回収予定設定　締日
            Tb2_Txt_KAISYUHI_M.ClearValue();                                                // 回収設定　回収予定設定　回収予定　月
            Tb2_Txt_KAISYUHI_D.ClearValue();                                                // 回収設定　回収予定設定　回収予定　日
            Tb2_Txt_KAISYUSIGHT_M.ClearValue();                                             // 回収設定　回収予定設定　回収期日　月
            Tb2_Txt_KAISYUSIGHT_D.ClearValue();                                             // 回収設定　回収予定設定　回収期日　日
            Tb2_Txt_Y_KINGAKU.ClearValue();                                                 // 回収設定　回収予定設定　約定金額
            Tb2_Txt_BUNKATSU_1.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上①　分割率
            Tb2_Txt_SIGHT_M_1.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上①　月
            Tb2_Txt_SIGHT_D_1.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上①　日
            Tb2_Txt_BUNKATSU_2.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上②　分割率
            Tb2_Txt_SIGHT_M_2.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上②　月
            Tb2_Txt_SIGHT_D_2.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上②　日
            Tb2_Txt_BUNKATSU_3.ClearValue();                                                // 回収設定　回収予定設定　約定金額以上③　分割率
            Tb2_Txt_SIGHT_M_3.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上③　月
            Tb2_Txt_SIGHT_D_3.ClearValue();                                                 // 回収設定　回収予定設定　約定金額以上③　日
            // 状態
            Tb2_Txt_KAISYUSIGHT_M.Enabled = false;                                          // 回収設定　回収予定設定　回収期日　月
            Tb2_Txt_KAISYUSIGHT_D.Enabled = false;                                          // 回収設定　回収予定設定　回収期日　日
            Tb2_Txt_Y_KINGAKU.Enabled = false;                                              // 回収設定　回収予定設定　約定金額
            Tb2_Cmb_MIMAN.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額未満　入金区分コンボ
            Tb2_Cmb_IJOU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①　入金区分コンボ
            Tb2_Txt_BUNKATSU_1.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上①　分割率
            Tb2_Cmb_HASU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①　端数設定コンボ
            Tb2_Txt_SIGHT_M_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　月
            Tb2_Txt_SIGHT_D_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　日
            Tb2_Cmb_IJOU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②　入金区分コンボ
            Tb2_Txt_BUNKATSU_2.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上②　分割率
            Tb2_Cmb_HASU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②　端数設定コンボ
            Tb2_Txt_SIGHT_M_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　月
            Tb2_Txt_SIGHT_D_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　日
            Tb2_Cmb_IJOU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③　入金区分コンボ
            Tb2_Txt_BUNKATSU_3.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上③　分割率
            Tb2_Cmb_HASU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③　端数設定コンボ
            Tb2_Txt_SIGHT_M_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　月
            Tb2_Txt_SIGHT_D_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　日　
            // 状態　ラベル関係
            Tb2_Lbl_SIGHT_Main.Enabled = false;
            Tb2_Lbl_SIGHT_D.Enabled = false;
            Tb2_Lbl_SIGHT_M.Enabled = false;
            Tb2_Lbl_Y_KINGAKU.Enabled = false;                                              // 回収設定　回収予定設定　約定金額ラベル
            Tb2_Lbl_Y_KINGAKU_EN.Enabled = false;                                           // 回収設定　回収予定設定　約定金額円ラベル
            Tb2_Lbl_MIMAN.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額未満ラベル
            Tb2_Lbl_IJOU_1.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上①ラベル
            Tb2_Lbl_IJOU_2.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上②ラベル
            Tb2_Lbl_IJOU_3.Enabled = false;                                                 // 回収設定　回収予定設定　約定金額以上③ラベル
            Tb2_Lbl_BUNKATSU.Enabled = false;                                               // 回収設定　回収予定設定　約定金額以上　分割ラベル
            Tb2_Lbl_BUNKATSU_1.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上①　％ラベル
            Tb2_Lbl_BUNKATSU_2.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上②　％ラベル
            Tb2_Lbl_BUNKATSU_3.Enabled = false;                                             // 回収設定　回収予定設定　約定金額以上③　％ラベル
            Tb2_Lbl_HASU.Enabled = false;                                                   // 回収設定　回収予定設定　約定金額以上　端数ラベル
            Tb2_Lbl_SIGHT.Enabled = false;                                                  // 回収設定　回収予定設定　約定金額以上　回収期日ラベル
            Tb2_Lbl_SIGHT_M_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　ヶ月後ラベル
            Tb2_Lbl_SIGHT_M_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　ヶ月後ラベル
            Tb2_Lbl_SIGHT_M_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　ヶ月後ラベル
            Tb2_Lbl_SIGHT_D_1.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上①　日ラベル
            Tb2_Lbl_SIGHT_D_2.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上②　日ラベル
            Tb2_Lbl_SIGHT_D_3.Enabled = false;                                              // 回収設定　回収予定設定　約定金額以上③　日ラベル

            // 専用入金口座グループ
            // 値
            Tb2_Txt_SEN_GINKOCD.ClearValue();                                               // 回収設定　専用入金口座　銀行コード
            Tb2_Txt_SEN_GINKONM.ClearValue();                                               // 回収設定　専用入金口座　銀行名称
            Tb2_Txt_SEN_SITENCD.ClearValue();                                               // 回収設定　専用入金口座　支店コード
//-- <2016/03/11 支店名も表示対応>
            Tb2_Txt_SEN_SITENNM.ClearValue();                                               // 回収設定　専用入金口座　支店名称
//-- <2016/03/11>
            Tb2_Txt_SEN_KSITENCD.ClearValue();                                              // 回収設定　専用入金口座　仮想支店コード
            Tb2_Txt_SEN_KSITENNM.ClearValue();                                              // 回収設定　専用入金口座　仮想支店名称
            Tb2_Cmb_YOKINSYU.SelectedIndex = -1;                                            // 回収設定　専用入金口座　預金種別コンボ
            Tb2_Txt_SEN_KOZANO.ClearValue();                                                // 回収設定　専用入金口座　口座番号
            // 状態
//-- <2016/03/21>
            if (Global.cUsrSec.nMFLG < 2)
            {
                Tb2_Txt_SEN_GINKOCD.Enabled = false;
            }
            else { Tb2_Txt_SEN_GINKOCD.Enabled = true; }
//-- <2016/03/21>
            Tb2_Txt_SEN_SITENCD.Enabled = false;                                            // 回収設定　専用入金口座　支店コード
            Tb2_Txt_SEN_KSITENCD.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店コード
            Tb2_Txt_SEN_KSITENNM.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店名称
            Tb2_Cmb_YOKINSYU.Enabled = false;                                               // 回収設定　専用入金口座　預金種別コンボ
            Tb2_Txt_SEN_KOZANO.Enabled = false;                                             // 回収設定　専用入金口座　口座番号
            Tb2_Lbl_SEN_SITENCD.Enabled = false;                                            // 回収設定　専用入金口座　支店ラベル
            Tb2_Lbl_SEN_KSITENCD.Enabled = false;                                           // 回収設定　専用入金口座　仮想支店ラベル
            Tb2_Lbl_SEN_YOKINSYU.Enabled = false;                                           // 回収設定　専用入金口座　預金種別ラベル
            Tb2_Lbl_SEN_KOZANO.Enabled = false;                                             // 回収設定　専用入金口座　口座番号ラベル
            
            // 各設定グループ
            // 値
//-- <2016/03/14 追加>
            Tb2_Chk_JIDOU_GAKUSYU.Checked = true;                                           // 回収設定　各設定　カナ自動学習
            Tb2_Chk_NYUKIN_YOTEI.Checked = false;                                           // 回収設定　各設定　入金予定利用
            Tb2_Chk_RYOSYUSYO.Checked = false;                                              // 回収設定　各設定　領収書発行する
            Tb2_Chk_TESURYO_GAKUSYU.Checked = false;                                        // 回収設定　各設定　手数料自動学習する
            Tb2_Chk_TESURYO_GOSA.Checked = true;                                            // 回収設定　各設定　手数料誤差利用する
//-- <2016/03/14>
            Tb2_Txt_SHIN_KAISYACD.ClearValue();                                             // 回収設定　各設定　信用調査用企業コード
            Tb2_Txt_YOSIN.ClearValue();                                                     // 回収設定　各設定　与信限度額
            Tb2_Txt_YOSHINRANK.ClearValue();                                                // 回収設定　各設定　与信ランク
            // 状態


            // 外貨関連グループ
            // 値
            Tb2_Txt_GAIKA_KEY_F.ClearValue();                                               // 回収設定　外貨関連　照合キー（前）
            Tb2_Txt_GAIKA_KEY_B.ClearValue();                                               // 回収設定　外貨関連　照合キー（後）
            // 状態
            Tb2_Cmb_TSUKA.Enabled = false;                                                  // 回収設定　外貨関連　取引通貨コンボ
            Tb2_Txt_GAIKA_KEY_F.Enabled = false;                                            // 回収設定　外貨関連　照合キー（前）
            Tb2_Txt_GAIKA_KEY_B.Enabled = false;                                            // 回収設定　外貨関連　照合キー（後）
            Tb2_Lbl_TSUKA.Enabled = false;                                                  // 回収設定　外貨関連　取引通貨ラベル
            Tb2_Lbl_GAIKA_KEY_F.Enabled = false;                                            // 回収設定　外貨関連　照合キー（前）ラベル
            Tb2_Lbl_GAIKA_KEY_T.Enabled = false;                                            // 回収設定　外貨関連　照合キー（後）ラベル
            
            
            // 被振込口座設定グループ            
            // 値
            Tb2_Txt_HIFURIKOZA_1.ClearValue();                                              // 回収設定　被振込口座設定　口座ID①
            Tb2_Txt_HIBKCD_1.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード①
            Tb2_Txt_HIBKNM_1.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称①
            Tb2_Txt_HIBRCD_1.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード①
            Tb2_Txt_HIBRNM_1.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称①
            Tb2_Txt_HIYOKN_1.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別①
            Tb2_Txt_HIKOZANO_1.ClearValue();                                                // 回収設定　被振込口座設定　口座番号①
            Tb2_Txt_HIFURIKOZA_2.ClearValue();                                              // 回収設定　被振込口座設定　口座ID②
            Tb2_Txt_HIBKCD_2.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード②
            Tb2_Txt_HIBKNM_2.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称②
            Tb2_Txt_HIBRCD_2.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード②
            Tb2_Txt_HIBRNM_2.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称②
            Tb2_Txt_HIYOKN_2.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別②
            Tb2_Txt_HIKOZANO_2.ClearValue();                                                // 回収設定　被振込口座設定　口座番号②
            Tb2_Txt_HIFURIKOZA_3.ClearValue();                                              // 回収設定　被振込口座設定　口座ID③
            Tb2_Txt_HIBKCD_3.ClearValue();                                                  // 回収設定　被振込口座設定　銀行コード③
            Tb2_Txt_HIBKNM_3.ClearValue();                                                  // 回収設定　被振込口座設定　銀行名称③
            Tb2_Txt_HIBRCD_3.ClearValue();                                                  // 回収設定　被振込口座設定　支店コード③
            Tb2_Txt_HIBRNM_3.ClearValue();                                                  // 回収設定　被振込口座設定　支店名称③
            Tb2_Txt_HIYOKN_3.ClearValue();                                                  // 回収設定　被振込口座設定　預金種別③
            Tb2_Txt_HIKOZANO_3.ClearValue();                                                // 回収設定　被振込口座設定　口座番号③
            // 状態


            //　【　支払条件タブ　】
            // 値
            Tb3_Txt_ShimeNm.ClearValue();                                                   // 支払条件タブ　

            //**ICS-S 2012/05/22
            //**Cmb_KEICD.SelectedIndex = 1;
            //Cmb_KEICD.SelectedIndex = 0;
            if (Tb1_Cmb_KEICD.Items.Count > 0) { Tb1_Cmb_KEICD.SelectedIndex = 0; }
            //**ICS-E
            Tb5_Txt_STAN_CD.Text = "";
            Tb5_Txt_STAN_NM.Text = "";
            Tb5_Txt_SBCOD.ExCodeDB = "";
            Tb5_Txt_SBCOD_NM.Text = "";
            Tb5_Txt_SKCOD.ExCodeDB = "";
            Tb5_Txt_SKINM.Text = "";
            Tb5_Chk_NAYOSE.Checked = true;
            //Chk_F_SETUIN.Checked = false;
            Tb5_Chk_F_SETUIN.Checked = true;
            Tb3_Txt_BCOD.ExCodeDB = "";
            Tb3_Txt_BNAM.Text = "";
            Tb3_Txt_KCOD.ExCodeDB = "";
            Tb3_Txt_KINM.Text = "";
            Tb3_Txt_SHINO.Text = "";
            Tb3_Txt_SHINM.Text = "";
            Tb3_Txt_SHIMEBI.Text = "";
            Tb3_Txt_SHIHARAIMM.Text = "";
            Tb3_Txt_SIHARAIDD.Text = "";
            Tb3_Txt_SKIJITUMM.Text = "";
            Tb3_Txt_SKIJITUDD.Text = "";
            Tb3_Cmb_HARAI_H.Text = "0:前営業日";
            Tb3_Cmb_KIJITU_H.Text = "0:前営業日";
            Lbl_Old_New1.Text = "";
            Tb1_Lbl_SHO_ID_V.Text = "";
            Tb3_Txt_SKBNCOD.Text = "";
            Tb3_Txt_V_YAKUJO.Text = "";
            Tb3_Txt_YAKUJOA_L.Text = "";
            Tb3_Txt_YAKUJOA_M.Text = "";
            Tb3_Txt_YAKUJOB_LH.Text = "";
            Tb3_Txt_YAKUJOB_H1.Text = "";
            Tb3_Txt_YAKUJOB_R1.Text = "";
            Tb3_Txt_YAKUJOB_U1.Text = "";
            Tb3_Txt_YAKUJOB_H2.Text = "";
            Tb3_Txt_YAKUJOB_R2.Text = "";
            Tb3_Txt_YAKUJOB_U2.Text = "";
            Tb3_Txt_YAKUJOB_H3.Text = "";
            Tb3_Txt_YAKUJOB_R3.Text = "";
            Tb3_Txt_YAKUJOB_U3.Text = "";
            Txt_STYMD.Value = 0;
            Txt_EDYMD.Value = 0;
            Txt_LUSR.Text = "";
            Txt_LMOD.Text = "";
            Tb_Main.SelectedIndex = 0;
            Tb_Main.Enabled = false;

            Tb3_Lbl_Old_New2.Text = "";
            Tb4_Lbl_GIN_ID_V.Text = "";
            Tb4_Lbl_Old_New3.Text = "";
            BindNavi1.Enabled = true;
            {
                int count;
                mcBsLogic.Cnt_TRCD(out count);
                BindNavi1_First.Enabled = (count > 0);
                BindNavi1_Prev.Enabled = (count > 0);
            }
            //BindNavi1_First.Enabled = false; //true;
            //BindNavi1_Prev.Enabled = false; // true;
            BindNavi1_Next.Enabled = false;
            BindNavi1_End.Enabled = false;
            Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";
            Tb4_Cmb_TESUU.Text = "1:自社負担";
            Tb4_Cmb_SOUKIN.Text = "7:電信";
            //Tb4_Radio_KAIIN0.Checked = true;
            //Tb5_Cmb_HORYU_F.Text = "0:比率";
            Tb5_Cmb_F_SOUFU.Text = "0:送付しない";
            Tb5_Cmb_ANNAI.Text = "1:パターン１";
            Tb5_Cmb_TSOKBN.Text = "1:自社負担";
            //**>>ICS-S 2013/03/04
            //**Tb5_Cmb_SHITU.Text = "0:印刷しない";
            Tb5_Cmb_SHITU.Text = "1:印刷する";
            //**<<ICS-E

//-- <2016/03/11 >
//            Tb5_Txt_HOVAL.ExNumValue = 100;
            Tb5_Txt_HOVAL.ExNumValue = 100.000M;
//-- <2016/03/11>
            Tb5_Txt_HRORYUGAKU.ClearValue();
            Tb5_Cmb_HORYU_F.SelectedIndex = -1;
            Tb5_Cmb_HRKBN.SelectedIndex = -1;

            Tb5_Txt_TEGVAL.ClearValue();
            Tb6_Txt_ENG_NAME.ClearValue();
            Tb6_Txt_ENG_ADDR.ClearValue();
            Tb6_Txt_ENG_KZNO.ClearValue();
            Tb6_Txt_ENG_SWIF.ClearValue();
            Tb6_Txt_ENG_BNKNAM.ClearValue();
            Tb6_Txt_ENG_BRNNAM.ClearValue();
            Tb6_Txt_ENG_BNKADDR.ClearValue();
        }

//-- <9999>


        /// <summary>
        /// 画面へのデータセット(From財務)
        /// </summary>
        public void SetDispVal_Z()
        {
            try
            {
                //取引先CD変更フラグをリセット
                nTRCDflg = 0;
                //画面項目のセット
                if (Global.nTRCD_Type == 0)
                {
                    Txt_TRCD.ExCodeDB = Global.TRCD.PadLeft(Global.nTRCD_Len, '0');
                }
                else
                {
                    Txt_TRCD.ExCodeDB = Global.TRCD.TrimEnd(' ');
                }
                Txt_RYAKU.Text = Global.RYAKU;       //取引先略称
                Txt_TORI_NAM.Text = Global.TORI_NAM; //取引先名称
                Txt_KNLD.Text = Global.KNLD;         //50音
                Lbl_Old_New1.Text = "【　新規　】";
                //BindNavi1.Enabled = false;           //
                //Tb_Main.SelectedIndex = 0;            // <---V01.15.01 HWY DELETE ◀(6490)
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSetDispVal_Z　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// 画面へのデータセット(From債務)
        /// </summary>
        public void SetDispVal_S()
        {
            try
            {
                // V12.03.01 SS_TORI,SS_TSHOH,SS_FRIGINの中から最新の更新日時を取得
                mcBsLogic.Get_UPDATE();

                mcBsLogic.Get_ZYMD();

                //支払依頼から呼ばれたときは除外
                if (Global.nDispMode != 0)
                {
                    //取引先CD変更フラグをリセット
                    nTRCDflg = 0;
                }

                //画面項目のセット
                //if (Global.nTRCD_Type == 0 && Global.TRCD != "")
                //{
                //    Txt_TRCD.ExCodeDB = Global.TRCD.PadLeft(Global.nTRCD_Len, '0');
                //}
                //else
                //{
                //    Txt_TRCD.ExCodeDB = Global.TRCD.TrimEnd(' ');
                //}

                SetTRCDText(Global.TRCD);

                if (Global.nTRCD_HJ == 1 && Global.HJCD != "")
                {
                    Lbl_Haifun.Enabled = true;
                    Txt_HJCD.ReadOnlyEx = false;
                    Txt_HJCD.Text = Global.HJCD.PadLeft(6, '0');
                }
                else
                {
                    Lbl_Haifun.Enabled = false;
                    Txt_HJCD.ReadOnlyEx = true;
                    Txt_HJCD.Text = "";
                }
                Txt_RYAKU.Text = Global.RYAKU;
                Txt_TORI_NAM.Text = Global.TORI_NAM;
                Txt_KNLD.Text = Global.KNLD;

                Cbo_SAIKEN.SelectedIndexChanged -= Cbo_SAIKEN_SelectedIndexChanged;
                Cbo_SAIKEN.DataSource = Create_Cbo_SAIKEN_List((mcBsLogic.Exists_Nyukin_Data(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text) == false));
                Cbo_SAIKEN.SelectedIndexChanged += Cbo_SAIKEN_SelectedIndexChanged;

                Cbo_SAIMU.SelectedIndexChanged -= Cbo_SAIMU_SelectedIndexChanged;
                Cbo_SAIMU.DataSource = Create_Cbo_SAIMU_List((mcBsLogic.Exists_Siharai_Data(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text) == false));
                Cbo_SAIMU.SelectedIndexChanged += Cbo_SAIMU_SelectedIndexChanged;

                // --->V01.15.01 HWY ADD ▼(6490)
                Cbo_SAIKEN.SelectedIndexChanged -= Cbo_SAIKEN_SelectedIndexChanged;
                Cbo_SAIMU.SelectedIndexChanged -= Cbo_SAIMU_SelectedIndexChanged;
                // <---V01.15.01 HWY ADD ▲(6490)
                switch (Global.TGASW)
                {
                    case "0":
                        Cbo_SAIKEN.SelectedValue = (Global.SAIKEN == "1" ? sUse : sNotUse);
                        Cbo_SAIMU.SelectedValue = (Global.SAIMU == "1" ? sUse : sNotUse);
                        break;
                    case "1":
                        Cbo_SAIKEN.SelectedValue = sDueOnly;
                        Cbo_SAIMU.SelectedValue = sDueOnly;
                        break;
                    case "2":
                        Cbo_SAIKEN.SelectedValue = sDueOnly;
                        Cbo_SAIMU.SelectedValue = (Global.SAIMU == "1" ? sUse : sNotUse);
                        break;
                    case "3":
                        Cbo_SAIKEN.SelectedValue = (Global.SAIKEN == "1" ? sUse : sNotUse);
                        Cbo_SAIMU.SelectedValue = sDueOnly;
                        break;
                    default:
                        break;
                }
                // --->V01.15.01 HWY ADD ▼(6490)
                Cbo_SAIKEN.SelectedIndexChanged += Cbo_SAIKEN_SelectedIndexChanged;
                Cbo_SAIMU.SelectedIndexChanged += Cbo_SAIMU_SelectedIndexChanged;
                // <---V01.15.01 HWY ADD ▲(6490)
                TAB_Enable_Control();

                Lbl_Old_New1.Text = "【　変更　】";
                BindNavi1.Enabled = true;
                if (Global.ZIP != "")                                                                                   // 郵便番号　タブ１
                {
                    sZIP_Before = Global.ZIP;
                    Tb1_Txt_ZIP.Text = Global.ZIP;
                }
                else
                {
                    sZIP_Before = "";
                    Tb1_Txt_ZIP.Text = "";
                }
                // de3_10677 差分 -->
                Tb5_Txt_STAN_NM.Text = "";
                Tb5_Txt_SBCOD_NM.Text = "";
                Tb5_Txt_SKINM.Text = "";
                // de3_10677 差分 <--
                Txt_ZIP_Leave(Tb1_Txt_ZIP, null);
                Tb1_Txt_ADDR1.Text = Global.ADDR1;                                                                      // 住所１　タブ１
                Tb1_Txt_ADDR2.Text = Global.ADDR2;                                                                      // 住所２　タブ１
                Tb1_Txt_SBUSYO.Text = Global.SBUSYO;                                                                    // 部署名　タブ１
                Tb1_Txt_STANTO.Text = Global.STANTO;                                                                    // 相手先担当者　タブ１
                Tb1_Txt_TEL.Text = Global.TEL;                                                                          // 電話番号　タブ１
                Tb1_Txt_FAX.Text = Global.FAX;                                                                          // FAX番号　タブ１
                Tb1_Cmb_KEICD.SelectedValue = (Global.KEICD != "" ? Convert.ToInt32(Global.KEICD) : -1);                // 敬称ID　タブ１
                Tb5_Txt_STAN_CD.Text = (Global.nKMAN == 0 ? "" : Global.STAN);                                          // 主担当者　タブ５
                if (Tb5_Txt_STAN_CD.Text != "")
                {
                    sTNAM = mcBsLogic.Get_TNAM(Global.STAN);
                    if (sTNAM != "")
                    {
                        Tb5_Txt_STAN_CD.IsError = false;
                        Tb5_Txt_STAN_NM.Text = sTNAM;
                    }
                    else
                    {
                        Tb5_Txt_STAN_CD.IsError = true;
                        Tb5_Txt_STAN_CD.Focus();
                        // de3_10677 差分 -->
                        //return;
                        // de3_10677 差分 <--
                    }
                }
                else if (Tb5_Txt_STAN_CD.Text == "")
                {
                    Tb5_Txt_STAN_CD.IsError = false;
                    Tb5_Txt_STAN_NM.Text = "";
                }
                if (Global.nKCOD_Type == 0 && Global.SKICD != "")                                                      
                {
                    string sKCOD = mcBsLogic.Conv_KICDtoKCOD(Global.SKICD);
                    Tb5_Txt_SKCOD.ExCodeDB = sKCOD.PadLeft(Global.nKCOD_Len, '0');
                }
                else if (Global.nKCOD_Type == 1 && Global.SKICD != "")
                {
                    string sKCOD = mcBsLogic.Conv_KICDtoKCOD(Global.SKICD);
                    Tb5_Txt_SKCOD.ExCodeDB = sKCOD.TrimEnd(' ');
                }
                else
                {
                    Tb5_Txt_SKCOD.ExCodeDB = "";
                }
                if (Tb5_Txt_SKCOD.ExCodeDB != "")
                {
                    string sKCOD = Tb5_Txt_SKCOD.ExCodeDB;
                    sKCOD = Global.nKCOD_Type == 0 ? sKCOD.PadLeft(Global.nKCOD_Len, '0') : sKCOD.PadRight(Global.nKCOD_Len);
                    sKNAM = mcBsLogic.Get_KNAM(sKCOD);
                    if (sKNAM != "")
                    {
                        Tb5_Txt_SKCOD.IsError = false;
                        Tb5_Txt_SKINM.Text = sKNAM;
                    }
                    else
                    {
                        Tb5_Txt_SKCOD.IsError = true;
                        Tb5_Txt_SKCOD.Focus();
                        // de3_10677 差分 -->
                        //return;
                        // de3_10677 差分 <--
                    }
                }
                else if (Tb5_Txt_SKCOD.ExCodeDB == "")
                {
                    Tb5_Txt_SKCOD.IsError = false;
                    Tb5_Txt_SKINM.Text = "";
                }
                if (Global.nBCOD_F == 1)
                {
                    //部門コード使用可
                    if (Global.nBCOD_Type == 0 && Global.SBCOD != "")
                    {
                        Tb5_Txt_SBCOD.ExCodeDB = Global.SBCOD.PadLeft(Global.nBCOD_Len, '0');
                    }
                    else if (Global.nBCOD_Type == 1 && Global.SBCOD != "")
                    {
                        Tb5_Txt_SBCOD.ExCodeDB = Global.SBCOD.TrimEnd(' ');
                    }
                    else
                    {
                        Tb5_Txt_SBCOD.ExCodeDB = "";
                    }
                }
                else
                {
                    //部門コード使用不可
                    Tb5_Txt_SBCOD.ExCodeDB = "";
                }
                if (Tb5_Txt_SBCOD.ExCodeDB != "")
                {
                    //sBNAM = mcBsLogic.Get_BNAM(Global.SBCOD.TrimEnd(' '));
                    sBNAM = mcBsLogic.Get_BNAM(Global.SBCOD);
                    if (sBNAM != "")
                    {
                        Tb5_Txt_SBCOD.IsError = false;
                        Tb5_Txt_SBCOD_NM.Text = sBNAM;
                    }
                    else
                    {
                        Tb5_Txt_SBCOD.IsError = true;
                        Tb5_Txt_SBCOD.Focus();
                        // de3_10677 差分 -->
                        //return;
                        // de3_10677 差分 <--
                    }
                }
                else if (Tb5_Txt_SBCOD.ExCodeDB == "")
                {
                    Tb5_Txt_SBCOD.IsError = false;
                    Tb5_Txt_SBCOD_NM.Text = "";
                }
                Tb5_Chk_NAYOSE.Checked = (Global.NAYOSE == "0" ? false : true);
                Tb5_Chk_F_SETUIN.Checked = (Global.F_SETUIN == "0" ? false : true);
                Chk_STFLG.Checked = (Global.STFLG == "0" ? false : true);
                Txt_STYMD.Value = (Global.STYMD != "" ? int.Parse(Global.STYMD) : 0);
                Txt_EDYMD.Value = (Global.EDYMD != "" ? int.Parse(Global.EDYMD) : 0);
                Txt_LUSR.Text = mcBsLogic.Get_UserName(Global.LUSR);
                Txt_LMOD.Text = (Global.LMOD.Length == 8 ? Global.LMOD.Insert(6, "/").Insert(4, "/") : "");
                Txt_ZSTYMD.Text = (Global.ZSTYMD.Length == 8 ? Global.ZSTYMD.Insert(6, "/").Insert(4, "/") : "");
                Txt_ZEDYMD.Text = (Global.ZEDYMD.Length == 8 ? Global.ZEDYMD.Insert(6, "/").Insert(4, "/") : "");

                //回収設定タブへのデータセット                                                                                  // 【回収設定タブ】
                Tb2_Txt_TOKUKANA.Text = Global.TOKUKANA;                                                                        // 照合用フリガナ
                if (Global.FUTAN != "")                                                                                         // 負担手数料区分コンボ
                {
                    Tb2_Cmb_FUTAN.Text = Global.FUTAN + ":" + mcBsLogic.Get_Tesuu_NM(Convert.ToInt32(Global.FUTAN));
                }
//-- <2016/02/15 クリア追加>
                else
                { Tb2_Cmb_FUTAN.SelectedIndex = -1; }
//-- <2016/02/15>

                Tb2_Chk_GAIKA.Checked = (Global.GAIKA == "1" ? true : false);
                Tb2_Cmb_TSUKA.Text = Global.TSUKA;
                Tb2_Txt_GAIKA_KEY_F.Text = Global.GAIKA_KEY_F;
                Tb2_Txt_GAIKA_KEY_B.Text = Global.GAIKA_KEY_B;

                // 約定を指定
                if (Tb2_Chk_GAIKA.Checked)
                {
                    Tb2_Chk_YAKUJO.Enabled = false;
                }
                else
                {
                    Tb2_Chk_YAKUJO.Checked = (Global.YAKUJYO == "1" ? true : false);
                    Tb2_Chk_YAKUJO.Enabled = true;
                }
                Tb2_Txt_SHIME.Text = Global.SHIME;                                                                              // 締日
                if (Tb2_Txt_SHIME.Text == "99")
                {
                    Tb2_Txt_SHIME.Text = "末";
                }
                if (Global.KAISYUHI != "")                                                                                      // 回収予定日　3桁
                {
                    Tb2_Txt_KAISYUHI_M.Text = Global.KAISYUHI.Substring(0, 1);
                    Tb2_Txt_KAISYUHI_D.Text = Global.KAISYUHI.Substring(1, 2);
                }
//-- <2016/02/17 ""の場合クリア>
                else
                {
                    Tb2_Txt_KAISYUHI_M.Text = "";
                    Tb2_Txt_KAISYUHI_D.Text = "";
                }
//-- <2016/012/17>
                if (Tb2_Txt_KAISYUHI_D.Text == "99")
                {
                    Tb2_Txt_KAISYUHI_D.Text = "末";
                }
//-- <2016/03/08 条件も追加する>
//                if (Global.KAISYUSIGHT != "")                                                                                   // 回収期日　3桁
                if (Global.KAISYUSIGHT != "" && (mcBsLogic.Get_NKUBN(Global.KAISYU.ToString(), 1) == "1" || mcBsLogic.Get_NKUBN(Global.MIMAN.ToString(), 1) == "1"))
//-- <2016/03/08>                                                                         
                {
//-- <2016/03/08 ENABLEDも行う>
                    Tb2_Lbl_SIGHT_Main.Enabled = true;
                    Tb2_Lbl_SIGHT_M.Enabled = true;
                    Tb2_Lbl_SIGHT_D.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = true;
//-- <2016/03/08>
                    Tb2_Txt_KAISYUSIGHT_M.Text = Global.KAISYUSIGHT.Substring(0, 1);
                    Tb2_Txt_KAISYUSIGHT_D.Text = Global.KAISYUSIGHT.Substring(1, 2);
                }
//-- <2016/02/15 ""の場合はクリア>
                else
                {
                    Tb2_Txt_KAISYUSIGHT_M.Text = "";
                    Tb2_Txt_KAISYUSIGHT_D.Text = "";
                }
//-- <2016/02/15>
                if (Tb2_Txt_KAISYUSIGHT_D.Text == "99")
                {
                    Tb2_Txt_KAISYUSIGHT_D.Text = "末";
                }
                if (Global.HOLIDAY != "")                                                                                       // 休業日設定
                {
                    Tb2_Cmb_HOLIDAY.Text = Global.HOLIDAY + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.HOLIDAY));
                }
//-- <2016/02/17 ""の場合も追加>
                else
                { Tb2_Cmb_HOLIDAY.SelectedIndex = -1; }
//-- <2016/02/17>
//-- <2016/02/09 ""も定義しておく>
//                if (Global.YAKUJYO == "0")
                if (Global.YAKUJYO == "0" || Global.YAKUJYO == "")
//-- <2016/02/09>
                {
                    if (Global.KAISYU != "")
                    {
                        Tb2_Cmb_KAISYU.SelectedValue = Global.KAISYU;                                                           // 回収方法コンボ
                    }
//-- <2016/02/17 ""の場合を追加>
                    else
                    {
                        Tb2_Cmb_KAISYU.SelectedIndex = -1;
                    }
//-- <2016/02/17>
                }
                else
                {
                    if (Global.Y_KINGAKU != "")                                                                                 // 約定金額
                    {
                        Tb2_Txt_Y_KINGAKU.ExNumValue = Convert.ToDecimal(Global.Y_KINGAKU);
                    }
//-- <2016/02/17 ""の場合を追加>
                    else
                    { Tb2_Txt_Y_KINGAKU.Text = ""; }
//-- <2016/02/17>
                    if (Global.MIMAN != "")                                                                                     // 約定金額未満コンボ
                    {
                        Tb2_Cmb_MIMAN.SelectedValue = Global.MIMAN;
                    }
//-- <2016/02/17 ""の場合を追加>
                    else
                    { Tb2_Cmb_MIMAN.SelectedIndex = -1; }
//-- <2016/02/17>
//-- <2016/02/17 判断追加>
//                    Tb2_Cmb_IJOU_1.Text = Global.IJOU_1 + ":" + mcBsLogic.Get_NKUBN(Global.IJOU_1);                             // 約定金額以上①コンボ
                    if (Global.IJOU_1 != "")
                    {
                        Tb2_Cmb_IJOU_1.SelectedValue = Global.IJOU_1;
                    }
                    else
                    { Tb2_Cmb_IJOU_1.SelectedIndex = -1; }
//-- <2016/02/17>
                    if (Global.BUNKATSU_1 != "")                                                                                // 分割①
                    {
                        Tb2_Txt_BUNKATSU_1.ExNumValue = Convert.ToDecimal(Global.BUNKATSU_1);
                    }
//-- <2016/02/17 ""の場合を追加>
                    if (Global.HASU_1 != "")                                                                                    // 端数単位①
                    {
                        Tb2_Cmb_HASU_1.Text = Global.HASU_1 + ":" + mcBsLogic.Get_HasuUnit_NM_Saiken(Convert.ToInt32(Global.HASU_1));
                    }
                    else
                    { Tb2_Cmb_HASU_1.SelectedIndex = -1; }
//-- <2016/02/17>
//-- <2016/02/15 ""も対応、99を末へ>
//                    if (Global.SIGHT_1 != "")                                                                                 // 回収期日①　3桁
//                    {
//                        Tb2_Txt_SIGHT_M_1.Text = Global.SIGHT_1.Substring(0, 1);
//                        Tb2_Txt_SIGHT_D_1.Text = Global.SIGHT_1.Substring(1, 2);
//                    }
                    if (Global.SIGHT_1 != "" && mcBsLogic.Get_NKUBN(Global.IJOU_1, 1) == "1")                                   // 回収期日①　3桁
                    {
                        Tb2_Txt_SIGHT_M_1.Text = Global.SIGHT_1.Substring(0, 1);
                        Tb2_Txt_SIGHT_D_1.Text = Global.SIGHT_1.Substring(1, 2);
                    }
                    else
                    {
                        Tb2_Txt_SIGHT_M_1.Text = "";
                        Tb2_Txt_SIGHT_D_1.Text = "";
                    }
                    if (Tb2_Txt_SIGHT_D_1.Text == "99")
                    {
                        Tb2_Txt_SIGHT_D_1.Text = "末";
                    }
//-- <2016/02/15>
                    Tb2_Cmb_IJOU_1.Enabled = true;
                    Tb2_Txt_BUNKATSU_1.Enabled = true;
                    Tb2_Cmb_HASU_1.Enabled = true;
//-- <2016/02/17 入金区分が期日ありの場合を追加>
//                    Tb2_Txt_SIGHT_M_1.Enabled = true;
//                    Tb2_Txt_SIGHT_D_1.Enabled = true;
//                    Tb2_Lbl_IJOU_1.Enabled = true;
//                    Tb2_Lbl_BUNKATSU_1.Enabled = true;
//                    Tb2_Lbl_SIGHT_M_1.Enabled = true;
//                    Tb2_Lbl_SIGHT_D_1.Enabled = true;

                    Tb2_Lbl_IJOU_1.Enabled = true;
                    Tb2_Lbl_BUNKATSU_1.Enabled = true;
                    if (mcBsLogic.Get_NKUBN(Global.IJOU_1, 1) == "1")
                    {
                        Tb2_Txt_SIGHT_M_1.Enabled = true;
                        Tb2_Txt_SIGHT_D_1.Enabled = true;
                        Tb2_Lbl_SIGHT_M_1.Enabled = true;
                        Tb2_Lbl_SIGHT_D_1.Enabled = true;
                    }
                    else
                    {
                        Tb2_Txt_SIGHT_M_1.Enabled = false;
                        Tb2_Txt_SIGHT_D_1.Enabled = false;
                        Tb2_Lbl_SIGHT_M_1.Enabled = false;
                        Tb2_Lbl_SIGHT_D_1.Enabled = false;
                    }
//-- <2016/02/17>
                    if (Global.IJOU_2 != "" && Global.IJOU_2 != "0")                                                                                    // 約定金額以上②の判断
                    {
//-- <2016/02/17 見直し>
                        //Tb2_Cmb_IJOU_2.Text = Global.IJOU_2 + ":" + mcBsLogic.Get_NKUBN(Global.IJOU_2);                         // 約定金額以上②コンボ
                        //Tb2_Txt_BUNKATSU_2.ExNumValue = Convert.ToDecimal(Global.BUNKATSU_2);                                   // 分割②
                        //Tb2_Cmb_HASU_2.Text = Global.HASU_2 + ":" + mcBsLogic.Get_HasuUnit_NM_Saiken(Convert.ToInt32(Global.HASU_2));   // 端数処理②コンボ
                        //Tb2_Txt_SIGHT_M_2.Text = Global.SIGHT_2.Substring(0, 1);                                                // 回収期日②月
                        //Tb2_Txt_SIGHT_D_2.Text = Global.SIGHT_2.Substring(1, 2);                                                // 回収期日②日

                        //Tb2_Cmb_IJOU_2.Enabled = true;
                        //Tb2_Txt_BUNKATSU_2.Enabled = true;
                        //Tb2_Cmb_HASU_2.Enabled = true;
                        //Tb2_Txt_SIGHT_M_2.Enabled = true;
                        //Tb2_Txt_SIGHT_D_2.Enabled = true;

                        //Tb2_Lbl_IJOU_2.Enabled = true;
                        //Tb2_Lbl_BUNKATSU_2.Enabled = true;
                        //Tb2_Lbl_SIGHT_M_2.Enabled = true;
                        //Tb2_Lbl_SIGHT_D_2.Enabled = true;

                        Tb2_Cmb_IJOU_2.SelectedValue = Global.IJOU_2;                                                           // 約定金額以上②
                        Tb2_Txt_BUNKATSU_2.ExNumValue = Convert.ToDecimal(Global.BUNKATSU_2);                                   // 分割②

                        if (Global.HASU_2 != "")
                        {
                            Tb2_Cmb_HASU_2.Text = Global.HASU_2 + ":" + mcBsLogic.Get_HasuUnit_NM_Saiken(Convert.ToInt32(Global.HASU_2));   // 端数処理②コンボ
                        }
                        else
                        { Tb2_Cmb_HASU_2.SelectedIndex = -1; }

                        if (Global.SIGHT_2 != "" && mcBsLogic.Get_NKUBN(Global.IJOU_2, 1) == "1")
                        {
                            Tb2_Txt_SIGHT_M_2.Text = Global.SIGHT_2.Substring(0, 1);                                                // 回収期日②月
                            Tb2_Txt_SIGHT_D_2.Text = Global.SIGHT_2.Substring(1, 2);                                                // 回収期日②日
                        }
                        else
                        {
                            Tb2_Txt_SIGHT_M_2.Text = "";                                                                            // 回収期日②月
                            Tb2_Txt_SIGHT_D_2.Text = "";                                                                            // 回収期日②日
                        }
                        if (Tb2_Txt_SIGHT_D_2.Text == "99")
                        {
                            Tb2_Txt_SIGHT_D_2.Text = "末";
                        }

                        Tb2_Cmb_IJOU_2.Enabled = true;
                        Tb2_Txt_BUNKATSU_2.Enabled = true;
                        Tb2_Cmb_HASU_2.Enabled = true;
                        Tb2_Lbl_IJOU_2.Enabled = true;
                        Tb2_Lbl_BUNKATSU_2.Enabled = true;

                        if (mcBsLogic.Get_NKUBN(Global.IJOU_2, 1) == "1")
                        {
                            Tb2_Txt_SIGHT_M_2.Enabled = true;
                            Tb2_Txt_SIGHT_D_2.Enabled = true;
                            Tb2_Lbl_SIGHT_M_2.Enabled = true;
                            Tb2_Lbl_SIGHT_D_2.Enabled = true;
                        }
                        else
                        {
                            Tb2_Txt_SIGHT_M_2.Enabled = false;
                            Tb2_Txt_SIGHT_D_2.Enabled = false;
                            Tb2_Lbl_SIGHT_M_2.Enabled = false;
                            Tb2_Lbl_SIGHT_D_2.Enabled = false;
                        }
                    }
                    else if (Global.IJOU_1 != "" && Global.IJOU_2 == "0")
                    {
                        Tb2_Cmb_IJOU_2.SelectedValue = "";                                                                      // 約定金額以上②コンボ
                        Tb2_Txt_BUNKATSU_2.ExNumValue = 0;                                                                      // 分割②
                        Tb2_Cmb_HASU_2.SelectedIndex = -1;                                                                      // 端数処理②コンボ
                        Tb2_Txt_SIGHT_M_2.Text = "";                                                                            // 回収期日②月
                        Tb2_Txt_SIGHT_D_2.Text = "";                                                                            // 回収期日②日

                        Tb2_Cmb_IJOU_2.Enabled = true;
                        Tb2_Lbl_IJOU_2.Enabled = true;

                        Tb2_Lbl_BUNKATSU_2.Enabled = false;
                        Tb2_Txt_BUNKATSU_2.Enabled = false;
                        Tb2_Cmb_HASU_2.Enabled = false;
                        Tb2_Txt_SIGHT_M_2.Enabled = false;
                        Tb2_Txt_SIGHT_D_2.Enabled = false;
                        Tb2_Lbl_SIGHT_M_2.Enabled = false;
                        Tb2_Lbl_SIGHT_D_2.Enabled = false;
                    }

                    if (Global.IJOU_3 != "" && Global.IJOU_3 != "0")
                    {
//-- <2016/02/16 見直し>
//                        Tb2_Cmb_IJOU_3.Text = Global.IJOU_3 + ":" + mcBsLogic.Get_NKUBN(Global.IJOU_3);
//                        Tb2_Txt_BUNKATSU_3.ExNumValue = Convert.ToDecimal(Global.BUNKATSU_3);
//                        Tb2_Cmb_HASU_3.Text = Global.HASU_3 + ":" + mcBsLogic.Get_HasuUnit_NM_Saiken(Convert.ToInt32(Global.HASU_3));
//                        Tb2_Txt_SIGHT_M_3.Text = Global.SIGHT_3.Substring(0, 1);
//                        Tb2_Txt_SIGHT_D_3.Text = Global.SIGHT_3.Substring(1, 2);

//                        Tb2_Cmb_IJOU_3.Enabled = true;
//                        Tb2_Txt_BUNKATSU_3.Enabled = true;
//                        Tb2_Cmb_HASU_3.Enabled = true;
//                        Tb2_Txt_SIGHT_M_3.Enabled = true;
//                        Tb2_Txt_SIGHT_D_3.Enabled = true;

//                        Tb2_Lbl_IJOU_3.Enabled = true;
//                        Tb2_Lbl_BUNKATSU_3.Enabled = true;
//                        Tb2_Lbl_SIGHT_M_3.Enabled = true;
//                        Tb2_Lbl_SIGHT_D_3.Enabled = true;
//                    }

                        Tb2_Cmb_IJOU_3.SelectedValue = Global.IJOU_3;
                        Tb2_Txt_BUNKATSU_3.ExNumValue = Convert.ToDecimal(Global.BUNKATSU_3);

                        if (Global.HASU_3 != "")
                        {
                            Tb2_Cmb_HASU_3.Text = Global.HASU_3 + ":" + mcBsLogic.Get_HasuUnit_NM_Saiken(Convert.ToInt32(Global.HASU_3));
                        }
                        else
                        { Tb2_Cmb_HASU_3.SelectedIndex = -1; }

                        if (Global.SIGHT_3 != "" && mcBsLogic.Get_NKUBN(Global.IJOU_3, 1) == "1")
                        {
                            Tb2_Txt_SIGHT_M_3.Text = Global.SIGHT_3.Substring(0, 1);
                            Tb2_Txt_SIGHT_D_3.Text = Global.SIGHT_3.Substring(1, 2);
                        }
                        else
                        {
                            Tb2_Txt_SIGHT_M_3.Text = "";
                            Tb2_Txt_SIGHT_D_3.Text = "";
                        }
                        if (Tb2_Txt_SIGHT_D_3.Text == "99")
                        {
                            Tb2_Txt_SIGHT_D_3.Text = "末";
                        }

                        Tb2_Cmb_IJOU_3.Enabled = true;
                        Tb2_Txt_BUNKATSU_3.Enabled = true;
                        Tb2_Cmb_HASU_3.Enabled = true;
                        Tb2_Lbl_IJOU_3.Enabled = true;
                        Tb2_Lbl_BUNKATSU_3.Enabled = true;

                        if (mcBsLogic.Get_NKUBN(Global.IJOU_3, 1) == "1")
                        {
                            Tb2_Txt_SIGHT_M_3.Enabled = true;
                            Tb2_Txt_SIGHT_D_3.Enabled = true;
                            Tb2_Lbl_SIGHT_M_3.Enabled = true;
                            Tb2_Lbl_SIGHT_D_3.Enabled = true;
                        }
                        else
                        {
                            Tb2_Txt_SIGHT_M_3.Enabled = false;
                            Tb2_Txt_SIGHT_D_3.Enabled = false;
                            Tb2_Lbl_SIGHT_M_3.Enabled = false;
                            Tb2_Lbl_SIGHT_D_3.Enabled = false;
                        }
                    }
                    else if (Global.IJOU_1 != "" && Global.IJOU_2 != "" && Global.IJOU_3 == "0")
                    {
                        Tb2_Cmb_IJOU_3.SelectedValue = "";  
                        Tb2_Txt_BUNKATSU_3.ExNumValue = 0;                                                                      // 分割②
                        Tb2_Cmb_HASU_3.SelectedIndex = -1;                                                                      // 端数処理②コンボ
                        Tb2_Txt_SIGHT_M_3.Text = "";                                                                            // 回収期日②月
                        Tb2_Txt_SIGHT_D_3.Text = "";                                                                            // 回収期日②日

                        Tb2_Cmb_IJOU_3.Enabled = true;
                        Tb2_Lbl_IJOU_3.Enabled = true;

                        Tb2_Lbl_BUNKATSU_3.Enabled = false;
                        Tb2_Txt_BUNKATSU_3.Enabled = false;
                        Tb2_Cmb_HASU_3.Enabled = false;
                        Tb2_Txt_SIGHT_M_3.Enabled = false;
                        Tb2_Txt_SIGHT_D_3.Enabled = false;
                        Tb2_Lbl_SIGHT_M_3.Enabled = false;
                        Tb2_Lbl_SIGHT_D_3.Enabled = false;
                    }
                    else
                    {
                        Tb2_Cmb_IJOU_3.SelectedIndex = -1;                                                                      // 約定金額以上②コンボ
                        Tb2_Txt_BUNKATSU_3.ExNumValue = 0;                                                                      // 分割②
                        Tb2_Cmb_HASU_3.SelectedIndex = -1;                                                                      // 端数処理②コンボ
                        Tb2_Txt_SIGHT_M_3.Text = "";                                                                            // 回収期日②月
                        Tb2_Txt_SIGHT_D_3.Text = "";                                                                            // 回収期日②日

                        Tb2_Cmb_IJOU_3.Enabled = false;
                        Tb2_Lbl_IJOU_3.Enabled = false;

                        Tb2_Lbl_BUNKATSU_3.Enabled = false;
                        Tb2_Txt_BUNKATSU_3.Enabled = false;
                        Tb2_Cmb_HASU_3.Enabled = false;
                        Tb2_Txt_SIGHT_M_3.Enabled = false;
                        Tb2_Txt_SIGHT_D_3.Enabled = false;
                        Tb2_Lbl_SIGHT_M_3.Enabled = false;
                        Tb2_Lbl_SIGHT_D_3.Enabled = false;
                    }
//-- <2016/02/17>
                }

                Tb2_Txt_SEN_GINKOCD.ExCodeDB = Global.SEN_GINKOCD;
                Tb2_Txt_SEN_GINKONM.Text = mcBsLogic.Get_BANKNM(Global.SEN_GINKOCD);
                Tb2_Txt_SEN_SITENCD.ExCodeDB = Global.SEN_SITENCD;
//-- <2016/03/11 支店名称表示追加>
                Tb2_Txt_SEN_SITENNM.Text = mcBsLogic.Get_SITENNM(Global.SEN_GINKOCD, Global.SEN_SITENCD);
//-- <2016/03/11>
                if (Global.SEN_KOZANO != "")
                {
                    Tb2_Txt_SEN_KSITENCD.ClearValue();
                    if (Global.SEN_KOZANO.Length == 3 || Global.SEN_KOZANO.Length == 10)
                    {
                        Tb2_Txt_SEN_KSITENCD.ExCodeDB = Global.SEN_KOZANO.Substring(0, 3);
                    }
                }
                else
                {
                    Tb2_Txt_SEN_KSITENCD.ClearValue();
                    Tb2_Txt_SEN_KOZANO.ClearValue();
                }
                Tb2_Txt_SEN_KSITENNM.Text = Global.KASO_SITENNM;
                if (Global.YOKINSYU != "")
                {
                    Tb2_Cmb_YOKINSYU.Text = Global.YOKINSYU + ":" + mcBsLogic.Get_Sen_YokinType_NM(Convert.ToInt32(Global.YOKINSYU));
                }
//-- <2016/02/15 クリア追加>
                else
                {
                    Tb2_Cmb_YOKINSYU.SelectedIndex = -1;
                }
//-- <2016/02/15>
                if (Global.SEN_KOZANO.Length == 10)
                {
                    Tb2_Txt_SEN_KOZANO.ExCodeDB = Global.SEN_KOZANO.Substring(3, 7);
                }
                else if (Global.SEN_KOZANO.Length == 7)
                {
                    Tb2_Txt_SEN_KOZANO.ExCodeDB = Global.SEN_KOZANO;
                }
                else
                {
                    Tb2_Txt_SEN_KOZANO.ClearValue();
                }

//-- <9999>
//                Tb2_Txt_SEN_SITENCD.Enabled = true;
//                Tb2_Txt_SEN_KSITENCD.Enabled = true;
//                Tb2_Txt_SEN_KSITENNM.Enabled = true;
//                Tb2_Lbl_SEN_SITENCD.Enabled = true;
//                Tb2_Lbl_SEN_KSITENCD.Enabled = true;
                if (Tb2_Txt_SEN_GINKOCD.Text != "")
                {
                    Tb2_Txt_SEN_SITENCD.Enabled = true;
                    Tb2_Txt_SEN_KSITENCD.Enabled = true;
                    Tb2_Txt_SEN_KSITENNM.Enabled = true;
                    Tb2_Cmb_YOKINSYU.Enabled = true;
                    Tb2_Txt_SEN_KOZANO.Enabled = true;
                    Tb2_Lbl_SEN_SITENCD.Enabled = true;
                    Tb2_Lbl_SEN_KSITENCD.Enabled = true;
                    Tb2_Lbl_SEN_YOKINSYU.Enabled = true;
                    Tb2_Lbl_SEN_KOZANO.Enabled = true;
                }
                else
                {
                    Tb2_Txt_SEN_SITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENNM.Enabled = false;
                    Tb2_Cmb_YOKINSYU.Enabled = false;
                    Tb2_Txt_SEN_KOZANO.Enabled = false;
                    Tb2_Lbl_SEN_SITENCD.Enabled = false;
                    Tb2_Lbl_SEN_KSITENCD.Enabled = false;
                    Tb2_Lbl_SEN_YOKINSYU.Enabled = false;
                    Tb2_Lbl_SEN_KOZANO.Enabled = false;
                }
//-- <9999>

                Tb2_Chk_JIDOU_GAKUSYU.Checked = (Global.JIDOU_GAKUSYU == "1" ? true : false);
                Tb2_Chk_NYUKIN_YOTEI.Checked = (Global.NYUKIN_YOTEI == "1" ? true : false);
                Tb2_Chk_TESURYO_GAKUSYU.Checked = (Global.TESURYO_GAKUSYU == "1" ? true : false);
                Tb2_Chk_TESURYO_GOSA.Checked = (Global.TESURYO_GOSA == "1" ? true : false);
                Tb2_Chk_RYOSYUSYO.Checked = (Global.RYOSYUSYO == "1" ? true : false);
                Tb2_Txt_SHIN_KAISYACD.Text = Global.SHIN_KAISYACD;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //if (Global.YOSIN != "")
                if (Global.YOSIN != "" && Global.YOSIN != "0")
                // <---V01.15.01 HWY UPDATE ▲(6490)
                {
                    Tb2_Txt_YOSIN.ExNumValue = Convert.ToDecimal(Global.YOSIN);
                }
//-- <2016/02/17 ""を追加>
                else
                {
                    Tb2_Txt_YOSIN.ClearValue();
                }
//-- <2016/02/17>
                Tb2_Txt_YOSHINRANK.Text = Global.YOSHINRANK;

                if (Global.HIFURIKOZA_1 != "")
                {
//--
//                    Tb2_Txt_HIFURIKOZA_1.ExNumValue = Convert.ToDecimal(Global.HIFURIKOZA_1);
                    Tb2_Txt_HIFURIKOZA_1.ExNumValue = Convert.ToInt32(Global.HIFURIKOZA_1);
//--
                    Tb2_Chk_HiFuri_1.Checked = true;
//--
//                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToDecimal(Global.HIFURIKOZA_1));
                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Global.HIFURIKOZA_1));
//--

                    if (dt.Rows.Count > 0)
                    {
                        Tb2_Txt_HIBKCD_1.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                        Tb2_Txt_HIBKNM_1.Text = mcBsLogic.Get_BANKNM(dt.Rows[0]["OWNBKCOD"].ToString());
                        Tb2_Txt_HIBRCD_1.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                        Tb2_Txt_HIBRNM_1.Text = mcBsLogic.Get_SITENNM(dt.Rows[0]["OWNBKCOD"].ToString(), dt.Rows[0]["OWNBRCOD"].ToString());
                        Tb2_Txt_HIYOKN_1.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                        Tb2_Txt_HIKOZANO_1.Text = dt.Rows[0]["KOZANO"].ToString();
                    }
                }
//-- <2016/02/17 ""を追加>
                else
                {
                    Tb2_Txt_HIFURIKOZA_1.ExNumValue = 0;
                    Tb2_Chk_HiFuri_1.Checked = false;
                    Tb2_Txt_HIBKCD_1.Text = "";
                    Tb2_Txt_HIBKNM_1.Text = "";
                    Tb2_Txt_HIBRCD_1.Text = "";
                    Tb2_Txt_HIBRNM_1.Text = "";
                    Tb2_Txt_HIYOKN_1.Text = "";
                    Tb2_Txt_HIKOZANO_1.Text = "";
                }
//-- <2016/02/17>
                if (Global.HIFURIKOZA_2 != "")
                {
//--
//                    Tb2_Txt_HIFURIKOZA_2.ExNumValue = Convert.ToDecimal(Global.HIFURIKOZA_2);
                    Tb2_Txt_HIFURIKOZA_2.ExNumValue = Convert.ToInt32(Global.HIFURIKOZA_2);
//--
                    Tb2_Chk_HiFuri_2.Checked = true;
//--
//                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToDecimal(Global.HIFURIKOZA_2));
                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Global.HIFURIKOZA_2));
//--
                    if (dt.Rows.Count > 0)
                    {
                        Tb2_Txt_HIBKCD_2.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                        Tb2_Txt_HIBKNM_2.Text = mcBsLogic.Get_BANKNM(dt.Rows[0]["OWNBKCOD"].ToString());
                        Tb2_Txt_HIBRCD_2.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                        Tb2_Txt_HIBRNM_2.Text = mcBsLogic.Get_SITENNM(dt.Rows[0]["OWNBKCOD"].ToString(), dt.Rows[0]["OWNBRCOD"].ToString());
                        Tb2_Txt_HIYOKN_2.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                        Tb2_Txt_HIKOZANO_2.Text = dt.Rows[0]["KOZANO"].ToString();
                    }
                }
//-- <2016/02/17 ""を追加>
                else
                {
                    Tb2_Txt_HIFURIKOZA_2.ExNumValue = 0;
                    Tb2_Chk_HiFuri_2.Checked = false;
                    Tb2_Txt_HIBKCD_2.Text = "";
                    Tb2_Txt_HIBKNM_2.Text = "";
                    Tb2_Txt_HIBRCD_2.Text = "";
                    Tb2_Txt_HIBRNM_2.Text = "";
                    Tb2_Txt_HIYOKN_2.Text = "";
                    Tb2_Txt_HIKOZANO_2.Text = "";
                }
//-- <2016/02/17>
                if (Global.HIFURIKOZA_3 != "")
                {
//--
//                    Tb2_Txt_HIFURIKOZA_3.ExNumValue = Convert.ToDecimal(Global.HIFURIKOZA_3);
                    Tb2_Txt_HIFURIKOZA_3.ExNumValue = Convert.ToInt32(Global.HIFURIKOZA_3);
//--
                    Tb2_Chk_HiFuri_3.Checked = true;
//--
//                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToDecimal(Global.HIFURIKOZA_3));
                    DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Global.HIFURIKOZA_3));
//--
                    if (dt.Rows.Count > 0)
                    {
                        Tb2_Txt_HIBKCD_3.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                        Tb2_Txt_HIBKNM_3.Text = mcBsLogic.Get_BANKNM(dt.Rows[0]["OWNBKCOD"].ToString());
                        Tb2_Txt_HIBRCD_3.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                        Tb2_Txt_HIBRNM_3.Text = mcBsLogic.Get_SITENNM(dt.Rows[0]["OWNBKCOD"].ToString(), dt.Rows[0]["OWNBRCOD"].ToString());
                        Tb2_Txt_HIYOKN_3.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                        Tb2_Txt_HIKOZANO_3.Text = dt.Rows[0]["KOZANO"].ToString();
                    }
                }
//-- <2016/02/17 ""を追加>
                else
                {
                    Tb2_Txt_HIFURIKOZA_3.ExNumValue = 0;
                    Tb2_Chk_HiFuri_3.Checked = false;
                    Tb2_Txt_HIBKCD_3.Text = "";
                    Tb2_Txt_HIBKNM_3.Text = "";
                    Tb2_Txt_HIBRCD_3.Text = "";
                    Tb2_Txt_HIBRNM_3.Text = "";
                    Tb2_Txt_HIYOKN_3.Text = "";
                    Tb2_Txt_HIKOZANO_3.Text = "";
                }
//-- <2016/02/17>





                //控除情報タブへのデータセット
                if (Global.GENSEN == "0")
                {
                    Tb5_Chk_GENSEN.Checked = false;
                    Tb5_Chk_OUTPUT.Checked = false;
                }
                else if (Global.GENSEN == "1")
                {
                    Tb5_Chk_OUTPUT.Checked = true;
                    Tb5_Chk_GENSEN.Checked = true;
                    Tb5_Radio_GENSEN1.Checked = true;
                }
                else if (Global.GENSEN == "2")
                {
                    Tb5_Chk_OUTPUT.Checked = true;
                    Tb5_Chk_GENSEN.Checked = true;
                    Tb5_Radio_GENSEN2.Checked = true;
                }
                else if (Global.GENSEN == "3")
                {
                    Tb5_Chk_OUTPUT.Checked = true;
                    Tb5_Chk_GENSEN.Checked = false;
                }
                if (Global.GOU == "0")
                {
                    Tb5_Cmb_GOU.SelectedValue = 0;
                }
                else if (Global.GOU != "")
                {
                    Tb5_Cmb_GOU.Text = Global.GOU + ":" + mcBsLogic.Get_Gou_NM(int.Parse(Global.GOU));
                }
                else
                {
                    Tb5_Cmb_GOU.SelectedValue = 0;
                }
                if (Global.GGKBN != "")
                {
                    Tb5_Cmb_GGKBN.SelectedValue = (Global.GGKBN != "" ? int.Parse(Global.GGKBN) : -1);
                }
                if (Global.GSKUBN != "0" && Global.GSKUBN != "")
                {
                    Tb5_Cmb_GSKUBN.SelectedValue = int.Parse(Global.GSKUBN);
                }
                else
                {
                    Tb5_Cmb_GSKUBN.SelectedValue = -1;
                }
                //Tb4_Chk_HORYU.Checked = (Global.HORYU == "0" ? false : true);
                //if (!Tb4_Chk_HORYU.Checked)
                //{
                //    Tb4_Txt_HOVAL.ExNumValue = 100;
                //}
                //else
                //{
                //    Tb4_Txt_HOVAL.ExNumValue = Convert.ToDecimal(Global.HOVAL);
                //}
                //**Tb4_Txt_HOVAL.ExNumValue = Convert.ToDecimal(Global.HOVAL);
                nHOVAL_Before = Tb5_Txt_HOVAL.ExNumValue; //変更チェック用

                //その他情報タブへのデータセット
                Tb5_Cmb_F_SOUFU.Text = (Global.F_SOUFU != "" ? Global.F_SOUFU + ":" + mcBsLogic.Get_FSoufu_NM(Convert.ToInt32(Global.F_SOUFU)) : "0:送付しない");
                Tb5_Cmb_ANNAI.Text = (Global.ANNAI != "" ? Global.ANNAI + ":" + mcBsLogic.Get_Annai_NM(Convert.ToInt32(Global.ANNAI)) : "1:パターン１");
                Tb5_Cmb_TSOKBN.Text = (Global.TSOKBN != "" ? Global.TSOKBN + ":" + mcBsLogic.Get_Tsokbn_NM(Convert.ToInt32(Global.TSOKBN)) : "1:自社負担");
                // Ver.01.02.04 [SS_4816] Toda -->
                Tb5_Txt_TEGVAL.ExNumValue = (Global.TEGVAL != "" ? Convert.ToDecimal(Global.TEGVAL) : 0);
                // Ver.01.02.04 <--
                //Tb5_Cmb_SZEI.Text = (Global.SZEI != "" ? Global.SZEI + ":" + mcBsLogic.Get_HasuShori_NM(Convert.ToInt32(Global.SZEI)) : "0:切り捨て");
                //**>>ICS-S 2013/03/04
                //**Tb5_Cmb_SHITU.Text = (Global.F_SHITU != "" ? Global.F_SHITU + ":" + mcBsLogic.Get_HaraiTuuti_NM(Convert.ToInt32(Global.F_SHITU)) : "0:印刷しない");
                Tb5_Cmb_SHITU.Text = (Global.F_SHITU != "" ? Global.F_SHITU + ":" + mcBsLogic.Get_HaraiTuuti_NM(Convert.ToInt32(Global.F_SHITU)) : "1:印刷する");
                //**<<ICS-E
                Tb5_Txt_DM1.Text = Global.DM1;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //Tb5_Txt_DM2.ExNumValue = (Global.DM2 != "" ? Convert.ToInt32(Global.DM2) : 0);
                //Tb5_Txt_DM3.ExNumValue = (Global.DM3 != "" ? Convert.ToInt32(Global.DM3) : 0);
                if (Global.DM2 != "" && Global.DM2 != "0" )
                {
                    Tb5_Txt_DM2.ExNumValue = Convert.ToInt32(Global.DM2);
                }
                else
                {
                    Tb5_Txt_DM2.ClearValue();
                }
                if (Global.DM3 != "" && Global.DM3 != "0" )
                {
                    Tb5_Txt_DM3.ExNumValue = Convert.ToInt32(Global.DM3);
                }
                else
                {
                    Tb5_Txt_DM3.ClearValue();
                }

                // <---V01.15.01 HWY UPDATE ▲(6490)

                Tb1_Txt_UsrNo.Text = Global.CDM1;
                //**>>ICS-S 2013/05/20
                Tb5_Txt_FAC.Text = Global.CDM2;
                //Tb5_Txt_RefNo.Text = Global.CD03;
                Tb1_Chk_Jyoto.Checked = (Global.IDM1 == "0" ? false : true); 
                //**<<ICS-E

                Txt_GRPID.ClearValue();
                Txt_GRPNM.ClearValue();

                //if (Global.nGroup == 0)
                //{
                //    Txt_GRPID.Enabled = false;
                //}
                //else
                //{
                //    Txt_GRPID.Enabled = true;
                //}

                Txt_SaikenDaihyoCd.ClearValue();
                Txt_SaikenDaihyoHj.ClearValue();

                if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
                {
                    string sDaiCd = "";
                    string sDHjCd = "";
                    // 子供だったら親コードを取得
                    if (mcBsLogic.Get_SaikenDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd))
                    {
                        Txt_SaikenDaihyoCd.Text = sDaiCd;
                        if (Global.nTRCD_HJ == 1)
                        {
                            Txt_SaikenDaihyoHj.Text = sDHjCd.PadLeft(6, '0');
                        }
                        Chk_SAIKEN_FLG.Enabled = false;
                        //-- <2016/03/15 債権のフラグも変更不可>
                        Cbo_SAIKEN.Enabled = false;
                        //-- <2016/03/15>
                    }
                    //-- <2016/03/15>
                    else
                    {
                        if (mcBsLogic.Get_MySaikenDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0"))
                        {
                            Chk_SAIKEN_FLG.Enabled = false;
                            Cbo_SAIKEN.Enabled = false;
                        }
                        else
                        {
                            if (Global.cUsrSec.nMFLG < 2)
                            {
                                Chk_SAIKEN_FLG.Enabled = false;
                                Cbo_SAIKEN.Enabled = false;
                            }
                            else
                            {
                                Chk_SAIKEN_FLG.Enabled = true;
                                Cbo_SAIKEN.Enabled = true;
                            }
                        }
                    }
                    //-- <2016/03/21>
                    if (Global.cUsrSec.nMFLG < 2)
                    {
                        Tb2_Txt_SEN_GINKOCD.ReadOnly = true;
                    }
                    else { Tb2_Txt_SEN_GINKOCD.ReadOnly = false; }
                    //-- <2016/03/21>
                    //-- <2016/03/15>
                }
                else
                {
                    Chk_SAIKEN_FLG.Enabled = false;
                }
                Chk_SAIKEN_FLG.Checked = (Global.SAIKEN_FLG == "0" ? false : true);
                //if (!(Global.cUsrSec.nMFLG < 2))
                //{
                //    if (Chk_SAIKEN_FLG.Checked == true)
                //    {
                //        Chk_SAIKEN_FLG.Enabled = !mcBsLogic.Chk_SaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                //    }
                //}

                Txt_SaimuDaihyoCd.ClearValue();
                Txt_SaimuDaihyoHj.ClearValue();

                if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    string sDaiCd = "";
                    string sDHjCd = "";
                    if (mcBsLogic.Get_SaimuDaihyo(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd))
                    {
                        Txt_SaimuDaihyoCd.Text = sDaiCd;
                        if (Global.nTRCD_HJ == 1)
                        {
                            Txt_SaimuDaihyoHj.Text = sDHjCd.PadLeft(6, '0');
                        }
                    }
                }
                Set_Enabled_Cbo_SAIMU(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                Set_Enabled_Chk_SAIMU_FLG(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");

                Chk_SAIMU_FLG.Checked = (Global.SAIMU_FLG == "0" ? false : true);

                Txt_TRFURI.Text = Global.TRFURI;
                Tb1_Txt_TRMAIL.Text = Global.TRMAIL;
                Tb1_Txt_TRURL.Text = Global.TRURL;
                Tb1_Txt_BIKO.Text = Global.BIKO;
                Tb1_Txt_E_TANTOCD.ExCodeDB = Global.E_TANTOCD;
                Tb1_Txt_E_TANTONM.Text = Global.E_TANTONM;
                Tb1_Txt_MYNO_AITE.Text = Global.MYNO_AITE;
//-- <2016/02/14 相殺使用しない場合を追加>
//                Tb1_Chk_SOSAI.Checked = (Global.SOSAI == "0" ? false : true);
                if (Global.nSOSAI_F == 0)
                { Tb1_Chk_SOSAI.Checked = false; }
                else
                { Tb1_Chk_SOSAI.Checked = (Global.SOSAI == "0" ? false : true); }
//-- <2016/02/14>
                Tb1_Chk_SRYOU_F.Checked = (Global.SRYOU_F == "1" ? true : false);
                if (Global.GRPID != "0" && Global.GRPID != "")
                {
                    Txt_GRPID.ExNumValue = Convert.ToDecimal(Global.GRPID);
                    Txt_GRPNM.Text = Global.GRPIDNM;
                }

                Tb3_Txt_SHIHARAIMM.Text = Global.SHIHARAIMM_tb1;
                Tb3_Txt_SIHARAIDD.Text = Global.SIHARAIDD_tb1;
                Tb3_Txt_SKIJITUMM.Text = Global.SKIJITUMM_tb1;
                Tb3_Txt_SKIJITUDD.Text = Global.SKIJITUDD_tb1;

                // --->V01.15.01 HWY UPDATE ▼(6490)
                //if (Global.TEGVAL != "")
               if (Global.TEGVAL != "" && Global.TEGVAL != "0")
                {
                    Tb5_Txt_TEGVAL.ExNumValue = Convert.ToDecimal(Global.TEGVAL);
                }
               // <---V01.15.01 HWY UPDATE ▲(6490)

                if(Global.GSSKBN == "1")
                {
                    Tb5_Rdo_GSSKBN1.Checked = true;
                }
                else
                {
                    Tb5_Rdo_GSSKBN2.Checked = true;
                }
                
                if (Global.HORYU == "0")
                {
                    Tb5_Rdo_HORYU0.Checked = true;
                }
                else if(Global.HORYU == "1")
                {
                    Tb5_Rdo_HORYU1.Checked = true;
                }
                else if (Global.HORYU == "2")
                {
                    Tb5_Rdo_HORYU2.Checked = true;
                }
                if (Global.HR_KIJYUN != "")
                {
                    Tb5_Txt_HR_KIJYUN.ExNumValue = Convert.ToDecimal(Global.HR_KIJYUN);
                }
                if (Global.HORYU != "0")
                {
//-- <2016/04/02>
//                    Tb5_Cmb_HORYU_F.Text = (Global.HORYU_F == "0" ? "0:比率" : "1:定額");
                    Tb5_Cmb_HORYU_F.Text = (Global.HORYU_F == "1" ? "1:比率" : "2:定額");
//-- <2016/04/02>
                    if (Global.HRORYUGAKU != "")
                    {
                        Tb5_Txt_HRORYUGAKU.ExNumValue = Convert.ToDecimal(Global.HRORYUGAKU);
                    }
                    Tb5_Txt_HOVAL.ExNumValue = Convert.ToDecimal(Global.HOVAL);
                    Tb5_Cmb_HRKBN.SelectedValue = (Global.HRKBN != "" ? int.Parse(Global.HRKBN) : -1);
                    if (Global.HRKBN != "0" && Global.HRKBN != "")
                    {
                        Tb5_Cmb_HRKBN.SelectedValue = int.Parse(Global.HRKBN);
                    }
                    else
                    {
                        Tb5_Cmb_HRKBN.SelectedValue = -1;
                    }

                }

//-- <2016/03/23>
                if (bHORYUNull && bKOUJYONull)
                {
                    Tb4_Grp_GENSEN.Enabled = false;
                    Tb5_Rdo_HORYU0.Enabled = false;
                    Tb5_Rdo_HORYU1.Enabled = false;
                    Tb5_Rdo_HORYU2.Enabled = false;
                }
                else if (bHORYUNull) { Tb5_Rdo_HORYU1.Enabled = false; }
                else if (bKOUJYONull) { Tb5_Rdo_HORYU2.Enabled = false; }


//-- <2016/02/15 挙動見直し>
                bEventCancel = true;
//-- <2016/02/15>
                
                Tb3_Rdo_GAI_F0.Checked = Global.GAI_F == "0" ? true : false;
                Tb3_Rdo_GAI_F1.Checked = Global.GAI_F == "1" ? true : false;

//-- <2016/02/15 挙動見直し>
                bEventCancel = false;
//-- <2016/02/15>

                if (Global.GAI_F == "0")
                {
//-- <2016/03/10 表示のみに修正>
//                    Tb3_Chk_SHO_ID.Enabled = true;
                    Tb3_Chk_SHO_ID.Enabled = false;
//-- <2016/03/10>
                    Tb3_Txt_BNAM.ClearValue();
                    Tb3_Txt_KINM.ClearValue();
                }
                else
                {
                    Tb3_Chk_SHO_ID.Enabled = false;
                    Tb3_Txt_BNAM.ClearValue();
                    Tb3_Txt_KINM.ClearValue();
                }

                // 外貨設定タブ
                Tb6_Cmb_HEI_CD.SelectedValue = Global.HEI_CD;
                if (Global.GAI_SF == "0")
                {
                    Tb6_Rdo_GAI_SF0.Checked = true;
                }
                else
                {
                    Tb6_Rdo_GAI_SF1.Checked = true;
                }

                if (Global.GAI_SH == "0")
                {
                    Tb6_Rdo_GAI_SH0.Checked = true;
                }
                else
                {
                    Tb6_Rdo_GAI_SH1.Checked = true;
                }

                if (Global.GAI_KZID != "")
                {
                    Tb6_Cmb_GAI_KZID.SelectedValue = Convert.ToInt32(Global.GAI_KZID);
                }
//-- <2016/03/09 非選択項目>
//                if (Global.GAI_TF != "")
//                {
//                    Tb6_Cmb_GAI_TF.Text = Global.GAI_TF + ":" + mcBsLogic.Get_Tesuu_NM(Convert.ToInt32(Global.GAI_TF));
//                }
                Tb6_Cmb_GAI_TF.Text = "1:送金受取人";
//-- <2016/03/09>
                Tb6_Txt_ENG_NAME.Text = Global.ENG_NAME;
                Tb6_Txt_ENG_ADDR.Text = Global.ENG_ADDR;
                Tb6_Txt_ENG_KZNO.Text = Global.ENG_KZNO;
                Tb6_Txt_ENG_SWIF.Text = Global.ENG_SWIF;
                Tb6_Txt_ENG_BNKNAM.Text = Global.ENG_BNKNAM;
                Tb6_Txt_ENG_BRNNAM.Text = Global.ENG_BRNNAM;
                Tb6_Txt_ENG_BNKADDR.Text = Global.ENG_BNKADDR;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSetDispVal_S　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }


        /// <summary>
        /// 支払条件タブ.取引先支払方法テーブルからの画面設定
        /// </summary>
        private void Set_Tb1_SS_TSHOH(int iCurrentCnt, int iCnt)
        {
            try
            {
                //■支払条件タブ
                //チェックboxOn/Off判定
                if (iCnt < 2)
                {
                    Tb3_Chk_SHO_ID.Checked = false;
                }
                else
                {
                    Tb3_Chk_SHO_ID.Checked = true;
                }
                //次データ判定
                if (Global.SHO_ID_tb1 == null)
                {
                    Global.SHO_ID_tb1 = "0";
                }
                //新規 or 変更
                if (iCnt != 0)
                {
                    Tb3_Lbl_Old_New2.Text = "【　変更　】";
                    Tb1_Lbl_SHO_ID_V.Text = iCurrentCnt.ToString(); // <--- V02.37.01 YMP UPDATE ◀(122172)Global.SHO_ID_tb1をiCurrentCnt.ToString()に変更
                    BindNavi2_Selected.Text = iCurrentCnt.ToString();
                    BindNavi2_Cnt.Text = "/ " + iCnt.ToString();
                    if (Global.KICD_tb1 == "0")
                    {
                        Tb3_Txt_KCOD.ExCodeDB = "";
                    }
                    else if (Global.nKCOD_Type == 0 && Global.KICD_tb1 != "")
                    {
                        string sKCOD = mcBsLogic.Conv_KICDtoKCOD(Global.KICD_tb1);
                        Tb3_Txt_KCOD.ExCodeDB = sKCOD.PadLeft(Global.nKCOD_Len, '0');
                    }
                    else if (Global.nKCOD_Type == 1 && Global.KICD_tb1 != "")
                    {
                        Global.KCOD_tb1 = mcBsLogic.Conv_KICDtoKCOD(Global.KICD_tb1);
                        Tb3_Txt_KCOD.ExCodeDB = Global.KCOD_tb1.TrimEnd(' ');
                    }
                    else
                    {
                        Tb3_Txt_KCOD.ExCodeDB = "";
                    }
                    if (Global.KICD_tb1 == "0" || Global.KICD_tb1 == "")
                    {
                        Tb3_Txt_KINM.Text = "全て";
                    }
                    else if (Global.KICD_tb1 != "")
                    {
                        Tb1_Txt_KCOD_Validating(Tb3_Txt_KCOD, null);
                    }
                    
                    if (Global.SHINO_tb1 != "")
                    {
                        Tb3_Txt_SHINO.Text = Global.SHINO_tb1.PadLeft(3, '0');
                    }
                    else
                    {
                        Tb3_Txt_SHINO.Text = "";
                    }

                    if (Global.BCOD_tb1 == "0")
                    {
                        Tb3_Txt_BCOD.ExCodeDB = "";
                    }
                    else if (Global.nBCOD_Type == 0 && Global.BCOD_tb1 != "")
                    {
                        Tb3_Txt_BCOD.ExCodeDB = Global.BCOD_tb1.PadLeft(Global.nBCOD_Len, '0');
                    }
                    else if (Global.nBCOD_Type == 1 && Global.BCOD_tb1 != "")
                    {
                        Tb3_Txt_BCOD.ExCodeDB = Global.BCOD_tb1.TrimEnd(' ');
                    }
                    else
                    {
                        Tb3_Txt_BCOD.ExCodeDB = "";
                    }
                    if (Global.BCOD_tb1 == "0" || Global.BCOD_tb1 == "")
                    {
                        Tb3_Txt_BNAM.Text = "全て";
                    }
                    else if (Global.BCOD_tb1 != "")
                    {
                        Tb1_Txt_BCOD_Validating(Tb3_Txt_BCOD, null);
                    }

                    //休日補正の退避
                    sTmpHrai_H = Global.HARAI_H_tb1;
                    sTmpKijitu_H = Global.KIJITU_H_tb1;

                    Tb1_Txt_SHINO_Validating(null, null);

                    // de3_10681 差分 -->
                    //休日補正の復帰
                    Global.HARAI_H_tb1 = sTmpHrai_H;
                    Global.KIJITU_H_tb1 = sTmpKijitu_H;
                    if (sTmpHrai_H != "")
                    {
                        Tb3_Cmb_HARAI_H.Text = sTmpHrai_H + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(sTmpHrai_H));
                    }
                    if (sTmpKijitu_H != "")
                    {
                        Tb3_Cmb_KIJITU_H.Text = sTmpKijitu_H + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(sTmpKijitu_H));
                    }
                    //if (nTRCD_ChgFlg == 1)
                    //{
                    //    //休日補正の復帰
                    //    Global.HARAI_H_tb1 = sTmpHrai_H;
                    //    Global.KIJITU_H_tb1 = sTmpKijitu_H;
                    //    if (sTmpHrai_H != "")
                    //    {
                    //        Tb3_Cmb_HARAI_H.Text = sTmpHrai_H + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(sTmpHrai_H));
                    //    }
                    //    if (sTmpKijitu_H != "")
                    //    {
                    //        Tb3_Cmb_KIJITU_H.Text = sTmpKijitu_H + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(sTmpKijitu_H));
                    //    }
                    //}
                    // de3_10681 差分 <--
                }
                else
                {
                    Tb3_Lbl_Old_New2.Text = "【　新規　】";
                    Tb1_Lbl_SHO_ID_V.Text = "1";
                    BindNavi2_Selected.Text = "1";
                    BindNavi2_Cnt.Text = "/ " + "1";
                    Tb3_Txt_BCOD.ExCodeDB = "";
                    Tb3_Txt_BNAM.Text = "";
                    Tb3_Txt_KCOD.ExCodeDB = "";
                    Tb3_Txt_KINM.Text = "";
                    Tb3_Txt_SHINO.Text = "";
                    Tb3_Txt_SHINM.Text = "";
                    Tb3_Cmb_HARAI_H.SelectedIndex = -1;
                    Tb3_Cmb_KIJITU_H.SelectedIndex = -1;
                    //取引先に該当データがない場合は自社支払方法を検索しない為、
                    //ここで初期化
                    Tb3_Txt_SHIMEBI.Text = "";
                    Tb3_Txt_SHIHARAIMM.Text = "";
                    Tb3_Txt_SIHARAIDD.Text = "";
                    Tb3_Txt_SKIJITUMM.Text = "";
                    Tb3_Txt_SKIJITUDD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_V_YAKUJO.ExNumValue = 0;
                    Tb3_Txt_YAKUJOA_L.Text = "";
                    Tb3_Txt_YAKUJOA_M.Text = "";
                    Tb3_Txt_YAKUJOB_LH.Text = "";
                    Tb3_Txt_YAKUJOB_H1.Text = "";
                    Tb3_Txt_YAKUJOB_R1.Text = "";
                    Tb3_Txt_YAKUJOB_U1.Text = "";
                    Tb3_Txt_YAKUJOB_H2.Text = "";
                    Tb3_Txt_YAKUJOB_R2.Text = "";
                    Tb3_Txt_YAKUJOB_U2.Text = "";
                    Tb3_Txt_YAKUJOB_H3.Text = "";
                    Tb3_Txt_YAKUJOB_R3.Text = "";
                    Tb3_Txt_YAKUJOB_U3.Text = "";
                }

                if (Global.SHO_ID_tb1 != "")
                {
                    if (iCnt - iCurrentCnt > 0)
                    {
                        Tb3_BindNavi_Next.Enabled = true;
                        Tb3_BindNavi_Last.Enabled = true;
                    }
                    else
                    {
                        Tb3_BindNavi_Next.Enabled = false;
                        Tb3_BindNavi_Last.Enabled = false;
                    }
                    //前データ判定
                    if (int.Parse(BindNavi2_Selected.Text) > 1)
                    {
                        Tb3_BindNavi_First.Enabled = true;
                        Tb3_BindNavi_Prev.Enabled = true;
                    }
                    else
                    {
                        Tb3_BindNavi_First.Enabled = false;
                        Tb3_BindNavi_Prev.Enabled = false;
                    }
                }
                else
                {
                    Tb3_BindNavi_First.Enabled = false;
                    Tb3_BindNavi_Next.Enabled = false;
                    Tb3_BindNavi_Prev.Enabled = false;
                    Tb3_BindNavi_Last.Enabled = false;
                }

                //新規 or 変更
                if (iCnt != 0)
                {
                    //取引先が変更された場合は支払方法をDBの値で上書
                    if (Global.KUBN1_tb3 != "")
                    {
                        string sKbn1 = Global.KUBN1_tb3.PadLeft(2) + ":" + Global.KUBNNM1_tb3;

                        //if (Tb3_Lbl_HARAI_KBN1.Text != sKbn1)
                        //{
                        Tb3_Lbl_HARAI_KBN1.Text = sKbn1;
                        string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':')));
                        Tb3_Cmb_HARAI_KBN1.DataSource = GetOwnBankList(sSKBKIND == "8");// <--- V02.37.01 YMP ADD ◀(122172)
                        //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                        if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                        {
                            Tb3_Cmb_HARAI_KBN1.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & false;
                        }
                        else if (Global.BANKNM1_tb3 != "" || Global.SITENNM1_tb3 != "" || Global.KOZA1_tb3 != "" || Global.KOZANO1_tb3 != "" || Global.IRAININ1_tb3 != "")
                        {
                            //Tb3_Cmb_HARAI_KBN1.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK1_tb3, Global.SITEN1_tb3,
                            //                                                             Global.KOZA1_tb3, Global.KOZANO1_tb3, Global.IRAININ1_tb3);
                            Tb3_Cmb_HARAI_KBN1.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK1_tb3, Global.SITEN1_tb3,
                                                                                         Global.KOZA1_tb3, Global.KOZANO1_tb3, Global.IRAININ1_tb3, sSKBKIND);
                            Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & true;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN1.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & true;
                        }
                        //}
                    }
                    else
                    {
                        Tb3_Lbl_HARAI_KBN1.Text = "";
                        Tb3_Cmb_HARAI_KBN1.DataSource = null;
                        Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & false;
                    }

                    if (Global.KUBN2_tb3 != "")
                    {
                        string sKbn2 = Global.KUBN2_tb3.PadLeft(2) + ":" + Global.KUBNNM2_tb3;
                        //if (Tb3_Lbl_HARAI_KBN2.Text != sKbn2)
                        //{
                        Tb3_Lbl_HARAI_KBN2.Text = sKbn2;
                        string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':')));
                        Tb3_Cmb_HARAI_KBN2.DataSource = GetOwnBankList(sSKBKIND == "8");// <--- V02.37.01 YMP ADD ◀(122172)
                        //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                        if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                        {
                            Tb3_Cmb_HARAI_KBN2.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & false;
                        }
                        else if (Global.BANKNM2_tb3 != "" || Global.SITENNM2_tb3 != "" || Global.KOZA2_tb3 != "" || Global.KOZANO2_tb3 != "" || Global.IRAININ2_tb3 != "")
                        {
                            //Tb3_Cmb_HARAI_KBN2.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK2_tb3, Global.SITEN2_tb3,
                            //                                                             Global.KOZA2_tb3, Global.KOZANO2_tb3, Global.IRAININ2_tb3); 
                            Tb3_Cmb_HARAI_KBN2.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK2_tb3, Global.SITEN2_tb3,
                                                                                         Global.KOZA2_tb3, Global.KOZANO2_tb3, Global.IRAININ2_tb3, sSKBKIND);
                            Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & true;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN2.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & true;
                        }
                        //}
                    }
                    else
                    {
                        Tb3_Lbl_HARAI_KBN2.Text = "";
                        Tb3_Cmb_HARAI_KBN2.DataSource = null;
                        Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & false;
                    }


                    if (Global.KUBN3_tb3 != "")
                    {
                        string sKbn3 = Global.KUBN3_tb3.PadLeft(2) + ":" + Global.KUBNNM3_tb3;
                        //if (Tb3_Lbl_HARAI_KBN3.Text != sKbn3)
                        //{
                        Tb3_Lbl_HARAI_KBN3.Text = sKbn3;
                        string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':')));
                        Tb3_Cmb_HARAI_KBN3.DataSource = GetOwnBankList(sSKBKIND == "8");// <--- V02.37.01 YMP ADD ◀(122172)
                        //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                        if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                        {
                            Tb3_Cmb_HARAI_KBN3.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & false;
                        }
                        else if (Global.BANKNM3_tb3 != "" || Global.SITENNM3_tb3 != "" || Global.KOZA3_tb3 != "" || Global.KOZANO3_tb3 != "" || Global.IRAININ3_tb3 != "")
                        {
                            //Tb3_Cmb_HARAI_KBN3.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK3_tb3, Global.SITEN3_tb3,
                            //                                                             Global.KOZA3_tb3, Global.KOZANO3_tb3, Global.IRAININ3_tb3);
                            Tb3_Cmb_HARAI_KBN3.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK3_tb3, Global.SITEN3_tb3,
                                                                                         Global.KOZA3_tb3, Global.KOZANO3_tb3, Global.IRAININ3_tb3, sSKBKIND);
                            Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & true;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN3.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & true;
                        }
                        //}
                    }
                    else
                    {
                        Tb3_Lbl_HARAI_KBN3.Text = "";
                        Tb3_Cmb_HARAI_KBN3.DataSource = null;
                        Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & false;
                    }

                    if (Global.KUBN4_tb3 != "")
                    {
                        string sKbn4 = Global.KUBN4_tb3.PadLeft(2) + ":" + Global.KUBNNM4_tb3;
                        //if (Tb3_Lbl_HARAI_KBN4.Text != sKbn4)
                        //{
                        Tb3_Lbl_HARAI_KBN4.Text = sKbn4;
                        string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':')));
                        Tb3_Cmb_HARAI_KBN4.DataSource = GetOwnBankList(sSKBKIND == "8");// <--- V02.37.01 YMP ADD ◀(122172)
                        //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                        if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                        {
                            Tb3_Cmb_HARAI_KBN4.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & false;
                        }
                        else if (Global.BANKNM4_tb3 != "" || Global.SITENNM4_tb3 != "" || Global.KOZA4_tb3 != "" || Global.KOZANO4_tb3 != "" || Global.IRAININ4_tb3 != "")
                        {
                            //Tb3_Cmb_HARAI_KBN4.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK4_tb3, Global.SITEN4_tb3,
                            //                                                             Global.KOZA4_tb3, Global.KOZANO4_tb3, Global.IRAININ4_tb3);
                            Tb3_Cmb_HARAI_KBN4.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK4_tb3, Global.SITEN4_tb3,
                                                                                         Global.KOZA4_tb3, Global.KOZANO4_tb3, Global.IRAININ4_tb3, sSKBKIND);
                            Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & true;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN4.SelectedValue = -1;
                            Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & true;
                        }
                        //}
                    }
                    else
                    {
                        Tb3_Lbl_HARAI_KBN4.Text = "";
                        Tb3_Cmb_HARAI_KBN4.DataSource = null;
                        Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & false;
                    }

                }
                else
                {
                    Tb3_Lbl_HARAI_KBN1.Text = "";
                    Tb3_Lbl_HARAI_KBN2.Text = "";
                    Tb3_Lbl_HARAI_KBN3.Text = "";
                    Tb3_Lbl_HARAI_KBN4.Text = "";
                    Tb3_Cmb_HARAI_KBN1.DataSource = null;
                    Tb3_Cmb_HARAI_KBN2.DataSource = null;
                    Tb3_Cmb_HARAI_KBN3.DataSource = null;
                    Tb3_Cmb_HARAI_KBN4.DataSource = null;
                    Tb3_Cmb_HARAI_KBN1.Text = "";
                    Tb3_Cmb_HARAI_KBN2.Text = "";
                    Tb3_Cmb_HARAI_KBN3.Text = "";
                    Tb3_Cmb_HARAI_KBN4.Text = "";
                }
                Chg_DispControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSet_Tb1_SS_TSHOH　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// 支払条件タブ.自社支払方法テーブルからの画面設定
        /// </summary>
        private void Set_Tb1_SS_SHOHO()
        {
            try
            {
                Tb3_Txt_SHIMEBI.Text = Global.SIMEBI_tb1;
                Tb3_Txt_ShimeNm.Text = "日締";
                Tb3_Txt_SHIHARAIMM.Text = Global.SHIHARAIMM_tb1;
                Tb3_Txt_SHIHARAIMM.ExNumValue = Convert.ToDecimal(Global.SHIHARAIMM_tb1);

                Tb3_Txt_SIHARAIDD.Text = Global.SIHARAIDD_tb1;
                Tb3_Txt_SIHARAIDD.ExNumValue = Convert.ToDecimal(Global.SIHARAIDD_tb1);
                Tb3_Txt_SKIJITUMM.Text = Global.SKIJITUMM_tb1;
                Tb3_Txt_SKIJITUMM.ExNumValue = Convert.ToDecimal(Global.SKIJITUMM_tb1);
                Tb3_Txt_SKIJITUDD.Text = Global.SKIJITUDD_tb1;
                Tb3_Txt_SKIJITUDD.ExNumValue = Convert.ToDecimal(Global.SKIJITUDD_tb1);
                Tb3_Txt_SKBNCOD.Text = Global.SKBNCOD_tb1;
                if (Global.SKBNCOD_tb1 != "")
                {
                    string sSKBNCOD = Global.SKBNCOD_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_SKBNCOD.Text = Tb3_Txt_SKBNCOD.Text + ":" + sSKBNM;
                    Tb3_Txt_SKBNCOD.Text = Tb3_Txt_SKBNCOD.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_SKBNCOD.Text = "";
                }
                if (Global.V_YAKUJO_tb1 != "")
                {
                    Tb3_Txt_V_YAKUJO.ExNumValue = Convert.ToInt64(Global.V_YAKUJO_tb1);
                }
                else
                {
                    Tb3_Txt_V_YAKUJO.ExNumValue = 0;
                }
                Tb3_Txt_YAKUJOA_L.Text = Global.YAKUJOA_L_tb1;
                if (Global.YAKUJOA_L_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOA_L_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOA_L.Text = Tb3_Txt_YAKUJOA_L.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOA_L.Text = Tb3_Txt_YAKUJOA_L.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOA_L.Text = "";
                }
                Tb3_Txt_YAKUJOA_M.Text = Global.YAKUJOA_M_tb1;
                if (Global.YAKUJOA_M_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOA_M_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOA_M.Text = Tb3_Txt_YAKUJOA_M.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOA_M.Text = Tb3_Txt_YAKUJOA_M.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOA_M.Text = "";
                }
                Tb3_Txt_YAKUJOB_LH.Text = Global.YAKUJOB_LH_tb1;
                if (Global.YAKUJOB_LH_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOB_LH_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOB_LH.Text = Tb3_Txt_YAKUJOB_LH.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOB_LH.Text = Tb3_Txt_YAKUJOB_LH.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOB_LH.Text = "";
                }
                Tb3_Txt_YAKUJOB_H1.Text = Global.YAKUJOB_H1_tb1;
                if (Global.YAKUJOB_H1_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOB_H1_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOB_H1.Text = Tb3_Txt_YAKUJOB_H1.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOB_H1.Text = Tb3_Txt_YAKUJOB_H1.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOB_H1.Text = "";
                }
                Tb3_Txt_YAKUJOB_R1.Text = Global.YAKUJOB_R1_tb1;
                if (Global.YAKUJOB_R1_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_R1.ExNumValue = Convert.ToDecimal(Global.YAKUJOB_R1_tb1);
                }
                if (Global.YAKUJOB_U1_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_U1.Text = mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.YAKUJOB_U1_tb1));
                }
                else
                {
                    Tb3_Txt_YAKUJOB_U1.Text = "";
                }
                Tb3_Txt_YAKUJOB_H2.Text = Global.YAKUJOB_H2_tb1;
                if (Global.YAKUJOB_H2_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOB_H2_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOB_H2.Text = Tb3_Txt_YAKUJOB_H2.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOB_H2.Text = Tb3_Txt_YAKUJOB_H2.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOB_H2.Text = "";
                }
                Tb3_Txt_YAKUJOB_R2.Text = Global.YAKUJOB_R2_tb1;
                if (Global.YAKUJOB_R2_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_R2.ExNumValue = Convert.ToDecimal(Global.YAKUJOB_R2_tb1);
                }
                if (Global.YAKUJOB_U2_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_U2.Text = mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.YAKUJOB_U2_tb1));
                }
                else
                {
                    Tb3_Txt_YAKUJOB_U2.Text = "";
                }
                Tb3_Txt_YAKUJOB_H3.Text = Global.YAKUJOB_H3_tb1;
                if (Global.YAKUJOB_H3_tb1 != "")
                {
                    string sSKBNCOD = Global.YAKUJOB_H3_tb1;
                    sSKBNM = mcBsLogic.Get_SKUBN(sSKBNCOD);
//-- <9999>
//                    Tb3_Txt_YAKUJOB_H3.Text = Tb3_Txt_YAKUJOB_H3.Text + ":" + sSKBNM;
                    Tb3_Txt_YAKUJOB_H3.Text = Tb3_Txt_YAKUJOB_H3.Text.PadLeft(2, ' ') + ":" + sSKBNM;
//-- <9999>
                }
                else
                {
                    Tb3_Txt_YAKUJOB_H3.Text = "";
                }
                Tb3_Txt_YAKUJOB_R3.Text = Global.YAKUJOB_R3_tb1;
                if (Global.YAKUJOB_R3_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_R3.ExNumValue = Convert.ToDecimal(Global.YAKUJOB_R3_tb1);
                }
                if (Global.YAKUJOB_U3_tb1 != "")
                {
                    Tb3_Txt_YAKUJOB_U3.Text = mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.YAKUJOB_U3_tb1));
                }
                else
                {
                    Tb3_Txt_YAKUJOB_U3.Text = "";
                }
                if (Global.HARAI_H_tb1 != "")
                {
                    Tb3_Cmb_HARAI_H.Text = Global.HARAI_H_tb1 + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.HARAI_H_tb1));
                }
                else
                {
                    Tb3_Cmb_HARAI_H.Text = "";
                }
                if (Global.KIJITU_H_tb1 != "")
                {
                    Tb3_Cmb_KIJITU_H.Text = Global.KIJITU_H_tb1 + ":" + mcBsLogic.Get_Hosei_NM(Convert.ToInt32(Global.KIJITU_H_tb1));
                }
                else
                {
                    Tb3_Cmb_KIJITU_H.Text = "";
                }

                //支払方法の変更により、依頼先情報タブの支払方法も再生成
                if (Global.SKBNCOD_tb1 == "1")
                {
                    Tb3_Lbl_HARAI_KBN1.Text = Tb3_Txt_YAKUJOA_L.Text;
                    Tb3_Lbl_HARAI_KBN2.Text = Tb3_Txt_YAKUJOA_M.Text;
                    Tb3_Lbl_HARAI_KBN3.Text = "";
                    Tb3_Lbl_HARAI_KBN4.Text = "";
                }
                else if (Global.SKBNCOD_tb1 == "2")
                {
                    Tb3_Lbl_HARAI_KBN1.Text = Tb3_Txt_YAKUJOB_LH.Text;
                    Tb3_Lbl_HARAI_KBN2.Text = Tb3_Txt_YAKUJOB_H1.Text;
                    Tb3_Lbl_HARAI_KBN3.Text = Tb3_Txt_YAKUJOB_H2.Text;
                    Tb3_Lbl_HARAI_KBN4.Text = Tb3_Txt_YAKUJOB_H3.Text;
                }
                else
                {
                    Tb3_Lbl_HARAI_KBN1.Text = Tb3_Txt_SKBNCOD.Text;
                    Tb3_Lbl_HARAI_KBN2.Text = "";
                    Tb3_Lbl_HARAI_KBN3.Text = "";
                    Tb3_Lbl_HARAI_KBN4.Text = "";
                }

                //支払区分が設定されていない行はコンボボックスdisable&clear
                if (Tb3_Lbl_HARAI_KBN1.Text == "")
                {
                    Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & false;
                    Tb3_Cmb_HARAI_KBN1.DataSource = null;
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & true;
                }
                if (Tb3_Lbl_HARAI_KBN2.Text == "")
                {
                    Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & false;
                    Tb3_Cmb_HARAI_KBN2.DataSource = null;
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & true;
                }
                if (Tb3_Lbl_HARAI_KBN3.Text == "")
                {
                    Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & false;
                    Tb3_Cmb_HARAI_KBN3.DataSource = null;
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & true;
                }
                if (Tb3_Lbl_HARAI_KBN4.Text == "")
                {
                    Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & false;
                    Tb3_Cmb_HARAI_KBN4.DataSource = null;
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & true;
                }
//-- <2016/02/14>
                bEventCancel = true;
//-- <>                
                Chg_DispControl();
//-- <2016/02/14>
                bEventCancel = false;
//-- <>
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSet_Tb1_SS_SHOHO　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// 振込先情報タブ.振込先銀行テーブルからの画面設定
        /// </summary>
        private void Set_Tb2_SS_FRIGIN(int iCurrentCnt, int iCnt)
        {
            try
            {
                //チェックBoxOn/Off判定
                if (iCnt < 2)
                {
                    Tb4_Chk_GIN_ID.Checked = false;
                }
                else
                {
                    Tb4_Chk_GIN_ID.Checked = true;
                }
                //前データ判定
                if (Global.GIN_ID_tb2 == null)
                {
                    Global.GIN_ID_tb2 = "0";
                }
                if (Global.GIN_ID_tb2 != "")
                {
                    if (iCurrentCnt > 1)
                    {
                        Tb4_BindNavi_First.Enabled = true;
                        Tb4_BindNavi_Prev.Enabled = true;
                    }
                    else
                    {
                        Tb4_BindNavi_First.Enabled = false;
                        Tb4_BindNavi_Prev.Enabled = false;
                    }
                    //次データ判定
                    if (iCnt - iCurrentCnt > 0)
                    {
                        Tb4_BindNavi_Next.Enabled = true;
                        Tb4_BindNavi_End.Enabled = true;
                    }
                    else
                    {
                        Tb4_BindNavi_Next.Enabled = false;
                        Tb4_BindNavi_End.Enabled = false;
                    }
                }
                else
                {
                    Tb4_BindNavi_First.Enabled = false;
                    Tb4_BindNavi_Prev.Enabled = false;
                    Tb4_BindNavi_Next.Enabled = false;
                    Tb4_BindNavi_End.Enabled = false;
                }
                //新規 or 変更
                if (iCnt != 0)
                {
                    Tb4_Lbl_Old_New3.Text = "【　変更　】";                                                                                             // タブ４ラベル
                    Tb4_BindNavi_Selected.Text = iCurrentCnt.ToString();
                    Tb4_BindNavi_Cnt.Text = "/ " + iCnt.ToString();
                    Tb4_Lbl_GIN_ID_V.Text = iCurrentCnt.ToString();// <--- V02.37.01 YMP UPDATE ◀(122172)Global.GIN_ID_tb2をiCurrentCnt.ToString()に変更// 振込先銀行IDカウンター
                    Tb4_Txt_BANK_CD.Text = Global.BANK_CD_tb2;                                                                                          // 銀行コード
                    Tb2_Txt_BANK_CD_Validating(Tb4_Txt_BANK_CD, null);                                                                                  // 銀行名
                    Tb4_Txt_SITEN_ID.Text = Global.SITEN_ID_tb2;                                                                                        // 銀行支店コード
                    Tb2_Txt_SITEN_ID_Validating(Tb4_Txt_SITEN_ID, null);                                                                                // 銀行支店名
                    if (Global.YOKIN_TYP_tb2 != "")                                                                                                     // 預金種別
                    {
                        Tb4_Cmb_YOKIN_TYP.Text = Global.YOKIN_TYP_tb2 + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(Global.YOKIN_TYP_tb2));        
                    }
//-- <2016/02/16 念のため>
                    else
                    { Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金"; }
//-- <2016/02/16>
                    if (Global.KOUZA_tb2 != "")                                                                                                         // 口座番号
                    {
                        Tb4_Txt_KOUZA.ExCodeValue = Global.KOUZA_tb2;
                    }
                    else
                    {
                        Tb4_Txt_KOUZA.ClearValue();
                    }
                    Tb4_Txt_MEIGI.Text = Global.MEIGI_tb2;                                                                                              // 名義人名
                    Tb4_Txt_MEIGIK.Text = Global.MEIGIK_tb2;                                                                                            // 名義人カナ
                    if (Global.TESUU_tb2 != "")                                                                                                         // 手数料負担コンボ
                    {
                        Tb4_Cmb_TESUU.Text = Global.TESUU_tb2 + ":" + mcBsLogic.Get_Tesuu_NM(Convert.ToInt32(Global.TESUU_tb2));
                    }
//-- <2016/02/16 念のため>
                    else
                    { Tb4_Cmb_TESUU.Text = "1:自社負担"; }
//-- <2016/02/16>
                    if (Global.SOUKIN_tb2 != "")                                                                                                        // 送金区分
                    {
                        Tb4_Cmb_SOUKIN.Text = Global.SOUKIN_tb2 + ":" + mcBsLogic.Get_Soukin_NM(Convert.ToInt32(Global.SOUKIN_tb2));
                    }
//-- <2016/02/16 念のため>
                    else
                    { Tb4_Cmb_SOUKIN.Text = "7:電信"; }
//-- <2016/02/16>
                    // --->V01.15.01 HWY UPDATE ▼(6490)
                    //Tb4_Txt_GENDO.ExNumValue = Convert.ToDecimal(Global.GENDO_tb2);                                                                     // 負担限度額
                    if (Global.GENDO_tb2 != "0" && Global.GENDO_tb2 != "")
                    {
                        Tb4_Txt_GENDO.ExNumValue = Convert.ToDecimal(Global.GENDO_tb2);     
                    }
                    else
                    {
                        Tb4_Txt_GENDO.ClearValue();
                    }
                    // <---V01.15.01 HWY UPDATE ▲(6490)
                    Tb4_Chk_FDEF.Checked = Global.FDEF == "1" ? true : false;                                                                           // 初期値
//-- <2016/03/10 条件追加>
                    if (iCnt > 1)
                    {
                        if (Global.cUsrSec.nMFLG < 2) { Tb4_Chk_FDEF.Enabled = false; }
                        else { Tb4_Chk_FDEF.Enabled = true; }
                    }
                    else { Tb4_Chk_FDEF.Enabled = false; }
//-- <2016/03/10>
                    Tb4_Chk_DDEF.Checked = Global.DDEF == "1" ? true : false;                                                                           // でんさい代表口座
                    if (Global.FTESUID != "")                                                                                                           // 手数料IDコンボ
                    {
                        Tb4_Cmb_FTESUID.SelectedValue = Convert.ToInt32(Global.FTESUID);
                    }
//-- <2016/02/16 念のため>
                    else
                    { Tb4_Cmb_FTESUID.SelectedValue = -1; }
//-- <2016/02/16>
                    Tb2_Chk_DTESUSW.Checked = Global.DTESUSW == "1" ? true : false;                                                                     // でんさい手数料設定
                    Tb2_Cmb_DTESU.SelectedIndex = -1;                                                                                                   // でんさい手数料負担コンボ
                    if (Global.DTESUSW == "1")
                    {
                        if (Global.DTESU != "")
                        {
                            Tb2_Cmb_DTESU.Text = Global.DTESU + ":" + mcBsLogic.Get_Tesuu_NM(Convert.ToInt32(Global.DTESU));
                        }
                    }
                }
                else
                {
                    Tb4_Lbl_Old_New3.Text = "【　新規　】";                                 // タブ４　新規・修正
                    Tb4_BindNavi_Selected.Text = "1";                                       // ナビゲーション
                    Tb4_BindNavi_Cnt.Text = "/ 1";                                          // ナビゲーション
                    Tb4_Lbl_GIN_ID_V.Text = "1";                                            // 振込先銀行IDカウンター
                    Tb4_Txt_BANK_CD.Text = "";                                              // 銀行コード
                    Tb4_Txt_BANK_NM.Text = "";                                              // 銀行名称
                    Tb4_Txt_SITEN_ID.Text = "";                                             // 銀行支店コード
                    Tb4_Txt_SITEN_NM.Text = "";                                             // 銀行支店名称
                    Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";                                  // 預金種別
                    Tb4_Txt_KOUZA.ClearValue(); ;                                           // 口座番号
                    Tb4_Txt_MEIGI.Text = "";                                                // 名義人名
                    Tb4_Txt_MEIGIK.Text = "";                                               // 名義人カナ
                    Tb4_Cmb_TESUU.Text = "1:自社負担";                                      // 手数料負担
                    Tb4_Cmb_SOUKIN.Text = "7:電信";                                         // 送金区分
                    Tb4_Txt_GENDO.ClearValue();                                             // 負担限度額
//-- <2016/02/08 初期値>
                    Tb4_Chk_FDEF.Checked = true;                                            // 初期値
//-- <2016/02/16 追加>
//-- <2016/03/10 初期では表示のみ>
                    Tb4_Chk_FDEF.Enabled = false;
//-- <2016/03/10>
                    Tb4_Cmb_FTESUID.SelectedValue = -1;                                     // 手数料IDコンボ
                    Tb4_Chk_DDEF.Checked = false;                                           // でんさい代表口座
                    Tb2_Chk_DTESUSW.Checked = false;                                        // でんさい手数料設定
                    Tb2_Cmb_DTESU.SelectedValue = -1;                                       // でんさい手数料負担コンボ
//-- <2016/02/16>
//-- <2016/02/08>
                }
                Chg_DispControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSet_Tb2_SS_FRIGIN　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        /// <summary>
        /// メイン画面の情報を取得
        /// </summary>
        private void Get_Main_Data()
        {
            try
            {
                //履歴関連 ＠2011/07 履歴対応
                //変更判定&dtRIREKIへの格納
                if (Global.RYAKU != Txt_RYAKU.Text)
                {
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    Set_dtRIREKI(0, 0, "RYAKU", 2, "取引先名称（略称）", Global.RYAKU, Txt_RYAKU.Text);
                }

                if (Global.TORI_NAM != Txt_TORI_NAM.Text)
                {
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    Set_dtRIREKI(0, 0, "TORI_NAM", 2, "取引先名称（正式）", Global.TORI_NAM, Txt_TORI_NAM.Text);
                }

                if (Global.KNLD != Txt_KNLD.Text)
                {
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    Set_dtRIREKI(0, 0, "KNLD", 2, "50音", Global.KNLD, Txt_KNLD.Text);
                }

                if (Global.TRFURI != Txt_TRFURI.Text)
                {
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    Set_dtRIREKI(0, 0, "TRFURI", 2, "取引先名称（フリガナ）", Global.TRFURI, Txt_TRFURI.Text);
                }

                string sTGASW = "";
                if (Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly && Cbo_SAIMU.SelectedValue.ToString() == sDueOnly)
                {
                    sTGASW = "1";
                }
                else if (Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly)
                {
                    sTGASW = "2";
                }
                else if (Cbo_SAIMU.SelectedValue.ToString() == sDueOnly)
                {
                    sTGASW = "3";
                }
                else
                {
                    sTGASW = "0";
                }
                if (Global.TGASW != sTGASW)
                {
                    string TGASW_DB = "";
                    string TGASW_Disp = "";
                    if (Global.TGASW != "")
                    {
                        TGASW_DB = Global.TGASW + ":" + mcBsLogic.Get_TGASW_NM(Global.TGASW);
                    }
                    if (sTGASW != "")
                    {
                        TGASW_Disp = sTGASW + ":" + mcBsLogic.Get_TGASW_NM(sTGASW);
                    }
                    Set_dtRIREKI(0, 0, "TGASW", 2, "期日管理のみSW", TGASW_DB, TGASW_Disp);
                }

                string sSW = "";
                if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.SAIKEN != sSW)
                {
                    string SW_DB = "";
                    string SW_Disp = "";
                    if (Global.SAIKEN != "")
                    {
                        SW_DB = Global.SAIKEN + ":" + mcBsLogic.Get_SAIKEN_NM(Global.SAIKEN);
                    }
                    if (sSW != "")
                    {
                        SW_Disp = sSW + ":" + mcBsLogic.Get_SAIKEN_NM(sSW);
                    }
                    Set_dtRIREKI(0, 0, "SAIKEN", 2, "得意先SW", SW_DB, SW_Disp);
                }
                if (Chk_SAIKEN_FLG.Checked == true)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.SAIKEN_FLG != sSW)
                {
                    string SW_DB = "";
                    string SW_Disp = "";
                    if (Global.SAIKEN_FLG != "")
                    {
                        SW_DB = Global.SAIKEN_FLG + ":" + mcBsLogic.Get_SAIKEN_FLG_NM(Global.SAIKEN_FLG);
                    }
                    if (sSW != "")
                    {
                        SW_Disp = sSW + ":" + mcBsLogic.Get_SAIKEN_FLG_NM(sSW);
                    }
                    Set_dtRIREKI(0, 0, "SAIKEN_FLG", 2, "入金代表者SW", SW_DB, SW_Disp);
                }
                if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.SAIMU != sSW)
                {
                    string SW_DB = "";
                    string SW_Disp = "";
                    if (Global.SAIMU != "")
                    {
                        SW_DB = Global.SAIMU + ":" + mcBsLogic.Get_SAIMU_NM(Global.SAIMU);
                    }
                    if (sSW != "")
                    {
                        SW_Disp = sSW + ":" + mcBsLogic.Get_SAIMU_NM(sSW);
                    }
                    Set_dtRIREKI(0, 0, "SAIMU", 2, "仕入先SW", SW_DB, SW_Disp);
                }
                if (Chk_SAIMU_FLG.Checked == true)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.SAIMU_FLG != sSW)
                {
                    string SW_DB = "";
                    string SW_Disp = "";
                    if (Global.SAIMU_FLG != "")
                    {
                        SW_DB = Global.SAIMU_FLG + ":" + mcBsLogic.Get_SAIMU_FLG_NM(Global.SAIMU_FLG);
                    }
                    if (sSW != "")
                    {
                        SW_Disp = sSW + ":" + mcBsLogic.Get_SAIMU_FLG_NM(sSW);
                    }
                    Set_dtRIREKI(0, 0, "SAIMU_FLG", 2, "支払代表者SW", SW_DB, SW_Disp);
                }

                if (Global.GRPID != Txt_GRPID.ExNumValue.ToString())
                {
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    Set_dtRIREKI(0, 0, "GRPID", 2, "取引先グループ", Global.GRPID, Txt_GRPID.ExNumValue.ToString());
                }

                Global.SAIKEN = (Cbo_SAIKEN.SelectedValue.ToString() == sUse ? "1" : "0");
                Global.SAIKEN_FLG = (Chk_SAIKEN_FLG.Checked == true ? "1" : "0");
                Global.SAIMU = (Cbo_SAIMU.SelectedValue.ToString() == sUse ? "1" : "0");
                Global.SAIMU_FLG = (Chk_SAIMU_FLG.Checked == true ? "1" : "0");

                string sSTAN = "";
                if (Global.nKMAN != 0)
                {
                    sSTAN = Tb5_Txt_STAN_CD.Text;
                    if (Global.STAN != sSTAN)
                    {
                        string sSTAN_DB = "";
                        string sSTAN_Disp = "";
                        if (Global.STAN != "")
                        {
                            sSTAN_DB = Global.STAN + ":" + mcBsLogic.Get_TNAM(Global.STAN);
                        }
                        if (sSTAN != "")
                        {
                            sSTAN_Disp = sSTAN + ":" + mcBsLogic.Get_TNAM(sSTAN);
                        }
                        Set_dtRIREKI(5, 0, "STAN", 2, "主担当者", sSTAN_DB, sSTAN_Disp);
                    }
                }

                string sSBCOD = "";
                if (Global.nBCOD_F == 1)
                {
                    if (Tb5_Txt_SBCOD.ExCodeDB != "")
                    {
                        sSBCOD = (Global.nBCOD_Type == 0 ? Tb5_Txt_SBCOD.ExCodeDB.PadLeft(Global.nBCOD_Len, '0') : Tb5_Txt_SBCOD.ExCodeDB.PadRight(Global.nBCOD_Len, ' '));
                    }

                    if (Global.SBCOD != sSBCOD)
                    {
                        string sSBCOD_DB = "";
                        string sSBCOD_Disp = "";
                        if (Global.SBCOD != "")
                        {
                            sSBCOD_DB = Global.SBCOD + ":" + mcBsLogic.Get_BNAM(Global.SBCOD);
                        }
                        if (sSBCOD != "")
                        {
                            sSBCOD_Disp = sSBCOD + ":" + mcBsLogic.Get_BNAM(sSBCOD);
                        }
                        Set_dtRIREKI(5, 0, "SBCOD", 2, "初期表示部門ｺｰﾄﾞ", sSBCOD_DB, sSBCOD_Disp);
                    }
                }

                string sSKICD = "";
                if (Tb5_Txt_SKCOD.ExCodeDB != "")
                {
                    //sSKICD = mcBsLogic.Conv_KCODtoKICD(Txt_SKCOD.Text);
                    sSKICD = mcBsLogic.Conv_KCODtoKICD(Global.nKCOD_Type == 0 ? Tb5_Txt_SKCOD.ExCodeDB.PadLeft(Global.nKCOD_Len,'0'): Tb5_Txt_SKCOD.ExCodeDB.PadRight(Global.nKCOD_Len));
                }
                if (Global.SKICD != sSKICD)
                {
                    string sKICD_DB = "";
                    string sKICD_Disp = "";
                    if (Global.SKICD != "")
                    {
                        sKICD_DB = mcBsLogic.Conv_KICDtoKCOD(Global.SKICD) + ":" + mcBsLogic.Get_KNAM(mcBsLogic.Conv_KICDtoKCOD(Global.SKICD));
                        if (sKICD_DB == ":")
                        {
                            sKICD_DB = "";
                        }
                    }
                    if (sSKICD != "")
                    {
                        sKICD_Disp = mcBsLogic.Conv_KICDtoKCOD(sSKICD) + ":" + mcBsLogic.Get_KNAM(mcBsLogic.Conv_KICDtoKCOD(sSKICD));
                        if (sKICD_Disp == ":")
                        {
                            sKICD_Disp = "";
                        }
                    }
                    Set_dtRIREKI(5, 0, "SKICD", 2, "初期表示科目ｺｰﾄﾞ", sKICD_DB, sKICD_Disp);
                }

                string sNAYOSE = (Tb5_Chk_NAYOSE.Checked == true ? "1" : "0");
                if (Global.NAYOSE != sNAYOSE)
                {
                    Set_dtRIREKI(5, 0, "NAYOSE", 2, "名寄SW", mcBsLogic.Get_NayoseNM(Global.NAYOSE), mcBsLogic.Get_NayoseNM(sNAYOSE));
                }

                // Ver.01.02.03 Toda -->
                //string sF_SETUIN = (Chk_SAIMU.Checked == true && Tb5_Chk_F_SETUIN.Checked == true ? "1" : "0");
                string sF_SETUIN = (Tb5_Chk_F_SETUIN.Checked == true ? "1" : "0");
                // Ver.01.02.03 <--
                if (Global.F_SETUIN != sF_SETUIN)
                {
                    Set_dtRIREKI(5, 0, "F_SETUIN", 2, "節印実行", mcBsLogic.Get_SetuinNM(Global.F_SETUIN), mcBsLogic.Get_SetuinNM(sF_SETUIN));
                }

                string sSTFLG = (Chk_STFLG.Checked == true ? "1" : "0");
                if (Global.STFLG != sSTFLG)
                {
                    string sSTFLG_DB = "";
                    string sSTFLG_Disp = "";
                    if (Global.STFLG != "")
                    {
                        sSTFLG_DB = Global.STFLG + ":" + mcBsLogic.Get_STFLGNM(Global.STFLG);
                    }
                    if (sSTFLG != "")
                    {
                        sSTFLG_Disp = sSTFLG + ":" + mcBsLogic.Get_STFLGNM(sSTFLG);
                    }
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iZCheck = 1; }
                    Set_dtRIREKI(0, 0, "STFLG", 2, "取引停止", sSTFLG_DB, sSTFLG_Disp);
                }

                string sSTYMD = Txt_STYMD.Value.ToString();
                if (Global.STYMD != sSTYMD && (Global.STYMD.Length == 8 || sSTYMD.Length == 8))
                {
                    string sSTYMD_DB = "";
                    string sSTYMD_Disp = "";
                    if (Global.STYMD.Length == 8)
                    {
                        sSTYMD_DB = Global.STYMD.Insert(4, "/").Insert(7, "/");
                    }
                    if (sSTYMD.Length == 8)
                    {
                        sSTYMD_Disp = sSTYMD.Insert(4, "/").Insert(7, "/");
                    }
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iZCheck = 1; }
                    Set_dtRIREKI(0, 0, "STYMD", 2, "使用開始日", sSTYMD_DB, sSTYMD_Disp);
                }

                string sEDYMD = Txt_EDYMD.Value.ToString();
                if (Global.EDYMD != sEDYMD && (Global.EDYMD.Length == 8 || sEDYMD.Length == 8))
                {
                    string sEDYMD_DB = "";
                    string sEDYMD_Disp = "";
                    if (Global.EDYMD.Length == 8)
                    {
                        sEDYMD_DB = Global.EDYMD.Insert(4, "/").Insert(7, "/");
                    }
                    if (sEDYMD.Length == 8)
                    {
                        sEDYMD_Disp = sEDYMD.Insert(4, "/").Insert(7, "/");
                    }
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iDispChangeFlg = 1; }
                    if (Lbl_Old_New1.Text == "【　変更　】") { Global.iZCheck = 1; }
                    Set_dtRIREKI(0, 0, "EDYMD", 2, "使用終了日", sEDYMD_DB, sEDYMD_Disp);
                }


                //画面項目のGlobalへの格納
                if (!Global.bIchigen)
                {
                    Global.TRCD = (Global.nTRCD_Type == 0 ? Txt_TRCD.ExCodeDB.PadLeft(Global.nTRCD_Len, '0') : Txt_TRCD.ExCodeDB.PadRight(Global.nTRCD_Len, ' '));
                }
                else
                {
                    Global.TRCD = Global.nIchigenCode;
                }

                if (Global.nTRCD_HJ == 1)
                {
                    Global.HJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                }
                else
                {
                    Global.HJCD = "0";
                }
                Global.TRKBN = "0";
                Global.RYAKU = Txt_RYAKU.Text;
                Global.TORI_NAM = Txt_TORI_NAM.Text;
                Global.KNLD = Txt_KNLD.Text;
                Global.TGASW = sTGASW;

                Global.STAN = sSTAN;
                Global.SJBCD = "0";
                Global.SBCOD = sSBCOD;
                Global.SKICD = sSKICD;
                Global.NAYOSE = sNAYOSE;
                Global.F_SETUIN = sF_SETUIN;
                Global.STFLG = sSTFLG;
                Global.STYMD = sSTYMD;
                Global.EDYMD = sEDYMD;

                Global.GRPID = Txt_GRPID.ExNumValue.ToString();
                Global.TRFURI = Txt_TRFURI.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Main_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }


        /// <summary>
        /// 基本情報タブの画面情報を取得
        /// </summary>
        private void Get_Tb1_Data()
        {
            try
            {
                string sZIP = "";
                if (Tb1_Txt_ZIP.Text != "")
                {
//-- < 8桁だったら>
//                    sZIP = Tb1_Txt_ZIP.Text.Remove(3, 1);
                    if (Tb1_Txt_ZIP.Text.Length == 8)
                    {
                        sZIP = Tb1_Txt_ZIP.Text.Remove(3, 1);
                    }
                    else
                    {
                        sZIP = Tb1_Txt_ZIP.Text;
                    }
//-- <>
                }
                if (Global.ZIP != sZIP)
                {
                    string sZIP_DB = "";
                    string sZIP_Disp = "";
                    if (Global.ZIP != "")
                    {
                        sZIP_DB = Global.ZIP.Insert(3, "-");
                    }
                    if (sZIP != "")
                    {
                        sZIP_Disp = sZIP.Insert(3, "-");
                    }
                    Set_dtRIREKI(0, 0, "ZIP", 2, "郵便番号", sZIP_DB, sZIP_Disp);
                }

                if (Global.ADDR1 != Tb1_Txt_ADDR1.Text)
                {
                    Set_dtRIREKI(0, 0, "ADDR1", 2, "住所１", Global.ADDR1, Tb1_Txt_ADDR1.Text);
                }

                if (Global.ADDR2 != Tb1_Txt_ADDR2.Text)
                {
                    Set_dtRIREKI(0, 0, "ADDR2", 2, "住所２", Global.ADDR2, Tb1_Txt_ADDR2.Text);
                }

                if (Global.TEL != Tb1_Txt_TEL.Text)
                {
                    Set_dtRIREKI(0, 0, "TEL", 2, "TEL", Global.TEL, Tb1_Txt_TEL.Text);
                }

                if (Global.FAX != Tb1_Txt_FAX.Text)
                {
                    Set_dtRIREKI(0, 0, "FAX", 2, "FAX", Global.FAX, Tb1_Txt_FAX.Text);
                }

                if (Global.SBUSYO != Tb1_Txt_SBUSYO.Text)
                {
                    Set_dtRIREKI(0, 0, "SBUSYO", 2, "先方担当部署", Global.SBUSYO, Tb1_Txt_SBUSYO.Text);
                }

                if (Global.STANTO != Tb1_Txt_STANTO.Text)
                {
                    Set_dtRIREKI(0, 0, "STANTO", 2, "先方担当者", Global.STANTO, Tb1_Txt_STANTO.Text);
                }

                if (Tb1_Cmb_KEICD.SelectedValue != null)
                {
                    if (Global.KEICD != Tb1_Cmb_KEICD.SelectedValue.ToString())
                    {
                        Set_dtRIREKI(0, 0, "KEICD", 2, "敬称区分", Global.KEICD + ":" + mcBsLogic.Get_KeiNM(Global.KEICD),
                                     Tb1_Cmb_KEICD.SelectedValue.ToString() + ":" + mcBsLogic.Get_KeiNM(Tb1_Cmb_KEICD.SelectedValue.ToString()));
                    }
                }

                if (Global.TRMAIL != Tb1_Txt_TRMAIL.Text)
                {
                    Set_dtRIREKI(0, 0, "TRMAIL", 2, "ﾒｰﾙｱﾄﾞﾚｽ", Global.TRMAIL, Tb1_Txt_TRMAIL.Text);
                }

                if (Global.TRURL != Tb1_Txt_TRURL.Text)
                {
                    Set_dtRIREKI(0, 0, "TRURL", 2, "ﾎｰﾑﾍﾟｰｼﾞ", Global.TRURL, Tb1_Txt_TRURL.Text);
                }

                if (Global.BIKO != Tb1_Txt_BIKO.Text)
                {
                    Set_dtRIREKI(0, 0, "BIKO", 2, "備考", Global.BIKO, Tb1_Txt_BIKO.Text);
                }

                if (Global.E_TANTOCD != Tb1_Txt_E_TANTOCD.Text)
                {
                    Set_dtRIREKI(0, 0, "E_TANTOCD", 2, "営業担当者", Global.E_TANTOCD, Tb1_Txt_E_TANTOCD.Text);
                }

                if (Global.CDM1 != Tb1_Txt_UsrNo.Text)
                {
                    Set_dtRIREKI(0, 0, "CDM1", 2, "利用者番号", Global.CDM1, Tb1_Txt_UsrNo.Text);
                }

                string sJyoto = "";
                if (Tb1_Chk_Jyoto.Checked == true)
                {
                    sJyoto = "1";
                }
                else
                {
                    sJyoto = "0";
                }
                if (Global.IDM1 != sJyoto)
                {
                    Set_dtRIREKI(0, 0, "IDM1", 2, "譲渡制限ﾌﾗｸﾞ", mcBsLogic.Get_JyotoNM(Global.IDM1), mcBsLogic.Get_JyotoNM(sJyoto));
                }

                if (Global.MYNO_AITE != Tb1_Txt_MYNO_AITE.Text)
                {
                    Set_dtRIREKI(0, 0, "MYNO_AITE", 2, "相手先法人番号", Global.MYNO_AITE, Tb1_Txt_MYNO_AITE.Text);
                }

                string sSosai = "";
//-- <2016/02/14 相殺使用しないを追加>
//                if (Tb1_Chk_SOSAI.Checked == true)
//                {
//                    sSosai = "1";
//                }
//                else
//                {
//                    sSosai = "0";
//                }
                if (Global.nSOSAI_F == 0)
                { sSosai = "0"; }
                else
                {
                    if (Tb1_Chk_SOSAI.Checked == true)
                    {
                        sSosai = "1";
                    }
                    else
                    {
                        sSosai = "0";
                    }
                }
//-- <2016/02/14>
                if (Global.SOSAI != sSosai)
                {
                    Set_dtRIREKI(0, 0, "SOSAI", 2, "相殺許可SW", mcBsLogic.Get_SosaiNM(Global.SOSAI), mcBsLogic.Get_SosaiNM(sSosai));
                }

                if (Tb1_Chk_SRYOU_F.Checked == true)
                {
                    sSosai = "1";
                }
                else
                {
                    sSosai = "0";
                }
                if (Global.SRYOU_F != sSosai)
                {
                    Set_dtRIREKI(0, 0, "SRYOU_F", 2, "相殺領収書発行SW", mcBsLogic.Get_SosaiRyouNM(Global.SRYOU_F), mcBsLogic.Get_SosaiRyouNM(sSosai));
                }

                Global.ZIP = sZIP;
                Global.ADDR1 = Tb1_Txt_ADDR1.Text;
                Global.ADDR2 = Tb1_Txt_ADDR2.Text;
                Global.TEL = Tb1_Txt_TEL.Text;
                Global.FAX = Tb1_Txt_FAX.Text;
                Global.SBUSYO = Tb1_Txt_SBUSYO.Text;
                Global.STANTO = Tb1_Txt_STANTO.Text;
                if (Tb1_Cmb_KEICD.SelectedValue != null)
                {
                    Global.KEICD = Tb1_Cmb_KEICD.SelectedValue.ToString();
                }

                Global.TRMAIL = Tb1_Txt_TRMAIL.Text;
                Global.TRURL = Tb1_Txt_TRURL.Text;
                Global.BIKO = Tb1_Txt_BIKO.Text;
                //---> V02.01.03 YME UPDATE ▼【PostgreSQL対応】
                //Global.E_TANTOCD = Tb1_Txt_E_TANTOCD.Text;
                Global.E_TANTOCD = Tb1_Txt_E_TANTOCD.ExCodeDB;
                //<--- V02.01.03 YME UPDATE ▲【PostgreSQL対応】

                Global.CDM1 = Tb1_Txt_UsrNo.Text;
                Global.IDM1 = (Tb1_Chk_Jyoto.Checked == true ? "1" : "0");

                Global.MYNO_AITE = Tb1_Txt_MYNO_AITE.Text;
//-- <2016/02/14 相殺使用しないを追加>
//                Global.SOSAI = (Tb1_Chk_SOSAI.Checked == true ? "1" : "0");
                if (Global.nSOSAI_F == 0)
                { Global.SOSAI = "0"; }
                else
                { Global.SOSAI = (Tb1_Chk_SOSAI.Checked == true ? "1" : "0"); }
//-- <2016/02/14>
                Global.SRYOU_F = (Tb1_Chk_SRYOU_F.Checked == true ? "1" : "0");
            }
            catch (Exception ex)
            {
//-- <2016/03/09 文言等>
                MessageBox.Show(
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb1_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }


        /// <summary>
        /// 回収設定タブの画面情報を取得
        /// </summary>
        private void Get_Tb2_Data()
        {
//-- <2016/03/09 トラップ追加>
            try
            { 
//-- <2016/03/09>
                    #region 入金消込設定
                    if (Global.TOKUKANA != Tb2_Txt_TOKUKANA.Text)
                    {
                        Set_dtRIREKI(3, 0, "TOKUKANA", 2, "照合用フリガナ", Global.TOKUKANA, Tb2_Txt_TOKUKANA.Text);
                    }

                    if (Global.FUTAN != mcBsLogic.Get_Tesuu_CD(Tb2_Cmb_FUTAN.Text))
                    {
                        string sFutan_DB = "";
                        if (Global.FUTAN != "")
                        {
                            sFutan_DB = Global.FUTAN + ":" + mcBsLogic.Get_Tesuu_NM(int.Parse(Global.FUTAN));
                        }
                        Set_dtRIREKI(3, 0, "FUTAN", 2, "手数料負担区分", sFutan_DB, Tb2_Cmb_FUTAN.Text);
                    }
                    #endregion

                    #region 回収予定設定

                    string sSW = "";
                    if (Tb2_Chk_YAKUJO.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.YAKUJYO != sSW)
                    {
                        Set_dtRIREKI(3, 0, "YAKUJO", 2, "約定使用SW", mcBsLogic.Get_YakujyoNM(Global.YAKUJYO), mcBsLogic.Get_YakujyoNM(sSW));
                    }

                    string sKaisyu = "";
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Cmb_KAISYU.Enabled == true)
                    if (Tb2_Cmb_KAISYU.SelectedIndex > -1)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = (Tb2_Cmb_KAISYU.SelectedItem as TBLKUBUN).KUBUNCD;
                    }
                    if (Global.KAISYU != sKaisyu)
                    {
                        Set_dtRIREKI(3, 0, "KAISYU", 2, "回収設定", mcBsLogic.Get_NKUBN_Display(Global.KAISYU), Tb2_Cmb_KAISYU.Text);
                    }

                    string sShime = (Global.SHIME == "99" ? "末" : Global.SHIME);
                    if (sShime != Tb2_Txt_SHIME.Text)
                    {
                        Set_dtRIREKI(3, 0, "SHIME", 2, "締日", sShime, Tb2_Txt_SHIME.Text);
                    }

                    sKaisyu = Tb2_Txt_KAISYUHI_M.Text + Tb2_Txt_KAISYUHI_D.Text.Replace("末", "99").PadLeft(2, '0');
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_KAISYUHI_M.Enabled == false)
                    if (Tb2_Txt_KAISYUHI_D.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = "";
                    }
                    if (Global.KAISYUHI != sKaisyu)
                    {
                        Set_dtRIREKI(3, 0, "KAISYUHI", 2, "回収予定", Global.KAISYUHI, sKaisyu);
                    }

                    sKaisyu = Tb2_Txt_KAISYUSIGHT_M.Text + Tb2_Txt_KAISYUSIGHT_D.Text.Replace("末", "99").PadLeft(2, '0');
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_KAISYUSIGHT_M.Enabled == false)
                    if (Tb2_Txt_KAISYUSIGHT_D.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = "";
                    }
                    if (Global.KAISYUSIGHT != sKaisyu)
                    {
                        //Set_dtRIREKI(0, 0, "KAISYUSIGHT", 2, "回収サイト", Global.KAISYUSIGHT, sKaisyu);
                        Set_dtRIREKI(3, 0, "KAISYUSIGH", 2, "回収サイト", Global.KAISYUSIGHT, sKaisyu);
                    }

                    string sY_Kingaku_Disp = "0";
                    string sY_Kingaku_DB = "0";
                    if (Tb2_Txt_Y_KINGAKU.ExNumValue != 0)
                    {
                        sY_Kingaku_Disp = Tb2_Txt_Y_KINGAKU.ExNumValue.ToString("#,##0");
                    }
                    if (Global.Y_KINGAKU != "")
                    {
                        sY_Kingaku_DB = Convert.ToInt64(Global.Y_KINGAKU).ToString("#,##0");
                    }
                    if (sY_Kingaku_DB != sY_Kingaku_Disp)
                    {
                        Set_dtRIREKI(3, 0, "Y_KINGAKU", 2, "約定金額", sY_Kingaku_DB, sY_Kingaku_Disp);
                    }


                    string sHoliday_DB = "";
                    string sHoliday_Disp = "";
                    if (Tb2_Cmb_HOLIDAY.SelectedIndex.ToString() != "-1")
                    {
                        sHoliday_Disp = Tb2_Cmb_HOLIDAY.SelectedIndex.ToString();
                    }
                    if (Global.HOLIDAY != sHoliday_Disp)
                    {
                        if (Global.HOLIDAY == "0")
                        {
                            sHoliday_DB = "0:前営業日";
                        }
                        else if (Global.HOLIDAY == "1")
                        {
                            sHoliday_DB = "1:当日";
                        }
                        else if (Global.HOLIDAY == "2")
                        {
                            sHoliday_DB = "2:後営業日";
                        }
                        Set_dtRIREKI(3, 0, "HOLIDAY", 2, "休業日設定", sHoliday_DB, Tb2_Cmb_HOLIDAY.Text);
                    }

                    string sY_Miman = "";
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Cmb_MIMAN.Enabled == true)
                    if (Tb2_Cmb_MIMAN.SelectedIndex > -1)
                    // Ver.01.02.03 <--
                    {
                        sY_Miman = (Tb2_Cmb_MIMAN.SelectedItem as TBLKUBUN).KUBUNCD;
                    }
                    if (Global.MIMAN != sY_Miman)
                    {
                        Set_dtRIREKI(3, 0, "MIMAN", 2, "約定金額未満回収区分", mcBsLogic.Get_NKUBN_Display(Global.MIMAN), Tb2_Cmb_MIMAN.Text);
                    }

                    string sY_Ijyou = "";
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Cmb_IJOU_1.Enabled == true)
                    if (Tb2_Cmb_IJOU_1.SelectedIndex > -1)
                    // Ver.01.02.03 <--
                    {
                        sY_Ijyou = (Tb2_Cmb_IJOU_1.SelectedItem as TBLKUBUN).KUBUNCD;
                    }
                    if (Global.IJOU_1 != sY_Ijyou)
                    {
                        Set_dtRIREKI(3, 0, "IJOU_1", 2, "約定金額以上回収区分①", mcBsLogic.Get_NKUBN_Display(Global.IJOU_1), Tb2_Cmb_IJOU_1.Text);
                    }

                    string Bunkatsu = Tb2_Txt_BUNKATSU_1.ExNumValue.ToString("0.000");//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】[00」のみ追加
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_BUNKATSU_1.Enabled == false)
                    if (Tb2_Txt_BUNKATSU_1.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        Bunkatsu = "";
                    }
                    if (Global.BUNKATSU_1 != Bunkatsu)
                    {
                        Set_dtRIREKI(3, 0, "BUNKATSU_1", 2, "分割１", Global.BUNKATSU_1, Bunkatsu);
                    }

                    string Hasu = "";
                    if (Global.HASU_1 != "")
                        Hasu = Global.HASU_1 + ":" + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.HASU_1));
                    if (Hasu != Tb2_Cmb_HASU_1.Text)
                    {
                        Set_dtRIREKI(3, 0, "HASU_1", 2, "端数処理１", Hasu, Tb2_Cmb_HASU_1.Text);
                    }

                    sKaisyu = Tb2_Txt_SIGHT_M_1.Text + Tb2_Txt_SIGHT_D_1.Text.Replace("末", "99").PadLeft(2, '0');
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_SIGHT_M_1.Enabled == false)
                    if (Tb2_Txt_SIGHT_D_1.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = "";
                    }
                    if (Global.SIGHT_1 != sKaisyu)
                    {
                        Set_dtRIREKI(3, 0, "SIGHT_1", 2, "回収サイト１", Global.SIGHT_1, sKaisyu);
                    }

                    sY_Ijyou = "";
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Cmb_IJOU_2.Enabled == true)
                    if (Tb2_Cmb_IJOU_2.SelectedIndex > 0)
                    // Ver.01.02.03 <--
                    {
                        sY_Ijyou = (Tb2_Cmb_IJOU_2.SelectedItem as TBLKUBUN).KUBUNCD;
                    }
                    else if (Tb2_Cmb_IJOU_2.SelectedIndex == 0)
                    {
                        sY_Ijyou = "0";
                    }
                    if (Global.IJOU_2 != sY_Ijyou)
                    {
                        Set_dtRIREKI(3, 0, "IJOU_2", 2, "約定金額以上回収区分②", mcBsLogic.Get_NKUBN_Display(Global.IJOU_2), Tb2_Cmb_IJOU_2.Text);
                    }

                    Bunkatsu = Tb2_Txt_BUNKATSU_2.ExNumValue.ToString("0.000");//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】[00」のみ追加
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_BUNKATSU_2.Enabled == false)
                    if (Tb2_Txt_BUNKATSU_2.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        Bunkatsu = "";
                    }
                    if (Global.BUNKATSU_2 != Bunkatsu)
                    {
                        Set_dtRIREKI(3, 0, "BUNKATSU_2", 2, "分割２", Global.BUNKATSU_2, Bunkatsu);
                    }

                    Hasu = "";
                    if (Global.HASU_2 != "")
                        Hasu = Global.HASU_2 + ":" + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.HASU_2));
                    if (Hasu != Tb2_Cmb_HASU_2.Text)
                    {
                        Set_dtRIREKI(3, 0, "HASU_2", 2, "端数処理２", Hasu, Tb2_Cmb_HASU_2.Text);
                    }

                    sKaisyu = Tb2_Txt_SIGHT_M_2.Text + Tb2_Txt_SIGHT_D_2.Text.Replace("末", "99").PadLeft(2, '0');
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_SIGHT_M_2.Enabled == false)
                    if (Tb2_Txt_SIGHT_D_2.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = "";
                    }
                    if (Global.SIGHT_2 != sKaisyu)
                    {
                        Set_dtRIREKI(3, 0, "SIGHT_2", 2, "回収サイト２", Global.SIGHT_2, sKaisyu);
                    }

                    sY_Ijyou = "";
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Cmb_IJOU_3.Enabled == true)
                    if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
                    // Ver.01.02.03 <--
                    {
                        sY_Ijyou = (Tb2_Cmb_IJOU_3.SelectedItem as TBLKUBUN).KUBUNCD;
                    }
                    else if (Tb2_Cmb_IJOU_3.SelectedIndex == 0)
                    {
                        sY_Ijyou = "0";
                    }
                    if (Global.IJOU_3 != sY_Ijyou)
                    {
                        Set_dtRIREKI(3, 0, "IJOU_3", 2, "約定金額以上回収区分③", mcBsLogic.Get_NKUBN_Display(Global.IJOU_3), Tb2_Cmb_IJOU_3.Text);
                    }

                    Bunkatsu = Tb2_Txt_BUNKATSU_3.ExNumValue.ToString("0.000");//<--- V02.01.01 HWPO UPDATE ◀【PostgreSQL対応】[00」のみ追加
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_BUNKATSU_3.Enabled == false)
                    if (Tb2_Txt_BUNKATSU_3.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        Bunkatsu = "";
                    }
                    if (Global.BUNKATSU_3 != Bunkatsu)
                    {
                        Set_dtRIREKI(3, 0, "BUNKATSU_3", 2, "分割３", Global.BUNKATSU_3, Bunkatsu);
                    }

                    Hasu = "";
                    if (Global.HASU_3 != "")
                        Hasu = Global.HASU_3 + ":" + mcBsLogic.Get_HasuUnit_NM(Convert.ToInt32(Global.HASU_3));
                    if (Hasu != Tb2_Cmb_HASU_3.Text)
                    {
                        Set_dtRIREKI(3, 0, "HASU_3", 2, "端数処理３", Hasu, Tb2_Cmb_HASU_3.Text);
                    }

                    sKaisyu = Tb2_Txt_SIGHT_M_3.Text + Tb2_Txt_SIGHT_D_3.Text.Replace("末", "99").PadLeft(2, '0');
                    // Ver.01.02.03 Toda -->
                    //if (Tb2_Txt_SIGHT_M_3.Enabled == false)
                    if (Tb2_Txt_SIGHT_D_3.Text.Length == 0)
                    // Ver.01.02.03 <--
                    {
                        sKaisyu = "";
                    }
                    if (Global.SIGHT_3 != sKaisyu)
                    {
                        Set_dtRIREKI(3, 0, "SIGHT_3", 2, "回収サイト３", Global.SIGHT_3, sKaisyu);
                    }
                    #endregion

                    #region 専用入金口座
                    if (Global.SEN_GINKOCD != Tb2_Txt_SEN_GINKOCD.Text)
                    {
                        string sSenGin_Old = "";
                        string sSenGin_New = "";

                        if (Global.SEN_GINKOCD != "")
                        {
                            sSenGin_Old = Global.SEN_GINKOCD + ":" + mcBsLogic.Get_BANKNM(Global.SEN_GINKOCD);
                        }
                        if (Tb2_Txt_SEN_GINKOCD.ExCodeDB != "")
                        {
                            sSenGin_New = Tb2_Txt_SEN_GINKOCD.ExCodeDB + ":" + mcBsLogic.Get_BANKNM(Tb2_Txt_SEN_GINKOCD.ExCodeDB);
                        }
                        //Set_dtRIREKI(0, 0, "SEN_GINKOCD", 2, "専用入金口座　銀行コード", sSenGin_Old, sSenGin_New);
                        Set_dtRIREKI(3, 0, "SEN_GINKOC", 2, "専用入金口座　銀行コード", sSenGin_Old, sSenGin_New);
                    }

                    if (Global.SEN_SITENCD != Tb2_Txt_SEN_SITENCD.ExCode)
                    {
                        //Set_dtRIREKI(0, 0, "SEN_SITENCD", 2, "専用入金口座　支店コード", Global.SEN_SITENCD, Tb2_Txt_SEN_SITENCD.ExCode);
                        Set_dtRIREKI(3, 0, "SEN_SITENC", 2, "専用入金口座　支店コード", Global.SEN_SITENCD, Tb2_Txt_SEN_SITENCD.ExCode);
                    }

                    if (Global.SEN_SITENCD != Tb2_Txt_SEN_SITENCD.ExCode)
                    {
                        //Set_dtRIREKI(0, 0, "SEN_SITENCD", 2, "専用入金口座　支店コード", Global.SEN_SITENCD, Tb2_Txt_SEN_SITENCD.ExCode);
                        Set_dtRIREKI(3, 0, "SEN_SITENC", 2, "専用入金口座　支店コード", Global.SEN_SITENCD, Tb2_Txt_SEN_SITENCD.ExCode);
                    }

                    if (Global.SEN_KOZANO.Length == 3 || Global.SEN_KOZANO.Length == 10)
                    {
                        string sSenKSitten = Global.SEN_KOZANO.Substring(0, 3);
                        if (sSenKSitten != Tb2_Txt_SEN_KSITENCD.ExCodeDB)
                        {
                            //Set_dtRIREKI(0, 0, "SEN_KSITENCD", 2, "専用入金口座　仮想支店コード", sSenKSitten, Tb2_Txt_SEN_KSITENCD.ExCode);
                            Set_dtRIREKI(3, 0, "SEN_KSITEN", 2, "専用入金口座　仮想支店コード", sSenKSitten, Tb2_Txt_SEN_KSITENCD.ExCode);
                        }
                    }

                    if (Global.KASO_SITENNM != Tb2_Txt_SEN_KSITENNM.Text)
                    {
                        //Set_dtRIREKI(0, 0, "SEN_SHITENMEI", 2, "専用入金口座　仮想支店名", Global.KASO_SITENNM, Tb2_Txt_SEN_KSITENNM.Text);
                        Set_dtRIREKI(3, 0, "SEN_SHITEN", 2, "専用入金口座　仮想支店名", Global.KASO_SITENNM, Tb2_Txt_SEN_KSITENNM.Text);
                    }

                    if (Global.YOKINSYU != mcBsLogic.Get_Sen_YokinType_CD(Tb2_Cmb_YOKINSYU.Text))
                    {
                        string sYokinTyp_DB = "";
                        if (Global.YOKINSYU != "")
                        {
                            sYokinTyp_DB = Global.YOKIN_TYP_tb2 + ":" + mcBsLogic.Get_Sen_YokinType_NM(int.Parse(Global.YOKINSYU));
                        }
                        Set_dtRIREKI(3, 0, "YOKINSYU", 2, "専用入金口座　預金種別", sYokinTyp_DB, Tb2_Cmb_YOKINSYU.Text);
                    }

                    if (Global.SEN_KOZANO.Length == 7 || Global.SEN_KOZANO.Length == 10)
                    {
                        string sSenKSitten = Global.SEN_KOZANO.Substring(Global.SEN_KOZANO.Length - 7, 7);
                        if (sSenKSitten != Tb2_Txt_SEN_KOZANO.ExCodeDB)
                        {
                            //Set_dtRIREKI(0, 0, "SEN_KSITENCD", 2, "専用入金口座　口座番号", sSenKSitten, Tb2_Txt_SEN_KOZANO.ExCode);
                            Set_dtRIREKI(3, 0, "SEN_KSITEN", 2, "専用入金口座　口座番号", sSenKSitten, Tb2_Txt_SEN_KOZANO.ExCode);
                        }
                    }
                    #endregion

                    #region 各設定
                    sSW = "";
                    if (Tb2_Chk_JIDOU_GAKUSYU.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.JIDOU_GAKUSYU != sSW)
                    {
                        //Set_dtRIREKI(0, 0, "JIDOU_GAKUSYU", 2, "カナ自動学習", mcBsLogic.Get_SetteiNM(Global.JIDOU_GAKUSYU), mcBsLogic.Get_SetteiNM(sSW));
                        Set_dtRIREKI(3, 0, "JIDOU_GAKU", 2, "カナ自動学習", mcBsLogic.Get_SetteiNM(Global.JIDOU_GAKUSYU), mcBsLogic.Get_SetteiNM(sSW));
                    }

                    sSW = "";
                    if (Tb2_Chk_NYUKIN_YOTEI.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.NYUKIN_YOTEI != sSW)
                    {
                        //Set_dtRIREKI(0, 0, "NYUKIN_YOTEI", 2, "入金予定利用", mcBsLogic.Get_SetteiNM(Global.NYUKIN_YOTEI), mcBsLogic.Get_SetteiNM(sSW));
                        Set_dtRIREKI(3, 0, "NYUKIN_YOT", 2, "入金予定利用", mcBsLogic.Get_SetteiNM(Global.NYUKIN_YOTEI), mcBsLogic.Get_SetteiNM(sSW));
                    }

                    sSW = "";
                    if (Tb2_Chk_RYOSYUSYO.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.RYOSYUSYO != sSW)
                    {
                        Set_dtRIREKI(3, 0, "RYOSYUSYO", 2, "領収書発行", mcBsLogic.Get_SetteiNM(Global.RYOSYUSYO), mcBsLogic.Get_SetteiNM(sSW));
                    }

                    sSW = "";
                    if (Tb2_Chk_TESURYO_GAKUSYU.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.TESURYO_GAKUSYU != sSW)
                    {
                        //Set_dtRIREKI(0, 0, "TESURYO_GAKUSYU", 2, "手数料自動学習", mcBsLogic.Get_SetteiNM(Global.TESURYO_GAKUSYU), mcBsLogic.Get_SetteiNM(sSW));
                        Set_dtRIREKI(3, 0, "TESURYO_GA", 2, "手数料自動学習", mcBsLogic.Get_SetteiNM(Global.TESURYO_GAKUSYU), mcBsLogic.Get_SetteiNM(sSW));
                    }

                    sSW = "";
                    if (Tb2_Chk_TESURYO_GOSA.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.TESURYO_GOSA != sSW)
                    {
                        //Set_dtRIREKI(0, 0, "TESURYO_GOSA", 2, "手数料誤差利用", mcBsLogic.Get_SetteiNM(Global.TESURYO_GOSA), mcBsLogic.Get_SetteiNM(sSW));
                        Set_dtRIREKI(3, 0, "TESURYO_GO", 2, "手数料誤差利用", mcBsLogic.Get_SetteiNM(Global.TESURYO_GOSA), mcBsLogic.Get_SetteiNM(sSW));
                    }

                    if (Global.SHIN_KAISYACD != Tb2_Txt_SHIN_KAISYACD.Text)
                    {
                        //Set_dtRIREKI(0, 0, "SHIN_KAISYACD", 2, "信用調査用企業コード", Global.SHIN_KAISYACD, Tb2_Txt_SHIN_KAISYACD.Text);
                        Set_dtRIREKI(3, 0, "SHIN_KAISY", 2, "信用調査用企業コード", Global.SHIN_KAISYACD, Tb2_Txt_SHIN_KAISYACD.Text);
                    }

                    string sY_Yoshin_Disp = "0";
                    string sY_Yoshin_DB = "0";
                    if (Tb2_Txt_YOSIN.ExNumValue != 0)
                    {
                        sY_Yoshin_Disp = Tb2_Txt_YOSIN.ExNumValue.ToString("#,##0");
                    }
                    if (Global.YOSIN != "")
                    {
                        sY_Yoshin_DB = Convert.ToInt64(Global.YOSIN).ToString("#,##0");
                    }
                    if (sY_Yoshin_DB != sY_Yoshin_Disp)
                    {
                        Set_dtRIREKI(3, 0, "YOSIN", 2, "与信限度額", sY_Yoshin_DB, sY_Yoshin_Disp);
                    }

                    if (Global.YOSHINRANK != Tb2_Txt_YOSHINRANK.Text)
                    {
                        Set_dtRIREKI(3, 0, "YOSHINRANK", 2, "与信ランク", Global.YOSHINRANK, Tb2_Txt_YOSHINRANK.Text);
                    }
                    #endregion

                    #region 外貨関連
                    sSW = "";
                    if (Tb2_Chk_GAIKA.Checked == true)
                    {
                        sSW = "1";
                    }
                    else
                    {
                        sSW = "0";
                    }
                    if (Global.GAIKA != sSW)
                    {
                        Set_dtRIREKI(3, 0, "GAIKA", 2, "外貨を使用する", mcBsLogic.Get_GaikaNM(Global.GAIKA), mcBsLogic.Get_GaikaNM(sSW));
                    }

                    if (Global.TSUKA != Tb2_Cmb_TSUKA.Text)
                    {
                        Set_dtRIREKI(3, 0, "TSUKA", 2, "取引通貨", Global.TSUKA, Tb2_Cmb_TSUKA.Text);
                    }

                    if (Global.GAIKA_KEY_F != Tb2_Txt_GAIKA_KEY_F.Text)
                    {
                        //Set_dtRIREKI(0, 0, "GAIKA_KEY_F", 2, "照合ｷｰ（前）", Global.GAIKA_KEY_F, Tb2_Txt_GAIKA_KEY_F.Text);
                        Set_dtRIREKI(3, 0, "GAIKA_KEYF", 2, "照合ｷｰ（前）", Global.GAIKA_KEY_F, Tb2_Txt_GAIKA_KEY_F.Text);
                    }

                    if (Global.GAIKA_KEY_B != Tb2_Txt_GAIKA_KEY_B.Text)
                    {
                        //Set_dtRIREKI(0, 0, "GAIKA_KEY_B", 2, "照合ｷｰ（後）", Global.GAIKA_KEY_B, Tb2_Txt_GAIKA_KEY_B.Text);
                        Set_dtRIREKI(3, 0, "GAIKA_KEYB", 2, "照合ｷｰ（後）", Global.GAIKA_KEY_B, Tb2_Txt_GAIKA_KEY_B.Text);
                    }
                    #endregion

                    #region 被振込口座設定
                    if (Global.HIFURIKOZA_1 != Tb2_Txt_HIFURIKOZA_1.Text)
                    {
                        //Set_dtRIREKI(0, 0, "HIFURIKOZA_1", 2, "被振込口座設定１", Global.HIFURIKOZA_1, Tb2_Txt_HIFURIKOZA_1.Text);
                        Set_dtRIREKI(3, 0, "HIFURIKOZA", 2, "被振込口座設定１", Global.HIFURIKOZA_1, Tb2_Txt_HIFURIKOZA_1.Text);
                    }
                    if (Global.HIFURIKOZA_2 != Tb2_Txt_HIFURIKOZA_2.Text)
                    {
                        //Set_dtRIREKI(0, 0, "HIFURIKOZA_2", 2, "被振込口座設定２", Global.HIFURIKOZA_2, Tb2_Txt_HIFURIKOZA_2.Text);
                        Set_dtRIREKI(3, 0, "HIFURIKOZA", 2, "被振込口座設定２", Global.HIFURIKOZA_2, Tb2_Txt_HIFURIKOZA_2.Text);
                    }
                    if (Global.HIFURIKOZA_3 != Tb2_Txt_HIFURIKOZA_3.Text)
                    {
                        //Set_dtRIREKI(0, 0, "HIFURIKOZA_3", 2, "被振込口座設定３", Global.HIFURIKOZA_3, Tb2_Txt_HIFURIKOZA_3.Text);
                        Set_dtRIREKI(3, 0, "HIFURIKOZA", 2, "被振込口座設定３", Global.HIFURIKOZA_3, Tb2_Txt_HIFURIKOZA_3.Text);
                    }
                    #endregion

                    Global.TOKUKANA = Tb2_Txt_TOKUKANA.Text;
                    Global.FUTAN = mcBsLogic.Get_Tesuu_CD(Tb2_Cmb_FUTAN.Text);
                    Global.YAKUJYO = Tb2_Chk_YAKUJO.Checked == true ? "1" : "0";

                    Global.SHIME = Tb2_Txt_SHIME.Text == "末" ? "99" : Tb2_Txt_SHIME.Text;
//-- <9999>
//                    Global.KAISYUHI = Tb2_Txt_KAISYUHI_M.Text + (Tb2_Txt_KAISYUHI_D.Text == "末" ? "99" : Tb2_Txt_KAISYUHI_D.Text);
//                    Global.KAISYUSIGHT = Tb2_Txt_KAISYUSIGHT_M.Text + (Tb2_Txt_KAISYUSIGHT_D.Text == "末" ? "99" : Tb2_Txt_KAISYUSIGHT_D.Text);
                    if (Tb2_Txt_KAISYUHI_D.Text.Length != 0)
                    {
                        Global.KAISYUHI = Tb2_Txt_KAISYUHI_M.Text + (Tb2_Txt_KAISYUHI_D.Text.PadLeft(1, '0') == "末" ? "99" : Tb2_Txt_KAISYUHI_D.Text.PadLeft(2, '0'));
                    }
                    else
                    { Global.KAISYUHI = ""; }
                    if (Tb2_Txt_KAISYUSIGHT_D.Text.Length != 0)
                    {
                        Global.KAISYUSIGHT = Tb2_Txt_KAISYUSIGHT_M.Text.PadLeft(1, '0') + (Tb2_Txt_KAISYUSIGHT_D.Text == "末" ? "99" : Tb2_Txt_KAISYUSIGHT_D.Text.PadLeft(2, '0'));
                    }
                    else
                    { Global.KAISYUSIGHT = ""; }
//-- <9999>
                    Global.HOLIDAY = mcBsLogic.Get_Hosei_CD(Tb2_Cmb_HOLIDAY.Text);

                    if (Global.YAKUJYO == "0")
                    {
                        Global.KAISYU = "";
                        if (Tb2_Cmb_KAISYU.SelectedIndex != -1)
                        {
                            Global.KAISYU = (Tb2_Cmb_KAISYU.SelectedItem as TBLKUBUN).KUBUNCD;
                        }
                        Global.Y_KINGAKU = "";
                        Global.MIMAN = "";

                        Global.IJOU_1 = "";
                        Global.BUNKATSU_1 = "";
                        Global.HASU_1 = "";
                        Global.SIGHT_1 = "";

                        Global.IJOU_2 = "";
                        Global.BUNKATSU_2 = "";
                        Global.HASU_2 = "";
                        Global.SIGHT_2 = "";

                        Global.IJOU_3 = "";
                        Global.BUNKATSU_3 = "";
                        Global.HASU_3 = "";
                        Global.SIGHT_3 = "";
                    }
                    else
                    {
                        Global.KAISYU = "";
                        Global.Y_KINGAKU = Tb2_Txt_Y_KINGAKU.ExNumValue.ToString();
                        Global.MIMAN = (Tb2_Cmb_MIMAN.SelectedItem as TBLKUBUN).KUBUNCD;
                        Global.IJOU_1 = (Tb2_Cmb_IJOU_1.SelectedItem as TBLKUBUN).KUBUNCD;
                        Global.BUNKATSU_1 = Tb2_Txt_BUNKATSU_1.ExNumValue.ToString();
                        Global.HASU_1 = Tb2_Cmb_HASU_1.Text.Substring(0, Tb2_Cmb_HASU_1.Text.IndexOf(':'));
//-- <9999>
//                        Global.SIGHT_1 = Tb2_Txt_SIGHT_M_1.Text + (Tb2_Txt_SIGHT_D_1.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_1.Text);
//-- <2016/03/08>                
//                        if (Tb2_Txt_SIGHT_D_1.Text.Length != 0)
                        if (Tb2_Txt_SIGHT_D_1.Text.Length != 0 && mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_1.SelectedValue.ToString(), 1) == "1")
//-- <2016/03/08>
                        {
                            Global.SIGHT_1 = Tb2_Txt_SIGHT_M_1.Text.PadLeft(1, '0') + (Tb2_Txt_SIGHT_D_1.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_1.Text.PadLeft(2, '0'));
                        }
                        else
                        { Global.SIGHT_1 = ""; }
//-- <9999>

                        //if (Tb2_Cmb_IJOU_2.Text != "" && (Tb2_Cmb_IJOU_2.SelectedItem as TBLKUBUN).KUBUNCD != "")
                        if(Tb2_Cmb_IJOU_2.SelectedIndex > 0)
                        {
                            Global.IJOU_2 = (Tb2_Cmb_IJOU_2.SelectedItem as TBLKUBUN).KUBUNCD;
                            Global.BUNKATSU_2 = Tb2_Txt_BUNKATSU_2.ExNumValue.ToString();
                            Global.HASU_2 = Tb2_Cmb_HASU_2.Text.Substring(0, Tb2_Cmb_HASU_2.Text.IndexOf(':'));
//-- <9999>
//                            Global.SIGHT_2 = Tb2_Txt_SIGHT_M_2.Text + (Tb2_Txt_SIGHT_D_2.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_2.Text);

//-- <2016/03/08 >
//                            if (Tb2_Txt_SIGHT_D_2.Text.Length != 0)
                            if (Tb2_Txt_SIGHT_D_2.Text.Length != 0 && mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_2.SelectedValue.ToString(), 1) == "1")
//-- <2016/03/08>
                            {
                                Global.SIGHT_2 = Tb2_Txt_SIGHT_M_2.Text.PadLeft(1, '0') + (Tb2_Txt_SIGHT_D_2.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_2.Text.PadLeft(2, '0'));
                            }
                            else
                            { Global.SIGHT_2 = ""; }
//-- <9999>                    
                        }
//-- <2016/03/08 >
                        else
                        {
                            Global.IJOU_2 = Tb2_Cmb_IJOU_2.SelectedIndex == 0 ? "0" : "";
                            Global.BUNKATSU_2 = "";
                            Global.HASU_2 = "";
                            Global.SIGHT_2 = "";

                            Global.IJOU_3 = "";
                            Global.BUNKATSU_3 = "";
                            Global.HASU_3 = "";
                            Global.SIGHT_3 = "";
                        }
//-- <2016/03/08 >

                        //if (Tb2_Cmb_IJOU_3.Text != "" && (Tb2_Cmb_IJOU_3.SelectedItem as TBLKUBUN).KUBUNCD != "")
                        if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
                        {
                            Global.IJOU_3 = (Tb2_Cmb_IJOU_3.SelectedItem as TBLKUBUN).KUBUNCD;
                            Global.BUNKATSU_3 = Tb2_Txt_BUNKATSU_3.ExNumValue.ToString();
                            Global.HASU_3 = Tb2_Cmb_HASU_3.Text.Substring(0, Tb2_Cmb_HASU_3.Text.IndexOf(':'));
//-- <9999>
//                            Global.SIGHT_3 = Tb2_Txt_SIGHT_M_3.Text + (Tb2_Txt_SIGHT_D_3.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_3.Text);
//-- <2016/03/08 >
//                            if (Tb2_Txt_SIGHT_D_3.Text.Length != 0)
                            if (Tb2_Txt_SIGHT_D_3.Text.Length != 0 && mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_3.SelectedValue.ToString(), 1) == "1")
//-- <2016/03/08>
                            {
                                Global.SIGHT_3 = Tb2_Txt_SIGHT_M_3.Text.PadLeft(1, '0') + (Tb2_Txt_SIGHT_D_3.Text == "末" ? "99" : Tb2_Txt_SIGHT_D_3.Text.PadLeft(2, '0'));
                            }
                            else
                            { Global.SIGHT_3 = ""; }
//-- <9999>
                        }
//-- <2016/03/08 >
                        else
                        {
                            Global.IJOU_3 = Tb2_Cmb_IJOU_3.SelectedIndex == 0 ? "0" : "";
                            Global.BUNKATSU_3 = "";
                            Global.HASU_3 = "";
                            Global.SIGHT_3 = "";
                        }
//-- <2016/03/08 >
                    }

//-- <9999>
//                    Global.SEN_GINKOCD = Tb2_Txt_SEN_GINKOCD.ExCode;
//                    Global.SEN_SITENCD = Tb2_Txt_SEN_SITENCD.ExCode;
//                    Global.KASO_SITENNM = Tb2_Txt_SEN_KSITENNM.Text;
//                    Global.SEN_KOZANO = Tb2_Txt_SEN_KSITENCD.ExCode + Tb2_Txt_SEN_KOZANO.ExCode;
//                    Global.YOKINSYU = mcBsLogic.Get_Sen_YokinType_CD(Tb2_Cmb_YOKINSYU.Text);
                    if (Tb2_Txt_SEN_GINKOCD.Text.Length != 0)
                    {
                        Global.SEN_GINKOCD = Tb2_Txt_SEN_GINKOCD.ExCode;
                        Global.SEN_SITENCD = Tb2_Txt_SEN_SITENCD.ExCode;
                        Global.KASO_SITENNM = Tb2_Txt_SEN_KSITENNM.Text;
                        Global.SEN_KOZANO = Tb2_Txt_SEN_KSITENCD.ExCode + Tb2_Txt_SEN_KOZANO.ExCode;
                        Global.YOKINSYU = mcBsLogic.Get_Sen_YokinType_CD(Tb2_Cmb_YOKINSYU.Text);
                    }
                    else
                    {
                        Global.SEN_GINKOCD = "";
                        Global.SEN_SITENCD = "";
                        Global.KASO_SITENNM = "";
                        Global.SEN_KOZANO = "";
                        Global.YOKINSYU = "";
                    }
//-- <9999>
                    Global.JIDOU_GAKUSYU = Cbo_SAIKEN.SelectedValue.ToString() == sUse ? (Tb2_Chk_JIDOU_GAKUSYU.Checked == true ? "1" : "0") : "";
                    Global.NYUKIN_YOTEI = (Tb2_Chk_NYUKIN_YOTEI.Checked == true ? "1" : "0");
                    Global.TESURYO_GAKUSYU = (Tb2_Chk_TESURYO_GAKUSYU.Checked == true ? "1" : "0");
                    Global.TESURYO_GOSA = Cbo_SAIKEN.SelectedValue.ToString() == sUse ? (Tb2_Chk_TESURYO_GOSA.Checked == true ? "1" : "0") : "";
                    Global.RYOSYUSYO = (Tb2_Chk_RYOSYUSYO.Checked == true ? "1" : "0");
                    Global.SHIN_KAISYACD = Tb2_Txt_SHIN_KAISYACD.Text;
                    Global.YOSIN = Cbo_SAIKEN.SelectedValue.ToString() == sUse ? Tb2_Txt_YOSIN.ExNumValue.ToString() : "";
                    Global.YOSHINRANK = Tb2_Txt_YOSHINRANK.Text;

                    Global.GAIKA = (Tb2_Chk_GAIKA.Checked == true ? "1" : "0");
                    Global.TSUKA = Tb2_Cmb_TSUKA.Text;
                    Global.GAIKA_KEY_F = Tb2_Txt_GAIKA_KEY_F.Text;
                    Global.GAIKA_KEY_B = Tb2_Txt_GAIKA_KEY_B.Text;

                    if (Tb2_Txt_HIFURIKOZA_1.Text != "")
                    {
                        Global.HIFURIKOZA_1 = Tb2_Txt_HIFURIKOZA_1.Text;
                    }
                    else
                    {
                        Global.HIFURIKOZA_1 = "";
                    }

                    if (Tb2_Txt_HIFURIKOZA_2.Text != "")
                    {
                        Global.HIFURIKOZA_2 = Tb2_Txt_HIFURIKOZA_2.Text;
                    }
                    else
                    {
                        Global.HIFURIKOZA_2 = "";
                    }

                    if (Tb2_Txt_HIFURIKOZA_3.Text != "")
                    {
                        Global.HIFURIKOZA_3 = Tb2_Txt_HIFURIKOZA_3.Text;
                    }
                    else
                    {
                        Global.HIFURIKOZA_3 = "";
                    }
//-- <2016/03/09 トラップ追加>
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb2_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
//-- <2016/03/09>
        }


        /// <summary>
        /// 支払条件タブの画面情報を取得
        /// </summary>
        private void Get_Tb3_Data()
        {
            try
            {
                //履歴関連 ＠2011/07 履歴対応
                //変更判定&dtRIREKIへの格納

                string sGAIKA = "";
                if (Tb3_Rdo_GAI_F0.Checked == true)
                {
                    sGAIKA = "0";
                }
                else
                {
                    sGAIKA = "1";
                }
                if (Global.GAI_F != sGAIKA)
                {
                    Set_dtRIREKI(5, 0, "GAI_F", 2, "取引区分", mcBsLogic.Get_TorihikiNM(Global.GAI_F), mcBsLogic.Get_TorihikiNM(sGAIKA));
                }

                string sBCOD = "0";
                if (Tb3_Txt_BCOD.ExCodeDB != "")
                {
                    if (Tb3_Txt_BCOD.ExCodeDB == "0")
                    {
                        sBCOD = "0";
                    }
                    else if (Global.nBCOD_Type == 0)
                    {
                        sBCOD = Tb3_Txt_BCOD.ExCodeDB.PadLeft(Global.nBCOD_Len, '0');
                    }
                    else
                    {
                        sBCOD = Tb3_Txt_BCOD.ExCodeDB.PadRight(Global.nBCOD_Len, ' ');
                    }
                }
                if (Global.BCOD_tb1 != sBCOD)
                {
                    string sBCOD_DB = "";
                    string sBCOD_Disp = "";
                    if (Global.BCOD_tb1 != "0")
                    {
                        sBCOD_DB = Global.BCOD_tb1 + ":" + mcBsLogic.Get_BNAM(Global.BCOD_tb1);
                    }
                    else
                    {
                        sBCOD_DB = "0:全て";
                    }
                    if (sBCOD != "0")
                    {
                        sBCOD_Disp = sBCOD + ":" + mcBsLogic.Get_BNAM(sBCOD);
                    }
                    else
                    {
                        sBCOD_Disp = "0:全て";
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "BCOD", 2, "発生部門", sBCOD_DB, sBCOD_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "BCOD", 2, "発生部門", sBCOD_DB, sBCOD_Disp);
                    }
                    //**<<ICS-E
                }

                string sKICD = "0";
                if (Tb3_Txt_KCOD.ExCodeDB != "")
                {
                    //sKICD = (Tb1_Txt_KCOD.Text == "0" ? "0" : mcBsLogic.Conv_KCODtoKICD(Tb1_Txt_KCOD.Text));
                    sKICD = mcBsLogic.Conv_KCODtoKICD(Global.nKCOD_Type == 0 ? Tb3_Txt_KCOD.ExCodeDB.PadLeft(Global.nKCOD_Len, '0') : Tb3_Txt_KCOD.ExCodeDB.PadRight(Global.nKCOD_Len));
                }
                if (Global.KICD_tb1 != sKICD)
                {
                    string sKICD_DB = "";
                    string sKICD_Disp = "";
                    if (Global.KICD_tb1 != "0")
                    {
                        sKICD_DB = mcBsLogic.Conv_KICDtoKCOD(Global.KICD_tb1) + ":" + mcBsLogic.Get_KNAM(mcBsLogic.Conv_KICDtoKCOD(Global.KICD_tb1));
                        if (sKICD_DB == ":")
                        {
                            sKICD_DB = "";
                        }
                    }
                    else
                    {
                        sKICD_DB = "0:全て";
                    }
                    if (sKICD != "0")
                    {
                        sKICD_Disp = mcBsLogic.Conv_KICDtoKCOD(sKICD) + ":" + mcBsLogic.Get_KNAM(mcBsLogic.Conv_KICDtoKCOD(sKICD));
                        if (sKICD_Disp == ":")
                        {
                            sKICD_Disp = "";
                        }
                    }
                    else
                    {
                        sKICD_Disp = "0:全て";
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "KICD", 2, "発生科目", sKICD_DB, sKICD_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "KICD", 2, "発生科目", sKICD_DB, sKICD_Disp);
                    }
                    //**<<ICS-E
                }

                //if (Global.SHINO_tb1 != Tb1_Txt_SHINO.Text)
                if (Tb3_Txt_SHINO.Text != (string.IsNullOrEmpty(Global.SHINO_tb1) ? "" : Global.SHINO_tb1.PadLeft(3, '0')))
                {
                    string sShino_DB = "";
                    string sShino_Disp = "";
                    //if (Global.SHINO_tb1 != "" && Global.SHINO_tb1 != null)
                    if (!string.IsNullOrEmpty(Global.SHINO_tb1))
                    {
                        sShino_DB = Global.SHINO_tb1.PadLeft(3, '0') + ":" + mcBsLogic.Get_SHINM(Global.SHINO_tb1);
                    }
                    if (Tb3_Txt_SHINO.Text != "")
                    {
                        sShino_Disp = Tb3_Txt_SHINO.Text + ":" + mcBsLogic.Get_SHINM(Tb3_Txt_SHINO.Text);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SHINO", 2, "支払方法", sShino_DB, sShino_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SHINO", 2, "支払方法", sShino_DB, sShino_Disp);
                    }
                    //**<<ICS-E
                }

                string sHarai_Before = "";
                string ｓKijitu_Before = "";
                mcBsLogic.Get_HoseiDT(Global.TRCD, Global.HJCD, Tb1_Lbl_SHO_ID_V.Text, out sHarai_Before, out ｓKijitu_Before);

                if (sHarai_Before != mcBsLogic.Get_Hosei_CD(Tb3_Cmb_HARAI_H.Text))
                {
                    string sHarai_DB = "";
                    if (sHarai_Before != "")
                    {
                        sHarai_DB = sHarai_Before + ":" + mcBsLogic.Get_Hosei_NM(int.Parse(sHarai_Before));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "HARAI_H", 2, "休日補正(支払日)", sHarai_DB, Tb1_Cmb_HARAI_H.Text);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "HARAI_H", 2, "休日補正(支払日)", sHarai_DB, Tb3_Cmb_HARAI_H.Text);
                    }
                    //**<<ICS-E
                }
                if (ｓKijitu_Before != mcBsLogic.Get_Hosei_CD(Tb3_Cmb_KIJITU_H.Text))
                {
                    string sKijitu_DB = "";
                    if (ｓKijitu_Before != "")
                    {
                        sKijitu_DB = ｓKijitu_Before + ":" + mcBsLogic.Get_Hosei_NM(int.Parse(ｓKijitu_Before));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "KIJITU_H", 2, "休日補正(支払期日)", sKijitu_DB, Tb1_Cmb_KIJITU_H.Text);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "KIJITU_H", 2, "休日補正(支払期日)", sKijitu_DB, Tb3_Cmb_KIJITU_H.Text);
                    }
                    //**<<ICS-E
                }

                //Globalへの画面情報の格納
                Global.SHO_ID_tb1 = Tb1_Lbl_SHO_ID_V.Text;
                Global.BCOD_tb1 = sBCOD;
                Global.KICD_tb1 = sKICD;
                Global.SHINO_tb1 = (Tb3_Txt_SHINO.Text != "" ? Tb3_Txt_SHINO.Text : null);
                Global.HARAI_H_tb1 = mcBsLogic.Get_Hosei_CD(Tb3_Cmb_HARAI_H.Text);
                Global.KIJITU_H_tb1 = mcBsLogic.Get_Hosei_CD(Tb3_Cmb_KIJITU_H.Text);


                string[,] sTRCD_Tb4Array = null;
                string sSKUBN1 = ""; //, sBANK1 = "", sSITEN1 = "", sKOZATYP1 = "", sKOZANO1 = "", sIRAININ1 = "";
                string sSKUBN2 = ""; //, sBANK2 = "", sSITEN2 = "", sKOZATYP2 = "", sKOZANO2 = "", sIRAININ2 = "";
                string sSKUBN3 = ""; //, sBANK3 = "", sSITEN3 = "", sKOZATYP3 = "", sKOZANO3 = "", sIRAININ3 = "";
                string sSKUBN4 = ""; //, sBANK4 = "", sSITEN4 = "", sKOZATYP4 = "", sKOZANO4 = "", sIRAININ4 = "";
                string sKozaId1 = "", sKozaId2 = "", sKozaId3 = "", sKozaId4 = "";
                mcBsLogic.Get_TRCD_Tb4(Global.TRCD, Global.HJCD, Tb1_Lbl_SHO_ID_V.Text, out sTRCD_Tb4Array);

                //支払区分が値を持つ場合、1行目の内容を格納
                if (Tb3_Lbl_HARAI_KBN1.Text != "")
                {
                    #region omit
                    //sSKUBN1 = Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':'));
                    //if (Tb3_Cmb_HARAI_KBN1.SelectedIndex != -1)
                    //{
                    //    sBANK1 = sBKNAMArray[Tb3_Cmb_HARAI_KBN1.SelectedIndex, 0];
                    //    sSITEN1 = sBRNAMArray[Tb3_Cmb_HARAI_KBN1.SelectedIndex, 0];
                    //    sKOZATYP1 = sYKNKINDArray[Tb3_Cmb_HARAI_KBN1.SelectedIndex, 0];
                    //    sKOZANO1 = sKOZANOArray[Tb3_Cmb_HARAI_KBN1.SelectedIndex];
                    //    sIRAININ1 = sIRAININArray[Tb3_Cmb_HARAI_KBN1.SelectedIndex];
                    //}
                    #endregion
                    var list = Tb3_Cmb_HARAI_KBN1.DataSource as List<KeyValuePair<int, OwnBank>>;
                    sSKUBN1 = Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':')).Trim();
                    if (Tb3_Cmb_HARAI_KBN1.SelectedIndex != -1 && list != null)
                    {
                        int index = Tb3_Cmb_HARAI_KBN1.SelectedIndex;

                        if (mcBsLogic.Get_SKBKIND(sSKUBN2) != "8")
                        {
                            sKozaId1 = list[index].Value.OwnId;
                        }
                        else
                        {
                            sKozaId1 = list[index].Value.FacId;
                        }
                    }
                }

                //支払区分が値を持つ場合、2行目の内容を格納
                if (Tb3_Lbl_HARAI_KBN2.Text != "")
                {
                    #region omit
                    //sSKUBN2 = Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':'));
                    //if (Tb3_Cmb_HARAI_KBN2.SelectedIndex != -1)
                    //{
                    //    sBANK2 = sBKNAMArray[Tb3_Cmb_HARAI_KBN2.SelectedIndex, 0];
                    //    sSITEN2 = sBRNAMArray[Tb3_Cmb_HARAI_KBN2.SelectedIndex, 0];
                    //    sKOZATYP2 = sYKNKINDArray[Tb3_Cmb_HARAI_KBN2.SelectedIndex, 0];
                    //    sKOZANO2 = sKOZANOArray[Tb3_Cmb_HARAI_KBN2.SelectedIndex];
                    //    sIRAININ2 = sIRAININArray[Tb3_Cmb_HARAI_KBN2.SelectedIndex];
                    //}
                    #endregion
                    var list = Tb3_Cmb_HARAI_KBN2.DataSource as List<KeyValuePair<int, OwnBank>>;
                    sSKUBN2 = Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':')).Trim();
                    if (Tb3_Cmb_HARAI_KBN2.SelectedIndex != -1 && list != null)
                    {
                        int index = Tb3_Cmb_HARAI_KBN2.SelectedIndex;

                        if (mcBsLogic.Get_SKBKIND(sSKUBN2) != "8")
                        {
                            sKozaId2 = list[index].Value.OwnId;
                        }
                        else
                        {
                            sKozaId2 = list[index].Value.FacId;
                        }
                    }
                }

                //支払区分が値を持つ場合、3行目の内容を格納
                if (Tb3_Lbl_HARAI_KBN3.Text != "")
                {
                    #region omit
                    //sSKUBN3 = Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':'));
                    //if (Tb3_Cmb_HARAI_KBN3.SelectedIndex != -1)
                    //{
                    //    sBANK3 = sBKNAMArray[Tb3_Cmb_HARAI_KBN3.SelectedIndex, 0];
                    //    sSITEN3 = sBRNAMArray[Tb3_Cmb_HARAI_KBN3.SelectedIndex, 0];
                    //    sKOZATYP3 = sYKNKINDArray[Tb3_Cmb_HARAI_KBN3.SelectedIndex, 0];
                    //    sKOZANO3 = sKOZANOArray[Tb3_Cmb_HARAI_KBN3.SelectedIndex];
                    //    sIRAININ3 = sIRAININArray[Tb3_Cmb_HARAI_KBN3.SelectedIndex];
                    //}
                    #endregion
                    var list = Tb3_Cmb_HARAI_KBN3.DataSource as List<KeyValuePair<int, OwnBank>>;
                    sSKUBN3 = Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':')).Trim();
                    if (Tb3_Cmb_HARAI_KBN3.SelectedIndex != -1 && list != null)
                    {
                        int index = Tb3_Cmb_HARAI_KBN3.SelectedIndex;

                        if (mcBsLogic.Get_SKBKIND(sSKUBN2) != "8")
                        {
                            sKozaId3 = list[index].Value.OwnId;
                        }
                        else
                        {
                            sKozaId3 = list[index].Value.FacId;
                        }
                    }
                }

                //支払区分が値を持つ場合、4行目の内容を格納
                if (Tb3_Lbl_HARAI_KBN4.Text != "")
                {
                    #region omit
                    //sSKUBN4 = Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':'));
                    //if (Tb3_Cmb_HARAI_KBN4.SelectedIndex != -1)
                    //{
                    //    sBANK4 = sBKNAMArray[Tb3_Cmb_HARAI_KBN4.SelectedIndex, 0];
                    //    sSITEN4 = sBRNAMArray[Tb3_Cmb_HARAI_KBN4.SelectedIndex, 0];
                    //    sKOZATYP4 = sYKNKINDArray[Tb3_Cmb_HARAI_KBN4.SelectedIndex, 0];
                    //    sKOZANO4 = sKOZANOArray[Tb3_Cmb_HARAI_KBN4.SelectedIndex];
                    //    sIRAININ4 = sIRAININArray[Tb3_Cmb_HARAI_KBN4.SelectedIndex];
                    //}
                    #endregion
                    var list = Tb3_Cmb_HARAI_KBN4.DataSource as List<KeyValuePair<int, OwnBank>>;
                    sSKUBN4 = Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':')).Trim();
                    if (Tb3_Cmb_HARAI_KBN4.SelectedIndex != -1 && list != null)
                    {
                        int index = Tb3_Cmb_HARAI_KBN4.SelectedIndex;

                        if (mcBsLogic.Get_SKBKIND(sSKUBN2) != "8")
                        {
                            sKozaId4 = list[index].Value.OwnId;
                        }
                        else
                        {
                            sKozaId4 = list[index].Value.FacId;
                        }
                    }
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(1行目.支払区分)
                if (sTRCD_Tb4Array[0, 0] != sSKUBN1)
                {
                    string sSkubn_DB = "";
                    string sSkubn_Disp = "";
                    if (sTRCD_Tb4Array[0, 0] != "")
                    {
                        sSkubn_DB = sTRCD_Tb4Array[0, 0] + ":" + mcBsLogic.Get_SKUBN(sTRCD_Tb4Array[0, 0]);
                    }
                    if (sSKUBN1 != "")
                    {
                        sSkubn_Disp = sSKUBN1 + ":" + mcBsLogic.Get_SKUBN(sSKUBN1);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN1", 2, "支払区分1", sSkubn_DB, sSkubn_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN1", 2, "支払区分1", sSkubn_DB, sSkubn_Disp);
                    }
                    //**<<ICS-E
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(1行目.口座ID)
                //if (sTRCD_Tb4Array[0, 1] != sKozaId1)
                //{
                //    string sOwnId_DB = "";
                //    if (sTRCD_Tb4Array[0, 1] != "")
                //    {
                //        sOwnId_DB = sTRCD_Tb4Array[0, 1];
                //    }
                //    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                //    {
                //        Set_dtRIREKI(3, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID1", 2, "口座ID/ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ1", sOwnId_DB, sKozaId1);
                //    }
                //}
                string sOWNIDOld1 = "";
                string sFACIDOld1 = "";
                string sOWNIDNew1 = "";
                string sFACIDNew1 = "";
                string sSKB1 = mcBsLogic.Get_SKBKIND(sTRCD_Tb4Array[0, 0]);
                if (sSKB1 != "8")
                {
                    sOWNIDOld1 = sTRCD_Tb4Array[0, 1];
                }
                else
                {
                    sFACIDOld1 = sTRCD_Tb4Array[0, 1];
                }
                if (sSKUBN1 != "8")
                {
                    sOWNIDNew1 = sKozaId1;
                }
                else
                {
                    sFACIDNew1 = sKozaId1;
                }
                if (sOWNIDOld1 != sOWNIDNew1)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID1", 2, "口座ID1", sOWNIDOld1, sOWNIDNew1);
                    }
                }
                if (sFACIDOld1 != sFACIDNew1)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "FACID1", 2, "ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ1", sFACIDOld1, sFACIDNew1);
                    }
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(2行目.支払区分)
                if (sTRCD_Tb4Array[1, 0] != sSKUBN2)
                {
                    string sSkubn_DB = "";
                    string sSkubn_Disp = "";
                    if (sTRCD_Tb4Array[1, 0] != "")
                    {
                        sSkubn_DB = sTRCD_Tb4Array[1, 0] + ":" + mcBsLogic.Get_SKUBN(sTRCD_Tb4Array[1, 0]);
                    }
                    if (sSKUBN2 != "")
                    {
                        sSkubn_Disp = sSKUBN2 + ":" + mcBsLogic.Get_SKUBN(sSKUBN2);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN2", 2, "支払区分2", sSkubn_DB, sSkubn_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN2", 2, "支払区分2", sSkubn_DB, sSkubn_Disp);
                    }
                    //**<<ICS-E
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(2行目.口座ID)
                //if (sTRCD_Tb4Array[1, 1] != sKozaId2)
                //{
                //    string sOwnId_DB = "";
                //    if (sTRCD_Tb4Array[1, 1] != "")
                //    {
                //        sOwnId_DB = sTRCD_Tb4Array[1, 1];
                //    }
                //    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                //    {
                //        Set_dtRIREKI(3, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID2", 2, "口座ID/ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ2", sOwnId_DB, sKozaId2);
                //    }
                //}
                string sOWNIDOld2 = "";
                string sFACIDOld2 = "";
                string sOWNIDNew2 = "";
                string sFACIDNew2 = "";
                string sSKB2 = mcBsLogic.Get_SKBKIND(sTRCD_Tb4Array[1, 0]);
                if (sSKB2 != "8")
                {
                    sOWNIDOld2 = sTRCD_Tb4Array[1, 1];
                }
                else
                {
                    sFACIDOld2 = sTRCD_Tb4Array[1, 1];
                }
                if (sSKUBN2 != "8")
                {
                    sOWNIDNew2 = sKozaId2;
                }
                else
                {
                    sFACIDNew2 = sKozaId2;
                }
                if (sOWNIDOld2 != sOWNIDNew2)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID2", 2, "口座ID2", sOWNIDOld2, sOWNIDNew2);
                    }
                }
                if (sFACIDOld2 != sFACIDNew2)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "FACID2", 2, "ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ2", sFACIDOld2, sFACIDNew2);
                    }
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(3行目.支払区分)
                if (sTRCD_Tb4Array[2, 0] != sSKUBN3)
                {
                    string sSkubn_DB = "";
                    string sSkubn_Disp = "";
                    if (sTRCD_Tb4Array[2, 0] != "")
                    {
                        sSkubn_DB = sTRCD_Tb4Array[2, 0] + ":" + mcBsLogic.Get_SKUBN(sTRCD_Tb4Array[2, 0]);
                    }
                    if (sSKUBN3 != "")
                    {
                        sSkubn_Disp = sSKUBN3 + ":" + mcBsLogic.Get_SKUBN(sSKUBN3);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN3", 2, "支払区分3", sSkubn_DB, sSkubn_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN3", 2, "支払区分3", sSkubn_DB, sSkubn_Disp);
                    }
                    //**<<ICS-E
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(3行目.口座ID)
                //if (sTRCD_Tb4Array[2, 1] != sKozaId3)
                //{
                //    string sOwnId_DB = "";
                //    if (sTRCD_Tb4Array[2, 1] != "")
                //    {
                //        sOwnId_DB = sTRCD_Tb4Array[2, 1];
                //    }
                //    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                //    {
                //        Set_dtRIREKI(3, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID3", 2, "口座ID/ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ3", sOwnId_DB, sKozaId3);
                //    }
                //}
                string sOWNIDOld3 = "";
                string sFACIDOld3 = "";
                string sOWNIDNew3 = "";
                string sFACIDNew3 = "";
                string sSKB3 = mcBsLogic.Get_SKBKIND(sTRCD_Tb4Array[2, 0]);
                if (sSKB3 != "8")
                {
                    sOWNIDOld3 = sTRCD_Tb4Array[2, 1];
                }
                else
                {
                    sFACIDOld3 = sTRCD_Tb4Array[2, 1];
                }
                if (sSKUBN3 != "8")
                {
                    sOWNIDNew3 = sKozaId3;
                }
                else
                {
                    sFACIDNew3 = sKozaId3;
                }
                if (sOWNIDOld3 != sOWNIDNew3)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID3", 2, "口座ID3", sOWNIDOld3, sOWNIDNew3);
                    }
                }
                if (sFACIDOld3 != sFACIDNew3)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "FACID3", 2, "ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ3", sFACIDOld3, sFACIDNew3);
                    }
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(4行目.支払区分)
                if (sTRCD_Tb4Array[3, 0] != sSKUBN4)
                {
                    string sSkubn_DB = "";
                    string sSkubn_Disp = "";
                    if (sTRCD_Tb4Array[3, 0] != "")
                    {
                        sSkubn_DB = sTRCD_Tb4Array[3, 0] + ":" + mcBsLogic.Get_SKUBN(sTRCD_Tb4Array[3, 0]);
                    }
                    if (sSKUBN4 != "")
                    {
                        sSkubn_Disp = sSKUBN4 + ":" + mcBsLogic.Get_SKUBN(sSKUBN4);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(2, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN4", 2, "支払区分4", sSkubn_DB, sSkubn_Disp);
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "SI_KUBN4", 2, "支払区分4", sSkubn_DB, sSkubn_Disp);
                    }
                    //**<<ICS-E
                }

                //DBの値と画面の値を比較し、差分があれば変更履歴を積む(3行目.口座ID)
                //if (sTRCD_Tb4Array[3, 1] != sKozaId4)
                //{
                //    string sOwnId_DB = "";
                //    if (sTRCD_Tb4Array[3, 1] != "")
                //    {
                //        sOwnId_DB = sTRCD_Tb4Array[3, 1];
                //    }
                //    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                //    {
                //        Set_dtRIREKI(3, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID4", 2, "口座ID/ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ4", sOwnId_DB, sKozaId4);
                //    }
                //}
                string sOWNIDOld4 = "";
                string sFACIDOld4 = "";
                string sOWNIDNew4 = "";
                string sFACIDNew4 = "";
                string sSKB4 = mcBsLogic.Get_SKBKIND(sTRCD_Tb4Array[3, 0]);
                if (sSKB3 != "8")
                {
                    sOWNIDOld4 = sTRCD_Tb4Array[3, 1];
                }
                else
                {
                    sFACIDOld4 = sTRCD_Tb4Array[3, 1];
                }
                if (sSKUBN4 != "8")
                {
                    sOWNIDNew4 = sKozaId4;
                }
                else
                {
                    sFACIDNew4 = sKozaId4;
                }
                if (sOWNIDOld4 != sOWNIDNew4)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "OWNID4", 2, "口座ID4", sOWNIDOld4, sOWNIDNew4);
                    }
                }
                if (sFACIDOld4 != sFACIDNew4)
                {
                    if (Tb3_Lbl_Old_New2.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "FACID4", 2, "ﾌｧｸﾀﾘﾝｸﾞ会社ｺｰﾄﾞ4", sFACIDOld4, sFACIDNew4);
                    }
                }

                //Globalへの画面データ格納(1行目)
                if (Tb3_Lbl_HARAI_KBN1.Text != "")
                {
                    Global.KUBN1_tb3 = Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':'));
                    if (Tb3_Cmb_HARAI_KBN1.SelectedIndex != -1)
                    {
                        if (mcBsLogic.Get_SKBKIND(Global.KUBN1_tb3) != "8")
                        {
                            Global.OWNID1_tb3 = sOWNIDArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString())];
                        }
                        else
                        {
                            Global.OWNID1_tb3 = sFACIDArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString())];
                        }
                        Global.BANK1_tb3 = sBKNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString()), 0];
                        Global.SITEN1_tb3 = sBRNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString()), 0];
                        Global.KOZA1_tb3 = sYKNKINDArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString()), 0];
                        Global.KOZANO1_tb3 = sKOZANOArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString())];
                        //Global.IRAININ1_tb3 = sIRAININArray[int.Parse(Tb3_Cmb_HARAI_KBN1.SelectedValue.ToString())];
                    }
                    else
                    {
                        Global.OWNID1_tb3 = null;
                        Global.BANK1_tb3 = null;
                        Global.SITEN1_tb3 = null;
                        Global.KOZA1_tb3 = null;
                        Global.KOZANO1_tb3 = null;
                        Global.IRAININ1_tb3 = null;
                    }
                }
                else
                {
                    Global.OWNID1_tb3 = null;
                    Global.KUBN1_tb3 = null;
                    Global.BANK1_tb3 = null;
                    Global.SITEN1_tb3 = null;
                    Global.KOZA1_tb3 = null;
                    Global.KOZANO1_tb3 = null;
                    Global.IRAININ1_tb3 = null;
                }

                //Globalへの画面データ格納(2行目)
                if (Tb3_Lbl_HARAI_KBN2.Text != "")
                {
                    Global.KUBN2_tb3 = Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':'));
                    if (Tb3_Cmb_HARAI_KBN2.SelectedIndex != -1)
                    {
                        if (mcBsLogic.Get_SKBKIND(Global.KUBN2_tb3) != "8")
                        {
                            Global.OWNID2_tb3 = sOWNIDArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString())];
                        }
                        else
                        {
                            Global.OWNID2_tb3 = sFACIDArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString())];
                        }
                        Global.BANK2_tb3 = sBKNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString()), 0];
                        Global.SITEN2_tb3 = sBRNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString()), 0];
                        Global.KOZA2_tb3 = sYKNKINDArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString()), 0];
                        Global.KOZANO2_tb3 = sKOZANOArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString())];
                        //Global.IRAININ2_tb3 = sIRAININArray[int.Parse(Tb3_Cmb_HARAI_KBN2.SelectedValue.ToString())];
                    }
                    else
                    {
                        Global.OWNID2_tb3 = null;
                        Global.BANK2_tb3 = null;
                        Global.SITEN2_tb3 = null;
                        Global.KOZA2_tb3 = null;
                        Global.KOZANO2_tb3 = null;
                        Global.IRAININ2_tb3 = null;
                    }
                }
                else
                {
                    Global.OWNID2_tb3 = null;
                    Global.KUBN2_tb3 = null;
                    Global.BANK2_tb3 = null;
                    Global.SITEN2_tb3 = null;
                    Global.KOZA2_tb3 = null;
                    Global.KOZANO2_tb3 = null;
                    Global.IRAININ2_tb3 = null;
                }

                //Globalへの画面データ格納(3行目)
                if (Tb3_Lbl_HARAI_KBN3.Text != "")
                {
                    Global.KUBN3_tb3 = Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':'));
                    if (Tb3_Cmb_HARAI_KBN3.SelectedIndex != -1)
                    {
                        if (mcBsLogic.Get_SKBKIND(Global.KUBN3_tb3) != "8")
                        {
                            Global.OWNID3_tb3 = sOWNIDArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString())];
                        }
                        else
                        {
                            Global.OWNID3_tb3 = sFACIDArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString())];
                        }
                        Global.BANK3_tb3 = sBKNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString()), 0];
                        Global.SITEN3_tb3 = sBRNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString()), 0];
                        Global.KOZA3_tb3 = sYKNKINDArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString()), 0];
                        Global.KOZANO3_tb3 = sKOZANOArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString())];
                        //Global.IRAININ3_tb3 = sIRAININArray[int.Parse(Tb3_Cmb_HARAI_KBN3.SelectedValue.ToString())];
                    }
                    else
                    {
                        Global.OWNID3_tb3 = null;
                        Global.BANK3_tb3 = null;
                        Global.SITEN3_tb3 = null;
                        Global.KOZA3_tb3 = null;
                        Global.KOZANO3_tb3 = null;
                        Global.IRAININ3_tb3 = null;
                    }
                }
                else
                {
                    Global.OWNID3_tb3 = null;
                    Global.KUBN3_tb3 = null;
                    Global.BANK3_tb3 = null;
                    Global.SITEN3_tb3 = null;
                    Global.KOZA3_tb3 = null;
                    Global.KOZANO3_tb3 = null;
                    Global.IRAININ3_tb3 = null;
                }

                //Globalへの画面データ格納(4行目)
                if (Tb3_Lbl_HARAI_KBN4.Text != "")
                {
                    Global.KUBN4_tb3 = Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':'));
                    if (Tb3_Cmb_HARAI_KBN4.SelectedIndex != -1)
                    {
                        if (mcBsLogic.Get_SKBKIND(Global.KUBN4_tb3) != "8")
                        {
                            Global.OWNID4_tb3 = sOWNIDArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString())];
                        }
                        else
                        {
                            Global.OWNID4_tb3 = sFACIDArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString())];
                        }
                        Global.BANK4_tb3 = sBKNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString()), 0];
                        Global.SITEN4_tb3 = sBRNAMArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString()), 0];
                        Global.KOZA4_tb3 = sYKNKINDArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString()), 0];
                        Global.KOZANO4_tb3 = sKOZANOArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString())];
                        //Global.IRAININ4_tb3 = sIRAININArray[int.Parse(Tb3_Cmb_HARAI_KBN4.SelectedValue.ToString())];
                    }
                    else
                    {
                        Global.OWNID4_tb3 = null;
                        Global.BANK4_tb3 = null;
                        Global.SITEN4_tb3 = null;
                        Global.KOZA4_tb3 = null;
                        Global.KOZANO4_tb3 = null;
                        Global.IRAININ4_tb3 = null;
                    }
                }
                else
                {
                    Global.OWNID4_tb3 = null;
                    Global.KUBN4_tb3 = null;
                    Global.BANK4_tb3 = null;
                    Global.SITEN4_tb3 = null;
                    Global.KOZA4_tb3 = null;
                    Global.KOZANO4_tb3 = null;
                    Global.IRAININ4_tb3 = null;
                }

                Global.GAI_F = (Tb3_Rdo_GAI_F0.Checked == true ? "0" : "1");

                if (Tb3_Rdo_GAI_F1.Checked == true)
                {
                    Global.SHINO_tb1 = null;
                    Global.SHINO_tb3 = null;
                    Global.HARAI_H_tb1 = null;
                    Global.KIJITU_H_tb1 = null;
                    Global.KUBN1_tb3 = null;
                    Global.OWNID1_tb3 = null;
                    Global.KUBN2_tb3 = null;
                    Global.OWNID2_tb3 = null;
                    Global.KUBN3_tb3 = null;
                    Global.OWNID3_tb3 = null;
                    Global.KUBN4_tb3 = null;
                    Global.OWNID4_tb3 = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb3_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }

        /// <summary>
        /// 振込先情報タブの画面情報を取得
        /// </summary>
        private void Get_Tb4_Data()
        {
            try
            {
                //履歴関連 ＠2011/07 履歴対応
                //変更判定&dtRIREKIへの格納
                string sSW = "";
                if (Tb4_Chk_FDEF.Checked == true)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.FDEF != sSW)
                {
                    Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "FDEF", 2, "初期値SW", mcBsLogic.Get_ShokichiNM(Global.FDEF), mcBsLogic.Get_ShokichiNM(sSW));
                }

                if (Tb4_Chk_DDEF.Checked == true)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.DDEF != sSW)
                {
                    Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "DDEF", 2, "でんさい代表口座SW", mcBsLogic.Get_DensaiNM(Global.DDEF), mcBsLogic.Get_DensaiNM(sSW));
                }

                if (Global.BANK_CD_tb2 != Tb4_Txt_BANK_CD.Text)
                {
                    string sBANK_DB = "";
                    string sBANK_Disp = "";
                    if (Global.BANK_CD_tb2 != "")
                    {
                        sBANK_DB = Global.BANK_CD_tb2 + ":" + mcBsLogic.Get_BANKNM(Global.BANK_CD_tb2);
                    }
                    if (Tb4_Txt_BANK_CD.Text != "")
                    {
                        sBANK_Disp = Tb4_Txt_BANK_CD.Text + ":" + mcBsLogic.Get_BANKNM(Tb4_Txt_BANK_CD.Text);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "BANK_CD", 2, "銀行コード", sBANK_DB, sBANK_Disp);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "BANK_CD", 2, "銀行コード", sBANK_DB, sBANK_Disp);
                    }
                    //**<<ICS-E
                }

                if (Global.SITEN_ID_tb2 != Tb4_Txt_SITEN_ID.Text)
                {
                    string sSITEN_DB = "";
                    string sSITEN_Disp = "";
                    if (Global.SITEN_ID_tb2 != "")
                    {
                        sSITEN_DB = Global.SITEN_ID_tb2 + ":" + mcBsLogic.Get_SITENNM(Global.BANK_CD_tb2, Global.SITEN_ID_tb2);
                    }
                    if (Tb4_Txt_SITEN_ID.Text != "")
                    {
                        sSITEN_Disp = Tb4_Txt_SITEN_ID.Text + ":" + mcBsLogic.Get_SITENNM(Tb4_Txt_BANK_CD.Text, Tb4_Txt_SITEN_ID.Text);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "SITEN_ID", 2, "支店コード", sSITEN_DB, sSITEN_Disp);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "SITEN_ID", 2, "支店コード", sSITEN_DB, sSITEN_Disp);
                    }
                    //**<<ICS-E
                }

                if (Global.YOKIN_TYP_tb2 != mcBsLogic.Get_YokinType_CD(Tb4_Cmb_YOKIN_TYP.Text))
                {
                    string sYokinTyp_DB = "";
                    if (Global.YOKIN_TYP_tb2 != "")
                    {
                        sYokinTyp_DB = Global.YOKIN_TYP_tb2 + ":" + mcBsLogic.Get_YokinType_NM(int.Parse(Global.YOKIN_TYP_tb2));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "YOKIN_TYP", 2, "預金種別", sYokinTyp_DB, Tb2_Cmb_YOKIN_TYP.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "YOKIN_TYP", 2, "預金種別", sYokinTyp_DB, Tb4_Cmb_YOKIN_TYP.Text);
                    }
                    //**<<ICS-E
                }

                if (Global.KOUZA_tb2 != Tb4_Txt_KOUZA.Text)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "KOUZA", 2, "口座番号", Global.KOUZA_tb2, Tb2_Txt_KOUZA.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "KOUZA", 2, "口座番号", Global.KOUZA_tb2, Tb4_Txt_KOUZA.Text);
                    }
                    //**<<ICS-E
                }

                if (Global.MEIGI_tb2 != Tb4_Txt_MEIGI.Text)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "MEIGI", 2, "名義人", Global.MEIGI_tb2, Tb2_Txt_MEIGI.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "MEIGI", 2, "名義人", Global.MEIGI_tb2, Tb4_Txt_MEIGI.Text);
                    }
                    //**<<ICS-E
                }

                if (Global.MEIGIK_tb2 != Tb4_Txt_MEIGIK.Text)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "MEIGIK", 2, "名義人カナ", Global.MEIGIK_tb2, Tb2_Txt_MEIGIK.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "MEIGIK", 2, "名義人カナ", Global.MEIGIK_tb2, Tb4_Txt_MEIGIK.Text);
                    }
                    //**<<ICS-E
                }

//-- <>
//                if (Global.FTESUID != Convert.ToString(Tb4_Cmb_FTESUID.SelectedValue))
                if (Global.FTESUID != Convert.ToString(Tb4_Cmb_FTESUID.SelectedValue ?? 0))
//-- <>
                {
                    string sFTesuID = "";
                    if (Global.FTESUID != "")
                    {
                        sFTesuID = Global.FTESUID + ":" + mcBsLogic.Get_TesuuIdNm(Global.FTESUID);
                    }

                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "FTESUID", 2, "手数料ID", sFTesuID, Tb4_Cmb_FTESUID.Text);
                    }
                }

                if (Global.TESUU_tb2 != mcBsLogic.Get_Tesuu_CD(Tb4_Cmb_TESUU.Text))
                {
                    string sTesuu_DB = "";
                    if (Global.TESUU_tb2 != "")
                    {
                        sTesuu_DB = Global.TESUU_tb2 + ":" + mcBsLogic.Get_Tesuu_NM(int.Parse(Global.TESUU_tb2));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "TESUU", 2, "手数料負担", sTesuu_DB, Tb2_Cmb_TESUU.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "TESUU", 2, "手数料負担", sTesuu_DB, Tb4_Cmb_TESUU.Text);
                    }
                    //**<<ICS-E
                }

                if (Global.SOUKIN_tb2 != mcBsLogic.Get_Soukin_CD(Tb4_Cmb_SOUKIN.Text))
                {
                    string sSoukin_DB = "";
                    if (Global.SOUKIN_tb2 != "")
                    {
                        sSoukin_DB = Global.SOUKIN_tb2 + ":" + mcBsLogic.Get_Soukin_NM(int.Parse(Global.SOUKIN_tb2));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "SOUKIN", 2, "送金区分", sSoukin_DB, Tb2_Cmb_SOUKIN.Text);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "SOUKIN", 2, "送金区分", sSoukin_DB, Tb4_Cmb_SOUKIN.Text);
                    }
                    //**<<ICS-E
                }

                string sGEND_Disp = "0";
                string sGEND_DB = "0";
                if (Global.TESUU_tb2 == "1")
                {
                    sGEND_Disp = Tb4_Txt_GENDO.ExNumValue.ToString("#,##0");
                }
                if (Global.GENDO_tb2 != "")
                {
                    sGEND_DB = Convert.ToInt64(Global.GENDO_tb2).ToString("#,##0");
                }
                //if (Global.GENDO_tb2 != sGEND_Disp)
                if (sGEND_DB != sGEND_Disp)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(1, int.Parse(Tb2_Lbl_GIN_ID_V.Text), "GENDO", 2, "手数料負担限度額", sGEND_DB, sGEND_Disp);
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "GENDO", 2, "手数料負担限度額", sGEND_DB, sGEND_Disp);
                    }
                    //**<<ICS-E
                }

                if (Tb2_Chk_DTESUSW.Checked == true)
                {
                    sSW = "1";
                }
                else
                {
                    sSW = "0";
                }
                if (Global.DTESUSW != sSW)
                {
                    Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "DTESUSW", 2, "でんさい手数料設定", mcBsLogic.Get_DensaiTesuuNM(Global.DTESUSW), mcBsLogic.Get_DensaiTesuuNM(sSW));
                }

                if (Global.DTESU != mcBsLogic.Get_Tesuu_CD(Tb2_Cmb_DTESU.Text))
                {
                    string sTesuu_DB = "";
                    if (Global.DTESU != "")
                    {
                        sTesuu_DB = Global.DTESU + ":" + mcBsLogic.Get_Tesuu_NM(int.Parse(Global.DTESU));
                    }
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "DTESU", 2, "でんさい手数料負担", sTesuu_DB, Tb2_Cmb_DTESU.Text);
                    }
                }

                if (Tb3_Rdo_GAI_F0.Checked == true && Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    //Globalへの画面データ格納
                    Global.GIN_ID_tb2 = Tb4_Lbl_GIN_ID_V.Text;

                    Global.FDEF = Tb4_Chk_FDEF.Checked ? "1" : "0";
                    Global.DDEF = Tb4_Chk_DDEF.Checked ? "1" : "0";

                    Global.BANK_CD_tb2 = Tb4_Txt_BANK_CD.Text;
                    Global.SITEN_ID_tb2 = Tb4_Txt_SITEN_ID.Text;
                    Global.YOKIN_TYP_tb2 = mcBsLogic.Get_YokinType_CD(Tb4_Cmb_YOKIN_TYP.Text);
                    Global.KOUZA_tb2 = Tb4_Txt_KOUZA.Text;
                    Global.MEIGI_tb2 = Tb4_Txt_MEIGI.Text;
                    Global.MEIGIK_tb2 = Tb4_Txt_MEIGIK.Text;

                    Global.TESUU_tb2 = mcBsLogic.Get_Tesuu_CD(Tb4_Cmb_TESUU.Text);
                    Global.SOUKIN_tb2 = mcBsLogic.Get_Soukin_CD(Tb4_Cmb_SOUKIN.Text);
                    Global.GENDO_tb2 = Tb4_Txt_GENDO.ExNumValue.ToString();
//-- <2016/03/15>
//                    Global.FTESUID = Tb4_Cmb_FTESUID.SelectedValue.ToString();
                    Global.FTESUID = Tb4_Cmb_FTESUID.SelectedValue != null ? Tb4_Cmb_FTESUID.SelectedValue.ToString() : "";
//-- <2016/03/15>
                    Global.DTESUSW = Tb2_Chk_DTESUSW.Checked ? "1" : "0";
                    Global.DTESU = mcBsLogic.Get_Tesuu_CD(Tb2_Cmb_DTESU.Text);
                }
                else
                {
                    Global.GIN_ID_tb2 = "";

                    Global.FDEF = "";
                    Global.DDEF = "";

                    Global.BANK_CD_tb2 = "";
                    Global.SITEN_ID_tb2 = "";
                    Global.YOKIN_TYP_tb2 = "";
                    Global.KOUZA_tb2 = "";
                    Global.MEIGI_tb2 = "";
                    Global.MEIGIK_tb2 = "";

                    Global.TESUU_tb2 = "";
                    Global.SOUKIN_tb2 = "";
                    Global.GENDO_tb2 = "";
                    Global.FTESUID = "";
                    Global.DTESUSW = "";
                    Global.DTESU = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb4_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }


        /// <summary>
        /// その他情報タブの控除情報を取得
        /// </summary>
        private void Get_Tb5_Data_Koujyo()
        {
            try
            {
                //履歴関連 ＠2011/07 履歴対応
                //変更判定&dtRIREKIへの格納
                string sGENSEN = "";
                if (!Tb5_Chk_GENSEN.Checked && !Tb5_Chk_OUTPUT.Checked)
                {
                    sGENSEN = "0";
                }
                else if (Tb5_Chk_GENSEN.Checked && Tb5_Radio_GENSEN1.Checked)
                {
                    sGENSEN = "1";
                }
                else if (Tb5_Chk_GENSEN.Checked && Tb5_Radio_GENSEN2.Checked)
                {
                    sGENSEN = "2";
                }
                else if (!Tb5_Chk_GENSEN.Checked && Tb5_Chk_OUTPUT.Checked)
                {
                    sGENSEN = "3";
                }
                if (Global.GENSEN != sGENSEN)
                {
                    string sGENSEN_DB = "";
                    string sGENSEN_Disp = "";
                    if (Global.GENSEN != "")
                    {
                        sGENSEN_DB = Global.GENSEN + ":" + mcBsLogic.Get_GENSENNM(Global.GENSEN);
                    }
                    if (sGENSEN != "")
                    {
                        sGENSEN_Disp = sGENSEN + ":" + mcBsLogic.Get_GENSENNM(sGENSEN);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "GENSEN", 2, "源泉税計算区分", sGENSEN_DB, sGENSEN_Disp);
                    Set_dtRIREKI(5, 0, "GENSEN", 2, "源泉税計算区分", sGENSEN_DB, sGENSEN_Disp);
                    //**<<ICS-E
                }

                string sGSSKBN = "";
                if (Tb5_Rdo_GSSKBN1.Checked == true)
                {
                    sGSSKBN = "1";
                }
                else
                {
                    sGSSKBN = "2";
                }
                if (Global.GSSKBN != sGSSKBN)
                {
                    Set_dtRIREKI(5, 0, "GSSKBN", 2, "計算基準", mcBsLogic.Get_GSSKBN_NM(Global.GSSKBN), mcBsLogic.Get_GSSKBN_NM(sGSSKBN));
                }


                string sGOU = "";
                string sGGKBN = "";
                string sGGKBNM = "";
                string sGSKUBN = "";
                if (!Tb5_Chk_GENSEN.Checked && !Tb5_Chk_OUTPUT.Checked)
                {
                    sGOU = "";
                    sGGKBN = "";
                    sGGKBNM = "";
                    sGSKUBN = "0";
                }
                else if (Tb5_Chk_GENSEN.Checked && (Tb5_Radio_GENSEN1.Checked || Tb5_Radio_GENSEN2.Checked))
                {
                    if (Tb5_Cmb_GOU.Text != "")
                    {
                        sGOU = mcBsLogic.Get_Gou_CD(Tb5_Cmb_GOU.Text);
                    }
                    else
                    {
                        sGOU = "0";
                    }
                    if (Tb5_Cmb_GGKBN.SelectedValue != null)
                    {
                        if (Tb5_Cmb_GGKBN.SelectedValue.ToString() != "-1")
                        {
                            sGGKBN = Tb5_Cmb_GGKBN.SelectedValue.ToString();
                            sGGKBNM = Tb5_Cmb_GGKBN.Text.Substring(Tb5_Cmb_GGKBN.Text.IndexOf(':') + 1);
                        }
                        else
                        {
                            sGGKBN = "";
                            sGGKBNM = "";
                        }
                    }
                    else
                    {
                        sGGKBN = "";
                        sGGKBNM = "";
                    }
                    if (Tb5_Cmb_GSKUBN.Text != "")
                    {
                        sGSKUBN = Tb5_Cmb_GSKUBN.Text.Substring(0, Tb5_Cmb_GSKUBN.Text.IndexOf(':'));
                    }
                    else
                    {
                        sGSKUBN = null;
                    }
                }
                else if (!Tb5_Chk_GENSEN.Checked && Tb5_Chk_OUTPUT.Checked)
                {
                    if (Tb5_Cmb_GOU.Text != "")
                    {
                        sGOU = mcBsLogic.Get_Gou_CD(Tb5_Cmb_GOU.Text);
                    }
                    else
                    {
                        sGOU = "0";
                    }
                    if (Tb5_Cmb_GGKBN.SelectedValue != null)
                    {
                        if (Tb5_Cmb_GGKBN.SelectedValue.ToString() != "-1")
                        {
                            sGGKBN = Tb5_Cmb_GGKBN.SelectedValue.ToString();
                            sGGKBNM = Tb5_Cmb_GGKBN.Text.Substring(Tb5_Cmb_GGKBN.Text.IndexOf(':') + 1);
                        }
                        else
                        {
                            sGGKBN = "";
                            sGGKBNM = "";
                        }
                    }
                    else
                    {
                        sGGKBN = "";
                        sGGKBNM = "";
                    }
                    sGSKUBN = "0";
                }
                if (Global.GOU != sGOU)
                {
                    string sGOU_DB = "";
                    string sGOU_Disp = "";
                    if (Global.GOU != "" && Global.GOU != null)
                    {
                        sGOU_DB = Global.GOU + ":" + mcBsLogic.Get_Gou_NM(int.Parse(Global.GOU));
                    }
                    if (sGOU != "")
                    {
                        sGOU_Disp = Tb5_Cmb_GOU.Text;
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "GOU", 2, "第204条第1項-号", sGOU_DB, sGOU_Disp);
                    Set_dtRIREKI(5, 0, "GOU", 2, "第204条第1項-号", sGOU_DB, sGOU_Disp);
                    //**<<ICS-E
                }
                if (Global.GGKBN != sGGKBN)
                {
                    string sGGKBN_DB = "";
                    if (Global.GGKBN != "" && Global.GGKBNM != "")
                    {
                        sGGKBN_DB = string.Format("{0}:{1}", Global.GGKBN, Global.GGKBNM);
                    }
                    string sGGKBN_Disp = "";
                    if (Tb5_Cmb_GGKBN.Text != "")
                    {
                        sGGKBN_Disp = Tb5_Cmb_GGKBN.Text; //.Substring(Tb4_Cmb_GGKBN.Text.IndexOf(':'));
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "GGKBN", 2, "源泉区分ｺｰﾄﾞ", sGGKBN_DB, sGGKBN_Disp);
                    Set_dtRIREKI(5, 0, "GGKBN", 2, "源泉区分ｺｰﾄﾞ", sGGKBN_DB, sGGKBN_Disp);
                    //**<<ICS-E
                }
                if (Global.GSKUBN != sGSKUBN)
                {
                    string sGSKUBN_DB = "";
                    string sGSKUBN_Disp = "";
                    if (Global.GSKUBN != "" && Global.GSKUBN != "0")
                    {
                        sGSKUBN_DB = Global.GSKUBN + ":" + mcBsLogic.Get_SKUBN(Global.GSKUBN);
                    }
                    if (sGSKUBN != null && sGSKUBN != "")
                    {
                        sGSKUBN_Disp = Tb5_Cmb_GSKUBN.Text;
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "GSKUBN", 2, "源泉税支払区分", sGSKUBN_DB, sGSKUBN_Disp);
                    Set_dtRIREKI(5, 0, "GSKUBN", 2, "源泉税支払区分", sGSKUBN_DB, sGSKUBN_Disp);
                    //**<<ICS-E
                }
                string sHORYU = "";
                string sHOVAL = "";
                string sHRORYUGAKU = "";
                if (Tb5_Rdo_HORYU1.Checked == true)                                             //支払保留を使用する
                {
                    sHORYU = "1";
//-- <2016/03/14>
//                    sHOVAL = Tb5_Txt_HOVAL.ExNumValue.ToString("0.0");
//                    sHRORYUGAKU = "0";
                    sHOVAL = Tb5_Txt_HOVAL.ExNumValue.ToString("#0.##0");
                    sHRORYUGAKU = Tb5_Txt_HRORYUGAKU.ExNumValue.ToString();
//-- <2016/03/14>
                }
                else if (Tb5_Rdo_HORYU2.Checked == true)                                        //自動控除を使用する
                {
                    sHORYU = "2";
//-- <2016/03/14>
//                    sHOVAL = "100.0";
                    sHOVAL = Tb5_Txt_HOVAL.ExNumValue.ToString("#0.##0");
//-- <2016/03/14>
                    sHRORYUGAKU = Tb5_Txt_HRORYUGAKU.ExNumValue.ToString();
                }
                else
                {
                    sHORYU = "0";
//-- <2016/03/14>
//                    sHOVAL = "100.0";
                    sHOVAL = "100.000";
//-- <2016/03/14>
                    sHRORYUGAKU = "0";
                }
                if (Global.HORYU != sHORYU)
                {
                    string sHORYU_DB = "";
                    string sHORYU_Disp = "";
                    if (Global.HORYU != "")
                    {
                        sHORYU_DB = Global.HORYU + ":" + mcBsLogic.Get_HoryuNM(Global.HORYU);
                    }
                    if (sHORYU != "")
                    {
                        sHORYU_Disp = sHORYU + ":" + mcBsLogic.Get_HoryuNM(sHORYU);
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "HORYU", 2, "支払保留SW", sHORYU_DB, sHORYU_Disp);
                    Set_dtRIREKI(5, 0, "HORYU", 2, "支払保留SW", sHORYU_DB, sHORYU_Disp);
                    //**<<ICS-E
                }

                string sKijyun_Disp = "0";
                string sKijyun_DB = "0";
                if (Tb5_Txt_HR_KIJYUN.ExNumValue != 0)
                {
                    sKijyun_Disp = Tb5_Txt_HR_KIJYUN.ExNumValue.ToString("#,##0");
                }
                if (Global.HR_KIJYUN != "")
                {
                    sKijyun_DB = Convert.ToInt64(Global.HR_KIJYUN).ToString("#,##0");
                }
                if (sKijyun_DB != sKijyun_Disp)
                {
                    Set_dtRIREKI(5, 0, "HR_KIJYUN", 2, "計算適用基準額", sKijyun_DB, sKijyun_Disp);
                }

                string sHoryuF_Disp = "";
                string sHoryuF_DB = "";
                sHoryuF_Disp = Tb5_Cmb_HORYU_F.Text;
//-- <2016/04/02>
//                if (sHoryuF_Disp == "")
//                {
//                    sHoryuF_Disp = "0:比率";
//                }
//                if (Global.HORYU_F == "0")
//                {
//                    sHoryuF_DB = "0:比率";
//                }
//                else
//                {
//                    sHoryuF_DB = "1:定額";
//                }
                if (sHoryuF_Disp == "" || sHoryuF_Disp == "0")
                {
                    sHoryuF_Disp = "1:比率";
                }
                // Ver.01.02.03 [SS_4666]対応 Toda -->
                //if (Global.HORYU_F == "0" || Global.HORYU_F == "1")
                if (Global.HORYU_F == "" || Global.HORYU_F == "0" || Global.HORYU_F == "1")
                // Ver.01.02.03 <--
                {
                    sHoryuF_DB = "1:比率";
                }
                else
                {
                    sHoryuF_DB = "2:定額";
                }
//-- <2016/04/02>
                if (sHoryuF_DB != sHoryuF_Disp)
                {
                    Set_dtRIREKI(5, 0, "HORYU_F", 2, "計算区分ﾌﾗｸﾞ", sHoryuF_DB, sHoryuF_Disp);
                }

                if (Global.HOVAL != sHOVAL)
                {
                    string sHOVAL_DB = "";
                    string sHOVAL_Disp = "";
                    if (Global.HOVAL != "")
                    {
                        sHOVAL_DB = Global.HOVAL +"%";
                    }
                    if (sHOVAL != "")
                    {
                        sHOVAL_Disp = sHOVAL + "%";
                    }
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "HOVAL", 2, "支払率", sHOVAL_DB, sHOVAL_Disp);
                    Set_dtRIREKI(5, 0, "HOVAL", 2, "支払率", sHOVAL_DB, sHOVAL_Disp);
                    //**<<ICS-E
                }

                string sHRORYUGAKU_Disp = Tb5_Txt_HRORYUGAKU.ExNumValue.ToString("#,##0");
                string sHRORYUGAKU_DB = "0";
                if (Global.HRORYUGAKU != "")
                {
                    sHRORYUGAKU_DB = Convert.ToInt64(Global.HRORYUGAKU).ToString("#,##0");
                }
                if (sHRORYUGAKU_DB != sHRORYUGAKU_Disp)
                {
                    Set_dtRIREKI(5, 0, "HRORYUGAKU", 2, "定額", sHRORYUGAKU_DB, sHRORYUGAKU_Disp);
                }

                string sHRKBN_DB = "";
                string sHRKBN_Disp = "";
                if (Global.HRKBN != "" && Global.HRKBN != "0")
                {
                    sHRKBN_DB = Global.HRKBN + ":" + mcBsLogic.Get_SKUBN(Global.HRKBN);
                }
                if (!string.IsNullOrEmpty(Tb5_Cmb_HRKBN.Text))
                {
                    sHRKBN_Disp = Tb5_Cmb_HRKBN.Text;
                }
                if (sHRKBN_DB != sHRKBN_Disp)
                {
                    Set_dtRIREKI(5, 0, "HRKBN", 2, "作成区分", sHRKBN_DB, sHRKBN_Disp);
                }

                //源泉税関連
                Global.GENSEN = sGENSEN;
                if (!Tb5_Chk_GENSEN.Checked && !Tb5_Chk_OUTPUT.Checked)
                {
                    //**ICS-S 2012/05/21
                    //**Global.GOU = "0";
                    Global.GOU = null;
                    //**ICS-E
                    Global.GGKBN = null;
                    Global.GGKBNM = null;
                    Global.GSKUBN = "0";
                }
                else if (Tb5_Chk_GENSEN.Checked && (Tb5_Radio_GENSEN1.Checked || Tb5_Radio_GENSEN2.Checked))
                {
                    if (Tb5_Cmb_GOU.Text != "")
                    {
                        Global.GOU = mcBsLogic.Get_Gou_CD(Tb5_Cmb_GOU.Text);
                    }
                    else
                    {
                        Global.GOU = "0";
                    }
                    if (Tb5_Cmb_GGKBN.SelectedValue != null)
                    {
                        if (Tb5_Cmb_GGKBN.SelectedValue.ToString() != "-1")
                        {
                            Global.GGKBN = Tb5_Cmb_GGKBN.SelectedValue.ToString();
                            Global.GGKBNM = Tb5_Cmb_GGKBN.Text.Substring(Tb5_Cmb_GGKBN.Text.IndexOf(':') + 1);
                        }
                        else
                        {
                            Global.GGKBN = null;
                            Global.GGKBNM = null;
                        }
                    }
                    else
                    {
                        Global.GGKBN = null;
                        Global.GGKBNM = null;
                    }
                    if (Tb5_Cmb_GSKUBN.Text != "")
                    {
                        Global.GSKUBN = Tb5_Cmb_GSKUBN.Text.Substring(0, Tb5_Cmb_GSKUBN.Text.IndexOf(':'));
                    }
                    else
                    {
                        Global.GSKUBN = "0";
                    }
                }
                else if (!Tb5_Chk_GENSEN.Checked && Tb5_Chk_OUTPUT.Checked)
                {
                    if (Tb5_Cmb_GOU.Text != "")
                    {
                        Global.GOU = mcBsLogic.Get_Gou_CD(Tb5_Cmb_GOU.Text);
                    }
                    else
                    {
                        Global.GOU = "0";
                    }
                    if (Tb5_Cmb_GGKBN.SelectedValue != null)
                    {
                        if (Tb5_Cmb_GGKBN.SelectedValue.ToString() != "-1")
                        {
                            Global.GGKBN = Tb5_Cmb_GGKBN.SelectedValue.ToString();
                            Global.GGKBNM = Tb5_Cmb_GGKBN.Text.Substring(Tb5_Cmb_GGKBN.Text.IndexOf(':') + 1);
                        }
                        else
                        {
                            Global.GGKBN = null;
                            Global.GGKBNM = null;
                        }
                    }
                    else
                    {
                        Global.GGKBN = null;
                        Global.GGKBNM = null;
                    }
                    Global.GSKUBN = "0";
                }

                //控除関連
                Global.HORYU = sHORYU;
                Global.HOVAL = sHOVAL;
                Global.HRORYUGAKU = sHRORYUGAKU;
                if (Tb5_Cmb_HORYU_F.SelectedIndex != -1)
                {
                    //-- <2016/04/09 インデックス＋１>
                    //                    Global.HORYU_F = Tb5_Cmb_HORYU_F.SelectedIndex.ToString();
                    Global.HORYU_F = (Tb5_Cmb_HORYU_F.SelectedIndex + 1).ToString();
                    //-- <2016/04/09>
                }
                else
                {
                    Global.HORYU_F = "0";
                }
//                else
//                {
////-- <2016/04/02>
////                    Global.HORYU_F = "0";
//                    Global.HORYU_F = "1";
////-- <2016/04/02>
//                }
                Global.HR_KIJYUN = Tb5_Txt_HR_KIJYUN.ExNumValue.ToString();
                if (Tb5_Cmb_HRKBN.SelectedIndex != -1)
                {
                    Global.HRKBN = Tb5_Cmb_HRKBN.SelectedValue.ToString();
                }
                else
                {
                    // 
                    Global.HRKBN = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nその他(Get_Tb5_Data_Koujyo　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }


        /// <summary>
        /// その他情報タブの画面情報を取得
        /// </summary>
        private void Get_Tb5_Data()
        {
            try
            {
                //履歴関連 ＠2011/07 履歴対応
                //変更判定&dtRIREKIへの格納
                if (Global.F_SOUFU != mcBsLogic.Get_FSoufu_CD(Tb5_Cmb_F_SOUFU.Text))
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "F_SOUFU", 2, "送付案内出力有無", Global.F_SOUFU + ":" 
                    //**             + mcBsLogic.Get_FSoufu_NM(int.Parse(Global.F_SOUFU)), Tb5_Cmb_F_SOUFU.Text);
                    Set_dtRIREKI(5, 0, "F_SOUFU", 2, "送付案内出力有無", Global.F_SOUFU + ":"
                                 + mcBsLogic.Get_FSoufu_NM(int.Parse(Global.F_SOUFU)), Tb5_Cmb_F_SOUFU.Text);
                    //**<<ICS-E
                }
                if (Global.ANNAI != mcBsLogic.Get_Annai_CD(Tb5_Cmb_ANNAI.Text))
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "ANNAI", 2, "案内文ﾊﾟﾀｰﾝ", Global.ANNAI + ":" 
                    //**             + mcBsLogic.Get_Annai_NM(int.Parse(Global.ANNAI)), Tb5_Cmb_ANNAI.Text);
                    Set_dtRIREKI(5, 0, "ANNAI", 2, "案内文ﾊﾟﾀｰﾝ", Global.ANNAI + ":"
                                 + mcBsLogic.Get_Annai_NM(int.Parse(Global.ANNAI)), Tb5_Cmb_ANNAI.Text);
                    //**<<ICS-E
                }
                if (Global.TSOKBN != mcBsLogic.Get_Tsokbn_CD(Tb5_Cmb_TSOKBN.Text))
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "TSOKBN", 2, "手形送料負担区分", Global.TSOKBN + ":" 
                    //**             + mcBsLogic.Get_Tsokbn_NM(int.Parse(Global.TSOKBN)), Tb5_Cmb_TSOKBN.Text);
                    Set_dtRIREKI(5, 0, "TSOKBN", 2, "手形送料負担区分", Global.TSOKBN + ":"
                                 + mcBsLogic.Get_Tsokbn_NM(int.Parse(Global.TSOKBN)), Tb5_Cmb_TSOKBN.Text);
                    //**<<ICS-E
                }

                string sSORYOU_Disp = Tb5_Txt_TEGVAL.ExNumValue.ToString("#,##0");
                string sSORYOU_DB = "0";
                if (Global.TEGVAL != "")
                {
                    sSORYOU_DB = Convert.ToInt64(Global.TEGVAL).ToString("#,##0");
                }
                if (sSORYOU_DB != sSORYOU_Disp)
                {
                    if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                    {
                        Set_dtRIREKI(5, 0, "TEGVAL", 2, "送料", sSORYOU_DB, sSORYOU_Disp);
                    }
                }

                if (Global.F_SHITU != mcBsLogic.Get_HaraiTuuti_CD(Tb5_Cmb_SHITU.Text))
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "F_SHITU", 2, "支払通知発行区分", Global.F_SHITU + ":"
                    //**             + mcBsLogic.Get_HaraiTuuti_NM(int.Parse(Global.F_SHITU)), Tb5_Cmb_SHITU.Text);
                    Set_dtRIREKI(5, 0, "F_SHITU", 2, "支払通知発行区分", Global.F_SHITU + ":"
                                 + mcBsLogic.Get_HaraiTuuti_NM(int.Parse(Global.F_SHITU)), Tb5_Cmb_SHITU.Text);
                    //**<<ICS-E
                }
                if (Global.DM1 != Tb5_Txt_DM1.Text)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "DM1", 2, "補助ｺｰﾄﾞ1", Global.DM1, Tb5_Txt_DM1.Text);
                    Set_dtRIREKI(5, 0, "DM1", 2, "補助ｺｰﾄﾞ1", Global.DM1, Tb5_Txt_DM1.Text);
                    //**<<ICS-E
                }
                if (Global.DM2 != Tb5_Txt_DM2.ExNumValue.ToString())
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "DM2", 2, "補助ｺｰﾄﾞ2", Global.DM2, Tb5_Txt_DM2.ExNumValue.ToString());
                    //**>>ICS-S 2013/07/26 -W0013
                    //**Set_dtRIREKI(5, 0, "DM2", 2, "補助ｺｰﾄﾞ2", Global.DM2, Tb5_Txt_DM2.ExNumValue.ToString());
                    Set_dtRIREKI(5, 0, "DM2", 2, "補助ｺｰﾄﾞ2", Global.DM2 == "0" ? "" : Global.DM2, Tb5_Txt_DM2.ExNumValue.ToString() == "0" ? "" : Tb5_Txt_DM2.ExNumValue.ToString());
                    //**<<ICS-E
                }
                if (Global.DM3 != Tb5_Txt_DM3.ExNumValue.ToString())
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "DM3", 2, "補助ｺｰﾄﾞ3", Global.DM3, Tb5_Txt_DM3.ExNumValue.ToString());
                    //**>>ICS-S 2013/07/26 -W0013
                    //**Set_dtRIREKI(5, 0, "DM3", 2, "補助ｺｰﾄﾞ3", Global.DM3, Tb5_Txt_DM3.ExNumValue.ToString());
                    Set_dtRIREKI(5, 0, "DM3", 2, "補助ｺｰﾄﾞ3", Global.DM3 == "0" ? "" : Global.DM3, Tb5_Txt_DM3.ExNumValue.ToString() == "0" ? "" : Tb5_Txt_DM3.ExNumValue.ToString());
                    //**<<ICS-E
                }
                //**>>ICS-S 2013/05/20
                if (Global.CDM2 != Tb5_Txt_FAC.Text)
                {
                    //**>>ICS-S 2013/06/12
                    //**Set_dtRIREKI(0, 0, "CDM2", 2, "仕入先番号", Global.CDM2, Tb5_Txt_FAC.Text);
                    Set_dtRIREKI(5, 0, "CDM2", 2, "仕入先番号", Global.CDM2, Tb5_Txt_FAC.Text);
                    //**<<ICs-E
                }
                //if (Global.CD03 != Tb5_Txt_RefNo.Text)
                //{
                //    //**>>ICS-S 2013/06/12
                //    //**Set_dtRIREKI(0, 0, "CD03", 2, "依頼人Ref.No.", Global.CD03, Tb5_Txt_RefNo.Text);
                //    Set_dtRIREKI(5, 0, "CD03", 2, "依頼人Ref.No.", Global.CD03, Tb5_Txt_RefNo.Text);
                //    //**<<ICS-E
                //}
                //**<<ICS-E

                //画面情報の格納
                Global.F_SOUFU = mcBsLogic.Get_FSoufu_CD(Tb5_Cmb_F_SOUFU.Text);
                Global.ANNAI = mcBsLogic.Get_Annai_CD(Tb5_Cmb_ANNAI.Text);
                Global.TSOKBN = mcBsLogic.Get_Tsokbn_CD(Tb5_Cmb_TSOKBN.Text);
                Global.F_SHITU = Cbo_SAIMU.SelectedValue.ToString() == sUse ? mcBsLogic.Get_HaraiTuuti_CD(Tb5_Cmb_SHITU.Text) : "0";
                Global.DM1 = Tb5_Txt_DM1.Text;
                Global.DM2 = Tb5_Txt_DM2.ExNumValue.ToString();
                Global.DM3 = Tb5_Txt_DM3.ExNumValue.ToString();

                //**>>ICS-S 2013/05/20
                Global.CDM2 = Tb5_Txt_FAC.Text;
                //Global.CD03 = Tb5_Txt_RefNo.Text;
                //**<<ICS-E

                Global.TEGVAL = Cbo_SAIMU.SelectedValue.ToString() == sUse ? Tb5_Txt_TEGVAL.ExNumValue.ToString() : "";
//-- <2016/04/29 1:支払金額、2:税抜金額>
//                Global.GSSKBN = (Tb5_Rdo_GSSKBN1.Checked == true ? "1" : "0");
                Global.GSSKBN = (Tb5_Rdo_GSSKBN1.Checked == true ? "1" : "2");
//-- <2016/04/29>
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb5_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }

        /// <summary>
        /// 外貨設定タブの画面情報を取得
        /// </summary>
        private void Get_Tb6_Data()
        {
            try
            {
                if (Global.HEI_CD != Tb6_Cmb_HEI_CD.Text)
                {
                    Set_dtRIREKI(6, 0, "HEI_CD", 2, "取引通貨", Global.HEI_CD, Tb6_Cmb_HEI_CD.Text);
                }

                string sSW = "";
                if (Tb6_Rdo_GAI_SF0.Checked == true)
                {
                    sSW = "0";
                }
                else
                {
                    sSW = "1";
                }
                if (Global.GAI_SF != sSW)
                {
                    Set_dtRIREKI(6, 0, "GAI_SF", 2, "送金種類", mcBsLogic.Get_GaiSFNM(Global.GAI_SF), mcBsLogic.Get_GaiSFNM(sSW));
                }

                sSW = "";
                if (Tb6_Rdo_GAI_SH0.Checked == true)
                {
                    sSW = "0";
                }
                else
                {
                    sSW = "1";
                }
                if (Global.GAI_SH != sSW)
                {
                    Set_dtRIREKI(6, 0, "GAI_SH", 2, "送金支払方法", mcBsLogic.Get_GaiSHNM(Global.GAI_SH), mcBsLogic.Get_GaiSHNM(sSW));
                }

                string sKID = Convert.ToString(Tb6_Cmb_GAI_KZID.SelectedValue);
                if (Global.GAI_KZID != sKID)
                {
                    Set_dtRIREKI(6, 0, "GAI_KZID", 2, "出金口座", Global.GAI_KZID, sKID);
                }

//-- <2016/03/09 非選択項目>
//                if (Global.GAI_TF != mcBsLogic.Get_Tesuu_CD(Tb6_Cmb_GAI_TF.Text))
//                {
//                    string sTesuu_DB = "";
//                    if (Global.GAI_TF != "")
//                    {
//                        sTesuu_DB = Global.GAI_TF + ":" + mcBsLogic.Get_Tesuu_NM(int.Parse(Global.GAI_TF));
//                    }
//                    Set_dtRIREKI(6, 0, "GAI_TF", 2, "手数料負担", sTesuu_DB, Tb6_Cmb_GAI_TF.Text);
//                }
//-- <2016/03/09>
                if (Global.ENG_NAME != Tb6_Txt_ENG_NAME.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_NAME", 2, "受取人名(PAYEE NAME)", Global.ENG_NAME, Tb6_Txt_ENG_NAME.Text);
                }
                if (Global.ENG_ADDR != Tb6_Txt_ENG_ADDR.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_ADDR", 2, "住所(ADDR)", Global.ENG_ADDR, Tb6_Txt_ENG_ADDR.Text);
                }
                if (Global.ENG_KZNO != Tb6_Txt_ENG_KZNO.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_KZNO", 2, "口座番号/IBANｺｰﾄﾞ", Global.ENG_KZNO, Tb6_Txt_ENG_KZNO.Text);
                }
                if (Global.ENG_SWIF != Tb6_Txt_ENG_SWIF.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_SWIF", 2, "SWIFT(BIC)ｺｰﾄﾞ", Global.ENG_SWIF, Tb6_Txt_ENG_SWIF.Text);
                }
                if (Global.ENG_BNKNAM != Tb6_Txt_ENG_BNKNAM.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_BNKNAM", 2, "被仕向銀行名", Global.ENG_BNKNAM, Tb6_Txt_ENG_BNKNAM.Text);
                }
                if (Global.ENG_BRNNAM != Tb6_Txt_ENG_BRNNAM.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_BRNNAM", 2, "被仕向支店名", Global.ENG_BRNNAM, Tb6_Txt_ENG_BRNNAM.Text);
                }
                if (Global.ENG_BNKADDR != Tb6_Txt_ENG_BNKADDR.Text)
                {
                    Set_dtRIREKI(6, 0, "ENG_BNKADDR", 2, "被仕向銀行住所", Global.ENG_BNKADDR, Tb6_Txt_ENG_BNKADDR.Text);
                }

                if (Tb3_Rdo_GAI_F1.Checked == true)
                {
                    Global.HEI_CD = Tb6_Cmb_HEI_CD.Text;
                    Global.GAI_SF = (Tb6_Rdo_GAI_SF0.Checked == true ? "0" : "1");
                    Global.GAI_SH = (Tb6_Rdo_GAI_SH0.Checked == true ? "0" : "1");
                    Global.GAI_KZID = Tb6_Cmb_GAI_KZID.SelectedValue.ToString();
//-- <2016/03/09 非選択項目>
//                    Global.GAI_TF = mcBsLogic.Get_Tesuu_CD(Tb6_Cmb_GAI_TF.Text);
                    Global.GAI_TF = "1";
//-- <2016/03/09>       
                    Global.ENG_NAME = Tb6_Txt_ENG_NAME.Text;
                    Global.ENG_ADDR = Tb6_Txt_ENG_ADDR.Text;
                    Global.ENG_KZNO = Tb6_Txt_ENG_KZNO.Text;
                    Global.ENG_SWIF = Tb6_Txt_ENG_SWIF.Text;
                    Global.ENG_BNKNAM = Tb6_Txt_ENG_BNKNAM.Text;
                    Global.ENG_BRNNAM = Tb6_Txt_ENG_BRNNAM.Text;
                    Global.ENG_BNKADDR = Tb6_Txt_ENG_BNKADDR.Text;
                }
                else
                {
                    Global.HEI_CD = "";
                    Global.GAI_SF = "0";
                    Global.GAI_SH = "0";
                    Global.GAI_KZID = "";
////-- <2016/03/09 非選択項目>
////                    Global.GAI_TF = "";
//                    Global.GAI_TF = "1";
////-- <2016/03/09>
                    Global.GAI_TF = "";
                    Global.ENG_NAME = "";
                    Global.ENG_ADDR = "";
                    Global.ENG_KZNO = "";
                    Global.ENG_SWIF = "";
                    Global.ENG_BNKNAM = "";
                    Global.ENG_BRNNAM = "";
                    Global.ENG_BNKADDR = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/09 文言等>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGet_Tb6_Data　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/09>
            }
        }
        #endregion


        #region 入力チェック
        /// <summary>
        /// 手形管理のみ使用フラグON用の入力チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chk_DispVal_TGASW_ON()
        {
            try
            {
                //エラーをリセット
                Txt_TRCD.IsError = false;
                Txt_RYAKU.IsError = false;
                Txt_TORI_NAM.IsError = false;
                nErrFlg = 0;

                //必須チェック
                if ((Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                {
                    Txt_TRCD.IsError = true;
                    Txt_TRCD.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_TRCD.IsError = false;
                }
                if (Txt_RYAKU.Text == "")
                {
                    Txt_RYAKU.IsError = true;
                    Txt_RYAKU.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_RYAKU.IsError = false;
                }
                if (Txt_TORI_NAM.Text == "")
                {
                    Txt_TORI_NAM.IsError = true;
                    Txt_TORI_NAM.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_TORI_NAM.IsError = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_DispVal_TGASW_ON　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
//-- <2016/02/17 エラー時にはnErrFlgに1を立てておかないと処理が継続されてしまう構造では？>
                nErrFlg = 1;
//-- <2016/02/17>                
            }
        }

        //
        #region 見直し　チェック機能コメントアウト
//        /// <summary>
//        /// 手形管理のみ使用フラグOFF用の入力チェック
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void Chk_DispVal_TGASW_OFF()
//        {
//            try
//            {
//                //エラーをリセット
//                Txt_TRCD.IsError = false;
//                Txt_RYAKU.IsError = false;
//                Txt_TORI_NAM.IsError = false;
//                Txt_STYMD.IsError = false;
//                Tb5_Txt_HOVAL.IsError = false;
//                nErrFlg = 0;

//                //必須チェック
//                if (Txt_TRCD.ExCodeDB == "")
//                {
//                    Txt_TRCD.IsError = true;
//                    Txt_TRCD.Focus();
//                    nErrFlg = 1;
//                    return;
//                }
//                else
//                {
//                    Txt_TRCD.IsError = false;
//                }
//                if (Txt_RYAKU.Text == "")
//                {
//                    Txt_RYAKU.IsError = true;
//                    Txt_RYAKU.Focus();
//                    nErrFlg = 1;
//                    return;
//                }
//                else
//                {
//                    Txt_RYAKU.IsError = false;
//                }
//                if (Txt_TORI_NAM.Text == "")
//                {
//                    Txt_TORI_NAM.IsError = true;
//                    Txt_TORI_NAM.Focus();
//                    nErrFlg = 1;
//                    return;
//                }
//                else
//                {
//                    Txt_TORI_NAM.IsError = false;
//                }

//                // 得意先チェックON時
//                if (Chk_SAIKEN.Checked == true)
//                {
////-- <2016/02/16 照合用フリガナ未入力チェック及び負担手数料選択>
//                    if (Tb1_Txt_E_TANTOCD.Text != "")                                                               // 営業担当者
//                    {
//                        Tb_Main.SelectedIndex = 0;
//                        Tb1_Txt_E_TANTOCD.IsError = true;
//                        Tb1_Txt_E_TANTOCD.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
//                    if (Tb2_Cmb_FUTAN.SelectedIndex == -1)                                                          // 負担手数料コンボ
//                    {
//                        Tb_Main.SelectedIndex = 1;
//                        Tb2_Cmb_FUTAN.IsError = true;
//                        Tb2_Cmb_FUTAN.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
////-- <2016/02/16>
//                    if (Tb2_Chk_YAKUJO.Checked == false && Tb2_Cmb_KAISYU.SelectedIndex == -1)                      // 回収方法のチェック
//                    {
//                        Tb_Main.SelectedIndex = 1;
//                        Tb2_Cmb_KAISYU.IsError = true;
//                        Tb2_Cmb_KAISYU.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }

//                    if (Tb2_Txt_SHIME.Text == "")                                                                   // 回収締日
//                    {
//                        Tb_Main.SelectedIndex = 1;
//                        Tb2_Txt_SHIME.IsError = true;
//                        Tb2_Txt_SHIME.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
//                    if (Tb2_Txt_KAISYUHI_M.Text == "")                                                              // 回収期日月
//                    {
//                        Tb_Main.SelectedIndex = 1;
//                        Tb2_Txt_KAISYUHI_M.IsError = true;
//                        Tb2_Txt_KAISYUHI_M.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
//                    if (Tb2_Txt_KAISYUHI_D.Text == "")                                                              // 回収期日日
//                    {
//                        Tb_Main.SelectedIndex = 1;
//                        Tb2_Txt_KAISYUHI_D.IsError = true;
//                        Tb2_Txt_KAISYUHI_D.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
////-- <2016/02/17 回収締日と0ヶ月目の回収日 日と大小チェック>
//                    if (Tb2_Txt_KAISYUHI_M.Text == "0")
//                    {
//                        int nSHIME = 99;
//                        if (Tb2_Txt_SHIME.Text == "末" || Tb2_Txt_SHIME.Text == "99")
//                        {
//                            nSHIME = 99;
//                        }
//                        else { nSHIME = Convert.ToInt32(Tb2_Txt_SHIME.Text); }
//                        if (nSHIME > Convert.ToInt32(Tb2_Txt_KAISYUHI_D.Text == "末" ? "99" : Tb2_Txt_KAISYUHI_D.Text))
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_KAISYUHI_D.IsError = true;
//                            Tb2_Txt_KAISYUHI_D.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                    }
////-- <2016/02/17>


////-- <2016/02/15 入金区分ではなく入金種別で参照及び98：電子債権も入る>
////                    if (Tb2_Chk_YAKUJO.Checked == false && (Tb2_Cmb_KAISYU.SelectedValue.ToString() == "2" || Tb2_Cmb_KAISYU.SelectedValue.ToString() == "21" || Tb2_Cmb_KAISYU.SelectedValue.ToString() == "22"))
//                    if (Tb2_Chk_YAKUJO.Checked == false && mcBsLogic.Get_NKUBN(Tb2_Cmb_KAISYU.SelectedValue.ToString(), 1) == "1" )         // 入金種別が期日ありの場合
////-- <2016/02/15>
//                    {
//                        if (Tb2_Txt_KAISYUSIGHT_M.Text == "")                                               // 回収期日月
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_KAISYUSIGHT_M.IsError = true;
//                            Tb2_Txt_KAISYUSIGHT_M.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Txt_KAISYUSIGHT_D.Text == "")                                               // 回収期日日
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_KAISYUSIGHT_D.IsError = true;
//                            Tb2_Txt_KAISYUSIGHT_D.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                    }

//                    if (Tb2_Chk_YAKUJO.Checked == true)                                                     // 約定を指定にチェック
//                    {
//                        decimal nPer = 0;
//                        int nHasu = 1;
////-- <2016/02/17 Textの値を採用する>
////                        if (Tb2_Txt_Y_KINGAKU.ExNumValue == 0)                                              // 約定金額
//                        if (Tb2_Txt_Y_KINGAKU.Text == "")                                              // 約定金額
////-- <2016/02/17>
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_Y_KINGAKU.IsError = true;
//                            Tb2_Txt_Y_KINGAKU.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Cmb_HOLIDAY.SelectedIndex == -1)                                            // 休業日設定
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_HOLIDAY.IsError = true;
//                            Tb2_Cmb_HOLIDAY.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Cmb_MIMAN.SelectedIndex == -1)                                              // 約定金額未満
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_MIMAN.IsError = true;
//                            Tb2_Cmb_MIMAN.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
////-- <2016/02/16 約定金額未満の期日あり回収区分のエラーチェック>
//                        if (mcBsLogic.Get_NKUBN(Tb2_Cmb_MIMAN.SelectedValue.ToString(), 1) == "1")          // 約定金額未満の入金区分が期日ありの場合
//                        {
//                            if (Tb2_Txt_KAISYUSIGHT_M.Text == "")                                           // 回収期日月
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_KAISYUSIGHT_M.IsError = true;
//                                Tb2_Txt_KAISYUSIGHT_M.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
//                            if (Tb2_Txt_KAISYUSIGHT_D.Text == "")                                           // 回収期日日
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_KAISYUSIGHT_D.IsError = true;
//                                Tb2_Txt_KAISYUSIGHT_D.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
//                        }
////-- <2016/02/16>
//                        //①
//                        if(Tb2_Cmb_IJOU_1.SelectedIndex == -1)                                              // 約定金額以上①
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_IJOU_1.IsError = true;
//                            Tb2_Cmb_IJOU_1.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
////-- <2016/02/17 Textを使用する>
////                        if (Tb2_Txt_BUNKATSU_1.ExNumValue == 0)                                             // 分割率①
//                        if (Tb2_Txt_BUNKATSU_1.Text == "")                                             // 分割率①
////-- <2016/02/17>                        
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_BUNKATSU_1.IsError = true;
//                            Tb2_Txt_BUNKATSU_1.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        nPer = Tb2_Txt_BUNKATSU_1.ExNumValue;                                               // 分割率①をワークへ加算
//                        if (Tb2_Cmb_HASU_1.SelectedIndex == -1)                                             // 端数処理①
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_HASU_1.IsError = true;
//                            Tb2_Cmb_HASU_1.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
////-- <2016/02/16 実現したいのは端数処理が0の重複チェックでは？>
////                        nHasu = Tb2_Cmb_HASU_1.SelectedIndex;                                               // 端数処理コンボのインデックスをワークへ
//                        if (Tb2_Cmb_HASU_1.SelectedIndex == 0)
//                        { nHasu = 0; }
////-- <2016/02/16>
////-- <2016/02/17 入金区分が期日ありの場合>
//                        if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_1.SelectedValue.ToString(), 1) == "1")
//                        {
////-- <2016/02/17>
//                            if (Tb2_Txt_SIGHT_M_1.Text == "")   　                                                // 回収サイト月①
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_SIGHT_M_1.IsError = true;
//                                Tb2_Txt_SIGHT_M_1.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
//                            if (Tb2_Txt_SIGHT_D_1.Text == "")                                                     // 回収サイト日① 
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_SIGHT_D_1.IsError = true;
//                                Tb2_Txt_SIGHT_D_1.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
////-- <2016/02/17 追加>
//                        }
////-- <2016/02/17>
//                        //②
//                        if (Tb2_Cmb_IJOU_2.SelectedIndex != 0)                                               // 約定金額以上②
//                        {
//                            if (Tb2_Txt_BUNKATSU_2.ExNumValue == 0)                                          // 分割率②
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_BUNKATSU_2.IsError = true;
//                                Tb2_Txt_BUNKATSU_2.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
//                            nPer += Tb2_Txt_BUNKATSU_2.ExNumValue;                                           // 分割率②をワークへ加算
//                            if (Tb2_Cmb_HASU_2.SelectedIndex == -1)                                          // 端数処理②
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Cmb_HASU_2.IsError = true;
//                                Tb2_Cmb_HASU_2.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
////-- <2016/02/16 端数処理の0:端数重複チェックを追加>
////                            if (nHasu != 0)                                                                  // 端数処理チェック
////                            {
////                                nHasu = Tb2_Cmb_HASU_2.SelectedIndex;
////                            }
//                            if (nHasu == 0)                                                                    // 0:端数重複チェック
//                            {
//                                if (Tb2_Cmb_HASU_2.SelectedIndex == 0)
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Cmb_HASU_1.IsError = true;
//                                    Tb2_Cmb_HASU_1.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
//                            }
//                            else if (Tb2_Cmb_HASU_2.SelectedIndex == 0)
//                            {
//                                nHasu = 0;
//                            }
////-- <>
////-- <2016/02/17 約定金額以上②の入金区分が期日ありだったらを追加>
//                            if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_2.SelectedValue.ToString(), 1) == "1")
//                            {
////-- <2016/02/17>
//                                if (Tb2_Txt_SIGHT_M_2.Text == "")                                                   // 回収サイト月②
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Txt_SIGHT_M_2.IsError = true;
//                                    Tb2_Txt_SIGHT_M_2.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
//                                if (Tb2_Txt_SIGHT_D_2.Text == "")                                                    // 回収サイト日②
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Txt_SIGHT_D_2.IsError = true;
//                                    Tb2_Txt_SIGHT_D_2.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
////-- <2016/02/17 追加>
//                            }
////-- <2016/02/17>
//                        }
//                        //③
//                        if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
//                        {
//                            if (Tb2_Txt_BUNKATSU_3.ExNumValue == 0)                                                  // 分割率③
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Txt_BUNKATSU_3.IsError = true;
//                                Tb2_Txt_BUNKATSU_3.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
//                            nPer += Tb2_Txt_BUNKATSU_3.ExNumValue;                                                   // 分割率③をワークへ加算
//                            if (Tb2_Cmb_HASU_3.SelectedIndex == -1)                                                  // 端数処理③コンボ
//                            {
//                                Tb_Main.SelectedIndex = 1;
//                                Tb2_Cmb_HASU_3.IsError = true;
//                                Tb2_Cmb_HASU_3.Focus();
//                                nErrFlg = 1;
//                                return;
//                            }
////-- <2016/02/17 0:端数の重複チェックをしたいのでは？>
////                            if (nHasu != 0)                                                                          // 端数処理チェック
////                            {
////                                nHasu = Tb2_Cmb_HASU_3.SelectedIndex;
////                            }
//                            if (nHasu == 0)                                                                    // 0:端数重複チェック
//                            {
//                                if (Tb2_Cmb_HASU_3.SelectedIndex == 0)
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Cmb_HASU_1.IsError = true;
//                                    Tb2_Cmb_HASU_1.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
//                            }
//                            else if (Tb2_Cmb_HASU_3.SelectedIndex == 0)
//                            {
//                                nHasu = 0;
//                            }
////-- <2016/02/17>
////-- <2016/02/17 約定金額以上③の入金区分が期日ありの場合を追加>
//                            if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_3.SelectedValue.ToString(), 1) == "1")
//                            {
////-- <2016/02/17>
//                                if (Tb2_Txt_SIGHT_M_3.Text == "")                                                   // 回収期日月③
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Txt_SIGHT_M_3.IsError = true;
//                                    Tb2_Txt_SIGHT_M_3.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
//                                if (Tb2_Txt_SIGHT_D_3.Text == "")                                                   // 回収期日日③
//                                {
//                                    Tb_Main.SelectedIndex = 1;
//                                    Tb2_Txt_SIGHT_D_3.IsError = true;
//                                    Tb2_Txt_SIGHT_D_3.Focus();
//                                    nErrFlg = 1;
//                                    return;
//                                }
////-- <2016/02/17 追加>
//                            }
////-- <2016/02/17>
//                        }
//                        if (nPer != 100)
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_BUNKATSU_1.IsError = true;
//                            Tb2_Txt_BUNKATSU_1.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (nHasu != 0)
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_HASU_1.IsError = true;
//                            Tb2_Cmb_HASU_1.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        if (Tb2_Cmb_HOLIDAY.SelectedIndex == -1)
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_HOLIDAY.IsError = true;
//                            Tb2_Cmb_HOLIDAY.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                    }

////-- <2016/02/17 仮想入金口座フラグは必須を求めるものではないのでロジックを変更する>
////                    if (Global.nF_SENYOU == 1)                                                              // 管理テーブルで入金専用口座を使用するの場合
////                    {
////                        if(Tb2_Txt_SEN_GINKOCD.Text =="")
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Txt_SEN_GINKOCD.IsError = true;
////                            Tb2_Txt_SEN_GINKOCD.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                        if (Tb2_Txt_SEN_SITENCD.Text == "")
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Txt_SEN_SITENCD.IsError = true;
////                            Tb2_Txt_SEN_SITENCD.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                        if (Tb2_Txt_SEN_KSITENCD.Text == "")
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Txt_SEN_KSITENCD.IsError = true;
////                            Tb2_Txt_SEN_KSITENCD.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                        if (Tb2_Txt_SEN_KSITENNM.Text == "")
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Txt_SEN_KSITENNM.IsError = true;
////                            Tb2_Txt_SEN_KSITENNM.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                        if (Tb2_Cmb_YOKINSYU.SelectedIndex == -1)
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Cmb_YOKINSYU.IsError = true;
////                            Tb2_Cmb_YOKINSYU.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                        if (Tb2_Txt_SEN_KOZANO.Text == "")
////                        {
////                            Tb_Main.SelectedIndex = 1;
////                            Tb2_Txt_SEN_KOZANO.IsError = true;
////                            Tb2_Txt_SEN_KOZANO.Focus();
////                            nErrFlg = 1;
////                            return;
////                        }
////                
////                    }
//                    // 専用口座の何れかが揃わないとエラーにする。
//                    if (Tb2_Txt_SEN_GINKOCD.Text != "" || Tb2_Txt_SEN_SITENCD.Text != "" ||
//                        Tb2_Txt_SEN_KSITENCD.Text != "" || Tb2_Txt_SEN_KSITENNM.Text != "" || 
//                        Tb2_Cmb_YOKINSYU.SelectedIndex != -1 || Tb2_Txt_SEN_KOZANO.Text != "")
//                    {
//                        if (Tb2_Txt_SEN_GINKOCD.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_SEN_GINKOCD.IsError = true;
//                            Tb2_Txt_SEN_GINKOCD.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Txt_SEN_SITENCD.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_SEN_SITENCD.IsError = true;
//                            Tb2_Txt_SEN_SITENCD.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Txt_SEN_KSITENCD.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_SEN_KSITENCD.IsError = true;
//                            Tb2_Txt_SEN_KSITENCD.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Txt_SEN_KSITENNM.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_SEN_KSITENNM.IsError = true;
//                            Tb2_Txt_SEN_KSITENNM.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Cmb_YOKINSYU.SelectedIndex == -1)
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Cmb_YOKINSYU.IsError = true;
//                            Tb2_Cmb_YOKINSYU.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }
//                        if (Tb2_Txt_SEN_KOZANO.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 1;
//                            Tb2_Txt_SEN_KOZANO.IsError = true;
//                            Tb2_Txt_SEN_KOZANO.Focus();
//                            nErrFlg = 1;
//                            return;
//                        }                        
//                    }
////-- <2016/02/17>
//                }
             
//                //SS_TSHOHを登録しない場合はチェック不要
//                if (Tb3_Txt_SHINO.Text == "" && Tb3_Rdo_GAI_F0.Checked == true && Chk_SAIMU.Checked == true)
//                {
//                    Tb_Main.SelectedIndex = 2;
//                    Tb3_Txt_SHINO.IsError = true;
//                    Tb3_Txt_SHINO.Focus();
//                    nErrFlg = 1;
//                    return;
//                }
//                else
//                {
//                    Tb3_Txt_SHINO.IsError = false;
//                }

//                //振銀情報は全入力or全未入力

//                // 振込先情報メンテあり＆国内取引＆債務で使用する      コメント追加　2016/02/16
//                if (nDispChgFlg_FRIGIN == 1 && Tb3_Rdo_GAI_F0.Checked == true && Chk_SAIMU.Checked == true)
//                {
//                    ////if ((Tb2_Txt_BANK_CD.Text != "" && Tb2_Txt_SITEN_ID.Text != "" && Tb2_Txt_KOUZA.Text != "" && Tb2_Txt_MEIGI.Text != "" && Tb2_Txt_MEIGIK.Text != "") ||
//                    ////    (Tb2_Txt_BANK_CD.Text == "" && Tb2_Txt_SITEN_ID.Text == "" && Tb2_Txt_KOUZA.Text == "" && Tb2_Txt_MEIGI.Text == "" && Tb2_Txt_MEIGIK.Text == ""))
////-- <>
////                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "")
//                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "" &&
//                        Tb4_Cmb_FTESUID.SelectedIndex != -1)
////-- <>
//                    {
//                        Tb4_Txt_BANK_CD.IsError = false;
//                        Tb4_Txt_SITEN_ID.IsError = false;
//                        Tb4_Txt_KOUZA.IsError = false;
//                        Tb4_Txt_MEIGI.IsError = false;
//                        Tb4_Txt_MEIGIK.IsError = false;
////-- <>
//                        Tb4_Cmb_FTESUID.IsError = false;
////-- <>
//                    }
//                    else
//                    {
//                        if (Tb4_Txt_BANK_CD.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_BANK_CD.IsError = true;
//                            Tb4_Txt_BANK_CD.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Txt_SITEN_ID.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_SITEN_ID.IsError = true;
//                            Tb4_Txt_SITEN_ID.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Txt_KOUZA.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_KOUZA.IsError = true;
//                            Tb4_Txt_KOUZA.Focus();
//                            nErrFlg = 1;
//                        }
//                        //else if (Tb2_Txt_MEIGI.Text == "")
//                        //{
//                        //    Tb2_Txt_MEIGI.IsError = true;
//                        //    Tb2_Txt_MEIGI.Focus();
//                        //}
//                        else if (Tb4_Txt_MEIGIK.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_MEIGIK.IsError = true;
//                            Tb4_Txt_MEIGIK.Focus();
//                            nErrFlg = 1;
//                        }
////-- <>                 
//                        else if (Tb4_Cmb_FTESUID.SelectedIndex == -1)
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Cmb_FTESUID.IsError = true;
//                            Tb4_Cmb_FTESUID.Focus();
//                            nErrFlg = 1;
//                        }
////-- <>
//                        if (nErrFlg == 1)
//                        {
//                            //Tb_Main.SelectedIndex = 1;
//                            return;
//                        }
//                    }
//                }
//                // 国内取引、仕入先がチェックオンの場合
//                else if (Tb3_Rdo_GAI_F0.Checked == true && Chk_SAIMU.Checked == true)
//                {
////-- <2016/02/ 見直し>
//                    //if (Tb4_Txt_BANK_CD.Text != "" || Tb4_Txt_SITEN_ID.Text != "" || Tb4_Txt_KOUZA.Text != "" || Tb4_Txt_MEIGI.Text != "" || Tb4_Txt_MEIGIK.Text != "")
//                    //{
//                    //    if (Tb4_Txt_BANK_CD.Text == "")
//                    //    {
//                    //        Tb_Main.SelectedIndex = 3;
//                    //        Tb4_Txt_BANK_CD.IsError = true;
//                    //        Tb4_Txt_BANK_CD.Focus();
//                    //        nErrFlg = 1;
//                    //    }
//                    //    else if (Tb4_Txt_SITEN_ID.Text == "")
//                    //    {
//                    //        Tb_Main.SelectedIndex = 3;
//                    //        Tb4_Txt_SITEN_ID.IsError = true;
//                    //        Tb4_Txt_SITEN_ID.Focus();
//                    //        nErrFlg = 1;
//                    //    }
//                    //    else if (Tb4_Txt_KOUZA.Text == "")
//                    //    {
//                    //        Tb_Main.SelectedIndex = 3;
//                    //        Tb4_Txt_KOUZA.IsError = true;
//                    //        Tb4_Txt_KOUZA.Focus();
//                    //        nErrFlg = 1;
//                    //    }
//                    //    else if (Tb4_Txt_MEIGIK.Text == "")
//                    //    {
//                    //        Tb_Main.SelectedIndex = 3;
//                    //        Tb4_Txt_MEIGIK.IsError = true;
//                    //        Tb4_Txt_MEIGIK.Focus();
//                    //        nErrFlg = 1;
//                    //    }

//                    //    if (nErrFlg == 1)
//                    //    {
//                    //        //Tb_Main.SelectedIndex = 1;
//                    //        return;
//                    //    }
//                    //}
//                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "" &&
//                        Tb4_Cmb_FTESUID.SelectedIndex != -1)
//                    {

//                    }
//                    else
//                    {
//                        if (Tb4_Txt_BANK_CD.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_BANK_CD.IsError = true;
//                            Tb4_Txt_BANK_CD.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Txt_SITEN_ID.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_SITEN_ID.IsError = true;
//                            Tb4_Txt_SITEN_ID.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Txt_KOUZA.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_KOUZA.IsError = true;
//                            Tb4_Txt_KOUZA.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Txt_MEIGIK.Text == "")
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Txt_MEIGIK.IsError = true;
//                            Tb4_Txt_MEIGIK.Focus();
//                            nErrFlg = 1;
//                        }
//                        else if (Tb4_Cmb_FTESUID.SelectedIndex == -1)
//                        {
//                            Tb_Main.SelectedIndex = 3;
//                            Tb4_Cmb_FTESUID.IsError = true;
//                            Tb4_Cmb_FTESUID.Focus();
//                            nErrFlg = 1;
//                        }

//                        if (nErrFlg == 1)
//                        {
//                            //Tb_Main.SelectedIndex = 1;
//                            return;
//                        }
//                    }
////-- <>
//                }

//                //取引停止の開始＆終了年月日大小chk
//                if (Txt_STYMD.Value != 0 && Txt_EDYMD.Value != 0)
//                {
//                    if (Txt_STYMD.Value > Txt_EDYMD.Value)
//                    {
//                        Txt_STYMD.IsError = true;
//                        Txt_STYMD.Focus();
//                        nErrFlg = 1;
//                        return;
//                    }
//                }

//                if (Tb5_Rdo_HORYU0.Checked == false && Tb5_Txt_HOVAL.Text == "100.0")
//                {
//                    Tb5_Txt_HOVAL.IsError = true;
//                    Tb_Main.SelectedIndex = 4;
//                    Tb5_Txt_HOVAL.Focus();
//                    nErrFlg = 1;
//                    //return;
//                }

//                if (mcBsLogic.Chk_GinFuriSKBN(Txt_TRCD.ExCodeDB, Txt_HJCD.Text.Trim() == "" ? "0" : Txt_HJCD.Text.Trim(), Tb3_Txt_SHINO.Text, BindNavi2_Selected.Text))
//                {
//                    if (Tb4_Txt_BANK_CD.Text.Trim() == "")
//                    {
//                        Tb_Main.SelectedIndex = 3;
//                        Tb4_Txt_BANK_CD.IsError = true;
//                        Tb4_Txt_BANK_CD.Focus();
//                        nErrFlg = 1;
//                        //Tb_Main.SelectedIndex = 1;
//                        return;
//                    }
//                }
////-- <>
//                if (!Chk_TGASW.Checked && !Chk_SAIKEN.Checked && !Chk_SAIMU.Checked)
//                {
//                    Chk_TGASW.IsError = true;
//                    Chk_TGASW.Focus();
//                    nErrFlg = 1;
//                    return;
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
////-- <2016/02/17 エラー時にはフラグをたてておく>
//                nErrFlg = 1;
////-- <2016/02/17>
//            }
//        }
        #endregion
        /// <summary>
        /// 手形管理のみ使用フラグOFF用の入力チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chk_DispVal_TGASW_OFF()
        {
//-- <2016/03/15 支払条件で銀行無し状況フラグ>
            bool bFRIJYOUHO = false;
//-- <2016/03/15>
            try
            {
                //エラーをリセット
                Txt_TRCD.IsError = false;
                Txt_RYAKU.IsError = false;
                Txt_TORI_NAM.IsError = false;
                Txt_STYMD.IsError = false;
                Tb5_Txt_HOVAL.IsError = false;
                nErrFlg = 0;

                //必須チェック
                if ((Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                {
                    Txt_TRCD.IsError = true;
                    Txt_TRCD.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_TRCD.IsError = false;
                }
                if (Txt_RYAKU.Text == "")
                {
                    Txt_RYAKU.IsError = true;
                    Txt_RYAKU.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_RYAKU.IsError = false;
                }
                if (Txt_TORI_NAM.Text == "")
                {
                    Txt_TORI_NAM.IsError = true;
                    Txt_TORI_NAM.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Txt_TORI_NAM.IsError = false;
                }

                // 得意先チェックON時
                if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
                {
                    //営業担当者
                    if (Tb1_Txt_E_TANTOCD.Text == "")
                    {
                        Tb_Main.SelectedIndex = 0;
                        Tb1_Txt_E_TANTOCD.IsError = true;
                        Tb1_Txt_E_TANTOCD.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    // ---> V02.20.01 SLH ADD ▼(105046)
                    //敬称区分
                    if (Tb1_Cmb_KEICD.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 0;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        MessageBox.Show(
                            "敬称が選択されていません。",
                            Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Tb1_Cmb_KEICD.IsError = true;
                        Tb1_Cmb_KEICD.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    // <--- V02.20.01 SLH ADD ▲(105046)

//-- <2016/03/14 照合用フリガナ必須を追加>
                    //照合用フリガナ
                    if (Tb2_Txt_TOKUKANA.Text == "")
                    {
                        Tb_Main.SelectedIndex = 1;
                        Tb2_Txt_TOKUKANA.IsError = true;
                        Tb2_Txt_TOKUKANA.Focus();
                        nErrFlg = 1;
                        return;
                    }
//-- <2016/03/14>
                    
                    //負担手数料コンボ
                    if (Tb2_Cmb_FUTAN.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 1;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb2_Cmb_FUTAN.IsError = true;
                        Tb2_Cmb_FUTAN.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    //外貨使用する
                    if (Tb2_Chk_GAIKA.Checked)
                    {
                        //取引通貨コンボ
                        if (Tb2_Cmb_TSUKA.SelectedIndex == -1)
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_TSUKA.IsError = true;
                            Tb2_Cmb_TSUKA.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        //照合キー(前）→ 必須解除(#123626)
                        //if (Tb2_Txt_GAIKA_KEY_F.Text == "")
                        //{
                        //    Tb_Main.SelectedIndex = 1;
                        //    Tb2_Txt_GAIKA_KEY_F.IsError = true;
                        //    Tb2_Txt_GAIKA_KEY_F.Focus();
                        //    nErrFlg = 1;
                        //    return;
                        //}
                    }

                    //回収方法のチェック
                    if (Tb2_Chk_YAKUJO.Checked == false && Tb2_Cmb_KAISYU.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 1;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb2_Cmb_KAISYU.IsError = true;
                        Tb2_Cmb_KAISYU.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    // 締日未入力
                    if (Tb2_Txt_SHIME.Text == "")
                    {
                        Tb_Main.SelectedIndex = 1;
                        Tb2_Txt_SHIME.IsError = true;
                        Tb2_Txt_SHIME.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    //回収予定日　月未入力
                    if (Tb2_Txt_KAISYUHI_M.Text == "")
                    {
                        Tb_Main.SelectedIndex = 1;
                        Tb2_Txt_KAISYUHI_M.IsError = true;
                        Tb2_Txt_KAISYUHI_M.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //回収予定日　日未入力
                    if (Tb2_Txt_KAISYUHI_D.Text == "")
                    {
                        Tb_Main.SelectedIndex = 1;
                        Tb2_Txt_KAISYUHI_D.IsError = true;
                        Tb2_Txt_KAISYUHI_D.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    //回収締日と0ヶ月目の回収日 日と大小チェック
                    if (Tb2_Txt_KAISYUHI_M.Text == "0")
                    {
                        int nSHIME = 99;
                        if (Tb2_Txt_SHIME.Text == "末" || Tb2_Txt_SHIME.Text == "99")
                        {
                            nSHIME = 99;
                        }
                        else { nSHIME = Convert.ToInt32(Tb2_Txt_SHIME.Text); }
                        if (nSHIME > Convert.ToInt32(Tb2_Txt_KAISYUHI_D.Text == "末" ? "99" : Tb2_Txt_KAISYUHI_D.Text))
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_KAISYUHI_D.IsError = true;
                            Tb2_Txt_KAISYUHI_D.Focus();
                            nErrFlg = 1;
                            return;
                        }
                    }

                    // 入金種別が期日ありの場合　回収期日未入力
                    if (Tb2_Chk_YAKUJO.Checked == false 
                        && mcBsLogic.Get_NKUBN(Tb2_Cmb_KAISYU.SelectedValue.ToString(), 1) == "1")
                    {
                        // 回収期日月
                        if (Tb2_Txt_KAISYUSIGHT_M.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_KAISYUSIGHT_M.IsError = true;
                            Tb2_Txt_KAISYUSIGHT_M.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        // 回収期日日
                        if (Tb2_Txt_KAISYUSIGHT_D.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_KAISYUSIGHT_D.IsError = true;
                            Tb2_Txt_KAISYUSIGHT_D.Focus();
                            nErrFlg = 1;
                            return;
                        }
                    }

                    // 休業日設定
                    if (Tb2_Cmb_HOLIDAY.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 1;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb2_Cmb_HOLIDAY.IsError = true;
                        Tb2_Cmb_HOLIDAY.Focus();
                        nErrFlg = 1;
                        return;
                    }

                    // 約定を指定にチェック
                    if (Tb2_Chk_YAKUJO.Checked == true)
                    {
                        decimal nPer = 0;
                        int nHasu = 1;
                        
                        //-- <2016/02/17 Textの値を採用する>
                        // 約定金額
                        if (Tb2_Txt_Y_KINGAKU.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_Y_KINGAKU.IsError = true;
                            Tb2_Txt_Y_KINGAKU.Focus();
                            nErrFlg = 1;
                            return;
                        }

                        // 約定金額未満
                        if (Tb2_Cmb_MIMAN.SelectedIndex == -1)                                              
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_MIMAN.IsError = true;
                            Tb2_Cmb_MIMAN.Focus();
                            nErrFlg = 1;
                            return;
                        }

                        // 約定金額未満の入金区分が期日ありの場合
                        if (mcBsLogic.Get_NKUBN(Tb2_Cmb_MIMAN.SelectedValue.ToString(), 1) == "1")          
                        {
                            // 回収期日月
                            if (Tb2_Txt_KAISYUSIGHT_M.Text == "")                                           
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_KAISYUSIGHT_M.IsError = true;
                                Tb2_Txt_KAISYUSIGHT_M.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            // 回収期日日
                            if (Tb2_Txt_KAISYUSIGHT_D.Text == "")                                           
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_KAISYUSIGHT_D.IsError = true;
                                Tb2_Txt_KAISYUSIGHT_D.Focus();
                                nErrFlg = 1;
                                return;
                            }
                        }

                        // 約定金額以上①
                        if (Tb2_Cmb_IJOU_1.SelectedIndex == -1)                                              
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_IJOU_1.IsError = true;
                            Tb2_Cmb_IJOU_1.Focus();
                            nErrFlg = 1;
                            return;
                        }

                        //-- <Textを使用する>
                        // 分割率①                                             
                        if (Tb2_Txt_BUNKATSU_1.Text == "")                                            
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_BUNKATSU_1.IsError = true;
                            Tb2_Txt_BUNKATSU_1.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        // 分割率①をワークへ加算
                        nPer = Tb2_Txt_BUNKATSU_1.ExNumValue;
                        // 端数処理①                    
                        if (Tb2_Cmb_HASU_1.SelectedIndex == -1)                                             
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_HASU_1.IsError = true;
                            Tb2_Cmb_HASU_1.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        // 端数処理コンボのインデックスをワークへ                                               
                        if (Tb2_Cmb_HASU_1.SelectedIndex == 0)
                        { nHasu = 0; }
                        //-- <入金区分が期日ありの場合>
                        // 約定金額以上①
                        if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_1.SelectedValue.ToString(), 1) == "1")
                        {
                            // 回収サイト月①
                            if (Tb2_Txt_SIGHT_M_1.Text == "")   　                                                
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_SIGHT_M_1.IsError = true;
                                Tb2_Txt_SIGHT_M_1.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            // 回収サイト日① 
                            if (Tb2_Txt_SIGHT_D_1.Text == "")                                                     
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_SIGHT_D_1.IsError = true;
                                Tb2_Txt_SIGHT_D_1.Focus();
                                nErrFlg = 1;
                                return;
                            }
                        }

                        // 約定金額以上②
                        if (Tb2_Cmb_IJOU_2.SelectedIndex != 0)                                               
                        {
                            // 分割率②
                            if (Tb2_Txt_BUNKATSU_2.ExNumValue == 0)                                          
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_BUNKATSU_2.IsError = true;
                                Tb2_Txt_BUNKATSU_2.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            // 分割率②をワークへ加算
                            nPer += Tb2_Txt_BUNKATSU_2.ExNumValue;
                            // 端数処理②           
                            if (Tb2_Cmb_HASU_2.SelectedIndex == -1)                                          
                            {
                                Tb_Main.SelectedIndex = 1;
                                // ---> V02.21.01 KKL ADD ▼(106612)
                                SendKeys.SendWait("{ENTER}");
                                SendKeys.SendWait("+{TAB}");
                                // <--- V02.21.01 KKL ADD ▲(106612)
                                Tb2_Cmb_HASU_2.IsError = true;
                                Tb2_Cmb_HASU_2.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            //-- <端数処理の0:端数重複チェックを追加>
                            // 0:端数重複チェック
                            if (nHasu == 0)                                                                    
                            {
                                if (Tb2_Cmb_HASU_2.SelectedIndex == 0)
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Cmb_HASU_1.IsError = true;
                                    Tb2_Cmb_HASU_1.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                            }
                            else if (Tb2_Cmb_HASU_2.SelectedIndex == 0)
                            {
                                nHasu = 0;
                            }
                            //-- <約定金額以上②の入金区分が期日ありだったらを追加>
                            if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_2.SelectedValue.ToString(), 1) == "1")
                            {
                                // 回収サイト月②
                                if (Tb2_Txt_SIGHT_M_2.Text == "")                                                   
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Txt_SIGHT_M_2.IsError = true;
                                    Tb2_Txt_SIGHT_M_2.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                                // 回収サイト日②
                                if (Tb2_Txt_SIGHT_D_2.Text == "")                                                    
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Txt_SIGHT_D_2.IsError = true;
                                    Tb2_Txt_SIGHT_D_2.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                            }
                        }
//-- <2016/03/08 追加>
                        if (Tb2_Cmb_IJOU_2.Enabled && Tb2_Cmb_IJOU_2.SelectedIndex - 1 == Tb2_Cmb_IJOU_1.SelectedIndex)
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Cmb_IJOU_2.Focus();
                            Tb2_Cmb_IJOU_2.IsError = true;
                            nErrFlg = 1;
                            return;
                        }
//-- <2016/03/08>
                        //約定金額以上③
                        if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
                        {
                            // 分割率③
                            if (Tb2_Txt_BUNKATSU_3.ExNumValue == 0)                                                  
                            {
                                Tb_Main.SelectedIndex = 1;
                                Tb2_Txt_BUNKATSU_3.IsError = true;
                                Tb2_Txt_BUNKATSU_3.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            // 分割率③をワークへ加算
                            nPer += Tb2_Txt_BUNKATSU_3.ExNumValue;
                            // 端数処理③コンボ                     
                            if (Tb2_Cmb_HASU_3.SelectedIndex == -1)                                                  
                            {
                                Tb_Main.SelectedIndex = 1;
                                // ---> V02.21.01 KKL ADD ▼(106612)
                                SendKeys.SendWait("{ENTER}");
                                SendKeys.SendWait("+{TAB}");
                                // <--- V02.21.01 KKL ADD ▲(106612)
                                Tb2_Cmb_HASU_3.IsError = true;
                                Tb2_Cmb_HASU_3.Focus();
                                nErrFlg = 1;
                                return;
                            }
                            // 0:端数重複チェック                          
                            if (nHasu == 0)                                                                    
                            {
                                if (Tb2_Cmb_HASU_3.SelectedIndex == 0)
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Cmb_HASU_1.IsError = true;
                                    Tb2_Cmb_HASU_1.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                            }
                            else if (Tb2_Cmb_HASU_3.SelectedIndex == 0)
                            {
                                nHasu = 0;
                            }

                            //-- <約定金額以上③の入金区分が期日ありの場合を追加>
                            if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_3.SelectedValue.ToString(), 1) == "1")
                            {
                                // 回収期日月③
                                if (Tb2_Txt_SIGHT_M_3.Text == "")                                                   
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Txt_SIGHT_M_3.IsError = true;
                                    Tb2_Txt_SIGHT_M_3.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                                // 回収期日日③
                                if (Tb2_Txt_SIGHT_D_3.Text == "")                                                   
                                {
                                    Tb_Main.SelectedIndex = 1;
                                    Tb2_Txt_SIGHT_D_3.IsError = true;
                                    Tb2_Txt_SIGHT_D_3.Focus();
                                    nErrFlg = 1;
                                    return;
                                }
                            }
                        }
//-- <2016/03/08 追加>
                        if (Tb2_Cmb_IJOU_3.Enabled && 
                            (Tb2_Cmb_IJOU_3.SelectedIndex - 1 == Tb2_Cmb_IJOU_1.SelectedIndex
                            || Tb2_Cmb_IJOU_3.SelectedIndex == Tb2_Cmb_IJOU_2.SelectedIndex))
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Cmb_IJOU_3.Focus();
                            Tb2_Cmb_IJOU_3.IsError = true;
                            nErrFlg = 1;
                            return;
                        }
//-- <2016/03/08>
                        if (nPer != 100)
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_BUNKATSU_1.IsError = true;
                            Tb2_Txt_BUNKATSU_1.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (nHasu != 0)
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Cmb_HASU_1.IsError = true;
                            Tb2_Cmb_HASU_1.Focus();
                            nErrFlg = 1;
                            return;
                        }
                    }
                    else
                    {
                        if (Tb2_Cmb_HOLIDAY.SelectedIndex == -1)
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_HOLIDAY.IsError = true;
                            Tb2_Cmb_HOLIDAY.Focus();
                            nErrFlg = 1;
                            return;
                        }
                    }

                    // 専用口座の何れかが揃わないとエラーにする。
                    if (Tb2_Txt_SEN_GINKOCD.Text != "" || Tb2_Txt_SEN_SITENCD.Text != "" ||
                        Tb2_Txt_SEN_KSITENCD.Text != "" || Tb2_Txt_SEN_KSITENNM.Text != "" ||
                        Tb2_Cmb_YOKINSYU.SelectedIndex != -1 || Tb2_Txt_SEN_KOZANO.Text != "")
                    {
                        if (Tb2_Txt_SEN_GINKOCD.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_SEN_GINKOCD.IsError = true;
                            Tb2_Txt_SEN_GINKOCD.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (Tb2_Txt_SEN_SITENCD.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_SEN_SITENCD.IsError = true;
                            Tb2_Txt_SEN_SITENCD.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (Tb2_Txt_SEN_KSITENCD.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_SEN_KSITENCD.IsError = true;
                            Tb2_Txt_SEN_KSITENCD.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (Tb2_Txt_SEN_KSITENNM.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_SEN_KSITENNM.IsError = true;
                            Tb2_Txt_SEN_KSITENNM.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (Tb2_Cmb_YOKINSYU.SelectedIndex == -1)
                        {
                            Tb_Main.SelectedIndex = 1;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb2_Cmb_YOKINSYU.IsError = true;
                            Tb2_Cmb_YOKINSYU.Focus();
                            nErrFlg = 1;
                            return;
                        }
                        if (Tb2_Txt_SEN_KOZANO.Text == "")
                        {
                            Tb_Main.SelectedIndex = 1;
                            Tb2_Txt_SEN_KOZANO.IsError = true;
                            Tb2_Txt_SEN_KOZANO.Focus();
                            nErrFlg = 1;
                            return;
                        }
                    }

                }

                // ---> V02.20.01 SLH ADD ▼(105046)
                //敬称区分
                if (Tb1_Cmb_KEICD.SelectedIndex == -1)
                {
                    Tb_Main.SelectedIndex = 0;
                    // ---> V02.21.01 KKL ADD ▼(106612)
                    SendKeys.SendWait("{ENTER}");
                    SendKeys.SendWait("+{TAB}");
                    // <--- V02.21.01 KKL ADD ▲(106612)
                    MessageBox.Show(
                        "敬称が選択されていません。",
                        Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Tb1_Cmb_KEICD.IsError = true;
                    Tb1_Cmb_KEICD.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Tb1_Cmb_KEICD.IsError = false;
                }
                // <--- V02.20.01 SLH ADD ▲(105046)

                //SS_TSHOHを登録しない場合はチェック不要
                if (Tb3_Txt_SHINO.Text == "" && Tb3_Rdo_GAI_F0.Checked == true && Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    Tb_Main.SelectedIndex = 2;
                    Tb3_Txt_SHINO.IsError = true;
                    Tb3_Txt_SHINO.Focus();
                    nErrFlg = 1;
                    return;
                }
                else
                {
                    Tb3_Txt_SHINO.IsError = false;
//-- <2016/03/15 >
//-- <2016/04/03>
//                // これだと、小切手はチェック対象となるので作り替え
//                    if (Tb3_Cmb_HARAI_KBN1.SelectedIndex != -1 || Tb3_Cmb_HARAI_KBN2.SelectedIndex != -1 ||
//                        Tb3_Cmb_HARAI_KBN3.SelectedIndex != -1 || Tb3_Cmb_HARAI_KBN4.SelectedIndex != -1)
//                    {
//                        bFRIJYOUHO = true;
//                    }
//                    else { bFRIJYOUHO = false; }
                //支払種別を参照する
                    string sKIND1 = "";
                    string sKIND2 = "";
                    string sKIND3 = "";
                    string sKIND4 = "";
//-- <2016/04/09 海外の場合は無視>
                    if (Tb3_Rdo_GAI_F1.Checked == true)
                    {
                        bFRIJYOUHO = false;
                    }
                    else
                    {
//-- <2016/04/09>
                        if (Global.KUBN1_tb3 != "")
                        {
                            sKIND1 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':')));
                        }
                        else { sKIND1 = ""; }

                        if (Global.KUBN2_tb3 != "")
                        {
                            sKIND2 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':')));
                        }
                        else { sKIND2 = ""; }

                        if (Global.KUBN3_tb3 != "")
                        {
                            sKIND3 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':')));
                        }
                        else { sKIND3 = ""; }
                        if (Global.KUBN4_tb3 != "")
                        {
                            sKIND4 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':')));
                        }
                        else { sKIND4 = ""; }

                        if (sKIND1 == "6" || sKIND1 == "7" || sKIND1 == "8" || sKIND1 == "12" ||
                            sKIND2 == "6" || sKIND2 == "7" || sKIND2 == "8" || sKIND2 == "12" ||
                            sKIND3 == "6" || sKIND3 == "7" || sKIND3 == "8" || sKIND3 == "12" ||
                            sKIND4 == "6" || sKIND4 == "7" || sKIND4 == "8" || sKIND4 == "12")
                        {
                            bFRIJYOUHO = true;
                        }
                        else { bFRIJYOUHO = false; }
//-- <2016/04/09>
                    }
//-- <2016/04/09>
//-- <2016/04/03>
                }

                //振銀情報は全入力or全未入力

                // 振込先情報メンテあり＆国内取引＆債務で使用する      コメント追加　2016/02/16
                if (nDispChgFlg_FRIGIN == 1 && Tb3_Rdo_GAI_F0.Checked == true && Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    ////if ((Tb2_Txt_BANK_CD.Text != "" && Tb2_Txt_SITEN_ID.Text != "" && Tb2_Txt_KOUZA.Text != "" && Tb2_Txt_MEIGI.Text != "" && Tb2_Txt_MEIGIK.Text != "") ||
                    ////    (Tb2_Txt_BANK_CD.Text == "" && Tb2_Txt_SITEN_ID.Text == "" && Tb2_Txt_KOUZA.Text == "" && Tb2_Txt_MEIGI.Text == "" && Tb2_Txt_MEIGIK.Text == ""))
                    //-- <>
                    //                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "")
                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "" &&
                        Tb4_Cmb_FTESUID.SelectedIndex != -1)
                    //-- <>
                    {
                        Tb4_Txt_BANK_CD.IsError = false;
                        Tb4_Txt_SITEN_ID.IsError = false;
                        Tb4_Txt_KOUZA.IsError = false;
                        Tb4_Txt_MEIGI.IsError = false;
                        Tb4_Txt_MEIGIK.IsError = false;
                        //-- <>
                        Tb4_Cmb_FTESUID.IsError = false;
                        //-- <>
                    }
                    else
                    {
                        if (Tb4_Txt_BANK_CD.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_BANK_CD.IsError = true;
                            Tb4_Txt_BANK_CD.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Txt_SITEN_ID.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_SITEN_ID.IsError = true;
                            Tb4_Txt_SITEN_ID.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Txt_KOUZA.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_KOUZA.IsError = true;
                            Tb4_Txt_KOUZA.Focus();
                            nErrFlg = 1;
                       }
                        else if (Tb4_Txt_MEIGIK.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_MEIGIK.IsError = true;
                            Tb4_Txt_MEIGIK.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Cmb_FTESUID.SelectedIndex == -1)
                        {
                            Tb_Main.SelectedIndex = 3;
                            // ---> V02.21.01 KKL ADD ▼(106612)
                            SendKeys.SendWait("{ENTER}");
                            SendKeys.SendWait("+{TAB}");
                            // <--- V02.21.01 KKL ADD ▲(106612)
                            Tb4_Cmb_FTESUID.IsError = true;
                            Tb4_Cmb_FTESUID.Focus();
                            nErrFlg = 1;
                        }
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                // 国内取引、仕入先がチェックオンの場合
//-- <>
//                else if (Tb3_Rdo_GAI_F0.Checked == true && Chk_SAIMU.Checked == true)
                else if (Tb3_Rdo_GAI_F0.Checked == true && Cbo_SAIMU.SelectedValue.ToString() == sUse && bFRIJYOUHO)
//-- <>
                {
                    if (Tb4_Txt_BANK_CD.Text != "" && Tb4_Txt_SITEN_ID.Text != "" && Tb4_Txt_KOUZA.Text != "" && Tb4_Txt_MEIGI.Text != "" && Tb4_Txt_MEIGIK.Text != "" &&
                        Tb4_Cmb_FTESUID.SelectedIndex != -1)
                    {

                    }
                    else
                    {
                        if (Tb4_Txt_BANK_CD.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_BANK_CD.IsError = true;
                            Tb4_Txt_BANK_CD.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Txt_SITEN_ID.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_SITEN_ID.IsError = true;
                            Tb4_Txt_SITEN_ID.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Txt_KOUZA.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_KOUZA.IsError = true;
                            Tb4_Txt_KOUZA.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Txt_MEIGIK.Text == "")
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Txt_MEIGIK.IsError = true;
                            Tb4_Txt_MEIGIK.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb4_Cmb_FTESUID.SelectedIndex == -1)
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Cmb_FTESUID.IsError = true;
                            Tb4_Cmb_FTESUID.Focus();
                            nErrFlg = 1;
                        }
                        else if (Tb2_Chk_DTESUSW.Checked)
                        {
                            if (Tb2_Cmb_DTESU.SelectedIndex == -1)
                            {
                                Tb_Main.SelectedIndex = 3;
                                // ---> V02.21.01 KKL ADD ▼(106612)
                                SendKeys.SendWait("{ENTER}");
                                SendKeys.SendWait("+{TAB}");
                                // <--- V02.21.01 KKL ADD ▲(106612)
                                Tb2_Cmb_DTESU.IsError = true;
                                Tb2_Cmb_DTESU.Focus();
                                nErrFlg = 1;
                            }
                        }
                        // その他タブ
                        else if (Tb5_Chk_GENSEN.Checked || Tb5_Chk_OUTPUT.Checked)
                        {
                            if (Tb5_Cmb_GOU.SelectedIndex == -1)
                            {
                                Tb_Main.SelectedIndex = 4;
                                // ---> V02.21.01 KKL ADD ▼(106612)
                                SendKeys.SendWait("{ENTER}");
                                SendKeys.SendWait("+{TAB}");
                                // <--- V02.21.01 KKL ADD ▲(106612)
                                Tb5_Cmb_GOU.IsError = true;
                                Tb5_Cmb_GOU.Focus();
                                nErrFlg = 1;
                            }
                            if (Tb5_Cmb_GGKBN.SelectedIndex == -1)
                            {
                                Tb_Main.SelectedIndex = 4;
                                // ---> V02.21.01 KKL ADD ▼(106612)
                                SendKeys.SendWait("{ENTER}");
                                SendKeys.SendWait("+{TAB}");
                                // <--- V02.21.01 KKL ADD ▲(106612)
                                Tb5_Cmb_GGKBN.IsError = true;
                                Tb5_Cmb_GGKBN.Focus();
                                nErrFlg = 1;
                            }
                            if (Tb5_Chk_GENSEN.Checked)
                            {
                                if (Tb5_Cmb_GSKUBN.SelectedIndex == -1)
                                {
                                    Tb_Main.SelectedIndex = 4;
                                    // ---> V02.21.01 KKL ADD ▼(106612)
                                    SendKeys.SendWait("{ENTER}");
                                    SendKeys.SendWait("+{TAB}");
                                    // <--- V02.21.01 KKL ADD ▲(106612)
                                    Tb5_Cmb_GSKUBN.IsError = true;
                                    Tb5_Cmb_GSKUBN.Focus();
                                    nErrFlg = 1;
                                }
                            }
                        }
                        else if (!Tb5_Rdo_HORYU0.Checked)
                        {

                        }
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                // 海外
                if (Tb3_Rdo_GAI_F1.Checked == true && Cbo_SAIMU.SelectedValue.ToString() == sUse)
                {
                    //取引通貨
                    if (Tb6_Cmb_HEI_CD.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 5;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb6_Cmb_HEI_CD.IsError = true;
                        Tb6_Cmb_HEI_CD.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //出金口座
                    if (Tb6_Cmb_GAI_KZID.SelectedIndex == -1)
                    {
                        Tb_Main.SelectedIndex = 5;
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb6_Cmb_GAI_KZID.IsError = true;
                        Tb6_Cmb_GAI_KZID.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //受取人名
                    if (Tb6_Txt_ENG_NAME.Text == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_NAME.IsError = true;
                        Tb6_Txt_ENG_NAME.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //住所
                    if (Tb6_Txt_ENG_ADDR.Text == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_ADDR.IsError = true;
                        Tb6_Txt_ENG_ADDR.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //口座番号とSWIFT両方共に未入力
                    if (Tb6_Txt_ENG_KZNO.Text == "" && Tb6_Txt_ENG_SWIF.Text == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_KZNO.IsError = true;
                        Tb6_Txt_ENG_KZNO.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //被仕向銀行
                    if (Tb6_Txt_ENG_BNKNAM.Text  == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_BNKNAM.IsError = true;
                        Tb6_Txt_ENG_BNKNAM.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //被仕向支店
                    if (Tb6_Txt_ENG_BRNNAM.Text == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_BRNNAM.IsError = true;
                        Tb6_Txt_ENG_BRNNAM.Focus();
                        nErrFlg = 1;
                        return;
                    }
                    //被仕向銀行住所
                    if (Tb6_Txt_ENG_BNKADDR.Text == "")
                    {
                        Tb_Main.SelectedIndex = 5;
                        Tb6_Txt_ENG_BNKADDR.IsError = true;
                        Tb6_Txt_ENG_BNKADDR.Focus();
                        nErrFlg = 1;
                        return;
                    }

                }

                //取引停止の開始＆終了年月日大小chk
                if (Txt_STYMD.Value != 0 && Txt_EDYMD.Value != 0)
                {
                    if (Txt_STYMD.Value > Txt_EDYMD.Value)
                    {
                        Txt_STYMD.IsError = true;
                        Txt_STYMD.Focus();
                        nErrFlg = 1;
                        return;
                    }
                }

                // その他控除チェック
//-- <2016/03/16>
                if (Tb5_Rdo_HORYU0.Checked == false)
                { 
                    // 適用基準額が１以上
                    if (!(Tb5_Txt_HR_KIJYUN.ExNumValue > 0))
                    {
                        Tb5_Txt_HR_KIJYUN.IsError = true;
                        Tb_Main.SelectedIndex = 4;
                        Tb5_Txt_HR_KIJYUN.Focus();
                        nErrFlg = 1;
                    }
                    // 計算区分未選択
                    else if (Tb5_Cmb_HORYU_F.SelectedIndex == -1)
                    {
                        // ---> V02.21.01 KKL ADD ▼(106612)
                        SendKeys.SendWait("{ENTER}");
                        SendKeys.SendWait("+{TAB}");
                        // <--- V02.21.01 KKL ADD ▲(106612)
                        Tb5_Cmb_HORYU_F.IsError = true;
                        Tb_Main.SelectedIndex = 4;
                        Tb5_Cmb_HORYU_F.Focus();
                        nErrFlg = 1;
                    }
//                if (Tb5_Rdo_HORYU0.Checked == false && Tb5_Txt_HOVAL.Text == "100.0")
                    // 支払保留を使用するで比率選択時　比率は100.000未満
                    else if (Tb5_Rdo_HORYU1.Checked && Tb5_Cmb_HORYU_F.SelectedIndex == 0 && !(Tb5_Txt_HOVAL.ExNumValue > 0.000M && Tb5_Txt_HOVAL.ExNumValue < 100.000M))
//-- <2016/03/16>
                    {
                        Tb5_Txt_HOVAL.IsError = true;
                        Tb_Main.SelectedIndex = 4;
                        Tb5_Txt_HOVAL.Focus();
                        nErrFlg = 1;
                        //return;
                    }
//-- <2016/03/16>
                    // 自動控除を使用するで比率選択時　比率は0.001以上100.000未満
                    else if (Tb5_Rdo_HORYU2.Checked && Tb5_Cmb_HORYU_F.SelectedIndex == 0 && !(Tb5_Txt_HOVAL.ExNumValue > 0.000M && Tb5_Txt_HOVAL.ExNumValue < 100.000M))
                    {
                        Tb5_Txt_HOVAL.IsError = true;
                        Tb_Main.SelectedIndex = 4;
                        Tb5_Txt_HOVAL.Focus();
                        nErrFlg = 1;
                    }
                    // 定額選択時　定額は１以上
                    else if (Tb5_Cmb_HORYU_F.SelectedIndex == 1 && !(Tb5_Txt_HRORYUGAKU.ExNumValue > 0))
                    {
                        Tb5_Txt_HRORYUGAKU.IsError = true;
                        Tb_Main.SelectedIndex = 4;
                        Tb5_Txt_HRORYUGAKU.Focus();
                        nErrFlg = 1;
                    }
                }
//-- <2016/03/16>
                if (!Tb4_Chk_FDEF.Checked)
                {
//-- <2016/04/06 補助コード使用しないの条件を追加>
                    if (Global.nTRCD_HJ == 0)
                    {
                        if (!mcBsLogic.Chk_FRIGINFDEF(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, "0", Tb4_Lbl_GIN_ID_V.Text))
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Chk_FDEF.IsError = true;
                            Tb4_Chk_FDEF.Focus();
                            nErrFlg = 1;
                        }
                    }
                    else
                    {
                        //-- <2016/04/06>
                        if (!mcBsLogic.Chk_FRIGINFDEF(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text, Tb4_Lbl_GIN_ID_V.Text))
                        {
                            Tb_Main.SelectedIndex = 3;
                            Tb4_Chk_FDEF.IsError = true;
                            Tb4_Chk_FDEF.Focus();
                            nErrFlg = 1;
                        }
//-- <2016/04/06>
                    }
//-- <2016/04/06>
                }







                if (mcBsLogic.Chk_GinFuriSKBN(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text.Trim() == "" ? "0" : Txt_HJCD.Text.Trim(), Tb3_Txt_SHINO.Text, BindNavi2_Selected.Text))
                {
                    if (Tb4_Txt_BANK_CD.Text.Trim() == "")
                    {
                        Tb_Main.SelectedIndex = 3;
                        Tb4_Txt_BANK_CD.IsError = true;
                        Tb4_Txt_BANK_CD.Focus();
                        nErrFlg = 1;
                        //Tb_Main.SelectedIndex = 1;
                        return;
                    }
                }
                //-- <>
                if (Cbo_SAIKEN.SelectedValue.ToString() == sNotUse && Cbo_SAIMU.SelectedValue.ToString() == sNotUse)
                {
                    Cbo_SAIKEN.IsError = true;
                    Cbo_SAIKEN.Focus();
                    nErrFlg = 1;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/15 >
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nChk_DispVal_TGASW_OFF　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/15>
//-- <2016/02/17 エラー時にはフラグをたてておく>
                nErrFlg = 1;
//-- <2016/02/17>
            }
        }
        #endregion


        #region 画面制御
        /// <summary>
        /// Fキー押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSMTORI_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:        
                case Keys.F2:
                    // 2013.3.6 修正
                    if ((e.KeyCode == Keys.F1 & FKB.F01_Enabled == true) | (e.KeyCode == Keys.F2 & FKB.F02_Enabled == true))
                    {
                        if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
                        {
                            res = MessageBox.Show(
//-- <2016/03/22>
//                                "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                                "変更されています。確定しますか？", "保存確認", MessageBoxButtons.YesNoCancel,
//-- <2016/03/22>
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                            if (res == DialogResult.Cancel)
                            {
                                return;
                            }
                            else if (res == DialogResult.No)
                            {
                                nTRCDflg = 1;
                                Sel_SSTORI();
                            }
                            else if (res == DialogResult.Yes)
                            {
                                nErrFlg = 0;
                                Ins_SSTORI();
                                if (nErrFlg == 1)
                                {
                                    return;
                                }
                                else
                                {
                                    // ---> V02.37.01 YMP DELETE ▼(122172)
                                    //nDispChgFlg_Main = 0;
                                    //nDispChgFlg_TSHOH = 0;
                                    //nDispChgFlg_FRIGIN = 0;
                                    //Btn_REG.Enabled = false;
                                    //FKB.F10_Enabled = false;
                                    // <--- V02.37.01 YMP DELETE ▲(122172)
                                }
                            }
                        }
                        if (e.KeyCode == Keys.F1)
                        {
                            Global.nExpPrn = 1;
                        }
                        else
                        {
                            Global.nExpPrn = 2;
                        }
                        // ---> V02.37.01 YMP ADD ▼(122172)
                        Sel_TabData();
                        nDispChgFlg_Main = 0;
                        nDispChgFlg_TSHOH = 0;
                        nDispChgFlg_FRIGIN = 0;
                        Btn_REG.Enabled = false;
                        FKB.F10_Enabled = false;
                        // <--- V02.37.01 YMP ADD ▲(122172)
                        mcBsLogic.PrnSettingView();

                    }
                    break;
                case Keys.F6:
                    Global.dNow = mcBsLogic.Get_DBTime();

                    // トランザクション処理
                    //DbTransaction trn = (Global.cConKaisya).BeginTransaction(IsolationLevel.ReadCommitted);
                    DbTransaction trn = (Global.cConSaikenSaimu).BeginTransaction(IsolationLevel.ReadCommitted);
                    Global.cCmdSel.Transaction = trn;
                    Global.cCmdIns.Transaction = trn;
                    Global.cCmdDel.Transaction = trn;
                    
                    try
                    {
                        // 2013.3.6 修正
                        if (FKB.F06_Enabled == true)
                        {
                            if (Txt_TRCD.ExCodeDB != "" && Lbl_Old_New1.Text == "【　変更　】")
                            {
                                string sDelTarget = (Txt_HJCD.Text != "" ? Txt_TRCD.ExCodeDB + "-" + Txt_HJCD.Text : Txt_TRCD.ExCodeDB);

                                string hedmsg = "";
                                // Ver.01.11.01 [SIAS-8083] Toda -->
//                                if (mcBsLogic.Chk_SS_SHDATA(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") == true)
//                                {
//                                    hedmsg = "取引先コード：" + sDelTarget + "の支払依頼データがあります。\n削除できません。";
////-- <2016/03/22>
////                                    MessageBox.Show(hedmsg, Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//                                    MessageBox.Show(hedmsg, "削除確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
////-- <2016/03/22>
//                                    trn.Rollback();
//                                    return;
//                                }

//                                if (mcBsLogic.Chk_SS_SJDATA(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") == true)
//                                {
////-- <2016/03/10 文言修正>
////                                    hedmsg = "取引先コード：" + sDelTarget + "の手形管理データがあります。\n削除できません。";
//                                    hedmsg = "取引先コード：" + sDelTarget + "の支払・管理データがあります。\n削除できません。";
////-- <2016/03/10>
////-- <2016/03/22>                                    
////                                    MessageBox.Show(hedmsg, Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//                                    MessageBox.Show(hedmsg, "削除確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
////-- <2016/03/22>
//                                    trn.Rollback();
//                                    return;
//                                }
                                //---> V01.16.02 HWPO ADD ▼(8836)
                                string sDaiCd = "";
                                string sDHjCd = "";
                                if (mcBsLogic.Get_MySaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") 
                                  || mcBsLogic.Get_SaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd)
                                  || mcBsLogic.Get_MySaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0")
                                  || mcBsLogic.Get_SaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd))
                                {
                                    hedmsg = "代表者マスター登録に登録されている為、削除できません。";
                                }
                                //<--- V01.16.02 HWPO ADD ▲(8836)
                                if (Cbo_SAIMU.SelectedValue.ToString() != sNotUse)
                                {
                                    if (mcBsLogic.Exists_Saimu_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
                                    {
                                        hedmsg = "取引先コード：" + sDelTarget + "の支払依頼データがあります。\n削除できません。";
                                    }
                                    else if (mcBsLogic.Exists_Siharai_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
                                    {
                                        hedmsg = "取引先コード：" + sDelTarget + "の支払（期日）データがあります。\n削除できません。";
                                    }
                                }
                                if (Cbo_SAIKEN.SelectedValue.ToString() != sNotUse)
                                {
                                    if (mcBsLogic.Exists_Saiken_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
                                    {
                                        hedmsg = "取引先コード：" + sDelTarget + "の請求・入金データがあります。\n削除できません。";
                                    }
                                    else if (mcBsLogic.Exists_Nyukin_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
                                    {
                                        hedmsg = "取引先コード：" + sDelTarget + "の入金（期日）データがあります。\n削除できません。";
                                    }
                                }

                                if (hedmsg != "")
                                {
                                    MessageBox.Show(hedmsg, "削除確認", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    trn.Rollback();
                                    return;
                                }
                                // Ver.01.11.01 <--
//----------------------------------------------------------------------------------<削除チェック追加>

                                //テーブル削除
                                if (MessageBox.Show(
//-- <2016/03/22>
//                                    hedmsg + "取引先コード：" + sDelTarget + " を削除しますか。", Global.sPrgName,
                                    hedmsg + "取引先コード：" + sDelTarget + " を削除しますか。", "削除確認",
//-- <2016/03/22>
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                                {
                                    trn.Rollback();
                                    return;
                                }
                                else
                                {
                                    //キー取得
                                    string sTRCD = Txt_TRCD.ExCodeDB;
                                    string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");

                                    //削除対象の情報をdtRIREKIにセット ＠2011/07 履歴対応
                                    Set_dtRIREKI(0, 0, "", 9, null, null, null);

                                    //Global.dNow = IcsSSUtil.IDate.GetDBNow(Global.cConCommon);
                                    //Global.dNow = mcBsLogic.Get_DBTime();

                                    //削除項目を履歴に登録 ＠2011/07 履歴対応
                                    mcBsLogic.Insert_SS_RKITORI();

                                    //削除処理
                                    mcBsLogic.Del_SS_FRIGIN_ALL(sTRCD, sHJCD);
                                    mcBsLogic.Del_SS_TSHOH_ALL(sTRCD, sHJCD);
                                    mcBsLogic.Del_SS_TORI(sTRCD, sHJCD);

                                    // 2024/10/17 #127552 垣内▼
                                    trn.Commit();
                                    // 2024/10/17 #127552 垣内▲

                                    Global.dtRIREKI.Rows.Clear();
                                    Global.dtRIREKI.AcceptChanges();

                                    Refresh_DataCnt();
                                    int cnt;
                                    mcBsLogic.Cnt_TRCD(out cnt);
                                    this.BindNavi1_Prev.Enabled = (cnt > 0);
                                    this.BindNavi1_First.Enabled = (cnt > 0);
                                    if (!SetNextSS_TORI(sTRCD, sHJCD))
                                    {
                                        mcBsLogic.Init_DispVal();
                                        //Tb_Main.SelectedIndex = 0;         // <---V01.15.01 HWY DELETE ◀(6490)
                                        Txt_TRCD.Focus();
                                        SetDispVal_S();
                                        nDispChgFlg_Main = 0;
                                        nDispChgFlg_TSHOH = 0;
                                        nDispChgFlg_FRIGIN = 0;
                                        Sel_TabData();
                                        Lbl_Old_New1.Text = "";
                                        Txt_HJCD.ReadOnlyEx = true;
                                    }
                                    nDispChgFlg_Main = 0;
                                    nDispChgFlg_TSHOH = 0;
                                    nDispChgFlg_FRIGIN = 0;
                                    Btn_REG.Enabled = false;
                                    FKB.F10_Enabled = false;
                                    Lbl_Haifun.Enabled = false;
                                    Txt_TRCD.Focus();
                                }
                            }
                        }
                        // 2024/10/17 #127552 垣内▼
                        //trn.Commit();
                        // 2024/10/17 #127552 垣内▲
                        Global.bZUpdFlg = true;
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        MessageBox.Show(
//-- <2016/03/22>
//                            "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nfrmSMTORI_KeyDown　\r\nVer" + Global.sPrgVer,
                            Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
                    }
                    break;
                case Keys.F8:
                    if (FKB.F08_Enabled == true)
                    {
                        if (Txt_TRCD.Focused == true)
                        {
                            // ---> V02.28.01 KKL ADD ▼(No.115107)
                            if (!ConfirmExistingData())
                            {
                            // <--- V02.28.01 KKL ADD ▲(No.115107)
                                DialogManager.ZToriData srcZToriData = DlgMng.DispZTORI("");
                                if (srcZToriData != null)
                                {
                                    Txt_TRCD.ExCodeDB = srcZToriData.COD;
                                    Txt_HJCD.Text = "";
                                    Txt_TRCD.IsError = false;
                                    SendKeys.Send("{TAB}");
                                }
                            }// <--- V02.28.01 KKL ADD◂(No.115107)
                        }
                        fKeyClick = false;// <--- V02.28.01 KKL ADD◂(No.115107)
                    }
                    break;
                case Keys.F9:
                    if (FKB.F09_Enabled == true)
                    {
                        if (Txt_TRCD.Focused == true)
                        {
                            // ---> V02.28.01 KKL ADD ▼(No.115107)
                            if (!ConfirmExistingData())
                            {
                            // <--- V02.28.01 KKL ADD ▲(No.115107)
                                bool bHJCD = (Global.nTRCD_HJ == 1 ? true : false);
                                //DialogManager.SToriData srcToriData = DlgMng.DispTTORI("", bHJCD);
                                DialogManager.SToriData srcToriData = DlgMng.DispTORI("", true, false, 0, 0, Global.nUcod, DialogManager.S_WORD.NASHI);
                                if (srcToriData != null)
                                {
                                    Txt_TRCD.ExCodeDB = srcToriData.COD;
                                    if (bHJCD == true)
                                    {
                                        Lbl_Haifun.Enabled = true;
                                        Txt_HJCD.ReadOnlyEx = false;
                                        Txt_HJCD.Text = srcToriData.HOJO.ToString();
                                        Txt_HJCD.Select();
                                    }
                                    else
                                    {
                                        Lbl_Haifun.Enabled = false;
                                        Txt_HJCD.ReadOnlyEx = true;
                                    }
                                    //Txt_HJCD.Focus();
                                    SendKeys.Send("{TAB}");
                                }
                            }// <--- V02.28.01 KKL ADD◂(No.115107)
                        }
                        else if (Txt_HJCD.Focused == true)
                        {
                            // ---> V02.28.01 KKL ADD ▼(No.115107)
                            if (!ConfirmExistingData())
                            {
                            // <--- V02.28.01 KKL ADD ▲(No.115107)
                                DialogManager.SToriData srcToriData = DlgMng.DispTTORI(Txt_TRCD.ExCodeDB, true);
                                if (srcToriData != null)
                                {
                                    Txt_TRCD.ExCodeDB = srcToriData.COD;
                                    Txt_HJCD.Text = srcToriData.HOJO.ToString();
                                    SendKeys.Send("{TAB}");
                                }
                            }// <--- V02.28.01 KKL ADD◂(No.115107)
                        }
                        else if (Txt_GRPID.Focused == true)
                        {
//-- <2016/02/08 コメントアウトしている。なんで？作成途中？>
                            //DialogManager.ToriGrpData trgData = DlgMng.DispToriGrp(cmbSecKmk.SelectedIndex, Global.nUcod);
                            //if (trgData != null)
                            //{
                            //    Txt_GRPID.Text = trgData.GRPID.ToString();
                            //    Txt_GRPNM.Text = trgData.GRPNM;
                            //}
                            // セキュリテイ条件なしで呼び出し
                            DialogManager.ToriGrpData trgData = DlgMng.DispToriGrp(0, Global.nUcod);
                            if (trgData != null)
                            {
                                Txt_GRPID.Text = trgData.GRPID.ToString();
                                Txt_GRPNM.Text = trgData.GRPNM;
                                SendKeys.Send("{TAB}");
                            }
//-- <2016/02/08>
                        }
                        else if(Tb2_Txt_SEN_GINKOCD.Focused == true)
                        {
                            DialogManager.BankData bnkData = DlgMng.DispBank();
                            if (bnkData != null)
                            {
                                Tb2_Txt_SEN_GINKOCD.ExCodeDB = bnkData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb2_Txt_SEN_SITENCD.Focused == true)
                        {
                            DialogManager.BranchData brnData = DlgMng.DispBranch(Tb2_Txt_SEN_GINKOCD.ExCode, true);
                            if (brnData != null)
                            {
                                Tb2_Txt_SEN_SITENCD.ExCodeDB = brnData.BRCOD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb2_Txt_SEN_KSITENCD.Focused == true)
                        {
                            DialogManager.BranchData brnData = DlgMng.DispBranch(Tb2_Txt_SEN_GINKOCD.ExCode, true);
                            if (brnData != null)
                            {
                                Tb2_Txt_SEN_KSITENCD.ExCodeDB = brnData.BRCOD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb2_Txt_HIFURIKOZA_1.Focused == true)
                        {
                            DialogManager.OwnBnkData ownData = DlgMng.DispOwnBank();
                            if (ownData != null)
                            {
                                Tb2_Txt_HIFURIKOZA_1.ExNumValue = ownData.OWNID;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb2_Txt_HIFURIKOZA_2.Focused == true)
                        {
                            DialogManager.OwnBnkData ownData = DlgMng.DispOwnBank();
                            if (ownData != null)
                            {
                                Tb2_Txt_HIFURIKOZA_2.ExNumValue = ownData.OWNID;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb2_Txt_HIFURIKOZA_3.Focused == true)
                        {
                            DialogManager.OwnBnkData ownData = DlgMng.DispOwnBank();
                            if (ownData != null)
                            {
                                Tb2_Txt_HIFURIKOZA_3.ExNumValue = ownData.OWNID;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb3_Txt_SHINO.Focused == true)
                        {
                            Tb1_Btn_SHINO.PerformClick();
                        }
                        else if (Tb5_Txt_STAN_CD.Focused == true)
                        {
                            DialogManager.TantoData srcTntData = DlgMng.DispTanto();
                            if (srcTntData != null)
                            {
                                Tb5_Txt_STAN_CD.Text = srcTntData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb5_Txt_SBCOD.Focused == true)
                        {
                            DialogManager.BumonData srcBmnData = DlgMng.DispBumon("", 0);
                            if (srcBmnData != null)
                            {
                                Tb5_Txt_SBCOD.ExCodeDB = srcBmnData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb5_Txt_SKCOD.Focused == true)
                        {
                            //--->V01.12.01 ATT UPDATE ▼ (7063)
                            //DialogManager.KamokuData srcKmkData = DlgMng.DispKamoku();
                            Boolean bGAI_F = (Global.GAI_F == "0" ? false : true);
                            DialogManager.KamokuData srcKmkData = DlgMng.DispKamoku(0, Global.nUcod, "", bGAI_F);
                            //<---V01.12.01 ATT UPDATE ▲ (7063)
                            if (srcKmkData != null)
                            {
                                Tb5_Txt_SKCOD.ExCodeDB = srcKmkData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb3_Txt_BCOD.Focused == true)
                        {
                            DialogManager.BumonData srcBmnData = DlgMng.DispBumon("", 0);
                            if (srcBmnData != null)
                            {
                                Tb3_Txt_BCOD.ExCodeDB = srcBmnData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb3_Txt_KCOD.Focused == true)
                        {
                            //--->V01.12.01 ATT UPDATE ▼ (7063)
                            //DialogManager.KamokuData srcKmkData = DlgMng.DispKamoku();
                            Boolean bGAI_F = (Global.GAI_F == "0" ? false : true);
                            DialogManager.KamokuData srcKmkData = DlgMng.DispKamoku(0, Global.nUcod, "", bGAI_F);
                            //<---V01.12.01 ATT UPDATE ▲ (7063)
                            if (srcKmkData != null)
                            {
                                Tb3_Txt_KCOD.ExCodeDB = srcKmkData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb4_Txt_BANK_CD.Focused == true)
                        {
                            DialogManager.BankData srcBnkData = DlgMng.DispBank();
                            if (srcBnkData != null)
                            {
                                Tb4_Txt_BANK_CD.Text = srcBnkData.COD;
                                SendKeys.Send("{TAB}");
                            }
                        }
                        else if (Tb4_Txt_SITEN_ID.Focused == true)
                        {
                            if (Tb4_Txt_BANK_CD.Text != "")
                            {
                                DialogManager.BranchData srcBrnData = DlgMng.DispBranch(Tb4_Txt_BANK_CD.Text, true);
                                if (srcBrnData != null)
                                {
                                    Tb4_Txt_SITEN_ID.Text = srcBrnData.BRCOD;
                                    SendKeys.Send("{TAB}");
                                }
                            }
                            else
                            {
                                DialogManager.BankData srcBnkData = DlgMng.DispBank();
                                if (srcBnkData != null)
                                {
                                    Tb4_Txt_BANK_CD.Text = srcBnkData.COD;
                                    SendKeys.Send("{TAB}");
                                }

                                DialogManager.BranchData srcBrnData = DlgMng.DispBranch(Tb4_Txt_BANK_CD.Text, true);
                                if (srcBrnData != null)
                                {
                                    Tb4_Txt_SITEN_ID.Text = srcBrnData.BRCOD;
                                    SendKeys.Send("{TAB}");
                                }
                            }
                        }
                        else if (Tb1_Txt_E_TANTOCD.Focused == true)
                        {
                            DialogManager.SaikenTantoData saiData = DlgMng.DispSaikenTanto(Global.cKaisya.nKESN, "");
                            if (saiData != null)
                            {
//-- <2016/02/17 英数の末尾スペース排除>
//                                Tb1_Txt_E_TANTOCD.Text = saiData.COD;
                                Tb1_Txt_E_TANTOCD.Text = saiData.COD.TrimEnd();
//-- <2016/02/17>
                                Tb1_Txt_E_TANTONM.Text = saiData.MEI;
//-- <2016/02/08 項目送り>
                                SendKeys.Send("{TAB}");
//-- <2016/02/08>
                            }
                        }
                        fKeyClick = false;// <--- V02.28.01 KKL ADD◂(No.115107)
                    }
                    break;
                case Keys.F10:
                    // SIAS_4228 差分 -->
                    if (Validate() == false) return;
                    //Validate();
                    // SIAS_4228 差分 <--
                    if (FKB.F10_Enabled == true)
                    {
                        //int iSelectTab = Tb_Main.SelectedIndex;
                        //if (iSelectTab == 4)
                        //{
                        //    iSelectTab = 0;
                        //}
                        //else
                        //{
                        //    iSelectTab++;
                        //}
                        //Tb_Main.SelectedIndex = iSelectTab;
//-- <2016/03/10 取りあえずメッセージ>
                        if (MessageBox.Show("変更を確定しますか？", "保存確認", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                        {
//-- <2016/03/10>
                            Ins_SSTORI();
                            if (nErrFlg == 0)
                            {
//-- <2016/03/14>
                                
                                //FKB.F10_Enabled = false;   // <---V01.15.01 HWY DELETE ◀(6490)
//-- <2016/03/14>
                                // ---> V02.37.01 YMP ADD ▼(122172)
                                Sel_TabData();
                                nDispChgFlg_TSHOH = 0;
                                // <--- V02.37.01 YMP ADD ▲(122172)
                                Txt_TRCD.Focus();
                                // --->V01.15.01 HWY ADD ▼(6490)
                                FKB.F10_Enabled = false;
                                nDispChgFlg_Main = 0;
                                nDispChgFlg_FRIGIN = 0;
                                // <---V01.15.01 HWY ADD ▲(6490)
                            }
//-- <2016/03/10 取りあえずメッセージ>
                        }
//-- <2016/03/10>
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 郵便番号にフォーカス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_ZIP_Enter(object sender, EventArgs e)
        {
            if (Tb1_Txt_ZIP.Text.Length == 8)
            {
                Tb1_Txt_ZIP.Text = Tb1_Txt_ZIP.Text.Remove(3, 1);
            }
        }


        /// <summary>
        /// 郵便番号からフォーカスアウト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_ZIP_Leave(object sender, EventArgs e)
        {
            if (Tb1_Txt_ZIP.Text == "")
            {
                Tb1_Txt_ZIP.IsError = false;
            }
            else if (Tb1_Txt_ZIP.Text.Length < 7)
            {
                Tb1_Txt_ZIP.IsError = true;
                Tb1_Txt_ZIP.Focus();
                return;
            }
            else if (Tb1_Txt_ZIP.Text.Length == 7)
            {
                Tb1_Txt_ZIP.Text = Tb1_Txt_ZIP.Text.Insert(3, "-");
                Tb1_Txt_ZIP.IsError = false;
            }
        }


        /// <summary>
        /// 支払条件タブ.複数条件有無の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb1_Chk_SHO_ID_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb3_Chk_SHO_ID.Checked == true)
            {
                Tb1_BindNavi2.Enabled = true;
                if (int.Parse(BindNavi2_Cnt.Text.Remove(0, 2)) > 1 && int.Parse(BindNavi2_Cnt.Text.Remove(0, 2)) > int.Parse(BindNavi2_Selected.Text))
                {
                    Tb3_BindNavi_Next.Enabled = true;
                    Tb3_BindNavi_Last.Enabled = true;
                }
                else
                {
                    Tb3_BindNavi_Next.Enabled = false;
                    Tb3_BindNavi_Last.Enabled = false;
                }

                if (int.Parse(BindNavi2_Selected.Text) > 1)
                {
                    Tb3_BindNavi_First.Enabled = true;
                    Tb3_BindNavi_Prev.Enabled = true;
                }
                else
                {
                    Tb3_BindNavi_First.Enabled = false;
                    Tb3_BindNavi_Prev.Enabled = false;
                }
            }
            else
            {
//-- <2016/03/10 ナビゲーションが違う>
//                Tb4_BindNavi_First.Enabled = false;
//                Tb4_BindNavi_Prev.Enabled = false;
//                Tb4_BindNavi_Next.Enabled = false;
//                Tb4_BindNavi_End.Enabled = false;
                Tb3_BindNavi_First.Enabled = false;
                Tb3_BindNavi_Prev.Enabled = false;
                Tb3_BindNavi_Next.Enabled = false;
                Tb3_BindNavi_Last.Enabled = false;
//-- <2016/03/10>
            }
        }


        /// <summary>
        /// 振込先情報タブ.複数条件有無の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb2_Chk_GIN_ID_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb4_Chk_GIN_ID.Checked == true)
            {
                Tb2_BindNavi3.Enabled = true;
                if (int.Parse(Tb4_BindNavi_Cnt.Text.Remove(0, 2)) > 1 && int.Parse(Tb4_BindNavi_Cnt.Text.Remove(0, 2)) > int.Parse(Tb4_BindNavi_Selected.Text))
                {
                    Tb4_BindNavi_Next.Enabled = true;
                    Tb4_BindNavi_End.Enabled = true;
                }
                else
                {
                    Tb4_BindNavi_Next.Enabled = false;
                    Tb4_BindNavi_End.Enabled = false;
                }

                if (int.Parse(Tb4_BindNavi_Selected.Text) > 1)
                {
                    Tb4_BindNavi_First.Enabled = true;
                    Tb4_BindNavi_Prev.Enabled = true;
                }
                else
                {
                    Tb4_BindNavi_First.Enabled = false;
                    Tb4_BindNavi_Prev.Enabled = false;
                }
            }
            else
            {
                Tb4_BindNavi_First.Enabled = false;
                Tb4_BindNavi_Prev.Enabled = false;
                Tb4_BindNavi_Next.Enabled = false;
                Tb4_BindNavi_End.Enabled = false;
            }
        }


        /// <summary>
        /// 会員区分で協力会員を選択⇒協力会費計算を有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Radio_KAIIN1_CheckedChanged(object sender, EventArgs e)
        {
            Chg_DispControl();
        }


        /// <summary>
        /// 控除情報タブ.(材料)負担チェック切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Chk_KYZAF_CheckedChanged(object sender, EventArgs e)
        {
            Chg_DispControl();
        }


        /// <summary>
        /// 控除情報タブ.(労務)負担チェック切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Chk_KYROF_CheckedChanged(object sender, EventArgs e)
        {
            Chg_DispControl();
        }


        /// <summary>
        /// 控除情報タブ.(外注)負担チェック切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Chk_KYGAF_CheckedChanged(object sender, EventArgs e)
        {
            Chg_DispControl();
        }


        /// <summary>
        /// 控除情報タブ.(経費)負担チェック切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb4_Chk_KYKEF_CheckedChanged(object sender, EventArgs e)
        {
            Chg_DispControl();
        }


        /// <summary>
        /// 画面制御一括管理
        /// </summary>
        private void Chg_DispControl()
        {
            //取引先CD・取引先補助CD・略称・取引先名称・50音の入力完了まで
            //他の項目は入力させない(取引先補助CDは入力可能である場合)
            if (Txt_TRCD.IsError == false && Txt_HJCD.IsError == false && Txt_RYAKU.IsError == false && Txt_TORI_NAM.IsError == false)
            {
                if (
                        ((Txt_TRCD.ExCodeDB != "")
                            && (((Global.nTRCD_HJ == 1) && (Txt_HJCD.Text != "")) ||
                                ((Global.nTRCD_HJ == 0) && ((Txt_HJCD.Text == "") || (Txt_HJCD.Text == "0"))))
                            && (Txt_RYAKU.Text != "")
                            && (Txt_TORI_NAM.Text != "")
                        )
                    || (Global.nDispMode == 1))
                {
                    TabControl.TabPageCollection tabPages = Tb_Main.TabPages;

                    if ((Global.nSAIKEN_F == 1 || Global.nKIJITU_F == 1) && Global.bEnabledState == true)
                    {
                        Cbo_SAIKEN.Enabled = true;
                        if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
                        {
                            string sDaiCd = "";
                            string sDHjCd = "";
                            // 子供の時、親が設定されているか確認
                            if (mcBsLogic.Get_SaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd))
                            {
                                Txt_SaikenDaihyoCd.Text = sDaiCd;
                                if (Global.nTRCD_HJ == 1)
                                {
                                    Txt_SaikenDaihyoHj.Text = sDHjCd.PadLeft(6, '0');
                                }
                                Cbo_SAIKEN.Enabled = false;
                                Chk_SAIKEN_FLG.Enabled = false;
                            }
                            else
                            {
                                if (mcBsLogic.Get_MySaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0"))
                                {
                                    Cbo_SAIKEN.Enabled = false;
                                    Chk_SAIKEN_FLG.Enabled = false;
                                }
                                else
                                {
                                    if (Global.cUsrSec.nMFLG < 2)
                                    {
                                        Cbo_SAIKEN.Enabled = false;
                                        Chk_SAIKEN_FLG.Enabled = false;
                                    }
                                    else
                                    {
                                        Cbo_SAIKEN.Enabled = !(mcBsLogic.Exists_Saiken_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text) || mcBsLogic.Exists_Sousai_Tokui(Txt_TRCD.ExCodeDB, Txt_HJCD.Text));
                                        Chk_SAIKEN_FLG.Enabled = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Chk_SAIKEN_FLG.Enabled = false;
                        }
                    }
                    else
                    {
                        Cbo_SAIKEN.Enabled = false;
                        Chk_SAIKEN_FLG.Enabled = false;
                        //2018/11/02 ICS.吉岡 ▼(SIAS-9897)＜マスタ権限を｢参照のみ｣にしているユーザーの場合、回収設定タブと支払条件タブが選択できません＞
                        tabPages[1].Enabled = (Cbo_SAIKEN.SelectedValue.ToString() == sUse);    // 回収設定タブ
                        //tabPages[1].Enabled = false;    // 回収設定タブ
                        //2018/11/02 ICS.吉岡 ▲(SIAS-9897)＜マスタ権限を｢参照のみ｣にしているユーザーの場合、回収設定タブと支払条件タブが選択できません＞
                    }
                    if ((Global.nSAIMU_F == 1 || Global.nKIJITU_F == 1) && Global.bEnabledState == true)
                    {
                        if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
                        {
                            string sDaiCd = "";
                            string sDHjCd = "";
                            //子供だったら親がいるかを確認する
                            if (mcBsLogic.Get_SaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd))
                            {
                                Txt_SaimuDaihyoCd.Text = sDaiCd;
                                if (Global.nTRCD_HJ == 1)
                                {
                                    Txt_SaimuDaihyoHj.Text = sDHjCd.PadLeft(6, '0');
                                }
                            }
                        }
                        Set_Enabled_Cbo_SAIMU(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                        Set_Enabled_Chk_SAIMU_FLG(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                    }
                    else
                    {
                        Cbo_SAIMU.Enabled = false;
                        Chk_SAIMU_FLG.Enabled = false;

                        //2018/11/02 ICS.吉岡 ▼(SIAS-9897)＜マスタ権限を｢参照のみ｣にしているユーザーの場合、回収設定タブと支払条件タブが選択できません＞
                        tabPages[2].Enabled = (Cbo_SAIMU.SelectedValue.ToString() == sUse);    // 支払条件タブ
                        //tabPages[2].Enabled = false;    // 支払条件タブ
                        //2018/11/02 ICS.吉岡 ▲(SIAS-9897)＜マスタ権限を｢参照のみ｣にしているユーザーの場合、回収設定タブと支払条件タブが選択できません＞
                        tabPages[3].Enabled = false;    // 振込先情報タブ
                        tabPages[5].Enabled = false;    // 外貨設定タブ
                    }
                    if (Global.nGroup == 0)
                    {
                        Lbl_GRPID.Enabled = false;
                        Txt_GRPID.Enabled = false;
                        Txt_GRPNM.Enabled = false;
                        if (Global.GRPID == "0")
                        {
                            Txt_GRPID.ClearValue();
                            Txt_GRPNM.ClearValue();
                        }
                    }
                    else
                    {
                        if (Cbo_SAIMU.SelectedValue.ToString() != sUse)
                        {
                            Lbl_GRPID.Enabled = false;
                            Txt_GRPID.Enabled = false;
                            Txt_GRPNM.Enabled = false;
                            Txt_GRPID.ClearValue();
                            Txt_GRPNM.ClearValue();
                        }
                        else
                        {
                            if (Global.bEnabledState == true)
                            {
                                Lbl_GRPID.Enabled = true;
                                Txt_GRPID.Enabled = true;
                                Txt_GRPID.ReadOnlyEx = false;
                                Txt_GRPNM.Enabled = true;
                            }
                        }
                    }
                    if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
                    {
                        if (Global.GAI_F == "0")
                        {
                            GAI_F_Kirikae(0);
                        }
                        else
                        {
                            GAI_F_Kirikae(1);
                        }
                    }
                    bool SousaiEnabled = !(mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text) || mcBsLogic.Exists_Sousai_Tokui(Txt_TRCD.ExCodeDB, Txt_HJCD.Text));
                    if (Cbo_SAIMU.SelectedValue.ToString() != sUse)
                    {
                        // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
                        if (Global.cUsrSec.nMFLG < 2 || (Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                        {
                            Txt_RYAKU.ReadOnlyEx = true;
                            Txt_TORI_NAM.ReadOnlyEx = true;
                            Txt_TRFURI.ReadOnlyEx = true;
                            Txt_KNLD.ReadOnlyEx = true;
                            Lbl_ZIP.Enabled = true;
                            Tb1_Txt_ZIP.ReadOnlyEx = true;
                            Lbl_ADDR1.Enabled = true;
                            Tb1_Txt_ADDR1.ReadOnlyEx = true;
                            Lbl_ADDR2.Enabled = true;
                            Tb1_Txt_ADDR2.ReadOnlyEx = true;

                            Tb1_Txt_TRMAIL.ReadOnlyEx = true;
                            Tb1_Txt_TRURL.ReadOnlyEx = true;
                            Tb1_Txt_BIKO.ReadOnlyEx = true;
                            Tb1_Txt_E_TANTOCD.ReadOnlyEx = true;
                            Tb1_Txt_MYNO_AITE.ReadOnlyEx = true;

                            Lbl_SBUSYO.Enabled = true;
                            Tb1_Txt_SBUSYO.ReadOnlyEx = true;
                            Lbl_STANTO.Enabled = true;
                            Tb1_Txt_STANTO.ReadOnlyEx = true;
                            Lbl_KEICD.Enabled = true;
                            Tb1_Cmb_KEICD.Enabled = false;
                            Lbl_TEL.Enabled = true;
                            Tb1_Txt_TEL.ReadOnlyEx = true;
                            Lbl_FAX.Enabled = true;
                            Tb1_Txt_FAX.ReadOnlyEx = true;
                            Chk_STFLG.Enabled = false;
                            cDfTitl1.Enabled = true;
                            Lbl_STYMD.Enabled = true;
                            Txt_STYMD.Enabled = false;
                            Lbl_EDYMD.Enabled = true;
                            Txt_EDYMD.Enabled = false;
                            cDfTitl2.Enabled = true;
                            Lbl_ZSTYMD.Enabled = true;
                            Txt_ZSTYMD.Enabled = false;
                            Lbl_ZEDYMD.Enabled = true;
                            Txt_ZEDYMD.Enabled = false;

                            Lbl_LUSR.Enabled = true;
                            Txt_LUSR.Enabled = false;
                            Lbl_LMOD.Enabled = true;
                            Txt_LMOD.Enabled = false;

                            // 第一タブは別箇所で制御

                            // 第二タブ
                            Tb4_Chk_GIN_ID.Enabled = false;
                            Tb4_Chk_FDEF.Enabled = false;
                            Tb4_Chk_DDEF.Enabled = false;
                            Tb4_Cmb_FTESUID.Enabled = false;
                            Tb2_Chk_DTESUSW.Enabled = false;
                            Tb2_Cmb_DTESU.Enabled = false;

                            Tb4_BindNavi_Add.Enabled = false;
                            Tb4_BindNavi_DEL.Enabled = false;
                            Tb4_Txt_BANK_CD.ReadOnlyEx = true;
                            Tb4_Txt_SITEN_ID.ReadOnlyEx = true;
                            Tb4_Cmb_YOKIN_TYP.Enabled = false;
                            Tb4_Txt_KOUZA.ReadOnlyEx = true;
                            Tb4_Txt_MEIGI.ReadOnlyEx = true;
                            Tb4_Txt_MEIGIK.ReadOnlyEx = true;
                            Tb4_Cmb_TESUU.Enabled = false;
                            Tb4_Cmb_SOUKIN.Enabled = false;
                            Tb4_Txt_GENDO.ReadOnlyEx = true;

                            // 第三タブは別箇所で制御

                            // 第四タブ
                            Tb5_Chk_GENSEN.Enabled = false;
                            Tb5_Chk_OUTPUT.Enabled = false;

                            // 第五タブ
                            Lbl_STAN.Enabled = false;
                            Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                            Lbl_SBCOD.Enabled = true;
                            Tb5_Txt_SBCOD.ReadOnlyEx = true;
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = true;
                            Tb5_Chk_NAYOSE.Enabled = false;
                            Tb5_Chk_F_SETUIN.Enabled = false;
                            Tb5_Cmb_F_SOUFU.Enabled = false;
                            Tb5_Cmb_ANNAI.Enabled = false;
                            Tb5_Cmb_TSOKBN.Enabled = false;
                            Tb5_Txt_TEGVAL.ReadOnlyEx = true;
                            Tb5_Cmb_SHITU.Enabled = false;
                            Tb5_Txt_DM1.ReadOnlyEx = true;
                            Tb5_Txt_DM2.ReadOnlyEx = true;
                            Tb5_Txt_DM3.ReadOnlyEx = true;
                            Tb5_Txt_FAC.ReadOnlyEx = true;
                            Tb1_Txt_UsrNo.ReadOnlyEx = true;
                            Tb1_Chk_Jyoto.Enabled = false;

                            Tb1_Chk_SOSAI.Enabled = false;
                            Tb1_Chk_SRYOU_F.Enabled = false;

                            Tb5_Rdo_HORYU0.Enabled = false;
                            Tb5_Rdo_HORYU1.Enabled = false;
                            Tb5_Rdo_HORYU2.Enabled = false;
                        }
                        else
                        {
                            Txt_RYAKU.ReadOnlyEx = false;
                            Txt_TORI_NAM.ReadOnlyEx = false;
                            Txt_TRFURI.ReadOnlyEx = false;
                            Txt_KNLD.ReadOnlyEx = false;
                            Lbl_ZIP.Enabled = true;
                            Tb1_Txt_ZIP.ReadOnlyEx = false;
                            Lbl_ADDR1.Enabled = true;
                            Tb1_Txt_ADDR1.ReadOnlyEx = false;
                            Lbl_ADDR2.Enabled = true;
                            Tb1_Txt_ADDR2.ReadOnlyEx = false;

                            Tb1_Txt_TRMAIL.ReadOnlyEx = false;
                            Tb1_Txt_TRURL.ReadOnlyEx = false;
                            Tb1_Txt_BIKO.ReadOnlyEx = false;
                            Tb1_Txt_E_TANTOCD.ReadOnlyEx = Global.nSAIKEN_F != 1;
                            Tb1_Txt_MYNO_AITE.ReadOnlyEx = false;

                            Lbl_SBUSYO.Enabled = true;
                            Tb1_Txt_SBUSYO.ReadOnlyEx = false;
                            Lbl_STANTO.Enabled = true;
                            Tb1_Txt_STANTO.ReadOnlyEx = false;
                            Lbl_KEICD.Enabled = true;
                            Tb1_Cmb_KEICD.Enabled = true;
                            Lbl_TEL.Enabled = true;
                            Tb1_Txt_TEL.ReadOnlyEx = false;
                            Lbl_FAX.Enabled = true;
                            Tb1_Txt_FAX.ReadOnlyEx = false;
                            Chk_STFLG.Enabled = true;
                            cDfTitl1.Enabled = true;
                            Lbl_STYMD.Enabled = true;
                            Txt_STYMD.Enabled = true;
                            Lbl_EDYMD.Enabled = true;
                            Txt_EDYMD.Enabled = true;
                            cDfTitl2.Enabled = true;
                            Lbl_ZSTYMD.Enabled = true;
                            Txt_ZSTYMD.Enabled = true;
                            Lbl_ZEDYMD.Enabled = true;
                            Txt_ZEDYMD.Enabled = true;

                            Lbl_LUSR.Enabled = true;
                            Txt_LUSR.Enabled = true;
                            Lbl_LMOD.Enabled = true;
                            Txt_LMOD.Enabled = true;

                            // 第一タブは別箇所で制御

                            // 第二タブ
                            Tb4_Chk_GIN_ID.Enabled = false;
                            Tb4_BindNavi_Add.Enabled = true;
                            Tb4_BindNavi_DEL.Enabled = true;
                            Tb4_Chk_DDEF.Enabled = true;
                            Tb4_Cmb_FTESUID.Enabled = true;
                            Tb2_Chk_DTESUSW.Enabled = true;
                            if (Tb2_Chk_DTESUSW.Checked)
                            { Tb2_Cmb_DTESU.Enabled = true; }
                            else
                            { Tb2_Cmb_DTESU.Enabled = false; }
                            Tb4_Txt_BANK_CD.ReadOnlyEx = false;
                            Tb4_Txt_SITEN_ID.ReadOnlyEx = false;
                            Tb4_Cmb_YOKIN_TYP.Enabled = true;
                            Tb4_Txt_KOUZA.ReadOnlyEx = false;
                            Tb4_Txt_MEIGI.ReadOnlyEx = false;
                            Tb4_Txt_MEIGIK.ReadOnlyEx = false;
                            Tb4_Cmb_TESUU.Enabled = true;
                            Tb4_Cmb_SOUKIN.Enabled = true;
                            Tb4_Txt_GENDO.ReadOnlyEx = false;

                            // 第三タブは別箇所で制御

                            // 第四タブ
                            Tb5_Chk_OUTPUT.Enabled = true;
                            Tb5_Chk_GENSEN.Enabled = Tb5_Chk_OUTPUT.Checked;

                            // 第五タブ
                            Lbl_STAN.Enabled = false;
                            Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                            Lbl_SBCOD.Enabled = (Global.nBCOD_F == 0 ? false : true);
                            Tb5_Txt_SBCOD.ReadOnlyEx = (Global.nBCOD_F == 0 ? true : false);
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = false;
                            Tb5_Chk_NAYOSE.Enabled = false;
                            Tb5_Chk_F_SETUIN.Enabled = false;
                            Tb5_Cmb_F_SOUFU.Enabled = true;
                            Tb5_Cmb_ANNAI.Enabled = true;
                            Tb5_Cmb_TSOKBN.Enabled = true;
                            Tb5_Txt_TEGVAL.ReadOnlyEx = false;

                            Tb5_Cmb_SHITU.Enabled = true;
                            Tb5_Txt_DM1.ReadOnlyEx = false;
                            Tb5_Txt_DM2.ReadOnlyEx = false;
                            Tb5_Txt_DM3.ReadOnlyEx = false;
                            Tb5_Txt_FAC.ReadOnlyEx = false;
                            Tb1_Txt_UsrNo.ReadOnlyEx = false;
                            Tb1_Chk_Jyoto.Enabled = true;

                            if (Global.nSOSAI_F == 0)
                            { Tb1_Chk_SOSAI.Enabled = false; }
                            else
                            { Tb1_Chk_SOSAI.Enabled = SousaiEnabled; }
                            if (Tb1_Chk_SOSAI.Checked)
                            { Tb1_Chk_SRYOU_F.Enabled = true; }
                            Tb5_Rdo_HORYU0.Enabled = true;
                            Tb5_Rdo_HORYU1.Enabled = true;
                            Tb5_Rdo_HORYU2.Enabled = true;

                        }

                        Tb_Main.Enabled = true;
                        //tabPages[0].Enabled = true;  // #111516　竹内2022/02/24  2022/03/11仕様変更
                        //tabPages[4].Enabled = true;  // #111516　竹内2022/03/08
                    }
                    else
                    {
                        // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
                        if (Global.cUsrSec.nMFLG < 2 || (Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                        {
                            Txt_RYAKU.ReadOnlyEx = true;
                            Txt_TORI_NAM.ReadOnlyEx = true;
                            Txt_TRFURI.ReadOnlyEx = true;
                            Txt_KNLD.ReadOnlyEx = true;
                            Lbl_ZIP.Enabled = true;
                            Tb1_Txt_ZIP.ReadOnlyEx = true;
                            Lbl_ADDR1.Enabled = true;
                            Tb1_Txt_ADDR1.ReadOnlyEx = true;
                            Lbl_ADDR2.Enabled = true;
                            Tb1_Txt_ADDR2.ReadOnlyEx = true;

                            Tb1_Txt_TRMAIL.ReadOnlyEx = true;
                            Tb1_Txt_TRURL.ReadOnlyEx = true;
                            Tb1_Txt_BIKO.ReadOnlyEx = true;
                            Tb1_Txt_E_TANTOCD.ReadOnlyEx = true;
                            Tb1_Txt_MYNO_AITE.ReadOnlyEx = true;

                            Lbl_SBUSYO.Enabled = true;
                            Tb1_Txt_SBUSYO.ReadOnlyEx = true;
                            Lbl_STANTO.Enabled = true;
                            Tb1_Txt_STANTO.ReadOnlyEx = true;
                            Lbl_KEICD.Enabled = true;
                            Tb1_Cmb_KEICD.Enabled = false;
                            Lbl_TEL.Enabled = true;
                            Tb1_Txt_TEL.ReadOnlyEx = true;
                            Lbl_FAX.Enabled = true;
                            Tb1_Txt_FAX.ReadOnlyEx = true;
                            Lbl_STAN.Enabled = true;
                            Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                            Lbl_SBCOD.Enabled = true;
                            Tb5_Txt_SBCOD.ReadOnlyEx = true;
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = true;
                            Tb5_Chk_NAYOSE.Enabled = false;
                            Tb5_Chk_F_SETUIN.Enabled = false;
                            Chk_STFLG.Enabled = false;
                            cDfTitl1.Enabled = true;
                            Lbl_STYMD.Enabled = true;
                            Txt_STYMD.Enabled = false;
                            Lbl_EDYMD.Enabled = true;
                            Txt_EDYMD.Enabled = false;
                            cDfTitl2.Enabled = true;
                            Lbl_ZSTYMD.Enabled = true;
                            Txt_ZSTYMD.Enabled = false;
                            Lbl_ZEDYMD.Enabled = true;
                            Txt_ZEDYMD.Enabled = false;

                            Lbl_LUSR.Enabled = true;
                            Txt_LUSR.Enabled = false;
                            Lbl_LMOD.Enabled = true;
                            Txt_LMOD.Enabled = false;

                            // 第一タブは別箇所で制御

                            // 第二タブ
                            Tb4_Chk_GIN_ID.Enabled = false;
                            Tb4_BindNavi_Add.Enabled = false;
                            Tb4_BindNavi_DEL.Enabled = false;

                            Tb4_Chk_FDEF.Enabled = false;
                            Tb4_Chk_DDEF.Enabled = false;
                            Tb4_Cmb_FTESUID.Enabled = false;
                            Tb2_Chk_DTESUSW.Enabled = false;
                            Tb2_Cmb_DTESU.Enabled = false;

                            Tb4_Txt_BANK_CD.ReadOnlyEx = true;
                            Tb4_Txt_SITEN_ID.ReadOnlyEx = true;
                            Tb4_Cmb_YOKIN_TYP.Enabled = false;
                            Tb4_Txt_KOUZA.ReadOnlyEx = true;
                            Tb4_Txt_MEIGI.ReadOnlyEx = true;
                            Tb4_Txt_MEIGIK.ReadOnlyEx = true;
                            Tb4_Cmb_TESUU.Enabled = false;
                            Tb4_Cmb_SOUKIN.Enabled = false;
                            Tb4_Txt_GENDO.ReadOnlyEx = true;

                            // 第三タブは別箇所で制御

                            // 第四タブ
                            Tb5_Chk_GENSEN.Enabled = false;
                            Tb5_Chk_OUTPUT.Enabled = false;

                            // 第五タブ
                            Tb5_Cmb_F_SOUFU.Enabled = false;
                            Tb5_Cmb_ANNAI.Enabled = false;
                            Tb5_Cmb_TSOKBN.Enabled = false;
                            Tb5_Txt_TEGVAL.ReadOnlyEx = true;

                            Tb5_Cmb_SHITU.Enabled = false;
                            Tb5_Txt_DM1.ReadOnlyEx = true;
                            Tb5_Txt_DM2.ReadOnlyEx = true;
                            Tb5_Txt_DM3.ReadOnlyEx = true;
                            Tb5_Txt_FAC.ReadOnlyEx = true;
                            Tb1_Txt_UsrNo.ReadOnlyEx = true;
                            Tb1_Chk_Jyoto.Enabled = false;

                            Tb1_Chk_SOSAI.Enabled = false;
                            Tb1_Chk_SRYOU_F.Enabled = false;

                            Tb5_Rdo_HORYU0.Enabled = false;
                            Tb5_Rdo_HORYU1.Enabled = false;
                            Tb5_Rdo_HORYU2.Enabled = false;

                            Tb_Main.Enabled = true;
                            Tb_Main.Refresh();
                        }
                        else
                        {
                            Txt_RYAKU.ReadOnlyEx = false;
                            Txt_TORI_NAM.ReadOnlyEx = false;
                            Txt_TRFURI.ReadOnlyEx = false;
                            Txt_KNLD.ReadOnlyEx = false;
                            Lbl_ZIP.Enabled = true;
                            Tb1_Txt_ZIP.ReadOnlyEx = false;
                            Lbl_ADDR1.Enabled = true;
                            Tb1_Txt_ADDR1.ReadOnlyEx = false;
                            Lbl_ADDR2.Enabled = true;
                            Tb1_Txt_ADDR2.ReadOnlyEx = false;

                            Tb1_Txt_TRMAIL.ReadOnlyEx = false;
                            Tb1_Txt_TRURL.ReadOnlyEx = false;
                            Tb1_Txt_BIKO.ReadOnlyEx = false;
                            Tb1_Txt_E_TANTOCD.ReadOnlyEx = Global.nSAIKEN_F != 1;
                            Tb1_Txt_MYNO_AITE.ReadOnlyEx = false;

                            Lbl_SBUSYO.Enabled = true;
                            Tb1_Txt_SBUSYO.ReadOnlyEx = false;
                            Lbl_STANTO.Enabled = true;
                            Tb1_Txt_STANTO.ReadOnlyEx = false;
                            Lbl_KEICD.Enabled = true;
                            Tb1_Cmb_KEICD.Enabled = true;
                            Lbl_TEL.Enabled = true;
                            Tb1_Txt_TEL.ReadOnlyEx = false;
                            Lbl_FAX.Enabled = true;
                            Tb1_Txt_FAX.ReadOnlyEx = false;
                            Lbl_STAN.Enabled = (Global.nKMAN == 0 ? false : true);
                            Tb5_Txt_STAN_CD.ReadOnlyEx = (Global.nKMAN == 0 ? true : false);
                            Lbl_SBCOD.Enabled = (Global.nBCOD_F == 0 ? false : true);
                            Tb5_Txt_SBCOD.ReadOnlyEx = (Global.nBCOD_F == 0 ? true : false);
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = false;
                            if ((Tb5_Chk_GENSEN.Checked || Tb5_Chk_OUTPUT.Checked) && Tb3_Rdo_GAI_F0.Checked)
                            {
                                Tb5_Chk_NAYOSE.Enabled = true;
                                Tb5_Chk_F_SETUIN.Checked = false;
                                Tb5_Chk_F_SETUIN.Enabled = false;
                            }
                            else if (Tb3_Rdo_GAI_F1.Checked)
                            {
                                Tb5_Chk_NAYOSE.Checked = false;
                                Tb5_Chk_NAYOSE.Enabled = false;
                                Tb5_Chk_F_SETUIN.Checked = false;
                                Tb5_Chk_F_SETUIN.Enabled = false;
                            }
                            else
                            {
                                Tb5_Chk_NAYOSE.Enabled = true;
                                Tb5_Chk_F_SETUIN.Enabled = true;
                            }
//-- <2016/03/14>
                            Chk_STFLG.Enabled = true;
                            cDfTitl1.Enabled = true;
                            Lbl_STYMD.Enabled = true;
                            Txt_STYMD.Enabled = true;
                            Lbl_EDYMD.Enabled = true;
                            Txt_EDYMD.Enabled = true;
                            cDfTitl2.Enabled = true;
                            Lbl_ZSTYMD.Enabled = true;
                            Txt_ZSTYMD.Enabled = true;
                            Lbl_ZEDYMD.Enabled = true;
                            Txt_ZEDYMD.Enabled = true;

                            Lbl_LUSR.Enabled = true;
                            Txt_LUSR.Enabled = true;
                            Lbl_LMOD.Enabled = true;
                            Txt_LMOD.Enabled = true;

                            // 第一タブは別箇所で制御

                            // 第二タブ
                            Tb4_Chk_GIN_ID.Enabled = false;
                            Tb4_BindNavi_Add.Enabled = true;
                            Tb4_BindNavi_DEL.Enabled = true;
                            Tb4_Chk_DDEF.Enabled = true;
                            Tb4_Cmb_FTESUID.Enabled = true;
                            Tb2_Chk_DTESUSW.Enabled = true;
                            if (Tb2_Chk_DTESUSW.Checked)
                            { Tb2_Cmb_DTESU.Enabled = true; }
                            else
                            { Tb2_Cmb_DTESU.Enabled = false; }

                            Tb4_Txt_BANK_CD.ReadOnlyEx = false;
                            Tb4_Txt_SITEN_ID.ReadOnlyEx = false;
                            Tb4_Cmb_YOKIN_TYP.Enabled = true;
                            Tb4_Txt_KOUZA.ReadOnlyEx = false;
                            Tb4_Txt_MEIGI.ReadOnlyEx = false;
                            Tb4_Txt_MEIGIK.ReadOnlyEx = false;
                            Tb4_Cmb_TESUU.Enabled = true;
                            Tb4_Cmb_SOUKIN.Enabled = true;
                            Tb4_Txt_GENDO.ReadOnlyEx = false;

                            // 第三タブは別箇所で制御

                            // 第四タブ
                            Tb5_Chk_OUTPUT.Enabled = true;
                            Tb5_Chk_GENSEN.Enabled = Tb5_Chk_OUTPUT.Checked;

                            // 第五タブ
                            Tb5_Cmb_F_SOUFU.Enabled = true;
                            Tb5_Cmb_ANNAI.Enabled = true;
                            Tb5_Cmb_TSOKBN.Enabled = true;
                            Tb5_Txt_TEGVAL.ReadOnlyEx = false;

                            Tb5_Cmb_SHITU.Enabled = true;
                            Tb5_Txt_DM1.ReadOnlyEx = false;
                            Tb5_Txt_DM2.ReadOnlyEx = false;
                            Tb5_Txt_DM3.ReadOnlyEx = false;
                            Tb5_Txt_FAC.ReadOnlyEx = false;
                            Tb1_Txt_UsrNo.ReadOnlyEx = false;
                            Tb1_Chk_Jyoto.Enabled = true;

                            if (Global.nSOSAI_F == 0)
                            { Tb1_Chk_SOSAI.Enabled = false; }
                            else
                            { Tb1_Chk_SOSAI.Enabled = SousaiEnabled; }
                            if (Tb1_Chk_SOSAI.Checked)
                            { Tb1_Chk_SRYOU_F.Enabled = true; }
                            if (Tb5_Chk_GENSEN.Checked || Tb5_Chk_OUTPUT.Checked)
                            {
                                Tb5_Rdo_HORYU0.Enabled = false;
                                Tb5_Rdo_HORYU1.Enabled = false;
                                Tb5_Rdo_HORYU2.Enabled = false;
                            }
                            else
                            {
                                Tb5_Rdo_HORYU0.Enabled = true;
                                Tb5_Rdo_HORYU1.Enabled = true;
                                Tb5_Rdo_HORYU2.Enabled = true;
                            }
                            if (!Tb5_Rdo_HORYU0.Checked)
                            {
                                Tb5_Chk_GENSEN.Enabled = false;
                                Tb5_Chk_OUTPUT.Enabled = false;
                            }
                            Tb_Main.Enabled = true;
                            Tb_Main.Refresh();
                        }
                    }
                }
                else
                {
                    if (Global.cUsrSec.nMFLG < 2 || (Txt_TRCD.ExCodeDB == "" && !Global.bIchigen))
                    {
                        Cbo_SAIKEN.Enabled = false;
                        Chk_SAIKEN_FLG.Enabled = false;
                        Cbo_SAIMU.Enabled = false;
                        Chk_SAIMU_FLG.Enabled = false;
                        Txt_GRPID.ReadOnlyEx = true;

                        Txt_RYAKU.ReadOnlyEx = true;
                        Txt_TORI_NAM.ReadOnlyEx = true;
                        Txt_TRFURI.ReadOnlyEx = true;
                        Txt_KNLD.ReadOnlyEx = true;
                        Lbl_ZIP.Enabled = true;
                        Tb1_Txt_ZIP.ReadOnlyEx = true;
                        Lbl_ADDR1.Enabled = true;
                        Tb1_Txt_ADDR1.ReadOnlyEx = true;
                        Lbl_ADDR2.Enabled = true;
                        Tb1_Txt_ADDR2.ReadOnlyEx = true;

                        Tb1_Txt_TRMAIL.ReadOnlyEx = true;
                        Tb1_Txt_TRURL.ReadOnlyEx = true;
                        Tb1_Txt_BIKO.ReadOnlyEx = true;
                        Tb1_Txt_E_TANTOCD.ReadOnlyEx = true;
                        Tb1_Txt_MYNO_AITE.ReadOnlyEx = true;

                        Lbl_SBUSYO.Enabled = true;
                        Tb1_Txt_SBUSYO.ReadOnlyEx = true;
                        Lbl_STANTO.Enabled = true;
                        Tb1_Txt_STANTO.ReadOnlyEx = true;
                        Lbl_KEICD.Enabled = true;
                        Tb1_Cmb_KEICD.Enabled = false;
                        Lbl_TEL.Enabled = true;
                        Tb1_Txt_TEL.ReadOnlyEx = true;
                        Lbl_FAX.Enabled = true;
                        Tb1_Txt_FAX.ReadOnlyEx = true;
                        Lbl_STAN.Enabled = true;
                        Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                        Lbl_SBCOD.Enabled = true;
                        Tb5_Txt_SBCOD.ReadOnlyEx = true;
                        Lbl_SKCOD.Enabled = true;
                        Tb5_Txt_SKCOD.ReadOnlyEx = true;
                        Tb5_Chk_NAYOSE.Enabled = false;
                        Tb5_Chk_F_SETUIN.Enabled = false;
                        Chk_STFLG.Enabled = false;
                        cDfTitl1.Enabled = true;
                        Lbl_STYMD.Enabled = true;
                        Txt_STYMD.Enabled = false;
                        Lbl_EDYMD.Enabled = true;
                        Txt_EDYMD.Enabled = false;
                        cDfTitl2.Enabled = true;
                        Lbl_ZSTYMD.Enabled = true;
                        Txt_ZSTYMD.Enabled = false;
                        Lbl_ZEDYMD.Enabled = true;
                        Txt_ZEDYMD.Enabled = false;

                        Lbl_LUSR.Enabled = true;
                        Txt_LUSR.Enabled = false;
                        Lbl_LMOD.Enabled = true;
                        Txt_LMOD.Enabled = false;

                        // 第一タブは別箇所で制御

                        // 第二タブ
                        Tb4_Chk_GIN_ID.Enabled = false;
                        Tb4_BindNavi_Add.Enabled = false;
                        Tb4_BindNavi_DEL.Enabled = false;

                        Tb4_Chk_FDEF.Enabled = false;
                        Tb4_Chk_DDEF.Enabled = false;
                        Tb4_Cmb_FTESUID.Enabled = false;
                        Tb2_Chk_DTESUSW.Enabled = false;
                        Tb2_Cmb_DTESU.Enabled = false;

                        Tb4_Txt_BANK_CD.ReadOnlyEx = true;
                        Tb4_Txt_SITEN_ID.ReadOnlyEx = true;
                        Tb4_Cmb_YOKIN_TYP.Enabled = false;
                        Tb4_Txt_KOUZA.ReadOnlyEx = true;
                        Tb4_Txt_MEIGI.ReadOnlyEx = true;
                        Tb4_Txt_MEIGIK.ReadOnlyEx = true;
                        Tb4_Cmb_TESUU.Enabled = false;
                        Tb4_Cmb_SOUKIN.Enabled = false;
                        Tb4_Txt_GENDO.ReadOnlyEx = true;

                        // 第三タブは別箇所で制御

                        // 第四タブ
                        Tb5_Chk_GENSEN.Enabled = false;
                        Tb5_Chk_OUTPUT.Enabled = false;

                        // 第五タブ
                        Tb5_Cmb_F_SOUFU.Enabled = false;
                        Tb5_Cmb_ANNAI.Enabled = false;
                        Tb5_Cmb_TSOKBN.Enabled = false;
                        Tb5_Txt_TEGVAL.ReadOnlyEx = true;

                        Tb5_Cmb_SHITU.Enabled = false;
                        Tb5_Txt_DM1.ReadOnlyEx = true;
                        Tb5_Txt_DM2.ReadOnlyEx = true;
                        Tb5_Txt_DM3.ReadOnlyEx = true;
                        Tb5_Txt_FAC.ReadOnlyEx = true;
                        Tb1_Txt_UsrNo.ReadOnlyEx = true;
                        Tb1_Chk_Jyoto.Enabled = false;

                        Tb1_Chk_SOSAI.Enabled = false;
                        Tb1_Chk_SRYOU_F.Enabled = false;

                        Tb5_Rdo_HORYU0.Enabled = false;
                        Tb5_Rdo_HORYU1.Enabled = false;
                        Tb5_Rdo_HORYU2.Enabled = false;

                        Tb_Main.Enabled = false;
                    }
                    else
                    {
                        Txt_RYAKU.ReadOnlyEx = false;
                        Txt_TORI_NAM.ReadOnlyEx = false;
                        Txt_TRFURI.ReadOnlyEx = false;
                        Txt_KNLD.ReadOnlyEx = false;
                        Lbl_ZIP.Enabled = true;
                        Tb1_Txt_ZIP.ReadOnlyEx = false;
                        Lbl_ADDR1.Enabled = true;
                        Tb1_Txt_ADDR1.ReadOnlyEx = false;
                        Lbl_ADDR2.Enabled = true;
                        Tb1_Txt_ADDR2.ReadOnlyEx = false;

                        Tb1_Txt_TRMAIL.ReadOnlyEx = false;
                        Tb1_Txt_TRURL.ReadOnlyEx = false;
                        Tb1_Txt_BIKO.ReadOnlyEx = false;
                        Tb1_Txt_E_TANTOCD.ReadOnlyEx = Global.nSAIKEN_F != 1;
                        Tb1_Txt_MYNO_AITE.ReadOnlyEx = false;

                        Lbl_SBUSYO.Enabled = true;
                        Tb1_Txt_SBUSYO.ReadOnlyEx = false;
                        Lbl_STANTO.Enabled = true;
                        Tb1_Txt_STANTO.ReadOnlyEx = false;
                        Lbl_KEICD.Enabled = true;
                        Tb1_Cmb_KEICD.Enabled = true;
                        Lbl_TEL.Enabled = true;
                        Tb1_Txt_TEL.ReadOnlyEx = false;
                        Lbl_FAX.Enabled = true;
                        Tb1_Txt_FAX.ReadOnlyEx = false;
                        Chk_STFLG.Enabled = true;
                        cDfTitl1.Enabled = true;
                        Lbl_STYMD.Enabled = true;
                        Txt_STYMD.Enabled = true;
                        Lbl_EDYMD.Enabled = true;
                        Txt_EDYMD.Enabled = true;
                        cDfTitl2.Enabled = true;
                        Lbl_ZSTYMD.Enabled = true;
                        Txt_ZSTYMD.Enabled = true;
                        Lbl_ZEDYMD.Enabled = true;
                        Txt_ZEDYMD.Enabled = true;

                        Lbl_LUSR.Enabled = true;
                        Txt_LUSR.Enabled = true;
                        Lbl_LMOD.Enabled = true;
                        Txt_LMOD.Enabled = true;

                        // 第一タブは別箇所で制御

                        // 第二タブ
                        Tb4_Chk_GIN_ID.Enabled = false;
                        Tb4_BindNavi_Add.Enabled = true;
                        Tb4_BindNavi_DEL.Enabled = true;
                        Tb4_Chk_DDEF.Enabled = true;
                        Tb4_Cmb_FTESUID.Enabled = true;
                        Tb2_Chk_DTESUSW.Enabled = true;
                        if (Tb2_Chk_DTESUSW.Checked)
                        { Tb2_Cmb_DTESU.Enabled = true; }
                        else
                        { Tb2_Cmb_DTESU.Enabled = false; }
                        Tb4_Txt_BANK_CD.ReadOnlyEx = false;
                        Tb4_Txt_SITEN_ID.ReadOnlyEx = false;
                        Tb4_Cmb_YOKIN_TYP.Enabled = true;
                        Tb4_Txt_KOUZA.ReadOnlyEx = false;
                        Tb4_Txt_MEIGI.ReadOnlyEx = false;
                        Tb4_Txt_MEIGIK.ReadOnlyEx = false;
                        Tb4_Cmb_TESUU.Enabled = true;
                        Tb4_Cmb_SOUKIN.Enabled = true;
                        Tb4_Txt_GENDO.ReadOnlyEx = false;

                        // 第三タブは別箇所で制御

                        // 第四タブ
                        Tb5_Chk_OUTPUT.Enabled = true;
                        Tb5_Chk_GENSEN.Enabled = Tb5_Chk_OUTPUT.Checked;

                        // 第五タブ
                        if (Cbo_SAIMU.SelectedValue.ToString() != sUse)
                        {
                            Lbl_STAN.Enabled = false;
                            Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                            Lbl_SBCOD.Enabled = (Global.nBCOD_F == 0 ? false : true);
                            Tb5_Txt_SBCOD.ReadOnlyEx = (Global.nBCOD_F == 0 ? true : false);
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = false;
                            Tb5_Chk_NAYOSE.Enabled = false;
                            Tb5_Chk_F_SETUIN.Enabled = false;
                        }
                        else
                        {
                            Lbl_STAN.Enabled = true;
                            Tb5_Txt_STAN_CD.ReadOnlyEx = false;
                            Lbl_SBCOD.Enabled = true;
                            Tb5_Txt_SBCOD.ReadOnlyEx = false;
                            Lbl_SKCOD.Enabled = true;
                            Tb5_Txt_SKCOD.ReadOnlyEx = false;
                            Tb5_Chk_NAYOSE.Enabled = true;
                            Tb5_Chk_F_SETUIN.Enabled = true;
                        }
                        Tb5_Cmb_F_SOUFU.Enabled = true;
                        Tb5_Cmb_ANNAI.Enabled = true;
                        Tb5_Cmb_TSOKBN.Enabled = true;
                        Tb5_Txt_TEGVAL.ReadOnlyEx = false;

                        Tb5_Cmb_SHITU.Enabled = true;
                        Tb5_Txt_DM1.ReadOnlyEx = false;
                        Tb5_Txt_DM2.ReadOnlyEx = false;
                        Tb5_Txt_DM3.ReadOnlyEx = false;
                        Tb5_Txt_FAC.ReadOnlyEx = false;
                        Tb1_Txt_UsrNo.ReadOnlyEx = false;
                        Tb1_Chk_Jyoto.Enabled = true;

                        if (Global.nSOSAI_F == 0)
                        { Tb1_Chk_SOSAI.Enabled = false; }
                        else
                        { Tb1_Chk_SOSAI.Enabled = true; }

                        if (Tb1_Chk_SOSAI.Checked)
                        { Tb1_Chk_SRYOU_F.Enabled = true; }

                        if (Tb5_Chk_GENSEN.Checked || Tb5_Chk_OUTPUT.Checked)
                        {
                            Tb5_Rdo_HORYU0.Enabled = false;
                            Tb5_Rdo_HORYU1.Enabled = false;
                            Tb5_Rdo_HORYU2.Enabled = false;
                        }
                        else
                        {
                            Tb5_Rdo_HORYU0.Enabled = true;
                            Tb5_Rdo_HORYU1.Enabled = true;
                            Tb5_Rdo_HORYU2.Enabled = true;
                        }
                        if (!Tb5_Rdo_HORYU0.Checked)
                        {
                            Tb5_Chk_GENSEN.Enabled = false;
                            Tb5_Chk_OUTPUT.Enabled = false;
                        }
                        Tb_Main.Enabled = true;
                    }
                }
            }

            if (Global.bIchigen)
            {
                if (Global.cUsrSec.nMFLG < 2)                           // 参照権限
                {
                    //一見登録時固有処理
                    BindNavi1.Enabled = false;
                    Txt_TRCD.ReadOnlyEx = true;
                    Txt_HJCD.ReadOnlyEx = true;
                    Txt_RYAKU.ReadOnlyEx = true;
                    Txt_TORI_NAM.ReadOnlyEx = true;
                    Txt_TRFURI.ReadOnlyEx = true;
                    Txt_KNLD.ReadOnlyEx = true;
                    if (Global.nShTgSW == 0)
                    {
                        // 債務業務から呼び出し
                        Cbo_SAIKEN.SelectedValue = sNotUse;
                        Cbo_SAIMU.SelectedValue = sUse;
                    }
                    else
                    {
                        // 期日業務から呼び出し
                        Cbo_SAIKEN.SelectedValue = sDueOnly;
                        Cbo_SAIMU.SelectedValue = sDueOnly;
                    }
                    Cbo_SAIKEN.Enabled = false;
                    Cbo_SAIMU.Enabled = false;
                }
                else
                {
                    //一見登録時固有処理
                    BindNavi1.Enabled = false;
                    Txt_TRCD.ReadOnlyEx = true;
                    Txt_HJCD.ReadOnlyEx = true;
                    Txt_RYAKU.ReadOnlyEx = false;
                    Txt_TORI_NAM.ReadOnlyEx = false;
                    Txt_TRFURI.ReadOnlyEx = false;
                    Txt_KNLD.ReadOnlyEx = true;
                    if (Global.nShTgSW == 0)
                    {
                        // 債務業務から呼び出し
                        Cbo_SAIKEN.SelectedValue = sNotUse;
                        Cbo_SAIMU.SelectedValue = sUse;
                    }
                    else
                    {
                        // 期日業務から呼び出し
                        Cbo_SAIKEN.SelectedValue = sDueOnly;
                        Cbo_SAIMU.SelectedValue = sDueOnly;
                    }
                    Cbo_SAIKEN.Enabled = false;
                    Chk_SAIKEN_FLG.Enabled = false;
                    Cbo_SAIMU.Enabled = false;
                    Chk_SAIMU_FLG.Enabled = false;
                    Tb3_Rdo_GAI_F1.Enabled = false;
                    Tb5_Chk_NAYOSE.Checked = false;
                    Tb5_Chk_NAYOSE.Enabled = false;
                    Txt_GRPID.Enabled = false;
                }
            }

            //回収設定タブ制御
            Chg_Kaisyu_Control();

            //支払条件タブ制御
            this.Chg_SHINO_Control();
            if (Tb3_Rdo_GAI_F1.Checked)
            {
                Tb3_Chk_SHO_ID.Enabled = false;
                Tb3_Cmb_HARAI_H.Enabled = false;
                Tb3_Cmb_KIJITU_H.Enabled = false;
                Tb3_BindNavi_Add.Enabled = false;
                Tb3_BindNavi_DEL.Enabled = false;
            }

            // 外貨設定タブ制御
            Chg_Gaika_Control();

            //F6ボタン制御
            // セキュリティ対応(ﾏｽﾀ権限：削除未満の場合、削除処理を使用不可にする)
            if (Global.cUsrSec.nMFLG < 3)
            {
                FKB.F06_Enabled = false;
                MNU_DELETE.Enabled = false;
            }
            else
            {
                if (Lbl_Old_New1.Text == "【　変更　】")
                {
                    if (!(Global.sTRCD_R != "" && Global.sTRNAM_R != ""))
                    {
                        string sDaiCd = "";
                        string sDHjCd = "";
                        if (mcBsLogic.Get_SaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd) ||
                            mcBsLogic.Get_MySaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") ||
                            mcBsLogic.Get_SaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", out sDaiCd, out sDHjCd) ||
                            mcBsLogic.Get_MySaikenDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") ||
                            mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text) ||
                            mcBsLogic.Exists_Sousai_Tokui(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
                        {
                            FKB.F06_Enabled = false;
                            MNU_DELETE.Enabled = false;
                        }
                        else
                        {
                            FKB.F06_Enabled = true;
                            MNU_DELETE.Enabled = true;
                        }
                    }
                }
                else
                {
                    FKB.F06_Enabled = false;
                    MNU_DELETE.Enabled = false;
                }
            }

            if ((Tb3_Txt_SHIMEBI.Text == "0") ||
                (Tb3_Txt_SHIMEBI.Text == "31") ||
                (Tb3_Txt_SHIMEBI.Text == "99"))
            {
                Tb3_Txt_SHIMEBI.Text = "末";
            }

            if ((Tb3_Txt_SIHARAIDD.Text == "0") ||
                (Tb3_Txt_SIHARAIDD.Text == "31") ||
                (Tb3_Txt_SIHARAIDD.Text == "99"))
            {
                Tb3_Txt_SIHARAIDD.Text = "末";
            }
            if ((Tb3_Txt_SKIJITUDD.Text == "0") ||
                (Tb3_Txt_SKIJITUDD.Text == "31") ||
                (Tb3_Txt_SKIJITUDD.Text == "99"))
            {
                Tb3_Txt_SKIJITUDD.Text = "末";
            }

            if (Global.bIchigen)
            {
                Tb3_Rdo_GAI_F1.Enabled = false;
                Tb5_Chk_NAYOSE.Checked = false;
                Tb5_Chk_NAYOSE.Enabled = false;
            }

            if (Global.nGAIKA_F == 0)                                       // 外貨の使用なし
            {
                if (Global.nSAIMU_F == 1)
                {
                    TabControl.TabPageCollection tabPages = Tb_Main.TabPages;
                    tabPages[5].Enabled = false;                                // 外貨設定タブ
                    labelBase49.Visible = false;
                    Tb3_Rdo_GAI_F0.Visible = false;
                    Tb3_Rdo_GAI_F1.Visible = false;
                }
                if (Global.nSAIKEN_F == 1)
                {
                    groupBox8.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 得意先コンボボックスのリストを作成する
        /// </summary>
        /// <param name="enableNotUse"></param>
        /// <returns></returns>
        private List<ComboList> Create_Cbo_SAIKEN_List(bool enableNotUse)
        {
            List<ComboList> comboList = new List<ComboList>();
            if (enableNotUse)
            {
                comboList.Add(new ComboList("使用しない", sNotUse));
            }
            if (Global.nSAIKEN_F == 1) comboList.Add(new ComboList("使用する", sUse));
            if (Global.nKIJITU_F == 1) comboList.Add(new ComboList("期日管理のみ", sDueOnly));

            return comboList;
        }

        /// <summary>
        /// 仕入先コンボボックスのリストを作成する
        /// </summary>
        /// <param name="enableNotUse"></param>
        /// <returns></returns>
        private List<ComboList> Create_Cbo_SAIMU_List(bool enableNotUse)
        {
            List<ComboList> comboList = new List<ComboList>();
            if (enableNotUse)
            {
                comboList.Add(new ComboList("使用しない", sNotUse));
            }
            if (Global.nSAIMU_F == 1) comboList.Add(new ComboList("使用する", sUse));
            if (Global.nKIJITU_F == 1) comboList.Add(new ComboList("期日管理のみ", sDueOnly));

            return comboList;
        }

        #endregion

        #region 回収設定タブ制御
        private void Chg_Kaisyu_Control()
        {
            if (Global.cUsrSec.nMFLG < 2)
            {
                // 入金消込設定
                Tb2_Txt_TOKUKANA.ReadOnlyEx = true;
                Tb2_Cmb_FUTAN.Enabled = false;

                // 回収予定設定
                if (Tb2_Chk_YAKUJO.Checked == false)
                {
                    Tb2_Chk_YAKUJO.Enabled = false;
                    Tb2_Cmb_KAISYU.Enabled = false;
                    Tb2_Txt_SHIME.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUHI_M.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUHI_D.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUSIGHT_M.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUSIGHT_D.ReadOnlyEx = true;
                    Tb2_Cmb_HOLIDAY.Enabled = false;

                    Tb2_Txt_Y_KINGAKU.Enabled = false;
                    Tb2_Cmb_HOLIDAY.Enabled = false;
                    Tb2_Cmb_MIMAN.Enabled = false;
                    Tb2_Cmb_IJOU_1.Enabled = false;
                    Tb2_Txt_BUNKATSU_1.Enabled = false;
                    Tb2_Cmb_HASU_1.Enabled = false;
                    Tb2_Txt_SIGHT_M_1.Enabled = false;
                    Tb2_Txt_SIGHT_D_1.Enabled = false;
                    Tb2_Cmb_IJOU_2.Enabled = false;
                    Tb2_Txt_BUNKATSU_2.Enabled = false;
                    Tb2_Cmb_HASU_2.Enabled = false;
                    Tb2_Txt_SIGHT_M_2.Enabled = false;
                    Tb2_Txt_SIGHT_D_2.Enabled = false;
                    Tb2_Cmb_IJOU_3.Enabled = false;
                    Tb2_Txt_BUNKATSU_3.Enabled = false;
                    Tb2_Cmb_HASU_3.Enabled = false;
                    Tb2_Txt_SIGHT_M_3.Enabled = false;
                    Tb2_Txt_SIGHT_D_3.Enabled = false;
                }
                else
                {
                    Tb2_Chk_YAKUJO.Enabled = false;
                    Tb2_Cmb_KAISYU.Enabled = false;
                    Tb2_Txt_SHIME.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUHI_M.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUHI_D.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUSIGHT_M.ReadOnlyEx = true;
                    Tb2_Txt_KAISYUSIGHT_D.ReadOnlyEx = true;
                    Tb2_Cmb_HOLIDAY.Enabled = false;

                    Tb2_Txt_Y_KINGAKU.ReadOnlyEx = true;
                    Tb2_Cmb_HOLIDAY.Enabled = false;
                    Tb2_Cmb_MIMAN.Enabled = false;
                    Tb2_Cmb_IJOU_1.Enabled = false;
                    Tb2_Txt_BUNKATSU_1.ReadOnlyEx = true;
                    Tb2_Cmb_HASU_1.Enabled = false;
                    Tb2_Txt_SIGHT_M_1.ReadOnlyEx = true;
                    Tb2_Txt_SIGHT_D_1.ReadOnlyEx = true;
                    Tb2_Cmb_IJOU_2.Enabled = false;
                    Tb2_Txt_BUNKATSU_2.ReadOnlyEx = true;
                    Tb2_Cmb_HASU_2.Enabled = false;
                    Tb2_Txt_SIGHT_M_2.ReadOnlyEx = true;
                    Tb2_Txt_SIGHT_D_2.ReadOnlyEx = true;
                    Tb2_Cmb_IJOU_3.Enabled = false;
                    Tb2_Txt_BUNKATSU_3.ReadOnlyEx = true;
                    Tb2_Cmb_HASU_3.Enabled = false;
                    Tb2_Txt_SIGHT_M_3.ReadOnlyEx = true;
                    Tb2_Txt_SIGHT_D_3.ReadOnlyEx = true;
                }

                // 専用入金口座（仮想口座）
                Tb2_Txt_SEN_GINKOCD.ReadOnly = true;
                Tb2_Txt_SEN_SITENCD.ReadOnlyEx = true;
                Tb2_Txt_SEN_KSITENCD.ReadOnlyEx = true;
                Tb2_Txt_SEN_KSITENNM.ReadOnlyEx = true;
                Tb2_Cmb_YOKINSYU.Enabled = false;
                Tb2_Txt_SEN_KOZANO.ReadOnlyEx = true;

                // 各設定
                Tb2_Chk_JIDOU_GAKUSYU.Enabled = false;
                Tb2_Chk_NYUKIN_YOTEI.Enabled = false;
                Tb2_Chk_RYOSYUSYO.Enabled = false;
                Tb2_Chk_TESURYO_GAKUSYU.Enabled = false;
                Tb2_Chk_TESURYO_GOSA.Enabled = false;
                Tb2_Txt_SHIN_KAISYACD.ReadOnlyEx = true;
                Tb2_Txt_YOSIN.ReadOnlyEx = true;
                Tb2_Txt_YOSHINRANK.ReadOnlyEx = true;

                // 外貨関連
                Tb2_Chk_GAIKA.Enabled = false;
                Tb2_Cmb_TSUKA.Enabled = false;
                Tb2_Txt_GAIKA_KEY_F.ReadOnlyEx = true;
                Tb2_Txt_GAIKA_KEY_B.ReadOnlyEx = true;

                // 被振込口座設定
                Tb2_Chk_HiFuri_1.Enabled = false;
                Tb2_Txt_HIFURIKOZA_1.ReadOnlyEx = true;
                Tb2_Chk_HiFuri_2.Enabled = false;
                Tb2_Txt_HIFURIKOZA_2.ReadOnlyEx = true;
                Tb2_Chk_HiFuri_3.Enabled = false;
                Tb2_Txt_HIFURIKOZA_3.ReadOnlyEx = true;
            }
            else
            {
                Tb2_Chk_GAIKA.Enabled = !mcBsLogic.Exists_Sousai_Tokui(Txt_TRCD.ExCodeDB, Txt_HJCD.Text);
            }
        }
        #endregion

        #region 外貨設定タブ制御
        private void Chg_Gaika_Control()
        {
            if (Global.cUsrSec.nMFLG < 2)
            {
                Tb6_Cmb_HEI_CD.Enabled = false;
                Tb6_Rdo_GAI_SF0.Enabled = false;
                Tb6_Rdo_GAI_SF1.Enabled = false;
                Tb6_Rdo_GAI_SH0.Enabled = false;
                Tb6_Rdo_GAI_SH1.Enabled = false;
                Tb6_Cmb_GAI_KZID.Enabled = false;
                Tb6_Cmb_GAI_TF.Enabled = false;
                Tb6_Txt_ENG_NAME.ReadOnlyEx = true;
                Tb6_Txt_ENG_ADDR.ReadOnlyEx = true;
                Tb6_Txt_ENG_KZNO.ReadOnlyEx = true;
                Tb6_Txt_ENG_SWIF.ReadOnlyEx = true;
                Tb6_Txt_ENG_BNKNAM.ReadOnlyEx = true;
                Tb6_Txt_ENG_BRNNAM.ReadOnlyEx = true;
                Tb6_Txt_ENG_BNKADDR.ReadOnlyEx = true;
            }
        }
        #endregion

        #region 支払条件タブ関連

        /// <summary>
        /// 会社情報登録の「部門別に作成する」のフラグが寝ている場合
        /// </summary>
        private void Chg_SHINO_Control()
        {
            bool bGaika = true;
            if (Chk_SAIMU_FLG.Checked == true
                || mcBsLogic.Chk_SaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0")
                || mcBsLogic.Get_MySaimuDaihyo(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0"))
            {
                bGaika = false;
            }

            if (Global.nBCD_ZMAK == 0)
            {
                if (Tb3_Lbl_Old_New2.Text == "【　新規　】")
                {
                    Tb3_Txt_BCOD.ReadOnlyEx = true;
                    Tb3_Txt_BNAM.Text = "全て";
//-- <2016/03/10 表示のみに修正>
//                    Tb3_Chk_SHO_ID.Enabled = Global.bEnabledState;
//-- <2016/03/10>
                    Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                    Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
                    Tb3_Txt_KCOD.ReadOnlyEx = Global.bReadOnlyState;
                    Tb3_Txt_SHINO.ReadOnlyEx = Global.bReadOnlyState;
                    Tb1_Btn_SHINO.Enabled = Global.bEnabledState;
                    Tb3_Cmb_HARAI_H.Enabled = Global.bEnabledState;
                    Tb3_Cmb_KIJITU_H.Enabled = Global.bEnabledState;
                    Tb3_Lbl_Old_New2.ForeColor = Color.Black;

                    Tb3_Rdo_GAI_F0.Enabled = Global.bEnabledState;
                    Tb3_Rdo_GAI_F1.Enabled = Global.bEnabledState && bGaika && !mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text);
                    Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                    Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
                }
                else
                {
                    //変更・未使用
                    if (!string.IsNullOrEmpty(Tb3_Txt_BCOD.ExCodeDB))
                    {
//-- <2016/03/10 表示のみに修正>
//                        Tb3_Chk_SHO_ID.Enabled = Global.bEnabledState;
//-- <2016/03/10>
                        Tb3_Txt_BCOD.ReadOnlyEx = true;
                        Tb3_Txt_KCOD.ReadOnlyEx = true;
                        Tb3_Txt_SHINO.ReadOnlyEx = true;
                        Tb1_Btn_SHINO.Enabled = false;
                        Tb3_Cmb_HARAI_H.Enabled = false;
                        Tb3_Cmb_KIJITU_H.Enabled = false;
                        Tb3_Lbl_Old_New2.Text = "【　未使用　】";
                        Tb3_Lbl_Old_New2.ForeColor = Color.Red;

                        Tb3_Rdo_GAI_F0.Enabled = false;
                        Tb3_Rdo_GAI_F1.Enabled = false;
                        // --->V02.22.01 KKL UPDATE ▼(108560)
                        //Tb3_BindNavi_Add.Enabled = false;
                        //Tb3_BindNavi_DEL.Enabled = false;
                        Tb3_BindNavi_Add.Enabled = true;
                        Tb3_BindNavi_DEL.Enabled = true;
                        Tb3_Cmb_HARAI_KBN1.Enabled = false;
                        Tb3_Cmb_HARAI_KBN2.Enabled = false;
                        Tb3_Cmb_HARAI_KBN3.Enabled = false;
                        Tb3_Cmb_HARAI_KBN4.Enabled = false;
                        // <---V02.22.01 KKL UPDATE ▲(108560)
                    }
                    else
                    {
//-- <2016/03/10 表示のみに修正>
//                        Tb3_Chk_SHO_ID.Enabled = Global.bEnabledState;
//-- <2016/03/10>
                        Tb3_Txt_BCOD.ReadOnlyEx = true;
                        if (Global.GAI_F == "0")
                        {
                            Tb3_Txt_BNAM.Text = "全て";
                        }
                        else
                        {
                            Tb3_Txt_BNAM.Text = "";
                        }
                        Tb3_Txt_KCOD.ReadOnlyEx = Global.bReadOnlyState;
                        Tb3_Txt_SHINO.ReadOnlyEx = Global.bReadOnlyState;
                        Tb1_Btn_SHINO.Enabled = Global.bEnabledState;
                        Tb3_Cmb_HARAI_H.Enabled = Global.bEnabledState;
                        Tb3_Cmb_KIJITU_H.Enabled = Global.bEnabledState;
                        Tb3_Lbl_Old_New2.ForeColor = Color.Black;

                        Tb3_Rdo_GAI_F0.Enabled = Global.bEnabledState;
                        Tb3_Rdo_GAI_F1.Enabled = Global.bEnabledState && bGaika && !mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text);
                        if (Global.bEnabledState)
                        {
                            if (Tb3_Rdo_GAI_F0.Checked)
                            {
                                Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                                Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
                            }
                        }
                        else
                        {
                            Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                            Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
                        }
                    }
                }
            }
            else
            {
//-- <2016/03/10 表示のみに修正>
//                Tb3_Chk_SHO_ID.Enabled = Global.bEnabledState;
//-- <2016/03/10>
                Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
                Tb3_Txt_KCOD.ReadOnlyEx = Global.bReadOnlyState;
                Tb3_Txt_BCOD.ReadOnlyEx = Global.bReadOnlyState;
                Tb3_Txt_SHINO.ReadOnlyEx = Global.bReadOnlyState;
                Tb1_Btn_SHINO.Enabled = Global.bEnabledState;
                Tb3_Cmb_HARAI_H.Enabled = Global.bEnabledState;
                Tb3_Cmb_KIJITU_H.Enabled = Global.bEnabledState;

                Tb3_Rdo_GAI_F0.Enabled = Global.bEnabledState;
                Tb3_Rdo_GAI_F1.Enabled = Global.bEnabledState && bGaika && !mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text);
                Tb3_BindNavi_Add.Enabled = Global.bEnabledState;
                Tb3_BindNavi_DEL.Enabled = Global.bEnabledState;
            }
        }

        /// <summary>
        /// 支払条件タブの検索
        /// </summary>
        private void Sel_SS_TSHOH(bool hasChanged)// <--- V02.37.01 YMP UPDATE ◀122172)引数に変更状態を追加
        {
            try
            {
                //現在の取引先コードで取引先支払方法テーブルを検索
                string sTRCD = Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB.TrimEnd(' ');
                string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                int iSS_TSHOH_cnt;
                mcBsLogic.Sel_SS_TSHOH(sTRCD, sHJCD, out iSS_TSHOH_cnt);

                //**>>
                Global.nTSHOH_cnt = iSS_TSHOH_cnt;
                //**<<

                //画面に検索結果を設定
                // ---> V02.37.01 YMP UPDATE ▼(122172)
                //Set_Tb1_SS_TSHOH(1, iSS_TSHOH_cnt);
                int iCurrentCount = !hasChanged ? 1 : int.Parse(BindNavi2_Selected.Text);
                Set_Tb1_SS_TSHOH(iCurrentCount, iSS_TSHOH_cnt);
                // <--- V02.37.01 YMP UPDATE ▲(122172)
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
        }

        /// <summary>
        /// 支払条件タブ.支払条件選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb1_Btn_SHINO_Click(object sender, EventArgs e)
        {
            DialogManager.ShiharaiData srcShiharaiData = DlgMng.DispShiharai();
            if (srcShiharaiData != null)
            {
                Tb3_Txt_SHINO.Text = srcShiharaiData.NO.ToString().PadLeft(3, '0');
                Tb1_Txt_SHINO_Validating(Tb3_Txt_SHINO, null);
                SendKeys.Send("{TAB}");
            }
        }

        /// <summary>
        /// 号の生成
        /// </summary>
        private void Generate_Tb4_Cmb_GOU()
        {
            try
            {
                System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                list = (
                    new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                );
                System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                list2 = (
                    new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                );
                int iCombo;
                string sCombo;
                if (Tb5_Radio_GENSEN1.Checked == true)
                {
                    iCombo = 0;
                    sCombo = "";
                    list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                    iCombo = 1;
                    sCombo = "1:原稿料・作曲料等";
                    list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                    iCombo = 2;
                    sCombo = "2:弁護士・税理士等";
                    list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));

                    Tb5_Cmb_GOU.DisplayMember = "Value";
                    Tb5_Cmb_GOU.ValueMember = "Key";
                    Tb5_Cmb_GOU.DataSource = list;
                }
                else
                {
                    iCombo = 0;
                    sCombo = "2:弁護士・税理士等";
                    list2.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));

                    Tb5_Cmb_GOU.DisplayMember = "Value";
                    Tb5_Cmb_GOU.ValueMember = "Key";
                    Tb5_Cmb_GOU.DataSource = list2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGenerate_Tb4_Cmb_GOU　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        #endregion

        #region 振込先情報タブ関連
        /// <summary>
        /// 振込先情報タブの検索
        /// </summary>
        private void Sel_SS_FRIGIN(bool hasChanged)// <--- V02.37.01 YMP UPDATE ◀(122172)引数に変更状態を追加
        {
            try
            {
                //現在の取引先コードで取引先銀行テーブルを検索
                string sTRCD = Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB.TrimEnd(' ');
                string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                int iSS_FRIGIN_cnt;
                mcBsLogic.Sel_SS_FRIGIN(sTRCD, sHJCD, out iSS_FRIGIN_cnt);

                //**>>
                Global.nFRGIN_cnt = iSS_FRIGIN_cnt;
                //**<<

                //画面への値設定
                // ---> V02.37.01 YMP UPDATE ▼(122172)
                //Set_Tb2_SS_FRIGIN(1, iSS_FRIGIN_cnt);
                int iCurretCount = !hasChanged ? 1 : int.Parse(Tb4_BindNavi_Selected.Text);
                Set_Tb2_SS_FRIGIN(iCurretCount, iSS_FRIGIN_cnt);
                // <--- V02.37.01 YMP UPDATE ▲(122172)
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
        }
        #endregion

        private List<KeyValuePair<int, string>> GetTesuuID(string sOwnBkCod)
        {
            var TesuuIdList = new List<KeyValuePair<int, string>>();
            DataTable dt = mcBsLogic.GetTesuuIdInfo(sOwnBkCod);
            if (dt.Rows.Count == 0) return TesuuIdList;

            foreach(DataRow dr in dt.Rows)
            {
                int key = Convert.ToInt32(dr["TESUID"].ToString());
                string item = key.ToString() + ":" + dr["TESUNAM"].ToString();
                TesuuIdList.Add(new KeyValuePair<int, string>(key, item));
            }

            return TesuuIdList;
        }
        #region 依頼先情報タブ関連
        private List<KeyValuePair<int, OwnBank>> GetOwnBankList(bool factoring)
        {
            var ownBankList = new List<KeyValuePair<int, OwnBank>>();
            mcBsLogic.Get_OWNBK(out sOWNIDArray, out sFACIDArray,
                                out sBKNAMArray, out sBRNAMArray, out sYKNKINDArray,
                                out sKOZANOArray, out sIRAININArray, out sFACNAMArray);
            if (sBKNAMArray == null) { return ownBankList; }

            for (int i = 0; i < sBKNAMArray.GetLength(0); i++)
            {
                //FA名称が設定されているデータは一括専用
                if (!factoring)
                {
                    //if (sFACNAMArray[i].ToString() != "") { continue; }
                    int key = i;
                    //string sIrainin = sIRAININArray[i];

                    //if (sIrainin == "0000000000")
                    //{
                    //    sIrainin = "          ";
                    //}
                    string sCombo = mcBsLogic.StringCut(string.Format("{0,-30}", sBKNAMArray[i, 1].ToString()), 30) + "："
                                   + mcBsLogic.StringCut(string.Format("{0,-30}", sBRNAMArray[i, 1].ToString()), 30) + "："
                                   + sYKNKINDArray[i, 1].ToString().PadRight(4, '　') + "："
                                   + sKOZANOArray[i].ToString().PadRight(7); //+"：" + sIrainin; // sIRAININArray[i];
                    var item = new OwnBank()
                    {
                        OwnId = sOWNIDArray[i],
                        FacId = sFACIDArray[i],
                        Bank = sBKNAMArray[i, 0],
                        Branch = sBRNAMArray[i, 0],
                        Kind = sYKNKINDArray[i, 0],
                        Account = sKOZANOArray[i],
                        //ContractNo = sIRAININArray[i],
                        ContractNo = sOWNIDArray[i],
                        Item = sCombo,
                    };
                    ownBankList.Add(new KeyValuePair<int, OwnBank>(key, item));
                }
                else
                {
                    if (sFACNAMArray[i].ToString() == "") { continue; }
                    int key = i;
                    //string sIrainin = sIRAININArray[i];

                    //if (sIrainin == "0000000000")
                    //{
                    //    sIrainin = "          ";
                    //}
                    string sCombo = mcBsLogic.StringCut(string.Format("{0,-59}", sFACNAMArray[i].ToString()), 59); //+"：" + sIrainin; // sIRAININArray[i];
                    var item = new OwnBank()
                    {
                        OwnId = sOWNIDArray[i],
                        FacId = sFACIDArray[i],
                        Bank = sBKNAMArray[i, 0],
                        Branch = sBRNAMArray[i, 0],
                        Kind = sYKNKINDArray[i, 0],
                        Account = sKOZANOArray[i],
                        //ContractNo = sIRAININArray[i],
                        Factoring = sFACNAMArray[i],
                        Item = sCombo,
                    };
                    ownBankList.Add(new KeyValuePair<int, OwnBank>(key, item));
                }
            }
            return ownBankList;
        }

        /// <summary>
        /// 支払区分が変更される都度に依頼先情報タブの支払方法を再生成
        /// </summary>
        private void Generate_Tb3_Cmb(int iItem)
        {
            try
            {
                #region omit
                ////自社銀行支払情報コンボボックスの生成(支払区分名で選択候補が分岐)
                // mcBsLogic.Get_OWNBK(out sBKNAMArray, out sBRNAMArray, out sYKNKINDArray, 
                //                    out sKOZANOArray, out sIRAININArray, out sFACNAMArray);

                //if (sBKNAMArray != null)
                //{
                //    //自社銀行支払情報の数だけLOOP(一括支払じゃない場合のリスト)
                //    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                //    list = (
                //        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                //    );
                //    for (int i = 0; i < sBKNAMArray.GetLength(0); i++)
                //    {
                //        //FA名称が設定されているデータは一括専用
                //        if (sFACNAMArray[i].ToString() == "")
                //        {
                //            int iCombo = i;
                //            string sIrainin = sIRAININArray[i];

                //            if (sIrainin == "0000000000")
                //            {
                //                sIrainin = "          ";
                //            }

                //            string sCombo = mcBsLogic.StringCut(string.Format("{0,-20}", sBKNAMArray[i, 1].ToString()), 20) + "："
                //                           + mcBsLogic.StringCut(string.Format("{0,-20}", sBRNAMArray[i, 1].ToString()), 20) + "："
                //                           + sYKNKINDArray[i, 1].ToString().PadRight(3, '　') + "："
                //                           + sKOZANOArray[i].ToString().PadRight(7) + "：" + sIrainin; // sIRAININArray[i];
                //            list.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                //        }
                //    }

                //    //自社銀行支払情報の数だけLOOP(一括支払の場合のリスト)
                //    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>
                //    list2 = (
                //        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, string>>()
                //    );
                //    for (int i = 0; i < sFACNAMArray.Length; i++)
                //    {
                //        if (sFACNAMArray[i].ToString() != "")
                //        {
                //            int iCombo = i;
                //            string sIrainin = sIRAININArray[i];

                //            if (sIrainin == "0000000000")
                //            {
                //                sIrainin = "          ";
                //            }

                //            string sCombo = mcBsLogic.StringCut(string.Format("{0,-59}", sFACNAMArray[i].ToString()), 59) + "：" + sIrainin; // sIRAININArray[i];
                //            list2.Add(new System.Collections.Generic.KeyValuePair<int, string>(iCombo, sCombo));
                //        }                        
                //    }
                #endregion
                //支払区分の変更があった項目の支払情報を生成
                switch (iItem)
                {
                    case 1:
                        string sHARAI_KBN1 = "";
                        if (Tb3_Lbl_HARAI_KBN1.Text != "")
                        {
                            sHARAI_KBN1 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':')));
                        }
                        #region omit
                        //if (sHARAI_KBN1 != "8" && sHARAI_KBN1 != "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN1.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN1.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN1.DataSource = ownBankList;
                        //}
                        //else if (sHARAI_KBN1 == "8")
                        //{
                        //    Tb3_Cmb_HARAI_KBN1.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN1.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN1.DataSource = ownBankFactoringList;
                        //}
                        //else if (sHARAI_KBN1 == "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN1.DataSource = null;
                        //}
                        #endregion
                        if (sHARAI_KBN1 == "")
                        {
                            Tb3_Cmb_HARAI_KBN1.DataSource = null;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN1.DisplayMember = "Value";
                            Tb3_Cmb_HARAI_KBN1.ValueMember = "Key";
                            Tb3_Cmb_HARAI_KBN1.DataSource = GetOwnBankList(sHARAI_KBN1 == "8");
                        }
                        break;
                    case 2:
                        string sHARAI_KBN2 = "";
                        if (Tb3_Lbl_HARAI_KBN2.Text != "")
                        {
                            sHARAI_KBN2 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':')));
                        }
                        #region omit
                        //if (sHARAI_KBN2 != "8" && sHARAI_KBN2 != "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN2.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN2.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN2.DataSource = ownBankList;
                        //}
                        //else if (sHARAI_KBN2 == "8")
                        //{
                        //    Tb3_Cmb_HARAI_KBN2.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN2.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN2.DataSource = ownBankFactoringList;
                        //}
                        //else if (sHARAI_KBN2 == "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN2.DataSource = null;
                        //}
                        #endregion
                        if (sHARAI_KBN2 == "")
                        {
                            Tb3_Cmb_HARAI_KBN2.DataSource = null;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN2.DisplayMember = "Value";
                            Tb3_Cmb_HARAI_KBN2.ValueMember = "Key";
                            Tb3_Cmb_HARAI_KBN2.DataSource = GetOwnBankList(sHARAI_KBN2 == "8");
                        }
                        break;
                    case 3:
                        string sHARAI_KBN3 = "";
                        if (Tb3_Lbl_HARAI_KBN3.Text != "")
                        {
                            sHARAI_KBN3 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':')));
                        }
                        #region omit
                        //if (sHARAI_KBN3 != "8" && sHARAI_KBN3 != "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN3.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN3.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN3.DataSource = ownBankList;
                        //}
                        //else if (sHARAI_KBN3 == "8")
                        //{
                        //    Tb3_Cmb_HARAI_KBN3.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN3.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN3.DataSource = ownBankFactoringList;
                        //}
                        //else if (sHARAI_KBN3 == "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN3.DataSource = null;
                        //}
                        #endregion
                        if (sHARAI_KBN3 == "")
                        {
                            Tb3_Cmb_HARAI_KBN3.DataSource = null;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN3.DisplayMember = "Value";
                            Tb3_Cmb_HARAI_KBN3.ValueMember = "Key";
                            Tb3_Cmb_HARAI_KBN3.DataSource = GetOwnBankList(sHARAI_KBN3 == "8");
                        }
                        break;
                    case 4:
                        string sHARAI_KBN4 = "";
                        if (Tb3_Lbl_HARAI_KBN4.Text != "")
                        {
                            sHARAI_KBN4 = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':')));
                        }
                        #region omit
                        //if (sHARAI_KBN4 != "8" && sHARAI_KBN4 != "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN4.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN4.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN4.DataSource = ownBankList;
                        //}
                        //else if (sHARAI_KBN4 == "8")
                        //{
                        //    Tb3_Cmb_HARAI_KBN4.DisplayMember = "Value";
                        //    Tb3_Cmb_HARAI_KBN4.ValueMember = "Key";
                        //    Tb3_Cmb_HARAI_KBN4.DataSource = ownBankFactoringList;
                        //}
                        //else if (sHARAI_KBN4 == "")
                        //{
                        //    Tb3_Cmb_HARAI_KBN4.DataSource = null;
                        //}
                        #endregion
                        if (sHARAI_KBN4 == "")
                        {
                            Tb3_Cmb_HARAI_KBN4.DataSource = null;
                        }
                        else
                        {
                            Tb3_Cmb_HARAI_KBN4.DisplayMember = "Value";
                            Tb3_Cmb_HARAI_KBN4.ValueMember = "Key";
                            Tb3_Cmb_HARAI_KBN4.DataSource = GetOwnBankList(sHARAI_KBN4 == "8");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nGenerate_Tb3_Cmb　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        #endregion


        #region BindNavi関係
        //Navi1_Prev
        private void BindNavi1_Prev_Click(object sender, EventArgs e)
        {
            try
            {
//--                bEventCancel = true;


                // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                nBindNavi = 1;
                ValidateChildren();
                nTRCDflg = 1; // <---TTA V01.14.02 ADD ◀(8428)
                if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1|| nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, 
//-- <2016/03/24>
//                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                        nBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        //---> V01.14.01 HWPO ADD ▼(8510)
                        if(Global.GAI_F == "1")
                        {
                            Flg_Tsh_Fri = false;
                            GAI_F_Kirikae(0);
                        }
                        Sel_SSTORI();// <--- V02.37.01 YMP ADD ◀122172)
                        //<--- V01.14.01 HWPO ADD ▲(8510)
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                            nBindNavi = 0;
                            return;
                        }
                    }
                }

                //Tb_Main.SelectedIndex = 0;       // <---V01.15.01 HWY DELETE ◀(6490)

                if (Txt_TRCD.ExCodeDB != "" && DataCnt.Text != "0")
                {
                    nTRCD_ChgFlg = 1;
                    //一件前の取引先を表示
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text != "" ? sHJCD = Txt_HJCD.Text : "0");
                    bool bHighDataExist;
                    bool bLowDataExist;
                    mcBsLogic.Sel_SS_TORI_Prev(sTRCD, sHJCD, out bHighDataExist, out bLowDataExist);

                    if (bLowDataExist == true)
                    {
                        BindNavi1_Prev.Enabled = true;
                        BindNavi1_First.Enabled = true;
                    }
                    else
                    {
                        BindNavi1_Prev.Enabled = false;
                        BindNavi1_First.Enabled = false;
                    }

                    if (bHighDataExist == true)
                    {
                        BindNavi1_Next.Enabled = true;
                        BindNavi1_End.Enabled = true;
                    }
                    else
                    {
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_End.Enabled = false;

                    }
                    DataCnt.Text = (Convert.ToInt32(DataCnt.Text.Replace("/", "").Replace(",", "")) - 1).ToString("#,##0");
                    SetDispVal_S();
                    Sel_TabData();

                    //if (Chk_TGASW.Checked == true)
                    //{
                    //    Chk_TGASW.Checked = false;
                    //    Chk_TGASW.Checked = true;
                    //}
                }
                else
                {
                    nTRCD_ChgFlg = 1;
                    //取引先最終行を表示
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    int iCnt;
                    mcBsLogic.Sel_SS_TORI_Last(out iCnt);
                    if (iCnt == 0)
                    {
                        MessageBox.Show(
//-- <2016/03/22>
//                            "該当データは存在しません", Global.sPrgName + "　Ver" + Global.sPrgVer,
                            "該当データがありません。", Global.sPrgName,
//-- <2016/03/22>
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                        nBindNavi = 0;
                        return;
                    }
                    else if (iCnt == 1)
                    {
                        BindNavi1_Prev.Enabled = false;
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_First.Enabled = false;
                        BindNavi1_End.Enabled = false;
                    }
                    else if (iCnt > 1)
                    {
                        BindNavi1_Prev.Enabled = true;
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_First.Enabled = true;
                        BindNavi1_End.Enabled = false;
                    }
                    DataCnt.Text = MaxCnt.Text.Replace("/", "").Replace(",", "");
//--
//                    bEventCancel = true;
//--
                    SetDispVal_S();
                    Sel_TabData();

                    //if (Chk_TGASW.Checked == true)
                    //{
                    //    Chk_TGASW.Checked = false;
                    //    Chk_TGASW.Checked = true;
                    //}
                }

//                bEventCancel = false;


                nTRCDflg = 0;
                nTRCD_ChgFlg = 0;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;

                Txt_TRCD.Focus();
                Txt_TRCD.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi1_Prev_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
            nBindNavi = 0;
        }


        //Navi1_1st
        private void BindNavi1_First_Click(object sender, EventArgs e)
        {
            try
            {
                // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                nBindNavi = 1;
                ValidateChildren();
                nTRCDflg = 1; // <---TTA V01.14.02 ADD ◀(8428)
                if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                        nBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        //---> V01.14.01 HWPO ADD ▼(8510)
                        if (Global.GAI_F == "1")
                        {
                            Flg_Tsh_Fri = false;
                            GAI_F_Kirikae(0);
                        }
                        //<--- V01.14.01 HWPO ADD ▲(8510)
                        Sel_SSTORI();// <--- V02.37.01 YMP ADD ◀122172)
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                            nBindNavi = 0;
                            return;
                        }
                    }
                }

                //Tb_Main.SelectedIndex = 0;                // <---V01.15.01 HWY DELETE ◀(6490)

                nTRCD_ChgFlg = 1;
                //取引先1件目を表示
                string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                int iCnt;
                mcBsLogic.Sel_SS_TORI_1st(out iCnt);
                if (iCnt == 0)
                {
                    MessageBox.Show(
//-- <2016/03/22>
//                        "該当データは存在しません", Global.sPrgName + "　Ver" + Global.sPrgVer,
                        "該当データがありません。", Global.sPrgName,
//-- <2016/03/22>
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                    nBindNavi = 0;
                    return;
                }
                DataCnt.Text = "1";
                SetDispVal_S();
                Sel_TabData();

                //if (Chk_TGASW.Checked == true)
                //{
                //    Chk_TGASW.Checked = false;
                //    Chk_TGASW.Checked = true;
                //}

                if (iCnt == 1)
                {
                    BindNavi1_Prev.Enabled = false;
                    BindNavi1_Next.Enabled = false;
                    BindNavi1_First.Enabled = false;
                    BindNavi1_End.Enabled = false;
                }
                else if (iCnt > 1)
                {
                    BindNavi1_Prev.Enabled = false;
                    BindNavi1_Next.Enabled = true;
                    BindNavi1_First.Enabled = false;
                    BindNavi1_End.Enabled = true;
                }

                nTRCDflg = 0;
                nTRCD_ChgFlg = 0;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;

                Txt_TRCD.Focus();
                Txt_TRCD.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi1_First_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
            nBindNavi = 0;
        }


        //Navi1_Next
        private void BindNavi1_Next_Click(object sender, EventArgs e)
        {
            try
            {
                // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                nBindNavi = 1;
                ValidateChildren();
                nTRCDflg = 1; // <---TTA V01.14.02 ADD ◀(8428)
                if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1|| nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                        nBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        //---> V01.14.01 HWPO ADD ▼(8510)
                        if (Global.GAI_F == "1")
                        {
                            Flg_Tsh_Fri = false;
                            GAI_F_Kirikae(0);
                        }
                        //<--- V01.14.01 HWPO ADD ▲(8510)
                        Sel_SSTORI();// <--- V02.37.01 YMP ADD ◀122172)
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            // V12.03.02 ﾎﾞﾀﾝ押下での移動時、正しく支払銀行が表示されていなかったのを修正しました
                            nBindNavi = 0;
                            return;
                        }
                    }
                }

                //Tb_Main.SelectedIndex = 0;     // <---V01.15.01 HWY DELETE ◀(6490)

                if (Txt_TRCD.ExCodeDB != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                    SetNextSS_TORI(sTRCD, sHJCD);
                }

                #region omit
                //if (Txt_TRCD.ExCodeDB != "")
                //{
                //    nTRCD_ChgFlg = 1;
                //    //一件後の取引先を表示
                //    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                //    string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                //    bool bHighDataExist;
                //    bool bLowDataExist;
                //    mcBsLogic.Sel_SS_TORI_Next(sTRCD, sHJCD, out bHighDataExist, out bLowDataExist);

                //    if (bHighDataExist == true)
                //    {
                //        BindNavi1_Next.Enabled = true;
                //        BindNavi1_End.Enabled = true;
                //    }
                //    else
                //    {
                //        BindNavi1_Next.Enabled = false;
                //        BindNavi1_End.Enabled = false;
                //    }

                //    if (bLowDataExist == true)
                //    {
                //        BindNavi1_Prev.Enabled = true;
                //        BindNavi1_First.Enabled = true;
                //    }
                //    else
                //    {
                //        BindNavi1_Prev.Enabled = false;
                //        BindNavi1_First.Enabled = false;
                //    }
                //    SetDispVal_S();
                //    Sel_TabData();

                //    if (Chk_TGASW.Checked == true)
                //    {
                //        Chk_TGASW.Checked = false;
                //        Chk_TGASW.Checked = true;
                //    }
                //}
                #endregion

                nTRCDflg = 0;
                nTRCD_ChgFlg = 0;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;

                Txt_TRCD.Focus();
                Txt_TRCD.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message, 
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi1_Next_Click　\r\nVer" + Global.sPrgVer, 
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        private bool SetNextSS_TORI(string sTRCD, string sHJCD)
        {
            nTRCD_ChgFlg = 1;
            //一件後の取引先を表示
            bool bHighDataExist;
            bool bLowDataExist;
            bool exists = mcBsLogic.Sel_SS_TORI_Next(sTRCD, sHJCD, out bHighDataExist, out bLowDataExist);

            if (bHighDataExist == true)
            {
                BindNavi1_Next.Enabled = true;
                BindNavi1_End.Enabled = true;
            }
            else
            {
                BindNavi1_Next.Enabled = false;
                BindNavi1_End.Enabled = false;
            }

            if (bLowDataExist == true)
            {
                BindNavi1_Prev.Enabled = true;
                BindNavi1_First.Enabled = true;
            }
            else
            {
                BindNavi1_Prev.Enabled = false;
                BindNavi1_First.Enabled = false;
            }

            if (exists)
            {
                DataCnt.Text = (Convert.ToInt32(DataCnt.Text.Replace("/", "").Replace(",", "")) + 1).ToString("#,##0");
                SetDispVal_S();
                Sel_TabData();
            }

            //if (Chk_TGASW.Checked == true)
            //{
            //    Chk_TGASW.Checked = false;
            //    Chk_TGASW.Checked = true;
            //}

            return exists;
        }


        //Navi1_End
        private void BindNavi1_End_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                nTRCDflg = 1; // <---TTA V01.14.02 ADD ◀(8428)
                if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        //---> V01.14.01 HWPO ADD ▼(8510)
                        if (Global.GAI_F == "1")
                        {
                            Flg_Tsh_Fri = false;
                            GAI_F_Kirikae(0);
                        }
                        //<--- V01.14.01 HWPO ADD ▲(8510)
                        Sel_SSTORI();// <--- V02.37.01 YMP ADD ◀122172)
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                //Tb_Main.SelectedIndex = 0;      // <---V01.15.01 HWY DELETE ◀(6490)

                if (Txt_TRCD.ExCodeDB != "")
                {
                    nTRCD_ChgFlg = 1;
                    //取引先最終行を表示
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    int iCnt;
                    mcBsLogic.Sel_SS_TORI_Last(out iCnt);
                    if (iCnt == 0)
                    {
                        MessageBox.Show(
//-- <2016/03/22>
//                            "該当データは存在しません",
//                            Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            "該当データがありません。",
                            Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Information);
//-- <2016/03/22>
                        return;
                    }
                    DataCnt.Text = MaxCnt.Text.Replace("/", "").Replace(",", "");
                    SetDispVal_S();
                    Sel_TabData();

                    //if (Chk_TGASW.Checked == true)
                    //{
                    //    Chk_TGASW.Checked = false;
                    //    Chk_TGASW.Checked = true;
                    //}

                    if (iCnt == 1)
                    {
                        BindNavi1_Prev.Enabled = false;
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_First.Enabled = false;
                        BindNavi1_End.Enabled = false;
                    }
                    else if (iCnt > 1)
                    {
                        BindNavi1_Prev.Enabled = true;
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_First.Enabled = true;
                        BindNavi1_End.Enabled = false;
                    }
                }
                nTRCDflg = 0;
                nTRCD_ChgFlg = 0;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;

                Txt_TRCD.Focus();
                Txt_TRCD.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi1_End_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        //Navi2_Prev
        private void BindNavi2_Prev_Click(object sender, EventArgs e)
        {
            try
            {
                nTabBindNavi = 1;
                ValidateChildren();
                if (nDispChgFlg_TSHOH == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        nTabBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                        string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                        string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                        if ((Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") || mcBsLogic.Chk_UniqKey(Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == true)
                        {
                            Ins_SSTORI();
                            if (nErrFlg == 1)
                            {
                                nTabBindNavi = 0;
                                return;
                            }
                        }
                        else
                        {
                            string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                            string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                            MessageBox.Show(
                                "既に"
                                + "\nID：" + sSHOID
                                + "\n部門：" + sBMNNM
                                + "\n科目：" + sKMKNM
                                + "\nは登録済です。",
//-- <2016/03/22>
//                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            Tb3_Txt_BCOD.Focus();
                            nTabBindNavi = 0;
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb1_Lbl_SHO_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.Trim(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sSHO_ID = Tb1_Lbl_SHO_ID_V.Text;
                    int iCnt;
                    int iCurrentCnt = int.Parse(BindNavi2_Selected.Text) - 1;
                    if (iCurrentCnt == 0)
                    {
                        iCurrentCnt = 1;
                    }
                    mcBsLogic.Sel_SS_TSHOH_Prev(sTRCD, sHJCD, sSHO_ID, out iCnt);
                    Set_Tb1_SS_TSHOH(iCurrentCnt, iCnt);
                }
                if (BindNavi2_Selected.Text == "1")
                {
                    Tb3_BindNavi_First.Enabled = false;
                    Tb3_BindNavi_Prev.Enabled = false;
                }
                else
                {
                    Tb3_BindNavi_First.Enabled = true;
                    Tb3_BindNavi_Prev.Enabled = true;
                }
                nDispChgFlg_TSHOH = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi2_Prev_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            nTabBindNavi = 0;
        }


        //Navi2_Next
        private void BindNavi2_Next_Click(object sender, EventArgs e)
        {
            try
            {
                nTabBindNavi = 1;
                ValidateChildren();
                if (nDispChgFlg_TSHOH == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        nTabBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                        string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                        string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                        if ((Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") || mcBsLogic.Chk_UniqKey(Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == true)
                        {
                            Ins_SSTORI();
                            if (nErrFlg == 1)
                            {
                                nTabBindNavi = 0;
                                return;
                            }
                        }
                        else
                        {
                            string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                            string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                            MessageBox.Show(
                                "既に"
                                + "\nID：" + sSHOID
                                + "\n部門：" + sBMNNM
                                + "\n科目：" + sKMKNM
                                + "\nは登録済です。",
//-- <2016/03/22>
//                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            Tb3_Txt_BCOD.Focus();
                            nTabBindNavi = 0;
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb1_Lbl_SHO_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sSHO_ID = Tb1_Lbl_SHO_ID_V.Text;
                    int iCnt;
                    int iCurrentCnt = int.Parse(BindNavi2_Selected.Text) + 1;
                    mcBsLogic.Sel_SS_TSHOH_Next(sTRCD, sHJCD, sSHO_ID, out iCnt);
                    Set_Tb1_SS_TSHOH(iCurrentCnt, iCnt);
                }
                nDispChgFlg_TSHOH = 0;

                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi2_Next_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            nTabBindNavi = 0;
        }


        //Navi2_Add
        private void BindNavi2_Add_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_TSHOH == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？",
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        Tb3_Txt_BCOD.Focus();
                    }
                    else if (res == DialogResult.Yes)
                    {
                        string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                        string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                        string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                        if ((Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") || mcBsLogic.Chk_UniqKey(Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == true)
                        {
                            Ins_SSTORI();
                            if (nErrFlg == 1)
                            {
                                return;
                            }
                            Tb3_Txt_BCOD.Focus();
                        }
                        else
                        {
                            string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                            string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                            MessageBox.Show(
                                "既に"
                                + "\nID：" + sSHOID
                                + "\n部門：" + sBMNNM
                                + "\n科目：" + sKMKNM
                                + "\nは登録済です。",
//-- <2016/03/22>
//                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            Tb3_Txt_BCOD.Focus();
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    int iSHO_ID;
                    int iCnt;
                    mcBsLogic.Sel_MaxSHO_ID(sTRCD, sHJCD, out iSHO_ID, out iCnt);
                    Tb3_BindNavi_First.Enabled = true;
                    Tb3_BindNavi_Prev.Enabled = true;
                    Tb3_BindNavi_Next.Enabled = false;
                    Tb3_BindNavi_Last.Enabled = false;
                    Tb3_Chk_SHO_ID.Checked = true;
                    Tb1_Lbl_SHO_ID_V.Text = iSHO_ID.ToString();
                    BindNavi2_Selected.Text = iCnt.ToString();
                    BindNavi2_Cnt.Text = "/ " + iCnt.ToString();
                    Tb3_Lbl_Old_New2.Text = "【　新規　】";
                    Tb3_Txt_BCOD.ExCodeDB = "";
                    Tb3_Txt_BNAM.Text = "";
                    Tb3_Txt_KCOD.ExCodeDB = "";
                    Tb3_Txt_KINM.Text = "";
                    Tb3_Txt_SHINO.Text = "";
                    Tb3_Txt_SHINM.Text = "";
                    Tb3_Cmb_HARAI_H.SelectedIndex = -1;
                    Tb3_Cmb_KIJITU_H.SelectedIndex = -1;
                    Tb3_Txt_SHIMEBI.Text = "";
                    Tb3_Txt_SHIHARAIMM.Text = "";
                    Tb3_Txt_SIHARAIDD.Text = "";
                    Tb3_Txt_SKIJITUMM.Text = "";
                    Tb3_Txt_SKIJITUDD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_V_YAKUJO.ExNumValue = 0;
                    Tb3_Txt_YAKUJOA_L.Text = "";
                    Tb3_Txt_YAKUJOA_M.Text = "";
                    Tb3_Txt_YAKUJOB_LH.Text = "";
                    Tb3_Txt_YAKUJOB_H1.Text = "";
                    Tb3_Txt_YAKUJOB_R1.Text = "";
                    Tb3_Txt_YAKUJOB_U1.Text = "";
                    Tb3_Txt_YAKUJOB_H2.Text = "";
                    Tb3_Txt_YAKUJOB_R2.Text = "";
                    Tb3_Txt_YAKUJOB_U2.Text = "";
                    Tb3_Txt_YAKUJOB_H3.Text = "";
                    Tb3_Txt_YAKUJOB_R3.Text = "";
                    Tb3_Txt_YAKUJOB_U3.Text = "";
                    //タブ3も同期を取る
                    Tb3_Lbl_HARAI_KBN1.Text = "";
                    Tb3_Lbl_HARAI_KBN2.Text = "";
                    Tb3_Lbl_HARAI_KBN3.Text = "";
                    Tb3_Lbl_HARAI_KBN4.Text = "";
                    Tb3_Cmb_HARAI_KBN1.Text = "";
                    Tb3_Cmb_HARAI_KBN2.Text = "";
                    Tb3_Cmb_HARAI_KBN3.Text = "";
                    Tb3_Cmb_HARAI_KBN4.Text = "";

                    Tb3_Cmb_HARAI_KBN1.DataSource = null;
                    Tb3_Cmb_HARAI_KBN2.DataSource = null;
                    Tb3_Cmb_HARAI_KBN3.DataSource = null;
                    Tb3_Cmb_HARAI_KBN4.DataSource = null;

                    //BindNavi
                    Tb3_BindNavi_First.Enabled = true;
                    Tb3_BindNavi_Prev.Enabled = true;
                    Tb3_BindNavi_Next.Enabled = false;
                    Tb3_BindNavi_Last.Enabled = false;
                }
                nDispChgFlg_TSHOH = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }

                //支払条件タブ制御
                this.Chg_SHINO_Control();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi2_Add_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //Navi2_Del
        private void BindNavi2_DEL_Click(object sender, EventArgs e)
        {
            // トランザクション処理
//-- <9999>
//            DbTransaction trn = (Global.cConKaisya).BeginTransaction(IsolationLevel.ReadCommitted);
            DbTransaction trn = (Global.cConSaikenSaimu).BeginTransaction(IsolationLevel.ReadCommitted);
//-- <9999>
            Global.cCmdSel.Transaction = trn;
            Global.cCmdIns.Transaction = trn;
            Global.cCmdDel.Transaction = trn;

            try
            {
                //表示中の取引先支払方法を削除
                if (Tb3_Lbl_Old_New2.Text != "【　新規　】")
                {
                    int count = mcBsLogic.Get_SS_TSHOH_Count(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                    if (count <= 1)
                    {
                        MessageBox.Show("支払条件は削除（０件に）できません。"
                            , Global.sPrgName
                            , MessageBoxButtons.OK
                            , MessageBoxIcon.Information);
                        trn.Rollback();
                        return;
                    }
                    string sBNAM = (Tb3_Txt_BCOD.ExCodeDB != "" ? Tb3_Txt_BNAM.Text : "全て");
                    string sKNAM = (Tb3_Txt_KCOD.ExCodeDB != "" ? Tb3_Txt_KINM.Text : "全て");

                    string hedmsg = "";
                    if (mcBsLogic.Chk_SS_SHDATA(Txt_TRCD.ExCodeDB,
                                                Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0",
                                                Tb3_Txt_BCOD.ExCodeDB,
                                                mcBsLogic.Conv_KCODtoKICD(Tb3_Txt_KCOD.ExCodeDB),
                                                Global.SKBNCOD_tb1) == true)
                    {
                        hedmsg = "支払依頼データがあります。\n";
                    }

                    if (MessageBox.Show(
                        //**>>ICS-S 2013/05/17
                        //**hedmsg + "ID：" + Tb1_Lbl_SHO_ID_V.Text + "\n部門：" + sBNAM + "\n科目：" + sKNAM + "\nを削除しますか。",
                        hedmsg + "ID：" + Tb1_Lbl_SHO_ID_V.Text + "\n部門：" + sBNAM + "\n科目：" + sKNAM + "\nを削除しますか？",
                        //**<<ICS-E
//-- <2016/03/22>
//                    Global.sPrgName, MessageBoxButtons.OKCancel, hedmsg == "" ? MessageBoxIcon.Question : MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                    "削除確認", MessageBoxButtons.OKCancel, hedmsg == "" ? MessageBoxIcon.Question : MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
//-- <2016/03/22>
                    {
                        trn.Rollback();
                        return;
                    }
                    else
                    {
                        //**>>ICS-S 2013/05/17
                        if (MessageBox.Show("削除を実行した場合、\n"
                            + "ID：" + Tb1_Lbl_SHO_ID_V.Text
                            + "\n部門：" + sBNAM
                            + "\n科目：" + sKNAM
                            + "\nの設定がされているデータに矛盾が発生します。"
                            + "\nそれでも削除しますか？"
//-- <2016/03/22>
//                            , Global.sPrgName,
                            , "削除確認",
//-- <2016/03/22>
                            MessageBoxButtons.OKCancel, hedmsg == "" ? MessageBoxIcon.Question : MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                        {
                            trn.Rollback();
                            return;
                        }
                        else
                        {
                            string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                            mcBsLogic.Del_SS_TSHOH(Txt_TRCD.ExCodeDB, sHJCD, Tb1_Lbl_SHO_ID_V.Text);

                            //Global.dNow = IcsSSUtil.IDate.GetDBNow(Global.cConCommon);
//-- <9999>                            
//                            Global.dNow = mcBsLogic.Get_DBTime();
///////////// DBから時間取得しているか確認する必要あり！
                            Global.dNow = mcBsLogic.Get_DBTime(trn);
//-- <9999>
                            //**>>ICS-S 2013/06/12 履歴対応
                            Global.nTSHOH_cnt_OLD = int.Parse(Tb1_Lbl_SHO_ID_V.Text);
                            Set_dtRIREKI(1, int.Parse(Tb1_Lbl_SHO_ID_V.Text), "", 9, null, null, null);
                            mcBsLogic.Insert_SS_RKITORI();
                            Global.dtRIREKI.Clear();
                            //**<<ICS-E

                            Sel_SS_TSHOH(false);// <--- V02.37.01 YMP UPDATE ◀(122172)引数にfalseを渡す
                            //**
                            while (Global.nTSHOH_cnt >= Global.nTSHOH_cnt_OLD)
                            {
                                Global.dtRIREKI.Clear();
                                Set_dtRIREKI(1, Global.nTSHOH_cnt, "SHO_ID", 2, "支払条件ID", (Global.nTSHOH_cnt + 1).ToString(), Global.nTSHOH_cnt.ToString());
                                mcBsLogic.Insert_SS_RKITORI();
                                Global.nTSHOH_cnt--;
                            }
                            //**
                            nDispChgFlg_TSHOH = 0;
                        }


                        //**string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                        //**mcBsLogic.Del_SS_TSHOH(Txt_TRCD.ExCodeDB, sHJCD, Tb1_Lbl_SHO_ID_V.Text);
                        //**
                        //**Sel_SS_TSHOH();
                        //**nDispChgFlg_TSHOH = 0;
                        //**<<ICS-E
                    }
                }
                trn.Commit();
            }
//-- <9999>
//            catch
            catch (Exception ex)
//-- <9999>
            {
//-- <9999 >
                trn.Rollback();
                MessageBox.Show(
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi2_DEL_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <9999>
            }
        }


        //Navi3_Prev
        private void BindNavi3_Prev_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb4_Lbl_GIN_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sGIN_ID = Tb4_Lbl_GIN_ID_V.Text;
                    int iSS_FRIGIN_cnt;
                    int iCnt;
                    int iCurrentCnt = int.Parse(Tb4_BindNavi_Selected.Text) - 1;
                    mcBsLogic.Sel_SS_FRIGIN_Prev(sTRCD, sHJCD, sGIN_ID, out iCnt, out iSS_FRIGIN_cnt);
//-- <2016/02/14>
//--                    bEventCancel = true;
//-- <>
                    Set_Tb2_SS_FRIGIN(iCurrentCnt, iCnt);
//-- <2016/02/14>
//--                    bEventCancel = false;
//-- <>
                }
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi3_Prev_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        //Navi3_Next
        private void BindNavi3_Next_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb4_Lbl_GIN_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sGIN_ID = Tb4_Lbl_GIN_ID_V.Text;
                    int iSS_FRIGIN_cnt;
                    int iCnt;
                    int iCurrentCnt = int.Parse(Tb4_BindNavi_Selected.Text) + 1;
                    mcBsLogic.Sel_SS_FRIGIN_Next(sTRCD, sHJCD, sGIN_ID, out iCnt, out iSS_FRIGIN_cnt);
//-- <2016/02/14>
//--                    bEventCancel = true;
//-- <2016/02/14>
                    Set_Tb2_SS_FRIGIN(iCurrentCnt, iCnt);
//-- <2016/02/14>
//--                    bEventCancel = false;
//-- <>
                }
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi3_Next_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        //Navi3_Add
        private void BindNavi3_Add_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？", 
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                        Tb4_Txt_BANK_CD.Focus();
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                        Tb4_Txt_BANK_CD.Focus();
                    }
                }

                if (Txt_TRCD.ExCodeDB != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    int iGIN_ID;
                    int iCnt;
                    mcBsLogic.Sel_MaxGIN_ID(sTRCD, sHJCD, out iGIN_ID, out iCnt);
                    Tb4_Chk_GIN_ID.Checked = true;
                    Tb4_Lbl_GIN_ID_V.Text = iGIN_ID.ToString();
                    Tb4_BindNavi_Selected.Text = iCnt.ToString();
                    Tb4_BindNavi_Cnt.Text = "/ " + iCnt.ToString();
                    Tb4_BindNavi_First.Enabled = true;
                    Tb4_BindNavi_Prev.Enabled = true;
                    Tb4_BindNavi_Next.Enabled = false;
                    Tb4_BindNavi_End.Enabled = false;
                    Tb4_Lbl_Old_New3.Text = "【　新規　】";
                    Tb4_Txt_BANK_CD.Text = "";
                    Tb4_Txt_BANK_NM.Text = "";
                    Tb4_Txt_SITEN_ID.Text = "";
                    Tb4_Txt_SITEN_NM.Text = "";
                    Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";
                    Tb4_Txt_KOUZA.ClearValue();
                    Tb4_Txt_MEIGI.Text = "";
                    Tb4_Txt_MEIGIK.Text = "";
                    Tb4_Cmb_TESUU.Text = "1:自社負担";
                    Tb4_Cmb_SOUKIN.Text = "7:電信";
                    Tb4_Txt_GENDO.ExNumValue = 0;

                    Tb4_Chk_FDEF.Checked = false;
                    Tb4_Chk_DDEF.Checked = false;
                    Tb4_Cmb_FTESUID.SelectedIndex = -1;
                    Tb2_Chk_DTESUSW.Checked = false;
                }
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi3_Add_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }


        //Navi3_Del
        private void BindNavi3_DEL_Click(object sender, EventArgs e)
        {

            // トランザクション処理
            DbTransaction trn = (Global.cConSaikenSaimu).BeginTransaction(IsolationLevel.ReadCommitted);
            Global.cCmdSel.Transaction = trn;
            Global.cCmdIns.Transaction = trn;
            Global.cCmdDel.Transaction = trn;

            DbTransaction trnZ = (Global.cConKaisya).BeginTransaction(IsolationLevel.ReadCommitted);
            Global.cCmdSelZ.Transaction = trnZ;

            try
            {
//-- <9999>
                Tb4_BindNavi_DEL.Enabled = false;
//-- <9999>
                if (Tb4_Lbl_Old_New3.Text == "【　変更　】")
                {
                    if (Tb4_BindNavi_Cnt.Text.Replace("/", "").Trim() == "1")
                    {
                        if (mcBsLogic.Chk_GinFuriSKBN(Txt_TRCD.ExCodeDB, Txt_HJCD.Text.Trim() == "" ? "0" : Txt_HJCD.Text.Trim(), Tb3_Txt_SHINO.Text, BindNavi2_Selected.Text))
                        {
                            MessageBox.Show("支払条件が存在する為、振込先銀行は削除（０件に）できません。", Global.sPrgName, MessageBoxButtons.OK);
                            trn.Rollback();
                            trnZ.Rollback();
                            return;
                        }
                    }
                    else
                    {
                        if (Global.FDEF == "1")
                        {
                            MessageBox.Show("初期値に設定されているため削除できません。\n\r他の振込先に初期値を変更してから削除してください。", Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            trn.Rollback();
                            trnZ.Rollback();
                            return;
                        }
                    }
                    string hedmsg = "";
                    if (mcBsLogic.Chk_SS_SHDATA(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0", Tb4_Lbl_GIN_ID_V.Text) == true)
                    {
                        hedmsg = "支払依頼データがあります。\n";
                    }

                    //表示中の振銀を削除
                    if (MessageBox.Show(hedmsg 
                        + "ID：" + Tb4_Lbl_GIN_ID_V.Text
                        + "\n銀行：" + Tb4_Txt_BANK_CD.Text + "：" + Tb4_Txt_BANK_NM.Text
                        + "\n支店：" + Tb4_Txt_SITEN_ID.Text + "：" + Tb4_Txt_SITEN_NM.Text
                        //**>>ICS-S 2013/05/17
                        //**+ "\nを削除しますか。"
                        + "\nを削除しますか？"
                        //**<<ICS-E
//-- <2016/03/22>
//                        , Global.sPrgName,
                        , "削除確認",
//-- <2016/03/22>
                        MessageBoxButtons.OKCancel, hedmsg == "" ? MessageBoxIcon.Question : MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                    {
                        trn.Rollback();
                        trnZ.Rollback();
                        return;
                    }
                    else
                    {
                        //**>>
                        //**string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                        //**mcBsLogic.Del_SS_FRIGIN(Txt_TRCD.ExCodeDB, sHJCD, Tb2_Lbl_GIN_ID_V.Text);
                        //**
                        //**Sel_SS_FRIGIN();
                        //**nDispChgFlg_FRIGIN = 0;
                        //**if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                        //**{
                        //**    Btn_REG.Enabled = false;
                        //**}
                        if (MessageBox.Show("削除を実行した場合、\n"
                            + "ID：" + Tb4_Lbl_GIN_ID_V.Text
                            + "\n銀行：" + Tb4_Txt_BANK_CD.Text + "：" + Tb4_Txt_BANK_NM.Text
                            + "\n支店：" + Tb4_Txt_SITEN_ID.Text + "：" + Tb4_Txt_SITEN_NM.Text
                            + "\nの設定がされているデータに矛盾が発生します。"
                            + "\nそれでも削除しますか？"
//-- <2016/03/22>
//                            , Global.sPrgName,
                            , "削除確認",
//-- <2016/03/22>
                            MessageBoxButtons.OKCancel, hedmsg == "" ? MessageBoxIcon.Question : MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                        {
                            trn.Rollback();
                            trnZ.Rollback();
                            return;
                        }
                        else
                        {
                            string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");
                            mcBsLogic.Del_SS_FRIGIN(Txt_TRCD.ExCodeDB, sHJCD, Tb4_Lbl_GIN_ID_V.Text);

                            //Global.dNow = IcsSSUtil.IDate.GetDBNow(Global.cConCommon);
//-- <9999>
//                            Global.dNow = mcBsLogic.Get_DBTime();
                            Global.dNow = mcBsLogic.Get_DBTime(trn);
//-- <9999>

                            //**>>ICS-S 2013/06/12 履歴対応
                            Global.nFRGIN_cnt_OLD = int.Parse(Tb4_Lbl_GIN_ID_V.Text);
                            Set_dtRIREKI(2, int.Parse(Tb4_Lbl_GIN_ID_V.Text), "", 9, null, null, null);
                            mcBsLogic.Insert_SS_RKITORI();
                            Global.dtRIREKI.Clear();
                            //**<<ICS-E

                            Sel_SS_FRIGIN(false);// <--- V02.37.01 YMP UPDATE ◀(122172)引数にfalseを渡す
                            //**
                            while (Global.nFRGIN_cnt >= Global.nFRGIN_cnt_OLD)
                            {
                                Global.dtRIREKI.Clear();
                                Set_dtRIREKI(2, Global.nFRGIN_cnt, "GIN_ID", 2, "振込先銀行ID", (Global.nFRGIN_cnt + 1).ToString(), Global.nFRGIN_cnt.ToString());
                                mcBsLogic.Insert_SS_RKITORI();
                                Global.nFRGIN_cnt--;
                            }
                            //**
                            nDispChgFlg_FRIGIN = 0;
                            if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                            {
                                Btn_REG.Enabled = false;
                                FKB.F10_Enabled = false;
                            }
                        }
                        //**<<

                        trn.Commit();
                        trnZ.Rollback();
                    }
                }
//--
                else
                {
                    trn.Commit();
                    trnZ.Rollback();
                }
//--
            }
            catch (Exception ex)
            {
                trn.Rollback();
                trnZ.Rollback();
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nBindNavi3_DEL_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            finally
            {
                //-- <9999>
                Application.DoEvents(); 
                Tb4_BindNavi_DEL.Enabled = true;
                //-- <9999>
            }
        }


        //Navi4_Prev
        private void BindNavi4_Prev_Click(object sender, EventArgs e)
        {
            nTabBindNavi = 1;
            ValidateChildren();
            if (Txt_TRCD.ExCodeDB != "")
            {
                BindNavi2_Prev_Click(sender, e);
            }
            nTabBindNavi = 0;
        }


        //Navi4_Next
        private void BindNavi4_Next_Click(object sender, EventArgs e)
        {
            nTabBindNavi = 1;
            ValidateChildren();
            if (Txt_TRCD.ExCodeDB != "")
            {
                BindNavi2_Next_Click(sender, e);
            }
            nTabBindNavi = 0;
        }
        #endregion



        #region 終了処理
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSMTORI_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
                {
                    if (!bHenkou)
                    {

                        res = MessageBox.Show(
                            //-- <2016/03/22>
                            //                        "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                            "変更されています。確定しますか？", "保存確認", MessageBoxButtons.YesNoCancel,
                            //-- <2016/03/22>
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                    }
                    else
                    {
                        res = MessageBox.Show(
                            "変更されています。確定しますか？", "保存確認", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    }
                    if (res == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
////-- <2016/03/14 再チェック　不足分があるかも>
//                        if (Chk_TGASW.Checked == true)
//                        {
//                            //手形管理のみ使用フラグON用の入力チェック
//                            Chk_DispVal_TGASW_ON();
//                            if (nErrFlg == 1)
//                            {
//                                e.Cancel = true;
//                                return;
//                            }
//                        }
//                        else
//                        {
////-- <2016/03/22>
////                            Chk_DispVal_TGASW_ON();
///////////////////////                            Chk_DispVal_TGASW_OFF();
////-- <2016/03/22>
////                            if (nErrFlg == 1)
////                            {
////                                e.Cancel = true;
////                                return;
////                            }
//                        }
//                        // いいえを選択されても必須はチェックしてからでないと登録が無いものが作成されてしまう。
////-- <2016/03/14>
                    }
//                    else if (res == DialogResult.Yes)
                    else if (res == DialogResult.Yes || res == DialogResult.OK) 
                    {
                        nErrFlg = 0;
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            nDispChgFlg_Main = 0;
                            nDispChgFlg_TSHOH = 0;
                            nDispChgFlg_FRIGIN = 0;
                            Btn_REG.Enabled = false;
                            FKB.F10_Enabled = false;

                            bHenkou = false;

                        }
                    }
                }

                if (!Global.bZMode && Global.cUsrTbl.nENDSW == 1 && Txt_TRCD.ReadOnlyEx != true)
                {
                    if (MessageBox.Show(
//-- <2016/03/22>
//                        Global.sPrgName + "を終了しますか。", Global.sPrgName, MessageBoxButtons.OKCancel,
                        Global.sPrgName + "を終了しますか。", "処理終了", MessageBoxButtons.OKCancel,
//-- <2016/03/22>
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                #region 旧ｺｰﾄﾞ
                //if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1|| nDispChgFlg_FRIGIN == 1)
                //{
                //    res = MessageBox.Show(
                //        "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                //        MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                //    if (res == DialogResult.Cancel)
                //    {
                //        e.Cancel = true;
                //        return;
                //    }
                //    else if (res == DialogResult.No)
                //    {
                //    }
                //    else if (res == DialogResult.Yes)
                //    {
                //        nErrFlg = 0;
                //        Ins_SSTORI();
                //        if (nErrFlg == 1)
                //        {
                //            e.Cancel = true;
                //            return;
                //        }
                //        else
                //        {
                //            nDispChgFlg_Main = 0;
                //            nDispChgFlg_TSHOH = 0;
                //            nDispChgFlg_FRIGIN = 0;
                //            Btn_REG.Enabled = false;
                //        }
                //    }
                //}

                //if (Global.bZMode)
                //{
                //    if (Global.bZUpdFlg == true)
                //    {
                //        Environment.Exit(1);
                //    }
                //    else
                //    {
                //        Environment.Exit(0);
                //    }
                //}
                //else
                //{
                //    if (Global.cUsrTbl.nENDSW == 1 && Txt_TRCD.ReadOnlyEx != true)
                //    {
                //        if (MessageBox.Show(
                //            Global.sPrgName + "を終了しますか。", Global.sPrgName, MessageBoxButtons.OKCancel,
                //            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                //        {
                //            e.Cancel = true;
                //            return;
                //        }
                //    }

                //    #region
                //    //画面サイズ、ロケーション
                //    //Form form = sender as Form;
                //    //// フォーム情報の保存
                //    //if (form != null)
                //    //{
                //    //    SalBoolean commonDbHandleCreate = false;
                //    //    try
                //    //    {
                //    //        // 共通DBのハンドルが渡されていない場合は内部で作成する
                //    //        if (Var.formSaveCommonSqlHandle == SalSqlHandle.Null)
                //    //        {
                //    //            if (String.IsNullOrEmpty(Sql.User))
                //    //            {
                //    //                Sql.User = Var.formSaveUser.ToString("0000");
                //    //            }
                //    //            DBACCLIBV.Int.CommonDbConnect(ref Var.formSaveCommonSqlHandle);
                //    //            commonDbHandleCreate = true;
                //    //        }

                //    //        if (!Var.formSaveCommonSqlHandle.IsNull)
                //    //        {
                //    //            cOPTION_INF optionInfo = new cOPTION_INF();

                //    //            Var.formWindowState = (Int32)form.WindowState;
                //    //            String strKey1 = form.GetType().Name;
                //    //            // フォームの位置
                //    //            optionInfo.INF_SetNum(strKey1, Global.OptionTextFormLocationX, Var.formSaveKeyNo, Var.formLocationX);
                //    //            optionInfo.INF_SetNum(strKey1, Global.OptionTextFormLocationY, Var.formSaveKeyNo, Var.formLocationY);
                //    //            // フォームの幅
                //    //            optionInfo.INF_SetNum(strKey1, Global.OptionTextFormWidth, Var.formSaveKeyNo, Var.formWidth);
                //    //            // フォームの高さ
                //    //            optionInfo.INF_SetNum(strKey1, Global.OptionTextFormHeight, Var.formSaveKeyNo, Var.formHeight);
                //    //            // フォームの状態
                //    //            optionInfo.INF_SetNum(strKey1, Global.OptionTextWindowsState, Var.formSaveKeyNo, Var.formWindowState);
                //    //            // DB保存
                //    //            if (OPTIONV.Int.op_SetOption1(Var.formSaveCommonSqlHandle, Var.formSaveProgramID, Var.formSaveUser, optionInfo) > 0)
                //    //            {
                //    //                Var.formSaveCommonSqlHandle.Commit();
                //    //            }
                //    //            else
                //    //            {
                //    //                Var.formSaveCommonSqlHandle.PrepareAndExecute("ROLLBACK");
                //    //            }
                //    //        }
                //    //    }
                //    //    finally
                //    //    {
                //    //        // 内部でSalSqlHandleを生成した場合はDisconnectを行う
                //    //        if (commonDbHandleCreate)
                //    //        {
                //    //            Var.formSaveCommonSqlHandle.Disconnect();
                //    //        }
                //    //    }
                //    //}
                //    #endregion

                //    if (Global.sTRNAM_R != "" && nIchiUpdFlg == 1)
                //    {
                //        nInsertFlg = 0;
                //        Environment.Exit(100);
                //    }
                //    else
                //    {
                //        Environment.Exit(0);
                //    }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nFormClosing　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        private void frmSMTORI_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (Global.bZMode)
                {
                    if (Global.bZUpdFlg == true)
                    {
                        //Environment.Exit(1);
                        Environment.ExitCode = 1;
                    }
                    else
                    {
                        //Environment.Exit(0);
                        Environment.ExitCode = 0;
                    }
                }
                else
                {
                    if (Global.sTRNAM_R != "" && nIchiUpdFlg == 1)
                    {
                        nInsertFlg = 0;
                        //Environment.Exit(100);
                        Environment.ExitCode = 100;
                    }
                    else
                    {
                        //Environment.Exit(0);
                        Environment.ExitCode = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nFormClosed　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            
        }
        #endregion


        #region F9制御
        private void Txt_TRCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            FKB.F08_Enabled = true;
            MNU_SEARCH.Enabled = true;
            MNU_Z_SACH.Enabled = true;

            // SIAS_4228 差分 -->
            if (Txt_TRCD.ExCodeDB != '0'.ToString().PadRight(Global.nTRCD_Len, '0') | Global.nTRCD_Type != 0)
            {
                Bk_TRCD_Pr.Text = Txt_TRCD.ExCodeDB;
            }
            //Bk_TRCD_Pr.Text = Txt_TRCD.ExCodeDB;
            // SIAS_4228 差分 <--
        }

        private void Txt_TRCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            FKB.F08_Enabled = false;
            MNU_SEARCH.Enabled = false;
            MNU_Z_SACH.Enabled = false;

            //Chg_DispControl();
        }

        private void Txt_HJCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;

            Bk_HJCD_Pr.Text = Txt_HJCD.Text;
        }

        private void Txt_HJCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;

            Chg_DispControl();
        }

        private void Txt_STAN_CD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Txt_STAN_CD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Txt_SBCOD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Txt_SBCOD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Txt_SKICD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Txt_SKCOD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb1_Txt_BCOD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb1_Txt_BCOD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb1_Txt_KICD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb1_Txt_KCOD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_BANK_CD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
            //sBank_Before = Tb2_Txt_BANK_CD.Text;
        }

        private void Tb2_Txt_BANK_CD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_SITEN_ID_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_SITEN_ID_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb1_Txt_E_TANTOCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb1_Txt_E_TANTOCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Txt_GRPID_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Txt_GRPID_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }
        #endregion


        #region 画面制御
        /// <summary>
        /// 画面ｺﾝﾄﾛｰﾙの切替(取引先CD、部門CD、科目CD)
        /// </summary>
        private void Set_DispControl()
        {
            Txt_TRCD.MaxLength = Global.nTRCD_Len;
            //Lbl_Haifun.Enabled = false;
            //Txt_HJCD.ReadOnlyEx = true;
            //Lbl_SBCOD.Enabled = (Global.nBCOD_F == 0 ? false : Global.bEnabledState);
            //Txt_SBCOD.ReadOnlyEx = (Global.nBCOD_F == 0 ? true : Global.bReadOnlyState);
            Tb5_Txt_SBCOD.MaxLength = Global.nBCOD_Len;
            //Lbl_STAN.Enabled = (Global.nKMAN == 0 ? false : Global.bEnabledState);
            //Txt_STAN_CD.ReadOnlyEx = (Global.nKMAN == 0 ? true : Global.bReadOnlyState);
            Tb5_Txt_SKCOD.MaxLength = Global.nKCOD_Len;
            Tb3_Txt_KCOD.MaxLength = Global.nKCOD_Len;
            Tb3_Txt_BCOD.MaxLength = Global.nBCOD_Len;
            //Chk_TGASW.Enabled = (Global.nKANRI_F == 1 ? Global.bEnabledState : false);

            Tb1_Txt_E_TANTOCD.ExDatasCode.Code.CodeLength = Global.nETAN_Len;
            if (Global.nETAN_Type == 1)
            {
                Tb1_Txt_E_TANTOCD.ExDatasCode.Code.CodeType = eCodeType.Suuji;
            }
            else
            {
                Tb1_Txt_E_TANTOCD.ExDatasCode.Code.CodeType = eCodeType.Eisuu;
            }
        }


        /// <summary>
        /// エラー項目がある場合のタブ変更拒否
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb_Main_SelectedIndexChanged(object sender, EventArgs e)
        {
            //--->V01.12.01 ATT ADD ▼ (7774)(8084)
            if (Tb_Main.SelectedIndex == 1)
            {
                if (Global.nKanri_IDATA == 0)
                {
                    Tb2_Lbl_SEN_GINKOCD.Enabled = false;
                    Tb2_Txt_SEN_GINKOCD.Enabled = false;
                    Tb2_Txt_SEN_GINKONM.Enabled = false;
                    Tb2_Lbl_SEN_SITENCD.Enabled = false;
                    Tb2_Txt_SEN_SITENCD.Enabled = false;
                    Tb2_Txt_SEN_SITENNM.Enabled = false;
                    Tb2_Lbl_SEN_KSITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENNM.Enabled = false;
                    Tb2_Lbl_SEN_YOKINSYU.Enabled = false;
                    Tb2_Cmb_YOKINSYU.Enabled = false;
                    Tb2_Lbl_SEN_KOZANO.Enabled = false;
                    Tb2_Txt_SEN_KOZANO.Enabled = false;
                }
                else if(Tb2_Txt_SEN_GINKOCD.Text == "")
                {
                    Tb2_Lbl_SEN_GINKOCD.Enabled = true;
                    Tb2_Txt_SEN_GINKOCD.Enabled = true;
                    Tb2_Txt_SEN_GINKONM.Enabled = true;
                    Tb2_Lbl_SEN_SITENCD.Enabled = false;
                    Tb2_Txt_SEN_SITENCD.Enabled = false;
                    Tb2_Txt_SEN_SITENNM.Enabled = false;
                    Tb2_Lbl_SEN_KSITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENCD.Enabled = false;
                    Tb2_Txt_SEN_KSITENNM.Enabled = false;
                    Tb2_Lbl_SEN_YOKINSYU.Enabled = false;
                    Tb2_Cmb_YOKINSYU.Enabled = false;
                    Tb2_Lbl_SEN_KOZANO.Enabled = false;
                    Tb2_Txt_SEN_KOZANO.Enabled = false;
                }
                else
                {
                    Tb2_Lbl_SEN_GINKOCD.Enabled = true;
                    Tb2_Txt_SEN_GINKOCD.Enabled = true;
                    Tb2_Txt_SEN_GINKONM.Enabled = true;
                    Tb2_Lbl_SEN_SITENCD.Enabled = true;
                    Tb2_Txt_SEN_SITENCD.Enabled = true;
                    Tb2_Txt_SEN_SITENNM.Enabled = true;
                    Tb2_Lbl_SEN_KSITENCD.Enabled = true;
                    Tb2_Txt_SEN_KSITENCD.Enabled = true;
                    Tb2_Txt_SEN_KSITENNM.Enabled = true;
                    Tb2_Lbl_SEN_YOKINSYU.Enabled = true;
                    //--->V02.19.01 SMN UPDATE ▼ (103696)
                    //Tb2_Cmb_YOKINSYU.Enabled = true;
                    Tb2_Cmb_YOKINSYU.Enabled = !Global.bReadOnlyState;
                    //<---V02.19.01 SMN UPDATE ▲ (103696)
                    Tb2_Lbl_SEN_KOZANO.Enabled = true;
                    Tb2_Txt_SEN_KOZANO.Enabled = true;
                }
                if (Tb2_Cmb_KAISYU.SelectedIndex.ToString() == "-1")
                {
                    return;
                }
                else if (mcBsLogic.Get_NKUBN(Tb2_Cmb_KAISYU.SelectedValue.ToString(), 1) == "1")
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = true;
                    Tb2_Lbl_SIGHT_M.Enabled = true;
                    Tb2_Lbl_SIGHT_D.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = true;
                }
                else
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = false;
                    Tb2_Lbl_SIGHT_M.Enabled = false;
                    Tb2_Lbl_SIGHT_D.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_M.ClearValue();
                    Tb2_Txt_KAISYUSIGHT_D.ClearValue();
                }
            }
            //<---V01.12.01 ATT ADD ▲ (7774)(8084)
            if (Tb_Main.SelectedIndex == 2)
            {
                if (Tb3_Txt_SIHARAIDD.Text == "99")
                {
                    Tb3_Txt_SIHARAIDD.Text = "末";
                }
                if (Tb3_Txt_SKIJITUDD.Text == "99")
                {
                    Tb3_Txt_SKIJITUDD.Text = "末";
                }
            }

            if (Tb3_Txt_BCOD.IsError == true)
            {
//-- <2016/03/08 タブのIDが間違っている>
//                Tb_Main.SelectedIndex = 0;
                Tb_Main.SelectedIndex = 2;
//-- <2016/03/08>
                Tb3_Txt_BCOD.Focus();
                return;
            }
            else if (Tb3_Txt_KCOD.IsError == true)
            {
//-- <2016/03/08 タブのIDが間違っている>
//                Tb_Main.SelectedIndex = 0;
                Tb_Main.SelectedIndex = 2;
//-- <2016/03/08>
                Tb3_Txt_KCOD.Focus();
                return;
            }
            else if (Tb3_Txt_SHINO.IsError == true)
            {
                Tb_Main.SelectedIndex = 2;
                Tb3_Txt_SHINO.Focus();
                return;
            }
            else if (Tb4_Txt_BANK_CD.IsError == true)
            {
                Tb_Main.SelectedIndex = 3;
                Tb4_Txt_BANK_CD.Focus();
                return;
            }
            else if (Tb4_Txt_SITEN_ID.IsError == true)
            {
//-- <2016/03/08 タブのIDが違っている>
//                Tb_Main.SelectedIndex = 1;
                Tb_Main.SelectedIndex = 3;
//-- <2016/03/08>
                Tb4_Txt_SITEN_ID.Focus();
                return;
            }
//-- <2016/03/08 追加>
            else if (Tb5_Txt_STAN_CD.IsError == true)
            {
                Tb_Main.SelectedIndex = 4;
                Tb5_Txt_STAN_CD.Focus();
                return;
            }
            else if (Tb5_Txt_SBCOD.IsError == true)
            {
                Tb_Main.SelectedIndex = 4;
                Tb5_Txt_SBCOD.Focus();
                return;
            }
            else if (Tb5_Txt_SKCOD.IsError == true)
            {
                Tb_Main.SelectedIndex = 4;
                Tb5_Txt_SKCOD.Focus();
                return;
            }
//-- <2016/03/08>
        }
        #endregion


        #region 入力可能文字列の制御
        /// <summary>
        /// 取引先コード:コードタイプが0の場合、数字以外入力不可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TRCD_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ---> V02.26.01 KSM DELETE ▼(No.113951)
            //if (Global.nTRCD_Type == 0)
            //{
            //    if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
            //    {
            //        e.Handled = true;
            //    }
            //}
            //else
            //{
            //    if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '-')
            //    {
            //        e.Handled = true;
            //    }
            //}
            // <--- V02.26.01 KSM DELETE ▲(No.113951)
            //---> V01.14.01 HWPO ADD ▼(8510)
            if(nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
            {
                RemoveTRCDEventHandler();
            }
            //<--- V01.14.01 HWPO ADD ▲(8510)
        }


        /// <summary>
        /// 郵便番号：数字以外の入力不可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_ZIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }


        /// <summary>
        /// TEL：数字・-・(・)以外の入力不可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TEL_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '(' && e.KeyChar != ')')
            {
                e.Handled = true;
            }
        }


        /// <summary>
        /// FAX：数字・-・(・)以外の入力不可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_FAX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '(' && e.KeyChar != ')')
            {
                e.Handled = true;
            }
        }


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 部門コード:コードタイプが0の場合、数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Txt_SBCOD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (Global.nBCOD_Type == 0)
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    else
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '/')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 科目コード:コードタイプが0の場合、数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Txt_SKICD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (Global.nKCOD_Type == 0)
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    else
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '/')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 部門コード:コードタイプが0の場合、数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Tb1_Txt_BCOD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (Global.nBCOD_Type == 0)
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    else
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '/')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 科目コード:コードタイプが0の場合、数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Tb1_Txt_KICD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (Global.nKCOD_Type == 0)
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    else
        //    {
        //        if ((e.KeyChar < '0' || e.KeyChar > 'z') && e.KeyChar != '\b' && e.KeyChar != '-' && e.KeyChar != '/')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 支払方法：数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Tb1_Txt_SHINO_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //    {
        //        e.Handled = true;
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 銀行コード：数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Tb2_Txt_BANK_CD_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //    {
        //        e.Handled = true;
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)


        // ---> V02.26.01 KSM DELETE ▼(No.113951)
        ///// <summary>
        ///// 支店コード：数字以外入力不可
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Tb2_Txt_SITEN_ID_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
        //    {
        //        e.Handled = true;
        //    }
        //}
        // <--- V02.26.01 KSM DELETE ▲(No.113951)
        #endregion


        #region 変更チェック
        //取引先略称
        private void Txt_RYAKU_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //取引先名称
        private void Txt_TORI_NAM_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //50音
        private void Txt_KNLD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //郵便番号
        private void Txt_ZIP_TextChanged(object sender, EventArgs e)
        {
            if (Tb1_Txt_ZIP.Text.Length == 7 || Tb1_Txt_ZIP.Text.Length == 8)
            {
                if (sZIP_Before != Tb1_Txt_ZIP.Text && sZIP_Before != Tb1_Txt_ZIP.Text.Remove(3,1))
                {
                    nDispChgFlg_Main = 1;
                    Btn_REG.Enabled = true;
                    FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
                }
            }
            else
            {
                nDispChgFlg_Main = 1;
                Btn_REG.Enabled = true;
                FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
            }
        }

        //住所1
        private void Txt_ADDR1_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //住所2
        private void Txt_ADDR2_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //部署
        private void Txt_SBUSYO_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //担当者
        private void Txt_STANTO_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //敬称
        private void Cmb_KEICD_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //TEL
        private void Txt_TEL_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //FAX
        private void Txt_FAX_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //初期値関連.主担当者コード
        private void Txt_STAN_CD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //初期値関連.部門コード
        private void Txt_SBCOD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //初期値関連.科目コード
        private void Txt_SKCOD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //名寄せ
        private void Chk_NAYOSE_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //節印
        private void Chk_F_SETUIN_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //取引停止
        private void Chk_STFLG_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //支払条件タブ.発生部門
        private void Tb1_Txt_BCOD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //支払条件タブ.科目
        private void Tb1_Txt_KCOD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //支払条件タブ.支払方法
        private void Tb1_Txt_SHINO_TextChanged(object sender, EventArgs e)
        {
            nShinoChgFlg = 1;
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //支払条件タブ.支払日休日補正
        private void Tb1_Cmb_HARAI_H_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //支払条件タブ.支払期日休日補正
        private void Tb1_Cmb_KIJITU_H_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //振込先情報タブ.銀行コード
        private void Tb2_Txt_BANK_CD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Tb4_Txt_BANK_NM.Text = "";
            Tb4_Txt_SITEN_ID.Text = "";
            Tb4_Txt_SITEN_NM.Text = "";
            Tb4_Txt_SITEN_ID.ReadOnlyEx = true;

            Tb4_Cmb_FTESUID.DataSource = null;

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.支店コード
        private void Tb2_Txt_SITEN_ID_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Tb4_Txt_SITEN_NM.Text = "";

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.預金種別
        private void Tb2_Cmb_YOKIN_TYP_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.口座番号
        private void Tb2_Txt_KOUZA_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.名義人名
        private void Tb2_Txt_MEIGI_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.名義人カナ
        private void Tb2_Txt_MEIGIK_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.手数料負担変更
        private void Tb2_Cmb_TESUU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Tb4_Txt_GENDO.Enabled = (Tb4_Cmb_TESUU.Text == "1:自社負担" ? true : false);
            Tb4_Txt_GENDO.ExNumValue = (Tb4_Cmb_TESUU.Text == "1:自社負担" ? Tb4_Txt_GENDO.ExNumValue : 0);

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.送金区分
        private void Tb2_Cmb_SOUKIN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //振込先情報タブ.負担限度額
        private void Tb2_Txt_GENDO_TextChanged(object sender, EventArgs e)
        {
            if (!Tb4_Txt_GENDO.IsEdited) { return; }     // <---V01.15.01 HWY ADD ◀(6490)
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb4_Txt_BANK_CD.Text == "" && Tb4_Txt_SITEN_ID.Text == "" && Tb4_Txt_KOUZA.Text == "" && Tb4_Txt_MEIGI.Text == "" && Tb4_Txt_MEIGIK.Text == "")
            {
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
        }

        //依頼先情報タブ.支払区分1
        private void Tb3_Cmb_HARAI_KBN1_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //依頼先情報タブ.支払区分2
        private void Tb3_Cmb_HARAI_KBN2_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //依頼先情報タブ.支払区分3
        private void Tb3_Cmb_HARAI_KBN3_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //依頼先情報タブ.支払区分4
        private void Tb3_Cmb_HARAI_KBN4_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_TSHOH = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.送付案内
        private void Tb5_Cmb_F_SOUFU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.案内文
        private void Tb5_Cmb_ANNAI_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.送料負担区分
        private void Tb5_Cmb_TSOKBN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.端数処理
        private void Tb5_Cmb_SZEI_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.支払通知発行区分
        private void Tb5_Cmb_SHITU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.補助コード1
        private void Tb5_Txt_DM1_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.補助コード2
        private void Tb5_Txt_DM2_TextChanged(object sender, EventArgs e)
        {
            if (!Tb5_Txt_DM2.IsEdited) { return; }   // <---V01.15.01 HWY ADD ◀(6490)	
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //その他情報タブ.補助コード3
        private void Tb5_Txt_DM3_TextChanged(object sender, EventArgs e)
        {
            if (!Tb5_Txt_DM3.IsEdited) { return; }   // <---V01.15.01 HWY ADD ◀(6490)	
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //控除情報タブ.源泉計算チェック切り替え
        private void Tb5_Chk_GENSEN_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;

            if (Tb5_Chk_GENSEN.Checked == true)
            {
                //源泉税関連項目enabled=true
                Tb5_Lbl_GSSKBN.Enabled = true;
                Tb5_Rdo_GSSKBN1.Enabled = Global.bEnabledState;
                Tb5_Rdo_GSSKBN2.Enabled = Global.bEnabledState;
                Tb5_Radio_GENSEN1.Enabled = Global.bEnabledState;
                Tb5_Radio_GENSEN2.Enabled = Global.bEnabledState;
                Tb4_Lbl_GSKUBN.Enabled = true;
                Tb5_Cmb_GSKUBN.Enabled = Global.bEnabledState;
                //支払保留使用NG
                Tb4_Lbl_HOVAL.Enabled = false;
                Tb5_Txt_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL_TANI.Enabled = false;
                //源泉区分に初期値を設定
                if (Tb5_Cmb_GSKUBN.Items.Count != 0)
                {
                    Tb5_Cmb_GSKUBN.SelectedIndex = 0;
                }
                else
                {
                    Tb5_Cmb_GSKUBN.Enabled = false;
                }
            }
            else
            {
                //源泉税関連項目enabled=false
                Tb5_Lbl_GSSKBN.Enabled = false;
                Tb5_Rdo_GSSKBN1.Enabled = false;
                Tb5_Rdo_GSSKBN2.Enabled = false;
                Tb5_Radio_GENSEN1.Enabled = false;
                Tb5_Radio_GENSEN2.Enabled = false;
                Tb4_Lbl_GSKUBN.Enabled = false;
                Tb5_Cmb_GSKUBN.Enabled = false;
                //源泉区分の選択を消去
                Tb5_Cmb_GSKUBN.SelectedIndex = -1;
                Tb5_Rdo_GSSKBN1.Checked = true;
                Tb5_Radio_GENSEN1.Checked = true;
            }
        }

        private void Tb5_Chk_OUTPUT_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;

            if (Tb5_Chk_OUTPUT.Checked == true)
            {
                //名寄せ・節印殺し
                Tb5_Chk_F_SETUIN.Enabled = false;
                Tb5_Chk_F_SETUIN.Checked = false;
                //源泉税関連項目enabled=true
                Tb5_Chk_GENSEN.Enabled = true;
                Tb5_Lbl_GOU.Enabled = true;
                Tb5_Cmb_GOU.Enabled = Global.bEnabledState;
                Tb5_Lbl_GGKBN.Enabled = true;
                Tb5_Cmb_GGKBN.Enabled = Global.bEnabledState;
                Tb5_Cmb_GSKUBN.SelectedIndex = -1;
                //支払保留使用NG
                Tb4_Lbl_HOVAL.Enabled = false;
                Tb5_Txt_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL_TANI.Enabled = false;
                Tb5_Rdo_HORYU0.Checked = true;
                Tb5_Rdo_HORYU0.Enabled = false;
                Tb5_Rdo_HORYU1.Enabled = false;
                Tb5_Rdo_HORYU2.Enabled = false;
            }
            else
            {
                //名寄せ・節印復活(但しチェックはfalseのまま)
                Tb5_Chk_NAYOSE.Enabled = Global.bEnabledState;
                Tb5_Chk_F_SETUIN.Enabled = Global.bEnabledState;
                if (!Tb3_Rdo_GAI_F0.Checked)
                {
                    Tb5_Chk_NAYOSE.Enabled = false;
                }
                //源泉税関連項目enabled=false
                Tb5_Chk_GENSEN.Enabled = false;
                Tb5_Chk_GENSEN.Checked = false;
                Tb5_Lbl_GOU.Enabled = false;
                Tb5_Cmb_GOU.Enabled = false;
                Tb5_Cmb_GOU.SelectedIndex = -1;
                Tb5_Lbl_GGKBN.Enabled = false;
                Tb5_Cmb_GGKBN.Enabled = false;
                Tb5_Cmb_GGKBN.SelectedIndex = -1;

                //支払保留使用NG解除
                Tb5_Rdo_HORYU0.Enabled = true;
                Tb5_Rdo_HORYU1.Enabled = true;
                Tb5_Rdo_HORYU2.Enabled = true;
            }
        }

        //控除情報タブ.源泉税計算1
        private void Tb5_Radio_GENSEN1_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //控除情報タブ.源泉税計算2
        private void Tb5_Radio_GENSEN2_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Generate_Tb4_Cmb_GOU();
        }

        //控除情報タブ.号
        private void Tb5_Cmb_GOU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Tb5_Cmb_GOU.SelectedValueChanged += Tb4_Cmb_GOU_TextChanged;

        }

        //控除情報タブ.源泉区分
        private void Tb5_Cmb_GGKBN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //控除情報タブ.支払区分
        private void Tb5_Cmb_GSKUBN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //控除情報タブ.支払保留
        private void Tb5_Chk_HORYU_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            //if (Tb4_Chk_HORYU.Checked == true)
            //{
            //    Tb4_Chk_GENSEN.Checked = false;
            //    Tb4_Chk_GENSEN.Enabled = false;
            //    Tb4_Radio_GENSEN1.Enabled = false;
            //    Tb4_Radio_GENSEN2.Enabled = false;
            //    Tb4_Lbl_GOU.Enabled = false;
            //    Tb4_Cmb_GOU.Enabled = false;
            //    Tb4_Lbl_GGKBN.Enabled = false;
            //    Tb4_Cmb_GGKBN.Enabled = false;
            //    Tb4_Lbl_GSKUBN.Enabled = false;
            //    Tb4_Cmb_GSKUBN.Enabled = false;
            //    Tb4_Lbl_HOVAL.Enabled = true;
            //    Tb4_Txt_HOVAL.ReadOnlyEx = false;
            //    Tb4_Lbl_HOVAL_TANI.Enabled = true;
            //}
            //else
            //{
            //    Tb5_Chk_GENSEN.Enabled = Global.bEnabledState;
            //    Tb4_Lbl_HOVAL.Enabled = false;
            //    Tb5_Txt_HOVAL.ReadOnlyEx = true;
            //    Tb4_Lbl_HOVAL_TANI.Enabled = false;
            //}
            Generate_Tb5_Cmb_HORYU();
        }

        //控除情報タブ.支払率
        private void Tb4_Txt_HOVAL_TextChanged(object sender, EventArgs e)
        {
            if (nHOVAL_Before != Tb5_Txt_HOVAL.ExNumValue)
            {
                nDispChgFlg_Main = 1;
                Btn_REG.Enabled = true;
                FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
            }
        }

        //控除情報タブ.協力会費計算有無
        private void Tb4_Chk_KYKAI_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            Chg_DispControl();
        }

        //開始年月日
        private void Txt_STYMD_ValueEdited(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //終了年月日
        private void Txt_EDYMD_ValueEdited(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }
        #endregion


        #region 取引先CD～50音の5項目がすべて入力されたかチェック用
        /// <summary>
        /// 取引先略称
        /// 正式名称が空白ならコピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_RYAKU_Leave(object sender, EventArgs e)
        {
            if (Txt_TORI_NAM.Text == "" && Txt_RYAKU.Text != "")
            {
                Txt_TORI_NAM.Text = Txt_RYAKU.Text;
            }

            Chg_DispControl();
        }

        /// <summary>
        /// 取引先名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TORI_NAM_Leave(object sender, EventArgs e)
        {
            Chg_DispControl();
        }

        /// <summary>
        /// 50音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_KNLD_Leave(object sender, EventArgs e)
        {
            // 不具合表-0040によりコメントとする Start
            ////サブ項目有効化判断前に入力チェックさせたいので
            ////ここに臨時追加(可能なら後で整理)
            //if (Txt_KNLD.Text != "")
            //{
            //    Regex regex = new Regex("^[ｱｰ-ﾞﾟｧｨｩｪｫｯｬｭｮ]+$");
            //    if (!regex.IsMatch(Txt_KNLD.Text))
            //    {
            //        Txt_KNLD.Focus();
            //        Txt_KNLD.IsError = true;
            //    }
            //}
            // 不具合表-0040によりコメントとする End

            Chg_DispControl();
        }
        #endregion


        #region validationメソッド
        //取引先CD
        private void Txt_TRCD_Validating(object sender, CancelEventArgs e)
        {
            SetTRCDText(Txt_TRCD.ExCodeDB);

            // SIAS_4228 差分 -->
            if (Global.nTRCD_Type == 0)
            {
                if (Txt_TRCD.ExCodeDB == '0'.ToString().PadRight(Global.nTRCD_Len, '0'))
                {
                    Txt_TRCD.Focus();
                    Txt_TRCD.IsError = true;
                    e.Cancel = true;
                    return;
                }
            }
            // SIAS_4228 差分 <--
            
            // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
            if(Global.cUsrSec.nMFLG < 2)
            {
                if(Txt_TRCD.ExCodeDB != "" && Global.nTRCD_HJ == 0)
                {
                    if (!mcBsLogic.SS_TORI_Exist(Txt_TRCD.ExCodeDB, 000000))
                    {
                        Txt_TRCD.Focus();
                        return;
                    }
                }
            }

            if (Txt_TRCD.ExCodeDB == Bk_TRCD_Pr.Text)
            {
                return;
            }

            // Ver.00.01.09 [SS_1312]対応 -->
            //if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
            // ---> V02.28.01 KKL UPDATE ▼(No.115107)
            //if (bN == false && (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1))
            if (bN == false && (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1) && !fKeyClick)
            // <--- V02.28.01 KKL UPDATE ▲(No.115107)
            // Ver.00.01.09 <--
            {
                res = MessageBox.Show(
                    "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (res == DialogResult.Cancel)
                {
                    //if (Global.nTRCD_Type == 0)
                    //{
                    //    Txt_TRCD.ExCodeDB = Txt_TRCD.ExCodeDB.PadLeft(Global.nTRCD_Len, '0');
                    //}
                    SetTRCDText(Bk_TRCD_Pr.Text);
                    Txt_TRCD.Focus();
                    return;
                }
                else if (res == DialogResult.No)
                {
                    //---> V01.14.01 HWPO ADD ▼(8510)
                    if (Global.GAI_F == "1")
                    {
                        Flg_Tsh_Fri = false;
                        GAI_F_Kirikae(0);
                    }
                    //<--- V01.14.01 HWPO ADD ▲(8512)
                    nTRCDflg = 1;
                    Sel_SSTORI();
                }
                else if (res == DialogResult.Yes)
                {
                    Bk_TRCD_Nx.Text = Txt_TRCD.ExCodeDB;
                    //Txt_TRCD.ExCodeDB = Bk_TRCD_Pr.Text;
                    SetTRCDText(Bk_TRCD_Pr.Text);
                    Ins_SSTORI();
                    if (nErrFlg == 1)
                    {
                        return;
                    }
                    Txt_TRCD.ExCodeDB = Bk_TRCD_Nx.Text;
                    Bk_TRCD_Nx.Text = "";
                    Sel_SSTORI();
                }
            }
            else
            {
                Sel_SSTORI(sender);
            }
        }

        //取引先補助CD
        private void Txt_HJCD_Validating(object sender, CancelEventArgs e)
        {
            // セキュリティ対応(ﾏｽﾀ権限：参照以下の場合、項目の編集不可)
            if (Global.cUsrSec.nMFLG < 2)
            {
                if (Txt_HJCD.Text != "" && Global.nTRCD_HJ == 1)
                {
                    if (!mcBsLogic.SS_TORI_Exist(Txt_TRCD.ExCodeDB, int.Parse(Txt_HJCD.Text)))
                    {
                        Txt_HJCD.Focus();
                        return;
                    }
                }
            }

            if (Txt_HJCD.Text == Bk_HJCD_Pr.Text)
            {
                return;
            }

            // ---> V02.28.01 KKL UPDATE ▼(No.115107)
            //if (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1)
            if ((nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1) && !fKeyClick)
            // <--- V02.28.01 KKL UPDATE ▲(No.115107)
            {
                res = MessageBox.Show(
                    "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (res == DialogResult.Cancel)
                {
                    Txt_HJCD.Text = Bk_HJCD_Pr.Text.PadLeft(6, '0');
                    Txt_HJCD.Focus();
                    return;
                }
                else if (res == DialogResult.No)
                {
                    //---> V01.14.01 HWPO ADD ▼(8510)
                    if (Global.GAI_F == "1")
                    {
                        Flg_Tsh_Fri = false;
                        GAI_F_Kirikae(0);
                    }
                    //<--- V01.14.01 HWPO ADD ▲(8510)
                    nTRCDflg = 1;
                    Sel_SSTORI();
                }
                else if (res == DialogResult.Yes)
                {
                    Bk_HJCD_Nx.Text = Txt_HJCD.Text;
                    Txt_HJCD.Text = Bk_HJCD_Pr.Text;
                    Ins_SSTORI();
                    if (nErrFlg == 1)
                    {
                        return;
                    }
                    Txt_HJCD.Text = Bk_HJCD_Nx.Text;
                    Bk_HJCD_Nx.Text = "";
                    Sel_SSTORI();
                }
            }
            else
            {
                Sel_SSTORI(sender);
            }
        }

        private void Sel_SSTORI() { this.Sel_SSTORI(null); } 
        private void Sel_SSTORI(object sender)
        {
            try
            {
                nTRCD_ChgFlg = 1;
                //Refresh_DataCnt();

                //取引先CDが空白ならフラグをリセットしてreturn
                if (Txt_TRCD.ExCodeDB == "" && !Global.bIchigen)
                {
                    Set_InitVal();
                    SetDispVal_S();
                    Sel_TabData();

                    Lbl_Old_New1.Text = "";
                    FKB.F06_Enabled = false;
                    MNU_DELETE.Enabled = false;
                    nTRCDflg = 0;
                    nTRCD_ChgFlg = 0;
                    nDispChgFlg_Main = 0;
                    nDispChgFlg_TSHOH = 0;
                    nDispChgFlg_FRIGIN = 0;
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                    Lbl_Haifun.Enabled = false;
                    Txt_HJCD.ReadOnlyEx = true;
                    if (Txt_TRCD.ExCodeDB == "")
                    {
                        Txt_TRCD.Focus();
                    }
                    //else if (Txt_HJCD.ReadOnlyEx == false && Txt_HJCD.Text == "")
                    //{
                    //    //Txt_HJCD.Focus();
                    //}
                    //else
                    //{
                    //    //Txt_RYAKU.Focus();
                    //}
                    Chg_DispControl();
                    return;
                }
                else
                {
                    if (Global.nTRCD_HJ == 1)
                    {
                        Lbl_Haifun.Enabled = true;
                        Txt_HJCD.ReadOnlyEx = false;
                    }
                }

                //取引先CD変更フラグを確認し、変更有なら財務取引先を検索
                //変更無しならスルー
                
                if (nTRCDflg == 1)
                {
                    SetTRCDText(Txt_TRCD.ExCodeDB);
                    //Txt_TRCD.ExCodeDB = (Global.nTRCD_Type == 0 ? Txt_TRCD.ExCodeDB.PadLeft(Global.nTRCD_Len, '0') : Txt_TRCD.ExCodeDB.Trim().ToUpper());
                    Txt_TRCD.IsError = false;
                    Txt_HJCD.Text = (Txt_HJCD.Text != "" ? Txt_HJCD.Text.PadLeft(6, '0') : "000000");
                    int iCnt;
                    bool bHighDataExist;
                    bool bLowDataExist;
                    mcBsLogic.Sel_SS_TORI(Global.bIchigen ? Global.nIchigenCode : Txt_TRCD.ExCodeDB, Txt_HJCD.Text, out iCnt, out bHighDataExist, out bLowDataExist);

                    //債務にデータがなかったら財務のデータを初期値に設定
                    if (iCnt == 0)
                    {
                        // セキュリティ
                        if (Global.cUsrSec.nMFLG < 2)
                        {
                            return;
                        }
                        mcBsLogic.Init_DispVal();
                        Global.TRCD = Global.bIchigen ? Global.nIchigenCode : (Global.nTRCD_Type == 0 ? Txt_TRCD.ExCodeDB.PadLeft(Global.nTRCD_Len, '0') : Txt_TRCD.ExCodeDB.Trim().ToUpper());
                        Txt_HJCD.Text = (Txt_HJCD.Text != "" ? Txt_HJCD.Text.PadLeft(6, '0') : "000000");
                        Global.HJCD = Txt_HJCD.Text;
                        SetDispVal_S();
                        Sel_TabData();

                        mcBsLogic.Sel_TRNAM(Txt_TRCD.ExCodeDB, out iCnt);
                        if (iCnt != 0)
                        {
                            //画面に財務取引先の情報を反映
                            SetDispVal_Z();
                        }
                        else
                        {
                            Lbl_Old_New1.Text = "【　新規　】";
                            BindNavi1.Enabled = true;
                            //Txt_TRCD.ExCodeDB = Txt_TRCD.ExCodeDB;

                            if (sender != null)
                            {
                                var nextControl = Global.nTRCD_HJ == 1 && sender.Equals(Txt_TRCD)
                                    ? Txt_HJCD : Txt_RYAKU;
                                nextControl.Focus();
                            }
                            //if (Global.nTRCD_HJ == 0)
                            //{
                            //    //Txt_RYAKU.Focus();
                            //}
                            //else
                            //{
                            //    //Txt_HJCD.Focus();
                            //}

                            TabControl.TabPageCollection tabPages = Tb_Main.TabPages;
                            tabPages[0].Enabled = false;    // 基本情報タブ
                            tabPages[1].Enabled = false;    // 回収設定タブ
                            tabPages[2].Enabled = false;    // 支払条件タブ
                            tabPages[3].Enabled = false;    // 振込先情報タブ
                            tabPages[4].Enabled = false;    // その他情報タブ
                            tabPages[5].Enabled = false;    // 外貨設定タブ

//-- <2016/02/18 >
                            Cbo_SAIKEN.Enabled = false;
                            Cbo_SAIMU.Enabled = false;
                            DataCnt.Text = "0";
//-- <>
                            Refresh();
                        }
                    }
                    else
                    {
                        SetDispVal_S(); //債務取引先の情報を画面に設定
                        Sel_TabData();  //更新モードなので各タブのデータを検索

                        int iTRCDCnt;
                        mcBsLogic.Cnt_TRCD_Pos(Global.TRCD, Convert.ToInt32(Global.HJCD), out iTRCDCnt);
                        DataCnt.Text = iTRCDCnt.ToString("#,##0");
                    }
                    //--->V01.13.01 ATT ADD ▼ (7775)
                    if (sender != null)
                    {
                        var nextControl = Global.nTRCD_HJ == 1 && sender.Equals(Txt_TRCD)
                            ? Txt_HJCD : Txt_RYAKU;
                        nextControl.Focus();
                    }
                    //<---V01.13.01 ATT ADD ▲ (7775)
                    if (bHighDataExist == true && iCnt != 0)
                    {
                        BindNavi1_Next.Enabled = true;
                        BindNavi1_End.Enabled = true;
                    }
                    else
                    {
                        BindNavi1_Next.Enabled = false;
                        BindNavi1_End.Enabled = false;
                    }
                    if (bLowDataExist == true || MaxCnt.Text.Replace("/", "").Replace(",", "") != "0")
                    {
                        if (DataCnt.Text != "1")
                        {
                            BindNavi1_Prev.Enabled = true;
                            BindNavi1_First.Enabled = true;
                        }
                        else
                        {
                            BindNavi1_Prev.Enabled = false;
                            BindNavi1_First.Enabled = false;
                        }
                    }
                    else
                    {
                        BindNavi1_Prev.Enabled = false;
                        BindNavi1_First.Enabled = false;
                    }

                    nTRCDflg = 0;
                    nTRCD_ChgFlg = 0;
                    Chg_DispControl();

                    //手形管理のみで使用チェックON時にON→ONのケースで各項目がEnabled=trueとなってしまう件の対処
                    //if (Chk_TGASW.Checked == true)
                    //{
                    //    Chk_TGASW.Checked = false;
                    //    Chk_TGASW.Checked = true;
                    //}

                    nDispChgFlg_Main = 0;
                    nDispChgFlg_TSHOH = 0;
                    nDispChgFlg_FRIGIN = 0;
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSel_SSTORI　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //50音
        private void Txt_KNLD_Validating(object sender, CancelEventArgs e)
        {
            if (Txt_KNLD.Text != "")
            {
                // 不具合表-0040によりコメントとする Start
                //Regex regex = new Regex("^[ｱｰ-ﾞﾟｧｨｩｪｫｯｬｭｮ]+$");
                //if (!regex.IsMatch(Txt_KNLD.Text))
                //{
                //    Txt_KNLD.Focus();
                //    Txt_KNLD.IsError = true;
                //}
                // 不具合表-0040によりコメントとする End
            }
        }

        //初期値関連.主担当者コード
        private void Txt_STAN_CD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (Tb5_Txt_STAN_CD.Text != "")
                {
                    Tb5_Txt_STAN_CD.Text = Tb5_Txt_STAN_CD.Text.PadLeft(12, '0');
                    sTNAM = mcBsLogic.Get_TNAM(Tb5_Txt_STAN_CD.Text);
                    string sBSCOD = mcBsLogic.Get_TBMN(Tb5_Txt_STAN_CD.Text);
                    if (sTNAM != "")
                    {
                        Tb5_Txt_STAN_CD.IsError = false;
                        Tb5_Txt_STAN_NM.Text = sTNAM;
                    }
                    else
                    {
                        Tb5_Txt_STAN_NM.Text = "";
                        Tb5_Txt_STAN_CD.IsError = true;
                        Tb5_Txt_STAN_CD.Focus();
                        return;
                    }
                }
                else if (Tb5_Txt_STAN_CD.Text == "")
                {
                    Tb5_Txt_STAN_CD.IsError = false;
                    Tb5_Txt_STAN_NM.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTxt_STAN_CD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //初期値関連.部門コード
        private void Txt_SBCOD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (Tb5_Txt_SBCOD.ExCodeDB != "")
                {
                    Tb5_Txt_SBCOD.ExCodeDB = (Global.nBCOD_Type == 0 ? Tb5_Txt_SBCOD.ExCodeDB.PadLeft(Global.nBCOD_Len, '0') : Tb5_Txt_SBCOD.ExCodeDB.Trim().ToUpper());

                    //名称取得
                    sBNAM = mcBsLogic.Get_BNAM(Global.nBCOD_Type == 0 ? Tb5_Txt_SBCOD.ExCodeDB : Tb5_Txt_SBCOD.ExCodeDB.Trim().ToUpper().PadRight(Global.nBCOD_Len));
                    if (sBNAM != "")
                    {
                        Tb5_Txt_SBCOD.IsError = false;
                        Tb5_Txt_SBCOD_NM.Text = sBNAM;
                    }
                    else
                    {
                        Tb5_Txt_SBCOD_NM.Text = "";
                        Tb5_Txt_SBCOD.IsError = true;
                        Tb5_Txt_SBCOD.Focus();
                        return;
                    }
                }
                else if (Tb5_Txt_SBCOD.ExCodeDB == "")
                {
                    Tb5_Txt_SBCOD.IsError = false;
                    Tb5_Txt_SBCOD_NM.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTxt_SBCOD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //科目コード
        private void Txt_SKCOD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (Tb5_Txt_SKCOD.ExCodeDB != "")
                {
                    Tb5_Txt_SKCOD.ExCodeDB = (Global.nKCOD_Type == 0 ? Tb5_Txt_SKCOD.ExCodeDB.PadLeft(Global.nKCOD_Len, '0') : Tb5_Txt_SKCOD.ExCodeDB.Trim().ToUpper());
                    sKNAM = mcBsLogic.Get_KNMX(Global.nKCOD_Type == 0 ? Tb5_Txt_SKCOD.ExCodeDB.PadLeft(Global.nKCOD_Len, '0') : Tb5_Txt_SKCOD.ExCodeDB.Trim().ToUpper().PadRight(Global.nKCOD_Len));

                    if (sKNAM != "")
                    {
                        Tb5_Txt_SKCOD.IsError = false;
                        Tb5_Txt_SKINM.Text = sKNAM;
                    }
                    else
                    {
                        Tb5_Txt_SKINM.Text = "";
                        Tb5_Txt_SKCOD.IsError = true;
                        Tb5_Txt_SKCOD.Focus();
                        return;
                    }
                }
                else if (Tb5_Txt_SKCOD.ExCodeDB == "")
                {
                    Tb5_Txt_SKCOD.IsError = false;
                    Tb5_Txt_SKINM.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTxt_SKCOD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //支払条件.部門CD
        private void Tb1_Txt_BCOD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                //発生部門CD変更フラグを確認し、変更有なら財務取引先を検索
                //変更無しならスルー
                if (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0")
                {
                    Tb3_Txt_BCOD.ExCodeDB = "";
                    Tb3_Txt_BNAM.Text = "全て";
                    Tb3_Txt_BCOD.IsError = false;
                }
                else
                {
                    Tb3_Txt_BCOD.ExCodeDB = (Global.nBCOD_Type == 0 ? Tb3_Txt_BCOD.ExCodeDB.PadLeft(Global.nBCOD_Len, '0') : Tb3_Txt_BCOD.ExCodeDB.Trim().ToUpper());
                    sBNAM = mcBsLogic.Get_BNAM(Global.nBCOD_Type == 0 ? Tb3_Txt_BCOD.ExCodeDB : Tb3_Txt_BCOD.ExCodeDB.Trim().ToUpper().PadRight(Global.nBCOD_Len));
                    if (sBNAM != "")
                    {
                        Tb3_Txt_BNAM.Text = sBNAM;
                        Tb3_Txt_BCOD.IsError = false;
                    }
                    else
                    {
                        Tb3_Txt_BNAM.Text = "";
                        Tb3_Txt_BCOD.IsError = true;
                        Tb3_Txt_BCOD.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb1_Txt_BCOD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //支払条件.科目CD
        private void Tb1_Txt_KCOD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                //科目CD変更フラグを確認し、変更有なら財務取引先を検索
                //変更無しならスルー
                if (Tb3_Txt_KCOD.ExCodeDB == "0" || Tb3_Txt_KCOD.ExCodeDB == "")
                {
                    Tb3_Txt_KCOD.ExCodeDB = "";
                    Tb3_Txt_KINM.Text = "全て";
                    Tb3_Txt_KCOD.IsError = false;
                }
                else
                {
                    Tb3_Txt_KCOD.ExCodeDB = (Global.nKCOD_Type == 0 ? Tb3_Txt_KCOD.ExCodeDB.PadLeft(Global.nKCOD_Len, '0') : Tb3_Txt_KCOD.ExCodeDB.Trim().ToUpper());
                    sKNAM = mcBsLogic.Get_KNAM(Global.nKCOD_Type == 0 ? Tb3_Txt_KCOD.ExCodeDB : Tb3_Txt_KCOD.ExCodeDB.Trim().ToUpper().PadRight(Global.nKCOD_Len));
                    if (sKNAM != "")
                    {
                        Tb3_Txt_KINM.Text = sKNAM;
                        Tb3_Txt_KCOD.IsError = false;
                    }
                    else
                    {
                        Tb3_Txt_KINM.Text = "";
                        Tb3_Txt_KCOD.IsError = true;
                        Tb3_Txt_KCOD.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb1_Txt_KCOD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //支払条件.支払方法
        private void Tb1_Txt_SHINO_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                //支払方法変更フラグを確認し、変更有なら財務取引先を検索
                //変更無しならスルー
                if (Tb3_Txt_SHINO.Text != "")
                {
                    if (nTRCDflg == 1 || nShinoChgFlg == 1)
                    {
                        Tb3_Txt_SHINO.Text = Tb3_Txt_SHINO.Text.PadLeft(3, '0');
                        string sSHINO = Tb3_Txt_SHINO.Text;
                        mcBsLogic.Sel_SHINM(sSHINO, out sSHINM);

                        //支払方法が存在した場合、支払方法テーブルから紐付く情報を取得
                        if (sSHINM != "")
                        {
                            Tb3_Txt_SHINM.Text = sSHINM;
                            Tb3_Txt_SHINO.IsError = false;

                            mcBsLogic.Sel_SS_SHOHO(sSHINO);

                            if (Tb3_Lbl_Old_New2.Text == "【　新規　】")
                            {
                                nTRCDflg = 0;
                            }
                            else
                            {
                                if (nShinoChgFlg == 0)
                                {
                                    nTRCDflg = 1;
                                }
                            }
                            Set_Tb1_SS_SHOHO();
                            Set_Tb3_Cmb();
                        }
                        else
                        {
                            Tb3_Txt_SHINO.IsError = true;
                            Tb3_Txt_SHINO.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    Tb3_Txt_SHINO.IsError = false;
                }
                //支払方法が空白ならクリア
                if (Tb3_Txt_SHINO.Text == "")
                {
                    //支払条件タブ
                    Tb3_Txt_SHINM.Text = "";
                    Tb3_Txt_SHIMEBI.Text = "";
                    Tb3_Txt_SHIHARAIMM.Text = "";
                    Tb3_Txt_SKIJITUMM.Text = "";
                    Tb3_Txt_SIHARAIDD.Text = "";
                    Tb3_Txt_SKIJITUDD.Text = "";
                    Tb3_Cmb_HARAI_H.Text = "0:前営業日";
                    Tb3_Cmb_KIJITU_H.Text = "0:前営業日";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_V_YAKUJO.Text = "";
                    Tb3_Txt_YAKUJOA_L.Text = "";
                    Tb3_Txt_YAKUJOA_M.Text = "";
                    Tb3_Txt_YAKUJOB_LH.Text = "";
                    Tb3_Txt_YAKUJOB_H1.Text = "";
                    Tb3_Txt_YAKUJOB_R1.Text = "";
                    Tb3_Txt_YAKUJOB_U1.Text = "";
                    Tb3_Txt_YAKUJOB_H2.Text = "";
                    Tb3_Txt_YAKUJOB_R2.Text = "";
                    Tb3_Txt_YAKUJOB_U2.Text = "";
                    Tb3_Txt_YAKUJOB_H3.Text = "";
                    Tb3_Txt_YAKUJOB_R3.Text = "";
                    Tb3_Txt_YAKUJOB_U3.Text = "";
                    //依頼先情報タブ
                    Tb3_Lbl_HARAI_KBN1.Text="";
                    Tb3_Lbl_HARAI_KBN2.Text="";
                    Tb3_Lbl_HARAI_KBN3.Text="";
                    Tb3_Lbl_HARAI_KBN4.Text = "";
                    nDispChgFlg_TSHOH = 0;// <--- V02.37.01 YMP ADD  ◀(122172)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb1_Txt_SHINO_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        /// <summary>
        /// 指定された支払方法に紐付く情報を依頼先情報タブに設定
        /// </summary>
        private void Set_Tb3_Cmb()
        {
            try
            {
                string sTRCD = Txt_TRCD.ExCodeDB;
                string sHJCD = (Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
                int nID = Convert.ToInt32(BindNavi2_Selected.Text);
                string sSHINO = Tb3_Txt_SHINO.Text;

                //コンボボックスの生成
                if (Tb3_Lbl_HARAI_KBN1.Text != "")
                {
                    Generate_Tb3_Cmb(1);
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN1.DataSource = null;
                }
                if (Tb3_Lbl_HARAI_KBN2.Text != "")
                {
                    Generate_Tb3_Cmb(2);
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN2.DataSource = null;
                }
                if (Tb3_Lbl_HARAI_KBN3.Text != "")
                {
                    Generate_Tb3_Cmb(3);
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN3.DataSource = null;
                }
                if (Tb3_Lbl_HARAI_KBN4.Text != "")
                {
                    Generate_Tb3_Cmb(4);
                }
                else
                {
                    Tb3_Cmb_HARAI_KBN4.DataSource = null;
                }

                //初期値の確定
                if (nTRCDflg == 1 || nTabBindNavi == 1)
                {
                    //取引先支払方法の値を設定
                    mcBsLogic.Get_TSHOH(sTRCD, sHJCD, nID);
                }
                else
                {
                    //自社支払方法の値を設定
                    mcBsLogic.Get_SHOHO(sSHINO);
                    if (Global.KUBN1_tb3 != "" && mcBsLogic.Get_SKBKIND(Global.KUBN1_tb3) == "8")
                    {
                        mcBsLogic.Get_Facid_To_Ownbk(1, Global.OWNID1);
                    }
                    if (Global.KUBN2_tb3 != "" && mcBsLogic.Get_SKBKIND(Global.KUBN2_tb3) == "8")
                    {
                        mcBsLogic.Get_Facid_To_Ownbk(2, Global.OWNID2);
                    }
                    if (Global.KUBN3_tb3 != "" && mcBsLogic.Get_SKBKIND(Global.KUBN3_tb3) == "8")
                    {
                        mcBsLogic.Get_Facid_To_Ownbk(3, Global.OWNID3);
                    }
                    if (Global.KUBN4_tb3 != "" && mcBsLogic.Get_SKBKIND(Global.KUBN4_tb3) == "8")
                    {
                        mcBsLogic.Get_Facid_To_Ownbk(4, Global.OWNID4);
                    }
                }

                if (Tb3_Lbl_HARAI_KBN1.Text != "")
                {
                    string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN1.Text.Substring(0, Tb3_Lbl_HARAI_KBN1.Text.IndexOf(':')));
                    //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                    if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                    {
                        Tb3_Cmb_HARAI_KBN1.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & false;
                    }
                    else if (Global.BANKNM1_tb3 != "" || Global.SITENNM1_tb3 != "" || Global.KOZA1_tb3 != "" || Global.KOZANO1_tb3 != "" || Global.IRAININ1_tb3 != "")
                    {
                        //Tb3_Cmb_HARAI_KBN1.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK1_tb3, Global.SITEN1_tb3,
                        //                                                             Global.KOZA1_tb3, Global.KOZANO1_tb3, Global.IRAININ1_tb3, sSKBKIND);
                        Tb3_Cmb_HARAI_KBN1.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK1_tb3, Global.SITEN1_tb3,
                                                                                     Global.KOZA1_tb3, Global.KOZANO1_tb3, Global.IRAININ1_tb3, sSKBKIND);
                        Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & true;
                    }
                    else
                    {
                        Tb3_Cmb_HARAI_KBN1.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN1.Enabled = Global.bEnabledState & true;
                    }
                }
                if (Tb3_Lbl_HARAI_KBN2.Text != "")
                {
                    string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN2.Text.Substring(0, Tb3_Lbl_HARAI_KBN2.Text.IndexOf(':')));
                    //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                    if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                    {
                        Tb3_Cmb_HARAI_KBN2.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & false;
                    }
                    else if (Global.BANKNM2_tb3 != "" || Global.SITENNM2_tb3 != "" || Global.KOZA2_tb3 != "" || Global.KOZANO2_tb3 != "" || Global.IRAININ2_tb3 != "")
                    {
                        //Tb3_Cmb_HARAI_KBN2.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK2_tb3, Global.SITEN2_tb3,
                        //                                                             Global.KOZA2_tb3, Global.KOZANO2_tb3, Global.IRAININ2_tb3, sSKBKIND);
                        Tb3_Cmb_HARAI_KBN2.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK2_tb3, Global.SITEN2_tb3,
                                                                                     Global.KOZA2_tb3, Global.KOZANO2_tb3, Global.IRAININ2_tb3, sSKBKIND);

                        Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & true;
                    }
                    else
                    {
                        Tb3_Cmb_HARAI_KBN2.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN2.Enabled = Global.bEnabledState & true;
                    }
                }
                if (Tb3_Lbl_HARAI_KBN3.Text != "")
                {
                    string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN3.Text.Substring(0, Tb3_Lbl_HARAI_KBN3.Text.IndexOf(':')));
                    //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                    if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                    {
                        Tb3_Cmb_HARAI_KBN3.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & false;
                    }
                    else if (Global.BANKNM3_tb3 != "" || Global.SITENNM3_tb3 != "" || Global.KOZA3_tb3 != "" || Global.KOZANO3_tb3 != "" || Global.IRAININ3_tb3 != "")
                    {
                        //Tb3_Cmb_HARAI_KBN3.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK3_tb3, Global.SITEN3_tb3,
                        //                                                             Global.KOZA3_tb3, Global.KOZANO3_tb3, Global.IRAININ3_tb3, sSKBKIND);
                        Tb3_Cmb_HARAI_KBN3.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK3_tb3, Global.SITEN3_tb3,
                                                                                     Global.KOZA3_tb3, Global.KOZANO3_tb3, Global.IRAININ3_tb3, sSKBKIND);

                        Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & true;
                    }
                    else
                    {
                        Tb3_Cmb_HARAI_KBN3.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN3.Enabled = Global.bEnabledState & true;
                    }
                }
                if (Tb3_Lbl_HARAI_KBN4.Text != "")
                {
                    string sSKBKIND = mcBsLogic.Get_SKBKIND(Tb3_Lbl_HARAI_KBN4.Text.Substring(0, Tb3_Lbl_HARAI_KBN4.Text.IndexOf(':')));
                    //if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11" || sSKBKIND == "12")
                    if (sSKBKIND == "4" || sSKBKIND == "5" || sSKBKIND == "9" || sSKBKIND == "11")
                    {
                        Tb3_Cmb_HARAI_KBN4.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & false;
                    }
                    else if (Global.BANKNM4_tb3 != "" || Global.SITENNM4_tb3 != "" || Global.KOZA4_tb3 != "" || Global.KOZANO4_tb3 != "" || Global.IRAININ4_tb3 != "")
                    {
                        //Tb3_Cmb_HARAI_KBN4.SelectedValue = mcBsLogic.Get_SelectedKey(Global.BANK4_tb3, Global.SITEN4_tb3,
                        //                                                             Global.KOZA4_tb3, Global.KOZANO4_tb3, Global.IRAININ4_tb3, sSKBKIND);
                        Tb3_Cmb_HARAI_KBN4.SelectedIndex = mcBsLogic.Get_SelectedKey(Global.BANK4_tb3, Global.SITEN4_tb3,
                                                                                     Global.KOZA4_tb3, Global.KOZANO4_tb3, Global.IRAININ4_tb3, sSKBKIND);


                        Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & true;
                    }
                    else
                    {
                        Tb3_Cmb_HARAI_KBN4.SelectedValue = -1;
                        Tb3_Cmb_HARAI_KBN4.Enabled = Global.bEnabledState & true;
                    }
                }

                //支払方法変更フラグをリセット
                nShinoChgFlg = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nSet_Tb3_Cmb　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //振込先情報.銀行CD
        private void Tb2_Txt_BANK_CD_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                //string[] sArray = null;

                //銀行CD変更フラグを確認し、変更有なら財務銀行を検索
                //変更無しならスルー
                if (Tb4_Txt_BANK_CD.Text != "")
                {
                    Tb4_Txt_BANK_CD.Text = Tb4_Txt_BANK_CD.Text.PadLeft(4, '0');
                    sBANKNM = mcBsLogic.Get_BANKNM(Tb4_Txt_BANK_CD.Text);
                    // ---> V02.37.01 YMP ADD ▼(122172)
                    Tb4_Cmb_FTESUID.DisplayMember = "Value";
                    Tb4_Cmb_FTESUID.ValueMember = "Key";
                    Tb4_Cmb_FTESUID.DataSource = GetTesuuID(Tb4_Txt_BANK_CD.Text);
                    // <--- V02.37.01 YMP ADD ▲(122172)

                    if (sBANKNM != "")
                    {
                        Tb4_Txt_BANK_NM.Text = sBANKNM;
                        Tb4_Txt_SITEN_ID.ReadOnlyEx = false;
                        Tb4_Txt_BANK_CD.IsError = false;
                    }
                    else
                    {
                        Tb4_Txt_BANK_NM.Text = "";
                        Tb4_Txt_SITEN_ID.Text = "";
                        Tb4_Txt_SITEN_NM.Text = "";
                        Tb4_Txt_SITEN_ID.ReadOnlyEx = true;
                        Tb4_Txt_BANK_CD.IsError = true;
                        Tb4_Txt_BANK_CD.Focus();
                        return;
                    }
                }
                else if (Tb4_Txt_BANK_CD.Text == "")
                {
                    Tb4_Txt_BANK_NM.Text = "";
                    Tb4_Txt_SITEN_ID.Text = "";
                    Tb4_Txt_SITEN_NM.Text = "";
                    Tb4_Txt_SITEN_ID.ReadOnlyEx = true;
                    Tb4_Txt_BANK_CD.IsError = false;
                    nDispChgFlg_FRIGIN = 0;// <--- V02.37.01 YMP ADD ◀(122172)
                }
                // ---> V02.37.01 YMP DELETE ▼(122172)
                //Tb4_Cmb_FTESUID.DisplayMember = "Value";
                //Tb4_Cmb_FTESUID.ValueMember = "Key";
                //Tb4_Cmb_FTESUID.DataSource = GetTesuuID(Tb4_Txt_BANK_CD.Text);
                // <--- V02.37.01 YMP DELETE ▲(122172)
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb2_Txt_BANK_CD_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        //振込先情報.支店ID
        private void Tb2_Txt_SITEN_ID_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                //支店CD変更フラグを確認し、変更有なら財務銀行支店を検索
                //変更無しならスルー
                if (Tb4_Txt_SITEN_ID.Text != "")
                {
                    Tb4_Txt_SITEN_ID.Text = Tb4_Txt_SITEN_ID.Text.PadLeft(3, '0');
                    if (Tb4_Txt_BANK_CD.Text == "")
                    {
                        Tb4_Txt_SITEN_ID.IsError = true;
                        Tb4_Txt_SITEN_ID.Focus();
                        return;
                    }
                    else
                    {
                        string sBANK_CD = Tb4_Txt_BANK_CD.Text;
                        string sSITEN_ID = Tb4_Txt_SITEN_ID.Text;
                        sSITENNM = mcBsLogic.Get_SITENNM(sBANK_CD, sSITEN_ID);

                        if (sSITENNM != "" && sBANK_CD != "")
                        {
                            Tb4_Txt_SITEN_NM.Text = sSITENNM;
                            Tb4_Txt_SITEN_ID.IsError = false;
                            Tb4_Txt_BANK_CD.IsError = false;
                        }
                        else
                        {
                            Tb4_Txt_SITEN_ID.IsError = true;
                            Tb4_Txt_SITEN_ID.Focus();
                            return;
                        }
                    }
                }
                else if (Tb4_Txt_SITEN_ID.Text == "")
                {
                    Tb4_Txt_SITEN_NM.Text = "";
                    Tb4_Txt_SITEN_ID.IsError = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb2_Txt_SITEN_ID_Validating　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }
        #endregion


        #region 履歴関連 ＠2011/07 履歴対応
        //変更情報をdtRIREKIに格納
        private void Set_dtRIREKI(int nBNID, int nID, string sItemNM_B, int nKBN, string sItemNM_R, string sBeforeData, string sAfterData)
        {
            DataRow dRow = Global.dtRIREKI.NewRow();
            string trcd = Txt_TRCD.ExCodeDB;
            if (Global.nTRCD_Type == 1) { trcd = trcd.PadRight(Global.nTRCD_Len); }
            dRow["RKTRCD"] = trcd;

            if (Txt_HJCD.Text == null || Txt_HJCD.Text == "")
            {
                dRow["RKHJCD"] = "0";
            }
            else
            {
                dRow["RKHJCD"] = Txt_HJCD.Text;
            }

            dRow["RKBNID"] = nBNID;
            dRow["RKID"] = nID;
            dRow["RKKKM"] = sItemNM_B;
            dRow["RKKBN"] = nKBN;
            if (sItemNM_R != null)
            {
                dRow["RKNM"] = sItemNM_R;
            }
            else
            {
                dRow["RKNM"] = DBNull.Value;
            }
            if (sBeforeData != null)
            {
                dRow["RKBITM"] = sBeforeData;
            }
            else
            {
                dRow["RKBITM"] = DBNull.Value;
            }
            if (sAfterData != null)
            {
                dRow["RKAITEM"] = sAfterData;
            }
            else
            {
                dRow["RKAITEM"] = DBNull.Value;
            }
            Global.dtRIREKI.Rows.Add(dRow);
        }
        #endregion

        

        private void Tb2_Txt_MEIGIK_Validating(object sender, CancelEventArgs e)
        {
            if (Tb4_Txt_MEIGIK.Text != "")
            {
                Tb4_Txt_MEIGIK.Text = ToZenkakuKana(Tb4_Txt_MEIGIK.Text);
            }
        }

        public static string ToZenkakuKana(string str)
        {
            if (str == null || str.Length == 0)
            {
                return str;
            }

            char[] cs = str.ToCharArray();
            int f = str.Length;

            for (int i = 0; i < f; i++)
            {
                char c = cs[i];
                // ｦ(0xFF66) ～ ﾟ(0xFF9F)
                if ('ｦ' <= c && c <= 'ﾟ')
                {
                    char m = ConvertToZenkakuKanaChar(c);
                    if (m != '\0')
                    {
                        cs[i] = m;
                    }
                }
            }

            return new string(cs);
        }

        private static char ConvertToZenkakuKanaChar(char hankakuChar)
        {
            switch (hankakuChar)
            {
                case 'ｧ':
                    return 'ｱ';
                case 'ｨ':
                    return 'ｲ';
                case 'ｩ':
                    return 'ｳ';
                case 'ｪ':
                    return 'ｴ';
                case 'ｫ':
                    return 'ｵ';
                case 'ｰ':
                    return '-';
                case 'ｬ':
                    return 'ﾔ';
                case 'ｭ':
                    return 'ﾕ';
                case 'ｮ':
                    return 'ﾖ';
                case 'ｯ':
                    return 'ﾂ';
                default:
                    return hankakuChar;
            }
        }

        private void frmSMTORI_Shown(object sender, EventArgs e)
        {
            // 2012.10.10 S
            if (Global.sTRCD_R == "" && Global.sTRNAM_R != "")
            {
                // 何故かこうしないと一見入力時にTABがグレーにならない
                Txt_TORI_NAM.Focus();
                Txt_RYAKU.Focus();

                //起動時点では登録ボタンは押下不可
                Btn_REG.Enabled = false;
                FKB.F10_Enabled = false;
                nDispChgFlg_Main = 0;
                nDispChgFlg_TSHOH = 0;
                nDispChgFlg_FRIGIN = 0;
            }
            // 2012.10.10 E
        }

        private void Tb4_Txt_HOVAL_Validating(object sender, CancelEventArgs e)
        {
            if (nHOVAL_Before != Tb5_Txt_HOVAL.ExNumValue)
            {
                nDispChgFlg_Main = 1;
                Btn_REG.Enabled = true;
                FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
            }

        }

        //private void Tb5_Txt_UsrNo_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (!((e.KeyChar >= 'a' && e.KeyChar <= 'z') ||
        //          (e.KeyChar >= 'A' && e.KeyChar <= 'Z') ||
        //          (e.KeyChar >= '0' && e.KeyChar <= '9')))
        //    {
        //        if (e.KeyChar != '\b')
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //}

        //private void Tb5_Txt_UsrNo_Validating(object sender, CancelEventArgs e)
        //{
        //    Tb1_Txt_UsrNo.Text = Tb1_Txt_UsrNo.Text.ToUpper();
        //}

        //private void Tb5_Txt_UsrNo_TextChanged(object sender, EventArgs e)
        //{
        //    nDispChgFlg_Main = 1;
        //    Btn_REG.Enabled = true;
        //    FKB.F10_Enabled = true;
        //}
        //**>>ICS-S 2013/05/20
        private void txt_RefNo_Validated(object sender, EventArgs e)
        {
            //Tb5_Txt_RefNo.Text = Tb5_Txt_RefNo.Text.ToUpper();
        }

        private void Tb5_Txt_RefNo_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Txt_FAC_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Chk_Jyoto_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        //2013/07/16 ICS.居軒 ▼分割印刷＆プレビュー対応
        private void MNU_OPT_PAGE_Click(object sender, EventArgs e)
        {
            // 頁数設定ダイアログを表示する
            // Ver.01.02.02 Toda -->
            //IcsSSSPrint.PrintOption.ShowOptionDlg(Global.cConKaisya, Global.sPrgId);
            IcsSSSPrint.PrintOption.ShowOptionDlg(Global.cConSaikenSaimu, Global.sPrgId);
            // Ver.01.02.02 <--
        }
        //2013/07/16 ICS.居軒 ▲分割印刷＆プレビュー対応

        //**<<

        private void Tb1_Chk_SOSAI_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb1_Chk_SOSAI.Checked == true)
            {
                Tb1_Chk_SRYOU_F.Enabled = true;
            }
            else
            {
                Tb1_Chk_SRYOU_F.Enabled = false;
                Tb1_Chk_SRYOU_F.Checked = false;
            }

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Txt_GRPID_Validating(object sender, CancelEventArgs e)
        {
            // Ver.01.09.02 [SIAS_7220] Toda -->
            //if (Txt_GRPID.ExNumValue != 0)
            //{
            //    Txt_GRPNM.Text = mcBsLogic.Get_GrpNm(Txt_GRPID.ExNumValue.ToString());
            //}
            //else
            //{
            //    Txt_GRPNM.ClearValue();
            //}
            if (Txt_GRPID.ExNumValue == 0)
            {
                Txt_GRPNM.ClearValue();
                return;
            }

            string txt = mcBsLogic.Get_GrpNm(Txt_GRPID.ExNumValue.ToString());
            if (txt == "")
            {
                Txt_GRPNM.ClearValue();
                e.Cancel = true;
                Txt_GRPID.IsError = true;
                return;
            }
            else
            {
                Txt_GRPNM.Text = txt;
            }
            // Ver.01.09.02 <--
        }

        private void Tb1_Txt_E_TANTOCD_Validating(object sender, CancelEventArgs e)
        {
            if (Tb1_Txt_E_TANTOCD.ExCode != "")
            {
                //---> V02.01.01 HWPO UPDATE ▼【PostgreSQL対応】
                //Tb1_Txt_E_TANTONM.Text = mcBsLogic.Get_ETanNm(Tb1_Txt_E_TANTOCD.ExCode);
                Tb1_Txt_E_TANTONM.Text = mcBsLogic.Get_ETanNm(Tb1_Txt_E_TANTOCD.ExCodeDB);
                //<--- V02.01.01 HWPO UPDATE ▲【PostgreSQL対応】
                if (Tb1_Txt_E_TANTONM.Text == "")
                {
                    Tb1_Txt_E_TANTOCD.IsError = true;
                    e.Cancel = true;
                }
            }
            else
            {
                Tb1_Txt_E_TANTONM.ClearValue();
            }
        }

        private void Tb2_Chk_YAKUJO_CheckedChanged(object sender, EventArgs e)
        {
            if ( Tb2_Chk_YAKUJO.Checked == true)
            {
                Tb2_Cmb_KAISYU.Enabled = false;
                Tb2_Cmb_KAISYU.SelectedIndex = -1;

                Tb2_Txt_Y_KINGAKU.Enabled = true;
                Tb2_Lbl_Y_KINGAKU_EN.Enabled = true;
                Tb2_Cmb_MIMAN.Enabled = true;
                Tb2_Cmb_IJOU_1.Enabled = true;
                //Tb2_Txt_BUNKATSU_1.Enabled = true;
                //Tb2_Cmb_HASU_1.Enabled = true;
                //Tb2_Txt_SIGHT_M_1.Enabled = true;
                //Tb2_Txt_SIGHT_D_1.Enabled = true;
                //Tb2_Cmb_IJOU_2.Enabled = true;
                //Tb2_Txt_BUNKATSU_2.Enabled = true;
                //Tb2_Cmb_HASU_2.Enabled = true;
                //Tb2_Txt_SIGHT_M_2.Enabled = true;
                //Tb2_Txt_SIGHT_D_2.Enabled = true;
                //Tb2_Cmb_IJOU_3.Enabled = true;
                //Tb2_Txt_BUNKATSU_3.Enabled = true;
                //Tb2_Cmb_HASU_3.Enabled = true;
                //Tb2_Txt_SIGHT_M_3.Enabled = true;
                //Tb2_Txt_SIGHT_D_3.Enabled = true;

                Tb2_Lbl_Y_KINGAKU.Enabled = true;
                Tb2_Lbl_MIMAN.Enabled = true;
                Tb2_Lbl_IJOU_1.Enabled = true;
                //Tb2_Lbl_IJOU_2.Enabled = true;
                //Tb2_Lbl_IJOU_3.Enabled = true;
                Tb2_Lbl_BUNKATSU.Enabled = true;
                Tb2_Lbl_BUNKATSU_1.Enabled = true;
                //Tb2_Lbl_BUNKATSU_2.Enabled = true;
                //Tb2_Lbl_BUNKATSU_3.Enabled = true;
                Tb2_Lbl_HASU.Enabled = true;
                Tb2_Lbl_SIGHT.Enabled = true;
                Tb2_Lbl_SIGHT_M_1.Enabled = true;
                //Tb2_Lbl_SIGHT_M_2.Enabled = true;
                //Tb2_Lbl_SIGHT_M_3.Enabled = true;
                Tb2_Lbl_SIGHT_D_1.Enabled = true;
                //Tb2_Lbl_SIGHT_D_2.Enabled = true;
                //Tb2_Lbl_SIGHT_D_3.Enabled = true;
//-- <9999>
                Tb2_Txt_KAISYUSIGHT_D.ClearValue();
                Tb2_Txt_KAISYUSIGHT_M.ClearValue();
                Tb2_Txt_KAISYUSIGHT_D.Enabled = false;
                Tb2_Txt_KAISYUSIGHT_M.Enabled = false;
                Tb2_Lbl_SIGHT_Main.Enabled = false;
                Tb2_Lbl_SIGHT_M.Enabled = false;
                Tb2_Lbl_SIGHT_D.Enabled = false;
//-- <9999>
            }
            else
            {
                Tb2_Cmb_KAISYU.Enabled = true;

//-- <9999>
                Tb2_Txt_KAISYUSIGHT_D.ClearValue();
                Tb2_Txt_KAISYUSIGHT_M.ClearValue();
                Tb2_Txt_KAISYUSIGHT_D.Enabled = false;
                Tb2_Txt_KAISYUSIGHT_M.Enabled = false;
                Tb2_Lbl_SIGHT_Main.Enabled = false;
                Tb2_Lbl_SIGHT_M.Enabled = false;
                Tb2_Lbl_SIGHT_D.Enabled = false;
//-- <9999>

                Tb2_Txt_Y_KINGAKU.ClearValue();
                Tb2_Txt_BUNKATSU_1.ClearValue();
                Tb2_Txt_SIGHT_M_1.ClearValue();
                Tb2_Txt_SIGHT_D_1.ClearValue();
                Tb2_Txt_BUNKATSU_2.ClearValue();
                Tb2_Txt_SIGHT_M_2.ClearValue();
                Tb2_Txt_SIGHT_D_2.ClearValue();
                Tb2_Txt_BUNKATSU_3.ClearValue();
                Tb2_Txt_SIGHT_M_3.ClearValue();
                Tb2_Txt_SIGHT_D_3.ClearValue();

                Tb2_Txt_Y_KINGAKU.Enabled = false;
                Tb2_Lbl_Y_KINGAKU_EN.Enabled = false;
                Tb2_Cmb_MIMAN.Enabled = false;
                Tb2_Cmb_IJOU_1.Enabled = false;
                Tb2_Txt_BUNKATSU_1.Enabled = false;
                Tb2_Cmb_HASU_1.Enabled = false;
                Tb2_Txt_SIGHT_M_1.Enabled = false;
                Tb2_Txt_SIGHT_D_1.Enabled = false;
                Tb2_Cmb_IJOU_2.Enabled = false;
                Tb2_Txt_BUNKATSU_2.Enabled = false;
                Tb2_Cmb_HASU_2.Enabled = false;
                Tb2_Txt_SIGHT_M_2.Enabled = false;
                Tb2_Txt_SIGHT_D_2.Enabled = false;
                Tb2_Cmb_IJOU_3.Enabled = false;
                Tb2_Txt_BUNKATSU_3.Enabled = false;
                Tb2_Cmb_HASU_3.Enabled = false;
                Tb2_Txt_SIGHT_M_3.Enabled = false;
                Tb2_Txt_SIGHT_D_3.Enabled = false;

                Tb2_Cmb_MIMAN.SelectedIndex = -1;
                Tb2_Cmb_IJOU_1.SelectedIndex = -1;
                Tb2_Cmb_HASU_1.SelectedIndex = -1;
                Tb2_Cmb_IJOU_2.SelectedIndex = -1;
                Tb2_Cmb_HASU_2.SelectedIndex = -1;
                Tb2_Cmb_IJOU_3.SelectedIndex = -1;
                Tb2_Cmb_HASU_3.SelectedIndex = -1;

                Tb2_Lbl_Y_KINGAKU.Enabled = false;
                Tb2_Lbl_MIMAN.Enabled = false;
                Tb2_Lbl_IJOU_1.Enabled = false;
                Tb2_Lbl_IJOU_2.Enabled = false;
                Tb2_Lbl_IJOU_3.Enabled = false;
                Tb2_Lbl_BUNKATSU.Enabled = false;
                Tb2_Lbl_BUNKATSU_1.Enabled = false;
                Tb2_Lbl_BUNKATSU_2.Enabled = false;
                Tb2_Lbl_BUNKATSU_3.Enabled = false;
                Tb2_Lbl_HASU.Enabled = false;
                Tb2_Lbl_SIGHT.Enabled = false;
                Tb2_Lbl_SIGHT_M_1.Enabled = false;
                Tb2_Lbl_SIGHT_M_2.Enabled = false;
                Tb2_Lbl_SIGHT_M_3.Enabled = false;
                Tb2_Lbl_SIGHT_D_1.Enabled = false;
                Tb2_Lbl_SIGHT_D_2.Enabled = false;
                Tb2_Lbl_SIGHT_D_3.Enabled = false;
            }
        }

        private void Tb2_Chk_GAIKA_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb2_Chk_GAIKA.Checked == true)
            {
                Tb2_Cmb_TSUKA.Enabled = true;
                Tb2_Txt_GAIKA_KEY_F.Enabled = true;
                Tb2_Txt_GAIKA_KEY_B.Enabled = true;
                
                Tb2_Lbl_TSUKA.Enabled = true;
                Tb2_Lbl_GAIKA_KEY_F.Enabled = true;
                Tb2_Lbl_GAIKA_KEY_T.Enabled = true;

                Tb2_Chk_YAKUJO.Checked = false;
                Tb2_Chk_YAKUJO.Enabled = false;
            }
            else
            {
                Tb2_Cmb_TSUKA.SelectedIndex = -1;
                Tb2_Txt_GAIKA_KEY_F.ClearValue();
                Tb2_Txt_GAIKA_KEY_B.ClearValue();

                Tb2_Cmb_TSUKA.Enabled = false;
                Tb2_Txt_GAIKA_KEY_F.Enabled = false;
                Tb2_Txt_GAIKA_KEY_B.Enabled = false;

                Tb2_Lbl_TSUKA.Enabled = false;
                Tb2_Lbl_GAIKA_KEY_F.Enabled = false;
                Tb2_Lbl_GAIKA_KEY_T.Enabled = false;

                Tb2_Chk_YAKUJO.Enabled = true;
            }
            SetKaisyuComboList(Tb2_Chk_GAIKA.Checked);
        }

        private void Tb2_Txt_SEN_GINKOCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_SEN_GINKOCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_SEN_GINKOCD_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_SEN_GINKOCD.ExCodeDB == "")
            {
                Tb2_Txt_SEN_GINKONM.ClearValue();
                Tb2_Txt_SEN_SITENCD.ClearValue();
//-- <2016/03/11 支店名称表示追加>
                Tb2_Txt_SEN_SITENNM.ClearValue();
//-- <2016/03/11>
                Tb2_Txt_SEN_KSITENCD.ClearValue();
                Tb2_Txt_SEN_KSITENNM.ClearValue();

                Tb2_Txt_SEN_SITENCD.Enabled = false;
                Tb2_Txt_SEN_KSITENCD.Enabled = false;
                Tb2_Txt_SEN_KSITENNM.Enabled = false;

                Tb2_Lbl_SEN_SITENCD.Enabled = false;
                Tb2_Lbl_SEN_KSITENCD.Enabled = false;
//-- <9999>
                Tb2_Cmb_YOKINSYU.SelectedIndex = -1;
                Tb2_Txt_SEN_KOZANO.ClearValue();
                Tb2_Cmb_YOKINSYU.Enabled = false;
                Tb2_Txt_SEN_KOZANO.Enabled = false;
                Tb2_Lbl_SEN_YOKINSYU.Enabled = false;
                Tb2_Lbl_SEN_KOZANO.Enabled = false;
//-- <9999>
                return;
            }

            Tb2_Txt_SEN_GINKONM.Text = mcBsLogic.Get_BANKNM(Tb2_Txt_SEN_GINKOCD.ExCode);

            if (Tb2_Txt_SEN_GINKONM.Text == "")
            {
                Tb2_Txt_SEN_GINKOCD.IsError = true;
                e.Cancel = true;

                Tb2_Txt_SEN_GINKONM.ClearValue();
                Tb2_Txt_SEN_SITENCD.ClearValue();
//-- <2016/03/11 支店名称表示追加>
                Tb2_Txt_SEN_SITENNM.ClearValue();
//-- <2016/03/11>
                Tb2_Txt_SEN_KSITENCD.ClearValue();
                Tb2_Txt_SEN_KSITENNM.ClearValue();

                Tb2_Txt_SEN_SITENCD.Enabled = false;
                Tb2_Txt_SEN_KSITENCD.Enabled = false;
                Tb2_Txt_SEN_KSITENNM.Enabled = false;

                Tb2_Lbl_SEN_SITENCD.Enabled = false;
                Tb2_Lbl_SEN_KSITENCD.Enabled = false;
//-- <9999>
                Tb2_Cmb_YOKINSYU.SelectedIndex = -1;
                Tb2_Txt_SEN_KOZANO.ClearValue();
                Tb2_Cmb_YOKINSYU.Enabled = false;
                Tb2_Txt_SEN_KOZANO.Enabled = false;
                Tb2_Lbl_SEN_YOKINSYU.Enabled = false;
                Tb2_Lbl_SEN_KOZANO.Enabled = false;
//-- <9999>
                return;
            }

            Tb2_Txt_SEN_SITENCD.Enabled = true;
            Tb2_Txt_SEN_SITENNM.Enabled = true;//<---V01.12.01 ATT ADD ◀(8084)
            Tb2_Txt_SEN_KSITENCD.Enabled = true;
            Tb2_Txt_SEN_KSITENNM.Enabled = true;
            Tb2_Lbl_SEN_SITENCD.Enabled = true;
            Tb2_Lbl_SEN_KSITENCD.Enabled = true;
//-- <9999>
            Tb2_Cmb_YOKINSYU.Enabled = true;
            Tb2_Txt_SEN_KOZANO.Enabled = true;
            Tb2_Lbl_SEN_YOKINSYU.Enabled = true;
            Tb2_Lbl_SEN_KOZANO.Enabled = true;
//-- <9999>
        }

        private void Tb2_Txt_SEN_SITENCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;        
        }

        private void Tb2_Txt_SEN_SITENCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_SEN_KSITENCD_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_SEN_KSITENCD_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_SEN_SITENCD_Validating(object sender, CancelEventArgs e)
        {
//-- <2016/03/11 >
//            if (Tb2_Txt_SEN_SITENCD.ExCodeDB == "")
//            {
//                return;
//            }
//
//            string sBranchNm = mcBsLogic.Get_SITENNM(Tb2_Txt_SEN_GINKOCD.ExCode, Tb2_Txt_SEN_SITENCD.ExCode);
//
//            if (sBranchNm == "")
//            {
//                Tb2_Txt_SEN_SITENCD.IsError = true;
//                e.Cancel = true;
//                return;
//            }

            if (Tb2_Txt_SEN_SITENCD.ExCodeDB == "")
            {
                Tb2_Txt_SEN_SITENNM.ClearValue();
                return;
            }

            Tb2_Txt_SEN_SITENNM.Text = mcBsLogic.Get_SITENNM(Tb2_Txt_SEN_GINKOCD.ExCodeDB, Tb2_Txt_SEN_SITENCD.ExCodeDB);

            if (Tb2_Txt_SEN_GINKONM.Text == "")
            {
                Tb2_Txt_SEN_SITENNM.ClearValue();
                Tb2_Txt_SEN_GINKOCD.IsError = true;
                e.Cancel = true;
                return;
            }
//-- <2016/03/11>
        }

        private void Tb2_Txt_SEN_KSITENCD_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_SEN_KSITENCD.ExCodeDB == "")
            {
                Tb2_Txt_SEN_KSITENNM.ClearValue();
                return;
            }

            //Tb2_Txt_SEN_KSITENNM.Text = mcBsLogic.Get_SITENNM(Tb2_Txt_SEN_GINKOCD.ExCode, Tb2_Txt_SEN_KSITENCD.ExCode);
            //if (Tb2_Txt_SEN_KSITENNM.Text == "")
            //{
            //    Tb2_Txt_SEN_KSITENCD.IsError = true;
            //    e.Cancel = true;
            //    return;
            //}
        }

        private void Tb2_Chk_HiFuri_1_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb2_Chk_HiFuri_1.Checked == true)
            {
                Tb2_Txt_HIFURIKOZA_1.Enabled = true;
            }
            else
            {
                Tb2_Txt_HIFURIKOZA_1.Enabled = false;
                Tb2_Txt_HIFURIKOZA_1.ClearValue();
                Tb2_Txt_HIBKCD_1.ClearValue();
                Tb2_Txt_HIBKNM_1.ClearValue();
                Tb2_Txt_HIBRCD_1.ClearValue();
                Tb2_Txt_HIBRNM_1.ClearValue();
                Tb2_Txt_HIYOKN_1.ClearValue();
                Tb2_Txt_HIKOZANO_1.ClearValue();
            }

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Chk_HiFuri_2_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb2_Chk_HiFuri_2.Checked == true)
            {
                Tb2_Txt_HIFURIKOZA_2.Enabled = true;
            }
            else
            {
                Tb2_Txt_HIFURIKOZA_2.Enabled = false;
                Tb2_Txt_HIFURIKOZA_2.ClearValue();
                Tb2_Txt_HIBKCD_2.ClearValue();
                Tb2_Txt_HIBKNM_2.ClearValue();
                Tb2_Txt_HIBRCD_2.ClearValue();
                Tb2_Txt_HIBRNM_2.ClearValue();
                Tb2_Txt_HIYOKN_2.ClearValue();
                Tb2_Txt_HIKOZANO_2.ClearValue();
            }

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Chk_HiFuri_3_CheckedChanged(object sender, EventArgs e)
        {
            if (Tb2_Chk_HiFuri_3.Checked == true)
            {
                Tb2_Txt_HIFURIKOZA_3.Enabled = true;
            }
            else
            {
                Tb2_Txt_HIFURIKOZA_3.Enabled = false;
                Tb2_Txt_HIFURIKOZA_3.ClearValue();
                Tb2_Txt_HIBKCD_3.ClearValue();
                Tb2_Txt_HIBKNM_3.ClearValue();
                Tb2_Txt_HIBRCD_3.ClearValue();
                Tb2_Txt_HIBRNM_3.ClearValue();
                Tb2_Txt_HIYOKN_3.ClearValue();
                Tb2_Txt_HIKOZANO_3.ClearValue();
            }

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Txt_HIFURIKOZA_1_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_HIFURIKOZA_1.ExNumValue == 0)
            {
                Tb2_Txt_HIBKCD_1.ClearValue();
                Tb2_Txt_HIBKNM_1.ClearValue();
                Tb2_Txt_HIBRCD_1.ClearValue();
                Tb2_Txt_HIBRNM_1.ClearValue();
                Tb2_Txt_HIYOKN_1.ClearValue();
                Tb2_Txt_HIKOZANO_1.ClearValue();
                return;
            }

//-- <2016/03/11 >
//            DataTable dt = mcBsLogic.GetOwnBankInfo(Tb2_Txt_HIFURIKOZA_1.ExNumValue);
            DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Tb2_Txt_HIFURIKOZA_1.ExNumValue));
//-- <2016/03/11>
            if (dt.Rows.Count == 0)
            {
                Tb2_Txt_HIFURIKOZA_1.IsError = true;
                e.Cancel = true;
            }
            else
            {
                Tb2_Txt_HIBKCD_1.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                Tb2_Txt_HIBKNM_1.Text = mcBsLogic.Get_BANKNM(Tb2_Txt_HIBKCD_1.Text);
                Tb2_Txt_HIBRCD_1.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                Tb2_Txt_HIBRNM_1.Text = mcBsLogic.Get_SITENNM(Tb2_Txt_HIBKCD_1.Text, Tb2_Txt_HIBRCD_1.Text);
                Tb2_Txt_HIYOKN_1.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                Tb2_Txt_HIKOZANO_1.Text = dt.Rows[0]["KOZANO"].ToString();
            }
        }

        private void Tb2_Txt_HIFURIKOZA_2_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_HIFURIKOZA_2.ExNumValue == 0)
            {
                Tb2_Txt_HIBKCD_2.ClearValue();
                Tb2_Txt_HIBKNM_2.ClearValue();
                Tb2_Txt_HIBRCD_2.ClearValue();
                Tb2_Txt_HIBRNM_2.ClearValue();
                Tb2_Txt_HIYOKN_2.ClearValue();
                Tb2_Txt_HIKOZANO_2.ClearValue();
                return;
            }

//--
//            DataTable dt = mcBsLogic.GetOwnBankInfo(Tb2_Txt_HIFURIKOZA_2.ExNumValue);
            DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Tb2_Txt_HIFURIKOZA_2.ExNumValue));
//--
            if (dt.Rows.Count == 0)
            {
                Tb2_Txt_HIFURIKOZA_2.IsError = true;
                e.Cancel = true;
            }
            else
            {
                Tb2_Txt_HIBKCD_2.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                Tb2_Txt_HIBKNM_2.Text = mcBsLogic.Get_BANKNM(Tb2_Txt_HIBKCD_2.Text);
                Tb2_Txt_HIBRCD_2.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                Tb2_Txt_HIBRNM_2.Text = mcBsLogic.Get_SITENNM(Tb2_Txt_HIBKCD_2.Text, Tb2_Txt_HIBRCD_2.Text);
                Tb2_Txt_HIYOKN_2.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                Tb2_Txt_HIKOZANO_2.Text = dt.Rows[0]["KOZANO"].ToString();
            }
        }

        private void Tb2_Txt_HIFURIKOZA_3_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_HIFURIKOZA_3.ExNumValue == 0)
            {
                Tb2_Txt_HIBKCD_3.ClearValue();
                Tb2_Txt_HIBKNM_3.ClearValue();
                Tb2_Txt_HIBRCD_3.ClearValue();
                Tb2_Txt_HIBRNM_3.ClearValue();
                Tb2_Txt_HIYOKN_3.ClearValue();
                Tb2_Txt_HIKOZANO_3.ClearValue();
                return;
            }

//-- <>
//            DataTable dt = mcBsLogic.GetOwnBankInfo(Tb2_Txt_HIFURIKOZA_3.ExNumValue);
            DataTable dt = mcBsLogic.GetOwnBankInfo(Convert.ToInt32(Tb2_Txt_HIFURIKOZA_3.ExNumValue));
//--
            if (dt.Rows.Count == 0)
            {
                Tb2_Txt_HIFURIKOZA_3.IsError = true;
                e.Cancel = true;
            }
            else
            {
                Tb2_Txt_HIBKCD_3.Text = dt.Rows[0]["OWNBKCOD"].ToString();
                Tb2_Txt_HIBKNM_3.Text = mcBsLogic.Get_BANKNM(Tb2_Txt_HIBKCD_3.Text);
                Tb2_Txt_HIBRCD_3.Text = dt.Rows[0]["OWNBRCOD"].ToString();
                Tb2_Txt_HIBRNM_3.Text = mcBsLogic.Get_SITENNM(Tb2_Txt_HIBKCD_3.Text, Tb2_Txt_HIBRCD_3.Text);
                Tb2_Txt_HIYOKN_3.Text = dt.Rows[0]["YOKNKIND"].ToString() + ":" + mcBsLogic.Get_YokinType_NM(Convert.ToInt32(dt.Rows[0]["YOKNKIND"].ToString()));
                Tb2_Txt_HIKOZANO_3.Text = dt.Rows[0]["KOZANO"].ToString();
            }
        }

        private void Tb2_Txt_HIFURIKOZA_1_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_HIFURIKOZA_1_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_HIFURIKOZA_2_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_HIFURIKOZA_2_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb2_Txt_HIFURIKOZA_3_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb2_Txt_HIFURIKOZA_3_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        private void Tb3_Txt_SHINO_Enter(object sender, EventArgs e)
        {
            FKB.F09_Enabled = true;
            MNU_SEARCH.Enabled = true;
        }

        private void Tb3_Txt_SHINO_Leave(object sender, EventArgs e)
        {
            FKB.F09_Enabled = false;
            MNU_SEARCH.Enabled = false;
        }

        #region コメントアウト
        //private void GAI_F(object sender, EventArgs e)
        //{
        //    if (bEventCancel == true)
        //    {
        //        bEventCancel = false;
        //        return;
        //    }

        //    if (Tb3_Rdo_GAI_F0.Checked == true)
        //    {
        //        if (MessageBox.Show("既に登録されている外貨設定が削除されます。\n削除しますか？"
        //                            , Global.sPrgName
        //                            , MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
        //        {
        //            bEventCancel = true;
        //            Tb3_Rdo_GAI_F1.Checked = true;
        //            return;
        //        }
        //        GAI_F_Kirikae(0);
        //        nDispChgFlg_Main = 1;
        //        //nDispChgFlg_TSHOH = 1;
        //        //nDispChgFlg_FRIGIN = 1;

        //        Btn_REG.Enabled = true;
        //        FKB.F10_Enabled = true;
        //    }
        //    else
        //    {
        //        if (MessageBox.Show("既に登録されている支払条件、及び振込先情報が削除されます。\n削除しますか？"
        //            , Global.sPrgName
        //            , MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
        //        {
        //            bEventCancel = true;
        //            Tb3_Rdo_GAI_F0.Checked = true;
        //            return;
        //        }
        //        GAI_F_Kirikae(1);

        //        string sTRCD = Txt_TRCD.ExCodeDB;
        //        string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");

        //        mcBsLogic.Del_SS_TSHOH_ALL(sTRCD, sHJCD);
        //        mcBsLogic.Del_SS_FRIGIN_ALL(sTRCD, sHJCD);

        //        nDispChgFlg_TSHOH = 0;
        //        nDispChgFlg_FRIGIN = 0;

        //        if (nDispChgFlg_Main == 0)
        //        {
        //            Btn_REG.Enabled = false;
        //            FKB.F10_Enabled = false;
        //        }
        //    }

        //}
        //private void GAI_F_Kirikae(int nFlg)
        //{
        //    if (Global.nSAIMU_F == 0)
        //    {
        //        return;
        //    }

        //    if (bEventCancel == true)
        //    {
        //        bEventCancel = false;
        //        return;
        //    }

        //    TabControl.TabPageCollection tabPages = Tb_Main.TabPages;

        //    if (nFlg == 0)
        //    {
        //        //Global.GAI_SF = "0";                 // 外貨設定：◎送金種類
        //        //Global.GAI_SH = "0";                 // 外貨設定：◎送金支払方法
        //        //Global.GAI_KZID = "";                // 外貨設定：出金口座
        //        //Global.GAI_TF = "";                  // 外貨設定：手数料負担
        //        //Global.ENG_NAME = "";                // 外貨設定：英語表記　受取人名
        //        //Global.ENG_ADDR = "";                // 外貨設定：英語表記　住所
        //        //Global.ENG_KZNO = "";                // 外貨設定：外国向け送金設定　口座番号
        //        //Global.ENG_SWIF = "";                // 外貨設定：外国向け送金設定　SWIFTコード
        //        //Global.ENG_BNKNAM = "";              // 外貨設定：外国向け送金設定　被仕向銀行名
        //        //Global.ENG_BRNNAM = "";              // 外貨設定：外国向け送金設定　被仕向支店名
        //        //Global.ENG_BNKADDR = "";             // 外貨設定：外国向け送金設定　被仕向銀行住所

        //        Tb3_Lbl_Old_New2.Enabled = true;
        //        if (Tb3_Lbl_Old_New2.Text == "")
        //        {
        //            Tb3_Lbl_Old_New2.Text = "【　新規　】";
        //        }

        //        Tb6_Cmb_HEI_CD.SelectedIndex = -1;
        //        Tb6_Rdo_GAI_SF0.Checked = true;
        //        Tb6_Rdo_GAI_SH0.Checked = true;
        //        Tb6_Cmb_GAI_KZID.SelectedIndex = -1;
        //        Tb6_Cmb_GAI_TF.SelectedIndex = -1;

        //        Tb6_Txt_ENG_NAME.ClearValue();
        //        Tb6_Txt_ENG_ADDR.ClearValue();
        //        Tb6_Txt_ENG_KZNO.ClearValue();
        //        Tb6_Txt_ENG_SWIF.ClearValue();
        //        Tb6_Txt_ENG_BNKNAM.ClearValue();
        //        Tb6_Txt_ENG_BRNNAM.ClearValue();
        //        Tb6_Txt_ENG_BNKADDR.ClearValue();

        //        Tb3_Chk_SHO_ID.Enabled = true;
        //        Tb1_Lbl_SHO_ID_V.Enabled = true;
        //        Tb3_Lbl_SHINO_ID.Enabled = true;
        //        //BindNavi2_Selected.Text = "1";
        //        //BindNavi2_Cnt.Text = "/ 1";
        //        Tb3_Txt_BCOD.Enabled = true;
        //        Tb3_Txt_BNAM.Enabled = true;
        //        Tb3_Txt_KCOD.Enabled = true;
        //        Tb3_Txt_KINM.Enabled = true;
        //        Tb3_Txt_SHINO.Enabled = true;
        //        Tb3_Txt_SHINM.Enabled = true;
        //        Tb3_Txt_SHIMEBI.Enabled = true;
        //        Tb3_Txt_ShimeNm.Enabled = true;
        //        Tb3_Txt_SHIHARAIMM.Enabled = true;
        //        Tb3_Txt_SIHARAIDD.Enabled = true;
        //        Tb3_Cmb_HARAI_H.Enabled = true;
        //        Tb3_Txt_SKIJITUMM.Enabled = true;
        //        Tb3_Txt_SKIJITUDD.Enabled = true;
        //        Tb3_Cmb_KIJITU_H.Enabled = true;
        //        Tb3_Txt_SKBNCOD.Enabled = true;
        //        Tb3_Txt_V_YAKUJO.Enabled = true;
        //        Tb3_Txt_YAKUJOA_L.Enabled = true;
        //        Tb3_Txt_YAKUJOA_M.Enabled = true;
        //        Tb3_Txt_YAKUJOB_LH.Enabled = true;
        //        Tb3_Txt_YAKUJOB_H1.Enabled = true;
        //        Tb3_Txt_YAKUJOB_R1.Enabled = true;
        //        Tb3_Txt_YAKUJOB_U1.Enabled = true;
        //        Tb3_Txt_YAKUJOB_H2.Enabled = true;
        //        Tb3_Txt_YAKUJOB_R2.Enabled = true;
        //        Tb3_Txt_YAKUJOB_U2.Enabled = true;
        //        Tb3_Txt_YAKUJOB_H3.Enabled = true;
        //        Tb3_Txt_YAKUJOB_R3.Enabled = true;
        //        Tb3_Txt_YAKUJOB_U3.Enabled = true;
        //        //Tb3_Cmb_HARAI_KBN1.Enabled = true;
        //        //Tb3_Cmb_HARAI_KBN2.Enabled = true;
        //        //Tb3_Cmb_HARAI_KBN3.Enabled = true;
        //        //Tb3_Cmb_HARAI_KBN4.Enabled = true;
        //        Tb1_Lbl_BCOD.Enabled = true;
        //        Tb1_Lbl_KCOD.Enabled = true;
        //        Tb1_Lbl_SHINO.Enabled = true;
        //        Tb1_Lbl_SHIMEBI.Enabled = true;
        //        Tb1_Lbl_HARAI_H.Enabled = true;
        //        Tb1_Lbl_SHIHARAIMM.Enabled = true;
        //        Tb1_Lbl_SIHARAIDD.Enabled = true;
        //        Tb1_Lbl_HOSEI.Enabled = true;
        //        Tb1_Lbl_KIJITU_H.Enabled = true;
        //        Tb1_Lbl_SKIJITUMM.Enabled = true;
        //        Tb1_Lbl_SKIJITUDD.Enabled = true;
        //        Tb1_Lbl_HOSEI_K.Enabled = true;
        //        Tb1_Lbl_SKBNCOD.Enabled = true;
        //        Tb1_Lbl_V_YAKUJO.Enabled = true;
        //        Tb1_Lbl_YAKUJOA_L.Enabled = true;
        //        Tb1_Lbl_YAKUJOA_M.Enabled = true;
        //        Tb1_Lbl_V_YAKUJO_EN.Enabled = true;
        //        Tb1_SEPT2.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_LH.Enabled = true;
        //        Tb1_Lbl_YAKUJOB1.Enabled = true;
        //        Tb1_Lbl_YAKUJOB2.Enabled = true;
        //        Tb1_Lbl_YAKUJOB3.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_R.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_U.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_R1.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_R2.Enabled = true;
        //        Tb1_Lbl_YAKUJOB_R3.Enabled = true;
        //        Tb1_Lbl_HARAI_KBN_H.Enabled = true;
        //        Tb4_Lbl_GIN_ID_V.Text = "1";

        //        tabPages[3].Enabled = true;
        //        tabPages[5].Enabled = false;
        //        Tb_Main.Refresh();
        //    }
        //    else
        //    {
        //        Tb3_Lbl_Old_New2.Enabled = false;
        //        Tb3_Lbl_Old_New2.Text = "";

        //        Tb3_Chk_SHO_ID.Checked = false;
        //        Tb1_Lbl_SHO_ID_V.Text = "1";
        //        BindNavi2_Selected.Text = "1";
        //        BindNavi2_Cnt.Text = "/ 1";
        //        Tb3_BindNavi_First.Enabled = false;
        //        Tb3_BindNavi_Prev.Enabled = false;
        //        Tb3_BindNavi_Next.Enabled = false;
        //        Tb3_BindNavi_Last.Enabled = false;
        //        Tb3_BindNavi_Add.Enabled = false;
        //        Tb3_BindNavi_DEL.Enabled = false;
        //        Tb3_BindNavi_Add.Enabled = false;
        //        Tb3_BindNavi_DEL.Enabled = false;

        //        Tb3_Txt_BCOD.ClearValue();
        //        Tb3_Txt_BNAM.ClearValue();
        //        Tb3_Txt_KCOD.ClearValue();
        //        Tb3_Txt_KINM.ClearValue();
        //        Tb3_Txt_SHINO.ClearValue();
        //        Tb3_Txt_SHINM.ClearValue();
        //        Tb3_Txt_SHIMEBI.ClearValue();
        //        Tb3_Txt_ShimeNm.ClearValue();
        //        Tb3_Txt_SHIHARAIMM.ClearValue();
        //        Tb3_Txt_SIHARAIDD.ClearValue();
        //        Tb3_Cmb_HARAI_H.SelectedIndex = -1;
        //        Tb3_Txt_SKIJITUMM.ClearValue();
        //        Tb3_Txt_SKIJITUDD.ClearValue();
        //        Tb3_Cmb_KIJITU_H.SelectedIndex = -1;
        //        Tb3_Txt_SKBNCOD.ClearValue();
        //        Tb3_Txt_V_YAKUJO.ClearValue();
        //        Tb3_Txt_YAKUJOA_L.ClearValue();
        //        Tb3_Txt_YAKUJOA_M.ClearValue();
        //        Tb3_Txt_YAKUJOB_LH.ClearValue();
        //        Tb3_Txt_YAKUJOB_H1.ClearValue();
        //        Tb3_Txt_YAKUJOB_R1.ClearValue();
        //        Tb3_Txt_YAKUJOB_U1.ClearValue();
        //        Tb3_Txt_YAKUJOB_H2.ClearValue();
        //        Tb3_Txt_YAKUJOB_R2.ClearValue();
        //        Tb3_Txt_YAKUJOB_U2.ClearValue();
        //        Tb3_Txt_YAKUJOB_H3.ClearValue();
        //        Tb3_Txt_YAKUJOB_R3.ClearValue();
        //        Tb3_Txt_YAKUJOB_U3.ClearValue();
        //        Tb3_Cmb_HARAI_KBN1.SelectedIndex = -1;
        //        Tb3_Cmb_HARAI_KBN2.SelectedIndex = -1;
        //        Tb3_Cmb_HARAI_KBN3.SelectedIndex = -1;
        //        Tb3_Cmb_HARAI_KBN4.SelectedIndex = -1;
        //        Tb3_Lbl_HARAI_KBN1.Text = "";
        //        Tb3_Lbl_HARAI_KBN2.Text = "";
        //        Tb3_Lbl_HARAI_KBN3.Text = "";
        //        Tb3_Lbl_HARAI_KBN4.Text = "";

        //        Tb3_Chk_SHO_ID.Enabled = false;
        //        Tb1_Lbl_SHO_ID_V.Enabled = false;
        //        Tb3_Lbl_SHINO_ID.Enabled = false;
        //        //BindNavi2_Selected.Text = "1";
        //        //BindNavi2_Cnt.Text = "/ 1";
        //        Tb3_Txt_BCOD.Enabled = false;
        //        Tb3_Txt_BNAM.Enabled = false;
        //        Tb3_Txt_KCOD.Enabled = false;
        //        Tb3_Txt_KINM.Enabled = false;
        //        Tb3_Txt_SHINO.Enabled = false;
        //        Tb3_Txt_SHINM.Enabled = false;
        //        Tb3_Txt_SHIMEBI.Enabled = false;
        //        Tb3_Txt_ShimeNm.Enabled = false;
        //        Tb3_Txt_SHIHARAIMM.Enabled = false;
        //        Tb3_Txt_SIHARAIDD.Enabled = false;
        //        Tb3_Cmb_HARAI_H.Enabled = false;
        //        Tb3_Txt_SKIJITUMM.Enabled = false;
        //        Tb3_Txt_SKIJITUDD.Enabled = false;
        //        Tb3_Cmb_KIJITU_H.Enabled = false;
        //        Tb3_Txt_SKBNCOD.Enabled = false;
        //        Tb3_Txt_V_YAKUJO.Enabled = false;
        //        Tb3_Txt_YAKUJOA_L.Enabled = false;
        //        Tb3_Txt_YAKUJOA_M.Enabled = false;
        //        Tb3_Txt_YAKUJOB_LH.Enabled = false;
        //        Tb3_Txt_YAKUJOB_H1.Enabled = false;
        //        Tb3_Txt_YAKUJOB_R1.Enabled = false;
        //        Tb3_Txt_YAKUJOB_U1.Enabled = false;
        //        Tb3_Txt_YAKUJOB_H2.Enabled = false;
        //        Tb3_Txt_YAKUJOB_R2.Enabled = false;
        //        Tb3_Txt_YAKUJOB_U2.Enabled = false;
        //        Tb3_Txt_YAKUJOB_H3.Enabled = false;
        //        Tb3_Txt_YAKUJOB_R3.Enabled = false;
        //        Tb3_Txt_YAKUJOB_U3.Enabled = false;
        //        Tb3_Cmb_HARAI_KBN1.Enabled = false;
        //        Tb3_Cmb_HARAI_KBN2.Enabled = false;
        //        Tb3_Cmb_HARAI_KBN3.Enabled = false;
        //        Tb3_Cmb_HARAI_KBN4.Enabled = false;
        //        Tb1_Lbl_BCOD.Enabled = false;
        //        Tb1_Lbl_KCOD.Enabled = false;
        //        Tb1_Lbl_SHINO.Enabled = false;
        //        Tb1_Lbl_SHIMEBI.Enabled = false;
        //        Tb1_Lbl_HARAI_H.Enabled = false;
        //        Tb1_Lbl_SHIHARAIMM.Enabled = false;
        //        Tb1_Lbl_SIHARAIDD.Enabled = false;
        //        Tb1_Lbl_HOSEI.Enabled = false;
        //        Tb1_Lbl_KIJITU_H.Enabled = false;
        //        Tb1_Lbl_SKIJITUMM.Enabled = false;
        //        Tb1_Lbl_SKIJITUDD.Enabled = false;
        //        Tb1_Lbl_HOSEI_K.Enabled = false;
        //        Tb1_Lbl_SKBNCOD.Enabled = false;
        //        Tb1_Lbl_V_YAKUJO.Enabled = false;
        //        Tb1_Lbl_YAKUJOA_L.Enabled = false;
        //        Tb1_Lbl_YAKUJOA_M.Enabled = false;
        //        Tb1_Lbl_V_YAKUJO_EN.Enabled = false;
        //        Tb1_SEPT2.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_LH.Enabled = false;
        //        Tb1_Lbl_YAKUJOB1.Enabled = false;
        //        Tb1_Lbl_YAKUJOB2.Enabled = false;
        //        Tb1_Lbl_YAKUJOB3.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_R.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_U.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_R1.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_R2.Enabled = false;
        //        Tb1_Lbl_YAKUJOB_R3.Enabled = false;
        //        Tb1_Lbl_HARAI_KBN_H.Enabled = false;

        //        Tb4_Chk_GIN_ID.Checked = false;
        //        Tb4_Lbl_GIN_ID_V.Text = "1";

        //        Tb4_BindNavi_Selected.Text = "1";
        //        Tb4_BindNavi_Cnt.Text = "/ 1";

        //        Tb4_Chk_FDEF.Checked = false;
        //        Tb4_Chk_DDEF.Checked = false;
        //        Tb4_Lbl_GIN_ID_V.Text = "";
        //        Tb4_Lbl_Old_New3.Text = "";
        //        BindNavi1.Enabled = true;
        //        {
        //            int count;
        //            mcBsLogic.Cnt_TRCD(out count);
        //            BindNavi1_First.Enabled = (count > 0);
        //            BindNavi1_Prev.Enabled = (count > 0);
        //        }
        //        //BindNavi1_Next.Enabled = false;
        //        //BindNavi1_End.Enabled = false;
        //        Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";
        //        Tb4_Cmb_TESUU.Text = "1:自社負担";
        //        Tb4_Cmb_SOUKIN.Text = "7:電信";
        //        Tb4_Txt_BANK_CD.ClearValue();
        //        Tb4_Txt_BANK_NM.ClearValue();
        //        Tb4_Txt_SITEN_ID.ClearValue();
        //        Tb4_Txt_SITEN_NM.ClearValue();
        //        Tb4_Cmb_YOKIN_TYP.SelectedIndex = -1;
        //        Tb4_Txt_KOUZA.ClearValue();
        //        Tb4_Txt_MEIGI.ClearValue();
        //        Tb4_Txt_MEIGIK.ClearValue();
        //        Tb4_Cmb_FTESUID.SelectedIndex = -1;
        //        Tb4_Cmb_TESUU.SelectedIndex = -1;
        //        Tb4_Cmb_SOUKIN.SelectedIndex = -1;
        //        Tb4_Txt_GENDO.ClearValue();
        //        Tb2_Chk_DTESUSW.Checked = false;
        //        Tb2_Cmb_DTESU.SelectedIndex = -1;

        //        tabPages[3].Enabled = false;
        //        tabPages[5].Enabled = true;
        //        Tb_Main.Refresh();
        //    }
        //}
        #endregion
        private void GAI_F(object sender, EventArgs e)
        {
            if (bEventCancel == true)
            {
                bEventCancel = false;
                return;
            }
            if (Tb3_Rdo_GAI_F0.Checked == true)                                                                         // 国内取引
            {
                //---------------------------------------------
                // 支払依頼データが存在すれば、変更不可
                if (Txt_TRCD.ExCodeDB != "" && Lbl_Old_New1.Text == "【　変更　】")
                {
                    string sDelTarget = (Txt_HJCD.Text != "" ? Txt_TRCD.ExCodeDB + "-" + Txt_HJCD.Text : Txt_TRCD.ExCodeDB);

                    string hedmsg = "";
                    if (mcBsLogic.Chk_SS_SHDATA(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") == true)
                    {
                        hedmsg = "取引先コード：" + sDelTarget + "の支払依頼データがあります。\n変更できません。";
                        MessageBox.Show(hedmsg, Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                // 登録されている外貨設定があれば表示する
                if (Tb6_Cmb_GAI_KZID.SelectedIndex != -1)
                {
                    if (MessageBox.Show("既に登録されている外貨設定が削除されます。\n削除しますか？"
                                        , "削除確認"
                                        , MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                    {
                        bEventCancel = true;


                        //--                        bEventCancel = false;


                        Tb3_Rdo_GAI_F1.Checked = true;
                        //-- <9999>
                        Tb3_Rdo_GAI_F1.Select();
                        //-- <9999>
                        return;
                    }

                    bKOKUKAI = true;

                    bHenkou = true;

                    GAI_F_Kirikae(0);
                    nDispChgFlg_Main = 1;
                    //nDispChgFlg_TSHOH = 1;
                    //nDispChgFlg_FRIGIN = 1;

                    Btn_REG.Enabled = true;
                    FKB.F10_Enabled = true;
                    //-- <2016/03/09 F06 Enabled=false>
                    FKB.F06_Enabled = false;
                    MNU_DELETE.Enabled = false;
                    //-- <2016/03/09>

                }
                else
                {

                    bKOKUKAI = true;

                    GAI_F_Kirikae(0);
                    nDispChgFlg_Main = 1;

                    Btn_REG.Enabled = true;
                    FKB.F10_Enabled = true;
                    //-- <2016/03/09 F06 Enabled=false>
                    FKB.F06_Enabled = false;
                    MNU_DELETE.Enabled = false;
                    //-- <2016/03/09>
                }
                bEventCancel = false;

                Global.GAI_F = "0";
            }
            else
            {
                //---------------------------------------------
                // 支払依頼データが存在すれば、変更不可
                if (Txt_TRCD.ExCodeDB != "" && Lbl_Old_New1.Text == "【　変更　】")
                {
                    string sDelTarget = (Txt_HJCD.Text != "" ? Txt_TRCD.ExCodeDB + "-" + Txt_HJCD.Text : Txt_TRCD.ExCodeDB);

                    string hedmsg = "";
                    if (mcBsLogic.Chk_SS_SHDATA(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0") == true)
                    {
                        hedmsg = "取引先コード：" + sDelTarget + "の支払依頼データがあります。\n変更できません。";
                        MessageBox.Show(hedmsg, Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                if (Tb3_Txt_SHIMEBI.Text != "")
                {

                    if (MessageBox.Show("既に登録されている支払条件、及び振込先情報が削除されます。\n削除しますか？"
                            , "削除確認"
                            , MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                    {

                        bEventCancel = true;

                        //                        bEventCancel = false;


                        Tb3_Rdo_GAI_F0.Checked = true;
                        //-- <9999>
                        Tb3_Rdo_GAI_F0.Select();
                        //-- <9999>
                        return;
                    }

                    bKOKUKAI = true;

                    bHenkou = true;

                    GAI_F_Kirikae(1);

                    string sTRCD = Txt_TRCD.ExCodeDB;
                    string sHJCD = (Txt_HJCD.Text != "" ? Txt_HJCD.Text : "0");

                    //---> V01.14.01 HWPO DELETE ▼(8510)
                    //mcBsLogic.Del_SS_TSHOH_ALL(sTRCD, sHJCD);
                    //mcBsLogic.Del_SS_FRIGIN_ALL(sTRCD, sHJCD);

                    //nDispChgFlg_TSHOH = 0;
                    //nDispChgFlg_FRIGIN = 0;
                    //<--- V01.14.01 HWPO DELETE ▲(8510)

                    //                    if (nDispChgFlg_Main == 0)
                    //                    {
                    //                        Btn_REG.Enabled = false;
                    //                        FKB.F10_Enabled = false;
                    //                    }
                    nDispChgFlg_Main = 1;
                    Flg_Tsh_Fri = true;//<--- V01.14.01 HWPO ADD ◀(8510)


                }

                bKOKUKAI = true;

                GAI_F_Kirikae(1);

                //---> V01.14.01 HWPO DELETE ▼(8510)
                //nDispChgFlg_TSHOH = 0;
                //nDispChgFlg_FRIGIN = 0;
                //<--- V01.14.01 HWPO DELETE ▲(8510)

                if (nDispChgFlg_Main == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
                bEventCancel = false;

                Global.GAI_F = "1";

                Chk_SAIMU_FLG.Checked = false;
            }

            Set_Enabled_Cbo_SAIMU(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
            Set_Enabled_Chk_SAIMU_FLG(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
            //--->V01.12.01 ATT ADD ▼ (7063)
            Tb3_Txt_KCOD.ClearValue();
            Tb3_Txt_KINM.ClearValue();
            Tb5_Txt_SKCOD.ClearValue();
            Tb5_Txt_SKINM.ClearValue();
            //<---V01.12.01 ATT ADD ▲ (7063)
        }
        private void GAI_F_Kirikae(int nFlg)
        {
            if (Global.nSAIMU_F == 0)
            {
                return;
            }

            if (bEventCancel == true)
            {
                bEventCancel = false;
                return;
            }

            TabControl.TabPageCollection tabPages = Tb_Main.TabPages;

            if (nFlg == 0)                             // 国内取引
            {
                //Global.GAI_SF = "0";                 // 外貨設定：◎送金種類
                //Global.GAI_SH = "0";                 // 外貨設定：◎送金支払方法
                //Global.GAI_KZID = "";                // 外貨設定：出金口座
                //Global.GAI_TF = "";                  // 外貨設定：手数料負担
                //Global.ENG_NAME = "";                // 外貨設定：英語表記　受取人名
                //Global.ENG_ADDR = "";                // 外貨設定：英語表記　住所
                //Global.ENG_KZNO = "";                // 外貨設定：外国向け送金設定　口座番号
                //Global.ENG_SWIF = "";                // 外貨設定：外国向け送金設定　SWIFTコード
                //Global.ENG_BNKNAM = "";              // 外貨設定：外国向け送金設定　被仕向銀行名
                //Global.ENG_BRNNAM = "";              // 外貨設定：外国向け送金設定　被仕向支店名
                //Global.ENG_BNKADDR = "";             // 外貨設定：外国向け送金設定　被仕向銀行住所

                Tb3_Lbl_Old_New2.Enabled = true;
                if (Tb3_Lbl_Old_New2.Text == "")
                {
                    Tb3_Lbl_Old_New2.Text = "【　新規　】";
                }

                Tb6_Cmb_HEI_CD.SelectedIndex = -1;
                Tb6_Rdo_GAI_SF0.Checked = true;
                Tb6_Rdo_GAI_SH0.Checked = true;
                Tb6_Cmb_GAI_KZID.SelectedIndex = -1;
//-- <2016/03/08 非選択項目>
//                Tb6_Cmb_GAI_TF.SelectedIndex = -1;
                Tb6_Cmb_GAI_TF.SelectedIndex = 0;
//-- <2016/03/08>

                Tb6_Txt_ENG_NAME.ClearValue();
                Tb6_Txt_ENG_ADDR.ClearValue();
                Tb6_Txt_ENG_KZNO.ClearValue();
                Tb6_Txt_ENG_SWIF.ClearValue();
                Tb6_Txt_ENG_BNKNAM.ClearValue();
                Tb6_Txt_ENG_BRNNAM.ClearValue();
                Tb6_Txt_ENG_BNKADDR.ClearValue();

//-- <2016/03/10 表示のみに修正>
//                Tb3_Chk_SHO_ID.Enabled = true;
//-- <2016/03/10>
                Tb1_Lbl_SHO_ID_V.Enabled = true;
                Tb3_Lbl_SHINO_ID.Enabled = true;
                //BindNavi2_Selected.Text = "1";
                //BindNavi2_Cnt.Text = "/ 1";
                Tb3_Txt_BCOD.Enabled = true;
                Tb3_Txt_BNAM.Enabled = true;
                Tb3_Txt_KCOD.Enabled = true;
                Tb3_Txt_KINM.Enabled = true;
                Tb3_Txt_SHINO.Enabled = true;
                Tb3_Txt_SHINM.Enabled = true;
                Tb3_Txt_SHIMEBI.Enabled = true;
                Tb3_Txt_ShimeNm.Enabled = true;
                Tb3_Txt_SHIHARAIMM.Enabled = true;
                Tb3_Txt_SIHARAIDD.Enabled = true;
                Tb3_Cmb_HARAI_H.Enabled = true;
                Tb3_Txt_SKIJITUMM.Enabled = true;
                Tb3_Txt_SKIJITUDD.Enabled = true;
                Tb3_Cmb_KIJITU_H.Enabled = true;
                Tb3_Txt_SKBNCOD.Enabled = true;
                Tb3_Txt_V_YAKUJO.Enabled = true;
                Tb3_Txt_YAKUJOA_L.Enabled = true;
                Tb3_Txt_YAKUJOA_M.Enabled = true;
                Tb3_Txt_YAKUJOB_LH.Enabled = true;
                Tb3_Txt_YAKUJOB_H1.Enabled = true;
                Tb3_Txt_YAKUJOB_R1.Enabled = true;
                Tb3_Txt_YAKUJOB_U1.Enabled = true;
                Tb3_Txt_YAKUJOB_H2.Enabled = true;
                Tb3_Txt_YAKUJOB_R2.Enabled = true;
                Tb3_Txt_YAKUJOB_U2.Enabled = true;
                Tb3_Txt_YAKUJOB_H3.Enabled = true;
                Tb3_Txt_YAKUJOB_R3.Enabled = true;
                Tb3_Txt_YAKUJOB_U3.Enabled = true;
                //Tb3_Cmb_HARAI_KBN1.Enabled = true;
                //Tb3_Cmb_HARAI_KBN2.Enabled = true;
                //Tb3_Cmb_HARAI_KBN3.Enabled = true;
                //Tb3_Cmb_HARAI_KBN4.Enabled = true;
//-- 
                if (bKOKUKAI)
                {
                    Tb3_Lbl_Old_New2.Text = "【　新規　】";
                    Tb1_Lbl_SHO_ID_V.Text = "1";
                    BindNavi2_Selected.Text = "1";
                    BindNavi2_Cnt.Text = "/ " + "1";
                    Tb3_Txt_BCOD.ExCodeDB = "";
                    Tb3_Txt_BNAM.Text = "";
                    Tb3_Txt_KCOD.ExCodeDB = "";
                    Tb3_Txt_KINM.Text = "";
                    Tb3_Txt_SHINO.Text = "";
                    Tb3_Txt_SHINM.Text = "";
                    Tb3_Cmb_HARAI_H.SelectedIndex = -1;
                    Tb3_Cmb_KIJITU_H.SelectedIndex = -1;
                    //取引先に該当データがない場合は自社支払方法を検索しない為、
                    //ここで初期化
                    Tb3_Txt_SHIMEBI.Text = "";
                    Tb3_Txt_SHIHARAIMM.Text = "";
                    Tb3_Txt_SIHARAIDD.Text = "";
                    Tb3_Txt_SKIJITUMM.Text = "";
                    Tb3_Txt_SKIJITUDD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_SKBNCOD.Text = "";
                    Tb3_Txt_V_YAKUJO.ExNumValue = 0;
                    Tb3_Txt_YAKUJOA_L.Text = "";
                    Tb3_Txt_YAKUJOA_M.Text = "";
                    Tb3_Txt_YAKUJOB_LH.Text = "";
                    Tb3_Txt_YAKUJOB_H1.Text = "";
                    Tb3_Txt_YAKUJOB_R1.Text = "";
                    Tb3_Txt_YAKUJOB_U1.Text = "";
                    Tb3_Txt_YAKUJOB_H2.Text = "";
                    Tb3_Txt_YAKUJOB_R2.Text = "";
                    Tb3_Txt_YAKUJOB_U2.Text = "";
                    Tb3_Txt_YAKUJOB_H3.Text = "";
                    Tb3_Txt_YAKUJOB_R3.Text = "";
                    Tb3_Txt_YAKUJOB_U3.Text = "";
                }
//-- <>
                Tb1_Lbl_BCOD.Enabled = true;
                Tb1_Lbl_KCOD.Enabled = true;
                Tb1_Lbl_SHINO.Enabled = true;
                Tb1_Lbl_SHIMEBI.Enabled = true;
                Tb1_Lbl_HARAI_H.Enabled = true;
                Tb1_Lbl_SHIHARAIMM.Enabled = true;
                Tb1_Lbl_SIHARAIDD.Enabled = true;
                Tb1_Lbl_HOSEI.Enabled = true;
                Tb1_Lbl_KIJITU_H.Enabled = true;
                Tb1_Lbl_SKIJITUMM.Enabled = true;
                Tb1_Lbl_SKIJITUDD.Enabled = true;
                Tb1_Lbl_HOSEI_K.Enabled = true;
                Tb1_Lbl_SKBNCOD.Enabled = true;
                Tb1_Lbl_V_YAKUJO.Enabled = true;
                Tb1_Lbl_YAKUJOA_L.Enabled = true;
                Tb1_Lbl_YAKUJOA_M.Enabled = true;
                Tb1_Lbl_V_YAKUJO_EN.Enabled = true;
                Tb1_SEPT2.Enabled = true;
                Tb1_Lbl_YAKUJOB_LH.Enabled = true;
                Tb1_Lbl_YAKUJOB1.Enabled = true;
                Tb1_Lbl_YAKUJOB2.Enabled = true;
                Tb1_Lbl_YAKUJOB3.Enabled = true;
                Tb1_Lbl_YAKUJOB_R.Enabled = true;
                Tb1_Lbl_YAKUJOB_U.Enabled = true;
                Tb1_Lbl_YAKUJOB_R1.Enabled = true;
                Tb1_Lbl_YAKUJOB_R2.Enabled = true;
                Tb1_Lbl_YAKUJOB_R3.Enabled = true;
                Tb1_Lbl_HARAI_KBN_H.Enabled = true;
//                Tb4_Lbl_GIN_ID_V.Text = "1";

//-- <2016/03/13 初期値>
                if (bKOKUKAI)
                {
                    Tb4_Lbl_Old_New3.Text = "【　新規　】";                                 // タブ４　新規・修正
                    Tb4_BindNavi_Selected.Text = "1";                                       // ナビゲーション
                    Tb4_BindNavi_Cnt.Text = "/ 1";                                          // ナビゲーション
                    Tb4_Lbl_GIN_ID_V.Text = "1";                                            // 振込先銀行IDカウンター
                    Tb4_Txt_BANK_CD.Text = "";                                              // 銀行コード
                    Tb4_Txt_BANK_NM.Text = "";                                              // 銀行名称
                    Tb4_Txt_SITEN_ID.Text = "";                                             // 銀行支店コード
                    Tb4_Txt_SITEN_NM.Text = "";                                             // 銀行支店名称
                    Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";                                  // 預金種別
                    Tb4_Txt_KOUZA.ClearValue(); ;                                           // 口座番号
                    Tb4_Txt_MEIGI.Text = "";                                                // 名義人名
                    Tb4_Txt_MEIGIK.Text = "";                                               // 名義人カナ
                    Tb4_Cmb_TESUU.Text = "1:自社負担";                                      // 手数料負担
                    Tb4_Cmb_SOUKIN.Text = "7:電信";                                         // 送金区分
                    Tb4_Txt_GENDO.ClearValue();                                             // 負担限度額
                    Tb4_Chk_FDEF.Checked = true;                                            // 初期値
                    Tb4_Chk_FDEF.Enabled = false;
                    Tb4_Cmb_FTESUID.SelectedValue = -1;                                     // 手数料IDコンボ
                    Tb4_Chk_DDEF.Checked = false;                                           // でんさい代表口座
                    Tb2_Chk_DTESUSW.Checked = false;                                        // でんさい手数料設定
                    Tb2_Cmb_DTESU.SelectedValue = -1;                                       // でんさい手数料負担コンボ
                }
                    bKOKUKAI = false;

//-- <2016/03/13>
//-- <2016/03/14>                
                    // 状態
                    Tb5_Chk_NAYOSE.Enabled = true;
                    Tb5_Chk_F_SETUIN.Enabled = true;

                    Tb5_Grp_TEGATA.Enabled = true;
                    Tb5_Grp_OTHER.Enabled = true;
                    Tb5_Grp_FAC.Enabled = true;
//-- <2016/03/23>
//                    Tb4_Grp_GENSEN.Enabled = true;                   
//                    Tb4_Grp_KJ.Enabled = true;
                if (bHORYUNull && bKOUJYONull)                                              // 控除関連　保留及び控除の支払区分候補が無ければ控除関連はグレーアウト
                {
                    Tb4_Grp_KJ.Enabled = false;
                }
                else if (bHORYUNull && Global.HRKBN != "" && Global.HORYU == "1")
                { Tb4_Grp_KJ.Enabled = false; }
                else if (bKOUJYONull && Global.HRKBN != ""&& Global.HORYU == "2")
                { Tb4_Grp_KJ.Enabled = false; }
                else if (bHORYUNull && Global.HRKBN == "")
                { Tb5_Rdo_HORYU1.Enabled = false; }
                else if (bKOUJYONull && Global.HRKBN == "")
                { Tb5_Rdo_HORYU2.Enabled = false; }
                else 
                {
                    //2018/11/02 ICS.吉岡 ▼(SIAS-9898)＜マスタ権限を｢参照のみ｣にしているユーザーの場合でもその他情報タブが更新できます。＞
                    //Tb4_Grp_KJ.Enabled = true;
                    Tb4_Grp_KJ.Enabled = (Global.cUsrSec.nMFLG < 2 ? false : true);
                    //2018/11/02 ICS.吉岡 ▲(SIAS-9898)＜マスタ権限を｢参照のみ｣にしているユーザーの場合でもその他情報タブが更新できます。＞

                }
                
                if (bGENNull)                                                               // 源泉税計算グループ　支払区分候補が無ければグレーアウト
                {
                    Tb4_Grp_GENSEN.Enabled = false;
                }
                else { Tb4_Grp_GENSEN.Enabled = true; }
//-- <2016/03/23>
//-- <2016/03/14>

                tabPages[3].Enabled = true;
                tabPages[5].Enabled = false;
                // --->V01.15.01 HWY ADD ▼(6490)
                this.Txt_TRCD.Validating -= new System.ComponentModel.CancelEventHandler(this.Txt_TRCD_Validating);
                Tb_Main.SelectedIndex = Tb_Main.SelectedIndex == 5  && Tb3_Rdo_GAI_F0.Checked ? 0 : Tb_Main.SelectedIndex; 
                this.Txt_TRCD.Validating += new System.ComponentModel.CancelEventHandler(this.Txt_TRCD_Validating);
                // <---V01.15.01 HWY ADD ▲(6490)
                Tb_Main.Refresh();
            }
            else　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　// 海外取引
            {
                Tb3_Lbl_Old_New2.Enabled = false;
                Tb3_Lbl_Old_New2.Text = "";

                Tb3_Chk_SHO_ID.Checked = false;
                Tb1_Lbl_SHO_ID_V.Text = "1";
                BindNavi2_Selected.Text = "1";
                BindNavi2_Cnt.Text = "/ 1";
                Tb3_BindNavi_First.Enabled = false;
                Tb3_BindNavi_Prev.Enabled = false;
                Tb3_BindNavi_Next.Enabled = false;
                Tb3_BindNavi_Last.Enabled = false;
                Tb3_BindNavi_Add.Enabled = false;
                Tb3_BindNavi_DEL.Enabled = false;
                Tb3_BindNavi_Add.Enabled = false;
                Tb3_BindNavi_DEL.Enabled = false;

                Tb3_Txt_BCOD.ClearValue();
                Tb3_Txt_BNAM.ClearValue();
                Tb3_Txt_KCOD.ClearValue();
                Tb3_Txt_KINM.ClearValue();
                Tb3_Txt_SHINO.ClearValue();
                Tb3_Txt_SHINM.ClearValue();
                Tb3_Txt_SHIMEBI.ClearValue();
                Tb3_Txt_ShimeNm.ClearValue();
                Tb3_Txt_SHIHARAIMM.ClearValue();
                Tb3_Txt_SIHARAIDD.ClearValue();
                Tb3_Cmb_HARAI_H.SelectedIndex = -1;
                Tb3_Txt_SKIJITUMM.ClearValue();
                Tb3_Txt_SKIJITUDD.ClearValue();
                Tb3_Cmb_KIJITU_H.SelectedIndex = -1;
                Tb3_Txt_SKBNCOD.ClearValue();
                Tb3_Txt_V_YAKUJO.ClearValue();
                Tb3_Txt_YAKUJOA_L.ClearValue();
                Tb3_Txt_YAKUJOA_M.ClearValue();
                Tb3_Txt_YAKUJOB_LH.ClearValue();
                Tb3_Txt_YAKUJOB_H1.ClearValue();
                Tb3_Txt_YAKUJOB_R1.ClearValue();
                Tb3_Txt_YAKUJOB_U1.ClearValue();
                Tb3_Txt_YAKUJOB_H2.ClearValue();
                Tb3_Txt_YAKUJOB_R2.ClearValue();
                Tb3_Txt_YAKUJOB_U2.ClearValue();
                Tb3_Txt_YAKUJOB_H3.ClearValue();
                Tb3_Txt_YAKUJOB_R3.ClearValue();
                Tb3_Txt_YAKUJOB_U3.ClearValue();
                Tb3_Cmb_HARAI_KBN1.SelectedIndex = -1;
                Tb3_Cmb_HARAI_KBN2.SelectedIndex = -1;
                Tb3_Cmb_HARAI_KBN3.SelectedIndex = -1;
                Tb3_Cmb_HARAI_KBN4.SelectedIndex = -1;
                Tb3_Lbl_HARAI_KBN1.Text = "";
                Tb3_Lbl_HARAI_KBN2.Text = "";
                Tb3_Lbl_HARAI_KBN3.Text = "";
                Tb3_Lbl_HARAI_KBN4.Text = "";

                Tb3_Chk_SHO_ID.Enabled = false;
                Tb1_Lbl_SHO_ID_V.Enabled = false;
                Tb3_Lbl_SHINO_ID.Enabled = false;
                //BindNavi2_Selected.Text = "1";
                //BindNavi2_Cnt.Text = "/ 1";
                Tb3_Txt_BCOD.Enabled = false;
                Tb3_Txt_BNAM.Enabled = false;
                Tb3_Txt_KCOD.Enabled = false;
                Tb3_Txt_KINM.Enabled = false;
                Tb3_Txt_SHINO.Enabled = false;
                Tb3_Txt_SHINM.Enabled = false;
                Tb3_Txt_SHIMEBI.Enabled = false;
                Tb3_Txt_ShimeNm.Enabled = false;
                Tb3_Txt_SHIHARAIMM.Enabled = false;
                Tb3_Txt_SIHARAIDD.Enabled = false;
                Tb3_Cmb_HARAI_H.Enabled = false;
                Tb3_Txt_SKIJITUMM.Enabled = false;
                Tb3_Txt_SKIJITUDD.Enabled = false;
                Tb3_Cmb_KIJITU_H.Enabled = false;
                Tb3_Txt_SKBNCOD.Enabled = false;
                Tb3_Txt_V_YAKUJO.Enabled = false;
                Tb3_Txt_YAKUJOA_L.Enabled = false;
                Tb3_Txt_YAKUJOA_M.Enabled = false;
                Tb3_Txt_YAKUJOB_LH.Enabled = false;
                Tb3_Txt_YAKUJOB_H1.Enabled = false;
                Tb3_Txt_YAKUJOB_R1.Enabled = false;
                Tb3_Txt_YAKUJOB_U1.Enabled = false;
                Tb3_Txt_YAKUJOB_H2.Enabled = false;
                Tb3_Txt_YAKUJOB_R2.Enabled = false;
                Tb3_Txt_YAKUJOB_U2.Enabled = false;
                Tb3_Txt_YAKUJOB_H3.Enabled = false;
                Tb3_Txt_YAKUJOB_R3.Enabled = false;
                Tb3_Txt_YAKUJOB_U3.Enabled = false;
                Tb3_Cmb_HARAI_KBN1.Enabled = false;
                Tb3_Cmb_HARAI_KBN2.Enabled = false;
                Tb3_Cmb_HARAI_KBN3.Enabled = false;
                Tb3_Cmb_HARAI_KBN4.Enabled = false;
                Tb1_Lbl_BCOD.Enabled = false;
                Tb1_Lbl_KCOD.Enabled = false;
                Tb1_Lbl_SHINO.Enabled = false;
                Tb1_Lbl_SHIMEBI.Enabled = false;
                Tb1_Lbl_HARAI_H.Enabled = false;
                Tb1_Lbl_SHIHARAIMM.Enabled = false;
                Tb1_Lbl_SIHARAIDD.Enabled = false;
                Tb1_Lbl_HOSEI.Enabled = false;
                Tb1_Lbl_KIJITU_H.Enabled = false;
                Tb1_Lbl_SKIJITUMM.Enabled = false;
                Tb1_Lbl_SKIJITUDD.Enabled = false;
                Tb1_Lbl_HOSEI_K.Enabled = false;
                Tb1_Lbl_SKBNCOD.Enabled = false;
                Tb1_Lbl_V_YAKUJO.Enabled = false;
                Tb1_Lbl_YAKUJOA_L.Enabled = false;
                Tb1_Lbl_YAKUJOA_M.Enabled = false;
                Tb1_Lbl_V_YAKUJO_EN.Enabled = false;
                Tb1_SEPT2.Enabled = false;
                Tb1_Lbl_YAKUJOB_LH.Enabled = false;
                Tb1_Lbl_YAKUJOB1.Enabled = false;
                Tb1_Lbl_YAKUJOB2.Enabled = false;
                Tb1_Lbl_YAKUJOB3.Enabled = false;
                Tb1_Lbl_YAKUJOB_R.Enabled = false;
                Tb1_Lbl_YAKUJOB_U.Enabled = false;
                Tb1_Lbl_YAKUJOB_R1.Enabled = false;
                Tb1_Lbl_YAKUJOB_R2.Enabled = false;
                Tb1_Lbl_YAKUJOB_R3.Enabled = false;
                Tb1_Lbl_HARAI_KBN_H.Enabled = false;

                Tb4_Chk_GIN_ID.Checked = false;
                Tb4_Lbl_GIN_ID_V.Text = "1";

                Tb4_BindNavi_Selected.Text = "1";
                Tb4_BindNavi_Cnt.Text = "/ 1";

//-- <2016/03/24>
//                Tb4_Chk_FDEF.Checked = false;
                Tb4_Chk_FDEF.Checked = true;
//-- <2016/03/24>
                Tb4_Chk_DDEF.Checked = false;
//                Tb4_Lbl_GIN_ID_V.Text = "";
                Tb4_Lbl_Old_New3.Text = "";
                BindNavi1.Enabled = true;
                {
                    int count;
                    mcBsLogic.Cnt_TRCD(out count);
                    BindNavi1_First.Enabled = (count > 0);
                    BindNavi1_Prev.Enabled = (count > 0);
                }
                //BindNavi1_Next.Enabled = false;
                //BindNavi1_End.Enabled = false;
                Tb4_Cmb_YOKIN_TYP.Text = "1:普通預金";
                Tb4_Cmb_TESUU.Text = "1:自社負担";
                Tb4_Cmb_SOUKIN.Text = "7:電信";
                Tb4_Txt_BANK_CD.ClearValue();
                Tb4_Txt_BANK_NM.ClearValue();
                Tb4_Txt_SITEN_ID.ClearValue();
                Tb4_Txt_SITEN_NM.ClearValue();
                Tb4_Cmb_YOKIN_TYP.SelectedIndex = -1;
                Tb4_Txt_KOUZA.ClearValue();
                Tb4_Txt_MEIGI.ClearValue();
                Tb4_Txt_MEIGIK.ClearValue();
                Tb4_Cmb_FTESUID.SelectedIndex = -1;
                Tb4_Cmb_TESUU.SelectedIndex = -1;
                Tb4_Cmb_SOUKIN.SelectedIndex = -1;
                Tb4_Txt_GENDO.ClearValue();
                Tb2_Chk_DTESUSW.Checked = false;
                Tb2_Cmb_DTESU.SelectedIndex = -1;

//-- <2016/03/14 その他情報>
                // 値
                Tb5_Chk_NAYOSE.Checked = false;                                             // 名寄せ
                Tb5_Chk_F_SETUIN.Checked = false;                                           // 節印

                Tb5_Chk_GENSEN.Checked = false;
                Tb5_Chk_OUTPUT.Checked = false;
                Tb5_Cmb_GOU.Text = "";
                Tb5_Cmb_GGKBN.Text = "";
                Tb5_Cmb_GSKUBN.Text = "";

                Tb5_Rdo_HORYU0.Checked = true;
                Tb5_Txt_HR_KIJYUN.Text = "";
                Tb5_Cmb_HORYU_F.Text = "";
                Tb5_Txt_HOVAL.Text = "100.000";
                Tb5_Txt_HRORYUGAKU.Text = "";
                Tb5_Cmb_HRKBN.Text = "";
                // 状態
                Tb5_Chk_NAYOSE.Enabled = false;
                Tb5_Chk_F_SETUIN.Enabled = false;

                Tb5_Grp_TEGATA.Enabled = false;
                Tb5_Grp_OTHER.Enabled = false;
                Tb5_Grp_FAC.Enabled = false;
                Tb4_Grp_GENSEN.Enabled = false;
                Tb4_Grp_KJ.Enabled = false;
//-- <2016/03/14>

                tabPages[3].Enabled = false;
                tabPages[5].Enabled = true;
                // --->V01.15.01 HWY ADD ▼(6490)
                this.Txt_TRCD.Validating -= new System.ComponentModel.CancelEventHandler(this.Txt_TRCD_Validating);
                Tb_Main.SelectedIndex = Tb_Main.SelectedIndex == 3 && !Tb3_Rdo_GAI_F0.Checked ? 0 : Tb_Main.SelectedIndex;
                this.Txt_TRCD.Validating += new System.ComponentModel.CancelEventHandler(this.Txt_TRCD_Validating);
                // <---V01.15.01 HWY ADD ▲(6490)
                Tb_Main.Refresh();
            }
        }

        private void Txt_TRFURI_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Txt_GRPID_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }


        private void Cbo_SAIKEN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;

            if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
            {
                Chk_SAIKEN_FLG.Enabled = true;
            }
            else
            {
                Chk_SAIKEN_FLG.Checked = false;
                Chk_SAIKEN_FLG.Enabled = false;
            }

            bool b = (this.ActiveControl == Cbo_SAIKEN);

            TAB_Enable_Control();

            if (b) Cbo_SAIKEN.Focus();

            Tb_Main.Enabled = true;
            Tb_Main.Refresh();
        }

        private void Cbo_SAIMU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;

            if (Global.nGroup == 1 && Cbo_SAIMU.SelectedValue.ToString() == sUse)
            {
                Lbl_GRPID.Enabled = true;
                Txt_GRPID.Enabled = true;
                Txt_GRPID.ReadOnlyEx = false;
                Txt_GRPNM.Enabled = true;
            }
            else
            {
                Lbl_GRPID.Enabled = false;
                Txt_GRPID.Enabled = false;
                Txt_GRPNM.Enabled = false;
                Txt_GRPID.ClearValue();
                Txt_GRPNM.ClearValue();
            }

            if (!(Global.cUsrSec.nMFLG < 2))
            {
                if (Cbo_SAIMU.SelectedValue.ToString() != sUse)
                {
                    Chk_SAIMU_FLG.Checked = false;
                }
                Set_Enabled_Chk_SAIMU_FLG(Txt_TRCD.ExCodeDB, Global.nTRCD_HJ == 1 ? Txt_HJCD.Text : "0");
            }

            bool b = (this.ActiveControl == Cbo_SAIMU);

            TAB_Enable_Control();

            if (b) Cbo_SAIMU.Focus();

            Tb_Main.Enabled = true;
            Tb_Main.Refresh();
        }


        private void Chk_SAIKEN_FLG_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Chk_SAIMU_FLG_CheckedChanged(object sender, EventArgs e)
        {
            if (Chk_SAIMU_FLG.Checked)
            {
                Tb3_Rdo_GAI_F1.Enabled = false;
            }
            else
            {
                Tb3_Rdo_GAI_F1.Enabled = Global.bEnabledState && !mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text);
                // 代表者マスターに登録済みの取引先は、支払代表者チェックボックスも変更不可
            }

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
        }

        private void Tb1_Txt_TRMAIL_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_TRURL_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_BIKO_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_E_TANTOCD_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_UsrNo_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_MYNO_AITE_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Chk_SRYOU_F_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_TextBox_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb2_Cmb_IJOU_1.ContainsFocus)
            {
//-- <9999>
//                if (Tb2_Cmb_IJOU_1.SelectedIndex != -1)
//                {
//                    Tb2_Txt_BUNKATSU_1.Enabled = true;
//                    Tb2_Cmb_HASU_1.Enabled = true;
//                    Tb2_Txt_SIGHT_M_1.Enabled = true;
//                    Tb2_Txt_SIGHT_D_1.Enabled = true;
//                    Tb2_Cmb_IJOU_2.Enabled = true;
//                    Tb2_Cmb_IJOU_2.SelectedIndex = 0;
//
//                    Tb2_Lbl_BUNKATSU_1.Enabled = true;
//                    Tb2_Lbl_SIGHT_M_1.Enabled = true;
//                    Tb2_Lbl_SIGHT_D_1.Enabled = true;
//                    Tb2_Lbl_IJOU_2.Enabled = true;
//                }

                if (Tb2_Cmb_IJOU_2.SelectedIndex < 1)
                {
                    Tb2_Txt_BUNKATSU_1.Enabled = true;
                    Tb2_Cmb_HASU_1.Enabled = true;

                    Tb2_Lbl_BUNKATSU_1.Enabled = true;
                    Tb2_Lbl_HASU.Enabled = true;

                    if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_1.SelectedValue.ToString(), 1) == "1")
                    {
//                        Tb2_Txt_BUNKATSU_1.Enabled = true;
//                        Tb2_Cmb_HASU_1.Enabled = true;
                        Tb2_Txt_SIGHT_M_1.Enabled = true;
                        Tb2_Txt_SIGHT_D_1.Enabled = true;

//                        Tb2_Lbl_HASU.Enabled = true;
//                        Tb2_Lbl_BUNKATSU_1.Enabled = true;
                        Tb2_Lbl_SIGHT.Enabled = true;
                        Tb2_Lbl_SIGHT_M_1.Enabled = true;
                        Tb2_Lbl_SIGHT_D_1.Enabled = true;

                    }
                    else                                         
                    {
                        Tb2_Txt_SIGHT_M_1.ClearValue();
                        Tb2_Txt_SIGHT_D_1.ClearValue();

                        Tb2_Txt_SIGHT_M_1.Enabled = false;
                        Tb2_Txt_SIGHT_D_1.Enabled = false;

                        Tb2_Lbl_SIGHT.Enabled = false;
                        Tb2_Lbl_SIGHT_M_1.Enabled = false;
                        Tb2_Lbl_SIGHT_D_1.Enabled = false;
                    }

                    if (Tb2_Cmb_IJOU_2.SelectedIndex == -1)
                    {
                        Tb2_Lbl_IJOU_2.Enabled = true;
                        Tb2_Cmb_IJOU_2.Enabled = true;
                        Tb2_Cmb_IJOU_2.SelectedIndex = 0;
                    }
                    else
                    {
                        Tb2_Lbl_IJOU_2.Enabled = true;
                        Tb2_Cmb_IJOU_2.Enabled = true;
                    }
                }
//-- <9999>
            }
            else if (Tb2_Cmb_IJOU_2.ContainsFocus)
            {
//-- <9999>
//                if (Tb2_Cmb_IJOU_2.SelectedIndex > 0)
//                {
//                    Tb2_Txt_BUNKATSU_2.Enabled = true;
//                    Tb2_Cmb_HASU_2.Enabled = true;
//                    Tb2_Txt_SIGHT_M_2.Enabled = true;
//                    Tb2_Txt_SIGHT_D_2.Enabled = true;
//                    Tb2_Cmb_IJOU_3.Enabled = true;
//                    Tb2_Cmb_IJOU_3.SelectedIndex = 0;
//
//                    Tb2_Lbl_BUNKATSU_2.Enabled = true;
//                    Tb2_Lbl_SIGHT_M_2.Enabled = true;
//                    Tb2_Lbl_SIGHT_D_2.Enabled = true;
//                    Tb2_Lbl_IJOU_3.Enabled = true;
//                }
//                else if (Tb2_Cmb_IJOU_2.SelectedIndex == 0)
//                {
//                    Tb2_Txt_BUNKATSU_2.Enabled = false;
//                    Tb2_Cmb_HASU_2.Enabled = false;
//                    Tb2_Txt_SIGHT_M_2.Enabled = false;
//                    Tb2_Txt_SIGHT_D_2.Enabled = false;
//                    Tb2_Cmb_IJOU_3.Enabled = false;
//                
//
//                    Tb2_Txt_BUNKATSU_2.ClearValue();
//                    Tb2_Cmb_HASU_2.SelectedIndex = -1;
//                    Tb2_Txt_SIGHT_M_2.ClearValue();
//                    Tb2_Txt_SIGHT_D_2.ClearValue();
//
//                    Tb2_Cmb_IJOU_3.SelectedIndex = -1;
//                    Tb2_Txt_BUNKATSU_3.ClearValue();
//                    Tb2_Cmb_HASU_3.SelectedIndex = -1;
//                    Tb2_Txt_SIGHT_M_3.ClearValue();
//                    Tb2_Txt_SIGHT_D_3.ClearValue();
//
//                    Tb2_Lbl_BUNKATSU_2.Enabled = false;
//                    Tb2_Lbl_SIGHT_M_2.Enabled = false;
//                    Tb2_Lbl_SIGHT_D_2.Enabled = false;
//                    Tb2_Lbl_IJOU_3.Enabled = false;
//
//                    Tb2_Txt_BUNKATSU_3.Enabled = false;
//                    Tb2_Cmb_HASU_3.Enabled = false;
//                    Tb2_Txt_SIGHT_M_3.Enabled = false;
//                    Tb2_Txt_SIGHT_D_3.Enabled = false;
//
//                    Tb2_Lbl_BUNKATSU_3.Enabled = false;
//                    Tb2_Lbl_SIGHT_M_3.Enabled = false;
//                    Tb2_Lbl_SIGHT_D_3.Enabled = false;
//                }
//            }
//            else if (Tb2_Cmb_IJOU_3.ContainsFocus)
//            {
//                if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
//                {
//                    Tb2_Txt_BUNKATSU_3.Enabled = true;
//                    Tb2_Cmb_HASU_3.Enabled = true;
//                    Tb2_Txt_SIGHT_M_3.Enabled = true;
//                    Tb2_Txt_SIGHT_D_3.Enabled = true;
//
//                    Tb2_Lbl_BUNKATSU_3.Enabled = true;
//                    Tb2_Lbl_SIGHT_M_3.Enabled = true;
//                    Tb2_Lbl_SIGHT_D_3.Enabled = true;
//                }
//                else if (Tb2_Cmb_IJOU_3.SelectedIndex == 0)
//                {
//                    Tb2_Txt_BUNKATSU_3.Enabled = false;
//                    Tb2_Cmb_HASU_3.Enabled = false;
//                    Tb2_Txt_SIGHT_M_3.Enabled = false;
//                    Tb2_Txt_SIGHT_D_3.Enabled = false;
//
//                    Tb2_Txt_BUNKATSU_3.ClearValue();
//                    Tb2_Cmb_HASU_3.SelectedIndex = -1;
//                    Tb2_Txt_SIGHT_M_3.ClearValue();
//                    Tb2_Txt_SIGHT_D_3.ClearValue();
//
//                    Tb2_Lbl_BUNKATSU_3.Enabled = false;
//                    Tb2_Lbl_SIGHT_M_3.Enabled = false;
//                    Tb2_Lbl_SIGHT_D_3.Enabled = false;
//                }

                if (Tb2_Cmb_IJOU_2.SelectedIndex > 0)
                {
                    Tb2_Txt_BUNKATSU_2.Enabled = true;
                    Tb2_Cmb_HASU_2.Enabled = true;

                    Tb2_Lbl_BUNKATSU_2.Enabled = true;

                    if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_2.SelectedValue.ToString(), 1) == "1")
                    {
                        Tb2_Txt_SIGHT_M_2.Enabled = true;
                        Tb2_Txt_SIGHT_D_2.Enabled = true;
                        Tb2_Lbl_SIGHT_M_2.Enabled = true;
                        Tb2_Lbl_SIGHT_D_2.Enabled = true;
                    }
                    else
                    {
                        Tb2_Txt_SIGHT_M_2.ClearValue();
                        Tb2_Txt_SIGHT_D_2.ClearValue();
                        Tb2_Txt_SIGHT_M_2.Enabled = false;
                        Tb2_Txt_SIGHT_D_2.Enabled = false;
                        Tb2_Lbl_SIGHT_M_2.Enabled = false;
                        Tb2_Lbl_SIGHT_D_2.Enabled = false;
                    }

                    if (Tb2_Cmb_IJOU_3.SelectedIndex == -1)
                    {
                        Tb2_Cmb_IJOU_3.Enabled = true;
                        Tb2_Lbl_IJOU_3.Enabled = true;

                        Tb2_Cmb_IJOU_3.SelectedIndex = 0;
                    }
                    else
                    {
                        Tb2_Cmb_IJOU_3.Enabled = true;
                        Tb2_Lbl_IJOU_3.Enabled = true;
                    }
                }
                else if (Tb2_Cmb_IJOU_2.SelectedIndex == 0)
                {
                    Tb2_Txt_BUNKATSU_2.Enabled = false;
                    Tb2_Cmb_HASU_2.Enabled = false;
                    Tb2_Txt_SIGHT_M_2.Enabled = false;
                    Tb2_Txt_SIGHT_D_2.Enabled = false;
                    Tb2_Cmb_IJOU_3.Enabled = false;


                    Tb2_Txt_BUNKATSU_2.ClearValue();
                    Tb2_Cmb_HASU_2.SelectedIndex = -1;
                    Tb2_Txt_SIGHT_M_2.ClearValue();
                    Tb2_Txt_SIGHT_D_2.ClearValue();

                    Tb2_Cmb_IJOU_3.SelectedIndex = -1;
                    Tb2_Txt_BUNKATSU_3.ClearValue();
                    Tb2_Cmb_HASU_3.SelectedIndex = -1;
                    Tb2_Txt_SIGHT_M_3.ClearValue();
                    Tb2_Txt_SIGHT_D_3.ClearValue();

                    Tb2_Lbl_BUNKATSU_2.Enabled = false;
                    Tb2_Lbl_SIGHT_M_2.Enabled = false;
                    Tb2_Lbl_SIGHT_D_2.Enabled = false;
                    Tb2_Lbl_IJOU_3.Enabled = false;

                    Tb2_Txt_BUNKATSU_3.Enabled = false;
                    Tb2_Cmb_HASU_3.Enabled = false;
                    Tb2_Txt_SIGHT_M_3.Enabled = false;
                    Tb2_Txt_SIGHT_D_3.Enabled = false;

                    Tb2_Lbl_BUNKATSU_3.Enabled = false;
                    Tb2_Lbl_SIGHT_M_3.Enabled = false;
                    Tb2_Lbl_SIGHT_D_3.Enabled = false;
                }
            }
            else if (Tb2_Cmb_IJOU_3.ContainsFocus)
            {
                if (Tb2_Cmb_IJOU_3.SelectedIndex > 0)
                {
//-- <9999>
                    Tb2_Txt_BUNKATSU_3.Enabled = true;
                    Tb2_Cmb_HASU_3.Enabled = true;
//                    Tb2_Txt_SIGHT_M_3.Enabled = true;
//                    Tb2_Txt_SIGHT_D_3.Enabled = true;
//
                    Tb2_Lbl_BUNKATSU_3.Enabled = true;
//                    Tb2_Lbl_SIGHT_M_3.Enabled = true;
//                    Tb2_Lbl_SIGHT_D_3.Enabled = true;
                    if (mcBsLogic.Get_NKUBN(Tb2_Cmb_IJOU_3.SelectedValue.ToString(), 1) == "1")
                    {
//-- <2016/03/08 誤植>
//                        Tb2_Txt_SIGHT_M_3.Enabled = true;
//                        Tb2_Lbl_SIGHT_D_3.Enabled = true;
//                        Tb2_Txt_SIGHT_M_3.Enabled = true;
//                        Tb2_Lbl_SIGHT_D_3.Enabled = true;
                        Tb2_Txt_SIGHT_M_3.Enabled = true;
                        Tb2_Txt_SIGHT_D_3.Enabled = true;
                        Tb2_Lbl_SIGHT_M_3.Enabled = true;
                        Tb2_Lbl_SIGHT_D_3.Enabled = true;
                    }
                    else
                    {
//                        Tb2_Txt_SIGHT_M_3.ClearValue();
//                        Tb2_Txt_SIGHT_M_3.ClearValue();
//
//                        Tb2_Txt_SIGHT_M_3.Enabled = false;
//                        Tb2_Lbl_SIGHT_D_3.Enabled = false;
//                        Tb2_Txt_SIGHT_M_3.Enabled = false;
//                        Tb2_Lbl_SIGHT_D_3.Enabled = false;

                        Tb2_Txt_SIGHT_M_3.ClearValue();
                        Tb2_Txt_SIGHT_D_3.ClearValue();

                        Tb2_Txt_SIGHT_M_3.Enabled = false;
                        Tb2_Txt_SIGHT_D_3.Enabled = false;
                        Tb2_Lbl_SIGHT_M_3.Enabled = false;
                        Tb2_Lbl_SIGHT_D_3.Enabled = false;
//-- <2016/03/08>
                    }
                }
                else if (Tb2_Cmb_IJOU_3.SelectedIndex == 0)
                {
                    Tb2_Txt_BUNKATSU_3.Enabled = false;
                    Tb2_Cmb_HASU_3.Enabled = false;
                    Tb2_Txt_SIGHT_M_3.Enabled = false;
                    Tb2_Txt_SIGHT_D_3.Enabled = false;

                    Tb2_Txt_BUNKATSU_3.ClearValue();
                    Tb2_Cmb_HASU_3.SelectedIndex = -1;
                    Tb2_Txt_SIGHT_M_3.ClearValue();
                    Tb2_Txt_SIGHT_D_3.ClearValue();

                    Tb2_Lbl_BUNKATSU_3.Enabled = false;
                    Tb2_Lbl_SIGHT_M_3.Enabled = false;
                    Tb2_Lbl_SIGHT_D_3.Enabled = false;
                }

            }
//-- <9999>
            else if (Tb2_Cmb_KAISYU.ContainsFocus)
            {
                //if (mcBsLogic.Get_NKUBN(Tb2_Cmb_KAISYU.SelectedValue.ToString(),1) == "1")
                if (mcBsLogic.Get_NKUBN(Tb2_Cmb_KAISYU.SelectedValue == null ? "" : Tb2_Cmb_KAISYU.SelectedValue.ToString(), 1) == "1") 
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = true;
                    Tb2_Lbl_SIGHT_M.Enabled = true;
                    Tb2_Lbl_SIGHT_D.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = true;
                }
                else
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = false;
                    Tb2_Lbl_SIGHT_M.Enabled = false;
                    Tb2_Lbl_SIGHT_D.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_M.ClearValue();
                    Tb2_Txt_KAISYUSIGHT_D.ClearValue();
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = false;
                }
            }
            else if (Tb2_Cmb_MIMAN.ContainsFocus)
            {
                if (mcBsLogic.Get_NKUBN(Tb2_Cmb_MIMAN.SelectedValue.ToString(), 1) == "1")
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = true;
                    Tb2_Lbl_SIGHT_M.Enabled = true;
                    Tb2_Lbl_SIGHT_D.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = true;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = true;
                }
                else
                {
                    Tb2_Lbl_SIGHT_Main.Enabled = false;
                    Tb2_Lbl_SIGHT_M.Enabled = false;
                    Tb2_Lbl_SIGHT_D.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_M.ClearValue();
                    Tb2_Txt_KAISYUSIGHT_D.ClearValue();
                    Tb2_Txt_KAISYUSIGHT_M.Enabled = false;
                    Tb2_Txt_KAISYUSIGHT_D.Enabled = false;
                }
            }
//-- <9999>
        }

        private void Tb2_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Cmb_DTESU_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Chk_DTESUSW_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb2_Chk_DTESUSW.Checked == true)
            {
//-- <2016/02/15 制御追加>
                Tb2_Cmb_DTESU.SelectedIndex = 0;
//-- <2016/02/15>
                Tb2_Cmb_DTESU.Enabled = true;
                Tb2_Lbl_DTESU.Enabled = true;
            }
            else
            {
                Tb2_Cmb_DTESU.SelectedIndex = -1;
                Tb2_Cmb_DTESU.Enabled = false;
                Tb2_Lbl_DTESU.Enabled = false;
            }
        }

        private void Tb4_Cmb_FTESUID_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb4_Chk_FDEF_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
//-- <2016/03/10 >
            if (Tb4_BindNavi_Cnt.Text.Replace("/", "").Replace(" ", "") != "1")
            {
                Tb4_Chk_FDEF.Enabled = true;
            }
            else { Tb4_Chk_FDEF.Enabled = false; }
//-- <2016/03/10>
        }

        private void Tb4_Chk_DDEF_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_FRIGIN = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void TAB_Enable_Control()
        {
            TabControl.TabPageCollection tabPages = Tb_Main.TabPages;

            if (Cbo_SAIKEN.SelectedValue.ToString() == sUse && Cbo_SAIMU.SelectedValue.ToString() == sUse)
            {
                tabPages[0].Enabled = true;
                tabPages[1].Enabled = true;
                tabPages[2].Enabled = true;
                tabPages[3].Enabled = true;
                tabPages[4].Enabled = true;
                tabPages[5].Enabled = true;
                bN = true;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //Tb_Main.SelectedIndex = 0;
                Tb_Main.SelectedIndex =Tb_Main.SelectedIndex ;
                // <---V01.15.01 HWY UPDATE ▲(6490)
                bN = false;
                Lbl_STAN.Enabled = (Global.nKMAN == 0 ? false : true);
                Tb5_Txt_STAN_CD.ReadOnlyEx = (Global.nKMAN == 0 ? true : false);
                Tb5_Grp_HJCD.Enabled = true;
                Tb5_Txt_DM1.ReadOnlyEx = false;
                Tb5_Txt_DM2.ReadOnlyEx = false;
                Tb5_Txt_DM3.ReadOnlyEx = false;

                if (Tb3_Rdo_GAI_F0.Checked == true)
                {
                    GAI_F_Kirikae(0);
                }
                else
                {
                    GAI_F_Kirikae(1);
                }          
            }
            else if (Cbo_SAIKEN.SelectedValue.ToString() == sUse)
            {
                tabPages[0].Enabled = true;
                tabPages[1].Enabled = true;
                tabPages[2].Enabled = false;
                tabPages[3].Enabled = false;
                tabPages[4].Enabled = true;
                tabPages[5].Enabled = false;
                bN = true;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //Tb_Main.SelectedIndex = 0;
                if (Tb_Main.SelectedIndex == 2 || Tb_Main.SelectedIndex == 3 || Tb_Main.SelectedIndex == 5 )
                {
                    Tb_Main.SelectedIndex = 0;
                }
                // <---V01.15.01 HWY UPDATE ▲(6490)
                bN = false;
                Tb5_Chk_NAYOSE.Enabled = false;
                Tb5_Chk_F_SETUIN.Enabled = false;
                Lbl_STAN.Enabled = false;
                Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                Tb5_Grp_TEGATA.Enabled = false;
                Tb5_Grp_OTHER.Enabled = false;
                Tb5_Grp_FAC.Enabled = false;
                Tb5_Grp_HJCD.Enabled = false;
                Tb5_Txt_DM1.ReadOnlyEx = true;
                Tb5_Txt_DM2.ReadOnlyEx = true;
                Tb5_Txt_DM3.ReadOnlyEx = true;
                Tb4_Grp_GENSEN.Enabled = false;
                Tb4_Grp_KJ.Enabled = false;
            }
            else if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
            {
                tabPages[0].Enabled = true;
                tabPages[1].Enabled = false;
                tabPages[2].Enabled = true;
                tabPages[3].Enabled = true;
                tabPages[4].Enabled = true;
                tabPages[5].Enabled = true;
                bN = true;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //Tb_Main.SelectedIndex = 0; 
                Tb_Main.SelectedIndex = Tb_Main.SelectedIndex == 1 ? 0 : Tb_Main.SelectedIndex;
                // <---V01.15.01 HWY UPDATE ▲(6490)
                bN = false;
                Lbl_STAN.Enabled = (Global.nKMAN == 0 ? false : true);
                Tb5_Txt_STAN_CD.ReadOnlyEx = (Global.nKMAN == 0 ? true : false);
                Tb5_Grp_HJCD.Enabled = true;
                Tb5_Txt_DM1.ReadOnlyEx = false;
                Tb5_Txt_DM2.ReadOnlyEx = false;
                Tb5_Txt_DM3.ReadOnlyEx = false;

                if (Tb3_Rdo_GAI_F0.Checked == true)
                {
                    GAI_F_Kirikae(0);
                }
                else
                {
                    GAI_F_Kirikae(1);
                }
            }
            else if (Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly || Cbo_SAIMU.SelectedValue.ToString() == sDueOnly)
            {
                // ▼#111516　竹内　2022/02/18
                //tabPages[0].Enabled = false;
                tabPages[0].Enabled = true;
                // ▲#111516　竹内　2022/02/18
                tabPages[1].Enabled = false;
                tabPages[2].Enabled = false;
                tabPages[3].Enabled = false;
                tabPages[4].Enabled = true;
                tabPages[5].Enabled = false;

                bN = true;
                // --->V01.15.01 HWY UPDATE ▼(6490)
                //Tb_Main.SelectedIndex = 4;
                Tb_Main.SelectedIndex = Tb_Main.SelectedIndex  != 4 ? 0 : Tb_Main.SelectedIndex;
                if (Cbo_SAIKEN.SelectedValue.ToString() == sDueOnly && Cbo_SAIMU.SelectedValue.ToString() == sDueOnly)
                {
                    // ▼#111516　竹内　2022/02/24
                    //Tb_Main.SelectedIndex = 4;
                    Tb_Main.SelectedIndex = Tb_Main.SelectedIndex == 4 ? 4 : 0;
                    // ▲#111516　竹内　2022/02/24
                }
                // ▼#111516　竹内　2022/02/24
                //else
                //{
                //    Tb_Main.SelectedIndex = Tb_Main.SelectedIndex != 4 ? 0 : 4;
                //}
                // ▲#111516　竹内　2022/02/24
                // <---V01.15.01 HWY UPDATE ▲(6490)
                bN = false;

                Tb5_Chk_NAYOSE.Enabled = false;
                Tb5_Chk_F_SETUIN.Enabled = false;
                Lbl_STAN.Enabled = false;
                Tb5_Txt_STAN_CD.ReadOnlyEx = true;
                Tb5_Grp_TEGATA.Enabled = false;
                Tb5_Grp_OTHER.Enabled = false;
                Tb5_Grp_FAC.Enabled = false;
                Tb5_Grp_HJCD.Enabled = false;
                Tb5_Txt_DM1.ReadOnlyEx = true;
                Tb5_Txt_DM2.ReadOnlyEx = true;
                Tb5_Txt_DM3.ReadOnlyEx = true;
                Tb4_Grp_GENSEN.Enabled = false;
                Tb4_Grp_KJ.Enabled = false;
            }
            else
            {
                // ▼#111516　竹内　2022/02/24
                //tabPages[0].Enabled = true;
                //bN = true;
                //Tb_Main.SelectedIndex = 0;
                //bN = false;
                //tabPages[0].Enabled = false;
                // ▼#111516　竹内　2022/03/08
                //Tb_Main.SelectedIndex = 0;
                //bN = true;
                bN = true;
                Tb_Main.SelectedIndex = 0;
                bN = false;
                // ▲#111516　竹内　2022/03/08
                // ▼#111516　竹内　2022/03/11　仕様変更
                //tabPages[0].Enabled = true;
                tabPages[0].Enabled = false;
                // ▲#111516　竹内　2022/03/11　仕様変更
                // ▲#111516　竹内　2022/02/24
                tabPages[1].Enabled = false;
                tabPages[2].Enabled = false;
                tabPages[3].Enabled = false;
                tabPages[4].Enabled = false;
                tabPages[5].Enabled = false;
            }
            //2018/11/02 ICS.吉岡 ▼(SIAS-9898)＜マスタ権限を｢参照のみ｣にしているユーザーの場合でもその他情報タブが更新できます。＞
            Chg_DispControl();
            //2018/11/02 ICS.吉岡 ▲(SIAS-9898)＜マスタ権限を｢参照のみ｣にしているユーザーの場合でもその他情報タブが更新できます。＞
            
            Tb_Main.Refresh();
        }

        private void Tb6_Txt_TEGVAL_TextChanged(object sender, EventArgs e)
        {
            if (!Tb5_Txt_TEGVAL.IsEdited) { return; }   // <---V01.15.01 HWY ADD ◀(6490)	
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Rdo_HORYU0_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>

            if (Tb5_Rdo_HORYU0.Checked == true)                                     // その他　控除関連　使用しない
            {
                Tb5_Chk_OUTPUT.Enabled = true;                                      // その他　源泉税関連　源泉税計算する

                Tb5_Txt_HR_KIJYUN.ClearValue();                                     // その他　控除関連　適用基準額の値
                Tb5_Txt_HR_KIJYUN.Enabled = false;                                  // その他　控除関連　適用基準額
                Tb4_Lbl_KYVAL.Enabled = false;                                      // その他　控除関連　適用基準額ラベル
                Tb4_Lbl_KYVAL_Over.Enabled = false;                                 // その他　控除関連　適用基準額以上ラベル

                Tb5_Cmb_HORYU_F.SelectedIndex = -1;                                 // その他　控除関連　計算区分コンボの値
                Tb5_Cmb_HORYU_F.Enabled = false;                                    // その他　控除関連　計算区分コンボ
                Tb4_Lbl_KYCAL.Enabled = false;                                      // その他　控除関連　計算区分ラベル

//-- <2016/03/11 >
//                Tb5_Txt_HOVAL.ExNumValue = 100;                                   
                Tb5_Txt_HOVAL.ExNumValue = 100.000M;                                // その他　控除関連　比率の値の値
//-- <2016/03/11>
                Tb5_Txt_HOVAL.Enabled = false;                                      // その他　控除関連　比率の値
                Tb4_Lbl_HOVAL.Enabled = false;                                      // その他　控除関連　比率の値ラベル
                Tb4_Lbl_HOVAL_TANI.Enabled = false;                                 // その他　控除関連　比率の値％ラベル

                Tb5_Txt_HRORYUGAKU.ClearValue();                                    // その他　控除関連　定額の値
                Tb5_Txt_HRORYUGAKU.Enabled = false;                                 // その他　控除関連　定額
                Tb5_Lbl_HRORYUGAKU.Enabled = false;                                 // その他　控除関連　定額ラベル

                Tb5_Cmb_HRKBN.SelectedIndex = -1;                                   // その他　控除関連　作成区分の値
                Tb5_Cmb_HRKBN.Enabled = false;                                      // その他　控除関連　作成区分
                Tb5_Lbl_HRKBN.Enabled = false;                                      // その他　控除関連　作成区分ラベル
            }
            else
            {
                Tb5_Chk_OUTPUT.Checked = false;
                Tb5_Chk_OUTPUT.Enabled = false;

                Tb5_Txt_HR_KIJYUN.Enabled = true;
                Tb4_Lbl_KYVAL.Enabled = true;
                Tb4_Lbl_KYVAL_Over.Enabled = true;

                Tb5_Cmb_HORYU_F.Enabled = true;
                Tb4_Lbl_KYCAL.Enabled = true;

                //Tb5_Txt_HOVAL.Enabled = true;
                //Tb4_Lbl_HOVAL.Enabled = true;
                //Tb4_Lbl_HOVAL_TANI.Enabled = true;

                //Tb5_Txt_HRORYUGAKU.Enabled = true;
                //Tb5_Lbl_HRORYUGAKU.Enabled = true;

//-- <2016/03/22>
//                Tb5_Cmb_HRKBN.SelectedIndex = 0;
                Tb5_Cmb_HRKBN.SelectedIndex = -1;
//-- <2016/03/22>
                Tb5_Cmb_HRKBN.Enabled = true;
                Tb5_Lbl_HRKBN.Enabled = true;
            }
        }

        private void Tb5_Cmb_HORYU_F_SelectedIndexChanged(object sender, EventArgs e)
        {
//-- <2016/03/23>
            if (bHORYUNull && bKOUJYONull)
            { return; }
//-- <2016/03/23>

            if (Tb5_Cmb_HORYU_F.SelectedIndex == 0)
            {
                Tb5_Txt_HOVAL.Enabled = true;
                Tb4_Lbl_HOVAL.Enabled = true;
                Tb4_Lbl_HOVAL_TANI.Enabled = true;

                Tb5_Txt_HRORYUGAKU.ClearValue();
                Tb5_Txt_HRORYUGAKU.Enabled = false;
                Tb5_Lbl_HRORYUGAKU.Enabled = false;
            }
            else if (Tb5_Cmb_HORYU_F.SelectedIndex == 1)
            {
                Tb5_Txt_HRORYUGAKU.Enabled = true;
                Tb5_Lbl_HRORYUGAKU.Enabled = true;

//-- <2016/03/15>
//                Tb5_Txt_HOVAL.ExNumValue = 100;
//-- <206/03/21>
//                Tb5_Txt_HOVAL.ExNumValue = 100.000M;
                Tb5_Txt_HOVAL.Text = "0.000";
                Tb5_Txt_HOVAL.ExNumValue = 0.000M;
//-- <2016/03/21>
//-- <2016/03/15>
                Tb5_Txt_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL_TANI.Enabled = false;
            }
            else
            {
//-- <2016/03/15>
//                Tb5_Txt_HOVAL.ExNumValue = 100;
                Tb5_Txt_HOVAL.ExNumValue = 100.000M;
//-- <2016/03/15>
                Tb5_Txt_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL.Enabled = false;
                Tb4_Lbl_HOVAL_TANI.Enabled = false;

                Tb5_Txt_HRORYUGAKU.ClearValue();
                Tb5_Txt_HRORYUGAKU.Enabled = false;
                Tb5_Lbl_HRORYUGAKU.Enabled = false;
            }
        }

        private void Tb5_Txt_HR_KIJYUN_TextChanged(object sender, EventArgs e)
        {
//-- <2016/03/15>
            if (!Tb5_Txt_HR_KIJYUN.IsEdited)
            { return; }
//-- <2016/03/15>

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Txt_HOVAL_TextChanged(object sender, EventArgs e)
        {
//-- <2016/03/15>
            if (!Tb5_Txt_HOVAL.IsEdited)
            { return; }
//-- <2016/03/15>

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Txt_HRORYUGAKU_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb5_Cmb_HRKBN_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Chk_Jyoto_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb2_Txt_SHIME_Validating(object sender, CancelEventArgs e)
        {
//-- <2016/02/15 見直し0不許可、1～27までは入力値、未入力と28～99は末、締日と0ヶ月の日で大小チェック>
//            if (Tb2_Txt_SHIME.Text == "99" || Tb2_Txt_SHIME.Text == "28" || Tb2_Txt_SHIME.Text == "29" || Tb2_Txt_SHIME.Text == "30" || Tb2_Txt_SHIME.Text == "31")
//            {
//                Tb2_Txt_SHIME.Text = "末";
//            }
//            else if (Tb2_Txt_SHIME.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_SHIME.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_SHIME.IsError = true;
//                    return;
//                }
//                Tb2_Txt_SHIME.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 念のため末も>
//            if (Tb2_Txt_SHIME.Text != "")
            if (Tb2_Txt_SHIME.Text != "" && Tb2_Txt_SHIME.Text != "末")
//-- <2016/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_SHIME.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_SHIME.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    Tb2_Txt_SHIME.Text = "末";
                }
                //--->V01.12.01 ATT ADD ▼ (7942)
                else if(nDD.ToString().Length == 1)
                {
                    Tb2_Txt_SHIME.Text = "0" + nDD.ToString();
                }
                //<---V01.12.01 ATT ADD ▲ (7942)
            }
            else
            {
                Tb2_Txt_SHIME.Text = "末";
            }
//-- <2016/02/15>
        }

        private void Tb2_Txt_SHIME_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_SHIME.Text == "末")
            {
                Tb2_Txt_SHIME.Text = "99";
            }
        }

        private void Tb2_Txt_KAISYUHI_D_Validating(object sender, CancelEventArgs e)
        {
//-- <2016/02/15 見直し0不許可、1～27までは入力値、未入力と28～99は末、締日と0ヶ月の日で大小チェック>
//            if (Tb2_Txt_KAISYUHI_D.Text == "99" || Tb2_Txt_KAISYUHI_D.Text == "28" || Tb2_Txt_KAISYUHI_D.Text == "29" || Tb2_Txt_KAISYUHI_D.Text == "30" || Tb2_Txt_KAISYUHI_D.Text == "31")
//            {
//                Tb2_Txt_KAISYUHI_D.Text = "末";
//            }
//            else if (Tb2_Txt_KAISYUHI_D.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_KAISYUHI_D.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_KAISYUHI_D.IsError = true;
//                    return;
//                }
//                Tb2_Txt_KAISYUHI_D.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 念のため末も>
//            if (Tb2_Txt_KAISYUHI_D.Text != "")
            if (Tb2_Txt_KAISYUHI_D.Text != "" && Tb2_Txt_KAISYUHI_D.Text != "末")
//-- <2016/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_KAISYUHI_D.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_KAISYUHI_D.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    nDD = 99;
                    Tb2_Txt_KAISYUHI_D.Text = "末";
                }
            }
            else 
            {
                nDD = 99;
                Tb2_Txt_KAISYUHI_D.Text = "末";
            }
            
            // 大小チェック
            if (Tb2_Txt_KAISYUHI_M.Text == "0" || Tb2_Txt_KAISYUHI_M.Text == "")
            {
                int nSHIME;
                if (Tb2_Txt_SHIME.Text == "" || Tb2_Txt_SHIME.Text == "末")
                {
                    nSHIME = 99;
                }
                else
                {
                    nSHIME = Convert.ToInt32(Tb2_Txt_SHIME.Text);
                }

                if (nSHIME > nDD)
                {
                    e.Cancel = true;
                    Tb2_Txt_KAISYUHI_D.IsError = true;
                    return;
                }
            }
//-- <>
        }

        private void Tb2_Txt_KAISYUHI_D_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_KAISYUHI_D.Text == "末")
            {
                Tb2_Txt_KAISYUHI_D.Text = "99";
            }
        }

        private void Tb2_Txt_KAISYUSIGHT_D_Validating(object sender, CancelEventArgs e)
        {
//-- <>
//            if (Tb2_Txt_KAISYUSIGHT_D.Text == "99" || Tb2_Txt_KAISYUSIGHT_D.Text == "28" || Tb2_Txt_KAISYUSIGHT_D.Text == "29" || Tb2_Txt_KAISYUSIGHT_D.Text == "30" || Tb2_Txt_KAISYUSIGHT_D.Text == "31")
//            {
//                Tb2_Txt_KAISYUSIGHT_D.Text = "末";
//            }
//            else if (Tb2_Txt_KAISYUSIGHT_D.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_KAISYUSIGHT_D.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_KAISYUSIGHT_D.IsError = true;
//                    return;
//                }
//                Tb2_Txt_KAISYUSIGHT_D.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 念のため末も>
//            if (Tb2_Txt_KAISYUSIGHT_D.Text != "")
            if (Tb2_Txt_KAISYUSIGHT_D.Text != "" && Tb2_Txt_KAISYUSIGHT_D.Text != "末")
//-- <2016/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_KAISYUSIGHT_D.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_KAISYUSIGHT_D.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    nDD = 99;
                    Tb2_Txt_KAISYUSIGHT_D.Text = "末";
                }
            }
            else
            {
                nDD = 99;
                Tb2_Txt_KAISYUSIGHT_D.Text = "末";
            }
//-- <>
        }

        private void Tb2_Txt_KAISYUSIGHT_D_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_KAISYUSIGHT_D.Text == "末")
            {
                Tb2_Txt_KAISYUSIGHT_D.Text = "99";
            }
        }

        private void Tb2_Txt_SIGHT_D_1_Validating(object sender, CancelEventArgs e)
        {
//-- <>
//            if (Tb2_Txt_SIGHT_D_1.Text == "99" || Tb2_Txt_SIGHT_D_1.Text == "28" || Tb2_Txt_SIGHT_D_1.Text == "29" || Tb2_Txt_SIGHT_D_1.Text == "30" || Tb2_Txt_SIGHT_D_1.Text == "31")
//            {
//                Tb2_Txt_SIGHT_D_1.Text = "末";
//            }
//            else if (Tb2_Txt_SIGHT_D_1.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_1.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_SIGHT_D_1.IsError = true;
//                    return;
//                }
//                Tb2_Txt_SIGHT_D_1.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 末の際もある>
//            if (Tb2_Txt_SIGHT_D_1.Text != "")
            if (Tb2_Txt_SIGHT_D_1.Text != "" && Tb2_Txt_SIGHT_D_1.Text != "末")
//-- <2016/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_1.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_SIGHT_D_1.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    nDD = 99;
                    Tb2_Txt_SIGHT_D_1.Text = "末";
                }
            }
            else
            {
                nDD = 99;
                Tb2_Txt_SIGHT_D_1.Text = "末";
            }
//-- <>
        }

        private void Tb2_Txt_SIGHT_D_1_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_SIGHT_D_1.Text == "末")
            {
                Tb2_Txt_SIGHT_D_1.Text = "99";
            }
        }

        private void Tb2_Txt_SIGHT_D_2_Validating(object sender, CancelEventArgs e)
        {
//-- <>
//            if (Tb2_Txt_SIGHT_D_2.Text == "99" || Tb2_Txt_SIGHT_D_2.Text == "28" || Tb2_Txt_SIGHT_D_2.Text == "29" || Tb2_Txt_SIGHT_D_2.Text == "30" || Tb2_Txt_SIGHT_D_2.Text == "31")
//            {
//                Tb2_Txt_SIGHT_D_2.Text = "末";
//            }
//            else if (Tb2_Txt_SIGHT_D_2.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_2.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_SIGHT_D_2.IsError = true;
//                    return;
//                }
//                Tb2_Txt_SIGHT_D_2.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 念のため末も>
//            if (Tb2_Txt_SIGHT_D_2.Text != "")
            if (Tb2_Txt_SIGHT_D_2.Text != "" && Tb2_Txt_SIGHT_D_2.Text != "末")
//-- <2106/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_2.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_SIGHT_D_2.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    nDD = 99;
                    Tb2_Txt_SIGHT_D_2.Text = "末";
                }
            }
            else
            {
                nDD = 99;
                Tb2_Txt_SIGHT_D_2.Text = "末";
            }
//-- <>
        }

        private void Tb2_Txt_SIGHT_D_2_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_SIGHT_D_2.Text == "末")
            {
                Tb2_Txt_SIGHT_D_2.Text = "99";
            }
        }

        private void Tb2_Txt_SIGHT_D_3_Validating(object sender, CancelEventArgs e)
        {
//-- <>
//            if (Tb2_Txt_SIGHT_D_3.Text == "99" || Tb2_Txt_SIGHT_D_3.Text == "28" || Tb2_Txt_SIGHT_D_3.Text == "29" || Tb2_Txt_SIGHT_D_3.Text == "30" || Tb2_Txt_SIGHT_D_3.Text == "31")
//            {
//                Tb2_Txt_SIGHT_D_3.Text = "末";
//            }
//            else if (Tb2_Txt_SIGHT_D_3.Text != "")
//            {
//                int nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_3.Text);
//                if (nDD == 0 || (nDD >= 32 && nDD <= 98))
//                {
//                    e.Cancel = true;
//                    Tb2_Txt_SIGHT_D_3.IsError = true;
//                    return;
//                }
//                Tb2_Txt_SIGHT_D_3.Text = nDD.ToString();
//            }
            int nDD;
//-- <2016/03/08 念のため末も>
//            if (Tb2_Txt_SIGHT_D_3.Text != "")
            if (Tb2_Txt_SIGHT_D_3.Text != "" && Tb2_Txt_SIGHT_D_3.Text != "末")
//-- <2016/03/08>
            {
                nDD = Convert.ToInt32(Tb2_Txt_SIGHT_D_3.Text);
                if (nDD == 0)
                {
                    e.Cancel = true;
                    Tb2_Txt_SIGHT_D_3.IsError = true;
                    return;
                }
                else if (nDD >= 31 && nDD <= 99)
                {
                    nDD = 99;
                    Tb2_Txt_SIGHT_D_3.Text = "末";
                }
            }
            else
            {
                nDD = 99;
                Tb2_Txt_SIGHT_D_3.Text = "末";
            }
//-- <>
        }

        private void Tb2_Txt_SIGHT_D_3_Enter(object sender, EventArgs e)
        {
            if (Tb2_Txt_SIGHT_D_3.Text == "末")
            {
                Tb2_Txt_SIGHT_D_3.Text = "99";
            }
        }

        private void Tb2_Txt_KAISYUHI_M_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_KAISYUHI_M.Text != "")
            {
                Tb2_Txt_KAISYUHI_M.Text = Convert.ToInt32(Tb2_Txt_KAISYUHI_M.Text).ToString();
            }
//-- <9999 空送りで0代入>
            else
            {
                Tb2_Txt_KAISYUHI_M.Text = "0";
            }
//-- <9999>
        }

        private void Tb2_Txt_KAISYUSIGHT_M_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_KAISYUSIGHT_M.Text != "")
            {
                Tb2_Txt_KAISYUSIGHT_M.Text = Convert.ToInt32(Tb2_Txt_KAISYUSIGHT_M.Text).ToString();
            }
//-- <9999 空送りで0代入>
            else
            {
                Tb2_Txt_KAISYUSIGHT_M.Text = "0";
            }
//-- <9999>
        }

        private void Tb2_Txt_SIGHT_M_1_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_SIGHT_M_1.Text != "")
            {
                Tb2_Txt_SIGHT_M_1.Text = Convert.ToInt32(Tb2_Txt_SIGHT_M_1.Text).ToString();
            }
//-- <9999 空送りで0代入>
            else
            {
                Tb2_Txt_SIGHT_M_1.Text = "0";
            }
//-- <9999>
        }

        private void Tb2_Txt_SIGHT_M_2_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_SIGHT_M_2.Text != "")
            {
                Tb2_Txt_SIGHT_M_2.Text = Convert.ToInt32(Tb2_Txt_SIGHT_M_2.Text).ToString();
            }
//-- <9999 空送りで0代入>
            else
            {
                Tb2_Txt_SIGHT_M_2.Text = "0";
            }
//-- <9999>
        }

        private void Tb2_Txt_SIGHT_M_3_Validating(object sender, CancelEventArgs e)
        {
            if (Tb2_Txt_SIGHT_M_3.Text != "")
            {
                Tb2_Txt_SIGHT_M_3.Text = Convert.ToInt32(Tb2_Txt_SIGHT_M_3.Text).ToString();
            }
//-- <9999 空送りで0代入>
            else
            {
                Tb2_Txt_SIGHT_M_3.Text = "0";
            }
//-- <9999>
        }

        private void Tb4_BindNavi_First_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？",
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb4_Lbl_GIN_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sGIN_ID = "2";
                    int iSS_FRIGIN_cnt;
                    int iCnt;
                    int iCurrentCnt = 1;
                    mcBsLogic.Sel_SS_FRIGIN_Prev(sTRCD, sHJCD, sGIN_ID, out iCnt, out iSS_FRIGIN_cnt);
//-- <2016/02/14>
//--                    bEventCancel = true;
//-- <2016/02/14>
                    Set_Tb2_SS_FRIGIN(iCurrentCnt, iCnt);
//-- <2016/02/14>
//--                    bEventCancel = false;
//-- <2016/02/14>
                }
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb4_BindNzvi_Fast_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        private void Tb4_BindNavi_End_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateChildren();
                if (nDispChgFlg_FRIGIN == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？",
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        Ins_SSTORI();
                        if (nErrFlg == 1)
                        {
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb4_Lbl_GIN_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sGIN_ID = (Convert.ToInt32(Tb4_BindNavi_Cnt.Text.Replace("/", "")) - 1).ToString();
                    int iSS_FRIGIN_cnt;
                    int iCnt;
                    int iCurrentCnt = Convert.ToInt32(Tb4_BindNavi_Cnt.Text.Replace("/", ""));
                    mcBsLogic.Sel_SS_FRIGIN_Next(sTRCD, sHJCD, sGIN_ID, out iCnt, out iSS_FRIGIN_cnt);
//-- <2016/02/14>
//--                    bEventCancel = true;
//-- <2016/02/14>
                    Set_Tb2_SS_FRIGIN(iCurrentCnt, iCnt);
//-- <2016/02/14>
//--                    bEventCancel = false;
//-- <2016/02/14>
                }
                nDispChgFlg_FRIGIN = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb4_BindNavi_End_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
        }

        private void Tb3_BindNavi_First_Click(object sender, EventArgs e)
        {
            try
            {
                nTabBindNavi = 1;
                ValidateChildren();
                if (nDispChgFlg_TSHOH == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？",
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        nTabBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                        string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                        string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                        if ((Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") || mcBsLogic.Chk_UniqKey(Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == true)
                        {
                            Ins_SSTORI();
                            if (nErrFlg == 1)
                            {
                                nTabBindNavi = 0;
                                return;
                            }
                        }
                        else
                        {
                            string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                            string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                            MessageBox.Show(
                                "既に"
                                + "\nID：" + sSHOID
                                + "\n部門：" + sBMNNM
                                + "\n科目：" + sKMKNM
                                + "\nは登録済です。",
//-- <2016/03/22>
//                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            Tb3_Txt_BCOD.Focus();
                            nTabBindNavi = 0;
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb1_Lbl_SHO_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.Trim(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    string sSHO_ID = "2";
                    int iCnt;
                    int iCurrentCnt = 1;
                    if (iCurrentCnt == 0)
                    {
                        iCurrentCnt = 1;
                    }
                    mcBsLogic.Sel_SS_TSHOH_Prev(sTRCD, sHJCD, sSHO_ID, out iCnt);
                    Set_Tb1_SS_TSHOH(iCurrentCnt, iCnt);
                }
                if (BindNavi2_Selected.Text == "1")
                {
                    Tb3_BindNavi_First.Enabled = false;
                    Tb3_BindNavi_Prev.Enabled = false;
                }
                else
                {
                    Tb3_BindNavi_First.Enabled = true;
                    Tb3_BindNavi_Prev.Enabled = true;
                }
                nDispChgFlg_TSHOH = 0;
                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb3_BindNavi_First_Click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            nTabBindNavi = 0;
        }

        private void Tb3_BindNavi_Last_Click(object sender, EventArgs e)
        {
            try
            {
                nTabBindNavi = 1;
                ValidateChildren();
                if (nDispChgFlg_TSHOH == 1)
                {
                    res = MessageBox.Show(
                        "内容が変更されています。\nデータの移動前に確定しますか？",
//-- <2016/03/24>
//                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        Global.sPrgName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
//-- <2016/03/24>
                    if (res == DialogResult.Cancel)
                    {
                        Refresh();
                        nTabBindNavi = 0;
                        return;
                    }
                    else if (res == DialogResult.No)
                    {
                    }
                    else if (res == DialogResult.Yes)
                    {
                        string sSHOID = Tb1_Lbl_SHO_ID_V.Text;
                        string sBCOD = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_BCOD.ExCodeDB);
                        string sKCOD = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "0" : Tb3_Txt_KCOD.ExCodeDB);
                        if ((Chk_SAIMU_FLG.Checked == true && sBCOD == "0" && sKCOD == "0") || mcBsLogic.Chk_UniqKey(Txt_TRCD.ExCodeDB, Txt_HJCD.Text, sBCOD, sKCOD, ref sSHOID) == true)
                        {
                            Ins_SSTORI();
                            if (nErrFlg == 1)
                            {
                                nTabBindNavi = 0;
                                return;
                            }
                        }
                        else
                        {
                            string sBMNNM = (Tb3_Txt_BCOD.ExCodeDB == "" || Tb3_Txt_BCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_BNAM(Tb3_Txt_BCOD.ExCodeDB));
                            string sKMKNM = (Tb3_Txt_KCOD.ExCodeDB == "" || Tb3_Txt_KCOD.ExCodeDB == "0" ? "全て" : mcBsLogic.Get_KNMX(Tb3_Txt_KCOD.ExCodeDB));
                            MessageBox.Show(
                                "既に"
                                + "\nID：" + sSHOID
                                + "\n部門：" + sBMNNM
                                + "\n科目：" + sKMKNM
                                + "\nは登録済です。",
//-- <2016/03/22>
//                                Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
//-- <2016/03/22>
                            Tb3_Txt_BCOD.Focus();
                            nTabBindNavi = 0;
                            return;
                        }
                    }
                }

                if (Txt_TRCD.ExCodeDB != "" && Tb1_Lbl_SHO_ID_V.Text != "")
                {
                    string sTRCD = Txt_TRCD.ExCodeDB.TrimEnd(' ');
                    string sHJCD = (Txt_HJCD.Text == "" ? "0" : Txt_HJCD.Text);
                    //string sSHO_ID = Tb1_Lbl_SHO_ID_V.Text;
                    string sSHO_ID = (Convert.ToInt32(BindNavi2_Cnt.Text.Replace("/", "")) - 1).ToString();
                    int iCnt;
                    //int iCurrentCnt = int.Parse(BindNavi2_Selected.Text) + 1;
                    int iCurrentCnt = Convert.ToInt32(BindNavi2_Cnt.Text.Replace("/", ""));
                    mcBsLogic.Sel_SS_TSHOH_Next(sTRCD, sHJCD, sSHO_ID, out iCnt);
                    Set_Tb1_SS_TSHOH(iCurrentCnt, iCnt);
                }
                nDispChgFlg_TSHOH = 0;

                if (nDispChgFlg_Main == 0 && nDispChgFlg_TSHOH == 0 && nDispChgFlg_FRIGIN == 0)
                {
                    Btn_REG.Enabled = false;
                    FKB.F10_Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
//-- <2016/03/22>
//                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message,
//                    Global.sPrgName + "　Ver" + Global.sPrgVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    "エラーが発生しました。\n\nメッセージ：\n" + ex.Message + "\r\nTb3_BindNavi_Last_click　\r\nVer" + Global.sPrgVer,
                    Global.sPrgName, MessageBoxButtons.OK, MessageBoxIcon.Error);
//-- <2016/03/22>
            }
            nTabBindNavi = 0;
        }

        private void Tb6_Cmb_HEI_CD_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb6_Cmb_GAI_TF_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb6_Cmb_GAI_KZID_SelectedIndexChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb6_Rdo_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb6_Txt_TextChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        private void Tb1_Txt_UsrNo_Validating(object sender, CancelEventArgs e)
        {
            Tb1_Txt_UsrNo.Text = Tb1_Txt_UsrNo.Text.ToUpper();
        }

        private void Tb1_Txt_UsrNo_TextChanged_1(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
//-- <2016/03/09 F06 Enabled=false>
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
//-- <2016/03/09>
        }

        public bool fal { get; set; }

        private void Tb5_Txt_FAC_Validating(object sender, CancelEventArgs e)
        {
            Tb5_Txt_FAC.Text = Tb5_Txt_FAC.Text.ToUpper();
        }

        private void Tb2_Txt_TOKUKANA_TextChanged(object sender, EventArgs e)
        {
            //            ((TextBoxEx)sender).Text = checkString(((TextBoxEx)sender).Text);
            ((TextBoxEx)sender).ExCodeValue = checkString(((TextBoxEx)sender).Text);
            //本来であればTextへ変換処理後の文字列を入れたいが
            //それを行うと動作がおかしくなるため、ExCodeValueへ入れ
            //ValidatedのタイミングでExCodeValueの文字をTextへペースト
            //どうおかしくなるかというと
            //例として「ｱｲｳｴｵ」と平で打ち込みF6を押し「あいうえお」にしてからEnterを押下すると
            //結果が「ｵｴｳｲｱ」となる
            //打ち込みに対してなので「あ」を「ｱ」に変換後、フォーカスがｱの前に来てしまい
            //次に「い」がフォーカス位置に出力され…それが続くためにこの現象となる模様
            //変換が発生しない場合はこれに該当せず

            nDispChgFlg_Main = 1;
            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;

        }

        private string checkString(string strVal)
        {
            //1.片仮名化＋半角化を行う（変換処理１）
            //2.変換処理１の結果からByteとLengthを比較し、差異がある場合は全角（漢字や一部文字）を排除
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            int n0 = strVal.ToString().Length;
            string s5 = Microsoft.VisualBasic.Strings.StrConv(strVal, Microsoft.VisualBasic.VbStrConv.Katakana | Microsoft.VisualBasic.VbStrConv.Narrow, 0x411);
            int n1 = sjisEnc.GetByteCount(s5);
            string s6 = "";
            if (n0 != n1)
            {
                foreach (char c in s5)
                {
                    if (sjisEnc.GetByteCount(c.ToString()) != 1) { continue; }
                    s6 += c.ToString();
                }
            }
            return (n0 == n1) ? s5 : s6;
        }

        private void Tb2_Txt_TOKUKANA_Validating(object sender, CancelEventArgs e)
        {
            Tb2_Txt_TOKUKANA.Text = StringUtil.StrinUtil.RemoveHojinkaku(Tb2_Txt_TOKUKANA.Text);
        }

        private void Tb4_Chk_FDEF_Enter(object sender, EventArgs e)
        {
        }

        private void Tb2_Txt_TOKUKANA_Leave(object sender, EventArgs e)
        {
            // 入力文字の半角カナ変換
            string sValue = Tb2_Txt_TOKUKANA.ExCodeValue;
            //string sValue = txtFurikomiNM.Text;
            Tb2_Txt_TOKUKANA.Text = Global.RemoveHojinKaku(Global.ChangeCharacter(sValue));

        }

        private void Tb5_Rdo_GSSKBN1_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;

            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;
            
            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
        }

        private void Tb5_Rdo_GSSKBN2_CheckedChanged(object sender, EventArgs e)
        {
            nDispChgFlg_Main = 1;

            Btn_REG.Enabled = true;
            FKB.F10_Enabled = true;

            FKB.F06_Enabled = false;
            MNU_DELETE.Enabled = false;
        }

        /// <summary>
        /// 仕入先コンボボックスの使用可否を設定する
        /// </summary>
        /// <param name="trcd">取引先コード</param>
        /// <param name="hjcd">補助コード</param>
        private void Set_Enabled_Cbo_SAIMU(string trcd, string hjcd)
        {
            if (!Global.bEnabledState)
            {
                Cbo_SAIMU.Enabled = false;
                return;
            }
            if (Global.bIchigen)
            {
                Cbo_SAIMU.Enabled = false;
                return;
            }
            if (mcBsLogic.Exists_Saimu_Data(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
            {
                Cbo_SAIMU.Enabled = false;
                return;
            }
            if (mcBsLogic.Exists_Sousai_Siire(Txt_TRCD.ExCodeDB, Txt_HJCD.Text))
            {
                Cbo_SAIMU.Enabled = false;
                return;
            }
            if (Cbo_SAIMU.SelectedValue.ToString() == sUse)
            {
                if (mcBsLogic.Chk_SaimuDaihyo(trcd, hjcd))
                {
                    Cbo_SAIMU.Enabled = false;
                    return;
                }
                if (mcBsLogic.Get_MySaimuDaihyo(trcd, hjcd))
                {
                    Cbo_SAIMU.Enabled = false;
                    return;
                }
                if (mcBsLogic.Exists_Plural_SS_TSHOH_BK_All(trcd, hjcd))
                {
                    Cbo_SAIMU.Enabled = false;
                    return;
                }
            }

            Cbo_SAIMU.Enabled = true;
        }

        /// <summary>
        /// 支払代表者チェックボックスの使用可否を設定する
        /// </summary>
        /// <param name="trcd">取引先コード</param>
        /// <param name="hjcd">補助コード</param>
        private void Set_Enabled_Chk_SAIMU_FLG(string trcd, string hjcd)
        {
            if (!Global.bEnabledState)
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }            
            if (Cbo_SAIMU.SelectedValue.ToString() != sUse)
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }
            if (Global.bIchigen)
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }
            if (Global.GAI_F != "0")
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }
            if (mcBsLogic.Chk_SaimuDaihyo(trcd, hjcd))
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }
            if (mcBsLogic.Get_MySaimuDaihyo(trcd, hjcd))
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }
            if (mcBsLogic.Exists_Plural_SS_TSHOH_BK_All(trcd, hjcd))
            {
                Chk_SAIMU_FLG.Enabled = false;
                return;
            }

            Chk_SAIMU_FLG.Enabled = true;
        }
        // ---> V02.28.01 KKL ADD ▼(No.115107)
        private bool ConfirmExistingData()
        {
            if (bN == false && (nDispChgFlg_Main == 1 || nDispChgFlg_TSHOH == 1 || nDispChgFlg_FRIGIN == 1))
            {
                fKeyClick = true;
                res = MessageBox.Show(
                "変更されています。確定しますか？", Global.sPrgName, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (res == DialogResult.Cancel)
                {
                    fKeyClick = false;
                    return true;
                }
                else if (res == DialogResult.No)
                {
                    if (Global.GAI_F == "1")
                    {
                        Flg_Tsh_Fri = false;
                        GAI_F_Kirikae(0);
                    }
                    nTRCDflg = 1;
                    Sel_SSTORI();
                }
                else if (res == DialogResult.Yes)
                {
                    nErrFlg = 0;
                    Ins_SSTORI();
                    if (nErrFlg == 1)
                    {
                        return true;
                    }
                    else
                    {
                        nDispChgFlg_Main = 0;
                        nDispChgFlg_TSHOH = 0;
                        nDispChgFlg_FRIGIN = 0;
                        Btn_REG.Enabled = false;
                        FKB.F10_Enabled = false;
                    }
                }
            }
            return false;
        }

        private void MNU_SHOWHELP_Click(object sender, EventArgs e)
        {
            IcsComUtil.ComUtil.ShowHelpFile(this.TLB.TLB_FormId, Global.sPrgName, Global.nUcod);
        }

        private void Tb4_Txt_KOUZA_Validating(object sender, CancelEventArgs e)
        {
            if (!String.IsNullOrEmpty(Tb4_Txt_KOUZA.Text))
            {
                long lKouza = 0;
                Int64.TryParse(Tb4_Txt_KOUZA.Text, out lKouza);
                if (lKouza <= 0 || lKouza > 9999999)
                {
                    Tb4_Txt_KOUZA.IsError = true;
                    Tb4_Txt_KOUZA.Focus();
                    return;
                }
            }
        }
        // <--- V02.28.01 KKL ADD ▲(No.115107)
    }
}
